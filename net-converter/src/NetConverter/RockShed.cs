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
/// - cover (round 39: without CliffRock5, which read as giant boulders; 70 % bushes): a jittered 1.6 m scatter
///   of GreenBushWild01/02 and Rock01…Rock30 over the roof top, inset 1.5 m from the valley edge; every base at the roof top (row y + 0.2);
/// - the valley side kept open: no trees within 20 m of the column line and 10 m past each end (<see cref="ValleyStrip"/>; the tree mask and clear-rock-shed.json);
/// - portals: none. TunnelEntrance18 (26 × 14 × 16 m, 10 m opening) would have to sink 3.5 m below the road to meet the roof at + 6.5 m and would still
///   stand 4 m above the flush hill wall.
///
/// Round 41 (owner's A-v15 review: staggered top, too low in spots, sides not open enough): one layer of rows every 15.8 m, each carrying a pitch to the
/// grade (bridge 0.9.50; level and shingled on older bridges), underside >= 6.5 m (+ 0.1) above the highest road point under the row; Pillar1x8 every 6 m from
/// 0.5 m in the ground to the underside; the rocks around the shed re-seated (<see cref="Reseat"/>). Some lines above describe earlier rounds.
/// </summary>
public static class RockShed
{
    public const double RoofStep = 15.8, WallStep = 16, ColumnStep = 6, Clear = 6.5, Slab = 16, WallLength = 32, WallHeight = 16, Plate = 0.2,
        StripWidth = 20, StripEnds = 10,
        ShellHalf = 23.5, HillCap = 40, BenchSearch = 100, ClearMargin = 0.1, MaxPitch = 3, MaxPitchStep = 1;   // round 40: CS1 rock_c shells 46.7–47.7 m long. Round 40b: the hill edge is no longer capped at the
                                                             // shell's (axis + 25 m): every row reaches into the cut face, up to axis + 40 m

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

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        var c = Make(shed, edges, terrainTrue);
        if (c == null) return (res, "rock shed: no National Road course under the shed");
        // Round 40: the CS1 union of the 11 shells, half a shell (ShellHalf) before the first centre and after the last.
        double r0 = c.L0 - ShellHalf, r1 = c.L1 + ShellHalf, valley = c.Valley, hill = c.Hill;
        double Terr(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : double.MinValue;
        double TerrAt(double l, double d) { var (x, z) = c.Off(l, d); return Terr(x, z); }
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note) =>
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (open gallery)", note);

