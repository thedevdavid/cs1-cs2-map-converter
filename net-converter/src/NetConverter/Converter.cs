namespace NetConverter;

public enum Structure { Ground, Elevated, Tunnel }

public sealed class OutNode
{
    public required string Id { get; init; }
    public Vec3 Position { get; set; }
    public double Terrain { get; set; }
    public int? Cs1Node { get; init; }
    /// <summary>CS1 m_elevation byte (height above ground for elevated/bridge, depth for tunnels).</summary>
    public int Cs1ElevationByte { get; init; }
    /// <summary>Created by clipping, lies exactly on the CS2 border.</summary>
    public bool Border { get; set; }
    public bool Underground { get; init; }
    public double Elevation { get; set; }
    /// <summary>Bridge snapToSegment for a dead end that should join a nearby segment (radius m, prefabContains).</summary>
    public (double Radius, string Prefab)? Snap { get; set; }
    /// <summary>Height set by grade separation; later smoothing keeps it.</summary>
    public bool Pinned { get; set; }
}

public sealed class OutEdge
{
    public required string Id { get; init; }
    public required int Cs1Segment { get; init; }
    public required MapResult Map { get; init; }
    public required NetPrefabInfo Info { get; init; }
    /// <summary>Structure implied by the CS1 AI (bridge/tunnel/slope variant). Used only to decide which nodes may leave the ground.</summary>
    public required Structure AiStructure { get; init; }
    /// <summary>Final structure, from the end elevations.</summary>
    public Structure Structure { get; set; }
    public OutNode A { get; set; } = null!;
    public OutNode B { get; set; } = null!;
    public Bezier Curve { get; set; }
    public string Layer => Map.Layer;
    public bool HairpinFixed { get; set; }
    public bool RampSmoothed { get; set; }
    public bool Collapsed { get; set; }
    /// <summary>Quay wall side for the bridge ("left", "right", "both"): left = left of the direction a → b, seen from above.</summary>
    public string? Wall { get; set; }
}

public sealed class ConvertOptions
{
    public double Half { get; init; } = 7168;
    public double MergeDistance { get; init; } = 0.5;
    public double MinLength { get; init; } = 4;
    public double MaxLength { get; init; } = 1500;
    /// <summary>|y − terrain| below this counts as ground level.</summary>
    public double GroundTolerance { get; init; } = 0.5;
    /// <summary>Ramp pass: an edge is steep when its end elevations differ by more than this ...</summary>
    public double RampDelta { get; init; } = 6;
    /// <summary>... over less than this length.</summary>
    public double RampLength { get; init; } = 60;
    public double MaxGrade { get; init; } = 0.08;
    /// <summary>Overlap pass: same-layer edges closer than this ...</summary>
    public double OverlapDistance { get; init; } = 4;
    /// <summary>... for longer than this.</summary>
    public double OverlapLength { get; init; } = 20;
    public double AcuteAngleDegrees { get; init; } = 10;
    /// <summary>Nodes closer than MergeDistance are merged only when their heights differ by less than this (stacked crossings stay apart).</summary>
    public double MergeHeight { get; init; } = 2;
    /// <summary>Diverge/merge pairs leaving a node closer than this get their tangents spread to it.</summary>
    public double MinDivergeAngle { get; init; } = 12;
    /// <summary>CS2 terrain the importer will build on. When set, node elevations are measured against it and bridge/tunnel
    /// courses are split where the bridge's terrain-following control heights would leave the CS1 deck.</summary>
    public Heightmap? Terrain { get; init; }
    /// <summary>Deck fit: split a raised/sunk course when the predicted control-point height is off by more than this ...</summary>
    public double DeckTolerance { get; init; } = 2;
    /// <summary>... unless the pieces would be shorter than this.</summary>
    public double DeckMinPiece { get; init; } = 15;
    /// <summary>Upper course at a crossing with another layer stands at least this high (pillars, not a retaining-wall slab).</summary>
    public double MinBridgeElevation { get; init; } = 8;
    /// <summary>CS1 height difference at a crossing below which the crossing counts as at grade.</summary>
    public double GradeSeparation { get; init; } = 4;
    /// <summary>Ramp grade for tracks (roads use MaxGrade).</summary>
    public double RailMaxGrade { get; init; } = 0.04;
    /// <summary>Bridge profile: an interior node may sit at most this far from its neighbours' chord.</summary>
    public double BridgeChordDeviation { get; init; } = 1.5;
    /// <summary>Deck smoothing: largest grade change allowed at a deck node (0.01 = 1 %).</summary>
    public double MaxGradeChange { get; init; } = 0.01;
    /// <summary>Tunnel nodes sit at least this deep; shallower tunnel nodes go to TunnelDefault, or to 0 at a portal.</summary>
    public double MinTunnelDepth { get; init; } = 8;
    public double TunnelDefault { get; init; } = 12;
    /// <summary>Depth change allowed per metre walking into a tunnel from its portal.</summary>
    public double TunnelMaxGrade { get; init; } = 0.15;
    /// <summary>Raised nodes below this (next to a raised neighbour) are moved to 0 or to this value.</summary>
    public double MinRaised { get; init; } = 2;
    /// <summary>Raised courses longer than this are split (CS2 inserts its own mid-span node on longer spans, 5–8 m below the deck).</summary>
    public double MaxElevatedSpan { get; init; } = 75;
    /// <summary>Deck/span split nodes keep at least this distance from courses meeting the split course at an end.</summary>
    public double SplitClearance { get; init; } = 10;
    public double TinyStubLength { get; init; } = 10;
    public double RampTailDistance { get; init; } = 30;
}

