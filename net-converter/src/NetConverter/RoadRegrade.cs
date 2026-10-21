using System.Text.Json;

namespace NetConverter;

/// <summary>
/// Round 42 (owner, A-v16 review: "the roof should follow the road below. same height calculated from the road always. to make this easy and good looking,
/// you can slope or straighten that section of land &amp; road"): the National Road under the JP Rock shed on one straight grade.
/// - The course: the Medium Road edges under the shed, extended along the same road (degree-2 continuation) to Transition + Fit m past the shells.
/// - A least-squares line through the node heights from Fit m before the first CS1 shell centre to Fit m after the last; nodes in that range take it.
/// - Transition m on either side blend from the line back to the existing heights (smoothstep on arc length).
/// - Only node heights change (Elevation = new − Terrain); x/z, prefabs and topology stay. Every network file is written from the same nodes, and the
///   rock shed samples the road from them.
/// - The playable heightmap is not changed here: <see cref="WriteCorridor"/> writes the corridor for hm-rebuild (centreline every 8 m with the new y,
///   the half-width to flatten, and the hill-side bench width measured on the current heightmap so the cut face moves with the road).
/// </summary>
public static class RoadRegrade
{
    public const double Fit = 120, Transition = 120, CorridorStep = 8;

    public sealed record Result(List<(OutNode Node, double S, double Old, double New)> Nodes, List<(double S, double X, double Z, double OldY, double NewY)> Line,
        double SA, double SB, double Grade, double HalfWidth, double Side, string Log);

