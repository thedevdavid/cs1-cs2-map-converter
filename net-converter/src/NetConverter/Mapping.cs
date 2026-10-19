using System.Text.RegularExpressions;

namespace NetConverter;

public static class Layers
{
    public const string Highways = "highways", Rail = "rail", Roads = "roads", Power = "power", Walls = "walls", ShipPaths = "shippaths", Decor = "decor", Skip = "skip", Unmapped = "unmapped";
    /// <summary>Layers written as their own import files (not part of import-all.json).</summary>
    public static readonly string[] Separate = [Power, Walls, ShipPaths, Decor];
    /// <summary>Separate layers whose nodes sit on the ground (the game places pylons; seaways follow the water).</summary>
    public static readonly string[] Ground = [Power, ShipPaths];
    public static readonly string[] Imported = [Highways, Rail, Roads];
}

public sealed record MappingRule(int Line, Regex Pattern, string Target, string Layer, string Note)
{
    public bool IsSkip => Target.Equals("SKIP", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Result of mapping one CS1 prefab. <see cref="Rule"/> is null when nothing matched.</summary>
public sealed record MapResult(string Cs1Prefab, string? Cs2Prefab, string Layer, MappingRule? Rule, string? Suggestion)
{
    public bool Imported => Cs2Prefab != null && Layers.Imported.Contains(Layer);
}

public sealed class Mapping
{
    public List<MappingRule> Rules { get; } = new();

    public static Mapping Load(string path) => Parse(File.ReadAllLines(path));

    public static Mapping Parse(IEnumerable<string> lines)
    {
        var m = new Mapping(); int n = 0;
        foreach (var raw in lines)
        {
            n++;
            if (string.IsNullOrWhiteSpace(raw) || raw.TrimStart().StartsWith('#')) continue;
            var cols = raw.Split('\t');
            if (cols.Length < 3) throw new FormatException($"mapping line {n}: need pattern<TAB>target<TAB>layer");
            var layer = cols[2].Trim().ToLowerInvariant();
            if (layer is not (Layers.Highways or Layers.Rail or Layers.Roads or Layers.Power or Layers.Walls or Layers.ShipPaths or Layers.Decor or Layers.Skip))
                throw new FormatException($"mapping line {n}: unknown layer '{layer}'");
            m.Rules.Add(new MappingRule(n, new Regex(cols[0], RegexOptions.CultureInvariant), cols[1].Trim(), layer, cols.Length > 3 ? cols[3].Trim() : ""));
        }
        return m;
    }

    public MapResult Map(NetPrefabInfo info)
    {
        foreach (var r in Rules)
            if (r.Pattern.IsMatch(info.Name))
                return new MapResult(info.Name, r.IsSkip ? null : r.Target, r.IsSkip && !Layers.Separate.Contains(r.Layer) ? Layers.Skip : r.Layer, r, null);
        return new MapResult(info.Name, null, Layers.Unmapped, null, Suggest(info));
    }

    /// <summary>Heuristic suggestion for an unmapped prefab, from its AI, service and lane counts. Never applied automatically.</summary>
    public static string Suggest(NetPrefabInfo p)
    {
        int f = p.ForwardLanes, b = p.BackwardLanes, total = f + b;
        string ai = p.Ai;
        if (ai.Contains("Train")) return b == 0 || f == 0 ? "Oneway Train Track" : "Double Train Track";
        if (ai.Contains("PowerLine")) return "SKIP (power)";
        if (ai.Contains("Decoration") || ai.Contains("Wall") || ai.Contains("Canal") || ai.Contains("Quay")) return "SKIP";
        if (ai.Contains("Pedestrian")) return "Pavement Path or SKIP";
        if (ai.StartsWith("Road"))
        {
            if (total == 0) return "SKIP (no vehicle lanes)";
            bool highwayish = p.Name.Contains("Highway", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Interstate", StringComparison.OrdinalIgnoreCase);
            if (b == 0 || f == 0)
            {
                int n = Math.Clamp(Math.Max(f, b), 1, 5);
                return highwayish ? $"Highway Oneway - {n} lane{(n == 1 ? "" : "s")}" : n == 1 ? "Small Road Oneway - 1 lane" : "Small Road Oneway - 3 lanes";
            }
            if (highwayish) return $"Highway Twoway - {Math.Clamp(Math.Max(f, b), 2, 4)} lanes";
            return total <= 2 ? "Small Road" : total <= 4 ? "Medium Road" : "Large Road";
        }
        return "?";
    }
}
