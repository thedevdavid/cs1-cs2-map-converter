namespace NetConverter;

public sealed class Round4Stats
{
    public int Crossings, CrossingsAtGradeInCs1, CrossingsAlreadySeparated, CrossingNodesInserted, CrossingRampNodes;
    public int BridgeChains, BridgeChainsChanged, BridgeNodesChanged;
    public double BridgeMaxChange;
    public int TangentFailures, TangentSwapped, TangentStraightened;
    public int TinyStubsCollapsed, RampTailsSnapped, BorderStubsFolded;
    public int PortalsSetToZero, TunnelsDeepened, RaisedToMin, RaisedToGround, TunnelNodesCapped;
    public List<CrossingInfo> CrossingList { get; } = new();
    public List<RampTail> RampTails { get; } = new();
}

/// <summary>A point where courses of two layers cross without a shared node.</summary>
public sealed record CrossingInfo(string UpperEdge, string UpperPrefab, string LowerEdge, string LowerPrefab, double X, double Z,
    double Cs1Dy, double SeparationBefore, double UpperElevationAfter, string Action);

public sealed record RampTail(string Node, string Edge, string TargetEdge, double Distance, double X, double Z);

public static partial class Converter
{
    static string LayerKey(OutEdge e) => e.Layer;

    /// <summary>Samples of a course: parameter, x/z position, CS1 deck height, and the elevation the importer will interpolate.</summary>
    static (double T, Vec3 P, double Elev)[] Track(OutEdge e, int n = 32) =>
        Enumerable.Range(0, n + 1).Select(i =>
        {
            double t = i / (double)n;
            return (t, e.Curve.Eval(t), e.A.Elevation + (e.B.Elevation - e.A.Elevation) * t);
        }).ToArray();

    static bool Intersect2D(Vec3 a, Vec3 b, Vec3 c, Vec3 d, out double u, out double v)
    {
        double den = (b.X - a.X) * (d.Z - c.Z) - (b.Z - a.Z) * (d.X - c.X);
        u = v = 0;
        if (Math.Abs(den) < 1e-9) return false;
        u = ((c.X - a.X) * (d.Z - c.Z) - (c.Z - a.Z) * (d.X - c.X)) / den;
        v = ((c.X - a.X) * (b.Z - a.Z) - (c.Z - a.Z) * (b.X - a.X)) / den;
        return u >= 0 && u <= 1 && v >= 0 && v <= 1;
    }

