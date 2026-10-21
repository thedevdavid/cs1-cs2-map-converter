namespace NetConverter;

/// <summary>
/// Round 34 (owner, A-v9 shot 24: the covered road): the CS1 JP Rock shed (11 `rock_c` PO pieces, a ~440 m rock-faced gallery over the 4-lane National
/// Road at x −6453…−6245, z 677…1069) has no CS2 asset (bridges/ROCK-SHED.md: no rock shed, gallery or open-side tunnel net), so it is built as a ConcreteSet01
/// gallery along the CS1 road line, one undoable import (`rock-shed`), absolute y, no sink. All ConcreteSet01 pivots are centre-bottom (boundsMin y 0).
/// - roof: `BasePlateConcrete32` (32 × 0.2 × 32 m) bays centred over the road at road y + 6 m, each bay at its own height along the grade; where two bays
///   would step more than 1 m, two `BasePlateConcrete16` halves instead;
/// - valley side: `Pillar2x8` under each slab edge (top at the slab underside), `Pillar2x16` where the terrain there is more than 8 m below the slab;
/// - hill side: `Wall32x16` (32 × 16 × 1 m) from road level, between slab and terrain;
/// - the 11 `rock_c` positions: a `CliffRock5` on top of the wall line at each, the rock face of the CS1 shed.
/// </summary>
public static class RockShed
{
    public const double Bay = 32, Clear = 6;

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
        // Which side is uphill (the same along the shed: the hillside of the canyon).
        var m0 = At(al / 2); double nx = -m0.Tz, nz = m0.Tx;
        if (terrainTrue != null && Terr(m0.X + nx * 30, m0.Z + nz * 30) < Terr(m0.X - nx * 30, m0.Z - nz * 30)) { nx = -nx; nz = -nz; }
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        int slabs = 0, halves = 0, pillars = 0, walls = 0, k2 = 0;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note) =>
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed stand-in", note);
        void Pillar(double s, double slabY)
        {
            var c = At(s); double px = c.X - nx * (half + 1), pz = c.Z - nz * (half + 1), under = slabY;
            bool tall = Terr(px, pz) < under - 8;
            res.Add(O($"shed_pillar_{++pillars}", tall ? "Pillar2x16" : "Pillar2x8", px, pz, under - (tall ? 16 : 8), Yaw(c.Tx, c.Tz), "valley-side column under the roof edge"));
        }
        double? prevY = null;
        for (double s = 0; s < al; s += Bay)
        {
            var c = At(s + Bay / 2); double y = c.Y + Clear;
            bool step = prevY is { } py && Math.Abs(y - py) > 1;
            k2++;
            if (!step)
            {
                res.Add(O($"shed_roof_{k2}", "BasePlateConcrete32", c.X, c.Z, y, Yaw(c.Tx, c.Tz), "roof bay 32 m at road + 6 m"));
                slabs++;
            }
            else
                foreach (var hs in new[] { s + Bay / 4, s + 3 * Bay / 4 })
                {
                    var h = At(hs);
                    res.Add(O($"shed_roof_{k2}_{(hs < s + Bay / 2 ? "a" : "b")}", "BasePlateConcrete16", h.X, h.Z, h.Y + Clear, Yaw(h.Tx, h.Tz), "roof half bay 16 m (step > 1 m)"));
                    halves++;
                }
            prevY = y;
            Pillar(s, At(s).Y + Clear);
            res.Add(O($"shed_wall_{k2}", "Wall32x16", c.X + nx * (half + 1), c.Z + nz * (half + 1), c.Y, Yaw(c.Tx, c.Tz) - 90, "hill-side wall from road level (32 m side along the road)"));
            walls++;
        }
        Pillar(al, At(al).Y + Clear);
        // The rock face: a cliff piece on top of the wall line at each CS1 rock_c position.
        int rocks = 0;
        foreach (var r in shed)
        {
            var c = At(S(r.X, r.Z));
            res.Add(O($"shed_rock_{r.Id}", "CliffRock5", c.X + nx * (half + 4), c.Z + nz * (half + 4), c.Y + Clear, Yaw(c.Tx, c.Tz), $"rock face at CS1 rock_c {r.Id}, on the wall line"));
            rocks++;
        }
        return (res, FormattableString.Invariant($"rock shed gallery over {al:F0} m of {string.Join(", ", road.Select(e => e.Id))}: {slabs} BasePlateConcrete32 + {halves} BasePlateConcrete16 roof at road + 6 m, {pillars} valley-side pillars, {walls} Wall32x16, {rocks} CliffRock5 at the rock_c positions"));
    }
}
