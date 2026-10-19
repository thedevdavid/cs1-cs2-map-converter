using System.Buffers.Binary;
using System.IO.Compression;

namespace NetConverter;

/// <summary>Minimal RGBA canvas with anti-aliased thick lines and a dependency-free PNG encoder.</summary>
public sealed class Canvas(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public byte[] Rgba { get; } = new byte[width * height * 4];

    /// <summary>Thick line from (x0,y0) to (x1,y1) in pixels, alpha-blended ("over") with 1 px anti-aliasing.</summary>
    public void Line(double x0, double y0, double x1, double y1, double halfWidth, (byte R, byte G, byte B, byte A) c)
    {
        int minX = Math.Max(0, (int)Math.Floor(Math.Min(x0, x1) - halfWidth - 1));
        int maxX = Math.Min(Width - 1, (int)Math.Ceiling(Math.Max(x0, x1) + halfWidth + 1));
        int minY = Math.Max(0, (int)Math.Floor(Math.Min(y0, y1) - halfWidth - 1));
        int maxY = Math.Min(Height - 1, (int)Math.Ceiling(Math.Max(y0, y1) + halfWidth + 1));
        double dx = x1 - x0, dy = y1 - y0, len2 = dx * dx + dy * dy;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                double px = x + 0.5 - x0, py = y + 0.5 - y0;
                double t = len2 > 0 ? Math.Clamp((px * dx + py * dy) / len2, 0, 1) : 0;
                double ex = px - t * dx, ey = py - t * dy;
                double cov = Math.Clamp(halfWidth + 0.5 - Math.Sqrt(ex * ex + ey * ey), 0, 1);
                if (cov <= 0) continue;
                Blend(x, y, c, cov);
            }
    }

    void Blend(int x, int y, (byte R, byte G, byte B, byte A) c, double cov)
    {
        int i = (y * Width + x) * 4;
        double sa = c.A / 255.0 * cov, da = Rgba[i + 3] / 255.0;
        double oa = sa + da * (1 - sa);
        if (oa <= 0) return;
        Rgba[i] = (byte)Math.Round((c.R * sa + Rgba[i] * da * (1 - sa)) / oa);
        Rgba[i + 1] = (byte)Math.Round((c.G * sa + Rgba[i + 1] * da * (1 - sa)) / oa);
        Rgba[i + 2] = (byte)Math.Round((c.B * sa + Rgba[i + 2] * da * (1 - sa)) / oa);
        Rgba[i + 3] = (byte)Math.Round(oa * 255);
    }

    public void SavePng(string path)
    {
        using var fs = File.Create(path);
        fs.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr, Width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), Height);
        ihdr[8] = 8; ihdr[9] = 6; // 8-bit RGBA
        Chunk(fs, "IHDR", ihdr);
        using (var raw = new MemoryStream())
        {
            using (var z = new ZLibStream(raw, CompressionLevel.Optimal, leaveOpen: true))
            {
                int stride = Width * 4;
                for (int y = 0; y < Height; y++) { z.WriteByte(0); z.Write(Rgba, y * stride, stride); }
            }
            Chunk(fs, "IDAT", raw.ToArray());
        }
        Chunk(fs, "IEND", []);
    }

    static void Chunk(Stream s, string type, byte[] data)
    {
        Span<byte> len = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(len, data.Length);
        s.Write(len);
        var t = System.Text.Encoding.ASCII.GetBytes(type);
        s.Write(t); s.Write(data);
        uint crc = Crc(Crc(0xFFFFFFFF, t), data) ^ 0xFFFFFFFF;
        BinaryPrimitives.WriteUInt32BigEndian(len, crc);
        s.Write(len);
    }

    static readonly uint[] Table = Enumerable.Range(0, 256).Select(n =>
    {
        uint c = (uint)n;
        for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
        return c;
    }).ToArray();

    static uint Crc(uint crc, byte[] data)
    {
        foreach (var b in data) crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc;
    }
}