    /// <summary>
    /// Crossings between imported layers without a shared node. CS1 had every one of them grade-separated (the deck heights
    /// differ by ≥ GradeSeparation), but where both sides are ground courses the node elevations are 0 on both, and CS2 builds a
    /// level crossing or a low slab. The upper course gets a node at the crossing at its CS1 deck height, at least
    /// MinBridgeElevation above the terrain and above the lower course; the ramp walk then spreads it along the chain
    /// (MaxGrade for roads, RailMaxGrade for track). Crossings that CS1 had at grade are only listed.
    /// </summary>
    static void GradeSeparate(ConvertResult res, ConvertOptions o)
    {
        var r4 = res.R4;
        var edges = res.Edges.Where(e => e.Map.Imported).ToList();
        var tracks = edges.ToDictionary(e => e, e => Track(e), ReferenceEqualityComparer.Instance);
        const double cell = 64;
        var grid = new Dictionary<(long, long), List<OutEdge>>();
        foreach (var e in edges)
            foreach (var key in tracks[e].Select(s => ((long)Math.Floor(s.P.X / cell), (long)Math.Floor(s.P.Z / cell))).Distinct())
            {
                if (!grid.TryGetValue(key, out var l)) grid[key] = l = new();
                l.Add(e);
            }

        var splits = new Dictionary<OutEdge, List<(double T, double Elev)>>(ReferenceEqualityComparer.Instance);
        var seen = new HashSet<(OutEdge, OutEdge)>();
        foreach (var list in grid.Values)
            for (int i = 0; i < list.Count; i++)
                for (int j = i + 1; j < list.Count; j++)
                {
                    var e1 = list[i]; var e2 = list[j];
                    if (e1.Layer == e2.Layer) continue;
                    if (e1.A == e2.A || e1.A == e2.B || e1.B == e2.A || e1.B == e2.B) continue;
                    if (!seen.Add((e1, e2)) || !seen.Add((e2, e1))) continue;
                    var t1 = tracks[e1]; var t2 = tracks[e2];
                    (int, int, double, double)? hit = null;
                    for (int a = 0; a + 1 < t1.Length && hit == null; a++)
                        for (int b = 0; b + 1 < t2.Length; b++)
                            if (Intersect2D(t1[a].P, t1[a + 1].P, t2[b].P, t2[b + 1].P, out double u, out double v)) { hit = (a, b, u, v); break; }
                    if (hit is not var (ia, ib, fu, fv)) continue;
                    r4.Crossings++;

                    double T1 = t1[ia].T + (t1[ia + 1].T - t1[ia].T) * fu, T2 = t2[ib].T + (t2[ib + 1].T - t2[ib].T) * fv;
                    var p = e1.Curve.Eval(T1);
                    double y1 = p.Y, y2 = e2.Curve.Eval(T2).Y;
                    double el1 = t1[ia].Elev + (t1[ia + 1].Elev - t1[ia].Elev) * fu, el2 = t2[ib].Elev + (t2[ib + 1].Elev - t2[ib].Elev) * fv;
                    var (up, lo, tu, yu, yl, eu, el) = y1 >= y2 ? (e1, e2, T1, y1, y2, el1, el2) : (e2, e1, T2, y2, y1, el2, el1);
                    double terrain = o.Terrain?.Sample(p.X, p.Z) ?? up.A.Terrain + (up.B.Terrain - up.A.Terrain) * tu;

                    if (yu - yl < o.GradeSeparation)
                    {
                        r4.CrossingsAtGradeInCs1++;
                        r4.CrossingList.Add(new(up.Id, up.Map.Cs2Prefab!, lo.Id, lo.Map.Cs2Prefab!, p.X, p.Z, yu - yl, eu - el, eu, "at grade in CS1 (left)"));
                        continue;
                    }
                    if (el <= -o.GradeSeparation)
                    {
                        // The lower course is already in a cut or tunnel deep enough.
                        r4.CrossingsAlreadySeparated++;
                        r4.CrossingList.Add(new(up.Id, up.Map.Cs2Prefab!, lo.Id, lo.Map.Cs2Prefab!, p.X, p.Z, yu - yl, eu - el, eu, "separated (lower course sunk)"));
                        continue;
                    }
                    double target = Math.Max(Math.Max(o.MinBridgeElevation, yu - terrain), el + o.MinBridgeElevation);
                    if (eu >= target - 0.5)
                    {
                        r4.CrossingsAlreadySeparated++;
                        r4.CrossingList.Add(new(up.Id, up.Map.Cs2Prefab!, lo.Id, lo.Map.Cs2Prefab!, p.X, p.Z, yu - yl, eu - el, eu, "separated"));
                        continue;
                    }
                    if (!splits.TryGetValue(up, out var sl)) splits[up] = sl = new();
                    sl.Add((tu, Math.Round(target, 2)));
                    r4.CrossingList.Add(new(up.Id, up.Map.Cs2Prefab!, lo.Id, lo.Map.Cs2Prefab!, p.X, p.Z, yu - yl, eu - el, target, "upper raised at the crossing"));
                }

        // Split each raised course at its crossings, then ramp away from the new nodes.
        var seeds = new List<OutNode>();
        foreach (var (e, list) in splits)
        {
            var ordered = list.OrderBy(x => x.T).ToList();
            var pieces = new List<OutEdge>();
            var rest = e.Curve; double t0 = 0; var from = e.A; int k = 0;
            foreach (var (t, elev) in ordered)
            {
                double local = (t - t0) / (1 - t0);
                var (l, r) = rest.Split(local);
                if (l.LengthXZ() < o.MinLength || r.LengthXZ() < o.MinLength)
                {
                    // Too close to an end: raise that end instead.
                    var endNode = l.LengthXZ() < o.MinLength ? from : e.B;
                    if (endNode.Elevation < elev) { endNode.Elevation = elev; endNode.Pinned = true; seeds.Add(endNode); }
                    continue;
                }
                double terrain = o.Terrain?.Sample(l.D.X, l.D.Z) ?? e.A.Terrain + (e.B.Terrain - e.A.Terrain) * t;
                var node = new OutNode { Id = $"{e.Id}g{k}", Position = l.D with { Y = terrain + elev }, Terrain = terrain, Elevation = elev, Pinned = true };
                pieces.Add(CopyEdge(e, $"{e.Id}g{k}", from, node, l));
                from = node; rest = r; t0 = t; k++;
                seeds.Add(node);
                r4.CrossingNodesInserted++;
            }
            if (pieces.Count == 0) continue;
            pieces.Add(CopyEdge(e, $"{e.Id}g{k}", from, e.B, rest));
            int at = res.Edges.IndexOf(e);
            res.Edges.RemoveAt(at);
            res.Edges.InsertRange(at, pieces);
        }

        if (seeds.Count == 0) return;
        foreach (var layer in seeds.Select(s => res.Edges.First(e => e.A == s || e.B == s).Layer).Distinct().ToList())
        {
            var adj = Adjacency(res.Layer(layer));
            int before = res.Stats.RampNodesRaised;
            foreach (var s in seeds)
                if (adj.TryGetValue(s, out var inc))
                    foreach (var e in inc) RampWalk(e, s, adj, o, res.Stats);
            r4.CrossingRampNodes += res.Stats.RampNodesRaised - before;
        }
    }

