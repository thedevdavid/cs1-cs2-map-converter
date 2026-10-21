namespace NetConverter;

/// <summary>
/// Vanilla paint plan (audit/theme-remake.md §4), rasterised on a 16 m grid over the playable area with the priority
/// A1 fields &gt; P1 pavement &gt; S1 bank sand &gt; S4 dirt decals &gt; S3 bare earth &gt; G1 fertile grass &gt; G2 dry plains (S2, sand under water deeper
/// than 1.5 m, paints nothing). Slopes and heights on the CS2 playable heightmap in true metres, so both scales paint the same cells.
/// Each class is closed then opened by one cell (S1 only closed), small patches are dropped (S1 &lt; 400 m², S3 / G1 / G2 &lt; 0.01 km²) and the rest becomes
/// hole-free polygons simplified to 4 m. A1 and the PO pavement keep their own exact files; here they only block lower rules.
/// </summary>
public static class PaintPlan
{
    public const string BankSand = "Sand Surface 01", BareEarth = "Sand Surface 02", DryGrass = "Grass Surface 01", Pavement = "Pavement Surface 01";
    const int A1 = 1, P1 = 2, S1 = 3, S4 = 4, S3 = 5, G1 = 6, G2 = 7, S2 = -1;

    public sealed record Inputs(TerrainSurfaces? Sand, WaterSurface? Water, Func<double, double, double>? TerrainTrue, int[]? Fertility, GridSpec? FertilityGrid,
        IEnumerable<(double X, double Z)> Trees, IEnumerable<List<(double X, double Z)>> Fields, IEnumerable<List<(double X, double Z)>> PavementBlocks,
        IEnumerable<List<(double X, double Z)>> Decals, IEnumerable<List<(double X, double Z)>> StandIns);

    public sealed record Result(List<SurfacePolygon> Sand, List<SurfacePolygon> Grass, List<SurfacePolygon> PavementStandIns, Dictionary<string, (int Cells, int Polygons, double Km2)> PerRule, double StartMean);

