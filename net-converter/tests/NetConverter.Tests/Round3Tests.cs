using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Round 3: stacked nodes, near-parallel pairs, cross-layer junctions, dead-end reasons.</summary>
public class Round3Tests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");
    static ConvertResult Convert(DumpBuilder b) => Converter.Run(b.Build(), Mapping.Load(MappingFile));

    const string Hw2 = "2452038713.USRP 2L Interstate_Data", Hw2E = "2452038713.USRP 2L Interstate E", Ramp = "2452038713.USRP 1L Interstate Ramp_Data";
    const string Nat4 = "2097539846.USRP 4-Lane National Road_Data";

    static DumpBuilder Base() => new DumpBuilder()
        .Prefab(Hw2, "RoadAI", 2, 0).Prefab(Hw2E, "RoadBridgeAI", 2, 0).Prefab(Ramp, "RoadAI", 1, 0)
        .Prefab(Nat4, "RoadAI", 2, 2).Prefab("Gravel Road", "RoadAI", 1, 1).Prefab("Pedestrian Gravel", "PedestrianPathAI", 0, 0).Prefab("1889660581.Concrete Barrier_Data", "DecorationWallAI", 0, 0);

    [Fact]
    public void StackedNodesAreNotMerged()
    {
        // A ground carriageway passes under an elevated one; their CS1 nodes share x/z (0.2 m apart) but not height.
        var b = Base()
            .Node(1, -100, 0).Node(2, 0, 0).Node(3, 100, 0)
            .Node(4, 0.2, -100, y: 110, elevation: 10).Node(5, 0.2, 0, y: 110, elevation: 10).Node(6, 0.2, 100, y: 110, elevation: 10)
            .Seg(Hw2, 1, 2, [-100, 100, 0], [0, 100, 0]).Seg(Hw2, 2, 3, [0, 100, 0], [100, 100, 0])
            .Seg(Hw2E, 4, 5, [0.2, 110, -100], [0.2, 110, 0]).Seg(Hw2E, 5, 6, [0.2, 110, 0], [0.2, 110, 100]);
        var r = Convert(b);
        Assert.Equal(1, r.Stats.MergesRefusedForHeight);
        Assert.Equal(6, r.NodesOf(r.Edges).Count());
        Assert.Empty(r.AcuteJunctions);
    }

    [Fact]
    public void Diverge_RampTurnsAwayAndMainlineKeepsItsTangent()
    {
        // Mainline 1 -> 2 -> 3 along +x; a ramp leaves node 2 with the same initial tangent and ends 12 m to the side.
        var b = Base()
            .Node(1, -100, 0).Node(2, 0, 0).Node(3, 100, 0).Node(4, 100, 12)
            .Seg(Hw2, 1, 2, [-100, 100, 0], [0, 100, 0]).Seg(Hw2, 2, 3, [0, 100, 0], [100, 100, 0])
            .Seg(Ramp, 2, 4, [0, 100, 0], [100, 100, 12], pb: [33, 100, 0], pc: [66, 100, 10]);
        var r = Convert(b);
        var main = r.Edges.Single(e => e.Cs1Segment == 2);
        var ramp = r.Edges.Single(e => e.Cs1Segment == 3);
        Assert.Equal(new Vec3(33.33, 100, 0).Z, main.Curve.B.Z, 3);   // mainline untouched
        double angle = Math.Atan2(ramp.Curve.B.Z - ramp.Curve.A.Z, ramp.Curve.B.X - ramp.Curve.A.X) * 180 / Math.PI;
        Assert.Equal(12, angle, 1);                                      // turned toward its far end (+z)
        var res = Assert.Single(r.AcuteResolutions);
        Assert.Equal("diverge (tangents spread)", res.Category);
        Assert.Empty(r.AcuteJunctions);
    }

    [Fact]
    public void DuplicateTransitionPieceIsDropped()
    {
        // Ground and elevated copies of the same course from node 1, ending 1.5 m apart.
        var b = Base()
            .Node(1, 0, 0).Node(2, 100, 0).Node(3, 100, 1.5)
            .Seg(Hw2, 1, 2, [0, 100, 0], [100, 100, 0])
            .Seg(Hw2E, 1, 3, [0, 100, 0], [100, 100, 1.5], pb: [33, 100, 0], pc: [66, 100, 1]);
        var r = Convert(b);
        var res = Assert.Single(r.AcuteResolutions);
        Assert.Equal("duplicate (dropped)", res.Category);
        Assert.Single(r.Edges);
        Assert.Equal("duplicate transition piece", r.Fate[r.Edges[0].Cs1Segment == 1 ? 2 : 1]);
    }

    [Fact]
    public void CrossLayerJunction_IsSharedInImportAllAndExplained()
    {
        var b = Base()
            .Node(1, -100, 0).Node(2, 0, 0).Node(3, 0, 100)
            .Node(5, 500, 0).Node(6, 600, 0).Node(7, 700, 0)
            .Seg(Hw2, 1, 2, [-100, 100, 0], [0, 100, 0])
            .Seg(Nat4, 2, 3, [0, 100, 0], [0, 100, 100])
            .Seg("Gravel Road", 5, 6, [500, 100, 0], [600, 100, 0])
            .Seg("1889660581.Concrete Barrier_Data", 6, 7, [600, 100, 0], [700, 100, 0]);
        var r = Convert(b);
        // One node object shared by the highway and the road edge (written once in import-all.json).
        var hw = r.Edges.Single(e => e.Cs1Segment == 1); var road = r.Edges.Single(e => e.Cs1Segment == 2);
        Assert.Same(hw.B, road.A);
        Assert.StartsWith("cross-layer junction (highways + roads)", DeadEnds.Explain(r, new Vec3(0, 100, 0)).Reason);
        Assert.Equal("continues as SKIP: Concrete Barrier_Data", DeadEnds.Explain(r, new Vec3(600, 100, 0)).Reason);
        Assert.Equal("CS1 dead end (genuine)", DeadEnds.Explain(r, new Vec3(0, 100, 100)).Reason);

        var predicted = DeadEnds.Predicted(r, 7168);
        Assert.DoesNotContain(predicted, d => d.Position.DistXZ(new Vec3(0, 0, 0)) < 1);   // junction is not an end
        Assert.Contains(predicted, d => d.Position.DistXZ(new Vec3(600, 0, 0)) < 1);
    }

    [Theory]
    [InlineData("1234.Two-Lane Alley_Data", "Alley")]
    [InlineData("1234.Cul-De-Sac_Data", "Small Road")]
    [InlineData("1234.Zonable Pedestrian Gravel_Data", "Pedestrian Street")]
    [InlineData("Pedestrian Gravel", "Pavement Path")]
    public void Mapping_Round3Rules(string cs1, string? cs2) =>
        Assert.Equal(cs2, Mapping.Load(MappingFile).Map(new NetPrefabInfo(cs1, "", "", "RoadAI", 4, 1, 1)).Cs2Prefab);
}
