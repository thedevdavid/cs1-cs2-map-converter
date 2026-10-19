using System.Text.Json;
using NetConverter;

string? dumpPath = null, mappingPath = null, outDir = "out", livePath = null, baselineDir = null, heightmapPath = null, retryReport = null;
bool inventoryOnly = false, retry = false, layersOnly = false;
string? treesDir = null, rockHeightmapPath = null;
string poOfflinePath = Path.Combine("..", "cs1-offline", "out", "po-offline.json");
double rockDegrees = 40;
bool seawayAtWater = false, mergeCanal = true;
const string RockSurface = "gaga TTP - Pebbles 1L dark";
const string NetCorridorRule = "on a ground-level net corridor (half width + 2 m)";
{
    var cs2Hm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", "Heightmaps", "10190442-dktflls-1to1.png");
    if (File.Exists(cs2Hm)) rockHeightmapPath = cs2Hm;
}
ulong treeSeed = 20261019;
double rampDelta = 6, rampLength = 60, minBridge = 8, seaLevel = 97.5, waterVariant = double.NaN, heightFactor = 2.0, topologyFactor = 2.0;
bool absolute = true;
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--mapping" or "-m": mappingPath = args[++i]; break;
        case "--out" or "-o": outDir = args[++i]; break;
        case "--inventory": inventoryOnly = true; break;
        case "--live": livePath = args[++i]; break;
        case "--baseline": baselineDir = args[++i]; break;
        case "--retry": retry = true; break;
        case "--trees-dir": treesDir = args[++i]; break;
        case "--rock-heightmap": rockHeightmapPath = args[++i]; break;
        case "--seaway-at-water": seawayAtWater = true; break;
        case "--no-merge-canal": mergeCanal = false; break;
        case "--po-offline": poOfflinePath = args[++i]; break;
        case "--rock-degrees": rockDegrees = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--tree-seed": treeSeed = ulong.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--network-only": layersOnly = true; break;
        case "--heightmap": heightmapPath = args[++i]; break;
        case "--retry-report": retryReport = args[++i]; break;
        case "--ramp-delta": rampDelta = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--min-bridge-elevation": minBridge = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--height-factor": heightFactor = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--topology-factor": topologyFactor = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--height-mode": absolute = args[++i] == "absolute"; break;
        case "--water-variant": waterVariant = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--sea-level": seaLevel = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--ramp-length": rampLength = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture); break;
        case "--help" or "-h":
            Console.WriteLine("net-converter <dump.json> [--mapping mapping.tsv] [--out out] [--inventory] [--live out/live-graph.json] [--baseline out/round1] [--ramp-delta 6] [--ramp-length 60] [--retry] [--trees-dir DIR] [--tree-seed N] [--network-only] [--heightmap cs2-4096.png] [--retry-report report.json] [--min-bridge-elevation 8] [--sea-level 97.5] [--water-variant 93.0] [--height-factor 2] [--topology-factor 2] [--height-mode absolute|elevation]");
            return 0;
        default: dumpPath = args[i]; break;
    }
}
if (dumpPath == null)
{
    var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DakotaExport");
    dumpPath = Directory.Exists(dir) ? Directory.GetFiles(dir, "*.json").OrderDescending().FirstOrDefault() : null;
    if (dumpPath == null) { Console.Error.WriteLine("no dump given and none found in " + dir); return 2; }
}
mappingPath ??= File.Exists("mapping.tsv") ? "mapping.tsv" : Path.Combine(AppContext.BaseDirectory, "mapping.tsv");

Console.WriteLine($"dump    {dumpPath}");
var dump = Dump.Load(dumpPath);
// CS1 terrain (unscaled) for the objects' "off the ground" test.
Heightmap? cs1Terrain = null;
if (heightmapPath != null)
{
    cs1Terrain = Heightmap.LoadPng(heightmapPath);
    cs1Terrain.Orient(dump.Nodes.Values.Select(n => (n.Position.X, n.Position.Z, n.Terrain)));
}
// CS2 heights = heightFactor × CS1 (2048 m heightmap at Height Scale 4096 -> 2). Everything below works in CS2 metres.
var inventory = Outputs.Inventory(dump);
// The network is converted at topologyFactor (2×: the passes' metre thresholds are CS2 limits on the 2× map) and every
// written height is scaled by heightFactor / topologyFactor, so the 1× and 2× outputs have the same nodes and courses.
dump.ScaleHeights(topologyFactor);
Outputs.YScale = heightFactor / topologyFactor;
Console.WriteLine(inventory);
if (inventoryOnly) return 0;

Console.WriteLine($"mapping {mappingPath}");
var mapping = Mapping.Load(mappingPath);
Heightmap? terrain = null;
if (heightmapPath != null)
{
    terrain = Heightmap.LoadPng(heightmapPath, heightScale: 2048 * topologyFactor);
    double err = terrain.Orient(dump.Nodes.Values.Select(n => (n.Position.X, n.Position.Z, n.Terrain)));
    Console.WriteLine($"heightmap {heightmapPath}: {terrain.Size}², row 0 = {(terrain.NorthUp ? "north" : "south")}, mean |CS2 − CS1 terrain| at CS1 nodes {err:F2} m");
}
var result = Converter.Run(dump, mapping, new ConvertOptions { RampDelta = rampDelta, RampLength = rampLength, Terrain = terrain, MinBridgeElevation = minBridge });
Console.WriteLine($"deck fit: {result.Stats.DeckEdgesSplit} raised/sunk courses split into {result.Stats.DeckEdgesSplit + result.Stats.DeckSplits} pieces ({result.Stats.SpanSplits} splits only for span > 75 m; {result.Stats.DeckSplitsSkippedNearMerge} skipped next to a merging course)");
Directory.CreateDirectory(outDir);

