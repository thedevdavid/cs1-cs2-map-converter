namespace Cslmap2E2c;

/// <summary>
/// Earth2Cities' road placement in "terrain-aligned" mode, the mode it uses whenever the four Overpass
/// bbox settings parse (always true with its defaults). See E2C_CONTRACT.md §2.
///
/// E2C takes the centre of the settings bbox, builds a square of the playable width around it in
/// degrees (111 320 m per degree, longitude scaled by cos(centre lat)), and maps lat/lon linearly onto
/// x,z in [-W/2, W/2], clamping to the square. OriginX/Z, DefaultScale and rotation are ignored then.
/// Inverse: lat = cLat + z / 111320, lon = cLon + x / (111320 cos cLat).
/// </summary>
public sealed class E2cProjection
{
    public const double MetersPerDegree = 111320.0;

    public double CenterLat { get; }
    public double CenterLon { get; }
    public double PlayableWidth { get; }

    public E2cProjection(double centerLat = 0, double centerLon = 0, double playableWidth = 14336)
    {
        CenterLat = centerLat;
        CenterLon = centerLon;
        PlayableWidth = playableWidth;
    }

    double MetersPerDegreeLon => MetersPerDegree * Math.Cos(CenterLat * Math.PI / 180.0);

    public (double Lat, double Lon) ToGeo(double x, double z) =>
        (CenterLat + z / MetersPerDegree, CenterLon + x / MetersPerDegreeLon);

    /// <summary>Square E2C builds from the settings bbox centre (computed in double, stored as float, as E2C does).</summary>
    public (float Top, float Right, float Bottom, float Left) PlayableBounds()
    {
        double half = PlayableWidth * 0.5;
        double dLat = half / MetersPerDegree;
        double dLon = half / Math.Max(1.0, MetersPerDegreeLon);
        return ((float)Math.Min(85.0, CenterLat + dLat), (float)(CenterLon + dLon), (float)Math.Max(-85.0, CenterLat - dLat), (float)(CenterLon - dLon));
    }

    /// <summary>
    /// Forward model of E2C's placement with its float32 arithmetic, used by tests and the self-check
    /// to measure round-trip error. Returns the game x,z E2C should produce for a lat/lon read from the OSM file.
    /// </summary>
    public (float X, float Z) SimulateE2c(double lat, double lon)
    {
        var (top, right, bottom, left) = PlayableBounds();
        float flat = (float)lat, flon = (float)lon;
        float u = right - left > 1e-6f ? (flon - left) / (right - left) : 0.5f;
        float v = top - bottom > 1e-6f ? (flat - bottom) / (top - bottom) : 0.5f;
        u = Math.Clamp(u, 0f, 1f);
        v = Math.Clamp(v, 0f, 1f);
        float w = (float)PlayableWidth;
        return ((u - 0.5f) * w, (v - 0.5f) * w);
    }

    /// <summary>Bbox strings to type into E2C's Overpass Top/Right/Bottom/Left fields; only their centre matters.</summary>
    public (string Top, string Right, string Bottom, string Left) SettingsBbox(double halfSpanDeg = 0.1)
    {
        string F(double d) => d.ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture);
        return (F(CenterLat + halfSpanDeg), F(CenterLon + halfSpanDeg), F(CenterLat - halfSpanDeg), F(CenterLon - halfSpanDeg));
    }
}
