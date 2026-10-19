using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetConverter;

/// <summary>SplitMix64: small, fast and identical on every runtime, so a seed always gives the same output.</summary>
public struct Rng(ulong seed)
{
    ulong state = seed;
    public ulong Next()
    {
        ulong z = state += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
    public double NextDouble() => (Next() >> 11) * (1.0 / (1UL << 53));
    public int Next(int n) => (int)(NextDouble() * n);
    public static ulong Mix(ulong a, ulong b) => new Rng(a * 0x9E3779B97F4A7C15UL ^ b).Next();
}

public sealed record WeightedTarget(string Prefab, double Weight, string Extra, string Note)
{
    public bool IsSkip => Prefab == "SKIP";
    public bool IsCrop => Prefab == "CROP";
    public bool IsManual => Prefab == "MANUAL";
}

/// <summary>
/// regex → weighted targets, from tree-mapping.tsv / object-mapping.tsv. Rows with the same regex text form one group;
/// the first group (in file order) whose regex matches wins.
/// </summary>
public sealed class TargetMapping
{
    public List<(Regex Pattern, string Key, List<WeightedTarget> Targets)> Groups { get; } = new();

    public static TargetMapping Load(string path) => Parse(File.ReadAllLines(path));

    public static TargetMapping Parse(IEnumerable<string> lines)
    {
        var m = new TargetMapping();
        int n = 0;
        foreach (var raw in lines)
        {
            n++;
            if (string.IsNullOrWhiteSpace(raw) || raw.TrimStart().StartsWith('#')) continue;
            var c = raw.Split('\t');
            if (c.Length < 3) throw new FormatException($"line {n}: need regex<TAB>target<TAB>weight");
            var t = new WeightedTarget(c[1].Trim(), double.Parse(c[2], CultureInfo.InvariantCulture),
                c.Length > 3 ? c[3].Trim() : "", c.Length > 4 ? c[4].Trim() : "");
            var g = m.Groups.FirstOrDefault(x => x.Key == c[0]);
            if (g.Key == null) m.Groups.Add((new Regex(c[0], RegexOptions.CultureInvariant), c[0], new List<WeightedTarget> { t }));
            else g.Targets.Add(t);
        }
        return m;
    }

    public List<WeightedTarget>? Resolve(string cs1) => Groups.FirstOrDefault(g => g.Pattern.IsMatch(cs1)).Targets;

    public static WeightedTarget Pick(List<WeightedTarget> targets, ref Rng rng)
    {
        double total = targets.Sum(t => t.Weight), r = rng.NextDouble() * total;
        foreach (var t in targets) { r -= t.Weight; if (r < 0) return t; }
        return targets[^1];
    }
}

static class Json
{
    public static JsonWriterOptions Indented => new() { Indented = true };
    public static double R2(double v) => Math.Round(v, 2);
    public static string Short(string prefab) => prefab.Contains('.') && char.IsDigit(prefab[0]) ? prefab[(prefab.IndexOf('.') + 1)..] : prefab;
}

// ======================================================================================================= trees

/// <param name="Y">CS1 height, only for FixedHeight trees (snapped onto rocks or bridges).</param>
public sealed record TreeObject(string Prefab, double X, double Z, string Age, string Cs1, double? Y = null);

public sealed class TreeResult
{
    public string Source { get; set; } = "";
    public List<TreeObject> Objects { get; } = new();
    public Dictionary<string, int> Cs1Total { get; } = new();
    public Dictionary<string, int> Cs2Total { get; } = new();
    public Dictionary<string, int> Dropped { get; } = new();
    public Dictionary<string, string> Decision { get; } = new();
    public int Outside, SpacingRelaxed, Unassigned;
    public Dictionary<string, int> Masked { get; set; } = new();
    public int FixedHeight;
    public Dictionary<string, int> ScaledToAge { get; } = new();
    /// <summary>Trees removed by Mask, per rule (kept so a rule can be tried as an experiment file).</summary>
    public Dictionary<string, List<TreeObject>> Removed { get; } = new();
    /// <summary>CROP trees per 64 m raster cell (index = row·size + col, row 0 south), for the synthetic fertility layer.</summary>
    public Dictionary<int, int> CropCells { get; } = new();
    public int CropGridSize = 270;
    public double CropCellSize = 64;
}

public static class Trees
{
    static readonly (string Age, double P)[] Default = [("seedling", 0.05), ("sapling", 0.15), ("adult", 0.60), ("elderly", 0.20)];
    static readonly (string Age, double P)[] Old = [("adult", 0.30), ("elderly", 0.70)];
    static readonly (string Age, double P)[] Young = [("sapling", 0.50), ("adult", 0.50)];

    static string Age(string profile, ref Rng rng)
    {
        var table = profile switch { "old" => Old, "young" => Young, _ => Default };
        double r = rng.NextDouble();
        foreach (var (age, p) in table) { r -= p; if (r < 0) return age; }
        return table[^1].Age;
    }

    /// <summary>Per-tree points (dakota-export/cs1/trees/v1: {prefabs:[...], points:[x, z, prefabIndex, ...]}), exact positions and species.</summary>
    /// <summary>
    /// Since trees/v2 the point file also has y, flags and Tree Control scale per tree. FixedHeight trees (flag 0x20) keep
    /// their y; scale picks the CS2 age (CS2 sizes trees by age): &lt; 0.6 seedling, &lt; 0.85 sapling, ≤ 1.2 adult, else elderly.
    /// </summary>
    public static string AgeFromScale(double s) => s < 0.6 ? "seedling" : s < 0.85 ? "sapling" : s <= 1.2 ? "adult" : "elderly";