        // Round 41: roof rows every RoofStep (15.8 m: the 16 m slabs butt with 0.2 m overlap) over the CS1 extent, one layer. Each row's underside is at least
        // Clear above the highest point of the actual road under its 16 m span (round 41, item 3) and never below the 48 m smoothed profile + Clear; each row
        // carries a pitch from the slope between its neighbours (pitch[] here: the rise toward its +z end, the road's arc-length direction; the file
        // gets the bridge's sign, −rise, since 0.9.50 tilts the +z end down for + pitch), so a bridge that
        // applies pitch lays the rows in one plane without steps. A bridge that ignores it lays them level: the uphill row then sits on the downhill one's
        // uphill edge (shingle), its lip facing downhill. Each row reaches from the column line toward the hill until its slabs are inside the terrain
        // (terrain >= roof top along the row's centre and 6 m either side), + 2 m, capped at HillCap; 16 m slabs spread evenly across that width.
        int rowsN = (int)Math.Ceiling((r1 - r0 - Slab) / RoofStep) + 1, slabs = 0, capped = 0;
        double RoadMax(double l) => Enumerable.Range(0, 17).Max(k => c.At(l - Slab / 2 + k).Y);
        double rowStep = (r1 - r0 - Slab) / (rowsN - 1);   // even spacing, at most RoofStep (no near-duplicate end row)
        var ls = Enumerable.Range(0, rowsN).Select(i => r0 + Slab / 2 + i * rowStep).ToList();
        // Round 41c–d: one smooth roof profile over the rows. Lower bound per row: the highest road point under its 16 m span + Clear + ClearMargin (6.6 m).
        // Above it, a curve with |pitch| <= MaxPitch (3°) and a pitch change between neighbouring rows <= MaxPitchStep (1°) everywhere, also where the
        // road itself is steeper (owner, round 41d: no kinks; buy it with clearance). See Fit. A pitched row's downhill end sits lower than its centre,
        // so the bound is raised where the pitched underside comes within 6.6 m of the road, and the profile rebuilt.
        var bound = ls.Select(l => RoadMax(l) + Clear + ClearMargin).ToList();
        var roadGrade = ls.Select(l => Math.Abs(Math.Atan2(c.Profile(l + 24) - c.Profile(l - 24), 48)) * 180 / Math.PI).ToList();
        var steep = roadGrade.Select(g => g > MaxPitch).ToList();
        var ys = bound.ToList();
        List<double> Pitches() => ls.Select((l, i) => { int a = Math.Max(0, i - 1), b = Math.Min(rowsN - 1, i + 1); return b > a ? Math.Atan2(ys[b] - ys[a], ls[b] - ls[a]) * 180 / Math.PI : 0; }).ToList();
        void Fit()
        {
            // A dilation, then a morphological closing (dilation + erosion), of the row bounds with a structuring element that has the limits built in: curvature κ (a slope change of
            // 0.97 × MaxPitchStep per row step) near its apex, slope 0.98 × MaxPitch beyond. Dilation (the max of downward caps over the bound points) keeps
            // crests rounded and slopes within the cap (a closing alone would keep the bound's sharp crests); the erosion (the min of upward bowls) fills the valleys. The result is never
            // below the bound and needs no iteration; row heights are sampled from it.
            double step = ls.Count > 1 ? ls[1] - ls[0] : RoofStep, kap = Math.Tan(0.97 * MaxPitchStep * Math.PI / 180) / step, sl = Math.Tan(0.98 * MaxPitch * Math.PI / 180), x0 = sl / kap;
            double Q(double x) { x = Math.Abs(x); return x <= x0 ? kap * x * x / 2 : kap * x0 * x0 / 2 + sl * (x - x0); }
            var grid = new List<double>(); for (double l = ls[0]; l <= ls[^1] + 0.01; l += 1) grid.Add(l);
            var dil0 = grid.Select(g => Enumerable.Range(0, rowsN).Max(j => bound[j] - Q(g - ls[j]))).ToList();
            var dil = grid.Select(g => Enumerable.Range(0, grid.Count).Max(m => dil0[m] - Q(g - grid[m]))).ToList();   // dilated twice: the closing below keeps these rounded crests
            double Ero(double l) => Enumerable.Range(0, grid.Count).Min(m => dil[m] + Q(l - grid[m]));
            ys = ls.Select((l, i) => Math.Max(Ero(l), bound[i])).ToList();
        }
        Fit();
        var pitch = Pitches();
        for (int pass = 0; pass < 4; pass++)
        {
            bool raised = false;
            for (int i = 0; i < rowsN; i++)
            {
                double g = Math.Tan(pitch[i] * Math.PI / 180), worst = Enumerable.Range(0, 17).Min(k => ys[i] + (k - Slab / 2) * g - c.At(ls[i] - Slab / 2 + k).Y);
                if (worst < Clear + ClearMargin - 0.005) { bound[i] += Clear + ClearMargin - worst; raised = true; }
            }
            if (!raised) break;
            Fit(); pitch = Pitches();
        }
        double maxStep = Enumerable.Range(1, rowsN - 1).Max(i => Math.Abs(pitch[i] - pitch[i - 1]));
        var steepRoad = Enumerable.Range(0, rowsN).Where(i => roadGrade[i] > MaxPitch).ToList();
        var rowAt = new List<(double L, double Y, double Edge)>();
        var cappedRows = new List<string>();
        var bench = new List<double>();   // column line → first terrain at or above the roof top, per row
        var clearance = new List<(int Row, double Level, double Pitched)>();
        for (int i = 0; i < rowsN; i++)
        {
            double l = ls[i], y = ys[i], g = Math.Tan(pitch[i] * Math.PI / 180), top = y + Plate;
            var t = c.At(l);
            // Clearance over the road under the span: level plate (today's bridge) and pitched plate.
            clearance.Add((i + 1, Enumerable.Range(0, 17).Min(k => y - c.At(l - Slab / 2 + k).Y), Enumerable.Range(0, 17).Min(k => y + (k - Slab / 2) * g - c.At(l - Slab / 2 + k).Y)));
            double? meet = null, first = null;
            for (double d = valley; d <= HillCap + 0.01; d += 0.5)
                if (d >= hill && meet == null && new[] { -6.0, 0, 6 }.All(k => TerrAt(l + k, d) >= top)) meet = d;
            for (double d = valley; d <= valley + BenchSearch && first == null; d += 0.5)   // reported beyond the cap too: where the bench ends
                if (TerrAt(l, d) >= top) first = d;
            bench.Add(first is double f ? f - valley : double.PositiveInfinity);
            double edge = meet is double m ? Math.Min(Math.Max(m + 2, hill), HillCap) : HillCap;
            if (meet == null) { capped++; cappedRows.Add(FormattableString.Invariant($"row {i + 1} (face at {(first is double ff ? $"{ff - valley:F1} m" : "none")})")); }
            rowAt.Add((l, y, edge));
            int n = Math.Max(2, (int)Math.Ceiling((edge - valley) / Slab));
            for (int k = 0; k < n; k++)
            {
                double d = valley + Slab / 2 + (edge - valley - Slab) * k / (n - 1);
                var (x, z) = c.Off(l, d);
                res.Add(O($"shed_roof_{++slabs}", "BasePlateConcrete16", x, z, y, Yaw(t.Tx, t.Tz), FormattableString.Invariant($"roof slab {k + 1}/{n}, row {i + 1}, underside >= road + {Clear} m, rising {pitch[i]:F2}° toward +z, into the hill to {edge - valley:F1} m from the column line")) with { Pitch = Math.Round(-pitch[i], 2) + 0.0 });   // bridge 0.9.50: + tilts the +z end down
            }
        }
        double RoofTop(double l) => rowAt.Where(r => Math.Abs(r.L - l) <= Slab / 2 + 0.01).Select(r => r.Y).DefaultIfEmpty(c.RoofY(l)).Max() + Plate;
        // Underside at arc length l, highest of level and pitched over the rows covering l (a column top inside the 0.2 m plate either way).
        double Underside(double l) => Enumerable.Range(0, rowsN).Where(i => Math.Abs(ls[i] - l) <= Slab / 2 + 0.01)
            .Select(i => Math.Max(ys[i], ys[i] + (l - ls[i]) * Math.Tan(pitch[i] * Math.PI / 180))).DefaultIfEmpty(c.RoofY(l)).Max();

