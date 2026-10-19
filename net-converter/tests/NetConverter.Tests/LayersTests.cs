using System.Text.Json;
using NetConverter;

namespace NetConverter.Tests;

/// <summary>Trees, objects, outside connections, resources.</summary>
public class LayersTests
{
    static readonly string Dir = AppContext.BaseDirectory;
    static TargetMapping TreeMap => TargetMapping.Load(Path.Combine(Dir, "tree-mapping.tsv"));
    static TargetMapping ObjectMap => TargetMapping.Load(Path.Combine(Dir, "object-mapping.tsv"));

    [Fact]
    public void TargetMapping_GroupsRowsAndPicksByWeight()
    {
        var m = TargetMapping.Parse(["# c", "(?i)Pine\tA\t3\tdefault", "(?i)Pine\tB\t1\tdefault", "(?i)Corn\tCROP\t1\tdefault"]);
        var targets = m.Resolve("PD - Pine Tall_Data")!;
        Assert.Equal(2, targets.Count);
        var rng = new Rng(1);
        int a = Enumerable.Range(0, 4000).Count(_ => TargetMapping.Pick(targets, ref rng).Prefab == "A");
        Assert.InRange(a, 2800, 3200);
        Assert.True(m.Resolve("Corn Stalk")![0].IsCrop);
        Assert.Null(m.Resolve("Oak"));
    }

    [Theory]
    [InlineData("mp9-BorealPine", "SpruceTree01|EuropeanSpruce01")]
    [InlineData("1230960704.PD - Pine Tall_Data", "PineTree01")]
    [InlineData("mp9-LiveOaknvl", "OakTree01")]
    [InlineData("1238991822.Bare Tree E_Data", "BirchTree01")]
    [InlineData("2346312805.Filler Bush small 1_Data", "GreenBushWild01|GreenBushWild02")]
    [InlineData("1953870276.Corn Stalk _Data", "CROP")]
    [InlineData("883313481.RiverReeds Detail Edition cluste_Data", "SKIP")]
    public void TreeMapping_CoversTheDumpSpecies(string cs1, string expected) =>
        Assert.Equal(expected, string.Join("|", TreeMap.Resolve(cs1)!.Select(t => t.Prefab)));

    static JsonElement Raster(string json) => JsonDocument.Parse(json).RootElement;

    [Fact]
    public void Trees_FromRasters_AreDeterministicSpacedClippedAndCropsBecomeCells()
    {
        // total grid 1081 x 15.985 m; byPrefab grid 270 x 64 m. 16 m cell (541, 541) lies inside 64 m cell (135, 135) near the centre; (0, 0) at the SW corner (outside ±7168).
        int centreT = 541 * 1081 + 541, cornerT = 0, centreP = 135 * 270 + 135, cornerP = 0;
        var json =
            "{\"count\":30,\"raster\":{\"size\":270,\"cellSize\":64}," +
            $"\"total\":{{\"size\":1081,\"cellSize\":15.985199,\"cells\":[{cornerT},5,{centreT},25]}}," +
            $"\"byPrefab\":{{\"mp9-LiveOaknvl\":{{\"count\":20,\"cells\":[{centreP},20]}}," +
            $"\"1953870276.Corn Stalk _Data\":{{\"count\":5,\"cells\":[{centreP},5]}}," +
            $"\"1230960704.PD - Pine Tall_Data\":{{\"count\":5,\"cells\":[{cornerP},5]}}}}}}";
        var r1 = Trees.FromRasters(Raster(json), TreeMap, 7168, 42);
        var r2 = Trees.FromRasters(Raster(json), TreeMap, 7168, 42);
        Assert.Equal(r1.Objects.Select(o => (o.Prefab, o.X, o.Z, o.Age)), r2.Objects.Select(o => (o.Prefab, o.X, o.Z, o.Age)));
        Assert.Equal(5, r1.Outside);                          // the SW corner cell
        Assert.Equal(20, r1.Objects.Count);                   // 25 at the centre minus 5 corn
        Assert.All(r1.Objects, o => Assert.Equal("OakTree01", o.Prefab));
        Assert.Equal(5, r1.CropCells.Values.Sum());
        for (int i = 0; i < r1.Objects.Count; i++)
            for (int j = i + 1; j < r1.Objects.Count; j++)
                Assert.True(Math.Sqrt(Math.Pow(r1.Objects[i].X - r1.Objects[j].X, 2) + Math.Pow(r1.Objects[i].Z - r1.Objects[j].Z, 2)) >= 3 || r1.SpacingRelaxed > 0);
        Assert.All(r1.Objects, o => Assert.Contains(o.Age, new[] { "seedling", "sapling", "adult", "elderly" }));
    }

