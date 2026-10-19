using System.Text.Json;
using NetConverter;

namespace NetConverter.Tests;

/// <summary>v2/v3 dump fields against the hand-made fixture (fixtures/v3, written by tools/make-v3-fixture.mjs).</summary>
public class V3Tests
{
    static readonly string Fixture = Path.Combine(AppContext.BaseDirectory, "fixtures", "v3", "map-v3.json");
    static JsonDocument Doc() => JsonDocument.Parse(File.ReadAllText(Fixture));

    [Fact]
    public void WaterSurface_RefinesSourcesAndFlagsUnexplainedWater()
    {
        using var doc = Doc();
        var dump = Dump.Load(Fixture);
        dump.ScaleHeights(2);
        var surface = WaterSurface.Load(doc.RootElement, Fixture, null)!;
        Assert.Equal("waterSurface in the dump", surface.Source);
        var (md, overrides) = surface.Check(dump, 97.5, 2, 7168);
        Assert.Equal(203.4, overrides[1], 2);              // source 1: target 200, pond surface 203.4
        Assert.False(overrides.ContainsKey(2));             // river source within 1 m
        Assert.Contains("median surface 97.60", md);
        Assert.Contains("| 1 | 49 |", md);                  // the 150 m lake: no source within 400 m
        Assert.Equal(3, surface.Bodies().Count);
    }

    [Fact]
    public void WaterImport_WritesRefinedHeightsScaled()
    {
        var dump = Dump.Load(Fixture);
        dump.ScaleHeights(2);
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Outputs.YScale = 1;
            Outputs.WriteWaterImport(path, dump, 7168, 195, new Dictionary<int, double> { [1] = 203.4 }, 2);
            using var w = JsonDocument.Parse(File.ReadAllText(path));
            var s = w.RootElement.GetProperty("sources");
            Assert.Equal(406.8, s[0].GetProperty("height").GetDouble(), 2);
            Assert.True(s[0].TryGetProperty("refinedFrom", out _));
            Assert.Equal(195, s[1].GetProperty("height").GetDouble(), 2);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Resources_PassFertilityOreOil_OmitForestAndEmptyWater()
    {
        using var doc = Doc();
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Assert.True(Resources.WriteFromDump(path, doc.RootElement));
            using var r = JsonDocument.Parse(File.ReadAllText(path));
            var names = r.RootElement.GetProperty("layers").EnumerateObject().Select(p => p.Name).ToList();
            Assert.Equal(new[] { "fertility", "ore", "oil" }, names);
            Assert.Equal(8, r.RootElement.GetProperty("grid").GetProperty("size").GetInt32());
            Assert.Equal("replace", r.RootElement.GetProperty("options").GetProperty("mode").GetString());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Resources_GroundwaterOnlyWhenTheWaterLayerHasValues()
    {
        var json = """{"naturalResources":{"grid":{"size":2},"layers":{"fertility":[1,0,0,0],"water":[0,5,0,0],"forest":[9,9,9,9],"ore":null}}}""";
        using var doc = JsonDocument.Parse(json);
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Resources.WriteFromDump(path, doc.RootElement);
            using var r = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(new[] { "fertility", "groundwater" }, r.RootElement.GetProperty("layers").EnumerateObject().Select(p => p.Name));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TerrainSurfaces_SummaryAndRings()
    {
        using var doc = Doc();
        var t = TerrainSurfaces.Load(doc.RootElement)!;
        Assert.Equal(25, t.Cells.Count);                    // sparse flags: 9 Field + 16 Gravel
        Assert.Equal("Field", t.Name(32));
        var rings = t.Rings(4);
        var field = Assert.Single(rings, r => r.Class == 32);
        Assert.True(field.Outer);
        Assert.Equal(4, field.Ring.Count);                  // a 3×3 block simplifies to its 4 corners
        Assert.Contains((-8640 + 540 * 16.0, -8640 + 540 * 16.0), field.Ring);
        var md = t.Summary(7168);
        Assert.Contains("| 32 | Field | 9 |", md);
    }

    [Fact]
    public void TerrainSurfaces_HoleIsClockwise()
    {
        // 3×3 ring of class 1 around an empty centre: one outer ring and one hole.
        var cells = Enumerable.Range(0, 25).Select(i => { int c = i % 5, r = i / 5; return c >= 1 && c <= 3 && r >= 1 && r <= 3 && !(c == 2 && r == 2) ? 1 : 0; });
        var json = """{"terrainSurfaces":{"grid":{"size":5,"cellSize":10,"originX":0,"originZ":0},"legend":[{"id":1,"name":"a"}],"cells":["""
            + string.Join(",", cells) + "]}}";
        using var doc = JsonDocument.Parse(json);
        var rings = TerrainSurfaces.Load(doc.RootElement)!.Rings(0.1);
        Assert.Equal(2, rings.Count);
        Assert.Single(rings, r => r.Outer);
        Assert.Single(rings, r => !r.Outer);
    }

    [Fact]
    public void ProceduralObjects_ParseYawAndMapCandidates()
    {
        using var doc = Doc();
        var po = ProceduralObjects.Load(Fixture, doc.RootElement)!;   // file named by proceduralObjects.file
        Assert.Equal(3, po.Count);
        Assert.Equal(90, po[0].Yaw, 1);                    // yawDegrees null -> quaternion (0, sin 45°, 0, cos 45°)
        Assert.Equal(45, po[1].Yaw, 1);                    // yawDegrees
        Assert.Equal(new double[] { 3, 3, 3 }, po[1].Scale);
        Assert.Equal(new double[] { 1, 1, 1 }, po[2].Scale); // m_scale 0 = unscaled
        var map = TargetMapping.Load(Path.Combine(AppContext.BaseDirectory, "object-mapping.tsv"));
        var cand = ProceduralObjects.Candidates(po, map, 2, 7168, 1);
        Assert.Equal(2, cand.Count);                        // "Cube" maps to nothing
        var c = cand.Single(x => x.Source == "po 1");
        Assert.Matches("^(Rock|rzrock|RZarid)", c.Prefab);
        Assert.Equal("RuinsCastle01", cand.Single(x => x.Source == "po 3").Prefab);
        Assert.Equal(260, c.Y);
        Assert.Contains("### Cube (1)", ProceduralObjects.Manual(po, 2));
    }

    [Fact]
    public void Trees_ExactPointsFileFromTheFixture()
    {
        var map = TargetMapping.Load(Path.Combine(AppContext.BaseDirectory, "tree-mapping.tsv"));
        var r = Trees.FromPoints(Path.Combine(AppContext.BaseDirectory, "fixtures", "v3", "map-v3-trees.json"), map, 7168, 1);
        Assert.Equal(3, r.Objects.Count);
        Assert.Equal(1, r.Outside);
        Assert.Single(r.CropCells);
    }
}
