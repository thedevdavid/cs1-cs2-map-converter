using System.Globalization;
using System.Text.Json;

namespace NetConverter;

/// <summary>
/// Owner review of Run A, round 8: the dam building, the 345 kV line ends at the substation and the dam, pack cliff objects at the
/// PO mountains / rock work / falls steps, and a rock scatter around every rock and inside the rock surfaces. Each is its own import
/// file so it can be added to the live map.
/// </summary>
public static class Round8
{
    static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    /// <summary>Placeholders until the bridge reports the prefab names (owner: the usable CS2 buildings).</summary>
    public const string DamPrefab = "Hydroelectric Power Plant", SubstationPrefab = "Transformer Station";

    public static readonly string[] SmallRocks = Enumerable.Range(1, 10).Select(i => $"Rock{i:00}").ToArray();
    public static readonly string[] MediumRocks = Enumerable.Range(11, 20).Select(i => $"Rock{i:00}").ToArray();
    public static readonly string[] Boulders = Enumerable.Range(1, 8).Select(i => $"Boulder{i:00}").ToArray();
    static readonly string[] AridRocks = ["RZaridrock1", "RZaridrock2", "RZaridrock3", "RZaridrock4", "RZaridrock5"];
    static readonly string[] RzRocks = Enumerable.Range(1, 8).Select(i => $"rzrock{i}").ToArray();
    static readonly string[] MidCliffs = ["CliffRock5", "CliffRock6", "CliffStack"];
    static readonly string[] SideCliffs = ["CliffSide2", "CliffSide2Left", "CliffSide2Right"];

    public sealed record Building(int Id, string Prefab, double X, double Z, double Yaw);

    public static List<Building> Buildings(JsonElement root, string pattern) =>
        root.GetProperty("buildings").EnumerateArray()
            .Where(b => System.Text.RegularExpressions.Regex.IsMatch(b.GetProperty("prefab").GetString() ?? "", pattern))
            .Select(b => { var p = b.GetProperty("position").EnumerateArray().Select(v => v.GetDouble()).ToArray();
                return new Building(b.GetProperty("id").GetInt32(), b.GetProperty("prefab").GetString()!, p[0], p[2], Objects.Yaw(b.GetProperty("angle").GetDouble())); })
            .ToList();

    // ---------- 2. Dam ----------

    /// <summary>
    /// One dam at the centre of the CS1 Dam Electricity cluster. Variant A uses the converter's yaw (−CS1 angle), B its mirror
    /// (+CS1 angle, i.e. 360 − A) until the bridge's live test shows which convention spans the river.
    /// </summary>
    public static (PlacedObject A, PlacedObject B)? Dam(List<Building> dams)
    {
        if (dams.Count == 0) return null;
        double x = Math.Round(dams.Average(d => d.X), 1), z = Math.Round(dams.Average(d => d.Z), 1), yaw = dams[0].Yaw;
        string ids = string.Join("+", dams.Select(d => d.Id));
        return (new PlacedObject("dam_A", DamPrefab, "building", x, z, null, yaw, dams[0].Prefab, $"buildings {ids} (variant A, yaw −CS1 angle)"),
                new PlacedObject("dam_B", DamPrefab, "building", x, z, null, Math.Round((360 - yaw) % 360, 2), dams[0].Prefab, $"buildings {ids} (variant B, mirrored yaw)"));
    }

    // ---------- 3. Power line ends ----------

    public sealed record PowerEnd(string Target, OutEdge Replaces, OutNode Anchor, OutNode OldEnd, double X, double Z);

