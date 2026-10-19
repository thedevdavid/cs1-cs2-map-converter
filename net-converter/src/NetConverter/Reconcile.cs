using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetConverter;

public sealed record LiveSegment(string Key, string Prefab, int StartNode, int EndNode, Vec3 Start, Vec3 End, double Length);
public sealed record LiveNode(int Index, int Version, Vec3 Position, int Segments);

/// <summary>Network pulled from the running game (tools/pull-live-graph.mjs).</summary>
public sealed class LiveGraph
{
    public List<LiveSegment> Segments { get; } = new();
    public Dictionary<int, LiveNode> Nodes { get; } = new();
    /// <summary>Prefabs of the segments meeting at each live node.</summary>
    public Dictionary<int, HashSet<string>> NodePrefabs { get; } = new();

    public static LiveGraph Load(string path)
    {
        var g = new LiveGraph();
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        static Vec3 P(JsonElement e) => new(e.GetProperty("x").GetDouble(), e.GetProperty("y").GetDouble(), e.GetProperty("z").GetDouble());
        foreach (var s in doc.RootElement.GetProperty("segments").EnumerateArray())
        {
            var r = s.GetProperty("road");
            var seg = new LiveSegment($"{r.GetProperty("index").GetInt32()}:{r.GetProperty("version").GetInt32()}",
                s.GetProperty("prefab").GetString() ?? "", s.GetProperty("startNode").GetInt32(), s.GetProperty("endNode").GetInt32(),
                P(s.GetProperty("start")), P(s.GetProperty("end")), s.GetProperty("length").GetDouble());
            g.Segments.Add(seg);
            foreach (var n in new[] { seg.StartNode, seg.EndNode })
            {
                if (!g.NodePrefabs.TryGetValue(n, out var set)) g.NodePrefabs[n] = set = new HashSet<string>();
                set.Add(seg.Prefab);
            }
        }
        foreach (var n in doc.RootElement.GetProperty("nodes").EnumerateArray())
        {
            var id = n.GetProperty("node");
            int index = id.GetProperty("index").GetInt32();
            g.Nodes[index] = new LiveNode(index, id.GetProperty("version").GetInt32(), P(n.GetProperty("position")), n.GetProperty("segmentsConnected").GetInt32());
        }
        return g;
    }
}

/// <summary>An edge of an import body, as written to import-*.json.</summary>
public sealed record ImportEdge(string Layer, string Id, string Prefab, string A, string B, Bezier Curve);

public enum Coverage { Covered, WrongPrefab, Partial, Missing }

public sealed record EdgeCoverage(ImportEdge Edge, Coverage Status, int PointsMapped, int PointsAny);

public static class Reconciler
{
    public const double Tolerance = 6;
    public const double NodeBind = 1;
    static readonly double[] SampleT = [0.1, 0.3, 0.5, 0.7, 0.9];

