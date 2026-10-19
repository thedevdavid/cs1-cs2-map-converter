// Procedural Objects of a CS1 map, decoded offline from the map's mod data (no game needed).
// Usage: dotnet run src/po-offline.cs -- <payloadDir> <workshopDir> <outDir> [cs1Heightmap.png]
//   cs1Heightmap.png (optional): 1081² 16-bit CS1 heightmap export; used for the preview backdrop and above-terrain stats
//   payloadDir: written by `node src/extract.mjs` (ProceduralObjectsDataKey*.bin, ProceduralObjectsLayerData.bin)
// Writes po-offline.json, po-offline-heights.png/.json, po-offline-heights-preview.png, po-classes.md.
//
// PO stores per object the base prefab name, position, rotation and the full local vertex array (meshStatus 2 =
// edited), but no triangles. Triangles come from the base prefab's mesh in its asset .crp, matched by vertex count.
#:package System.Formats.Nrbf@9.0.0
#pragma warning disable SYSLIB5005
using System.Formats.Nrbf;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

var inv = CultureInfo.InvariantCulture;
string payloadDir = args[0], workshopDir = args[1], outDir = args[2];
string? heightmapPath = args.Length > 3 && File.Exists(args[3]) ? args[3] : null;
Directory.CreateDirectory(outDir);
const int MaxVerts = 2000;
const double SpikeFactor = 2;   // v2 spike rule: max height added > SpikeFactor × sqrt(footprint area)
const string NotLoadingPrefix = "1094334744.";   // original PO's Cube/Square; PO 3071492847 drops these on load

// ---------- decode PO containers ----------
var poBytes = Directory.GetFiles(payloadDir, "ProceduralObjectsDataKey*.bin").OrderBy(f => f.Length > 0 ? Path.GetFileName(f).Length : 0).ThenBy(f => f, StringComparer.Ordinal)
    .SelectMany(File.ReadAllBytes).ToArray();
var items = ((SZArrayRecord<SerializationRecord>)NrbfDecoder.Decode(new MemoryStream(poBytes))).GetArray();
var layers = ReadLayers(Path.Combine(payloadDir, "ProceduralObjectsLayerData.bin"));
string[] visNames = { "Always", "NightOnly", "DayOnly" };   // ProceduralObjects.Classes.ProceduralObjectVisibility

var objs = new List<Po>();
foreach (ClassRecord c in items)
{
    var o = new Po
    {
        Id = c.GetInt32("id"),
        BasePrefab = c.GetString("basePrefabName") ?? "",
        Type = c.GetString("objectType") ?? "",
        Texture = NullIfEmpty(c.GetString("customTextureName")),
        MeshStatus = Convert.ToInt32(c.GetRawValue("meshStatus"), inv),
        RenderDistance = Convert.ToSingle(c.GetRawValue("renderDistance"), inv),
        LayerId = Convert.ToInt32(c.GetRawValue("layerId"), inv),
        GroupRootId = Convert.ToInt32(c.GetRawValue("groupRootId"), inv),
        BelongsToGroup = c.GetBoolean("belongsToGroup"),
        Scale = Convert.ToSingle(c.GetRawValue("scale"), inv),
        FlipFaces = c.GetBoolean("flipFaces"),
        ModuleCount = c.GetClassRecord("modulesData") is ClassRecord md0 ? Convert.ToInt32(md0.GetRawValue("_size"), inv) : 0,
    };
    var p = c.GetClassRecord("position")!; o.Pos = new V3(p.GetSingle("x"), p.GetSingle("y"), p.GetSingle("z"));
    var r = c.GetClassRecord("rotation")!; o.Rot = new Q(r.GetSingle("x"), r.GetSingle("y"), r.GetSingle("z"), r.GetSingle("w"));
    var col = c.GetClassRecord("color"); o.Color = col == null ? null : new[] { col.GetSingle("r"), col.GetSingle("g"), col.GetSingle("b"), col.GetSingle("a") };
    var vis = c.GetClassRecord("visibility"); int vi = vis == null ? 0 : Convert.ToInt32(vis.GetRawValue("value__"), inv);
    o.Visibility = vi >= 0 && vi < visNames.Length ? visNames[vi] : vi.ToString(inv);
    if (c.GetRawValue("vertices") is SZArrayRecord<SerializationRecord> va)
        o.Local = va.GetArray().Select(x => { var v = (ClassRecord)x!; return new V3(v.GetSingle("x"), v.GetSingle("y"), v.GetSingle("z")); }).ToArray();
    objs.Add(o);
}
Console.WriteLine($"PO objects: {objs.Count}");

// ---------- base meshes from asset packages ----------
var meshCache = new Dictionary<string, List<MeshData>>();
foreach (var o in objs)
{
    if (!meshCache.TryGetValue(o.BasePrefab, out var meshes)) meshCache[o.BasePrefab] = meshes = LoadPrefabMeshes(o.BasePrefab);
    int n = o.Local?.Length ?? 0;
    var m = n > 0
        ? meshes.Where(x => x.Vertices.Length == n).OrderBy(x => IsLod(x.Name) ? 1 : 0).FirstOrDefault()
        : meshes.OrderBy(x => IsLod(x.Name) ? 1 : 0).ThenByDescending(x => x.Vertices.Length).FirstOrDefault();   // no stored vertices: base mesh as is
    if (m != null) { o.Tris = m.Triangles; o.MeshSource = $"{m.Package}:{m.Name}"; }
    else { o.MeshSource = meshes.Count == 0 ? "no package found (vanilla or missing asset)" : $"no mesh with {n} vertices ({string.Join(",", meshes.Select(x => x.Vertices.Length))})"; }
    if (o.Local == null && m != null) o.Local = m.Vertices;   // meshStatus 1: base mesh unchanged
}

// ---------- world space ----------
foreach (var o in objs)
{
    o.World = (o.Local ?? Array.Empty<V3>()).Select(v => o.Pos + o.Rot.Rotate(v)).ToArray();
    if (o.World.Length > 0)
    {
        o.Min = new V3(o.World.Min(v => v.X), o.World.Min(v => v.Y), o.World.Min(v => v.Z));
        o.Max = new V3(o.World.Max(v => v.X), o.World.Max(v => v.Y), o.World.Max(v => v.Z));
    }
    else { o.Min = o.Max = o.Pos; }
    var f = o.Rot.Rotate(new V3(0, 0, 1));
    o.Yaw = (Math.Atan2(f.X, f.Z) * 180 / Math.PI + 360) % 360;
    o.Loads = !o.BasePrefab.StartsWith(NotLoadingPrefix, StringComparison.Ordinal);
}

// ---------- classes ----------
var waterfalls = objs.Where(o => o.BasePrefab.Contains("Waterfall", StringComparison.OrdinalIgnoreCase)).ToList();
foreach (var o in objs) (o.Class, o.SubClass, o.Decal) = Classify(o, waterfalls);

