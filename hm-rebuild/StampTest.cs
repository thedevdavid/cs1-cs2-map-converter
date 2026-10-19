using System.Text;

namespace HmRebuild;

/// <summary>Synthetic fixture for `stamp`: two domes (one above 1024 m), a thin tall wall and a falls block on a gentle plane,
/// as per-layer rasters in the 4096 playable frame (mountain, rock work, falls steps), a 1081 fallback layer, and the
/// 1081 JSON/raster pair of the first extractor version.</summary>
public static class StampTest
{
    const int N = Peaks.N, S = Program.SrcN;
    static double Sq(double v) => v * v;

    static double Terrain(double x, double z) => 900 + 0.005 * x + 0.002 * z;

    // PO tops (absolute metres), NaN where the object does not cover the point.
    static double DomeA(double x, double z) { double r2 = Sq(x + 3000) + Sq(z + 2000), R = 500; return r2 < R * R ? Terrain(x, z) + 250 * (1 - r2 / (R * R)) : double.NaN; } // top ~1135 m
    static double DomeB(double x, double z) { double r2 = Sq(x - 2500) + Sq(z - 1500), R = 300; return r2 < R * R ? Terrain(x, z) + 120 * (1 - r2 / (R * R)) : double.NaN; }
    static double Wall(double x, double z) => Math.Abs(x - 500) < 16 && Math.Abs(z - 3000) < 240 ? Terrain(x, z) + 80 : double.NaN;               // 32 m thick, 80 m tall
    // Terraced cone: 10 m staircase steps between covered pixels (what a 16 m cell max leaves on a flank).
    static double Terraced(double x, double z) { double r = Math.Sqrt(Sq(x - 4000) + Sq(z + 4000)), R = 250; return r < R ? Terrain(x, z) + Math.Floor((R - r) * 0.6 / 10) * 10 + 5 : double.NaN; }
    static double Falls(double x, double z) => Math.Abs(x + 1000) < 60 && Math.Abs(z - 500) < 40 ? Terrain(x, z) + 30 : double.NaN;

    static double PxCentre(int k) => -7168 + 3.5 * (k + 0.5);          // x of column k; z of row k is the negative (row 0 north)
    static int Px(double wx) => (int)Math.Round((wx + 7168) / 3.5 - 0.5);
    static int Row(double wz) => (int)Math.Round((7168 - wz) / 3.5 - 0.5);

    static void Write4096(string path, params Func<double, double, double>[] fs)
    {
        var d = new ushort[N * N];
        Parallel.For(0, N, r =>
        {
            for (int c = 0; c < N; c++)
            {
                double x = PxCentre(c), z = -PxCentre(r), best = double.NaN;
                foreach (var f in fs) { double t = f(x, z); if (!double.IsNaN(t) && (double.IsNaN(best) || t > best)) best = t; }
                if (!double.IsNaN(best)) d[r * N + c] = (ushort)Math.Clamp(Math.Round(best * 32), 1, 65535);
            }
        });
        Program.SaveL16(path, d, N, N);
    }

