using System.Text.Json;
using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Height factor, absolute output, deck grade-change smoothing, tunnel/low-deck transitions.</summary>
public class HeightScaleTests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");
    const string Hw2 = "2452038713.USRP 2L Interstate_Data", Hw2B = "2452038713.USRP 2L Interstate B", Hw2T = "2452038713.USRP 2L Interstate T";

    static DumpBuilder Base() => new DumpBuilder().Prefab(Hw2, "RoadAI", 2, 0).Prefab(Hw2B, "RoadBridgeAI", 2, 0).Prefab(Hw2T, "RoadTunnelAI", 2, 0);

    [Fact]
    public void HeightFactor_DoublesHeightsAndAbsoluteOutputCarriesY()
    {
        var b = Base()
            .Node(1, 0, 0).Node(2, 100, 0, y: 110, elevation: 10).Node(3, 200, 0)
            .Seg(Hw2B, 1, 2, [0, 100, 0], [100, 110, 0]).Seg(Hw2B, 2, 3, [100, 110, 0], [200, 100, 0]);
        var dump = b.Build();
        dump.ScaleHeights(2);
        var r = Converter.Run(dump, Mapping.Load(MappingFile));
        var mid = r.NodesOf(r.Edges).Single(n => n.Id == "n2");
        Assert.Equal(200, mid.Terrain, 1);
        Assert.Equal(20, mid.Elevation, 1);

        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Outputs.WriteImport(path, "t", r.Edges, absolute: true);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal("absolute", doc.RootElement.GetProperty("options").GetProperty("heightMode").GetString());
            var nodes = doc.RootElement.GetProperty("nodes").EnumerateArray().ToDictionary(n => n.GetProperty("id").GetString()!);
            Assert.Equal(220, nodes["n2"].GetProperty("y").GetDouble(), 1);
            Assert.False(nodes["n2"].TryGetProperty("elevation", out _));        // raised: height from y
            Assert.Equal(0, nodes["n1"].GetProperty("elevation").GetDouble());   // ground: explicit 0
            Assert.All(doc.RootElement.GetProperty("edges").EnumerateArray(), e => Assert.False(e.TryGetProperty("elevation", out _)));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DeckSmoothing_RemovesGradeFlipsAboveOnePercent()
    {
        double[] h = [20, 26, 18, 25, 22, 27, 20];
        var b = Base();
        for (int i = 0; i < h.Length; i++) b.Node(i + 1, i * 80, 0, y: 100 + h[i], elevation: (int)h[i]);
        for (int i = 0; i + 1 < h.Length; i++) b.Seg(Hw2B, i + 1, i + 2, [i * 80, 100 + h[i], 0], [(i + 1) * 80, 100 + h[i + 1], 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        var n = r.NodesOf(r.Edges).OrderBy(x => x.Position.X).Select(x => x.Elevation).ToList();
        Assert.Equal(20, n[0], 1); Assert.Equal(20, n[^1], 1);
        double G(int i) => (n[i + 1] - n[i]) / 80 - (n[i] - n[i - 1]) / 80;
        for (int i = 1; i + 2 < n.Count; i++)
            Assert.False(Math.Abs(G(i)) > 0.011 && Math.Abs(G(i + 1)) > 0.011 && Math.Sign(G(i)) != Math.Sign(G(i + 1)), $"flip at node {i}: {string.Join(", ", n)}");
    }

    [Fact]
    public void DeckSmoothing_KeepsASingleHump()
    {
        var b = Base()
            .Node(1, 0, 0).Node(2, 100, 0, y: 110, elevation: 10).Node(3, 200, 0)
            .Seg(Hw2B, 1, 2, [0, 100, 0], [100, 110, 0]).Seg(Hw2B, 2, 3, [100, 110, 0], [200, 100, 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        Assert.Equal(10, r.NodesOf(r.Edges).Single(x => x.Id == "n2").Elevation, 1);
    }

    [Fact]
    public void TunnelTransition_HasPortalAtZeroAndNoShallowNodes()
    {
        // Ground -> -1.2 -> -5.5 -> -10 -> -13 (CS1 T/S pieces), as at (3713, 4676).
        double[] e = [0, -1.2, -5.5, -10, -13, -13];
        var b = Base();
        for (int i = 0; i < e.Length; i++) b.Node(i + 1, i * 80, 0, y: 100 + e[i], flags: e[i] < 0 ? ["Underground"] : []);
        for (int i = 0; i + 1 < e.Length; i++) b.Seg(i == 0 ? Hw2 : Hw2T, i + 1, i + 2, [i * 80, 100 + e[i], 0], [(i + 1) * 80, 100 + e[i + 1], 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        var n = r.NodesOf(r.Edges).OrderBy(x => x.Position.X).Select(x => x.Elevation).ToList();
        Assert.DoesNotContain(n, v => v < 0 && v > -8);
        // Exactly one course goes from the portal (0) to the first tunnel node.
        Assert.Equal(1, Enumerable.Range(0, n.Count - 1).Count(i => (n[i] == 0) != (n[i + 1] == 0)));
        Assert.True(r.R4.PortalsSetToZero >= 1);
    }

    [Fact]
    public void LowDeckNodeNextToBridge_GoesToTwoMetres()
    {
        var b = Base()
            .Node(1, 0, 0, y: 115, elevation: 15).Node(2, 100, 0, y: 101.5, elevation: 1).Node(3, 200, 0)
            .Seg(Hw2B, 1, 2, [0, 115, 0], [100, 101.5, 0]).Seg(Hw2, 2, 3, [100, 101.5, 0], [200, 100, 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        Assert.Equal(2, r.NodesOf(r.Edges).Single(x => x.Id == "n2").Elevation);
    }
}