    public static TreeResult FromPoints(string path, TargetMapping map, double half, ulong seed)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        var prefabs = root.GetProperty("prefabs").EnumerateArray().Select(p => p.GetString()!).ToArray();
        var pts = root.GetProperty("points").EnumerateArray().Select(p => p.GetDouble()).ToArray();
        double[]? Arr(string k) => root.TryGetProperty(k, out var a) && a.ValueKind == JsonValueKind.Array
            ? a.EnumerateArray().Select(v => v.ValueKind == JsonValueKind.Number ? v.GetDouble() : double.NaN).ToArray() : null;
        var ys = Arr("y"); var flags = Arr("flags"); var scales = Arr("scale");
        var res = new TreeResult { Source = $"points ({Path.GetFileName(path)}{(ys != null ? ", trees/v2: y, flags, scale" : "")})" };
        var rng = new Rng(seed);
        for (int i = 0, k = 0; i + 2 < pts.Length; i += 3, k++)
        {
            double? y = flags != null && ys != null && ((long)flags[k] & 0x20) != 0 ? ys[k] : null;
            string? age = scales != null && !double.IsNaN(scales[k]) && Math.Abs(scales[k] - 1) > 0.01 ? AgeFromScale(scales[k]) : null;
            if (y != null) res.FixedHeight++;
            if (age != null) res.ScaledToAge[age] = res.ScaledToAge.GetValueOrDefault(age) + 1;
            Emit(res, map, prefabs[(int)pts[i + 2]], pts[i], pts[i + 1], half, ref rng, null, y, age);
        }
        return res;
    }

    /// <summary>
    /// From the dump's rasters: the 1081 grid (≈16 m, total count per cell) gives density, the 270 grid (64 m, count per species)
    /// gives the species mix. Each 16 m cell's trees are scattered with jitter and ≈3 m minimum spacing (relaxed where a cell is
    /// too dense); then each 64 m cell's points get that cell's species multiset, shuffled. Deterministic for a seed.
    /// </summary>
    public static TreeResult FromRasters(JsonElement trees, TargetMapping map, double half, ulong seed, double spacing = 3)
    {
        var res = new TreeResult { Source = "rasters (trees.total 1081 grid + trees.byPrefab 270 grid)" };
        var total = trees.GetProperty("total");
        int tSize = total.GetProperty("size").GetInt32();
        double tCell = total.GetProperty("cellSize").GetDouble();
        int pSize = trees.GetProperty("raster").GetProperty("size").GetInt32();
        double pCell = trees.GetProperty("raster").GetProperty("cellSize").GetDouble();
        const double origin = -8640;
        res.CropGridSize = pSize; res.CropCellSize = pCell;

        // Species multiset per 64 m cell.
        var species = new Dictionary<int, List<(string Prefab, int Count)>>();
        foreach (var p in trees.GetProperty("byPrefab").EnumerateObject())
        {
            var cells = p.Value.GetProperty("cells").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            for (int i = 0; i + 1 < cells.Length; i += 2)
            {
                if (!species.TryGetValue(cells[i], out var l)) species[cells[i]] = l = new();
                l.Add((p.Name, cells[i + 1]));
            }
        }

        // Scatter positions per 16 m cell, then bucket them by 64 m cell.
        var occupied = new Dictionary<(long, long), List<(double X, double Z)>>();
        bool Free(double x, double z)
        {
            long cx = (long)Math.Floor(x / spacing), cz = (long)Math.Floor(z / spacing);
            for (long i = -1; i <= 1; i++)
                for (long j = -1; j <= 1; j++)
                    if (occupied.TryGetValue((cx + i, cz + j), out var l))
                        foreach (var (px, pz) in l)
                            if ((px - x) * (px - x) + (pz - z) * (pz - z) < spacing * spacing) return false;
            return true;
        }
        void Occupy(double x, double z)
        {
            var key = ((long)Math.Floor(x / spacing), (long)Math.Floor(z / spacing));
            if (!occupied.TryGetValue(key, out var l)) occupied[key] = l = new();
            l.Add((x, z));
        }

        var byBig = new Dictionary<int, List<(double X, double Z)>>();
        var cellsT = total.GetProperty("cells").EnumerateArray().Select(x => x.GetInt32()).ToArray();
        for (int i = 0; i + 1 < cellsT.Length; i += 2)
        {
            int idx = cellsT[i], count = cellsT[i + 1], row = idx / tSize, col = idx % tSize;
            var rng = new Rng(Rng.Mix(seed, (ulong)idx));
            double x0 = origin + col * tCell, z0 = origin + row * tCell;
            for (int k = 0; k < count; k++)
            {
                double x = 0, z = 0; bool ok = false;
                for (int attempt = 0; attempt < 12 && !ok; attempt++)
                {
                    x = x0 + rng.NextDouble() * tCell; z = z0 + rng.NextDouble() * tCell;
                    ok = Free(x, z);
                }
                if (!ok) res.SpacingRelaxed++;
                Occupy(x, z);
                int big = (int)Math.Clamp(Math.Floor((z - origin) / pCell), 0, pSize - 1) * pSize + (int)Math.Clamp(Math.Floor((x - origin) / pCell), 0, pSize - 1);
                if (!byBig.TryGetValue(big, out var l)) byBig[big] = l = new();
                l.Add((x, z));
            }
        }

        // Global mix, for 64 m cells whose species list is empty (the two rasters bin slightly differently).
        var global = species.Values.SelectMany(l => l).GroupBy(s => s.Prefab).Select(g => (g.Key, g.Sum(x => x.Count))).ToList();
        foreach (var (big, points) in byBig.OrderBy(kv => kv.Key))
        {
            var rng = new Rng(Rng.Mix(seed ^ 0xABCDEF, (ulong)big));
            var mix = species.TryGetValue(big, out var l) ? l : null;
            if (mix == null) res.Unassigned += points.Count;
            var bag = new List<string>();
            foreach (var (prefab, count) in mix ?? global) for (int k = 0; k < (mix == null ? 1 : count); k++) bag.Add(prefab);
            // Shuffle (Fisher–Yates), then hand out in order; cycle when the 16 m raster has more points than the 64 m one.
            for (int k = bag.Count - 1; k > 0; k--) { int j = rng.Next(k + 1); (bag[k], bag[j]) = (bag[j], bag[k]); }
            for (int k = 0; k < points.Count; k++)
            {
                string cs1 = mix == null ? PickGlobal(global, ref rng) : bag[k % bag.Count];
                Emit(res, map, cs1, points[k].X, points[k].Z, half, ref rng, big);
            }
        }
        return res;
    }