    static OutEdge CopyEdge(OutEdge e, string id, OutNode a, OutNode b, Bezier c) => new()
    {
        Id = id, Cs1Segment = e.Cs1Segment, Map = e.Map, Info = e.Info, AiStructure = e.AiStructure, Structure = e.Structure,
        A = a, B = b, Curve = c, HairpinFixed = e.HairpinFixed, RampSmoothed = e.RampSmoothed,
    };

    /// <summary>
    /// Wavy decks: along chains of raised degree-2 nodes on bridges/viaducts, the absolute deck height is smoothed where it
    /// oscillates: a node whose grade change exceeds MaxGradeChange while a neighbour bends the other way by more than that is
    /// pulled to within MaxGradeChange / (1/Δs₁ + 1/Δs₂) of its neighbours' chord, until no such flip is left. Single humps
    /// (overpasses) and arches are kept; CS1's bumpy node heights on flat viaducts are not.
    /// Chain ends and pinned (crossing) nodes stay fixed. Iterates to a fixed point.
    /// </summary>
    static void SmoothBridgeProfiles(List<OutEdge> edges, ConvertOptions o, Round4Stats r4)
    {
        var adj = Adjacency(edges);
        bool Raised(OutNode n) => n.Elevation > o.GroundTolerance;
        // Only real bridge decks: both pieces at the node are CS1 bridge/elevated pieces (ramps and their toes stay as they are).
        bool Deck(OutEdge e) => e.AiStructure == Structure.Elevated || (e.A.Elevation > 2 && e.B.Elevation > 2 && !e.RampSmoothed);
        bool Interior(OutNode n) => Raised(n) && !n.Border && !n.Pinned && adj[n].Count == 2 && adj[n].All(Deck);
        var done = new HashSet<OutNode>(ReferenceEqualityComparer.Instance);
        foreach (var start in adj.Keys.Where(Interior))
        {
            if (done.Contains(start)) continue;
            // Walk both ways to the chain ends.
            done.Add(start);
            List<OutNode> Walk(OutEdge first)
            {
                var list = new List<OutNode>();
                var prev = start; var edge = first;
                while (true)
                {
                    var next = edge.A == prev ? edge.B : edge.A;
                    list.Add(next);
                    if (!Interior(next) || done.Contains(next)) break;
                    done.Add(next);
                    edge = adj[next][0] == edge ? adj[next][1] : adj[next][0];
                    prev = next;
                }
                return list;
            }
            var back = Walk(adj[start][0]); back.Reverse();
            var nodes = back.Append(start).Concat(Walk(adj[start][1])).ToList();
            if (nodes.First() == nodes.Last()) continue; // closed loop of raised nodes: no fixed ends
            if (nodes.Count < 3) continue;
            r4.BridgeChains++;
            var h = nodes.Select(n => n.Terrain + n.Elevation).ToArray();
            var s = new double[nodes.Count];
            for (int i = 1; i < nodes.Count; i++) s[i] = s[i - 1] + nodes[i].Position.DistXZ(nodes[i - 1].Position);
            var orig = (double[])h.Clone();
            // Grade change at interior node i (slope out − slope in).
            double G(int i) => (h[i + 1] - h[i]) / Math.Max(1, s[i + 1] - s[i]) - (h[i] - h[i - 1]) / Math.Max(1, s[i] - s[i - 1]);
            for (int iter = 0; iter < 2000; iter++)
            {
                bool changed = false;
                for (int i = 1; i + 1 < nodes.Count; i++)
                {
                    double gi = G(i);
                    if (Math.Abs(gi) <= o.MaxGradeChange + 1e-6) continue;
                    // Only oscillation: a neighbour bends the other way by more than the limit. Single humps and arches stay.
                    bool flip = (i - 1 >= 1 && Math.Sign(G(i - 1)) == -Math.Sign(gi) && Math.Abs(G(i - 1)) > o.MaxGradeChange)
                             || (i + 2 < nodes.Count && Math.Sign(G(i + 1)) == -Math.Sign(gi) && Math.Abs(G(i + 1)) > o.MaxGradeChange);
                    if (!flip) continue;
                    double d1 = Math.Max(1, s[i] - s[i - 1]), d2 = Math.Max(1, s[i + 1] - s[i]);
                    double chord = h[i - 1] + (h[i + 1] - h[i - 1]) * d1 / (d1 + d2);
                    double limit = o.MaxGradeChange / (1 / d1 + 1 / d2);
                    h[i] = chord + Math.Sign(h[i] - chord) * limit;
                    changed = true;
                }
                if (!changed) break;
            }
            int moved = 0;
            for (int i = 1; i + 1 < nodes.Count; i++)
            {
                double d = h[i] - orig[i];
                if (Math.Abs(d) < 0.05) continue;
                nodes[i].Elevation = Math.Round(h[i] - nodes[i].Terrain, 2);
                r4.BridgeMaxChange = Math.Max(r4.BridgeMaxChange, Math.Abs(d));
                moved++;
            }
            if (moved > 0) { r4.BridgeChainsChanged++; r4.BridgeNodesChanged += moved; }
        }
    }