// ---------- po-offline.json ----------
long totalOut = 0, totalIn = 0;
using (var fs = File.Create(Path.Combine(outDir, "po-offline.json")))
{
    // layout: schema on line 1, header keys, then one object per line
    void Raw(string t) => fs.Write(Encoding.UTF8.GetBytes(t));
    Raw("{\"schema\":\"dakota-offline/cs1/po/v1\",\n");
    var hb = new MemoryStream(); var jo = new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }; var w = new Utf8JsonWriter(hb, jo);
    w.WriteStartObject();
    w.WriteString("source", "ProceduralObjectsDataKey + ProceduralObjectsDataKey1 of the map's data asset, decoded offline");
    w.WriteStartObject("coordinates");
    w.WriteString("units", "metres"); w.WriteString("axes", "CS1 world space: x east, y up, z north, origin map centre");
    w.WriteString("vertices", "world space = position + rotation * localVertex (PO scale is 1 for every object)");
    w.WriteString("yawDegrees", "heading of the object's local +z axis, clockwise from north (= Unity eulerAngles.y for yaw-only rotations)");
    w.WriteEndObject();
    w.WriteNumber("maxVerticesPerObject", MaxVerts);
    w.WriteString("decimation", "uniform vertex clustering on a cubic grid (cluster = mean position), degenerate triangles dropped; vertexCountOriginal/triangleCountOriginal keep the source counts");
    w.WriteStartArray("layers");
    foreach (var l in layers) { w.WriteStartObject(); w.WriteNumber("id", l.Id); w.WriteString("name", l.Name); w.WriteEndObject(); }
    w.WriteEndArray();
    w.WriteNumber("count", objs.Count);
    w.WriteNumber("notLoadingInPlayset", objs.Count(o => !o.Loads));
    w.WriteEndObject(); w.Flush();
    var header = Encoding.UTF8.GetString(hb.ToArray());
    Raw(header[1..^1] + ",\n\"objects\":[\n");
    for (int oi = 0; oi < objs.Count; oi++)
    {
        var o = objs[oi];
        var ob = new MemoryStream(); w = new Utf8JsonWriter(ob, jo);
        var (dv, dt) = Decimate(o.World, o.Tris, MaxVerts);
        totalIn += o.World.Length; totalOut += dv.Length;
        w.WriteStartObject();
        w.WriteNumber("id", o.Id);
        w.WriteString("basePrefab", o.BasePrefab);
        w.WriteString("baseInfoType", o.Type);
        w.WriteString("class", o.Class); w.WriteString("subClass", o.SubClass);
        w.WriteBoolean("loadsInPlayset", o.Loads);
        WriteV3(w, "position", o.Pos, 3);
        w.WriteStartArray("rotation"); foreach (var x in new[] { o.Rot.X, o.Rot.Y, o.Rot.Z, o.Rot.W }) w.WriteNumberValue(Math.Round(x, 6)); w.WriteEndArray();
        w.WriteNumber("yawDegrees", Math.Round(o.Yaw, 3));
        WriteV3(w, "boundsMin", o.Min, 2); WriteV3(w, "boundsMax", o.Max, 2);
        w.WriteNumber("meshStatus", o.MeshStatus);
        w.WriteBoolean("meshEdited", o.MeshStatus == 2);
        w.WriteString("meshSource", o.MeshSource);
        w.WriteNumber("vertexCountOriginal", o.World.Length);
        w.WriteNumber("triangleCountOriginal", o.Tris == null ? 0 : o.Tris.Length / 3);
        w.WriteBoolean("decimated", dv.Length < o.World.Length);
        if (o.Texture != null) w.WriteString("textureName", o.Texture); else w.WriteNull("textureName");
        if (o.Color != null) { w.WriteStartArray("color"); foreach (var x in o.Color) w.WriteNumberValue(Math.Round(x, 4)); w.WriteEndArray(); }
        if (o.BelongsToGroup || o.GroupRootId >= 0)
        {
            w.WriteStartObject("group");
            w.WriteNumber("rootId", o.GroupRootId >= 0 ? o.GroupRootId : o.Id);
            w.WriteBoolean("isRoot", o.GroupRootId < 0);
            w.WriteEndObject();
        }
        else w.WriteNull("group");
        var layer = layers.FirstOrDefault(l => l.Id == o.LayerId);
        if (o.LayerId > 0 || layer != null) { w.WriteStartObject("layer"); w.WriteNumber("id", o.LayerId); if (layer != null) w.WriteString("name", layer.Name); w.WriteEndObject(); } else w.WriteNull("layer");
        w.WriteString("visibility", o.Visibility);
        w.WriteNumber("renderDistance", o.RenderDistance);
        w.WriteBoolean("flipFaces", o.FlipFaces);
        w.WriteNumber("moduleCount", o.ModuleCount);
        w.WriteStartArray("vertices");
        foreach (var v in dv) { w.WriteNumberValue(Math.Round(v.X, 2)); w.WriteNumberValue(Math.Round(v.Y, 2)); w.WriteNumberValue(Math.Round(v.Z, 2)); }
        w.WriteEndArray();
        if (dt != null) { w.WriteStartArray("triangles"); foreach (var t in dt) w.WriteNumberValue(t); w.WriteEndArray(); } else w.WriteNull("triangles");
        w.WriteEndObject();
        w.Flush();
        fs.Write(ob.ToArray()); Raw(oi + 1 < objs.Count ? ",\n" : "\n");
    }
    Raw("]}\n");
}
Console.WriteLine($"vertices {totalIn} -> {totalOut} after decimation");

