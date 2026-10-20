using NetConverter;

namespace NetConverter.Tests;

public class BridgesTests
{
    [Fact]
    public void Load_ParsesPlansAndAssumedSegments()
    {
        var specs = Bridges.Load(Path.Combine(AppContext.BaseDirectory, "bridges.tsv"));
        var br001 = specs.Single(s => s.Id == "BR001");
        Assert.Equal(new[] { 168.0, 320, 320, 168 }, br001.Fixed);
        Assert.True(br001.Merge && br001.FixedFile);
        var br005 = specs.Single(s => s.Id == "BR005");
        Assert.Equal(2, br005.Count);
        Assert.False(br005.FixedFile);
        var br019 = specs.Single(s => s.Id == "BR019");
        Assert.Equal(89.6, br019.Span);
        Assert.True(br019.SegAssumed);
        Assert.Equal(64, br019.Seg);
        Assert.All(specs, s => Assert.InRange(s.Span ?? 1.4 * s.Seg, 0, 1.4 * s.Seg + 0.01));   // span plans stay inside 1.4 × seg
    }
}
