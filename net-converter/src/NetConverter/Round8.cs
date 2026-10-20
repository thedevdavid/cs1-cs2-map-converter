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

    /// <summary>Bridge live tests: TransformerStation01 (48 × 56 m; 02 is the small 24 × 32 one), HydroelectricPowerPlant01 (48 × 72 m).</summary>
    public const string DamPrefab = "HydroelectricPowerPlant01", SubstationPrefab = "TransformerStation01";
    /// <summary>The dam deck network (34 m wide, max slope 0, elevation 0) along the CS1 dam axis between the quays, level at the crest.</summary>
    public const string DamDeckPrefab = "Hydroelectric_Power_Plant_01 Dam";
    public static readonly (double X, double Z) DamDeckA = (1440, 4673), DamDeckB = (1576, 4694);
    public const double DamCrestTrue = 172;

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

    public sealed record PowerEnd(string Target, OutEdge Replaces, OutNode Anchor, OutNode OldEnd, double X, double Z, (double X, double Z)? Via = null, string Note = "");

    /// <summary>Clearance of the other buildings: their footprint half-diagonal (TransformerStation01 48 × 56 m → 37 m) + 40 m.</summary>
    public const double PassClearance = 77;

    static double SegDist(double px, double pz, double ax, double az, double bx, double bz)
    {
        double dx = bx - ax, dz = bz - az, l2 = dx * dx + dz * dz;
        double t = l2 > 0 ? Math.Clamp(((px - ax) * dx + (pz - az) * dz) / l2, 0, 1) : 0;
        return Math.Sqrt((ax + t * dx - px) * (ax + t * dx - px) + (az + t * dz - pz) * (az + t * dz - pz));
    }

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
        // A-v3: the dam course ran past the transformer station and the game routed it through the station's connector. Where a
        // course passes another target closer than PassClearance, it bends round it through an extra pylon on the far side.
        for (int i = 0; i < res.Count; i++)
        {
            var pe = res[i];
            foreach (var o in targets.Where(o => o.Name != pe.Target))
            {
                double ax = pe.Anchor.Position.X, az = pe.Anchor.Position.Z;
                if (SegDist(o.X, o.Z, ax, az, pe.X, pe.Z) >= PassClearance) continue;
                var t = targets.First(t => t.Name == pe.Target);
                // Away from the other building, perpendicular to the straight course.
                double lx = t.X - ax, lz = t.Z - az, ll = Math.Sqrt(lx * lx + lz * lz), nx = -lz / ll, nz = lx / ll;
                double side = (o.X - ax) * nx + (o.Z - az) * nz;
                if (side > 0) { nx = -nx; nz = -nz; }
                (double X, double Z) via = (o.X, o.Z);
                for (double r = PassClearance; r <= 300; r += 5)
                {
                    via = (o.X + nx * r, o.Z + nz * r);
                    double ex = t.X + 8 * (via.X - t.X) / Math.Sqrt((via.X - t.X) * (via.X - t.X) + (via.Z - t.Z) * (via.Z - t.Z)), ez = t.Z + 8 * (via.Z - t.Z) / Math.Sqrt((via.X - t.X) * (via.X - t.X) + (via.Z - t.Z) * (via.Z - t.Z));
                    if (SegDist(o.X, o.Z, ax, az, via.X, via.Z) >= PassClearance && SegDist(o.X, o.Z, via.X, via.Z, ex, ez) >= PassClearance)
                    {
                        res[i] = pe with { X = ex, Z = ez, Via = via, Note = string.Create(CultureInfo.InvariantCulture,
                            $"the straight course passed the {o.Name} {SegDist(o.X, o.Z, ax, az, pe.X, pe.Z):F0} m off; extra pylon {r:F0} m from it on the far side") };
                        break;
                    }
                }
            }
        }
        return res;
    }

    /// <summary>The dam edge as written to import-dam.json: west and east end (x, z) and the crest y in output metres.</summary>
    public sealed record DamFrame(double Wx, double Wz, double Ex, double Ez, double CrestOut);

    /// <summary>A-v6 live (bridge 0.9.26): the plant the dam edge builds has its High-voltage Marker at (1398.66, 148.18, 4718.33), 22.0 m back from the east
    /// end along the dam axis and 27.0 m to its left (reservoir side). The working patch (next-run/power-ends-a6.json) ended on that marker with radius 60
    /// (the substation marker is 148 m away) and an extra pylon 36.2 m past the east end, 48.6 m to the right, so no span exceeds about 210 m.
    /// Offsets in the dam frame (along from the east end, left of west to east), so both follow the dam edge.</summary>
    public static readonly (double Along, double Left) DamMarker = (-22.0, 27.0), DamPylon = (36.24, -48.56);

    public static (double X, double Z) InDamFrame(DamFrame d, (double Along, double Left) o)
    {
        double ux = d.Ex - d.Wx, uz = d.Ez - d.Wz, l = Math.Sqrt(ux * ux + uz * uz); ux /= l; uz /= l;
        return (d.Ex + o.Along * ux - o.Left * uz, d.Ez + o.Along * uz + o.Left * ux);
    }

    /// <summary>Gravel Road access courses: from the nearest point of an imported road (Gravel / Small / local, no highway or path) to each target.</summary>
    public static void WriteAccessRoads(string path, List<(string Name, double X, double Z)> targets, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor, double? crestOut)
    {
        var roads = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab is { } p && (p.Contains("Gravel") || p.Contains("Small Road") || p.Contains("Alley")) && e.Structure == Structure.Ground).ToList();
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", "Dakota Falls access roads");
        w.WriteString("_comment", "Round 26: Gravel Road access to the dam plant and TransformerStation01 (both RequireRoad). The start node attaches to the nearest CS1 road through snapToSegment; the end sits at the building front. Ground level, absolute heights.");
        w.WriteStartArray("nodes");
        var edgesOut = new List<(string Id, string A, string B, string Note)>();
        foreach (var (name, tx, tz) in targets)
        {
            (OutEdge E, Vec3 P, double D)? best = null;
            foreach (var e in roads)
                for (int k = 0; k <= 32; k++) { var p = e.Curve.Eval(k / 32.0); double d = Math.Sqrt((p.X - tx) * (p.X - tx) + (p.Z - tz) * (p.Z - tz)); if (best is not { } b || d < b.D) best = (e, p, d); }
            if (best is not { } bb) continue;
            double T(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : bb.P.Y * Outputs.YScale;
            w.WriteStartObject();
            w.WriteString("id", $"{name}_road_a"); w.WriteNumber("x", Json.R2(bb.P.X)); w.WriteNumber("y", Json.R2(T(bb.P.X, bb.P.Z))); w.WriteNumber("z", Json.R2(bb.P.Z)); w.WriteNumber("elevation", 0);
            w.WriteStartObject("snapToSegment"); w.WriteNumber("radius", 3); w.WriteString("prefabContains", bb.E.Map.Cs2Prefab); w.WriteEndObject();
            w.WriteEndObject();
            w.WriteStartObject();
            w.WriteString("id", $"{name}_road_b"); w.WriteNumber("x", Json.R2(tx)); w.WriteNumber("y", Json.R2(name == "plant" && crestOut is { } c ? c : T(tx, tz))); w.WriteNumber("z", Json.R2(tz)); w.WriteNumber("elevation", 0);
            w.WriteEndObject();
            edgesOut.Add(($"{name}_access", $"{name}_road_a", $"{name}_road_b", string.Create(CultureInfo.InvariantCulture, $"from {bb.E.Id} ({bb.E.Map.Cs2Prefab}) {bb.D:F0} m to the {name}")));
        }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        foreach (var (id, a, b, note) in edgesOut) { w.WriteStartObject(); w.WriteString("id", id); w.WriteString("a", a); w.WriteString("b", b); w.WriteString("prefab", "Gravel Road"); w.WriteString("note", note); w.WriteEndObject(); }
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 8); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", "absolute"); w.WriteBoolean("dryRun", false); w.WriteBoolean("bypassValidation", true);
        w.WriteEndObject();
        w.WriteEndObject();
        foreach (var (id, _, _, note) in edgesOut) Console.WriteLine($"access road {id}: {note}");
    }

    public static void WritePowerEnds(string path, List<PowerEnd> ends, Func<double, double, double>? cs2TerrainTrue, double heightFactor, bool absolute, DamFrame? dam = null)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", "Dakota Falls power line ends");
        w.WriteString("_comment", "The last course of each 345 kV circuit (import-power.json leaves those courses out). The end node carries connectTo {voltage high, radius 60} (bridge 0.9.13) and attaches to the building's high-voltage marker node; its x/z only picks the marker: the substation centre moved 8 m toward the line; for the dam, the plant marker as measured on A-v6, placed in the frame of the dam edge. The start node joins the existing pylon node through snapToSegment (radius 2).");
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
            bool onDam = e.Target == "dam" && dam != null;
            var (ex, ez) = onDam ? InDamFrame(dam!, DamMarker) : (e.X, e.Z);
            w.WriteNumber("x", Json.R2(ex)); w.WriteNumber("z", Json.R2(ez));
            double ground = onDam ? dam!.CrestOut : cs2TerrainTrue != null ? cs2TerrainTrue(ex, ez) * heightFactor : e.OldEnd.Terrain * Outputs.YScale;
            w.WriteNumber("y", Json.R2(ground)); w.WriteNumber("elevation", 0);
            // Bridge 0.9.13: attach to the nearest building's high-voltage marker node; the x/z only picks the marker.
            w.WriteStartObject("connectTo"); w.WriteString("voltage", "high"); w.WriteNumber("radius", 60); w.WriteEndObject();
            w.WriteEndObject();
            if (onDam)
            {
                var (px, pz) = InDamFrame(dam!, DamPylon);
                w.WriteStartObject();
                w.WriteString("id", $"{e.Target}_pylon");
                w.WriteNumber("x", Json.R2(px)); w.WriteNumber("z", Json.R2(pz));
                w.WriteNumber("y", Json.R2(cs2TerrainTrue != null ? cs2TerrainTrue(px, pz) * heightFactor : dam!.CrestOut)); w.WriteNumber("elevation", 0);
                w.WriteString("note", "A-v6: extra pylon before the plant marker, spans under 210 m");
                w.WriteEndObject();
            }
            if (e.Via is { } v)
            {
                w.WriteStartObject();
                w.WriteString("id", $"{e.Target}_via");
                w.WriteNumber("x", Json.R2(v.X)); w.WriteNumber("z", Json.R2(v.Z));
                w.WriteNumber("y", Json.R2(cs2TerrainTrue != null ? cs2TerrainTrue(v.X, v.Z) * heightFactor : e.OldEnd.Terrain * Outputs.YScale)); w.WriteNumber("elevation", 0);
                w.WriteString("note", e.Note);
                w.WriteEndObject();
            }
        }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        foreach (var e in ends)
        {
            var chain = new List<string> { e.Anchor.Id };
            if (e.Via != null) chain.Add($"{e.Target}_via");
            if (e.Target == "dam" && dam != null) chain.Add($"{e.Target}_pylon");
            chain.Add($"{e.Target}_end");
            var legs = Enumerable.Range(0, chain.Count - 1).Select(k => (Id: chain.Count == 2 ? $"{e.Replaces.Id}_to_{e.Target}" : $"{e.Replaces.Id}_to_{e.Target}_{(char)('a' + k)}", A: chain[k], B: chain[k + 1]));
            foreach (var (id, a, b) in legs)
            {
                w.WriteStartObject();
                w.WriteString("id", id);
                w.WriteString("a", a); w.WriteString("b", b);
                w.WriteString("prefab", e.Replaces.Map.Cs2Prefab);
                if (e.Note.Length > 0) w.WriteString("note", e.Note);
                w.WriteString("cs1_prefab", e.Replaces.Map.Cs1Prefab);
                w.WriteNumber("cs1_segment", e.Replaces.Cs1Segment);
                w.WriteEndObject();
            }
        }
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 64); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", absolute ? "absolute" : "elevation"); w.WriteBoolean("dryRun", false);
        w.WriteBoolean("bypassValidation", true);   // A-v6: OverlapExisting at the plant marker otherwise
        w.WriteEndObject();
        w.WriteEndObject();
    }

    /// <summary>
    /// The dam per the bridge contract (IMPORT_SCHEMA.md "Dams", 0.9.23–0.9.25, A-v5 analysis): ONE straight edge of the dam net, only the two end nodes, both
    /// at the crest (as close to the CS1 172 m as a 304 m bank-to-bank line allows, × scale, absolute y, level), flow "invert" (the prefab is FlowLeft and the river runs north → south, so the plant lands on the
    /// intended side), batchSize 1. The fixed sequence 64 + 4 × 18 + 32 + 4 × 18 + 64 m makes the edge 304 m. The line is searched around the CS1 dam site:
    /// centre within 400 m, heading 81° ± 15° (CS1 axis, west–east across the river); it must cross the CS1 water surface and have its middle at least
    /// 8.5 m below the crest (bridge.lowElevation); among those, the abutments nearest the crest height on the CS2 playable heightmap win.
    /// Writes the terrain profile along the chosen line next to the import file (dam-profile.md).
    /// </summary>
    public const double DamLength = 304;

    public static List<string> WriteDam(string path, Func<double, double, double>? terrainTrue, double heightFactor, Func<double, double, double?>? waterSurface = null)
    {
        Func<double, double, bool>? wet = waterSurface == null ? null : (x, z) => waterSurface(x, z) != null;
        var log = new List<string>();
        double T(double x, double z) => terrainTrue?.Invoke(x, z) ?? DamCrestTrue;
        double cx0 = (DamDeckA.X + DamDeckB.X) / 2, cz0 = (DamDeckA.Z + DamDeckB.Z) / 2;
        double head0 = Math.Atan2(DamDeckB.X - DamDeckA.X, DamDeckB.Z - DamDeckA.Z) * 180 / Math.PI;
        double half = DamLength / 2, crest = DamCrestTrue;
        (double Score, double Cx, double Cz, double Head, double Tw, double Te, double Tmid, int WetSamples, double Crest)? best = null;
        for (double dh = -15; dh <= 15; dh += 1)
        {
            double h = (head0 + dh) * Math.PI / 180, ux = Math.Sin(h), uz = Math.Cos(h);
            for (double ox = -400; ox <= 400; ox += 8)
                for (double oz = -400; oz <= 400; oz += 8)
                {
                    double cx = cx0 + ox, cz = cz0 + oz;
                    double tMid = T(cx, cz);
                    int wetN = 0;
                    if (wet != null) for (int k = 1; k < 16; k++) { double t = -half + DamLength * k / 16; if (wet(cx + ux * t, cz + uz * t)) wetN++; }
                    if (wet != null && wetN == 0) continue;                    // must cross the river
                    double tw = T(cx - ux * half, cz - uz * half), te = T(cx + ux * half, cz + uz * half);
                    // Crest: between the two bank heights, as close to the CS1 172 m as they allow; the middle must stay 8.5 m below it.
                    double c = Math.Clamp(DamCrestTrue, Math.Min(tw, te), Math.Max(tw, te));
                    if (c - tMid < 8.5) continue;                              // bridge.lowElevation at the middle
                    double score = Math.Abs(c - tw) + Math.Abs(c - te) + 0.5 * Math.Abs(c - DamCrestTrue) + 0.002 * (ox * ox + oz * oz) / 100 + 0.2 * Math.Abs(dh);
                    if (best == null || score < best.Value.Score) best = (score, cx, cz, head0 + dh, tw, te, tMid, wetN, c);
                }
        }
        if (best is not { } b) { log.Add("no dam line found that crosses the water with an 8.5 m deep valley within 400 m of the CS1 site; nothing written"); File.Delete(path); return log; }
        crest = Math.Round(b.Crest, 1);
        double hr = b.Head * Math.PI / 180, bx = Math.Sin(hr), bz = Math.Cos(hr);
        var w = (X: b.Cx - bx * half, Z: b.Cz - bz * half); var e = (X: b.Cx + bx * half, Z: b.Cz + bz * half);
        double y = Math.Round(crest * heightFactor, 2);
        using (var fs = File.Create(path))
        using (var wr = new Utf8JsonWriter(fs, Json.Indented))
        {
            wr.WriteStartObject();
            wr.WriteString("name", "Dakota Falls dam");
            wr.WriteString("_comment", "Hydro dam (bridge 0.9.25): one level straight edge of 304 m (fixed 64 + 4x18 + 32 + 4x18 + 64), only the two end nodes at the crest, flow invert (FlowLeft prefab, river north to south, reservoir north); the game builds the plant and the dam spans.");
            wr.WriteStartArray("nodes");
            foreach (var (id, p) in new[] { ("damW", w), ("damE", e) })
            { wr.WriteStartObject(); wr.WriteString("id", id); wr.WriteNumber("x", Json.R2(p.X)); wr.WriteNumber("y", y); wr.WriteNumber("z", Json.R2(p.Z)); wr.WriteEndObject(); }
            wr.WriteEndArray();
            wr.WriteStartArray("edges");
            wr.WriteStartObject();
            wr.WriteString("id", "dam"); wr.WriteString("a", "damW"); wr.WriteString("b", "damE"); wr.WriteString("prefab", DamDeckPrefab); wr.WriteString("flow", "invert");
            wr.WriteEndObject();
            wr.WriteEndArray();
            wr.WriteStartObject("options");
            wr.WriteNumber("batchSize", 1); wr.WriteBoolean("nodesFirst", true); wr.WriteString("heightMode", "absolute"); wr.WriteBoolean("bypassValidation", true); wr.WriteBoolean("dryRun", false);
            wr.WriteEndObject();
            wr.WriteEndObject();
        }
        // Profile along the line, every 16 m, with the terrain 40 m to either side (north = upstream).
        var md = new System.Text.StringBuilder();
        md.AppendLine("# Dam line profile (CS2 playable heightmap, true metres)").AppendLine();
        md.AppendLine(FormattableString.Invariant($"West abutment ({w.X:F1}, {w.Z:F1}), east abutment ({e.X:F1}, {e.Z:F1}), heading {b.Head:F1}° (CS1 axis {head0:F1}°), length {DamLength} m, crest {crest} m (× {heightFactor} = {y})."));
        md.AppendLine().AppendLine("| s m (from west) | x | z | terrain m | below crest m | CS1 water | terrain 40 m north | water 40 m north | terrain 40 m south | water 40 m south |").AppendLine("|---:|---:|---:|---:|---:|---|---:|---:|---:|---:|");
        string W(double x, double z) => waterSurface?.Invoke(x, z) is { } v ? v.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) : "";
        double nx = -bz, nz = bx;
        if (nz < 0) { nx = -nx; nz = -nz; }   // the side toward +z (north)
        for (double s0 = 0; s0 <= DamLength + 0.01; s0 += 16)
        {
            double x = w.X + bx * s0, z = w.Z + bz * s0, t = T(x, z);
            md.AppendLine(FormattableString.Invariant($"| {s0:F0} | {x:F0} | {z:F0} | {t:F1} | {crest - t:F1} | {(wet?.Invoke(x, z) == true ? "wet" : "")} | {T(x + nx * 40, z + nz * 40):F1} | {W(x + nx * 40, z + nz * 40)} | {T(x - nx * 40, z - nz * 40):F1} | {W(x - nx * 40, z - nz * 40)} |"));
        }
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "dam-profile.md"), md.ToString());
        log.Add(FormattableString.Invariant($"dam edge {DamLength} m heading {b.Head:F1}° (CS1 {head0:F1}°), crest {crest} m × {heightFactor}; west abutment ({w.X:F0}, {w.Z:F0}) terrain {b.Tw:F1} m (elevation {crest - b.Tw:F1}), east ({e.X:F0}, {e.Z:F0}) terrain {b.Te:F1} m (elevation {crest - b.Te:F1}); middle terrain {b.Tmid:F1} m ({crest - b.Tmid:F1} below the crest); {b.WetSamples}/15 samples over CS1 water; flow invert; profile dam-profile.md"));
        if (Math.Abs(crest - b.Tw) > 1 || Math.Abs(crest - b.Te) > 1) log.Add("an abutment is more than 1 m off the terrain (dam.elevation warning)");
        return log;
    }

    /// <summary>One level deck edge in absolute heights: crest y = 172 m true × heightFactor at both ends.</summary>
    public static void WriteDamDeck(string path, double heightFactor)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        double y = Json.R2(DamCrestTrue * heightFactor);
        w.WriteStartObject();
        w.WriteString("name", "Dakota Falls dam deck");
        w.WriteString("_comment", "Companion to import-dam.json: the HydroelectricPowerPlant01 deck network along the CS1 dam axis (quays (1440, 4673) to (1576, 4694)), level at the crest (172 m true × height factor). Bridge live test: builds as a level deck.");
        w.WriteStartArray("nodes");
        foreach (var (id, p) in new[] { ("dam_deck_w", DamDeckA), ("dam_deck_e", DamDeckB) })
        { w.WriteStartObject(); w.WriteString("id", id); w.WriteNumber("x", p.X); w.WriteNumber("y", y); w.WriteNumber("z", p.Z); w.WriteEndObject(); }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        w.WriteStartObject();
        w.WriteString("id", "dam_deck"); w.WriteString("a", "dam_deck_w"); w.WriteString("b", "dam_deck_e"); w.WriteString("prefab", DamDeckPrefab);
        w.WriteStartArray("control");
        foreach (var t in new[] { 1 / 3.0, 2 / 3.0 })
        { w.WriteStartArray(); w.WriteNumberValue(Json.R2(DamDeckA.X + t * (DamDeckB.X - DamDeckA.X))); w.WriteNumberValue(y); w.WriteNumberValue(Json.R2(DamDeckA.Z + t * (DamDeckB.Z - DamDeckA.Z))); w.WriteEndArray(); }
        w.WriteEndArray();
        w.WriteEndObject();
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 64); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", "absolute"); w.WriteBoolean("dryRun", false);
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
