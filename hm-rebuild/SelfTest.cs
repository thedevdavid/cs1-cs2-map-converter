using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace HmRebuild;

/// <summary>Synthetic tests: analytic surface sampled on the CS1 grid, converted end-to-end through PNG files.</summary>
public static class SelfTest
{
    // Metres. Tilted plane + broad Gaussian hill + a narrow tall hill that exceeds 1024 m and clips at 65535.
    public static double Height(double x, double z) =>
        100 + 0.02 * (x + 8640) + 0.01 * (z + 8640)
        + 500 * Math.Exp(-(Sq(x - 1000) + Sq(z + 2000)) / (2 * Sq(1500)))
        + 900 * Math.Exp(-(Sq(x + 4000) + Sq(z - 3000)) / (2 * Sq(400)));
    static double Sq(double v) => v * v;

    static int failures;

    public static int Run(string? keepDir = null)
    {
        var dir = keepDir ?? Path.Combine(Path.GetTempPath(), "hm-rebuild-selftest-" + Environment.ProcessId);
        Directory.CreateDirectory(dir);
        try
        {
            GeometryConstants();
            AnalyticSurface(dir, Kernel.CatmullRom, strict: true);
            AnalyticSurface(dir, Kernel.Lanczos3, strict: false); // informational: Lanczos-3 does not reproduce steep slopes exactly
            RgbInputUsesRedChannel(dir);
            RejectsWrongSize(dir);
            RejectsEightBit(dir);
            OutputNeverExceedsHalfCeiling(dir);
            PeaksBitIdenticalOutsideZone();
            PeaksVariantV2b();
            StampTest.Run(Check, dir);
        }
        finally { if (keepDir == null) Directory.Delete(dir, true); }
        Console.WriteLine(failures == 0 ? "ALL TESTS PASSED" : $"{failures} TEST(S) FAILED");
        return failures == 0 ? 0 : 1;
    }

    static void Check(string name, bool ok, string detail)
    {
        Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {name}: {detail}");
        if (!ok) failures++;
    }

    static void GeometryConstants()
    {
        // Target cell 0 centre: x = -7168 + 1.75 = -7166.25 -> source index (1473.75/16) = 92.109375
        double u0 = Program.SrcCoord(0), uN = Program.SrcCoord(4095);
        Check("geometry", Math.Abs(u0 - 92.109375) < 1e-12 && Math.Abs(uN - 987.890625) < 1e-12,
            $"cell 0 -> src {u0}, cell 4095 -> src {uN} (Lanczos-3 support {Math.Floor(u0) - 2}..{Math.Floor(uN) + 3} inside 0..1080)");
    }

    static string WriteSynthetic(string dir, string name, Func<double, double, double> h)
    {
        var data = new ushort[1081 * 1081];
        for (int r = 0; r < 1081; r++)
            for (int c = 0; c < 1081; c++)
                data[r * 1081 + c] = (ushort)Math.Clamp(Math.Round(h(-8640 + 16.0 * c, -8640 + 16.0 * r) * 64), 0, 65535);
        var p = Path.Combine(dir, name + ".png");
        Program.SaveL16(p, data, 1081, 1081);
        return p;
    }