    static string PickGlobal(List<(string Key, int Count)> global, ref Rng rng)
    {
        double r = rng.NextDouble() * global.Sum(g => g.Count);
        foreach (var (k, c) in global) { r -= c; if (r < 0) return k; }
        return global[^1].Key;
    }

    static void Emit(TreeResult res, TargetMapping map, string cs1, double x, double z, double half, ref Rng rng, int? bigCell, double? y = null, string? age = null)
    {
        res.Cs1Total[cs1] = res.Cs1Total.GetValueOrDefault(cs1) + 1;
        var targets = map.Resolve(cs1);
        if (targets == null)
        {
            res.Dropped[cs1] = res.Dropped.GetValueOrDefault(cs1) + 1;
            res.Decision[cs1] = "UNMAPPED (dropped)";
            return;
        }
        res.Decision[cs1] = string.Join(", ", targets.Select(t => t.IsSkip || t.IsCrop ? t.Prefab : $"{t.Prefab} {t.Weight:0.##}"));
        var t = TargetMapping.Pick(targets, ref rng);
        if (t.IsCrop)
        {
            res.Dropped[cs1] = res.Dropped.GetValueOrDefault(cs1) + 1;
            int cell = bigCell ?? (int)Math.Floor((z + 8640) / res.CropCellSize) * res.CropGridSize + (int)Math.Floor((x + 8640) / res.CropCellSize);
            res.CropCells[cell] = res.CropCells.GetValueOrDefault(cell) + 1;
            return;
        }
        if (t.IsSkip) { res.Dropped[cs1] = res.Dropped.GetValueOrDefault(cs1) + 1; return; }
        if (Math.Abs(x) > half || Math.Abs(z) > half) { res.Outside++; return; }
        res.Cs2Total[t.Prefab] = res.Cs2Total.GetValueOrDefault(t.Prefab) + 1;
        string ageNow = Age(t.Extra, ref rng);   // always draw, so the random sequence does not depend on the scale data
        res.Objects.Add(new TreeObject(t.Prefab, x, z, age ?? ageNow, cs1, y));
    }

    /// <summary>Drops trees matching the first rule that applies; returns the count per rule (audit finding 2).</summary>
    public static Dictionary<string, int> Mask(TreeResult res, params (string Reason, Func<double, double, bool> Drop)[] rules)
    {
        var counts = rules.ToDictionary(r => r.Reason, _ => 0);
        res.Removed.Clear();
        foreach (var (reason, _) in rules) res.Removed[reason] = new List<TreeObject>();
        res.Objects.RemoveAll(t =>
        {
            foreach (var (reason, drop) in rules)
                if (drop(t.X, t.Z))
                {
                    counts[reason]++;
                    res.Removed[reason].Add(t);
                    res.Cs2Total[t.Prefab]--;
                    return true;
                }
            return false;
        });
        res.Masked = counts;
        return counts;
    }

    /// <summary>Writes import-trees-NN.json chunks of at most chunkSize objects; returns the file names.</summary>
    public static List<string> WriteChunks(string dir, TreeResult res, int chunkSize) => WriteChunks(dir, res.Objects, chunkSize, "import-trees", "trees");

