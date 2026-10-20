// roads: carve/fill road corridors into the playable heightmap at the CS1 road height, for courses where the stamped PO
// rocks put the terrain above or below the road (net-converter road-terrain-mismatch.json). Core = road half width +
// shoulder at the road height; outside it a smoothstep grading over 10–20 m (longer for deeper cuts/fills). The river bed
// is untouched: pixels whose terrain is below the water level are never changed unless the road itself is lower.
// Pixels outside the corridors are copied bit for bit.
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HmRebuild;

public static class Roads
{
    const int N = Peaks.N;
    const double CellM = Peaks.CellM;

    public sealed record Course(string Id, string Prefab, double HalfWidth, double[] X, double[] Z, double[] Y, double ListedMaxDelta);

    public sealed record Options(double ShoulderM = 6, double BlendMinM = 10, double BlendMaxM = 20, double BlendPerDeltaM = 0.3, double WaterM = 93);

    public sealed record CourseResult(string Id, string Prefab, double MaxBeforeM, double MeanBeforeM, double MaxAfterM, double MeanAfterM, int Px, int RiverHeldPx);

    public sealed record Result(ushort[] Output, bool[] Zone, List<CourseResult> Courses, int ChangedPx, int RiverHeldPx);

    public static List<Course> Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        var list = new List<Course>();
        foreach (var e in doc.RootElement.GetProperty("edges").EnumerateArray())
        {
            var pts = e.GetProperty("points").EnumerateArray().Select(p => p.EnumerateArray().Select(v => v.GetDouble()).ToArray()).ToArray();
            list.Add(new Course(e.GetProperty("id").GetString()!, e.TryGetProperty("prefab", out var pf) ? pf.GetString() ?? "" : "",
                e.GetProperty("halfWidth").GetDouble(), pts.Select(p => p[0]).ToArray(), pts.Select(p => p[1]).ToArray(), pts.Select(p => p[2]).ToArray(),
                e.TryGetProperty("maxDelta", out var md) ? md.GetDouble() : double.NaN));
        }
        return list;
    }

    static double PxOf(double x) => (x + 7168) / CellM - 0.5;
    static double RowOf(double z) => (7168 - z) / CellM - 0.5;

    /// <summary>Bilinear terrain height (metres) at world (x, z).</summary>
    public static double Sample(ushort[] img, double x, double z)
    {
        double fx = PxOf(x), fy = RowOf(z);
        int x0 = Math.Clamp((int)Math.Floor(fx), 0, N - 2), y0 = Math.Clamp((int)Math.Floor(fy), 0, N - 2);
        double a = Math.Clamp(fx - x0, 0, 1), b = Math.Clamp(fy - y0, 0, 1);
        double M(int x, int y) => Peaks.ToM(img[y * N + x]);
        return (M(x0, y0) * (1 - a) + M(x0 + 1, y0) * a) * (1 - b) + (M(x0, y0 + 1) * (1 - a) + M(x0 + 1, y0 + 1) * a) * b;
    }

    public static (double max, double mean) Residual(ushort[] img, Course c)
    {
        double max = 0, sum = 0;
        for (int k = 0; k < c.X.Length; k++) { double d = Math.Abs(Sample(img, c.X[k], c.Z[k]) - c.Y[k]); max = Math.Max(max, d); sum += d; }
        return (max, sum / c.X.Length);
    }

    public static Result Run(ushort[] terrain, List<Course> courses, Options o)
    {
        var tM = new double[N * N];
        for (int i = 0; i < tM.Length; i++) tM[i] = Peaks.ToM(terrain[i]);
        // Per pixel: the strongest corridor claim (weight, target height); ties go to the nearer road centreline.
        var w = new double[N * N]; var target = new double[N * N]; var near = new double[N * N];
        Array.Fill(near, double.MaxValue);
        var owner = new int[N * N]; Array.Fill(owner, -1);
        for (int ci = 0; ci < courses.Count; ci++)
        {
            var c = courses[ci];
            double core = c.HalfWidth + o.ShoulderM;
            // Grading length from the deepest cut/fill on this course (terrain before vs road), within [min, max].
            double deepest = 0;
            for (int k = 0; k < c.X.Length; k++) deepest = Math.Max(deepest, Math.Abs(Sample(terrain, c.X[k], c.Z[k]) - c.Y[k]));
            double blend = Math.Clamp(o.BlendMinM + o.BlendPerDeltaM * deepest, o.BlendMinM, o.BlendMaxM);
            double reach = core + blend;
            for (int s = 0; s + 1 < c.X.Length; s++)
            {
                double ax = c.X[s], az = c.Z[s], bx = c.X[s + 1], bz = c.Z[s + 1];
                int px0 = (int)Math.Floor(PxOf(Math.Min(ax, bx) - reach)), px1 = (int)Math.Ceiling(PxOf(Math.Max(ax, bx) + reach));
                int py0 = (int)Math.Floor(RowOf(Math.Max(az, bz) + reach)), py1 = (int)Math.Ceiling(RowOf(Math.Min(az, bz) - reach));
                double dx = bx - ax, dz = bz - az, len2 = dx * dx + dz * dz;
                for (int py = Math.Max(0, py0); py <= Math.Min(N - 1, py1); py++)
                    for (int px = Math.Max(0, px0); px <= Math.Min(N - 1, px1); px++)
                    {
                        double x = -7168 + CellM * (px + 0.5), z = 7168 - CellM * (py + 0.5);
                        double t = len2 > 0 ? Math.Clamp(((x - ax) * dx + (z - az) * dz) / len2, 0, 1) : 0;
                        double qx = ax + t * dx, qz = az + t * dz, d = Math.Sqrt((x - qx) * (x - qx) + (z - qz) * (z - qz));
                        if (d > reach) continue;
                        double wt = d <= core ? 1 : 1 - SmoothStep(core, reach, d);
                        int i = py * N + px;
                        if (wt > w[i] + 1e-9 || (Math.Abs(wt - w[i]) <= 1e-9 && d < near[i]))
                        {
                            w[i] = wt; near[i] = d; owner[i] = ci;
                            target[i] = c.Y[s] + t * (c.Y[s + 1] - c.Y[s]);
                        }
                    }
            }
        }

        var output = (ushort[])terrain.Clone();
        var zone = new bool[N * N];
        int changed = 0, riverHeld = 0;
        var pxPer = new int[courses.Count]; var riverPer = new int[courses.Count];
        for (int i = 0; i < N * N; i++)
        {
            if (owner[i] < 0 || w[i] <= 0) continue;
            zone[i] = true;
            // River bed: terrain below the water level stays as it is, unless the road itself is below the water level.
            if (tM[i] < o.WaterM && target[i] >= o.WaterM) { riverHeld++; riverPer[owner[i]]++; continue; }
            double h = tM[i] + w[i] * (target[i] - tM[i]);
            if (target[i] >= o.WaterM) h = Math.Max(h, Math.Min(tM[i], o.WaterM));   // a cut never digs below the water level
            var v = Peaks.FromM(h);
            if (v != terrain[i]) { output[i] = v; changed++; pxPer[owner[i]]++; }
        }
        var res = new List<CourseResult>();
        for (int ci = 0; ci < courses.Count; ci++)
        {
            var (mb, ab) = Residual(terrain, courses[ci]); var (ma, aa) = Residual(output, courses[ci]);
            res.Add(new CourseResult(courses[ci].Id, courses[ci].Prefab, mb, ab, ma, aa, pxPer[ci], riverPer[ci]));
        }
        return new Result(output, zone, res, changed, riverHeld);
    }

    static double SmoothStep(double e0, double e1, double x) { double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1); return t * t * (3 - 2 * t); }

    const string Usage = "usage: hm-rebuild roads <playable-4096.png> <worldmap-4096.png> --courses road-terrain-mismatch.json\n" +
        "         [--shoulder 6] [--blend 10,20] [--water 93] [--out-dir DIR] [--name BASE] [--install DIR] [--world-tag .placeholder-hm-rebuild]\n" +
        "         [--preview-at x,z,label[,sizeM]]...";

    public static int Cli(string[] args)
    {
        if (args.Length < 3) { Console.Error.WriteLine(Usage); return 2; }
        string inPath = args[1], worldPath = args[2], worldTag = ".placeholder-hm-rebuild";
        string? coursesPath = null, outDir = null, outName = null, installDir = null;
        var at = new List<(double x, double z, string label, double size)>();
        var o = new Options(); var inv = CultureInfo.InvariantCulture;
        for (int i = 3; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new InputException($"missing value for {args[i]}");
            switch (args[i])
            {
                case "--courses": coursesPath = Next(); break;
                case "--shoulder": o = o with { ShoulderM = double.Parse(Next(), inv) }; break;
                case "--blend": { var p = Next().Split(','); o = o with { BlendMinM = double.Parse(p[0], inv), BlendMaxM = double.Parse(p[1], inv) }; break; }
                case "--water": o = o with { WaterM = double.Parse(Next(), inv) }; break;
                case "--out-dir": outDir = Next(); break;
                case "--name": outName = Next(); break;
                case "--install": installDir = Next(); break;
                case "--world-tag": worldTag = Next(); break;
                case "--preview-at":
                    {
                        var p = Next().Split(',');
                        at.Add((double.Parse(p[0], inv), double.Parse(p[1], inv), p.Length > 2 ? p[2] : $"at{at.Count + 1}", p.Length > 3 ? double.Parse(p[3], inv) : 800));
                        break;
                    }
                default: throw new InputException($"unknown option {args[i]}\n{Usage}");
            }
        }
        if (coursesPath == null) throw new InputException("--courses is required");
        var now = DateTime.Now; var prefix = now.ToString("MMddHHmm", inv);
        outDir ??= Path.Combine("out", prefix);
        Directory.CreateDirectory(outDir);

        var terrain = Peaks.LoadL16(inPath, out int w, out int h);
        if (w != N || h != N) throw new InputException($"playable map must be {N}x{N}");
        var world = Peaks.LoadL16(worldPath, out int ww, out int wh);
        if (ww != N || wh != N) throw new InputException($"world map must be {N}x{N}");
        var courses = Load(coursesPath);

        var res = Run(terrain, courses, o);
        long outside = 0;
        for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        if (outside != 0) throw new Exception($"BUG: {outside} pixels changed outside the corridors");

        var stem = Path.GetFileNameWithoutExtension(inPath);
        var outBase = $"{prefix}-" + (outName ?? Regex.Replace(stem, @"^\d{8}-", ""));
        var pOut = Path.Combine(outDir, outBase + ".png");
        var pWorld = Path.Combine(outDir, $"{outBase}_worldmap{worldTag}.png");
        if (File.Exists(pOut)) throw new InputException($"{pOut} exists; wait a minute or pass --name");
        Program.SaveL16(pOut, res.Output, N, N);
        var world2 = (ushort[])world.Clone();
        var inner = Peaks.BoxDown4(res.Output, N);
        for (int y = 0; y < Peaks.InnerN; y++) for (int x = 0; x < Peaks.InnerN; x++) world2[(Peaks.Inner0 + y) * N + Peaks.Inner0 + x] = inner[y * Peaks.InnerN + x];
        Program.SaveL16(pWorld, world2, N, N);

        foreach (var (wx, wz, label, size) in at)
        {
            int cx = (int)Math.Round(PxOf(wx)), cz = (int)Math.Round(RowOf(wz)), half = (int)(size / CellM / 2);
            int x0 = Math.Clamp(cx - half, 0, N - 1), y0 = Math.Clamp(cz - half, 0, N - 1), x1 = Math.Clamp(cx + half, 0, N - 1), y1 = Math.Clamp(cz + half, 0, N - 1);
            Peaks.SaveSideBySide(Path.Combine(outDir, $"{outBase}_{label}_before-after.png"), terrain, res.Output, N, x0, y0, x1 - x0 + 1, y1 - y0 + 1, 900);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# Road corridors: {Path.GetFileName(inPath)} -> {Path.GetFileName(pOut)}\n");
        sb.AppendLine($"Produced {now:yyyy-MM-dd HH:mm} (prefix {prefix}) from {Path.GetFileName(coursesPath)} ({courses.Count} edges).");
        sb.AppendLine($"Core = half width + {o.ShoulderM} m shoulders at the CS1 road height (linear along each segment); smoothstep grading over {o.BlendMinM}–{o.BlendMaxM} m " +
            $"({o.BlendMinM} m + {o.BlendPerDeltaM} × deepest cut/fill); terrain below {o.WaterM} m untouched unless the road is lower; cuts never go below {o.WaterM} m.\n");
        sb.AppendLine($"- pixels changed: {res.ChangedPx}; river-bed pixels held: {res.RiverHeldPx}; changed outside corridors: {outside} (asserted 0)");
        var after = res.Courses.Select(c => c.MaxAfterM).ToList();
        sb.AppendLine($"- residual |terrain − road| at the course points: max {after.Max():F2} m, mean of per-course max {after.Average():F2} m, mean over all points {res.Courses.Average(c => c.MeanAfterM):F2} m (before: max {res.Courses.Max(c => c.MaxBeforeM):F2} m)\n");
        sb.AppendLine("| edge | prefab | before max / mean (m) | after max / mean (m) | px changed | river px held |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (var c in res.Courses.OrderByDescending(c => c.MaxBeforeM))
            sb.AppendLine(string.Format(inv, "| {0} | {1} | {2:F2} / {3:F2} | {4:F2} / {5:F2} | {6} | {7} |", c.Id, c.Prefab, c.MaxBeforeM, c.MeanBeforeM, c.MaxAfterM, c.MeanAfterM, c.Px, c.RiverHeldPx));
        var statsPath = Path.Combine(outDir, $"{outBase}_stats.md");
        File.WriteAllText(statsPath, sb.ToString());
        Console.Write(sb.ToString());
        Console.WriteLine($"\nwrote {pOut}\nwrote {pWorld}\nwrote {statsPath}");
        if (installDir != null)
            foreach (var p in new[] { pOut, pWorld })
            {
                var dst = Path.Combine(installDir, Path.GetFileName(p));
                if (File.Exists(dst)) throw new InputException($"refusing to overwrite {dst}");
                File.Copy(p, dst); Console.WriteLine($"installed {dst}");
            }
        return 0;
    }
}
