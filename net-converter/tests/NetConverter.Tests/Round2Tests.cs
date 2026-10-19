using System.Text.Json;
using System.Text.Json.Nodes;
using NetConverter;

namespace NetConverter.Tests;

/// <summary>Round 2: hairpins, node-elevation structure, ramps, duplicates, overlaps, reconciliation.</summary>
public class Round2Tests
{
    static readonly string MappingFile = Path.Combine(AppContext.BaseDirectory, "mapping.tsv");

    // Small builder for synthetic dumps.
    internal sealed class DumpBuilder
    {
        readonly List<object> nodes = new(), segs = new();
        readonly Dictionary<string, object> prefabs = new();
        int nextSeg = 1;

        public DumpBuilder Prefab(string name, string ai, int fwd, int bwd, double halfWidth = 6)
        {
            prefabs[name] = new { service = "Road", subService = "None", ai, halfWidth, forwardVehicleLanes = fwd, backwardVehicleLanes = bwd };
            return this;
        }

        public DumpBuilder Node(int id, double x, double z, double y = 100, double terrain = 100, int elevation = 0, params string[] flags)
        {
            nodes.Add(new { id, prefab = "x", position = new[] { x, y, z }, elevation, terrain, flags });
            return this;
        }

        public DumpBuilder Seg(string prefab, int a, int b, double[] pa, double[] pd, double[]? pb = null, double[]? pc = null, bool invert = false)
        {
            pb ??= Lerp(pa, pd, 1 / 3.0); pc ??= Lerp(pa, pd, 2 / 3.0);
            segs.Add(new { id = nextSeg++, prefab, startNode = a, endNode = b, invert, bezier = new[] { pa, pb, pc, pd }, length = 0,
                startDirection = new[] { 0.0, 0, 0 }, endDirection = new[] { 0.0, 0, 0 } });
            return this;
        }

        static double[] Lerp(double[] a, double[] b, double t) => [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t];

        public Dump Build() => Dump.Parse(JsonSerializer.Serialize(new { schema = "dakota-export/cs1/v1", nodes, segments = segs, netPrefabs = prefabs }));
    }

    static ConvertResult Convert(DumpBuilder b) => Converter.Run(b.Build(), Mapping.Load(MappingFile));

