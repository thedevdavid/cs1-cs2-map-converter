using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NetConverter;

/// <summary>A CS1 raster on the map square: size × size cells, row 0 south, values at cell centres.</summary>
/// <param name="Vertex">true: index (row, col) is the sample point origin + col·cellSize (the CS1 heightmap vertex grid);
/// false: the cell centre origin + (col + 0.5)·cellSize.</param>
public sealed record GridSpec(int Size, double CellSize, double OriginX, double OriginZ, bool Vertex = false)
{
    public static GridSpec FromJson(JsonElement? grid, int defSize, double defCell, bool vertex = false)
    {
        int size = defSize; double cell = defCell, ox = -8640, oz = -8640;
        if (grid is { ValueKind: JsonValueKind.Object } g)
        {
            if (g.TryGetProperty("size", out var s)) size = s.GetInt32();
            if (g.TryGetProperty("cellSize", out var c)) cell = c.GetDouble();
            if (g.TryGetProperty("originX", out var x)) ox = x.GetDouble();
            if (g.TryGetProperty("originZ", out var z)) oz = z.GetDouble();
        }
        return new GridSpec(size, cell, ox, oz, vertex);
    }
    double Offset => Vertex ? 0 : 0.5;
    public (double X, double Z) Centre(int index) => (OriginX + (index % Size + Offset) * CellSize, OriginZ + (index / Size + Offset) * CellSize);
    public int Index(double x, double z)
    {
        int col = (int)Math.Floor((x - OriginX) / CellSize + (Vertex ? 0.5 : 0)), row = (int)Math.Floor((z - OriginZ) / CellSize + (Vertex ? 0.5 : 0));
        return col < 0 || row < 0 || col >= Size || row >= Size ? -1 : row * Size + col;
    }
}

// ============================================================================================== water surface

public sealed class WaterSurface
{
    public required GridSpec Grid { get; init; }
    /// <summary>Wet cells: index → (surface m, depth m), CS1 metres.</summary>
    public Dictionary<int, (double Surface, double Depth)> Cells { get; } = new();
    public string Source { get; init; } = "";

    /// <summary>
    /// v3 <c>waterSurface</c> ({grid, cells:[index, surface, depth, ...]}) from the main JSON, else the sibling
    /// <c>&lt;base&gt;-water.png</c> (16-bit, surface × 64, row 0 north; depth = surface − CS1 terrain where that is known).
    /// </summary>
    public static WaterSurface? Load(JsonElement root, string dumpPath, Func<double, double, double>? cs1Terrain)
    {
        if (root.TryGetProperty("waterSurface", out var ws) && ws.ValueKind == JsonValueKind.Object)
        {
            // The heightmap vertex grid: index = row·1081 + col, sample point x = −8640 + col·16 (SCHEMA.md v3).
            var w = new WaterSurface { Grid = GridSpec.FromJson(ws.TryGetProperty("grid", out var g) ? g : null, 1081, 16, vertex: true), Source = "waterSurface in the dump" };
            var c = ws.TryGetProperty("cells", out var cv) && cv.ValueKind == JsonValueKind.Array ? cv.EnumerateArray().Select(v => v.GetDouble()).ToArray() : [];
            for (int i = 0; i + 2 < c.Length; i += 3) w.Cells[(int)c[i]] = (c[i + 1], c[i + 2]);
            return w;
        }
        var png = Path.Combine(Path.GetDirectoryName(dumpPath)!, Path.GetFileNameWithoutExtension(dumpPath) + "-water.png");
        if (!File.Exists(png) || cs1Terrain == null) return null;
        var (wd, h, v) = Heightmap.ReadGray16(File.ReadAllBytes(png));
        var grid = new GridSpec(wd, 17280.0 / (wd - 1), -8640, -8640, Vertex: true);   // 1081 vertices, 16 m apart
        var res = new WaterSurface { Grid = grid, Source = Path.GetFileName(png) };
        for (int row = 0; row < h; row++)
            for (int col = 0; col < wd; col++)
            {
                double surface = v[row * wd + col] / 64.0;
                if (surface <= 0) continue;
                int index = (h - 1 - row) * wd + col;                     // PNG row 0 = north
                var (x, z) = grid.Centre(index);
                if (Math.Abs(x) > 7168 || Math.Abs(z) > 7168) continue;   // terrain only known inside the CS2 square
                double depth = surface - cs1Terrain(x, z);
                if (depth > 0.05) res.Cells[index] = (surface, depth);
            }
        return res;
    }

