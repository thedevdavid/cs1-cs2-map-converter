using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetConverter;

public sealed record SurfacePolygon(string Id, string Prefab, List<(double X, double Z)> Ring, string Note);

/// <summary>
/// Area surfaces for POST /import/surfaces (cs2mcp/SURFACES.md): one hole-free outer ring per surface, 3–2000 points (kept
/// under MaxPoints), CS2 metres within ±7168. Shapes are rasterised, cleaned of diagonal pinches, split through every hole,
/// traced along cell edges and simplified with Douglas–Peucker.
/// </summary>
public static class Surfaces
{
    public const double Half = 7168;
    public const int MaxPoints = 500;

    /// <summary>The 4 m grid over the CS2 square used for rasterising footprints.</summary>
    public static GridSpec FineGrid(double cell = 4) => new((int)Math.Round(2 * Half / cell), cell, -Half, -Half);

    /// <summary>Marks every cell whose centre lies within radius of a point.</summary>
    public static void Disc(HashSet<int> cells, GridSpec g, double x, double z, double radius)
    {
        int c0 = (int)Math.Floor((x - radius - g.OriginX) / g.CellSize), c1 = (int)Math.Floor((x + radius - g.OriginX) / g.CellSize);
        int r0 = (int)Math.Floor((z - radius - g.OriginZ) / g.CellSize), r1 = (int)Math.Floor((z + radius - g.OriginZ) / g.CellSize);
        for (int r = Math.Max(0, r0); r <= Math.Min(g.Size - 1, r1); r++)
            for (int c = Math.Max(0, c0); c <= Math.Min(g.Size - 1, c1); c++)
            {
                double cx = g.OriginX + (c + 0.5) * g.CellSize - x, cz = g.OriginZ + (r + 0.5) * g.CellSize - z;
                if (cx * cx + cz * cz <= radius * radius) cells.Add(r * g.Size + c);
            }
    }

    /// <summary>Footprint of a network segment: its half-width swept along the Bézier.</summary>
    public static void Sweep(HashSet<int> cells, GridSpec g, Bezier b, double halfWidth)
    {
        int n = Math.Max(2, (int)Math.Ceiling(b.LengthXZ() / (g.CellSize / 2)));
        for (int i = 0; i <= n; i++) { var p = b.Eval(i / (double)n); Disc(cells, g, p.X, p.Z, halfWidth); }
    }

    /// <summary>Fills one empty cell of every 2×2 block whose filled cells touch only diagonally (pinches make invalid rings).</summary>
    public static void FixPinches(HashSet<int> cells, GridSpec g)
    {
        for (int pass = 0; pass < 10; pass++)
        {
            var add = new List<int>();
            foreach (var i in cells)
            {
                int r = i / g.Size, c = i % g.Size;
                if (r + 1 >= g.Size) continue;
                // i = bottom-left of a block: pattern [x . / . x] or check the block to the left for [. x / x .].
                if (c + 1 < g.Size && cells.Contains(i + g.Size + 1) && !cells.Contains(i + 1) && !cells.Contains(i + g.Size)) add.Add(i + 1);
                if (c > 0 && cells.Contains(i + g.Size - 1) && !cells.Contains(i - 1) && !cells.Contains(i + g.Size)) add.Add(i + g.Size);
            }
            if (add.Count == 0) return;
            foreach (var a in add) cells.Add(a);
        }
    }

    static List<HashSet<int>> Components(HashSet<int> cells, GridSpec g)
    {
        var seen = new HashSet<int>(); var res = new List<HashSet<int>>();
        foreach (var s in cells)
        {
            if (!seen.Add(s)) continue;
            var comp = new HashSet<int>(); var st = new Stack<int>(); st.Push(s);
            while (st.Count > 0)
            {
                int i = st.Pop(); comp.Add(i);
                int r = i / g.Size, c = i % g.Size;
                foreach (var j in new[] { c > 0 ? i - 1 : -1, c < g.Size - 1 ? i + 1 : -1, r > 0 ? i - g.Size : -1, r < g.Size - 1 ? i + g.Size : -1 })
                    if (j >= 0 && cells.Contains(j) && seen.Add(j)) st.Push(j);
            }
            res.Add(comp);
        }
        return res;
    }

