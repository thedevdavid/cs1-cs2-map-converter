using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NetConverter;

public static class Outputs
{
    /// <summary>
    /// Factor applied to every height written (node y, elevation, water heights). The network is converted once at the
    /// topology factor (2×) and written at heightFactor / topologyFactor, so the 1× and 2× sets differ only in heights.
    /// </summary>
    public static double YScale { get; set; } = 1;
    /// <summary>CS1 segment id → Network Skins removals (v5), written as informational edge fields.</summary>
    public static Dictionary<int, (bool Pillars, bool Lights)> SkinFlags { get; } = new();
    static double R2(double v) => Math.Round(v, 2);

    static void P(Utf8JsonWriter w, Vec3 v)
    {
        w.WriteStartArray(); w.WriteNumberValue(R2(v.X)); w.WriteNumberValue(R2(v.Y)); w.WriteNumberValue(R2(v.Z)); w.WriteEndArray();
    }

    static Utf8JsonWriter Writer(Stream s) => new(s, new JsonWriterOptions { Indented = true });

    /// <param name="absolute">heightMode "absolute": every node carries y = terrain + elevation; only ground nodes carry
    /// elevation 0, raised/sunk nodes get their height from y (the bridge computes elevation = y − terrain); edges carry no elevation.</param>
    public static void WriteImport(string path, string name, IReadOnlyList<OutEdge> edges, bool absolute = false)
    {
        using var fs = File.Create(path);
        using var w = Writer(fs);
        w.WriteStartObject();
        w.WriteString("name", name);
        w.WriteString("_comment", "net-converter: CS1 Dakota Falls -> CS2, x/z metres unchanged (factor 1.0), clipped at ±7168 m; y = CS1 world height; elevation = metres above terrain");
        w.WriteStartArray("nodes");
        foreach (var n in edges.SelectMany(e => new[] { e.A, e.B }).Distinct())
        {
            w.WriteStartObject();
            w.WriteString("id", n.Id);
            w.WriteNumber("x", R2(n.Position.X)); w.WriteNumber("y", R2((absolute ? n.Terrain + n.Elevation : n.Position.Y) * YScale)); w.WriteNumber("z", R2(n.Position.Z));
            if (!absolute || n.Elevation == 0) w.WriteNumber("elevation", R2(n.Elevation * YScale));
            if (n.Border) w.WriteBoolean("border", true);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        foreach (var e in edges)
        {
            w.WriteStartObject();
            w.WriteString("id", e.Id);
            w.WriteString("a", e.A.Id); w.WriteString("b", e.B.Id);
            w.WriteString("prefab", e.Map.Cs2Prefab);
            w.WriteStartArray("control"); P(w, e.Curve.B); P(w, e.Curve.C); w.WriteEndArray();
            if (!absolute) { w.WriteStartArray("elevation"); w.WriteNumberValue(R2(e.A.Elevation * YScale)); w.WriteNumberValue(R2(e.B.Elevation * YScale)); w.WriteEndArray(); }
            if (e.Wall != null) w.WriteString("wall", e.Wall);
            if (SkinFlags.TryGetValue(e.Cs1Segment, out var sk)) { if (sk.Pillars) w.WriteBoolean("noPillars", true); if (sk.Lights) w.WriteBoolean("noStreetLights", true); }
            w.WriteString("structure", e.Structure.ToString().ToLowerInvariant());
            w.WriteString("cs1_prefab", e.Map.Cs1Prefab);
            w.WriteNumber("cs1_segment", e.Cs1Segment);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 64); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", absolute ? "absolute" : "elevation"); w.WriteBoolean("dryRun", false);
        w.WriteEndObject();
        w.WriteEndObject();
    }

    public static void WriteWater(string path, Dump d, double half)
    {
        using var fs = File.Create(path);
        using var w = Writer(fs);
        w.WriteStartObject();
        w.WriteString("_comment", "CS1 water sources. position = outputPosition; height = target surface height in metres (m_target/64); rate = outputRate (CS1 units, unscaled).");
        w.WriteNumber("seaLevel", R2(d.SeaLevel * YScale));
        w.WriteStartArray("sources");
        foreach (var s in d.WaterSources)
        {
            w.WriteStartObject();
            w.WriteNumber("id", s.Id); w.WriteString("type", s.Type);
            w.WritePropertyName("position"); P(w, s.Output with { Y = s.Output.Y * YScale });
            if (s.Input != s.Output) { w.WritePropertyName("inputPosition"); P(w, s.Input with { Y = s.Input.Y * YScale }); }
            w.WriteNumber("height", R2(s.TargetHeight * YScale));
            w.WriteNumber("rate", s.OutputRate); w.WriteNumber("inputRate", s.InputRate); w.WriteNumber("flow", s.Flow);
            w.WriteBoolean("inPlayable", Clipper.Outside(s.Output, half) <= 0);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    /// <summary>
    /// Water sources for POST /import/water: {sources:[{id,x,z,height,radius,type,rate}]}. height = CS1 target surface (m);
    /// radius = clamp(√rate, 20, 150) m from the CS1 output rate (16383 → 128 m, 3276 → 57 m); type "natural".
    /// Sources whose surface is at or below the suggested sea level are flagged, since the sea level already covers them.
    /// </summary>
    public static void WriteWaterImport(string path, Dump d, double half, double seaLevel, Dictionary<int, double>? overridesCs1 = null, double heightFactor = 1)
    {
        using var fs = File.Create(path);
        using var w = Writer(fs);
        w.WriteStartObject();
        w.WriteString("_comment", $"CS1 natural water sources. height = target water surface in true metres (Height Scale 2048). radius = clamp(sqrt(CS1 outputRate), 20, 150) m. belowSeaLevel: surface <= {seaLevel} m (the user's measured CS2 sea level for the lower river).");
        w.WriteNumber("seaLevel", seaLevel);
        w.WriteStartArray("sources");
        foreach (var s in d.WaterSources)
        {
            w.WriteStartObject();
            w.WriteNumber("id", s.Id);
            w.WriteNumber("x", R2(s.Output.X)); w.WriteNumber("z", R2(s.Output.Z));
            bool refined = overridesCs1 != null && overridesCs1.ContainsKey(s.Id);
            w.WriteNumber("height", R2(refined ? overridesCs1![s.Id] * heightFactor : s.TargetHeight * YScale));
            if (refined) w.WriteString("refinedFrom", "CS1 water surface (water-check.md)");
            w.WriteNumber("radius", Math.Round(Math.Clamp(Math.Sqrt(s.OutputRate), 20, 150)));
            w.WriteString("type", "natural");
            w.WriteNumber("rate", s.OutputRate);
            w.WriteBoolean("inPlayable", Clipper.Outside(s.Output, half) <= 0);
            w.WriteBoolean("belowSeaLevel", s.TargetHeight * YScale <= seaLevel);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    public static string Round4Report(ConvertResult r, double minBridge)
    {
        var ci = CultureInfo.InvariantCulture;
        var r4 = r.R4;
        var sb = new StringBuilder();
        sb.AppendLine("# Round 4 converter passes").AppendLine();
        sb.AppendLine($"- Crossings between layers without a shared node: {r4.Crossings}. Upper course raised at the crossing (≥ {minBridge} m, CS1 deck height): {r4.CrossingNodesInserted} nodes inserted, {r4.CrossingRampNodes} ramp nodes adjusted. Already separated: {r4.CrossingsAlreadySeparated}. At grade in CS1: {r4.CrossingsAtGradeInCs1}.");
        sb.AppendLine($"- Bridge profiles: {r4.BridgeChains} chains of CS1 bridge/elevated pieces, {r4.BridgeChainsChanged} changed, {r4.BridgeNodesChanged} nodes moved (max {r4.BridgeMaxChange:F1} m); grade flips above 1 % removed; single humps kept.");
        sb.AppendLine($"- Twisted courses (control point behind its end or self-intersection): {r4.TangentFailures}; {r4.TangentSwapped} fixed by swapping the control points, {r4.TangentStraightened} made straight.");
        sb.AppendLine($"- Tiny diverge stubs (< 10 m, junction to dead end) collapsed: {r4.TinyStubsCollapsed}.");
        sb.AppendLine($"- Highway ramp tails snapped onto the course beside them: {r4.RampTailsSnapped}.");
        sb.AppendLine($"- Transitions (no node in (−8, 0) m, portal over one course; no deck node in (0, 2) m beside a raised node): {r4.PortalsSetToZero} portal nodes set to 0, {r4.TunnelsDeepened} tunnel nodes set to −12 m, {r4.RaisedToGround} low nodes set to 0, {r4.RaisedToMin} set to 2 m, {r4.TunnelNodesCapped} tunnel nodes capped so depth grows at most 15 % from the portal.");
        sb.AppendLine($"- Heights: CS2 = {r.Dump.HeightFactor} × CS1 (Height Scale 4096 with the 2048 m heightmap). import files use heightMode \"absolute\": every node has y; ground nodes also carry elevation 0.");
        sb.AppendLine();
        sb.AppendLine("## Crossings").AppendLine();
        sb.AppendLine("| upper | prefab | lower | prefab | x | z | CS1 Δy m | CS2 Δelev before | upper elev after | action |").AppendLine("|---|---|---|---|---:|---:|---:|---:|---:|---|");
        foreach (var c in r4.CrossingList.OrderBy(c => c.Action).ThenBy(c => c.X))
            sb.AppendLine(string.Create(ci, $"| {c.UpperEdge} | {c.UpperPrefab} | {c.LowerEdge} | {c.LowerPrefab} | {c.X:F0} | {c.Z:F0} | {c.Cs1Dy:F1} | {c.SeparationBefore:F1} | {c.UpperElevationAfter:F1} | {c.Action} |"));
        sb.AppendLine();
        if (r4.RampTails.Count > 0)
        {
            sb.AppendLine("## Ramp tails snapped").AppendLine();
            sb.AppendLine("| node | ramp edge | attached to | distance m | x | z |").AppendLine("|---|---|---|---:|---:|---:|");
            foreach (var t in r4.RampTails) sb.AppendLine(string.Create(ci, $"| {t.Node} | {t.Edge} | {t.TargetEdge} | {t.Distance:F1} | {t.X:F0} | {t.Z:F0} |"));
        }
        return sb.ToString();
    }

    public static void WriteOutside(string path, ConvertResult r, double half)
    {
        var borders = Layers.Imported.SelectMany(l => r.NodesOf(r.Layer(l)).Where(n => n.Border).Select(n => (Layer: l, Node: n,
            Prefab: r.Layer(l).First(e => e.A == n || e.B == n).Map.Cs2Prefab!))).ToList();
        using var fs = File.Create(path);
        using var w = Writer(fs);
        w.WriteStartObject();
        w.WriteString("_comment", "borderNodes: network ends cut at the CS2 border (±7168 m); these become CS2 outside connections. cs1Buildings: CS1 outside-connection buildings (they sit at ±8640 m) with the nearest border node of the matching layer.");
        w.WriteStartArray("borderNodes");
        foreach (var (layer, n, prefab) in borders)
        {
            w.WriteStartObject();
            w.WriteString("layer", layer); w.WriteString("node", n.Id); w.WriteString("prefab", prefab);
            w.WriteString("side", Side(n.Position));
            w.WritePropertyName("position"); P(w, n.Position);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("cs1Buildings");
        foreach (var b in r.Dump.OutsideConnections)
        {
            w.WriteStartObject();
            w.WriteNumber("id", b.Id); w.WriteString("prefab", b.Prefab); w.WriteString("type", b.Kind);
            w.WriteBoolean("incoming", b.Incoming); w.WriteBoolean("outgoing", b.Outgoing);
            w.WritePropertyName("position"); P(w, b.Position);
            w.WriteString("side", Side(b.Position));
            string? layer = b.Kind switch { "train" => Layers.Rail, "road" => null, _ => "" };
            var cands = borders.Where(x => layer == null ? x.Layer is Layers.Highways or Layers.Roads : x.Layer == layer).ToList();
            if (layer != "" && cands.Count > 0)
            {
                var best = cands.MinBy(x => x.Node.Position.DistXZ(b.Position));
                w.WriteStartObject("nearestBorderNode");
                w.WriteString("node", best.Node.Id); w.WriteString("layer", best.Layer); w.WriteString("prefab", best.Prefab);
                w.WriteNumber("distance", R2(best.Node.Position.DistXZ(b.Position)));
                w.WriteEndObject();
            }
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    static string Side(Vec3 p) => Math.Abs(p.X) >= Math.Abs(p.Z) ? (p.X > 0 ? "east" : "west") : (p.Z > 0 ? "north" : "south");

    public static void WriteGeoJson(string path, ConvertResult r)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs);
        w.WriteStartObject();
        w.WriteString("type", "FeatureCollection");
        w.WriteString("name", "dakota-falls-cs2-networks");
        w.WriteStartObject("crs"); w.WriteString("type", "name"); w.WriteStartObject("properties"); w.WriteString("name", "CS2 world metres, x east, z north (y omitted)"); w.WriteEndObject(); w.WriteEndObject();
        w.WriteStartArray("features");
        foreach (var e in r.Edges)
        {
            w.WriteStartObject();
            w.WriteString("type", "Feature");
            w.WriteStartObject("geometry"); w.WriteString("type", "LineString");
            w.WriteStartArray("coordinates");
            for (int i = 0; i <= 12; i++)
            {
                var p = e.Curve.Eval(i / 12.0);
                w.WriteStartArray(); w.WriteNumberValue(R2(p.X)); w.WriteNumberValue(R2(p.Z)); w.WriteEndArray();
            }
            w.WriteEndArray(); w.WriteEndObject();
            w.WriteStartObject("properties");
            w.WriteString("id", e.Id); w.WriteString("layer", e.Layer);
            w.WriteString("cs1_prefab", e.Map.Cs1Prefab);
            if (e.Map.Cs2Prefab != null) w.WriteString("cs2_prefab", e.Map.Cs2Prefab); else w.WriteNull("cs2_prefab");
            w.WriteString("structure", e.Structure.ToString().ToLowerInvariant());
            w.WriteNumber("cs1_segment", e.Cs1Segment);
            w.WriteString("a", e.A.Id); w.WriteString("b", e.B.Id);
            w.WriteNumber("elev_a", e.A.Elevation); w.WriteNumber("elev_b", e.B.Elevation);
            w.WriteNumber("width", R2(e.Info.HalfWidth * 2));
            w.WriteEndObject();
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    /// <summary>Overlay class colours; null = not drawn.</summary>
    public static (byte, byte, byte, byte)? ClassColor(OutEdge e)
    {
        var p = e.Map.Cs2Prefab ?? "";
        return e.Layer switch
        {
            Layers.Highways => p.EndsWith("1 lane") ? ((byte)255, (byte)170, (byte)60, (byte)255) : ((byte)255, (byte)120, (byte)0, (byte)255),
            Layers.Rail => ((byte)170, (byte)60, (byte)220, (byte)255),
            Layers.Roads => p.StartsWith("Gravel") ? ((byte)190, (byte)150, (byte)100, (byte)255)
                : p.StartsWith("Small") ? ((byte)255, (byte)255, (byte)255, (byte)255) : ((byte)255, (byte)230, (byte)60, (byte)255),
            Layers.Power => ((byte)40, (byte)200, (byte)255, (byte)200),
            _ => null,
        };
    }

    public static void WriteOverlay(string path, ConvertResult r, int size, double metresPerPixel) =>
        RenderOverlay(r, size, metresPerPixel).SavePng(path);

    public static Canvas RenderOverlay(ConvertResult r, int size, double metresPerPixel)
    {
        var c = new Canvas(size, size);
        double half = size * metresPerPixel / 2;
        string[] order = [Layers.Power, Layers.Roads, Layers.Rail, Layers.Highways];
        foreach (var layer in order)
            foreach (var e in r.Layer(layer))
            {
                if (ClassColor(e) is not { } col) continue;
                double hw = layer == Layers.Power ? 0.6 : Math.Max(0.9, e.Info.HalfWidth / metresPerPixel);
                const int n = 12;
                var prev = e.Curve.A;
                for (int i = 1; i <= n; i++)
                {
                    var p = e.Curve.Eval(i / (double)n);
                    c.Line((prev.X + half) / metresPerPixel, (half - prev.Z) / metresPerPixel,
                           (p.X + half) / metresPerPixel, (half - p.Z) / metresPerPixel, hw, col);
                    prev = p;
                }
            }
        return c;
    }

    /// <summary>Isoperimetric quotient 4πA/P²: 1 for a circle, ≈ 0.785 for a square, low for merged blobs and strips.</summary>
    public static double Roundness(List<(double X, double Z)> ring)
    {
        double per = 0;
        for (int i = 0; i < ring.Count; i++) { var a = ring[i]; var b = ring[(i + 1) % ring.Count]; per += Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z)); }
        return per > 0 ? 4 * Math.PI * Math.Abs(Surfaces.Area(ring)) / (per * per) : 0;
    }

    /// <summary>The overlay with polygons drawn on top (red outlines, 2 px), for checking surfaces.</summary>
    public static void WritePolygonCheck(string path, ConvertResult r, IEnumerable<List<(double X, double Z)>> polygons, int size, double metresPerPixel)
    {
        var c = RenderOverlay(r, size, metresPerPixel);
        double half = size * metresPerPixel / 2;
        foreach (var ring in polygons)
            for (int i = 0; i < ring.Count; i++)
            {
                var a = ring[i]; var b = ring[(i + 1) % ring.Count];
                c.Line((a.X + half) / metresPerPixel, (half - a.Z) / metresPerPixel, (b.X + half) / metresPerPixel, (half - b.Z) / metresPerPixel, 1.2, (230, 30, 30, 255));
            }
        c.SavePng(path);
    }

    // ---------------------------------------------------------------- inventory and stats

    /// <summary>m_averageLength, or the Bézier length where the game left it 0 (fields, walls, terraforming nets).</summary>
    static double SegLen(DumpSegment s) => s.Length > 0 ? s.Length : s.Bezier.LengthXZ();

    public static string Inventory(Dump d)
    {
        var sb = new StringBuilder();
        var ci = CultureInfo.InvariantCulture;
        sb.AppendLine("## Inventory (CS1 dump)").AppendLine();
        sb.AppendLine($"Nodes {d.Nodes.Count}, segments {d.Segments.Count}, net prefabs {d.NetPrefabs.Count}, water sources {d.WaterSources.Count}, outside connections {d.OutsideConnections.Count}.").AppendLine();
        sb.AppendLine("### Segments per prefab").AppendLine();
        sb.AppendLine("| CS1 prefab | AI | segments | length km |").AppendLine("|---|---|---:|---:|");
        foreach (var g in d.Segments.GroupBy(s => s.Prefab).OrderByDescending(g => g.Sum(SegLen)))
            sb.AppendLine(string.Create(ci, $"| {g.Key} | {d.PrefabInfo(g.Key).Ai} | {g.Count()} | {g.Sum(SegLen) / 1000:F2} |"));
        sb.AppendLine();

        sb.AppendLine("### Node elevation").AppendLine();
        sb.AppendLine("CS1 `elevation` byte and measured y − terrain:").AppendLine();
        sb.AppendLine("| bucket | elevation byte | y − terrain |").AppendLine("|---|---:|---:|");
        (string, Func<double, bool>)[] buckets =
        [
            ("< -20", v => v < -20), ("-20..-5", v => v >= -20 && v < -5), ("-5..-0.5", v => v >= -5 && v < -0.5),
            ("±0.5", v => Math.Abs(v) <= 0.5), ("0.5..5", v => v > 0.5 && v <= 5), ("5..10", v => v > 5 && v <= 10),
            ("10..20", v => v > 10 && v <= 20), ("20..40", v => v > 20 && v <= 40), ("> 40", v => v > 40),
        ];
        foreach (var (label, f) in buckets)
            sb.AppendLine($"| {label} | {d.Nodes.Values.Count(n => f(n.Elevation))} | {d.Nodes.Values.Count(n => f(n.Position.Y - n.Terrain))} |");
        sb.AppendLine();
        int under = d.Nodes.Values.Count(n => n.Has("Underground")), outside = d.Nodes.Values.Count(n => n.Has("Outside"));
        int outOfSquare = d.Nodes.Values.Count(n => Clipper.Outside(n.Position, 7168) > 0);
        sb.AppendLine($"Underground nodes {under}; nodes with Outside flag {outside}; nodes beyond ±7168 m {outOfSquare}.").AppendLine();

        sb.AppendLine("### Water sources").AppendLine();
        foreach (var g in d.WaterSources.GroupBy(w => w.Type))
            sb.AppendLine(string.Create(ci, $"- {g.Key}: {g.Count()} (inside ±7168 m: {g.Count(w => Clipper.Outside(w.Output, 7168) <= 0)}; height {g.Min(w => w.TargetHeight):F1}–{g.Max(w => w.TargetHeight):F1} m)"));
        sb.AppendLine();
        sb.AppendLine("### Outside connections (CS1 buildings)").AppendLine();
        foreach (var g in d.OutsideConnections.GroupBy(b => b.Kind).OrderBy(g => g.Key))
            sb.AppendLine($"- {g.Key}: {g.Count()} (in {g.Count(b => b.Incoming)}, out {g.Count(b => b.Outgoing)})");
        sb.AppendLine();
        return sb.ToString();
    }

    public static string Stats(ConvertResult r, string dumpPath, string mappingPath)
    {
        var sb = new StringBuilder();
        var ci = CultureInfo.InvariantCulture;
        var st = r.Stats;
        sb.AppendLine("# net-converter stats").AppendLine();
        sb.AppendLine($"Dump `{dumpPath}`, mapping `{mappingPath}`.").AppendLine();
        sb.AppendLine("## Output layers").AppendLine();
        sb.AppendLine("| layer | edges | nodes | border nodes | length km | ground | elevated | tunnel |").AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var layer in new[] { Layers.Highways, Layers.Rail, Layers.Roads, Layers.Power, Layers.Skip, Layers.Unmapped })
        {
            var es = r.Layer(layer).ToList();
            if (es.Count == 0 && !Layers.Imported.Contains(layer)) continue;
            var ns = r.NodesOf(es).ToList();
            sb.AppendLine(string.Create(ci, $"| {layer} | {es.Count} | {ns.Count} | {ns.Count(n => n.Border)} | {es.Sum(e => e.Curve.LengthXZ()) / 1000:F1} | {es.Count(e => e.Structure == Structure.Ground)} | {es.Count(e => e.Structure == Structure.Elevated)} | {es.Count(e => e.Structure == Structure.Tunnel)} |"));
        }
        sb.AppendLine();
        sb.AppendLine("Only highways, rail and roads are written as import files. power, skip and unmapped appear in network.geojson only.").AppendLine();
        sb.AppendLine("## Transform").AppendLine();
        sb.AppendLine($"- segments in {st.SegmentsIn}; wholly outside ±7168 m {st.SegmentsOutside}; clipped at the border {st.SegmentsClipped} (→ {st.PiecesFromClip} pieces)");
        sb.AppendLine($"- split for > 1500 m: {st.SplitForLength}; node merges (< 0.5 m): {st.MergedNodes}; short edges (< 4 m) collapsed into one node: {st.CollapsedShort}; degenerate/duplicate edges dropped: {st.DroppedDegenerate}");
        sb.AppendLine();

        sb.AppendLine("## CS2 prefabs used").AppendLine();
        sb.AppendLine("| layer | CS2 prefab | edges | length km | from CS1 prefabs |").AppendLine("|---|---|---:|---:|---|");
        foreach (var g in r.Edges.Where(e => e.Map.Imported).GroupBy(e => (e.Layer, e.Map.Cs2Prefab)).OrderBy(g => g.Key.Layer).ThenBy(g => g.Key.Cs2Prefab))
            sb.AppendLine(string.Create(ci, $"| {g.Key.Layer} | {g.Key.Cs2Prefab} | {g.Count()} | {g.Sum(e => e.Curve.LengthXZ()) / 1000:F1} | {string.Join(", ", g.Select(e => e.Map.Cs1Prefab).Distinct())} |"));
        sb.AppendLine();

        sb.AppendLine("## Unmapped CS1 prefabs (need a human decision)").AppendLine();
        var unmapped = r.Maps.Values.Where(m => m.Layer == Layers.Unmapped).ToList();
        if (unmapped.Count == 0) sb.AppendLine("None: every network prefab in the dump matched a rule.");
        else
        {
            sb.AppendLine("| CS1 prefab | AI | segments | length km | suggestion |").AppendLine("|---|---|---:|---:|---|");
            foreach (var m in unmapped.OrderByDescending(m => r.Dump.Segments.Where(s => s.Prefab == m.Cs1Prefab).Sum(SegLen)))
            {
                var segs = r.Dump.Segments.Where(s => s.Prefab == m.Cs1Prefab).ToList();
                sb.AppendLine(string.Create(ci, $"| {m.Cs1Prefab} | {r.Dump.PrefabInfo(m.Cs1Prefab).Ai} | {segs.Count} | {segs.Sum(SegLen) / 1000:F2} | {m.Suggestion} |"));
            }
        }
        sb.AppendLine();

        sb.AppendLine("## Mapping decisions flagged in mapping.tsv").AppendLine();
        foreach (var m in r.Maps.Values.Where(m => m.Rule?.Note.Contains("DECISION") == true || m.Rule?.Note.Contains("no exact") == true))
            sb.AppendLine($"- `{m.Cs1Prefab}` → {m.Cs2Prefab ?? "SKIP"}: {m.Rule!.Note}");
        sb.AppendLine();

        sb.AppendLine("## Full prefab mapping").AppendLine();
        sb.AppendLine("| CS1 prefab | segments | → CS2 | layer | rule line |").AppendLine("|---|---:|---|---|---:|");
        foreach (var m in r.Maps.Values.OrderBy(m => m.Layer).ThenBy(m => m.Cs1Prefab))
            sb.AppendLine($"| {m.Cs1Prefab} | {r.Dump.Segments.Count(s => s.Prefab == m.Cs1Prefab)} | {m.Cs2Prefab ?? (m.Layer == Layers.Unmapped ? "UNMAPPED" : "SKIP")} | {m.Layer} | {m.Rule?.Line.ToString() ?? "-"} |");
        sb.AppendLine();
        sb.Append(Inventory(r.Dump));
        return sb.ToString();
    }
}
