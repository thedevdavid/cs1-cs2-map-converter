using System.Text.Json;

namespace NetConverter;

/// <summary>
/// Round 23/24 (owner): the CS2 editor's Terrain Inspector → MATERIALS brushes (terrain material painting, not area surfaces), one file per material,
/// POST /import/terrain-paint (bridge 0.9.29, IMPORT_SCHEMA.md "Terrain material painting"). x/z only, so both scales are the same.
/// Materials: Sand (Extra4), Dry (Extra1), Dirt (Extra3); no rock material (the shader puts rock on steep slopes), so the rock mask goes to optional/ unused.
/// - sand: every CS1 Sand cell (the 479-polygon source set), 16 m grid, closed/opened two cells, SmoothRings outlines, ≥ 1 500 m².
/// - rock: the slope ≥ 40° polygons (true-height heightmap) plus the PO rock / cliff footprints, as they are.
/// - dry grass: open dry ground (CS1 fertility &lt; 0.5, no tree within 48 m, not sand, field, water or road) on (a) the east / south-east plains
///   (bearing from the map centre 45°–180°, at most 60 m above the start-area mean, slope &lt; 12°) and (b) south-facing slopes anywhere
///   (8°–35°, downhill within 60° of south). CS1 painted no grass class; the CS1 theme tinted fertile land yellower and forest floor darker
///   (audit/theme-remake.md §1), so dry = low fertility, open; south-facing as the sunny, drier side.
/// - bare earth: the 128 CS1 R69 dirt / mud decals, one 11 m stroke at each.
/// Sand and dry grass: polygons, softEdge 6 m, strength 1.
/// </summary>
public static class TerrainPaint
{
    public sealed record Layer(string File, string Material, string Role, List<(string Id, List<(double X, double Z)> Ring, string Note)> Polygons,
        List<(string Id, double X, double Z, double Radius)>? Strokes = null, double SoftEdge = 0, bool Optional = false);

    /// <summary>Editor material names (bridge 0.9.29). "Rock" has no material; that layer is written to optional/ and not imported.</summary>
    public static readonly Dictionary<string, string> Materials = new()
    {
        ["sand"] = "Sand", ["rock"] = "(none: rock is automatic on steep slopes)", ["dry-grass"] = "Dry", ["bare-earth"] = "Dirt",
    };
    public const double SoftEdgeM = 6, DecalRadius = 11, MaxStamps = 250000;