    [Fact]
    public void Trees_FromPoints_UsesExactPositions()
    {
        var path = Path.Combine(Path.GetTempPath(), $"nc-{Guid.NewGuid():N}-trees.json");
        File.WriteAllText(path, """{"schema":"dakota-export/cs1/trees/v1","prefabs":["mp9-LiveOaknvl","1953870276.Corn Stalk _Data"],"points":[10.5,20.25,0, 30,40,1, 9000,0,0]}""");
        try
        {
            var r = Trees.FromPoints(path, TreeMap, 7168, 1);
            var t = Assert.Single(r.Objects);
            Assert.Equal((10.5, 20.25, "OakTree01"), (t.X, t.Z, t.Prefab));
            Assert.Equal(1, r.Outside);
            Assert.Single(r.CropCells);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(Math.PI / 2, 270)]
    [InlineData(-Math.PI / 2, 90)]
    [InlineData(Math.PI, 180)]
    public void Yaw_IsMinusAngleInDegrees(double radians, double yaw) => Assert.Equal(yaw, Objects.Yaw(radians), 2);

    [Fact]
    public void Objects_MapSkipManualAndOffGroundY()
    {
        var json = """
        {"buildings":[
          {"id":1,"prefab":"1978360460.Brown Surface Rock A Small_Data","position":[10,150,10],"angle":0},
          {"id":2,"prefab":"1978360460.Brown Surface Rock A Large_Data","position":[20,100.5,20],"angle":1.5707963},
          {"id":3,"prefab":"2452038713.USRP 2L Interstate Pillar_Data","position":[30,100,30],"angle":0},
          {"id":4,"prefab":"1251664417.Dam Electricity_Data","position":[40,120,40],"angle":0},
          {"id":5,"prefab":"Road Connection","position":[8640,100,0],"angle":0,"outsideConnection":true},
          {"id":6,"prefab":"Mystery Building","position":[50,100,50],"angle":0}],
         "props":{"encoding":"points","points":[{"id":7,"prefab":"Park Streetlight 01","position":[60,100,60],"angle":0}]}}
        """;
        var r = Objects.Build(JsonDocument.Parse(json).RootElement, ObjectMap, (x, z) => 100, 2, 7168, 1);
        Assert.Equal(6, r.Total);                              // the outside connection is not an object here
        Assert.Equal(3, r.Targeted);
        Assert.Equal(1, r.Skipped);
        Assert.Equal(1, r.ManualCount);
        Assert.Equal(1, r.Unmapped);
        var rock = r.Objects.Single(o => o.Source == "building 1");
        Assert.Equal(300, rock.Y);                             // 50 m above terrain -> y = 150 × 2
        Assert.Null(r.Objects.Single(o => o.Source == "building 2").Y);
        Assert.Equal(270, r.Objects.Single(o => o.Source == "building 2").Rotation, 1);
        Assert.Equal("StreetlightSingle01", r.Objects.Single(o => o.Source == "prop 7").Prefab);
        Assert.Equal(240, Assert.Single(r.Manual).Y);
    }

    [Theory]
    [InlineData(4756, 8647, 4756, 7168)]
    [InlineData(8649, 1917, 7168, 1917)]
    [InlineData(-8646, 2766, -7168, 2766)]
    [InlineData(7416, -8646, 7168, -7168)]
    public void Connections_ProjectToTheBorder(double x, double z, double px, double pz)
    {
        var (bx, bz) = Connections.ProjectToBorder(x, z, 7168);
        Assert.Equal((px, pz), (bx, bz));
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 0, 90)]
    [InlineData(0, -1, 180)]
    [InlineData(-1, 0, 270)]
    public void Connections_HeadingIsClockwiseFromPlusZ(double dx, double dz, double yaw) => Assert.Equal(yaw, Connections.Heading(dx, dz), 2);

    [Fact]
    public void Resources_SyntheticFertilityFollowsCropCells()
    {
        var t = new TreeResult();
        t.CropCells[135 * 270 + 135] = 100;   // 64 m cell at the map centre
        t.CropCells[135 * 270 + 136] = 25;
        var f = Resources.SyntheticFertility(t);
        Assert.Equal(512 * 512, f.Length);
        int centre = 256 * 512 + 256;          // 512-grid cell whose centre lies in 64 m cell (135, 135)
        Assert.Equal(255, f[centre]);
        Assert.Equal(128, f.Max(v => v == 255 ? 0 : v));      // sqrt(25/100) · 255
        Assert.True(f.Count(v => v > 0) > 0 && f.Count(v => v > 0) < 20);
    }
}