// ---------- heights raster ----------
const int G = 1081; const double Cell = 16, Half = 8640;
var maxY = new float[G * G]; var minY = new float[G * G]; var who = new int[G * G];
Array.Fill(maxY, float.NegativeInfinity); Array.Fill(minY, float.PositiveInfinity);
int stamped = 0;
void Put(int col, int row, double y, int id)
{
    if (col < 0 || row < 0 || col >= G || row >= G) return;
    int i = row * G + col;
    if (y > maxY[i]) { maxY[i] = (float)y; who[i] = id; }
    if (y < minY[i]) minY[i] = (float)y;
}
foreach (var o in objs)
{
    if (o.Decal || o.World.Length == 0 || Size(o) < 0.5) continue;   // decals and zero-size effect emitters carry no shape
    stamped++;
    var V = o.World;
    foreach (var v in V) Put((int)Math.Round((v.X + Half) / Cell), (int)Math.Round((Half - v.Z) / Cell), v.Y, o.Id);   // thin walls and vertical faces
    if (o.Tris == null) continue;
    for (int t = 0; t + 2 < o.Tris.Length; t += 3)
    {
        V3 a = V[o.Tris[t]], b = V[o.Tris[t + 1]], c = V[o.Tris[t + 2]];
        // grid coordinates: col = (x+8640)/16, row = (8640-z)/16 (row 0 = north)
        double ax = (a.X + Half) / Cell, az = (Half - a.Z) / Cell, bx = (b.X + Half) / Cell, bz = (Half - b.Z) / Cell, cx = (c.X + Half) / Cell, cz = (Half - c.Z) / Cell;
        double den = (bz - cz) * (ax - cx) + (cx - bx) * (az - cz);
        if (Math.Abs(den) < 1e-12) continue;
        int c0 = (int)Math.Ceiling(Math.Min(ax, Math.Min(bx, cx))), c1 = (int)Math.Floor(Math.Max(ax, Math.Max(bx, cx)));
        int r0 = (int)Math.Ceiling(Math.Min(az, Math.Min(bz, cz))), r1 = (int)Math.Floor(Math.Max(az, Math.Max(bz, cz)));
        for (int row = r0; row <= r1; row++)
            for (int col = c0; col <= c1; col++)
            {
                double l1 = ((bz - cz) * (col - cx) + (cx - bx) * (row - cz)) / den;
                double l2 = ((cz - az) * (col - cx) + (ax - cx) * (row - cz)) / den;
                double l3 = 1 - l1 - l2;
                if (l1 < -1e-9 || l2 < -1e-9 || l3 < -1e-9) continue;
                Put(col, row, l1 * a.Y + l2 * b.Y + l3 * c.Y, o.Id);
            }
    }
}
var cells = Enumerable.Range(0, G * G).Where(i => !float.IsNegativeInfinity(maxY[i])).ToArray();
float hmax = cells.Max(i => maxY[i]), hmin = cells.Min(i => maxY[i]);
var px16 = new ushort[G * G];
foreach (var i in cells) px16[i] = (ushort)Math.Clamp(Math.Round(maxY[i] * 64), 1, 65535);
WritePng(Path.Combine(outDir, "po-offline-heights.png"), G, G, 16, px16.Select(x => (int)x).ToArray());
var terrain = heightmapPath != null ? ReadGrey16(heightmapPath) : null;
int above1 = 0, above10 = 0;
if (terrain != null) foreach (var i in cells) { double dt = maxY[i] - terrain[i] / 64.0; if (dt > 1) above1++; if (dt > 10) above10++; }
var rgb = new int[G * G * 3];
for (int i = 0; i < G * G; i++)
{
    int bg = terrain == null ? 0 : (int)(30 + 110 * terrain[i] / 65535.0);
    if (float.IsNegativeInfinity(maxY[i])) { rgb[3 * i] = rgb[3 * i + 1] = rgb[3 * i + 2] = bg; continue; }
    double f = (maxY[i] - hmin) / Math.Max(1e-6, hmax - hmin);
    rgb[3 * i] = 255; rgb[3 * i + 1] = (int)(90 + 165 * f); rgb[3 * i + 2] = (int)(40 + 120 * f);
}
WritePng(Path.Combine(outDir, "po-offline-heights-preview.png"), G, G, 8, rgb, 3);
using (var sw = new StreamWriter(Path.Combine(outDir, "po-offline-heights.json"), false, new UTF8Encoding(false)))
{
    sw.Write("{\"schema\":\"dakota-offline/cs1/po-heights/v1\",\n");
    sw.Write($"\"grid\":{{\"size\":{G},\"cellSize\":16,\"rowAxis\":\"row 0 = north (same as the PNG and the CS1 heightmap)\",\"index\":\"row*1081 + col\",\"sampleX\":\"-8640 + col*16\",\"sampleZ\":\"8640 - row*16\"}},\n");
    sw.Write("\"png\":\"po-offline-heights.png (16-bit grey, value = maxY*64, 0 = no PO)\",\n");
    sw.Write("\"method\":\"every triangle of every non-decal PO object is rasterised at the grid sample points (barycentric y); every vertex is also splatted to its nearest sample so thin walls and vertical faces count\",\n");
    if (terrain != null) sw.Write($"\"aboveTerrain\":{{\"heightmap\":\"{Path.GetFileName(heightmapPath)}\",\"cellsAbove1m\":{above1},\"cellsAbove10m\":{above10}}},\n");
    sw.Write($"\"objectsStamped\":{stamped},\"cellCount\":{cells.Length},\"maxY\":{hmax.ToString("0.##", inv)},\"minOfMaxY\":{hmin.ToString("0.##", inv)},\n");
    sw.Write("\"cellsFormat\":\"[index, maxY, minY, objectIdOfMaxY]\",\n\"cells\":[\n");
    for (int k = 0; k < cells.Length; k++)
    {
        int i = cells[k];
        sw.Write($"[{i},{maxY[i].ToString("0.##", inv)},{minY[i].ToString("0.##", inv)},{who[i]}]{(k + 1 < cells.Length ? ",\n" : "\n")}");
    }
    sw.Write("]}\n");
}
var top = objs.Where(o => !o.Decal).OrderByDescending(o => o.Max.Y).First();
if (terrain != null) Console.WriteLine($"cells above CS1 terrain: >1 m {above1}, >10 m {above10}");
Console.WriteLine($"heights: {cells.Length} cells from {stamped} objects; max PO y {hmax:0.00} m (object {top.Id}, {top.BasePrefab} at {top.Pos.X:0},{top.Pos.Z:0})");

// ---------- po-classes.md ----------
var md = new StringBuilder();
md.AppendLine("# Procedural Objects by class");
md.AppendLine();
md.AppendLine($"Source: the Workshop map's PO data, decoded offline ({objs.Count} objects; {objs.Count(o => !o.Loads)} do not load with PO 3071492847). Generated by `src/po-offline.cs`; coordinates are CS1 world metres (x east, y up, z north).");
md.AppendLine();
md.AppendLine("Rules (first match wins): **decal** = 2-triangle quad, base prefab or texture named decal/marking, or a Sidewalks & More paving texture on an object less than 3 m tall; **other / effect emitter** = objects with no extent (all vertices collapsed to one point; here the 57 `Dam Electricity` objects, each carrying an Effects module); **terrain-like** = rocks, cliffs, streambeds; inside it **falls steps** = within 30 m of a waterfall object (PO cubes there count too), **mountain** = footprint ≥ 150 m or height ≥ 60 m, else **rock work**; **structure** = bridges, pillars, trusses, tunnel blocks and portals, rock shed, ruins, and the textured PO cubes (**paved block**: 8–39 m tall blocks with Sidewalks & More cobble/sidewalk textures); **other** = waterfall effects and the rest.");
md.AppendLine();
md.AppendLine("Stamp into terrain: terrain-like (all its subclasses). Rebuild as CS2 objects: structures. Ignore or redo as CS2 surfaces: decals. `po-offline-heights.png` contains every class except decals and zero-size objects. It does include the 46 animated waterfall meshes (water sheets); filter them by `objectId` if only solid ground should be stamped.");
md.AppendLine();
md.AppendLine("| class | subclass | objects | not loading | vertices (orig.) | bbox min x,y,z | bbox max x,y,z |");
md.AppendLine("|---|---|---:|---:|---:|---|---|");
foreach (var g in objs.GroupBy(o => (o.Class, o.SubClass)).OrderBy(g => ClassOrder(g.Key.Class)).ThenBy(g => g.Key.SubClass))
{
    var mn = new V3(g.Min(o => o.Min.X), g.Min(o => o.Min.Y), g.Min(o => o.Min.Z)); var mx = new V3(g.Max(o => o.Max.X), g.Max(o => o.Max.Y), g.Max(o => o.Max.Z));
    md.AppendLine($"| {g.Key.Class} | {g.Key.SubClass} | {g.Count()} | {g.Count(o => !o.Loads)} | {g.Sum(o => o.World.Length)} | {F(mn)} | {F(mx)} |");
}
md.AppendLine();
md.AppendLine("## Base prefabs per class");
md.AppendLine();
md.AppendLine("| class | base prefab | objects | mesh source |");
md.AppendLine("|---|---|---:|---|");
foreach (var g in objs.GroupBy(o => (o.Class, o.BasePrefab)).OrderBy(g => ClassOrder(g.Key.Class)).ThenByDescending(g => g.Count()))
    md.AppendLine($"| {g.Key.Class} | `{g.Key.BasePrefab}` | {g.Count()} | {g.First().MeshSource} |");
foreach (var cls in objs.Select(o => o.Class).Distinct().OrderBy(ClassOrder))
{
    md.AppendLine();
    md.AppendLine($"## Ten largest: {cls}");
    md.AppendLine();
    md.AppendLine("| id | subclass | base prefab | texture | position x,y,z | size x,y,z (m) | top y | loads |");
    md.AppendLine("|---:|---|---|---|---|---|---:|---|");
    foreach (var o in objs.Where(o => o.Class == cls).OrderByDescending(o => Size(o)).Take(10))
        md.AppendLine($"| {o.Id} | {o.SubClass} | `{o.BasePrefab}` | {o.Texture ?? ""} | {F(o.Pos)} | {F(o.Max - o.Min)} | {o.Max.Y:0.0} | {(o.Loads ? "yes" : "**no**")} |");
}
File.WriteAllText(Path.Combine(outDir, "po-classes.md"), md.ToString(), new UTF8Encoding(false));
foreach (var g in objs.GroupBy(o => (o.Class, o.SubClass)).OrderBy(g => ClassOrder(g.Key.Class)))
    Console.WriteLine($"  {g.Key.Class,-13} {g.Key.SubClass,-14} {g.Count(),4}");
Console.WriteLine($"unmatched meshes: {objs.Count(o => o.Tris == null)} ({string.Join("; ", objs.Where(o => o.Tris == null).GroupBy(o => o.BasePrefab).Select(g => g.Key + " x" + g.Count()))})");

// ---------- v2: terrain-like layers with spike and overhang filters (needs the CS1 heightmap) ----------
if (terrain == null) Console.WriteLine("v2 skipped: no CS1 heightmap given");
else
{
    string v2 = Path.Combine(outDir, "v2"); Directory.CreateDirectory(v2);
    double T(double x, double z)   // bilinear CS1 terrain (metres), grid row 0 = north
    {
        double c = Math.Clamp((x + Half) / Cell, 0, G - 1.001), rr = Math.Clamp((Half - z) / Cell, 0, G - 1.001);
        int c0 = (int)c, r0 = (int)rr; double fc = c - c0, fr = rr - r0;
        double h(int cc, int rw) => terrain[rw * G + cc] / 64.0;
        return (h(c0, r0) * (1 - fc) + h(c0 + 1, r0) * fc) * (1 - fr) + (h(c0, r0 + 1) * (1 - fc) + h(c0 + 1, r0 + 1) * fc) * fr;
    }
    // per-object stats on a 4 m lattice: covered samples, top/bottom surface per sample
    const double Lat = 4;
    var stats = new Dictionary<int, ObjStats>();
    foreach (var o in objs.Where(o => o.Class == "terrain-like"))
    {
        double x0 = Math.Floor(o.Min.X / Lat) * Lat, z0 = Math.Ceiling(o.Max.Z / Lat) * Lat;
        int cols = (int)Math.Ceiling((o.Max.X - x0) / Lat) + 1, rows = (int)Math.Ceiling((z0 - o.Min.Z) / Lat) + 1;
        var topS = new Dictionary<int, double>(); var bot = new Dictionary<int, double>();
        RasterObject(o, x0, z0, Lat, cols, rows, (c, rw, y) => { int k = rw * cols + c; if (!topS.TryGetValue(k, out var t) || y > t) topS[k] = y; if (!bot.TryGetValue(k, out var b) || y < b) bot[k] = y; });
        var st = new ObjStats { Id = o.Id, Sub = o.SubClass, Prefab = o.BasePrefab, Top = o.Max.Y, Bottom = o.Min.Y, Size = o.Max - o.Min, Samples = topS.Count };
        st.FootprintM2 = topS.Count > 0 ? topS.Count * Lat * Lat : Math.Max(1, (o.Max.X - o.Min.X) * (o.Max.Z - o.Min.Z));
        var terr = new List<double>(); int floating = 0; double maxGap = 0;
        foreach (var (k, t) in topS)
        {
            double x = x0 + (k % cols) * Lat, z = z0 - (k / cols) * Lat, tg = T(x, z);
            terr.Add(tg); st.MaxAdded = Math.Max(st.MaxAdded, t - tg);
            double gap = bot[k] - tg; if (gap > 5) { floating++; maxGap = Math.Max(maxGap, gap); }
        }
        if (terr.Count == 0) { terr.Add(T(o.Pos.X, o.Pos.Z)); st.MaxAdded = o.Max.Y - terr[0]; }
        terr.Sort(); st.TerrainMin = terr[0]; st.TerrainMedian = terr[terr.Count / 2]; st.TerrainMax = terr[^1];
        st.FloatingFraction = topS.Count > 0 ? (double)floating / topS.Count : 0; st.MaxGap = maxGap;
        st.SpikeLimit = SpikeFactor * Math.Sqrt(st.FootprintM2);
        st.Spike = st.MaxAdded > st.SpikeLimit;
        st.Overhang = st.FloatingFraction > 0.5;
        st.Decision = st.Spike ? "removed: spike" : st.Overhang && o.SubClass != "mountain" ? "removed: overhang" : st.Overhang ? "kept: mountain overhang filled solid" : "kept";
        stats[o.Id] = st;
    }
    var kept = objs.Where(o => stats.TryGetValue(o.Id, out var s) && s.Decision.StartsWith("kept")).ToList();
    var layerDefs = new (string key, Func<Po, bool> sel)[] { ("mountain", o => o.SubClass == "mountain"), ("rock-work", o => o.SubClass == "rock work"), ("falls-steps", o => o.SubClass == "falls steps"), ("terrain-like", o => true) };
    var summary = new List<string>();
    foreach (var (key, sel) in layerDefs)
    {
        var set = kept.Where(sel).ToList();
        File.WriteAllLines(Path.Combine(v2, $"ids-{key}.txt"), set.Select(o => o.Id.ToString(inv)));
        // CS1 frame: 1081 vertex grid, 16 m; CS2 frame: 4096 pixel centres, 3.5 m over ±7168 m. Both: value = metres × 32, 0 = none.
        foreach (var (frame, n, step, fx0, fz0) in new[] { ("cs1-1081", G, Cell, -Half, Half), ("cs2-4096", 4096, 3.5, -7168 + 1.75, 7168 - 1.75) })
        {
            var mx = new float[n * n]; Array.Fill(mx, float.NegativeInfinity);
            int splatted = 0;
            foreach (var o in set)
            {
                bool hit = false;
                RasterObject(o, fx0, fz0, step, n, n, (c, rw, y) => { hit = true; int i = rw * n + c; if (y > mx[i]) mx[i] = (float)y; });
                if (hit) continue;   // triangles only: snapping vertices to the nearest sample overstates steep flanks
                splatted++;
                foreach (var v in o.World)   // objects too small to cover any sample: their vertices, snapped to the nearest sample
                {
                    int c = (int)Math.Round((v.X - fx0) / step), rw = (int)Math.Round((fz0 - v.Z) / step);
                    if (c >= 0 && rw >= 0 && c < n && rw < n && v.Y > mx[rw * n + c]) mx[rw * n + c] = (float)v.Y;
                }
            }
            var px = new int[n * n]; int cellsN = 0; float topY = 0;
            for (int i = 0; i < px.Length; i++) if (!float.IsNegativeInfinity(mx[i])) { px[i] = (int)Math.Clamp(Math.Round(mx[i] * 32), 1, 65535); cellsN++; topY = Math.Max(topY, mx[i]); }
            WritePng(Path.Combine(v2, $"po-{key}-{frame}.png"), n, n, 16, px);
            summary.Add(string.Create(inv, $"| {key} | {frame} | {set.Count} | {cellsN} | {topY:0.0} | {splatted} |"));
        }
    }
    // tables
    string Row(ObjStats s) => string.Create(inv, $"| {s.Id} | {s.Sub} | `{s.Prefab.Replace("_Data", "")}` | {s.Size.X:0}×{s.Size.Z:0}×{s.Size.Y:0} | {s.FootprintM2:0} | {s.TerrainMedian:0.0} | {s.Top:0.0} | {s.MaxAdded:0.0} | {s.SpikeLimit:0.0} | {s.FloatingFraction * 100:0}% | {s.MaxGap:0.0} | {s.Decision} |");
    const string Head = "| id | subclass | base prefab | size x×z×y (m) | footprint m² | terrain under (median) | top y | max added | spike limit | floating (>5 m) | max gap | decision |\n|---:|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|";
    var md2 = new StringBuilder();
    md2.AppendLine("# PO terrain layers v2"); md2.AppendLine();
    md2.AppendLine($"Generated by `src/po-offline.cs` from the Workshop map's PO data and the CS1 heightmap `{Path.GetFileName(heightmapPath)}`. Only **terrain-like** objects (see `../po-classes.md`); structures, waterfall sheets, decals and zero-size emitters are in no v2 raster.");
    md2.AppendLine();
    md2.AppendLine("## Encoding"); md2.AppendLine();
    md2.AppendLine("All PNGs: 16-bit grey, row 0 = north, **value = metres × 32** (65535 = 2048 m), 0 = no object. Per cell the maximum over the layer's objects (no single-object winner).");
    md2.AppendLine();
    md2.AppendLine("- `*-cs1-1081.png`: CS1 frame, 1081 × 1081 samples at x = −8640 + col·16, z = 8640 − row·16 (the CS1 heightmap vertex grid; covers the exterior ring).");
    md2.AppendLine("- `*-cs2-4096.png`: CS2 playable frame, 4096 × 4096 pixels of 3.5 m over ±7168 m, sampled at pixel centres x = −7168 + (col + 0.5)·3.5, z = 7168 − (row + 0.5)·3.5, rasterised directly from the full-resolution triangles.");
    md2.AppendLine("- Both: every triangle is rasterised at the samples (barycentric y). Vertices are snapped to the nearest sample only for objects whose triangles cover no sample at all; snapping every vertex (as v1 and the in-game raster do) adds up to hundreds of metres on steep flanks at 16 m, see `../crosscheck.md`.");
    md2.AppendLine("- v1 note: `../po-offline-heights.png` used value = metres × 64 and so clipped everything above 1 024 m; v2 does not clip below 2 048 m.");
    md2.AppendLine();
    md2.AppendLine("| layer | frame | objects | cells | max y (m) | objects covering no sample (vertices snapped) |"); md2.AppendLine("|---|---|---:|---:|---:|---:|");
    summary.ForEach(l => md2.AppendLine(l));
    md2.AppendLine();
    md2.AppendLine("Object ids per layer: `ids-mountain.txt`, `ids-rock-work.txt`, `ids-falls-steps.txt`, `ids-terrain-like.txt` (union); removed: `ids-removed-spike.txt`, `ids-removed-overhang.txt`. Per-object numbers for all terrain-like objects: `objects-v2.json`.");
    md2.AppendLine();
    md2.AppendLine("## Rules"); md2.AppendLine();
    md2.AppendLine("Per object, on a 4 m lattice: footprint A = covered samples × 16 m²; terrain = bilinear CS1 heightmap; **max added** = max over samples of (object top − terrain); **floating** = share of samples whose underside is more than 5 m above the terrain.");
    md2.AppendLine();
    md2.AppendLine(string.Create(inv, $"- **Spike**: max added > {SpikeFactor} × √A (the spike limit column) → removed: a column much taller than it is wide. Applies to every terrain-like subclass."));
    md2.AppendLine("- **Overhang**: floating > 50 % → removed for rock work and falls steps; **kept** for mountains, which the max-per-cell raster fills solid down to the terrain anyway.");
    md2.AppendLine();
    md2.AppendLine("## Removed: spikes"); md2.AppendLine(); md2.AppendLine(Head);
    foreach (var s in stats.Values.Where(s => s.Spike).OrderByDescending(s => s.MaxAdded)) md2.AppendLine(Row(s));
    md2.AppendLine();
    md2.AppendLine("## Removed: overhangs (not mountain)"); md2.AppendLine(); md2.AppendLine(Head);
    foreach (var s in stats.Values.Where(s => !s.Spike && s.Overhang && s.Sub != "mountain").OrderByDescending(s => s.FloatingFraction)) md2.AppendLine(Row(s));
    md2.AppendLine();
    md2.AppendLine("## Kept: mountain overhangs (filled solid)"); md2.AppendLine(); md2.AppendLine(Head);
    foreach (var s in stats.Values.Where(s => !s.Spike && s.Overhang && s.Sub == "mountain").OrderByDescending(s => s.FloatingFraction)) md2.AppendLine(Row(s));
    md2.AppendLine();
    md2.AppendLine("## Objects named in the hm-rebuild review"); md2.AppendLine(); md2.AppendLine(Head);
    foreach (var id in new[] { 32, 33, 47, 48, 55, 334, 315, 331, 329, 658, 245, 343, 253, 224, 375, 305, 314, 684 })
    {
        if (stats.TryGetValue(id, out var s)) md2.AppendLine(Row(s));
        else { var o = objs.FirstOrDefault(x => x.Id == id); md2.AppendLine($"| {id} | {o?.Class}/{o?.SubClass} | `{o?.BasePrefab}` | | | | | | | | | not terrain-like: in no v2 layer |"); }
    }
    File.WriteAllText(Path.Combine(v2, "README.md"), md2.ToString(), new UTF8Encoding(false));
    File.WriteAllLines(Path.Combine(v2, "ids-removed-spike.txt"), stats.Values.Where(s => s.Spike).Select(s => s.Id.ToString(inv)));
    File.WriteAllLines(Path.Combine(v2, "ids-removed-overhang.txt"), stats.Values.Where(s => s.Decision == "removed: overhang").Select(s => s.Id.ToString(inv)));
    // falls steps: terrain under vs top, for capping at the water line later
    var fs2 = new StringBuilder();
    fs2.AppendLine("# Falls steps: CS1 terrain under each object vs its top"); fs2.AppendLine();
    fs2.AppendLine("The steps lie across the river channel; stamped as terrain they would dam it. Use this table to cap each step at the water line. The CS1 water surface is not in this data: it comes from DakotaExport v3 `waterSurface` once that export has run. Heights in metres (CS1 world y).");
    fs2.AppendLine();
    fs2.AppendLine("| id | base prefab | position x, z | terrain under min / median / max | object bottom | object top | top − median terrain | decision |"); fs2.AppendLine("|---:|---|---|---|---:|---:|---:|---|");
    foreach (var s in stats.Values.Where(s => s.Sub == "falls steps").OrderBy(s => s.Id))
    {
        var o = objs.First(x => x.Id == s.Id);
        fs2.AppendLine(string.Create(inv, $"| {s.Id} | `{s.Prefab.Replace("_Data", "")}` | {o.Pos.X:0}, {o.Pos.Z:0} | {s.TerrainMin:0.0} / {s.TerrainMedian:0.0} / {s.TerrainMax:0.0} | {s.Bottom:0.0} | {s.Top:0.0} | {s.Top - s.TerrainMedian:0.0} | {s.Decision} |"));
    }
    File.WriteAllText(Path.Combine(v2, "falls-steps.md"), fs2.ToString(), new UTF8Encoding(false));
    using (var jf = File.Create(Path.Combine(v2, "objects-v2.json")))
    using (var jw = new Utf8JsonWriter(jf, new JsonWriterOptions { Indented = true }))
    {
        jw.WriteStartArray();
        foreach (var s in stats.Values.OrderBy(s => s.Id))
        {
            jw.WriteStartObject();
            jw.WriteNumber("id", s.Id); jw.WriteString("subClass", s.Sub); jw.WriteString("basePrefab", s.Prefab);
            foreach (var (k, v) in new[] { ("footprintM2", s.FootprintM2), ("terrainMin", s.TerrainMin), ("terrainMedian", s.TerrainMedian), ("terrainMax", s.TerrainMax), ("bottom", s.Bottom), ("top", s.Top), ("maxAdded", s.MaxAdded), ("spikeLimit", s.SpikeLimit), ("floatingFraction", s.FloatingFraction), ("maxGap", s.MaxGap) })
                jw.WriteNumber(k, Math.Round(v, 3));
            jw.WriteString("decision", s.Decision);
            jw.WriteEndObject();
        }
        jw.WriteEndArray();
    }
    Console.WriteLine($"v2: {stats.Count} terrain-like, kept {kept.Count}, spikes {stats.Values.Count(s => s.Spike)}, overhangs removed {stats.Values.Count(s => s.Decision == "removed: overhang")}, mountain overhangs kept {stats.Values.Count(s => s.Decision.StartsWith("kept: mountain"))}");
    foreach (var l in summary) Console.WriteLine("  " + l);
}

// =====================================================================
static bool IsLod(string name) => System.Text.RegularExpressions.Regex.IsMatch(name, "lod", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
static string? NullIfEmpty(string? s) => string.IsNullOrEmpty(s) ? null : s;
static double Size(Po o) { var d = o.Max - o.Min; return Math.Max(d.X, Math.Max(d.Y, d.Z)); }
static string F(V3 v) => string.Create(CultureInfo.InvariantCulture, $"{v.X:0},{v.Y:0},{v.Z:0}");
static int ClassOrder(string c) => c switch { "terrain-like" => 0, "structure" => 1, "decal" => 2, _ => 3 };
static void WriteV3(Utf8JsonWriter w, string name, V3 v, int dec)
{ w.WriteStartArray(name); w.WriteNumberValue(Math.Round(v.X, dec)); w.WriteNumberValue(Math.Round(v.Y, dec)); w.WriteNumberValue(Math.Round(v.Z, dec)); w.WriteEndArray(); }

static (string cls, string sub, bool decal) Classify(Po o, List<Po> waterfalls)
{
    string n = o.BasePrefab, t = o.Texture ?? "";
    var size = o.Max - o.Min;
    bool quad = o.Tris != null && o.Tris.Length == 6;
    bool decalName = Rx(n, "decal|marking") || Rx(t, "decal|marking");
    bool paving = Rx(t, "Sidewalk|Cobble|Pavement|Asphalt") && size.Y < 3;
    if (quad || decalName || paving) return ("decal", quad ? "quad" : paving ? "paving" : "decal", true);
    if (Math.Max(size.X, Math.Max(size.Y, size.Z)) < 0.5) return ("other", o.ModuleCount > 0 ? "effect emitter (zero size)" : "zero size", false);
    if (Rx(n, "Waterfall")) return ("other", "water effect", false);
    if (Rx(n, "Bridge|Pillar|truss|Tunnel|shed|rock_c|Castle|Ruins|Wall|Fence")) return ("structure", Rx(n, "Bridge|Pillar|truss") ? "bridge/pillar" : Rx(n, "Tunnel|shed|rock_c") ? "tunnel/shed" : "wall/ruin", false);
    bool cube = Rx(n, "Cube_Data|Square_Data");
    if (cube) return waterfalls.Any(w => Near(o, w, 30)) ? ("terrain-like", "falls steps", false) : ("structure", "paved block", false);
    bool rock = Rx(n, "Rock|Formation|Streambed|domerock|outcrop|ledge|Erosion|Boulder|Cliff|Dam");
    if (rock)
    {
        string sub;
        if (Rx(n, "Dam")) sub = "dam";   // (the map's Dam Electricity objects are zero-size emitters, caught above)
        else if (waterfalls.Any(w => Near(o, w, 30))) sub = "falls steps";
        else if (Math.Max(size.X, size.Z) >= 150 || size.Y >= 60) sub = "mountain";
        else sub = "rock work";
        return ("terrain-like", sub, false);
    }
    return ("other", "other", false);
}
static bool Rx(string s, string p) => System.Text.RegularExpressions.Regex.IsMatch(s, p, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
static bool Near(Po a, Po b, double m) =>
    a.Min.X - m <= b.Max.X && a.Max.X + m >= b.Min.X && a.Min.Z - m <= b.Max.Z && a.Max.Z + m >= b.Min.Z && a.Min.Y - m <= b.Max.Y && a.Max.Y + m >= b.Min.Y;

static (V3[] v, int[]? t) Decimate(V3[] v, int[]? t, int max)
{
    if (v.Length <= max) return (v, t);
    var mn = new V3(v.Min(p => p.X), v.Min(p => p.Y), v.Min(p => p.Z)); var mx = new V3(v.Max(p => p.X), v.Max(p => p.Y), v.Max(p => p.Z));
    var d = mx - mn; double lo = 1e-4, hi = Math.Max(d.X, Math.Max(d.Y, d.Z)) + 1;
    Dictionary<(long, long, long), int> Cluster(double s, int[] map)
    {
        var cells = new Dictionary<(long, long, long), int>();
        for (int i = 0; i < v.Length; i++)
        {
            var k = ((long)Math.Floor((v[i].X - mn.X) / s), (long)Math.Floor((v[i].Y - mn.Y) / s), (long)Math.Floor((v[i].Z - mn.Z) / s));
            if (!cells.TryGetValue(k, out var id)) { id = cells.Count; cells[k] = id; }
            map[i] = id;
        }
        return cells;
    }
    var map = new int[v.Length];
    for (int it = 0; it < 40; it++) { double s = Math.Sqrt(lo * hi); if (Cluster(s, map).Count > max) lo = s; else hi = s; }
    int n = Cluster(hi, map).Count;
    var sum = new V3[n]; var cnt = new int[n];
    for (int i = 0; i < v.Length; i++) { sum[map[i]] = sum[map[i]] + v[i]; cnt[map[i]]++; }
    var outV = sum.Select((s, i) => s * (1.0 / cnt[i])).ToArray();
    if (t == null) return (outV, null);
    var seen = new HashSet<(int, int, int)>(); var outT = new List<int>();
    for (int k = 0; k + 2 < t.Length; k += 3)
    {
        int a = map[t[k]], b = map[t[k + 1]], c = map[t[k + 2]];
        if (a == b || b == c || a == c) continue;
        var key = Norm(a, b, c); if (!seen.Add(key)) continue;
        outT.Add(a); outT.Add(b); outT.Add(c);
    }
    return (outV, outT.ToArray());
    static (int, int, int) Norm(int a, int b, int c) { var s = new[] { a, b, c }; Array.Sort(s); return (s[0], s[1], s[2]); }
}

List<MeshData> LoadPrefabMeshes(string prefab)
{
    var result = new List<MeshData>();
    int dot = prefab.IndexOf('.');
    if (dot <= 0 || !prefab[..dot].All(char.IsDigit)) return result;
    string wsid = prefab[..dot], asset = prefab[(dot + 1)..];
    var dirs = new List<string> { Path.Combine(workshopDir, wsid) };
    if (wsid == "1094334744") dirs.Add(Path.Combine(workshopDir, "3071492847"));   // same cube/square meshes ship with the re-upload
    foreach (var dir in dirs.Where(Directory.Exists))
        foreach (var f in Directory.GetFiles(dir, "*.crp"))
        {
            var pkg = CrpPackage.Read(f);
            if (!pkg.Assets.Any(a => a.Type == 1 && a.Name == asset)) continue;
            foreach (var a in pkg.Assets.Where(a => a.Type == 4))
                try { var m = MeshData.Parse(pkg.ReadAsset(a)); m.Package = Path.GetFileName(f); result.Add(m); } catch { }
            if (result.Count > 0) return result;
        }
    return result;
}

static List<LayerInfo> ReadLayers(string path)
{
    var list = new List<LayerInfo>();
    if (!File.Exists(path)) return list;
    try
    {
        var root = NrbfDecoder.Decode(File.OpenRead(path));
        IEnumerable<SerializationRecord?> elems = root is SZArrayRecord<SerializationRecord> a ? a.GetArray() : new[] { root };
        foreach (var e in elems)
            if (e is ClassRecord c && c.HasMember("m_name"))
                list.Add(new LayerInfo(Convert.ToInt32(c.GetRawValue("m_id"), CultureInfo.InvariantCulture), c.GetString("m_name") ?? ""));
    }
    catch (Exception ex) { Console.WriteLine("layers: " + ex.Message); }
    return list;
}

static void WritePng(string path, int w, int h, int depth, int[] px, int channels = 1)
{
    using var fs = File.Create(path);
    fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    var ihdr = new byte[13];
    BE(ihdr, 0, w); BE(ihdr, 4, h); ihdr[8] = (byte)depth; ihdr[9] = (byte)(channels == 3 ? 2 : 0); ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
    Chunk(fs, "IHDR", ihdr);
    int bpp = depth / 8 * channels;
    var raw = new MemoryStream();
    using (var z = new ZLibStream(raw, CompressionLevel.Optimal, true))
    {
        var line = new byte[1 + w * bpp];
        for (int y = 0; y < h; y++)
        {
            line[0] = 0;
            if (depth == 16) for (int x = 0; x < w; x++) { int v = px[y * w + x]; line[1 + 2 * x] = (byte)(v >> 8); line[2 + 2 * x] = (byte)v; }
            else for (int x = 0; x < w * channels; x++) line[1 + x] = (byte)Math.Clamp(px[y * w * channels + x], 0, 255);
            z.Write(line);
        }
    }
    Chunk(fs, "IDAT", raw.ToArray());
    Chunk(fs, "IEND", Array.Empty<byte>());
    static void BE(byte[] b, int o, int v) { b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v; }
    static void Chunk(Stream s, string type, byte[] data)
    {
        var len = new byte[4]; BE(len, 0, data.Length); s.Write(len);
        var td = Encoding.ASCII.GetBytes(type).Concat(data).ToArray(); s.Write(td);
        var crc = new byte[4]; BE(crc, 0, (int)Crc(td)); s.Write(crc);
    }
    static uint Crc(byte[] d)
    {
        uint c = 0xFFFFFFFF;
        foreach (var b in d) { c ^= b; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1; }
        return c ^ 0xFFFFFFFF;
    }
}

// 16-bit greyscale PNG reader (CS1 heightmap exports), all five filter types
static int[] ReadGrey16(string path)
{
    var b = File.ReadAllBytes(path); int p = 8, w = 0, h = 0; var idat = new MemoryStream();
    while (p < b.Length)
    {
        int len = (b[p] << 24) | (b[p + 1] << 16) | (b[p + 2] << 8) | b[p + 3]; string type = Encoding.ASCII.GetString(b, p + 4, 4);
        if (type == "IHDR") { w = (b[p + 8] << 24) | (b[p + 9] << 16) | (b[p + 10] << 8) | b[p + 11]; h = (b[p + 12] << 24) | (b[p + 13] << 16) | (b[p + 14] << 8) | b[p + 15]; if (b[p + 16] != 16 || b[p + 17] != 0) throw new InvalidDataException("need 16-bit grey"); }
        else if (type == "IDAT") idat.Write(b, p + 8, len);
        else if (type == "IEND") break;
        p += 12 + len;
    }
    idat.Position = 0; var raw = new MemoryStream(); using (var z = new ZLibStream(idat, CompressionMode.Decompress)) z.CopyTo(raw);
    var r = raw.ToArray(); int stride = w * 2; var cur = new byte[stride]; var prev = new byte[stride]; var outv = new int[w * h];
    for (int y = 0; y < h; y++)
    {
        int f = r[y * (stride + 1)];
        for (int x = 0; x < stride; x++)
        {
            int a = x >= 2 ? cur[x - 2] : 0, up = prev[x], c = x >= 2 ? prev[x - 2] : 0, v = r[y * (stride + 1) + 1 + x];
            v += f switch { 1 => a, 2 => up, 3 => (a + up) >> 1, 4 => Paeth(a, up, c), _ => 0 };
            cur[x] = (byte)v;
        }
        for (int x = 0; x < w; x++) outv[y * w + x] = (cur[2 * x] << 8) | cur[2 * x + 1];
        (prev, cur) = (cur, prev);
    }
    return outv;
    static int Paeth(int a, int b, int c) { int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c); return pa <= pb && pa <= pc ? a : pb <= pc ? b : c; }
}

// rasterise an object's triangles at grid samples x = x0 + col·step, z = z0 − row·step (row 0 = north)
static void RasterObject(Po o, double x0, double z0, double step, int cols, int rows, Action<int, int, double> put)
{
    if (o.Tris == null) return;
    var V = o.World;
    for (int t = 0; t + 2 < o.Tris.Length; t += 3)
    {
        V3 a = V[o.Tris[t]], b = V[o.Tris[t + 1]], c = V[o.Tris[t + 2]];
        double ax = (a.X - x0) / step, az = (z0 - a.Z) / step, bx = (b.X - x0) / step, bz = (z0 - b.Z) / step, cx = (c.X - x0) / step, cz = (z0 - c.Z) / step;
        double den = (bz - cz) * (ax - cx) + (cx - bx) * (az - cz);
        if (Math.Abs(den) < 1e-12) continue;
        int c0 = Math.Max(0, (int)Math.Ceiling(Math.Min(ax, Math.Min(bx, cx)))), c1 = Math.Min(cols - 1, (int)Math.Floor(Math.Max(ax, Math.Max(bx, cx))));
        int r0 = Math.Max(0, (int)Math.Ceiling(Math.Min(az, Math.Min(bz, cz)))), r1 = Math.Min(rows - 1, (int)Math.Floor(Math.Max(az, Math.Max(bz, cz))));
        for (int row = r0; row <= r1; row++)
            for (int col = c0; col <= c1; col++)
            {
                double l1 = ((bz - cz) * (col - cx) + (cx - bx) * (row - cz)) / den, l2 = ((cz - az) * (col - cx) + (ax - cx) * (row - cz)) / den, l3 = 1 - l1 - l2;
                if (l1 < -1e-9 || l2 < -1e-9 || l3 < -1e-9) continue;
                put(col, row, l1 * a.Y + l2 * b.Y + l3 * c.Y);
            }
    }
}

record LayerInfo(int Id, string Name);

class ObjStats
{
    public int Id, Samples; public string Sub = "", Prefab = "", Decision = ""; public V3 Size;
    public double Top, Bottom, FootprintM2, TerrainMin, TerrainMedian, TerrainMax, MaxAdded, FloatingFraction, MaxGap, SpikeLimit; public bool Spike, Overhang;
}

class Po
{
    public int Id, MeshStatus, LayerId, GroupRootId, ModuleCount; public string BasePrefab = "", Type = "", Visibility = "", MeshSource = "", Class = "", SubClass = "";
    public string? Texture; public float RenderDistance, Scale; public bool BelongsToGroup, FlipFaces, Loads, Decal;
    public V3 Pos, Min, Max; public Q Rot; public float[]? Color; public V3[]? Local; public V3[] World = Array.Empty<V3>(); public int[]? Tris; public double Yaw;
}

readonly record struct V3(double X, double Y, double Z)
{
    public static V3 operator +(V3 a, V3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static V3 operator -(V3 a, V3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static V3 operator *(V3 a, double s) => new(a.X * s, a.Y * s, a.Z * s);
    public static V3 Cross(V3 a, V3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
}

readonly record struct Q(double X, double Y, double Z, double W)
{
    public V3 Rotate(V3 v) { var u = new V3(X, Y, Z); var t = V3.Cross(u, v) * 2; return v + t * W + V3.Cross(u, t); }
}

class MeshData
{
    public string Name = "", Package = ""; public V3[] Vertices = Array.Empty<V3>(); public int[] Triangles = Array.Empty<int>();
    // ColossalFramework PackageSerializer layout for UnityEngine.Mesh: bool, type string, name, then
    // Int32-counted arrays: vertices (3f), colors (4f), uv (2f), normals (3f), tangents (4f), boneWeights (32 B), bindposes (16f),
    // subMeshCount, and per sub-mesh an Int32-counted triangle index array.
    public static MeshData Parse(byte[] b)
    {
        var r = new BinaryReader(new MemoryStream(b));
        r.ReadByte(); var type = r.ReadString();
        if (!type.StartsWith("UnityEngine.Mesh")) throw new InvalidDataException(type);
        var m = new MeshData { Name = r.ReadString() };
        int n = r.ReadInt32(); m.Vertices = new V3[n];
        for (int i = 0; i < n; i++) m.Vertices[i] = new V3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        foreach (var size in new[] { 16, 8, 12, 16, 32, 64 }) { int k = r.ReadInt32(); r.BaseStream.Seek((long)k * size, SeekOrigin.Current); }
        int sub = r.ReadInt32(); var tris = new List<int>();
        for (int s = 0; s < sub; s++) { int k = r.ReadInt32(); for (int i = 0; i < k; i++) tris.Add(r.ReadInt32()); }
        if (tris.Any(t => t < 0 || t >= n)) throw new InvalidDataException("index out of range");
        m.Triangles = tris.ToArray();
        return m;
    }
}

class CrpPackage
{
    public string File = ""; public long DataOffset; public List<(string Name, int Type, long Offset, long Size)> Assets = new();
    public static CrpPackage Read(string file)
    {
        using var r = new BinaryReader(System.IO.File.OpenRead(file));
        if (new string(r.ReadChars(4)) != "CRAP") throw new InvalidDataException(file);
        int version = r.ReadUInt16(); r.ReadString(); if (version >= 3) r.ReadString(); r.ReadUInt32(); r.ReadString();
        int count = r.ReadInt32(); var p = new CrpPackage { File = file, DataOffset = r.ReadInt64() };
        for (int i = 0; i < count; i++) { var name = r.ReadString(); r.ReadString(); int type = r.ReadInt32(); long off = r.ReadInt64(), size = r.ReadInt64(); p.Assets.Add((name, type, off, size)); }
        return p;
    }
    public byte[] ReadAsset((string Name, int Type, long Offset, long Size) a)
    {
        using var fs = System.IO.File.OpenRead(File); fs.Seek(DataOffset + a.Offset, SeekOrigin.Begin);
        var b = new byte[a.Size]; fs.ReadExactly(b); return b;
    }
}