    /// <summary>4-connected wet bodies, largest first.</summary>
    /// <summary>Wet cells above sea level + 1 m with no CS1 source within 400 m, and their connected bodies (largest first).</summary>
    public (HashSet<int> Cells, List<List<int>> Bodies) Orphans(Dump dump, double seaLevelCs1)
    {
        var sources = dump.WaterSources.Select(s => (s.Output.X, s.Output.Z)).ToList();
        var orphan = Cells.Where(kv => kv.Value.Surface > seaLevelCs1 + 1).Select(kv => kv.Key)
            .Where(i => { var (x, z) = Grid.Centre(i); return !sources.Any(s => (s.X - x) * (s.X - x) + (s.Z - z) * (s.Z - z) <= 400 * 400); })
            .ToHashSet();
        var orphanSurface = new WaterSurface { Grid = Grid };
        foreach (var i in orphan) orphanSurface.Cells[i] = Cells[i];
        return (orphan, orphanSurface.Bodies());
    }

    public List<List<int>> Bodies()
    {
        var seen = new HashSet<int>();
        var bodies = new List<List<int>>();
        int n = Grid.Size;
        foreach (var start in Cells.Keys)
        {
            if (!seen.Add(start)) continue;
            var body = new List<int>();
            var stack = new Stack<int>(); stack.Push(start);
            while (stack.Count > 0)
            {
                int i = stack.Pop(); body.Add(i);
                int r = i / n, c = i % n;
                foreach (var j in new[] { c > 0 ? i - 1 : -1, c < n - 1 ? i + 1 : -1, r > 0 ? i - n : -1, r < n - 1 ? i + n : -1 })
                    if (j >= 0 && Cells.ContainsKey(j) && seen.Add(j)) stack.Push(j);
            }
            bodies.Add(body);
        }
        return bodies.OrderByDescending(b => b.Count).ToList();
    }

    public List<double> SurfacesWithin(double x, double z, double radius)
    {
        var list = new List<double>();
        int r = (int)Math.Ceiling(radius / Grid.CellSize) + 1, centre = Grid.Index(x, z);
        if (centre < 0) return list;
        int cr = centre / Grid.Size, cc = centre % Grid.Size;
        for (int row = Math.Max(0, cr - r); row <= Math.Min(Grid.Size - 1, cr + r); row++)
            for (int col = Math.Max(0, cc - r); col <= Math.Min(Grid.Size - 1, cc + r); col++)
            {
                int i = row * Grid.Size + col;
                if (!Cells.TryGetValue(i, out var cell)) continue;
                var (px, pz) = Grid.Centre(i);
                if ((px - x) * (px - x) + (pz - z) * (pz - z) <= radius * radius) list.Add(cell.Surface);
            }
        return list;
    }

    public static double Median(List<double> v) { if (v.Count == 0) return double.NaN; v.Sort(); return v.Count % 2 == 1 ? v[v.Count / 2] : (v[v.Count / 2 - 1] + v[v.Count / 2]) / 2; }

    /// <summary>
    /// water-check.md, and the per-source height overrides (CS1 metres) where the source target and the surface around it
    /// (median of wet cells within 100 m) disagree by more than 1 m.
    /// </summary>
    public (string Markdown, Dictionary<int, double> Overrides) Check(Dump dump, double seaLevelCs1, double factor, double half)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        var overrides = new Dictionary<int, double>();
        sb.AppendLine("# Water check (CS1 water surface vs import-water.json)").AppendLine();
        sb.AppendLine($"Source: {Source}; {Cells.Count:N0} wet cells (depth > 0.05 m) of {Grid.CellSize:F3} m. Heights in CS1 metres; the import file uses × {factor}.").AppendLine();

