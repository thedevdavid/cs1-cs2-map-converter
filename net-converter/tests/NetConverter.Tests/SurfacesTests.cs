using System.Text.Json;
using System.Text.RegularExpressions;
using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Surfaces (fields, grass, terrain surfaces) and the round-10 skip review mappings.</summary>
public class SurfacesTests
{
    static readonly string Dir = AppContext.BaseDirectory;
    static readonly GridSpec G = new(20, 10, 0, 0);

    static HashSet<int> Cells(params (int Col, int Row)[] cells) => cells.Select(c => c.Row * G.Size + c.Col).ToHashSet();

    [Fact]
    public void Polygonize_SquareIsOneRing()
    {
        var cells = new HashSet<int>();
        for (int r = 2; r < 6; r++) for (int c = 3; c < 8; c++) cells.Add(r * G.Size + c);
        var ring = Assert.Single(Surfaces.Polygonize(cells, G));
        Assert.Equal(4, ring.Count);
        Assert.Equal(50 * 40, Math.Abs(Surfaces.Area(ring)), 3);
    }

    [Fact]
    public void Polygonize_SplitsRingWithHoleIntoHoleFreePieces()
    {
        // 5×5 block with a 1-cell hole in the middle.
        var cells = new HashSet<int>();
        for (int r = 0; r < 5; r++) for (int c = 0; c < 5; c++) if (!(r == 2 && c == 2)) cells.Add(r * G.Size + c);
        var rings = Surfaces.Polygonize(cells, G);
        Assert.True(rings.Count >= 2);
        Assert.Equal(24 * 100, rings.Sum(r => Math.Abs(Surfaces.Area(r))), 3);   // total area kept, the hole left out
        Assert.All(rings, r => Assert.False(Surfaces.SelfIntersects(r)));
    }

    [Fact]
    public void FixPinches_FillsDiagonalContacts()
    {
        var cells = Cells((1, 1), (2, 2));
        Surfaces.FixPinches(cells, G);
        Assert.Equal(3, cells.Count);
        Assert.Single(Surfaces.Polygonize(Cells((1, 1), (2, 2)), G));
    }

    [Fact]
    public void Fields_AreSweptMergedAndRaiseFertility()
    {
        var b = new DumpBuilder().Prefab("2674210208.Field Wheat Network 3_Data", "RoadAI", 0, 0, halfWidth: 20)
            .Node(1, 0, 0).Node(2, 200, 0).Node(3, 200, 100)
            .Seg("2674210208.Field Wheat Network 3_Data", 1, 2, [0, 100, 0], [200, 100, 0])
            .Seg("2674210208.Field Wheat Network 3_Data", 2, 3, [200, 100, 0], [200, 100, 100]);
        var dump = b.Build();
        var fine = Surfaces.FineGrid(4);
        var cells = Surfaces.FieldFootprints(dump, fine, new Regex("(?i)Field .*Network"));
        var rings = Surfaces.Polygonize(cells, fine);
        var ring = Assert.Single(rings);                                  // the two touching footprints merge
        Assert.InRange(Math.Abs(Surfaces.Area(ring)), 0.8 * (240 * 40 + 120 * 40), 1.2 * (240 * 40 + 120 * 40));
        Assert.True(ring.Count <= Surfaces.MaxPoints);
        var fert = Resources.SyntheticFertility(new TreeResult(), (x, z) => { int i = fine.Index(x, z); return i >= 0 && cells.Contains(i); });
        Assert.Contains(fert, v => v == Resources.FieldFertility);
    }

    [Fact]
    public void Grass_ClustersBecomePaddedHulls()
    {
        var tufts = new List<(double X, double Z)>();
        for (int i = 0; i < 6; i++) tufts.Add((100 + i * 5, 100));       // a line of 6 tufts 5 m apart
        tufts.Add((900, 900));                                            // noise
        var hulls = Surfaces.GrassClusters(tufts);
        var h = Assert.Single(hulls);
        Assert.InRange(h.Min(p => p.X), 96.9, 97.1);                      // padded 3 m
        Assert.InRange(h.Max(p => p.Z), 102.9, 103.1);
    }

    [Fact]
    public void Grass_HullIsClippedToTheSquare()
    {
        var tufts = Enumerable.Range(0, 6).Select(i => (7166.0 + i, 0.0)).ToList();
        var h = Assert.Single(Surfaces.GrassClusters(tufts));
        Assert.All(h, p => Assert.True(p.X <= 7168));
    }

    [Fact]
    public void TerrainSurfaces_MapThroughSurfaceMapping()
    {
        var json = """{"terrainSurfaces":{"grid":{"size":4,"cellSize":100,"originX":0,"originZ":0},"legend":{"1":"gravel","2":"clip"},"cells":[1,1,0,0, 1,1,0,2, 0,0,0,2, 0,0,0,0]}}""";
        using var doc = JsonDocument.Parse(json);
        var t = TerrainSurfaces.Load(doc.RootElement)!;
        var s = Surfaces.FromTerrainSurfaces(t, TargetMapping.Load(Path.Combine(Dir, "surface-mapping.tsv")), out var decisions);
        var one = Assert.Single(s);
        Assert.Equal("gaga TTP - Pebbles 1M dark", one.Prefab);
        Assert.StartsWith("SKIP", decisions["clip"]);
    }

