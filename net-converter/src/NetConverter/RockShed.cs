namespace NetConverter;

/// <summary>
/// Round 34e (owner: "use the vanilla pylon props, and make it partially open", next-run/ref-gallery-1/2.webp, alpine avalanche galleries): the CS1 JP Rock
/// shed (~440 m over the 4-lane National Road at x −6453…−6245, z 677…1069) as an open-sided gallery, one undoable import (`rock-shed`), absolute y, no sink.
/// ConcreteSet01 pieces only (owner: "use the packs, not the vanilla"; bounds in next-run/concreteset01-bounds.txt, all pivot centre-bottom).
///
/// Round 37 (owner's A-v11 review, next-run/shots-a11/fix/24-*): the 62.5 m plate lay across the road and over the cliff, the roof slabs stepped and
/// gapped, the walls stood 9.5 m above the roof, rocks sat on top. Rebuilt along the road's own arc length:
/// - one profile: the road y smoothed over 48 m, roof underside = profile + 6.5 m; every piece takes its height from it, so neighbours differ by the grade only;
/// - roof: BasePlateConcrete16 only (the 62.5 m plate is wider than the road), two across from the valley edge to the wall, a row every 8 m (8 m overlap
///   along, so the outer side of the curve has no gaps), each row turned to the road bearing at its centre;
/// - hill side: Wall32x16 every 16 m (half overlap), set on the chord of the hill edge between its two ends, top flush with the roof (the rest goes into the
///   hill cut below the road);
/// - valley side: a ConcreteSet01 Pillar2x16 / Pillar2x8 stack every 8 m of road, roof underside down to the terrain;
/// - the 11 CS1 `rock_c` positions: CliffRock5 on the terrain behind the wall, never above the roof.
/// Yaw: the bridge applies quaternion.RotateY(yaw) (local +z → (sin, cos)), checked on A-v11's walls and plate, which ran along the road.
/// </summary>
public static class RockShed
{
    public const double RoofStep = 8, WallStep = 16, ColumnStep = 8, Clear = 6.5, Slab = 16, WallLength = 32, WallHeight = 16;

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        var road = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab == "Medium Road")
            .Where(e => shed.Any(r => Enumerable.Range(0, 9).Select(k => e.Curve.Eval(k / 8.0)).Any(p => Math.Sqrt((p.X - r.X) * (p.X - r.X) + (p.Z - r.Z) * (p.Z - r.Z)) < 40))).ToList();
        if (road.Count == 0) return (res, "rock shed: no National Road course under the shed");

        // The road centre line as one polyline, ordered along the shed, with arc length.
        var a = shed.MinBy(r => r.Z)!; var b = shed.MaxBy(r => r.Z)!;
        double ax = b.X - a.X, az = b.Z - a.Z, al = Math.Sqrt(ax * ax + az * az); ax /= al; az /= al;
        double S(double x, double z) => (x - a.X) * ax + (z - a.Z) * az;
        var raw = new List<(double X, double Z, double Y)>();
        foreach (var e in road)
            for (int k = 0; k <= 64; k++)
            {
                double t = k / 64.0; var p = e.Curve.Eval(t);
                raw.Add((p.X, p.Z, Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A))));
            }
        var line = new List<(double X, double Z, double Y, double L)>();
        foreach (var p in raw.Where(p => S(p.X, p.Z) >= -60 && S(p.X, p.Z) <= al + 60).OrderBy(p => S(p.X, p.Z)))
        {
            if (line.Count > 0 && Math.Sqrt((p.X - line[^1].X) * (p.X - line[^1].X) + (p.Z - line[^1].Z) * (p.Z - line[^1].Z)) < 0.5) continue;
            double l = line.Count == 0 ? 0 : line[^1].L + Math.Sqrt((p.X - line[^1].X) * (p.X - line[^1].X) + (p.Z - line[^1].Z) * (p.Z - line[^1].Z));
            line.Add((p.X, p.Z, p.Y, l));
        }
        // Arc-length range of the shed: the CS1 shed pieces projected onto the road.
        double LOf(double x, double z) => line.MinBy(q => (q.X - x) * (q.X - x) + (q.Z - z) * (q.Z - z)).L;
        double l0 = shed.Min(r => LOf(r.X, r.Z)), l1 = shed.Max(r => LOf(r.X, r.Z)), len = l1 - l0;
        (double X, double Z, double Y, double Tx, double Tz) At(double l)
        {
            l = Math.Clamp(l, 0, line[^1].L);
            int i = Math.Max(1, line.FindIndex(q => q.L >= l)); if (i < 0) i = line.Count - 1;
            var p = line[i - 1]; var q = line[i]; double seg = q.L - p.L, f = seg > 0 ? (l - p.L) / seg : 0;
            return (p.X + (q.X - p.X) * f, p.Z + (q.Z - p.Z) * f, p.Y + (q.Y - p.Y) * f, (q.X - p.X) / seg, (q.Z - p.Z) / seg);
        }
        double Profile(double l) => Enumerable.Range(-6, 13).Average(k => At(l + k * 4).Y);   // road y smoothed over 48 m
        double RoofY(double l) => Profile(l) + Clear;

        double half = FootprintShift.Cs2Half(road[0]);
        double Terr(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : double.MinValue;
        var m0 = At(l0 + len / 2); double side = 1;   // +1: the hill is on the left normal (−Tz, Tx)
        if (terrainTrue != null && Terr(m0.X - m0.Tz * 30, m0.Z + m0.Tx * 30) < Terr(m0.X + m0.Tz * 30, m0.Z - m0.Tx * 30)) side = -1;
        (double X, double Z) Off((double X, double Z, double Y, double Tx, double Tz) c, double d) => (c.X - c.Tz * side * d, c.Z + c.Tx * side * d);   // d > 0 toward the hill
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note) =>
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (open gallery)", note);

        double valley = -(half + 0.5), hill = half + 1;   // valley edge = column line; hill edge = wall centre line
        // Roof: two 16 m slabs across, one with its valley edge on the column line, one with its hill edge on the wall; they overlap in the middle.
        int rowsN = (int)Math.Ceiling((len - Slab) / RoofStep) + 1, slabs = 0;
        var across = new[] { valley + Slab / 2, hill - Slab / 2 };
        for (int i = 0; i < rowsN; i++)
        {
            double l = Math.Min(l0 + Slab / 2 + i * RoofStep, l1 - Slab / 2);
            var c = At(l); double y = RoofY(l);
            foreach (var d in across)
            {
                var (x, z) = Off(c, d);
                res.Add(O($"shed_roof_{++slabs}", "BasePlateConcrete16", x, z, y, Yaw(c.Tx, c.Tz), $"roof slab, row {i + 1} at road + {Clear} m (48 m smoothed)"));
            }
        }
        // Hill-side wall: each 32 m piece on the chord of the hill edge between its ends, top flush with the roof.
        int walls = 0;
        for (double l = l0; l < l1 - 1; l += WallStep)
        {
            double lc = Math.Min(l + WallLength / 2, l1 - WallLength / 2);
            var p = Off(At(lc - WallLength / 2), hill); var q = Off(At(lc + WallLength / 2), hill);
            double y = RoofY(lc) + 0.2 - WallHeight;
            // Wall32x16 runs along its local x: local x → (cos yaw, −sin yaw), so yaw = bearing − 90.
            res.Add(O($"shed_wall_{++walls}", "Wall32x16", (p.X + q.X) / 2, (p.Z + q.Z) / 2, y, Yaw(q.X - p.X, q.Z - p.Z) - 90, "hill-side wall, top flush with the roof"));
        }
        // Valley-side columns every 8 m of road: Pillar2x16 while more than 8 m remain, then one Pillar2x8; the lowest goes into the ground by what is left.
        int stacks = 0, p16 = 0, p8 = 0;
        for (double l = l0 + 1; l <= l1 - 1 + 0.01; l += ColumnStep)
        {
            var c = At(l); var (px, pz) = Off(c, valley + 1); double top = RoofY(l);
            double ground = terrainTrue != null ? Terr(px, pz) : c.Y, left = Math.Max(top - ground, 0.5); int piece = 0; stacks++;
            while (left > 0.05)
            {
                double h = left > 8 ? 16 : 8;
                res.Add(O($"shed_col_{stacks}_{++piece}", h > 8 ? "Pillar2x16" : "Pillar2x8", px, pz, top - h, Yaw(c.Tx, c.Tz), $"valley-side column, piece {piece} from the roof underside down"));
                if (h > 8) p16++; else p8++;
                top -= h; left -= h;
            }
        }
        // The CS1 rock_c pieces: rock on the terrain behind the wall, kept 4 m under the roof.
        int rocks = 0;
        foreach (var r in shed)
        {
            double l = Math.Clamp(LOf(r.X, r.Z), l0, l1); var c = At(l); var (x, z) = Off(c, hill + 7);
            double ground = terrainTrue != null ? Terr(x, z) : c.Y;
            res.Add(O($"shed_rock_{r.Id}", "CliffRock5", x, z, Math.Min(ground - 1, RoofY(l) - 4), Yaw(c.Tx, c.Tz), $"rock against the hill behind the wall at CS1 rock_c {r.Id}"));
            rocks++;
        }
        var check = Check(res, line, l0, l1, At, Off, valley, hill);
        return (res, FormattableString.Invariant($"rock shed open gallery over {len:F0} m of road ({string.Join(", ", road.Select(e => e.Id))}): {slabs} BasePlateConcrete16 ({rowsN} rows of 2, every {RoofStep} m) at road + {Clear} m on a 48 m smoothed profile, {walls} Wall32x16 on the hill side (every {WallStep} m, top flush with the roof), {stacks} valley-side columns every {ColumnStep} m ({p16} Pillar2x16 + {p8} Pillar2x8), {rocks} CliffRock5 behind the wall; {check}"));
    }

    /// <summary>
    /// Bounds check (next-run/concreteset01-bounds.txt): roof cover of the road strip from the column line to the wall, sampled every 1 m along and 1 m
    /// across; the largest height step between neighbouring roof rows; the hill edge covered by a wall (within 1 m of a wall's centre plane), every 1 m.
    /// </summary>
    static string Check(List<PlacedObject> objs, List<(double X, double Z, double Y, double L)> line, double l0, double l1,
        Func<double, (double X, double Z, double Y, double Tx, double Tz)> at, Func<(double X, double Z, double Y, double Tx, double Tz), double, (double X, double Z)> off, double valley, double hill)
    {
        static (double U, double V) Local(PlacedObject o, double x, double z)
        {
            double r = o.Rotation * Math.PI / 180, dx = x - o.X, dz = z - o.Z;
            return (dx * Math.Cos(r) - dz * Math.Sin(r), dx * Math.Sin(r) + dz * Math.Cos(r));   // local x, local z (inverse of RotateY)
        }
        var slabs = objs.Where(o => o.Prefab == "BasePlateConcrete16").ToList();
        int samples = 0, open = 0;
        for (double l = l0; l <= l1; l += 1)
            for (double d = valley + 0.5; d <= hill - 0.5; d += 1)
            {
                var (x, z) = off(at(l), d); samples++;
                if (!slabs.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= Slab / 2 && Math.Abs(v) <= Slab / 2; })) open++;
            }
        var ys = slabs.Select(o => o.Y ?? 0).Distinct().ToList();
        double step = ys.Count < 2 ? 0 : ys.Zip(ys.Skip(1), (p, q) => Math.Abs(q - p)).Max();
        var walls = objs.Where(o => o.Prefab == "Wall32x16").ToList();
        int wallSamples = 0, wallOpen = 0;
        for (double l = l0; l <= l1; l += 1)
        {
            var (x, z) = off(at(l), hill); wallSamples++;
            if (!walls.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= WallLength / 2 && Math.Abs(v) <= 1; })) wallOpen++;
        }
        double worstStep = walls.Count < 2 ? 0 : walls.Zip(walls.Skip(1), (p, q) => Math.Abs((q.Y ?? 0) - (p.Y ?? 0))).Max();
        return FormattableString.Invariant($"check: roof covers {100.0 * (samples - open) / samples:F1} % of the road strip ({open} of {samples} 1 m samples open), largest step between roof rows {step:F2} m (converter metres, 2× heights; half that in x1), the wall covers {100.0 * (wallSamples - wallOpen) / wallSamples:F1} % of the hill edge ({wallOpen} of {wallSamples} 1 m samples open), largest wall height step {worstStep:F2} m");
    }
}
