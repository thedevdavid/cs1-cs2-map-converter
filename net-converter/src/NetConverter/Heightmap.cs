using System.Buffers.Binary;
using System.IO.Compression;

namespace NetConverter;

/// <summary>
/// The CS2 playable heightmap made by hm-rebuild (4096², 16-bit grayscale, Height Scale 2048 → metres = value / 32),
/// pixel k centred at −7168 + 3.5·k + 1.75 m. Row order is detected against the CS1 node terrain (see <see cref="Orient"/>).
/// </summary>
public sealed class Heightmap
{
    public int Size { get; }
    public double Half { get; }
    public double MetresPerPixel => 2 * Half / Size;
    public double MetresPerUnit { get; }
    readonly ushort[] data;
    /// <summary>True when image row 0 is the north edge (z = +Half).</summary>
    public bool NorthUp { get; private set; } = true;

    public Heightmap(int size, ushort[] values, double half = 7168, double metresPerUnit = 2048.0 / 65536)
    {
        Size = size; data = values; Half = half; MetresPerUnit = metresPerUnit;
    }

    public static Heightmap LoadPng(string path, double half = 7168, double heightScale = 2048)
    {
        var (w, h, v) = ReadGray16(File.ReadAllBytes(path));
        if (w != h) throw new FormatException($"{path}: expected a square heightmap, got {w}x{h}");
        return new Heightmap(w, v, half, heightScale / 65536);
    }

    /// <summary>Bilinear terrain height (metres) at world x/z.</summary>
    public double Sample(double x, double z)
    {
        double px = (x + Half) / MetresPerPixel - 0.5;
        double py = (NorthUp ? Half - z : z + Half) / MetresPerPixel - 0.5;
        px = Math.Clamp(px, 0, Size - 1); py = Math.Clamp(py, 0, Size - 1);
        int x0 = (int)Math.Floor(px), y0 = (int)Math.Floor(py);
        int x1 = Math.Min(x0 + 1, Size - 1), y1 = Math.Min(y0 + 1, Size - 1);
        double fx = px - x0, fy = py - y0;
        double v = data[y0 * Size + x0] * (1 - fx) * (1 - fy) + data[y0 * Size + x1] * fx * (1 - fy)
                 + data[y1 * Size + x0] * (1 - fx) * fy + data[y1 * Size + x1] * fx * fy;
        return v * MetresPerUnit;
    }

    /// <summary>Picks the row order that best matches known terrain heights; returns the mean absolute error in metres.</summary>
    public double Orient(IEnumerable<(double X, double Z, double Terrain)> known)
    {
        var pts = known.Where(p => Math.Abs(p.X) < Half && Math.Abs(p.Z) < Half).ToList();
        if (pts.Count == 0) return double.NaN;
        double Err() => pts.Average(p => Math.Abs(Sample(p.X, p.Z) - p.Terrain));
        NorthUp = true; double up = Err();
        NorthUp = false; double down = Err();
        NorthUp = up <= down;
        return Math.Min(up, down);
    }

    /// <summary>Minimal PNG reader: 16-bit grayscale (or 16-bit RGB/RGBA, red channel), non-interlaced.</summary>
    internal static (int W, int H, ushort[] V) ReadGray16(byte[] png)
    {
        int pos = 8, w = 0, h = 0, colorType = 0, depth = 0;
        using var idat = new MemoryStream();
        while (pos < png.Length)
        {
            int len = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(pos));
            string type = System.Text.Encoding.ASCII.GetString(png, pos + 4, 4);
            var body = png.AsSpan(pos + 8, len);
            if (type == "IHDR")
            {
                w = BinaryPrimitives.ReadInt32BigEndian(body); h = BinaryPrimitives.ReadInt32BigEndian(body[4..]);
                depth = body[8]; colorType = body[9];
                if (body[12] != 0) throw new FormatException("interlaced PNG not supported");
            }
            else if (type == "IDAT") idat.Write(body);
            else if (type == "IEND") break;
            pos += 12 + len;
        }
        if (depth != 16) throw new FormatException($"expected a 16-bit PNG, got {depth}-bit");
        int channels = colorType switch { 0 => 1, 4 => 2, 2 => 3, 6 => 4, _ => throw new FormatException($"colour type {colorType}") };
        int bpp = 2 * channels, stride = w * bpp;
        idat.Position = 0;
        using var z = new ZLibStream(idat, CompressionMode.Decompress);
        var raw = new byte[(stride + 1) * h];
        int read = 0;
        while (read < raw.Length) { int n = z.Read(raw, read, raw.Length - read); if (n == 0) break; read += n; }

        var prev = new byte[stride]; var cur = new byte[stride];
        var values = new ushort[w * h];
        for (int y = 0; y < h; y++)
        {
            int f = raw[y * (stride + 1)];
            Array.Copy(raw, y * (stride + 1) + 1, cur, 0, stride);
            for (int i = 0; i < stride; i++)
            {
                int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                cur[i] = (byte)(cur[i] + f switch
                {
                    0 => 0, 1 => a, 2 => b, 3 => (a + b) / 2,
                    4 => Paeth(a, b, c),
                    _ => throw new FormatException($"filter {f}"),
                });
            }
            for (int x = 0; x < w; x++) values[y * w + x] = (ushort)(cur[x * bpp] << 8 | cur[x * bpp + 1]);
            (prev, cur) = (cur, prev);
        }
        return (w, h, values);
    }

    static int Paeth(int a, int b, int c)
    {
        int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }
}