    /// <summary>
    /// Tunnel and low-deck transitions CS2 can build: no node in (−MinTunnelDepth, 0). A shallow node next to a surface node
    /// (≥ 0) becomes the portal at 0, any other shallow tunnel node goes to −TunnelDefault, so the transition happens over
    /// exactly one course from the portal to the first tunnel node. Raised nodes in (0, MinRaised) next to a raised neighbour
    /// go to 0 or MinRaised (whichever is nearer); between ground neighbours they stay. Nearest-to-zero nodes first; repeats.
    /// </summary>
    static void NormalizeTransitions(ConvertResult res, ConvertOptions o)
    {
        var adj = Adjacency(res.Edges.Where(e => e.Map.Imported));
        IEnumerable<OutNode> Neighbours(OutNode n) => adj[n].Select(e => e.A == n ? e.B : e.A);
        var r4 = res.R4;
        for (int pass = 0; pass < 50; pass++)
        {
            bool changed = false;
            foreach (var n in adj.Keys.Where(n => n.Elevation < 0 && n.Elevation > -o.MinTunnelDepth).OrderByDescending(n => n.Elevation).ToList())
            {
                if (!(n.Elevation < 0 && n.Elevation > -o.MinTunnelDepth)) continue;
                if (Neighbours(n).Any(m => m.Elevation >= 0)) { n.Elevation = 0; r4.PortalsSetToZero++; }
                else { n.Elevation = -o.TunnelDefault; r4.TunnelsDeepened++; }
                changed = true;
            }
            foreach (var n in adj.Keys.Where(n => n.Elevation > 0 && n.Elevation < o.MinRaised && !n.Pinned).ToList())
            {
                if (!Neighbours(n).Any(m => m.Elevation > 0)) continue; // between ground neighbours: a small hump, leave it
                if (n.Elevation < o.MinRaised / 2) { n.Elevation = 0; r4.RaisedToGround++; }
                else { n.Elevation = o.MinRaised; r4.RaisedToMin++; }
                changed = true;
            }
            if (!changed) break;
        }

        // Portal grade: walking into the tunnel from each portal, depth ≤ max(MinTunnelDepth, TunnelMaxGrade · s).
        foreach (var portal in adj.Keys.Where(n => n.Elevation == 0).ToList())
            foreach (var first in adj[portal].Where(e => (e.A == portal ? e.B : e.A).Elevation < 0))
            {
                double s = 0; var prev = portal; var edge = first;
                for (int guard = 0; guard < 100; guard++)
                {
                    var next = edge.A == prev ? edge.B : edge.A;
                    if (next.Elevation >= 0) break;
                    s += edge.Curve.LengthXZ();
                    double cap = -Math.Max(o.MinTunnelDepth, o.TunnelMaxGrade * s);
                    if (next.Elevation >= cap) break;           // already shallow enough; deeper nodes beyond are reached at gentler grade
                    next.Elevation = Math.Round(cap, 2);
                    r4.TunnelNodesCapped++;
                    if (adj[next].Count != 2) break;
                    edge = adj[next][0] == edge ? adj[next][1] : adj[next][0];
                    prev = next;
                }
            }
    }

