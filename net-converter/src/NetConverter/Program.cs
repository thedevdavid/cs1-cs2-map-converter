using System.Text.Json;
using NetConverter;

string? dumpPath = null, mappingPath = null, outDir = "out", livePath = null, baselineDir = null, heightmapPath = null, retryReport = null;
bool inventoryOnly = false, retry = false, layersOnly = false;
string? treesDir = null, rockHeightmapPath = null, cs2HeightmapPath = null, runReportsDir = Path.Combine("out", "x1");
string poOfflinePath = Path.Combine("..", "cs1-offline", "out", "po-offline.json");
double rockDegrees = 40;
List<Round9.NetNode> bridgeNodes = new(); int gapEdges = 0; string gapLog = ""; var externalNodes = new HashSet<string>();
bool seawayAtWater = false, mergeCanal = true, keepHighwayStubs = false, bridgesOn = true, bridgesFixedOn = true;
const string RockSurface = "gaga TTP - Pebbles 1L dark";
const string NetCorridorRule = "on a ground-level net corridor (half width + 2 m)";
{
    var cs2Hm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", "Heightmaps", "10190442-dktflls-1to1.png");
    if (File.Exists(cs2Hm)) rockHeightmapPath = cs2Hm;
    // The playable CS2 heightmap (hm-rebuild): object y policy, cliff flanks, power-end ground, lake checks.
    // Default: the newest <MMddHHmm>-dktflls-1to1.png (next-run/RUNBOOK.md naming; A-v3 used 10190843).
    var hmDir = Path.GetDirectoryName(cs2Hm)!;
    if (Directory.Exists(hmDir))
        cs2HeightmapPath = Directory.GetFiles(hmDir, "*-dktflls-1to1.png").Where(p => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(p), @"^\d{8}-dktflls-1to1\.png$"))
            .OrderByDescending(p => Path.GetFileName(p), StringComparer.Ordinal).FirstOrDefault();
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
        case "--cs2-heightmap": cs2HeightmapPath = args[++i]; break;
        case "--run-reports": runReportsDir = args[++i]; break;
        case "--keep-highway-stubs": keepHighwayStubs = true; break;
        case "--bridges": bridgesOn = args[++i] != "off"; break;
        case "--bridges-fixed": bridgesFixedOn = args[++i] != "off"; break;
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
            Console.WriteLine("net-converter <dump.json> [--mapping mapping.tsv] [--out out] [--inventory] [--live out/live-graph.json] [--baseline out/round1] [--ramp-delta 6] [--ramp-length 60] [--retry] [--trees-dir DIR] [--tree-seed N] [--network-only] [--heightmap cs2-4096.png] [--retry-report report.json] [--min-bridge-elevation 8] [--sea-level 97.5] [--water-variant 93.0] [--cs2-heightmap 10190615-dktflls-1to1.png] [--height-factor 2] [--topology-factor 2] [--height-mode absolute|elevation]");
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
// CS2 playable heightmap, true metres (value / 32 at the 2048 scale), 3.5 m/px.
Heightmap? cs2Terrain = null;
if (cs2HeightmapPath != null && File.Exists(cs2HeightmapPath))
{
    cs2Terrain = Heightmap.LoadPng(cs2HeightmapPath);
    double err2 = cs2Terrain.Orient(dump.Nodes.Values.Select(n => (n.Position.X, n.Position.Z, n.Terrain)));
    Console.WriteLine($"cs2 heightmap {cs2HeightmapPath}: row 0 = {(cs2Terrain.NorthUp ? "north" : "south")}, mean |CS2 − CS1 terrain| at CS1 nodes {err2:F2} m");
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
    var boat = result.Layer(Layers.Walls).Where(e => e.Map.Cs2Prefab.EndsWith("Boatway")).ToList();
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
{
    // Owner review of A-v4: one-way highway courses that end in a CS1 dead end (End flag, inside the map) stand as stubs with the
    // game's dead-end warning. The chain from such an end back to its first junction is left out (up to 1 km).
    var hw = result.Layer(Layers.Highways).ToList();
    var deg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
    foreach (var e in result.Edges.Where(e => e.Map.Imported)) foreach (var n in new[] { e.A, e.B }) deg[n] = deg.GetValueOrDefault(n) + 1;
    var gone = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance); var stubLog = new List<string>();
    foreach (var end in keepHighwayStubs ? new List<OutNode>() : deg.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList())
    {
        if (end.Border || end.Cs1Node is not { } cn || !dump.Nodes.TryGetValue(cn, out var dn) || !dn.Flags.Contains("End")) continue;
        var first = hw.FirstOrDefault(e => (e.A == end || e.B == end) && e.Map.Cs2Prefab!.StartsWith("Highway Oneway"));
        if (first == null) continue;
        var chain = new List<OutEdge>(); double len = 0; var (e0, n0) = (first, end);
        while (true)
        {
            chain.Add(e0); len += e0.Curve.LengthXZ();
            var next = e0.A == n0 ? e0.B : e0.A;
            if (deg.GetValueOrDefault(next) != 2) break;
            var o = hw.FirstOrDefault(x => x != e0 && (x.A == next || x.B == next));
            if (o == null) break;
            (e0, n0) = (o, next);
            if (len > 1000) break;
        }
        if (len > 1000) { stubLog.Add(FormattableString.Invariant($"{end.Id} ({end.Position.X:F0}, {end.Position.Z:F0}): {len:F0}+ m to the first junction, kept")); continue; }
        foreach (var c in chain) { gone.Add(c); result.Fate[c.Cs1Segment] = "left out: one-way highway stub to a CS1 dead end"; }
        stubLog.Add(FormattableString.Invariant($"{end.Id} ({end.Position.X:F0}, {end.Position.Z:F0}) {first.Map.Cs2Prefab}: {chain.Count} courses, {len:F0} m ({string.Join(", ", chain.Select(c => c.Id))})"));
    }
    result.Edges.RemoveAll(gone.Contains);
    auditLog.AppendLine($"- One-way highway stubs to CS1 dead ends left out: {gone.Count} courses.");
    foreach (var l in stubLog) auditLog.AppendLine($"  - {l}");
    Console.WriteLine($"highway stubs: {gone.Count} courses left out ({stubLog.Count} ends)");
}
{
    // Run A-v3: walls and decor that run inside a highway's footprint are dropped by the game; move them sideways.
    // Ground truth: the drop messages in the import runs (tools/import-run.mjs reports, --run-reports); without them, the geometric rule.
    var reports = Directory.Exists(runReportsDir) ? Directory.GetFiles(runReportsDir, "run-*.md").OrderBy(p => p, StringComparer.Ordinal).ToList() : new List<string>();
    var drops = reports.Count > 0 ? FootprintShift.ParseDrops(reports) : null;
    if (drops != null) auditLog.AppendLine($"- Run reports read for game drops: {string.Join(", ", reports.Select(Path.GetFileName))} ({drops.Count} dropped courses).");
    // Only the courses still written (canals, flood walls): moving a dropped quay's node would bend the road it shares.
    var fs8 = FootprintShift.Apply(result, result.Layer(Layers.Walls).Where(e => e.Map.Cs2Prefab is not ("QuaySmall01" or "QuayMedium01")), topologyFactor, drops);
    auditLog.AppendLine($"- Courses inside a highway footprint (walls, decor) moved sideways by the two CS2 half-widths + 1 m: {fs8.Shifted.Count}; impossible (between carriageways) {fs8.Impossible.Count}.");
    foreach (var l in fs8.Shifted) auditLog.AppendLine($"  - shifted: {l}");
    foreach (var l in fs8.Impossible) auditLog.AppendLine($"  - IMPOSSIBLE: {l}");
    Console.WriteLine($"footprint shift: {fs8.Shifted.Count} moved, {fs8.Impossible.Count} impossible");
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
    // Live Run A: Pavement Path dead ends beside a road / gravel road / path join it through the bridge's snapToSegment.
    var snaps = PathSnap.Apply(result, topologyFactor);
    auditLog.AppendLine($"- Pavement Path dead ends snapped onto the segment beside them (snapToSegment, ≤ 20 m, Δy ≤ 3 m): {snaps.Count}{(snaps.Count > 0 ? ": " + string.Join("; ", snaps) : "")}.");
    Console.WriteLine($"path ends: {snaps.Count} with snapToSegment");
}
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
{
    // Bridge overrides (bridges.tsv): the chosen CS1 bridge decks become CS2 bridge prefabs in import-bridges.json (right after the network
    // import); BR001 merge and the BR022 arches go to import-bridges-fixed.json (fixedIndex; off by default, --bridges-fixed on).
    string FindB(string name) => File.Exists(name) ? name : Path.Combine(AppContext.BaseDirectory, name);
    var specs = Bridges.Load(FindB("bridges.tsv"));
    var segIdsB = Bridges.LoadSegIds(FindB(Path.Combine("..", "bridges", "data", "cs1-bridges.json")));
    Func<double, double, double?>? waterOut = surfaceEarly == null ? null : (x, z) => surfaceEarly.Cells.TryGetValue(surfaceEarly.Grid.Index(x, z), out var c) && c.Depth > 0.5 ? c.Surface * heightFactor : null;
    var (bMain, bFixed, bRep) = Bridges.Apply(result, dump, rootDoc.RootElement, specs, segIdsB, bridgesOn, bridgesFixedOn, waterOut, topologyFactor);
    foreach (var name in new[] { "import-bridges.json", "import-bridges-fixed.json" }) File.Delete(Path.Combine(outDir, name));
    // BR001's deck courses outside the merged span: noPillars (PLAN.md item 4), through the skin-flag writer.
    foreach (var seg in Bridges.NoPillarSegments) Outputs.SkinFlags[seg] = (true, Outputs.SkinFlags.TryGetValue(seg, out var sf) && sf.Lights);
    var rules = Bridges.LoadRules(FindB(Path.Combine("..", "bridges", "cs2-bridge-rules.json")));
    var checks = Bridges.Validate(bMain, rules, waterOut);
    foreach (var kv in Bridges.Validate(bFixed, rules, waterOut)) checks[kv.Key] = kv.Value;
    if (bMain.Edges.Count > 0)
        Round9.WriteNet(Path.Combine(outDir, "import-bridges.json"), "Dakota Falls bridges",
            "Bridge overrides (bridges.tsv): straight, constant-grade bridge-prefab edges between the CS1 deck chains' end nodes; ends shared with the network import join it through snapToSegment (radius 2). Import right after import-all.json, which leaves these decks out. Prefab names UNVERIFIED.", bMain.Nodes, bMain.Edges, bMain.Replaces);
    if (bFixed.Edges.Count > 0)
        Round9.WriteNet(Path.Combine(outDir, "import-bridges-fixed.json"), "Dakota Falls fixed-span bridges",
            $"BR001: the two carriageways merged onto one BXP SuspensionBridge03 edge abutment to abutment (976 m; the game cuts the spans and places the towers, bridge 0.9.22), split nodes outside it; BR022: one Arc Pedestrian Bridge Revised edge per arc chain. Import right after import-bridges.json. {(bridgesFixedOn ? "import-all.json leaves out the replaced courses." : "--bridges-fixed off: import-all.json still holds the replaced courses ('replaces'); do not import this file.")}",
            bFixed.Nodes, bFixed.Edges, bFixed.Replaces);
    auditLog.AppendLine().AppendLine("## Bridges (bridges.tsv)").AppendLine();
    auditLog.AppendLine(bridgesOn ? $"{bRep.Count} bridge overrides; {bMain.Edges.Count} edges in import-bridges.json, {bFixed.Edges.Count} in import-bridges-fixed.json ({(bridgesFixedOn ? "on" : "--bridges-fixed off")}). No prefab name is verified in game yet; --bridges off keeps every CS1 bridge as the plain mapped road. Rule checks: the bridge's 0.9.22 pre-checks computed offline against ../bridges/cs2-bridge-rules.json (bridge.lowElevation and vanilla prefabs are left to the runner's dry run). noPillars: BR006 deck, BR001 connectors and {Bridges.NoPillarSegments.Count} BR001 deck segments outside the merged span." : "--bridges off: no overrides.").AppendLine();
    auditLog.AppendLine("| bridge | prefab | plan | chains | edges | lengths m | file | notes | rule checks (cs2-bridge-rules.json) |").AppendLine("|---|---|---|---:|---:|---|---|---|---|");
    foreach (var r in bRep)
        auditLog.AppendLine(FormattableString.Invariant($"| {r.Id} | {r.Prefab} | {r.Plan} | {r.Chains} | {r.Lengths.Count} | {string.Join(", ", r.Lengths.Select(l => l.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)).GroupBy(l => l).Select(g => g.Count() > 1 ? $"{g.Count()}×{g.Key}" : g.Key))} | {(r.InFixedFile ? "fixed" : "bridges")} | {r.Notes}{(r.Problems.Count > 0 ? "; " + string.Join("; ", r.Problems) : "")} | {(checks.TryGetValue(r.Id, out var ck) && ck.Count > 0 ? string.Join("; ", ck) : "ok")} |"));
    Console.WriteLine($"bridges: {bRep.Count} overrides, {bMain.Edges.Count} edges -> import-bridges.json, {bFixed.Edges.Count} -> import-bridges-fixed.json ({(bridgesFixedOn ? "on" : "off")})");
    bridgeNodes = bMain.Nodes.Concat(bFixed.Nodes).ToList();
}
{
    // Network gaps (Run A): the rebuilt courses replace their originals in import-all.json (fresh-map runs; the original and its rebuild
    // overlapped on A-v4 and tore the 4-lane / 3-lane split at (463, −4520)).
    var (gn, ge, tn, te, glog) = Round9.Gaps(result, topologyFactor, bridgeNodes, out var gapReplaces);
    Round9.WriteNet(Path.Combine(outDir, "import-gaps.json"), "Dakota Falls network gaps",
        "Rebuilt Run A gaps; the courses in 'replaces' are left out of import-all.json. Ends join the network import through snapToSegment.", gn, ge, gapReplaces);
    if (te.Count > 0) Round9.WriteNet(Path.Combine(outDir, "import-gaps-e15883-taper.json"), "e15883_0 taper alternative",
        "Alternative to gap_e15883_3lane in import-gaps.json: 12 m of 4-lane, then 3-lane. Import only one of the two.", tn, te);
    var gr = gapReplaces.ToHashSet();
    int removedG = result.Edges.RemoveAll(e => gr.Contains(e.Id));
    gapEdges = ge.Count; gapLog = $"{string.Join("; ", glog)}; {removedG} originals left out of import-all.json ({string.Join(", ", gapReplaces)})";
    auditLog.AppendLine($"- Gaps: {ge.Count} edges in import-gaps.json; originals left out of import-all.json: {string.Join(", ", gapReplaces)}.");
    foreach (var n in gn) externalNodes.Add(n.Id);
}
{
    // Road QA fixes (tools/road-qa.mjs): a dead end where CS1 continued (pipeline dead end) snaps onto the nearest course of its layer within
    // 25 m (Δy ≤ 3 m), else its stub to the first junction is left out when shorter than 40 m; an isolated piece under 100 m that CS1 connected
    // to more network is left out. Nodes that other import files join (bridges, gaps) or that already snap count as connected.
    foreach (var n in bridgeNodes) externalNodes.Add(n.Id);
    var cs1Deg = new Dictionary<int, int>();
    foreach (var sg in dump.Segments) foreach (var k in new[] { sg.StartNode, sg.EndNode }) cs1Deg[k] = cs1Deg.GetValueOrDefault(k) + 1;
    var net = result.Edges.Where(e => e.Map.Imported).ToList();
    var degQ = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
    foreach (var e in net) foreach (var n in new[] { e.A, e.B }) degQ[n] = degQ.GetValueOrDefault(n) + 1;
    bool Connected(OutNode n) => n.Snap != null || externalNodes.Contains(n.Id);
    var snapped = new List<string>(); var removedStubs = new List<string>(); var residual = new List<string>();
    var drop = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
    foreach (var (n, d) in degQ.ToList())
    {
        if (d != 1 || n.Border || Connected(n) || n.Cs1Node is not { } cn || !dump.Nodes.TryGetValue(cn, out var dn)) continue;
        if (cs1Deg.GetValueOrDefault(cn) <= 1 || dn.Flags.Contains("End")) continue;   // genuine CS1 dead end
        var own = net.First(e => e.A == n || e.B == n);
        OutEdge? best = null; double bestD = 25; Vec3 bp = default;
        foreach (var e in net)
        {
            if (e == own || e.A == n || e.B == n || e.Layer != own.Layer) continue;
            for (int k = 0; k <= 16; k++) { var p = e.Curve.Eval(k / 16.0); double dd = Math.Sqrt((p.X - n.Position.X) * (p.X - n.Position.X) + (p.Z - n.Position.Z) * (p.Z - n.Position.Z)); if (dd < bestD) { bestD = dd; best = e; bp = p; } }
        }
        string at = FormattableString.Invariant($"{n.Id} ({n.Position.X:F0}, {n.Position.Z:F0}) {own.Map.Cs2Prefab}");
        if (best != null && Math.Abs(bp.Y - n.Position.Y) / topologyFactor <= 3)
        {
            n.Snap = (Math.Ceiling(bestD + 1.5), best.Map.Cs2Prefab!);
            snapped.Add(FormattableString.Invariant($"{at} → {best.Id} {best.Map.Cs2Prefab}, {bestD:F1} m"));
            continue;
        }
        // Stub to the first junction.
        var chain = new List<OutEdge> { own }; double len = own.Curve.LengthXZ(); var cur = own.A == n ? own.B : own.A; var ce = own;
        while (degQ.GetValueOrDefault(cur) == 2 && len < 40)
        {
            var o = net.First(e => e != ce && (e.A == cur || e.B == cur));
            chain.Add(o); len += o.Curve.LengthXZ(); cur = o.A == cur ? o.B : o.A; ce = o;
        }
        if (len < 40 && degQ.GetValueOrDefault(cur) > 2) { foreach (var c in chain) drop.Add(c); removedStubs.Add(FormattableString.Invariant($"{at}: {len:F0} m stub ({string.Join(", ", chain.Select(c => c.Id))})")); }
        else residual.Add(FormattableString.Invariant($"{at}: CS1 continued here (CS1 degree {cs1Deg.GetValueOrDefault(cn)}), nothing within 25 m and the stub is {len:F0}+ m"));
    }
    // Isolated short pieces.
    var at2 = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
    foreach (var e in net) foreach (var n in new[] { e.A, e.B }) (at2.TryGetValue(n, out var l) ? l : at2[n] = new()).Add(e);
    var seenQ = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance); var isoDropped = new List<string>(); var isoKept = new List<string>();
    foreach (var e0 in net)
    {
        if (!seenQ.Add(e0)) continue;
        var comp = new List<OutEdge>(); var q = new Queue<OutEdge>(); q.Enqueue(e0);
        while (q.Count > 0) { var e = q.Dequeue(); comp.Add(e); foreach (var n in new[] { e.A, e.B }) foreach (var o in at2[n]) if (seenQ.Add(o)) q.Enqueue(o); }
        var ns = comp.SelectMany(e => new[] { e.A, e.B }).Distinct(ReferenceEqualityComparer.Instance).Cast<OutNode>().ToList();
        double L = comp.Sum(e => e.Curve.LengthXZ());
        if (L >= 100 || ns.Any(n => n.Border || Connected(n))) continue;
        // CS1 connected the piece to more network when one of its CS1 nodes has more CS1 segments than the piece uses there.
        bool pipeline = ns.Any(n => n.Cs1Node is { } c && cs1Deg.GetValueOrDefault(c) > at2[n].Count);
        var m = comp[0].Curve.Eval(0.5);
        string what = FormattableString.Invariant($"({m.X:F0}, {m.Z:F0}) {comp[0].Map.Cs2Prefab}, {L:F0} m ({string.Join(", ", comp.Select(c => c.Id))})");
        if (pipeline) { foreach (var c in comp) drop.Add(c); isoDropped.Add(what); } else isoKept.Add(what + ": isolated in CS1 too");
    }
    foreach (var e in drop) result.Fate[e.Cs1Segment] = "road QA: left out (stub / isolated piece)";
    result.Edges.RemoveAll(drop.Contains);
    auditLog.AppendLine($"- Road QA fixes: pipeline dead ends snapped {snapped.Count}, stubs < 40 m left out {removedStubs.Count}, residual {residual.Count}; isolated pieces < 100 m left out {isoDropped.Count}, kept (isolated in CS1 too) {isoKept.Count}.");
    foreach (var l in snapped) auditLog.AppendLine($"  - snapped: {l}");
    foreach (var l in removedStubs) auditLog.AppendLine($"  - stub left out: {l}");
    foreach (var l in residual) auditLog.AppendLine($"  - RESIDUAL dead end: {l}");
    foreach (var l in isoDropped) auditLog.AppendLine($"  - isolated, left out: {l}");
    foreach (var l in isoKept) auditLog.AppendLine($"  - isolated, kept: {l}");
    Console.WriteLine($"road QA fixes: snapped {snapped.Count}, stubs left out {removedStubs.Count}, residual {residual.Count}, isolated left out {isoDropped.Count}, kept {isoKept.Count}");
}
Outputs.WriteImport(Path.Combine(outDir, "import-highways.json"), "Dakota Falls highways", result.Layer(Layers.Highways).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-rail.json"), "Dakota Falls rail", result.Layer(Layers.Rail).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-roads.json"), "Dakota Falls roads", result.Layer(Layers.Roads).ToList(), absolute);
Outputs.WriteImport(Path.Combine(outDir, "import-all.json"), "Dakota Falls network (all layers)",
    Layers.Imported.SelectMany(l => result.Layer(l)).ToList(), absolute);