    public static List<string> WriteChunks(string dir, List<TreeObject> objects, int chunkSize, string filePrefix, string importName, double heightFactor = 1)
    {
        Directory.CreateDirectory(dir);
        foreach (var old in Directory.GetFiles(dir, filePrefix + "-*.json").Where(f => System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), @"-\d\d\.json$"))) File.Delete(old);
        var files = new List<string>();
        for (int c = 0, start = 0; start < objects.Count; c++, start += chunkSize)
        {
            var name = $"{filePrefix}-{c + 1:00}.json";
            files.Add(name);
            using var fs = File.Create(Path.Combine(dir, name));
            using var w = new Utf8JsonWriter(fs);
            w.WriteStartObject();
            w.WriteString("name", $"{importName}-{c + 1:00}");
            w.WriteStartObject("options");
            w.WriteBoolean("dryRun", false); w.WriteNumber("batchSize", 500); w.WriteBoolean("bypassValidation", true); w.WriteString("report", "compact");
            w.WriteEndObject();
            w.WriteStartArray("objects");
            for (int i = start; i < Math.Min(start + chunkSize, objects.Count); i++)
            {
                var t = objects[i];
                w.WriteStartObject();
                w.WriteString("id", $"t{i + 1}");
                w.WriteString("prefab", t.Prefab);
                w.WriteNumber("x", Json.R2(t.X)); w.WriteNumber("z", Json.R2(t.Z));
                if (t.Y is double ty) w.WriteNumber("y", Json.R2(ty * heightFactor));
                w.WriteString("kind", "tree");
                w.WriteString("age", t.Age);
                w.WriteEndObject();
            }
            w.WriteEndArray();
            w.WriteEndObject();
        }
        return files;
    }

    public static string Readme(TreeResult res, List<string> files, int chunkSize)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Trees for POST /import/objects").AppendLine();
        sb.AppendLine($"Source: {res.Source}. {res.Objects.Count:N0} trees in {files.Count} files of ≤ {chunkSize:N0} ({string.Join(", ", files)}).");
        sb.AppendLine("No y: the game samples terrain, so the same files serve the 2× (out/) and the 1× (out/x1/) map.").AppendLine();
        sb.AppendLine($"- Outside ±7168 m (dropped): {res.Outside:N0}");
        sb.AppendLine($"- Points where ≈3 m spacing could not be kept (dense cells, placed anyway): {res.SpacingRelaxed:N0}");
        sb.AppendLine($"- Points in 64 m cells without a species list (got the global mix): {res.Unassigned:N0}");
        sb.AppendLine($"- Crop cells (Corn Stalk, for fertility): {res.CropCells.Count:N0} cells of {res.CropCellSize} m");
        if (res.FixedHeight > 0) sb.AppendLine($"- FixedHeight trees (on rocks or bridges, with y): {res.FixedHeight:N0} in the point file (before clipping and masking), written per scale as import-trees-fixed-NN.json in out/ and out/x1/");
        if (res.ScaledToAge.Count > 0) sb.AppendLine($"- Tree Control scale → age (trees with scale ≠ 1; the rest use the mapping age profile): {string.Join(", ", res.ScaledToAge.Select(a => $"{a.Key} {a.Value:N0}"))} (scale < 0.6 seedling, < 0.85 sapling, ≤ 1.2 adult, else elderly)");
        foreach (var (reason, n) in res.Masked) sb.AppendLine($"- Masked, {reason}: {n:N0}");
        sb.AppendLine();
        sb.AppendLine("## Per CS2 prefab").AppendLine().AppendLine("| CS2 prefab | trees |").AppendLine("|---|---:|");
        foreach (var (k, v) in res.Cs2Total.OrderByDescending(kv => kv.Value)) sb.AppendLine($"| {k} | {v:N0} |");
        sb.AppendLine().AppendLine("## Per CS1 species (decision from tree-mapping.tsv)").AppendLine();
        sb.AppendLine("| CS1 prefab | CS1 trees | dropped (SKIP / CROP / outside mapping) | → CS2 |").AppendLine("|---|---:|---:|---|");
        foreach (var (k, v) in res.Cs1Total.OrderByDescending(kv => kv.Value))
            sb.AppendLine($"| {Json.Short(k)} | {v:N0} | {res.Dropped.GetValueOrDefault(k):N0} | {res.Decision.GetValueOrDefault(k)} |");
        return sb.ToString();
    }
}

// =================================================================================================== objects

public sealed record PlacedObject(string Id, string Prefab, string Kind, double X, double Z, double? Y, double Rotation, string Cs1, string Source);
public sealed record ManualObject(string Cs1, string Source, double X, double Y, double Z, double Rotation, string Note);

public sealed class ObjectResult
{
    public List<PlacedObject> Objects { get; } = new();
    public List<ManualObject> Manual { get; } = new();
    public Dictionary<string, (int Count, string Decision)> PerCs1 { get; } = new();
    public int Total, Targeted, Skipped, ManualCount, Unmapped, WithY, Outside;
    /// <summary>y under the old CS1-terrain rule (> 2 m off the CS1 terrain) but not under the CS2-terrain rule, and the reverse.</summary>
    public int LostY, GainedY;
}

public static class Objects
{
    /// <summary>
    /// CS2 yaw in degrees (about +y, 0 = +z, clockwise seen from above) from a CS1 angle in radians.
    /// CS1 draws buildings and props with Quaternion.AngleAxis(angle·180/π, Vector3.down), i.e. the angle turns the other way
    /// round the y axis, so yaw = −angle·180/π. UNVERIFIED until the bridge's asymmetric test confirms it.
    /// </summary>
    public static double Yaw(double cs1Radians)
    {
        double deg = -cs1Radians * 180 / Math.PI % 360;
        if (deg < 0) deg += 360;
        return Math.Round(deg, 2) % 360;
    }

