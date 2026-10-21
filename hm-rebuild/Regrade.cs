// regrade: re-profile a road along a centreline (net-converter regrade spec, e.g. the National Road under the JP Rock shed).
// Per point: s, x, z, oldY, y (true metres), hillBench (m), hill (unit x/z vector towards the hill side).
// Within halfWidth of the centreline the terrain is set to y; on the hill side a bench of hillBench metres beyond the
// half-width is also at y, then the existing cut face is shifted vertically by (y − oldY) for FaceM metres and tapered back
// to the existing terrain over FaceTaperM; on the valley side an apron of ApronM blends y into the existing terrain.
// Beyond the first/last point everything fades out over EndTaperM. Pixels outside the zone are copied bit for bit.
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HmRebuild;

public static class Regrade
{
    const int N = Peaks.N;
    const double CellM = Peaks.CellM;

    public sealed record Pt(double S, double X, double Z, double OldY, double Y, double Bench, double HillX, double HillZ);
    public sealed record Spec(string Name, double HalfWidth, List<Pt> Points);
    public sealed record Options(double ApronM = 10, double FaceM = 20, double FaceTaperM = 20, double EndTaperM = 10);
    public sealed record Result(ushort[] Output, bool[] Zone, int ChangedPx, double MaxCutM, double MaxFillM);