    /// <summary>
    /// A border edge shorter than maxLength whose inner node continues in exactly one other edge is folded into that edge:
    /// the neighbour ends at the border node and the stub goes (CS2 may flag a very short last segment as ShortDistance).
    /// </summary>
    static void FoldShortBorderEdges(ConvertResult res, double maxLength)
    {
        var adj = Adjacency(res.Edges.Where(e => e.Map.Imported));
        var drop = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        foreach (var (n, inc) in adj)
        {
            if (!n.Border || inc.Count != 1) continue;
            var stub = inc[0];
            if (drop.Contains(stub) || stub.Curve.LengthXZ() >= maxLength) continue;
            var inner = stub.A == n ? stub.B : stub.A;
            var others = adj[inner].Where(x => x != stub && !drop.Contains(x)).ToList();
            if (others.Count != 1 || others[0].Layer != stub.Layer) continue;
            var f = others[0];
            if (f.A == inner) f.A = n; else f.B = n;
            f.Curve = f.Curve.WithEnds(f.A.Position, f.B.Position);
            drop.Add(stub);
            res.Fate[stub.Cs1Segment] = "folded into its neighbour (short border edge)";
            res.R4.BorderStubsFolded++;
        }
        res.Edges.RemoveAll(drop.Contains);
    }

    /// <summary>Edges shorter than TinyStubLength from a junction (degree ≥ 3) to a dead end are dropped into the junction.</summary>
    static void CollapseTinyStubs(ConvertResult res, ConvertOptions o)
    {
        for (int pass = 0; pass < 5; pass++)
        {
            var edges = res.Edges.Where(e => e.Map.Imported).ToList();
            var adj = Adjacency(edges);
            var drop = edges.Where(e => e.Curve.LengthXZ() < o.TinyStubLength
                && ((adj[e.A].Count >= 3 && adj[e.B].Count == 1 && !e.B.Border) || (adj[e.B].Count >= 3 && adj[e.A].Count == 1 && !e.A.Border))).ToHashSet();
            if (drop.Count == 0) break;
            foreach (var e in drop) res.Fate[e.Cs1Segment] = "tiny stub collapsed into junction";
            res.R4.TinyStubsCollapsed += res.Edges.RemoveAll(drop.Contains);
        }
    }

