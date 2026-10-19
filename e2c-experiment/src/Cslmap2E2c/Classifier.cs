using System.Text.RegularExpressions;

namespace Cslmap2E2c;

/// <summary>Output classes. Road classes are written as OSM highway values; Rail goes to GeoJSON; Skip is dropped.</summary>
public enum NetClass { Skip, Rail, Motorway, MotorwayLink, Trunk, Primary, Secondary, Tertiary, Residential }

public enum Structure { Ground, Elevated, Bridge, Tunnel, Slope }

public sealed record MappingRule(string Pattern, NetClass Class, string Source)
{
    public Regex Regex { get; } = new(Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}

public static class NetClassExt
{
    public static string? OsmHighway(this NetClass c) => c switch
    {
        NetClass.Motorway => "motorway",
        NetClass.MotorwayLink => "motorway_link",
        NetClass.Trunk => "trunk",
        NetClass.Primary => "primary",
        NetClass.Secondary => "secondary",
        NetClass.Tertiary => "tertiary",
        NetClass.Residential => "residential",
        _ => null,
    };

    /// <summary>Category Earth2Cities derives from the OSM highway value (see E2C_CONTRACT.md §3).</summary>
    public static string? E2cCategory(this NetClass c) => c switch
    {
        NetClass.Motorway or NetClass.MotorwayLink or NetClass.Trunk => "motorway",
        NetClass.Primary => "primary",
        NetClass.Secondary => "secondary",
        NetClass.Tertiary => "tertiary",
        NetClass.Residential => "residential",
        _ => null,
    };

    public static bool IsRoad(this NetClass c) => c.OsmHighway() != null;

    public static NetClass ParseClass(string s) => s.Trim().ToLowerInvariant() switch
    {
        "skip" => NetClass.Skip,
        "rail" => NetClass.Rail,
        "motorway" => NetClass.Motorway,
        "motorway_link" => NetClass.MotorwayLink,
        "trunk" => NetClass.Trunk,
        "primary" => NetClass.Primary,
        "secondary" => NetClass.Secondary,
        "tertiary" => NetClass.Tertiary,
        "residential" => NetClass.Residential,
        _ => throw new FormatException($"Unknown class '{s}'"),
    };
}

/// <summary>
/// Prefab-name to class table. Rules are tested in order against "icls|basename" where basename is the
/// prefab name without the Workshop id prefix, the "_Data" suffix and the CS1 variant suffix
/// (" E", " B", " T", " S", " 0" ...). First match wins. User rules from --mapping are tried first.
/// </summary>
public sealed class Classifier
{
    // Seeded from the 552-prefab classification in dakota-falls-migration-analysis.md §2.2.
    static readonly (string Pattern, NetClass Class)[] DefaultRules =
    [
        // Decorative and non-road services first, so "Highway Guardrail" etc. never become roads.
        (@"^(Beautification Item|Pedestrian|Electricity|Landscaping|Airplane|Ship|Monorail|Metro|Cable ?Car|Bus Stop|Water|Fishing)", NetClass.Skip),
        (@"\|.*(fence|wall|barrier|guard ?rail|curb|kerb|parking|terraform|field|surface|sidewalk|pavement|path|power|kV|wire|pylon|IMT|decal|marking|invisible|quay|canal|bike|cycle|pedestrian|monorail|tram track|runway|taxiway)", NetClass.Skip),
        // Rail family (R2USA, R69, R0, BIG Railroads, vanilla Train Track, station tracks).
        (@"^Train Track", NetClass.Rail),
        (@"\|.*(R2USA|R69|\bR0\b|rail|train track|station track|yard track)", NetClass.Rail),
        // Highways. National roads carry icls "Highway" in USRP, so they are matched before the icls rule.
        (@"\|.*national road", NetClass.Primary),
        (@"\|.*(interstate|highway|freeway|motorway).*ramp|\|.*\bramp\b", NetClass.MotorwayLink),
        (@"\|.*(interstate|highway|freeway|motorway)", NetClass.Motorway),
        (@"^Highway", NetClass.Motorway),
        // Vanilla-style roads.
        (@"\|.*(six-lane|6-lane|large road|large avenue)", NetClass.Primary),
        (@"\|.*(four-lane|4-lane|medium road|avenue|boulevard)", NetClass.Secondary),
        (@"\|.*(basic road|two-lane|2-lane|small road|gravel road|oneway road|one-way road|asymmetric road|busway|tram road|road)", NetClass.Residential),
    ];