    static ushort[] LoadL16(string path, out int w)
    {
        using var img = Image.Load<L16>(path);
        int ww = w = img.Width; var d = new ushort[img.Width * img.Height];
        img.ProcessPixelRows(acc => { for (int y = 0; y < acc.Height; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < ww; x++) d[y * ww + x] = r[x].PackedValue; } });
        return d;
    }

    static void AnalyticSurface(string dir, Kernel kernel, bool strict)
    {
        var input = WriteSynthetic(dir, "synthetic-" + kernel, Height);
        var res = Program.Convert(new Program.Options { Input = input, OutDir = dir, Kernel = kernel }, verbose: false);
        var outImg = LoadL16(res.OneToOne, out int w);
        Check($"{kernel}: output size", w == 4096 && outImg.Length == 4096 * 4096, $"{w}x{outImg.Length / Math.Max(w, 1)}");

        // Exclude target cells whose kernel support touches a clipped (65535) source sample.
        var taps = Program.Taps(kernel);
        double maxErr = 0; int worstX = 0, worstZ = 0; long excluded = 0;
        for (int kz = 0; kz < 4096; kz++)
        {
            var (sz, wz) = taps[kz];
            for (int kx = 0; kx < 4096; kx++)
            {
                var (sx, wx) = taps[kx];
                bool clipped = false;
                for (int j = 0; j < wz.Length && !clipped; j++)
                    for (int i = 0; i < wx.Length && !clipped; i++)
                        clipped = res.Src[(sz + j) * 1081 + sx + i] == 65535;
                if (clipped) { excluded++; continue; }
                double expected = Height(-7168 + 3.5 * (kx + 0.5), -7168 + 3.5 * (kz + 0.5)) * 64 * 0.5;
                double err = Math.Abs(outImg[kz * 4096 + kx] - expected);
                if (err > maxErr) { maxErr = err; worstX = kx; worstZ = kz; }
            }
        }
        Check($"{kernel}: analytic surface{(strict ? "" : " (info only)")}", !strict || maxErr < 1.0,
            $"max |out - expected| = {maxErr:F3} u16 at ({worstX},{worstZ}); {excluded} cells near clipping excluded");
        Check($"{kernel}: clipped region present", excluded > 0, $"{excluded} cells excluded");
        Check($"{kernel}: p50 metres", Math.Abs(res.DstP50M - res.SrcWindowP50M) <= 0.1,
            $"input window {res.SrcWindowP50M:F3} m vs output {res.DstP50M:F3} m");

        // Full-extent reference: exact halving of every sample.
        var full = LoadL16(res.FullExtent, out int fw);
        bool fullOk = fw == 1081;
        for (int i = 0; i < full.Length && fullOk; i++) fullOk = full[i] == Program.ScaleValue(res.Src[i], 0.5);
        Check($"{kernel}: fullextent x0.5", fullOk, "1081 grid = round(src*0.5)");
        var prev = Image.Identify(res.Preview);
        Check($"{kernel}: preview", prev.Width == 1024 && prev.Height == 1024, $"{prev.Width}x{prev.Height}");
    }

    static void RgbInputUsesRedChannel(string dir)
    {
        using var img = new Image<Rgba64>(1081, 1081);
        img.ProcessPixelRows(acc => { for (int y = 0; y < 1081; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < 1081; x++) r[x] = new Rgba64((ushort)(x * 60), 1234, 4321, 65535); } });
        var p = Path.Combine(dir, "rgb.png");
        img.Save(p, new PngEncoder { BitDepth = PngBitDepth.Bit16, ColorType = PngColorType.Rgb });
        var d = Program.LoadCs1Export(p);
        Check("rgb16 input", d[500] == 30000 && d[1081 * 7 + 1000] == 60000, $"red channel read ({d[500]}, {d[1081 * 7 + 1000]})");
    }

    static void RejectsWrongSize(string dir)
    {
        var p = Path.Combine(dir, "wrong.png");
        Program.SaveL16(p, new ushort[1080 * 1080], 1080, 1080);
        Check("rejects 1080x1080", Throws(() => Program.LoadCs1Export(p)), "InputException");
    }

    static void RejectsEightBit(string dir)
    {
        using var img = new Image<L8>(1081, 1081);
        var p = Path.Combine(dir, "eight.png");
        img.Save(p, new PngEncoder { BitDepth = PngBitDepth.Bit8, ColorType = PngColorType.Grayscale });
        Check("rejects 8-bit", Throws(() => Program.LoadCs1Export(p)), "InputException");
    }

    static void OutputNeverExceedsHalfCeiling(string dir)
    {
        // Checkerboard of 0 / 65535 maximises Lanczos overshoot.
        var input = WriteSynthetic(dir, "checker", (x, z) => ((int)((x + 8640) / 16) + (int)((z + 8640) / 16)) % 2 == 0 ? 1100 : 0);
        var res = Program.Convert(new Program.Options { Input = input, OutDir = dir }, verbose: false);
        int max = res.Dst.Max(v => (int)v);
        Check("output <= 32768", max <= 32768, $"max output {max} on 0/65535 checkerboard");
    }

    static void PeaksBitIdenticalOutsideZone()
    {
        // 4096 map at 2048 m scale: a slope plus two hills clipped at 1024 m (one running off the west edge), plus noise
        // so every pixel is distinct. Peaks must only touch components + 12 px, rise above the plateau, stay under the cap.
        const int n = Peaks.N;
        var src = new ushort[n * n];
        var rng = new Random(7);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double m = 200 + 0.02 * x * 3.5
                    + 900 * Math.Exp(-(Sq(x - 2000) + Sq(y - 1500)) / (2 * Sq(150)))
                    + 950 * Math.Exp(-(Sq(x - 10) + Sq(y - 3000)) / (2 * Sq(200)))
                    + rng.NextDouble();
                src[y * n + x] = Peaks.FromM(Math.Min(m, Peaks.PlateauM));
            }
        var res = Peaks.Rebuild(src, n);
        long outside = 0;
        for (int i = 0; i < src.Length; i++) if (!res.ChangeZone[i] && res.Output[i] != src[i]) outside++;
        Check("peaks: bit-identical outside components + margin", outside == 0, $"{outside} pixels differ outside; {res.ChangedPixels} changed inside");
        Check("peaks: margin <= 12 px", res.MaxChangedDistPx <= Peaks.MarginPx, $"farthest changed pixel {res.MaxChangedDistPx} px");
        var big = res.Components.Where(c => c.Pixels > 1000).ToList();
        Check("peaks: two clipped hills found", big.Count == 2, string.Join(", ", res.Components.Select(c => c.Pixels)));
        bool raised = big.All(c => c.PeakM > Peaks.PlateauM + 5 && c.PeakM <= c.CapM + 1e-6 && c.PeakM <= Peaks.AbsoluteCapM);
        Check("peaks: domes above plateau, under cap", raised, string.Join("; ", big.Select(c => $"R={c.RadiusPx:F1}px s={c.RimSlope:F3} peak={c.PeakM:F1} cap={c.CapM:F1}")));
        int flat = 0, clipped = 0; for (int i = 0; i < src.Length; i++) if (src[i] >= Peaks.Ceiling) { clipped++; if (res.Output[i] == Peaks.Ceiling) flat++; }
        Check("peaks: no flat top", flat <= clipped / 200, $"{flat} of {clipped} clipped pixels still at 1024 m (rim pixels only)");
        var west = res.Components.First(c => c.X0 == 0);
        int edgeMax = 0; for (int y = west.Y0; y <= west.Y1; y++) edgeMax = Math.Max(edgeMax, res.Output[y * n]);
        double rise = west.PeakM - Peaks.PlateauM, edgeRise = Peaks.ToM(edgeMax) - Peaks.PlateauM;
        Check("peaks: map edge stays near plateau", edgeRise < 0.05 * rise, $"edge x=0 rises {edgeRise:F2} m vs dome rise {rise:F1} m");
    }

    static void PeaksVariantV2b()
    {
        // v2b parameters: peak factor 4, cap factor 5, lift starting 12 px outside, variant only for components >= 1000 px.
        const int n = Peaks.N;
        var src = new ushort[n * n];
        var rng = new Random(11);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double m = 300 + 0.02 * x * 3.5 + 800 * Math.Exp(-(Sq(x - 1200) + Sq(y - 900)) / (2 * Sq(120))) + rng.NextDouble();
                if (x >= 3000 && x < 3003 && y >= 3000 && y < 3003) m = 2000; // tiny 9 px component, stays on the v2 profile
                src[y * n + x] = Peaks.FromM(Math.Min(m, Peaks.PlateauM));
            }
        var prm = new Peaks.Params(PeakFactor: 4, CapFactor: 5, StartOutsidePx: 12, MinVariantPixels: 1000);
        var res = Peaks.Rebuild(src, n, prm);
        long outside = 0;
        for (int i = 0; i < src.Length; i++) if (!res.ChangeZone[i] && res.Output[i] != src[i]) outside++;
        Check("v2b: bit-identical outside components + margin", outside == 0, $"{outside} pixels differ outside; {res.ChangedPixels} changed");
        Check("v2b: margin <= 12 px", res.MaxChangedDistPx <= Peaks.MarginPx, $"farthest changed pixel {res.MaxChangedDistPx} px");
        var big = res.Components[0]; var tiny = res.Components.Last();
        double expected = Math.Min(Peaks.PlateauM + 4 * big.RadiusPx * Peaks.CellM * big.RimSlope, big.CapM);
        Check("v2b: big component peak = plateau + 4 R s (within noise)", Math.Abs(big.PeakM - expected) < 6 && big.PeakM <= big.CapM + 1e-6,
            $"peak {big.PeakM:F1} m, expected {expected:F1} m, cap {big.CapM:F1} m, profile {big.Profile}");
        Check("v2b: small component keeps the v2 profile", tiny.Profile.StartsWith("v2") && tiny.Pixels == 9, $"{tiny.Pixels} px, {tiny.Profile}");
        // The lift starts outside: a pixel 6 px outside the plateau is raised, one 13 px outside is not.
        int raised6 = 0, moved13 = 0;
        var dOut = Peaks.Edt(n, n, i => src[i] >= Peaks.Ceiling);
        for (int i = 0; i < src.Length; i++)
        {
            if (Math.Abs(dOut[i] - 6) < 0.5 && res.Output[i] > src[i]) raised6++;
            if (dOut[i] > 12.5 && res.Output[i] != src[i]) moved13++;
        }
        Check("v2b: lift starts outside the plateau", raised6 > 100 && moved13 == 0, $"{raised6} pixels raised at ~6 px out, {moved13} changed beyond 12 px");
    }

    static bool Throws(Action a) { try { a(); return false; } catch (InputException) { return true; } }
}
