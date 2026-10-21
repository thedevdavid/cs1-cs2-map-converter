using NetConverter;

namespace NetConverter.Tests;

public class ConverterTests
{
    static readonly string Fixture = Path.Combine(AppContext.BaseDirectory, "fixtures", "synthetic.json");
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");

    static ConvertResult Run() => Converter.Run(Dump.Load(Fixture), Mapping.Load(MappingFile));

    [Fact]
    public void Clip_SplitsAtBorderAndPutsCutNodeExactlyOnIt()
    {
        var r = Run();
        var e = Assert.Single(r.Edges, x => x.Cs1Segment == 1);
        Assert.Equal("n1", e.A.Id);
        Assert.True(e.B.Border);
        Assert.Equal(7168.0, e.B.Position.X);   // exact, not approximately
        Assert.Equal(0.0, e.B.Position.Z, 6);
        Assert.Equal(e.B.Position, e.Curve.D);
        Assert.InRange(e.Curve.LengthXZ(), 167.9, 168.1);
        Assert.Equal(1, r.Stats.BorderNodesPerLayer[Layers.Roads]);
    }

    [Fact]
    public void Clip_DropsSegmentsWhollyOutside()
    {
        var r = Run();
        Assert.DoesNotContain(r.Edges, x => x.Cs1Segment == 8);
        Assert.Equal(1, r.Stats.SegmentsOutside);
    }

    [Fact]
    public void Clip_CurveThatLeavesAndReentersGivesTwoPieces()
    {
        var b = new Bezier(new(7000, 0, 0), new(7400, 0, 300), new(7400, 0, 600), new(7000, 0, 900));
        var iv = Clipper.InsideIntervals(b, 7168);
        Assert.Equal(2, iv.Count);
        Assert.Equal(0, iv[0].T0);
        Assert.Equal(1, iv[1].T1);
        Assert.Equal(7168, b.Eval(iv[0].T1).X, 6);
    }

    [Fact]
    public void Merge_NodesCloserThanHalfMetreBecomeOne_AndShortEdgeCollapses()
    {
        var r = Run();
        var e2 = Assert.Single(r.Edges, x => x.Cs1Segment == 2);
        var e4 = Assert.Single(r.Edges, x => x.Cs1Segment == 4);
        // 11 and 12 are 0.3 m apart -> merged; segment 3 (2 m) collapses, so 2 and 4 share a node.
        Assert.DoesNotContain(r.Edges, x => x.Cs1Segment == 3);
        Assert.Same(e2.B, e4.A);
        Assert.Equal("n11", e2.B.Id);
        Assert.Equal(e2.B.Position, e4.Curve.A);
    }

    [Fact]
    public void Bridge_GetsElevationAndOneWayInvertReversesDirection()
    {
        var r = Run();
        var e = Assert.Single(r.Edges, x => x.Cs1Segment == 5);
        Assert.Equal(Layers.Highways, e.Layer);
        Assert.Equal("Highway Oneway - 2 lanes", e.Map.Cs2Prefab);
        Assert.Equal(Structure.Elevated, e.Structure);
        // invert = true: forward lanes run end -> start, so a = CS1 end node 21.
        Assert.Equal("n21", e.A.Id);
        Assert.Equal(20, e.A.Elevation);
        Assert.Equal(0, e.B.Elevation);
        Assert.Equal(new Vec3(100, 140, 500), e.Curve.A);
    }

    [Fact]
    public void Mapping_UnknownPrefabIsReportedWithSuggestionAndNotImported()
    {
        var r = Run();
        var m = r.Maps["Mystery Road"];
        Assert.Equal(Layers.Unmapped, m.Layer);
        Assert.Null(m.Cs2Prefab);
        Assert.False(m.Imported);
        Assert.Equal("Medium Road", m.Suggestion);
        Assert.Contains(r.Edges, x => x.Cs1Segment == 6 && x.Layer == Layers.Unmapped);
        Assert.Contains("Mystery Road", Outputs.Stats(r, "d", "m"));
    }