    /// <summary>
    /// The power-line dead end nearest each building, matched one to one (nearest pair first). The new course runs from the old
    /// edge's far node (the anchor) to the building centre moved 8 m toward the anchor (fallback until the bridge reports the
    /// connection point).
    /// </summary>
    public static List<PowerEnd> PowerEnds(IReadOnlyList<OutEdge> power, IReadOnlyList<(string Name, double X, double Z)> targets)
    {
        var deg = new Dictionary<OutNode, int>(ReferenceEqualityComparer.Instance);
        foreach (var e in power) foreach (var n in new[] { e.A, e.B }) deg[n] = deg.GetValueOrDefault(n) + 1;
        var ends = deg.Where(kv => kv.Value == 1 && !kv.Key.Border).Select(kv => kv.Key).ToList();
        var pairs = (from t in targets from n in ends
                     select (t, n, d: Math.Sqrt((n.Position.X - t.X) * (n.Position.X - t.X) + (n.Position.Z - t.Z) * (n.Position.Z - t.Z)))).OrderBy(p => p.d).ToList();
        var res = new List<PowerEnd>();
        var usedT = new HashSet<string>(); var usedN = new HashSet<OutNode>(ReferenceEqualityComparer.Instance);
        foreach (var (t, n, _) in pairs)
        {
            if (usedT.Contains(t.Name) || usedN.Contains(n)) continue;
            usedT.Add(t.Name); usedN.Add(n);
            var e = power.First(e => e.A == n || e.B == n);
            var anchor = e.A == n ? e.B : e.A;
            double dx = anchor.Position.X - t.X, dz = anchor.Position.Z - t.Z, L = Math.Sqrt(dx * dx + dz * dz);
            res.Add(new PowerEnd(t.Name, e, anchor, n, t.X + 8 * dx / L, t.Z + 8 * dz / L));
        }
        return res;
    }

    public static void WritePowerEnds(string path, List<PowerEnd> ends, Func<double, double, double>? cs2TerrainTrue, double heightFactor, bool absolute)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", "Dakota Falls power line ends");
        w.WriteString("_comment", "Replaces the last course of each 345 kV circuit (undo the edges in 'replaces' first). The end node sits on the building centre moved 8 m toward the line (fallback until the bridge reports the connection point); the start node joins the existing pylon node through snapToSegment (radius 2).");
        w.WriteStartArray("replaces"); foreach (var e in ends) w.WriteStringValue(e.Replaces.Id); w.WriteEndArray();
        w.WriteStartArray("nodes");
        foreach (var e in ends)
        {
            w.WriteStartObject();
            w.WriteString("id", e.Anchor.Id);
            w.WriteNumber("x", Json.R2(e.Anchor.Position.X)); w.WriteNumber("y", Json.R2((absolute ? e.Anchor.Terrain + e.Anchor.Elevation : e.Anchor.Position.Y) * Outputs.YScale)); w.WriteNumber("z", Json.R2(e.Anchor.Position.Z));
            w.WriteNumber("elevation", Json.R2(e.Anchor.Elevation * Outputs.YScale));
            w.WriteStartObject("snapToSegment"); w.WriteNumber("radius", 2); w.WriteString("prefabContains", e.Replaces.Map.Cs2Prefab); w.WriteEndObject();
            w.WriteEndObject();
            w.WriteStartObject();
            w.WriteString("id", $"{e.Target}_end");
            w.WriteNumber("x", Json.R2(e.X)); w.WriteNumber("z", Json.R2(e.Z));
            double ground = cs2TerrainTrue != null ? cs2TerrainTrue(e.X, e.Z) * heightFactor : e.OldEnd.Terrain * Outputs.YScale;
            w.WriteNumber("y", Json.R2(ground)); w.WriteNumber("elevation", 0);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        foreach (var e in ends)
        {
            w.WriteStartObject();
            w.WriteString("id", $"{e.Replaces.Id}_to_{e.Target}");
            w.WriteString("a", e.Anchor.Id); w.WriteString("b", $"{e.Target}_end");
            w.WriteString("prefab", e.Replaces.Map.Cs2Prefab);
            w.WriteString("replaces", e.Replaces.Id);
            w.WriteString("cs1_prefab", e.Replaces.Map.Cs1Prefab);
            w.WriteNumber("cs1_segment", e.Replaces.Cs1Segment);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 64); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", absolute ? "absolute" : "elevation"); w.WriteBoolean("dryRun", false);
        w.WriteEndObject();
        w.WriteEndObject();
    }

    // ---------- 4. Cliff objects at the PO terrain pieces ----------

    public sealed record PoPiece(int Id, string SubClass, double X, double Z, double Yaw, double Length, double Width, double Height, double SummitX, double SummitZ);

