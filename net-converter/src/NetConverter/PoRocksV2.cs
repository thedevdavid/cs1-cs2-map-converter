using System.Globalization;
using System.Text.RegularExpressions;

namespace NetConverter;

/// <summary>
/// PO rock objects, v3 (owner reviews of A-v3 and A-v4). The terrain already carries the PO mountain mass; the objects add rock:
/// <list type="bullet">
/// <item>every piece sits with its mesh bottom (live bounds, prefab-bounds.json) H − v below the terrain: y = terrain − bounds.min.y − (H − v),
/// visible v = clamp(0.45 H, 15, 70) m for pieces of H ≥ 12 m, 0.6 H for rocks under 12 m (v2 ignored bounds.min.y and buried the cliffs);</item>
/// <item>a PO object rising more than 60 m above the terrain around it gets CliffSide pieces down its steepest flank every 50 m instead of one
/// piece at the summit; other objects a CliffRock / CliffStack (≥ 36 m), rzrock (12–36 m) or Boulder (&lt; 12 m) at the summit;</item>
/// <item>extra rzrock / Boulder pieces inside large footprints; no two pieces within 80 m share prefab and heading (± 20°);</item>
/// <item>largest PO objects first, inside the map, capped at 1 500.</item>
/// </list>
/// </summary>
public static class PoRocksV2
{
    static readonly CultureInfo Ci = CultureInfo.InvariantCulture;
    static readonly string[] Boulders = Enumerable.Range(1, 8).Select(i => $"Boulder{i:00}").ToArray();
    static readonly string[] RzRocks = Enumerable.Range(1, 8).Select(i => $"rzrock{i}").ToArray();
    static readonly string[] MidCliffs = ["CliffRock5", "CliffRock6", "CliffStack"];
    static readonly string[] FlankCliffs = ["CliffSide2", "CliffSide2Left", "CliffSide2Right", "CliffSideTall", "CliffSlim"];

