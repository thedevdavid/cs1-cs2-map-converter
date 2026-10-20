using System.Text.Json;

namespace NetConverter;

/// <summary>
/// Mesh bounds per object prefab, measured live (tools/fetch-bounds.mjs → prefab-bounds.json, bridge GET /prefabs/info).
/// Pack rocks and cliffs have their pivot well above the mesh bottom (CliffFreeStanding: 130.7 m below the pivot), so a y that
/// ignores bounds.min.y floats or buries them.
/// </summary>
public sealed record PrefabBound(double MinY, double MaxY, double SizeX, double SizeZ)
{
    public double Height => MaxY - MinY;
    public double HalfFootprint => Math.Max(SizeX, SizeZ) / 2;

    public static Dictionary<string, PrefabBound> Load(string path)
    {
        var res = new Dictionary<string, PrefabBound>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path)) return res;
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        foreach (var p in doc.RootElement.GetProperty("prefabs").EnumerateObject())
        {
            var mn = p.Value.GetProperty("min"); var mx = p.Value.GetProperty("max");
            res[p.Name] = new PrefabBound(mn.GetProperty("y").GetDouble(), mx.GetProperty("y").GetDouble(),
                mx.GetProperty("x").GetDouble() - mn.GetProperty("x").GetDouble(), mx.GetProperty("z").GetDouble() - mn.GetProperty("z").GetDouble());
        }
        return res;
    }

    /// <summary>Absolute y that puts the mesh bottom <paramref name="sink"/> m below the terrain (terrain already in output metres).</summary>
    public double Y(double terrainOut, double sink) => Math.Round(terrainOut - MinY - sink, 2);
}