    [Fact]
    public void Mapping_SkipAndFirstMatchWins()
    {
        var m = Mapping.Parse([
            "# comment",
            "(?i)Ramp\tHighway Oneway - 1 lane\thighways",
            "(?i)Interstate\tHighway Oneway - 3 lanes\thighways",
            "(?i)Fence\tSKIP\tskip\tdecoration",
            "(?i)kV\tSKIP\tpower",
        ]);
        NetPrefabInfo P(string n) => new(n, "Road", "None", "RoadAI", 5, 1, 0);
        Assert.Equal("Highway Oneway - 1 lane", m.Map(P("USRP 1L Interstate Ramp_Data")).Cs2Prefab);
        Assert.Equal("Highway Oneway - 3 lanes", m.Map(P("USRP 3L Interstate_Data")).Cs2Prefab);
        var fence = m.Map(P("Fence"));
        Assert.Null(fence.Cs2Prefab);
        Assert.Equal(Layers.Skip, fence.Layer);
        Assert.Equal(Layers.Power, m.Map(P("345 kV H Frame")).Layer);
        Assert.Equal(Layers.Unmapped, m.Map(P("Something Else")).Layer);
    }

    [Theory]
    [InlineData("X", "TrainTrackAI", 1, 1, "Double Train Track")]
    [InlineData("X", "TrainTrackAI", 1, 0, "Oneway Train Track")]
    [InlineData("Some Interstate", "RoadAI", 3, 0, "Highway Oneway - 3 lanes")]
    [InlineData("Some Interstate Ramp", "RoadAI", 1, 0, "Highway Oneway - 1 lane")]
    [InlineData("Some Road", "RoadAI", 1, 1, "Small Road")]
    [InlineData("Some Road", "RoadAI", 3, 3, "Large Road")]
    [InlineData("Field", "RoadAI", 0, 0, "SKIP (no vehicle lanes)")]
    [InlineData("Wall", "DecorationWallAI", 0, 0, "SKIP")]
    public void Mapping_FallbackSuggestion(string name, string ai, int f, int b, string expected) =>
        Assert.Equal(expected, Mapping.Suggest(new NetPrefabInfo(name, "", "", ai, 4, f, b)));

    [Theory]
    [InlineData("2452038713.USRP 1L Interstate Ramp E", "Highway Oneway - 1 lane")]
    [InlineData("2452038713.USRP 3L Interstate S", "Highway Oneway - 3 lanes")]
    [InlineData("2452038713.USRP 4L Interstate_Data", "Highway Oneway - 4 lanes")]
    [InlineData("Two Lane Highway Twoway Connection", "Highway Twoway - 2 lanes")]
    [InlineData("2610252544.R2USA W GrWo 2x2 T0", "Double Train Track")]
    [InlineData("Train Connection Track", "Double Train Track")]
    [InlineData("Gravel Road Elevated", "Gravel Road")]
    [InlineData("2097539846.USRP 4-Lane National Road_Data", "Highway Twoway - 4 lanes")]
    [InlineData("2966434181.Retaining Walls - AiO #01_Data", "QuaySmall01")]
    [InlineData("2674210208.Field Wheat Network 3_Data", null)]
    [InlineData("Monorail Oneway Track", "Arc Pedestrian Bridge")]
    public void MappingFile_MapsRealPrefabNames(string cs1, string? cs2)
    {
        var r = Mapping.Load(MappingFile).Map(new NetPrefabInfo(cs1, "", "", "RoadAI", 4, 1, 1));
        Assert.NotEqual(Layers.Unmapped, r.Layer);
        Assert.Equal(cs2, r.Cs2Prefab);
    }

    [Fact]
    public void Png_WritesValidSignatureAndSize()
    {
        var c = new Canvas(8, 8);
        c.Line(0, 4, 8, 4, 1, (255, 0, 0, 255));
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.png");
        c.SavePng(path);
        var bytes = File.ReadAllBytes(path);
        File.Delete(path);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes[..4]);
        Assert.Equal(255, c.Rgba[(4 * 8 + 3) * 4 + 3]);
        Assert.Equal(0, c.Rgba[3]);
    }
}