        sb.AppendLine("## Sources").AppendLine();
        sb.AppendLine("| id | x | z | target | surface at source | ring 100 m median (cells) | Δ target − ring | action |").AppendLine("|---:|---:|---:|---:|---:|---:|---:|---|");
        double TargetCs1(WaterSource s) => s.TargetHeight / dump.HeightFactor;
        foreach (var s in dump.WaterSources)
        {
            double x = s.Output.X, z = s.Output.Z;
            int idx = Grid.Index(x, z);
            string at = idx >= 0 && Cells.TryGetValue(idx, out var here) ? here.Surface.ToString("F2", ci) : "dry";
            var ring = SurfacesWithin(x, z, 100);
            double med = Median(ring), target = TargetCs1(s), diff = target - med;
            string action = "keep";
            if (!double.IsNaN(med) && Math.Abs(diff) > 1)
            {
                overrides[s.Id] = Math.Round(med, 2);
                action = $"height → {med:F2} (×{factor} = {med * factor:F2})";
            }
            else if (double.IsNaN(med)) action = "keep (no wet cell within 100 m)";
            sb.AppendLine(string.Create(ci, $"| {s.Id} | {x:F0} | {z:F0} | {target:F2} | {at} | {(double.IsNaN(med) ? "-" : med.ToString("F2", ci))} ({ring.Count}) | {(double.IsNaN(diff) ? "-" : diff.ToString("+0.00;-0.00", ci))} | {action} |"));
        }
        sb.AppendLine().AppendLine($"Sources changed: {overrides.Count}.").AppendLine();

        var bodies = Bodies();
        sb.AppendLine("## Wet bodies").AppendLine();
        if (bodies.Count > 0)
        {
            var main = bodies[0].Select(i => Cells[i].Surface).ToList();
            double med = Median(main);
            sb.AppendLine(string.Create(ci, $"Largest body (the lower river): {bodies[0].Count:N0} cells ({bodies[0].Count * Grid.CellSize * Grid.CellSize / 1e6:F2} km²), median surface {med:F2} m (× {factor} = {med * factor:F2}) vs chosen sea level {seaLevelCs1:F2} m (× {factor} = {seaLevelCs1 * factor:F2}): Δ {med - seaLevelCs1:+0.00;-0.00} m."));
            if (med < seaLevelCs1 - 0.5)
                sb.AppendLine().AppendLine(string.Create(ci, $"**Sea level is {seaLevelCs1 - med:F2} m above the measured lower river ({(seaLevelCs1 - med) * factor:F2} m at × {factor}).** CS2 floods everything below the sea level, so the river banks between {med:F1} and {seaLevelCs1:F1} m would go under. Recommendation: lower the sea level to about {Math.Ceiling(med * 2) / 2:F1} m ({Math.Ceiling(med * 2) / 2 * factor:F1} m at × {factor}), i.e. pass --sea-level {Math.Ceiling(med * 2) / 2:F1}."));
            sb.AppendLine().AppendLine("| body | cells | km² | median surface | min | max | centre x | centre z |").AppendLine("|---:|---:|---:|---:|---:|---:|---:|---:|");
            int k = 0;
            foreach (var b in bodies.Take(15))
            {
                var surf = b.Select(i => Cells[i].Surface).ToList();
                var cx = b.Average(i => Grid.Centre(i).X); var cz = b.Average(i => Grid.Centre(i).Z);
                sb.AppendLine(string.Create(ci, $"| {++k} | {b.Count:N0} | {b.Count * Grid.CellSize * Grid.CellSize / 1e6:F3} | {Median(surf):F2} | {surf.Min():F2} | {surf.Max():F2} | {cx:F0} | {cz:F0} |"));
            }
            sb.AppendLine();
        }

        // Wet cells above sea level with no source within 400 m: lakes or upper reaches the sources would not fill.
        var (orphan, groups) = Orphans(dump, seaLevelCs1);
        sb.AppendLine("## Wet cells above sea level that no source within 400 m explains").AppendLine();
        sb.AppendLine($"{orphan.Count:N0} cells in {groups.Count} patches (largest 25 listed). Each may need its own source.").AppendLine();
        sb.AppendLine("| patch | cells | km² | median surface (×factor) | centre x | centre z | inside ±7168 |").AppendLine("|---:|---:|---:|---:|---:|---:|---|");
        int p = 0;
        foreach (var g in groups.Take(25))
        {
            var surf = g.Select(i => Cells[i].Surface).ToList();
            var cx = g.Average(i => Grid.Centre(i).X); var cz = g.Average(i => Grid.Centre(i).Z);
            sb.AppendLine(string.Create(ci, $"| {++p} | {g.Count:N0} | {g.Count * Grid.CellSize * Grid.CellSize / 1e6:F3} | {Median(surf):F2} ({Median(surf) * factor:F2}) | {cx:F0} | {cz:F0} | {(Math.Abs(cx) <= half && Math.Abs(cz) <= half ? "yes" : "no")} |"));
        }
        return (sb.ToString(), overrides);
    }
}