    readonly List<MappingRule> _rules = new();

    public IReadOnlyList<MappingRule> Rules => _rules;

    public Classifier(IEnumerable<MappingRule>? userRules = null)
    {
        if (userRules != null) _rules.AddRange(userRules);
        _rules.AddRange(DefaultRules.Select(r => new MappingRule(r.Pattern, r.Class, "default")));
    }

    /// <summary>Reads "regex&lt;TAB&gt;class" lines; '#' starts a comment.</summary>
    public static List<MappingRule> LoadRules(string path)
    {
        var list = new List<MappingRule>();
        foreach (var (raw, i) in File.ReadAllLines(path).Select((l, i) => (l, i + 1)))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new FormatException($"{path}:{i}: expected 'regex<TAB>class'");
            list.Add(new MappingRule(parts[0], NetClassExt.ParseClass(parts[1]), $"{Path.GetFileName(path)}:{i}"));
        }
        return list;
    }

    static readonly Regex WorkshopPrefix = new(@"^\d+\.", RegexOptions.CultureInvariant);
    static readonly Regex VariantSuffix = new(@"\s+(E|B|T|S|E\d|B\d|T\d|S\d|0|Elevated|Bridge|Tunnel|Slope)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Splits a CS1 prefab name into its base name and the structure variant encoded in the suffix.</summary>
    public static (string BaseName, Structure Structure) SplitPrefab(string prefab, string itemClass)
    {
        var name = WorkshopPrefix.Replace(prefab, "");
        if (name.EndsWith("_Data", StringComparison.Ordinal)) name = name[..^5];
        var structure = Structure.Ground;
        var m = VariantSuffix.Match(name);
        if (m.Success)
        {
            structure = char.ToUpperInvariant(m.Groups[1].Value[0]) switch
            {
                'E' => Structure.Elevated,
                'B' => Structure.Bridge,
                'T' => Structure.Tunnel,
                'S' => Structure.Slope,
                _ => Structure.Ground,
            };
            name = name[..m.Index];
        }
        if (itemClass.Contains("Tunnel", StringComparison.OrdinalIgnoreCase)) structure = Structure.Tunnel;
        return (name.Trim(), structure);
    }

    public (NetClass Class, MappingRule? Rule) Classify(string itemClass, string baseName)
    {
        var key = itemClass + "|" + baseName;
        foreach (var r in _rules)
            if (r.Regex.IsMatch(key)) return (r.Class, r);
        return (NetClass.Skip, null);
    }

    /// <summary>Counts car lanes per direction from the SegmentTypes lane list; falls back to the name.</summary>
    public static (int Forward, int Backward) CountLanes(IReadOnlyList<CslLane>? lanes, string baseName)
    {
        int f = 0, b = 0;
        if (lanes != null)
            foreach (var l in lanes)
            {
                if (l.Type != "Vehicle" || l.VehicleType != "Car") continue;
                if (l.Direction is "Forward" or "Both") f++;
                if (l.Direction is "Backward" or "Both") b++;
            }
        if (f + b > 0) return (f, b);

        // Name fallbacks: "USRP 3L Interstate" (one-way), "Four-Lane Road" (two-way).
        var m = Regex.Match(baseName, @"\b(\d)L\b");
        if (m.Success) return (int.Parse(m.Groups[1].Value), 0);
        if (Regex.IsMatch(baseName, "one-?way", RegexOptions.IgnoreCase)) return (Regex.IsMatch(baseName, "large|six", RegexOptions.IgnoreCase) ? 3 : 2, 0);
        if (Regex.IsMatch(baseName, "six|6-lane|large", RegexOptions.IgnoreCase)) return (3, 3);
        if (Regex.IsMatch(baseName, "four|4-lane|medium", RegexOptions.IgnoreCase)) return (2, 2);
        return (1, 1);
    }
}
