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
    /// <summary>Crossings where the clearance guard took lift back (last Apply).</summary>
    public static int GuardedCrossings { get; private set; }

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
        // Clearance guard (round 32c): a lifted course must not come closer than min(its CS1 clearance, 7 m) to a course crossing above it (A-v8 check:
        // e12097_0 under e6041_0b at (3646, 2130) dropped from 7.7 m to 5.8 m). The deficit comes off the lower course's lifted ends, never below their
        // original height. Two passes (a node can sit under two crossings).
        var liftOf = lifts.ToDictionary(l => l.Node, l => l, ReferenceEqualityComparer.Instance);
        var touched = net.Where(e => liftOf.ContainsKey(e.A) || liftOf.ContainsKey(e.B)).ToList();
        var samples = net.ToDictionary(e => e, e => Enumerable.Range(0, 17).Select(k => e.Curve.Eval(k / 16.0)).ToArray(), ReferenceEqualityComparer.Instance);
        var cells = new Dictionary<(int, int), List<OutEdge>>();
        foreach (var e in net) foreach (var p in samples[e]) { var key = ((int)Math.Floor(p.X / 64), (int)Math.Floor(p.Z / 64)); if (!cells.TryGetValue(key, out var l)) cells[key] = l = new(); if (l.Count == 0 || l[^1] != e) l.Add(e); }
        double YAt(OutEdge e, double t) => Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A));
        int guarded = 0;
        for (int pass = 0; pass < 2; pass++)
            foreach (var e in touched)
            {
                var pe = samples[e]; var seen = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
                foreach (var p in pe)
                    if (cells.TryGetValue(((int)Math.Floor(p.X / 64), (int)Math.Floor(p.Z / 64)), out var l))
                        foreach (var f in l)
                        {
                            if (f == e || !seen.Add(f) || f.A == e.A || f.A == e.B || f.B == e.A || f.B == e.B) continue;
                            var pf = samples[f];
                            for (int i = 0; i < 16; i++) for (int j = 0; j < 16; j++)
                            {
                                double d1x = pe[i + 1].X - pe[i].X, d1z = pe[i + 1].Z - pe[i].Z, d2x = pf[j + 1].X - pf[j].X, d2z = pf[j + 1].Z - pf[j].Z, den = d1x * d2z - d1z * d2x;
                                if (Math.Abs(den) < 1e-9) continue;
                                double u = ((pf[j].X - pe[i].X) * d2z - (pf[j].Z - pe[i].Z) * d2x) / den, v = ((pf[j].X - pe[i].X) * d1z - (pf[j].Z - pe[i].Z) * d1x) / den;
                                if (u < 0 || u > 1 || v < 0 || v > 1) continue;
                                double te = (i + u) / 16, tf = (j + v) / 16, ye = YAt(e, te), yf = YAt(f, tf);
                                if (yf <= ye) continue;                                                    // e is the upper one here
                                double want = 7;
                                double before = (liftOf.TryGetValue(e.A, out var a0) ? a0.Before + e.A.Terrain * ys : Round9.Y(e.A)) * (1 - te) + (liftOf.TryGetValue(e.B, out var b0) ? b0.Before + e.B.Terrain * ys : Round9.Y(e.B)) * te;
                                double gapBefore = yf - before;                                            // the clearance before the lift
                                double target = Math.Min(want, gapBefore);
                                double deficit = target - (yf - ye);
                                if (deficit <= 0.05) continue;
                                foreach (var n in new[] { e.A, e.B })
                                {
                                    if (!liftOf.TryGetValue(n, out var ln)) continue;
                                    double room = (Round9.Y(n) - (ln.Before + n.Terrain * ys));            // how much this node was lifted
                                    double down = Math.Min(room, deficit);
                                    if (down <= 0) continue;
                                    n.Elevation -= down / ys; n.Position = n.Position with { Y = n.Position.Y - down / ys };
                                    liftOf[n] = ln with { After = ln.After - down };
                                }
                                guarded++;
                            }
                        }
            }
        lifts = lifts.Select(l => liftOf[l.Node]).ToList();
        GuardedCrossings = guarded;
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