// ========================================================================================= terrain surfaces

public sealed class TerrainSurfaces
{
    public required GridSpec Grid { get; init; }
    public Dictionary<int, string> Legend { get; } = new();
    /// <summary>index → class id; class 0 / absent = none.</summary>
    public Dictionary<int, int> Cells { get; } = new();

    /// <summary>
    /// v3 <c>terrainSurfaces</c> (may be null): {grid, legend ({id: name} or [{id, name}]), cells: dense size² array or sparse [index, class, ...]}.
    /// </summary>
    public static TerrainSurfaces? Load(JsonElement root)
    {
        if (!root.TryGetProperty("terrainSurfaces", out var ts) || ts.ValueKind != JsonValueKind.Object) return null;
        var t = new TerrainSurfaces { Grid = GridSpec.FromJson(ts.TryGetProperty("grid", out var g) ? g : null, 1081, 15.985199) };
        if (ts.TryGetProperty("legend", out var leg))
        {
            if (leg.ValueKind == JsonValueKind.Object)
                foreach (var p in leg.EnumerateObject()) t.Legend[int.Parse(p.Name, CultureInfo.InvariantCulture)] = p.Value.GetString() ?? p.Name;
            else if (leg.ValueKind == JsonValueKind.Array)
                foreach (var e in leg.EnumerateArray()) t.Legend[e.GetProperty("id").GetInt32()] = e.GetProperty("name").GetString() ?? "";
        }
        var cells = ts.TryGetProperty("cells", out var cv) && cv.ValueKind == JsonValueKind.Array ? cv.EnumerateArray().Select(v => v.GetInt32()).ToArray() : [];
        string encoding = ts.TryGetProperty("encoding", out var enc) && enc.ValueKind == JsonValueKind.String ? enc.GetString()!.ToLowerInvariant() : "";
        // v3 writes "cells: sparse flat [index, value, ...] for value != 0"; dense only when said so (or no encoding and size² values).
        bool dense = encoding.Contains("dense") || (!encoding.Contains("sparse") && cells.Length == t.Grid.Size * t.Grid.Size);
        if (dense) { for (int i = 0; i < cells.Length; i++) if (cells[i] != 0) t.Cells[i] = cells[i]; }
        else for (int i = 0; i + 1 < cells.Length; i += 2) if (cells[i + 1] != 0) t.Cells[cells[i]] = cells[i + 1];
        return t;
    }

    public string Name(int cls) => Legend.TryGetValue(cls, out var n) ? n : $"class {cls}";

