using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>audit/cs2-side.md "Before the run" converter items.</summary>
public class AuditFixTests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");

    static Dump WaterDump(params (int Id, double X, double Z, double Level, long Rate)[] s)
    {
        var json = "{\"nodes\":[],\"segments\":[],\"netPrefabs\":{},\"waterSources\":[" + string.Join(",", s.Select(w =>
            System.FormattableString.Invariant($"{{\"id\":{w.Id},\"type\":\"Natural\",\"inputPosition\":[{w.X},0,{w.Z}],\"outputPosition\":[{w.X},0,{w.Z}],\"targetHeight\":{w.Level},\"inputRate\":{w.Rate},\"outputRate\":{w.Rate},\"flow\":1}}"))) + "]}";
        return Dump.Parse(json);
    }

    [Fact]
    public void Water_DropsBelowSeaClampsNearBorderDropsFarOutside()
    {
        var d = WaterDump((1, 0, 0, 97.9, 400), (2, 0, 100, 150, 400), (3, 7300, 0, 150, 400), (4, 8000, 0, 150, 400));
        var (src, log) = Water.Plan(d, 97.5, 2, 7168, (x, z) => 149.5, null, false);
        Assert.DoesNotContain(src, s => s.Id == "1");             // 97.9 × 2 ≤ 97.5 × 2 + 1
        Assert.Contains(src, s => s.Id == "2");
        var moved = src.Single(s => s.Id == "3");
        Assert.Equal(7168 - 20 - 14, moved.X, 3);                   // radius 20 (√400)
        Assert.Equal(150.5, moved.HeightCs1 * 1, 2);                // raised to ground + 2 m at × 2 → (149.5·2 + 2)/2
        Assert.DoesNotContain(src, s => s.Id == "4");             // 832 m outside
        Assert.Equal(3, log.Count);
    }

    [Fact]
    public void Water_MergesCanalFeeds()
    {
        var d = WaterDump((5, 1200, 4500, 280, 100), (6, 1250, 4520, 280.4, 100), (7, 1300, 4540, 285, 100));
        var (src, _) = Water.Plan(d, 97.5, 1, 7168, null, null, true);
        Assert.Equal(2, src.Count);                                // 5+6 merged, 7 differs by > 1 m
        Assert.Contains(src, s => s.Id == "5+6");
    }

    [Fact]
    public void ShortBorderEdgeIsFoldedIntoItsNeighbour()
    {
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1)
            .Node(1, 7000, 0).Node(2, 7164, 0).Node(3, 7300, 0)
            .Seg("Gravel Road", 1, 2, [7000, 100, 0], [7164, 100, 0])
            .Seg("Gravel Road", 2, 3, [7164, 100, 0], [7300, 100, 0]);
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        var e = Assert.Single(r.Edges);
        Assert.True(e.B.Border);
        Assert.Equal(7168, e.B.Position.X, 3);
        Assert.Equal(1, r.R4.BorderStubsFolded);
    }

    [Fact]
    public void OverlappingSquaresUnionIntoOneHoleFreePolygon()
    {
        List<(double X, double Z)> Sq(double x, double z) => [(x, z), (x + 16, z), (x + 16, z + 16), (x, z + 16)];
        var polys = new List<SurfacePolygon> { new("a", "P", Sq(0, 0), ""), new("b", "P", Sq(8, 8), ""), new("c", "P", Sq(100, 100), ""), new("d", "Q", Sq(4, 4), "") };
        var (res, pairs, groups) = Surfaces.UnionOverlapping(polys, 1);
        Assert.Equal(1, pairs);                                    // only same-prefab pairs
        Assert.Equal(1, groups);
        Assert.Equal(3, res.Count);
        var u = res.Single(p => p.Id.StartsWith("au") || p.Id.StartsWith("bu"));
        Assert.InRange(Math.Abs(Surfaces.Area(u.Ring)), 16 * 16 * 2 - 64 - 40, 16 * 16 * 2 - 64 + 40);
    }

    [Fact]
    public void OverlappingCirclesShrinkToTouch()
    {
        var circles = new List<(double Cx, double Cz, double R, List<(double X, double Z)> Ring)> { (0, 0, 100, []), (150, 0, 100, []) };
        Assert.Equal(1, Surfaces.SeparateCircles(circles));
        Assert.True(circles[0].R + circles[1].R <= 150);
        Assert.Equal(48, circles[0].Ring.Count);
    }

    [Fact]
    public void TreeMaskCountsPerRule()
    {
        var t = new TreeResult();
        foreach (var x in new[] { 1.0, 2.0, 3.0 }) { t.Objects.Add(new TreeObject("OakTree01", x, 0, "adult", "x")); t.Cs2Total["OakTree01"] = t.Cs2Total.GetValueOrDefault("OakTree01") + 1; }
        var m = Trees.Mask(t, ("low", (x, z) => x < 1.5), ("net", (x, z) => x > 2.5));
        Assert.Equal(1, m["low"]); Assert.Equal(1, m["net"]);
        Assert.Single(t.Objects);
        Assert.Equal(1, t.Cs2Total["OakTree01"]);
    }

    [Fact]
    public void PathSnap_JoinsPathEndBesideRoad_SkipsFarOrSteep()
    {
        var b = new DumpBuilder().Prefab("Basic Road", "RoadAI", 1, 1).Prefab("Pedestrian Gravel", "PedestrianPathAI", 0, 0, 2)
            .Node(1, 0, 0).Node(2, 300, 0).Node(3, 100, 10).Node(4, 100, 60).Node(5, 200, 30).Node(6, 200, 90)
            .Seg("Basic Road", 1, 2, [0, 100, 0], [300, 100, 0])
            .Seg("Pedestrian Gravel", 3, 4, [100, 100, 10], [100, 100, 60])     // end 10 m beside the road
            .Seg("Pedestrian Gravel", 5, 6, [200, 100, 30], [200, 100, 90]);    // end 30 m away: too far
        var r = Converter.Run(b.Build(), Mapping.Load(MappingFile));
        var log = PathSnap.Apply(r, 2);
        var snapped = Assert.Single(log);
        var n = r.Edges.SelectMany(e => new[] { e.A, e.B }).Single(x => x.Snap != null);
        Assert.Equal(100, n.Position.X, 1); Assert.Equal(10, n.Position.Z, 1);
        Assert.Equal(("Small Road", 12.0), (n.Snap!.Value.Prefab, n.Snap.Value.Radius));
    }
}