    static List<(int Class, List<(double X, double Z)> Ring, bool Outer)> Trace(HashSet<int> cells, GridSpec g, double tol)
    {
        var t = new TerrainSurfaces { Grid = g };
        foreach (var i in cells) t.Cells[i] = 1;
        return t.Rings(tol);
    }

    /// <summary>Hole-free simplified rings covering the cells.</summary>
    public static List<List<(double X, double Z)>> Polygonize(HashSet<int> cells, GridSpec g, double tol = 2)
    {
        var result = new List<List<(double X, double Z)>>();
        var work = new HashSet<int>(cells);
        FixPinches(work, g);
        var queue = new Queue<HashSet<int>>(Components(work, g));
        int guard = 0;
        while (queue.Count > 0 && guard++ < 100000)
        {
            var comp = queue.Dequeue();
            var rings = Trace(comp, g, 0.01);
            var hole = rings.FirstOrDefault(r => !r.Outer);
            if (hole.Ring != null && comp.Count > 1)
            {
                // Cut through the hole along a grid column: both halves open the hole to the outside.
                int cut = (int)Math.Round((hole.Ring.Average(p => p.X) - g.OriginX) / g.CellSize);
                var left = comp.Where(i => i % g.Size < cut).ToHashSet();
                var right = comp.Where(i => i % g.Size >= cut).ToHashSet();
                if (left.Count == 0 || right.Count == 0) { cut = (int)Math.Round(comp.Average(i => i % g.Size)); left = comp.Where(i => i % g.Size < cut).ToHashSet(); right = comp.Where(i => i % g.Size >= cut).ToHashSet(); }
                foreach (var part in new[] { left, right }) { FixPinches(part, g); foreach (var c in Components(part, g)) queue.Enqueue(c); }
                continue;
            }
            foreach (var r in rings.Where(r => r.Outer))
            {
                // Re-trace at the requested tolerance, coarsening until the ring fits the point budget. A tolerance that
                // collapses a small ring (< 3 points, no outer ring left) keeps the previous ring.
                List<(double X, double Z)>? Outer(double t) => Trace(comp, g, t).FirstOrDefault(x => x.Outer).Ring;
                var ring = Outer(tol) ?? r.Ring;
                for (double t = tol * 2; ring.Count > MaxPoints && t < 200; t *= 2) ring = Outer(t) ?? ring;
                // Douglas–Peucker is not topology-preserving: fall back to finer tolerances until the ring is simple.
                for (double t = tol / 2; SelfIntersects(ring) && t >= 0.01; t /= 2) ring = Outer(t) ?? ring;
                if (ring.Count >= 3 && Math.Abs(Area(ring)) >= 1) result.Add(ring);
                break;
            }
        }
        return result;
    }

    public static bool SelfIntersects(List<(double X, double Z)> ring)
    {
        int n = ring.Count;
        static double Cross((double X, double Z) o, (double X, double Z) a, (double X, double Z) b) => (a.X - o.X) * (b.Z - o.Z) - (a.Z - o.Z) * (b.X - o.X);
        for (int i = 0; i < n; i++)
            for (int j = i + 2; j < n; j++)
            {
                if (i == 0 && j == n - 1) continue; // adjacent through the closing edge
                var a = ring[i]; var b = ring[(i + 1) % n]; var c = ring[j]; var d = ring[(j + 1) % n];
                double d1 = Cross(c, d, a), d2 = Cross(c, d, b), d3 = Cross(a, b, c), d4 = Cross(a, b, d);
                if (d1 * d2 < 0 && d3 * d4 < 0) return true;
            }
        return false;
    }

    public static double Area(List<(double X, double Z)> ring)
    {
        double a = 0;
        for (int k = 0; k < ring.Count; k++) { var p = ring[k]; var q = ring[(k + 1) % ring.Count]; a += p.X * q.Z - q.X * p.Z; }
        return a / 2;
    }

    // ------------------------------------------------------------------------------------------------ fields