public sealed class ConvertStats
{
    public int SegmentsIn, SegmentsOutside, SegmentsClipped, PiecesFromClip, SplitForLength, MergedNodes, CollapsedShort, DroppedDegenerate;
    public int MergesRefusedForHeight, DeckSplits, DeckEdgesSplit, SpanSplits, DeckSplitsSkippedNearMerge;
    public int HairpinsFound, HairpinsFixed, DuplicatesDropped, RampsSmoothed, RampNodesRaised, RampsUnresolved;
    public List<string> HairpinLog { get; } = new();
    public Dictionary<string, int> EdgesPerLayer { get; } = new();
    public Dictionary<string, int> NodesPerLayer { get; } = new();
    public Dictionary<string, int> BorderNodesPerLayer { get; } = new();
}

public sealed record Overlap(string Layer, OutEdge E1, OutEdge E2, double Length, bool SharedNode);
public sealed record AcuteResolution(string Layer, string Node, string Edge1, string Edge2, double AngleBefore, double AngleAfter, string Category);
public sealed record AcuteJunction(string Layer, OutNode Node, OutEdge E1, OutEdge E2, double AngleDegrees);

public sealed class ConvertResult
{
    public required Dump Dump { get; init; }
    public List<OutEdge> Edges { get; } = new();
    public Dictionary<string, MapResult> Maps { get; } = new();
    /// <summary>What the converter did with each CS1 segment (imported, SKIP, outside, collapsed, duplicate, ...).</summary>
    public Dictionary<int, string> Fate { get; } = new();
    public List<AcuteResolution> AcuteResolutions { get; } = new();
    public Round4Stats R4 { get; } = new();
    public ConvertStats Stats { get; } = new();
    public List<Overlap> Overlaps { get; } = new();
    public List<AcuteJunction> AcuteJunctions { get; } = new();
    public IEnumerable<OutEdge> Layer(string layer) => Edges.Where(e => e.Layer == layer);
    public IEnumerable<OutNode> NodesOf(IEnumerable<OutEdge> edges) => edges.SelectMany(e => new[] { e.A, e.B }).Distinct();
}

public static partial class Converter
{
    public static Structure Classify(NetPrefabInfo info, DumpNode? a, DumpNode? b)
    {
        if (info.Ai.Contains("Tunnel")) return Structure.Tunnel;
        if (info.Ai.Contains("Bridge") || info.Ai.Contains("Elevated")) return Structure.Elevated;
        if (a != null && b != null && a.Has("Underground") && b.Has("Underground")) return Structure.Tunnel;
        return Structure.Ground;
    }

    public static ConvertResult Run(Dump dump, Mapping mapping, ConvertOptions? options = null)
    {
        var o = options ?? new ConvertOptions();
        var res = new ConvertResult { Dump = dump };
        var st = res.Stats;
        var nodes = new Dictionary<string, OutNode>();

        OutNode NodeFor(int cs1Id)
        {
            var key = $"n{cs1Id}";
            if (!nodes.TryGetValue(key, out var n))
            {
                var d = dump.Nodes[cs1Id];
                nodes[key] = n = new OutNode
                {
                    Id = key, Position = d.Position, Terrain = d.Terrain, Cs1Node = cs1Id,
                    Cs1ElevationByte = d.Elevation, Underground = d.Has("Underground"),
                };
            }
            return n;
        }

        foreach (var seg in dump.Segments)
        {
            st.SegmentsIn++;
            var info = dump.PrefabInfo(seg.Prefab);
            if (!res.Maps.TryGetValue(seg.Prefab, out var prefabMap)) res.Maps[seg.Prefab] = prefabMap = mapping.Map(info);
            var map = RoadsToHighways.Apply(prefabMap, seg.Bezier);   // round 47: sidewalked roads outside the starting area become highways
            dump.Nodes.TryGetValue(seg.StartNode, out var sn);
            dump.Nodes.TryGetValue(seg.EndNode, out var en);
            if (sn == null || en == null) continue;

            bool hairpinFixed = false;
            var bezier = map.Imported ? CheckHairpin(seg, st, out hairpinFixed) : seg.Bezier;
            var structure = Classify(info, sn, en);
            var intervals = Clipper.InsideIntervals(bezier, o.Half);
            if (intervals.Count == 0) { st.SegmentsOutside++; res.Fate[seg.Id] = "outside the border"; continue; }
            bool whole = intervals.Count == 1 && intervals[0].T0 == 0 && intervals[0].T1 == 1;
            res.Fate[seg.Id] = map.Imported ? (whole ? "imported" : "imported (clipped)") : map.Layer == Layers.Skip ? "SKIP" : map.Layer;
            if (!whole) { st.SegmentsClipped++; st.PiecesFromClip += intervals.Count; }

            int k = 0, piece = 0;
            foreach (var (t0, t1) in intervals)
            {
                var curve = bezier.Sub(t0, t1);
                OutNode Cut(double t, Vec3 p)
                {
                    var id = $"c{seg.Id}_{k++}";
                    var n = new OutNode
                    {
                        Id = id, Position = Clipper.SnapToBorder(p, o.Half), Border = true,
                        Terrain = sn.Terrain + (en.Terrain - sn.Terrain) * t,
                        Underground = structure == Structure.Tunnel,
                    };
                    nodes[id] = n; return n;
                }
                var a = t0 == 0 ? NodeFor(seg.StartNode) : Cut(t0, curve.A);
                var b = t1 == 1 ? NodeFor(seg.EndNode) : Cut(t1, curve.D);
                curve = curve.WithEnds(a.Position, b.Position);

                // One-way networks run a -> b. Invert means the forward lanes run end -> start.
                bool reverse = info.OneWay && (seg.Invert ^ (info.ForwardLanes == 0));
                if (reverse) { (a, b) = (b, a); curve = curve.Reverse(); }

                foreach (var (ca, cb, cc) in SplitLong(curve, a, b, o.MaxLength, $"m{seg.Id}_{k}", nodes, st).ToList())
                    res.Edges.Add(new OutEdge
                    {
                        Id = $"e{seg.Id}_{piece++}",
                        Cs1Segment = seg.Id, Map = map, Info = info, AiStructure = structure, Structure = structure,
                        A = ca, B = cb, Curve = cc, HairpinFixed = hairpinFixed,
                    });
            }
        }

        var layers = res.Edges.Select(e => e.Layer).Distinct().ToList();
        foreach (var layer in layers)
        {
            MergeAndCollapse(res, layer, o);
            DropDuplicates(res, layer);
        }

        foreach (var layer in layers)
        {
            var edges = res.Layer(layer).ToList();
            if (o.Terrain != null)
                foreach (var n in res.NodesOf(edges)) n.Terrain = o.Terrain.Sample(n.Position.X, n.Position.Z);
            AssignElevations(edges, o);
            // Power lines are ground courses; the game places the pylons.
            if (Layers.Ground.Contains(layer)) foreach (var n in res.NodesOf(edges)) n.Elevation = 0;
            // Decorative layers keep every node at its exact CS1 height above the terrain.
            if (layer == Layers.Decor) foreach (var n in res.NodesOf(edges)) n.Elevation = Math.Round(n.Position.Y - n.Terrain, 2);
        }

        // Crossings between layers need every layer's elevations; they insert nodes and ramp away from them.
        GradeSeparate(res, o);

        foreach (var layer in layers)
        {
            var edges = res.Layer(layer).ToList();
            SmoothRamps(edges, o, st);
            if (!Layers.Imported.Contains(layer)) continue;
            if (o.Terrain != null) FitDecks(res, layer, o);
            ResolveAcute(res, layer, o);
            SmoothBridgeProfiles(res.Layer(layer).ToList(), o, res.R4);
        }

        NormalizeTransitions(res, o);
        CollapseTinyStubs(res, o);
        SnapRampTails(res, o);
        FixTangents(res);
        FoldShortBorderEdges(res, 10);

        foreach (var layer in layers)
        {
            var edges = res.Layer(layer).ToList();
            foreach (var e in edges) e.Structure = StructureOf(e, o);
            st.EdgesPerLayer[layer] = edges.Count;
            var ns = res.NodesOf(edges).ToList();
            st.NodesPerLayer[layer] = ns.Count;
            st.BorderNodesPerLayer[layer] = ns.Count(n => n.Border);
            if (Layers.Imported.Contains(layer))
            {
                res.Overlaps.AddRange(FindOverlaps(layer, edges, o));
                res.AcuteJunctions.AddRange(FindAcuteJunctions(layer, edges, o));
            }
        }
        return res;
    }