    /// <summary>Heights (H of the W × D × H footprint) from cs2-playset/ASSET_PACKS.md, the fallback where no live bounds exist.</summary>
    public static Dictionary<string, double> LoadHeights(string assetPacksMd)
    {
        var res = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(assetPacksMd)) return res;
        var rx = new Regex(@"^\| `([^`]+)` \|.*\| ([\d.]+) × ([\d.]+) × ([\d.]+) \|");
        foreach (var line in File.ReadLines(assetPacksMd))
            if (rx.Match(line) is { Success: true } m) res[m.Groups[1].Value] = double.Parse(m.Groups[4].Value, Ci);
        return res;
    }

    public static double Visible(double h) => h < 12 ? 0.6 * h : Math.Min(h, Math.Clamp(0.45 * h, 15, 70));

    public sealed record Placed(PlacedObject O, double Sink);

    public static (List<Placed> Objects, int Capped, int FlankObjects, int Renamed) Build(List<Round8.PoPiece> pieces, Dictionary<string, PrefabBound> bounds,
        Dictionary<string, double> heights, Func<double, double, double>? terrainTrue, double heightFactor, double half, ulong seed, int cap = 1500)
    {
        // Candidates: (PO object, index, position, kind, heading) with kind 0 summit, 1 flank, 2 extra inside the footprint.
        var cands = new List<(double Size, Round8.PoPiece P, int K, double X, double Z, int Kind, double? Yaw)>();
        int flankObjects = 0;
        foreach (var p in pieces.OrderByDescending(p => Math.Max(p.Length, p.Width)).ThenBy(p => p.Id))
        {
            double size = Math.Max(p.Length, p.Width);
            if (size < 3) continue;
            var rng = new Rng(Rng.Mix(seed, (ulong)p.Id));
            bool flank = false;
            if (terrainTrue != null && size >= 36)
            {
                // Rise: summit terrain over the median of a ring just outside the footprint.
                double r = size / 2 + 30, top = terrainTrue(p.SummitX, p.SummitZ);
                var ring = Enumerable.Range(0, 16).Select(k => terrainTrue(p.X + r * Math.Sin(k * Math.PI / 8), p.Z + r * Math.Cos(k * Math.PI / 8))).OrderBy(v => v).ToList();
                flank = top - ring[ring.Count / 2] > 60;
            }
            int k = 0;
            if (flank)
            {
                // Steepest flank: the direction with the largest drop from the summit to the footprint edge (16 directions).
                double reach = size / 2, top = terrainTrue!(p.SummitX, p.SummitZ), bestDrop = double.MinValue, bdx = 0, bdz = 1;
                for (int d = 0; d < 16; d++)
                {
                    double a = d * Math.PI / 8, dx = Math.Sin(a), dz = Math.Cos(a);
                    double drop = top - terrainTrue(p.SummitX + dx * reach, p.SummitZ + dz * reach);
                    if (drop > bestDrop) { bestDrop = drop; bdx = dx; bdz = dz; }
                }
                double yaw = Math.Atan2(bdx, bdz) * 180 / Math.PI;
                int before = cands.Count;
                for (double s = 25; s <= Math.Max(25, reach); s += 50)
                {
                    double x = p.SummitX + bdx * s, z = p.SummitZ + bdz * s;
                    if (Math.Abs(x) > half - 1 || Math.Abs(z) > half - 1) continue;
                    cands.Add((size, p, k++, x, z, 1, yaw));
                }
                if (cands.Count > before) flankObjects++;
            }
            else if (Math.Abs(p.SummitX) <= half - 1 && Math.Abs(p.SummitZ) <= half - 1) cands.Add((size, p, k++, p.SummitX, p.SummitZ, 0, null));
            int extra = size < 36 ? 0 : Math.Clamp((int)Math.Round(size / 70), 1, 4) - 1;
            double rr = p.Yaw * Math.PI / 180, fx = Math.Sin(rr), fz = Math.Cos(rr);
            for (int e = 0; e < extra; e++)
            {
                double u = (rng.NextDouble() - 0.5) * p.Length * 0.7, w = (rng.NextDouble() - 0.5) * p.Width * 0.7;
                double x = p.X + u * fx + w * fz, z = p.Z + u * fz - w * fx;
                if (Math.Abs(x) > half - 1 || Math.Abs(z) > half - 1) continue;
                cands.Add((size, p, k++, x, z, 2, null));
            }
        }
        var kept = cands.Take(cap).ToList();
        var res = new List<Placed>();
        int renamed = 0;
        foreach (var c in kept)
        {
            var rng = new Rng(Rng.Mix(seed ^ 0x5EEDUL, (ulong)(c.P.Id * 64 + c.K)));
            double size = Math.Max(c.P.Length, c.P.Width);
            string[] set = c.Kind == 1 ? FlankCliffs : c.Kind == 2 ? (c.K % 3 == 2 ? Boulders : RzRocks) : size < 12 ? Boulders : size < 36 ? RzRocks : MidCliffs;
            int pick = rng.Next(set.Length);
            double yaw = c.Yaw is { } fy ? fy + (rng.NextDouble() - 0.5) * 30 : rng.NextDouble() * 360;
            // No two pieces within 80 m with the same prefab and heading (± 20°): try the other prefabs of the set, then turn.
            for (int tries = 0; tries < set.Length * 4; tries++)
            {
                string pf = set[(pick + tries % set.Length) % set.Length];
                double yw = (yaw + tries / set.Length * 97) % 360;
                bool clash = res.Any(o => o.O.Prefab == pf && (o.O.X - c.X) * (o.O.X - c.X) + (o.O.Z - c.Z) * (o.O.Z - c.Z) < 80 * 80
                    && Math.Abs(((o.O.Rotation - yw) % 360 + 540) % 360 - 180) < 20);
                if (!clash) { if (tries > 0) renamed++; pick = (pick + tries % set.Length) % set.Length; yaw = yw; break; }
            }
            string prefab = set[pick];
            double? y = null, sink = 0;
            if (terrainTrue != null && bounds.TryGetValue(prefab, out var b))
            {
                sink = b.Height - Visible(b.Height);
                y = b.Y(terrainTrue(c.X, c.Z) * heightFactor, sink.Value);
            }
            else if (terrainTrue != null && heights.TryGetValue(prefab, out var h))
            {
                sink = h - Visible(h);   // no live bounds: pivot assumed at the mesh bottom
                y = Math.Round(terrainTrue(c.X, c.Z) * heightFactor - sink.Value, 2);
            }
            res.Add(new Placed(new PlacedObject($"porock{c.P.Id}_{c.K}", prefab, "prop", Math.Round(c.X, 2), Math.Round(c.Z, 2), y, Math.Round(((yaw % 360) + 360) % 360, 1),
                $"PO {c.P.SubClass}", $"po {c.P.Id}"), sink ?? 0));
        }
        return (res, cands.Count - kept.Count, flankObjects, renamed);
    }

    public static string SinkHistogram(IEnumerable<Placed> objs)
    {
        var bands = new (string Name, double Lo, double Hi)[] { ("< 1 m", -1e9, 1), ("1–5 m", 1, 5), ("5–10 m", 5, 10), ("10–20 m", 10, 20), ("20–50 m", 20, 50), ("50–100 m", 50, 100), ("> 100 m", 100, 1e9) };
        var list = objs.ToList();
        return string.Join(", ", bands.Select(b => (b.Name, N: list.Count(o => o.Sink >= b.Lo && o.Sink < b.Hi))).Where(b => b.N > 0).Select(b => $"{b.Name} {b.N}"));
    }
}
