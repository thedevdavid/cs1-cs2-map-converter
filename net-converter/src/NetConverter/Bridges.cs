using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetConverter;

/// <summary>
/// Bridge overrides (bridges.tsv, ../bridges/PLAN.md): the deck chain of each chosen CS1 bridge becomes straight CS2 bridge-prefab edges of
/// legal length (0.6–1.4 × m_SegmentLength) between the chain's end nodes, at a constant grade from the end nodes' deck heights, with no
/// span split and no deck smoothing. Approaches keep the base road and meet the deck at ordinary nodes (snapToSegment onto the network
/// import's node). BR001 merges its two carriageways onto one two-way fixed-span suspension (off by default, import-bridges-fixed.json).
/// </summary>
public static class Bridges
{
    static readonly CultureInfo Ci = CultureInfo.InvariantCulture;
    static string F(FormattableString f) => f.ToString(Ci);

    public sealed record Spec(string Id, Regex Select, string Prefab, double[]? Fixed, int? Count, double? Span, bool Merge, bool FixedFile, double Seg, bool SegAssumed, string Notes, string Alt, bool NoPillars = false, bool Drop = false);

    public static List<Spec> Load(string path)
    {
        var res = new List<Spec>();
        if (!File.Exists(path)) return res;
        foreach (var line in File.ReadAllLines(path).Where(l => l.Length > 0 && !l.StartsWith('#')))
        {
            var c = line.Split('\t');
            double[]? fx = null; int? count = null; double? span = null; bool merge = false, ff = false, np = false, drop = false;
            foreach (var part in c[3].Split(';'))
            {
                if (part.StartsWith("fixed:")) fx = part[6..].Split(',').Select(v => double.Parse(v, Ci)).ToArray();
                else if (part.StartsWith("count:")) count = int.Parse(part[6..], Ci);
                else if (part.StartsWith("span:")) span = double.Parse(part[5..], Ci);
                else if (part == "merge") merge = true;
                else if (part == "fixedfile") ff = true;
                else if (part == "nopillars") np = true;
                else if (part == "drop") drop = true;
            }
            var segTxt = c[4].Trim();
            res.Add(new Spec(c[0], new Regex(c[1], RegexOptions.CultureInvariant), c[2], fx, count, span, merge, ff,
                double.Parse(segTxt.Split(' ')[0], Ci), segTxt.Contains("[U]"), c.Length > 5 ? c[5] : "", c.Length > 6 ? c[6] : "", np, drop));
        }
        return res;
    }

    public static Dictionary<string, int[]> LoadSegIds(string cs1BridgesJson)
    {
        var res = new Dictionary<string, int[]>();
        if (!File.Exists(cs1BridgesJson)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(cs1BridgesJson));
        foreach (var b in doc.RootElement.EnumerateArray())
            res[b.GetProperty("id").GetString()!] = b.GetProperty("segIds").EnumerateArray().Select(v => v.GetInt32()).ToArray();
        return res;
    }

    /// <summary>Bridge rules per prefab from ../bridges/cs2-bridge-rules.json (offline parse of the BXP packs; vanilla prefabs are not in it).</summary>
    public sealed record Rule(string Name, double Seg, bool CanCurve, double ElevationOnWater, double Min, double Max, double[]? FixedLengths, double FixedMin, double FixedMax);

    public static Dictionary<string, Rule> LoadRules(string path, string? livePath = null)
    {
        var res = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase);
        if (livePath != null && File.Exists(livePath) && livePath != path) foreach (var kv in LoadRules(livePath)) res[kv.Key] = kv.Value;
        if (!File.Exists(path)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        foreach (var p in doc.RootElement.GetProperty("prefabs").EnumerateArray())
        {
            static double N(JsonElement e, string k, double d = 0) => e.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : d;
            string name = p.GetProperty("name").GetString()!;
            double seg = N(p, "segmentLength");
            double[]? fixedL = p.TryGetProperty("fixedSegments", out var fsg) && fsg.ValueKind == JsonValueKind.Array && fsg.GetArrayLength() > 0 ? fsg.EnumerateArray().Select(x => N(x, "length")).ToArray() : null;
            double min = 0, max = double.MaxValue, fmin = 0, fmax = 0;
            if (p.TryGetProperty("edgeLengthRange", out var r) && r.ValueKind == JsonValueKind.Object) { min = N(r, "min"); max = N(r, "max", double.MaxValue); }
            if (p.TryGetProperty("importEdge", out var ie) && ie.ValueKind == JsonValueKind.Object) { fmin = N(ie, "lengthMin"); fmax = N(ie, "lengthMax"); }
            res[name] = new Rule(name, seg, p.TryGetProperty("canCurve", out var cc) && cc.ValueKind == JsonValueKind.True, N(p, "elevationOnWater", 10), min, max, fixedL, fmin, fmax);
        }
        return res;
    }

