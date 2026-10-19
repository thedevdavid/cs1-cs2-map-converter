using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NetConverter;

public sealed record WaterOut(string Id, double X, double Z, double HeightCs1, double Radius, long Rate, string Note);

/// <summary>
/// Water sources for /import/water after the CS2-side audit (audit/cs2-side.md finding 6), in CS1 metres; the writer scales.
/// Every source is "natural" (a constant-level source in CS2's new water system); never polluted, never a negative depth.
/// </summary>
public static class Water
{
    /// <param name="seaLevelCs1">Sea level in CS1 metres (97.5).</param>
    /// <param name="factor">Output height factor (1 or 2): the drop rule "height ≤ S + 1 m" is applied in output metres.</param>
    /// <param name="terrainCs1">True-height terrain (CS1 metres) for the depth fallback of moved sources.</param>
    /// <param name="overridesCs1">Heights refined from the CS1 water surface (v3).</param>
    public static (List<WaterOut> Sources, List<string> Log) Plan(Dump dump, double seaLevelCs1, double factor, double half,
        Func<double, double, double>? terrainCs1, Dictionary<int, double>? overridesCs1, bool mergeCanal)
    {
        var log = new List<string>();
        var list = new List<WaterOut>();
        var ci = CultureInfo.InvariantCulture;
        foreach (var s in dump.WaterSources)
        {
            double h = overridesCs1 != null && overridesCs1.TryGetValue(s.Id, out var o) ? o : s.TargetHeight / dump.HeightFactor;
            double x = s.Output.X, z = s.Output.Z;
            double radius = Math.Round(Math.Clamp(Math.Sqrt(s.OutputRate), 20, 150));
            if (h * factor <= seaLevelCs1 * factor + 1) { log.Add(string.Create(ci, $"{s.Id}: dropped, level {h:F2} m ≤ sea level {seaLevelCs1} + 1 m (× {factor})")); continue; }
            double outside = Math.Max(Math.Abs(x), Math.Abs(z)) - half;
            if (outside > 500) { log.Add(string.Create(ci, $"{s.Id}: dropped, {outside:F0} m outside the map (backdrop only)")); continue; }
            string note = "";
            if (outside > -radius - 14)
            {
                double lim = half - radius - 14;
                double nx = Math.Clamp(x, -lim, lim), nz = Math.Clamp(z, -lim, lim);
                if (nx != x || nz != z)
                {
                    note = string.Create(ci, $"moved inward from ({x:F0}, {z:F0})");
                    x = nx; z = nz;
                    if (terrainCs1 != null)
                    {
                        double ground = terrainCs1(x, z);
                        if (h * factor < ground * factor + 2) { note += string.Create(ci, $"; level raised {h:F2} → {(ground * factor + 2) / factor:F2} (depth ≥ 2 m)"); h = (ground * factor + 2) / factor; }
                    }
                    log.Add($"{s.Id}: {note}");
                }
            }
            list.Add(new WaterOut(s.Id.ToString(ci), x, z, h, radius, s.OutputRate, note));
        }

        if (mergeCanal)
        {
            // Small canal feeds (radius ≤ 30) in the canal area: single-link groups within 150 m and level difference < 1 m.
            var small = list.Where(w => w.Radius <= 30 && w.X >= 1100 && w.X <= 1450 && w.Z >= 4450 && w.Z <= 4650).ToList();
            var groups = new List<List<WaterOut>>();
            foreach (var w in small.OrderBy(w => w.HeightCs1))
            {
                var g = groups.FirstOrDefault(g => g.Any(o => Math.Abs(o.HeightCs1 - w.HeightCs1) < 1 && Math.Sqrt((o.X - w.X) * (o.X - w.X) + (o.Z - w.Z) * (o.Z - w.Z)) <= 150));
                if (g == null) groups.Add(g = new List<WaterOut>());
                g.Add(w);
            }
            foreach (var g in groups.Where(g => g.Count > 1))
            {
                double cx = g.Average(w => w.X), cz = g.Average(w => w.Z);
                double r = Math.Round(Math.Min(150, g.Max(w => Math.Sqrt((w.X - cx) * (w.X - cx) + (w.Z - cz) * (w.Z - cz)) + w.Radius)));
                var merged = new WaterOut(string.Join("+", g.Select(w => w.Id)), cx, cz, g.Average(w => w.HeightCs1), r, g.Sum(w => w.Rate), $"merged {g.Count} canal feeds");
                foreach (var w in g) list.Remove(w);
                list.Add(merged);
                log.Add(string.Create(ci, $"{merged.Id}: merged {g.Count} canal feeds into one source at ({cx:F0}, {cz:F0}), radius {r:F0} m, level {merged.HeightCs1:F2}"));
            }
        }
        return (list, log);
    }

    public static void Write(string path, List<WaterOut> sources, double factor, double seaLevelOut, double half)
    {
        using var fs = File.Create(path);
        using var w = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });
        w.WriteStartObject();
        w.WriteString("_comment", $"Water sources after the CS2 audit rules (water-check.md lists every change). type natural = constant-level source; height = water level in metres (× {factor}); never polluted.");
        w.WriteNumber("seaLevel", seaLevelOut);
        w.WriteStartArray("sources");
        foreach (var s in sources)
        {
            w.WriteStartObject();
            if (int.TryParse(s.Id, out var id)) w.WriteNumber("id", id); else w.WriteString("id", s.Id);
            w.WriteNumber("x", Json.R2(s.X)); w.WriteNumber("z", Json.R2(s.Z));
            w.WriteNumber("height", Json.R2(s.HeightCs1 * factor));
            w.WriteNumber("radius", s.Radius);
            w.WriteString("type", "natural");
            w.WriteNumber("rate", s.Rate);
            w.WriteBoolean("inPlayable", Math.Abs(s.X) <= half && Math.Abs(s.Z) <= half);
            if (s.Note.Length > 0) w.WriteString("note", s.Note);
            w.WriteEndObject();
        }
        w.WriteEndArray();
        w.WriteEndObject();
    }
}
