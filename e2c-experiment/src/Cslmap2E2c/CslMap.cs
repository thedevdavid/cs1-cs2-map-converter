using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace Cslmap2E2c;

/// <summary>Subset of the CSL Map View export (CSLExportXML v4.x) that the converter needs.</summary>
public sealed record CslNode(int Id, double X, double Y, double Z, bool Underground, string Service, string SubService);

public sealed record CslSegment(int Id, int StartNode, int EndNode, string ItemClass, string Prefab, string? Name, IReadOnlyList<(double X, double Y, double Z)> Points);

public sealed record CslLane(string Type, string VehicleType, string Direction);

public sealed class CslMap
{
    public string Version { get; init; } = "";
    public string City { get; init; } = "";
    public double SeaLevel { get; init; }
    public Dictionary<int, CslNode> Nodes { get; } = new();
    public List<CslSegment> Segments { get; } = new();
    /// <summary>Lane lists per prefab name, from the SegmentTypes section.</summary>
    public Dictionary<string, List<CslLane>> SegmentTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Loads a .cslmap file; gzip-compressed files (CSL Map View writes .cslmap.gz) are detected by magic bytes.</summary>
    public static CslMap Load(string path)
    {
        using var file = File.OpenRead(path);
        var head = new byte[2];
        int read = file.Read(head, 0, 2);
        file.Position = 0;
        Stream input = read == 2 && head[0] == 0x1f && head[1] == 0x8b ? new GZipStream(file, CompressionMode.Decompress) : file;
        using (input)
            return Parse(XDocument.Load(input));
    }

    public static CslMap Parse(XDocument doc)
    {
        var root = doc.Root ?? throw new InvalidDataException("Empty XML document.");
        if (root.Name.LocalName != "CSLExportXML")
            throw new InvalidDataException($"Expected <CSLExportXML> root, found <{root.Name.LocalName}>.");

        var map = new CslMap
        {
            Version = (string?)root.Attribute("version") ?? "",
            City = (string?)root.Element("City") ?? "",
            SeaLevel = ParseD((string?)root.Element("SeaLevel"), 0),
        };

        foreach (var n in root.Element("Nodes")?.Elements("Node") ?? [])
        {
            var pos = n.Element("Pos");
            if (pos == null) continue;
            int id = (int)n.Attribute("id")!;
            map.Nodes[id] = new CslNode(id,
                ParseD((string?)pos.Attribute("x"), 0), ParseD((string?)pos.Attribute("y"), 0), ParseD((string?)pos.Attribute("z"), 0),
                string.Equals((string?)n.Attribute("ug"), "true", StringComparison.OrdinalIgnoreCase),
                (string?)n.Attribute("srv") ?? "", (string?)n.Attribute("subsrv") ?? "");
        }

        foreach (var s in root.Element("Segments")?.Elements("Seg") ?? [])
        {
            var pts = new List<(double, double, double)>();
            foreach (var p in s.Element("Points")?.Elements("P") ?? [])
                pts.Add((ParseD((string?)p.Attribute("x"), 0), ParseD((string?)p.Attribute("y"), 0), ParseD((string?)p.Attribute("z"), 0)));
            map.Segments.Add(new CslSegment(
                (int)s.Attribute("id")!, (int?)s.Attribute("sn") ?? -1, (int?)s.Attribute("en") ?? -1,
                (string?)s.Attribute("icls") ?? "", (string?)s.Element("Name") ?? "",
                (string?)s.Element("CustomName"), pts));
        }

        foreach (var st in root.Element("SegmentTypes")?.Elements("ST") ?? [])
        {
            var name = (string?)st.Attribute("name");
            if (name == null) continue;
            map.SegmentTypes[name] = (st.Element("Lanes")?.Elements("Lane") ?? [])
                .Select(l => new CslLane((string?)l.Attribute("type") ?? "", (string?)l.Attribute("vType") ?? "", (string?)l.Attribute("dir") ?? ""))
                .ToList();
        }
        return map;
    }

    static double ParseD(string? s, double fallback) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;
}
