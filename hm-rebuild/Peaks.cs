// peaks: rebuild the clipped CS1 summits (1024 m plateaus) of a 1:1 playable heightmap at Height Scale 2048 m.
// Components = 8-connected pixels at the output ceiling (round(65535*0.5) = 32768). Each gets a smooth dome that
// continues the rim slope inward. Pixels farther than MarginPx from every component are copied bit for bit.
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HmRebuild;

public static class Peaks
{
    public const int N = 4096;
    public const double CellM = 3.5;
    public const double ScaleM = 2048;
    public const ushort Ceiling = 32768;          // 1024 m plateau after the x0.5 rebuild
    public const double PlateauM = Ceiling * ScaleM / 65535.0;
    public const int MarginPx = 12;               // max blend distance outside a component
    public const double AbsoluteCapM = 1900;
    public const double CapFactor = 2.5;          // peak <= plateau + CapFactor * R * s
    public const double PeakFactor = 1.5;         // dome height = PeakFactor * R * s; steepest interior slope ~2 s
    public const int RimRingMinPx = 12, RimRingMaxPx = 30;
    public const double NoiseAmpM = 4;

    /// <summary>Variant parameters. Defaults reproduce v2. Components smaller than MinVariantPixels always use the v2 profile.</summary>
    public sealed record Params(double PeakFactor = PeakFactor, double CapFactor = CapFactor, int StartOutsidePx = 0, int MinVariantPixels = 0)
    {
        public bool UsesVariant(int pixels) => StartOutsidePx > 0 && pixels >= MinVariantPixels;
    }

    public sealed record Component(int Id, int Pixels, double RadiusPx, double RimSlope, int RimSamples, double EdgeSlope, double PeakM, double CapM, string Profile, int X0, int Y0, int X1, int Y1);

    public sealed record Result(ushort[] Output, bool[] ChangeZone, List<Component> Components, int ChangedPixels, int MaxChangedDistPx);

    public static double ToM(double v) => v * ScaleM / 65535.0;
    public static ushort FromM(double m) => (ushort)Math.Clamp(Math.Round(m * 65535.0 / ScaleM, MidpointRounding.AwayFromZero), 0, 65535);

