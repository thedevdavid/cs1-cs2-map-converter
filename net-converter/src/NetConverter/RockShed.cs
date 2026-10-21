namespace NetConverter;

/// <summary>
/// Round 34e (owner: "use the vanilla pylon props, and make it partially open", next-run/ref-gallery-1/2.webp, alpine avalanche galleries): the CS1 JP Rock
/// shed (~440 m over the 4-lane National Road at x −6453…−6245, z 677…1069) as an open-sided gallery, one undoable import (`rock-shed`), absolute y, no sink.
/// ConcreteSet01 pieces only (owner: "use the packs, not the vanilla"; bounds in next-run/concreteset01-bounds.txt, all pivot centre-bottom).
///
/// Round 37 (owner's A-v11 review, next-run/shots-a11/fix/24-*): rebuilt along the road's own arc length:
/// - one profile: the road y smoothed over 48 m, roof underside = profile + 6.5 m; every piece takes its height from it;
/// - roof: BasePlateConcrete16 only (the 62.5 m plate is wider than the road), two across from the valley edge to the wall, a row every 8 m (8 m overlap),
///   each row turned to the road bearing at its centre;
/// - hill side: Wall32x16 every 16 m (half overlap), on the chord of the hill edge between its two ends, top flush with the roof.
/// Yaw: the bridge applies quaternion.RotateY(yaw) (local +z → (sin, cos)), checked on A-v11's walls and plate, which ran along the road.
///
/// Round 38 (owner's A-v12 review: "24 doesnt look like the example inspo images"; ref-gallery-1 inside, ref-gallery-2 outside):
/// - colonnade: slim Pillar1x8 stacks every 4 m under the valley edge, roof underside down to the terrain;
/// - fascia: the valley row of slabs gets 4 more plates stacked under it at 0.2 m steps (a 1 m concrete edge);
/// - cover: the 11 CS1 rock_c positions as CliffRock5 back on the roof, set toward the hill so their valley side stays on the roof; a jittered 2.5 m scatter
///   of GreenBushWild01/02 and Rock01…Rock30 over the roof top, inset 1.5 m from the valley edge; every base at the roof top (row y + 0.2);
/// - the valley side kept open: no trees within 20 m of the column line and 10 m past each end (<see cref="ValleyStrip"/>; the tree mask and clear-rock-shed.json);
/// - portals: none. TunnelEntrance18 (26 × 14 × 16 m, 10 m opening) would have to sink 3.5 m below the road to meet the roof at + 6.5 m and would still
///   stand 4 m above the flush hill wall.
/// </summary>
public static class RockShed
{
    public const double RoofStep = 8, WallStep = 16, ColumnStep = 4, Clear = 6.5, Slab = 16, WallLength = 32, WallHeight = 16, Plate = 0.2,
        CoverStep = 2.5, StripWidth = 20, StripEnds = 10;
    public const int FasciaPlates = 4;

    /// <summary>The road under the shed as one polyline with arc length, the shed's range on it and its hill side.</summary>
    sealed class Course
    {
        public required List<(double X, double Z, double Y, double L)> Line;
        public double L0, L1, Half, Side;
        public double Len => L1 - L0;
        public double Valley => -(Half + 0.5);   // the column line
        public double Hill => Half + 1;          // the wall's centre line
        public required List<OutEdge> Road;

        public (double X, double Z, double Y, double Tx, double Tz) At(double l)
        {
            l = Math.Clamp(l, 0, Line[^1].L);
            int i = Line.FindIndex(q => q.L >= l); i = i < 1 ? 1 : i;
            var p = Line[i - 1]; var q = Line[i]; double seg = q.L - p.L, f = seg > 0 ? (l - p.L) / seg : 0;
            return (p.X + (q.X - p.X) * f, p.Z + (q.Z - p.Z) * f, p.Y + (q.Y - p.Y) * f, (q.X - p.X) / seg, (q.Z - p.Z) / seg);
        }
        public double LOf(double x, double z) => Line.MinBy(q => (q.X - x) * (q.X - x) + (q.Z - z) * (q.Z - z)).L;
        public double Profile(double l) => Enumerable.Range(-6, 13).Average(k => At(l + k * 4).Y);   // road y smoothed over 48 m
        public double RoofY(double l) => Profile(l) + Clear;
        /// <summary>The point d metres across the road from arc length l; d &gt; 0 toward the hill.</summary>
        public (double X, double Z) Off(double l, double d) { var c = At(l); return (c.X - c.Tz * Side * d, c.Z + c.Tx * Side * d); }
    }

