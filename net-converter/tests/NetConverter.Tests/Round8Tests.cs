using NetConverter;

namespace NetConverter.Tests;

/// <summary>Round 8: dam variants, cliff bands, rock scatter.</summary>
public class Round8Tests
{
    static Round8.PoPiece Piece(int id, double l, double w, double h = 30) => new(id, "mountain", id * 1000, 0, 90, l, w, h, id * 1000, 0);

    [Fact]
    public void Dam_CentreOfClusterAndMirroredVariant()
    {
        var dams = new List<Round8.Building> { new(1, "Dam", 0, 0, 261.7), new(2, "Dam", 10, 0, 261.7), new(3, "Dam", 0, 20, 261.7), new(4, "Dam", 10, 20, 261.7) };
        var (a, b) = Round8.Dam(dams)!.Value;
        Assert.Equal((5.0, 10.0), (a.X, a.Z));
        Assert.Equal(261.7, a.Rotation);
        Assert.Equal(98.3, b.Rotation, 2);
        Assert.Null(a.Y);
    }

    [Fact]
    public void Cliffs_ByFootprintBand_BigOnesGetSummitAndFlank()
    {
        var pieces = new List<Round8.PoPiece> { Piece(1, 2, 2), Piece(2, 8, 5), Piece(3, 20, 10), Piece(4, 50, 40), Piece(5, 120, 110), Piece(6, 400, 300) };
        var (objs, capped, bands) = Round8.Cliffs(pieces, (x, z) => -Math.Abs(x - 6000) - Math.Abs(z), 7168, 1);
        Assert.DoesNotContain(objs, o => o.Source == "po 1");                 // < 3 m
        Assert.Contains(objs, o => o.Source == "po 3" && o.Prefab.StartsWith("rzrock"));
        Assert.Contains(objs, o => o.Source == "po 4" && o.Prefab is "CliffRock5" or "CliffRock6" or "CliffStack");
        Assert.Contains(objs, o => o.Source == "po 5" && o.Prefab == "CliffSide2");
        Assert.Contains(objs, o => o.Source == "po 6" && o.Prefab == "CliffFreeStanding");
        Assert.Equal(3, objs.Count(o => o.Source == "po 6" && o.Prefab.StartsWith("CliffSide")));   // 60, 120, 180 m ≤ 200 m reach
        Assert.All(objs, o => Assert.Null(o.Y));
        Assert.Equal(0, capped);
        var (capObjs, capped2, _) = Round8.Cliffs(pieces, null, 7168, 1, cap: 2);
        Assert.Equal(2, capObjs.Count); Assert.True(capped2 > 0);
        Assert.All(capObjs, o => Assert.Equal("po 6", o.Source));                 // largest first
    }

    [Fact]
    public void Scatter_DeterministicSpacedAndMixed()
    {
        var src = new List<(double, double)> { (0, 0), (500, 500) };
        var (a, _) = Round8.Scatter(src, [], [], null, 7168, 42);
        var (b, _) = Round8.Scatter(src, [], [], null, 7168, 42);
        Assert.Equal(a.Select(o => (o.Prefab, o.X, o.Z)), b.Select(o => (o.Prefab, o.X, o.Z)));
        double area = 2 * Math.PI * 60 * 60;
        Assert.InRange(a.Count, area / 500 * 0.6, area / 500 * 1.2);
        for (int i = 0; i < a.Count; i++)
            for (int j = i + 1; j < a.Count; j++)
                Assert.True(Math.Sqrt((a[i].X - a[j].X) * (a[i].X - a[j].X) + (a[i].Z - a[j].Z) * (a[i].Z - a[j].Z)) >= 6);
        Assert.All(a, o => Assert.True(Round8.SmallRocks.Contains(o.Prefab) || Round8.MediumRocks.Contains(o.Prefab) || Round8.Boulders.Contains(o.Prefab)));
        var (wet, rej) = Round8.Scatter(src, [], [], (x, z) => true, 7168, 42);
        Assert.Empty(wet); Assert.True(rej["water"] > 0);
    }
}