    public static List<Layer> Build(PaintPlan.Inputs inp, IEnumerable<List<(double X, double Z)>> rock40, IEnumerable<List<(double X, double Z)>> poRock,
        IEnumerable<(double X, double Z)> decalCentres, IEnumerable<OutEdge> roads, double half = 7168)
    {
        var g = Surfaces.FineGrid(16); int n = g.Size; double cs = g.CellSize;
        var block = new bool[n * n];
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
        Mark(block, inp.Fields);
        Mark(block, inp.PavementBlocks);
        // Roads: the cells their courses pass through (a painted road verge is fine, a painted carriageway is not).
        foreach (var e in roads.Where(e => e.Map.Imported))
            for (int s = 0; s <= 32; s++)
            {
                var p = e.Curve.Eval(s / 32.0); int i = (int)((p.X - g.OriginX) / cs), j = (int)((p.Z - g.OriginZ) / cs);
                if (i >= 0 && j >= 0 && i < n && j < n) block[j * n + i] = true;
            }
        int sandFlag = inp.Sand?.Legend.FirstOrDefault(kv => kv.Value.Contains("Sand", StringComparison.OrdinalIgnoreCase)).Key ?? 0;
        bool IsSand(double x, double z) => inp.Sand != null && inp.Sand.Cells.TryGetValue(inp.Sand.Grid.Index(x, z), out var v) && (sandFlag != 0 ? (v & sandFlag) != 0 : inp.Sand.Name(v).Contains("Sand"));
        bool Wet(double x, double z) => inp.Water != null && inp.Water.Cells.TryGetValue(inp.Water.Grid.Index(x, z), out var c) && c.Depth > 0.05;
        double T(double x, double z) => inp.TerrainTrue?.Invoke(x, z) ?? 0;
        double Fert(double x, double z) => inp.Fertility != null && inp.FertilityGrid != null && inp.FertilityGrid.Index(x, z) is var fi && fi >= 0 && fi < inp.Fertility.Length ? inp.Fertility[fi] / 255.0 : 0;
        var treeCount = new int[n * n];
        foreach (var (x, z) in inp.Trees) { int i = (int)((x - g.OriginX) / cs), j = (int)((z - g.OriginZ) / cs); if (i >= 0 && j >= 0 && i < n && j < n) treeCount[j * n + i]++; }
        int TreesNear(int k, int r) { int i = k % n, j = k / n, s = 0; for (int dj = -r; dj <= r; dj++) for (int di = -r; di <= r; di++) { int ii = i + di, jj = j + dj; if (ii >= 0 && jj >= 0 && ii < n && jj < n) s += treeCount[jj * n + ii]; } return s; }
        double sum = 0; int cnt = 0;
        for (double x = -1557; x <= 1557; x += 100) for (double z = -1557; z <= 1557; z += 100) { sum += T(x, z); cnt++; }
        double startMean = cnt > 0 ? sum / cnt : 0;

        var sand = new bool[n * n]; var dry = new bool[n * n];
        int plains = 0, southSlopes = 0;
        for (int k = 0; k < n * n; k++)
        {
            var (x, z) = g.Centre(k);
            if (IsSand(x, z)) { sand[k] = true; continue; }
            if (block[k] || Wet(x, z) || TreesNear(k, 1) > 0 || Fert(x, z) >= 0.5) continue;
            double gx = (T(x + 8, z) - T(x - 8, z)) / 16, gz = (T(x, z + 8) - T(x, z - 8)) / 16;
            double slope = Math.Atan(Math.Sqrt(gx * gx + gz * gz)) * 180 / Math.PI;
            double bearing = (Math.Atan2(x, z) * 180 / Math.PI + 360) % 360;              // 0 = north (+z), 90 = east
            if (bearing >= 45 && bearing <= 180 && T(x, z) <= startMean + 60 && slope < 12) { dry[k] = true; plains++; continue; }
            // South-facing: the downhill direction (−gradient) within 60° of south (−z), i.e. the terrain rises to the north: gz / |g| ≥ cos 60°.
            double gl = Math.Sqrt(gx * gx + gz * gz);
            if (slope >= 8 && slope <= 35 && gl > 0 && gz / gl >= 0.5) { dry[k] = true; southSlopes++; }
        }
        List<List<(double X, double Z)>> Smooth(bool[] m, string name)
        {
            m = PaintPlan.CloseOpen(m, n);
            var set = new HashSet<int>(); for (int k = 0; k < n * n; k++) if (m[k]) set.Add(k);
            var (polys, _, _) = Surfaces.MaskPolygons(set, g, SmoothRings.MinArea, cs / 2, 3000);
            return SmoothRings.Smooth(polys, name.Aggregate(1469598103934665603UL, (h, ch) => (h ^ ch) * 1099511628211UL), half);
        }
        var layers = new List<Layer>();
        layers.Add(new("import-terrain-paint-sand.json", Materials["sand"], "CS1 painted sand (all Sand cells)",
            Smooth(sand, "tp sand").Select((r, i) => ($"tp_sand_{i + 1}", r, "CS1 sand")).ToList(), SoftEdge: SoftEdgeM));
        layers.Add(new("import-terrain-paint-rock.json", Materials["rock"], "slope ≥ 40° and the PO rock / cliff footprints",
            rock40.Where(r => r.Count >= 3).Select((r, i) => ($"tp_rock40_{i + 1}", r, "slope ≥ 40°"))
                .Concat(poRock.Where(r => r.Count >= 3).Select((r, i) => ($"tp_porock_{i + 1}", r, "PO rock / cliff footprint"))).ToList(), Optional: true));
        layers.Add(new("import-terrain-paint-dry-grass.json", Materials["dry-grass"], $"dry open ground: east / south-east plains ({plains} cells) and south-facing slopes ({southSlopes} cells)",
            Smooth(dry, "tp dry grass").Select((r, i) => ($"tp_dry_{i + 1}", r, "dry grass")).ToList(), SoftEdge: SoftEdgeM));
        layers.Add(new("import-terrain-paint-bare-earth.json", Materials["bare-earth"], "CS1 dirt / mud decals", new(),
            decalCentres.Select((c, i) => ($"tp_earth_{i + 1}", Math.Round(c.X, 2), Math.Round(c.Z, 2), DecalRadius)).ToList()));
        return layers;
    }

    public static void Write(string path, Layer l)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", Path.GetFileNameWithoutExtension(path).Replace("import-", ""));
        w.WriteString("_comment", l.Optional
            ? $"NOT IMPORTED: {l.Role}. The game has no rock material (the terrain shader puts rock on steep slopes itself); kept as a mask only."
            : $"Terrain material paint (editor Terrain → Materials, POST /import/terrain-paint, bridge 0.9.29): {l.Role}. CS2 x/z metres, the same at both scales.");
        w.WriteStartObject("options");
        w.WriteBoolean("dryRun", false); w.WriteNumber("strength", 1); w.WriteNumber("softEdge", l.SoftEdge); w.WriteNumber("minStamp", 4); w.WriteNumber("maxStamps", MaxStamps);
        w.WriteEndObject();
        w.WriteStartArray("polygons");
        foreach (var (id, ring, note) in l.Polygons)
        {
            w.WriteStartObject();
            w.WriteString("id", id);
            w.WriteString("material", l.Material);
            w.WriteStartArray("polygon");
            foreach (var (x, z) in ring) { w.WriteStartArray(); w.WriteNumberValue(Json.R2(x)); w.WriteNumberValue(Json.R2(z)); w.WriteEndArray(); }
            w.WriteEndArray();
            w.WriteNumber("strength", 1);
            w.WriteNumber("softEdge", l.SoftEdge);
            w.WriteString("note", note);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("strokes");
        foreach (var (id, x, z, r) in l.Strokes ?? new())
        {
            w.WriteStartObject();
            w.WriteString("id", id); w.WriteString("material", l.Material);
            w.WriteNumber("x", x); w.WriteNumber("z", z); w.WriteNumber("radius", r); w.WriteNumber("strength", 1);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }
}
