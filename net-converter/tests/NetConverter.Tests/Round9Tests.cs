using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Round 9: fort wall props along the CS1 Castle Wall Bézier.</summary>
public class Round9Tests
{
    [Fact]
    public void Fort_FillsTheWallWithFullThenShortPieces()
    {
        var d = new DumpBuilder().Prefab("Castle Wall 1", "DecorationWallAI", 0, 0, 2.5).Node(1, 0, 0).Node(2, 0, 36)
            .Seg("Castle Wall 1", 1, 2, [0, 100, 0], [0, 100, 36]).Build();
        var rules = Round9.LoadWallRules(Path.Combine(AppContext.BaseDirectory, "fort-walls.tsv"));
        var props = Round9.Fort(d, rules, 7168);
        Assert.Equal(new[] { "EU_Wall01", "EU_Wall01", "EU_Wall01", "EU_Wall01", "EU_Wall01Short" }, props.Select(p => p.Prefab));   // 4 × 8 + 4 = 36
        Assert.All(props, p => Assert.Equal(90, p.Rotation, 1));    // along +z, offset 90
        Assert.Equal(4, props[0].Z, 1);
        Assert.Equal(34, props[^1].Z, 1);
    }
}