    static Course? Make(List<PoStructures.Row> shed, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue)
    {
        var road = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab == "Medium Road")
            .Where(e => shed.Any(r => Enumerable.Range(0, 9).Select(k => e.Curve.Eval(k / 8.0)).Any(p => Math.Sqrt((p.X - r.X) * (p.X - r.X) + (p.Z - r.Z) * (p.Z - r.Z)) < 40))).ToList();
        if (road.Count == 0) return null;
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
        var c = new Course { Line = line, Road = road, Half = FootprintShift.Cs2Half(road[0]), Side = 1 };
        // Arc-length range of the shed: the CS1 shed pieces projected onto the road.
        c.L0 = shed.Min(r => c.LOf(r.X, r.Z)); c.L1 = shed.Max(r => c.LOf(r.X, r.Z));
        // +1: the hill is on the left normal (−Tz, Tx).
        double lm = c.L0 + c.Len / 2;
        if (terrainTrue != null) { var (hx, hz) = c.Off(lm, 30); var (vx, vz) = c.Off(lm, -30); if (terrainTrue(hx, hz) < terrainTrue(vx, vz)) c.Side = -1; }
        return c;
    }

    /// <summary>
    /// Round 38: the open valley side, a ring from the column line out to StripWidth m, StripEnds m past each end of the shed (plan only). The tree mask drops
    /// trees inside it, and it is written as clear-rock-shed.json for clearing trees already placed on an open map.
    /// </summary>
    public static List<(double X, double Z)>? ValleyStrip(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var c = shed.Count == 0 ? null : Make(shed, edges, terrainTrue);
        if (c == null) return null;
        var ls = new List<double>();
        for (double l = c.L0 - StripEnds; l < c.L1 + StripEnds; l += 4) ls.Add(l);
        ls.Add(c.L1 + StripEnds);
        var ring = ls.Select(l => c.Off(l, c.Valley + 1)).ToList();   // from just under the slab edge (the columns stand clear of trees)
        ring.AddRange(ls.AsEnumerable().Reverse().Select(l => c.Off(l, c.Valley - StripWidth)));
        return ring.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList();
    }

    public static bool Inside(List<(double X, double Z)> ring, double x, double z)
    {
        bool inside = false;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
            if ((ring[i].Z > z) != (ring[j].Z > z) && x < (ring[j].X - ring[i].X) * (z - ring[i].Z) / (ring[j].Z - ring[i].Z) + ring[i].X) inside = !inside;
        return inside;
    }

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor, int seed = 38)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        var c = Make(shed, edges, terrainTrue);
        if (c == null) return (res, "rock shed: no National Road course under the shed");
        double l0 = c.L0, l1 = c.L1, len = c.Len, valley = c.Valley, hill = c.Hill;
        double Terr(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : double.MinValue;
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note, string kind = "object") =>
            new(id, prefab, kind, Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (open gallery)", note);

        // Roof: two 16 m slabs across, one with its valley edge on the column line, one with its hill edge on the wall; they overlap in the middle.
        // The valley one carries the fascia: FasciaPlates more plates stacked under it at 0.2 m steps.
        int rowsN = (int)Math.Ceiling((len - Slab) / RoofStep) + 1, slabs = 0, fascia = 0;
        var rowAt = new List<(double L, double Y)>();
        var across = new[] { valley + Slab / 2, hill - Slab / 2 };
        for (int i = 0; i < rowsN; i++)
        {
            double l = Math.Min(l0 + Slab / 2 + i * RoofStep, l1 - Slab / 2);
            var t = c.At(l); double y = c.RoofY(l);
            rowAt.Add((l, y));
            foreach (var d in across)
            {
                var (x, z) = c.Off(l, d);
                res.Add(O($"shed_roof_{++slabs}", "BasePlateConcrete16", x, z, y, Yaw(t.Tx, t.Tz), $"roof slab, row {i + 1} at road + {Clear} m (48 m smoothed)"));
            }
            var (fx, fz) = c.Off(l, across[0]);
            for (int k = 1; k <= FasciaPlates; k++)
                res.Add(O($"shed_fascia_{i + 1}_{k}", "BasePlateConcrete16", fx, fz, y - k * Plate, Yaw(t.Tx, t.Tz), $"valley-edge fascia, plate {k} under row {i + 1}"));
            fascia += FasciaPlates;
        }
        // The visible roof top at arc length l: the highest row covering it, plus the plate.
        double RoofTop(double l) => rowAt.Where(r => Math.Abs(r.L - l) <= Slab / 2 + 0.01).Select(r => r.Y).DefaultIfEmpty(c.RoofY(l)).Max() + Plate;

        // Hill-side wall: each 32 m piece on the chord of the hill edge between its ends, top flush with the roof.
        int walls = 0;
        for (double l = l0; l < l1 - 1; l += WallStep)
        {
            double lc = Math.Min(l + WallLength / 2, l1 - WallLength / 2);
            var p = c.Off(lc - WallLength / 2, hill); var q = c.Off(lc + WallLength / 2, hill);
            double y = c.RoofY(lc) + Plate - WallHeight;
            // Wall32x16 runs along its local x: local x → (cos yaw, −sin yaw), so yaw = bearing − 90.
            res.Add(O($"shed_wall_{++walls}", "Wall32x16", (p.X + q.X) / 2, (p.Z + q.Z) / 2, y, Yaw(q.X - p.X, q.Z - p.Z) - 90, "hill-side wall, top flush with the roof"));
        }
        // Valley-side colonnade every 4 m of road: Pillar1x8 stacked from the roof underside (under the fascia) down to the terrain; the lowest goes into the ground.
        int stacks = 0, p8 = 0;
        for (double l = l0 + 1; l <= l1 - 1 + 0.01; l += ColumnStep)
        {
            var t = c.At(l); var (px, pz) = c.Off(l, valley + 0.5);
            double top = rowAt.Where(r => Math.Abs(r.L - l) <= Slab / 2 + 0.01).Select(r => r.Y).DefaultIfEmpty(c.RoofY(l)).Min() - FasciaPlates * Plate;
            double ground = terrainTrue != null ? Terr(px, pz) : t.Y, left = Math.Max(top - ground, 0.5); int piece = 0; stacks++;
            while (left > 0.05)
            {
                res.Add(O($"shed_col_{stacks}_{++piece}", "Pillar1x8", px, pz, top - 8, Yaw(t.Tx, t.Tz), $"valley-side column, piece {piece} from the fascia underside down"));
                p8++; top -= 8; left -= 8;
            }
        }
        // Cover: the CS1 rock_c pieces as CliffRock5 on the roof (bounds x −19.1…17.1, y −2.57…14.35, z −18.1…18.2), turned to the road and set toward
        // the hill so no part reaches past the valley edge; base (bounds min y) at the roof top.
        int rocks = 0;
        const double cliffReach = 19.1, cliffBase = 2.57;
        foreach (var r in shed)
        {
            double l = Math.Clamp(c.LOf(r.X, r.Z), l0 + 4, l1 - 4); var t = c.At(l); var (x, z) = c.Off(l, valley + 1.5 + cliffReach);
            res.Add(O($"shed_rock_{r.Id}", "CliffRock5", x, z, RoofTop(l) + cliffBase, Yaw(t.Tx, t.Tz), $"rock cover on the roof at CS1 rock_c {r.Id}"));
            rocks++;
        }
        // Scatter over the roof top: a jittered 2.5 m grid, 60 % bushes, 40 % small rocks (the rock-scatter layer's Rock01…Rock30), 1.5 m clear of the valley edge.
        var rng = new Random(seed);
        int bushes = 0, stones = 0;
        for (double l = l0 + 1; l <= l1 - 1; l += CoverStep)
            for (double d = valley + 1.5; d <= hill + 1; d += CoverStep)
            {
                double jl = Math.Clamp(l + (rng.NextDouble() - 0.5) * CoverStep * 0.8, l0 + 1, l1 - 1), jd = Math.Max(valley + 1.5, d + (rng.NextDouble() - 0.5) * CoverStep * 0.8);
                var (x, z) = c.Off(jl, jd); double top = RoofTop(jl), yaw = rng.NextDouble() * 360;
                if (rng.NextDouble() < 0.6)
                {
                    res.Add(O($"shed_bush_{++bushes}", rng.NextDouble() < 0.5 ? "GreenBushWild01" : "GreenBushWild02", x, z, top, yaw, "roof cover bush", "tree"));
                }
                else
                {
                    int k = rng.Next(1, 31);
                    // Rock01…Rock30 have their pivot at the centre, about 0.3 m above their base.
                    res.Add(O($"shed_stone_{++stones}", $"Rock{k:00}", x, z, top + 0.3, yaw, "roof cover stone"));
                }
            }
        var check = Check(res, c);
        return (res, FormattableString.Invariant($"rock shed open gallery over {len:F0} m of road ({string.Join(", ", c.Road.Select(e => e.Id))}): {slabs} BasePlateConcrete16 ({rowsN} rows of 2, every {RoofStep} m) at road + {Clear} m on a 48 m smoothed profile + {fascia} fascia plates ({FasciaPlates} under each valley slab), {walls} Wall32x16 on the hill side (every {WallStep} m, top flush with the roof), {stacks} valley-side columns every {ColumnStep} m ({p8} Pillar1x8), roof cover {rocks} CliffRock5 + {bushes} bushes + {stones} stones; no portals (TunnelEntrance18 would stand 4 m above the hill wall); {check}"));
    }

    /// <summary>
    /// Bounds check (next-run/concreteset01-bounds.txt): roof cover of the road strip from the column line to the wall, sampled every 1 m along and 1 m
    /// across; the largest height step between neighbouring roof rows; the hill edge covered by a wall (within 1 m of a wall's centre plane), every 1 m;
    /// roof-cover pieces past the valley edge.
    /// </summary>
    static string Check(List<PlacedObject> objs, Course c)
    {
        static (double U, double V) Local(PlacedObject o, double x, double z)
        {
            double r = o.Rotation * Math.PI / 180, dx = x - o.X, dz = z - o.Z;
            return (dx * Math.Cos(r) - dz * Math.Sin(r), dx * Math.Sin(r) + dz * Math.Cos(r));   // local x, local z (inverse of RotateY)
        }
        var slabs = objs.Where(o => o.Id.StartsWith("shed_roof_")).ToList();
        int samples = 0, open = 0;
        for (double l = c.L0; l <= c.L1; l += 1)
            for (double d = c.Valley + 0.5; d <= c.Hill - 0.5; d += 1)
            {
                var (x, z) = c.Off(l, d); samples++;
                if (!slabs.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= Slab / 2 && Math.Abs(v) <= Slab / 2; })) open++;
            }
        var ys = slabs.Where((o, i) => i % 2 == 0).Select(o => o.Y ?? 0).ToList();   // one per row, in row order (two slabs per row)
        double step = ys.Count < 2 ? 0 : ys.Zip(ys.Skip(1), (p, q) => Math.Abs(q - p)).Max();
        var walls = objs.Where(o => o.Prefab == "Wall32x16").ToList();
        int wallSamples = 0, wallOpen = 0;
        for (double l = c.L0; l <= c.L1; l += 1)
        {
            var (x, z) = c.Off(l, c.Hill); wallSamples++;
            if (!walls.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= WallLength / 2 && Math.Abs(v) <= 1; })) wallOpen++;
        }
        double worstStep = walls.Count < 2 ? 0 : walls.Zip(walls.Skip(1), (p, q) => Math.Abs((q.Y ?? 0) - (p.Y ?? 0))).Max();
        // Cover past the valley edge: signed distance across the road (toward the hill > 0) of each cover piece's valley-most reach.
        int over = 0;
        foreach (var o in objs.Where(o => o.Id.StartsWith("shed_rock_") || o.Id.StartsWith("shed_bush_") || o.Id.StartsWith("shed_stone_")))
        {
            double l = c.LOf(o.X, o.Z); var t = c.At(l);
            double d = ((o.X - t.X) * -t.Tz + (o.Z - t.Z) * t.Tx) * c.Side;
            double reach = o.Prefab == "CliffRock5" ? 19.1 : o.Prefab.StartsWith("GreenBush") ? 1.5 : 0.5;
            if (d - reach < c.Valley - 0.01) over++;
        }
        return FormattableString.Invariant($"check: roof covers {100.0 * (samples - open) / samples:F1} % of the road strip ({open} of {samples} 1 m samples open), largest step between roof rows {step:F2} m, the wall covers {100.0 * (wallSamples - wallOpen) / wallSamples:F1} % of the hill edge ({wallOpen} of {wallSamples} 1 m samples open), largest wall height step {worstStep:F2} m, {over} roof-cover pieces reach past the valley edge");
    }
}