    [Fact]
    public void Hairpin_IsDetectedAndRebuilt()
    {
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1)
            .Node(1, 0, 0).Node(2, 50, 0)
            // Control points thrown far behind the ends: the curve folds back on itself.
            .Seg("Gravel Road", 1, 2, [0, 100, 0], [50, 100, 0], pb: [120, 100, 40], pc: [-70, 100, 40]);
        Assert.True(Converter.IsHairpin(b.Build().Segments[0].Bezier, out _));
        var r = Convert(b);
        Assert.Equal(1, r.Stats.HairpinsFound);
        var e = Assert.Single(r.Edges);
        Assert.True(e.HairpinFixed);
        Assert.False(Converter.IsHairpin(e.Curve, out _));
    }

    [Fact]
    public void Hairpin_NormalCurveIsLeftAlone()
    {
        var bz = new Bezier(new(0, 0, 0), new(30, 0, 0), new(60, 0, 10), new(90, 0, 40));
        Assert.False(Converter.IsHairpin(bz, out _));
    }

    [Theory]
    [InlineData("2452038713.USRP 2L Interstate E", "Highway Oneway - 2 lanes")]
    [InlineData("2452038713.USRP 2L Interstate B", "Highway Oneway - 2 lanes")]
    [InlineData("2452038713.USRP 2L Interstate S", "Highway Oneway - 2 lanes")]
    [InlineData("2452038713.USRP 2L Interstate T", "Highway Oneway - 2 lanes")]
    [InlineData("2452038713.USRP 2L Interstate + Exit Lane_Data", "Highway Oneway - 3 lanes")]
    [InlineData("2452038713.USRP 2L Interstate + 2 Exit Lanes E", "Highway Oneway - 4 lanes")]
    [InlineData("2452038713.USRP 3L Interstate + Exit Lane_Data", "Highway Oneway - 4 lanes")]
    [InlineData("2452038713.USRP 4L Interstate + 2 Exit Lanes_Data", "Highway Oneway - 5 lanes")]
    public void Usrp_SuffixesKeepBaseAndExitLanesAddLanes(string cs1, string cs2) =>
        Assert.Equal(cs2, Mapping.Load(MappingFile).Map(new NetPrefabInfo(cs1, "", "", "RoadAI", 6, 2, 0)).Cs2Prefab);

    [Fact]
    public void Structure_ComesFromNodeElevationNotSuffix()
    {
        // An "E" (elevated) variant lying on the ground is ground; a plain variant on raised nodes is elevated.
        var b = new DumpBuilder()
            .Prefab("2452038713.USRP 2L Interstate E", "RoadBridgeAI", 2, 0)
            .Prefab("2452038713.USRP 2L Interstate_Data", "RoadAI", 2, 0)
            .Node(1, 0, 0).Node(2, 100, 0)
            .Node(3, 0, 300, y: 115, elevation: 15).Node(4, 100, 300, y: 115, elevation: 15)
            .Seg("2452038713.USRP 2L Interstate E", 1, 2, [0, 100, 0], [100, 100, 0])
            .Seg("2452038713.USRP 2L Interstate_Data", 3, 4, [0, 115, 300], [100, 115, 300]);
        var r = Convert(b);
        Assert.Equal(Structure.Ground, r.Edges.Single(e => e.Cs1Segment == 1).Structure);
        var high = r.Edges.Single(e => e.Cs1Segment == 2);
        Assert.Equal(Structure.Elevated, high.Structure);
        Assert.Equal(15, high.A.Elevation);
    }

    [Fact]
    public void Ramp_SteepEdgeIsSpreadOverGroundChainAtEightPercent()
    {
        // Bridge node at +20 m, then a 30 m edge to the ground and three 100 m ground edges.
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1).Prefab("Gravel Road Bridge", "RoadBridgeAI", 1, 1)
            .Node(1, -100, 0, y: 120, elevation: 20).Node(2, 0, 0, y: 120, elevation: 20)
            .Node(3, 30, 0).Node(4, 130, 0).Node(5, 230, 0).Node(6, 330, 0)
            .Seg("Gravel Road Bridge", 1, 2, [-100, 120, 0], [0, 120, 0])
            .Seg("Gravel Road", 2, 3, [0, 120, 0], [30, 100, 0])
            .Seg("Gravel Road", 3, 4, [30, 100, 0], [130, 100, 0])
            .Seg("Gravel Road", 4, 5, [130, 100, 0], [230, 100, 0])
            .Seg("Gravel Road", 5, 6, [230, 100, 0], [330, 100, 0]);
        var r = Convert(b);
        Assert.Equal(1, r.Stats.RampsSmoothed);
        var n = r.Edges.SelectMany(e => new[] { e.A, e.B }).Distinct().ToDictionary(x => x.Id);
        Assert.Equal(20, n["n2"].Elevation);
        Assert.Equal(20 - 0.08 * 30, n["n3"].Elevation, 1);   // 17.6
        Assert.Equal(20 - 0.08 * 130, n["n4"].Elevation, 1);  // 9.6
        Assert.Equal(2, n["n5"].Elevation, 1);                // 1.6 by the ramp, then lifted to the 2 m minimum next to a raised node
        Assert.Equal(0, n["n6"].Elevation);                   // 250 m out the ramp has reached the ground
        foreach (var e in r.Edges)
            Assert.True(Math.Abs(e.A.Elevation - e.B.Elevation) / e.Curve.LengthXZ() <= 0.085, e.Id);
    }

    [Fact]
    public void Duplicates_AreDroppedAndParallelUnconnectedOverlapIsReported()
    {
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1)
            .Node(1, 0, 0).Node(2, 100, 0).Node(3, 100.2, 0.1).Node(4, 0.2, 0.1)
            .Node(5, 0, 300).Node(6, 100, 300).Node(7, 0, 303).Node(8, 100, 303)
            .Seg("Gravel Road", 1, 2, [0, 100, 0], [100, 100, 0])
            .Seg("Gravel Road", 3, 4, [100.2, 100, 0.1], [0.2, 100, 0.1])      // same course, reversed, ends merge
            .Seg("Gravel Road", 5, 6, [0, 100, 300], [100, 100, 300])
            .Seg("Gravel Road", 7, 8, [0, 100, 303], [100, 100, 303]);        // 3 m beside, unconnected
        var r = Convert(b);
        Assert.Equal(1, r.Stats.DuplicatesDropped);
        Assert.Equal(3, r.Edges.Count);
        var o = Assert.Single(r.Overlaps);
        Assert.False(o.SharedNode);
        Assert.InRange(o.Length, 90, 101);
    }

    [Fact]
    public void Overlap_StackedPiecesAreNotOverlaps()
    {
        var b = new DumpBuilder().Prefab("Gravel Road", "RoadAI", 1, 1)
            .Node(5, 0, 300).Node(6, 100, 300).Node(7, 0, 302, y: 120, elevation: 20).Node(8, 100, 302, y: 120, elevation: 20)
            .Seg("Gravel Road", 5, 6, [0, 100, 300], [100, 100, 300])
            .Seg("Gravel Road", 7, 8, [0, 120, 302], [100, 120, 302]);
        Assert.Empty(Convert(b).Overlaps);
    }

    static string WriteTemp(object o)
    {
        var p = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        File.WriteAllText(p, JsonSerializer.Serialize(o));
        return p;
    }

    [Fact]
    public void Reconcile_CoveredMissingWrongPrefabAndEntityBoundRetry()
    {
        var import = new
        {
            name = "t",
            nodes = new object[]
            {
                new { id = "a", x = 0.0, y = 0.0, z = 0.0, elevation = 0 }, new { id = "b", x = 100.0, y = 0.0, z = 0.0, elevation = 0 },
                new { id = "c", x = 200.0, y = 0.0, z = 0.0, elevation = 0 }, new { id = "d", x = 300.0, y = 0.0, z = 0.0, elevation = 0 },
            },
            edges = new object[]
            {
                new { id = "ab", a = "a", b = "b", prefab = "Gravel Road", control = new[] { new[] { 33.0, 0, 0 }, new[] { 66.0, 0, 0 } } },
                new { id = "bc", a = "b", b = "c", prefab = "Gravel Road", control = new[] { new[] { 133.0, 0, 0 }, new[] { 166.0, 0, 0 } } },
                new { id = "cd", a = "c", b = "d", prefab = "Gravel Road", control = new[] { new[] { 233.0, 0, 0 }, new[] { 266.0, 0, 0 } } },
            },
            options = new { batchSize = 64 },
        };
        object Seg(int idx, string prefab, int sn, int en, double x0, double x1) => new
        {
            road = new { index = idx, version = 1 }, prefab, startNode = sn, endNode = en,
            start = new { x = x0, y = 0.0, z = 0.0 }, end = new { x = x1, y = 0.0, z = 0.0 }, length = x1 - x0,
        };
        object Node(int idx, double x) => new { node = new { index = idx, version = 3 }, position = new { x, y = 0.0, z = 0.0 }, segmentsConnected = 1 };
        var live = new
        {
            segments = new[] { Seg(10, "Gravel Road", 1, 2, 0.5, 100), Seg(11, "Small Road", 3, 4, 200, 300) },
            nodes = new[] { Node(1, 0.5), Node(2, 100), Node(3, 200), Node(4, 300) },
        };
        var importPath = WriteTemp(import); var livePath = WriteTemp(live); var retryPath = importPath + ".retry";
        try
        {
            var g = LiveGraph.Load(livePath);
            var cov = Reconciler.Run(g, Reconciler.ReadImport(importPath, Layers.Roads, out _)).ToDictionary(c => c.Edge.Id);
            Assert.Equal(Coverage.Covered, cov["ab"].Status);
            Assert.Equal(Coverage.Missing, cov["bc"].Status);
            Assert.Equal(Coverage.WrongPrefab, cov["cd"].Status);

            var ids = cov.Values.Where(Reconciler.NeedsRetry).Select(c => c.Edge.Id).ToHashSet();
            var (n, bound, fresh) = Reconciler.WriteRetry(importPath, retryPath, ids, g, new HashSet<string> { "Gravel Road" });
            Assert.Equal(1, n);
            Assert.Equal(1, bound);   // b binds to live node 2 (carries Gravel Road)
            Assert.Equal(1, fresh);   // c's live node 3 carries only Small Road: not bound
            var retry = JsonNode.Parse(File.ReadAllText(retryPath))!;
            var nb = retry["nodes"]!.AsArray().Single(x => (string)x!["id"]! == "b")!;
            Assert.Equal(2, (int)nb["entity"]!["index"]!);
            Assert.Equal(3, (int)nb["entity"]!["version"]!);
            Assert.Null(retry["nodes"]!.AsArray().Single(x => (string)x!["id"]! == "c")!["entity"]);
        }
        finally { File.Delete(importPath); File.Delete(livePath); File.Delete(retryPath); }
    }
}
