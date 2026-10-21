// hm-rebuild: CS1 1081x1081 heightmap export -> CS2 1:1 playable heightmap (4096x4096, Height Scale 2048 m).
// Geometry: CS1 sample i sits at world x = -8640 + 16*i (same for z along rows, row 0 stays row 0).
// CS2 target pixel k is treated as a cell centre at x = -7168 + 3.5*(k + 0.5).
using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace HmRebuild;

public static class Program
{
    public const int SrcN = 1081;
    public const double SrcOrigin = -8640, SrcStep = 16;
    public const int DstN = 4096;
    public const double DstOrigin = -7168, DstStep = 3.5;

    public static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            if (args.Length > 0 && args[0] == "peaks") return Peaks.Run(args);
            if (args.Length > 0 && args[0] == "stamp") return Stamp.Cli(args);
            if (args.Length > 0 && args[0] == "roads") return Roads.Cli(args);
            if (args.Length > 0 && args[0] == "regrade") return Regrade.Cli(args);
            if (args.Length > 0 && args[0] == "selftest") return SelfTest.Run(keepDir: args.Length > 2 && args[1] == "--keep" ? args[2] : null);
            var o = Options.Parse(args);
            if (o == null) { Usage(); return 2; }
            Convert(o, verbose: true);
            return 0;
        }
        catch (InputException e) { Console.Error.WriteLine("error: " + e.Message); return 1; }
    }

    static void Usage() => Console.Error.WriteLine(
        "usage: hm-rebuild <cs1-export-1081.png> [--out-dir DIR] [--value-scale 0.5] [--height-scale-out 2048] [--kernel bicubic|lanczos3]\n" +
        "       hm-rebuild peaks <playable-4096.png> <worldmap-4096.png> [--out-dir DIR]\n" +
        "       hm-rebuild selftest [--keep DIR]");

    public sealed class Options
    {
        public string Input = "";
        public string OutDir = ".";
        public double ValueScale = 0.5;
        public double HeightScaleOut = 2048;
        public Kernel Kernel = Kernel.CatmullRom;

        public static Options? Parse(string[] a)
        {
            var o = new Options();
            for (int i = 0; i < a.Length; i++)
            {
                string Next() => i + 1 < a.Length ? a[++i] : throw new InputException($"missing value for {a[i]}");
                switch (a[i])
                {
                    case "--out-dir": o.OutDir = Next(); break;
                    case "--value-scale": o.ValueScale = double.Parse(Next(), CultureInfo.InvariantCulture); break;
                    case "--height-scale-out": o.HeightScaleOut = double.Parse(Next(), CultureInfo.InvariantCulture); break;
                    case "--kernel":
                        o.Kernel = Next() switch
                        {
                            "lanczos3" => Kernel.Lanczos3,
                            "bicubic" => Kernel.CatmullRom,
                            var k => throw new InputException($"unknown kernel '{k}'"),
                        };
                        break;
                    case "-h": case "--help": return null;
                    default:
                        if (a[i].StartsWith("--")) throw new InputException($"unknown option {a[i]}");
                        if (o.Input != "") throw new InputException("only one input file");
                        o.Input = a[i]; break;
                }
            }
            return o.Input == "" ? null : o;
        }
    }

    public sealed record Result(string OneToOne, string FullExtent, string Preview, ushort[] Src, ushort[] Dst, double SrcWindowP50M, double DstP50M);

    public static Result Convert(Options o, bool verbose)
    {
        var src = LoadCs1Export(o.Input);
        var stem = Path.GetFileNameWithoutExtension(o.Input);
        Directory.CreateDirectory(o.OutDir);
        var p1to1 = Path.Combine(o.OutDir, $"{stem}-1to1-4096.png");
        var pFull = Path.Combine(o.OutDir, $"{stem}-fullextent-1081-x0.5.png");
        var pPrev = Path.Combine(o.OutDir, $"{stem}-1to1-4096_preview.png");

        var dst = Rebuild(src, o.ValueScale, o.Kernel);
        var full = new ushort[src.Length];
        for (int i = 0; i < src.Length; i++) full[i] = ScaleValue(src[i], o.ValueScale);

        SaveL16(p1to1, dst, DstN, DstN);
        SaveL16(pFull, full, SrcN, SrcN);
        SavePreview(pPrev, dst, DstN, 1024);

        // Playable window of the source: samples whose centres fall inside [-7168, 7168].
        int w0 = (int)Math.Ceiling((DstOrigin - SrcOrigin) / SrcStep), w1 = (int)Math.Floor((-DstOrigin - SrcOrigin) / SrcStep);
        var window = new List<ushort>();
        for (int y = w0; y <= w1; y++) for (int x = w0; x <= w1; x++) window.Add(src[y * SrcN + x]);

        Func<double, double> inM = v => v / 64.0;                       // CS1: 65535 ~ 1024 m
        Func<double, double> outM = v => v * o.HeightScaleOut / 65535.0; // CS2: 65535 = Height Scale
        var sIn = Stats.Of(src);
        var sWin = Stats.Of(window.ToArray());
        var sOut = Stats.Of(dst, ScaleValue(65535, o.ValueScale));
        if (verbose)
        {
            Console.WriteLine($"input  {o.Input}");
            Console.WriteLine($"kernel {o.Kernel}, value scale {o.ValueScale}, output Height Scale {o.HeightScaleOut} m");
            sIn.Print("input full 1081 (17.28 km)", inM, "1024 m");
            sWin.Print($"input playable window [{w0}..{w1}]", inM, "1024 m");
            sOut.Print("output 1to1 4096 (14.336 km)", outM, $"{o.HeightScaleOut} m");
            double d = outM(sOut.P50) - inM(sWin.P50);
            Console.WriteLine($"p50 check: input window {inM(sWin.P50):F2} m vs output {outM(sOut.P50):F2} m, diff {d:+0.00;-0.00} m -> {(Math.Abs(d) <= 0.1 ? "OK" : "MISMATCH (>0.1 m)")}");
            Console.WriteLine($"wrote {p1to1}\nwrote {pFull}\nwrote {pPrev}");
        }
        return new Result(p1to1, pFull, pPrev, src, dst, inM(sWin.P50), outM(sOut.P50));
    }

    public static ushort ScaleValue(double v, double scale) =>
        (ushort)Math.Clamp(Math.Round(v * scale, MidpointRounding.AwayFromZero), 0, 65535);

    /// <summary>Continuous separable resampling of the 1081 grid onto 4096 cell centres, then value scaling.</summary>
    public static ushort[] Rebuild(ushort[] src, double valueScale, Kernel kernel)
    {
        var taps = Taps(kernel);
        // Pass 1: along x (columns) for every source row.
        var tmp = new double[SrcN * DstN];
        Parallel.For(0, SrcN, y =>
        {
            int row = y * SrcN;
            for (int k = 0; k < DstN; k++)
            {
                var (s, w) = taps[k]; double acc = 0;
                for (int j = 0; j < w.Length; j++) acc += w[j] * src[row + Math.Clamp(s + j, 0, SrcN - 1)];
                tmp[y * DstN + k] = acc;
            }
        });
        // Pass 2: along z (rows).
        var dst = new ushort[DstN * DstN];
        Parallel.For(0, DstN, k =>
        {
            var (s, w) = taps[k];
            for (int x = 0; x < DstN; x++)
            {
                double acc = 0;
                for (int j = 0; j < w.Length; j++) acc += w[j] * tmp[Math.Clamp(s + j, 0, SrcN - 1) * DstN + x];
                // Clamp to the source range before scaling so kernel overshoot near clipped peaks can never exceed 65535*scale.
                dst[k * DstN + x] = ScaleValue(Math.Clamp(acc, 0, 65535), valueScale);
            }
        });
        return dst;
    }

    /// <summary>Fractional source index of target cell k.</summary>
    public static double SrcCoord(int k) => (DstOrigin + DstStep * (k + 0.5) - SrcOrigin) / SrcStep;

    public static (int start, double[] w)[] Taps(Kernel kernel)
    {
        int a = kernel == Kernel.Lanczos3 ? 3 : 2;
        var r = new (int, double[])[DstN];
        for (int k = 0; k < DstN; k++)
        {
            double u = SrcCoord(k);
            int start = (int)Math.Floor(u) - a + 1;
            var w = new double[2 * a]; double sum = 0;
            for (int j = 0; j < w.Length; j++) { w[j] = KernelAt(kernel, u - (start + j)); sum += w[j]; }
            for (int j = 0; j < w.Length; j++) w[j] /= sum;
            r[k] = (start, w);
        }
        return r;
    }

    static double KernelAt(Kernel k, double x)
    {
        x = Math.Abs(x);
        if (k == Kernel.CatmullRom)
            return x < 1 ? 1.5 * x * x * x - 2.5 * x * x + 1 : x < 2 ? -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2 : 0;
        if (x < 1e-12) return 1;
        if (x >= 3) return 0;
        double px = Math.PI * x;
        return 3 * Math.Sin(px) * Math.Sin(px / 3) / (px * px);
    }

    public static ushort[] LoadCs1Export(string path)
    {
        if (!File.Exists(path)) throw new InputException($"not found: {path}");
        var info = Image.Identify(path);
        var meta = info.Metadata.GetPngMetadata();
        if (info.Width != SrcN || info.Height != SrcN)
            throw new InputException($"expected {SrcN}x{SrcN}, got {info.Width}x{info.Height} (CS1 map-editor heightmap export only)");
        if (meta.BitDepth != PngBitDepth.Bit16)
            throw new InputException($"expected a 16-bit PNG, got bit depth {meta.BitDepth}");
        var data = new ushort[SrcN * SrcN];
        switch (meta.ColorType)
        {
            case PngColorType.Grayscale:
            case PngColorType.GrayscaleWithAlpha:
                using (var img = Image.Load<L16>(path))
                    img.ProcessPixelRows(acc => { for (int y = 0; y < SrcN; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < SrcN; x++) data[y * SrcN + x] = r[x].PackedValue; } });
                break;
            case PngColorType.Rgb:
            case PngColorType.RgbWithAlpha:
                using (var img = Image.Load<Rgba64>(path)) // red channel
                    img.ProcessPixelRows(acc => { for (int y = 0; y < SrcN; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < SrcN; x++) data[y * SrcN + x] = r[x].R; } });
                break;
            default: throw new InputException($"unsupported PNG colour type {meta.ColorType}");
        }
        return data;
    }

    public static void SaveL16(string path, ushort[] data, int w, int h)
    {
        using var img = new Image<L16>(w, h);
        img.ProcessPixelRows(acc => { for (int y = 0; y < h; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < w; x++) r[x] = new L16(data[y * w + x]); } });
        img.Save(path, new PngEncoder { BitDepth = PngBitDepth.Bit16, ColorType = PngColorType.Grayscale });
    }

    static void SavePreview(string path, ushort[] data, int n, int size)
    {
        int f = n / size; var box = new double[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                double s = 0;
                for (int dy = 0; dy < f; dy++) for (int dx = 0; dx < f; dx++) s += data[(y * f + dy) * n + x * f + dx];
                box[y * size + x] = s / (f * f);
            }
        double min = box.Min(), range = Math.Max(1, box.Max() - min);
        using var img = new Image<L8>(size, size);
        img.ProcessPixelRows(acc => { for (int y = 0; y < size; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < size; x++) r[x] = new L8((byte)Math.Round((box[y * size + x] - min) / range * 255)); } });
        img.Save(path);
    }
}