    [Fact]
    public void SurfaceWriter_FollowsTheContract()
    {
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Surfaces.Write(path, "surfaces-01", [new SurfacePolygon("s1", "Grass Surface 01", [(0, 0), (10, 0), (10, 10)], "n")], "c");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var s = doc.RootElement.GetProperty("surfaces")[0];
            Assert.Equal("Grass Surface 01", s.GetProperty("prefab").GetString());
            Assert.Equal(3, s.GetProperty("polygon").GetArrayLength());
            Assert.False(s.TryGetProperty("holes", out _));
            Assert.Equal(50, doc.RootElement.GetProperty("options").GetProperty("batchSize").GetInt32());
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("Pedestrian Gravel", "Pavement Path", Layers.Roads)]
    [InlineData("Pedestrian Elevated", "Pavement Path", Layers.Roads)]
    [InlineData("Quay", "QuaySmall01", Layers.Walls)]
    [InlineData("2966434181.Retaining Walls - AiO #01_Data", "QuaySmall01", Layers.Walls)]
    [InlineData("Canal", "Medium Boatway", Layers.Walls)]
    [InlineData("Flood Wall", "Sound Barrier", Layers.Walls)]
    [InlineData("Ship Path", "Medium Seaway", Layers.ShipPaths)]
    [InlineData("Ship Connection Path", "Medium Seaway", Layers.ShipPaths)]
    [InlineData("Airplane Path", null, Layers.Skip)]
    [InlineData("Airplane Connection Path", null, Layers.Skip)]
    [InlineData("Castle Wall 1", null, Layers.Skip)]
    [InlineData("1889660581.Concrete Barrier_Data", null, Layers.Skip)]
    [InlineData("2105051034.Terraforming Network Ext 0.5U_Data", null, Layers.Skip)]
    [InlineData("2674210208.Field Wheat Network 3_Data", null, Layers.Skip)]
    public void SkipReview_NetworkMapping(string cs1, string? cs2, string layer)
    {
        var m = Mapping.Load(Path.Combine(Dir, "mapping.tsv")).Map(new NetPrefabInfo(cs1, "", "", "RoadAI", 4, 0, 0));
        Assert.Equal(cs2, m.Cs2Prefab);
        Assert.Equal(layer, m.Layer);
    }
}

public class FieldCircleTests
{
    [Fact]
    public void ClosedRingBecomesOneCircleOfRingRadiusPlusHalfWidth()
    {
        // A ring of radius 60 m from 4 quarter arcs (cubic approximation, k = 0.5523), field half-width 40 m.
        double r = 60, k = 0.5523 * r;
        var b = new Round2Tests.DumpBuilder().Prefab("2674210208.Field Wheat Network 3_Data", "RoadAI", 0, 0, halfWidth: 40)
            .Node(1, r, 0).Node(2, 0, r).Node(3, -r, 0).Node(4, 0, -r)
            .Seg("2674210208.Field Wheat Network 3_Data", 1, 2, [r, 100, 0], [0, 100, r], pb: [r, 100, k], pc: [k, 100, r])
            .Seg("2674210208.Field Wheat Network 3_Data", 2, 3, [0, 100, r], [-r, 100, 0], pb: [-k, 100, r], pc: [-r, 100, k])
            .Seg("2674210208.Field Wheat Network 3_Data", 3, 4, [-r, 100, 0], [0, 100, -r], pb: [-r, 100, -k], pc: [-k, 100, -r])
            .Seg("2674210208.Field Wheat Network 3_Data", 4, 1, [0, 100, -r], [r, 100, 0], pb: [k, 100, -r], pc: [r, 100, -k]);
        var open = new HashSet<int>();
        var circles = Surfaces.FieldCircles(b.Build(), new System.Text.RegularExpressions.Regex("(?i)Field .*Network"), Surfaces.FineGrid(4), open);
        var c = Assert.Single(circles);
        Assert.Equal(100, c.R, 0);
        Assert.Equal(48, c.Ring.Count);
        Assert.Empty(open);
        Assert.True(Outputs.Roundness(c.Ring) > 0.95);
    }
}

public class RockSurfaceTests
{
    [Fact]
    public void SteepBandBecomesRockPolygonsAboveTheThreshold()
    {
        // 64² map over ±224 m (7 m/px): flat at 100 m except a 60 m-wide band x ∈ [0, 60] rising 1.5 m per metre (56°).
        const int n = 64; const double half = 224;
        var v = new ushort[n * n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                double x = -half + (c + 0.5) * 2 * half / n;
                double h = 100 + 1.5 * Math.Clamp(x, 0, 60);
                v[r * n + c] = (ushort)Math.Round(h * 32);
            }
        var hm = new Heightmap(n, v, half);
        var rock = Surfaces.SteepRock(hm, 40, 400, 4, 1500);
        var p = Assert.Single(rock.Polygons);
        Assert.InRange(rock.KeptArea, 40 * 400, 80 * 448);     // the band, minus the outermost rows
        Assert.InRange(p.Min(q => q.X), -10, 5);
        Assert.InRange(p.Max(q => q.X), 55, 70);
        Assert.Empty(Surfaces.SteepRock(hm, 60, 400, 4, 1500).Polygons);   // 56° < 60°
    }
}
