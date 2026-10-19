using NetConverter;

namespace NetConverter.Tests;

public class FootprintShiftTests
{
    [Fact]
    public void ParseDrops_ReadsIdPrefabAndMove()
    {
        var p = Path.Combine(Path.GetTempPath(), $"run-test-{Guid.NewGuid():N}.md");
        File.WriteAllLines(p, [
            "## Failures: walls `out/x1/import-walls.json`", "",
            "- e1588_0 QuaySmall01: dropped by the game: the course runs inside the existing 'Highway Oneway - 1 lane' segment 62316:3 (centre lines up to 3.0 m apart, the two half-widths need 5.5 m); move it at least 3.0 m sideways, or leave it out",
            "- e835_0 QuaySmall01: the batch was applied but only 0 of 3 points along the course lie on a segment of this prefab"]);
        var d = FootprintShift.ParseDrops([p]);
        File.Delete(p);
        Assert.Single(d);
        Assert.Equal(("Highway Oneway - 1 lane", 3.0), d["e1588_0"]);
    }
}