public enum Kernel { Lanczos3, CatmullRom }

public sealed class InputException(string m) : Exception(m);

public sealed record Stats(int Min, int Max, double Mean, int P1, int P50, int P99, int Ceiling, double PctAtCeiling)
{
    public static Stats Of(ushort[] d, int ceiling = 65535)
    {
        var hist = new long[65536]; double sum = 0;
        foreach (var v in d) { hist[v]++; sum += v; }
        int Pct(double p) { long t = (long)Math.Ceiling(p * d.Length), c = 0; for (int i = 0; i < 65536; i++) { c += hist[i]; if (c >= t) return i; } return 65535; }
        int min = 0; while (hist[min] == 0) min++;
        int max = 65535; while (hist[max] == 0) max--;
        return new Stats(min, max, sum / d.Length, Pct(0.01), Pct(0.5), Pct(0.99), ceiling, 100.0 * hist[ceiling] / d.Length);
    }

    public void Print(string label, Func<double, double> m, string scale)
    {
        Console.WriteLine($"{label}:");
        Console.WriteLine($"  u16   min={Min} max={Max} mean={Mean:F1} p1={P1} p50={P50} p99={P99} at{Ceiling}={PctAtCeiling:F3}%");
        Console.WriteLine($"  m@{scale}  min={m(Min):F2} max={m(Max):F2} mean={m(Mean):F2} p1={m(P1):F2} p50={m(P50):F2} p99={m(P99):F2}");
    }
}