    /// <summary>
    /// The bridge's 0.9.22 pre-checks, computed offline for one import file (IMPORT_SCHEMA.md "Bridge prefabs"): bridge.length.short,
    /// bridge.fixed.repeat / .short / .chained, bridge.straight (all edges here are straight), bridge.waterClearance (9 points). lowElevation needs the
    /// net's elevation limit, which is not in the offline rules, so the runner's dry run checks it. Returns bridge id → findings.
    /// </summary>
    public static Dictionary<string, List<string>> Validate(Output o, Dictionary<string, Rule> rules, Func<double, double, double?>? waterOut, Func<double, double, double>? terrOut = null)
    {
        var res = new Dictionary<string, List<string>>();
        var nodes = o.Nodes.GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
        var fixedEdges = o.Edges.Where(e => rules.TryGetValue(e.Prefab, out var r) && r.FixedLengths != null).ToList();
        foreach (var e in o.Edges)
        {
            string id = e.Id.Split('_')[0].ToUpperInvariant();
            if (!res.TryGetValue(id, out var list)) res[id] = list = new();
            if (!rules.TryGetValue(e.Prefab, out var r))
            {
                if (e.Note.Contains("approach")) continue;   // base road connector
                string msg = $"{e.Prefab}: not in cs2-bridge-rules.json (vanilla or base net; checked by the runner's dry run)";
                if (!list.Contains(msg)) list.Add(msg);
                continue;
            }
            var a = nodes[e.A]; var b = nodes[e.B];
            double L = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
            if (r.FixedLengths != null)
            {
                if (L > r.FixedMax) list.Add(F($"bridge.fixed.repeat {e.Id}: {L:F0} m > {r.FixedMax:F0} m"));
                else if (L < r.FixedMin) list.Add(F($"bridge.fixed.short (warning) {e.Id}: {L:F0} m < {r.FixedMin:F0} m"));
                else list.Add(F($"fixed {e.Id}: {L:F0} m within {r.FixedMin:F0}–{r.FixedMax:F0} m (nominal {r.FixedLengths.Sum():F0})"));
                if (fixedEdges.Any(x => x != e && (x.A == e.A || x.A == e.B || x.B == e.A || x.B == e.B))) list.Add($"bridge.fixed.chained {e.Id}");
            }
            else
            {
                if (L < r.Min) list.Add(F($"bridge.length.short {e.Id}: {L:F0} m < {r.Min:F1} m"));
                if (L > r.Max) list.Add(F($"{e.Id}: {L:F0} m > {r.Max:F1} m, the game cuts it into {Math.Ceiling((L - 0.16) / r.Max):F0} pieces"));
            }
            if (waterOut != null)
                for (int k = 1; k <= 9; k++)
                {
                    double t = k / 10.0, x = a.X + t * (b.X - a.X), z = a.Z + t * (b.Z - a.Z);
                    if (waterOut(x, z) is { } w && a.Y + t * (b.Y - a.Y) - w < r.ElevationOnWater) { list.Add(F($"bridge.waterClearance (warning) {e.Id}: {a.Y + t * (b.Y - a.Y) - w:F1} m < {r.ElevationOnWater:F0} m over the water")); break; }
                }
            if (terrOut != null && !e.Prefab.Contains("Pedestrian"))
            {
                double h = new[] { 0.0, 0.5, 1.0 }.Max(t => a.Y + t * (b.Y - a.Y) - terrOut(a.X + t * (b.X - a.X), a.Z + t * (b.Z - a.Z)));
                if (h < MinElevated) list.Add(F($"bridge.lowElevation {e.Id}: {h:F1} m < {MinElevated:F0} m above the terrain"));
            }
        }
        return res;
    }

    /// <summary>CS1 segments whose remaining courses in the network import get noPillars (BR001 deck outside the merged span).</summary>
    public static HashSet<int> NoPillarSegments { get; } = new();

    public sealed record Report(string Id, string Prefab, string Plan, int Chains, List<double> Lengths, List<string> Problems, bool InFixedFile, string Notes);

    public sealed class Output
    {
        public List<Round9.NetNode> Nodes { get; } = new();
        public List<Round9.NetEdge> Edges { get; } = new();
        public List<string> Replaces { get; } = new();
    }