    /// <summary>
    /// Ramp tails: a highway dead end within RampTailDistance of a highway course it is not connected to, running roughly
    /// along it (&lt; 45°), is snapped onto that course; the course is split there so the ramp gets a real junction.
    /// </summary>
    static void SnapRampTails(ConvertResult res, ConvertOptions o)
    {
        var edges = res.Layer(Layers.Highways).ToList();
        var adj = Adjacency(res.Edges.Where(e => e.Map.Imported));
        foreach (var (n, inc) in adj.ToList())
        {
            if (inc.Count != 1 || n.Border || inc[0].Layer != Layers.Highways) continue;
            var ramp = inc[0];
            var tangent = n == ramp.B ? ramp.Curve.D - ramp.Curve.C : ramp.Curve.A - ramp.Curve.B;
            OutEdge? best = null; double bestD = o.RampTailDistance, bestT = 0;
            foreach (var e in edges)
            {
                if (e == ramp || e.A == n || e.B == n) continue;
                if (Math.Min(e.A.Position.DistXZ(n.Position), e.B.Position.DistXZ(n.Position)) > e.Curve.LengthXZ() + o.RampTailDistance) continue;
                for (int i = 0; i <= 32; i++)
                {
                    double t = i / 32.0;
                    double d = e.Curve.Eval(t).DistXZ(n.Position);
                    if (d < bestD) { bestD = d; best = e; bestT = t; }
                }
            }
            if (best == null || bestT < 0.03 || bestT > 0.97) continue;
            var dir = best.Curve.Eval(Math.Min(1, bestT + 0.01)) - best.Curve.Eval(Math.Max(0, bestT - 0.01));
            double cos = Math.Abs(tangent.X * dir.X + tangent.Z * dir.Z) / (Math.Sqrt(tangent.X * tangent.X + tangent.Z * tangent.Z) * Math.Sqrt(dir.X * dir.X + dir.Z * dir.Z) + 1e-9);
            if (cos < Math.Cos(45 * Math.PI / 180)) continue;
            if (Math.Abs(best.A.Elevation + (best.B.Elevation - best.A.Elevation) * bestT - n.Elevation) > 4) continue; // passes over/under

            var (l, r) = best.Curve.Split(bestT);
            if (l.LengthXZ() < o.MinLength || r.LengthXZ() < o.MinLength) continue;
            res.R4.RampTails.Add(new(n.Id, ramp.Id, best.Id, bestD, n.Position.X, n.Position.Z));
            n.Position = l.D with { Y = n.Position.Y };
            n.Elevation = Math.Round(best.A.Elevation + (best.B.Elevation - best.A.Elevation) * bestT, 2);
            ramp.Curve = ramp.Curve.WithEnds(ramp.A.Position, ramp.B.Position);
            int at = res.Edges.IndexOf(best);
            res.Edges.RemoveAt(at);
            res.Edges.InsertRange(at, [CopyEdge(best, best.Id + "s0", best.A, n, l), CopyEdge(best, best.Id + "s1", n, best.B, r)]);
            edges = res.Layer(Layers.Highways).ToList();
            res.R4.RampTailsSnapped++;
        }
    }

