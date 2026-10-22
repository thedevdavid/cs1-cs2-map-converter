using System.Text.RegularExpressions;
using NetConverter;
using DumpBuilder = NetConverter.Tests.Round2Tests.DumpBuilder;

namespace NetConverter.Tests;

/// <summary>Round 11b: the fort as a ruined stone line along the CS1 Castle Wall Bézier.</summary>
public class Round9Tests
{
    [Fact]
    public void Fort_StonesAlongTheWallCairnsInsideStandingStonesAtEndsKeepAtCentroid()
    {
        // Two walls of an L, 48 m and 36 m.
        var d = new DumpBuilder().Prefab("Castle Wall 1", "DecorationWallAI", 0, 0, 2.5).Node(1, 0, 0).Node(2, 0, 48).Node(3, 36, 48)
            .Seg("Castle Wall 1", 1, 2, [0, 100, 0], [0, 100, 48]).Seg("Castle Wall 1", 2, 3, [0, 100, 48], [36, 100, 48]).Build();
        var (objs, keep) = Round9.Fort(d, new Regex("(?i)Castle Wall"), 7168, 1);
        var stones = objs.Where(o => o.Prefab.StartsWith("RuinsStoneFormation")).ToList();
        Assert.Equal(4 + 3, stones.Count);                                         // 48 / 12 + 36 / 12
        Assert.All(stones.Take(4), o => Assert.Equal(0, o.Rotation, 1));           // first wall runs along +z
        Assert.All(stones.Skip(4), o => Assert.Equal(90, o.Rotation, 1));          // second along +x
        Assert.Equal(3, objs.Count(o => o.Prefab.StartsWith("StandingStone")));    // three distinct ends
        var cairns = objs.Where(o => o.Prefab.StartsWith("RuinsStoneCairns")).ToList();
        Assert.Equal(2 + 1, cairns.Count);                                         // every 25 m
        Assert.All(cairns.Take(2), o => Assert.InRange(o.X, 3, 6));                // inside: towards the centroid (+x of the first wall)
        Assert.Equal("RuinsCastle01", objs[^1].Prefab);   // round 51: the keep (was OldCastle01)
        Assert.NotNull(keep);
        Assert.All(objs, o => Assert.Null(o.Y));
    }
}