    /// <summary>Regrades the shed road in place (internal heights: CS2 metres at the topology factor). Null when there is no shed or road.</summary>
    public static Result? Apply(ConvertResult res, List<PoStructures.Row> rows, Func<double, double, double>? terrainTrue, double topologyFactor)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        if (shed.Count == 0) return null;
        bool Road(OutEdge e) => RockShed.IsShedRoad(e);
        double Near(OutEdge e, double x, double z) => Enumerable.Range(0, 17).Min(k => e.Curve.Eval(k / 16.0).DistXZ(new Vec3(x, 0, z)));
        var chain = res.Edges.Where(e => Road(e) && shed.Any(r => Near(e, r.X, r.Z) < 40)).ToList();
        if (chain.Count == 0) return null;
        var byNode = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in res.Edges.Where(Road)) foreach (var n in new[] { e.A, e.B }) { if (!byNode.TryGetValue(n, out var l)) byNode[n] = l = new(); l.Add(e); }
        // Order the shed edges into one path, then extend both ends along the same road until Fit + Transition past the outermost shells.
        var path = OrderPath(chain);
        if (path == null) return null;
        double reach = Fit + Transition + 60;
        for (int end = 0; end < 2; end++)
            for (double added = 0; added < reach;)
            {
                var (edge, node) = end == 0 ? (path.Edges[0], path.Nodes[0]) : (path.Edges[^1], path.Nodes[^1]);
                var next = byNode.GetValueOrDefault(node)?.Where(x => x != edge && !path.Edges.Contains(x)).ToList();
                if (next == null || next.Count != 1) break;
                var e = next[0]; var other = e.A == node ? e.B : e.A;
                if (end == 0) { path.Edges.Insert(0, e); path.Nodes.Insert(0, other); } else { path.Edges.Add(e); path.Nodes.Add(other); }
                added += e.Curve.LengthXZ();
            }
        // Arc length per node.
        var s = new List<double> { 0 };
        for (int i = 0; i < path.Edges.Count; i++) s.Add(s[^1] + path.Edges[i].Curve.LengthXZ());
        (double S, double X, double Z) Proj(double x, double z)
        {
            double best = double.MaxValue, bs = 0, bx = 0, bz = 0;
            for (int i = 0; i < path.Edges.Count; i++)
            {
                var e = path.Edges[i]; bool fwd = e.A == path.Nodes[i]; double len = s[i + 1] - s[i];
                for (int k = 0; k <= 32; k++) { double t = k / 32.0; var p = e.Curve.Eval(fwd ? t : 1 - t); double d = p.DistXZ(new Vec3(x, 0, z)); if (d < best) { best = d; bs = s[i] + t * len; bx = p.X; bz = p.Z; } }
            }
            return (bs, bx, bz);
        }
        var shellS = shed.Select(r => Proj(r.X, r.Z).S).ToList();
        double sA = shellS.Min(), sB = shellS.Max();
        double H(OutNode n) => n.Terrain + n.Elevation;
        // Least squares through the node heights in [sA − Fit, sB + Fit].
        var fitPts = path.Nodes.Select((n, i) => (S: s[i], Y: H(n))).Where(p => p.S >= sA - Fit && p.S <= sB + Fit).ToList();
        if (fitPts.Count < 2) return null;
        double mS = fitPts.Average(p => p.S), mY = fitPts.Average(p => p.Y);
        double slope = fitPts.Sum(p => (p.S - mS) * (p.Y - mY)) / fitPts.Sum(p => (p.S - mS) * (p.S - mS)), icpt = mY - slope * mS;
        // When the road does not continue far enough on both sides for the transitions (here it ends in junctions with one-way highway ramps 70 m before
        // the first shell and 17 m after the last), the line runs through the two end nodes instead: they keep their heights, so nothing outside the
        // stretch moves and no transition is needed (owner's alternative: "or end heights if that keeps cut/fill small").
        bool endLine = s[0] > sA - Fit - Transition || s[^1] < sB + Fit + Transition;
        string mode = "least squares through the node heights over " + FormattableString.Invariant($"{sA - Fit:F0}…{sB + Fit:F0} m with {Transition} m transitions");
        // Then: least squares through the node heights under the roof (shells ± RockShed.ShellHalf), anchored at a junction node that lies under the roof
        // (it keeps its height, so the ramps there stay as they are); nodes outside the roof keep their heights, the grade changes at the first node under it.
        double c0 = sA - RockShed.ShellHalf, c1 = sB + RockShed.ShellHalf;
        bool InCore(int i) => s[i] >= c0 - 0.01 && s[i] <= c1 + 0.01;
        if (endLine)
        {
            // The outermost nodes under the roof keep their heights (each is a junction, or the end of an approach edge that stays as it is), and the
            // nodes between them go on the straight line through those two: the road under the roof is one grade from the first to the last of them.
            var core = Enumerable.Range(0, path.Nodes.Count).Where(InCore).ToList();
            int i0 = core.First(), i1 = core.Last();
            slope = (H(path.Nodes[i1]) - H(path.Nodes[i0])) / (s[i1] - s[i0]); icpt = H(path.Nodes[i0]) - slope * s[i0];
            mode = FormattableString.Invariant($"through the outermost nodes under the roof, {path.Nodes[i0].Id} and {path.Nodes[i1].Id} ({s[i0]:F0} and {s[i1]:F0} m; roof {c0:F0}…{c1:F0} m), which keep their heights; the road ends in junctions {sA - s[0]:F0} m before the first shell and {s[^1] - sB:F0} m after the last (no room for {Transition} m transitions), so nodes outside keep theirs and the roof's first {Math.Max(0, s[i0] - c0):F0} m and last {Math.Max(0, c1 - s[i1]):F0} m lie over unchanged road");
        }
        double Blend(double si)
        {
            if (endLine) return si >= c0 - 0.01 && si <= c1 + 0.01 ? 1 : 0;
            double u = si < sA - Fit ? (si - (sA - Fit - Transition)) / Transition : si > sB + Fit ? ((sB + Fit + Transition) - si) / Transition : 1;
            u = Math.Clamp(u, 0, 1); return u * u * (3 - 2 * u);
        }
        var nodes = new List<(OutNode, double, double, double)>();
        for (int i = 0; i < path.Nodes.Count; i++)
        {
            var n = path.Nodes[i]; double old = H(n), w = Blend(s[i]);
            if (w <= 0) continue;
            double neu = old + w * (icpt + slope * s[i] - old);
            if (n.Border) continue;
            nodes.Add((n, s[i], old, neu));
        }
        foreach (var (n, _, _, neu) in nodes) { n.Elevation = Math.Round(neu - n.Terrain, 3); n.Position = n.Position with { Y = neu }; }
        // Edge curves follow the node heights (the inner control points keep their x/z; their y on the chord).
        foreach (var e in path.Edges.Concat(nodes.SelectMany(q => byNode.GetValueOrDefault(q.Item1) ?? [])).Distinct())
        {
            double ya = H(e.A), yb = H(e.B);
            e.Curve = new Bezier(e.Curve.A with { Y = ya }, e.Curve.B with { Y = ya + (yb - ya) / 3 }, e.Curve.C with { Y = ya + 2 * (yb - ya) / 3 }, e.Curve.D with { Y = yb });
        }
        // Centreline every 8 m over the regraded stretch, old and new heights (internal units).
        var line = new List<(double, double, double, double, double)>();
        double s0 = sA - Fit - Transition, s1 = sB + Fit + Transition;
        for (double si = Math.Max(0, s0); si <= Math.Min(s[^1], s1) + 0.01; si += CorridorStep)
        {
            int i = Math.Clamp(s.FindIndex(v => v >= si) - 1, 0, path.Edges.Count - 1);
            var e = path.Edges[i]; bool fwd = e.A == path.Nodes[i]; double t = (si - s[i]) / Math.Max(1e-6, s[i + 1] - s[i]);
            var p = e.Curve.Eval(fwd ? t : 1 - t);
            double oldY = 0, newY = H(path.Nodes[i]) + t * (H(path.Nodes[i + 1]) - H(path.Nodes[i]));
            var a = nodes.FirstOrDefault(q => q.Item1 == path.Nodes[i]); var b = nodes.FirstOrDefault(q => q.Item1 == path.Nodes[i + 1]);
            double oa = a.Item1 != null ? a.Item3 : H(path.Nodes[i]), ob = b.Item1 != null ? b.Item3 : H(path.Nodes[i + 1]);
            oldY = oa + t * (ob - oa);
            line.Add((si, p.X, p.Z, oldY, newY));
        }
        // Hill side: the side where the terrain 30 m off the road is higher, at the shed's middle.
        double side = 1;
        if (terrainTrue != null)
        {
            var mid = line.MinBy(q => Math.Abs(q.Item1 - (sA + sB) / 2)); var nxt = line.MinBy(q => Math.Abs(q.Item1 - ((sA + sB) / 2 + 8)));
            double tx = nxt.Item2 - mid.Item2, tz = nxt.Item3 - mid.Item3, tl = Math.Sqrt(tx * tx + tz * tz); tx /= tl; tz /= tl;
            if (terrainTrue(mid.Item2 - tz * 30, mid.Item3 + tx * 30) < terrainTrue(mid.Item2 + tz * 30, mid.Item3 - tx * 30)) side = -1;
        }
        double half = FootprintShift.Cs2Half(chain[0]);
        double cut = nodes.Count == 0 ? 0 : nodes.Max(q => q.Item3 - q.Item4), fill = nodes.Count == 0 ? 0 : nodes.Max(q => q.Item4 - q.Item3);
        string log = FormattableString.Invariant($"{path.Edges.Count} edges ({string.Join(", ", path.Edges.Select(e => e.Id))}), shells at arc {sA:F0}…{sB:F0} m of {s[^1]:F0}; line {mode}, grade {slope / topologyFactor * 100:F2} % in true metres ({Math.Atan(slope / topologyFactor) * 180 / Math.PI:F2}°), {nodes.Count(q => Math.Abs(q.Item4 - q.Item3) > 0.005)} nodes moved, cut max {cut / topologyFactor:F2} m, fill max {fill / topologyFactor:F2} m (true metres)");
        return new Result(nodes, line, sA, sB, slope, half, side, log);
    }

    sealed class PathOf { public List<OutEdge> Edges = new(); public List<OutNode> Nodes = new(); }

    static PathOf? OrderPath(List<OutEdge> edges)
    {
        var deg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges) foreach (var n in new[] { e.A, e.B }) deg[n] = deg.GetValueOrDefault(n) + 1;
        var start = deg.FirstOrDefault(kv => kv.Value == 1).Key;
        if (start == null) return null;
        var p = new PathOf(); p.Nodes.Add(start); var left = edges.ToList();
        while (left.Count > 0)
        {
            var n = p.Nodes[^1]; var e = left.FirstOrDefault(x => x.A == n || x.B == n);
            if (e == null) return null;
            left.Remove(e); p.Edges.Add(e); p.Nodes.Add(e.A == n ? e.B : e.A);
        }
        return p;
    }

    /// <summary>
    /// The playable terrain as hm-rebuild will leave it after <see cref="WriteCorridor"/> (true metres): within the flatten half-width of the centreline
    /// the new road height; out to 100 m on either side the current terrain shifted by the road's change (the cut face moves with the road); beyond, as is.
    /// The rock shed's columns, roof depth and nearby rocks are placed on this, so they fit once the heightmap is regraded.
    /// </summary>
    public static Func<double, double, double> PredictTerrain(Result r, Func<double, double, double> terrainTrue, double topologyFactor) => (x, z) =>
    {
        double t = terrainTrue(x, z);
        if (r.Line.Count == 0) return t;
        var best = r.Line.MinBy(q => (q.X - x) * (q.X - x) + (q.Z - z) * (q.Z - z));
        double d = Math.Sqrt((best.X - x) * (best.X - x) + (best.Z - z) * (best.Z - z)), dy = (best.NewY - best.OldY) / topologyFactor;
        if (d <= r.HalfWidth + 2) return best.NewY / topologyFactor;
        return d <= 100 ? t + dy * Math.Clamp((100 - d) / 20, 0, 1) : t;
    };

    /// <summary>
    /// The corridor for hm-rebuild (true metres): centreline every 8 m with the old and new road y, the half-width to flatten to the road (road half
    /// width + 2 m), and per point the hill-side bench width on the current playable heightmap (road edge → where the terrain is 3 m above the old road),
    /// so the cut face can move with the road.
    /// </summary>
    public static void WriteCorridor(string path, Result r, Func<double, double, double>? terrainTrue, double topologyFactor, string heightmapName)
    {
        double flatten = r.HalfWidth + 2;
        var pts = new List<object>();
        for (int i = 0; i < r.Line.Count; i++)
        {
            var (si, x, z, oldY, newY) = r.Line[i];
            var (_, x2, z2, _, _) = r.Line[Math.Min(i + 1, r.Line.Count - 1)]; var (_, x0, z0, _, _) = r.Line[Math.Max(i - 1, 0)];
            double tx = x2 - x0, tz = z2 - z0, tl = Math.Sqrt(tx * tx + tz * tz); tx /= tl; tz /= tl;
            double oT = oldY / topologyFactor, nT = newY / topologyFactor;
            double? bench = null;
            if (terrainTrue != null)
                for (double d = r.HalfWidth; d <= r.HalfWidth + 80; d += 0.5)
                    if (terrainTrue(x - tz * r.Side * d, z + tx * r.Side * d) >= oT + 3) { bench = Math.Round(d - r.HalfWidth, 1); break; }
            pts.Add(new { s = Math.Round(si - r.SA, 1), x = Math.Round(x, 2), z = Math.Round(z, 2), oldY = Math.Round(oT, 2), y = Math.Round(nT, 2), hillBench = bench,
                hill = new[] { Math.Round(-tz * r.Side, 4), Math.Round(tx * r.Side, 4) } });
        }
        var doc = new
        {
            name = "regrade-rock-shed",
            _comment = "Round 42: the National Road under the JP Rock shed on one straight grade. For hm-rebuild: within halfWidth of the centreline set the terrain to y " +
                "(true metres); on the hill side (unit vector 'hill', x/z) keep a bench of hillBench metres beyond the half-width at y, then the cut face as it is now, " +
                "shifted by (y − oldY). s = arc length from the first CS1 shell centre (negative before it). Points every 8 m, transitions included.",
            heightmap = heightmapName,
            grade = Math.Round(r.Grade / topologyFactor, 5),
            halfWidth = Math.Round(flatten, 1),
            roadHalfWidth = Math.Round(r.HalfWidth, 1),
            shells = new { first = 0.0, last = Math.Round(r.SB - r.SA, 1) },
            points = pts,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
    }
}