    /// <summary>
    /// Buildings (except outside connections) and prop points from the dump. y (CS1 y × heightFactor) is written only where the object
    /// is clearly on a structure: with the CS2 playable heightmap (true metres) more than 3 m above it; without it, more than 2 m off
    /// the CS1 terrain. Buildings never get y (the game levels their lot; CS1 y put the substation below ground). Otherwise the game samples terrain.
    /// </summary>
    public static ObjectResult Build(JsonElement root, TargetMapping map, Func<double, double, double>? cs1Terrain, double heightFactor, double half, ulong seed,
        Func<double, double, double>? cs2Terrain = null)
    {
        var res = new ObjectResult();
        var rng = new Rng(seed);
        void Take(string source, int id, string prefab, JsonElement pos, double angle, string? fallbackKind)
        {
            res.Total++;
            var p = pos.EnumerateArray().Select(v => v.GetDouble()).ToArray();
            double yaw = Yaw(angle);
            var targets = map.Resolve(prefab);
            if (targets == null)
            {
                res.Unmapped++;
                res.PerCs1[prefab] = (res.PerCs1.GetValueOrDefault(prefab).Count + 1, "UNMAPPED");
                return;
            }
            var t = TargetMapping.Pick(targets, ref rng);
            res.PerCs1[prefab] = (res.PerCs1.GetValueOrDefault(prefab).Count + 1,
                t.IsSkip ? $"SKIP: {t.Note}" : t.IsManual ? $"MANUAL: {t.Note}" : string.Join(", ", targets.Select(x => x.Prefab).Distinct()));
            if (t.IsSkip) { res.Skipped++; return; }
            if (t.IsManual)
            {
                res.ManualCount++;
                res.Manual.Add(new ManualObject(prefab, $"{source} {id}", p[0], p[1] * heightFactor, p[2], yaw, t.Note));
                return;
            }
            if (Math.Abs(p[0]) > half || Math.Abs(p[2]) > half) { res.Outside++; return; }
            double? y = null;
            string kind = string.IsNullOrEmpty(t.Extra) ? fallbackKind ?? "prop" : t.Extra;
            bool old = cs1Terrain != null && Math.Abs(p[1] - cs1Terrain(p[0], p[2])) > 2;
            bool keep = kind != "building" && (cs2Terrain != null ? p[1] - cs2Terrain(p[0], p[2]) > 3 : old);
            if (old && !keep) res.LostY++;
            if (keep && !old) res.GainedY++;
            if (keep) { y = Json.R2(p[1] * heightFactor); res.WithY++; }
            res.Targeted++;
            res.Objects.Add(new PlacedObject($"{source[0]}{id}", t.Prefab, kind,
                p[0], p[2], y, yaw, prefab, $"{source} {id}"));
        }

        foreach (var b in root.GetProperty("buildings").EnumerateArray())
        {
            if (b.TryGetProperty("outsideConnection", out var oc) && oc.GetBoolean()) continue;
            Take("building", b.GetProperty("id").GetInt32(), b.GetProperty("prefab").GetString()!, b.GetProperty("position"),
                b.TryGetProperty("angle", out var a) ? a.GetDouble() : 0, "building");
        }
        if (root.TryGetProperty("props", out var props) && props.TryGetProperty("points", out var pts))
            foreach (var p in pts.EnumerateArray())
                Take("prop", p.GetProperty("id").GetInt32(), p.GetProperty("prefab").GetString()!, p.GetProperty("position"),
                    p.TryGetProperty("angle", out var a) ? a.GetDouble() : 0, "prop");
        return res;
    }

    public static void Write(string path, string name, IEnumerable<PlacedObject> objects)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", name);
        w.WriteString("_comment", "rotation = yaw in degrees about +y, 0 = +z, clockwise from above, from CS1 m_angle as −angle·180/π (UNVERIFIED sign). y only where the object is clearly on a structure: > 3 m above the CS2 playable heightmap (else > 2 m off the CS1 terrain); never for buildings (CS1 y × height factor).");
        w.WriteStartObject("options");
        w.WriteBoolean("dryRun", false); w.WriteNumber("batchSize", 500); w.WriteBoolean("bypassValidation", true); w.WriteString("report", "compact");
        w.WriteEndObject();
        w.WriteStartArray("objects");
        foreach (var o in objects)
        {
            w.WriteStartObject();
            w.WriteString("id", o.Id);
            w.WriteString("prefab", o.Prefab);
            w.WriteNumber("x", Json.R2(o.X)); w.WriteNumber("z", Json.R2(o.Z));
            if (o.Y is double y) w.WriteNumber("y", y);
            w.WriteNumber("rotation", o.Rotation);
            w.WriteString("kind", o.Kind);
            if (o.Kind is "plant" or "tree") w.WriteString("age", "adult");
            w.WriteString("cs1", Json.Short(o.Cs1));
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }
}

// =============================================================================================== connections

public sealed record ConnectionObject(string Id, string Prefab, string Net, double X, double Z, double Rotation, string? Node, string? Layer, int? Cs1Building, string Note);

public sealed class ConnectionResult
{
    public List<ConnectionObject> Objects { get; } = new();
    /// <summary>Every border node: (layer, node, prefab, x, z, matched CS1 building id or null, distance along the border).</summary>
    public List<(string Layer, OutNode Node, string Prefab, int? Cs1, double Distance)> BorderNodes { get; } = new();
    /// <summary>Every CS1 connection: (building, projected point, border nodes matched).</summary>
    public List<(OutsideBuilding B, double Px, double Pz, List<string> Nodes)> Cs1 { get; } = new();
}

public static class Connections
{
    // CS2 names from Assets.NAME[...] in Locale.cok (2026-10-19); reconcile with cs2mcp/PREFAB_NAMES.md when it exists.
    public const string RoadOneway = "Road Outside Connection - Oneway", RoadTwoway = "Road Outside Connection - Twoway";
    public const string TrainOneway = "Train Outside Connection - Oneway", TrainTwoway = "Train Outside Connection - Twoway";
    public const string Airplane = "Airplane Outside Connection", Ship = "Ship Outside Connection - Twoway";