    /// <summary>Field networks (CS1 "Field ... Network" segments): their footprints on the 4 m grid, inside the CS2 square.</summary>
    public static HashSet<int> FieldFootprints(Dump dump, GridSpec g, Regex fieldPrefab)
    {
        var cells = new HashSet<int>();
        foreach (var s in dump.Segments.Where(s => fieldPrefab.IsMatch(s.Prefab)))
            Sweep(cells, g, s.Bezier, dump.PrefabInfo(s.Prefab).HalfWidth);
        return cells;
    }

    /// <summary>
    /// Centre-pivot fields: CS1 draws each field as a closed ring of field-network segments; the field is the disc of radius
    /// ring radius + network half-width. Closed rings whose sampled radius varies by less than 5 m become one circle polygon
    /// each (48 points, clipped to the square), so neighbouring fields stay separate; open arcs are swept onto openCells.
    /// </summary>
    public static List<(double Cx, double Cz, double R, List<(double X, double Z)> Ring)> FieldCircles(Dump dump, Regex fieldPrefab, GridSpec g, HashSet<int> openCells)
    {
        var segs = dump.Segments.Where(s => fieldPrefab.IsMatch(s.Prefab)).ToList();
        var at = new Dictionary<int, List<DumpSegment>>();
        foreach (var sg in segs) foreach (var n in new[] { sg.StartNode, sg.EndNode }) { if (!at.TryGetValue(n, out var l)) at[n] = l = new(); l.Add(sg); }
        var seen = new HashSet<int>();
        var res = new List<(double, double, double, List<(double, double)>)>();
        foreach (var start in at.Keys)
        {
            if (!seen.Add(start)) continue;
            var nodes = new List<int>(); var ring = new HashSet<DumpSegment>(ReferenceEqualityComparer.Instance); var st = new Stack<int>(); st.Push(start);
            while (st.Count > 0)
            {
                int n = st.Pop(); nodes.Add(n);
                foreach (var sg in at[n]) { ring.Add(sg); foreach (var m in new[] { sg.StartNode, sg.EndNode }) if (seen.Add(m)) st.Push(m); }
            }
            var pts = ring.SelectMany(sg => Enumerable.Range(0, 9).Select(i => sg.Bezier.Eval(i / 8.0))).ToList();
            double cx = pts.Average(p => p.X), cz = pts.Average(p => p.Z);
            var radii = pts.Select(p => Math.Sqrt((p.X - cx) * (p.X - cx) + (p.Z - cz) * (p.Z - cz))).ToList();
            bool closed = nodes.All(n => at[n].Count == 2);
            double hw = dump.PrefabInfo(ring.First().Prefab).HalfWidth;
            if (closed && radii.Max() - radii.Min() < 5)
            {
                double r = radii.Average() + hw;
                var circle = Enumerable.Range(0, 48).Select(k => (cx + r * Math.Cos(k * Math.PI / 24), cz + r * Math.Sin(k * Math.PI / 24))).ToList();
                var clipped = ClipSquare(circle, Half);
                if (clipped.Count >= 3 && Math.Abs(Area(clipped)) >= 1) res.Add((cx, cz, r, clipped.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList()));
            }
            else
            {
                // Open arcs (part of a pivot circle): fit a circle to the CS1 field segments (owner review of A-v5: true circles, not cell blobs).
                var fit = SmoothRings.FitCircle(pts.Select(p => (p.X, p.Z)).ToList());
                if (fit.Rms < 6 && fit.R >= 40 && fit.R <= 800)
                {
                    double r = fit.R + hw;
                    var circle = Enumerable.Range(0, 48).Select(k => (fit.Cx + r * Math.Cos(k * Math.PI / 24), fit.Cz + r * Math.Sin(k * Math.PI / 24))).ToList();
                    var clipped = ClipSquare(circle, Half);
                    if (clipped.Count >= 3 && Math.Abs(Area(clipped)) >= 1) res.Add((fit.Cx, fit.Cz, r, clipped.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList()));
                }
                else foreach (var sg in ring) Sweep(openCells, g, sg.Bezier, hw);
            }
        }
        return res;
    }

