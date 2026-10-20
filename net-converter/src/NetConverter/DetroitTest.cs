using System.Globalization;
using System.Text.Json;

namespace NetConverter;

/// <summary>
/// A-v8 test layer for the Detroit crossing (owner, bridges/DETROIT-ALTERNATIVES.md): BR001 stays as it is in the network; two standalone test decks go
/// parallel to it, offset north-west (away from the approaches) by 200 m and 400 m from the CS1 tower line (1314, −3786)–(1751, −3210):
/// (a) option 2, `BXP Suspension Bridge - Highway Twoway - 6 Lanes`: two edges of ~362 m between the CS1 tower positions (towers at 0 / 362 / 724 m),
///     plus 150 m plain two-way highway stubs on both sides;
/// (b) option 3, `BXP Double Deck Suspension Bridge - Highway` (one straight edge of the ~976 m river part, the game cuts it into 3 × 325 m spans) with
///     `BXP Double Deck Suspension Bridge - Lower Highway` under it (one edge, the game cuts it into ≤ 448 m pieces), plus one-way 3-lane stubs of 150 m
///     on both decks, the lower deck running the other way.
/// Deck height = BR001's. Not connected to the network (stub ends in the air), bypassValidation, absolute heights.
/// </summary>
public static class DetroitTest
{
    static readonly (double X, double Z) T1 = (1314, -3786), T2 = (1751, -3210);
    public const double RiverPart = 976, Stub = 150;
    /// <summary>TODO (live): the lower deck's offset below the upper one is a guess until /prefabs/info gives the double-deck geometry.</summary>
    public const double LowerDeckDrop = 8;