    public static List<ImportEdge> ReadImport(string path, string layer, out Dictionary<string, Vec3> nodes)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        nodes = new Dictionary<string, Vec3>();
        foreach (var n in doc.RootElement.GetProperty("nodes").EnumerateArray())
            nodes[n.GetProperty("id").GetString()!] = new Vec3(n.GetProperty("x").GetDouble(), n.TryGetProperty("y", out var y) ? y.GetDouble() : 0, n.GetProperty("z").GetDouble());
        var list = new List<ImportEdge>();
        foreach (var e in doc.RootElement.GetProperty("edges").EnumerateArray())
        {
            var a = nodes[e.GetProperty("a").GetString()!]; var b = nodes[e.GetProperty("b").GetString()!];
            Bezier c;
            if (e.TryGetProperty("control", out var ctl) && ctl.GetArrayLength() == 2)
            {
                Vec3 Ctl(int i) { var p = ctl[i]; return new Vec3(p[0].GetDouble(), 0, p[p.GetArrayLength() - 1].GetDouble()); }
                c = new Bezier(a, Ctl(0), Ctl(1), b);
            }
            else c = new Bezier(a, Vec3.Lerp(a, b, 1 / 3.0), Vec3.Lerp(a, b, 2 / 3.0), b);
            list.Add(new ImportEdge(layer, e.GetProperty("id").GetString()!, e.GetProperty("prefab").GetString()!, e.GetProperty("a").GetString()!, e.GetProperty("b").GetString()!, c));
        }
        return list;
    }

    /// <summary>
    /// Geometric coverage. Live segments carry only their end points and length, so each one is drawn as the import
    /// curve whose two ends coincide with it (within 1 m, any layer), else as its chord with a tolerance widened by the
    /// sagitta implied by length vs chord. A sample point is covered when it is within 6 m (+ sagitta) of a live segment.
    /// </summary>
    public static List<EdgeCoverage> Run(LiveGraph live, IReadOnlyList<ImportEdge> edges)
    {
        // Exact shape lookup by end points.
        var byEnds = new Dictionary<(long, long, long, long), Bezier>();
        static (long, long) Q(Vec3 p) => ((long)Math.Round(p.X), (long)Math.Round(p.Z));
        foreach (var e in edges)
        {
            var (ax, az) = Q(e.Curve.A); var (bx, bz) = Q(e.Curve.D);
            byEnds[(ax, az, bx, bz)] = e.Curve;
            byEnds[(bx, bz, ax, az)] = e.Curve.Reverse();
        }

        var shapes = new List<(LiveSegment Seg, List<Vec3> Line, double Tol)>();
        foreach (var s in live.Segments)
        {
            Bezier? exact = null;
            var (ax, az) = Q(s.Start); var (bx, bz) = Q(s.End);
            for (long dx = -1; dx <= 1 && exact == null; dx++)
                for (long dz = -1; dz <= 1 && exact == null; dz++)
                    for (long ex = -1; ex <= 1 && exact == null; ex++)
                        for (long ez = -1; ez <= 1 && exact == null; ez++)
                            if (byEnds.TryGetValue((ax + dx, az + dz, bx + ex, bz + ez), out var c)
                                && c.A.DistXZ(s.Start) < NodeBind && c.D.DistXZ(s.End) < NodeBind) exact = c;
            if (exact is { } bz2)
                shapes.Add((s, Enumerable.Range(0, 17).Select(i => bz2.Eval(i / 16.0)).ToList(), Tolerance));
            else
            {
                double chord = s.Start.DistXZ(s.End);
                double sag = Math.Sqrt(3 * chord * Math.Max(0, s.Length - chord) / 8);
                shapes.Add((s, [s.Start, s.End], Tolerance + Math.Min(sag, 40)));
            }
        }

        const double cell = 64;
        var grid = new Dictionary<(long, long), List<int>>();
        for (int i = 0; i < shapes.Count; i++)
        {
            var line = shapes[i].Line; double pad = shapes[i].Tol;
            double minX = line.Min(p => p.X) - pad, maxX = line.Max(p => p.X) + pad, minZ = line.Min(p => p.Z) - pad, maxZ = line.Max(p => p.Z) + pad;
            for (long x = (long)Math.Floor(minX / cell); x <= (long)Math.Floor(maxX / cell); x++)
                for (long z = (long)Math.Floor(minZ / cell); z <= (long)Math.Floor(maxZ / cell); z++)
                {
                    if (!grid.TryGetValue((x, z), out var l)) grid[(x, z)] = l = new List<int>();
                    l.Add(i);
                }
        }

        var result = new List<EdgeCoverage>();
        foreach (var e in edges)
        {
            int mapped = 0, any = 0;
            foreach (var t in SampleT)
            {
                var p = e.Curve.Eval(t);
                bool m = false, a = false;
                if (grid.TryGetValue(((long)Math.Floor(p.X / cell), (long)Math.Floor(p.Z / cell)), out var l))
                    foreach (var i in l)
                    {
                        if (Converter.DistanceToPolyline(p, shapes[i].Line) >= shapes[i].Tol) continue;
                        a = true;
                        if (string.Equals(shapes[i].Seg.Prefab, e.Prefab, StringComparison.OrdinalIgnoreCase)) { m = true; break; }
                    }
                if (m) mapped++;
                if (a) any++;
            }
            var status = mapped == SampleT.Length ? Coverage.Covered
                : any == SampleT.Length ? Coverage.WrongPrefab
                : any == 0 ? Coverage.Missing
                : Coverage.Partial;
            result.Add(new EdgeCoverage(e, status, mapped, any));
        }
        return result;
    }

    /// <summary>Edges that go into the retry file: nothing built along them, or at most 2 of 5 sample points.</summary>
    public static bool NeedsRetry(EdgeCoverage c) => c.Status == Coverage.Missing || (c.Status == Coverage.Partial && c.PointsAny <= 2);

    /// <summary>
    /// Writes an import body with only the given edges. Nodes within 1 m of a live node that already carries a network of
    /// the same layer get its `entity`, so the retry attaches instead of building a second node.
    /// </summary>
    public static (int Edges, int BoundNodes, int NewNodes) WriteRetry(string importPath, string retryPath, ISet<string> edgeIds, LiveGraph live, ISet<string> layerPrefabs)
    {
        var root = JsonNode.Parse(File.ReadAllText(importPath))!.AsObject();
        var edges = root["edges"]!.AsArray().Where(e => edgeIds.Contains((string)e!["id"]!)).Select(e => e!.DeepClone()).ToList();
        var used = edges.SelectMany(e => new[] { (string)e!["a"]!, (string)e!["b"]! }).ToHashSet();

        const double cell = 4;
        var grid = new Dictionary<(long, long), List<LiveNode>>();
        foreach (var n in live.Nodes.Values)
        {
            if (!live.NodePrefabs.TryGetValue(n.Index, out var ps) || !ps.Overlaps(layerPrefabs)) continue;
            var key = ((long)Math.Floor(n.Position.X / cell), (long)Math.Floor(n.Position.Z / cell));
            if (!grid.TryGetValue(key, out var l)) grid[key] = l = new List<LiveNode>();
            l.Add(n);
        }

        int bound = 0, fresh = 0;
        var nodes = new JsonArray();
        foreach (var n in root["nodes"]!.AsArray())
        {
            if (!used.Contains((string)n!["id"]!)) continue;
            var copy = n.DeepClone().AsObject();
            var p = new Vec3((double)copy["x"]!, 0, (double)copy["z"]!);
            LiveNode? best = null; double bestD = NodeBind;
            long cx = (long)Math.Floor(p.X / cell), cz = (long)Math.Floor(p.Z / cell);
            for (long dx = -1; dx <= 1; dx++)
                for (long dz = -1; dz <= 1; dz++)
                    if (grid.TryGetValue((cx + dx, cz + dz), out var l))
                        foreach (var ln in l)
                        {
                            double d = ln.Position.DistXZ(p);
                            if (d < bestD) { bestD = d; best = ln; }
                        }
            if (best != null)
            {
                copy["entity"] = new JsonObject { ["index"] = best.Index, ["version"] = best.Version };
                bound++;
            }
            else fresh++;
            nodes.Add(copy);
        }

        // An edge whose two ends bind to the same live node cannot be built; drop it.
        var entityOf = nodes.ToDictionary(n => (string)n!["id"]!, n => n!["entity"]?["index"]?.GetValue<int>());
        edges.RemoveAll(e => entityOf[(string)e!["a"]!] is int x && entityOf[(string)e!["b"]!] is int y && x == y);

        var outRoot = new JsonObject
        {
            ["name"] = (string?)root["name"] + " (retry)",
            ["_comment"] = "net-converter retry: only edges missing in the live game; nodes with `entity` attach to existing network nodes.",
            ["nodes"] = nodes,
            ["edges"] = new JsonArray(edges.ToArray()),
            ["options"] = root["options"]!.DeepClone(),
        };
        File.WriteAllText(retryPath, outRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        return (edges.Count, bound, fresh);
    }

    public static string Table(string title, IEnumerable<EdgeCoverage> cov)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"### {title}").AppendLine();
        sb.AppendLine("| layer | prefab | edges | covered | wrong prefab | partial | missing | → retry |").AppendLine("|---|---|---:|---:|---:|---:|---:|---:|");
        foreach (var g in cov.GroupBy(c => (c.Edge.Layer, c.Edge.Prefab)).OrderBy(g => g.Key.Layer).ThenBy(g => g.Key.Prefab))
            sb.AppendLine(Row(g.Key.Layer, g.Key.Prefab, g.ToList()));
        foreach (var g in cov.GroupBy(c => c.Edge.Layer).OrderBy(g => g.Key))
            sb.AppendLine(Row($"**{g.Key}**", "**all**", g.ToList()));
        sb.AppendLine();
        return sb.ToString();

        static string Row(string layer, string prefab, List<EdgeCoverage> l) =>
            $"| {layer} | {prefab} | {l.Count} | {l.Count(c => c.Status == Coverage.Covered)} | {l.Count(c => c.Status == Coverage.WrongPrefab)} | {l.Count(c => c.Status == Coverage.Partial)} | {l.Count(c => c.Status == Coverage.Missing)} | {l.Count(NeedsRetry)} |";
    }

    public static string NotCovered(IEnumerable<EdgeCoverage> cov)
    {
        var sb = new StringBuilder();
        var list = cov.Where(c => c.Status != Coverage.Covered).ToList();
        sb.AppendLine("### Edges not fully covered").AppendLine();
        if (list.Count == 0) { sb.AppendLine("None.").AppendLine(); return sb.ToString(); }
        sb.AppendLine("| layer | edge | prefab | status | points mapped/any | x | z |").AppendLine("|---|---|---|---|---|---:|---:|");
        foreach (var c in list)
        {
            var m = c.Edge.Curve.Eval(0.5);
            sb.AppendLine(string.Create(CultureInfo.InvariantCulture, $"| {c.Edge.Layer} | {c.Edge.Id} | {c.Edge.Prefab} | {c.Status} | {c.PointsMapped}/{c.PointsAny} | {m.X:F0} | {m.Z:F0} |"));
        }
        sb.AppendLine();
        return sb.ToString();
    }

    public static string Diagnostics(ConvertResult r)
    {
        var ci = CultureInfo.InvariantCulture;
        var st = r.Stats;
        var sb = new StringBuilder();
        sb.AppendLine("## Converter fixes (round 2)").AppendLine();
        sb.AppendLine($"- Hairpins (Bézier length > 1.6 × chord, self-intersection, or a control point behind its end), checked on every imported CS1 segment: **{st.HairpinsFound}** found, {st.HairpinsFixed} rebuilt.");
        foreach (var l in st.HairpinLog.Take(50)) sb.AppendLine($"  - {l}");
        sb.AppendLine($"- Exact duplicate edges dropped: **{st.DuplicatesDropped}**.");
        sb.AppendLine($"- Structure now from node elevation (not the B/E/S/T prefab suffix): {r.Edges.Count(e => Layers.Imported.Contains(e.Layer) && e.Structure != e.AiStructure)} imported edges differ from the suffix-based class ({string.Join(", ", r.Edges.Where(e => Layers.Imported.Contains(e.Layer) && e.Structure != e.AiStructure).GroupBy(e => $"{e.AiStructure}→{e.Structure}").Select(g => $"{g.Key} {g.Count()}"))}).");
        sb.AppendLine($"- Elevation ramps (Δelevation > 6 m over < 60 m) smoothed to ≤ 8 %: **{st.RampsSmoothed}** ramps, {st.RampNodesRaised} nodes raised/lowered, {st.RampsUnresolved} still steeper than 12 % (blocked by a junction or the border).");
        sb.AppendLine();

        sb.AppendLine($"- Node merges refused because the nodes are stacked (height difference ≥ 2 m): **{st.MergesRefusedForHeight}**.");
        sb.AppendLine();
        sb.AppendLine("## Near-parallel pairs resolved (round 3)").AppendLine();
        foreach (var g in r.AcuteResolutions.GroupBy(a => a.Category)) sb.AppendLine($"- {g.Key}: {g.Count()}");
        sb.AppendLine();
        sb.AppendLine("| layer | node | edge 1 | edge 2 | angle before ° | after ° | category |").AppendLine("|---|---|---|---|---:|---:|---|");
        foreach (var a in r.AcuteResolutions)
            sb.AppendLine(string.Create(ci, $"| {a.Layer} | {a.Node} | {a.Edge1} | {a.Edge2} | {a.AngleBefore:F1} | {(double.IsNaN(a.AngleAfter) ? "-" : a.AngleAfter.ToString("F1", ci))} | {a.Category} |"));
        sb.AppendLine();
        sb.AppendLine("## Acute junctions remaining (< 10°)").AppendLine();
        sb.AppendLine("Two edges of one layer leaving the same node almost parallel. CS2 builds a long junction there, which looks like a ramp folding back into the highway.").AppendLine();
        foreach (var g in r.AcuteJunctions.GroupBy(a => a.Layer)) sb.AppendLine($"- {g.Key}: {g.Count()} pairs at {g.Select(a => a.Node).Distinct().Count()} nodes");
        sb.AppendLine();
        sb.AppendLine("| layer | node | x | z | edge 1 | edge 2 | angle ° |").AppendLine("|---|---|---:|---:|---|---|---:|");
        foreach (var a in r.AcuteJunctions.OrderBy(a => a.AngleDegrees).Take(60))
            sb.AppendLine(string.Create(ci, $"| {a.Layer} | {a.Node.Id} | {a.Node.Position.X:F0} | {a.Node.Position.Z:F0} | {a.E1.Id} ({a.E1.Map.Cs2Prefab}) | {a.E2.Id} ({a.E2.Map.Cs2Prefab}) | {a.AngleDegrees:F1} |"));
        sb.AppendLine();

        sb.AppendLine("## Overlaps (same layer, < 4 m apart for > 20 m)").AppendLine();
        foreach (var g in r.Overlaps.GroupBy(o => o.Layer))
            sb.AppendLine($"- {g.Key}: {g.Count()} pairs ({g.Count(o => o.SharedNode)} share a node, i.e. a shallow diverge; {g.Count(o => !o.SharedNode)} unconnected)");
        sb.AppendLine();
        sb.AppendLine("| layer | edge 1 | edge 2 | close m | shared node | x | z | CS1 prefabs |").AppendLine("|---|---|---|---:|---|---:|---:|---|");
        foreach (var o in r.Overlaps.OrderBy(o => o.SharedNode).ThenByDescending(o => o.Length))
        {
            var mid = o.E1.Curve.Eval(0.5);
            sb.AppendLine(string.Create(ci, $"| {o.Layer} | {o.E1.Id} | {o.E2.Id} | {o.Length:F0} | {(o.SharedNode ? "yes" : "no")} | {mid.X:F0} | {mid.Z:F0} | {o.E1.Map.Cs1Prefab} / {o.E2.Map.Cs1Prefab} |"));
        }
        sb.AppendLine();
        return sb.ToString();
    }
}
