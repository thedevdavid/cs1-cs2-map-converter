namespace NetConverter;

/// <summary>
/// A CS1 terrain-surface class as soft blobs (owner review of A-v4: the 16 m sand cells rendered as big tan rectangles): connected
/// patches of at least <c>minArea</c>, grown by a round buffer, smoothed by majority passes on a 4 m grid, hole-free, simplified.
/// </summary>
public static class SmoothSurfaces
{
    public static (List<List<(double X, double Z)>> Polygons, int Patches, int Kept) Blobs(TerrainSurfaces t, string className, double minArea = 2000, double buffer = 12,
        int passes = 2, double tol = 4, double half = 7168)
    {
        var flag = t.Legend.FirstOrDefault(kv => kv.Value.Contains(className, StringComparison.OrdinalIgnoreCase)).Key;
        var coarse = t.Cells.Where(kv => flag != 0 ? (kv.Value & flag) != 0 : t.Name(kv.Value).Contains(className, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key).ToHashSet();
        int cn = t.Grid.Size; double cs = t.Grid.CellSize;
        // Patches on the coarse grid (4-neighbour), area filter.
        var seen = new HashSet<int>(); var keep = new List<int>(); int patches = 0, kept = 0;
        foreach (var s in coarse)
        {
            if (!seen.Add(s)) continue;
            var comp = new List<int> { s }; var q = new Queue<int>(); q.Enqueue(s);
            while (q.Count > 0)
            {
                int i = q.Dequeue(), r = i / cn, c = i % cn;
                foreach (var j in new[] { c > 0 ? i - 1 : -1, c < cn - 1 ? i + 1 : -1, r > 0 ? i - cn : -1, r < cn - 1 ? i + cn : -1 })
                    if (j >= 0 && coarse.Contains(j) && seen.Add(j)) { comp.Add(j); q.Enqueue(j); }
            }
            patches++;
            if (comp.Count * cs * cs >= minArea) { keep.AddRange(comp); kept++; }
        }
        // Fine 4 m grid over the map; each kept coarse cell fills its fine cells, then the round buffer.
        var g = Surfaces.FineGrid(4); int n = g.Size; double fc = g.CellSize;
        var m = new bool[n * n];
        int rad = (int)Math.Ceiling(buffer / fc);
        var disc = new List<(int, int)>();
        for (int dr = -rad; dr <= rad; dr++) for (int dc = -rad; dc <= rad; dc++) if ((dr * dr + dc * dc) * fc * fc <= buffer * buffer) disc.Add((dr, dc));
        int per = (int)Math.Round(cs / fc);
        var fine = new HashSet<int>();
        foreach (var i in keep)
        {
            var (x, z) = t.Grid.Centre(i);
            int c0 = (int)Math.Floor((x - cs / 2 - g.OriginX) / fc), r0 = (int)Math.Floor((z - cs / 2 - g.OriginZ) / fc);
            for (int a = 0; a < per; a++) for (int b = 0; b < per; b++) { int r = r0 + a, c = c0 + b; if (r >= 0 && r < n && c >= 0 && c < n) fine.Add(r * n + c); }
        }
        foreach (var i in fine)
        {
            int r = i / n, c = i % n;
            foreach (var (dr, dc) in disc) { int rr = r + dr, cc = c + dc; if (rr >= 0 && rr < n && cc >= 0 && cc < n) m[rr * n + cc] = true; }
        }
        // Majority smoothing (≥ 5 of the 3 × 3 block) over the bounding box.
        int rMin = n, rMax = -1, cMin = n, cMax = -1;
        for (int i = 0; i < m.Length; i++) if (m[i]) { int r = i / n, c = i % n; rMin = Math.Min(rMin, r); rMax = Math.Max(rMax, r); cMin = Math.Min(cMin, c); cMax = Math.Max(cMax, c); }
        for (int p = 0; p < passes && rMax >= 0; p++)
        {
            var next = (bool[])m.Clone();
            for (int r = Math.Max(1, rMin - 1); r <= Math.Min(n - 2, rMax + 1); r++)
                for (int c = Math.Max(1, cMin - 1); c <= Math.Min(n - 2, cMax + 1); c++)
                {
                    int s = 0;
                    for (int dr = -1; dr <= 1; dr++) for (int dc = -1; dc <= 1; dc++) if (m[(r + dr) * n + c + dc]) s++;
                    next[r * n + c] = s >= 5;
                }
            m = next;
        }
        var mask = new HashSet<int>();
        for (int i = 0; i < m.Length; i++) if (m[i]) mask.Add(i);
        var (polys, _, _) = Surfaces.MaskPolygons(mask, g, minArea, tol, 2000);
        return (polys.Select(pl => Surfaces.ClipSquare(pl, half)).Where(pl => pl.Count >= 3).ToList(), patches, kept);
    }
}