    /// <summary>CS1 connection buildings sit at ±8640; the nearest CS2 border point is the position clamped to ±half.</summary>
    public static (double X, double Z) ProjectToBorder(double x, double z, double half)
    {
        double cx = Math.Clamp(x, -half, half), cz = Math.Clamp(z, -half, half);
        if (Math.Abs(cx) < half && Math.Abs(cz) < half)
        {
            // Inside: push to the nearest edge.
            if (half - Math.Abs(cx) < half - Math.Abs(cz)) cx = Math.CopySign(half, cx); else cz = Math.CopySign(half, cz);
        }
        return (cx, cz);
    }

    /// <summary>Yaw (0 = +z, clockwise from above) of a direction in x/z.</summary>
    public static double Heading(double dx, double dz)
    {
        double deg = Math.Atan2(dx, dz) * 180 / Math.PI;
        if (deg < 0) deg += 360;
        return Math.Round(deg, 2);
    }

    /// <summary>
    /// One outsideConnection object per border node of the imported network whose projected CS1 connection building (same
    /// transport) lies within matchRadius along the border; air and ship connections as objects at the projected CS1 position.
    /// </summary>
    public static ConnectionResult Build(ConvertResult r, double half, double matchRadius = 300)
    {
        var res = new ConnectionResult();
        var adj = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in r.Edges.Where(e => e.Map.Imported))
            foreach (var n in new[] { e.A, e.B })
            {
                if (!adj.TryGetValue(n, out var l)) adj[n] = l = new();
                l.Add(e);
            }
        var border = adj.Where(kv => kv.Key.Border).Select(kv => (Node: kv.Key, Edge: kv.Value[0])).ToList();
        var cs1 = r.Dump.OutsideConnections.Select(b => (B: b, P: ProjectToBorder(b.Position.X, b.Position.Z, half))).ToList();

        // Follow the CS1 network from the clipped segment outwards to the node flagged Outside; the connection building there
        // is the match (roads curve between ±7168 and ±8640, so the projected building can be far from the border crossing).
        var segsAt = new Dictionary<int, List<DumpSegment>>();
        foreach (var sg in r.Dump.Segments)
            foreach (var n in new[] { sg.StartNode, sg.EndNode })
            {
                if (!segsAt.TryGetValue(n, out var l)) segsAt[n] = l = new();
                l.Add(sg);
            }
        var segById = r.Dump.Segments.ToDictionary(sg => sg.Id);
        DumpNode? OutsideNode(int segId)
        {
            if (!segById.TryGetValue(segId, out var sg)) return null;
            var start = Clipper.Outside(r.Dump.Nodes[sg.StartNode].Position, half) > 0 ? sg.StartNode : sg.EndNode;
            var seen = new HashSet<int> { start };
            var queue = new Queue<int>(); queue.Enqueue(start);
            while (queue.Count > 0 && seen.Count < 400)
            {
                var n = queue.Dequeue();
                if (r.Dump.Nodes.TryGetValue(n, out var dn) && dn.Has("Outside")) return dn;
                foreach (var x in segsAt.GetValueOrDefault(n) ?? [])
                {
                    var other = x.StartNode == n ? x.EndNode : x.StartNode;
                    if (x.Id == segId || !seen.Add(other)) continue;
                    if (r.Dump.Nodes.TryGetValue(other, out var on) && Clipper.Outside(on.Position, half) <= 0) continue; // stay outside the CS2 square
                    queue.Enqueue(other);
                }
            }
            return null;
        }

        foreach (var (node, edge) in border)
        {
            string net = edge.Layer == Layers.Rail ? "rail" : "road";
            var same = cs1.Where(c => (c.B.Kind == "train" ? "rail" : c.B.Kind == "road" ? "road" : "") == net).ToList();
            var match = same
                .Select(c => (c, d: Math.Sqrt((c.P.X - node.Position.X) * (c.P.X - node.Position.X) + (c.P.Z - node.Position.Z) * (c.P.Z - node.Position.Z))))
                .OrderBy(x => x.d).FirstOrDefault();
            bool ok = match.c.B != null && match.d <= matchRadius;
            var m = System.Text.RegularExpressions.Regex.Match(node.Id, @"^c(\d+)_");
            if (m.Success && OutsideNode(int.Parse(m.Groups[1].Value)) is { } outside)
            {
                var traced = same.Select(c => (c, d: c.B.Position.DistXZ(outside.Position))).OrderBy(x => x.d).FirstOrDefault();
                if (traced.c.B != null && traced.d <= 150)
                {
                    double dx = traced.c.P.X - node.Position.X, dz = traced.c.P.Z - node.Position.Z;
                    match = (traced.c, Math.Sqrt(dx * dx + dz * dz));
                    ok = true;
                }
            }
            string prefab = net == "rail"
                ? (edge.Map.Cs2Prefab == "Oneway Train Track" ? TrainOneway : TrainTwoway)
                : (edge.Info.OneWay ? RoadOneway : RoadTwoway);
            res.BorderNodes.Add((edge.Layer, node, edge.Map.Cs2Prefab!, ok ? match.c.B.Id : null, ok ? match.d : double.NaN));
            if (!ok) continue;
            // Outward heading: from the course's inner end towards the border node.
            var inner = edge.A == node ? edge.Curve.B : edge.Curve.C;
            res.Objects.Add(new ConnectionObject($"oc_{node.Id}", prefab, net, node.Position.X, node.Position.Z,
                Heading(node.Position.X - inner.X, node.Position.Z - inner.Z), node.Id, edge.Layer, match.c.B.Id,
                $"{edge.Map.Cs2Prefab} border node; CS1 {match.c.B.Prefab} {match.c.B.Id} ({match.d:F0} m from its border projection)"));
        }

