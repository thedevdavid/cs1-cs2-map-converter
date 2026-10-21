namespace NetConverter;

/// <summary>
/// Round 34e (owner: "use the vanilla pylon props, and make it partially open", next-run/ref-gallery-1/2.webp, alpine avalanche galleries): the CS1 JP Rock
/// shed (~440 m over the 4-lane National Road at x −6453…−6245, z 677…1069) as an open-sided gallery, one undoable import (`rock-shed`), absolute y, no sink:
/// - hill side closed: ConcreteSet01 `Wall32x16` (32 × 16 × 1 m, pivot centre-bottom) from road level, every 32 m;
/// - roof: `BasePlateConcrete32` (32 × 0.2 × 32 m) bays centred over the road at road y + 6.5 m, each at its own height; where two bays would step more than
///   1 m, two `BasePlateConcrete16` halves;
/// - valley side open: a slim column every 8 m on the valley edge, from road level up to the slab underside (<see cref="Column"/>: the vanilla pillar prop,
///   to be confirmed live; ConcreteSet01 Pillar2x8 until then);
/// - the 11 CS1 `rock_c` positions: cliff pieces behind the wall on the hill side (no rock on the roof).
/// - guardrail: none yet (no vanilla guardrail prop or net confirmed).
/// </summary>
public static class RockShed
{
    /// <summary>The valley-side column prop and its height (m, pivot at the base). One place to change once the vanilla pillar prop is measured live.</summary>
    public const string Column = "Pillar2x8";
    public const double ColumnHeight = 8.0;
    public const double Bay = 32, ColumnStep = 8, Clear = 6.5;

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        var road = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab == "Medium Road")
            .Where(e => shed.Any(r => Enumerable.Range(0, 9).Select(k => e.Curve.Eval(k / 8.0)).Any(p => Math.Sqrt((p.X - r.X) * (p.X - r.X) + (p.Z - r.Z) * (p.Z - r.Z)) < 40))).ToList();
        if (road.Count == 0) return (res, "rock shed: no National Road course under the shed");
        var pts = new List<(double X, double Z, double Y, double Tx, double Tz)>();
        foreach (var e in road)
            for (int k = 0; k <= 64; k++)
            {
                double t = k / 64.0; var p = e.Curve.Eval(t); var q = e.Curve.Eval(Math.Min(1, t + 0.01)); var o = e.Curve.Eval(Math.Max(0, t - 0.01));
                double tx = q.X - o.X, tz = q.Z - o.Z, tl = Math.Sqrt(tx * tx + tz * tz);
                pts.Add((p.X, p.Z, Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A)), tx / tl, tz / tl));
            }
        var a = shed.MinBy(r => r.Z)!; var b = shed.MaxBy(r => r.Z)!;
        double ax = b.X - a.X, az = b.Z - a.Z, al = Math.Sqrt(ax * ax + az * az); ax /= al; az /= al;
        double S(double x, double z) => (x - a.X) * ax + (z - a.Z) * az;
        var along = pts.Select(p => (P: p, S: S(p.X, p.Z))).Where(q => q.S >= -40 && q.S <= al + 40).OrderBy(q => q.S).ToList();
        (double X, double Z, double Y, double Tx, double Tz) At(double s) => along.MinBy(q => Math.Abs(q.S - s)).P;
        double half = FootprintShift.Cs2Half(road[0]);
        double Terr(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : double.MinValue;
        var m0 = At(al / 2); double nx = -m0.Tz, nz = m0.Tx;   // uphill normal
        if (terrainTrue != null && Terr(m0.X + nx * 30, m0.Z + nz * 30) < Terr(m0.X - nx * 30, m0.Z - nz * 30)) { nx = -nx; nz = -nz; }
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note) =>
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (open gallery)", note);
        int full = 0, halves = 0, walls = 0, cols = 0;
        double? prevY = null; var roofAt = new List<(double S0, double S1, double Y)>();
        for (double s = 0; s < al; s += Bay)
        {
            var c = At(s + Bay / 2); double y = c.Y + Clear;
            if (prevY is { } py && Math.Abs(y - py) > 1)
                foreach (var hs in new[] { s, s + Bay / 2 })
                {
                    var h = At(hs + Bay / 4); double hy = h.Y + Clear;
                    res.Add(O($"shed_roof_{full + halves + 1}", "BasePlateConcrete16", h.X, h.Z, hy, Yaw(h.Tx, h.Tz), "roof half bay 16 m at road + 6.5 m (step > 1 m)"));
                    roofAt.Add((hs, hs + Bay / 2, hy)); halves++;
                }
            else { res.Add(O($"shed_roof_{full + halves + 1}", "BasePlateConcrete32", c.X, c.Z, y, Yaw(c.Tx, c.Tz), "roof bay 32 m at road + 6.5 m")); roofAt.Add((s, s + Bay, y)); full++; }
            prevY = y;
            res.Add(O($"shed_wall_{++walls}", "Wall32x16", c.X + nx * (half + 1), c.Z + nz * (half + 1), c.Y, Yaw(c.Tx, c.Tz) - 90, "hill-side wall from road level (32 m side along the road)"));
        }
        // Valley-side columns every 8 m, base at road level, top at the slab underside (the prop's own height decides; sunk below road level if it is taller).
        for (double s = 0; s <= al + 0.01; s += ColumnStep)
        {
            var c = At(s); double underside = (roofAt.FirstOrDefault(r => s >= r.S0 - 0.01 && s <= r.S1 + 0.01) is { S1: > 0 } r ? r.Y : c.Y + Clear);
            res.Add(O($"shed_col_{++cols}", Column, c.X - nx * (half + 0.5), c.Z - nz * (half + 0.5), underside - ColumnHeight, Yaw(c.Tx, c.Tz), $"open valley-side column, top at the roof underside ({Column})"));
        }
        // The CS1 rock_c pieces as rock behind the wall on the hill side.
        int rocks = 0;
        foreach (var r in shed)
        {
            var c = At(S(r.X, r.Z));
            res.Add(O($"shed_rock_{r.Id}", "CliffRock5", c.X + nx * (half + 12), c.Z + nz * (half + 12), c.Y + 2.57, Yaw(c.Tx, c.Tz), $"rock behind the hill-side wall at CS1 rock_c {r.Id}"));
            rocks++;
        }
        return (res, FormattableString.Invariant($"rock shed open gallery over {al:F0} m of {string.Join(", ", road.Select(e => e.Id))}: roof {full} BasePlateConcrete32 + {halves} BasePlateConcrete16 at road + 6.5 m, {walls} Wall32x16 on the hill side, {cols} valley-side columns ({Column}, every 8 m), {rocks} CliffRock5 behind the wall; no guardrail"));
    }
}