    public string Summary(double half)
    {
        var ci = CultureInfo.InvariantCulture;
        double a = Grid.CellSize * Grid.CellSize;
        var sb = new StringBuilder();
        sb.AppendLine("# Terrain surfaces (CS1)").AppendLine();
        sb.AppendLine($"Grid {Grid.Size}² × {Grid.CellSize:F3} m, origin ({Grid.OriginX}, {Grid.OriginZ}), row 0 south. Polygons: surfaces.geojson (CS2 metres, simplified to 4 m).").AppendLine();
        sb.AppendLine("## Legend and area").AppendLine().AppendLine("| class | name | cells | km² | km² inside ±7168 |").AppendLine("|---:|---|---:|---:|---:|");
        foreach (var g in Cells.GroupBy(kv => kv.Value).OrderBy(g => g.Key))
        {
            int inside = g.Count(kv => { var (x, z) = Grid.Centre(kv.Key); return Math.Abs(x) <= half && Math.Abs(z) <= half; });
            sb.AppendLine(string.Create(ci, $"| {g.Key} | {Name(g.Key)} | {g.Count():N0} | {g.Count() * a / 1e6:F3} | {inside * a / 1e6:F3} |"));
        }
        foreach (var (id, name) in Legend.Where(l => !Cells.ContainsValue(l.Key)).OrderBy(l => l.Key))
            sb.AppendLine($"| {id} | {name} | 0 | 0 | 0 |");
        sb.AppendLine().AppendLine("## Largest patches").AppendLine().AppendLine("| class | cells | km² | min x | min z | max x | max z |").AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var (cls, patch) in Patches().OrderByDescending(p => p.Cells.Count).Take(20))
        {
            var pts = patch.Select(Grid.Centre).ToList();
            double h = Grid.CellSize / 2;
            sb.AppendLine(string.Create(ci, $"| {Name(cls)} | {patch.Count:N0} | {patch.Count * a / 1e6:F3} | {pts.Min(p => p.X) - h:F0} | {pts.Min(p => p.Z) - h:F0} | {pts.Max(p => p.X) + h:F0} | {pts.Max(p => p.Z) + h:F0} |"));
        }
        return sb.ToString();
    }

    public List<(int Class, List<int> Cells)> Patches()
    {
        var seen = new HashSet<int>(); var res = new List<(int, List<int>)>(); int n = Grid.Size;
        foreach (var (start, cls) in Cells)
        {
            if (!seen.Add(start)) continue;
            var patch = new List<int>(); var stack = new Stack<int>(); stack.Push(start);
            while (stack.Count > 0)
            {
                int i = stack.Pop(); patch.Add(i);
                int r = i / n, c = i % n;
                foreach (var j in new[] { c > 0 ? i - 1 : -1, c < n - 1 ? i + 1 : -1, r > 0 ? i - n : -1, r < n - 1 ? i + n : -1 })
                    if (j >= 0 && Cells.TryGetValue(j, out var cj) && cj == cls && seen.Add(j)) stack.Push(j);
            }
            res.Add((cls, patch));
        }
        return res;
    }

    /// <summary>
    /// Boundary rings per class: every cell edge with a different class on the other side becomes a directed edge (interior on
    /// the left); edges chain into closed rings; rings are simplified with Douglas–Peucker. Counter-clockwise rings are outer
    /// boundaries, clockwise ones holes.
    /// </summary>
    public List<(int Class, List<(double X, double Z)> Ring, bool Outer)> Rings(double tolerance)
    {
        int n = Grid.Size;
        var result = new List<(int, List<(double, double)>, bool)>();
        foreach (var cls in Cells.Values.Distinct())
        {
            // Directed edges on the lattice of cell corners (col, row), interior on the left (counter-clockwise outer rings).
            var next = new Dictionary<(int, int), List<(int, int)>>();
            void Add((int, int) a, (int, int) b) { if (!next.TryGetValue(a, out var l)) next[a] = l = new(); l.Add(b); }
            bool Is(int col, int row) => col >= 0 && row >= 0 && col < n && row < n && Cells.TryGetValue(row * n + col, out var c) && c == cls;
            foreach (var (i, c) in Cells)
            {
                if (c != cls) continue;
                int row = i / n, col = i % n;
                if (!Is(col, row - 1)) Add((col, row), (col + 1, row));           // south edge, going +x
                if (!Is(col + 1, row)) Add((col + 1, row), (col + 1, row + 1));   // east edge, going +z
                if (!Is(col, row + 1)) Add((col + 1, row + 1), (col, row + 1));   // north edge, going −x
                if (!Is(col - 1, row)) Add((col, row + 1), (col, row));           // west edge, going −z
            }
            while (next.Count > 0)
            {
                var start = next.Keys.First();
                var ring = new List<(int, int)> { start };
                var cur = start;
                while (true)
                {
                    var l = next[cur];
                    var to = l[^1]; l.RemoveAt(l.Count - 1);
                    if (l.Count == 0) next.Remove(cur);
                    if (to == start) break;
                    ring.Add(to); cur = to;
                    if (!next.ContainsKey(cur)) break;
                }
                var pts = ring.Select(p => (Grid.OriginX + p.Item1 * Grid.CellSize, Grid.OriginZ + p.Item2 * Grid.CellSize)).ToList();
                double area = 0;
                for (int k = 0; k < pts.Count; k++) { var a = pts[k]; var b = pts[(k + 1) % pts.Count]; area += a.Item1 * b.Item2 - b.Item1 * a.Item2; }
                var simple = Simplify(pts, tolerance);
                if (simple.Count >= 3) result.Add((cls, simple, area > 0));
            }
        }
        return result;
    }

    static List<(double X, double Z)> Simplify(List<(double X, double Z)> ring, double tol)
    {
        if (ring.Count < 4) return ring;
        // Split the closed ring at its two farthest-apart points and simplify both halves.
        int far = 0; double best = -1;
        for (int i = 1; i < ring.Count; i++) { double d = Sq(ring[i], ring[0]); if (d > best) { best = d; far = i; } }
        var a = DP(ring.GetRange(0, far + 1), tol);
        var b = DP(ring.GetRange(far, ring.Count - far).Append(ring[0]).ToList(), tol);
        return a.Take(a.Count - 1).Concat(b.Take(b.Count - 1)).ToList();
    }

    static double Sq((double X, double Z) a, (double X, double Z) b) => (a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z);

    static List<(double X, double Z)> DP(List<(double X, double Z)> pts, double tol)
    {
        if (pts.Count < 3) return pts;
        var a = pts[0]; var b = pts[^1];
        double dx = b.X - a.X, dz = b.Z - a.Z, len = Math.Sqrt(dx * dx + dz * dz);
        int idx = 0; double max = 0;
        for (int i = 1; i < pts.Count - 1; i++)
        {
            double d = len > 0 ? Math.Abs(dz * (pts[i].X - a.X) - dx * (pts[i].Z - a.Z)) / len : Math.Sqrt(Sq(pts[i], a));
            if (d > max) { max = d; idx = i; }
        }
        if (max <= tol) return [a, b];
        var left = DP(pts.GetRange(0, idx + 1), tol);
        var right = DP(pts.GetRange(idx, pts.Count - idx), tol);
        return left.Take(left.Count - 1).Concat(right).ToList();
    }

    public void WriteGeoJson(string path, double tolerance)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs);
        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        w.WriteString("name", "cs1-terrain-surfaces");
        w.WriteStartArray("features");
        foreach (var (cls, ring, outer) in Rings(tolerance))
        {
            w.WriteStartObject();
            w.WriteString("type", "Feature");
            w.WriteStartObject("geometry"); w.WriteString("type", "Polygon");
            w.WriteStartArray("coordinates"); w.WriteStartArray();
            foreach (var (x, z) in ring.Append(ring[0])) { w.WriteStartArray(); w.WriteNumberValue(Json.R2(x)); w.WriteNumberValue(Json.R2(z)); w.WriteEndArray(); }
            w.WriteEndArray(); w.WriteEndArray();
            w.WriteEndObject();
            w.WriteStartObject("properties");
            w.WriteNumber("class", cls); w.WriteString("name", Name(cls)); w.WriteString("ring", outer ? "outer" : "hole");
            w.WriteEndObject();
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }
}

