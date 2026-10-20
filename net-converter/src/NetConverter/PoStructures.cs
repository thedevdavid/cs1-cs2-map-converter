using System.Text.Json;

namespace NetConverter;

/// <summary>
/// Round 20: the PO structure objects of bridges/PLAN-structures.md, from bridges/data/po-structures-proposal.json (one row per PO object).
/// BR006 red deck arch: one StoneArchRailBridge at the centre of the two arch meshes (27, 274) plus ConcreteSet01 Pillar1x16 columns and
/// Pillar4x16 piers at the 68 cube positions; BR022 pads as BasePlateConcrete32. Optional: the gantry 619–621 (HWY Truss Gantry 2L  A) and
/// the ITBP piers of BR001 / BR003 (only once noPillars works). Everything else (dam blocks, abutments, rock shed, bearings, net swaps) is
/// left out, as the plan says.
/// </summary>
public static class PoStructures
{
    public sealed record Row(int Id, string Role, string Prefab, double X, double Z, double Yaw, double YBottom, double[] Size);
    public sealed record Bound(double MinY, double MaxY);

    public const string Arch = "StoneArchRailBridge", Column = "Pillar1x16", Pier = "Pillar4x16", Pad = "BasePlateConcrete32",
        Gantry = "HWY Truss Gantry 2L  A", ItbpPier = "ITBP_Pillar Piers 01";

    public static List<Row> Load(string path)
    {
        var res = new List<Row>();
        if (!File.Exists(path)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        foreach (var o in doc.RootElement.EnumerateArray())
            res.Add(new Row(o.GetProperty("id").GetInt32(), o.GetProperty("role").GetString() ?? "", o.GetProperty("prefab").GetString() ?? "",
                o.GetProperty("x").GetDouble(), o.GetProperty("z").GetDouble(), o.GetProperty("yaw").GetDouble(), o.GetProperty("yBottom").GetDouble(),
                o.GetProperty("size").EnumerateArray().Select(v => v.GetDouble()).ToArray()));
        return res;
    }

    /// <summary>Live bounds from po-structure-bounds.json (tools/fetch-po-structure-bounds.mjs); prefabs the game did not have are absent.</summary>
    public static Dictionary<string, Bound> LoadBounds(string path)
    {
        var res = new Dictionary<string, Bound>();
        if (!File.Exists(path)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        foreach (var p in doc.RootElement.GetProperty("prefabs").EnumerateArray())
            if (p.TryGetProperty("boundsMin", out var mn) && p.TryGetProperty("boundsMax", out var mx))
                res[p.GetProperty("name").GetString()!] = new Bound(mn.GetProperty("y").GetDouble(), mx.GetProperty("y").GetDouble());
        return res;
    }

    /// <summary>
    /// y: none (the bridge grounds the mesh bottom on the terrain, boundsMin) where the PO bottom is within 1.5 m of the CS2 terrain; else
    /// absolute, bottom × height factor − bounds.min.y (pivot at the base, min.y 0, where the prefab was not measured: logged).
    /// The arch always gets y = foot height: its bounds reach 100 m below the pivot (a buried foundation), so boundsMin would lift it 100 m.
    /// </summary>
    public static (List<PlacedObject> Main, List<PlacedObject> Gantries, List<PlacedObject> Piers, List<string> Log) Build(
        List<Row> rows, Func<double, double, double>? terrain, double heightFactor, Dictionary<string, Bound> bounds)
    {
        var main = new List<PlacedObject>(); var gantries = new List<PlacedObject>(); var piers = new List<PlacedObject>(); var log = new List<string>();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var unmeasured = new SortedSet<string>();
        double? YFor(string prefab, double x, double z, double bottom)
        {
            if (terrain != null && bottom - terrain(x, z) <= 1.5) return null;

            if (!bounds.TryGetValue(prefab, out var b)) { unmeasured.Add(prefab); b = new Bound(0, 0); }
            return Math.Round(bottom * heightFactor - b.MinY, 2);
        }
        PlacedObject P(string id, string prefab, double x, double z, double? y, double yaw, string note)
            => new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), y, Math.Round(((yaw % 360) + 360) % 360, 1), "PO structure", note);
        var byId = rows.ToDictionary(r => r.Id);
        foreach (var r in rows)
        {
            if (r.Prefab.StartsWith(Arch))
            {
                // The arch centre: midpoint of the two red arch meshes (27 and its twin 274).
                var twin = byId.GetValueOrDefault(274) ?? r;
                double x = (r.X + twin.X) / 2, z = (r.Z + twin.Z) / 2;
                main.Add(P($"pos{r.Id}", Arch, x, z, Math.Round(r.YBottom * heightFactor, 2), r.Yaw, $"PO {r.Id}+{twin.Id} {r.Role}: one arch object, CS1 arch {r.Size[0]:F0} × {r.Size[1]:F0} × {r.Size[2]:F0} m"));
                log.Add(string.Format(ci, "BR006 arch {0} at ({1:F1}, {2:F1}) yaw {3}, foot y {4:F1} × {5}", Arch, x, z, r.Yaw, r.YBottom, heightFactor));
            }
            else if (r.Prefab.StartsWith(Column) || r.Prefab.StartsWith(Pier) || r.Prefab.StartsWith(Pad))
            {
                string prefab = r.Prefab.StartsWith(Column) ? Column : r.Prefab.StartsWith(Pier) ? Pier : Pad;
                main.Add(P($"pos{r.Id}", prefab, r.X, r.Z, YFor(prefab, r.X, r.Z, r.YBottom), r.Yaw, $"PO {r.Id} {r.Role} ({r.Size[0]:F0} × {r.Size[1]:F0} × {r.Size[2]:F0} m)"));
            }
            else if (r.Prefab == Gantry)
            {
                // The frame's posts (619, 621) stand on the deck; the gantry stands where they do.
                double bottom = new[] { 619, 621 }.Where(byId.ContainsKey).Select(i => byId[i].YBottom).DefaultIfEmpty(r.YBottom).Min();
                gantries.Add(P($"pos{r.Id}", Gantry, r.X, r.Z, YFor(Gantry, r.X, r.Z, bottom), r.Yaw, $"PO 619–621 {r.Role}"));
            }
            else if (r.Prefab == ItbpPier)
            {
                // A 3.6 m pier cap: its top under the deck, at the top of the PO pier.
                double top = r.YBottom + r.Size[1];
                double maxY = bounds.TryGetValue(ItbpPier, out var b) ? b.MaxY : 0;
                if (maxY == 0) unmeasured.Add(ItbpPier);
                piers.Add(P($"pos{r.Id}", ItbpPier, r.X, r.Z, Math.Round(top * heightFactor - maxY, 2), r.Yaw, $"PO {r.Id} {r.Role}: cap top at the PO pier top"));
            }
        }
        log.Add($"{main.Count} objects ({Round8.Counts(main)}), {main.Count(o => o.Y == null)} grounded (no y, boundsMin), {main.Count(o => o.Y != null)} with y (the arch foot, columns standing on the arch)");
        log.Add($"optional: {gantries.Count} gantry ({(gantries.All(o => o.Y == null) ? "grounded" : "with y")}), {piers.Count} piers ({Round8.Counts(piers)}), cap tops at the PO pier tops");
        if (unmeasured.Count > 0) log.Add($"not in the running game, pivot assumed at the base: {string.Join(", ", unmeasured)}");
        return (main, gantries, piers, log);
    }
}