// ---------------------------------------------------------------------- CS2-side audit fixes (audit/cs2-side.md)
using var rootDoc = JsonDocument.Parse(File.ReadAllText(dumpPath));
var surfaceEarly = WaterSurface.Load(rootDoc.RootElement, dumpPath, cs1Terrain == null ? null : cs1Terrain.Sample);
var auditLog = new System.Text.StringBuilder();
auditLog.AppendLine().AppendLine("## CS2 audit fixes (audit/cs2-side.md, before the run)").AppendLine();
auditLog.AppendLine($"- Short border edges folded into their neighbour (< 10 m): {result.R4.BorderStubsFolded}.");
{
    // Finding 8: east-border Gravel Road ends become road outside connections. Keep one per CS1 connection (the closest),
    // pull the others 1 m inside the map so the game does not tag them.
    var pre = Connections.Build(result, 7168);
    var gravel = pre.BorderNodes.Where(b => b.Prefab == "Gravel Road").ToList();
    var keep = gravel.Where(b => b.Cs1 != null).GroupBy(b => b.Cs1).Select(g => g.OrderBy(b => b.Distance).First().Node).ToHashSet(ReferenceEqualityComparer.Instance);
    var pulled = new List<string>();
    foreach (var b in gravel.Where(b => !keep.Contains(b.Node)))
    {
        var n = b.Node; var p = n.Position;
        n.Position = Math.Abs(p.X) >= Math.Abs(p.Z) ? p with { X = p.X - Math.Sign(p.X) } : p with { Z = p.Z - Math.Sign(p.Z) };
        n.Border = false;
        foreach (var e in result.Edges.Where(e => e.A == n || e.B == n)) e.Curve = e.Curve.WithEnds(e.A.Position, e.B.Position);
        pulled.Add(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{n.Id} ({p.X:F0}, {p.Z:F0})"));
    }
    auditLog.AppendLine($"- Gravel Road border ends: {gravel.Count}; kept as outside connections {keep.Count} (one per CS1 connection: {string.Join(", ", keep.Cast<OutNode>().Select(n => n.Id))}); pulled 1 m inside {pulled.Count}: {string.Join(", ", pulled)}.");
    Console.WriteLine($"gravel border ends: kept {keep.Count}, pulled inside {pulled.Count}");
}
{
    // Finding 4: boatways are ship lanes, not channels: keep them only where the canal holds water.
    var srcs = dump.WaterSources.Select(s => (s.Output.X, s.Output.Z, Level: s.TargetHeight / dump.HeightFactor)).ToList();
    bool Wet(double x, double z)
    {
        if (surfaceEarly != null && surfaceEarly.Cells.TryGetValue(surfaceEarly.Grid.Index(x, z), out var c) && c.Depth > 0.05) return true;
        double ground = cs1Terrain?.Sample(x, z) ?? double.MaxValue;
        return srcs.Any(s => (s.X - x) * (s.X - x) + (s.Z - z) * (s.Z - z) <= 150 * 150 && s.Level > ground + 0.2);
    }
    var boat = result.Layer(Layers.Walls).Where(e => e.Map.Cs2Prefab == "Medium Boatway").ToList();
    var dry = boat.Where(e => { var m = e.Curve.Eval(0.5); return !Wet(m.X, m.Z); }).ToHashSet(ReferenceEqualityComparer.Instance);
    foreach (OutEdge e in dry) result.Fate[e.Cs1Segment] = "boatway dropped: dry canal";
    result.Edges.RemoveAll(dry.Contains);
    auditLog.AppendLine($"- Boatways (CS1 canals): {boat.Count}; kept where water stands (CS1 water surface, or a source within 150 m above the ground) {boat.Count - dry.Count}, dropped {dry.Count}.");

    // Finding 4: flat QuaySmall01 edges (cross-slope < 0.4 m in this scale) get a forced wall: "wall" for the bridge on the
    // lower side, and the node-elevation fallback (≥ 4 m one side, ≥ 8 m both sides, in output metres).
    int flat = 0;
    var sides = new Dictionary<string, int>();
    if (cs1Terrain != null)
        foreach (var e in result.Layer(Layers.Walls).Where(e => e.Map.Cs2Prefab == "QuaySmall01"))
        {
            var m = e.Curve.Eval(0.5); var d = e.Curve.Eval(0.55) - e.Curve.Eval(0.45);
            double len = Math.Sqrt(d.X * d.X + d.Z * d.Z); if (len < 1e-6) continue;
            double lx = -d.Z / len, lz = d.X / len;   // left of a → b, seen from above (x east, z north)
            double diff = 0;
            foreach (var off in new[] { 4.0, 6.0, 8.0 })
            {
                double hl = cs1Terrain.Sample(m.X + lx * off, m.Z + lz * off) * heightFactor, hr = cs1Terrain.Sample(m.X - lx * off, m.Z - lz * off) * heightFactor;
                if (Math.Abs(hl - hr) > Math.Abs(diff)) diff = hl - hr;
            }
            if (Math.Abs(diff) >= 0.4) continue;   // the game raises the lower side by itself
            flat++;
            e.Wall = Math.Abs(diff) < 0.1 ? "both" : diff < 0 ? "left" : "right";
            sides[e.Wall] = sides.GetValueOrDefault(e.Wall) + 1;
            double need = (e.Wall == "both" ? 8 : 4) / Outputs.YScale;   // topology metres
            foreach (var n in new[] { e.A, e.B }) n.Elevation = Math.Max(n.Elevation, need);
        }
    auditLog.AppendLine($"- Flat QuaySmall01 edges (cross-slope < 0.4 m at × {heightFactor}): {flat}, wall {string.Join(", ", sides.Select(s => $"{s.Key} {s.Value}"))}; their nodes raised to ≥ 4 m (one side) / ≥ 8 m (both) as the fallback.");
    Console.WriteLine($"walls: boatways kept {boat.Count - dry.Count}/{boat.Count}; flat quays forced {flat}");
}
if (seawayAtWater)
{
    // Finding 7: seaways at the water surface instead of the riverbed (pending the bridge's dig test).
    int moved = 0;
    foreach (var n in result.NodesOf(result.Layer(Layers.ShipPaths)))
    {
        double level = seaLevel;
        if (surfaceEarly != null && surfaceEarly.Cells.TryGetValue(surfaceEarly.Grid.Index(n.Position.X, n.Position.Z), out var c)) level = c.Surface;
        double e = Math.Max(0, level * topologyFactor - n.Terrain);
        if (e > 0) { n.Elevation = Math.Round(e, 2); moved++; }
    }
    auditLog.AppendLine($"- Seaway nodes lifted to the water surface (--seaway-at-water): {moved}.");
}
else auditLog.AppendLine("- Seaways stay at the riverbed (pass --seaway-at-water to write them at the water surface once the dig test shows digging).");

{
    // v5 Network Skins: segments whose skin removes street lights or bridge pillars. The bridge has no edge field for either
    // (IMPORT_SCHEMA.md); the converter writes informational "noPillars" / "noStreetLights" (ignored by the bridge today)
    // and lists them in manual-objects.md.
    var r = rootDoc.RootElement;
    if (r.TryGetProperty("networkSkins", out var ns) && ns.ValueKind == JsonValueKind.Object)
    {
        var skinFlags = new Dictionary<int, (bool Pillars, bool Lights)>();
        foreach (var sk in ns.GetProperty("skins").EnumerateArray())
        {
            bool pil = false, lig = false;
            foreach (var m in sk.GetProperty("modifiers").EnumerateArray())
            {
                string type = m.GetProperty("type").GetString() ?? "";
                var f = m.GetProperty("fields");
                if (type == "PillarModifier" && f.TryGetProperty("Pillar", out var pv) && pv.ValueKind == JsonValueKind.Null) pil = true;
                if (type == "StreetLightModifier" && f.TryGetProperty("StreetLight", out var lv) && lv.ValueKind == JsonValueKind.Null) lig = true;
            }
            skinFlags[sk.GetProperty("index").GetInt32()] = (pil, lig);
        }
        var segs = ns.GetProperty("segments").EnumerateArray().Select(v => v.GetInt32()).ToArray();
        for (int i = 0; i + 1 < segs.Length; i += 2)
            if (skinFlags.TryGetValue(segs[i + 1], out var fl) && (fl.Pillars || fl.Lights)) Outputs.SkinFlags[segs[i]] = fl;
        Console.WriteLine($"network skins: {Outputs.SkinFlags.Count} segments (no pillars {Outputs.SkinFlags.Values.Count(v => v.Pillars)}, no street lights {Outputs.SkinFlags.Values.Count(v => v.Lights)})");
    }
}
Outputs.WriteImport(Path.Combine(outDir, "import-highways.json"), "Dakota Falls highways", result.Layer(Layers.Highways).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-rail.json"), "Dakota Falls rail", result.Layer(Layers.Rail).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-roads.json"), "Dakota Falls roads", result.Layer(Layers.Roads).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-all.json"), "Dakota Falls network (all layers)",
    Layers.Imported.SelectMany(l => result.Layer(l)).ToList(), absolute);