    /// <summary>
    /// Steep rock: heightmap pixels (true heights) whose slope reaches minDegrees, grouped into 4-connected patches of at least
    /// minArea m², traced into hole-free pieces simplified to tol, largest first, at most cap polygons.
    /// </summary>
    public static (List<List<(double X, double Z)>> Polygons, double SteepArea, double KeptArea, int Patches, int Dropped) SteepRock(
        Heightmap hm, double minDegrees = 40, double minArea = 400, double tol = 4, int cap = 1500)
    {
        int n = hm.Size; double px = hm.MetresPerPixel, half = hm.Half;
        var g = new GridSpec(n, px, -half, -half);
        double tan = Math.Tan(minDegrees * Math.PI / 180);
        var h = new double[n * n];
        for (int r = 0; r < n; r++) for (int c = 0; c < n; c++) { var (x, z) = g.Centre(r * n + c); h[r * n + c] = hm.Sample(x, z); }
        var steep = new HashSet<int>();
        for (int r = 1; r < n - 1; r++)
            for (int c = 1; c < n - 1; c++)
            {
                double dx = (h[r * n + c + 1] - h[r * n + c - 1]) / (2 * px), dz = (h[(r + 1) * n + c] - h[(r - 1) * n + c]) / (2 * px);
                if (dx * dx + dz * dz >= tan * tan) steep.Add(r * n + c);
            }
        var (kept, patches, dropped) = MaskPolygons(steep, g, minArea, tol, cap);
        return (kept, steep.Count * px * px, kept.Sum(p => Math.Abs(Area(p))), patches, dropped);
    }

    /// <summary>Hole-free, simplified polygons of the connected patches of a cell mask (patches below minArea dropped), largest first, capped.</summary>
    public static (List<List<(double X, double Z)>> Polygons, int Patches, int Dropped) MaskPolygons(HashSet<int> mask, GridSpec g, double minArea = 400, double tol = 4, int cap = 1500)
    {
        double px = g.CellSize;
        var comps = Components(mask, g).Where(cmp => cmp.Count * px * px >= minArea).OrderByDescending(cmp => cmp.Count).ToList();
        var polys = new List<List<(double X, double Z)>>();
        foreach (var cmp in comps)
        {
            if (polys.Count >= cap * 2) break; // enough candidates; the cap below keeps the largest
            polys.AddRange(Polygonize(cmp, g, tol).Where(p => Math.Abs(Area(p)) >= minArea));
        }
        var kept = polys.OrderByDescending(p => Math.Abs(Area(p))).Take(cap).ToList();
        return (kept, comps.Count, Math.Max(0, polys.Count - kept.Count));
    }

    public static bool PointInPolygon(double x, double z, List<(double X, double Z)> poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            if ((poly[i].Z > z) != (poly[j].Z > z) && x < (poly[j].X - poly[i].X) * (z - poly[i].Z) / (poly[j].Z - poly[i].Z) + poly[i].X) inside = !inside;
        return inside;
    }

    static bool Overlap(List<(double X, double Z)> a, List<(double X, double Z)> b)
    {
        if (a.Max(p => p.X) < b.Min(p => p.X) || b.Max(p => p.X) < a.Min(p => p.X) || a.Max(p => p.Z) < b.Min(p => p.Z) || b.Max(p => p.Z) < a.Min(p => p.Z)) return false;
        if (a.Any(p => PointInPolygon(p.X, p.Z, b)) || b.Any(p => PointInPolygon(p.X, p.Z, a))) return true;
        static double Cr((double X, double Z) o, (double X, double Z) p, (double X, double Z) q) => (p.X - o.X) * (q.Z - o.Z) - (p.Z - o.Z) * (q.X - o.X);
        for (int i = 0; i < a.Count; i++)
            for (int j = 0; j < b.Count; j++)
            {
                var p1 = a[i]; var p2 = a[(i + 1) % a.Count]; var q1 = b[j]; var q2 = b[(j + 1) % b.Count];
                if (Cr(q1, q2, p1) * Cr(q1, q2, p2) < 0 && Cr(p1, p2, q1) * Cr(p1, p2, q2) < 0) return true;
            }
        return false;
    }