    public static Result Rebuild(ushort[] src, int n, Params? prm = null, int seed = 1)
    {
        prm ??= new Params();
        var mask = new bool[n * n];
        for (int i = 0; i < mask.Length; i++) mask[i] = src[i] >= Ceiling;
        var label = new int[n * n];
        var comps = Label(mask, n, label);

        // Best candidate per pixel (max over components), as a metre value; NaN = untouched.
        var target = new double[n * n]; Array.Fill(target, double.NaN);
        var blendW = new double[n * n];     // smoothing weight
        var noiseAmp = new double[n * n];
        var capAt = new double[n * n];
        var zone = new bool[n * n];
        var results = new List<Component>();

        foreach (var c in comps)
        {
            // Window around the component; it may reach one pixel past the image so the map edge acts as a dome boundary
            // (a plateau running off the map returns to the plateau height there, matching the world-map exterior).
            int pad = RimRingMaxPx + 2;
            int x0 = Math.Max(-1, c.x0 - pad), y0 = Math.Max(-1, c.y0 - pad), x1 = Math.Min(n, c.x1 + pad), y1 = Math.Min(n, c.y1 + pad);
            int w = x1 - x0 + 1, h = y1 - y0 + 1;
            var inC = new bool[w * h];
            foreach (var p in c.pixels) inC[(p / n - y0) * w + p % n - x0] = true;
            // Distance (px, centre to centre) to the nearest pixel of the other class.
            var dIn = Edt(w, h, i => !inC[i]);
            var dOut = Edt(w, h, i => inC[i]);

            double radiusPx = 0;
            foreach (var p in c.pixels) radiusPx = Math.Max(radiusPx, dIn[(p / n - y0) * w + p % n - x0] - 0.5);
            radiusPx = Math.Max(radiusPx, 0.5);
            double rM = radiusPx * CellM;

            // Rim slope: mean gradient magnitude of the source on the ring RimRingMinPx..RimRingMaxPx outside. Closer in,
            // the slope is flattened by the bicubic support of the clipped samples (<= 9 px) and by CS1's own 1024 m clamp.
            double sum = 0; int cnt = 0;
            for (int yy = 0; yy < h; yy++)
                for (int xx = 0; xx < w; xx++)
                {
                    double d = dOut[yy * w + xx];
                    if (d < RimRingMinPx || d > RimRingMaxPx) continue;
                    int gx = x0 + xx, gy = y0 + yy;
                    if (gx < 1 || gy < 1 || gx > n - 2 || gy > n - 2) continue;
                    double dx = (ToM(src[gy * n + gx + 1]) - ToM(src[gy * n + gx - 1])) / (2 * CellM);
                    double dz = (ToM(src[(gy + 1) * n + gx]) - ToM(src[(gy - 1) * n + gx])) / (2 * CellM);
                    sum += Math.Sqrt(dx * dx + dz * dz); cnt++;
                }
            // Slope right at the rim (1..4 px): the dome starts with this slope so there is no crease along the old outline.
            double sumN = 0; int cntN = 0;
            for (int yy = 0; yy < h; yy++)
                for (int xx = 0; xx < w; xx++)
                {
                    double d = dOut[yy * w + xx];
                    if (d < 1 || d > 4) continue;
                    int gx = x0 + xx, gy = y0 + yy;
                    if (gx < 1 || gy < 1 || gx > n - 2 || gy > n - 2) continue;
                    double dx = (ToM(src[gy * n + gx + 1]) - ToM(src[gy * n + gx - 1])) / (2 * CellM);
                    double dz = (ToM(src[(gy + 1) * n + gx]) - ToM(src[(gy - 1) * n + gx])) / (2 * CellM);
                    sumN += Math.Sqrt(dx * dx + dz * dz); cntN++;
                }
            double s = cnt > 0 ? sum / cnt : 0.3;
            s = Math.Clamp(s, 0.05, 2.0);
            bool variant = prm.UsesVariant(c.pixels.Count);
            double capF = variant ? prm.CapFactor : CapFactor, peakF = variant ? prm.PeakFactor : PeakFactor;
            double cap = Math.Min(PlateauM + capF * rM * s, AbsoluteCapM);
            double peak = Math.Min(PlateauM + peakF * rM * s, cap);
            double k = (peak - PlateauM) / (rM * s);           // effective peak factor after the cap
            double sEdge = cntN > 0 ? sumN / cntN : s;
            double c0 = Math.Clamp(sEdge / s, 0, 1);            // starting slope as a fraction of s
            double a = 3 * k - 2 * c0, b = c0 - 2 * k;          // g(u) = c0 u + a u^2 + b u^3: g'(0)=c0, g(1)=k, g'(1)=0

            for (int yy = 0; yy < h; yy++)
                for (int xx = 0; xx < w; xx++)
                {
                    int li = yy * w + xx, gxx = x0 + xx, gyy = y0 + yy;
                    if (gxx < 0 || gyy < 0 || gxx >= n || gyy >= n) continue;
                    int gi = gyy * n + gxx;
                    double t, wt, amp;
                    if (variant)
                    {
                        // Lift field: starts StartOutsidePx outside the plateau with zero slope (smoothstep in the distance
                        // from that outer line), so the rim slope is lifted smoothly; reaches the peak at the inscribed centre.
                        // Tapered to zero at the map edge so the world-map seam stays put.
                        int m = prm.StartOutsidePx;
                        double dd = inC[li] ? m + (dIn[li] - 0.5) : m - (dOut[li] - 0.5);
                        if (dd <= 0 || (!inC[li] && dOut[li] > m)) continue;
                        double span = radiusPx + m, uu = Math.Min(1, dd / span);
                        double rise = peak - PlateauM;
                        double edgeDist = Math.Min(Math.Min(gxx, gyy), Math.Min(n - 1 - gxx, n - 1 - gyy)) + 0.5;
                        double edgeTaper = SmoothStep(0, m, edgeDist);
                        double lift = rise * uu * uu * (3 - 2 * uu) * edgeTaper;
                        t = ToM(src[gi]) + lift;
                        double slope = rise * 6 * uu * (1 - uu) / (span * CellM) * edgeTaper;
                        amp = inC[li] ? NoiseAmpM * Math.Clamp(slope / 0.3, 0, 1) * SmoothStep(0, 6, dIn[li] - 0.5) : 0;
                        wt = inC[li] ? 1 : 1 - SmoothStep(m - 5, m - 1, dOut[li]);
                    }
                    else if (inC[li])
                    {
                        double u = Math.Min(1, (dIn[li] - 0.5) / radiusPx);
                        t = PlateauM + rM * s * (c0 * u + a * u * u + b * u * u * u);
                        double slope = s * (c0 + 2 * a * u + 3 * b * u * u);
                        amp = NoiseAmpM * Math.Clamp(slope / 0.3, 0, 1) * SmoothStep(0, 6, dIn[li] - 0.5);
                        wt = 1;
                    }
                    else
                    {
                        double d = dOut[li];
                        if (d > MarginPx) continue;
                        // Raise rim pixels depressed by the clipped samples towards the continued slope line; never lower anything.
                        double ext = PlateauM - s * (d - 0.5) * CellM;
                        double fade = 1 - SmoothStep(MarginPx / 2.0, MarginPx, d);
                        double o = ToM(src[gi]);
                        t = o + fade * Math.Max(0, ext - o);
                        wt = 1 - SmoothStep(MarginPx - 5, MarginPx - 1, d);
                        amp = 0;
                    }
                    if (double.IsNaN(target[gi]) || t > target[gi]) { target[gi] = t; capAt[gi] = cap; }
                    blendW[gi] = Math.Max(blendW[gi], wt);
                    noiseAmp[gi] = Math.Max(noiseAmp[gi], amp);
                    zone[gi] = true;
                }
            results.Add(new Component(results.Count + 1, c.pixels.Count, radiusPx, s, cnt, sEdge, 0, cap,
                variant ? $"lift k={peakF}, start {prm.StartOutsidePx} px out" : $"v2 dome k={PeakFactor}", c.x0, c.y0, c.x1, c.y1));
        }

        // Working field in metres inside the zone; source elsewhere.
        var work = new double[n * n];
        for (int i = 0; i < work.Length; i++) work[i] = zone[i] ? target[i] : ToM(src[i]);
        // Light Gaussian (sigma 3 px) to take the medial-axis crease off the distance-transform domes, faded out before the margin edge.
        const int R = 9; const double sigma = 3;
        var kern = new double[2 * R + 1];
        for (int i = -R; i <= R; i++) kern[i + R] = Math.Exp(-i * i / (2 * sigma * sigma));
        var outM = (double[])work.Clone();
        var zoneIdx = Enumerable.Range(0, n * n).Where(i => zone[i] && blendW[i] > 0).ToArray();
        Parallel.ForEach(zoneIdx, i =>
        {
            int x = i % n, y = i / n; double acc = 0, ws = 0;
            for (int dy = -R; dy <= R; dy++)
            {
                int yy = y + dy; if (yy < 0 || yy >= n) continue;
                for (int dx = -R; dx <= R; dx++)
                {
                    int xx = x + dx; if (xx < 0 || xx >= n) continue;
                    double kw = kern[dx + R] * kern[dy + R];
                    acc += kw * work[yy * n + xx]; ws += kw;
                }
            }
            outM[i] = work[i] + blendW[i] * (acc / ws - work[i]);
        });

        var output = (ushort[])src.Clone();
        int changed = 0, maxDist = 0;
        var distToComp = Edt(n, n, i => mask[i]);
        for (int i = 0; i < n * n; i++)
        {
            if (!zone[i]) continue;
            double m = outM[i];
            if (noiseAmp[i] > 0) m += noiseAmp[i] * Fbm(i % n, i / n, seed);
            if (mask[i]) m = Math.Min(m, capAt[i]);
            m = Math.Max(m, ToM(src[i])); // never lower a pixel
            var v = FromM(m);
            if (v != src[i]) { changed++; maxDist = Math.Max(maxDist, (int)Math.Ceiling(distToComp[i])); }
            output[i] = v;
        }
        // Peak per component from the final output.
        for (int ci = 0; ci < comps.Count; ci++)
        {
            int best = 0; foreach (var p in comps[ci].pixels) best = Math.Max(best, output[p]);
            results[ci] = results[ci] with { PeakM = ToM(best) };
        }
        return new Result(output, zone, results, changed, maxDist);
    }