    public static Result Build(Inputs inp, double half = 7168)
    {
        var g = Surfaces.FineGrid(16); int n = g.Size; double cs = g.CellSize;
        var cls = new int[n * n];
        void Paint(IEnumerable<List<(double X, double Z)>> polys, int c)
        {
            foreach (var p in polys)
            {
                if (p.Count < 3) continue;
                double x0 = p.Min(q => q.X), x1 = p.Max(q => q.X), z0 = p.Min(q => q.Z), z1 = p.Max(q => q.Z);
                int i0 = Math.Max(0, (int)((x0 - g.OriginX) / cs)), i1 = Math.Min(n - 1, (int)((x1 - g.OriginX) / cs)), j0 = Math.Max(0, (int)((z0 - g.OriginZ) / cs)), j1 = Math.Min(n - 1, (int)((z1 - g.OriginZ) / cs));
                for (int j = j0; j <= j1; j++) for (int i = i0; i <= i1; i++)
                {
                    int k = j * n + i; if (cls[k] != 0) continue;
                    var (x, z) = g.Centre(k);
                    if (Surfaces.PointInPolygon(x, z, p)) cls[k] = c;
                }
            }
        }
        Paint(inp.Fields, A1);
        Paint(inp.PavementBlocks, P1);
        Paint(inp.StandIns, P1);
        // CS1 sand cells and the water.
        int sandFlag = inp.Sand?.Legend.FirstOrDefault(kv => kv.Value.Contains("Sand", StringComparison.OrdinalIgnoreCase)).Key ?? 0;
        bool IsSand(double x, double z) => inp.Sand != null && inp.Sand.Cells.TryGetValue(inp.Sand.Grid.Index(x, z), out var v) && (sandFlag != 0 ? (v & sandFlag) != 0 : inp.Sand.Name(v).Contains("Sand"));
        (double Depth, double Surface)? Water(double x, double z) => inp.Water != null && inp.Water.Cells.TryGetValue(inp.Water.Grid.Index(x, z), out var c) && c.Depth > 0.05 ? (c.Depth, c.Surface) : null;
        var wet = new bool[n * n];
        for (int k = 0; k < n * n; k++) { var (x, z) = g.Centre(k); wet[k] = Water(x, z) != null; }
        double T(double x, double z) => inp.TerrainTrue?.Invoke(x, z) ?? 0;
        double Slope(double x, double z) { double gx = (T(x + 8, z) - T(x - 8, z)) / 16, gz = (T(x, z + 8) - T(x, z - 8)) / 16; return Math.Atan(Math.Sqrt(gx * gx + gz * gz)) * 180 / Math.PI; }
        // Nearest wet cell within r cells: (distance m, surface).
        (double D, double S)? NearWet(int k, int r)
        {
            int i = k % n, j = k / n; (double, double)? best = null;
            for (int dj = -r; dj <= r; dj++) for (int di = -r; di <= r; di++)
            {
                int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n || !wet[jj * n + ii]) continue;
                double d = Math.Sqrt(di * di + dj * dj) * cs; if (best is { } b && b.Item1 <= d) continue;
                var (x, z) = g.Centre(jj * n + ii); best = (d, Water(x, z)!.Value.Surface);
            }
            return best;
        }
        for (int k = 0; k < n * n; k++)
        {
            if (cls[k] != 0) continue;
            var (x, z) = g.Centre(k);
            if (!IsSand(x, z)) continue;
            if (Water(x, z) != null && T(x, z) >= 95) continue;   // round 34: island tops above the shore band (playable terrain ≥ 95 m) stay grass
            if (Water(x, z) is { } w && w.Depth > 1.5) { cls[k] = S2; continue; }
            var nw = NearWet(k, 6);
            cls[k] = nw is { } q && (q.D <= 25 || T(x, z) <= q.S + 2) ? S1 : S3;
        }
        Paint(inp.Decals, S4);   // fills the cells sand did not take (S4 ranks below S1 only)
        // Decals over S3 cells: S4 outranks S3.
        foreach (var p in inp.Decals)
        {
            if (p.Count < 3) continue;
            double x0 = p.Min(q => q.X), x1 = p.Max(q => q.X), z0 = p.Min(q => q.Z), z1 = p.Max(q => q.Z);
            for (int j = Math.Max(0, (int)((z0 - g.OriginZ) / cs)); j <= Math.Min(n - 1, (int)((z1 - g.OriginZ) / cs)); j++)
                for (int i = Math.Max(0, (int)((x0 - g.OriginX) / cs)); i <= Math.Min(n - 1, (int)((x1 - g.OriginX) / cs)); i++)
                { int k = j * n + i; var (x, z) = g.Centre(k); if (cls[k] == S3 && Surfaces.PointInPolygon(x, z, p)) cls[k] = S4; }
        }
        // Trees per cell.
        var treeCount = new int[n * n];
        foreach (var (x, z) in inp.Trees) { int i = (int)((x - g.OriginX) / cs), j = (int)((z - g.OriginZ) / cs); if (i >= 0 && j >= 0 && i < n && j < n) treeCount[j * n + i]++; }
        int TreesNear(int k, int r) { int i = k % n, j = k / n, s = 0; for (int dj = -r; dj <= r; dj++) for (int di = -r; di <= r; di++) { int ii = i + di, jj = j + dj; if (ii >= 0 && jj >= 0 && ii < n && jj < n) s += treeCount[jj * n + ii]; } return s; }
        double Fert(double x, double z) => inp.Fertility != null && inp.FertilityGrid != null && inp.FertilityGrid.Index(x, z) is var fi && fi >= 0 && fi < inp.Fertility.Length ? inp.Fertility[fi] / 255.0 : 0;
        // Start area: the 5 × 5 central 623 m tiles (CS2's starting tiles sit at the centre); mean true height.
        double sum = 0; int cnt = 0;
        for (double x = -1557; x <= 1557; x += 100) for (double z = -1557; z <= 1557; z += 100) { sum += T(x, z); cnt++; }
        double startMean = cnt > 0 ? sum / cnt : 0;
        for (int k = 0; k < n * n; k++)
        {
            if (cls[k] != 0 || wet[k]) continue;
            var (x, z) = g.Centre(k);
            double f = Fert(x, z), sl = Slope(x, z);
            if (f >= 0.5 && TreesNear(k, 1) < 3 && sl < 8) { cls[k] = G1; continue; }
            // G2 "east of the range": the plains east of x = 0 [U, the theme plan names no boundary].
            if (x >= 0 && T(x, z) <= startMean + 60 && TreesNear(k, 3) == 0 && sl < 12 && f < 0.5) cls[k] = G2;
        }
        // Per class: close then open by one cell (bank strips S1 only closed: they are one or two cells wide), small patches dropped, polygons.
        var perRule = new Dictionary<string, (int, int, double)>();
        List<List<(double X, double Z)>> Polys(int[] classes, double minArea, string name, bool open = true)
        {
            var m = new bool[n * n]; for (int k = 0; k < n * n; k++) m[k] = classes.Contains(cls[k]);
            // Owner review of A-v5: close and open by two cells, simplify to half a cell, then organic outlines (SmoothRings).
            m = Close(Close(m, n), n); m = Open(Open(m, n), n);
            var set = new HashSet<int>(); for (int k = 0; k < n * n; k++) if (m[k]) set.Add(k);
            var (polys, _, _) = Surfaces.MaskPolygons(set, g, Math.Max(minArea, SmoothRings.MinArea), cs / 2, 3000);
            var smooth = SmoothRings.Smooth(polys, name.Aggregate(1469598103934665603UL, (h, ch) => (h ^ ch) * 1099511628211UL), half);
            perRule[name] = (set.Count, smooth.Count, smooth.Sum(p => Math.Abs(Surfaces.Area(p))) / 1e6);
            return smooth;
        }
        var sand = new List<SurfacePolygon>();
        sand.AddRange(Polys([S1], 1500, "S1 bank sand").Select((p, i) => new SurfacePolygon($"s1_{i + 1}", BankSand, p, "S1 bank sand")));
        sand.AddRange(Polys([S3, S4], 10000, "S3 bare earth + S4 dirt decals").Select((p, i) => new SurfacePolygon($"s3_{i + 1}", BareEarth, p, "S3/S4 bare earth")));
        var grass = Polys([G1, G2], 10000, "G1 fertile + G2 dry plains").Select((p, i) => new SurfacePolygon($"g_{i + 1}", DryGrass, p, "G1/G2 dry grass")).ToList();
        var standIn = inp.StandIns.Where(p => p.Count >= 3).Select((p, i) => new SurfacePolygon($"p1_{i + 1}", Pavement, Surfaces.ClipSquare(p, half), "P1 under a stand-in building without a lot")).Where(s => s.Ring.Count >= 3).ToList();
        perRule["S2 submerged sand (none)"] = (cls.Count(c => c == S2), 0, 0);
        perRule["P1 stand-in pavement"] = (0, standIn.Count, standIn.Sum(s => Math.Abs(Surfaces.Area(s.Ring))) / 1e6);
        return new Result(sand, grass, standIn, perRule, startMean);
    }

    static bool[] Erode(bool[] m, int n) { var r = new bool[m.Length]; for (int j = 1; j < n - 1; j++) for (int i = 1; i < n - 1; i++) { int k = j * n + i; r[k] = m[k] && m[k - 1] && m[k + 1] && m[k - n] && m[k + n]; } return r; }
    static bool[] Dilate(bool[] m, int n) { var r = new bool[m.Length]; for (int j = 0; j < n; j++) for (int i = 0; i < n; i++) { int k = j * n + i; r[k] = m[k] || (i > 0 && m[k - 1]) || (i < n - 1 && m[k + 1]) || (j > 0 && m[k - n]) || (j < n - 1 && m[k + n]); } return r; }
    static bool[] Open(bool[] m, int n) => Dilate(Erode(m, n), n);
    /// <summary>The paint plan's morphology: closed then opened by two cells.</summary>
    public static bool[] CloseOpen(bool[] m, int n) => Open(Open(Close(Close(m, n), n), n), n);
    public static bool[] Grow(bool[] m, int n) => Dilate(m, n);
    public static bool[] CloseOnly(bool[] m, int n) => Close(Close(m, n), n);
    static bool[] Close(bool[] m, int n) => Erode(Dilate(m, n), n);
}