    /// <summary>
    /// Overlapping polygons of the same prefab are merged (audit finding 10: overlaps z-fight and raise OverlapExisting):
    /// each overlapping group is rasterised at cell metres and traced into hole-free pieces; single polygons stay as they are.
    /// </summary>
    public static (List<SurfacePolygon> Result, int Pairs, int Groups) UnionOverlapping(List<SurfacePolygon> polys, double cell)
    {
        int pairs = 0;
        var parent = Enumerable.Range(0, polys.Count).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }
        for (int i = 0; i < polys.Count; i++)
            for (int j = i + 1; j < polys.Count; j++)
                if (polys[i].Prefab == polys[j].Prefab && Overlap(polys[i].Ring, polys[j].Ring)) { pairs++; parent[Find(i)] = Find(j); }
        var res = new List<SurfacePolygon>(); int groups = 0;
        foreach (var g in Enumerable.Range(0, polys.Count).GroupBy(Find))
        {
            var members = g.Select(i => polys[i]).ToList();
            if (members.Count == 1) { res.Add(members[0]); continue; }
            groups++;
            double minX = members.Min(m => m.Ring.Min(p => p.X)) - cell, minZ = members.Min(m => m.Ring.Min(p => p.Z)) - cell;
            double maxX = members.Max(m => m.Ring.Max(p => p.X)) + cell, maxZ = members.Max(m => m.Ring.Max(p => p.Z)) + cell;
            int size = (int)Math.Ceiling(Math.Max(maxX - minX, maxZ - minZ) / cell) + 1;
            var grid = new GridSpec(size, cell, minX, minZ);
            var cells = new HashSet<int>();
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                {
                    var (x, z) = grid.Centre(r * size + c);
                    if (members.Any(m => PointInPolygon(x, z, m.Ring))) cells.Add(r * size + c);
                }
            int k = 0;
            foreach (var ring in Polygonize(cells, grid, cell / 2))
                res.Add(new SurfacePolygon($"{members[0].Id}u{++k}", members[0].Prefab, ring, $"union of {members.Count} overlapping: {string.Join(", ", members.Select(m => m.Id))}"));
        }
        return (res, pairs, groups);
    }

    /// <summary>Overlapping field circles are shrunk until they just touch (keeps them circles; audit finding 10 asks for no overlap).</summary>
    public static int SeparateCircles(List<(double Cx, double Cz, double R, List<(double X, double Z)> Ring)> circles)
    {
        int changed = 0;
        for (int pass = 0; pass < 5; pass++)
        {
            bool any = false;
            for (int i = 0; i < circles.Count; i++)
                for (int j = i + 1; j < circles.Count; j++)
                {
                    var a = circles[i]; var b = circles[j];
                    double d = Math.Sqrt((a.Cx - b.Cx) * (a.Cx - b.Cx) + (a.Cz - b.Cz) * (a.Cz - b.Cz));
                    double over = a.R + b.R - d;
                    if (over <= 0.5) continue;
                    // Shrink both in proportion to their radii, leaving a 0.5 m gap.
                    double k = (d - 0.5) / (a.R + b.R);
                    circles[i] = a with { R = a.R * k }; circles[j] = b with { R = b.R * k };
                    any = true; changed++;
                }
            if (!any) break;
        }
        for (int i = 0; i < circles.Count; i++)
        {
            var c = circles[i];
            var ring = ClipSquare(Enumerable.Range(0, 48).Select(k => (c.Cx + c.R * Math.Cos(k * Math.PI / 24), c.Cz + c.R * Math.Sin(k * Math.PI / 24))).ToList(), Half);
            circles[i] = c with { Ring = ring.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList() };
        }
        return changed;
    }

    // ------------------------------------------------------------------------------------------------- grass

    /// <summary>DBSCAN (eps, minPts) over 2-D points; returns clusters of point indices (noise dropped).</summary>
    public static List<List<int>> Dbscan(IReadOnlyList<(double X, double Z)> pts, double eps, int minPts)
    {
        var grid = new Dictionary<(long, long), List<int>>();
        for (int i = 0; i < pts.Count; i++)
        {
            var k = ((long)Math.Floor(pts[i].X / eps), (long)Math.Floor(pts[i].Z / eps));
            if (!grid.TryGetValue(k, out var l)) grid[k] = l = new(); l.Add(i);
        }
        List<int> Near(int i)
        {
            var res = new List<int>(); long cx = (long)Math.Floor(pts[i].X / eps), cz = (long)Math.Floor(pts[i].Z / eps);
            for (long a = -1; a <= 1; a++) for (long b = -1; b <= 1; b++)
                    if (grid.TryGetValue((cx + a, cz + b), out var l))
                        foreach (var j in l) if ((pts[i].X - pts[j].X) * (pts[i].X - pts[j].X) + (pts[i].Z - pts[j].Z) * (pts[i].Z - pts[j].Z) <= eps * eps) res.Add(j);
            return res;
        }
        var label = new int[pts.Count]; // 0 = unvisited, -1 = noise, >0 = cluster
        int cluster = 0;
        for (int i = 0; i < pts.Count; i++)
        {
            if (label[i] != 0) continue;
            var nb = Near(i);
            if (nb.Count < minPts) { label[i] = -1; continue; }
            cluster++; label[i] = cluster;
            var queue = new Queue<int>(nb);
            while (queue.Count > 0)
            {
                int j = queue.Dequeue();
                if (label[j] == -1) label[j] = cluster;
                if (label[j] != 0) continue;
                label[j] = cluster;
                var nj = Near(j);
                if (nj.Count >= minPts) foreach (var k in nj) queue.Enqueue(k);
            }
        }
        return Enumerable.Range(1, cluster).Select(c => Enumerable.Range(0, pts.Count).Where(i => label[i] == c).ToList()).ToList();
    }

    /// <summary>Convex hull (Andrew's monotone chain), counter-clockwise.</summary>
    public static List<(double X, double Z)> ConvexHull(IEnumerable<(double X, double Z)> input)
    {
        var p = input.Distinct().OrderBy(q => q.X).ThenBy(q => q.Z).ToList();
        if (p.Count < 3) return p;
        static double Cross((double X, double Z) o, (double X, double Z) a, (double X, double Z) b) => (a.X - o.X) * (b.Z - o.Z) - (a.Z - o.Z) * (b.X - o.X);
        var h = new List<(double, double)>();
        foreach (var q in p) { while (h.Count >= 2 && Cross(h[^2], h[^1], q) <= 0) h.RemoveAt(h.Count - 1); h.Add(q); }
        int lower = h.Count + 1;
        for (int i = p.Count - 2; i >= 0; i--) { var q = p[i]; while (h.Count >= lower && Cross(h[^2], h[^1], q) <= 0) h.RemoveAt(h.Count - 1); h.Add(q); }
        h.RemoveAt(h.Count - 1);
        return h;
    }

    /// <summary>Sutherland–Hodgman clip of a convex polygon to the ±half square.</summary>
    public static List<(double X, double Z)> ClipSquare(List<(double X, double Z)> poly, double half)
    {
        var edges = new (Func<(double X, double Z), bool> In, Func<(double X, double Z), (double X, double Z), (double X, double Z)> Cut)[]
        {
            (p => p.X <= half, (a, b) => (half, a.Z + (b.Z - a.Z) * (half - a.X) / (b.X - a.X))),
            (p => p.X >= -half, (a, b) => (-half, a.Z + (b.Z - a.Z) * (-half - a.X) / (b.X - a.X))),
            (p => p.Z <= half, (a, b) => (a.X + (b.X - a.X) * (half - a.Z) / (b.Z - a.Z), half)),
            (p => p.Z >= -half, (a, b) => (a.X + (b.X - a.X) * (-half - a.Z) / (b.Z - a.Z), -half)),
        };
        var res = poly;
        foreach (var (In, Cut) in edges)
        {
            var input = res; res = new();
            for (int i = 0; i < input.Count; i++)
            {
                var cur = input[i]; var prev = input[(i + input.Count - 1) % input.Count];
                if (In(cur)) { if (!In(prev)) res.Add(Cut(prev, cur)); res.Add(cur); }
                else if (In(prev)) res.Add(Cut(prev, cur));
            }
            if (res.Count == 0) break;
        }
        return res;
    }

    /// <summary>
    /// Landscape Grass tufts: DBSCAN clusters (eps 12 m) of at least minPts tufts become the convex hull of the tufts padded by
    /// pad metres (each tuft as an 8-point circle), clipped to the CS2 square.
    /// </summary>
    public static List<List<(double X, double Z)>> GrassClusters(IReadOnlyList<(double X, double Z)> tufts, double eps = 12, int minPts = 5, double pad = 3)
    {
        var res = new List<List<(double X, double Z)>>();
        foreach (var cl in Dbscan(tufts, eps, minPts))
        {
            if (cl.Count < minPts) continue;
            var pts = cl.SelectMany(i => Enumerable.Range(0, 8).Select(k => (tufts[i].X + pad * Math.Cos(k * Math.PI / 4), tufts[i].Z + pad * Math.Sin(k * Math.PI / 4))));
            var hull = ClipSquare(ConvexHull(pts), Half);
            if (hull.Count >= 3 && Math.Abs(Area(hull)) >= 1) res.Add(hull.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList());
        }
        return res;
    }

    // ------------------------------------------------------------------------------------------ terrain surfaces

    /// <summary>v3 terrainSurfaces → surfaces via surface-mapping.tsv (regex on the legend name → CS2 surface prefab or SKIP).</summary>
    public static List<SurfacePolygon> FromTerrainSurfaces(TerrainSurfaces t, TargetMapping map, out Dictionary<string, string> decisions)
    {
        decisions = new();
        var res = new List<SurfacePolygon>();
        foreach (var cls in t.Cells.Values.Distinct().OrderBy(c => c))
        {
            string name = t.Name(cls);
            var targets = map.Resolve(name);
            if (targets == null || targets[0].IsSkip) { decisions[name] = targets == null ? "UNMAPPED (skipped)" : $"SKIP: {targets[0].Note}"; continue; }
            var cells = t.Cells.Where(kv => kv.Value == cls).Select(kv => kv.Key)
                .Where(i => { var (x, z) = t.Grid.Centre(i); return Math.Abs(x) <= Half && Math.Abs(z) <= Half; }).ToHashSet();
            var rings = Polygonize(cells, t.Grid);
            decisions[name] = $"{targets[0].Prefab} ({rings.Count} polygons)";
            int k = 0;
            foreach (var r in rings) res.Add(new SurfacePolygon($"ts{cls}_{++k}", targets[0].Prefab, r, $"CS1 surface {name}"));
        }
        return res;
    }

    public static void Write(string path, string name, IEnumerable<SurfacePolygon> surfaces, string comment)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs);
        w.WriteStartObject();
        w.WriteString("name", name);
        w.WriteString("_comment", comment);
        w.WriteStartObject("options");
        w.WriteBoolean("dryRun", false); w.WriteNumber("batchSize", 50); w.WriteBoolean("bypassValidation", true); w.WriteString("report", "compact");
        w.WriteEndObject();
        w.WriteStartArray("surfaces");
        foreach (var s in surfaces)
        {
            w.WriteStartObject();
            w.WriteString("id", s.Id); w.WriteString("prefab", s.Prefab);
            w.WriteStartArray("polygon");
            foreach (var (x, z) in s.Ring)
            {
                w.WriteStartArray();
                w.WriteNumberValue(Math.Round(Math.Clamp(x, -Half, Half), 2)); w.WriteNumberValue(Math.Round(Math.Clamp(z, -Half, Half), 2));
                w.WriteEndArray();
            }
            w.WriteEndArray();
            w.WriteString("note", s.Note);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    public static string Stats(string label, List<SurfacePolygon> s) =>
        string.Create(CultureInfo.InvariantCulture, $"{label}: {s.Count} polygons, {s.Sum(p => Math.Abs(Area(p.Ring))) / 1e6:F3} km², max {(s.Count == 0 ? 0 : s.Max(p => p.Ring.Count))} points");
}