    public static Spec Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        var r = doc.RootElement;
        var pts = r.GetProperty("points").EnumerateArray().Select(p =>
        {
            var h = p.GetProperty("hill");
            return new Pt(p.GetProperty("s").GetDouble(), p.GetProperty("x").GetDouble(), p.GetProperty("z").GetDouble(),
                p.GetProperty("oldY").GetDouble(), p.GetProperty("y").GetDouble(), p.GetProperty("hillBench").GetDouble(), h[0].GetDouble(), h[1].GetDouble());
        }).ToList();
        return new Spec(r.TryGetProperty("name", out var n) ? n.GetString() ?? "regrade" : "regrade", r.GetProperty("halfWidth").GetDouble(), pts);
    }

    static double SmoothStep(double e0, double e1, double x) { double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1); return t * t * (3 - 2 * t); }

    /// <summary>Nearest centreline projection of world (x, z): interpolated point, signed lateral offset (+ = hill side),
    /// distance beyond the ends (0 inside).</summary>
    static (Pt p, double lat, double beyond) Project(Spec sp, double x, double z)
    {
        double best = double.MaxValue; Pt bp = sp.Points[0]; double blat = 0, bbeyond = 0;
        var P = sp.Points;
        for (int k = 0; k + 1 < P.Count; k++)
        {
            var a = P[k]; var b = P[k + 1];
            double dx = b.X - a.X, dz = b.Z - a.Z, len2 = dx * dx + dz * dz;
            double tRaw = len2 > 0 ? ((x - a.X) * dx + (z - a.Z) * dz) / len2 : 0, t = Math.Clamp(tRaw, 0, 1);
            double qx = a.X + t * dx, qz = a.Z + t * dz, d2 = (x - qx) * (x - qx) + (z - qz) * (z - qz);
            if (d2 >= best) continue;
            best = d2;
            double hx = a.HillX + t * (b.HillX - a.HillX), hz = a.HillZ + t * (b.HillZ - a.HillZ), hl = Math.Sqrt(hx * hx + hz * hz);
            hx /= hl; hz /= hl;
            bp = new Pt(a.S + t * (b.S - a.S), qx, qz, a.OldY + t * (b.OldY - a.OldY), a.Y + t * (b.Y - a.Y), a.Bench + t * (b.Bench - a.Bench), hx, hz);
            double len = Math.Sqrt(len2);
            bbeyond = (k == 0 && tRaw < 0) ? -tRaw * len : (k + 2 == P.Count && tRaw > 1) ? (tRaw - 1) * len : 0;
            // Lateral offset along the hill vector (perpendicular part only, so the end caps don't count as lateral).
            double ux = len > 0 ? dx / len : 0, uz = len > 0 ? dz / len : 0;
            double ex = x - qx, ez = z - qz, along = ex * ux + ez * uz;
            double px = ex - along * ux, pz = ez - along * uz;
            double side = px * hx + pz * hz >= 0 ? 1 : -1;
            blat = side * Math.Sqrt(px * px + pz * pz);
        }
        return (bp, blat, bbeyond);
    }

    public static Result Run(ushort[] terrain, Spec sp, Options o)
    {
        double maxBench = sp.Points.Max(p => p.Bench);
        double reachHill = sp.HalfWidth + maxBench + o.FaceM + o.FaceTaperM, reachValley = sp.HalfWidth + o.ApronM;
        double reach = Math.Max(reachHill, reachValley) + o.EndTaperM;
        int x0 = (int)Math.Floor((sp.Points.Min(p => p.X) - reach + 7168) / CellM), x1 = (int)Math.Ceiling((sp.Points.Max(p => p.X) + reach + 7168) / CellM);
        int y0 = (int)Math.Floor((7168 - sp.Points.Max(p => p.Z) - reach) / CellM), y1 = (int)Math.Ceiling((7168 - sp.Points.Min(p => p.Z) + reach) / CellM);
        var output = (ushort[])terrain.Clone(); var zone = new bool[N * N];
        int changed = 0; double maxCut = 0, maxFill = 0; object gate = new();
        Parallel.For(Math.Max(0, y0), Math.Min(N - 1, y1) + 1, py =>
        {
            int ch = 0; double cut = 0, fill = 0;
            for (int px = Math.Max(0, x0); px <= Math.Min(N - 1, x1); px++)
            {
                double x = -7168 + CellM * (px + 0.5), z = 7168 - CellM * (py + 0.5);
                var (p, lat, beyond) = Project(sp, x, z);
                if (beyond > o.EndTaperM) continue;
                double old = Peaks.ToM(terrain[py * N + px]), delta = p.Y - p.OldY, a = Math.Abs(lat), h;
                if (a <= sp.HalfWidth) h = p.Y;
                else if (lat > 0)
                {
                    double benchEnd = sp.HalfWidth + p.Bench;
                    if (a <= benchEnd) h = p.Y;
                    else if (a <= benchEnd + o.FaceM + o.FaceTaperM) h = old + delta * (1 - SmoothStep(benchEnd + o.FaceM, benchEnd + o.FaceM + o.FaceTaperM, a));
                    else continue;
                }
                else
                {
                    if (a > sp.HalfWidth + o.ApronM) continue;
                    h = old + (p.Y - old) * (1 - SmoothStep(sp.HalfWidth, sp.HalfWidth + o.ApronM, a));
                }
                double we = 1 - SmoothStep(0, o.EndTaperM, beyond);
                h = old + we * (h - old);
                int i = py * N + px;
                zone[i] = true;
                var v = Peaks.FromM(h);
                if (v == terrain[i]) continue;
                output[i] = v; ch++;
                double d = Peaks.ToM(v) - old; cut = Math.Min(cut, d); fill = Math.Max(fill, d);
            }
            lock (gate) { changed += ch; maxCut = Math.Min(maxCut, cut); maxFill = Math.Max(maxFill, fill); }
        });
        return new Result(output, zone, changed, -maxCut, maxFill);
    }

    public static int Cli(string[] args)
    {
        const string usage = "usage: hm-rebuild regrade <playable-4096.png> --spec regrade.json [--apron 10] [--face 20] [--face-taper 20] [--end-taper 10]\n" +
                             "         [--out-dir DIR] [--name BASE] [--install DIR] [--table FILE.md]";
        if (args.Length < 2) { Console.Error.WriteLine(usage); return 2; }
        string inPath = args[1]; string? specPath = null, outDir = null, installDir = null, tablePath = null, outName = null;
        var o = new Options(); var inv = CultureInfo.InvariantCulture;
        for (int i = 2; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new InputException($"missing value for {args[i]}");
            switch (args[i])
            {
                case "--spec": specPath = Next(); break;
                case "--apron": o = o with { ApronM = double.Parse(Next(), inv) }; break;
                case "--face": o = o with { FaceM = double.Parse(Next(), inv) }; break;
                case "--face-taper": o = o with { FaceTaperM = double.Parse(Next(), inv) }; break;
                case "--end-taper": o = o with { EndTaperM = double.Parse(Next(), inv) }; break;
                case "--out-dir": outDir = Next(); break;
                case "--name": outName = Next(); break;
                case "--install": installDir = Next(); break;
                case "--table": tablePath = Next(); break;
                default: throw new InputException($"unknown option {args[i]}\n{usage}");
            }
        }
        if (specPath == null) throw new InputException("--spec is required");
        var now = DateTime.Now; var prefix = now.ToString("MMddHHmm", inv);
        outDir ??= Path.Combine("out", prefix); Directory.CreateDirectory(outDir);
        var terrain = Peaks.LoadL16(inPath, out int w, out int hgt);
        if (w != N || hgt != N) throw new InputException($"playable map must be {N}x{N}");
        var sp = Load(specPath);
        var res = Run(terrain, sp, o);
        long outside = 0;
        for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        if (outside != 0) throw new Exception($"BUG: {outside} pixels changed outside the regrade zone");

        var outBase = $"{prefix}-{outName ?? "dktflls-1to1"}";
        var pOut = Path.Combine(outDir, outBase + ".png");
        if (File.Exists(pOut)) throw new InputException($"{pOut} exists");
        Program.SaveL16(pOut, res.Output, N, N);

        // Preview crop around the regrade.
        int cx0 = (int)Math.Floor((sp.Points.Min(p => p.X) - 120 + 7168) / CellM), cx1 = (int)Math.Ceiling((sp.Points.Max(p => p.X) + 120 + 7168) / CellM);
        int cy0 = (int)Math.Floor((7168 - sp.Points.Max(p => p.Z) - 120) / CellM), cy1 = (int)Math.Ceiling((7168 - sp.Points.Min(p => p.Z) + 120) / CellM);
        Peaks.SaveSideBySide(Path.Combine(outDir, $"{outBase}_{sp.Name}_before-after.png"), terrain, res.Output, N, cx0, cy0, cx1 - cx0 + 1, cy1 - cy0 + 1, 900);

        // Table at the requested stations: spec old/new y, bench, face offset, and the terrain actually read before/after
        // on the centreline, at the bench edge and 10 m up the face.
        var sb = new StringBuilder();
        sb.AppendLine($"# Regrade {sp.Name}: {Path.GetFileName(inPath)} -> {Path.GetFileName(pOut)}\n");
        sb.AppendLine($"Produced {now:yyyy-MM-dd HH:mm}. Road band ±{sp.HalfWidth} m at y; hill bench at y; cut face shifted by (y − oldY) for {o.FaceM} m, tapered over {o.FaceTaperM} m; valley apron {o.ApronM} m; end taper {o.EndTaperM} m.");
        sb.AppendLine($"Pixels changed: {res.ChangedPx}; max cut {res.MaxCutM:F2} m; max fill {res.MaxFillM:F2} m; changed outside the zone: {outside} (asserted 0).\n");
        sb.AppendLine("| s (m) | old y | new y | Δ = cut-face offset | bench (m) | terrain centreline before → after | bench edge after | face +10 m before → after |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (double s in new double[] { -70, 0, 100, 200, 300, 400, 458 })
        {
            var p = Interp(sp, s);
            double Rd(ushort[] img, double off) => Roads.Sample(img, p.X + p.HillX * off, p.Z + p.HillZ * off);
            double edge = sp.HalfWidth + p.Bench - 0.5;
            sb.AppendLine(string.Format(inv, "| {0:F0} | {1:F2} | {2:F2} | {3:+0.00;-0.00;0.00} | {4:F1} | {5:F2} → {6:F2} | {7:F2} | {8:F2} → {9:F2} |",
                s, p.OldY, p.Y, p.Y - p.OldY, p.Bench, Rd(terrain, 0), Rd(res.Output, 0), Rd(res.Output, edge), Rd(terrain, edge + 10.5), Rd(res.Output, edge + 10.5)));
        }
        File.WriteAllText(Path.Combine(outDir, $"{outBase}_stats.md"), sb.ToString());
        if (tablePath != null) File.WriteAllText(tablePath, sb.ToString());
        Console.Write(sb.ToString());
        Console.WriteLine($"\nwrote {pOut}");
        if (installDir != null)
        {
            var dst = Path.Combine(installDir, Path.GetFileName(pOut));
            if (File.Exists(dst)) throw new InputException($"refusing to overwrite {dst}");
            File.Copy(pOut, dst); Console.WriteLine($"installed {dst}");
        }
        return 0;
    }

    static Pt Interp(Spec sp, double s)
    {
        var P = sp.Points;
        for (int k = 0; k + 1 < P.Count; k++)
        {
            if (s > P[k + 1].S && k + 2 < P.Count) continue;
            var a = P[k]; var b = P[k + 1]; double t = Math.Clamp((s - a.S) / (b.S - a.S), 0, 1);
            double hx = a.HillX + t * (b.HillX - a.HillX), hz = a.HillZ + t * (b.HillZ - a.HillZ), hl = Math.Sqrt(hx * hx + hz * hz);
            return new Pt(s, a.X + t * (b.X - a.X), a.Z + t * (b.Z - a.Z), a.OldY + t * (b.OldY - a.OldY), a.Y + t * (b.Y - a.Y), a.Bench + t * (b.Bench - a.Bench), hx / hl, hz / hl);
        }
        return P[^1];
    }
}
