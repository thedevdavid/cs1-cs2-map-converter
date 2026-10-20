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

    public sealed record Spec(string Id, Regex Select, string Prefab, double[]? Fixed, int? Count, double? Span, bool Merge, bool FixedFile, double Seg, bool SegAssumed, string Notes, string Alt);

    public static List<Spec> Load(string path)
    {
        var res = new List<Spec>();
        if (!File.Exists(path)) return res;
        foreach (var line in File.ReadAllLines(path).Where(l => l.Length > 0 && !l.StartsWith('#')))
        {
            var c = line.Split('\t');
            double[]? fx = null; int? count = null; double? span = null; bool merge = false, ff = false;
            foreach (var part in c[3].Split(';'))
            {
                if (part.StartsWith("fixed:")) fx = part[6..].Split(',').Select(v => double.Parse(v, Ci)).ToArray();
                else if (part.StartsWith("count:")) count = int.Parse(part[6..], Ci);
                else if (part.StartsWith("span:")) span = double.Parse(part[5..], Ci);
                else if (part == "merge") merge = true;
                else if (part == "fixedfile") ff = true;
            }
            var segTxt = c[4].Trim();
            res.Add(new Spec(c[0], new Regex(c[1], RegexOptions.CultureInvariant), c[2], fx, count, span, merge, ff,
                double.Parse(segTxt.Split(' ')[0], Ci), segTxt.Contains("[U]"), c.Length > 5 ? c[5] : "", c.Length > 6 ? c[6] : ""));
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

    /// <summary>Straight, constant-grade edges between a chain's end nodes (flow direction of the chain's first edge kept).</summary>
    static List<double> Resegment(Spec s, string tag, List<OutEdge> chain, OutNode start, OutNode end, Dictionary<OutNode, int> deg, Output o, List<string> problems, Func<double, double, double?>? waterOut)
    {
        bool forward = chain[0].A == start;
        var (a, b) = forward ? (start, end) : (end, start);
        double chord = Len(a.Position, b.Position), curve = chain.Sum(e => e.Curve.LengthXZ());
        int n = s.Count ?? (s.Span is { } sp ? Math.Max(1, (int)Math.Ceiling(chord / sp - 1e-9)) : 1);
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
            o.Edges.Add(new Round9.NetEdge($"{tag}_e{k}", ids[k], ids[k + 1], s.Prefab, Note: F($"{s.Id} bridge edge {k + 1}/{n}; CS1 segments {string.Join(",", chain.Select(e => e.Cs1Segment).Distinct())}")));
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
        bool enabled, bool fixedOn, Func<double, double, double?>? waterOut, double topologyFactor)
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
            if (s.Merge && s.Fixed != null) chains = Merge(s, sel, root, deg, target, problems, lengths, res, waterOut, topologyFactor);
            else
            {
                var decorDeg = deg;
                if (s.FixedFile)
                {
                    // Decor (BR022) is not in the imported layers: degrees within the selection.
                    decorDeg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
                    foreach (var e in sel) foreach (var n in new[] { e.A, e.B }) decorDeg[n] = decorDeg.GetValueOrDefault(n) + 1;
                }
                foreach (var (chain, a, b) in Chains(sel, decorDeg)) lengths.AddRange(Resegment(s, $"{s.Id.ToLowerInvariant()}_c{++chains}", chain, a, b, decorDeg, target, problems, waterOut));
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
        var ids = new List<string>();
        for (int k = 0; k < cuts.Length; k++)
        {
            string id = $"br001_m{k}";
            o.Nodes.Add(new Round9.NetNode(id, cx + ux * cuts[k], DeckY(cuts[k]), cz + uz * cuts[k]));
            ids.Add(id);
        }
        for (int k = 0; k < s.Fixed.Length; k++)
        {
            o.Edges.Add(new Round9.NetEdge($"br001_span{k}", ids[k], ids[k + 1], s.Prefab, Note: F($"BR001 fixed span {k}: {s.Fixed[k]:F0} m"), FixedIndex: k));
            lengths.Add(s.Fixed[k]);
        }
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
            if (forward) { o.Edges.Add(new Round9.NetEdge($"br001_in_{sLo.Id}", sLo.Id, endLo, prefab, Note: "BR001 approach onto the merged deck")); o.Edges.Add(new Round9.NetEdge($"br001_out_{sHi.Id}", endHi, sHi.Id, prefab, Note: "BR001 approach off the merged deck")); }
            else { o.Edges.Add(new Round9.NetEdge($"br001_in_{sHi.Id}", sHi.Id, endHi, prefab, Note: "BR001 approach onto the merged deck")); o.Edges.Add(new Round9.NetEdge($"br001_out_{sLo.Id}", endLo, sLo.Id, prefab, Note: "BR001 approach off the merged deck")); }
            var replaced = edges.Skip(lo).Take(hi - lo).ToList();
            o.Replaces.AddRange(replaced.Select(e => e.Id));
            problems.Add(F($"carriageway {start.Id}: {replaced.Count} courses between {sLo.Id} and {sHi.Id} replaced; connectors {Len(sLo.Position, new Vec3(o.Nodes.First(x => x.Id == endLo).X, 0, o.Nodes.First(x => x.Id == endLo).Z)):F0} / {Len(sHi.Position, new Vec3(o.Nodes.First(x => x.Id == endHi).X, 0, o.Nodes.First(x => x.Id == endHi).Z)):F0} m"));
            sel.RemoveAll(e => !replaced.Contains(e) && edges.Contains(e));   // only the replaced part leaves the network
        }
        // Clearance over the courses that pass beneath the deck (any layer, not BR001's own).
        var deck = ids.Select(id => o.Nodes.First(n => n.Id == id)).ToList();
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
