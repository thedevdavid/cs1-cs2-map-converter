using System.Buffers.Binary;
using System.IO.Compression;
using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Round 4: CS2 heightmap, deck fit over a canyon.</summary>
public class Round4Tests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");

    /// <summary>A 64² map over ±224 m (7 m/px), metres = value/32, with a V-shaped canyon along x = 0 (100 m deep, 100 m wide).</summary>
    static Heightmap Canyon(bool northUp = true)
    {
        const int n = 64; const double half = 224;
        var v = new ushort[n * n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                double x = -half + (c + 0.5) * 2 * half / n;
                double z = northUp ? half - (r + 0.5) * 2 * half / n : -half + (r + 0.5) * 2 * half / n;
                double h = 150 - Math.Max(0, 100 - 2 * Math.Abs(x)) + 0.01 * z;   // slight north tilt to tell the rows apart
                v[r * n + c] = (ushort)Math.Round(h * 32);
            }
        return new Heightmap(n, v, half);
    }

    [Fact]
    public void Heightmap_SamplesAndDetectsRowOrder()
    {
        var hm = Canyon(northUp: false);
        double err = hm.Orient([(-150, 150, 151.5), (-150, -150, 148.5), (150, 100, 151), (150, -100, 149)]);
        Assert.False(hm.NorthUp);
        Assert.True(err < 0.1, $"err {err}");
        Assert.Equal(57, hm.Sample(0, 0), 0);           // V floor (50) between pixel centres at ±3.5 m
        Assert.Equal(150, hm.Sample(-150, 0), 1);
    }

    [Fact]
    public void Heightmap_ReadsSixteenBitPng()
    {
        // 3x2 gray16 with Sub and Up filters.
        ushort[] px = [100, 200, 65535, 0, 32000, 1];
        var raw = new MemoryStream();
        byte[] Row(int y) => Enumerable.Range(0, 3).SelectMany(x => new[] { (byte)(px[y * 3 + x] >> 8), (byte)px[y * 3 + x] }).ToArray();
        var r0 = Row(0); var r1 = Row(1);
        raw.WriteByte(1); for (int i = 0; i < 6; i++) raw.WriteByte((byte)(r0[i] - (i >= 2 ? r0[i - 2] : 0)));   // Sub
        raw.WriteByte(2); for (int i = 0; i < 6; i++) raw.WriteByte((byte)(r1[i] - r0[i]));                      // Up
        var z = new MemoryStream();
        using (var zs = new ZLibStream(z, CompressionLevel.Optimal, leaveOpen: true)) zs.Write(raw.ToArray());
        var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        void Chunk(string t, byte[] d)
        {
            var len = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(len, d.Length);
            png.Write(len); png.Write(System.Text.Encoding.ASCII.GetBytes(t)); png.Write(d); png.Write(new byte[4]); // CRC not checked
        }
        var ihdr = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(ihdr, 3); BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), 2); ihdr[8] = 16;
        Chunk("IHDR", ihdr); Chunk("IDAT", z.ToArray()); Chunk("IEND", []);
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(path, png.ToArray());
        try { Assert.Throws<FormatException>(() => Heightmap.LoadPng(path)); } // not square
        finally { File.Delete(path); }
    }

    [Fact]
    public void DeckFit_SplitsAFlatBridgeOverACanyon()
    {
        // A flat 3-lane deck at y = 160 crossing the canyon from x = -120 to x = 120 (CS1 bridge piece).
        var b = new DumpBuilder().Prefab("2452038713.USRP 3L Interstate B", "RoadBridgeAI", 3, 0)
            .Node(1, -120, 0, y: 160, terrain: 150, elevation: 10).Node(2, 120, 0, y: 160, terrain: 150, elevation: 10)
            .Seg("2452038713.USRP 3L Interstate B", 1, 2, [-120, 160, 0], [120, 160, 0]);
        var hm = Canyon();
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = hm });
        Assert.True(r.Edges.Count > 1, "the canyon crossing must be split");
        Assert.Equal(1, r.Stats.DeckEdgesSplit);
        // Every node of the deck sits at y = 160 above the CS2 terrain.
        foreach (var n in r.NodesOf(r.Edges)) Assert.Equal(160, n.Terrain + n.Elevation, 1);
        var mid = r.NodesOf(r.Edges).Single(n => Math.Abs(n.Position.X) < 1);
        Assert.Equal(160 - hm.Sample(0, 0), mid.Elevation, 1);   // ~103 m over the canyon floor
        // Predicted control-point heights are within tolerance or the piece is at minimum length.
        foreach (var e in r.Edges)
        {
            foreach (var (p, t) in new[] { (e.Curve.B, 1 / 3.0), (e.Curve.C, 2 / 3.0) })
            {
                double got = hm.Sample(p.X, p.Z) + e.A.Elevation + (e.B.Elevation - e.A.Elevation) * t;
                Assert.True(Math.Abs(got - 160) <= 2 || e.Curve.LengthXZ() < 30, $"{e.Id}: {got:F1}");
            }
        }
    }

    [Fact]
    public void DeckFit_LeavesGroundCoursesAlone()
    {
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1)
            .Node(1, -120, 0, y: 150).Node(2, 120, 0, y: 150)
            .Seg("Gravel Road", 1, 2, [-120, 150, 0], [120, 150, 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Canyon() });
        Assert.Single(r.Edges);
        Assert.Equal(0, r.Stats.DeckEdgesSplit);
    }
}
