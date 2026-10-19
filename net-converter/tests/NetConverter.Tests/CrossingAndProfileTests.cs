using System.Text.Json;
using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Grade separation, bridge profiles, twisted courses, tiny stubs, water import.</summary>
public class CrossingAndProfileTests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");
    const string Hw2 = "2452038713.USRP 2L Interstate_Data", Hw2B = "2452038713.USRP 2L Interstate B", Rail = "2610252544.R2USA W GrWo 2x2_Data";

    static DumpBuilder Base() => new DumpBuilder()
        .Prefab(Hw2, "RoadAI", 2, 0).Prefab(Hw2B, "RoadBridgeAI", 2, 0).Prefab(Rail, "TrainTrackAI", 1, 1).Prefab("Gravel Road", "RoadAI", 1, 1);

    /// <summary>Flat terrain at 100 m over ±1024 m.</summary>
    static Heightmap Flat(double h = 100)
    {
        const int n = 64;
        return new Heightmap(n, Enumerable.Repeat((ushort)Math.Round(h * 32), n * n).ToArray(), 1024);
    }

    [Fact]
    public void GroundCrossingThatCs1HadSeparated_RaisesTheUpperCourse()
    {
        // Rail at y 112 on ground nodes (an embankment in CS1) crosses a highway at y 100; both have only ground nodes.
        var b = Base()
            .Node(1, -200, 0).Node(2, 200, 0)
            .Node(3, 0, -200, y: 112, terrain: 112).Node(4, 0, 200, y: 112, terrain: 112)
            .Seg(Hw2, 1, 2, [-200, 100, 0], [200, 100, 0])
            .Seg(Rail, 3, 4, [0, 112, -200], [0, 112, 200]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat(), MaxElevatedSpan = 1000 });
        Assert.Equal(1, r.R4.Crossings);
        Assert.Equal(1, r.R4.CrossingNodesInserted);
        var rail = r.Layer(Layers.Rail).ToList();
        Assert.Equal(2, rail.Count);
        var mid = rail.Select(e => e.B).First(n => Math.Abs(n.Position.Z) < 1);
        Assert.Equal(12, mid.Elevation, 1);                     // CS1 deck 112 over terrain 100 (≥ 8 m minimum)
        // Ramps away at 4 % for track: 12 − 0.04·200 = 4 m at the ends.
        Assert.All(rail, e => Assert.True(Math.Abs(e.A.Elevation - e.B.Elevation) / e.Curve.LengthXZ() <= 0.0401));
        Assert.Equal(0, r.Layer(Layers.Highways).Single().A.Elevation);
    }

    [Fact]
    public void LowBridgeOverAnotherLayer_IsRaisedToMinimum()
    {
        // Highway bridge only 3 m above a gravel road it crosses; CS1 decks differ by 5 m.
        var b = Base()
            .Node(1, -100, 0, y: 103, elevation: 3).Node(2, 100, 0, y: 103, elevation: 3)
            .Node(3, 0, -100, y: 98).Node(4, 0, 100, y: 98)
            .Seg(Hw2B, 1, 2, [-100, 103, 0], [100, 103, 0])
            .Seg("Gravel Road", 3, 4, [0, 98, -100], [0, 98, 100]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat(), MinBridgeElevation = 8 });
        var mid = r.Layer(Layers.Highways).Select(e => e.B).First(n => Math.Abs(n.Position.X) < 1);
        Assert.Equal(8, mid.Elevation, 1);
    }

    [Fact]
    public void CrossingAtGradeInCs1_IsOnlyListed()
    {
        var b = Base()
            .Node(1, -200, 0).Node(2, 200, 0).Node(3, 0, -200).Node(4, 0, 200)
            .Seg(Hw2, 1, 2, [-200, 100, 0], [200, 100, 0])
            .Seg(Rail, 3, 4, [0, 101, -200], [0, 101, 200]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat() });
        Assert.Equal(1, r.R4.CrossingsAtGradeInCs1);
        Assert.Equal(0, r.R4.CrossingNodesInserted);
    }

    [Fact]
    public void WavyBridge_IsSmoothedWithEndsFixed()
    {
        // Five bridge nodes 80 m apart at 20, 26, 18, 25, 20 m above flat terrain.
        double[] h = [20, 26, 18, 25, 20];
        var b = Base();
        for (int i = 0; i < 5; i++) b.Node(i + 1, i * 80, 0, y: 100 + h[i], elevation: (int)h[i]);
        for (int i = 0; i < 4; i++) b.Seg(Hw2B, i + 1, i + 2, [i * 80, 100 + h[i], 0], [(i + 1) * 80, 100 + h[i + 1], 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat(), MaxElevatedSpan = 1000 });
        var nodes = r.NodesOf(r.Edges).OrderBy(n => n.Position.X).ToList();
        Assert.Equal(20, nodes[0].Elevation, 1);
        Assert.Equal(20, nodes[^1].Elevation, 1);
        for (int i = 1; i + 1 < nodes.Count; i++)
            Assert.True(Math.Abs(nodes[i].Elevation - (nodes[i - 1].Elevation + nodes[i + 1].Elevation) / 2) <= 1.51, $"node {i}");
        Assert.Equal(1, r.R4.BridgeChainsChanged);
    }

    [Fact]
    public void LongRaisedSpan_IsSplitAtTheDeck()
    {
        // A level 160 m bridge at +30 m over flat ground: no deck error, but longer than 75 m, so it is split in two
        // with the new node on the deck (CS2 would otherwise insert its own mid-span node, several metres low).
        var b = Base()
            .Node(1, 0, 0, y: 130, elevation: 30).Node(2, 160, 0, y: 130, elevation: 30)
            .Seg(Hw2B, 1, 2, [0, 130, 0], [160, 130, 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat() });
        Assert.All(r.Edges, e => Assert.True(e.Curve.LengthXZ() <= 75, e.Id));
        Assert.All(r.NodesOf(r.Edges), n => Assert.Equal(130, n.Terrain + n.Elevation, 1));
    }

    [Fact]
    public void SpanSplit_IsSkippedWhereARampMergesAtAShallowAngle()
    {
        // Ramp 1 -> 3 merges into the mainline 2 -> 3 at node 3, converging from 11 m: its midpoint lies 6 m from the mainline.
        var b = Base()
            .Node(1, 0, 11, y: 130, elevation: 30).Node(2, 0, 0, y: 130, elevation: 30).Node(3, 160, 0, y: 130, elevation: 30)
            .Seg(Hw2B, 2, 3, [0, 130, 0], [160, 130, 0])
            .Seg("2452038713.USRP 1L Interstate Ramp E", 1, 3, [0, 130, 11], [160, 130, 0])
            .Prefab("2452038713.USRP 1L Interstate Ramp E", "RoadBridgeAI", 1, 0);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile), new ConvertOptions { Terrain = Flat() });
        Assert.Single(r.Edges, e => e.Cs1Segment == 2);          // the ramp stays one course
        Assert.True(r.Stats.DeckSplitsSkippedNearMerge >= 1);
    }

    [Fact]
    public void TinyDivergeStub_IsCollapsed()
    {
        var b = Base()
            .Node(1, -100, 0).Node(2, 0, 0).Node(3, 100, 0).Node(4, 6, 5)
            .Seg("Gravel Road", 1, 2, [-100, 100, 0], [0, 100, 0])
            .Seg("Gravel Road", 2, 3, [0, 100, 0], [100, 100, 0])
            .Seg("Gravel Road", 2, 4, [0, 100, 0], [6, 100, 5]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        Assert.Equal(1, r.R4.TinyStubsCollapsed);
        Assert.Equal(2, r.Edges.Count);
    }

    [Fact]
    public void WaterImport_HasRadiusFromRateAndSeaLevelFlag()
    {
        var json = """
        {"schema":"x","nodes":[],"segments":[],"netPrefabs":{},
         "waterSources":[{"id":1,"type":"Natural","inputPosition":[10,0,20],"outputPosition":[10,0,20],"targetHeight":202.2,"inputRate":16383,"outputRate":16383,"flow":1},
                         {"id":2,"type":"Natural","inputPosition":[0,0,0],"outputPosition":[0,0,0],"targetHeight":95,"inputRate":100,"outputRate":100,"flow":1}]}
        """;
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Outputs.WriteWaterImport(path, Dump.Parse(json), 7168, 97.5);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var s = doc.RootElement.GetProperty("sources");
            Assert.Equal(128, s[0].GetProperty("radius").GetDouble());
            Assert.Equal(20, s[1].GetProperty("radius").GetDouble());
            Assert.Equal("natural", s[0].GetProperty("type").GetString());
            Assert.False(s[0].GetProperty("belowSeaLevel").GetBoolean());
            Assert.True(s[1].GetProperty("belowSeaLevel").GetBoolean());
        }
        finally { File.Delete(path); }
    }
}