    static double Len(Vec3 a, Vec3 b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));

    /// <summary>Chains of the selected edges, split at nodes where the network (imported layers) has a degree other than 2.</summary>
    static List<(List<OutEdge> Edges, OutNode Start, OutNode End)> Chains(List<OutEdge> sel, Dictionary<OutNode, int> deg)
    {
        var at = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in sel) foreach (var n in new[] { e.A, e.B }) (at.TryGetValue(n, out var l) ? l : at[n] = new()).Add(e);
        bool Stop(OutNode n) => deg.GetValueOrDefault(n) != 2 || at[n].Count != 2;
        var seen = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        var res = new List<(List<OutEdge>, OutNode, OutNode)>();
        foreach (var e0 in sel)
        {
            if (seen.Contains(e0)) continue;
            // Walk back to a chain end, then forward.
            var (e, n) = (e0, e0.A);
            var guard = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
            while (!Stop(n) && guard.Add(e)) { var o = at[n].First(x => x != e); n = o.A == n ? o.B : o.A; e = o; }
            var start = n; var chain = new List<OutEdge>();
            var cur = start; var ce = e;
            while (true)
            {
                if (!seen.Add(ce)) break;
                chain.Add(ce);
                var next = ce.A == cur ? ce.B : ce.A;
                if (Stop(next)) { cur = next; break; }
                var o = at[next].FirstOrDefault(x => x != ce && !seen.Contains(x));
                if (o == null) { cur = next; break; }
                cur = next; ce = o;
            }
            res.Add((chain, start, cur));
        }
        return res;
    }

    static double Y(OutNode n) => Round9.Y(n);

    static (double D, double T) PointLine(Vec3 p, Vec3 a, Vec3 b)
    {
        double dx = b.X - a.X, dz = b.Z - a.Z, l2 = dx * dx + dz * dz;
        double t = l2 > 0 ? Math.Clamp(((p.X - a.X) * dx + (p.Z - a.Z) * dz) / l2, 0, 1) : 0;
        return (Math.Sqrt((a.X + t * dx - p.X) * (a.X + t * dx - p.X) + (a.Z + t * dz - p.Z) * (a.Z + t * dz - p.Z)), t);
    }

    static double LineDistance(Vec3 a1, Vec3 b1, Vec3 a2, Vec3 b2)
    {
        double m = double.MaxValue;
        for (int k = 0; k <= 8; k++)
        {
            var p = new Vec3(a1.X + k / 8.0 * (b1.X - a1.X), 0, a1.Z + k / 8.0 * (b1.Z - a1.Z));
            var q = new Vec3(a2.X + k / 8.0 * (b2.X - a2.X), 0, a2.Z + k / 8.0 * (b2.Z - a2.Z));
            m = Math.Min(m, Math.Min(PointLine(p, a2, b2).D, PointLine(q, a1, b1).D));
        }
        return m;
    }

    /// <summary>Bridge prefabs require ≥ 2 × the net's elevation limit above the terrain at an end or the middle (rule bridge.lowElevation, RequireElevated).</summary>
    public const double MinElevated = 8.0, RaiseTarget = 8.5, MaxRaise = 5.0;

    /// <summary>
    /// Straight, constant-grade edges between a chain's end nodes (flow direction of the chain's first edge kept). An edge lower than 8 m above the
    /// terrain at its ends and middle (bridge.lowElevation) lifts the whole deck to 8.5 m, end nodes included, when that is at most 5 m and the
    /// approaches stay within their grade; otherwise the chain keeps its base prefab (returns null, nothing written).
    /// </summary>
    static List<double>? Resegment(Spec s, string tag, List<OutEdge> chain, OutNode start, OutNode end, Dictionary<OutNode, int> deg, Output o, List<string> problems,
        Func<double, double, double?>? waterOut, Func<double, double, double>? terrOut, IReadOnlyList<OutEdge> network)
    {
        bool forward = chain[0].A == start;
        var (a, b) = forward ? (start, end) : (end, start);
        double chord = Len(a.Position, b.Position), curve = chain.Sum(e => e.Curve.LengthXZ());
        int n = s.Count ?? (s.Span is { } sp ? Math.Max(1, (int)Math.Ceiling(chord / sp - 1e-9)) : 1);
        // bridge.lowElevation: the highest of ends / middle per edge must reach 8 m.
        double Raise(int pieces)
        {
            double r = 0;
            for (int k = 0; k < pieces; k++)
            {
                double h = double.MinValue;
                foreach (var t in new[] { k / (double)pieces, (k + 0.5) / pieces, (k + 1) / (double)pieces })
                {
                    double x = a.Position.X + t * (b.Position.X - a.Position.X), z = a.Position.Z + t * (b.Position.Z - a.Position.Z);
                    h = Math.Max(h, Y(a) + t * (Y(b) - Y(a)) - terrOut!(x, z));
                }
                if (h < MinElevated) r = Math.Max(r, RaiseTarget - h);
            }
            return r;
        }
        double raise = 0;
        if (terrOut != null && !s.FixedFile)
        {
            raise = Raise(n);
            // Round 26 (A-v7 review F-ii): a non-fixed prefab may go as one edge (the game cuts it into equal spans); when that one edge already meets the
            // rule at its middle (BR017, BR038), no lift at all.
            if (raise > 0 && s.Fixed == null && n > 1 && Raise(1) == 0) { problems.Add(F($"{tag}: one edge instead of {n}: its middle stands ≥ 8 m above the terrain, no lift")); n = 1; raise = 0; }
            if (raise > 0)
            {
                // Owner (A-v5): raise the road before the bridge. From each end node walk outward along the approach (through nodes of degree 2),
                // lifting every node to at least deck − 6 % × distance, until the original profile is met. Fails (base prefab kept) when that is not
                // reached within 150 m or a junction comes first.
                var lift = new Dictionary<OutNode, double>(ReferenceEqualityComparer.Instance);
                var adjN = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
                foreach (var e in network.Where(e => e.Map.Imported && !chain.Contains(e))) foreach (var nn in new[] { e.A, e.B }) (adjN.TryGetValue(nn, out var l) ? l : adjN[nn] = new()).Add(e);
                string? fail = null;
                // Breadth-first from both deck ends through every branch (junctions included): each node is lifted to deck − 6 % × distance
                // until that falls below its own height; past 150 m with a lift still needed, the bridge keeps its base prefab.
                foreach (var node in new[] { a, b })
                {
                    lift[node] = Math.Max(lift.GetValueOrDefault(node), raise);
                    double deckY = Y(node) + raise;
                    var best = new Dictionary<OutNode, double>(ReferenceEqualityComparer.Instance) { [node] = 0 };
                    var q = new Queue<OutNode>(); q.Enqueue(node);
                    while (q.Count > 0 && fail == null)
                    {
                        var cn = q.Dequeue(); double dist0 = best[cn];
                        foreach (var e in adjN.GetValueOrDefault(cn) ?? new())
                        {
                            var nx = e.A == cn ? e.B : e.A; if (nx == a || nx == b) continue;
                            double dist = dist0 + e.Curve.LengthXZ();
                            if (best.TryGetValue(nx, out var dd) && dd <= dist) continue;
                            // Round 26 (owner, A-v7: "no reason to raise from land then lower"): one climb at ≤ 3 % (2 % for track) from up to 300 m back.
                            double grade = e.Layer == Layers.Rail ? 0.02 : 0.03;
                            double need = deckY - grade * dist - Y(nx);   // output metres, as the game measures it
                            if (need <= 0) continue;                                    // this branch meets its original profile
                            if (dist > 300) { fail = F($"the approach via {e.Id} would need more than 300 m at {grade:P0}"); break; }
                            best[nx] = dist; lift[nx] = Math.Max(lift.GetValueOrDefault(nx), need); q.Enqueue(nx);
                        }
                    }
                    if (fail != null) break;
                }
                if (fail != null)
                {
                    problems.Add(F($"{tag}: kept the base prefab: the deck needs +{raise:F1} m for bridge.lowElevation (≥ 8 m) and {fail}"));
                    return null;
                }
                foreach (var (node, up) in lift) { double dTop = up / Outputs.YScale; node.Elevation += dTop; node.Position = node.Position with { Y = node.Position.Y + dTop }; }
                foreach (var e in network.Where(e => lift.ContainsKey(e.A) || lift.ContainsKey(e.B))) e.Curve = e.Curve.WithEnds(e.A.Position, e.B.Position);
                problems.Add(F($"{tag}: deck lifted +{raise:F1} m to 8.5 m above the terrain; {lift.Count - 2} approach nodes re-graded at ≤ 3 % (track 2 %)"));
            }
        }
        // Curvature: the largest offset of the CS1 chain from the straight line it becomes.
        double maxOff = 0;
        foreach (var e in chain) for (int k = 0; k <= 8; k++)
        {
            var p = e.Curve.Eval(k / 8.0); double dx = b.Position.X - a.Position.X, dz = b.Position.Z - a.Position.Z, l = Math.Max(1e-9, Math.Sqrt(dx * dx + dz * dz));
            maxOff = Math.Max(maxOff, Math.Abs((p.X - a.Position.X) * dz - (p.Z - a.Position.Z) * dx) / l);
        }
        if (maxOff > 5) problems.Add(F($"{tag}: CS1 chain curves up to {maxOff:F0} m off the straight line ({curve:F0} m along, {chord:F0} m straight)"));
        var ids = new List<string>();
        for (int k = 0; k <= n; k++)
        {
            double t = k / (double)n;
            if (k == 0 || k == n)
            {
                var node = k == 0 ? a : b;
                bool shared = deg.GetValueOrDefault(node) > chain.Count(e => e.A == node || e.B == node);
                o.Nodes.Add(shared ? new Round9.NetNode(node.Id, node.Position.X, Y(node), node.Position.Z, 2, null) : new Round9.NetNode(node.Id, node.Position.X, Y(node), node.Position.Z));
                ids.Add(node.Id);
            }
            else
            {
                string id = $"{tag}_n{k}";
                o.Nodes.Add(new Round9.NetNode(id, a.Position.X + t * (b.Position.X - a.Position.X), Y(a) + t * (Y(b) - Y(a)), a.Position.Z + t * (b.Position.Z - a.Position.Z)));
                ids.Add(id);
            }
        }
        var lengths = new List<double>();
        for (int k = 0; k < n; k++)
        {
            o.Edges.Add(new Round9.NetEdge($"{tag}_e{k}", ids[k], ids[k + 1], s.Prefab, Note: F($"{s.Id} bridge edge {k + 1}/{n}; CS1 segments {string.Join(",", chain.Select(e => e.Cs1Segment).Distinct())}"), NoPillars: s.NoPillars));
            lengths.Add(chord / n);
        }
        foreach (var l in lengths)
            if (l < 0.6 * s.Seg - 1e-6 || l > 1.4 * s.Seg + 1e-6) problems.Add(F($"{tag}: edge {l:F0} m outside 0.6–1.4 × {s.Seg:F0} m ({0.6 * s.Seg:F0}–{1.4 * s.Seg:F0})"));
        if (waterOut != null)
            for (int k = 1; k < 8; k++)
            {
                double t = k / 8.0, x = a.Position.X + t * (b.Position.X - a.Position.X), z = a.Position.Z + t * (b.Position.Z - a.Position.Z);
                if (waterOut(x, z) is { } w && Y(a) + t * (Y(b) - Y(a)) - w < 10) { problems.Add(F($"{tag}: deck {Y(a) + t * (Y(b) - Y(a)) - w:F1} m over the water at ({x:F0}, {z:F0}) (< 10 m, m_ElevationOnWater [U])")); break; }
            }
        o.Replaces.AddRange(chain.Select(e => e.Id));
        return lengths;
    }

    /// <summary>
    /// Applies the overrides. Plain bridges always (unless <paramref name="enabled"/> is false): their chains leave the network and go to
    /// <c>Main</c>. Fixed-file bridges (BR001 merge, BR022 arches) go to <c>Fixed</c>; their source edges leave the network only when
    /// <paramref name="fixedOn"/> (until the bridge supports fixedIndex they stay plain elevated highway / dropped decor).
    /// </summary>
    public static (Output Main, Output Fixed, List<Report> Reports) Apply(ConvertResult res, Dump dump, JsonElement root, List<Spec> specs, Dictionary<string, int[]> segIds,
        bool enabled, bool fixedOn, Func<double, double, double?>? waterOut, double topologyFactor, Func<double, double, double>? terrOut = null)
    {
        var main = new Output(); var fix = new Output(); var reports = new List<Report>();
        if (!enabled) return (main, fix, reports);
        var deg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
        foreach (var e in res.Edges.Where(e => e.Map.Imported)) foreach (var n in new[] { e.A, e.B }) deg[n] = deg.GetValueOrDefault(n) + 1;
        var remove = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        foreach (var s in specs)
        {
            if (!segIds.TryGetValue(s.Id, out var ids)) { reports.Add(new Report(s.Id, s.Prefab, "", 0, new(), new() { "bridge id not in cs1-bridges.json" }, s.FixedFile, s.Notes)); continue; }
            var segSet = ids.Where(i => dump.Segments.Any(x => x.Id == i && s.Select.IsMatch(x.Prefab))).ToHashSet();
            var sel = res.Edges.Where(e => segSet.Contains(e.Cs1Segment) && e.Map.Cs2Prefab != null).ToList();
            var problems = new List<string>(); var lengths = new List<double>();
            var target = s.FixedFile ? fix : main;
            if (sel.Count == 0) { reports.Add(new Report(s.Id, s.Prefab, "", 0, lengths, new() { "no converted edges (outside the map or skipped)" }, s.FixedFile, s.Notes)); continue; }
            int chains = 0;
            if (s.Drop)
            {
                // The segments leave the network; nothing replaces them (BR004 dam-crest walkway).
                foreach (var e in sel) remove.Add(e);
                // Footpaths that only led onto the dropped walkway: removed up to their first junction (owner: no path by the dam).
                var ends = new Queue<OutNode>(sel.SelectMany(e => new[] { e.A, e.B }));
                var tail = new List<OutEdge>();
                while (ends.Count > 0)
                {
                    var n = ends.Dequeue();
                    var rest = res.Edges.Where(e => e.Map.Imported && !remove.Contains(e) && (e.A == n || e.B == n)).ToList();
                    if (rest.Count != 1 || rest[0].Map.Cs2Prefab != "Pavement Path") continue;
                    remove.Add(rest[0]); tail.Add(rest[0]);
                    ends.Enqueue(rest[0].A == n ? rest[0].B : rest[0].A);
                }
                reports.Add(new Report(s.Id, "(dropped)", "drop", 0, lengths, new() { F($"{sel.Count} courses dropped, {sel.Sum(e => e.Curve.LengthXZ()):F0} m; footpaths that only led onto it: {tail.Count} ({tail.Sum(e => e.Curve.LengthXZ()):F0} m)") }, false, s.Notes));
                continue;
            }
            if (s.Merge && s.Fixed != null)
            {
                var all = sel.ToList();
                chains = Merge(s, sel, root, deg, target, problems, lengths, res, waterOut, topologyFactor);
                if (s.NoPillars) foreach (var e in all.Where(e => !sel.Contains(e))) NoPillarSegments.Add(e.Cs1Segment);
            }
            else
            {
                var decorDeg = deg;
                if (s.FixedFile)
                {
                    // Decor (BR022) is not in the imported layers: degrees within the selection.
                    decorDeg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
                    foreach (var e in sel) foreach (var n in new[] { e.A, e.B }) decorDeg[n] = decorDeg.GetValueOrDefault(n) + 1;
                }
                var kept = new List<OutEdge>();
                var cl = Chains(sel, decorDeg).ToList();
                var bad = new Dictionary<int, string>();
                // (b) twin carriageways: the two straight bridge lines closer than 1.2 × the road width overlap.
                for (int i = 0; i < cl.Count; i++)
                    for (int j = i + 1; j < cl.Count; j++)
                    {
                        double w = 2 * FootprintShift.Cs2Half(cl[i].Edges[0]);
                        double dmin = LineDistance(cl[i].Start.Position, cl[i].End.Position, cl[j].Start.Position, cl[j].End.Position);
                        if (dmin < 1.2 * w) { bad.TryAdd(i, F($"carriageways overlap: {dmin:F1} m apart < 1.2 × {w:F0} m")); bad.TryAdd(j, F($"carriageways overlap: {dmin:F1} m apart < 1.2 × {w:F0} m")); }
                    }
                // (c) another course within 12 m of the deck in plan and within 6 m in height (a ramp through the deck; courses well below pass).
                for (int i = 0; i < cl.Count; i++)
                {
                    var (ch, ca, cb) = cl[i];
                    foreach (var e in res.Edges)
                    {
                        if (!e.Map.Imported || sel.Contains(e) || e.A == ca || e.B == ca || e.A == cb || e.B == cb) continue;
                        bool hit = false;
                        for (int k = 0; k <= 8 && !hit; k++)
                        {
                            var p = e.Curve.Eval(k / 8.0);
                            var (d, t) = PointLine(p, ca.Position, cb.Position);
                            if (d < 12 && t > 0.02 && t < 0.98 && Math.Abs(p.Y * Outputs.YScale - (Y(ca) + t * (Y(cb) - Y(ca)))) < 6) hit = true;
                        }
                        if (hit) { bad.TryAdd(i, F($"{e.Id} {e.Map.Cs2Prefab} runs within 12 m of the deck at the same height")); break; }
                    }
                }
                for (int i = 0; i < cl.Count; i++)
                {
                    var (chain, a, b) = cl[i]; string tag = $"{s.Id.ToLowerInvariant()}_c{++chains}";
                    if (bad.TryGetValue(i, out var why)) { problems.Add($"{tag}: kept the plain elevated base road: {why}"); kept.AddRange(chain); continue; }
                    var l = Resegment(s, tag, chain, a, b, decorDeg, target, problems, waterOut, terrOut, res.Edges);
                    if (l == null) { kept.AddRange(chain); continue; }
                    lengths.AddRange(l);
                    // (a) approach tangents: an approach meeting the straight deck at more than 4° gets its end control point on the deck line.
                    foreach (var node in new[] { a, b })
                    {
                        var other = node == a ? b : a;
                        double ux = node.Position.X - other.Position.X, uz = node.Position.Z - other.Position.Z, ul = Math.Sqrt(ux * ux + uz * uz); ux /= ul; uz /= ul;   // out of the deck
                        var approaches = res.Edges.Where(e => !chain.Contains(e) && e.Map.Imported && (e.A == node || e.B == node)).ToList();
                        // Round 26 (A-v7, BR005): with two or more approaches at one deck end (a merge or diverge at the abutment), straightening all of them onto
                        // the deck line made them overlap for 10–20 m and the game built a 4 m one-lane stub.
                        double Ang(OutEdge e) { var c = e.A == node ? e.Curve.B : e.Curve.C; double dx = c.X - node.Position.X, dz = c.Z - node.Position.Z, l = Math.Sqrt(dx * dx + dz * dz); return l < 1e-6 ? 0 : Math.Acos(Math.Clamp((dx * ux + dz * uz) / l, -1, 1)); }
                        // Only the through approach (the one closest to the deck line) is straightened; side ramps keep their own tangent.
                        if (approaches.Count > 1) { problems.Add(F($"{tag}: {approaches.Count} approaches meet at {node.Id}: only the through one is straightened")); approaches = approaches.OrderBy(Ang).Take(1).ToList(); }
                        foreach (var e in approaches)
                        {
                            var c = e.A == node ? e.Curve.B : e.Curve.C;
                            double cx = c.X - node.Position.X, cz = c.Z - node.Position.Z, cl2 = Math.Sqrt(cx * cx + cz * cz);
                            if (cl2 < 1e-6) continue;
                            double ang = Math.Acos(Math.Clamp((cx * ux + cz * uz) / cl2, -1, 1)) * 180 / Math.PI;
                            if (ang <= 4) continue;
                            var np = new Vec3(node.Position.X + ux * cl2, c.Y, node.Position.Z + uz * cl2);
                            e.Curve = e.A == node ? e.Curve with { B = np } : e.Curve with { C = np };
                            problems.Add(F($"{tag}: approach {e.Id} met the deck at {ang:F0}°, its end now runs straight onto the deck"));
                        }
                    }
                }
                sel.RemoveAll(kept.Contains);   // chains that keep their base prefab stay in the network
            }
            if (!s.FixedFile || fixedOn) foreach (var e in sel) remove.Add(e);
            if (s.SegAssumed) problems.Add(F($"segment length {s.Seg:F0} m assumed [U]"));
            reports.Add(new Report(s.Id, s.Prefab, string.Join(";", new[] { s.Fixed != null ? "fixed:" + string.Join(",", s.Fixed) : null, s.Count != null ? $"count:{s.Count}" : null, s.Span != null ? F($"span:{s.Span}") : null, s.Merge ? "merge" : null }.Where(x => x != null)),
                chains, lengths, problems, s.FixedFile, s.Notes));
        }
        foreach (var e in remove) res.Fate[e.Cs1Segment] = "bridge override (import-bridges*.json)";
        res.Edges.RemoveAll(remove.Contains);
        return (main, fix, reports);
    }

    /// <summary>
    /// BR001: the two 3-lane carriageways merge onto one two-way centreline over the fixed spans, on the line through the two CS1 towers and
    /// centred between them. Each end node splits into the two carriageways at their first node at least one approach edge (100 m) beyond
    /// the deck end; the carriageway courses between the split nodes are replaced. Clearance over everything passing beneath is reported.
    /// </summary>
    static int Merge(Spec s, List<OutEdge> sel, JsonElement root, Dictionary<OutNode, int> deg, Output o, List<string> problems, List<double> lengths,
        ConvertResult res, Func<double, double, double?>? waterOut, double topologyFactor)
    {
        var towers = root.GetProperty("buildings").EnumerateArray().Where(b => (b.GetProperty("prefab").GetString() ?? "").Contains("Detroit Bridge"))
            .Select(b => { var p = b.GetProperty("position").EnumerateArray().Select(v => v.GetDouble()).ToArray(); return (X: p[0], Z: p[2]); }).ToList();
        if (towers.Count < 2) { problems.Add("tower buildings (Detroit Bridge) not found"); return 0; }
        var (t1, t2) = (towers[0], towers[1]);
        double cx = (t1.X + t2.X) / 2, cz = (t1.Z + t2.Z) / 2, ux = t2.X - t1.X, uz = t2.Z - t1.Z, ul = Math.Sqrt(ux * ux + uz * uz); ux /= ul; uz /= ul;
        double total = s.Fixed!.Sum(), half = total / 2;
        problems.Add(F($"tower spacing {s.Fixed[1] + s.Fixed[2]:F0} m vs CS1 {ul:F0} m"));
        double Along(Vec3 p) => (p.X - cx) * ux + (p.Z - cz) * uz;
        // Carriageways: chains of the selected edges.
        var carriage = Chains(sel, deg).Where(c => c.Edges.Count > 0).OrderByDescending(c => c.Edges.Sum(e => e.Curve.LengthXZ())).Take(2).ToList();
        if (carriage.Count < 2) { problems.Add("fewer than two carriageway chains"); return carriage.Count; }
        // Deck height at a point: mean of the carriageways' heights at their nearest course point.
        double DeckY(double s0)
        {
            double sum = 0;
            foreach (var (edges, _, _) in carriage)
            {
                var best = edges.SelectMany(e => Enumerable.Range(0, 9).Select(k => (e, t: k / 8.0))).MinBy(q => Math.Abs(Along(q.e.Curve.Eval(q.t)) - s0));
                sum += Y(best.e.A) + best.t * (Y(best.e.B) - Y(best.e.A));
            }
            return sum / carriage.Count;
        }
        var cuts = new double[s.Fixed.Length + 1]; cuts[0] = -half; for (int k = 0; k < s.Fixed.Length; k++) cuts[k + 1] = cuts[k] + s.Fixed[k];
        // Bridge 0.9.22 contract: ONE straight edge abutment to abutment; CourseSplitSystem cuts the span sequence and places the towers at
        // L × 168/976 and L × 808/976 from a. No inner node, no fixedIndex, no control (all spans are straight).
        var ids = new List<string> { "br001_a", "br001_b" };
        double yA = DeckY(-half), yB = DeckY(half);
        o.Nodes.Add(new Round9.NetNode("br001_a", cx - ux * half, yA, cz - uz * half));
        o.Nodes.Add(new Round9.NetNode("br001_b", cx + ux * half, yB, cz + uz * half));
        o.Edges.Add(new Round9.NetEdge("br001_deck", "br001_a", "br001_b", s.Prefab, Note: F($"BR001 abutment to abutment, {total:F0} m = {string.Join(" + ", s.Fixed.Select(v => v.ToString("F0", Ci)))}; towers at {cuts[1] + half:F0} and {cuts[3] + half:F0} m from a")));
        lengths.Add(total);
        if (Math.Abs(yA - yB) > 0.5) problems.Add(F($"deck ends differ by {Math.Abs(yA - yB):F1} m (constant grade over the bridge)"));
        // Splits: per carriageway and end, the first chain node at least 100 m beyond the deck end; connectors keep the one-way flow.
        foreach (var (edges, start, end) in carriage)
        {
            var nodes = new List<OutNode> { start };
            { var cur = start; foreach (var e in edges) { cur = e.A == cur ? e.B : e.A; nodes.Add(cur); } }
            bool forward = edges[0].A == start;   // flow start → end
            var along = nodes.Select(n => Along(n.Position)).ToList();
            bool increasing = along[^1] > along[0];
            int lo = -1, hi = -1;
            for (int i = 0; i < nodes.Count; i++)
            {
                double a = increasing ? along[i] : -along[i];
                if (a <= -half - 100) lo = i;                       // last node before the west split zone
                if (hi < 0 && a >= half + 100) hi = i;             // first node past the east split zone
            }
            if (lo < 0 || hi < 0) { problems.Add($"carriageway {start.Id}–{end.Id}: no node 100 m beyond a deck end; not merged"); continue; }
            var sLo = nodes[lo]; var sHi = nodes[hi];
            // Deck end ids nearest each split along the axis.
            string endLo = increasing ? ids[0] : ids[^1], endHi = increasing ? ids[^1] : ids[0];
            foreach (var sn in new[] { sLo, sHi }) o.Nodes.Add(new Round9.NetNode(sn.Id, sn.Position.X, Y(sn), sn.Position.Z, 2, null));
            // Flow along the chain from start to end if forward, so lo → deck → hi (or reversed).
            var prefab = edges[0].Map.Cs2Prefab!;
            // Round 26 (A-v7, shot 05): straight connectors converged at 3.3° and the two 16 m carriageways overlapped over all 140 m. Each connector is now a
            // curve that leaves the deck end 12° off the axis towards its own side and arrives parallel to its carriageway, so the legs clear each other
            // about 40 m from the deck end.
            (Vec3 C1, Vec3 C2) Fork(OutNode split, int splitIdx, string deckId, int outward)
            {
                var dn = o.Nodes.First(x => x.Id == deckId);
                double ox = outward * ux, oz = outward * uz;                                         // out of the deck, along its axis
                double side = Math.Sign((split.Position.X - dn.X) * oz - (split.Position.Z - dn.Z) * ox);   // which side of the axis the carriageway lies
                double a = -side * 12 * Math.PI / 180, fx = ox * Math.Cos(a) - oz * Math.Sin(a), fz = ox * Math.Sin(a) + oz * Math.Cos(a);
                var far = nodes[Math.Clamp(splitIdx + (splitIdx == lo ? -1 : 1), 0, nodes.Count - 1)];
                double tx = far.Position.X - split.Position.X, tz = far.Position.Z - split.Position.Z, tl = Math.Sqrt(tx * tx + tz * tz);
                if (tl < 1e-6) { tx = ox; tz = oz; tl = 1; }
                double L = Math.Sqrt((split.Position.X - dn.X) * (split.Position.X - dn.X) + (split.Position.Z - dn.Z) * (split.Position.Z - dn.Z)) / 3;
                return (new Vec3(dn.X + fx * L, 0, dn.Z + fz * L), new Vec3(split.Position.X - tx / tl * L, 0, split.Position.Z - tz / tl * L));
            }
            int outLo = increasing ? -1 : 1;   // the lo split lies beyond the deck end on the −axis side when the chain runs along +axis
            var (loDeck, loSplit) = Fork(sLo, lo, endLo, outLo);
            var (hiDeck, hiSplit) = Fork(sHi, hi, endHi, -outLo);
            if (forward) { o.Edges.Add(new Round9.NetEdge($"br001_in_{sLo.Id}", sLo.Id, endLo, prefab, C1: loSplit, C2: loDeck, Note: "BR001 approach onto the merged deck, 12° fork")); o.Edges.Add(new Round9.NetEdge($"br001_out_{sHi.Id}", endHi, sHi.Id, prefab, C1: hiDeck, C2: hiSplit, Note: "BR001 approach off the merged deck, 12° fork")); }
            else { o.Edges.Add(new Round9.NetEdge($"br001_in_{sHi.Id}", sHi.Id, endHi, prefab, C1: hiSplit, C2: hiDeck, Note: "BR001 approach onto the merged deck, 12° fork")); o.Edges.Add(new Round9.NetEdge($"br001_out_{sLo.Id}", endLo, sLo.Id, prefab, C1: loDeck, C2: loSplit, Note: "BR001 approach off the merged deck, 12° fork")); }
            var replaced = edges.Skip(lo).Take(hi - lo).ToList();
            o.Replaces.AddRange(replaced.Select(e => e.Id));
            problems.Add(F($"carriageway {start.Id}: {replaced.Count} courses between {sLo.Id} and {sHi.Id} replaced; connectors {Len(sLo.Position, new Vec3(o.Nodes.First(x => x.Id == endLo).X, 0, o.Nodes.First(x => x.Id == endLo).Z)):F0} / {Len(sHi.Position, new Vec3(o.Nodes.First(x => x.Id == endHi).X, 0, o.Nodes.First(x => x.Id == endHi).Z)):F0} m"));
            sel.RemoveAll(e => !replaced.Contains(e) && edges.Contains(e));   // only the replaced part leaves the network
        }
        // Clearance over the courses that pass beneath the deck (any layer, not BR001's own).
        double minClear = double.MaxValue; string under = "";
        foreach (var e in res.Edges.Where(e => !sel.Contains(e)))
            for (int k = 0; k <= 8; k++)
            {
                var p = e.Curve.Eval(k / 8.0); double sp = Along(p);
                if (sp < -half || sp > half) continue;
                double off = Math.Abs((p.X - cx) * -uz + (p.Z - cz) * ux);
                if (off > 20) continue;
                double dy = DeckY(sp) - p.Y * Outputs.YScale;
                if (dy < minClear) { minClear = dy; under = $"{e.Id} {e.Map.Cs2Prefab}"; }
            }
        if (minClear < double.MaxValue) problems.Add(F($"clearance over {under}: {minClear:F1} m (deck − course, output metres)"));
        if (waterOut != null)
        {
            double minW = double.MaxValue;
            for (int k = 0; k <= 40; k++) { double sp = -half + total * k / 40; if (waterOut(cx + ux * sp, cz + uz * sp) is { } w) minW = Math.Min(minW, DeckY(sp) - w); }
            if (minW < double.MaxValue) problems.Add(F($"deck over the water: min {minW:F1} m"));
        }
        return carriage.Count;
    }
}