// ======================================================================================= procedural objects

public sealed record ProceduralObject(int Id, string BasePrefab, double X, double Y, double Z, double Yaw, double[] Scale)
{
    /// <summary>Effects Module effects (v4): prefab names, e.g. "Large Pool Steam".</summary>
    public List<string> Effects { get; init; } = new();
    /// <summary>Class and subclass from the offline decode (cs1-offline/out/po-offline.json), joined by id.</summary>
    public string Class { get; set; } = "unclassified";
    public string SubClass { get; set; } = "";
}

public static class ProceduralObjects
{
    /// <summary>
    /// <c>&lt;base&gt;-po.json</c> (v3). Read tolerantly until the exporter fixes the schema: an array under objects / po /
    /// proceduralObjects (or the root array); base prefab from basePrefab / prefab / baseInfo; position [x,y,z] or x,y,z;
    /// yaw in degrees from yaw, else from a Unity quaternion rotation [x,y,z,w], else from angle (radians, CS1 convention);
    /// scale as a number or [x,y,z].
    /// </summary>
    public static List<ProceduralObject>? Load(string dumpPath, JsonElement? main = null)
    {
        // The main JSON names the file (proceduralObjects.file); null there means PO was not active.
        string? named = main is { } m && m.TryGetProperty("proceduralObjects", out var po) && po.ValueKind == JsonValueKind.Object
            && po.TryGetProperty("file", out var f) && f.ValueKind == JsonValueKind.String ? f.GetString() : null;
        var path = named != null
            ? (Path.IsPathRooted(named) ? named : Path.Combine(Path.GetDirectoryName(dumpPath)!, Path.GetFileName(named)))
            : Path.Combine(Path.GetDirectoryName(dumpPath)!, Path.GetFileNameWithoutExtension(dumpPath) + "-po.json");
        if (!File.Exists(path)) return null;
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        JsonElement arr = root.ValueKind == JsonValueKind.Array ? root
            : new[] { "objects", "po", "proceduralObjects" }.Select(k => root.TryGetProperty(k, out var a) ? a : default).First(a => a.ValueKind == JsonValueKind.Array);
        var list = new List<ProceduralObject>();
        int n = 0;
        foreach (var o in arr.EnumerateArray())
        {
            n++;
            string prefab = new[] { "basePrefab", "prefab", "baseInfo" }.Select(k => o.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null).FirstOrDefault(s => s != null) ?? "?";
            double x, y, z;
            if (o.TryGetProperty("position", out var p)) { var a = p.EnumerateArray().Select(v => v.GetDouble()).ToArray(); (x, y, z) = (a[0], a[1], a[2]); }
            else (x, y, z) = (o.GetProperty("x").GetDouble(), o.TryGetProperty("y", out var yy) ? yy.GetDouble() : 0, o.GetProperty("z").GetDouble());
            double yaw = 0;
            if (o.TryGetProperty("yawDegrees", out var yd) && yd.ValueKind == JsonValueKind.Number) yaw = yd.GetDouble();
            else if (o.TryGetProperty("yaw", out var yw) && yw.ValueKind == JsonValueKind.Number) yaw = yw.GetDouble();
            else if (o.TryGetProperty("rotation", out var q) && q.ValueKind == JsonValueKind.Array && q.GetArrayLength() == 4)
            {
                var r = q.EnumerateArray().Select(v => v.GetDouble()).ToArray(); // x, y, z, w
                yaw = Math.Atan2(2 * (r[3] * r[1] + r[0] * r[2]), 1 - 2 * (r[1] * r[1] + r[0] * r[0])) * 180 / Math.PI;
            }
            else if (o.TryGetProperty("angle", out var ang)) yaw = Objects.Yaw(ang.GetDouble());
            yaw = ((yaw % 360) + 360) % 360;
            double[] scale = [1, 1, 1];
            if (o.TryGetProperty("scale", out var s))
            {
                // v3: a single m_scale, often 0 or 1 (PO bakes scaling into the vertices); 0 means unscaled.
                if (s.ValueKind == JsonValueKind.Number) { double v = s.GetDouble() == 0 ? 1 : s.GetDouble(); scale = [v, v, v]; }
                else if (s.ValueKind == JsonValueKind.Array) scale = s.EnumerateArray().Select(v => v.GetDouble()).ToArray();
            }
            int id = o.TryGetProperty("id", out var idv) && idv.ValueKind == JsonValueKind.Number ? idv.GetInt32() : n;
            var effects = o.TryGetProperty("effects", out var ef) && ef.ValueKind == JsonValueKind.Array
                ? ef.EnumerateArray().Select(e => e.TryGetProperty("prefab", out var ep) && ep.ValueKind == JsonValueKind.String ? ep.GetString()! : "?").ToList() : new List<string>();
            list.Add(new ProceduralObject(id, prefab, x, y, z, Math.Round(yaw, 2), scale) { Effects = effects });
        }
        return list;
    }