        foreach (var (b, p) in cs1)
        {
            var nodes = res.BorderNodes.Where(x => x.Cs1 == b.Id).Select(x => x.Node.Id).ToList();
            res.Cs1.Add((b, p.X, p.Z, nodes));
            if (b.Kind == "ship")
            {
                // Audit finding 3: the marker must sit on the seaway, so use the seaway border end nearest to the CS1 connection.
                var seaEnds = r.NodesOf(r.Layer(Layers.ShipPaths)).Where(n => n.Border).ToList();
                if (seaEnds.Count > 0)
                {
                    var n = seaEnds.OrderBy(n => (n.Position.X - p.X) * (n.Position.X - p.X) + (n.Position.Z - p.Z) * (n.Position.Z - p.Z)).First();
                    var e = r.Layer(Layers.ShipPaths).First(e => e.A == n || e.B == n);
                    var inner = e.A == n ? e.Curve.B : e.Curve.C;
                    res.Objects.Add(new ConnectionObject($"oc_ship_{b.Id}", Ship, "ship", n.Position.X, n.Position.Z,
                        Heading(n.Position.X - inner.X, n.Position.Z - inner.Z), n.Id, Layers.ShipPaths, b.Id,
                        $"on the seaway border end {n.Id}, facing outward (CS1 {b.Prefab} at ({b.Position.X:F0}, {b.Position.Z:F0})); drop this object if the seaway spawns its own marker (NOTES.md item 6)"));
                    continue;
                }
            }
            if (b.Kind is "plane" or "ship")
            {
                // Inward heading from the projected point towards the map centre; reversed to point outward.
                res.Objects.Add(new ConnectionObject($"oc_{b.Kind}_{b.Id}", b.Kind == "plane" ? Airplane : Ship, b.Kind == "plane" ? "air" : "ship",
                    p.X, p.Z, Heading(p.X, p.Z), null, null, b.Id, $"CS1 {b.Prefab} at ({b.Position.X:F0}, {b.Position.Z:F0}) clamped to the border; may need hand placement"));
            }
        }
        return res;
    }

    public static void Write(string path, ConnectionResult res)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", "outside-connections");
        w.WriteString("_comment", "kind outsideConnection; road/rail objects sit on border nodes of import-all.json (node = its id); rotation = outward heading, yaw degrees, 0 = +z, clockwise from above. Prefab names from Locale.cok, to reconcile with cs2mcp/PREFAB_NAMES.md.");
        w.WriteStartObject("options");
        w.WriteBoolean("dryRun", false); w.WriteNumber("batchSize", 50); w.WriteBoolean("bypassValidation", true); w.WriteString("report", "compact");
        w.WriteEndObject();
        w.WriteStartArray("objects");
        foreach (var o in res.Objects)
        {
            w.WriteStartObject();
            w.WriteString("id", o.Id); w.WriteString("prefab", o.Prefab); w.WriteString("kind", "outsideConnection"); w.WriteString("net", o.Net);
            w.WriteNumber("x", Json.R2(o.X)); w.WriteNumber("z", Json.R2(o.Z)); w.WriteNumber("rotation", o.Rotation);
            if (o.Node != null) w.WriteString("node", o.Node);
            if (o.Cs1Building is int id) w.WriteNumber("cs1Building", id);
            w.WriteString("note", o.Note);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }

    public static string Check(ConnectionResult res)
    {
        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("# Outside connections check").AppendLine();
        sb.AppendLine("A border node is matched by following the CS1 network from its clipped segment outwards to the node flagged Outside and taking the CS1 connection building of the same transport within 150 m of it; failing that, the building projected onto the CS2 border (±7168 m) within 300 m. distance = border node to the building's border projection.").AppendLine();
        sb.AppendLine("## Border nodes of the imported network").AppendLine();
        sb.AppendLine("| layer | node | prefab | x | z | CS1 connection | distance m |").AppendLine("|---|---|---|---:|---:|---|---:|");
        foreach (var b in res.BorderNodes.OrderBy(b => b.Layer).ThenBy(b => b.Node.Id))
            sb.AppendLine(string.Create(ci, $"| {b.Layer} | {b.Node.Id} | {b.Prefab} | {b.Node.Position.X:F0} | {b.Node.Position.Z:F0} | {(b.Cs1 is int id ? id.ToString(ci) : "none (no connection object)")} | {(double.IsNaN(b.Distance) ? "-" : b.Distance.ToString("F0", ci))} |"));
        sb.AppendLine().AppendLine("## CS1 connections").AppendLine();
        sb.AppendLine("| id | prefab | type | in/out | CS1 x | CS1 z | border x | border z | border nodes |").AppendLine("|---|---|---|---|---:|---:|---:|---:|---|");
        foreach (var c in res.Cs1.OrderBy(c => c.B.Kind).ThenBy(c => c.B.Id))
            sb.AppendLine(string.Create(ci, $"| {c.B.Id} | {c.B.Prefab} | {c.B.Kind} | {(c.B.Incoming ? "in" : "")}{(c.B.Outgoing ? "/out" : "")} | {c.B.Position.X:F0} | {c.B.Position.Z:F0} | {c.Px:F0} | {c.Pz:F0} | {(c.Nodes.Count > 0 ? string.Join(", ", c.Nodes) : c.B.Kind is "plane" or "ship" ? "object at the border point" : "NONE")} |"));
        return sb.ToString();
    }
}

