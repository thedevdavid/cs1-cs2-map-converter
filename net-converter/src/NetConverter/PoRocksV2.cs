using System.Globalization;
using System.Text.RegularExpressions;

namespace NetConverter;

/// <summary>
/// PO rock objects, v2 (owner review of A-v3: the v1 cliffs stood 140–195 m tall on terrain that already carries the mountain mass,
/// grass tops floating, rows of identical mesas). The objects only add rock texture now:
/// <list type="bullet">
/// <item>every piece of height H ≥ 12 m is sunk: absolute y = terrain − (H − v), v = clamp(0.25 H, 8, 35) m visible; smaller rocks sit on the terrain (no y);</item>
/// <item>small pieces for most PO objects (CliffRock5/6, CliffStack, rzrock1–8, Boulder01–08); one CliffSide*/CliffFreeStanding only on a PO mountain
/// that rises more than 120 m above the terrain around it;</item>
/// <item>no two pieces within 80 m share prefab and heading (± 20°);</item>
/// <item>largest PO objects first, capped at 700.</item>
/// </list>
/// </summary>
public static class PoRocksV2
{
    static readonly CultureInfo Ci = CultureInfo.InvariantCulture;
    static readonly string[] Boulders = Enumerable.Range(1, 8).Select(i => $"Boulder{i:00}").ToArray();
    static readonly string[] RzRocks = Enumerable.Range(1, 8).Select(i => $"rzrock{i}").ToArray();
    static readonly string[] MidCliffs = ["CliffRock5", "CliffRock6", "CliffStack"];
    static readonly string[] BigFlat = ["CliffSide2", "CliffSide2Left", "CliffSide2Right"];

    /// <summary>Heights (H of the W × D × H footprint) from cs2-playset/ASSET_PACKS.md; prefabs without a row count as small rocks.</summary>
    public static Dictionary<string, double> LoadHeights(string assetPacksMd)
    {
        var res = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(assetPacksMd)) return res;
        var rx = new Regex(@"^\| `([^`]+)` \|.*\| ([\d.]+) × ([\d.]+) × ([\d.]+) \|");
        foreach (var line in File.ReadLines(assetPacksMd))
            if (rx.Match(line) is { Success: true } m) res[m.Groups[1].Value] = double.Parse(m.Groups[4].Value, Ci);
        return res;
    }

    public static double Visible(double h) => h < 12 ? h : Math.Clamp(0.25 * h, 8, 35);

    public sealed record Placed(PlacedObject O, double Sink);

    public static (List<Placed> Objects, int Capped, int BigPieces, int Renamed) Build(List<Round8.PoPiece> pieces, Dictionary<string, double> heights,
        Func<double, double, double>? terrainTrue, double heightFactor, double half, ulong seed, int cap = 700)
    {
        // Candidates per PO object: count by footprint, largest objects first.
        var cands = new List<(double Size, Round8.PoPiece P, int K, double X, double Z, bool Big)>();
        foreach (var p in pieces.OrderByDescending(p => Math.Max(p.Length, p.Width)).ThenBy(p => p.Id))
        {
            double size = Math.Max(p.Length, p.Width);
            if (size < 3) continue;
            bool big = false;
            if (p.SubClass == "mountain" && terrainTrue != null)
            {
                // Prominence: summit terrain over the median of a ring just outside the footprint.
                double r = size / 2 + 30, top = terrainTrue(p.SummitX, p.SummitZ);
                var ring = Enumerable.Range(0, 16).Select(k => terrainTrue(p.X + r * Math.Sin(k * Math.PI / 8), p.Z + r * Math.Cos(k * Math.PI / 8))).OrderBy(v => v).ToList();
                big = top - ring[ring.Count / 2] > 120;
            }
            int n = size < 36 ? 1 : Math.Clamp((int)Math.Round(size / 70), 1, 4);
            var rng = new Rng(Rng.Mix(seed, (ulong)p.Id));
            double rr = p.Yaw * Math.PI / 180, fx = Math.Sin(rr), fz = Math.Cos(rr);
            for (int k = 0; k < n; k++)
            {
                // k = 0 at the summit, the rest spread inside the footprint in the object's frame.
                double u = k == 0 ? 0 : (rng.NextDouble() - 0.5) * p.Length * 0.7, w = k == 0 ? 0 : (rng.NextDouble() - 0.5) * p.Width * 0.7;
                double x = (k == 0 ? p.SummitX : p.X) + u * fx + w * fz, z = (k == 0 ? p.SummitZ : p.Z) + u * fz - w * fx;
                if (Math.Abs(x) > half - 1 || Math.Abs(z) > half - 1) continue;   // outside the map: before the cap
                cands.Add((size, p, k, x, z, big && k == 0));
            }
        }
        var kept = cands.Take(cap).ToList();
        var res = new List<Placed>();
        int bigCount = 0, renamed = 0;
        foreach (var c in kept)
        {
            if (Math.Abs(c.X) > half - 1 || Math.Abs(c.Z) > half - 1) continue;
            var rng = new Rng(Rng.Mix(seed ^ 0x5EEDUL, (ulong)(c.P.Id * 8 + c.K)));
            double size = Math.Max(c.P.Length, c.P.Width), aspect = size / Math.Max(1, Math.Min(c.P.Length, c.P.Width));
            // The summit piece of a large object is a CliffRock / CliffStack; the extra pieces inside its footprint are RZ rocks and boulders.
            string[] set = c.Big ? (aspect >= 1.4 ? BigFlat : ["CliffFreeStanding"]) : size < 12 ? Boulders : size < 36 || c.K > 0 ? (c.K % 3 == 2 ? Boulders : RzRocks) : MidCliffs;
            int pick = rng.Next(set.Length);
            double yaw = rng.NextDouble() * 360;
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
            double h = heights.TryGetValue(prefab, out var hh) ? hh : 0;
            double sink = h - Visible(h);
            double? y = sink > 0 && terrainTrue != null ? Math.Round(terrainTrue(c.X, c.Z) * heightFactor - sink, 2) : null;
            if (c.Big) bigCount++;
            res.Add(new Placed(new PlacedObject($"porock{c.P.Id}_{c.K}", prefab, "prop", Math.Round(c.X, 2), Math.Round(c.Z, 2), y, Math.Round(yaw, 1),
                $"PO {c.P.SubClass}", $"po {c.P.Id}"), y != null ? sink : 0));
        }
        return (res, cands.Count - kept.Count, bigCount, renamed);
    }

    public static string SinkHistogram(IEnumerable<Placed> objs)
    {
        var bands = new (string Name, double Lo, double Hi)[] { ("0 (on terrain)", -1, 0.001), ("0–5 m", 0.001, 5), ("5–10 m", 5, 10), ("10–20 m", 10, 20), ("20–50 m", 20, 50), ("50–100 m", 50, 100), ("> 100 m", 100, 1e9) };
        var list = objs.ToList();
        return string.Join(", ", bands.Select(b => (b.Name, N: list.Count(o => o.Sink > b.Lo && o.Sink <= b.Hi || (b.Lo < 0 && o.Sink <= 0.001)))).Where(b => b.N > 0).Select(b => $"{b.Name} {b.N}"));
    }
}
