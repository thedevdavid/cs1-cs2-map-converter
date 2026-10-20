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

    /// <summary>Deck under a point: the nearest imported road course within maxD m in plan, its output y (linear between the end heights) and the edge.</summary>
    public sealed class DeckSampler(IEnumerable<OutEdge> edges, double maxD = 6)
    {
        readonly List<(OutEdge E, Vec3[] P)> net = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab != "Pavement Path")
            .Select(e => (e, Enumerable.Range(0, 33).Select(k => e.Curve.Eval(k / 32.0)).ToArray())).ToList();

        public (double Y, OutEdge Edge, double D)? At(double x, double z)
        {
            (double Y, OutEdge Edge, double D)? best = null;
            foreach (var (e, p) in net)
                for (int k = 0; k < 32; k++)
                {
                    double dx = p[k + 1].X - p[k].X, dz = p[k + 1].Z - p[k].Z, l2 = dx * dx + dz * dz;
                    double u = l2 > 0 ? Math.Clamp(((x - p[k].X) * dx + (z - p[k].Z) * dz) / l2, 0, 1) : 0;
                    double d = Math.Sqrt((p[k].X + u * dx - x) * (p[k].X + u * dx - x) + (p[k].Z + u * dz - z) * (p[k].Z + u * dz - z));
                    if (d > maxD || best is { } b && b.D <= d) continue;
                    double t = (k + u) / 32;
                    best = (Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A)), e, d);
                }
            return best;
        }
    }

    /// <summary>Assumed depth of a CS2 elevated highway deck below its road surface (y of the course), output metres; geometry, so not scaled.</summary>
    public const double DeckDepth = 2.0;
    public const double StoneArchPitch = 28;

    public sealed record Result(List<PlacedObject> Main, List<PlacedObject> Br006Arches, List<PlacedObject> Gantries, List<PlacedObject> Piers, List<string> Log);

    /// <summary>
    /// Round 22 (owner, A-v6 review):
    /// - main file = BR006 variant (a): no kit objects at BR006 (the BXP truss arch deck stays alone), only the BR022 pads (grounded, boundsMin).
    /// - BR006 variant (b), its own file: 7 StoneArchRailBridge pieces at 28 m pitch along the arch axis (PO yaw; the 28 m side along the axis, so
    ///   rotation = yaw − 90), each top at the deck soffit (deck y − DeckDepth − bounds.max.y); the legs reach 100 m down into the terrain.
    /// - piers: under each BR001 / BR003 PO pier an ITBP_Pillar Piers 01 cap with its top at the soffit, and a stack of Pillar4x16 (4 × 16 × 4 m,
    ///   pivot at the base) from the cap down until the lowest piece reaches the terrain. Deck y from the course above the PO position.
    /// - gantry: grounded as before.
    /// </summary>
    public static Result Build(List<Row> rows, Func<double, double, double>? terrain, double heightFactor, Dictionary<string, Bound> bounds, DeckSampler deck)
    {
        var main = new List<PlacedObject>(); var arches = new List<PlacedObject>(); var gantries = new List<PlacedObject>(); var piers = new List<PlacedObject>(); var log = new List<string>();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var unmeasured = new SortedSet<string>();
        Bound B(string prefab) { if (bounds.TryGetValue(prefab, out var b)) return b; unmeasured.Add(prefab); return new Bound(0, 0); }
        double? Grounded(double x, double z, double bottom, double minY)
            => terrain != null && bottom - terrain(x, z) <= 1.5 ? null : Math.Round(bottom * heightFactor - minY, 2);
        PlacedObject P(string id, string prefab, double x, double z, double? y, double yaw, string note)
            => new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), y, Math.Round(((yaw % 360) + 360) % 360, 1), "PO structure", note);
        var byId = rows.ToDictionary(r => r.Id);
        int pierPieces = 0, noDeck = 0; double maxStack = 0;
        foreach (var r in rows)
        {
            if (r.Prefab.StartsWith(Arch))
            {
                var twin = byId.GetValueOrDefault(274) ?? r;
                double cx = (r.X + twin.X) / 2, cz = (r.Z + twin.Z) / 2, yr = r.Yaw * Math.PI / 180, fx = Math.Sin(yr), fz = Math.Cos(yr);
                var ab = B(Arch);
                for (int k = -3; k <= 3; k++)
                {
                    double x = cx + k * StoneArchPitch * fx, z = cz + k * StoneArchPitch * fz;
                    var d = deck.At(x, z);
                    double top = d?.Y - DeckDepth ?? (r.YBottom + r.Size[1]) * heightFactor;   // no course found: the CS1 arch top
                    arches.Add(P($"pos{r.Id}_{k + 3}", Arch, x, z, Math.Round(top - ab.MaxY, 2), r.Yaw - 90,
                        d is { } dd ? $"BR006 variant b, piece {k + 4}/7 under {dd.Edge.Id} (deck y {dd.Y:F1})" : "BR006 variant b, no course within 6 m: CS1 arch top"));
                }
                log.Add(string.Format(ci, "BR006 variant b: 7 {0} at {1} m pitch along yaw {2} from ({3:F1}, {4:F1}), rotation {5}", Arch, StoneArchPitch, r.Yaw, cx, cz, r.Yaw - 90));
            }
            else if (r.Prefab.StartsWith(Pad))
                main.Add(P($"pos{r.Id}", Pad, r.X, r.Z, Grounded(r.X, r.Z, r.YBottom, B(Pad).MinY), r.Yaw, $"PO {r.Id} {r.Role} ({r.Size[0]:F0} × {r.Size[1]:F0} × {r.Size[2]:F0} m)"));
            else if (r.Prefab == Gantry)
            {
                double bottom = new[] { 619, 621 }.Where(byId.ContainsKey).Select(i => byId[i].YBottom).DefaultIfEmpty(r.YBottom).Min();
                gantries.Add(P($"pos{r.Id}", Gantry, r.X, r.Z, Grounded(r.X, r.Z, bottom, B(Gantry).MinY), r.Yaw, $"PO 619–621 {r.Role}"));
            }
            else if (r.Prefab == ItbpPier)
            {
                var d = deck.At(r.X, r.Z);
                double soffit = d?.Y - DeckDepth ?? (r.YBottom + r.Size[1]) * heightFactor;
                if (d == null) noDeck++;
                double capY = soffit - B(ItbpPier).MaxY;
                piers.Add(P($"pos{r.Id}", ItbpPier, r.X, r.Z, Math.Round(capY, 2), r.Yaw, d is { } dd ? $"PO {r.Id} {r.Role}: cap under {dd.Edge.Id} (deck y {dd.Y:F1})" : $"PO {r.Id} {r.Role}: no course within 6 m, CS1 pier top"));
                double ground = terrain != null ? terrain(r.X, r.Z) * heightFactor : r.YBottom * heightFactor;
                var pb = B(Pier);
                double h = pb.MaxY - pb.MinY > 0 ? pb.MaxY - pb.MinY : 16;
                int n = 0;
                for (double y = capY - h; ; y -= h)
                {
                    piers.Add(P($"pos{r.Id}_s{n}", Pier, r.X, r.Z, Math.Round(y - pb.MinY, 2), r.Yaw, $"PO {r.Id} stack piece {n + 1} under the cap"));
                    n++;
                    if (y <= ground || n >= 20) break;
                }
                pierPieces += n; maxStack = Math.Max(maxStack, capY - ground);
            }
        }
        log.Add($"main (BR006 variant a, no kit objects at BR006): {main.Count} objects ({Round8.Counts(main)}), {main.Count(o => o.Y == null)} grounded");
        log.Add($"BR006 variant b: {arches.Count} {Arch}; gantry {gantries.Count} ({(gantries.All(o => o.Y == null) ? "grounded" : "with y")})");
        log.Add(string.Format(ci, "piers: {0} caps + {1} Pillar4x16 stack pieces (cap to terrain up to {2:F1} m), {3} caps without a course within 6 m", piers.Count - pierPieces, pierPieces, maxStack, noDeck));
        if (unmeasured.Count > 0) log.Add($"not measured, pivot assumed at the base: {string.Join(", ", unmeasured)}");
        return new Result(main, arches, gantries, piers, log);
    }

    /// <summary>CS1 segments of the courses the BR001 / BR003 piers stand under: noPillars on them (the ConcreteSet01 stacks replace the game's pillars).</summary>
    public static HashSet<int> PierSegments(List<Row> rows, DeckSampler deck)
        => rows.Where(r => r.Prefab == ItbpPier).Select(r => deck.At(r.X, r.Z)).Where(d => d != null).Select(d => d!.Value.Edge.Cs1Segment).ToHashSet();
}
