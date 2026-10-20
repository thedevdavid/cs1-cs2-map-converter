using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetConverter;

/// <summary>
/// Round 9: rock texture on every PO terrain-like footprint, the fort walls, PO paved blocks (pavement tops and quay sides),
/// lake sources for unexplained wet bodies, and the last network gap fixes. Each is its own import file.
/// </summary>
public static class Round9
{
    static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    // ---------- shared network writer (absolute heights) ----------

    public sealed record NetNode(string Id, double X, double Y, double Z, double? SnapRadius = null, string? SnapPrefab = null);
    public sealed record NetEdge(string Id, string A, string B, string Prefab, Vec3? C1 = null, Vec3? C2 = null, string? Wall = null, string Note = "", int? FixedIndex = null, bool NoPillars = false);

    public static void WriteNet(string path, string name, string comment, IReadOnlyList<NetNode> nodes, IReadOnlyList<NetEdge> edges, IReadOnlyList<string>? replaces = null)
    {
        var byId = nodes.GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, Json.Indented);
        w.WriteStartObject();
        w.WriteString("name", name);
        w.WriteString("_comment", comment);
        if (replaces != null) { w.WriteStartArray("replaces"); foreach (var r in replaces) w.WriteStringValue(r); w.WriteEndArray(); }
        w.WriteStartArray("nodes");
        foreach (var n in byId.Values)
        {
            w.WriteStartObject();
            w.WriteString("id", n.Id);
            w.WriteNumber("x", Json.R2(n.X)); w.WriteNumber("y", Json.R2(n.Y)); w.WriteNumber("z", Json.R2(n.Z));
            if (n.SnapRadius is { } r) { w.WriteStartObject("snapToSegment"); w.WriteNumber("radius", r); if (n.SnapPrefab != null) w.WriteString("prefabContains", n.SnapPrefab); w.WriteEndObject(); }
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartArray("edges");
        foreach (var e in edges)
        {
            var a = byId[e.A]; var b = byId[e.B];
            var c1 = e.C1 ?? new Vec3(a.X + (b.X - a.X) / 3, (2 * a.Y + b.Y) / 3, a.Z + (b.Z - a.Z) / 3);
            var c2 = e.C2 ?? new Vec3(a.X + 2 * (b.X - a.X) / 3, (a.Y + 2 * b.Y) / 3, a.Z + 2 * (b.Z - a.Z) / 3);
            w.WriteStartObject();
            w.WriteString("id", e.Id); w.WriteString("a", e.A); w.WriteString("b", e.B); w.WriteString("prefab", e.Prefab);
            w.WriteStartArray("control");
            foreach (var c in new[] { c1, c2 }) { w.WriteStartArray(); w.WriteNumberValue(Json.R2(c.X)); w.WriteNumberValue(Json.R2(c.Y)); w.WriteNumberValue(Json.R2(c.Z)); w.WriteEndArray(); }
            w.WriteEndArray();
            if (e.Wall != null) w.WriteString("wall", e.Wall);
            if (e.FixedIndex is { } fi) w.WriteNumber("fixedIndex", fi);
            if (e.NoPillars) w.WriteBoolean("noPillars", true);
            if (e.Note.Length > 0) w.WriteString("note", e.Note);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteStartObject("options");
        w.WriteNumber("batchSize", 64); w.WriteBoolean("nodesFirst", true); w.WriteString("heightMode", "absolute"); w.WriteBoolean("bypassValidation", true); w.WriteBoolean("dryRun", false);
        w.WriteEndObject();
        w.WriteEndObject();
    }

    static double Len(double ax, double az, double bx, double bz) => Math.Sqrt((ax - bx) * (ax - bx) + (az - bz) * (az - bz));

    // ---------- 2. Rock texture on the PO terrain-like footprints ----------

    /// <summary>Cells of the v2 terrain-like raster (3.5 m, value &gt; 0 inside a PO footprint) → hole-free polygons simplified to 4 m.</summary>
    public static (List<List<(double X, double Z)>> Polygons, double Area, int Patches, int Dropped) PoRock(string rasterPath, double minArea = 100)
    {
        var hm = Heightmap.LoadPng(rasterPath);   // row 0 north, value/32 = metres; only > 0 matters
        int n = hm.Size;
        var g = new GridSpec(n, hm.MetresPerPixel, -hm.Half, -hm.Half);
        var mask = new HashSet<int>();
        for (int i = 0; i < n * n; i++) { var (x, z) = g.Centre(i); if (hm.Sample(x, z) > 0) mask.Add(i); }
        var (polys, patches, dropped) = Surfaces.MaskPolygons(mask, g, minArea, 4, 2000);
        return (polys, mask.Count * g.CellSize * g.CellSize, patches, dropped);
    }

    // ---------- 3. The fort ----------

    /// <summary>
    /// The CS1 Castle Wall as a ruined stone line (round 11b; the EU_Wall pieces are 0.9 m garden walls, and no vanilla rampart exists):
    /// RuinsStoneFormation01/02 alternating every 12 m along each Bézier with the curve's heading, StandingStone01–08 at the segment ends
    /// (one per shared end), RuinsStoneCairns01–03 every 25 m, 3–6 m inside the wall line (towards the centroid of the walls within 300 m),
    /// and OldCastle01 at the centroid of the largest wall group as the keep (UNVERIFIED as a placeable object; drop it if a dry run
    /// rejects it). RuinsCastle01 stays in import-objects.json at the CS1 Castle Ruins positions.
    /// </summary>
    public static (List<PlacedObject> Objects, (double X, double Z)? Keep) Fort(Dump dump, Regex wall, double half, ulong seed)
    {
        var segs = dump.Segments.Where(s => wall.IsMatch(s.Prefab)).OrderBy(s => s.Id).ToList();
        var res = new List<PlacedObject>();
        if (segs.Count == 0) return (res, null);
        var mids = segs.Select(s => s.Bezier.Eval(0.5)).ToList();
        // Groups of walls within 300 m of each other (single link).
        var group = Enumerable.Range(0, segs.Count).ToArray();
        int Find(int i) => group[i] == i ? i : group[i] = Find(group[i]);
        for (int i = 0; i < segs.Count; i++)
            for (int j = i + 1; j < segs.Count; j++)
                if (Len(mids[i].X, mids[i].Z, mids[j].X, mids[j].Z) <= 300) group[Find(i)] = Find(j);
        var centroid = Enumerable.Range(0, segs.Count).GroupBy(Find).ToDictionary(g => g.Key, g => (X: g.Average(i => mids[i].X), Z: g.Average(i => mids[i].Z), N: g.Count()));
        var rng = new Rng(seed);
        var ends = new HashSet<(long, long)>();
        int k = 0;
        void Add(string prefab, double x, double z, double yaw, string cs1, string src)
        {
            if (Math.Abs(x) > half || Math.Abs(z) > half) return;
            res.Add(new PlacedObject($"fort{++k}", prefab, "prop", Math.Round(x, 2), Math.Round(z, 2), null, Math.Round((yaw % 360 + 360) % 360, 1), cs1, src));
        }
        for (int si = 0; si < segs.Count; si++)
        {
            var s = segs[si];
            var c = centroid[Find(si)];
            const int K = 64;
            var cum = new double[K + 1]; var pts = new Vec3[K + 1];
            for (int q = 0; q <= K; q++) { pts[q] = s.Bezier.Eval(q / (double)K); if (q > 0) cum[q] = cum[q - 1] + Len(pts[q].X, pts[q].Z, pts[q - 1].X, pts[q - 1].Z); }
            (double X, double Z, double Yaw, double Nx, double Nz) At(double d)
            {
                int q = Array.FindIndex(cum, v => v >= d); if (q <= 0) q = 1;
                double f = (d - cum[q - 1]) / Math.Max(1e-9, cum[q] - cum[q - 1]);
                double x = pts[q - 1].X + f * (pts[q].X - pts[q - 1].X), z = pts[q - 1].Z + f * (pts[q].Z - pts[q - 1].Z);
                double dx = pts[q].X - pts[q - 1].X, dz = pts[q].Z - pts[q - 1].Z, l = Math.Max(1e-9, Math.Sqrt(dx * dx + dz * dz));
                double nx = -dz / l, nz = dx / l;   // left normal; flipped towards the group centroid (inside)
                if ((c.X - x) * nx + (c.Z - z) * nz < 0) { nx = -nx; nz = -nz; }
                return (x, z, Math.Atan2(dx, dz) * 180 / Math.PI, nx, nz);
            }
            double total = cum[K];
            int n = Math.Max(1, (int)Math.Round(total / 12));
            for (int i = 0; i < n; i++)
            {
                var p = At((i + 0.5) * total / n);
                Add(i % 2 == 0 ? "RuinsStoneFormation01" : "RuinsStoneFormation02", p.X, p.Z, p.Yaw, s.Prefab, $"segment {s.Id}");
            }
            for (double d = 12.5; d < total; d += 25)
            {
                var p = At(d); double off = 3 + 3 * rng.NextDouble();
                Add($"RuinsStoneCairns0{1 + rng.Next(3)}", p.X + p.Nx * off, p.Z + p.Nz * off, rng.NextDouble() * 360, s.Prefab, $"segment {s.Id}");
            }
            foreach (var e in new[] { s.Bezier.A, s.Bezier.D })
                if (ends.Add(((long)Math.Round(e.X), (long)Math.Round(e.Z))))
                    Add($"StandingStone0{1 + rng.Next(8)}", e.X, e.Z, rng.NextDouble() * 360, s.Prefab, $"segment {s.Id} end");
        }
        var main = centroid.Values.MaxBy(v => v.N);
        Add("OldCastle01", main.X, main.Z, 0, "Castle Wall (keep)", "fort centroid");
        return (res, (main.X, main.Z));
    }

    // ---------- 4. PO paved blocks ----------

    public sealed record PavedBlock(int Id, string Prefab, double Yaw, double MinY, double MaxY, List<(double X, double Z)> Top, (double X, double Z)[] Corners);

    public static List<PavedBlock> LoadPavedBlocks(string poOfflinePath)
    {
        var res = new List<PavedBlock>();
        if (!File.Exists(poOfflinePath)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(poOfflinePath));
        foreach (var o in doc.RootElement.GetProperty("objects").EnumerateArray())
        {
            if (o.GetProperty("class").GetString() != "structure" || (o.TryGetProperty("subClass", out var sc) ? sc.GetString() : "") != "paved block") continue;
            if (!o.TryGetProperty("vertices", out var vs) || vs.GetArrayLength() < 9) continue;
            var v = vs.EnumerateArray().Select(x => x.GetDouble()).ToArray();
            double minY = double.MaxValue, maxY = double.MinValue;
            for (int k = 1; k < v.Length; k += 3) { minY = Math.Min(minY, v[k]); maxY = Math.Max(maxY, v[k]); }
            var top = new List<(double X, double Z)>();
            for (int k = 0; k + 2 < v.Length; k += 3) if (v[k + 1] >= maxY - 0.3) top.Add((v[k], v[k + 2]));
            var hull = Surfaces.ConvexHull(top);
            if (hull.Count < 3 || Math.Abs(Surfaces.Area(hull)) < 4)
            {
                // Sloped or tilted top: the plan footprint of the whole block.
                top.Clear();
                for (int k = 0; k + 2 < v.Length; k += 3) top.Add((v[k], v[k + 2]));
                hull = Surfaces.ConvexHull(top);
                if (hull.Count < 3 || Math.Abs(Surfaces.Area(hull)) < 4) continue;
            }
            double yaw = o.TryGetProperty("yawDegrees", out var yd) && yd.ValueKind == JsonValueKind.Number ? yd.GetDouble() : 0;
            double r = yaw * Math.PI / 180, fx = Math.Sin(r), fz = Math.Cos(r);
            double cx = hull.Average(p => p.X), cz = hull.Average(p => p.Z);
            double minF = double.MaxValue, maxF = double.MinValue, minR = double.MaxValue, maxR = double.MinValue;
            foreach (var (x, z) in top)
            {
                double dx = x - cx, dz = z - cz, f = dx * fx + dz * fz, rr = dx * fz - dz * fx;
                minF = Math.Min(minF, f); maxF = Math.Max(maxF, f); minR = Math.Min(minR, rr); maxR = Math.Max(maxR, rr);
            }
            (double X, double Z) P(double f, double rr) => (cx + f * fx + rr * fz, cz + f * fz - rr * fx);
            // Corners in order: (minF,minR) (maxF,minR) (maxF,maxR) (minF,maxR); sides 0-1 and 2-3 run along f.
            var corners = new[] { P(minF, minR), P(maxF, minR), P(maxF, maxR), P(minF, maxR) };
            if (maxF - minF < maxR - minR) corners = [corners[1], corners[2], corners[3], corners[0]];   // make 0-1 / 2-3 the long sides
            res.Add(new PavedBlock(o.GetProperty("id").GetInt32(), o.GetProperty("basePrefab").GetString() ?? "", yaw, minY, maxY, hull, corners));
        }
        return res;
    }

    /// <summary>Pavement polygons of the block tops (overlaps unioned) and QuaySmall01 edges along the long sides of blocks taller than 4 m.</summary>
    public static (List<SurfacePolygon> Surfaces, List<NetNode> Nodes, List<NetEdge> Edges, int Tall) PavedStructures(List<PavedBlock> blocks, string pavement, double heightFactor, double half)
    {
        var polys = blocks.Select(b => new SurfacePolygon($"pb{b.Id}", pavement, Surfaces.ClipSquare(b.Top, half), $"PO {b.Id} {b.Prefab} top")).Where(p => p.Ring.Count >= 3).ToList();
        var (u, _, _) = Surfaces.UnionOverlapping(polys, 1);
        var nodes = new List<NetNode>(); var edges = new List<NetEdge>(); int tall = 0;
        foreach (var b in blocks)
        {
            if (b.MaxY - b.MinY <= 4) continue;
            tall++;
            double y = b.MaxY * heightFactor;
            foreach (var (i, j) in new[] { (0, 1), (2, 3) })
            {
                var (ax, az) = b.Corners[i]; var (bx, bz) = b.Corners[j];
                if (Len(ax, az, bx, bz) < 4 || Math.Max(Math.Max(Math.Abs(ax), Math.Abs(az)), Math.Max(Math.Abs(bx), Math.Abs(bz))) > half) continue;
                nodes.Add(new NetNode($"pb{b.Id}_{i}", ax, y, az)); nodes.Add(new NetNode($"pb{b.Id}_{j}", bx, y, bz));
                edges.Add(new NetEdge($"pb{b.Id}_side{i}{j}", $"pb{b.Id}_{i}", $"pb{b.Id}_{j}", "QuaySmall01", Note: Fmt($"PO {b.Id} paved block, {b.MaxY - b.MinY:F0} m tall")));
            }
        }
        return (u, nodes, edges, tall);
    }

    // ---------- 5. Lakes ----------

    /// <summary>
    /// One natural source per wet body that no CS1 source within 400 m explains (water-check.md): at the body cell nearest its centroid,
    /// level = the body's median CS1 surface, radius = √(area/π) clamped 20–300 m. Bodies of one cell are left out.
    /// </summary>
    public static (List<WaterOut> Sources, int Bodies, int Outside, int Tiny) Lakes(WaterSurface ws, Dump dump, double seaLevelCs1, double half,
        Func<double, double, double>? cs2TerrainTrue, out List<string> log)
    {
        var (_, bodies) = ws.Orphans(dump, seaLevelCs1);
        var res = new List<WaterOut>(); int outside = 0, tiny = 0, k = 0;
        log = new List<string>();
        foreach (var b in bodies)
        {
            if (b.Count < 2) { tiny++; continue; }
            double cx = b.Average(i => ws.Grid.Centre(i).X), cz = b.Average(i => ws.Grid.Centre(i).Z);
            if (Math.Abs(cx) > half || Math.Abs(cz) > half) { outside++; continue; }
            int best = b.MinBy(i => { var (x, z) = ws.Grid.Centre(i); return (x - cx) * (x - cx) + (z - cz) * (z - cz); });
            var (sx, sz) = ws.Grid.Centre(best);
            k++;
            double area = b.Count * ws.Grid.CellSize * ws.Grid.CellSize;
            double radius = Math.Round(Math.Clamp(Math.Sqrt(area / Math.PI), 20, 300));
            var surf = b.Select(i => ws.Cells[i].Surface).ToList();
            double level = WaterSurface.Median(surf.ToList()), range = surf.Max() - surf.Min();
            // Run A-v3: lake7 was rejected ("water level is not above the terrain"): the CS2 terrain differs from CS1's. Move the
            // source to the body cell with the lowest CS2 terrain; if the level is still not 0.5 m above it, drop the lake.
            string moved = "";
            if (cs2TerrainTrue != null && level <= cs2TerrainTrue(sx, sz) + 0.5)
            {
                int low = b.MinBy(i => { var (x, z) = ws.Grid.Centre(i); return cs2TerrainTrue(x, z); });
                var (lx, lz) = ws.Grid.Centre(low); double t = cs2TerrainTrue(lx, lz);
                if (level <= t + 0.5) { log.Add(Fmt($"lake{k} dropped: level {level:F2} m is not above the CS2 terrain (lowest cell of the body {t:F2} m)")); continue; }
                moved = Fmt($"; moved to the lowest CS2 terrain cell ({lx:F0}, {lz:F0}, terrain {t:F2} m): the centre cell was {cs2TerrainTrue(sx, sz):F2} m");
                log.Add(Fmt($"lake{k}: centre terrain {cs2TerrainTrue(sx, sz):F2} m vs level {level:F2} m, moved to ({lx:F0}, {lz:F0}) terrain {t:F2} m"));
                (sx, sz) = (lx, lz);
            }
            res.Add(new WaterOut($"lake{k}", sx, sz, level, radius, (long)(radius * radius), Fmt($"wet body of {b.Count} cells ({area / 1e6:F3} km²), surface {surf.Min():F1}–{surf.Max():F1} m, no CS1 source within 400 m{(range > 3 ? "; SLOPING REACH: one level source will not fill it evenly, check in game" : "")}{moved}")));
        }
        return (res, bodies.Count, outside, tiny);
    }

    static string Fmt(FormattableString f) => f.ToString(Ci);

    /// <summary>
    /// Control points of a course bowed clear of a highway whose footprint would swallow it (the ends stay on their nodes, so only the
    /// middle samples move: control shift = 4/3 of the needed clearance + 1 m). Unchanged when nothing swallows it.
    /// </summary>
    public static (Vec3 B, Vec3 C, string Note) Clear(ConvertResult res, OutEdge e, double half, double topologyFactor)
    {
        var ex = new HashSet<OutNode>(ReferenceEqualityComparer.Instance) { e.A, e.B };
        var hit = FootprintShift.Inside(FootprintShift.Samples(e.Curve), half, (e.A.Position.Y + e.B.Position.Y) / 2, FootprintShift.Highways(res), ex, topologyFactor);
        if (hit is not { } h) return (e.Curve.B, e.Curve.C, "");
        double s = (h.Need + 1 - h.MinD) * 4 / 3;
        var off = new Vec3(h.Nx * s, 0, h.Nz * s);
        return (e.Curve.B + off, e.Curve.C + off, Fmt($"; bowed {s:F1} m away from {h.H.Id} {h.H.Map.Cs2Prefab} ({h.MinD:F1} m, needs {h.Need:F1})"));
    }

    // ---------- 6. Network gaps ----------

    /// <summary>Absolute y of a converter node in the output scale (as WriteImport writes it in absolute mode).</summary>
    public static double Y(OutNode n) => (n.Terrain + n.Elevation) * Outputs.YScale;

    public static Vec3 Scale(Vec3 p, OutNode a, OutNode b, double t) => p with { Y = Y(a) + t * (Y(b) - Y(a)) };

    /// <summary>
    /// The last network gaps of Run A, as courses that join the live network by snapToSegment (an end within 2 % of a segment end joins
    /// that node). Returns the main file and the alternative 4-lane taper for e15883_0.
    /// </summary>
    public static (List<NetNode> Nodes, List<NetEdge> Edges, List<NetNode> TaperNodes, List<NetEdge> TaperEdges, List<string> Log) Gaps(ConvertResult res, double topologyFactor)
        => Gaps(res, topologyFactor, null, out _);

    /// <summary>
    /// As above; <paramref name="extraNodes"/> are nodes of other import files (bridge decks) a gap may end on, and <paramref name="replaces"/>
    /// lists the network courses the gap edges rebuild: on a fresh map they must leave import-all.json, or the original and its rebuild overlap
    /// (A-v4: e15883_0 and gap_e15883_3lane tore the 4-lane / 3-lane split at (463, −4520)).
    /// </summary>
    public static (List<NetNode> Nodes, List<NetEdge> Edges, List<NetNode> TaperNodes, List<NetEdge> TaperEdges, List<string> Log) Gaps(ConvertResult res, double topologyFactor,
        IReadOnlyList<NetNode>? extraNodes, out List<string> replaces)
    {
        var log = new List<string>();
        replaces = new List<string>();
        var rep = replaces;
        var nodes = new List<NetNode>(); var edges = new List<NetEdge>();
        var byId = res.Edges.ToDictionary(e => e.Id);
        var nodeById = res.Edges.SelectMany(e => new[] { e.A, e.B }).GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.First());
        var degree = res.Edges.Where(e => e.Map.Imported).SelectMany(e => new[] { e.A, e.B }).GroupBy(n => n.Id).ToDictionary(g => g.Key, g => g.Count());
        NetNode Snap(OutNode n, double r, string prefab) => new(n.Id, n.Position.X, Y(n), n.Position.Z, r, prefab);
        Vec3 C(OutEdge e, Vec3 c, double t) => new(c.X, Y(e.A) + t * (Y(e.B) - Y(e.A)), c.Z);

        // e15883_0 (4-lane, failed twice): a 3-lane course between the 4-lane and the 3-lane ends; the taper file keeps 12 m of 4-lane.
        var taperNodes = new List<NetNode>(); var taperEdges = new List<NetEdge>();
        if (byId.TryGetValue("e15883_0", out var e4))
        {
            nodes.Add(Snap(e4.A, 3, "Highway Oneway")); nodes.Add(Snap(e4.B, 3, "Highway Oneway"));
            var (b1, b2, bow) = Clear(res, e4, FootprintShift.Cs2Half("Highway Oneway - 3 lanes", 8), topologyFactor);
            edges.Add(new NetEdge("gap_e15883_3lane", e4.A.Id, e4.B.Id, "Highway Oneway - 3 lanes", C(e4, b1, 1 / 3.0), C(e4, b2, 2 / 3.0), Note: "e15883_0 as a 3-lane course" + bow));
            rep.Add(e4.Id);
            if (bow.Length > 0) log.Add("e15883_0" + bow);
            // Taper: 12 m of 4-lane from the 4-lane end.
            bool aIs4 = res.Edges.Any(x => x != e4 && (x.A == e4.A || x.B == e4.A) && x.Map.Cs2Prefab == "Highway Oneway - 4 lanes");
            double total = e4.Curve.LengthXZ(), t = Math.Clamp(12 / total, 0.05, 0.5);
            var four = aIs4 ? e4.A : e4.B; var three = aIs4 ? e4.B : e4.A;
            var m = e4.Curve.Eval(aIs4 ? t : 1 - t);
            double my = Y(four) + (Y(three) - Y(four)) * t;
            taperNodes.Add(Snap(four, 3, "Highway Oneway")); taperNodes.Add(Snap(three, 3, "Highway Oneway"));
            taperNodes.Add(new NetNode("gap_e15883_taper", m.X, my, m.Z));
            taperEdges.Add(new NetEdge("gap_e15883_4lane", four.Id, "gap_e15883_taper", "Highway Oneway - 4 lanes", Note: "12 m 4-lane taper"));
            taperEdges.Add(new NetEdge("gap_e15883_rest", "gap_e15883_taper", three.Id, "Highway Oneway - 3 lanes", Note: "rest as 3-lane"));
            log.Add(Fmt($"e15883_0: 3-lane course {total:F0} m (alternative: 4-lane taper 12 m + 3-lane, import-gaps-e15883-taper.json)"));
        }
        // e20430_0b (3-lane, OverlapExisting / InvalidShape): 15.5 m from the parallel carriageway, inside the two half-widths;
        // control points moved 1 m away from the nearest other highway course.
        if (byId.TryGetValue("e20430_0b", out var e3))
        {
            var mid = e3.Curve.Eval(0.5);
            var other = res.Layer(Layers.Highways).Where(x => x != e3 && x.A != e3.A && x.B != e3.A && x.A != e3.B && x.B != e3.B)
                .Select(x => (x, p: Enumerable.Range(0, 17).Select(k => x.Curve.Eval(k / 16.0)).MinBy(p => Len(p.X, p.Z, mid.X, mid.Z)))).MinBy(q => Len(q.p.X, q.p.Z, mid.X, mid.Z));
            double dx = e3.B.Position.X - e3.A.Position.X, dz = e3.B.Position.Z - e3.A.Position.Z, L = Math.Sqrt(dx * dx + dz * dz);
            double nx = -dz / L, nz = dx / L;
            if ((other.p.X - mid.X) * nx + (other.p.Z - mid.Z) * nz > 0) { nx = -nx; nz = -nz; }   // away from the parallel course
            nodes.Add(Snap(e3.A, 3, "Highway Oneway")); nodes.Add(Snap(e3.B, 3, "Highway Oneway"));
            rep.Add(e3.Id);
            edges.Add(new NetEdge("gap_e20430_offset", e3.A.Id, e3.B.Id, e3.Map.Cs2Prefab!,
                C(e3, e3.Curve.B with { X = e3.Curve.B.X + nx, Z = e3.Curve.B.Z + nz }, 1 / 3.0), C(e3, e3.Curve.C with { X = e3.Curve.C.X + nx, Z = e3.Curve.C.Z + nz }, 2 / 3.0),
                Note: Fmt($"e20430_0b with its course bowed 1 m away from {other.x.Id} ({Len(other.p.X, other.p.Z, mid.X, mid.Z):F1} m off)")));
            log.Add(Fmt($"e20430_0b: course moved 1 m away from {other.x.Id} ({Len(other.p.X, other.p.Z, mid.X, mid.Z):F1} m at mid)"));
        }
        // Gravel: the CS1 gap (1462, 6173)–(1383, 6161), closed with a straight Gravel Road between the two dead ends.
        NetNode? Find(string id) => nodeById.TryGetValue(id, out var n) ? Snap(n, 3, "Gravel")
            : extraNodes?.FirstOrDefault(x => x.Id == id) is { } x ? x with { SnapRadius = 3, SnapPrefab = null } : null;   // e.g. a bridge deck end (BR020)
        foreach (var (a, b) in new[] { ("n895", "n26596") })
            if (Find(a) is { } na && Find(b) is { } nb)
            {
                nodes.Add(na); nodes.Add(nb);
                edges.Add(new NetEdge($"gap_{a}_{b}", a, b, "Gravel Road", Note: "CS1 gap closed"));
                log.Add(Fmt($"gravel gap {a}–{b}: {Len(na.X, na.Z, nb.X, nb.Z):F0} m"));
            }
        // Gravel pieces whose T-junction node the game absorbed (Run A): the junction end splits the merged segment (radius 6).
        foreach (var id in new[] { "e25579_0", "e36563_0" })
            if (byId.TryGetValue(id, out var g))
            {
                foreach (var n in new[] { g.A, g.B }) nodes.Add(Snap(n, degree.GetValueOrDefault(n.Id) >= 3 ? 6 : 3, "Gravel"));
                edges.Add(new NetEdge($"gap_{id}", g.A.Id, g.B.Id, g.Map.Cs2Prefab!, C(g, g.Curve.B, 1 / 3.0), C(g, g.Curve.C, 2 / 3.0), Note: "Run A: T-junction node absorbed"));
                rep.Add(id);
                log.Add($"{id}: rebuilt, junction end snaps radius 6");
            }
        // Pavement Path stubs (PathSnap): a short connector from the path end to the segment beside it, snap radius 20.
        foreach (var n in nodeById.Values.Where(n => n.Snap != null).OrderBy(n => n.Id, StringComparer.Ordinal))
        {
            var target = res.Layer(Layers.Roads).Where(e => e.Map.Cs2Prefab == n.Snap!.Value.Prefab && e.A != n && e.B != n)
                .Select(e => Enumerable.Range(0, 65).Select(k => e.Curve.Eval(k / 64.0)).MinBy(p => Len(p.X, p.Z, n.Position.X, n.Position.Z)))
                .MinBy(p => Len(p.X, p.Z, n.Position.X, n.Position.Z));
            double d = Len(target.X, target.Z, n.Position.X, n.Position.Z);
            if (d < 10) { log.Add(Fmt($"path stub {n.Id}: {d:F1} m, inside the road footprint; left to its snapToSegment in import-all.json")); continue; }
            nodes.Add(Snap(n, 2, PathSnap.PathPrefab));
            nodes.Add(new NetNode($"{n.Id}_join", target.X, target.Y * Outputs.YScale, target.Z, 20, n.Snap!.Value.Prefab));
            edges.Add(new NetEdge($"gap_{n.Id}_path", n.Id, $"{n.Id}_join", PathSnap.PathPrefab, Note: Fmt($"path stub joined to {n.Snap.Value.Prefab}, {d:F1} m")));
            log.Add(Fmt($"path stub {n.Id}: connector {d:F1} m to {n.Snap.Value.Prefab}"));
        }
        return (nodes, edges, taperNodes, taperEdges, log);
    }
}
