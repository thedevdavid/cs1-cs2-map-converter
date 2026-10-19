using System.Globalization;
using System.Text;

namespace NetConverter;

public sealed record DeadEnd(string Source, Vec3 Position, string StubPrefab, string Reason, string Detail);

/// <summary>Explains network ends that are not at the border from the CS1 dump and the converter's decisions.</summary>
public static class DeadEnds
{
    public const double BorderMargin = 60, Radius = 2;

    static int Rank(string prefab) =>
        prefab.StartsWith("Highway") ? 4 : prefab.Contains("Train") ? 3 : prefab.StartsWith("Large") || prefab.StartsWith("Medium") ? 2 : prefab.StartsWith("Small") ? 1 : 0;

    /// <summary>Why an end exists at p: the CS1 segments meeting within 2 m and what the converter did with each.</summary>
    public static (string Reason, string Detail) Explain(ConvertResult r, Vec3 p)
    {
        var cs1Nodes = r.Dump.Nodes.Values.Where(n => n.Position.DistXZ(p) <= Radius).ToList();
        if (cs1Nodes.Count == 0) return ("no CS1 node here (game split, or merged/moved by the converter)", "");
        var ids = cs1Nodes.Select(n => n.Id).ToHashSet();
        var segs = r.Dump.Segments.Where(s => ids.Contains(s.StartNode) || ids.Contains(s.EndNode)).ToList();
        string Fate(DumpSegment s) => r.Fate.GetValueOrDefault(s.Id, "not converted");
        string Layer(DumpSegment s) => r.Maps.TryGetValue(s.Prefab, out var m) ? m.Layer : "?";
        static string Short(string prefab) => prefab.Contains('.') && char.IsDigit(prefab[0]) ? prefab[(prefab.IndexOf('.') + 1)..] : prefab;
        var detail = string.Join("; ", segs.Select(s => $"{Short(s.Prefab)} [{Layer(s)}, {Fate(s)}]"));
        bool stacked = cs1Nodes.Count > 1 && cs1Nodes.Max(n => n.Position.Y) - cs1Nodes.Min(n => n.Position.Y) >= 2;

        if (segs.Count <= 1) return ("CS1 dead end (genuine)", detail);
        var importedLayers = segs.Where(s => Fate(s).StartsWith("imported")).Select(Layer).Distinct().ToList();
        if (importedLayers.Count > 1) return ($"cross-layer junction ({string.Join(" + ", importedLayers.OrderBy(x => x))}) split over separate import files", detail);
        var skipped = segs.FirstOrDefault(s => !Fate(s).StartsWith("imported") && Fate(s) is "SKIP" or "power" or "unmapped");
        if (skipped != null) return ($"continues as {Fate(skipped)}: {Short(skipped.Prefab)}", detail);
        if (segs.Any(s => Fate(s).StartsWith("collapsed"))) return ("neighbour collapsed (< 4 m)", detail);
        if (segs.Any(s => Fate(s).StartsWith("duplicate"))) return ("neighbour dropped as duplicate", detail);
        if (segs.Any(s => Fate(s) == "outside the border")) return ("continues outside the border", detail);
        if (stacked) return ("stacked crossing (nodes at different heights)", detail);
        return ("connected in the data, not joined in the game", detail);
    }

    public static List<DeadEnd> Live(ConvertResult r, LiveGraph live, double half)
    {
        var stubPrefab = new Dictionary<int, string>();
        foreach (var s in live.Segments) { stubPrefab.TryAdd(s.StartNode, s.Prefab); stubPrefab.TryAdd(s.EndNode, s.Prefab); }
        return live.Nodes.Values
            .Where(n => n.Segments == 1 && Clipper.Outside(n.Position, half - BorderMargin) < 0)
            .Select(n => { var (reason, detail) = Explain(r, n.Position); return new DeadEnd("live", n.Position, stubPrefab.GetValueOrDefault(n.Index, "?"), reason, detail); })
            .ToList();
    }

    /// <summary>Ends of the converter's own output graph (all imported layers together, as in import-all.json).</summary>
    public static List<DeadEnd> Predicted(ConvertResult r, double half)
    {
        var edges = r.Edges.Where(e => e.Map.Imported).ToList();
        var degree = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
        var prefabAt = new Dictionary<OutNode, string>(ReferenceEqualityComparer.Instance);
        foreach (var e in edges)
            foreach (var n in new[] { e.A, e.B }) { degree[n] = degree.GetValueOrDefault(n) + 1; prefabAt.TryAdd(n, e.Map.Cs2Prefab!); }
        return degree.Where(kv => kv.Value == 1 && !kv.Key.Border && Clipper.Outside(kv.Key.Position, half - BorderMargin) < 0)
            .Select(kv => { var (reason, detail) = Explain(r, kv.Key.Position); return new DeadEnd("output", kv.Key.Position, prefabAt[kv.Key], reason, detail); })
            .ToList();
    }

    public static string Report(ConvertResult r, List<DeadEnd> live, List<DeadEnd> predicted, string liveNote)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("# Dead ends").AppendLine();
        sb.AppendLine($"Network ends more than {BorderMargin} m inside the ±7168 m border, explained from the CS1 dump: the CS1 segments meeting within {Radius} m of the end and what the converter did with each.").AppendLine();
        sb.AppendLine(liveNote).AppendLine();

        void Aggregate(string title, List<DeadEnd> list)
        {
            sb.AppendLine($"## {title}: {list.Count}").AppendLine();
            sb.AppendLine("| reason | ends | stub prefabs |").AppendLine("|---|---:|---|");
            foreach (var g in list.GroupBy(d => d.Reason).OrderByDescending(g => g.Count()))
                sb.AppendLine($"| {g.Key} | {g.Count()} | {string.Join(", ", g.GroupBy(d => d.StubPrefab).OrderByDescending(x => x.Count()).Select(x => $"{x.Key} {x.Count()}"))} |");
            sb.AppendLine();
        }

        Aggregate("Live game (round-1 import, before round-3 fixes)", live);
        sb.AppendLine("### 20 worst live dead ends").AppendLine();
        sb.AppendLine("| stub prefab | x | z | reason | CS1 segments here [layer, converter decision] |").AppendLine("|---|---:|---:|---|---|");
        foreach (var d in live.OrderByDescending(d => Rank(d.StubPrefab)).ThenBy(d => Math.Abs(d.Position.X) + Math.Abs(d.Position.Z)).Take(20))
            sb.AppendLine(string.Create(ci, $"| {d.StubPrefab} | {d.Position.X:F0} | {d.Position.Z:F0} | {d.Reason} | {d.Detail} |"));
        sb.AppendLine();

        Aggregate("Predicted for the regenerated import-all.json (fresh map)", predicted);
        sb.AppendLine("| stub prefab | x | z | reason | CS1 segments here |").AppendLine("|---|---:|---:|---|---|");
        foreach (var d in predicted.Where(d => d.Reason != "CS1 dead end (genuine)").OrderByDescending(d => Rank(d.StubPrefab)).Take(30))
            sb.AppendLine(string.Create(ci, $"| {d.StubPrefab} | {d.Position.X:F0} | {d.Position.Z:F0} | {d.Reason} | {d.Detail} |"));
        sb.AppendLine();
        return sb.ToString();
    }
}
