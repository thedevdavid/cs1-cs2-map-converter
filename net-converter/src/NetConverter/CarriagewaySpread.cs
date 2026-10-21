namespace NetConverter;

/// <summary>
/// Round 35 (A-v10: the BR001 south approach pieces e16410, e24407, e31688, e35437, e35648 were not built): CS1's USRP carriageways run 16.3 m apart, but
/// two CS2 3-lane one-way highways need 18 m between centre lines (the bridge's "course runs inside the existing segment" check, 0.9.39). Parallel one-way
/// highway courses (angle under 15°, no shared node) closer than their two half-widths + Margin are pushed to the half-widths + Margin, each by half the deficit, perpendicular to
/// its direction; the nodes move and every course at those nodes follows. Border nodes stay.
/// Round 36 (A-v11, the same nine pieces again): the threshold is now the half-widths + Margin. CS1 runs the BR001 approach carriageways 16.3 m apart, 0.3 m
/// more than the two 8 m half-widths, so the pass above never moved them. In A-v10 and A-v11 every missing piece was the later-built one of such a 0.3 m pair
/// (9 of the 15 pairs); no pair with 2 m or more lost a piece. The bridge does not name the cause: its overlap check skips segments of the same prefab.
/// Only opposite-direction pairs get the margin; a ramp running beside its mainline after a diverge keeps the old rule, so junctions are not bent.
/// </summary>
public static class CarriagewaySpread
{
    /// <summary>Clearance between the two decks, beyond the half-widths, that the opposite carriageways of one road are pushed to (same-direction pairs: 0.3 m, only when they overlap).</summary>
    public const double Margin = 2.0;

    public static List<string> Apply(ConvertResult res, int passes = 6)
    {
        var log = new List<string>();
        for (int pass = 0; pass < passes; pass++)
        {
            var hw = res.Edges.Where(e => e.Map.Imported && e.Info.OneWay && (e.Map.Cs2Prefab ?? "").StartsWith("Highway")).ToList();
            var cells = new Dictionary<(int, int), List<OutEdge>>();
            foreach (var e in hw) { var m = e.Curve.Eval(0.5); var k = ((int)Math.Floor(m.X / 64), (int)Math.Floor(m.Z / 64)); if (!cells.TryGetValue(k, out var l)) cells[k] = l = new(); l.Add(e); }
            var push = new Dictionary<OutNode, (double X, double Z, int N)>(ReferenceEqualityComparer.Instance);
            int pairs = 0;
            foreach (var e in hw)
            {
                var m = e.Curve.Eval(0.5); double tx = e.B.Position.X - e.A.Position.X, tz = e.B.Position.Z - e.A.Position.Z, tl = Math.Sqrt(tx * tx + tz * tz);
                if (tl < 10) continue;
                tx /= tl; tz /= tl;
                for (int a = -1; a <= 1; a++) for (int b = -1; b <= 1; b++)
                    if (cells.TryGetValue(((int)Math.Floor(m.X / 64) + a, (int)Math.Floor(m.Z / 64) + b), out var l))
                        foreach (var f in l)
                        {
                            if (f == e || f.A == e.A || f.A == e.B || f.B == e.A || f.B == e.B) continue;
                            double fx = f.B.Position.X - f.A.Position.X, fz = f.B.Position.Z - f.A.Position.Z, fl = Math.Sqrt(fx * fx + fz * fz);
                            double dot = tx * fx / fl + tz * fz / fl;
                            if (fl < 10 || Math.Abs(dot) < Math.Cos(15 * Math.PI / 180)) continue;
                            bool opposite = dot < 0;                                          // traffic the other way: the far carriageway
                            var fm = f.Curve.Eval(0.5);
                            double nx = -tz, nz = tx, side = (fm.X - m.X) * nx + (fm.Z - m.Z) * nz, along = (fm.X - m.X) * tx + (fm.Z - m.Z) * tz;
                            if (Math.Abs(along) > tl / 2) continue;                          // not beside each other
                            if (Math.Abs(f.Curve.Eval(0.5).Y - m.Y) * Outputs.YScale > 6) continue;   // stacked, not side by side
                            double d = Math.Abs(side), need = FootprintShift.Cs2Half(e) + FootprintShift.Cs2Half(f);   // the bridge's own threshold (its drop messages)
                            if (d >= need && !opposite) continue;                              // a ramp beside its mainline: only a real overlap
                            need += opposite ? Margin : 0.3;                                    // the two carriageways of one road: clear of it (round 36)
                            if (d >= need - 0.05 || d < 3) continue;
                            double s = -(need - d) / 2 * Math.Sign(side);                    // e moves away from f
                            foreach (var n in new[] { e.A, e.B }) { var c = push.GetValueOrDefault(n); push[n] = (c.X + nx * s, c.Z + nz * s, c.N + 1); }
                            pairs++;
                        }
            }
            int moved = 0;
            foreach (var (n, c) in push)
            {
                if (n.Border) continue;   // Pinned fixes a height, not the plan position
                var off = new Vec3(c.X / c.N, 0, c.Z / c.N);
                n.Position = n.Position + off; moved++;
            }
            foreach (var e in res.Edges.Where(e => push.ContainsKey(e.A) || push.ContainsKey(e.B))) e.Curve = e.Curve.WithEnds(e.A.Position, e.B.Position);
            log.Add($"pass {pass + 1}: {pairs / 2} parallel carriageway pairs closer than their two half-widths (+ {Margin} m for opposite carriageways), {moved} nodes moved apart");
            if (pairs == 0) break;
        }
        return log;
    }
}