// Power stays out of import-all.json so it is a separately undoable import.
var power = result.Layer(Layers.Power).Where(e => e.Map.Cs2Prefab != null).ToList();
if (power.Count > 0) Outputs.WriteImport(Path.Combine(outDir, "import-power.json"), "Dakota Falls power lines", power, absolute);
Console.WriteLine($"power: {power.Count} courses -> {(power.Count > 0 ? "import-power.json" : "not written (power rule is SKIP in mapping.tsv)")}");
foreach (var (layer, title, fileName) in new[] { (Layers.Walls, "walls (quays, retaining walls, canals, flood walls)", "import-walls.json"), (Layers.ShipPaths, "ship paths", "import-shippaths.json"), (Layers.Decor, "DECORATIVE monorail arch bridge (not transit)", "import-decor-monorail.json") })
{
    var es = result.Layer(layer).Where(e => e.Map.Cs2Prefab != null).ToList();
    var file = Path.Combine(outDir, fileName);
    if (es.Count > 0) Outputs.WriteImport(file, $"Dakota Falls {title}", es, absolute); else File.Delete(file);
    Console.WriteLine($"{layer}: {es.Count} courses{(es.Count > 0 ? $" -> {fileName}" : "")} ({string.Join(", ", es.GroupBy(e => e.Map.Cs2Prefab).Select(g => $"{g.Key} {g.Count()}"))})");
}
File.Delete(Path.Combine(outDir, "import-quays.json"));
Outputs.WriteWater(Path.Combine(outDir, "water-sources.json"), dump, 7168);
// --sea-level is in CS1 metres (the user measured 97–98 m on a 1:1 map); scaled like every other height.
{
    // Finding 6: per-source rules (drop at/below sea level + 1 m, clamp near-border sources inward, drop far outside),
    // merge the small canal feeds, refine levels from the CS1 water surface (v3).
    string? waterCheck = null; Dictionary<int, double>? waterOverrides = null;
    if (surfaceEarly != null) (waterCheck, waterOverrides) = surfaceEarly.Check(dump, seaLevel, heightFactor, 7168);
    var (wsrc, wlog) = Water.Plan(dump, seaLevel, heightFactor, 7168, cs1Terrain == null ? null : cs1Terrain.Sample, waterOverrides, mergeCanal);
    Water.Write(Path.Combine(outDir, "import-water.json"), wsrc, heightFactor, seaLevel * heightFactor, 7168);
    var wmd = new System.Text.StringBuilder(waterCheck ?? "# Water check\n\nNo CS1 water surface in this dump (v3 adds it); only the source rules are applied.\n");
    wmd.AppendLine().AppendLine($"## Source changes (audit/cs2-side.md finding 6; sea level {seaLevel} × {heightFactor})").AppendLine();
    wmd.AppendLine($"{dump.WaterSources.Count} CS1 sources → {wsrc.Count} in import-water.json.").AppendLine();
    foreach (var l in wlog) wmd.AppendLine($"- {l}");
    if (!double.IsNaN(waterVariant))
    {
        // Second sea level to switch to in game without a rerun; the same per-source rules at that level.
        Dictionary<int, double>? vOverrides = surfaceEarly?.Check(dump, waterVariant, heightFactor, 7168).Item2;
        var (vsrc, vlog) = Water.Plan(dump, waterVariant, heightFactor, 7168, cs1Terrain == null ? null : cs1Terrain.Sample, vOverrides, mergeCanal);
        string vname = FormattableString.Invariant($"import-water-sea{waterVariant * heightFactor:0.#}.json");
        Water.Write(Path.Combine(outDir, vname), vsrc, heightFactor, waterVariant * heightFactor, 7168);
        wmd.AppendLine().AppendLine(FormattableString.Invariant($"## Variant: sea level {waterVariant} × {heightFactor} = {waterVariant * heightFactor:0.#} ({vname})")).AppendLine();
        wmd.AppendLine($"Import instead of import-water.json to try the measured level (falls back to {seaLevel} × {heightFactor} = {seaLevel * heightFactor:0.#}). {dump.WaterSources.Count} CS1 sources → {vsrc.Count}.").AppendLine();
        foreach (var l in vlog) wmd.AppendLine($"- {l}");
        Console.WriteLine($"water variant: sea level {waterVariant} -> {vsrc.Count} sources in {vname}");
    }
    File.WriteAllText(Path.Combine(outDir, "water-check.md"), wmd.ToString());
    auditLog.AppendLine($"- Water sources: {dump.WaterSources.Count} → {wsrc.Count} ({wlog.Count} changes, water-check.md).");
    Console.WriteLine($"water: {dump.WaterSources.Count} -> {wsrc.Count} sources ({wlog.Count} changes)");
}
File.WriteAllText(Path.Combine(outDir, "round4.md"), Outputs.Round4Report(result, minBridge));
var r4 = result.R4;
Console.WriteLine($"crossings {r4.Crossings}: raised {r4.CrossingNodesInserted} nodes (+{r4.CrossingRampNodes} ramp nodes), already separated {r4.CrossingsAlreadySeparated}, at grade in CS1 {r4.CrossingsAtGradeInCs1}");
Console.WriteLine($"bridge profiles: {r4.BridgeChainsChanged}/{r4.BridgeChains} chains, {r4.BridgeNodesChanged} nodes (max {r4.BridgeMaxChange:F1} m); tangents {r4.TangentFailures} ({r4.TangentSwapped} swapped, {r4.TangentStraightened} straight); tiny stubs {r4.TinyStubsCollapsed}; ramp tails snapped {r4.RampTailsSnapped}");
Console.WriteLine($"transitions: {r4.PortalsSetToZero} shallow tunnel nodes -> portal 0, {r4.TunnelsDeepened} -> -12 m; {r4.RaisedToGround} low deck nodes -> 0, {r4.RaisedToMin} -> 2 m; {r4.TunnelNodesCapped} tunnel nodes capped to the 15 % portal grade; height factor {heightFactor}, heightMode {(absolute ? "absolute" : "elevation")}");
Outputs.WriteOutside(Path.Combine(outDir, "outside-connections.json"), result, 7168);
Outputs.WriteGeoJson(Path.Combine(outDir, "network.geojson"), result);
Outputs.WriteOverlay(Path.Combine(outDir, "overlay-4096.png"), result, 4096, 3.5);
var stats = Outputs.Stats(result, dumpPath, mappingPath) + Reconciler.Diagnostics(result);
File.WriteAllText(Path.Combine(outDir, "stats.md"), stats);

foreach (var layer in new[] { Layers.Highways, Layers.Rail, Layers.Roads, Layers.Power, Layers.Skip, Layers.Unmapped })
    Console.WriteLine($"{layer,-9} edges {result.Stats.EdgesPerLayer.GetValueOrDefault(layer),5}  nodes {result.Stats.NodesPerLayer.GetValueOrDefault(layer),5}  border {result.Stats.BorderNodesPerLayer.GetValueOrDefault(layer),3}");
var unmapped = result.Maps.Values.Where(m => m.Layer == Layers.Unmapped).ToList();
Console.WriteLine(unmapped.Count == 0 ? "unmapped prefabs: none" : $"UNMAPPED prefabs ({unmapped.Count}): " + string.Join("; ", unmapped.Select(m => m.Cs1Prefab)));
Console.WriteLine($"hairpins {result.Stats.HairpinsFound}, duplicates dropped {result.Stats.DuplicatesDropped}, ramps smoothed {result.Stats.RampsSmoothed} ({result.Stats.RampNodesRaised} nodes, {result.Stats.RampsUnresolved} unresolved), overlaps {result.Overlaps.Count}, acute junctions {result.AcuteJunctions.Count}");

