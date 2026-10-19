using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Cslmap2E2c;

public static class Writers
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    static string G(double d) => d.ToString("0.#########", Inv);

    /// <summary>OSM XML in the shape E2C's OSM-to-roads.json step reads: &lt;osm&gt;, &lt;bounds&gt;, nodes with lat/lon, ways with nd refs and tags.</summary>
    public static void WriteOsm(string path, Network net, E2cProjection proj)
    {
        var (top, right, bottom, left) = proj.PlayableBounds();
        var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
        using var w = XmlWriter.Create(path, settings);
        w.WriteStartDocument();
        w.WriteStartElement("osm");
        w.WriteAttributeString("version", "0.6");
        w.WriteAttributeString("generator", "cslmap2e2c");
        w.WriteStartElement("bounds");
        w.WriteAttributeString("minlat", G(bottom));
        w.WriteAttributeString("minlon", G(left));
        w.WriteAttributeString("maxlat", G(top));
        w.WriteAttributeString("maxlon", G(right));
        w.WriteEndElement();

        foreach (var (id, p) in net.Nodes.OrderBy(kv => kv.Key))
        {
            var (lat, lon) = proj.ToGeo(p.X, p.Z);
            w.WriteStartElement("node");
            w.WriteAttributeString("id", id.ToString(Inv));
            w.WriteAttributeString("lat", G(lat));
            w.WriteAttributeString("lon", G(lon));
            w.WriteEndElement();
        }

        foreach (var way in net.Ways)
        {
            w.WriteStartElement("way");
            w.WriteAttributeString("id", way.Id.ToString(Inv));
            foreach (var nd in way.NodeIds)
            {
                w.WriteStartElement("nd");
                w.WriteAttributeString("ref", nd.ToString(Inv));
                w.WriteEndElement();
            }
            Tag(w, "highway", way.Class.OsmHighway()!);
            if (way.OneWay) Tag(w, "oneway", "yes");
            Tag(w, "lanes", way.Lanes.ToString(Inv));
            if (!string.IsNullOrWhiteSpace(way.Name)) Tag(w, "name", way.Name!);
            Tag(w, "cs1:prefab", way.Prefab);
            Tag(w, "cs1:lanes", way.SourceLanes.ToString(Inv));
            if (way.Structure != Structure.Ground) Tag(w, "cs1:structure", way.Structure.ToString().ToLowerInvariant());
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteEndDocument();
    }

    static void Tag(XmlWriter w, string k, string v)
    {
        w.WriteStartElement("tag");
        w.WriteAttributeString("k", k);
        w.WriteAttributeString("v", v);
        w.WriteEndElement();
    }

    /// <summary>
    /// roads.json in E2C's schema. E2C regenerates this file from overpass_roads.osm on every "Import Roads"
    /// click, so this copy is for inspection and for the hidden "Import Terrain Then Roads" path only.
    /// </summary>
    public static void WriteRoadsJson(string path, Network net, E2cProjection proj)
    {
        var refCount = new Dictionary<long, int>();
        foreach (var w in net.Ways)
            foreach (var id in w.NodeIds)
                refCount[id] = refCount.TryGetValue(id, out var c) ? c + 1 : 1;

        var (top, right, bottom, left) = proj.PlayableBounds();
        using var fs = File.Create(path);
        using var j = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });
        j.WriteStartObject();
        j.WriteStartObject("meta");
        j.WriteString("source", "cslmap2e2c");
        j.WriteString("crs", "WGS84");
        j.WriteStartArray("bbox");
        j.WriteNumberValue(left); j.WriteNumberValue(bottom); j.WriteNumberValue(right); j.WriteNumberValue(top);
        j.WriteEndArray();
        j.WriteEndObject();
        j.WriteStartArray("roads");
        foreach (var w in net.Ways)
        {
            j.WriteStartObject();
            j.WriteString("id", w.Id.ToString(Inv));
            if (string.IsNullOrWhiteSpace(w.Name)) j.WriteNull("name"); else j.WriteString("name", w.Name);
            j.WriteString("category", w.Class.E2cCategory());
            j.WriteBoolean("one_way", w.OneWay);
            j.WriteNumber("lanes", w.Lanes);
            j.WriteNull("speed_kph");
            j.WriteStartArray("points");
            foreach (var id in w.NodeIds)
            {
                var p = net.Nodes[id];
                var (lat, lon) = proj.ToGeo(p.X, p.Z);
                j.WriteStartObject();
                j.WriteNumber("lat", lat);
                j.WriteNumber("lon", lon);
                if (refCount[id] >= 2) j.WriteBoolean("junction", true);
                j.WriteEndObject();
            }
            j.WriteEndArray();
            j.WriteEndObject();
        }
        j.WriteEndArray();
        j.WriteEndObject();
    }

    /// <summary>Rail segments as GeoJSON LineStrings in CS2 game metres [x, z, y] (no CRS; y is the CS1 height).</summary>
    public static void WriteRailGeoJson(string path, Network net, double scale)
    {
        using var fs = File.Create(path);
        using var j = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });
        j.WriteStartObject();
        j.WriteString("type", "FeatureCollection");
        j.WriteString("name", "cs1-rail");
        j.WriteStartObject("properties_doc");
        j.WriteString("coordinates", "[x, z, y] in CS2 game metres; x,z scaled from CS1 by the factor below, y is the CS1 height in metres");
        j.WriteNumber("scale", scale);
        j.WriteEndObject();
        j.WriteStartArray("features");
        foreach (var r in net.Rail)
        {
            j.WriteStartObject();
            j.WriteString("type", "Feature");
            j.WriteStartObject("geometry");
            j.WriteString("type", "LineString");
            j.WriteStartArray("coordinates");
            foreach (var p in r.Points)
            {
                j.WriteStartArray();
                j.WriteNumberValue(Math.Round(p.X, 3)); j.WriteNumberValue(Math.Round(p.Z, 3)); j.WriteNumberValue(Math.Round(p.Y, 3));
                j.WriteEndArray();
            }
            j.WriteEndArray();
            j.WriteEndObject();
            j.WriteStartObject("properties");
            j.WriteNumber("cs1_segment", r.SegmentId);
            j.WriteNumber("cs1_start_node", r.StartNode);
            j.WriteNumber("cs1_end_node", r.EndNode);
            j.WriteString("prefab", r.Prefab);
            j.WriteString("icls", r.ItemClass);
            j.WriteString("structure", r.Structure.ToString().ToLowerInvariant());
            j.WriteEndObject();
            j.WriteEndObject();
        }
        j.WriteEndArray();
        j.WriteEndObject();
    }

    public static string FormatStats(Network net, ConvertOptions opt)
    {
        var s = net.Stats;
        var sb = new StringBuilder();
        sb.AppendLine($"source: {net.Source}   scale: {opt.Scale}   playable: {opt.PlayableWidth} m");
        sb.AppendLine($"segments in file: {s.SegmentsTotal}");
        sb.AppendLine("segments per class:");
        foreach (var (k, v) in s.SegmentsByClass) sb.AppendLine($"  {k,-14} {v,7}");
        sb.AppendLine($"road ways written: {net.Ways.Count}   rail lines: {net.Rail.Count}   OSM nodes: {net.Nodes.Count}");
        sb.AppendLine($"straight pieces E2C should queue (compare with its log line \"Queued N road segments\"): {net.Ways.Sum(w => w.NodeIds.Count - 1)}");
        sb.AppendLine($"road structures (E2C lays all on terrain): {string.Join(", ", s.StructureCounts.Select(kv => $"{kv.Key}={kv.Value}"))}");
        if (s.DroppedTunnels > 0) sb.AppendLine($"tunnels dropped: {s.DroppedTunnels}");
        if (opt.Clip) sb.AppendLine($"clipped to ±{opt.PlayableWidth / 2} m: road segments cut at the border {s.ClippedAtBorder}, entirely outside (dropped) {s.ClippedAway}; rail segments outside {s.RailClippedAway}");
        sb.AppendLine($"one-way ways reversed to forward direction: {s.ReversedOneWays}");
        sb.AppendLine($"points: {s.PointsIn} in, {s.PointsOut} out after simplify   degenerate segments: {s.DegenerateSegments}");
        sb.AppendLine($"E2C risks: nodes outside playable square (clamped) {s.PointsOutsidePlayable}; edges within 600 m of border (forced to Highway prefab) {s.EdgesNearBoundary}; edges < 6 m (snap merge) {s.EdgesShorterThanSnap}");
        sb.AppendLine("predicted E2C prefab per written (category, direction, lanes) [inferred from its resolver, verify in game]:");
        foreach (var (k, v) in s.PredictedE2cPrefab.OrderByDescending(kv => kv.Value)) sb.AppendLine($"  {v,7}  {k}");
        sb.AppendLine("road prefabs used:");
        foreach (var (k, v) in s.RoadPrefabs.OrderByDescending(kv => kv.Value)) sb.AppendLine($"  {v,7}  {k}");
        sb.AppendLine("skipped prefabs:");
        foreach (var (k, v) in s.SkippedPrefabs.OrderByDescending(kv => kv.Value)) sb.AppendLine($"  {v,7}  {k}");
        if (s.UnmatchedPrefabs.Count > 0)
        {
            sb.AppendLine("unmatched prefabs (no rule; skipped; add a --mapping rule if they matter):");
            foreach (var (k, v) in s.UnmatchedPrefabs.OrderByDescending(kv => kv.Value)) sb.AppendLine($"  {v,7}  {k}");
        }
        return sb.ToString();
    }

    public static string FormatSettings(E2cProjection proj, ConvertOptions opt)
    {
        var (t, r, b, l) = proj.SettingsBbox();
        return $"""
            Earth2Cities settings for this file (Options > Earth2Cities > Main):
              Overpass Top (North Lat)    {t}
              Overpass Right (East Lon)   {r}
              Overpass Bottom (South Lat) {b}
              Overpass Left (West Lon)    {l}
            Only the bbox centre ({proj.CenterLat.ToString(Inv)}, {proj.CenterLon.ToString(Inv)}) matters; it must equal the converter origin.
            Do NOT click "Step 1: Open BBox Map Tool" afterwards: it resets these four fields to the Atlantic default.
            Origin Lat/Lon, Origin X/Z and Default Scale are ignored by E2C in this mode.
            Map size assumed: {opt.PlayableWidth} m playable (E2C MapSizeKm 14).
            Copy overpass_roads.osm into the Earth2Cities mod folder, then click "Step 4: Import Roads".
            """;
    }
}