    /// <summary>
    /// Twisted courses: for a → b with controls c1, c2 require (c1 − a)·(b − a) &gt; 0, (b − c2)·(b − a) &gt; 0 and no
    /// self-intersection. On failure try the controls swapped, else make the course straight.
    /// </summary>
    static void FixTangents(ConvertResult res)
    {
        static bool Ok(Bezier b)
        {
            double cx = b.D.X - b.A.X, cz = b.D.Z - b.A.Z;
            return (b.B.X - b.A.X) * cx + (b.B.Z - b.A.Z) * cz > 0
                && (b.D.X - b.C.X) * cx + (b.D.Z - b.C.Z) * cz > 0
                && !SelfIntersects(b);
        }
        foreach (var e in res.Edges.Where(e => e.Map.Imported))
        {
            if (Ok(e.Curve)) continue;
            res.R4.TangentFailures++;
            var swapped = e.Curve with { B = e.Curve.C, C = e.Curve.B };
            if (Ok(swapped)) { e.Curve = swapped; res.R4.TangentSwapped++; continue; }
            var a = e.Curve.A; var d = e.Curve.D;
            e.Curve = new Bezier(a, Vec3.Lerp(a, d, 1 / 3.0), Vec3.Lerp(a, d, 2 / 3.0), d);
            res.R4.TangentStraightened++;
        }
    }
}

/// <summary>
/// Live Run A: Pavement Path dead ends (CS1 dead ends too) 7–16 m beside a road, gravel road or path. The bridge's
/// snapToSegment splits the nearest segment there, so the path joins it. Only ground ends next to a ground segment
/// within <c>maxDistance</c> and <c>maxDy</c> (CS1 metres), and only when the path edge stays ≥ 4 m long.
/// </summary>
public static class PathSnap
{
    public const string PathPrefab = "Pavement Path";

    public static List<string> Apply(ConvertResult res, double topologyFactor, double maxDistance = 20, double maxDy = 3)
    {
        var log = new List<string>();
        var net = res.Edges.Where(e => e.Map.Imported).ToList();
        var degree = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
        foreach (var e in net) foreach (var n in new[] { e.A, e.B }) degree[n] = degree.GetValueOrDefault(n) + 1;
        var targets = res.Layer(Layers.Roads).ToList();
        foreach (var path in targets.Where(e => e.Map.Cs2Prefab == PathPrefab).ToList())
            foreach (var (n, other) in new[] { (path.A, path.B), (path.B, path.A) })
            {
                if (degree[n] != 1 || n.Border || n.Snap != null || n.Elevation != 0) continue;
                OutEdge? best = null; double bestD = maxDistance; Vec3 at = default;
                foreach (var e in targets)
                {
                    if (e == path || e.A == n || e.B == n || e.A.Elevation != 0 || e.B.Elevation != 0) continue;
                    // Coarse samples, then a fine pass around the closest one (long segments).
                    double D(Vec3 p) => Math.Sqrt((p.X - n.Position.X) * (p.X - n.Position.X) + (p.Z - n.Position.Z) * (p.Z - n.Position.Z));
                    int kBest = Enumerable.Range(0, 33).MinBy(k => D(e.Curve.Eval(k / 32.0)));
                    for (int k = 0; k <= 64; k++)
                    {
                        double t = Math.Clamp((kBest - 1 + k / 32.0) / 32.0, 0, 1);
                        var p = e.Curve.Eval(t); double d = D(p);
                        if (d < bestD) { bestD = d; best = e; at = p; }
                    }
                }
                if (best == null) continue;
                double dy = Math.Abs(at.Y - n.Position.Y) / topologyFactor;
                double len = Math.Sqrt((at.X - other.Position.X) * (at.X - other.Position.X) + (at.Z - other.Position.Z) * (at.Z - other.Position.Z));
                if (dy > maxDy || len < 4) continue;
                n.Snap = (Math.Ceiling(bestD + 1.5), best.Map.Cs2Prefab!);   // ≥ 1.5 m margin
                log.Add(string.Create(System.Globalization.CultureInfo.InvariantCulture,
                    $"{n.Id} ({n.Position.X:F0}, {n.Position.Z:F0}) → {best.Id} {best.Map.Cs2Prefab}, {bestD:F1} m, Δy {dy:F1} m, radius {n.Snap.Value.Radius}"));
            }
        return log;
    }
}
