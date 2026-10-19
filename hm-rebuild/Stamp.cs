// stamp: raise the 1:1 playable map to the tops of terrain-like CS1 Procedural Objects (mountains, rock work, falls steps).
// PO heights come either as per-layer rasters in the 4096 playable frame (used as is) or on the CS1 1081 grid (16 m cells,
// resampled with the 1:1 geometry). They are stamped where they rise more than 0.5 m above the terrain, with a 16 px
// smoothstep falloff outside the footprint and a light Gaussian only where the PO raster steps by more than 6 m.
// The terrain is never lowered; pixels outside footprints + falloff margin are copied bit for bit.
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

namespace HmRebuild;

public static class Stamp
{
    const int N = Peaks.N, S = Program.SrcN;
    const double CellM = Peaks.CellM;

    /// <param name="FalloffPx">width of the smoothstep falloff outside the footprint (the change margin)</param>
    /// <param name="StepM">PO neighbour step that triggers smoothing (metres)</param>
    /// <param name="SmoothSigmaPx">Gaussian sigma applied near PO steps (0 = off)</param>
    /// <param name="MaxSlope">slope limit in m/m; 0 = off (default)</param>
    public sealed record Options(double MinRiseM = 0.5, int FalloffPx = 16, double StepM = 6, double SmoothSigmaPx = 1.5,
        double MaxSlope = 0, Kernel Kernel = Kernel.Lanczos3, double Po1081UnitsPerM = 64, double Po4096UnitsPerM = 32, HashSet<string>? Ids = null,
        Soften? Soft = null);

    /// <summary>Softened variant for low-poly meshes: Gaussian over the whole stamped area, raw surface kept near ridgelines,
    /// slope-scaled value noise so flanks read as rock.</summary>
    /// <param name="RidgeRaw">share of the raw surface within RidgeDistPx of a raw ridge (local maximum along any axis)</param>
    public sealed record Soften(double SigmaPx = 4, double RidgeRaw = 0.3, int RidgeDistPx = 8, double NoiseMinM = 2, double NoiseMaxM = 6,
        double[]? WavelengthsM = null, int Seed = 7)
    {
        public double[] Wavelengths => WavelengthsM ?? [60, 30, 20];
    }

    // ---------- PO field in the playable frame ----------

    /// <summary>PO tops on the 4096 playable grid. Key = object id (JSON input) or layer name (raster input).</summary>
    public sealed class PoField
    {
        public readonly double[] Top = new double[N * N];   // metres, NaN = no PO
        public readonly bool[] Steep = new bool[N * N];     // PO step > StepM to a covered neighbour (in the source frame)
        public readonly string?[] Key = new string?[N * N];
        public readonly List<string> Sources = [];
        public readonly Dictionary<string, int> SourceCells = [];
        public int Saturated;
        public PoField() => Array.Fill(Top, double.NaN);

        /// <summary>Max-merge another field (several layers stamped together).</summary>
        public void Merge(PoField o)
        {
            for (int i = 0; i < Top.Length; i++)
            {
                if (!double.IsNaN(o.Top[i]) && (double.IsNaN(Top[i]) || o.Top[i] > Top[i])) { Top[i] = o.Top[i]; Key[i] = o.Key[i]; }
                Steep[i] |= o.Steep[i];
            }
            Sources.AddRange(o.Sources); Saturated += o.Saturated;
            foreach (var (k, v) in o.SourceCells) SourceCells[k] = SourceCells.GetValueOrDefault(k) + v;
        }
    }

