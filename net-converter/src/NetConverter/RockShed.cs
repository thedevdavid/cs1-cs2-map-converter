namespace NetConverter;

/// <summary>
/// Round 34 (owner, A-v9 shot 24: the covered road): the CS1 JP Rock shed (11 `rock_c` PO pieces, a 430 m rock-faced gallery over the 4-lane National Road at
/// x −6453…−6245, z 677…1069) has no CS2 asset. A CS2 tunnel would need rock cover above the road, and hm-rebuild is carving that corridor down to the road
/// (road-terrain-mismatch.json), so the stand-in is a gallery wall: ConcreteSet01 `Wall32x16` every 32 m along the uphill side of the road and `Pillar4x16`
/// columns along the downhill side, both with their base at road height, between the first and the last shed piece. The cliff pieces of the round-34 rule
/// line the slope above where it is ≥ 35°.
/// </summary>
public static class RockShed
{
    public const string Wall = "Wall32x16", Column = "Pillar4x16";
    public const double Step = 32;

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase) || r.Prefab.Contains("rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        if (shed.Count == 0) shed = rows.Where(r => r.Role.Contains("rock", StringComparison.OrdinalIgnoreCase) && r.Role.Contains("shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        // The road under the shed: imported courses that pass within 40 m of a shed piece, the National Road (Medium Road) ones.
        var road = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab == "Medium Road")
            .Where(e => shed.Any(r => Enumerable.Range(0, 9).Select(k => e.Curve.Eval(k / 8.0)).Any(p => Math.Sqrt((p.X - r.X) * (p.X - r.X) + (p.Z - r.Z) * (p.Z - r.Z)) < 40))).ToList();
        if (road.Count == 0) return (res, "rock shed: no National Road course under the shed");
        // Dense samples along those courses: (x, z, output y, tangent).
        var pts = new List<(double X, double Z, double Y, double Tx, double Tz)>();
        foreach (var e in road)
            for (int k = 0; k <= 32; k++)
            {
                double t = k / 32.0; var p = e.Curve.Eval(t); var q = e.Curve.Eval(Math.Min(1, t + 0.01)); var o = e.Curve.Eval(Math.Max(0, t - 0.01));
                double tx = q.X - o.X, tz = q.Z - o.Z, tl = Math.Sqrt(tx * tx + tz * tz);
                pts.Add((p.X, p.Z, Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A)), tx / tl, tz / tl));
            }
        // Order along the shed axis (first to last piece), keep the stretch between them.
        var a = shed.MinBy(r => r.Z)!; var b = shed.MaxBy(r => r.Z)!;
        double ax = b.X - a.X, az = b.Z - a.Z, al = Math.Sqrt(ax * ax + az * az); ax /= al; az /= al;
        double S(double x, double z) => (x - a.X) * ax + (z - a.Z) * az;
        var along = pts.Select(p => (P: p, S: S(p.X, p.Z))).Where(q => q.S >= -10 && q.S <= al + 10).OrderBy(q => q.S).ToList();
        double half = FootprintShift.Cs2Half(road[0]);
        int k2 = 0;
        for (double s = 0; s <= al; s += Step)
        {
            var c = along.MinBy(q => Math.Abs(q.S - s)).P;
            // Uphill: the side where the playable terrain is higher.
            double nx = -c.Tz, nz = c.Tx;
            double up = terrainTrue == null ? 1 : terrainTrue(c.X + nx * 30, c.Z + nz * 30) - terrainTrue(c.X - nx * 30, c.Z - nz * 30);
            if (up < 0) { nx = -nx; nz = -nz; }
            double yawRoad = ((Math.Atan2(c.Tx, c.Tz) * 180 / Math.PI) % 360 + 360) % 360;
            k2++;
            res.Add(new PlacedObject($"shed_wall_{k2}", Wall, "object", Math.Round(c.X + nx * (half + 1.5), 2), Math.Round(c.Z + nz * (half + 1.5), 2), Math.Round(c.Y, 2),
                Math.Round((yawRoad + 270) % 360, 1), "JP Rock shed stand-in", "gallery wall, uphill side, base at road height (Wall32x16 long side along the road [U])"));
            res.Add(new PlacedObject($"shed_col_{k2}", Column, "object", Math.Round(c.X - nx * (half + 2), 2), Math.Round(c.Z - nz * (half + 2), 2), Math.Round(c.Y, 2),
                Math.Round(yawRoad, 1), "JP Rock shed stand-in", "gallery column, downhill side, base at road height"));
        }
        return (res, FormattableString.Invariant($"rock shed stand-in: {k2} Wall32x16 + {k2} Pillar4x16 over {al:F0} m of {string.Join(", ", road.Select(e => e.Id))} (no CS2 tunnel: the corridor is being carved, no rock cover)"));
    }
}