    public static List<string> Write(string outDir, double deckY, Func<double, double, double>? terrainOut, Func<double, double, double?>? waterOut, double heightFactor)
    {
        var log = new List<string>();
        double ux = T2.X - T1.X, uz = T2.Z - T1.Z, ul = Math.Sqrt(ux * ux + uz * uz); ux /= ul; uz /= ul;
        double nx = -uz, nz = ux;   // left of T1 → T2 = north-west
        double cx = (T1.X + T2.X) / 2, cz = (T1.Z + T2.Z) / 2;
        var nodes = new List<(string Id, double X, double Y, double Z)>();
        var edges = new List<(string Id, string A, string B, string Prefab, string Note)>();
        (double X, double Z) P(double along, double off) => (cx + ux * along + nx * off, cz + uz * along + nz * off);
        string N(string id, double along, double off, double y) { var (x, z) = P(along, off); nodes.Add((id, Math.Round(x, 2), Math.Round(y, 2), Math.Round(z, 2))); return id; }
        string Check(double off)
        {
            if (terrainOut == null) return "no terrain";
            int land = 0, total = 0; double minClear = double.MaxValue;
            for (double s = -RiverPart / 2; s <= RiverPart / 2; s += 8)
            {
                var (x, z) = P(s, off); total++;
                if (waterOut?.Invoke(x, z) is null) land++;
                minClear = Math.Min(minClear, deckY - terrainOut(x, z));
            }
            return string.Create(CultureInfo.InvariantCulture, $"{land}/{total} samples of the river part over land (no CS1 water), deck {minClear:F1} m above the lowest terrain gap");
        }
        // Offsets 200 m and 400 m north-west, as asked. Land check: the 976 m river part includes the banks; BR001's own line has about a third over land,
        // and the NW lines 150–900 m are no better (logged), so no island forces another offset.
        int Land(double off) { int l = 0; for (double s = -RiverPart / 2; s <= RiverPart / 2; s += 8) { var (x, z) = P(s, off); if (waterOut?.Invoke(x, z) is null) l++; } return l; }
        var cand = Enumerable.Range(0, 31).Select(k => 150.0 + 25 * k).Select(off => (Off: off, Land: Land(off))).OrderBy(c => c.Land).ThenBy(c => Math.Abs(c.Off - 200)).ToList();
        double o2 = 200, o3 = 400;
        log.Add($"BR001 line itself: {Land(0)}/123 samples of the river part over land; best NW offsets {string.Join(", ", cand.Take(5).Select(c => $"{c.Off} m {c.Land}"))}");
        // (a) option 2.
        double half = ul / 2;
        N("dt2_s0", -half - Stub, o2, deckY); N("dt2_t1", -half, o2, deckY); N("dt2_m", 0, o2, deckY); N("dt2_t2", half, o2, deckY); N("dt2_s3", half + Stub, o2, deckY);
        edges.Add(("dt2_stub_w", "dt2_s0", "dt2_t1", "Highway Twoway - 4 lanes", "plain stub (no vanilla 6-lane two-way highway known)"));
        edges.Add(("dt2_span_1", "dt2_t1", "dt2_m", "BXP Suspension Bridge - Highway Twoway - 6 Lanes", $"{half:F0} m, CS1 tower T1 → mid"));
        edges.Add(("dt2_span_2", "dt2_m", "dt2_t2", "BXP Suspension Bridge - Highway Twoway - 6 Lanes", $"{half:F0} m, mid → CS1 tower T2"));
        edges.Add(("dt2_stub_e", "dt2_t2", "dt2_s3", "Highway Twoway - 4 lanes", "plain stub"));
        log.Add(string.Create(CultureInfo.InvariantCulture, $"option 2 at {o2} m NW: 2 × {half:F0} m BXP twoway 6-lane spans + 2 × {Stub} m stubs; {Check(o2)}"));
        // (b) option 3 at 400 m: upper deck runs T1 → T2, the lower deck T2 → T1 under it.
        double r = RiverPart / 2, lowY = deckY - LowerDeckDrop;
        N("dt3_u0", -r - Stub, o3, deckY); N("dt3_u1", -r, o3, deckY); N("dt3_u2", r, o3, deckY); N("dt3_u3", r + Stub, o3, deckY);
        N("dt3_l0", r + Stub, o3, lowY); N("dt3_l1", r, o3, lowY); N("dt3_l2", -r, o3, lowY); N("dt3_l3", -r - Stub, o3, lowY);
        edges.Add(("dt3_upper_stub_w", "dt3_u0", "dt3_u1", "Highway Oneway - 3 lanes", "plain stub"));
        edges.Add(("dt3_upper", "dt3_u1", "dt3_u2", "BXP Double Deck Suspension Bridge - Highway", $"{RiverPart:F0} m, fixed 320 repeated (the game cuts 3 × 325 m), level (max slope 5 %)"));
        edges.Add(("dt3_upper_stub_e", "dt3_u2", "dt3_u3", "Highway Oneway - 3 lanes", "plain stub"));
        edges.Add(("dt3_lower_stub_e", "dt3_l0", "dt3_l1", "Highway Oneway - 3 lanes", "plain stub"));
        edges.Add(("dt3_lower", "dt3_l1", "dt3_l2", "BXP Double Deck Suspension Bridge - Lower Highway", $"{RiverPart:F0} m, {LowerDeckDrop} m below the upper deck (TODO: measure live)"));
        edges.Add(("dt3_lower_stub_w", "dt3_l2", "dt3_l3", "Highway Oneway - 3 lanes", "plain stub"));
        log.Add(string.Create(CultureInfo.InvariantCulture, $"option 3 at {o3} m NW: upper {RiverPart:F0} m + lower {RiverPart:F0} m ({LowerDeckDrop:F1} m lower, guess) + 4 × {Stub} m stubs; {Check(o3)}"));

        // Round 28 (A-v8 crashed natively during the combined step): one file per option, so each can be tried on its own after verify-end.
        File.Delete(Path.Combine(outDir, "import-detroit-test.json"));
        foreach (var (file, tag, what) in new[] { ("import-detroit-opt2.json", "dt2_", "option 2, BXP Suspension Bridge - Highway Twoway - 6 Lanes"), ("import-detroit-opt3.json", "dt3_", "option 3, BXP Double Deck Suspension Bridge - Highway + Lower Highway") })
        {
            using var fs = File.Create(Path.Combine(outDir, file));
            using var w = new Utf8JsonWriter(fs, Json.Indented);
            w.WriteStartObject();
            w.WriteString("name", Path.GetFileNameWithoutExtension(file).Replace("import-", ""));
            w.WriteString("_comment", $"TEST ONLY, {what}, next to the current BR001 (owner decides after seeing it). Standalone decks, not connected; undo with POST /import/undo?id=<importId>. A-v8: the combined test crashed the game natively; run this after verify-end.");
            w.WriteStartArray("nodes");
            foreach (var (id, x, y, z) in nodes.Where(nd => nd.Id.StartsWith(tag))) { w.WriteStartObject(); w.WriteString("id", id); w.WriteNumber("x", x); w.WriteNumber("y", y); w.WriteNumber("z", z); w.WriteEndObject(); }
            w.WriteEndArray();
            w.WriteStartArray("edges");
            foreach (var (id, a, b, prefab, note) in edges.Where(e => e.Id.StartsWith(tag))) { w.WriteStartObject(); w.WriteString("id", id); w.WriteString("a", a); w.WriteString("b", b); w.WriteString("prefab", prefab); w.WriteString("note", note); w.WriteEndObject(); }
            w.WriteEndArray();
            w.WriteStartObject("options");
            w.WriteNumber("batchSize", 1); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", "absolute"); w.WriteBoolean("dryRun", false); w.WriteBoolean("bypassValidation", true);
            w.WriteEndObject();
            w.WriteEndObject();
        }
        return log;
    }
}