    static double SmoothStep(double e0, double e1, double x) { double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1); return t * t * (3 - 2 * t); }

    sealed class RawComp { public List<int> pixels = []; public int x0 = int.MaxValue, y0 = int.MaxValue, x1 = -1, y1 = -1; }

    static List<RawComp> Label(bool[] mask, int n, int[] label)
    {
        var comps = new List<RawComp>(); var stack = new Stack<int>();
        for (int i = 0; i < mask.Length; i++)
        {
            if (!mask[i] || label[i] != 0) continue;
            var c = new RawComp(); comps.Add(c); label[i] = comps.Count; stack.Push(i);
            while (stack.Count > 0)
            {
                int p = stack.Pop(), x = p % n, y = p / n;
                c.pixels.Add(p); c.x0 = Math.Min(c.x0, x); c.x1 = Math.Max(c.x1, x); c.y0 = Math.Min(c.y0, y); c.y1 = Math.Max(c.y1, y);
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                    {
                        int xx = x + dx, yy = y + dy;
                        if (xx < 0 || yy < 0 || xx >= n || yy >= n) continue;
                        int q = yy * n + xx;
                        if (mask[q] && label[q] == 0) { label[q] = comps.Count; stack.Push(q); }
                    }
            }
        }
        return comps.OrderByDescending(c => c.pixels.Count).ToList();
    }

    /// <summary>Exact Euclidean distance transform (Felzenszwalb-Huttenlocher) to the nearest pixel where isSource holds.</summary>
    public static double[] Edt(int w, int h, Func<int, bool> isSource)
    {
        const double Inf = 1e20;
        var f = new double[w * h];
        for (int i = 0; i < f.Length; i++) f[i] = isSource(i) ? 0 : Inf;
        var tmp = new double[w * h];
        Parallel.For(0, h, y => { var row = new double[w]; for (int x = 0; x < w; x++) row[x] = f[y * w + x]; var d = Dt1(row); for (int x = 0; x < w; x++) tmp[y * w + x] = d[x]; });
        var res = new double[w * h];
        Parallel.For(0, w, x => { var col = new double[h]; for (int y = 0; y < h; y++) col[y] = tmp[y * w + x]; var d = Dt1(col); for (int y = 0; y < h; y++) res[y * w + x] = Math.Sqrt(d[y]); });
        return res;
    }

    static double[] Dt1(double[] f)
    {
        int n = f.Length; var d = new double[n]; var v = new int[n]; var z = new double[n + 1];
        int k = 0; v[0] = 0; z[0] = double.NegativeInfinity; z[1] = double.PositiveInfinity;
        for (int q = 1; q < n; q++)
        {
            double s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2.0 * q - 2.0 * v[k]);
            while (s <= z[k]) { k--; s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2.0 * q - 2.0 * v[k]); }
            k++; v[k] = q; z[k] = s; z[k + 1] = double.PositiveInfinity;
        }
        k = 0;
        for (int q = 0; q < n; q++) { while (z[k + 1] < q) k++; d[q] = (q - v[k]) * (q - v[k]) + f[v[k]]; }
        return d;
    }

    // Deterministic value-noise fBm in [-1, 1], base wavelength 24 px, 3 octaves.
    static double Fbm(int x, int y, int seed)
    {
        double sum = 0, amp = 1, norm = 0, freq = 1.0 / 24;
        for (int o = 0; o < 3; o++) { sum += amp * ValueNoise(x * freq, y * freq, seed + o * 1013); norm += amp; amp *= 0.5; freq *= 2; }
        return sum / norm;
    }

    static double ValueNoise(double x, double y, int seed)
    {
        int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
        double fx = x - xi, fy = y - yi; fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        double a = Hash(xi, yi, seed), b = Hash(xi + 1, yi, seed), c = Hash(xi, yi + 1, seed), d = Hash(xi + 1, yi + 1, seed);
        return (a + (b - a) * fx) * (1 - fy) + (c + (d - c) * fx) * fy;
    }

    static double Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177; h ^= h >> 16;
            return h / (double)uint.MaxValue * 2 - 1;
        }
    }

    // ---------- world map ----------

    public const int Inner0 = 1536, InnerN = 1024;

    /// <summary>hmap-ext's inner placement: integer 4x box-downsample, rounding half up ((sum + 8) / 16).</summary>
    public static ushort[] BoxDown4(ushort[] src, int n)
    {
        int m = n / 4; var o = new ushort[m * m];
        for (int y = 0; y < m; y++)
            for (int x = 0; x < m; x++)
            {
                ulong s = 0;
                for (int dy = 0; dy < 4; dy++) for (int dx = 0; dx < 4; dx++) s += src[(y * 4 + dy) * n + x * 4 + dx];
                o[y * m + x] = (ushort)((s + 8) / 16);
            }
        return o;
    }

    public static (double maxM, double meanM) SeamRing(ushort[] world, int wn)
    {
        int a = Inner0, b = Inner0 + InnerN - 1; double max = 0, sum = 0; int cnt = 0;
        void Pair(int ex, int ey, int ix, int iy) { double d = Math.Abs(ToM(world[ey * wn + ex]) - ToM(world[iy * wn + ix])); max = Math.Max(max, d); sum += d; cnt++; }
        for (int i = a; i <= b; i++) { Pair(i, a - 1, i, a); Pair(i, b + 1, i, b); Pair(a - 1, i, a, i); Pair(b + 1, i, b, i); }
        return (max, sum / cnt);
    }

    // ---------- previews ----------

    public static Image<Rgb24> Hillshade(ushort[] d, int n, int x0, int y0, int w, int h, double cellM, double minM, double maxM)
    {
        var img = new Image<Rgb24>(w, h);
        double az = 315 * Math.PI / 180, alt = 45 * Math.PI / 180;
        double lx = Math.Cos(alt) * Math.Sin(az), ly = -Math.Cos(alt) * Math.Cos(az), lz = Math.Sin(alt);
        img.ProcessPixelRows(acc =>
        {
            for (int y = 0; y < h; y++)
            {
                var row = acc.GetRowSpan(y);
                for (int x = 0; x < w; x++)
                {
                    int gx = Math.Clamp(x0 + x, 1, n - 2), gy = Math.Clamp(y0 + y, 1, n - 2);
                    double dzdx = (ToM(d[gy * n + gx + 1]) - ToM(d[gy * n + gx - 1])) / (2 * cellM);
                    double dzdy = (ToM(d[(gy + 1) * n + gx]) - ToM(d[(gy - 1) * n + gx])) / (2 * cellM);
                    double nx = -dzdx, ny = -dzdy, nz = 1, len = Math.Sqrt(nx * nx + ny * ny + 1);
                    double shade = Math.Max(0, (nx * lx + ny * ly + nz * lz) / len);
                    double t = (ToM(d[(y0 + y) * n + x0 + x]) - minM) / Math.Max(1, maxM - minM);
                    // hypsometric tint (green -> brown -> white) times hillshade
                    double r = t < 0.5 ? 0.35 + 0.5 * t : 0.6 + 0.8 * (t - 0.5), g = t < 0.5 ? 0.55 - 0.1 * t : 0.5 + 0.9 * (t - 0.5), bl = t < 0.5 ? 0.3 : 0.3 + 1.3 * (t - 0.5);
                    double sh = 0.35 + 0.65 * shade;
                    row[x] = new Rgb24((byte)Math.Clamp(r * sh * 255, 0, 255), (byte)Math.Clamp(g * sh * 255, 0, 255), (byte)Math.Clamp(bl * sh * 255, 0, 255));
                }
            }
        });
        return img;
    }

    internal static void SaveSideBySide(string path, ushort[] before, ushort[] after, int n, int x0, int y0, int w, int h, int maxSide)
    {
        double minM = double.MaxValue, maxM = double.MinValue;   // shared tint range for both sides
        foreach (var arr in new[] { before, after })
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) { double m = ToM(arr[(y0 + y) * n + x0 + x]); minM = Math.Min(minM, m); maxM = Math.Max(maxM, m); }
        using var a = Hillshade(before, n, x0, y0, w, h, CellM, minM, maxM);
        using var b = Hillshade(after, n, x0, y0, w, h, CellM, minM, maxM);
        double f = Math.Min(1.0, maxSide / (double)Math.Max(w, h));
        int sw = Math.Max(1, (int)(w * f)), sh = Math.Max(1, (int)(h * f));
        if (f < 1) { a.ResizeTo(sw, sh); b.ResizeTo(sw, sh); }
        using var canvas = new Image<Rgb24>(sw * 2 + 8, sh, new Rgb24(255, 255, 255));
        canvas.ProcessPixelRows(a, b, (cv, ra, rb) =>
        {
            for (int y = 0; y < sh; y++) { ra.GetRowSpan(y).CopyTo(cv.GetRowSpan(y)); rb.GetRowSpan(y).CopyTo(cv.GetRowSpan(y).Slice(sw + 8)); }
        });
        canvas.Save(path);
    }

    static void ResizeTo(this Image<Rgb24> img, int w, int h) =>
        SixLabors.ImageSharp.Processing.ProcessingExtensions.Mutate(img, c => SixLabors.ImageSharp.Processing.ResizeExtensions.Resize(c, w, h));

    // ---------- CLI ----------

    public static ushort[] LoadL16(string path, out int w, out int h)
    {
        using var img = Image.Load<L16>(path);
        int ww = w = img.Width; h = img.Height; var d = new ushort[img.Width * img.Height];
        img.ProcessPixelRows(acc => { for (int y = 0; y < acc.Height; y++) { var r = acc.GetRowSpan(y); for (int x = 0; x < ww; x++) d[y * ww + x] = r[x].PackedValue; } });
        return d;
    }

    const string Usage = "usage: hm-rebuild peaks <playable-4096.png> <worldmap-4096.png> [--out-dir DIR] [--suffix v2] [--world-tag TAG] [--time-prefix]\n" +
        "         [--peak-factor 1.5] [--cap-factor 2.5] [--start-outside 0..12] [--min-variant-pixels 0]";

    public static int Run(string[] args)
    {
        if (args.Length < 3) { Console.Error.WriteLine(Usage); return 2; }
        string inPath = args[1], worldPath = args[2], outDir = ".", suffix = "v2", worldTag = "";
        bool timePrefix = false;
        var prm = new Params();
        var inv0 = CultureInfo.InvariantCulture;
        for (int i = 3; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new InputException($"missing value for {args[i]}");
            switch (args[i])
            {
                case "--out-dir": outDir = Next(); break;
                case "--suffix": suffix = Next(); break;
                case "--world-tag": worldTag = Next(); break;
                case "--time-prefix": timePrefix = true; break;
                case "--peak-factor": prm = prm with { PeakFactor = double.Parse(Next(), inv0) }; break;
                case "--cap-factor": prm = prm with { CapFactor = double.Parse(Next(), inv0) }; break;
                case "--start-outside": prm = prm with { StartOutsidePx = int.Parse(Next(), inv0) }; break;
                case "--min-variant-pixels": prm = prm with { MinVariantPixels = int.Parse(Next(), inv0) }; break;
                default: throw new InputException($"unknown option {args[i]}\n{Usage}");
            }
        }
        if (prm.StartOutsidePx > MarginPx) throw new InputException($"--start-outside must be <= {MarginPx}");
        Directory.CreateDirectory(outDir);
        var src = LoadL16(inPath, out int w, out int h);
        if (w != N || h != N) throw new InputException($"playable map must be {N}x{N}");
        var world = LoadL16(worldPath, out int ww, out int wh);
        if (ww != N || wh != N) throw new InputException($"world map must be {N}x{N}");

        var stem = Path.GetFileNameWithoutExtension(inPath);
        // --time-prefix: owner naming <MMddHHmm>-<base>, base without an existing time prefix, "-4096" or v-suffixes.
        var outBase = timePrefix
            ? $"{DateTime.Now:MMddHHmm}-" + Regex.Replace(Regex.Replace(stem, @"^\d{8}-", ""), @"(-4096|-v\d+[a-z]*)", "")
            : $"{stem}-{suffix}";
        var pV2 = Path.Combine(outDir, $"{outBase}.png");
        var pWorld = Path.Combine(outDir, $"{outBase}_worldmap{worldTag}.png");

        var res = Rebuild(src, N, prm);

        // Self-check on the real data: outside the change zone the output is bit-identical.
        long diffOutside = 0, diffInside = 0;
        for (int i = 0; i < src.Length; i++) if (res.Output[i] != src[i]) { if (res.ChangeZone[i]) diffInside++; else diffOutside++; }
        if (diffOutside != 0) throw new Exception($"BUG: {diffOutside} pixels changed outside the component+margin zone");

        Program.SaveL16(pV2, res.Output, N, N);

        // World map: verify the existing inner is hmap-ext's box-downsample of the v1 map, then re-embed v2.
        var innerV1 = BoxDown4(src, N);
        long innerMismatch = 0;
        for (int y = 0; y < InnerN; y++) for (int x = 0; x < InnerN; x++) if (world[(Inner0 + y) * N + Inner0 + x] != innerV1[y * InnerN + x]) innerMismatch++;
        var seamBefore = SeamRing(world, N);
        var world2 = (ushort[])world.Clone();
        var innerV2 = BoxDown4(res.Output, N);
        for (int y = 0; y < InnerN; y++) for (int x = 0; x < InnerN; x++) world2[(Inner0 + y) * N + Inner0 + x] = innerV2[y * InnerN + x];
        var seamAfter = SeamRing(world2, N);
        long worldOutsideDiff = 0;
        for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
            {
                bool inner = x >= Inner0 && x < Inner0 + InnerN && y >= Inner0 && y < Inner0 + InnerN;
                if (!inner && world2[y * N + x] != world[y * N + x]) worldOutsideDiff++;
            }
        Program.SaveL16(pWorld, world2, N, N);

        // Previews: union bbox of all changes, plus the three largest components.
        var previews = new List<string>();
        int ux0 = N, uy0 = N, ux1 = -1, uy1 = -1;
        foreach (var c in res.Components) { ux0 = Math.Min(ux0, c.X0); uy0 = Math.Min(uy0, c.Y0); ux1 = Math.Max(ux1, c.X1); uy1 = Math.Max(uy1, c.Y1); }
        if (res.Components.Count > 0)
        {
            (int, int, int, int) Box(int a0, int b0, int a1, int b1, int pad) { int x0 = Math.Max(0, a0 - pad), y0 = Math.Max(0, b0 - pad), x1 = Math.Min(N - 1, a1 + pad), y1 = Math.Min(N - 1, b1 + pad); return (x0, y0, x1 - x0 + 1, y1 - y0 + 1); }
            var (bx, by, bw, bh) = Box(ux0, uy0, ux1, uy1, 48);
            var p = Path.Combine(outDir, $"{outBase}_changes_before-after.png"); SaveSideBySide(p, src, res.Output, N, bx, by, bw, bh, 1200); previews.Add(p);
            foreach (var c in res.Components.Take(3))
            {
                var (cx, cy, cw, ch) = Box(c.X0, c.Y0, c.X1, c.Y1, 40);
                var pc = Path.Combine(outDir, $"{outBase}_comp{c.Id}_before-after.png"); SaveSideBySide(pc, src, res.Output, N, cx, cy, cw, ch, 900); previews.Add(pc);
            }
        }

        // Report.
        var sb = new StringBuilder();
        var inv = CultureInfo.InvariantCulture;
        sb.AppendLine($"# Peak reconstruction: {Path.GetFileName(inPath)} -> {Path.GetFileName(pV2)}\n");
        sb.AppendLine($"Height Scale 2048 m (plateau = {Ceiling} = {PlateauM:F2} m). Components are 8-connected pixels at {Ceiling}. ");
        sb.AppendLine($"Dome: h = plateau + R·s·g(u), g(u) = c0·u + a·u² + b·u³ with g'(0)=c0 = s_edge/s (continues the slope measured 1–4 px outside, so no crease), steepening inward, g(1)={PeakFactor} (peak = plateau + {PeakFactor}·R·s), g'(1)=0; u = inner distance / R; the map edge counts as dome boundary. Rim slope = mean gradient on the ring {RimRingMinPx}–{RimRingMaxPx} px outside. ");
        sb.AppendLine($"Variant ({suffix}): peak factor {prm.PeakFactor}, cap factor {prm.CapFactor}, start {prm.StartOutsidePx} px outside (0 = v2 dome), applied to components ≥ {prm.MinVariantPixels} px; others use the v2 dome. Variant lift: h = source + (peak − plateau)·smoothstep(dd/(R + start)), dd = distance from the line `start` px outside the plateau, tapered to 0 at the map edge.\n");
        sb.AppendLine($"v2 cap: min(plateau + {CapFactor}·R·s, {AbsoluteCapM} m). Margin ≤ {MarginPx} px, σ=3 px smoothing, slope-scaled value noise ≤ {NoiseAmpM} m.\n");
        sb.AppendLine($"- clipped pixels: {res.Components.Sum(c => c.Pixels)} ({100.0 * res.Components.Sum(c => c.Pixels) / src.Length:F3} %) in {res.Components.Count} components");
        sb.AppendLine($"- pixels changed: {res.ChangedPixels}; farthest changed pixel {res.MaxChangedDistPx} px from a component; changed outside zone: {diffOutside} (asserted 0)");
        sb.AppendLine($"- output max: {ToM(res.Output.Max(v => (int)v)):F1} m");
        sb.AppendLine($"- world map: existing inner vs box_down4(v1) mismatches: {innerMismatch}; exterior pixels changed: {worldOutsideDiff}");
        sb.AppendLine("- a component's peak is the max output over its pixels; a small component within the margin of a variant component rides that lift, so its peak can exceed its own v2 cap (the variant's cap applies there).");
        sb.AppendLine($"- seam ring |exterior - interior| before: max {seamBefore.maxM:F2} m, mean {seamBefore.meanM:F3} m; after: max {seamAfter.maxM:F2} m, mean {seamAfter.meanM:F3} m\n");
        sb.AppendLine("| # | pixels | bbox (x0,y0)-(x1,y1) | inscribed R (px / m) | rim slope s, 12–30 px (m/m, samples) | edge slope 1–4 px | profile | peak (m) | cap (m) |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (var c in res.Components)
            sb.AppendLine(string.Format(inv, "| {0} | {1} | ({2},{3})-({4},{5}) | {6:F1} / {7:F1} | {8:F3} ({9}) | {10:F3} | {13} | {11:F1} | {12:F1} |", c.Id, c.Pixels, c.X0, c.Y0, c.X1, c.Y1, c.RadiusPx, c.RadiusPx * CellM, c.RimSlope, c.RimSamples, c.EdgeSlope, c.PeakM, c.CapM, c.Profile));
        sb.AppendLine("\nPreviews (hillshade + hypsometric tint, left before / right after):\n");
        foreach (var p in previews) sb.AppendLine($"- {Path.GetFileName(p)}");
        var statsPath = Path.Combine(outDir, "stats.md");
        File.WriteAllText(statsPath, sb.ToString());
        Console.Write(sb.ToString());
        Console.WriteLine($"\nwrote {pV2}\nwrote {pWorld}\nwrote {statsPath}");
        return 0;
    }
}