livePath ??= Path.Combine(outDir, "live-graph.json");
var liveLoaded = File.Exists(livePath) ? LiveGraph.Load(livePath) : null;
if (liveLoaded is { Segments.Count: 0 }) Console.WriteLine($"live graph {livePath} is empty: reconcile and dead-end reports skipped");
if (liveLoaded is { Segments.Count: > 0 })
{
    var live = liveLoaded;
    var md = new System.Text.StringBuilder();
    md.AppendLine("# Reconciliation with the live game").AppendLine();
    md.AppendLine($"Live graph `{livePath}`: {live.Segments.Count} segments, {live.Nodes.Count} nodes. Coverage: 5 points per import edge (t = 0.1 … 0.9), each within 6 m of a live segment (chord + sagitta allowance, or the import curve when both ends coincide).").AppendLine();
    md.AppendLine("Statuses: covered = all 5 points on a live segment of the mapped prefab; wrong prefab = all 5 on some network; partial = 1–4; missing = none. Retry = missing, or partial with at most 2 points.").AppendLine();
    baselineDir ??= Path.Combine(outDir, "round1");
    if (Directory.Exists(baselineDir))
    {
        var before = Layers.Imported.SelectMany(l => File.Exists(Path.Combine(baselineDir, $"import-{l}.json")) ? Reconciler.ReadImport(Path.Combine(baselineDir, $"import-{l}.json"), l, out _) : []).ToList();
        md.Append(Reconciler.Table($"Before fixes (`{baselineDir}`, the files that were imported)", Reconciler.Run(live, before)));
    }
    var now = Layers.Imported.SelectMany(l => Reconciler.ReadImport(Path.Combine(outDir, $"import-{l}.json"), l, out _)).ToList();
    var cov = Reconciler.Run(live, now);
    md.Append(Reconciler.Table("After fixes (current import-*.json)", cov));
    md.Append(Reconciler.NotCovered(cov));
    md.AppendLine("### Retry files").AppendLine();
    if (!retry) md.AppendLine("Not written (pass --retry; entity ids are only valid for the running game session).");
    foreach (var l in retry ? Layers.Imported : [])
    {
        var ids = cov.Where(c => c.Edge.Layer == l && Reconciler.NeedsRetry(c)).Select(c => c.Edge.Id).ToHashSet();
        var prefabs = result.Layer(l).Select(e => e.Map.Cs2Prefab!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var (n, bound, fresh) = Reconciler.WriteRetry(Path.Combine(outDir, $"import-{l}.json"), Path.Combine(outDir, $"import-{l}-retry.json"), ids, live, prefabs);
        md.AppendLine($"- `import-{l}-retry.json`: {n} edges, {bound} nodes bound to live entities, {fresh} new nodes");
        Console.WriteLine($"retry {l}: {n} edges, {bound} bound nodes, {fresh} new nodes");
    }
    md.AppendLine();
    md.Append(Reconciler.Diagnostics(result));
    File.WriteAllText(Path.Combine(outDir, "reconcile.md"), md.ToString());

    var liveEnds = DeadEnds.Live(result, live, 7168);
    var predicted = DeadEnds.Predicted(result, 7168);
    File.WriteAllText(Path.Combine(outDir, "deadends.md"), DeadEnds.Report(result, liveEnds, predicted,
        $"Live graph `{livePath}` ({live.Nodes.Count} nodes)."));
    Console.WriteLine($"dead ends: live {liveEnds.Count}, predicted {predicted.Count} ({predicted.Count(d => d.Reason == "CS1 dead end (genuine)")} genuine CS1 ends)");
    foreach (var g in liveEnds.GroupBy(d => d.Reason).OrderByDescending(g => g.Count())) Console.WriteLine($"  live  {g.Count(),3}  {g.Key}");
    foreach (var g in predicted.GroupBy(d => d.Reason).OrderByDescending(g => g.Count())) Console.WriteLine($"  pred  {g.Count(),3}  {g.Key}");
}
if (retryReport != null)
{
    // Failed edges by CS1 segment, so courses that were re-split (deck fit) are retried as their new pieces.
    using var rep = System.Text.Json.JsonDocument.Parse(File.ReadAllText(retryReport));
    var failedSegs = rep.RootElement.GetProperty("results").EnumerateArray()
        .Select(r => System.Text.RegularExpressions.Regex.Match(r.GetProperty("id").GetString()!, @"^e(\d+)_"))
        .Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).ToHashSet();
    var edges = result.Edges.Where(e => e.Map.Imported && failedSegs.Contains(e.Cs1Segment)).ToList();
    var retryPath = Path.Combine(outDir, "import-retry.json");
    Outputs.WriteImport(retryPath, "Dakota Falls retry", edges, absolute);
    var liveForRetry = livePath != null && File.Exists(livePath) ? LiveGraph.Load(livePath) : null;
    if (liveForRetry is { Segments.Count: > 0 })
    {
        var prefabs = result.Edges.Where(e => e.Map.Imported).Select(e => e.Map.Cs2Prefab!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var (n, bound, fresh) = Reconciler.WriteRetry(retryPath, retryPath, edges.Select(e => e.Id).ToHashSet(), liveForRetry, prefabs);
        Console.WriteLine($"retry: {failedSegs.Count} failed CS1 segments -> {n} edges, {bound} nodes bound to live entities, {fresh} new");
    }
    else Console.WriteLine($"retry: {failedSegs.Count} failed CS1 segments -> {edges.Count} edges, NOT entity-bound (live graph empty or missing)");
}
if (!layersOnly) WriteLayers();
Console.WriteLine($"written to {Path.GetFullPath(outDir)}");
return 0;

void WriteLayers()
{
    var root = rootDoc.RootElement;
    var baseDir = AppContext.BaseDirectory;
    string Find(string name) => File.Exists(name) ? name : Path.Combine(baseDir, name);
    var extra = new System.Text.StringBuilder();
    extra.AppendLine().AppendLine("## Other layers").AppendLine();
    // v3: sections that failed in the exporter are null and named here.
    if (root.TryGetProperty("sectionErrors", out var se) && se.ValueKind == JsonValueKind.Object)
        foreach (var e in se.EnumerateObject())
        {
            Console.WriteLine($"EXPORT SECTION FAILED: {e.Name}: {e.Value}");
            extra.AppendLine($"- Export section **{e.Name} failed** in CS1 ({e.Value}); its outputs are missing.");
        }
    extra.AppendLine($"- Power: {result.Layer(Layers.Power).Count(e => e.Map.Cs2Prefab != null)} courses of High-voltage Line in import-power.json (nodes on the ground).");

    // Trees: exact points when the exporter wrote <base>-trees.json, else the rasters.
    var tdir = treesDir ?? Path.Combine(outDir, "trees");
    var pointsPath = Path.Combine(Path.GetDirectoryName(dumpPath!)!, Path.GetFileNameWithoutExtension(dumpPath!) + "-trees.json");
    var treeMap = TargetMapping.Load(Find("tree-mapping.tsv"));
    var trees = File.Exists(pointsPath) ? Trees.FromPoints(pointsPath, treeMap, 7168, treeSeed) : Trees.FromRasters(root.GetProperty("trees"), treeMap, 7168, treeSeed);
    {
        // Audit finding 2: no trees in water or on ground nets (the game would hide them under nets, leave them standing in water).
        var corridor = new Dictionary<(long, long), List<(double X, double Z, double R)>>();
        foreach (var e in result.Edges.Where(e => e.Map.Imported || e.Layer == Layers.Walls))
        {
            int n = Math.Max(2, (int)Math.Ceiling(e.Curve.LengthXZ() / 2));
            for (int i = 0; i <= n; i++)
            {
                double t = i / (double)n;
                // Bridge or tunnel here (walls always count as ground: their forced elevation differs per scale, the trees do not).
                if (e.Layer != Layers.Walls && Math.Abs(e.A.Elevation + (e.B.Elevation - e.A.Elevation) * t) > 2) continue;
                var pt = e.Curve.Eval(t);
                var key = ((long)Math.Floor(pt.X / 16), (long)Math.Floor(pt.Z / 16));
                if (!corridor.TryGetValue(key, out var l)) corridor[key] = l = new();
                l.Add((pt.X, pt.Z, e.Info.HalfWidth + 2));
            }
        }
        bool OnNet(double x, double z)
        {
            long cx = (long)Math.Floor(x / 16), cz = (long)Math.Floor(z / 16);
            for (long a = -2; a <= 2; a++) for (long b = -2; b <= 2; b++)
                    if (corridor.TryGetValue((cx + a, cz + b), out var l))
                        foreach (var (px, pz, r) in l) if ((px - x) * (px - x) + (pz - z) * (pz - z) <= r * r) return true;
            return false;
        }
        var masked = Trees.Mask(trees,
            ("terrain below sea level", (x, z) => cs1Terrain != null && cs1Terrain.Sample(x, z) < seaLevel),
            ("inside the CS1 water surface", (x, z) => surfaceEarly != null && surfaceEarly.Cells.TryGetValue(surfaceEarly.Grid.Index(x, z), out var c) && c.Depth > 0.05),
            (NetCorridorRule, OnNet));
        // EXPERIMENT: the net-corridor trees in their own file, so the game's behaviour (hidden under nets, or fine at the
        // verge) can be tried and undone alone.
        var expFiles = Trees.WriteChunks(tdir, trees.Removed[NetCorridorRule], 50000, "experiment-trees-net-corridor", "trees-net-corridor-experiment");
        Console.WriteLine($"tree experiment: {trees.Removed[NetCorridorRule].Count:N0} net-corridor trees -> {string.Join(", ", expFiles)}");
        auditLog.AppendLine($"- EXPERIMENT trees: the {trees.Removed[NetCorridorRule].Count:N0} net-corridor trees are in {string.Join(", ", expFiles)} (import after the main tree files to try them; undo by its name).");
        Console.WriteLine($"tree mask: {string.Join(", ", masked.Select(m => $"{m.Key} {m.Value:N0}"))}");
        auditLog.AppendLine($"- Trees masked (same set for both scales): {string.Join("; ", masked.Select(m => $"{m.Key} {m.Value:N0}"))}; total {masked.Values.Sum():N0}, {trees.Objects.Count:N0} remain.");
    }
    // FixedHeight trees carry y, so they differ per scale: their own file in each output folder.
    var fixedTrees = trees.Objects.Where(t => t.Y != null).ToList();
    trees.Objects.RemoveAll(t => t.Y != null);
    var fixedFiles = Trees.WriteChunks(outDir, fixedTrees, 50000, "import-trees-fixed", "trees-fixed", heightFactor);
    if (fixedTrees.Count > 0) Console.WriteLine($"trees fixed height: {fixedTrees.Count} -> {string.Join(", ", fixedFiles)} (with y × {heightFactor})");
    var files = Trees.WriteChunks(tdir, trees, 50000);
    var treeReadme = Trees.Readme(trees, files, 50000);
    if (File.Exists(pointsPath))
    {
        // Compare with the raster-based set the earlier runs produced.
        var raster = Trees.FromRasters(root.GetProperty("trees"), treeMap, 7168, treeSeed);
        var cmp = new System.Text.StringBuilder();
        cmp.AppendLine().AppendLine("## Exact points vs rasters").AppendLine();
        cmp.AppendLine($"Exact: {trees.Objects.Count:N0} placed (outside {trees.Outside:N0}). Raster-based: {raster.Objects.Count:N0} placed (outside {raster.Outside:N0}). Difference {trees.Objects.Count - raster.Objects.Count:+#,0;-#,0;0}.").AppendLine();
        cmp.AppendLine("| CS2 prefab | exact | rasters |").AppendLine("|---|---:|---:|");
        foreach (var k in trees.Cs2Total.Keys.Union(raster.Cs2Total.Keys).OrderByDescending(k => trees.Cs2Total.GetValueOrDefault(k)))
            cmp.AppendLine($"| {k} | {trees.Cs2Total.GetValueOrDefault(k):N0} | {raster.Cs2Total.GetValueOrDefault(k):N0} |");
        treeReadme += cmp.ToString();
        Console.WriteLine($"trees: exact {trees.Objects.Count:N0} vs rasters {raster.Objects.Count:N0}");
    }
    File.WriteAllText(Path.Combine(tdir, "README.md"), treeReadme);
    Console.WriteLine($"trees: {trees.Objects.Count:N0} in {files.Count} files ({trees.Source}); outside {trees.Outside:N0}, relaxed spacing {trees.SpacingRelaxed:N0}, crop cells {trees.CropCells.Count:N0}");
    extra.AppendLine($"- Trees: {trees.Objects.Count:N0} objects in {files.Count} chunks under {tdir} ({trees.Source}); outside ±7168 m {trees.Outside:N0}; dropped {trees.Dropped.Values.Sum():N0} (see trees/README.md).");

    // Buildings, props, decorations.
    Func<double, double, double>? terr = cs1Terrain == null ? null : cs1Terrain.Sample;
    var objs = Objects.Build(root, TargetMapping.Load(Find("object-mapping.tsv")), terr, heightFactor, 7168, treeSeed + 1);
    Objects.Write(Path.Combine(outDir, "import-objects.json"), "objects", objs.Objects);
    Console.WriteLine($"objects: {objs.Targeted} placed of {objs.Total} (skip {objs.Skipped}, manual {objs.ManualCount}, unmapped {objs.Unmapped}, outside {objs.Outside}, with y {objs.WithY})");
    extra.AppendLine($"- Objects: {objs.Targeted} of {objs.Total} CS1 buildings/props got a CS2 target ({100.0 * objs.Targeted / Math.Max(1, objs.Total):F1} %); SKIP {objs.Skipped}, MANUAL {objs.ManualCount}, unmapped {objs.Unmapped}, outside ±7168 m {objs.Outside}; {objs.WithY} carry y.");
    extra.AppendLine().AppendLine("| CS1 prefab | count | decision |").AppendLine("|---|---:|---|");
    foreach (var (k, v) in objs.PerCs1.OrderByDescending(kv => kv.Value.Count)) extra.AppendLine($"| {k} | {v.Count} | {v.Decision} |");
    extra.AppendLine();

    // Outside connections.
    var conns = Connections.Build(result, 7168);
    Connections.Write(Path.Combine(outDir, "import-connections.json"), conns);
    File.WriteAllText(Path.Combine(outDir, "connections-check.md"), Connections.Check(conns));
    Console.WriteLine($"connections: {conns.Objects.Count} objects ({string.Join(", ", conns.Objects.GroupBy(o => o.Net).Select(g => $"{g.Key} {g.Count()}"))}); border nodes {conns.BorderNodes.Count}, matched {conns.BorderNodes.Count(b => b.Cs1 != null)}");
    extra.AppendLine($"- Outside connections: {conns.Objects.Count} objects ({string.Join(", ", conns.Objects.GroupBy(o => o.Net).Select(g => $"{g.Key} {g.Count()}"))}); border nodes {conns.BorderNodes.Count}, {conns.BorderNodes.Count(b => b.Cs1 != null)} with a CS1 connection (connections-check.md).");

    // Manual list.
    var man = new System.Text.StringBuilder();
    man.AppendLine("# Objects to place by hand").AppendLine();
    man.AppendLine($"Positions in CS2 metres (y = CS1 y × {heightFactor}); rotation = yaw degrees, 0 = +z, clockwise from above (sign unverified).").AppendLine();
    man.AppendLine("| CS1 prefab | source | x | y | z | rotation | why |").AppendLine("|---|---|---:|---:|---:|---:|---|");
    foreach (var m in objs.Manual) man.AppendLine(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"| {Json.Short(m.Cs1)} | {m.Source} | {m.X:F1} | {m.Y:F1} | {m.Z:F1} | {m.Rotation:F1} | {m.Note} |"));
    {
        var mono = dump.Segments.Where(s => s.Prefab.Contains("Monorail")).ToList();
        if (mono.Count > 0)
        {
            var pts = mono.SelectMany(s => new[] { s.Bezier.A, s.Bezier.D }).ToList();
            man.AppendLine().AppendLine("## Monorail arch bridge below the dam (decorative)").AppendLine();
            man.AppendLine(string.Create(System.Globalization.CultureInfo.InvariantCulture,
                $"{mono.Count} CS1 monorail segments ({mono.Sum(s => s.Length) / 1000:F2} km) form the white multi-arch \"vanilla asset bridge\": x {pts.Min(p => p.X):F0}…{pts.Max(p => p.X):F0}, z {pts.Min(p => p.Z):F0}…{pts.Max(p => p.Z):F0}, y {pts.Min(p => p.Y) / dump.HeightFactor:F0}…{pts.Max(p => p.Y) / dump.HeightFactor:F0} m (CS1; × {heightFactor} here)."));
            man.AppendLine().AppendLine("They are written as a decorative network in import-decor-monorail.json (Arc Pedestrian Bridge, exact elevations). For a closer look in CS2, build the crossing by hand as **Tied Arch Bridge - 4 lanes** or **Truss Arch Bridge - 4 lanes** (Bridges & Ports) along the same line, or place several **Arc Pedestrian Bridge** spans.");
        }
    }
    man.AppendLine().AppendLine("## Air and ship connections").AppendLine();
    man.AppendLine("Emitted as objects in import-connections.json at the CS1 position clamped to the border; place by hand if the bridge cannot.").AppendLine();
    man.AppendLine("| id | prefab | x | z | rotation | note |").AppendLine("|---|---|---:|---:|---:|---|");
    foreach (var o in conns.Objects.Where(o => o.Net is "air" or "ship")) man.AppendLine(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"| {o.Id} | {o.Prefab} | {o.X:F0} | {o.Z:F0} | {o.Rotation:F0} | {o.Note} |"));
    // Procedural Objects (v3 <base>-po.json).
    var po = ProceduralObjects.Load(dumpPath!, root);
    if (root.TryGetProperty("proceduralObjectsReason", out var poWhy) && poWhy.ValueKind == JsonValueKind.String)
        extra.AppendLine($"- Procedural Objects: none exported ({poWhy.GetString()}).");
    if (po != null)
    {
        int classified = ProceduralObjects.Classify(po, poOfflinePath);
        if (classified > 0) Console.WriteLine($"procedural objects: {classified}/{po.Count} classified from {poOfflinePath}");
        man.Append(ProceduralObjects.Manual(po, heightFactor));
        man.Append(ProceduralObjects.FallsFoam(po, heightFactor));
        var cand = ProceduralObjects.Candidates(po, TargetMapping.Load(Find("object-mapping.tsv")), heightFactor, 7168, treeSeed + 2, structuresOnly: classified > 0);
        Objects.Write(Path.Combine(outDir, "import-po-candidates.json"), "po-candidates-UNVERIFIED", cand);
        Console.WriteLine($"procedural objects: {po.Count} listed, {cand.Count} candidates (UNVERIFIED) in import-po-candidates.json");
        extra.AppendLine($"- Procedural Objects: {po.Count} listed in manual-objects.md; {cand.Count} map through object-mapping.tsv into import-po-candidates.json (UNVERIFIED: scale is not applied, PO meshes are not CS2 assets).");
    }
    if (Outputs.SkinFlags.Count > 0)
    {
        var segById = dump.Segments.ToDictionary(s => s.Id);
        man.AppendLine().AppendLine("## Network Skins (CS1): segments without bridge pillars or street lights").AppendLine();
        man.AppendLine("The import edges of these CS1 segments carry \"noPillars\" / \"noStreetLights\" (ignored by the bridge today). In CS2, remove pillars or lights by hand (or with a road-upgrade mod) along them.").AppendLine();
        man.AppendLine("| CS1 segment | prefab | x | z | no pillars | no street lights |").AppendLine("|---:|---|---:|---:|---|---|");
        foreach (var (id, fl) in Outputs.SkinFlags.OrderBy(k => k.Key))
            if (segById.TryGetValue(id, out var sg))
            {
                var mp = sg.Bezier.Eval(0.5);
                man.AppendLine(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"| {id} | {Json.Short(sg.Prefab)} | {mp.X:F0} | {mp.Z:F0} | {(fl.Pillars ? "yes" : "")} | {(fl.Lights ? "yes" : "")} |"));
            }
    }
    File.WriteAllText(Path.Combine(outDir, "manual-objects.md"), man.ToString());

    // Water surface (v3): check the sources against it and refine their heights.
    Func<double, double, double>? cs1Terr = cs1Terrain == null ? null : cs1Terrain.Sample;
    var surface = surfaceEarly;
    if (surface != null)
    {
        Console.WriteLine($"water surface: {surface.Cells.Count:N0} wet cells (water-check.md)");
        extra.AppendLine($"- Water surface: {surface.Cells.Count:N0} wet cells ({surface.Source}); source levels refined in import-water.json (water-check.md).");
    }

    // Terrain surfaces (v3, may be null).
    var surfaces = TerrainSurfaces.Load(root);
    if (surfaces != null)
    {
        File.WriteAllText(Path.Combine(outDir, "surfaces-summary.md"), surfaces.Summary(7168));
        surfaces.WriteGeoJson(Path.Combine(outDir, "surfaces.geojson"), 4);
        Console.WriteLine($"terrain surfaces: {surfaces.Cells.Count:N0} cells in {surfaces.Cells.Values.Distinct().Count()} classes -> surfaces-summary.md, surfaces.geojson");
        extra.AppendLine($"- Terrain surfaces: {surfaces.Cells.Count:N0} cells, {surfaces.Cells.Values.Distinct().Count()} classes (surfaces-summary.md, surfaces.geojson).");
    }

    // Barrier networks have no CS2 network twin: CS1 segments become barrier props every BarrierSpacing metres along the curve.
    const double BarrierSpacing = 3; // CS2 piece length not verified
    var barrierProps = new List<PlacedObject>();
    var barrierRx = new System.Text.RegularExpressions.Regex("(?i)(Concrete|Steel) Barrier");
    foreach (var s in dump.Segments.Where(s => barrierRx.IsMatch(s.Prefab)))
    {
        string prefab = s.Prefab.Contains("Steel", StringComparison.OrdinalIgnoreCase) ? "SafetyBarrier01" : "ConcreteBarrier01";
        var b = s.Bezier; double len = b.LengthXZ();
        int n = Math.Max(1, (int)Math.Round(len / BarrierSpacing));
        for (int k = 0; k < n; k++)
        {
            double t = (k + 0.5) / n;
            var pnt = b.Eval(t); var d = b.Eval(Math.Min(1, t + 0.01)) - b.Eval(Math.Max(0, t - 0.01));
            if (Math.Abs(pnt.X) > 7168 || Math.Abs(pnt.Z) > 7168) continue;
            double yCs1 = pnt.Y / dump.HeightFactor;   // the dump is scaled to the topology factor
            double? y = cs1Terrain != null && Math.Abs(yCs1 - cs1Terrain.Sample(pnt.X, pnt.Z)) > 2 ? Json.R2(yCs1 * heightFactor) : null;
            barrierProps.Add(new PlacedObject($"bar{s.Id}_{k}", prefab, "prop", pnt.X, pnt.Z, y, Connections.Heading(d.X, d.Z), s.Prefab, $"segment {s.Id}"));
        }
    }
    if (barrierProps.Count > 0) Objects.Write(Path.Combine(outDir, "import-walls-props.json"), "walls-props", barrierProps);
    Console.WriteLine($"walls props: {barrierProps.Count} barrier pieces every {BarrierSpacing} m");
    extra.AppendLine($"- Barrier props (import-walls-props.json): {barrierProps.Count} ConcreteBarrier01 / SafetyBarrier01 pieces every {BarrierSpacing} m along {dump.Segments.Count(s => barrierRx.IsMatch(s.Prefab))} CS1 barrier segments (piece length unverified).");

    // Surfaces: field networks -> Agriculture Surface 01, Landscape Grass clusters -> Grass Surface 01, v3 terrainSurfaces.
    var fine = Surfaces.FineGrid(4);
    var fieldCells = new HashSet<int>();   // open arcs only
    var circles = Surfaces.FieldCircles(dump, new System.Text.RegularExpressions.Regex("(?i)Field .*Network"), fine, fieldCells);
    int shrunk = Surfaces.SeparateCircles(circles);
    auditLog.AppendLine($"- Field circles: {shrunk} overlapping pairs shrunk to touch (0.5 m gap) instead of a union, so they stay circles.");
    var fields = circles.Select((c, k) => new SurfacePolygon($"field{k + 1}", "Agriculture Surface 01", c.Ring, string.Create(System.Globalization.CultureInfo.InvariantCulture, $"centre-pivot field, centre ({c.Cx:F0}, {c.Cz:F0}), radius {c.R:F0} m")))
        .Concat(Surfaces.Polygonize(fieldCells, fine).Select((r, k) => new SurfacePolygon($"fieldarc{k + 1}", "Agriculture Surface 01", r, "open field-network arc (swept footprint)"))).ToList();
    if (fields.Count > 0) Surfaces.Write(Path.Combine(outDir, "import-surfaces-fields.json"), "surfaces-fields", fields, "CS1 centre-pivot fields: one circle per closed field-network ring (radius = ring radius + network half-width), 48 points; open arcs swept on a 4 m grid. Circles may overlap their neighbours slightly.");
    Func<double, double, bool>? fieldMask = fields.Count == 0 ? null : (x, z) =>
    {
        foreach (var c in circles) if ((x - c.Cx) * (x - c.Cx) + (z - c.Cz) * (z - c.Cz) <= c.R * c.R) return true;
        int i = fine.Index(x, z); return i >= 0 && fieldCells.Contains(i);
    };
    Console.WriteLine(Surfaces.Stats("fields", fields));
    Outputs.WritePolygonCheck(Path.Combine(outDir, "fields-check.png"), result, fields.Select(f => f.Ring), 4096, 3.5);
    var round = fields.Select(f => Outputs.Roundness(f.Ring)).ToList();
    Console.WriteLine($"fields: roundness ≥ 0.85 (circle-like) {round.Count(q => q >= 0.85)}, 0.6–0.85 {round.Count(q => q >= 0.6 && q < 0.85)}, < 0.6 (blobs/strips) {round.Count(q => q < 0.6)} -> fields-check.png");
    extra.AppendLine($"- {Surfaces.Stats("Field surfaces (import-surfaces-fields.json)", fields)}; footprints also raise fertility to ≥ {Resources.FieldFertility}.");

    var tufts = root.TryGetProperty("props", out var pr) && pr.TryGetProperty("points", out var pp)
        ? pp.EnumerateArray().Where(t => (t.GetProperty("prefab").GetString() ?? "").Contains("Landscape Grass"))
            .Select(t => { var a = t.GetProperty("position").EnumerateArray().Select(v => v.GetDouble()).ToArray(); return (a[0], a[2]); }).ToList()
        : new List<(double, double)>();
    var grass = Surfaces.GrassClusters(tufts).Select((r, k) => new SurfacePolygon($"grass{k + 1}", "Grass Surface 01", r, "cluster of CS1 Landscape Grass props")).ToList();
    {
        // EXPERIMENT: the unmerged clusters too, to compare how overlaps render.
        Surfaces.Write(Path.Combine(outDir, "import-surfaces-grass-unmerged.json"), "surfaces-grass-unmerged-experiment", grass, "EXPERIMENT: grass clusters before the overlap union (overlapping polygons kept as they are).");
        var (u, pairs, groups) = Surfaces.UnionOverlapping(grass, 1);
        auditLog.AppendLine($"- Grass surfaces: {pairs} overlapping pairs merged in {groups} groups ({grass.Count} → {u.Count}).");
        grass = u;
    }
    if (grass.Count > 0) Surfaces.Write(Path.Combine(outDir, "import-surfaces-grass.json"), "surfaces-grass", grass, "OPTIONAL: Landscape Grass props clustered (DBSCAN 12 m, >= 5 tufts), convex hull padded 3 m, clipped to ±7168.");
    Console.WriteLine(Surfaces.Stats($"grass ({tufts.Count} tufts)", grass));
    extra.AppendLine($"- {Surfaces.Stats($"Grass surfaces from {tufts.Count} Landscape Grass props (import-surfaces-grass.json, optional)", grass)}.");

    // Steep rock (audit/theme.md: CS1 true-height slope >= 40°): same polygons for both scales, from the 1:1 heightmap.
    var rockHm = rockHeightmapPath ?? heightmapPath;
    if (rockHm != null && File.Exists(rockHm))
    {
        var hmTrue = Heightmap.LoadPng(rockHm);   // 2048 m scale = true CS1 heights
        double err = hmTrue.Orient(dump.Nodes.Values.Select(n => (n.Position.X, n.Position.Z, n.Terrain / dump.HeightFactor)));
        var rock = Surfaces.SteepRock(hmTrue, rockDegrees);
        var rocks = rock.Polygons.Select((r, k) => new SurfacePolygon($"rock{k + 1}", RockSurface, r, $"slope >= {rockDegrees}°")).ToList();
        Surfaces.Write(Path.Combine(outDir, "import-surfaces-rock.json"), "surfaces-rock", rocks,
            $"Automatic rock surface: pixels of {Path.GetFileName(rockHm)} (true heights, 3.5 m) with slope >= {rockDegrees}°, patches >= 400 m², hole-free, simplified to 4 m, largest {rocks.Count} kept. Prefab {RockSurface} (Terrain Textures Pack - Rocks, brown scree).");
        string msg = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"rock: {Path.GetFileName(rockHm)} (|err| vs CS1 nodes {err:F2} m), steep area {rock.SteepArea / 1e6:F2} km² in {rock.Patches} patches >= 400 m²; {rocks.Count} polygons kept ({rock.KeptArea / 1e6:F2} km²), {rock.Dropped} smallest dropped");
        Console.WriteLine(msg);
        extra.AppendLine($"- Rock surfaces (import-surfaces-rock.json): {msg.Substring(6)}.");
    }

    // R69 Big Decal Dirt/Mud: no decal without EAI, so each becomes a 16 × 16 m Dirty Surfaces Pack square at the CS1 position and angle.
    const double DecalSize = 16; // CS1 "Big Decal" footprint not in the dump
    var decals = new List<SurfacePolygon>();
    if (root.TryGetProperty("props", out var dp) && dp.TryGetProperty("points", out var dpts))
        foreach (var d in dpts.EnumerateArray())
        {
            string prefab = d.GetProperty("prefab").GetString() ?? "";
            if (!prefab.Contains("Decal", StringComparison.OrdinalIgnoreCase)) continue;
            var pos = d.GetProperty("position").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            if (Math.Abs(pos[0]) > 7168 || Math.Abs(pos[2]) > 7168) continue;
            double yaw = Objects.Yaw(d.TryGetProperty("angle", out var ang) ? ang.GetDouble() : 0) * Math.PI / 180;
            var (fx, fz) = (Math.Sin(yaw), Math.Cos(yaw));      // forward (0 = +z, clockwise from above)
            var (rx, rz) = (Math.Cos(yaw), -Math.Sin(yaw));     // right
            double h = DecalSize / 2;
            var ring = new[] { (1, 1), (1, -1), (-1, -1), (-1, 1) }.Select(c => (pos[0] + h * (c.Item1 * fx + c.Item2 * rx), pos[2] + h * (c.Item1 * fz + c.Item2 * rz))).ToList();
            string target = prefab.Contains("Mud", StringComparison.OrdinalIgnoreCase) ? "PDO23 Soil Surface 01" : "PDO23 Dirty Surface 01";
            decals.Add(new SurfacePolygon($"decal{d.GetProperty("id").GetInt32()}", target, ring, $"CS1 {Json.Short(prefab)}, {DecalSize} m square"));
        }
    {
        // EXPERIMENT: the 128 separate 16 m squares too, to compare with the merged shapes.
        Surfaces.Write(Path.Combine(outDir, "import-surfaces-decals-unmerged.json"), "surfaces-decals-unmerged-experiment", decals, "EXPERIMENT: one 16 x 16 m square per CS1 decal, overlaps kept (z-fighting and OverlapExisting possible).");
        var (u, pairs, groups) = Surfaces.UnionOverlapping(decals, 1);
        auditLog.AppendLine($"- Decal surfaces: {pairs} overlapping pairs merged in {groups} groups ({decals.Count} → {u.Count}).");
        decals = u;
    }
    if (decals.Count > 0) Surfaces.Write(Path.Combine(outDir, "import-surfaces-decals.json"), "surfaces-decals", decals, "CS1 R69 Big Decal Dirt/Mud as Dirty Surfaces Pack (148077) area surfaces: 16 x 16 m squares at the CS1 position and angle (footprint assumed). Dirt -> PDO23 Dirty Surface 01, Mud -> PDO23 Soil Surface 01.");
    Console.WriteLine(Surfaces.Stats("decal surfaces", decals));
    extra.AppendLine($"- {Surfaces.Stats("Decal surfaces (import-surfaces-decals.json; PDO23 Dirty Surface 01 / PDO23 Soil Surface 01)", decals)}.");

    if (surfaces != null)
    {
        var ts = Surfaces.FromTerrainSurfaces(surfaces, TargetMapping.Load(Find("surface-mapping.tsv")), out var tsDecisions);
        // One file per CS1 class (e.g. import-surfaces-sand.json), already unioned per class (rasterised), hole-free, simplified.
        File.Delete(Path.Combine(outDir, "import-surfaces.json"));
        foreach (var g in ts.GroupBy(t => t.Note.Replace("CS1 surface ", "").ToLowerInvariant()))
            Surfaces.Write(Path.Combine(outDir, $"import-surfaces-{g.Key}.json"), $"surfaces-{g.Key}", g.ToList(), $"CS1 terrainSurfaces class {g.Key} mapped through surface-mapping.tsv (audit/theme.md table): rasterised per class on the 16 m grid, union, hole-free pieces, simplified to 2 m.");
        Console.WriteLine(Surfaces.Stats("terrain surfaces", ts) + "; " + string.Join(", ", tsDecisions.Select(d => $"{d.Key} -> {d.Value}")));
        extra.AppendLine($"- {Surfaces.Stats("Terrain surfaces (import-surfaces.json)", ts)}: {string.Join("; ", tsDecisions.Select(d => $"{d.Key} → {d.Value}"))}.");
    }

    // Natural resources.
    if (Resources.WriteFromDump(Path.Combine(outDir, "import-resources.json"), root, fieldMask))
    {
        File.Delete(Path.Combine(outDir, "import-resources-synthetic.json"));
        Console.WriteLine("resources: import-resources.json from the dump's naturalResources");
        extra.AppendLine("- Resources: import-resources.json from the dump's naturalResources.");
    }
    else
    {
        var fert = Resources.SyntheticFertility(trees, fieldMask);
        Resources.WriteSynthetic(Path.Combine(outDir, "import-resources-synthetic.json"), fert);
        Console.WriteLine($"resources: no naturalResources in the dump; SYNTHETIC fertility from corn, {fert.Count(v => v > 0)} non-zero cells of 512²");
        extra.AppendLine($"- Resources: SYNTHETIC fertility from Corn Stalk density ({fert.Count(v => v > 0)} non-zero cells of 512²) in import-resources-synthetic.json; the dump has no naturalResources yet.");
    }
    // Decision table for the classes the owner asked about.
    extra.AppendLine().AppendLine("## Skip review (rounds 10-12)").AppendLine();
    extra.AppendLine("| class | CS1 source | count | mapped | kept skipped | decision |").AppendLine("|---|---|---:|---:|---:|---|");
    void SegRow(string cls, string regex, string decision)
    {
        var rx = new System.Text.RegularExpressions.Regex(regex);
        var segs = dump.Segments.Where(s => rx.IsMatch(s.Prefab)).ToList();
        int mapped = segs.Count(s => result.Fate.GetValueOrDefault(s.Id, "").StartsWith("imported") || (Layers.Separate.Contains(result.Maps.GetValueOrDefault(s.Prefab)?.Layer ?? "") && result.Maps[s.Prefab].Cs2Prefab != null && result.Fate.GetValueOrDefault(s.Id) != "outside the border"));
        extra.AppendLine($"| {cls} | network segments | {segs.Count} | {mapped} | {segs.Count - mapped} | {decision} |");
    }
    void ObjRow(string cls, string regex, string source, string decision)
    {
        var rx = new System.Text.RegularExpressions.Regex(regex);
        var rows = objs.PerCs1.Where(kv => rx.IsMatch(kv.Key)).ToList();
        int n = rows.Sum(kv => kv.Value.Count), skipped = rows.Where(kv => kv.Value.Decision.StartsWith("SKIP") || kv.Value.Decision.StartsWith("MANUAL")).Sum(kv => kv.Value.Count);
        extra.AppendLine($"| {cls} | {source} | {n} | {n - skipped} | {skipped} | {decision} |");
    }
    {
        var fs = dump.Segments.Where(s => System.Text.RegularExpressions.Regex.IsMatch(s.Prefab, "(?i)Field .*Network")).ToList();
        int inside = fs.Count(s => Clipper.InsideIntervals(s.Bezier, 7168).Count > 0);
        extra.AppendLine($"| Field networks | network segments | {fs.Count} | {inside} as surfaces | {fs.Count - inside} (outside ±7168) | → {fields.Count} Agriculture Surface 01 polygons (import-surfaces-fields.json) + fertility; not imported as networks |");
    }
    ObjRow("Landscape Grass", "(?i)Landscape Grass", "props", $"→ 1:1 pack grass plants (Styx tufts, ferns, pd Long Grass, undergrowth, flowers; kind plant, age adult) in import-objects.json; {grass.Count} Grass Surface 01 clusters remain as the optional alternative");
    ObjRow("Rocks (Surface Rock / Modular C / Breakwater)", "(?i)Surface Rock|Modular C|Breakwater", "buildings", "→ pack rocks (Arid, RZ, Cliffs and Rocks) mixed with vanilla Rock01-30, per class in object-mapping.tsv");
    extra.AppendLine($"| Decals (R69 dirt/mud) | props | {objs.PerCs1.Where(kv => kv.Key.Contains("Decal")).Sum(kv => kv.Value.Count)} | {decals.Count} as surfaces | {objs.PerCs1.Where(kv => kv.Key.Contains("Decal")).Sum(kv => kv.Value.Count) - decals.Count} (outside ±7168) | → 16 m squares of PDO23 Dirty Surface 01 (dirt) / PDO23 Soil Surface 01 (mud), import-surfaces-decals.json |");
    SegRow("Pedestrian Gravel / Elevated", "^Pedestrian (Gravel|Elevated)", "→ Pavement Path (roads layer, elevations kept); CS2 has no gravel path");
    SegRow("Quay", "^Quay", "→ QuayMedium01 (walls layer, import-walls.json)");
    SegRow("Canal", "^Canal", "→ Medium Boatway (walls layer); CS2 boatways are canals");
    SegRow("Flood Wall", "Flood Wall", "→ Sound Barrier (walls layer), the only wall network by name");
    SegRow("Retaining Walls AiO", "Retaining Wall", "→ QuaySmall01 (walls layer); no retaining-wall network, quay wall is the nearest (unverified inland)");
    extra.AppendLine($"| Concrete / Steel barriers | network segments | {dump.Segments.Count(s => barrierRx.IsMatch(s.Prefab))} | {barrierProps.Select(b => b.Source).Distinct().Count()} as props | {dump.Segments.Count(s => barrierRx.IsMatch(s.Prefab)) - barrierProps.Select(b => b.Source).Distinct().Count()} (outside ±7168) | → {barrierProps.Count} ConcreteBarrier01 / SafetyBarrier01 props every 3 m (import-walls-props.json) |");
    SegRow("Castle Wall", "Castle Wall", "NO TWIN, kept skipped: searched Assets.NAME for Wall/Castle/Rampart/Fort/Ruin (only garden walls EU_Wall*, RuinsCastle01 building, Sound Barrier)");
    SegRow("Ship paths", "^Ship (Connection )?Path", "→ Medium Seaway (import-shippaths.json), clipped to end on the border");
    SegRow("Airplane paths", "^Airplane", "kept skipped: the game routes planes");
    ObjRow("Ruins / wreck / factory", "(?i)Pier Ruins|Ship Wreck|Abandoned Factory|Castle Ruins", "buildings", "→ LeisurePier01Small01 / RubblePileWood02 / RuinsOldMill01 / RuinsCastle01 (nearest by name)");
    SegRow("Terraforming Network", "Terraforming", "kept skipped: CS1 terrain-shaping helper (terrain is in the heightmap)");
    ObjRow("Pillars", "(?i)Pillar", "buildings", "kept skipped: CS2 bridges spawn their own pillars");
    ObjRow("Modern Fence 02", "(?i)Modern Fence", "props", "→ FenceResidentialPieceLow01 (piece length unverified)");
    extra.AppendLine($"| Network Skins (v5) | segments | {Outputs.SkinFlags.Count} | {Outputs.SkinFlags.Count} flagged | 0 | note: no pillars on {Outputs.SkinFlags.Values.Count(v => v.Pillars)}, no street lights on {Outputs.SkinFlags.Values.Count(v => v.Lights)}; edge fields noPillars / noStreetLights (no bridge field yet), list in manual-objects.md |");
    extra.Append(auditLog);
    File.AppendAllText(Path.Combine(outDir, "stats.md"), extra.ToString());
}