// ================================================================================================= resources

public static class Resources
{
    public const int GridSize = 512;
    public const double CellSize = 33.75, Origin = -8640;

    static void WriteGrid(Utf8JsonWriter w)
    {
        w.WriteStartObject("grid");
        w.WriteNumber("size", GridSize); w.WriteNumber("cellSize", CellSize);
        w.WriteNumber("originX", Origin); w.WriteNumber("originZ", Origin); w.WriteString("rowAxis", "+z");
        w.WriteEndObject();
    }

    /// <summary>
    /// Pass-through of the v2 export's naturalResources to the /import/resources body: fertility, ore, oil; groundwater from the
    /// CS1 water layer only when it has non-zero cells; forest omitted (the game ignores it). Null (all-zero) layers are skipped.
    /// </summary>
    /// <param name="field">Optional field mask (x, z) → true: fertility there is raised to at least FieldFertility.</param>
    public static bool WriteFromDump(string path, JsonElement root, Func<double, double, bool>? field = null)
    {
        if (!root.TryGetProperty("naturalResources", out var nr) || nr.ValueKind != JsonValueKind.Object
            || !nr.TryGetProperty("layers", out var ly) || ly.ValueKind != JsonValueKind.Object) return false;
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs);
        w.WriteStartObject();
        w.WriteString("name", "resources");
        w.WriteStartObject("options"); w.WriteBoolean("dryRun", false); w.WriteString("mode", "replace"); w.WriteEndObject();
        if (nr.TryGetProperty("grid", out var g)) { w.WritePropertyName("grid"); g.WriteTo(w); } else WriteGrid(w);
        w.WriteStartObject("layers");
        var layers = nr.GetProperty("layers");
        foreach (var (cs1, cs2) in new[] { ("fertility", "fertility"), ("ore", "ore"), ("oil", "oil"), ("water", "groundwater") })
        {
            bool has = layers.TryGetProperty(cs1, out var layer) && layer.ValueKind == JsonValueKind.Array && layer.EnumerateArray().Any(v => v.GetInt32() != 0);
            if (cs1 == "fertility" && field != null)
            {
                // Field footprints raise fertility (the layer may be absent: then it is built from the fields alone).
                var spec = GridSpec.FromJson(nr.TryGetProperty("grid", out var gg) ? gg : null, GridSize, CellSize);
                var values = has ? layer.EnumerateArray().Select(v => v.GetInt32()).ToArray() : new int[spec.Size * spec.Size];
                for (int i = 0; i < values.Length; i++) { var (x, z) = spec.Centre(i); if (field(x, z)) values[i] = Math.Max(values[i], FieldFertility); }
                w.WriteStartArray(cs2); foreach (var v in values) w.WriteNumberValue(v); w.WriteEndArray();
                continue;
            }
            if (has) { w.WritePropertyName(cs2); layer.WriteTo(w); }
        }
        w.WriteEndObject();
        w.WriteEndObject();
        return true;
    }

    /// <summary>
    /// SYNTHETIC fertility until the v2 export exists: Corn Stalk density per 64 m cell, resampled (nearest) to the 512 grid
    /// and scaled so the densest crop cell is 255. Only the fertility layer is written.
    /// </summary>
    public const int FieldFertility = 220;

    public static int[] SyntheticFertility(TreeResult trees, Func<double, double, bool>? field = null)
    {
        var f = new int[GridSize * GridSize];
        if (field != null)
            for (int i = 0; i < f.Length; i++)
                if (field(Origin + (i % GridSize + 0.5) * CellSize, Origin + (i / GridSize + 0.5) * CellSize)) f[i] = FieldFertility;
        if (trees.CropCells.Count == 0) return f;
        double max = trees.CropCells.Values.Max();
        for (int row = 0; row < GridSize; row++)
            for (int col = 0; col < GridSize; col++)
            {
                double x = Origin + (col + 0.5) * CellSize, z = Origin + (row + 0.5) * CellSize;
                int big = (int)Math.Floor((z - Origin) / trees.CropCellSize) * trees.CropGridSize + (int)Math.Floor((x - Origin) / trees.CropCellSize);
                if (trees.CropCells.TryGetValue(big, out var n)) f[row * GridSize + col] = Math.Max(f[row * GridSize + col], (int)Math.Round(255 * Math.Sqrt(n / max)));
            }
        return f;
    }

    public static void WriteSynthetic(string path, int[] fertility)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs);
        w.WriteStartObject();
        w.WriteString("name", "resources-SYNTHETIC-fertility-from-corn");
        w.WriteString("_comment", "SYNTHETIC: fertility from CS1 Corn Stalk density (sqrt-scaled, max cell = 255); no ore/oil/forest. Replace with import-resources.json once the v2 export has naturalResources.");
        w.WriteStartObject("options"); w.WriteBoolean("dryRun", false); w.WriteString("mode", "replace"); w.WriteEndObject();
        WriteGrid(w);
        w.WriteStartObject("layers");
        w.WriteStartArray("fertility");
        foreach (var v in fertility) w.WriteNumberValue(v);
        w.WriteEndArray();
        w.WriteEndObject();
        w.WriteEndObject();
    }
}
