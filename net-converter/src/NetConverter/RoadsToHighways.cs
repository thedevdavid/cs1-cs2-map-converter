namespace NetConverter;

/// <summary>
/// Round 47 (owner: "there shouldn't be sidewalked roads pretty much nowhere on the map other than what's in the starting area interchange. everything
/// else must be highway that you currently selected as sidewalked roads"): a CS1 segment mapped to a sidewalked CS2 road becomes the matching highway,
/// unless its midpoint lies in the starting area, the 3 × 3 starting tiles around the origin (CS2: 21 × 21 tiles over 14 336 m, 682.7 m each, so
/// |x|, |z| <= 1 024 m). The mapping file stays as it is; this is decided per segment.
/// </summary>
public static class RoadsToHighways
{
    public const double StartHalf = 14336.0 / 21 * 1.5;   // 1 024 m

    /// <summary>Sidewalked CS2 roads → highway (widths from GET /prefabs/info: Small Road 16, Highway Twoway 2 lanes 12, 3 lanes 16, 4 lanes 20, Oneway 1 lane 8).</summary>
    public static readonly IReadOnlyDictionary<string, string> Targets = new Dictionary<string, string>
    {
        ["Small Road"] = "Highway Twoway - 2 lanes",
        ["Alley"] = "Highway Twoway - 2 lanes",
        ["Medium Road"] = "Highway Twoway - 4 lanes",          // CS1 3-lane (2+1) and 4-lane two-way roads
        ["Medium Road Asymmetric - 1+3 lanes"] = "Highway Twoway - 4 lanes",
        ["Large Road"] = "Highway Twoway - 4 lanes",           // no vanilla 6-lane two-way highway
        ["Small Road Oneway - 1 lane"] = "Highway Oneway - 1 lane",
        ["Small Road Oneway - 3 lanes"] = "Highway Oneway - 3 lanes",
    };

    public static bool InStartArea(double x, double z) => Math.Abs(x) <= StartHalf && Math.Abs(z) <= StartHalf;

    public static MapResult Apply(MapResult map, Bezier cs1Curve)
    {
        if (map.Cs2Prefab == null || !Targets.TryGetValue(map.Cs2Prefab, out var target)) return map;
        var m = cs1Curve.Eval(0.5);
        return InStartArea(m.X, m.Z) ? map : map with { Cs2Prefab = target };
    }
}