    // ---------------------------------------------------------------- hairpins

    /// <summary>
    /// True when the curve is implausible for a road piece: arc length over 1.6× the chord,
    /// a self-intersection, or a control point pointing backwards along the chord.
    /// </summary>
    public static bool IsHairpin(Bezier b, out string reason)
    {
        double chord = b.A.DistXZ(b.D), len = b.LengthXZ();
        reason = "";
        if (chord < 1e-3) { reason = "zero chord"; return true; }
        if (len > 1.6 * chord) { reason = $"length {len:F1} > 1.6 x chord {chord:F1}"; return true; }
        double cx = b.D.X - b.A.X, cz = b.D.Z - b.A.Z;
        if ((b.B.X - b.A.X) * cx + (b.B.Z - b.A.Z) * cz < -1e-6 || (b.D.X - b.C.X) * cx + (b.D.Z - b.C.Z) * cz < -1e-6)
        { reason = "control point behind its end"; return true; }
        if (SelfIntersects(b)) { reason = "self-intersection"; return true; }
        return false;
    }

    static bool SelfIntersects(Bezier b, int n = 24)
    {
        var p = Enumerable.Range(0, n + 1).Select(i => b.Eval(i / (double)n)).ToArray();
        for (int i = 0; i < n; i++)
            for (int j = i + 2; j < n; j++)
                if (SegmentsCross(p[i], p[i + 1], p[j], p[j + 1])) return true;
        return false;
    }

    static bool SegmentsCross(Vec3 a, Vec3 b, Vec3 c, Vec3 d)
    {
        static double Cross(Vec3 o, Vec3 p, Vec3 q) => (p.X - o.X) * (q.Z - o.Z) - (p.Z - o.Z) * (q.X - o.X);
        double d1 = Cross(c, d, a), d2 = Cross(c, d, b), d3 = Cross(a, b, c), d4 = Cross(a, b, d);
        return d1 * d2 < 0 && d3 * d4 < 0;
    }

    /// <summary>
    /// Checks the dump's Bézier. If it is a hairpin, rebuilds the control points from the segment's own
    /// directions with start and end swapped (the usual Invert mix-up), else falls back to a straight course.
    /// </summary>
    static Bezier CheckHairpin(DumpSegment seg, ConvertStats st, out bool fixedIt)
    {
        fixedIt = false;
        if (!IsHairpin(seg.Bezier, out var reason)) return seg.Bezier;
        st.HairpinsFound++;
        var a = seg.Bezier.A; var d = seg.Bezier.D;
        double k = a.DistXZ(d) / 3;
        (string Name, Bezier Curve)[] candidates =
        [
            ("directions swapped", new Bezier(a, a + seg.EndDirection * k, d + seg.StartDirection * k, d)),
            ("directions swapped and negated", new Bezier(a, a - seg.EndDirection * k, d - seg.StartDirection * k, d)),
            ("straight", new Bezier(a, Vec3.Lerp(a, d, 1 / 3.0), Vec3.Lerp(a, d, 2 / 3.0), d)),
        ];
        var pick = candidates.First(c => c.Name == "straight" || !IsHairpin(c.Curve, out _));
        st.HairpinsFixed++;
        fixedIt = true;
        st.HairpinLog.Add($"segment {seg.Id} ({seg.Prefab}): {reason} -> {pick.Name}");
        return pick.Curve;
    }

