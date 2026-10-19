using System.Text.Json;
using NetConverter;

namespace NetConverter.Tests;

/// <summary>Round 12: asset-pack grass, rocks and plants in the object output.</summary>
public class PacksTests
{
    static TargetMapping Map => TargetMapping.Load(Path.Combine(AppContext.BaseDirectory, "object-mapping.tsv"));

    [Theory]
    [InlineData("2803411105.Landscape Grass 01_Data", "Grass02 (Styx's)|Grass05 (Styx's)|gaga - Fern 1|PD-YellowFlower")]
    [InlineData("2803411105.Landscape Grass 06_Data", "pd Long Grass|ReikUndergrowth01Trimmed|ReikUndergrowth02Trimmed")]
    [InlineData("1988121829.Grey Breakwater Rocks 01_Data", "rzrock2|rzrock3|rzrock4|rzrock5|rzrock8")]
    [InlineData("1988876538.Brown Modular C Formation 01_Data", "CliffRock3")]
    public void PackMappings(string cs1, string targets) =>
        Assert.Equal(targets, string.Join("|", Map.Resolve(cs1)!.Select(t => t.Prefab)));

    [Fact]
    public void Grass_IsPlantWithAgeAdult()
    {
        var targets = Map.Resolve("2803411105.Landscape Grass 03_Data")!;
        Assert.All(targets, t => Assert.Equal("plant", t.Extra));
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}.json");
        try
        {
            Objects.Write(path, "t", [new PlacedObject("p1", "Grass01 (Styx's)", "plant", 1, 2, null, 30, "Landscape Grass 03", "prop 1")]);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var o = doc.RootElement.GetProperty("objects")[0];
            Assert.Equal("adult", o.GetProperty("age").GetString());
            Assert.Equal(30, o.GetProperty("rotation").GetDouble());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SmallRocks_MixPackAndVanilla()
    {
        var targets = Map.Resolve("1978360460.Brown Surface Rock A Small_Data")!;
        double pack = targets.Where(t => t.Prefab.StartsWith("RZarid")).Sum(t => t.Weight), all = targets.Sum(t => t.Weight);
        Assert.Equal(0.5, pack / all, 3);
    }
}