// Power stays out of import-all.json so it is a separately undoable import.
var power = result.Layer(Layers.Power).Where(e => e.Map.Cs2Prefab != null).ToList();
{
    // Run A-v3: the last courses that import-power-ends.json replaces stay out of import-power.json (else the new ends overlap them).
    var pTargets = new List<(string, double, double)>();
    var sb0 = Round8.Buildings(rootDoc.RootElement, "(?i)Substation"); if (sb0.Count > 0) pTargets.Add(("substation", sb0[0].X, sb0[0].Z));
    if (Round8.Dam(Round8.Buildings(rootDoc.RootElement, "(?i)Dam Electricity")) is { } dm0) pTargets.Add(("dam", dm0.A.X, dm0.A.Z));
    var replaced = Round8.PowerEnds(power, pTargets).Select(p => p.Replaces).ToHashSet(ReferenceEqualityComparer.Instance);
    var kept = power.Where(e => !replaced.Contains(e)).ToList();
    if (kept.Count > 0) Outputs.WriteImport(Path.Combine(outDir, "import-power.json"), "Dakota Falls power lines", kept, absolute,
        $"Left out (replaced by import-power-ends.json): {string.Join(", ", replaced.Cast<OutEdge>().Select(e => e.Id))}.");
}
Console.WriteLine($"power: {power.Count} courses -> {(power.Count > 0 ? "import-power.json" : "not written (power rule is SKIP in mapping.tsv)")}");
// Owner review of A-v4: quays / retaining walls (CS1 aesthetic only) and the monorail arch decor are dropped; canals and flood walls stay.
foreach (var dropped in new[] { "import-walls.json", "import-decor-monorail.json" }) File.Delete(Path.Combine(outDir, dropped));
foreach (var (layer, title, fileName) in new[] { (Layers.Walls, "canals and flood walls (Narrow Boatway, Sound Barrier)", "import-canals.json"), (Layers.ShipPaths, "ship paths", "import-shippaths.json") })
{
    var es = result.Layer(layer).Where(e => e.Map.Cs2Prefab != null && e.Map.Cs2Prefab is not ("QuaySmall01" or "QuayMedium01")).ToList();
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
    // Pack surfaces (Terrain Textures, Dirty Surfaces) and decals: vanilla only from now on (owner); the pack files stay in optional/.
    string OptDir()
    {
        var d = Path.Combine(outDir, "optional");
        if (!Directory.Exists(d))
        {
            Directory.CreateDirectory(d);
        }
        File.WriteAllText(Path.Combine(d, "README.md"), "# Optional pack surfaces\n\nNot imported by default: the owner chose vanilla surfaces only (2026-10-20). Terrain Textures Pack rock (40° slopes, PO terrain-like footprints), Dirty Surfaces Pack decals and the pack sand variant. Re-enable with tools/import-run.mjs --with rock,po-rock,decals,sand-pack. import-surfaces-sand-smooth.json is the earlier vanilla sand (smoothed CS1 sand blobs, Sand Surface 01), replaced by the paint plan (import-surfaces-sand.json); --with sand-smooth.\n");
        return d;
    }
    foreach (var old in new[] { "import-surfaces-rock.json", "import-surfaces-po-rock.json", "import-surfaces-decals.json", "import-surfaces-decals-unmerged.json" }) File.Delete(Path.Combine(outDir, old));
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
    var objs = Objects.Build(root, TargetMapping.Load(Find("object-mapping.tsv")), terr, heightFactor, 7168, treeSeed + 1, cs2Terrain == null ? null : cs2Terrain.Sample);
    var bounds = PrefabBound.Load(Find("prefab-bounds.json"));
    // Bridge 0.9.19 grounding (options.grounding): pack rocks carry no y, the bridge sets their bottom from the prefab bounds, sunk 15 %.
    int grounded = 0;
    for (int i = 0; i < objs.Objects.Count; i++)
    {
        var o = objs.Objects[i];
        if (!System.Text.RegularExpressions.Regex.IsMatch(o.Prefab, "^(rzrock|RZaridrock|Cliff)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) continue;
        if (o.Y != null) { objs.Objects[i] = o with { Y = null }; grounded++; }
    }
    var grounding = new Objects.Grounding();
    extra.AppendLine($"- Pack rocks: no y (their y was dropped on {grounded}); options.grounding {{mode boundsMin, sink 0.15, slopeSink}} grounds every object without y from its prefab bounds (bridge 0.9.19).");
    Objects.Write(Path.Combine(outDir, "import-objects.json"), "objects", objs.Objects, grounding);
    Console.WriteLine($"objects: {objs.Targeted} placed of {objs.Total} (skip {objs.Skipped}, manual {objs.ManualCount}, unmapped {objs.Unmapped}, outside {objs.Outside}, with y {objs.WithY}; vs the CS1-terrain rule: lost y {objs.LostY}, gained y {objs.GainedY})");
    extra.AppendLine($"- Object y policy: {(cs2Terrain != null ? $"y only > 3 m above the CS2 playable heightmap ({Path.GetFileName(cs2HeightmapPath)})" : "y where > 2 m off the CS1 terrain (no CS2 heightmap)")}, never for buildings: {objs.WithY} with y; against the old CS1-terrain rule {objs.LostY} lose y, {objs.GainedY} gain it.");
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
    if (Round8.Dam(Round8.Buildings(root, "(?i)Dam Electricity")) is { } damM)
        man.AppendLine().AppendLine("## Dam (automatic: import-dam.json, bridge 0.9.23)").AppendLine().AppendLine(FormattableString.Invariant(
            $"The game builds the plant and the dam from the one dam edge in import-dam.json. Fallback by hand: {Round8.DamPrefab} at ({damM.A.X}, {damM.A.Z}), rotation {damM.A.Rotation} (aligns with the CS1 dam axis, bridge live test). Deck: {Round8.DamDeckPrefab} from ({Round8.DamDeckA.X}, {Round8.DamDeckA.Z}) to ({Round8.DamDeckB.X}, {Round8.DamDeckB.Z}) at crest y {Round8.DamCrestTrue * heightFactor} (level). The power line ends at the plant through import-power-ends.json (connectTo high voltage)."));
    man.AppendLine().AppendLine("## Fort (CS1 Castle Wall 1 / 3, 12 segments)").AppendLine();
    man.AppendLine("import-fort.json marks the wall line with ruins (RuinsStoneFormation01/02 every 12 m, StandingStone01–08 at the ends, RuinsStoneCairns01–03 inside the line) " +
        "and puts OldCastle01 at the fort's centroid as the keep (unverified as a placeable object); RuinsCastle01 stands at the CS1 Castle Ruins positions (import-objects.json). " +
        "A true rampart needs a wall asset pack from PDX Mods: CS2 has no vanilla stone fort wall (EU_Wall01–09 are 0.9 m garden walls) and none is in the playset.");
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
            barrierProps.Add(new PlacedObject($"bar{s.Id}_{k}", prefab, "prop", pnt.X, pnt.Z, y, Math.Round((Connections.Heading(d.X, d.Z) + 90) % 360, 2), s.Prefab, $"segment {s.Id}"));
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
    var rockPolys = new List<SurfacePolygon>();
    if (rockHm != null && File.Exists(rockHm))
    {
        var hmTrue = Heightmap.LoadPng(rockHm);   // 2048 m scale = true CS1 heights
        double err = hmTrue.Orient(dump.Nodes.Values.Select(n => (n.Position.X, n.Position.Z, n.Terrain / dump.HeightFactor)));
        var rock = Surfaces.SteepRock(hmTrue, rockDegrees);
        var rocks = rock.Polygons.Select((r, k) => new SurfacePolygon($"rock{k + 1}", RockSurface, r, $"slope >= {rockDegrees}°")).ToList();
        rockPolys = rocks;
        Surfaces.Write(Path.Combine(OptDir(), "import-surfaces-rock.json"), "surfaces-rock", rocks,
            $"Automatic rock surface: pixels of {Path.GetFileName(rockHm)} (true heights, 3.5 m) with slope >= {rockDegrees}°, patches >= 400 m², hole-free, simplified to 4 m, largest {rocks.Count} kept. Prefab {RockSurface} (Terrain Textures Pack - Rocks, brown scree).");
        string msg = string.Create(System.Globalization.CultureInfo.InvariantCulture, $"rock: {Path.GetFileName(rockHm)} (|err| vs CS1 nodes {err:F2} m), steep area {rock.SteepArea / 1e6:F2} km² in {rock.Patches} patches >= 400 m²; {rocks.Count} polygons kept ({rock.KeptArea / 1e6:F2} km²), {rock.Dropped} smallest dropped");
        Console.WriteLine(msg);
        extra.AppendLine($"- Rock surfaces (import-surfaces-rock.json): {msg.Substring(6)}.");
    }

    // Round 8 (owner review of Run A): substation, dam, power-line ends, PO cliff objects, rock scatter; one import file each.
    {
        Func<double, double, double>? cs2True = cs2Terrain == null ? null : cs2Terrain.Sample;
        var sub = objs.Objects.Where(o => o.Prefab == Round8.SubstationPrefab).ToList();
        Objects.Write(Path.Combine(outDir, "import-substation.json"), "substation (round 8)", sub);
        var subB = Round8.Buildings(root, "(?i)Substation");
        var dams = Round8.Buildings(root, "(?i)Dam Electricity");
        var dam = Round8.Dam(dams);
        var damLog = new List<string>();
        if (dam is { } dv)
        {
            // Bridge live test: yaw 261.7 (−CS1 angle) aligns the plant with the CS1 dam axis; the mirrored variant is gone.
            // Bridge 0.9.23 builds the plant, the dam and its ends from one network edge (IMPORT_SCHEMA.md "Dams"); no plant object file.
            foreach (var dropped in new[] { "import-dam-B.json", "import-dam-deck.json" }) File.Delete(Path.Combine(outDir, dropped));
            damLog = Round8.WriteDam(Path.Combine(outDir, "import-dam.json"), cs2True, heightFactor);
        }
        var targets = new List<(string, double, double)>();
        if (subB.Count > 0) targets.Add(("substation", subB[0].X, subB[0].Z));
        if (dam is { } d2) targets.Add(("dam", d2.A.X, d2.A.Z));
        var ends = Round8.PowerEnds(result.Layer(Layers.Power).ToList(), targets);
        Round8.WritePowerEnds(Path.Combine(outDir, "import-power-ends.json"), ends, cs2True, heightFactor, absolute);
        var pieces = Round8.LoadPoPieces(poOfflinePath);
        // v1 cliff positions stay the rock scatter's sources (import-rock-scatter.json unchanged); import-po-rocks.json is v2.
        var (cliffs, capped, bands) = Round8.Cliffs(pieces, cs2True, 7168, treeSeed + 8);
        var heightsH = PoRocksV2.LoadHeights(Find(Path.Combine("..", "cs2-playset", "ASSET_PACKS.md")));
        var (v2, v2capped, v2big, v2renamed) = PoRocksV2.Build(pieces, bounds, heightsH, cs2True, heightFactor, 7168, treeSeed + 10);
        // No y: the bridge grounds each piece from its bounds; the per-object "sink" (fraction of the height, (H − v) / H) keeps the v3 visible-height rule.
        var poSinks = new Dictionary<string, double>();
        foreach (var p in v2) if (bounds.TryGetValue(p.O.Prefab, out var pbb) && pbb.Height > 0) poSinks[p.O.Id] = p.Sink / pbb.Height;
        Objects.Write(Path.Combine(outDir, "import-po-rocks.json"), "po-rocks-v3", v2.Select(p => p.O with { Y = null }), grounding, poSinks);
        var sources = objs.Objects.Where(o => Round8.IsRockLike(o.Prefab)).Concat(cliffs).Select(o => (o.X, o.Z)).ToList();
        Func<double, double, bool>? wet = surfaceEarly == null ? null : (x, z) => surfaceEarly.Cells.TryGetValue(surfaceEarly.Grid.Index(x, z), out var c) && c.Depth > 0.5;
        var (scatter, rejected) = Round8.Scatter(sources, rockPolys.Select(p => p.Ring), result.Edges.Where(e => e.Map.Imported), wet, 7168, treeSeed + 9);
        // Owner review of A-v4: no scatter rock in water (CS2 terrain below sea level); on slopes over 25° the rock is lowered by
        // tan(slope) × half its footprint so the downhill side does not float; over 45° it is left out.
        int sWater = 0, sSteep = 0, sSunk = 0;
        if (cs2Terrain != null)
        {
            var keptS = new List<PlacedObject>();
            foreach (var o in scatter)
            {
                double t0 = cs2Terrain.Sample(o.X, o.Z);
                if (t0 < seaLevel) { sWater++; continue; }
                double gx = (cs2Terrain.Sample(o.X + 3.5, o.Z) - cs2Terrain.Sample(o.X - 3.5, o.Z)) / 7, gz = (cs2Terrain.Sample(o.X, o.Z + 3.5) - cs2Terrain.Sample(o.X, o.Z - 3.5)) / 7;
                double slope = Math.Atan(Math.Sqrt(gx * gx + gz * gz)) * 180 / Math.PI;
                if (slope > 45) { sSteep++; continue; }
                if (slope > 25) sSunk++;   // the bridge's slopeSink lowers these
                keptS.Add(o with { Y = null });
            }
            scatter = keptS;
        }
        Objects.Write(Path.Combine(outDir, "import-rock-scatter.json"), "rock scatter", scatter, grounding);
        string ci(FormattableString f) => FormattableString.Invariant(f);
        var r8 = new List<string>
        {
            ci($"substation: {sub.Count} ({string.Join(", ", sub.Select(o => $"{o.Id} {o.Prefab} at ({o.X:F0}, {o.Z:F0}), rotation {o.Rotation}, no y"))}) -> import-substation.json"),
            dam is { } d3 ? "dam (network, bridge 0.9.23): " + string.Join("; ", damLog) + " -> import-dam.json" : "dam: no CS1 Dam Electricity building",
            "power ends: " + string.Join("; ", ends.Select(e => ci($"{e.Target}: CS1 end {e.OldEnd.Id} ({e.OldEnd.Position.X:F0}, {e.OldEnd.Position.Z:F0}), replaces {e.Replaces.Id}, new course {e.Anchor.Id} -> ({e.X:F1}, {e.Z:F1}), {Math.Sqrt((e.X - e.Anchor.Position.X) * (e.X - e.Anchor.Position.X) + (e.Z - e.Anchor.Position.Z) * (e.Z - e.Anchor.Position.Z)):F0} m"))) + " -> import-power-ends.json",
            ci($"PO rocks v3: {v2.Count} objects ({Round8.Counts(v2.Select(p => p.O))}) from {pieces.Count} PO pieces, {v2capped} over the cap of 1500 dropped, {v2big} PO objects > 60 m above their surroundings with CliffSide pieces down the steepest flank, {v2renamed} re-picked for the 80 m prefab/heading rule; with y {v2.Count(p => p.O.Y != null)}; sink depth (mesh bottom below terrain): {PoRocksV2.SinkHistogram(v2)} -> import-po-rocks.json (import name po-rocks-v3)"),
            ci($"PO cliffs v1 (not written; their positions feed the rock scatter): {cliffs.Count} objects from {pieces.Count} PO pieces (mountain {pieces.Count(p => p.SubClass == "mountain")}, rock work {pieces.Count(p => p.SubClass == "rock work")}, falls steps {pieces.Count(p => p.SubClass == "falls steps")}); bands {string.Join(", ", bands.Select(b => $"{b.Key} {b.Value}"))}; {capped} over the cap of 2500 dropped -> import-po-rocks.json"),
            ci($"rock scatter: in water left out {sWater}, slopes > 45° left out {sSteep}, slopes 25–45° (lowered by the bridge's slopeSink) {sSunk}; {scatter.Count} (small {scatter.Count(o => Round8.SmallRocks.Contains(o.Prefab))}, medium {scatter.Count(o => Round8.MediumRocks.Contains(o.Prefab))}, boulders {scatter.Count(o => Round8.Boulders.Contains(o.Prefab))}) around {sources.Count} rock/cliff objects (r 60 m) and in {rockPolys.Count} rock polygons; rejected road {rejected["road"]}, spacing {rejected["spacing"]}, water {rejected["water"]}, border {rejected["border"]} -> import-rock-scatter.json"),
        };
        foreach (var l in r8) { Console.WriteLine("round 8 " + l); extra.AppendLine($"- Round 8 {l}."); }
    }

    // Round 9: PO rock texture, the fort, PO paved blocks, lakes, network gaps; one import file each.
    {
        Func<double, double, double>? cs2True = cs2Terrain == null ? null : cs2Terrain.Sample;
        string ci(FormattableString f) => FormattableString.Invariant(f);
        var r9 = new List<string>();
        var smap = TargetMapping.Load(Find("surface-mapping.tsv"));
        var raster = Path.Combine(Path.GetDirectoryName(poOfflinePath) ?? ".", "v2", "po-terrain-like-cs2-4096.png");
        if (File.Exists(raster))
        {
            string prefab = smap.Resolve("po rock")?.FirstOrDefault()?.Prefab ?? RockSurface;
            var (polys, area, patches, dropped) = Round9.PoRock(raster);
            var poRock = polys.Select((r, k) => new SurfacePolygon($"porock{k + 1}", prefab, r, "PO terrain-like footprint")).ToList();
            Surfaces.Write(Path.Combine(OptDir(), "import-surfaces-po-rock.json"), "surfaces-po-rock", poRock,
                $"Rock texture on every PO mountain / rock work / falls steps footprint ({Path.GetFileName(raster)}, cells > 0, 3.5 m), hole-free, simplified to 4 m, patches >= 100 m². The 40° slope layer is import-surfaces-rock.json.");
            r9.Add(ci($"PO rock surfaces: {poRock.Count} polygons ({poRock.Sum(p => Math.Abs(Surfaces.Area(p.Ring))) / 1e6:F2} km² of {area / 1e6:F2} km² footprint, {patches} patches, {dropped} dropped), {prefab} -> import-surfaces-po-rock.json"));
        }
        var (fort, keep) = Round9.Fort(dump, new System.Text.RegularExpressions.Regex("(?i)Castle Wall"), 7168, treeSeed + 11);
        Objects.Write(Path.Combine(outDir, "import-fort.json"), "fort ruins (round 11b)", fort, new Objects.Grounding());
        r9.Add(ci($"fort: {fort.Count} objects along the 12 CS1 Castle Wall segments ({Round8.Counts(fort)}), keep OldCastle01 at ({keep?.X:F0}, {keep?.Z:F0}) (UNVERIFIED placeable) -> import-fort.json; RuinsCastle01 stays in import-objects.json"));
        var blocks = Round9.LoadPavedBlocks(poOfflinePath);
        string pave = smap.Resolve("pavement")?.FirstOrDefault()?.Prefab ?? "Pavement Surface 01";
        var (ps, pn, pe, tall) = Round9.PavedStructures(blocks, pave, heightFactor, 7168);
        // Run A-v3: paved-block sides the game dropped inside a highway are left out (run reports).
        var pbDrops = Directory.Exists(runReportsDir) ? FootprintShift.ParseDrops(Directory.GetFiles(runReportsDir, "run-*.md")) : new();
        var pbOut = pe.Where(e => pbDrops.ContainsKey(e.Id)).Select(e => e.Id).ToList();
        pe.RemoveAll(e => pbDrops.ContainsKey(e.Id));
        if (pbOut.Count > 0) r9.Add($"PO walls left out (dropped by the game inside a highway in the run reports): {string.Join(", ", pbOut)}");
        Surfaces.Write(Path.Combine(outDir, "import-po-structures.json"), "po-structures", ps, $"Tops of the PO paved blocks (convex hull of the top vertices), {pave}, overlaps unioned.");
        File.Delete(Path.Combine(outDir, "import-po-walls.json"));   // owner review of A-v4: not needed
        r9.Add(ci($"PO paved blocks: {blocks.Count} blocks -> {ps.Count} pavement polygons (import-po-structures.json); PO walls dropped (owner review of A-v4); tunnel/shed and bridge/pillar PO stay in manual-objects.md"));
        if (surfaceEarly != null)
        {
            var (lakes, bodies, outside, tiny) = Round9.Lakes(surfaceEarly, dump, seaLevel, 7168, cs2True, out var lakeLog);
            foreach (var l in lakeLog) r9.Add("lake " + l);
            Water.Write(Path.Combine(outDir, "import-water-lakes.json"), lakes, heightFactor, seaLevel * heightFactor, 7168);
            r9.Add(ci($"lakes: {bodies} wet bodies above sea level {seaLevel} + 1 m with no CS1 source within 400 m; {lakes.Count} sources (outside the map {outside}, single cells {tiny}) -> import-water-lakes.json (append after import-water*.json)"));
        }
        r9.Add($"gaps: {gapEdges} edges -> import-gaps.json, written before the network ({gapLog})");
        foreach (var l in r9) { Console.WriteLine("round 9 " + l); extra.AppendLine($"- Round 9 {l}."); }
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
        Surfaces.Write(Path.Combine(OptDir(), "import-surfaces-decals-unmerged.json"), "surfaces-decals-unmerged-experiment", decals, "EXPERIMENT: one 16 x 16 m square per CS1 decal, overlaps kept (z-fighting and OverlapExisting possible).");
        var (u, pairs, groups) = Surfaces.UnionOverlapping(decals, 1);
        auditLog.AppendLine($"- Decal surfaces: {pairs} overlapping pairs merged in {groups} groups ({decals.Count} → {u.Count}).");
        decals = u;
    }
    if (decals.Count > 0) Surfaces.Write(Path.Combine(OptDir(), "import-surfaces-decals.json"), "surfaces-decals", decals, "CS1 R69 Big Decal Dirt/Mud as Dirty Surfaces Pack (148077) area surfaces: 16 x 16 m squares at the CS1 position and angle (footprint assumed). Dirt -> PDO23 Dirty Surface 01, Mud -> PDO23 Soil Surface 01.");
    Console.WriteLine(Surfaces.Stats("decal surfaces", decals));
    extra.AppendLine($"- {Surfaces.Stats("Decal surfaces (import-surfaces-decals.json; PDO23 Dirty Surface 01 / PDO23 Soil Surface 01)", decals)}.");

    if (surfaces != null)
    {
        var ts = Surfaces.FromTerrainSurfaces(surfaces, TargetMapping.Load(Find("surface-mapping.tsv")), out var tsDecisions);
        // One file per CS1 class (e.g. import-surfaces-sand.json), already unioned per class (rasterised), hole-free, simplified.
        File.Delete(Path.Combine(outDir, "import-surfaces.json"));
        foreach (var g in ts.GroupBy(t => t.Note.Replace("CS1 surface ", "").ToLowerInvariant()))
            if (g.Key == "sand")
            {
                File.Delete(Path.Combine(outDir, "import-surfaces-sand.json"));
                var (blobs, sp, sk) = SmoothSurfaces.Blobs(surfaces, "Sand");
                // Vanilla surfaces only (owner): Sand Surface 01, on by default; the Dirty Surfaces Pack sand stays as an optional copy.
                var sandPrefab = g.First().Prefab;
                Surfaces.Write(Path.Combine(outDir, "import-surfaces-sand-smooth.json"), "surfaces-sand-smooth", blobs.Select((r, k) => new SurfacePolygon($"sand{k + 1}", "Sand Surface 01", r, "CS1 sand blob")).ToList(),
                    "CS1 sand as vanilla Sand Surface 01: patches >= 2000 m², 12 m round buffer, 2 majority-smoothing passes on a 4 m grid, hole-free, simplified to 4 m.");
                Surfaces.Write(Path.Combine(OptDir(), "import-surfaces-sand-smooth-pack.json"), "surfaces-sand-smooth-pack", blobs.Select((r, k) => new SurfacePolygon($"sand{k + 1}", sandPrefab, r, "CS1 sand blob")).ToList(),
                    $"OPTIONAL pack variant ({sandPrefab}) of import-surfaces-sand-smooth.json.");
                extra.AppendLine(FormattableString.Invariant($"- Sand: the 16 m-cell surfaces are dropped (owner review of A-v4); optional import-surfaces-sand-smooth.json: {blobs.Count} blobs from {sk} of {sp} patches >= 2000 m² ({blobs.Sum(b => Math.Abs(Surfaces.Area(b))) / 1e6:F2} km²)."));
                Console.WriteLine($"sand: {blobs.Count} smooth blobs (optional) from {sk} of {sp} patches");
            }
            else Surfaces.Write(Path.Combine(outDir, $"import-surfaces-{g.Key}.json"), $"surfaces-{g.Key}", g.ToList(), $"CS1 terrainSurfaces class {g.Key} mapped through surface-mapping.tsv (audit/theme.md table): rasterised per class on the 16 m grid, union, hole-free pieces, simplified to 2 m.");
        Console.WriteLine(Surfaces.Stats("terrain surfaces", ts) + "; " + string.Join(", ", tsDecisions.Select(d => $"{d.Key} -> {d.Value}")));
        extra.AppendLine($"- {Surfaces.Stats("Terrain surfaces (import-surfaces.json)", ts)}: {string.Join("; ", tsDecisions.Select(d => $"{d.Key} → {d.Value}"))}.");
    }

    // Vanilla paint plan (audit/theme-remake.md §4): sand, bare earth, dry grass, stand-in pavement; the default surface files.
    {
        int[]? fert = null; GridSpec? fertGrid = null;
        if (root.TryGetProperty("naturalResources", out var nrP) && nrP.ValueKind == JsonValueKind.Object && nrP.TryGetProperty("layers", out var lyP) && lyP.TryGetProperty("fertility", out var fl) && fl.ValueKind == JsonValueKind.Array)
        {
            fert = fl.EnumerateArray().Select(v => v.GetInt32()).ToArray();
            fertGrid = GridSpec.FromJson(nrP.TryGetProperty("grid", out var gg) ? gg : null, Resources.GridSize, Resources.CellSize);
        }
        // PO pavement blocks (import-po-structures.json) and stand-in buildings: objects that replace a CS1 building but bring no lot.
        var poBlocks = new List<List<(double X, double Z)>>();
        var psPath = Path.Combine(outDir, "import-po-structures.json");
        if (File.Exists(psPath))
            using (var pd = JsonDocument.Parse(File.ReadAllBytes(psPath)))
                foreach (var sp in pd.RootElement.GetProperty("surfaces").EnumerateArray())
                    poBlocks.Add(sp.GetProperty("polygon").EnumerateArray().Select(v => (v[0].GetDouble(), v[1].GetDouble())).ToList());
        var natural = new System.Text.RegularExpressions.Regex("(?i)rock|cliff|boulder|stone|log|rubble|ruins|grass|fern|flower|bush|tree|pier");
        var bById = root.GetProperty("buildings").EnumerateArray().ToDictionary(b => b.GetProperty("id").GetInt32(), b => b);
        var standIns = new List<List<(double X, double Z)>>();
        foreach (var o in objs.Objects.Where(o => o.Source.StartsWith("building ") && o.Kind != "building" && !natural.IsMatch(o.Prefab)))
        {
            if (!int.TryParse(o.Source[9..], out var bid) || !bById.TryGetValue(bid, out var b)) continue;
            double w = (b.TryGetProperty("width", out var bw) ? bw.GetDouble() : 1) * 8, l = (b.TryGetProperty("length", out var bl) ? bl.GetDouble() : 1) * 8;
            double r = o.Rotation * Math.PI / 180, fx = Math.Sin(r), fz = Math.Cos(r);
            (double, double) Pt(double u, double v) => (o.X + u * fz + v * fx, o.Z - u * fx + v * fz);
            standIns.Add(new List<(double, double)> { Pt(-w / 2, -l / 2), Pt(w / 2, -l / 2), Pt(w / 2, l / 2), Pt(-w / 2, l / 2) });
        }
        var plan = PaintPlan.Build(new PaintPlan.Inputs(surfaces, surfaceEarly, cs2Terrain == null ? null : cs2Terrain.Sample, fert, fertGrid,
            trees.Objects.Select(t => (t.X, t.Z)), fields.Select(p => p.Ring), poBlocks, decals.Select(p => p.Ring), standIns));
        Surfaces.Write(Path.Combine(outDir, "import-surfaces-sand.json"), "surfaces-sand", plan.Sand,
            "Paint plan S1/S3/S4 (audit/theme-remake.md §4): Sand Surface 01 on CS1 sand within 25 m of the water or ≤ 2 m above it; Sand Surface 02 on the other CS1 sand and on the CS1 dirt decals; none under water deeper than 1.5 m. 16 m grid, open/close one cell, simplified 4 m.");
        Surfaces.Write(Path.Combine(outDir, "import-surfaces-grass-dry.json"), "surfaces-grass-dry", plan.Grass,
            $"Paint plan G1/G2: {PaintPlan.DryGrass} [variant to confirm with the swatch test] on fertile open flats (fertility ≥ 0.5, slope < 8°, no forest) and on the dry eastern plains (x ≥ 0, ≤ start-area mean {plan.StartMean:F0} + 60 m, no tree within 48 m, slope < 12°).");
        File.Delete(Path.Combine(outDir, "import-surfaces-pavement.json"));
        if (plan.PavementStandIns.Count > 0) Surfaces.Write(Path.Combine(outDir, "import-surfaces-pavement.json"), "surfaces-pavement", plan.PavementStandIns,
            "Paint plan P1: Pavement Surface 01 under stand-in buildings that bring no lot (CS1 lot width × length, rotated).");
        File.Move(Path.Combine(outDir, "import-surfaces-sand-smooth.json"), Path.Combine(OptDir(), "import-surfaces-sand-smooth.json"), true);
        foreach (var (k, v) in plan.PerRule) extra.AppendLine(FormattableString.Invariant($"- Paint plan {k}: {v.Cells} cells, {v.Polygons} polygons, {v.Km2:F2} km²."));
        Console.WriteLine("paint plan: " + string.Join("; ", plan.PerRule.Select(kv => FormattableString.Invariant($"{kv.Key} {kv.Value.Polygons} polygons {kv.Value.Km2:F2} km²"))));
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
    SegRow("Canal", "^Canal", "→ Narrow Boatway (walls layer); CS2 boatways are canals");
    SegRow("Flood Wall", "Flood Wall", "→ Sound Barrier (walls layer), the only wall network by name");
    SegRow("Retaining Walls AiO", "Retaining Wall", "→ QuaySmall01 (walls layer); no retaining-wall network, quay wall is the nearest (unverified inland)");
    extra.AppendLine($"| Concrete / Steel barriers | network segments | {dump.Segments.Count(s => barrierRx.IsMatch(s.Prefab))} | {barrierProps.Select(b => b.Source).Distinct().Count()} as props | {dump.Segments.Count(s => barrierRx.IsMatch(s.Prefab)) - barrierProps.Select(b => b.Source).Distinct().Count()} (outside ±7168) | → {barrierProps.Count} ConcreteBarrier01 / SafetyBarrier01 props every 3 m (import-walls-props.json) |");
    SegRow("Castle Wall", "Castle Wall", "NO TWIN, kept skipped: searched Assets.NAME for Wall/Castle/Rampart/Fort/Ruin (only garden walls EU_Wall*, RuinsCastle01 building, Sound Barrier)");
    SegRow("Ship paths", "^Ship (Connection )?Path", "→ Medium Seaway (import-shippaths.json), clipped to end on the border");
    SegRow("Airplane paths", "^Airplane", "kept skipped: the game routes planes");
    ObjRow("Ruins / wreck / factory", "(?i)Pier Ruins|Ship Wreck|Abandoned Factory|Castle Ruins", "buildings", "→ LeisurePier01HeadSmall / RubblePileWood02 / RuinsOldMill01 / RuinsCastle01 (nearest by name)");
    SegRow("Terraforming Network", "Terraforming", "kept skipped: CS1 terrain-shaping helper (terrain is in the heightmap)");
    ObjRow("Pillars", "(?i)Pillar", "buildings", "kept skipped: CS2 bridges spawn their own pillars");
    ObjRow("Modern Fence 02", "(?i)Modern Fence", "props", "→ FenceResidentialPieceLow01 (piece length unverified)");
    extra.AppendLine($"| Network Skins (v5) | segments | {Outputs.SkinFlags.Count} | {Outputs.SkinFlags.Count} flagged | 0 | note: no pillars on {Outputs.SkinFlags.Values.Count(v => v.Pillars)}, no street lights on {Outputs.SkinFlags.Values.Count(v => v.Lights)}; edge fields noPillars / noStreetLights (no bridge field yet), list in manual-objects.md |");
    extra.Append(auditLog);
    File.AppendAllText(Path.Combine(outDir, "stats.md"), extra.ToString());
}