        // Hill-side wall only where the terrain at the capped roof edge stays below the roof (a visible gap), all on the one line HillCap, every 16 m with
        // half overlap; top flush with the roof, Wall32x16, or Wall32x32 where 16 m would not reach the ground.
        int w16 = 0, w32 = 0;
        var centres = new List<double>();
        for (double lc = r0 + WallLength / 2; lc < r1 - WallLength / 2; lc += WallStep) centres.Add(lc);
        centres.Add(r1 - WallLength / 2);
        double covered = double.MinValue;
        foreach (double lc in centres)
        {
            if (lc + WallLength / 2 <= covered + 0.01) continue;   // the end piece when the last regular one already reaches the end
            var span = Enumerable.Range(0, 9).Select(k => lc - WallLength / 2 + k * WallLength / 8).ToList();
            if (!span.Any(l => TerrAt(l, HillCap + 0.5) < RoofTop(l) - 0.3)) continue;
            covered = lc + WallLength / 2;
            var p = c.Off(lc - WallLength / 2, HillCap); var q = c.Off(lc + WallLength / 2, HillCap);
            double topY = RoofTop(lc), ground = span.Min(l => TerrAt(l, HillCap));
            bool tall = ground < topY - WallHeight;
            if (tall) w32++; else w16++;
            // Wall32xNN runs along its local x: local x → (cos yaw, −sin yaw), so yaw = bearing − 90.
            res.Add(O($"shed_wall_{w16 + w32}", tall ? "Wall32x32" : "Wall32x16", (p.X + q.X) / 2, (p.Z + q.Z) / 2, topY - (tall ? 32 : WallHeight), Yaw(q.X - p.X, q.Z - p.Z) - 90,
                FormattableString.Invariant($"hill-side wall where the terrain stays below the roof, top flush, base {(tall ? 32 : WallHeight) - (topY - ground):F1} m into the ground")));
        }
        // Valley-side colonnade every ColumnStep (6 m, round 41: slim columns with wide gaps, ref-gallery-1/2): Pillar1x8 from the terrain to the roof
        // underside. The bottom piece stands 0.5 m in the ground; taller stacks overlap so the top piece ends at the underside. Pillar1x8 is the pack's only
        // 1 m column, so under 7.5 m the single piece has to go deeper (counted).
        int stacks = 0, p8 = 0, deep = 0; double worstDeep = 0;
        for (double l = r0 + 1; l <= r1 - 1 + 0.01; l += ColumnStep)
        {
            var t = c.At(l); var (px, pz) = c.Off(l, valley + 0.5);
            double top = Underside(l), ground = terrainTrue != null ? Terr(px, pz) : t.Y, h = top - (ground - 0.5);
            int n = Math.Max(1, (int)Math.Ceiling(h / 8)); stacks++;
            if (n == 1 && 8 - (top - ground) > 0.5) { deep++; worstDeep = Math.Max(worstDeep, 8 - (top - ground)); }
            for (int k = 0; k < n; k++)
            {
                double baseY = n == 1 ? top - 8 : (ground - 0.5) + (h - 8) * k / (n - 1);
                res.Add(O($"shed_col_{stacks}_{k + 1}", "Pillar1x8", px, pz, baseY, Yaw(t.Tx, t.Tz), $"valley-side column, piece {k + 1} of {n}, terrain to roof underside"));
                p8++;
            }
        }
        // Round 39–40: nothing on the roof (no CliffRock5, no bushes, no stones); the bare plates run into the hill instead.
        string P((double X, double Z, double Y, double Tx, double Tz) p) => FormattableString.Invariant($"({p.X:F1}, {p.Z:F1})");
        var fin = bench.Where(double.IsFinite).ToList();
        int widest = fin.Count > 0 ? bench.IndexOf(fin.Max()) + 1 : 0;
        string benchText = FormattableString.Invariant($"column line → terrain at the roof top per row: min {(fin.Count > 0 ? fin.Min() : double.NaN):F1} m, max {(fin.Count > 0 ? fin.Max() : double.NaN):F1} m (widest at row {widest}), the cap at {HillCap - valley:F1} m{(fin.Count < bench.Count ? $", {bench.Count - fin.Count} rows not within {BenchSearch:F0} m" : "")}");
        var check = Check(res, c, r0, r1, rowAt, TerrAt, RoofTop);
        return (res, FormattableString.Invariant($"rock shed open gallery, CS1 shell extent {r1 - r0:F0} m of road from {P(c.At(r0))} to {P(c.At(r1))} (A-v14: {c.L1 - c.L0:F0} m from {P(c.At(c.L0))} to {P(c.At(c.L1))}) ({string.Join(", ", c.Road.Select(e => e.Id))}): {slabs} BasePlateConcrete16 in {rowsN} rows every {rowStep:F2} m, one layer, on one smooth profile (rise {pitch.Min():F2}…{pitch.Max():F2}° toward +z, file pitch = −rise; |rise| ≤ {MaxPitch}° and change between neighbouring rows max {maxStep:F2}° everywhere; road under the shed steeper than {MaxPitch}° over 48 m (bridged by the roof, not followed): {(steepRoad.Count == 0 ? "nowhere" : string.Join(", ", steepRoad.Select(i => FormattableString.Invariant($"row {i + 1} road {roadGrade[i]:F1}° at {P(c.At(ls[i]))}"))))}), underside >= the highest road point under each row + {Clear + ClearMargin} m (clearance over the road per row: level min {clearance.Min(q => q.Level):F2} max {clearance.Max(q => q.Level):F2} m, pitched min {clearance.Min(q => q.Pitched):F2} max {clearance.Max(q => q.Pitched):F2} m), into the cut face: row depth from the column line min {rowAt.Min(r => r.Edge - valley):F1} m, max {rowAt.Max(r => r.Edge - valley):F1} m ({capped} rows stopped at the cap, axis + {HillCap} m = {HillCap - valley:F1} m{(cappedRows.Count > 0 ? ": " + string.Join(", ", cappedRows) : "")}); {benchText}; hill-side wall only at gaps: {w16} Wall32x16 + {w32} Wall32x32 on the {HillCap} m line; {stacks} valley-side columns every {ColumnStep} m ({p8} Pillar1x8, bases 0.5 m in the ground{(deep > 0 ? $"; {deep} single pieces deeper, up to {worstDeep:F1} m, where the underside is under 7.5 m above the terrain" : "")}); bare roof; no portals; {check}"));
    }

    /// <summary>
    /// Bounds check (next-run/concreteset01-bounds.txt): roof cover of the road strip from the column line to the road's hill side, sampled every 1 m along
    /// and 1 m across; the largest height step between neighbouring roof rows; open gaps at the hill edge (terrain below the roof top behind the row's edge
    /// with no wall there), every 1 m.
    /// </summary>
    static string Check(List<PlacedObject> objs, Course c, double r0, double r1, List<(double L, double Y, double Edge)> rowAt, Func<double, double, double> terrAt, Func<double, double> roofTop)
    {
        static (double U, double V) Local(PlacedObject o, double x, double z)
        {
            double r = o.Rotation * Math.PI / 180, dx = x - o.X, dz = z - o.Z;
            return (dx * Math.Cos(r) - dz * Math.Sin(r), dx * Math.Sin(r) + dz * Math.Cos(r));   // local x, local z (inverse of RotateY)
        }
        var slabs = objs.Where(o => o.Id.StartsWith("shed_roof_")).ToList();
        int samples = 0, open = 0;
        for (double l = r0; l <= r1; l += 1)
            for (double d = c.Valley + 0.5; d <= c.Hill - 0.5; d += 1)
            {
                var (x, z) = c.Off(l, d); samples++;
                if (!slabs.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= Slab / 2 && Math.Abs(v) <= Slab / 2; })) open++;
            }
        double step = rowAt.Count < 2 ? 0 : rowAt.Zip(rowAt.Skip(1), (p, q) => Math.Abs(q.Y - p.Y)).Max();
        var walls = objs.Where(o => o.Prefab.StartsWith("Wall32x")).ToList();
        int edgeSamples = 0, gaps = 0;
        for (double l = r0; l <= r1; l += 1)
        {
            double edge = rowAt.Where(r => Math.Abs(r.L - l) <= Slab / 2 + 0.01).Select(r => r.Edge).DefaultIfEmpty(c.Hill).Max(); edgeSamples++;
            if (terrAt(l, edge + 0.5) >= roofTop(l) - 0.3) continue;
            var (x, z) = c.Off(l, HillCap);
            if (!walls.Any(o => { var (u, v) = Local(o, x, z); return Math.Abs(u) <= WallLength / 2 && Math.Abs(v) <= 1; })) gaps++;
        }
        return FormattableString.Invariant($"check: roof covers {100.0 * (samples - open) / samples:F1} % of the road strip ({open} of {samples} 1 m samples open), largest step between roof rows {step:F2} m, {gaps} of {edgeSamples} 1 m samples along the hill edge open (terrain below the roof, no wall)");
    }

    /// <summary>
    /// Round 41 (owner's A-v15 review: "cliffs ar positioned really shitty there. wrong height, wrong orientation"): the PO rocks and scatter rocks around the
    /// shed (x −6520…−6180, z 620…1130), which the bridge grounded at their pivot on the cut face, get an explicit y and yaw: turned with their long side along
    /// the local contour of the cut face, mesh bottom 0.75 m under the lowest terrain in their footprint (5 × 5 samples of the bounds), and left out where the
    /// footprint touches a roof slab or the road (road axis ± half width + 1 m).
    /// </summary>
    public static (List<PlacedObject> Objects, string Log) Reseat(IEnumerable<PlacedObject> rocks, List<PlacedObject> shedObjs, List<PoStructures.Row> rows, IEnumerable<OutEdge> edges,
        Func<double, double, double>? terrainTrue, double heightFactor, IReadOnlyDictionary<string, PrefabBound> bounds)
    {
        var all = rocks.ToList();
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var c = shed.Count == 0 || terrainTrue == null ? null : Make(shed, edges, terrainTrue);
        if (c == null) return (all, "no rock shed course or terrain: unchanged");
        double Terr(double x, double z) => terrainTrue!(x, z) * heightFactor;
        static bool InBox(double x, double z) => x >= -6520 && x <= -6180 && z >= 620 && z <= 1130;
        var slabs = shedObjs.Where(o => o.Id.StartsWith("shed_roof_")).ToList();
        bool OnSlab(double x, double z) => slabs.Any(o =>
        {
            double r = o.Rotation * Math.PI / 180, dx = x - o.X, dz = z - o.Z;
            return Math.Abs(dx * Math.Cos(r) - dz * Math.Sin(r)) <= Slab / 2 && Math.Abs(dx * Math.Sin(r) + dz * Math.Cos(r)) <= Slab / 2;
        });
        bool OnRoad(double x, double z)
        {
            double l = c.LOf(x, z); var t = c.At(l);
            if (l <= 0.5 || l >= c.Line[^1].L - 0.5) return false;
            return Math.Abs((x - t.X) * -t.Tz + (z - t.Z) * t.Tx) <= c.Half + 1;
        }
        var res = new List<PlacedObject>();
        int inBox = 0, moved = 0, dropped = 0, nob = 0;
        foreach (var o in all)
        {
            if (!InBox(o.X, o.Z)) { res.Add(o); continue; }
            inBox++;
            if (!bounds.TryGetValue(o.Prefab, out var b)) { nob++; res.Add(o); continue; }
            double gx = (Terr(o.X + 3.5, o.Z) - Terr(o.X - 3.5, o.Z)) / 7, gz = (Terr(o.X, o.Z + 3.5) - Terr(o.X, o.Z - 3.5)) / 7, gl = Math.Sqrt(gx * gx + gz * gz);
            double yaw = o.Rotation;
            if (gl > 0.05)
            {
                double tx = -gz / gl, tz = gx / gl;   // along the contour
                // Long side along the contour: local x → (cos, −sin) for a wider-than-deep piece, else local z → (sin, cos).
                yaw = b.SizeX >= b.SizeZ ? Math.Atan2(-tz, tx) * 180 / Math.PI : Math.Atan2(tx, tz) * 180 / Math.PI;
                yaw = (yaw % 360 + 360) % 360;
            }
            double rr = yaw * Math.PI / 180;
            var foot = new List<(double X, double Z)>();
            for (int i = 0; i <= 4; i++) for (int j = 0; j <= 4; j++)
                {
                    double u = (i / 4.0 - 0.5) * b.SizeX, v = (j / 4.0 - 0.5) * b.SizeZ;
                    foot.Add((o.X + u * Math.Cos(rr) + v * Math.Sin(rr), o.Z - u * Math.Sin(rr) + v * Math.Cos(rr)));
                }
            if (foot.Any(p => OnSlab(p.X, p.Z) || OnRoad(p.X, p.Z))) { dropped++; continue; }
            double ground = foot.Min(p => Terr(p.X, p.Z));
            res.Add(o with { Y = b.Y(ground, 0.75), Rotation = Math.Round(yaw, 1) });
            moved++;
        }
        return (res, $"{inBox} in the shed box (x −6520…−6180, z 620…1130): {moved} re-seated (yaw along the contour, mesh bottom 0.75 m under the lowest footprint terrain), {dropped} left out (footprint on a roof slab or the road){(nob > 0 ? $", {nob} without bounds unchanged" : "")}");
    }
}