    /// <summary>4096 playable-frame raster (row 0 north, value / unitsPerM metres, 0 = no PO): used without resampling.</summary>
    public static PoField From4096(string path, double unitsPerM, string key, double stepM)
    {
        var raw = Peaks.LoadL16(path, out int w, out int h);
        if (w != N || h != N) throw new InputException($"{path}: expected {N}x{N}");
        var f = new PoField(); f.Sources.Add($"{key}: {Path.GetFileName(path)} (4096 frame, /{unitsPerM})");
        int cells = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            if (raw[i] == 0) continue;
            if (raw[i] == 65535) f.Saturated++;
            f.Top[i] = raw[i] / unitsPerM; f.Key[i] = key; cells++;
        }
        f.SourceCells[key] = cells;
        double stepU = stepM * unitsPerM;
        Parallel.For(0, N, y =>
        {
            for (int x = 0; x < N; x++)
            {
                int i = y * N + x; if (raw[i] == 0) continue;
                bool st = false;
                if (x > 0 && raw[i - 1] != 0 && Math.Abs(raw[i] - raw[i - 1]) > stepU) st = true;
                if (x < N - 1 && raw[i + 1] != 0 && Math.Abs(raw[i] - raw[i + 1]) > stepU) st = true;
                if (y > 0 && raw[i - N] != 0 && Math.Abs(raw[i] - raw[i - N]) > stepU) st = true;
                if (y < N - 1 && raw[i + N] != 0 && Math.Abs(raw[i] - raw[i + N]) > stepU) st = true;
                f.Steep[i] = st;
            }
        });
        return f;
    }

    /// <summary>1081-grid PO heights resampled into the playable frame (normalized convolution, footprint = coverage >= 0.5).</summary>
    public static PoField From1081(PoGrid g, Kernel kernel, double stepM, string source)
    {
        var (top, _, cell) = Resample(g, kernel);
        var f = new PoField(); f.Sources.Add(source); f.Saturated = g.SaturatedCells;
        foreach (var (k, v) in g.CellsPerId) f.SourceCells[k] = v;
        var steepCell = new bool[S * S];
        for (int r = 1; r < S - 1; r++)
            for (int c = 1; c < S - 1; c++)
            {
                int i = r * S + c; double t = g.TopM[i]; if (double.IsNaN(t)) continue;
                foreach (int j in new[] { i - 1, i + 1, i - S, i + S })
                    if (!double.IsNaN(g.TopM[j]) && Math.Abs(t - g.TopM[j]) > stepM) { steepCell[i] = true; break; }
            }
        for (int p = 0; p < N * N; p++)
        {
            f.Top[p] = top[p];
            if (double.IsNaN(top[p])) continue;
            f.Key[p] = g.Id[cell[p]] ?? NearestId(g, cell[p]);
            f.Steep[p] = steepCell[cell[p]];
        }
        return f;
    }

    /// <summary>Finds per-layer rasters in a folder: file names containing the layer token (letters/digits only, case-insensitive),
    /// 4096 playable frame preferred over the 1081 CS1 frame.</summary>
    public static PoField FromLayers(string dir, IEnumerable<string> layers, Options o, List<string> log)
    {
        static string Norm(string s) => Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]", "");
        var pngs = Directory.GetFiles(dir, "*.png").Where(p => !Path.GetFileName(p).Contains("preview", StringComparison.OrdinalIgnoreCase)).ToList();
        var all = new PoField();
        foreach (var layer in layers)
        {
            var tok = Norm(layer);
            var cands = pngs.Where(p => Norm(Path.GetFileNameWithoutExtension(p)).Contains(tok))
                .Select(p => (p, info: Image.Identify(p))).ToList();
            var p4096 = cands.Where(c => c.info.Width == N && c.info.Height == N).Select(c => c.p).ToList();
            var p1081 = cands.Where(c => c.info.Width == S && c.info.Height == S).Select(c => c.p).ToList();
            if (p4096.Count > 1 || (p4096.Count == 0 && p1081.Count > 1))
                throw new InputException($"layer '{layer}' is ambiguous in {dir}: {string.Join(", ", (p4096.Count > 0 ? p4096 : p1081).Select(Path.GetFileName))}");
            PoField f;
            if (p4096.Count == 1) f = From4096(p4096[0], o.Po4096UnitsPerM, layer, o.StepM);
            else if (p1081.Count == 1)
            {
                var g = new PoGrid();
                var raw = Peaks.LoadL16(p1081[0], out _, out _);
                for (int i = 0; i < raw.Length; i++) { if (raw[i] == 0) continue; if (raw[i] == 65535) g.SaturatedCells++; g.Add(i, raw[i] / o.Po1081UnitsPerM, layer); }
                g.CellsPerId[layer] = raw.Count(v => v != 0);
                f = From1081(g, o.Kernel, o.StepM, $"{layer}: {Path.GetFileName(p1081[0])} (1081 frame, /{o.Po1081UnitsPerM}, resampled)");
            }
            else throw new InputException($"no 4096 or 1081 raster for layer '{layer}' in {dir}");
            log.Add(f.Sources[0]);
            all.Merge(f);
        }
        return all;
    }

    // ---------- 1081 grid (JSON / single raster) ----------

    /// <summary>Per-cell PO heights on the 1081 grid: best (highest) selected object per cell.</summary>
    public sealed class PoGrid
    {
        public readonly double[] TopM = new double[S * S];     // metres, NaN = no selected object
        public readonly string?[] Id = new string?[S * S];
        public readonly Dictionary<string, int> CellsPerId = [];
        public int SaturatedCells;
        public PoGrid() => Array.Fill(TopM, double.NaN);

        public void Add(int index, double maxY, string id)
        {
            if (index < 0 || index >= S * S) return;
            if (double.IsNaN(TopM[index]) || maxY > TopM[index]) { TopM[index] = maxY; Id[index] = id; }
        }
    }

    /// <summary>JSON (sparse [index, maxY, minY, objectId] rows) is preferred: object ids and heights above 1024 m.
    /// Without JSON the raster is one object "all" (value / unitsPerM metres; 65535 saturates).</summary>
    public static PoGrid LoadPo(string? rasterPath, string? jsonPath, Options o)
    {
        var g = new PoGrid();
        if (jsonPath != null && File.Exists(jsonPath))
        {
            using var doc = JsonDocument.Parse(File.ReadAllBytes(jsonPath));
            var rows = FindRows(doc.RootElement) ?? throw new InputException($"no [index, maxY, minY, objectId] array found in {jsonPath}");
            foreach (var r in rows.EnumerateArray())
            {
                if (r.ValueKind != JsonValueKind.Array || r.GetArrayLength() < 4) continue;
                int idx = r[0].GetInt32(); double maxY = r[1].GetDouble();
                string id = r[3].ValueKind == JsonValueKind.String ? r[3].GetString()! : r[3].GetRawText();
                g.CellsPerId[id] = g.CellsPerId.GetValueOrDefault(id) + 1;
                if (o.Ids != null && !o.Ids.Contains(id)) continue;
                g.Add(idx, maxY, id);
            }
        }
        else if (rasterPath != null)
        {
            var raw = Peaks.LoadL16(rasterPath, out int w, out int h);
            if (w != S || h != S) throw new InputException($"PO raster must be {S}x{S}, got {w}x{h}");
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == 0) continue;
                if (raw[i] == 65535) g.SaturatedCells++;
                g.CellsPerId["all"] = g.CellsPerId.GetValueOrDefault("all") + 1;
                if (o.Ids != null && !o.Ids.Contains("all")) continue;
                g.Add(i, raw[i] / o.Po1081UnitsPerM, "all");
            }
        }
        else throw new InputException("need --po-json or a PO raster");
        return g;
    }

    static JsonElement? FindRows(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Array) return e;
        if (e.ValueKind == JsonValueKind.Object)
            foreach (var p in e.EnumerateObject())
                if (p.Value.ValueKind == JsonValueKind.Array && p.Value.GetArrayLength() > 0 && p.Value[0].ValueKind == JsonValueKind.Array) return p.Value;
        return null;
    }

    public static HashSet<string> LoadIdList(string path) =>
        File.ReadAllLines(path).Select(l => l.Split('#')[0].Trim()).Where(l => l.Length > 0).ToHashSet();

    /// <summary>Normalized-convolution resample onto the 4096 frame: height = conv(m·h)/conv(m), coverage = conv(m).
    /// Footprint = coverage >= 0.5; height clamped to the range of the covered cells in the central 4x4 support (no ringing).</summary>
    public static (double[] topM, double[] cover, int[] cellIdx) Resample(PoGrid g, Kernel kernel)
    {
        var taps = Program.Taps(kernel);
        var mh = new double[S * N]; var mm = new double[S * N];
        Parallel.For(0, S, y =>
        {
            for (int k = 0; k < N; k++)
            {
                var (s0, w) = taps[k]; double a = 0, b = 0;
                for (int j = 0; j < w.Length; j++)
                {
                    int i = y * S + s0 + j; double t = g.TopM[i];
                    if (!double.IsNaN(t)) { a += w[j] * t; b += w[j]; }
                }
                mh[y * N + k] = a; mm[y * N + k] = b;
            }
        });
        var top = new double[N * N]; var cover = new double[N * N]; var cell = new int[N * N];
        Parallel.For(0, N, k =>
        {
            var (s0, w) = taps[k];
            double uz = Program.SrcCoord(k);
            for (int x = 0; x < N; x++)
            {
                double a = 0, b = 0;
                for (int j = 0; j < w.Length; j++) { a += w[j] * mh[(s0 + j) * N + x]; b += w[j] * mm[(s0 + j) * N + x]; }
                int p = k * N + x;
                cover[p] = b;
                cell[p] = (int)Math.Round(uz) * S + (int)Math.Round(Program.SrcCoord(x));
                if (b < 0.5) { top[p] = double.NaN; continue; }
                double h = a / b;
                double lo = double.MaxValue, hi = double.MinValue;
                int cz = (int)Math.Floor(uz), cx = (int)Math.Floor(Program.SrcCoord(x));
                for (int dz = -1; dz <= 2; dz++) for (int dx = -1; dx <= 2; dx++)
                    {
                        double t = g.TopM[(cz + dz) * S + cx + dx];
                        if (!double.IsNaN(t)) { lo = Math.Min(lo, t); hi = Math.Max(hi, t); }
                    }
                top[p] = lo <= hi ? Math.Clamp(h, lo, hi) : h;
            }
        });
        return (top, cover, cell);
    }

    static string? NearestId(PoGrid g, int c)
    {
        int cx = c % S, cy = c / S;
        for (int r = 1; r <= 2; r++)
            for (int dy = -r; dy <= r; dy++) for (int dx = -r; dx <= r; dx++)
                {
                    int x = cx + dx, y = cy + dy; if (x < 0 || y < 0 || x >= S || y >= S) continue;
                    if (g.Id[y * S + x] is { } id) return id;
                }
        return null;
    }

    // ---------- core ----------

    public sealed record KeyStat(string Key, int SourceCells, int StampedPx, double MaxAddedM, double MaxTopM, int X0, int Y0, int X1, int Y1);

    public sealed record Result(ushort[] Output, bool[] Zone, int ChangedPixels, int MaxChangedDistPx, List<KeyStat> Keys,
        List<(int x0, int y0, int x1, int y1, int px)> Regions, int FootprintPx, int StampPx, int SmoothedPx, int SlopeLimitedPx);

    public static Result Run(ushort[] terrain, PoField po, Options o)
    {
        var tM = new double[N * N];
        for (int i = 0; i < tM.Length; i++) tM[i] = Peaks.ToM(terrain[i]);

        // Stamp mask: inside the footprint and more than MinRise above the terrain.
        var stamp = new bool[N * N]; int footprint = 0, stampPx = 0;
        for (int i = 0; i < stamp.Length; i++)
        {
            if (double.IsNaN(po.Top[i])) continue;
            footprint++;
            if (po.Top[i] - tM[i] > o.MinRiseM) { stamp[i] = true; stampPx++; }
        }

        // Surface: stamp tops; outside, a smoothstep falloff over FalloffPx towards the terrain from the nearest stamped top.
        var (dist, nearest) = EdtNearest(N, N, stamp);
        var surf = (double[])tM.Clone();
        var zone = new bool[N * N];
        int fo = o.FalloffPx;
        Parallel.For(0, N * N, i =>
        {
            if (stamp[i]) { surf[i] = Math.Max(tM[i], po.Top[i]); zone[i] = true; return; }
            double d = dist[i];
            if (d > fo || nearest[i] < 0) return;
            zone[i] = true;
            double wgt = 1 - SmoothStep(0, fo, d);
            surf[i] = tM[i] + wgt * Math.Max(0, po.Top[nearest[i]] - tM[i]);
        });

        // The nearest-pixel falloff streaks radially where neighbouring margin pixels pick different stamped tops; smooth the
        // falloff ring (outside the footprint only) with a sigma-2 px Gaussian. Never below the terrain.
        {
            const int R = 6; const double sg = 2;
            var kern = new double[2 * R + 1];
            for (int i = -R; i <= R; i++) kern[i + R] = Math.Exp(-i * i / (2 * sg * sg));
            var src = (double[])surf.Clone();
            Parallel.For(0, N, y =>
            {
                for (int x = 0; x < N; x++)
                {
                    int i = y * N + x;
                    if (!zone[i] || stamp[i]) continue;
                    double acc = 0, ws = 0;
                    for (int dy = -R; dy <= R; dy++)
                    {
                        int yy = y + dy; if (yy < 0 || yy >= N) continue;
                        for (int dx = -R; dx <= R; dx++)
                        {
                            int xx = x + dx; if (xx < 0 || xx >= N) continue;
                            if (stamp[yy * N + xx]) continue;   // margin and terrain only: no interior heights pulled outward (no lip)
                            double kw = kern[dx + R] * kern[dy + R]; acc += kw * src[yy * N + xx]; ws += kw;
                        }
                    }
                    // Fade the smoothing out over the last 4 px so the ring still meets the untouched terrain exactly.
                    double w = 1 - SmoothStep(fo - 4, fo, dist[i]);
                    surf[i] = Math.Max(tM[i], src[i] + w * (acc / ws - src[i]));
                }
            });
        }

        // Light Gaussian only near PO steps (> StepM between neighbours), so staircase flanks smooth out while ridgelines keep
        // their shape. Weight 1 within 1 px of a step pixel, fading to 0 at 3 sigma; never below the terrain; zone only.
        int smoothed = 0;
        if (o.SmoothSigmaPx > 0)
        {
            int R = (int)Math.Ceiling(3 * o.SmoothSigmaPx);
            var steepIn = new bool[N * N];
            for (int i = 0; i < steepIn.Length; i++) steepIn[i] = po.Steep[i] && zone[i];
            var dSteep = Peaks.Edt(N, N, i => steepIn[i]);
            var kern = new double[2 * R + 1];
            for (int i = -R; i <= R; i++) kern[i + R] = Math.Exp(-i * i / (2 * o.SmoothSigmaPx * o.SmoothSigmaPx));
            var src = (double[])surf.Clone();
            var cnt = new int[N];
            Parallel.For(0, N, y =>
            {
                for (int x = 0; x < N; x++)
                {
                    int i = y * N + x;
                    if (!zone[i] || dSteep[i] > R) continue;
                    double acc = 0, ws = 0;
                    for (int dy = -R; dy <= R; dy++)
                    {
                        int yy = y + dy; if (yy < 0 || yy >= N) continue;
                        for (int dx = -R; dx <= R; dx++)
                        {
                            int xx = x + dx; if (xx < 0 || xx >= N) continue;
                            double kw = kern[dx + R] * kern[dy + R]; acc += kw * src[yy * N + xx]; ws += kw;
                        }
                    }
                    double w = 1 - SmoothStep(1, R, dSteep[i]);
                    surf[i] = Math.Max(tM[i], src[i] + w * (acc / ws - src[i]));
                    cnt[y]++;
                }
            });
            smoothed = cnt.Sum();
        }

        int softened = o.Soft != null ? ApplySoften(surf, tM, stamp, zone, dist, fo, o.Soft) : 0;

        // Raw stamped slope per key (before any limit).
        var rawSlope = new double[N * N];
        Parallel.For(1, N - 1, y =>
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x; if (!stamp[i]) continue;
                double gx = (surf[i + 1] - surf[i - 1]) / (2 * CellM), gz = (surf[i + N] - surf[i - N]) / (2 * CellM);
                rawSlope[i] = Math.Sqrt(gx * gx + gz * gz);
            }
        });

        int limited = o.MaxSlope > 0 ? SlopeLimit(surf, tM, zone, o.MaxSlope) : 0;

        var output = (ushort[])terrain.Clone();
        int changed = 0, maxDist = 0;
        var added = new double[N * N];
        for (int i = 0; i < output.Length; i++)
        {
            if (!zone[i]) continue;
            var v = Peaks.FromM(Math.Max(surf[i], tM[i]));
            if (v > terrain[i]) { output[i] = v; changed++; maxDist = Math.Max(maxDist, (int)Math.Ceiling(dist[i])); added[i] = Peaks.ToM(v) - tM[i]; }
        }

        // Per key (object id or layer): stamped pixels, max added height, max PO top, bbox (pixel attributed to the key of
        // its own PO pixel, or of the nearest stamped pixel in the falloff).
        var st = new Dictionary<string, (int px, double add, double top, int x0, int y0, int x1, int y1)>();
        for (int i = 0; i < output.Length; i++)
        {
            if (output[i] == terrain[i]) continue;
            int src = stamp[i] ? i : nearest[i];
            var key = src >= 0 ? po.Key[src] : null; if (key == null) continue;
            var s = st.GetValueOrDefault(key, (0, 0, 0, N, N, -1, -1));
            int x = i % N, y = i / N;
            st[key] = (s.px + 1, Math.Max(s.add, added[i]), Math.Max(s.top, src >= 0 ? po.Top[src] : 0), Math.Min(s.x0, x), Math.Min(s.y0, y), Math.Max(s.x1, x), Math.Max(s.y1, y));
        }
        var keys = po.SourceCells.Keys.Union(st.Keys)
            .Select(k => { var s = st.GetValueOrDefault(k); return new KeyStat(k, po.SourceCells.GetValueOrDefault(k), s.px, s.add, s.top, s.x0, s.y0, s.x1, s.y1); })
            .OrderByDescending(s => s.MaxAddedM).ThenByDescending(s => s.SourceCells).ToList();

        return new Result(output, zone, changed, maxDist, keys, Regions(output, terrain), footprint, stampPx, smoothed + softened, limited);
    }

    /// <summary>Softening: s = Gaussian(surf) over stamped pixels (weight 1 inside, fading to 0 across the falloff margin);
    /// within RidgeDistPx of a raw ridge pixel the result is (1 - RidgeRaw)·s + RidgeRaw·raw; then value noise (fBm over the
    /// given wavelengths) with amplitude NoiseMin..NoiseMax scaled by the smoothed slope (max at slope >= 1). Never below terrain.</summary>
    static int ApplySoften(double[] surf, double[] tM, bool[] stamp, bool[] zone, double[] dist, int fo, Soften sp)
    {
        var raw = (double[])surf.Clone();
        int R = (int)Math.Ceiling(3 * sp.SigmaPx);
        var kern = new double[2 * R + 1];
        for (int i = -R; i <= R; i++) kern[i + R] = Math.Exp(-i * i / (2 * sp.SigmaPx * sp.SigmaPx));
        // Separable blur of the whole field (cheap), used only where the weight is > 0.
        var tmp = new double[N * N]; var blur = new double[N * N];
        Parallel.For(0, N, y =>
        {
            for (int x = 0; x < N; x++)
            {
                double a = 0, w = 0;
                for (int d = -R; d <= R; d++) { int xx = x + d; if (xx < 0 || xx >= N) continue; a += kern[d + R] * raw[y * N + xx]; w += kern[d + R]; }
                tmp[y * N + x] = a / w;
            }
        });
        Parallel.For(0, N, y =>
        {
            for (int x = 0; x < N; x++)
            {
                double a = 0, w = 0;
                for (int d = -R; d <= R; d++) { int yy = y + d; if (yy < 0 || yy >= N) continue; a += kern[d + R] * tmp[yy * N + x]; w += kern[d + R]; }
                blur[y * N + x] = a / w;
            }
        });

        // Raw ridge pixels: stamped, higher than both neighbours along x, y or a diagonal by more than 0.5 m.
        var ridge = new bool[N * N];
        Parallel.For(1, N - 1, y =>
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x; if (!stamp[i]) continue;
                double v = raw[i];
                foreach (int d in new[] { 1, N, N + 1, N - 1 })
                    if (v - raw[i - d] > 0.5 && v - raw[i + d] > 0.5) { ridge[i] = true; break; }
            }
        });
        var dRidge = Peaks.Edt(N, N, i => ridge[i]);

        int n = 0;
        var cnt = new int[N];
        Parallel.For(1, N - 1, y =>
        {
            for (int x = 1; x < N - 1; x++)
            {
                int i = y * N + x; if (!zone[i]) continue;
                double w = stamp[i] ? 1 : 1 - SmoothStep(0, fo - 4, dist[i]);
                if (w <= 0) continue;
                double rawShare = sp.RidgeRaw * (1 - SmoothStep(sp.RidgeDistPx / 2.0, sp.RidgeDistPx, dRidge[i]));
                double sm = (1 - rawShare) * blur[i] + rawShare * raw[i];
                double gx = (blur[i + 1] - blur[i - 1]) / (2 * CellM), gz = (blur[i + N] - blur[i - N]) / (2 * CellM);
                double slope = Math.Sqrt(gx * gx + gz * gz);
                double amp = sp.NoiseMinM + (sp.NoiseMaxM - sp.NoiseMinM) * Math.Clamp(slope, 0, 1);
                double noise = amp * Fbm(x * CellM, y * CellM, sp.Wavelengths, sp.Seed);
                double v = sm + noise;
                surf[i] = Math.Max(tM[i], raw[i] + w * (v - raw[i]));
                cnt[y]++;
            }
        });
        foreach (var c in cnt) n += c;
        return n;
    }

    // Value-noise fBm in [-1, 1] over world metres, one octave per wavelength (amplitude halves per octave).
    static double Fbm(double xm, double zm, double[] wavelengths, int seed)
    {
        double sum = 0, amp = 1, norm = 0;
        for (int o = 0; o < wavelengths.Length; o++) { sum += amp * ValueNoise(xm / wavelengths[o], zm / wavelengths[o], seed + 1013 * o); norm += amp; amp *= 0.5; }
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

    /// <summary>Optional slope limit: lower envelope min_q surf(q) + k·|p − q| (5x5 chamfer, 16 directions), never below terrain.</summary>
    static int SlopeLimit(double[] surf, double[] tM, bool[] zone, double k)
    {
        var env = (double[])surf.Clone();
        (int dx, int dy)[] fwd = [(-1, 0), (-2, -1), (-1, -1), (0, -1), (1, -1), (2, -1), (-1, -2), (1, -2)];
        var cost = fwd.Select(d => k * CellM * Math.Sqrt(d.dx * d.dx + d.dy * d.dy)).ToArray();
        for (int pass = 0; pass < 2; pass++)
        {
            for (int y = 0; y < N; y++) for (int x = 0; x < N; x++)
                {
                    int i = y * N + x; double v = env[i];
                    for (int j = 0; j < fwd.Length; j++) { int xx = x + fwd[j].dx, yy = y + fwd[j].dy; if (xx >= 0 && xx < N && yy >= 0) v = Math.Min(v, env[yy * N + xx] + cost[j]); }
                    env[i] = v;
                }
            for (int y = N - 1; y >= 0; y--) for (int x = N - 1; x >= 0; x--)
                {
                    int i = y * N + x; double v = env[i];
                    for (int j = 0; j < fwd.Length; j++) { int xx = x - fwd[j].dx, yy = y - fwd[j].dy; if (xx >= 0 && xx < N && yy < N) v = Math.Min(v, env[yy * N + xx] + cost[j]); }
                    env[i] = v;
                }
        }
        int limited = 0;
        for (int i = 0; i < surf.Length; i++)
        {
            if (!zone[i]) continue;
            double v = Math.Max(tM[i], env[i]);
            if (v < surf[i] - 0.01) limited++;
            surf[i] = v;
        }
        return limited;
    }

    /// <summary>Exact Euclidean distance transform with the index of the nearest source pixel (-1 if none).</summary>
    public static (double[] dist, int[] nearest) EdtNearest(int w, int h, bool[] src)
    {
        const double Inf = 1e20;
        var g = new double[w * h]; var gx = new int[w * h];
        Parallel.For(0, h, y =>
        {
            var f = new double[w];
            for (int x = 0; x < w; x++) f[x] = src[y * w + x] ? 0 : Inf;
            var (d, arg) = Dt1(f);
            for (int x = 0; x < w; x++) { g[y * w + x] = d[x]; gx[y * w + x] = arg[x]; }
        });
        var dist = new double[w * h]; var near = new int[w * h];
        Parallel.For(0, w, x =>
        {
            var f = new double[h];
            for (int y = 0; y < h; y++) f[y] = g[y * w + x];
            var (d, arg) = Dt1(f);
            for (int y = 0; y < h; y++)
            {
                dist[y * w + x] = d[y] >= Inf / 2 ? double.PositiveInfinity : Math.Sqrt(d[y]);
                int ry = arg[y];
                near[y * w + x] = d[y] >= Inf / 2 ? -1 : ry * w + gx[ry * w + x];
            }
        });
        return (dist, near);
    }

    static (double[] d, int[] arg) Dt1(double[] f)
    {
        int n = f.Length; var d = new double[n]; var arg = new int[n]; var v = new int[n]; var z = new double[n + 1];
        int k = 0; v[0] = 0; z[0] = double.NegativeInfinity; z[1] = double.PositiveInfinity;
        for (int q = 1; q < n; q++)
        {
            double s = ((f[q] + (double)q * q) - (f[v[k]] + (double)v[k] * v[k])) / (2.0 * q - 2.0 * v[k]);
            while (s <= z[k]) { k--; s = ((f[q] + (double)q * q) - (f[v[k]] + (double)v[k] * v[k])) / (2.0 * q - 2.0 * v[k]); }
            k++; v[k] = q; z[k] = s; z[k + 1] = double.PositiveInfinity;
        }
        k = 0;
        for (int q = 0; q < n; q++) { while (z[k + 1] < q) k++; d[q] = (double)(q - v[k]) * (q - v[k]) + f[v[k]]; arg[q] = v[k]; }
        return (d, arg);
    }

    static double SmoothStep(double e0, double e1, double x) { double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1); return t * t * (3 - 2 * t); }

    /// <summary>Connected regions (8-neighbour) of changed pixels, largest first.</summary>
    static List<(int, int, int, int, int)> Regions(ushort[] a, ushort[] b)
    {
        var seen = new bool[N * N]; var res = new List<(int, int, int, int, int)>(); var st = new Stack<int>();
        for (int i = 0; i < a.Length; i++)
        {
            if (seen[i] || a[i] == b[i]) continue;
            int x0 = N, y0 = N, x1 = -1, y1 = -1, cnt = 0; seen[i] = true; st.Push(i);
            while (st.Count > 0)
            {
                int p = st.Pop(), x = p % N, y = p / N; cnt++;
                x0 = Math.Min(x0, x); x1 = Math.Max(x1, x); y0 = Math.Min(y0, y); y1 = Math.Max(y1, y);
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                    {
                        int xx = x + dx, yy = y + dy; if (xx < 0 || yy < 0 || xx >= N || yy >= N) continue;
                        int q = yy * N + xx; if (!seen[q] && a[q] != b[q]) { seen[q] = true; st.Push(q); }
                    }
            }
            res.Add((x0, y0, x1, y1, cnt));
        }
        return res.OrderByDescending(r => r.Item5).ToList();
    }

    // ---------- CLI ----------

    const string Usage =
        "usage: hm-rebuild stamp <playable-4096.png> <worldmap-4096.png> PO-SOURCE [options]\n" +
        "  PO-SOURCE: --po-dir DIR [--layers mountain,rock-work | --layer falls]   (per-layer rasters, 4096 frame preferred, 1081 fallback)\n" +
        "         or  --po-json FILE [--ids FILE] | --po-raster FILE              (1081 CS1 grid)\n" +
        "  options:   [--falloff 16] [--step 6] [--smooth-sigma 1.5] [--max-slope 0] [--soften [--soften-sigma 4] [--soften-noise 2,6]] [--min-rise 0.5] [--kernel lanczos3|bicubic]\n" +
        "             [--po4096-units-per-m 32] [--po-units-per-m 64] [--out-dir DIR] [--name BASE] [--install DIR]\n" +
        "             [--world-tag .placeholder-hm-rebuild] [--max-previews 12] [--preview-at x,z,label[,sizeM]]...";

    public static int Cli(string[] args)
    {
        if (args.Length < 3) { Console.Error.WriteLine(Usage); return 2; }
        string inPath = args[1], worldPath = args[2], worldTag = ".placeholder-hm-rebuild";
        string? poJson = null, poRaster = null, poDir = null, idsPath = null, outDir = null, outName = null, installDir = null; int maxPrev = 12;
        var layers = new List<string> { "mountain", "rock work" };
        var at = new List<(double x, double z, string label, double size)>();
        var o = new Options(); var inv = CultureInfo.InvariantCulture;
        for (int i = 3; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new InputException($"missing value for {args[i]}");
            switch (args[i])
            {
                case "--po-dir": poDir = Next(); break;
                case "--layers": case "--layer": layers = Next().Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); break;
                case "--po-json": poJson = Next(); break;
                case "--po-raster": poRaster = Next(); break;
                case "--ids": idsPath = Next(); break;
                case "--falloff": o = o with { FalloffPx = int.Parse(Next(), inv) }; break;
                case "--step": o = o with { StepM = double.Parse(Next(), inv) }; break;
                case "--smooth-sigma": o = o with { SmoothSigmaPx = double.Parse(Next(), inv) }; break;
                case "--max-slope": o = o with { MaxSlope = double.Parse(Next(), inv) }; break;
                case "--soften": o = o with { Soft = new Soften() }; break;
                case "--soften-sigma": o = o with { Soft = (o.Soft ?? new Soften()) with { SigmaPx = double.Parse(Next(), inv) } }; break;
                case "--soften-noise": { var p = Next().Split(','); o = o with { Soft = (o.Soft ?? new Soften()) with { NoiseMinM = double.Parse(p[0], inv), NoiseMaxM = double.Parse(p[1], inv) } }; break; }
                case "--min-rise": o = o with { MinRiseM = double.Parse(Next(), inv) }; break;
                case "--po-units-per-m": o = o with { Po1081UnitsPerM = double.Parse(Next(), inv) }; break;
                case "--po4096-units-per-m": o = o with { Po4096UnitsPerM = double.Parse(Next(), inv) }; break;
                case "--kernel": o = o with { Kernel = Next() == "bicubic" ? Kernel.CatmullRom : Kernel.Lanczos3 }; break;
                case "--out-dir": outDir = Next(); break;
                case "--name": outName = Next(); break;
                case "--install": installDir = Next(); break;
                case "--world-tag": worldTag = Next(); break;
                case "--max-previews": maxPrev = int.Parse(Next(), inv); break;
                case "--preview-at":
                    {
                        var parts = Next().Split(',');
                        at.Add((double.Parse(parts[0], inv), double.Parse(parts[1], inv), parts.Length > 2 ? parts[2] : $"at{at.Count + 1}", parts.Length > 3 ? double.Parse(parts[3], inv) : 1200));
                        break;
                    }
                default: throw new InputException($"unknown option {args[i]}\n{Usage}");
            }
        }
        if (idsPath != null) o = o with { Ids = LoadIdList(idsPath) };
        var stampTime = DateTime.Now;
        var prefix = stampTime.ToString("MMddHHmm", inv);
        outDir ??= Path.Combine("out", prefix);
        Directory.CreateDirectory(outDir);

        var terrain = Peaks.LoadL16(inPath, out int w, out int h);
        if (w != N || h != N) throw new InputException($"playable map must be {N}x{N}");
        var world = Peaks.LoadL16(worldPath, out int ww, out int wh);
        if (ww != N || wh != N) throw new InputException($"world map must be {N}x{N}");

        var srcLog = new List<string>();
        PoField po;
        if (poDir != null) po = FromLayers(poDir, layers, o, srcLog);
        else
        {
            var g = LoadPo(poRaster, poJson, o);
            var src = poJson != null && File.Exists(poJson) ? $"JSON {Path.GetFileName(poJson)}" : $"raster {Path.GetFileName(poRaster)}";
            po = From1081(g, o.Kernel, o.StepM, src + (o.Ids != null ? $", {o.Ids.Count} ids from {Path.GetFileName(idsPath)}" : ", all objects"));
            srcLog.Add(po.Sources[0]);
        }

        var res = Run(terrain, po, o);
        long outside = 0;
        for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        if (outside != 0) throw new Exception($"BUG: {outside} pixels changed outside footprints + falloff margin");

        var stem = Path.GetFileNameWithoutExtension(inPath);
        var outBase = $"{prefix}-" + (outName ?? Regex.Replace(Regex.Replace(stem, @"^\d{8}-", ""), @"(-4096|-v\d+[a-z]*)", ""));
        var pOut = Path.Combine(outDir, outBase + ".png");
        var pWorld = Path.Combine(outDir, $"{outBase}_worldmap{worldTag}.png");
        if (File.Exists(pOut)) throw new InputException($"{pOut} exists (same minute as an earlier run?); wait a minute or pass --name");
        Program.SaveL16(pOut, res.Output, N, N);

        var innerIn = Peaks.BoxDown4(terrain, N);
        long innerMismatch = 0;
        for (int y = 0; y < Peaks.InnerN; y++) for (int x = 0; x < Peaks.InnerN; x++)
                if (world[(Peaks.Inner0 + y) * N + Peaks.Inner0 + x] != innerIn[y * Peaks.InnerN + x]) innerMismatch++;
        var seamBefore = Peaks.SeamRing(world, N);
        var world2 = (ushort[])world.Clone();
        var innerOut = Peaks.BoxDown4(res.Output, N);
        for (int y = 0; y < Peaks.InnerN; y++) for (int x = 0; x < Peaks.InnerN; x++) world2[(Peaks.Inner0 + y) * N + Peaks.Inner0 + x] = innerOut[y * Peaks.InnerN + x];
        var seamAfter = Peaks.SeamRing(world2, N);
        Program.SaveL16(pWorld, world2, N, N);

        var previews = new List<string>();
        foreach (var (r, k) in res.Regions.Take(maxPrev).Select((r, k) => (r, k + 1)))
        {
            int pad = 32, x0 = Math.Max(0, r.x0 - pad), y0 = Math.Max(0, r.y0 - pad), x1 = Math.Min(N - 1, r.x1 + pad), y1 = Math.Min(N - 1, r.y1 + pad);
            var p = Path.Combine(outDir, $"{outBase}_stamp{k}_before-after.png");
            Peaks.SaveSideBySide(p, terrain, res.Output, N, x0, y0, x1 - x0 + 1, y1 - y0 + 1, 900);
            previews.Add(p);
        }
        foreach (var (wx, wz, label, size) in at)
        {
            int cx = (int)Math.Round((wx + 7168) / CellM - 0.5), cz = (int)Math.Round((7168 - wz) / CellM - 0.5), half = (int)(size / CellM / 2);
            int x0 = Math.Clamp(cx - half, 0, N - 1), y0 = Math.Clamp(cz - half, 0, N - 1), x1 = Math.Clamp(cx + half, 0, N - 1), y1 = Math.Clamp(cz + half, 0, N - 1);
            if (x1 <= x0 || y1 <= y0) continue;
            var p = Path.Combine(outDir, $"{outBase}_{label}_before-after.png");
            Peaks.SaveSideBySide(p, terrain, res.Output, N, x0, y0, x1 - x0 + 1, y1 - y0 + 1, 900);
            previews.Add(p);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"# PO stamp: {Path.GetFileName(inPath)} -> {Path.GetFileName(pOut)}\n");
        sb.AppendLine($"Produced {stampTime:yyyy-MM-dd HH:mm} (prefix {prefix}).\n");
        sb.AppendLine("Sources:\n");
        foreach (var s in srcLog) sb.AppendLine($"- {s}");
        sb.AppendLine($"\nStamp where PO top > terrain + {o.MinRiseM} m; smoothstep falloff {o.FalloffPx} px outside (height of the nearest stamped pixel); " +
            $"Gaussian σ {o.SmoothSigmaPx} px near PO steps > {o.StepM} m; slope limit {(o.MaxSlope > 0 ? $"{o.MaxSlope} m/m" : "off")}." +
            (o.Soft is { } so ? $" Softened: Gaussian σ {so.SigmaPx} px over the stamped area (fading across the falloff), {(1 - so.RidgeRaw) * 100:F0}/{so.RidgeRaw * 100:F0} smoothed/raw within {so.RidgeDistPx} px of raw ridges, value noise {so.NoiseMinM}–{so.NoiseMaxM} m scaled by slope at {string.Join("/", so.Wavelengths)} m wavelengths." : "") + "\n");
        if (po.Saturated > 0) sb.AppendLine($"- WARNING: {po.Saturated} PO raster values at 65535 (saturated; check the units per metre).");
        sb.AppendLine($"- footprint px: {res.FootprintPx}; stamped px (> {o.MinRiseM} m above terrain): {res.StampPx}; smoothed px: {res.SmoothedPx}; slope-limited px: {res.SlopeLimitedPx}; pixels changed: {res.ChangedPixels}");
        sb.AppendLine($"- farthest changed pixel from a stamped pixel: {res.MaxChangedDistPx} px; changed outside footprints + margin: {outside} (asserted 0)");
        sb.AppendLine($"- output max: {Peaks.ToM(res.Output.Max(v => (int)v)):F1} m; regions: {res.Regions.Count}");
        sb.AppendLine($"- world map: existing inner vs box_down4(input) mismatches: {innerMismatch}; seam ring before max {seamBefore.maxM:F2} m mean {seamBefore.meanM:F3} m, after max {seamAfter.maxM:F2} m mean {seamAfter.meanM:F3} m\n");
        sb.AppendLine("| key (object id or layer) | source cells/px | changed px | max added (m) | max PO top (m) | bbox px |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (var s in res.Keys)
            sb.AppendLine(string.Format(inv, "| {0} | {1} | {2} | {3:F1} | {4:F1} | {5} |", s.Key, s.SourceCells, s.StampedPx, s.MaxAddedM, s.MaxTopM, s.StampedPx > 0 ? $"({s.X0},{s.Y0})-({s.X1},{s.Y1})" : "-"));
        sb.AppendLine("\nRegions (largest first; previews left before / right after):\n");
        foreach (var (r, k) in res.Regions.Take(maxPrev).Select((r, k) => (r, k + 1)))
            sb.AppendLine($"- stamp{k}: {r.px} px, bbox ({r.x0},{r.y0})-({r.x1},{r.y1}), world x {r.x0 * CellM - 7168:F0}..{(r.x1 + 1) * CellM - 7168:F0}, z {7168 - (r.y1 + 1) * CellM:F0}..{7168 - r.y0 * CellM:F0}");
        foreach (var (wx, wz, label, size) in at) sb.AppendLine($"- {label}: crop {size:F0} m around world ({wx:F0}, {wz:F0})");
        var statsPath = Path.Combine(outDir, outName == null ? "stats.md" : $"{outBase}_stats.md");
        File.WriteAllText(statsPath, sb.ToString());
        Console.Write(sb.ToString());
        Console.WriteLine($"\nwrote {pOut}\nwrote {pWorld}\nwrote {statsPath}");

        if (installDir != null)
        {
            foreach (var p in new[] { pOut, pWorld })
            {
                var dst = Path.Combine(installDir, Path.GetFileName(p));
                if (File.Exists(dst)) throw new InputException($"refusing to overwrite {dst}");
                File.Copy(p, dst);
                Console.WriteLine($"installed {dst}");
            }
        }
        return 0;
    }
}