    // ---------------------------------------------------------------- topology

    static IEnumerable<(OutNode, OutNode, Bezier)> SplitLong(Bezier c, OutNode a, OutNode b, double max, string idBase, Dictionary<string, OutNode> nodes, ConvertStats st)
    {
        if (c.LengthXZ() <= max) { yield return (a, b, c); yield break; }
        st.SplitForLength++;
        var (l, r) = c.Split(0.5);
        var mid = new OutNode { Id = idBase + "_" + nodes.Count, Position = l.D, Terrain = (a.Terrain + b.Terrain) / 2, Underground = a.Underground && b.Underground };
        nodes[mid.Id] = mid;
        foreach (var x in SplitLong(l, a, mid, max, idBase, nodes, st)) yield return x;
        foreach (var x in SplitLong(r, mid, b, max, idBase, nodes, st)) yield return x;
    }

    /// <summary>
    /// Within one layer: merge nodes closer than MergeDistance, then collapse edges shorter than MinLength
    /// (their two ends become one node, so the graph stays connected). Border nodes win as representatives
    /// so cut points stay exactly on the border. Repeats until stable.
    /// </summary>
    static void MergeAndCollapse(ConvertResult res, string layer, ConvertOptions o)
    {
        var st = res.Stats;
        var edges = res.Layer(layer).ToList();
        var parent = new Dictionary<OutNode, OutNode>(ReferenceEqualityComparer.Instance);
        OutNode Find(OutNode n) { while (parent.TryGetValue(n, out var p) && p != n) n = p; return n; }
        void Union(OutNode x, OutNode y)
        {
            x = Find(x); y = Find(y);
            if (x == y) return;
            // Prefer border nodes, then original CS1 nodes.
            int Rank(OutNode n) => n.Border ? 2 : n.Cs1Node.HasValue ? 1 : 0;
            if (Rank(y) > Rank(x)) (x, y) = (y, x);
            parent[y] = x;
        }

        var all = edges.SelectMany(e => new[] { e.A, e.B }).Distinct().ToList();
        double cell = o.MergeDistance;
        var grid = new Dictionary<(long, long), List<OutNode>>();
        foreach (var n in all)
        {
            var key = ((long)Math.Floor(n.Position.X / cell), (long)Math.Floor(n.Position.Z / cell));
            for (long dx = -1; dx <= 1; dx++)
                for (long dz = -1; dz <= 1; dz++)
                    if (grid.TryGetValue((key.Item1 + dx, key.Item2 + dz), out var list))
                        foreach (var m in list)
                            if (m.Position.DistXZ(n.Position) < o.MergeDistance && Find(m) != Find(n))
                            {
                                if (Math.Abs(m.Position.Y - n.Position.Y) >= o.MergeHeight) { st.MergesRefusedForHeight++; continue; }
                                Union(m, n); st.MergedNodes++;
                            }
            if (!grid.TryGetValue(key, out var l)) grid[key] = l = new List<OutNode>();
            l.Add(n);
        }

        for (int pass = 0; pass < 10; pass++)
        {
            bool changed = false;
            foreach (var e in edges)
            {
                e.A = Find(e.A); e.B = Find(e.B);
                if (e.A == e.B) continue;
                if (e.Curve.WithEnds(e.A.Position, e.B.Position).LengthXZ() < o.MinLength) { Union(e.A, e.B); e.Collapsed = true; st.CollapsedShort++; changed = true; }
            }
            if (!changed) break;
        }

        foreach (var e in edges)
        {
            e.A = Find(e.A); e.B = Find(e.B);
            e.Curve = e.Curve.WithEnds(e.A.Position, e.B.Position);
        }
        int before = res.Edges.Count;
        foreach (var e in edges.Where(e => e.A == e.B || e.Curve.LengthXZ() < o.MinLength))
            res.Fate[e.Cs1Segment] = e.Collapsed ? "collapsed (< 4 m)" : "degenerate after merge";
        res.Edges.RemoveAll(e => e.Layer == layer && (e.A == e.B || e.Curve.LengthXZ() < o.MinLength));
        st.DroppedDegenerate += before - res.Edges.Count;
    }

    /// <summary>
    /// Exact duplicates: same layer, same two end nodes (either direction) and the curves no more than 1 m apart.
    /// The first one is kept.
    /// </summary>
    static void DropDuplicates(ConvertResult res, string layer)
    {
        var kept = new Dictionary<(string, string), List<OutEdge>>();
        var drop = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        foreach (var e in res.Layer(layer))
        {
            var key = string.CompareOrdinal(e.A.Id, e.B.Id) < 0 ? (e.A.Id, e.B.Id) : (e.B.Id, e.A.Id);
            if (!kept.TryGetValue(key, out var list)) kept[key] = list = new List<OutEdge>();
            if (list.Any(k => SameShape(k.Curve, e.Curve))) drop.Add(e); else list.Add(e);
        }
        foreach (var e in drop) res.Fate[e.Cs1Segment] = "duplicate";
        res.Stats.DuplicatesDropped += res.Edges.RemoveAll(drop.Contains);
    }