    public static void Run(Action<string, bool, string> check, string dir)
    {
        var terrain = new ushort[N * N];
        for (int r = 0; r < N; r++) for (int c = 0; c < N; c++) terrain[r * N + c] = Peaks.FromM(Terrain(PxCentre(c), -PxCentre(r)));

        // --- v2 layout: per-layer rasters in the 4096 playable frame ---
        var v2 = Path.Combine(dir, "v2"); Directory.CreateDirectory(v2);
        Write4096(Path.Combine(v2, "po-heights-mountain-4096.png"), DomeA, DomeB);
        Write4096(Path.Combine(v2, "po-heights-rock-work-4096.png"), Wall, Terraced);
        Write4096(Path.Combine(v2, "po-heights-falls-steps-4096.png"), Falls);
        var o = new Stamp.Options();
        var log = new List<string>();
        var po = Stamp.FromLayers(v2, ["mountain", "rock work"], o, log);
        var res = Stamp.Run(terrain, po, o);

        long outside = 0;
        for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        check("stamp: bit-identical outside footprints + falloff", outside == 0, $"{outside} differ outside; {res.ChangedPixels} changed; farthest {res.MaxChangedDistPx} px");
        check("stamp: falloff margin <= 16 px", res.MaxChangedDistPx <= 16, $"{res.MaxChangedDistPx} px");
        check("stamp: 4096 layers used as is", log.Count == 2 && log.All(l => l.Contains("4096 frame")), string.Join(" | ", log));

        double TopAt(ushort[] img, double wx, double wz) => Peaks.ToM(img[Row(wz) * N + Px(wx)]);
        double a = TopAt(res.Output, -3000, -2000), aExp = Terrain(-3000, -2000) + 250;
        check("stamp: dome A top (4096 layer, > 1024 m)", Math.Abs(a - aExp) < 0.2 && a > 1024, $"{a:F2} m vs {aExp:F2} m");
        double b = TopAt(res.Output, 2500, 1500), bExp = Terrain(2500, 1500) + 120;
        check("stamp: dome B top", Math.Abs(b - bExp) < 0.2, $"{b:F2} m vs {bExp:F2} m");
        check("stamp: falls not in the default layers", Enumerable.Range(Row(540), Row(460) - Row(540) + 1).All(r => Enumerable.Range(Px(-1060), Px(-940) - Px(-1060) + 1).All(c => res.Output[r * N + c] == terrain[r * N + c])),
            "falls area unchanged with mountain + rock work");

        // Falloff: from the dome rim outwards the surface decreases monotonically to the terrain (no lip / outline step).
        int rimRow = Row(-2000); double prev = double.MaxValue, worstRise = 0; var prof = new List<string>();
        for (int c = Px(-3000 + 480); c <= Px(-3000 + 560); c++) { double v = Peaks.ToM(res.Output[rimRow * N + c]) - Terrain(PxCentre(c), -PxCentre(rimRow)); worstRise = Math.Max(worstRise, v - prev); prev = v; prof.Add(v.ToString("F2")); }
        check("stamp: falloff monotonic outside dome A rim", worstRise <= 0.05, $"largest rise outward {worstRise:F3} m; profile {string.Join(" ", prof)}");

        // Step smoothing: the 10 m terraces of the cone are softened; without smoothing they stay single-pixel steps.
        var resNoSmooth = Stamp.Run(terrain, po, o with { SmoothSigmaPx = 0 });
        double MaxStep(ushort[] img) { double m = 0; int r = Row(-4000); for (int c = Px(3800); c < Px(4000); c++) m = Math.Max(m, Math.Abs(Peaks.ToM(img[r * N + c + 1]) - Peaks.ToM(img[r * N + c]))); return m; }
        double sm = MaxStep(res.Output), raw = MaxStep(resNoSmooth.Output);
        check("stamp: step smoothing softens terraces", sm < 0.6 * raw && res.SmoothedPx > 0, $"max step {sm:F1} m vs {raw:F1} m without smoothing; {res.SmoothedPx} px smoothed");
        bool domeUntouched = Math.Abs(TopAt(res.Output, -3000, -2000) - TopAt(resNoSmooth.Output, -3000, -2000)) < 0.05;
        check("stamp: smoothing leaves gentle relief alone", domeUntouched, "dome A top identical with and without smoothing");

        // Softened variant: same zone, nothing outside it changes; dome top stays close (blur + 30 % raw near ridges + noise
        // <= 6 m); the terraces lose their steps; noise makes the plain dome flank non-uniform.
        var resS = Stamp.Run(terrain, po, o with { Soft = new Stamp.Soften() });
        long outsideS = 0;
        for (int i = 0; i < terrain.Length; i++) if (!resS.Zone[i] && resS.Output[i] != terrain[i]) outsideS++;
        check("soften: bit-identical outside footprints + falloff", outsideS == 0 && resS.MaxChangedDistPx <= 16, $"{outsideS} differ outside; farthest {resS.MaxChangedDistPx} px");
        double aS = TopAt(resS.Output, -3000, -2000);
        check("soften: dome A top kept within 8 m", Math.Abs(aS - aExp) < 8, $"{aS:F2} m vs {aExp:F2} m");
        double smS = MaxStep(resS.Output);
        check("soften: terraces smoothed further", smS < sm, $"max step {smS:F2} m vs {sm:F2} m (step smoothing only)");
        double maxDev = 0;
        for (int c = Px(-3000 + 100); c < Px(-3000 + 400); c++) maxDev = Math.Max(maxDev, Math.Abs(Peaks.ToM(resS.Output[rimRow * N + c]) - Peaks.ToM(res.Output[rimRow * N + c])));
        check("soften: flank noise present and bounded", maxDev > 0.5 && maxDev < 9, $"max |soft - plain| on dome A flank {maxDev:F2} m");

        // Falls as its own layer.
        var poF = Stamp.FromLayers(v2, ["falls"], o, []);
        var resF = Stamp.Run(terrain, poF, o);
        double f = TopAt(resF.Output, -1000, 500), fExp = Terrain(-1000, 500) + 30;
        check("stamp: --layer falls stamps only the falls", Math.Abs(f - fExp) < 0.2 && resF.Output[Row(-2000) * N + Px(-3000)] == terrain[Row(-2000) * N + Px(-3000)], $"falls top {f:F2} m vs {fExp:F2} m; dome A untouched");

        // Optional slope limit still bounds slopes.
        var resL = Stamp.Run(terrain, po, o with { MaxSlope = 1.2, SmoothSigmaPx = 0 });
        double worst = 0;
        for (int y = 1; y < N; y++) for (int x = 1; x < N; x++)
            {
                int i = y * N + x; if (!resL.Zone[i]) continue;
                worst = Math.Max(worst, Math.Max(Math.Abs(Peaks.ToM(resL.Output[i]) - Peaks.ToM(resL.Output[i - 1])), Math.Abs(Peaks.ToM(resL.Output[i]) - Peaks.ToM(resL.Output[i - N]))) / 3.5);
            }
        check("stamp: --max-slope 1.2 bounds slopes", worst <= 1.2 + 0.02, $"steepest neighbour step {worst:F3} m/m");

        // --- 1081 fallback layer (no 4096 file for it) ---
        var v1081 = Path.Combine(dir, "v1081"); Directory.CreateDirectory(v1081);
        var r1081 = new ushort[S * S];
        for (int r = 0; r < S; r++) for (int c = 0; c < S; c++) { double t = DomeB(-8640 + 16.0 * c, 8640 - 16.0 * r); if (!double.IsNaN(t)) r1081[r * S + c] = (ushort)Math.Round(t * 32); }
        Program.SaveL16(Path.Combine(v1081, "mountain-1081.png"), r1081, S, S);
        var log2 = new List<string>();
        var resB = Stamp.Run(terrain, Stamp.FromLayers(v1081, ["mountain"], o with { Po1081UnitsPerM = 32 }, log2), o);
        double b2 = TopAt(resB.Output, 2500, 1500);
        check("stamp: 1081 layer fallback (resampled)", log2[0].Contains("1081 frame") && Math.Abs(b2 - bExp) < 1.5, $"dome B top {b2:F2} m vs {bExp:F2} m");

        // --- v1 layout: 1081 JSON with object ids ---
        var sb = new StringBuilder("{\"version\":1,\"cells\":[");
        var raster = new ushort[S * S]; bool first = true;
        for (int r = 0; r < S; r++)
            for (int c = 0; c < S; c++)
            {
                double x = -8640 + 16.0 * c, z = 8640 - 16.0 * r, best = 0;
                foreach (var (id, fn) in new (string, Func<double, double, double>)[] { ("dome-a", DomeA), ("dome-b", DomeB), ("wall-1", Wall) })
                {
                    double t = fn(x, z); if (double.IsNaN(t)) continue;
                    sb.Append(first ? "" : ",").Append(FormattableString.Invariant($"[{r * S + c},{t:F3},{Terrain(x, z) - 5:F3},\"{id}\"]")); first = false;
                    best = Math.Max(best, t);
                }
                if (best > 0) raster[r * S + c] = (ushort)Math.Clamp(Math.Round(best * 64), 1, 65535);
            }
        sb.Append("]}");
        var json = Path.Combine(dir, "po-offline-heights.json"); File.WriteAllText(json, sb.ToString());
        var rasterPath = Path.Combine(dir, "po-offline-heights.png"); Program.SaveL16(rasterPath, raster, S, S);
        var gJ = Stamp.LoadPo(rasterPath, json, o with { Ids = ["dome-a", "wall-1"] });
        var resJ = Stamp.Run(terrain, Stamp.From1081(gJ, o.Kernel, o.StepM, "json"), o);
        double aJ = TopAt(resJ.Output, -3000, -2000);
        bool bUntouched = resJ.Output[Row(1500) * N + Px(2500)] == terrain[Row(1500) * N + Px(2500)];
        check("stamp: 1081 JSON path with id filter", Math.Abs(aJ - aExp) < 1.5 && bUntouched && resJ.Keys.Any(k => k.Key == "dome-a" && k.StampedPx > 0),
            $"dome A {aJ:F2} m vs {aExp:F2} m; dome-b filtered out");
        var gR = Stamp.LoadPo(rasterPath, null, o);
        check("stamp: 1081 raster flags saturation", gR.SaturatedCells > 0, $"{gR.SaturatedCells} saturated cells");
    }
}