    /// <summary>Joins the offline classes (po-classes.md rules) by object id; returns how many matched.</summary>
    public static int Classify(List<ProceduralObject> pos, string offlinePath)
    {
        if (!File.Exists(offlinePath)) return 0;
        using var doc = JsonDocument.Parse(File.ReadAllText(offlinePath));
        var byId = doc.RootElement.GetProperty("objects").EnumerateArray()
            .ToDictionary(o => o.GetProperty("id").GetInt32(), o => (o.GetProperty("class").GetString() ?? "?", o.TryGetProperty("subClass", out var sc) ? sc.GetString() ?? "" : ""));
        int n = 0;
        foreach (var p in pos)
            if (byId.TryGetValue(p.Id, out var c)) { p.Class = c.Item1; p.SubClass = c.Item2; n++; }
        return n;
    }

    /// <summary>Effects Module emitters (the dam steam / falls foam), one line per object.</summary>
    public static string FallsFoam(List<ProceduralObject> pos, double factor)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        var fx = pos.Where(p => p.Effects.Count > 0).ToList();
        sb.AppendLine().AppendLine("## Falls foam (PO Effects Module)").AppendLine();
        sb.AppendLine($"{fx.Count} objects carry {fx.Sum(p => p.Effects.Count)} effects. CS2 has no PO effects: add water/steam effects by hand or leave them out. Position in CS2 metres (y × {factor}).").AppendLine();
        sb.AppendLine("| id | base prefab | x | y | z | effects |").AppendLine("|---:|---|---:|---:|---:|---|");
        foreach (var p in fx) sb.AppendLine(string.Create(ci, $"| {p.Id} | {Json.Short(p.BasePrefab)} | {p.X:F1} | {p.Y * factor:F1} | {p.Z:F1} | {string.Join(", ", p.Effects.GroupBy(e => e).Select(g => g.Count() > 1 ? $"{g.Key} ×{g.Count()}" : g.Key))} |"));
        return sb.ToString();
    }

    public static string Manual(List<ProceduralObject> pos, double factor)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine().AppendLine("## Procedural Objects (CS1, from <base>-po.json)").AppendLine();
        sb.AppendLine($"{pos.Count:N0} objects. Position in CS2 metres (y × {factor}); yaw degrees, 0 = +z, clockwise from above (unverified); scale x/y/z.").AppendLine();
        sb.AppendLine("| class | subclass | objects |").AppendLine("|---|---|---:|");
        foreach (var c in pos.GroupBy(p => (p.Class, p.SubClass)).OrderBy(c => c.Key.Class).ThenBy(c => c.Key.SubClass)) sb.AppendLine($"| {c.Key.Class} | {c.Key.SubClass} | {c.Count()} |");
        sb.AppendLine();
        foreach (var g in pos.GroupBy(p => (p.Class, p.SubClass, p.BasePrefab)).OrderBy(g => g.Key.Class).ThenBy(g => g.Key.SubClass).ThenByDescending(g => g.Count()))
        {
            sb.AppendLine($"### {g.Key.Class} / {g.Key.SubClass}: {Json.Short(g.Key.BasePrefab)} ({g.Count()})").AppendLine();
            sb.AppendLine("| id | x | y | z | yaw | scale |").AppendLine("|---:|---:|---:|---:|---:|---|");
            foreach (var p in g) sb.AppendLine(string.Create(ci, $"| {p.Id} | {p.X:F1} | {p.Y * factor:F1} | {p.Z:F1} | {p.Yaw:F1} | {string.Join(" × ", p.Scale.Select(v => v.ToString("0.##", ci)))} |"));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>The subset whose base prefab maps through object-mapping.tsv to a real CS2 prefab, as ordinary objects (with y; PO float).</summary>
    /// <param name="structuresOnly">With the offline classes joined: only class "structure" (terrain-like objects are stamped into the terrain, decals are surfaces).</param>
    public static List<PlacedObject> Candidates(List<ProceduralObject> pos, TargetMapping map, double factor, double half, ulong seed, bool structuresOnly = false)
    {
        var rng = new Rng(seed);
        var res = new List<PlacedObject>();
        foreach (var p in pos)
        {
            if (structuresOnly && p.Class != "structure") continue;
            if (Math.Abs(p.X) > half || Math.Abs(p.Z) > half) continue;
            var targets = map.Resolve(p.BasePrefab);
            if (targets == null) continue;
            var t = TargetMapping.Pick(targets, ref rng);
            if (t.IsSkip || t.IsManual) continue;
            res.Add(new PlacedObject($"po{p.Id}", t.Prefab, string.IsNullOrEmpty(t.Extra) ? "prop" : t.Extra, p.X, p.Z, Json.R2(p.Y * factor), p.Yaw, p.BasePrefab, $"po {p.Id}"));
        }
        return res;
    }
}