    static bool SameShape(Bezier x, Bezier y)
    {
        if (x.A.DistXZ(y.A) > x.A.DistXZ(y.D)) y = y.Reverse();
        for (int i = 0; i <= 8; i++)
            if (x.Eval(i / 8.0).DistXZ(y.Eval(i / 8.0)) > 1) return false;
        return true;
    }

    // ---------------------------------------------------------------- elevation

    /// <summary>
    /// Node elevation (metres above terrain) comes from the node, not the prefab suffix: y − terrain when the CS1 node is
    /// raised or sunk (m_elevation byte &gt; 0, Underground flag) or touches a bridge/tunnel/slope piece; else 0.
    /// |delta| under the tolerance snaps to 0.
    /// </summary>
    static void AssignElevations(List<OutEdge> edges, ConvertOptions o)
    {
        var structural = new HashSet<OutNode>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges)
            if (e.AiStructure != Structure.Ground) { structural.Add(e.A); structural.Add(e.B); }
        foreach (var n in edges.SelectMany(e => new[] { e.A, e.B }).Distinct())
        {
            bool free = structural.Contains(n) || n.Cs1ElevationByte > 0 || n.Underground;
            if (!free) { n.Elevation = 0; continue; }
            double delta = n.Position.Y - n.Terrain;
            if (n.Underground && delta > -o.GroundTolerance) delta = 0;
            n.Elevation = Math.Abs(delta) < o.GroundTolerance ? 0 : Math.Round(delta, 2);
        }
    }

    public static Structure StructureOf(OutEdge e, ConvertOptions o)
    {
        double hi = Math.Max(e.A.Elevation, e.B.Elevation), lo = Math.Min(e.A.Elevation, e.B.Elevation);
        if (lo < -o.GroundTolerance && hi <= 0) return Structure.Tunnel;
        if (hi > o.GroundTolerance) return Structure.Elevated;
        if (lo < -o.GroundTolerance) return Structure.Tunnel;
        return Structure.Ground;
    }

    /// <summary>
    /// Elevation ramps: an edge whose end elevations differ by more than RampDelta over less than RampLength makes CS2
    /// build retaining-wall stubs. The low end and the following nodes along a simple (degree-2) chain are raised to
    /// e(s) = e_high − MaxGrade·s until the ramp reaches the ground or a node that is already high enough.
    /// CS2 interpolates elevation linearly along each edge, so no intermediate nodes are needed.
    /// </summary>
    static Dictionary<OutNode, List<OutEdge>> Adjacency(IEnumerable<OutEdge> edges)
    {
        var adj = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges)
            foreach (var n in new[] { e.A, e.B })
            {
                if (!adj.TryGetValue(n, out var l)) adj[n] = l = new List<OutEdge>();
                l.Add(e);
            }
        return adj;
    }

    /// <summary>
    /// Spreads a raised (or sunk) node outward: starting with edge e from node hi, the far node and the following degree-2
    /// chain get e(s) = e_hi − MaxGrade·s until the ramp reaches a node that is already at least that high (or deep).
    /// Returns true when the ramp ran out before a junction or the border.
    /// </summary>
    static bool RampWalk(OutEdge e, OutNode hi, Dictionary<OutNode, List<OutEdge>> adj, ConvertOptions o, ConvertStats st)
    {
        // Round 26 (A-v7 diagnosis R2): the ramp also runs in absolute height, y(s) = y_hi ∓ grade·s: on rising terrain a falling elevation alone is a
        // rising road (the BR002 hump). A junction is never rewritten: the walk stops before it.
        double sign = Math.Sign(hi.Elevation), topY = hi.Terrain + hi.Elevation;
        double s = 0;
        var edge = e; var from = hi; var to = e.A == hi ? e.B : e.A;
        if (to.Elevation * sign < 0) return false; // passes from bridge to tunnel; leave it
        for (int guard = 0; guard < 200; guard++)
        {
            s += edge.Curve.LengthXZ();
            double g = (e.Layer == Layers.Rail ? o.RailMaxGrade : o.MaxGrade) * s;
            // The lower of the absolute ramp (no hump where the terrain rises) and the above-terrain ramp (ends where the terrain falls away).
            double need = sign * Math.Min(sign * (topY - sign * g - to.Terrain), sign * (hi.Elevation - sign * g));
            if (sign * need <= sign * to.Elevation + 0.01) return true;
            adj.TryGetValue(to, out var next);
            if (next is { Count: > 2 }) return false;                 // a junction: left as it is
            to.Elevation = Math.Round(need, 2);
            st.RampNodesRaised++;
            edge.RampSmoothed = true;
            if (to.Border || next is not { Count: 2 }) return false;
            var nextEdge = next[0] == edge ? next[1] : next[0];
            from = to;
            to = nextEdge.A == from ? nextEdge.B : nextEdge.A;
            edge = nextEdge;
            edge.RampSmoothed = true;
        }
        return false;
    }

    static void SmoothRamps(List<OutEdge> edges, ConvertOptions o, ConvertStats st)
    {
        var adj = Adjacency(edges);

        // Highest first, so one big ramp is not processed in pieces.
        var steep = edges.Where(e => IsSteep(e, o)).OrderByDescending(e => Math.Max(Math.Abs(e.A.Elevation), Math.Abs(e.B.Elevation))).ToList();
        foreach (var e in steep)
        {
            if (!IsSteep(e, o)) continue; // fixed by an earlier ramp
            var (hi, lo) = Math.Abs(e.A.Elevation) >= Math.Abs(e.B.Elevation) ? (e.A, e.B) : (e.B, e.A);
            if (lo.Elevation * Math.Sign(hi.Elevation) < 0) { st.RampsUnresolved++; continue; }
            bool reachedGround = RampWalk(e, hi, adj, o, st);
            st.RampsSmoothed++;
            if (!reachedGround && IsSteepAfter(e, o)) st.RampsUnresolved++;
        }
    }

    static bool IsSteep(OutEdge e, ConvertOptions o) =>
        Math.Abs(e.A.Elevation - e.B.Elevation) > o.RampDelta && e.Curve.LengthXZ() < o.RampLength;

    static bool IsSteepAfter(OutEdge e, ConvertOptions o) =>
        Math.Abs(e.A.Elevation - e.B.Elevation) / Math.Max(1, e.Curve.LengthXZ()) > o.MaxGrade * 1.5;

    // ---------------------------------------------------------------- deck fit

    /// <summary>
    /// The importer gives the Bézier's inner control points the terrain height under them plus the linearly interpolated end
    /// elevations. Over a canyon that bends a flat CS1 deck down to the valley floor (SteepSlope, NotEnoughClearance).
    /// For every raised or sunk course, predict those two heights from the CS2 heightmap and compare them with the deck the CS1
    /// data wants there (straight line between the node heights plus the CS1 curve's own vertical bend). If either is off by
    /// more than DeckTolerance, split the course at t = 0.5 with a node at the CS1 deck height, and repeat on the halves.
    /// </summary>
    static void FitDecks(ConvertResult res, string layer, ConvertOptions o)
    {
        var hm = o.Terrain!;
        var st = res.Stats;
        var result = new List<OutEdge>();
        var adjacent = Adjacency(res.Layer(layer));
        // A split node must not land on another course meeting this one at an end (a ramp merging at a shallow angle):
        // the half next to the merge would lie inside the other road and the game drops it (OverlapExisting).
        bool ClearOfNeighbours(OutEdge e, OutEdge root, Vec3 p) =>
            new[] { e.A, e.B }.SelectMany(n => adjacent.TryGetValue(n, out var l) ? l : []).Where(x => x != e && x != root)
                .All(x => Enumerable.Range(0, 17).Min(i => x.Curve.Eval(i / 16.0).DistXZ(p)) >= o.SplitClearance);
        foreach (var e in res.Layer(layer).ToList())
        {
            var pieces = new List<OutEdge>();
            Fit(e, 0, pieces, e);
            if (pieces.Count > 1)
            {
                st.DeckEdgesSplit++;
                st.DeckSplits += pieces.Count - 1;
                int i = res.Edges.IndexOf(e);
                res.Edges.RemoveAt(i);
                res.Edges.InsertRange(i, pieces);
            }
        }

        double Abs(OutNode n) => n.Terrain + n.Elevation;

        double DeckError(OutEdge e)
        {
            if (Math.Abs(e.A.Elevation) < o.GroundTolerance && Math.Abs(e.B.Elevation) < o.GroundTolerance) return 0;
            // Tunnels: the tube shape is invisible, and splitting would shorten the portal course (too steep for CS2).
            if (e.A.Elevation < 0 || e.B.Elevation < 0) return 0;
            var c = e.Curve;
            double worst = 0;
            foreach (var (p, t) in new[] { (c.B, 1 / 3.0), (c.C, 2 / 3.0) })
            {
                double bend = p.Y - (c.A.Y + (c.D.Y - c.A.Y) * t);   // CS1 vertical curvature at this control point
                double want = Abs(e.A) + (Abs(e.B) - Abs(e.A)) * t + bend;
                double got = hm.Sample(p.X, p.Z) + e.A.Elevation + (e.B.Elevation - e.A.Elevation) * t;
                worst = Math.Max(worst, Math.Abs(got - want));
            }
            return worst;
        }

        void Fit(OutEdge e, int depth, List<OutEdge> into, OutEdge root)
        {
            double len = e.Curve.LengthXZ();
            bool raised = e.A.Elevation >= 0 && e.B.Elevation >= 0 && Math.Max(e.A.Elevation, e.B.Elevation) > o.MinRaised;
            bool longSpan = raised && len > o.MaxElevatedSpan;   // otherwise the game inserts its own mid-span node, which sags
            if (depth >= 5 || len < 2 * o.DeckMinPiece || (!longSpan && DeckError(e) <= o.DeckTolerance)) { into.Add(e); return; }
            var (l, r) = e.Curve.Split(0.5);
            if (!ClearOfNeighbours(e, root, l.D)) { st.DeckSplitsSkippedNearMerge++; into.Add(e); return; }
            if (longSpan && DeckError(e) <= o.DeckTolerance) st.SpanSplits++;
            double want = (Abs(e.A) + Abs(e.B)) / 2 + (l.D.Y - (e.Curve.A.Y + e.Curve.D.Y) / 2);
            double terrain = hm.Sample(l.D.X, l.D.Z);
            var mid = new OutNode
            {
                Id = $"{e.Id}d{depth}", Position = l.D with { Y = want }, Terrain = terrain,
                Underground = e.A.Underground && e.B.Underground,
                Elevation = Math.Round(want - terrain, 2),
            };
            OutEdge Piece(string suffix, OutNode a, OutNode b, Bezier c) => new()
            {
                Id = e.Id + suffix, Cs1Segment = e.Cs1Segment, Map = e.Map, Info = e.Info, AiStructure = e.AiStructure,
                Structure = e.Structure, A = a, B = b, Curve = c, HairpinFixed = e.HairpinFixed, RampSmoothed = e.RampSmoothed,
            };
            Fit(Piece("a", e.A, mid, l), depth + 1, into, root);
            Fit(Piece("b", mid, e.B, r), depth + 1, into, root);
        }
    }

    // ---------------------------------------------------------------- near-parallel pairs

    /// <summary>Unit tangent (x/z) of the edge leaving node n, as an angle in radians.</summary>
    static double LeaveAngle(OutEdge e, OutNode n)
    {
        var (from, to, far) = n == e.A ? (e.Curve.A, e.Curve.B, e.Curve.D) : (e.Curve.D, e.Curve.C, e.Curve.A);
        var d = to - from;
        if (Math.Abs(d.X) + Math.Abs(d.Z) < 1e-6) d = far - from;
        return Math.Atan2(d.Z, d.X);
    }

    static double AngleDiff(double a, double b)
    {
        double d = (b - a) * 180 / Math.PI;
        while (d > 180) d -= 360;
        while (d <= -180) d += 360;
        return d;
    }

    /// <summary>Rotates the control point at node n about n by deg degrees.</summary>
    static Bezier RotateAt(OutEdge e, OutNode n, double deg)
    {
        double r = deg * Math.PI / 180, c = Math.Cos(r), s = Math.Sin(r);
        Vec3 Rot(Vec3 origin, Vec3 p) { var v = p - origin; return origin + new Vec3(v.X * c - v.Z * s, v.Y, v.X * s + v.Z * c); }
        var b = e.Curve;
        return n == e.A ? b with { B = Rot(b.A, b.B) } : b with { C = Rot(b.D, b.C) };
    }

    /// <summary>
    /// Pairs of same-layer edges leaving one node at less than AcuteAngleDegrees:
    /// - duplicate: their far ends coincide (≤ 2 m apart, heights within 2 m) — the same course twice, e.g. a ground and an
    ///   elevated copy at a transition. The shorter-elevation copy is dropped and the kept edge takes the higher far-end elevation.
    /// - diverge/merge: real separate courses (ramp leaving or joining, on- and off-ramp sharing a road end, track switch).
    ///   Both stay; their tangents at the shared node are rotated apart to MinDivergeAngle so CS2 does not build one long
    ///   merged junction. A rotation that would create a hairpin is reverted.
    /// </summary>
    static void ResolveAcute(ConvertResult res, string layer, ConvertOptions o)
    {
        var edges = res.Layer(layer).ToList();
        var at = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges)
            foreach (var n in new[] { e.A, e.B })
            {
                if (!at.TryGetValue(n, out var l)) at[n] = l = new();
                l.Add(e);
            }

        var dropped = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        foreach (var (n, list) in at)
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = i + 1; j < list.Count; j++)
                {
                    var e1 = list[i]; var e2 = list[j];
                    if (dropped.Contains(e1) || dropped.Contains(e2)) continue;
                    double diff = AngleDiff(LeaveAngle(e1, n), LeaveAngle(e2, n));
                    if (Math.Abs(diff) >= o.AcuteAngleDegrees) continue;

                    var f1 = e1.A == n ? e1.B : e1.A; var f2 = e2.A == n ? e2.B : e2.A;
                    if (f1 == f2 || (f1.Position.DistXZ(f2.Position) <= 2 && Math.Abs(f1.Position.Y - f2.Position.Y) < 2))
                    {
                        var (keep, drop) = Math.Abs(f1.Elevation) >= Math.Abs(f2.Elevation) ? (e1, e2) : (e2, e1);
                        var kf = keep == e1 ? f1 : f2; var df = keep == e1 ? f2 : f1;
                        if (Math.Abs(df.Elevation) > Math.Abs(kf.Elevation)) kf.Elevation = df.Elevation;
                        dropped.Add(drop);
                        res.Fate[drop.Cs1Segment] = "duplicate transition piece";
                        res.AcuteResolutions.Add(new(layer, n.Id, keep.Id, drop.Id, Math.Abs(diff), double.NaN, "duplicate (dropped)"));
                        continue;
                    }

                    double need = (o.MinDivergeAngle - Math.Abs(diff)) / 2;
                    if (need <= 0) continue;
                    // Which way is "apart": by the current tangents, or by the far ends when the tangents coincide.
                    double sign = Math.Abs(diff) > 1e-3 ? Math.Sign(diff)
                        : Math.Sign(AngleDiff(Math.Atan2(f1.Position.Z - n.Position.Z, f1.Position.X - n.Position.X),
                                              Math.Atan2(f2.Position.Z - n.Position.Z, f2.Position.X - n.Position.X)));
                    if (sign == 0) sign = 1;
                    // A through edge (another edge leaves the node in the opposite direction) keeps its tangent; the ramp turns alone.
                    // Ties (both opposite the same edge): the one with the opposite edge's prefab continues, else the wider one.
                    bool Opposite(OutEdge e, bool samePrefab) => list.Any(x => x != e1 && x != e2
                        && Math.Abs(AngleDiff(LeaveAngle(e, n), LeaveAngle(x, n))) > 170
                        && (!samePrefab || x.Map.Cs2Prefab == e.Map.Cs2Prefab));
                    bool t1 = Opposite(e1, false), t2 = Opposite(e2, false);
                    if (t1 && t2) { t1 = Opposite(e1, true); t2 = Opposite(e2, true); }
                    if (t1 == t2 && Opposite(e1, false)) { t1 = e1.Info.HalfWidth > e2.Info.HalfWidth; t2 = e2.Info.HalfWidth > e1.Info.HalfWidth; }
                    double r1 = t1 && !t2 ? 0 : !t1 && t2 ? 2 * need : need, r2 = 2 * need - r1;
                    var c1 = RotateAt(e1, n, -sign * r1); var c2 = RotateAt(e2, n, sign * r2);
                    bool ok = !IsHairpin(c1, out _) && !IsHairpin(c2, out _);
                    if (ok) { e1.Curve = c1; e2.Curve = c2; }
                    double after = Math.Abs(AngleDiff(LeaveAngle(e1, n), LeaveAngle(e2, n)));
                    res.AcuteResolutions.Add(new(layer, n.Id, e1.Id, e2.Id, Math.Abs(diff), after, ok ? "diverge (tangents spread)" : "diverge (spread would fold; left)"));
                }
        }
        res.Stats.DuplicatesDropped += res.Edges.RemoveAll(dropped.Contains);
    }

    // ---------------------------------------------------------------- diagnostics

    static List<Vec3> Samples(Bezier b, double step)
    {
        int n = Math.Max(2, (int)Math.Ceiling(b.LengthXZ() / step));
        return Enumerable.Range(0, n + 1).Select(i => b.Eval(i / (double)n)).ToList();
    }

    public static double DistanceToPolyline(Vec3 p, IReadOnlyList<Vec3> line) => NearestOnPolyline(p, line).Distance;

    /// <summary>Horizontal distance to the polyline and the height of the nearest point on it.</summary>
    public static (double Distance, double Y) NearestOnPolyline(Vec3 p, IReadOnlyList<Vec3> line)
    {
        double best = double.MaxValue, bestY = p.Y;
        for (int i = 0; i + 1 < line.Count; i++)
        {
            var a = line[i]; var b = line[i + 1];
            double dx = b.X - a.X, dz = b.Z - a.Z, l2 = dx * dx + dz * dz;
            double t = l2 > 0 ? Math.Clamp(((p.X - a.X) * dx + (p.Z - a.Z) * dz) / l2, 0, 1) : 0;
            double ex = a.X + t * dx - p.X, ez = a.Z + t * dz - p.Z;
            double d2 = ex * ex + ez * ez;
            if (d2 < best) { best = d2; bestY = a.Y + t * (b.Y - a.Y); }
        }
        return (Math.Sqrt(best), bestY);
    }

    static bool Close(Vec3 p, IReadOnlyList<Vec3> line, double dist)
    {
        var (d, y) = NearestOnPolyline(p, line);
        return d < dist && Math.Abs(y - p.Y) < dist; // stacked pieces (one above the other) do not overlap
    }

    /// <summary>
    /// Same-layer edge pairs whose courses run within OverlapDistance of each other for more than OverlapLength
    /// (squeezed parallel carriageways, duplicated courses, ramps peeling off at a very shallow angle).
    /// </summary>
    static IEnumerable<Overlap> FindOverlaps(string layer, List<OutEdge> edges, ConvertOptions o)
    {
        const double step = 2;
        var samples = edges.Select(e => Samples(e.Curve, step)).ToList();
        double cell = 16;
        var grid = new Dictionary<(long, long), List<int>>();
        for (int i = 0; i < edges.Count; i++)
            foreach (var key in samples[i].Select(p => ((long)Math.Floor(p.X / cell), (long)Math.Floor(p.Z / cell))).Distinct())
            {
                if (!grid.TryGetValue(key, out var l)) grid[key] = l = new List<int>();
                l.Add(i);
            }

        var result = new List<Overlap>();
        for (int i = 0; i < edges.Count; i++)
        {
            var candidates = new HashSet<int>();
            foreach (var p in samples[i])
            {
                long cx = (long)Math.Floor(p.X / cell), cz = (long)Math.Floor(p.Z / cell);
                for (long dx = -1; dx <= 1; dx++)
                    for (long dz = -1; dz <= 1; dz++)
                        if (grid.TryGetValue((cx + dx, cz + dz), out var l))
                            foreach (var j in l) if (j > i) candidates.Add(j);
            }
            foreach (var j in candidates)
            {
                // Close length measured along edge i, and along j, take the smaller.
                double li = samples[i].Count(p => Close(p, samples[j], o.OverlapDistance)) * edges[i].Curve.LengthXZ() / samples[i].Count;
                if (li <= o.OverlapLength) continue;
                double lj = samples[j].Count(p => Close(p, samples[i], o.OverlapDistance)) * edges[j].Curve.LengthXZ() / samples[j].Count;
                double len = Math.Min(li, lj);
                if (len <= o.OverlapLength) continue;
                bool shared = edges[i].A == edges[j].A || edges[i].A == edges[j].B || edges[i].B == edges[j].A || edges[i].B == edges[j].B;
                result.Add(new Overlap(layer, edges[i], edges[j], len, shared));
            }
        }
        return result;
    }

    /// <summary>Pairs of edges leaving the same node at less than AcuteAngleDegrees (ramp diverges; CS2 builds teardrop junctions there).</summary>
    static IEnumerable<AcuteJunction> FindAcuteJunctions(string layer, List<OutEdge> edges, ConvertOptions o)
    {
        var at = new Dictionary<OutNode, List<(OutEdge E, double Angle)>>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges)
        {
            foreach (var (n, from, to) in new[] { (e.A, e.Curve.A, e.Curve.B), (e.B, e.Curve.D, e.Curve.C) })
            {
                var dir = to - from;
                if (Math.Abs(dir.X) + Math.Abs(dir.Z) < 1e-6) dir = (n == e.A ? e.Curve.D : e.Curve.A) - from;
                if (!at.TryGetValue(n, out var l)) at[n] = l = new();
                l.Add((e, Math.Atan2(dir.Z, dir.X)));
            }
        }
        foreach (var (n, list) in at)
            for (int i = 0; i < list.Count; i++)
                for (int j = i + 1; j < list.Count; j++)
                {
                    double d = Math.Abs(list[i].Angle - list[j].Angle) * 180 / Math.PI;
                    if (d > 180) d = 360 - d;
                    if (d < o.AcuteAngleDegrees) yield return new AcuteJunction(layer, n, list[i].E, list[j].E, d);
                }
    }
}
