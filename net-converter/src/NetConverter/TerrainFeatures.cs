namespace NetConverter;

/// <summary>
/// Round 27: the landscape spots of the revised paint plan (audit/theme-remake.md §4, N1–N13) as terrain-paint layers with the four editor brushes
/// (Dry / Lush / Dirt / Sand), one file per feature and brush. 16 m grid over the playable area, true heights from the playable CS2 heightmap.
/// Areas become organic polygons (SmoothRings) with the round-26 feather pass; rings, streaks and dabs become strokes (radius and position jittered,
/// seeded per feature). Order in the runner (later strokes add weight): N1, N12, N13, N6, N4, N5, N3, N2, N8, N7, N11, N9, N10.
/// </summary>
public static class TerrainFeatures
{
    public sealed record Inputs(PaintPlan.Inputs P, int[]? Ore, int[]? Oil, IEnumerable<OutEdge> Edges, IEnumerable<(double X, double Z)> Buildings, (double X, double Z)? DamUpstream);

    public static List<TerrainPaint.Layer> Build(Inputs inp, List<string> log, double half = 7168)
    {
        var P = inp.P;
        var g = Surfaces.FineGrid(16); int n = g.Size; double cs = g.CellSize; int N = n * n;
        double T(double x, double z) => P.TerrainTrue?.Invoke(x, z) ?? 0;
        var ht = new double[N]; var slope = new double[N]; var gxA = new double[N]; var gzA = new double[N];
        var wet = new bool[N]; var surf = new double[N]; var sand = new bool[N]; var block = new bool[N]; var trees = new int[N];
        int sandFlag = P.Sand?.Legend.FirstOrDefault(kv => kv.Value.Contains("Sand", StringComparison.OrdinalIgnoreCase)).Key ?? 0;
        for (int k = 0; k < N; k++)
        {
            var (x, z) = g.Centre(k);
            ht[k] = T(x, z);
            double gx = (T(x + 8, z) - T(x - 8, z)) / 16, gz = (T(x, z + 8) - T(x, z - 8)) / 16;
            gxA[k] = gx; gzA[k] = gz; slope[k] = Math.Atan(Math.Sqrt(gx * gx + gz * gz)) * 180 / Math.PI;
            if (P.Water != null && P.Water.Cells.TryGetValue(P.Water.Grid.Index(x, z), out var c) && c.Depth > 0.05) { wet[k] = true; surf[k] = c.Surface; }
            sand[k] = P.Sand != null && P.Sand.Cells.TryGetValue(P.Sand.Grid.Index(x, z), out var v) && (sandFlag != 0 ? (v & sandFlag) != 0 : P.Sand.Name(v).Contains("Sand"));
        }
        void Mark(bool[] m, IEnumerable<List<(double X, double Z)>> polys)
        {
            foreach (var p in polys)
            {
                if (p.Count < 3) continue;
                double x0 = p.Min(q => q.X), x1 = p.Max(q => q.X), z0 = p.Min(q => q.Z), z1 = p.Max(q => q.Z);
                for (int j = Math.Max(0, (int)((z0 - g.OriginZ) / cs)); j <= Math.Min(n - 1, (int)((z1 - g.OriginZ) / cs)); j++)
                    for (int i = Math.Max(0, (int)((x0 - g.OriginX) / cs)); i <= Math.Min(n - 1, (int)((x1 - g.OriginX) / cs)); i++)
                    { int k = j * n + i; if (m[k]) continue; var (x, z) = g.Centre(k); if (Surfaces.PointInPolygon(x, z, p)) m[k] = true; }
            }
        }
        Mark(block, P.Fields); Mark(block, P.PavementBlocks);
        var road = new bool[N];
        foreach (var e in inp.Edges.Where(e => e.Map.Imported))
            for (int s = 0; s <= 32; s++) { var p = e.Curve.Eval(s / 32.0); int i = (int)((p.X - g.OriginX) / cs), j = (int)((p.Z - g.OriginZ) / cs); if (i >= 0 && j >= 0 && i < n && j < n) road[j * n + i] = true; }
        foreach (var (x, z) in P.Trees) { int i = (int)((x - g.OriginX) / cs), j = (int)((z - g.OriginZ) / cs); if (i >= 0 && j >= 0 && i < n && j < n) trees[j * n + i]++; }
        int Near(int[] a, int k, int r) { int i = k % n, j = k / n, s = 0; for (int dj = -r; dj <= r; dj++) for (int di = -r; di <= r; di++) { int ii = i + di, jj = j + dj; if (ii >= 0 && jj >= 0 && ii < n && jj < n) s += a[jj * n + ii]; } return s; }
        double Res(int[]? a, double x, double z) { if (a == null || P.FertilityGrid == null) return 0; int idx = P.FertilityGrid.Index(x, z); return idx >= 0 && idx < a.Length ? a[idx] / 255.0 : 0; }
        double sum = 0; int cnt = 0;
        for (double x = -1557; x <= 1557; x += 100) for (double z = -1557; z <= 1557; z += 100) { sum += T(x, z); cnt++; }
        double startMean = cnt > 0 ? sum / cnt : 0;
        bool Free(int k) => !wet[k] && !block[k] && !road[k];

        // Water bodies (4-connected wet cells): the main river is the largest; distances from water (8-neighbour chamfer, metres).
        var comp = new int[N]; Array.Fill(comp, -1); var compSize = new List<int>(); var compSurf = new List<double>();
        for (int k = 0; k < N; k++)
        {
            if (!wet[k] || comp[k] >= 0) continue;
            int id = compSize.Count, size = 0; double ss = 0; var q = new Queue<int>(); q.Enqueue(k); comp[k] = id;
            while (q.Count > 0) { int c = q.Dequeue(); size++; ss += surf[c]; int i = c % n, j = c / n; foreach (var (di, dj) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue; int nk = jj * n + ii; if (wet[nk] && comp[nk] < 0) { comp[nk] = id; q.Enqueue(nk); } } }
            compSize.Add(size); compSurf.Add(ss / size);
        }
        int main = compSize.Count == 0 ? -1 : compSize.IndexOf(compSize.Max());
        (double[] D, int[] From) Dist(Func<int, bool> src, double maxM)
        {
            var d = new double[N]; var from = new int[N]; Array.Fill(d, double.MaxValue); Array.Fill(from, -1);
            var pq = new PriorityQueue<int, double>();
            for (int k = 0; k < N; k++) if (src(k)) { d[k] = 0; from[k] = k; pq.Enqueue(k, 0); }
            while (pq.TryDequeue(out int c, out double dc))
            {
                if (dc > d[c] || dc > maxM) continue;
                int i = c % n, j = c / n;
                for (int dj = -1; dj <= 1; dj++) for (int di = -1; di <= 1; di++)
                {
                    if (di == 0 && dj == 0) continue; int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue;
                    int nk = jj * n + ii; double nd = dc + cs * (di != 0 && dj != 0 ? 1.4142 : 1);
                    if (nd < d[nk]) { d[nk] = nd; from[nk] = from[c]; pq.Enqueue(nk, nd); }
                }
            }
            return (d, from);
        }
        var (dWater, fromWater) = Dist(k => wet[k], 160);

        List<List<(double X, double Z)>> Smooth(bool[] m, string name, bool open = true)
        {
            m = open ? PaintPlan.CloseOpen(m, n) : PaintPlan.CloseOnly(m, n);   // thin features (scree, rims) are only closed, not opened
            var set = new HashSet<int>(); for (int k = 0; k < N; k++) if (m[k]) set.Add(k);
            var (polys, _, _) = Surfaces.MaskPolygons(set, g, SmoothRings.MinArea, cs / 2, 3000);
            return SmoothRings.Smooth(polys, name.Aggregate(1469598103934665603UL, (h, ch) => (h ^ ch) * 1099511628211UL), half);
        }
        List<List<(double X, double Z)>> Feather(bool[] m, string name, bool open = true)
        {
            m = (open ? PaintPlan.CloseOpen(m, n) : PaintPlan.CloseOnly(m, n));
            return Smooth(PaintPlan.Grow(m, n), name + " feather", false);
        }
        var layers = new List<TerrainPaint.Layer>();
        void Area(string file, string material, string role, bool[] m, double strength, double softEdge, bool open = true)
        {
            var polys = Smooth(m, file, open);
            layers.Add(new TerrainPaint.Layer(file, material, role, polys.Select((r, i) => ($"{Path.GetFileNameWithoutExtension(file).Replace("import-terrain-paint-", "")}_{i + 1}", r, role)).ToList(),
                SoftEdge: softEdge, Feather: Feather(m, file, open), Strength: strength));
            log.Add(FormattableString.Invariant($"{file}: {material} {strength}, {polys.Count} polygons, {polys.Sum(p => Math.Abs(Surfaces.Area(p))) / 1e6:F2} km², {m.Count(b => b)} cells ({role})"));
        }
        void Strokes(string file, string material, string role, List<(double X, double Z, double R)> st, double strength)
        {
            string tag = Path.GetFileNameWithoutExtension(file).Replace("import-terrain-paint-", "");
            layers.Add(new TerrainPaint.Layer(file, material, role, new(), st.Select((s, i) => ($"{tag}_{i + 1}", Math.Round(s.X, 2), Math.Round(s.Z, 2), Math.Round(s.R, 1))).ToList(),
                Strength: strength, FeatherStrokes: false));
            log.Add(FormattableString.Invariant($"{file}: {material} {strength}, {st.Count} strokes ({role})"));
        }
        Random Rng(string name) => new((int)(name.Aggregate(2166136261u, (h, ch) => (h ^ ch) * 16777619u) & 0x7fffffff));
        (double X, double Z) Jit(Random r, int k, double amp) { var (x, z) = g.Centre(k); return (x + (r.NextDouble() - 0.5) * 2 * amp, z + (r.NextDouble() - 0.5) * 2 * amp); }

        // N1 cottonwood / willow bottoms: within 150 m of the river or a lake, ≤ 4 m above its surface, not sand → Lush 0.6, soft 30 m.
        var n1 = new bool[N];
        for (int k = 0; k < N; k++) if (Free(k) && !sand[k] && dWater[k] <= 150 && fromWater[k] >= 0 && ht[k] - surf[fromWater[k]] <= 4) n1[k] = true;
        Area("import-terrain-paint-n1-riparian-lush.json", "Lush", "N1 cottonwood / willow bottoms", n1, 0.6, 30);

        // N12 fertile land: fertility ≥ 0.5, not field, not forest → Dry 0.35.
        var n12 = new bool[N];
        for (int k = 0; k < N; k++) { var (x, z) = g.Centre(k); if (Free(k) && Res(P.Fertility, x, z) >= 0.5 && Near(trees, k, 1) < 3) n12[k] = true; }
        Area("import-terrain-paint-n12-fertile-dry.json", "Dry", "N12 CS1 fertile land (yellower)", n12, 0.35, 30);

        // N13 bare high ground: above the highest height with tree cover (99th percentile of tree cells), slope < 30° → Dry 0.5.
        var treeH = Enumerable.Range(0, N).Where(k => trees[k] > 0).Select(k => ht[k]).OrderBy(h => h).ToList();
        double treeline = treeH.Count > 0 ? treeH[(int)(treeH.Count * 0.99)] : double.MaxValue;
        var n13 = new bool[N];
        for (int k = 0; k < N; k++) if (Free(k) && ht[k] > treeline && slope[k] < 30) n13[k] = true;
        Area("import-terrain-paint-n13-high-ground-dry.json", "Dry", FormattableString.Invariant($"N13 bare high ground above the treeline {treeline:F0} m"), n13, 0.5, 30);

        // Foothill band (N4, N6): start mean + 60 … + 450 m.
        bool Foothill(int k) => ht[k] >= startMean + 60 && ht[k] <= startMean + 450;
        // N6 ponderosa savanna floor: band, 1–6 trees per cell, slope < 25° → Dry 0.4 + Dirt dabs r 4–8.
        var n6 = new bool[N]; var r6 = Rng("n6"); var dabs6 = new List<(double, double, double)>();
        for (int k = 0; k < N; k++)
            if (Free(k) && Foothill(k) && trees[k] >= 1 && trees[k] <= 6 && slope[k] < 25)
            { n6[k] = true; if (r6.NextDouble() < 0.3) { var (x, z) = Jit(r6, k, 8); dabs6.Add((x, z, 4 + 4 * r6.NextDouble())); } }
        Area("import-terrain-paint-n6-savanna-dry.json", "Dry", "N6 ponderosa savanna floor", n6, 0.4, 20, open: false);
        Strokes("import-terrain-paint-n6-savanna-dirt.json", "Dirt", "N6 bare soil dabs between the pines", dabs6, 0.45);

        // N4 hogback sandstone: band, slope 18–38°, east-facing (downhill bearing 45–135°) → Sand 0.5 streaks along the contour.
        var r4 = Rng("n4"); var st4 = new List<(double, double, double)>(); int c4 = 0;
        for (int k = 0; k < N; k++)
        {
            if (!Free(k) || !Foothill(k) || slope[k] < 18 || slope[k] > 38) continue;
            double bearing = (Math.Atan2(-gxA[k], -gzA[k]) * 180 / Math.PI + 360) % 360;   // downhill direction, 0 = north, 90 = east
            if (bearing < 45 || bearing > 135) continue;
            c4++;
            if (r4.NextDouble() > 0.35) continue;
            double gl = Math.Sqrt(gxA[k] * gxA[k] + gzA[k] * gzA[k]); double tx = -gzA[k] / gl, tz = gxA[k] / gl;   // along the contour
            var (x, z) = Jit(r4, k, 6);
            for (int s = -1; s <= 1; s++) st4.Add((x + tx * 9 * s, z + tz * 9 * s, (6 + 3 * r4.NextDouble()) * (0.7 + 0.6 * r4.NextDouble())));
        }
        Strokes("import-terrain-paint-n4-hogback-sand.json", "Sand", $"N4 hogback sandstone streaks along the contour ({c4} cells)", st4, 0.5);

        // N5 talus / scree aprons: slope 28–40° within 60 m downhill of slope ≥ 45° → Dirt 0.8.
        var (dCliff, fromCliff) = Dist(k => slope[k] >= 45, 64);
        var n5 = new bool[N];
        for (int k = 0; k < N; k++) if (Free(k) && slope[k] >= 28 && slope[k] <= 40 && dCliff[k] <= 60 && fromCliff[k] >= 0 && ht[k] < ht[fromCliff[k]]) n5[k] = true;
        Area("import-terrain-paint-n5-scree-dirt.json", "Dirt", "N5 talus / scree aprons below cliffs", n5, 0.8, 15, open: false);

        // N3 reservoir drawdown ring: 0–2.5 m above the reservoir behind the dam, slope < 25° → Dirt 0.7 (fading with height).
        var st3 = new List<(double, double, double)>(); var r3 = Rng("n3");
        if (inp.DamUpstream is { } du)
        {
            // The reservoir: wet cells connected to the water nearest the upstream point at the same surface (± 1.5 m); the CS1 water runs on through
            // the dam into the river below, so plain connectivity would take the whole river.
            int dk = g.Index(du.X, du.Z);
            var (dd, _) = Dist(k => k == dk, 600);
            int seed = Enumerable.Range(0, N).Where(k => wet[k] && dd[k] <= 600).OrderBy(k => dd[k]).DefaultIfEmpty(-1).First();
            if (seed >= 0)
            {
                double level = surf[seed]; var resv = new bool[N]; var q = new Queue<int>(); q.Enqueue(seed); resv[seed] = true; int cells = 0;
                while (q.Count > 0) { int c = q.Dequeue(); cells++; int i = c % n, j = c / n; foreach (var (di, dj) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue; int nk = jj * n + ii; if (wet[nk] && !resv[nk] && Math.Abs(surf[nk] - level) < 1.5) { resv[nk] = true; q.Enqueue(nk); } } }
                var (dRes, _) = Dist(k => resv[k], 200);
                for (int k = 0; k < N; k++)
                    if (!wet[k] && dRes[k] <= 160 && ht[k] - level >= 0 && ht[k] - level <= 2.5 && slope[k] < 25) { var (x, z) = Jit(r3, k, 5); st3.Add((x, z, 10 * (0.8 + 0.4 * r3.NextDouble()))); }
                log.Add(FormattableString.Invariant($"N3: reservoir level {level:F1} m ({cells} wet cells at ± 1.5 m)"));
            }
        }
        Strokes("import-terrain-paint-n3-drawdown-dirt.json", "Dirt", "N3 reservoir drawdown ring, 0–2.5 m above the level", st3, 0.7);

        // N2 gravel-pit ponds: lakes < 0.3 km² within 1.5 km of the main river, flat shores (< 3°) → Dirt ring 8–25 m + Sand 0–8 m.
        var (dMain, _) = Dist(k => main >= 0 && comp[k] == main, 1500);
        var pits = new HashSet<int>();
        for (int id = 0; id < compSize.Count; id++) if (id != main && compSize[id] * cs * cs < 0.3e6) pits.Add(id);
        var st2d = new List<(double, double, double)>(); var st2s = new List<(double, double, double)>(); var r2 = Rng("n2"); var used = new HashSet<int>();
        for (int k = 0; k < N; k++)
        {
            if (wet[k] || fromWater[k] < 0 || !pits.Contains(comp[fromWater[k]]) || dWater[k] > 25) continue;
            if (dMain[fromWater[k]] > 1500 || slope[k] >= 3) continue;
            used.Add(comp[fromWater[k]]);
            var (x, z) = Jit(r2, k, 4);
            if (dWater[k] <= 16) st2s.Add((x, z, 6 * (0.7 + 0.6 * r2.NextDouble())));
            st2d.Add((x, z, 10 * (0.7 + 0.6 * r2.NextDouble())));
        }
        Strokes("import-terrain-paint-n2-pit-banks-dirt.json", "Dirt", $"N2 gravel-pit pond banks, {used.Count} ponds", st2d, 0.6);
        Strokes("import-terrain-paint-n2-pit-banks-sand.json", "Sand", "N2 gravel-pit pond shore", st2s, 0.6);

        // N8 playas: closed depressions on the plains (priority-flood fill), depth 0.4–3 m, 0.5–20 ha, dry → Lush rim (outer 30 %) + Dirt centre.
        var fill = (double[])ht.Clone();
        {
            var pq = new PriorityQueue<int, double>(); var done = new bool[N];
            for (int k = 0; k < N; k++) { int i = k % n, j = k / n; if (i == 0 || j == 0 || i == n - 1 || j == n - 1 || wet[k]) { done[k] = true; pq.Enqueue(k, fill[k]); } }
            while (pq.TryDequeue(out int c, out double hc))
            {
                int i = c % n, j = c / n;
                foreach (var (di, dj) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue; int nk = jj * n + ii;
                    if (done[nk]) continue; done[nk] = true; fill[nk] = Math.Max(fill[nk], hc); pq.Enqueue(nk, fill[nk]);
                }
            }
        }
        var n8rim = new bool[N]; var n8mid = new bool[N]; int playas = 0;
        {
            var seen = new bool[N];
            for (int k = 0; k < N; k++)
            {
                if (seen[k] || fill[k] - ht[k] < 0.05 || wet[k]) continue;
                var cells = new List<int>(); var q = new Queue<int>(); q.Enqueue(k); seen[k] = true;
                while (q.Count > 0) { int c = q.Dequeue(); cells.Add(c); int i = c % n, j = c / n; foreach (var (di, dj) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int ii = i + di, jj = j + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue; int nk = jj * n + ii; if (!seen[nk] && !wet[nk] && fill[nk] - ht[nk] >= 0.05) { seen[nk] = true; q.Enqueue(nk); } } }
                double depth = cells.Max(c => fill[c] - ht[c]), area = cells.Count * cs * cs;
                bool plains = cells.All(c => ht[c] <= startMean + 60 && slope[c] < 8);
                if (depth < 0.4 || depth > 3 || area < 5000 || area > 200000 || !plains || cells.Any(c => block[c])) continue;
                playas++;
                foreach (var c in cells) { if (fill[c] - ht[c] < 0.3 * depth) n8rim[c] = true; else n8mid[c] = true; }
            }
        }
        Area("import-terrain-paint-n8-playa-rim-lush.json", "Lush", $"N8 playa rims ({playas} playas)", n8rim, 0.5, 15, open: false);
        Area("import-terrain-paint-n8-playa-centre-dirt.json", "Dirt", "N8 playa centres (dried mud)", n8mid, 0.6, 10, open: false);

        // N7 prairie dog towns: flat plains (< 4°), no tree within 100 m, free, fertility < 0.5; ~1 town per 12 km² of eligible plains, 3–15 ha each.
        var elig = Enumerable.Range(0, N).Where(k => { var (x, z) = g.Centre(k); return Free(k) && slope[k] < 4 && ht[k] <= startMean + 60 && Near(trees, k, 6) == 0 && Res(P.Fertility, x, z) < 0.5; }).ToList();
        {
            int a = 0, b = 0, c = 0, d = 0;
            for (int k = 0; k < N; k++) { if (!Free(k)) continue; a++; if (slope[k] >= 4 || ht[k] > startMean + 60) continue; b++; if (Near(trees, k, 6) != 0) continue; c++; var (x, z) = g.Centre(k); if (Res(P.Fertility, x, z) < 0.5) d++; }
            log.Add($"N7 eligibility: free {a}, flat plains {b}, no tree within 100 m {c}, fertility < 0.5 {d}");
        }
        var r7 = Rng("n7"); int towns = (int)Math.Round(elig.Count * cs * cs / 12e6);
        // At least one town when 0.15 km² qualifies (a town is 3–15 ha). Round 34 kept more trees (the tree mask moved to the playable heightmap), and the
        // eligible plains fell from 1.0 to 0.47 km², under the earlier 0.5 km² floor; A-v10 got an empty N7 file.
        if (towns == 0 && elig.Count * cs * cs >= 0.15e6) towns = 1;   // the open plains are small on this map (trees almost everywhere): at least one town
        var n7 = new bool[N]; var dabs7 = new List<(double, double, double)>(); var eligSet = elig.ToHashSet();
        for (int t = 0; t < towns && elig.Count > 0; t++)
        {
            int ck = elig[r7.Next(elig.Count)]; var (cx, cz) = g.Centre(ck);
            double areaHa = 3 + 12 * r7.NextDouble(), rad = Math.Sqrt(areaHa * 1e4 / Math.PI);
            int rc = (int)Math.Ceiling(rad / cs), ci = ck % n, cj = ck / n; var cellsT = new List<int>();
            for (int dj = -rc; dj <= rc; dj++) for (int di = -rc; di <= rc; di++)
            {
                int ii = ci + di, jj = cj + dj; if (ii < 0 || jj < 0 || ii >= n || jj >= n) continue; int nk = jj * n + ii;
                double wob = 1 + 0.15 * Math.Sin(Math.Atan2(dj, di) * 3 + t);
                if (Math.Sqrt(di * di + dj * dj) * cs <= rad * wob && eligSet.Contains(nk)) { n7[nk] = true; cellsT.Add(nk); }
            }
            double cover = 0, target = 0.3 * cellsT.Count * cs * cs;
            while (cover < target && cellsT.Count > 0) { int kk = cellsT[r7.Next(cellsT.Count)]; var (x, z) = Jit(r7, kk, 8); double r = 3 + 3 * r7.NextDouble(); dabs7.Add((x, z, r)); cover += Math.PI * r * r; }
        }
        Area("import-terrain-paint-n7-prairie-dog-dry.json", "Dry", $"N7 prairie dog towns ({towns})", n7, 0.8, 15, open: false);
        Strokes("import-terrain-paint-n7-prairie-dog-dirt.json", "Dirt", "N7 prairie dog mounds (30 % cover)", dabs7, 0.5);

        // N11 CS1 ore / oil ground: ore ≥ 0.4 → Dirt 0.5, oil ≥ 0.4 → Dirt 0.3, soft 20 m.
        var n11o = new bool[N]; var n11l = new bool[N];
        for (int k = 0; k < N; k++) { if (!Free(k)) continue; var (x, z) = g.Centre(k); if (Res(inp.Ore, x, z) >= 0.4) n11o[k] = true; else if (Res(inp.Oil, x, z) >= 0.4) n11l[k] = true; }
        Area("import-terrain-paint-n11-ore-dirt.json", "Dirt", "N11 CS1 ore ground", n11o, 0.5, 20);
        Area("import-terrain-paint-n11-oil-dirt.json", "Dirt", "N11 CS1 oil ground (well pads)", n11l, 0.3, 20);

        // N9 dirt / gravel roads and tracks (CS1 gravel / dirt prefabs): Dirt 0.5, 3–5 m beyond each edge; N10 rail ballast outside towns: Dirt 0.35, 2–3 m.
        var r9 = Rng("n9"); var st9 = new List<(double, double, double)>(); var st10 = new List<(double, double, double)>();
        var bld = new Dictionary<(int, int), int>();
        foreach (var (x, z) in inp.Buildings) { var key = ((int)Math.Floor(x / 200), (int)Math.Floor(z / 200)); bld[key] = bld.GetValueOrDefault(key) + 1; }
        int Town(double x, double z) { int s = 0, bx = (int)Math.Floor(x / 200), bz = (int)Math.Floor(z / 200); for (int a = -1; a <= 1; a++) for (int b = -1; b <= 1; b++) s += bld.GetValueOrDefault((bx + a, bz + b)); return s; }
        foreach (var e in inp.Edges.Where(e => e.Map.Imported && e.Structure == Structure.Ground))
        {
            string cs1 = e.Map.Cs1Prefab ?? "", cs2 = e.Map.Cs2Prefab ?? "";
            bool dirtRoad = cs2.Contains("Gravel") || System.Text.RegularExpressions.Regex.IsMatch(cs1, "(?i)gravel|dirt|unpaved|trail|track(?!.*train)");
            bool rail = e.Layer == Layers.Rail;
            if (!dirtRoad && !rail) continue;
            double hw = FootprintShift.Cs2Half(e), r = rail ? hw + 2.5 : hw + 4, len = e.Curve.LengthXZ();
            int steps = Math.Max(1, (int)(len / (r * 0.9)));
            for (int s = 0; s <= steps; s++)
            {
                var p = e.Curve.Eval((double)s / steps);
                if (rail && Town(p.X, p.Z) > 6) continue;
                (rail ? st10 : st9).Add((p.X + (r9.NextDouble() - 0.5) * 2, p.Z + (r9.NextDouble() - 0.5) * 2, r * (0.9 + 0.2 * r9.NextDouble())));
            }
        }
        Strokes("import-terrain-paint-n9-dirt-roads-dirt.json", "Dirt", "N9 dirt / gravel road and track edges", st9, 0.5);
        Strokes("import-terrain-paint-n10-rail-ballast-dirt.json", "Dirt", "N10 rail ballast outside towns", st10, 0.35);
        return layers;
    }
}
