namespace NetConverter;

/// <summary>
/// Round 32 (owner, A-v8 vs the CS1 reference shot of interchange 5): CS1 draws every elevated (bridge-AI) road piece on pillars, while CS2 draws a raised
/// course below 2 × its elevation limit (8 m for the highways) as an embankment between retaining walls. So every course that was an elevated piece in CS1
/// is lifted to at least <see cref="Target"/> output metres above the terrain, as far as the grade allows: nodes where an elevated chain meets a ground or
/// tunnel course (and border nodes) stay where they are, and the lift ramps up from them at ≤ 8 % (output), shortest path along the elevated courses.
/// A node is never lowered.
/// </summary>
public static class FlyoverLift
{
    public const double Target = 8.5, Grade = 0.08;

    public sealed record Lift(OutNode Node, double Before, double After);

    public static List<Lift> Apply(ConvertResult res, out List<(OutEdge Edge, double From, double To)> edges)
    {
        double ys = Outputs.YScale;
        var net = res.Edges.Where(e => e.Map.Imported).ToList();
        var elevated = net.Where(e => e.AiStructure == Structure.Elevated && e.Layer != Layers.Rail).ToList();
        var adj = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in net) foreach (var n in new[] { e.A, e.B }) (adj.TryGetValue(n, out var l) ? l : adj[n] = new()).Add(e);
        var elevSet = elevated.ToHashSet(ReferenceEqualityComparer.Instance);
        var nodes = elevated.SelectMany(e => new[] { e.A, e.B }).Distinct(ReferenceEqualityComparer.Instance).Cast<OutNode>().ToList();
        // Anchors: a node that also carries a non-elevated course (the ramp touches down there), a border node, or a tunnel node.
        bool Anchor(OutNode n) => n.Border || n.Elevation < 0 || adj[n].Any(e => !elevSet.Contains(e));
        // Shortest distance from any anchor along elevated courses, and the anchor's output height there.
        var dist = new Dictionary<OutNode, (double D, double Y)>(ReferenceEqualityComparer.Instance);
        var pq = new PriorityQueue<OutNode, double>();
        foreach (var n in nodes.Where(Anchor)) { dist[n] = (0, Round9.Y(n)); pq.Enqueue(n, 0); }
        while (pq.TryDequeue(out var n, out var dn))
        {
            if (dist[n].D < dn) continue;
            foreach (var e in adj[n].Where(elevSet.Contains))
            {
                var m = e.A == n ? e.B : e.A; double d = dn + e.Curve.LengthXZ();
                if (dist.TryGetValue(m, out var dm) && dm.D <= d) continue;
                dist[m] = (d, dist[n].Y); pq.Enqueue(m, d);
            }
        }
        var lifts = new List<Lift>();
        foreach (var n in nodes)
        {
            if (Anchor(n)) continue;
            double terrOut = n.Terrain * ys, cur = Round9.Y(n), want = terrOut + Target;
            if (dist.TryGetValue(n, out var a)) want = Math.Min(want, a.Y + Grade * a.D);   // the grade from the nearest touch-down
            if (want <= cur + 0.05) continue;
            double up = (want - cur) / ys;   // topology metres
            lifts.Add(new Lift(n, cur - terrOut, want - terrOut));
            n.Elevation += up; n.Position = n.Position with { Y = n.Position.Y + up };
        }
        var lifted = lifts.Select(l => l.Node).ToHashSet(ReferenceEqualityComparer.Instance);
        edges = new();
        foreach (var e in net.Where(e => lifted.Contains(e.A) || lifted.Contains(e.B)))
        {
            e.Curve = e.Curve.WithEnds(e.A.Position, e.B.Position);
            if (e.Structure == Structure.Ground) e.Structure = Structure.Elevated;
        }
        foreach (var e in elevated.Where(e => lifted.Contains(e.A) || lifted.Contains(e.B)))
        {
            double Before(OutNode n) => lifts.FirstOrDefault(l => l.Node == n)?.Before ?? (Round9.Y(n) - n.Terrain * ys);
            edges.Add((e, Math.Max(Before(e.A), Before(e.B)), Math.Max(Round9.Y(e.A) - e.A.Terrain * ys, Round9.Y(e.B) - e.B.Terrain * ys)));
        }
        return lifts;
    }
}