    public static List<PoPiece> LoadPoPieces(string path)
    {
        var res = new List<PoPiece>();
        if (!File.Exists(path)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        foreach (var o in doc.RootElement.GetProperty("objects").EnumerateArray())
        {
            string sub = o.TryGetProperty("subClass", out var s) ? s.GetString() ?? "" : "";
            if (o.GetProperty("class").GetString() != "terrain-like" || sub is not ("mountain" or "rock work" or "falls steps")) continue;
            var pos = o.GetProperty("position").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            double yaw = o.TryGetProperty("yawDegrees", out var yd) && yd.ValueKind == JsonValueKind.Number ? yd.GetDouble() : 0;
            yaw = (yaw % 360 + 360) % 360;
            double r = yaw * Math.PI / 180, fx = Math.Sin(r), fz = Math.Cos(r);   // forward (0 = +z, clockwise)
            double minF = double.MaxValue, maxF = double.MinValue, minR = double.MaxValue, maxR = double.MinValue, minY = double.MaxValue, maxY = double.MinValue, sx = pos[0], sz = pos[2];
            if (o.TryGetProperty("vertices", out var vs) && vs.ValueKind == JsonValueKind.Array && vs.GetArrayLength() >= 9)
            {
                var v = vs.EnumerateArray().Select(x => x.GetDouble()).ToArray();
                for (int k = 0; k + 2 < v.Length; k += 3)
                {
                    double dx = v[k] - pos[0], dz = v[k + 2] - pos[2];
                    double f = dx * fx + dz * fz, rr = dx * fz - dz * fx;
                    minF = Math.Min(minF, f); maxF = Math.Max(maxF, f); minR = Math.Min(minR, rr); maxR = Math.Max(maxR, rr);
                    if (v[k + 1] > maxY) { maxY = v[k + 1]; sx = v[k]; sz = v[k + 2]; }
                    minY = Math.Min(minY, v[k + 1]);
                }
            }
            else
            {
                var mn = o.GetProperty("boundsMin").EnumerateArray().Select(x => x.GetDouble()).ToArray();
                var mx = o.GetProperty("boundsMax").EnumerateArray().Select(x => x.GetDouble()).ToArray();
                minF = mn[2] - pos[2]; maxF = mx[2] - pos[2]; minR = mn[0] - pos[0]; maxR = mx[0] - pos[0]; minY = mn[1]; maxY = mx[1];
            }
            res.Add(new PoPiece(o.GetProperty("id").GetInt32(), sub, pos[0], pos[2], yaw, maxF - minF, maxR - minR, maxY - minY, sx, sz));
        }
        return res;
    }

    /// <summary>
    /// Pack cliff objects by footprint (the larger horizontal extent in the object's own frame): 3–12 m Arid rocks and Boulders,
    /// 12–36 m RZ rocks, 36–80 m CliffRock5/6/CliffStack, 80–160 m by aspect and height (CliffSide2/Left/Right, CliffSideTall, CliffSlim,
    /// CliffFreeStanding), &gt; 160 m a CliffFreeStanding at the summit plus CliffSide pieces every 60 m down the steepest flank
    /// (CS2 heightmap). Rotation = PO yaw (flank pieces face down the flank); no y. Largest pieces first, capped at <paramref name="cap"/>.
    /// </summary>
    public static (List<PlacedObject> Objects, int Capped, Dictionary<string, int> PerBand) Cliffs(List<PoPiece> pieces, Func<double, double, double>? terrain,
        double half, ulong seed, int cap = 2500)
    {
        var all = new List<(double Size, PlacedObject O)>();
        var bands = new Dictionary<string, int>();
        void Add(PoPiece p, string prefab, double x, double z, double yaw, string band, int k)
        {
            if (Math.Abs(x) > half - 1 || Math.Abs(z) > half - 1) return;
            all.Add((Math.Max(p.Length, p.Width), new PlacedObject($"pocliff{p.Id}_{k}", prefab, "prop", Math.Round(x, 2), Math.Round(z, 2), null,
                Math.Round((yaw % 360 + 360) % 360, 2), $"PO {p.SubClass}", $"po {p.Id}")));
            bands[band] = bands.GetValueOrDefault(band) + 1;
        }
        foreach (var p in pieces.OrderBy(p => p.Id))
        {
            var rng = new Rng(Rng.Mix(seed, (ulong)p.Id));
            double size = Math.Max(p.Length, p.Width), aspect = size / Math.Max(1, Math.Min(p.Length, p.Width));
            if (size < 3) continue;
            if (size < 12) Add(p, rng.Next(2) == 0 ? AridRocks[rng.Next(AridRocks.Length)] : Boulders[rng.Next(Boulders.Length)], p.X, p.Z, p.Yaw, "3–12 m", 0);
            else if (size < 36) Add(p, RzRocks[rng.Next(RzRocks.Length)], p.X, p.Z, p.Yaw, "12–36 m", 0);
            else if (size < 80) Add(p, MidCliffs[rng.Next(MidCliffs.Length)], p.X, p.Z, p.Yaw, "36–80 m", 0);
            else if (size <= 160)
            {
                string prefab = p.Height > 120 ? (aspect >= 1.6 ? "CliffSlim" : aspect >= 1.25 ? "CliffSideTall" : "CliffFreeStanding")
                              : aspect >= 1.4 ? SideCliffs[1 + rng.Next(2)] : "CliffSide2";
                Add(p, prefab, p.X, p.Z, p.Yaw, "80–160 m", 0);
            }
            else
            {
                Add(p, "CliffFreeStanding", p.SummitX, p.SummitZ, p.Yaw, "> 160 m summit", 0);
                // Steepest flank: the direction with the largest drop from the summit to the footprint edge.
                double reach = size / 2, bestDrop = double.MinValue, bdx = 0, bdz = 1;
                double top = terrain?.Invoke(p.SummitX, p.SummitZ) ?? 0;
                for (int d = 0; d < 16; d++)
                {
                    double a = d * Math.PI / 8, dx = Math.Sin(a), dz = Math.Cos(a);
                    double drop = terrain == null ? -d : top - terrain(p.SummitX + dx * reach, p.SummitZ + dz * reach);
                    if (drop > bestDrop) { bestDrop = drop; bdx = dx; bdz = dz; }
                }
                double flankYaw = Math.Atan2(bdx, bdz) * 180 / Math.PI;
                int k = 1;
                for (double s = 60; s <= reach; s += 60, k++)
                    Add(p, SideCliffs[rng.Next(SideCliffs.Length)], p.SummitX + bdx * s, p.SummitZ + bdz * s, flankYaw, "> 160 m flank", k);
            }
        }
        var kept = all.OrderByDescending(a => a.Size).ThenBy(a => a.O.Id, StringComparer.Ordinal).Take(cap).Select(a => a.O).ToList();
        return (kept, all.Count - kept.Count, bands);
    }

    // ---------- 5. Rock scatter ----------

    /// <summary>
    /// Vanilla rocks within <paramref name="radius"/> m of every rock/cliff object and inside the rock surfaces, about one per
    /// <paramref name="areaPerRock"/> m² (a 10 m cell grid, one dart per cell with that probability, so overlapping sources do not
    /// add up), 70 % Rock01–10, 25 % Rock11–30, 5 % Boulder01–08, ≥ 6 m from each other and from a network course's edge, none in
    /// CS1 water or outside the map. Deterministic for a seed.
    /// </summary>
    public static (List<PlacedObject> Objects, Dictionary<string, int> Rejected) Scatter(IEnumerable<(double X, double Z)> rocks, IEnumerable<List<(double X, double Z)>> polygons,
        IEnumerable<OutEdge> courses, Func<double, double, bool>? wet, double half, ulong seed, double radius = 60, double areaPerRock = 500, double spacing = 6)
    {
        const double cell = 10;
        int n = (int)Math.Ceiling(2 * half / cell);
        var mask = new bool[n * n];
        int Ix(double v) => Math.Clamp((int)Math.Floor((v + half) / cell), 0, n - 1);
        foreach (var (x, z) in rocks)
            for (int i = Ix(x - radius); i <= Ix(x + radius); i++)
                for (int j = Ix(z - radius); j <= Ix(z + radius); j++)
                {
                    double cx = -half + (i + 0.5) * cell, cz = -half + (j + 0.5) * cell;
                    if ((cx - x) * (cx - x) + (cz - z) * (cz - z) <= radius * radius) mask[j * n + i] = true;
                }
        foreach (var poly in polygons)
        {
            double x0 = poly.Min(p => p.X), x1 = poly.Max(p => p.X), z0 = poly.Min(p => p.Z), z1 = poly.Max(p => p.Z);
            for (int i = Ix(x0); i <= Ix(x1); i++)
                for (int j = Ix(z0); j <= Ix(z1); j++)
                    if (Surfaces.PointInPolygon(-half + (i + 0.5) * cell, -half + (j + 0.5) * cell, poly)) mask[j * n + i] = true;
        }
        // Network courses as 4 m samples with their half width, hashed on a 32 m grid.
        var roadHash = new Dictionary<(int, int), List<(double X, double Z, double R)>>();
        foreach (var e in courses)
        {
            var a = e.Curve.Eval(0); var b = e.Curve.Eval(1);
            int steps = Math.Max(2, (int)Math.Ceiling(Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z)) * 1.3 / 4));
            for (int k = 0; k <= steps; k++)
            {
                var p = e.Curve.Eval(k / (double)steps);
                var key = ((int)Math.Floor(p.X / 32), (int)Math.Floor(p.Z / 32));
                (roadHash.TryGetValue(key, out var l) ? l : roadHash[key] = new()).Add((p.X, p.Z, e.Info.HalfWidth + spacing));
            }
        }
        bool NearRoad(double x, double z)
        {
            int kx = (int)Math.Floor(x / 32), kz = (int)Math.Floor(z / 32);
            for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++)
                if (roadHash.TryGetValue((kx + dx, kz + dz), out var l))
                    foreach (var p in l) if ((p.X - x) * (p.X - x) + (p.Z - z) * (p.Z - z) < p.R * p.R) return true;
            return false;
        }
        var placed = new Dictionary<(int, int), List<(double X, double Z)>>();
        bool Crowded(double x, double z)
        {
            int kx = (int)Math.Floor(x / spacing), kz = (int)Math.Floor(z / spacing);
            for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++)
                if (placed.TryGetValue((kx + dx, kz + dz), out var l))
                    foreach (var p in l) if ((p.X - x) * (p.X - x) + (p.Z - z) * (p.Z - z) < spacing * spacing) return true;
            return false;
        }
        var res = new List<PlacedObject>();
        var rejected = new Dictionary<string, int> { ["road"] = 0, ["spacing"] = 0, ["water"] = 0, ["border"] = 0 };
        var rng = new Rng(seed);
        double p0 = cell * cell / areaPerRock;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                if (!mask[j * n + i]) continue;
                double u = rng.NextDouble(), ox = rng.NextDouble(), oz = rng.NextDouble(), kind = rng.NextDouble(), pick = rng.NextDouble(), yaw = rng.NextDouble();
                if (u >= p0) continue;
                double x = -half + (i + ox) * cell, z = -half + (j + oz) * cell;
                if (Math.Abs(x) > half - 2 || Math.Abs(z) > half - 2) { rejected["border"]++; continue; }
                if (wet != null && wet(x, z)) { rejected["water"]++; continue; }
                if (NearRoad(x, z)) { rejected["road"]++; continue; }
                if (Crowded(x, z)) { rejected["spacing"]++; continue; }
                var set = kind < 0.70 ? SmallRocks : kind < 0.95 ? MediumRocks : Boulders;
                string prefab = set[Math.Min(set.Length - 1, (int)(pick * set.Length))];
                var key = ((int)Math.Floor(x / spacing), (int)Math.Floor(z / spacing));
                (placed.TryGetValue(key, out var l) ? l : placed[key] = new()).Add((x, z));
                res.Add(new PlacedObject($"scatter{res.Count + 1}", prefab, "prop", Math.Round(x, 2), Math.Round(z, 2), null, Math.Round(yaw * 360, 1), "rock scatter", "scatter"));
            }
        return (res, rejected);
    }

    public static bool IsRockLike(string prefab) =>
        System.Text.RegularExpressions.Regex.IsMatch(prefab, "(?i)rock|cliff|boulder|stone");

    public static string Counts(IEnumerable<PlacedObject> objs) =>
        string.Join(", ", objs.GroupBy(o => o.Prefab).OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key} {g.Count()}"));
}
