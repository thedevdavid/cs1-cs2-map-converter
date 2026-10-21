namespace NetConverter;

/// <summary>
/// Round 34c (owner: "the covered road … should be custom made out of pylon props"): the CS1 JP Rock shed (11 `rock_c` PO pieces, a ~440 m gallery of rock
/// slabs cantilevered over the 4-lane National Road at x −6453…−6245, z 677…1069, held by dark posts on the valley edge) rebuilt from pylon props, one
/// undoable import (`rock-shed`), absolute y, no sink (next-run/pylon-props.txt for the bounds):
/// - posts: Island-Type Bridge Pier `ITBP_Single-Sided Beam 01` (15.2 × 21.5 × 4 m, an L-shaped post with a one-sided cantilever, pivot at the post top,
///   beam toward local +x up to 13.2 m) every 16 m on the valley edge, beam pointing uphill over the road, post top at road y + 6.5 m; the slabs span the
///   rest of the road width (owner: no T-piers on the centreline). Variants 02–05 are unmeasured,
///   so 01 throughout; the 21.5 m post goes into the slope where the valley is shallower.
/// - roof: rock slabs resting on the beams at road y + 7 m, long axis along the road, one per 12 m over the whole length (the 11 CS1 positions
///   duplicated): `CliffRock6` (60.7 × 8.5 × 55.3 flat slab, min y −1.1), every third a `CliffRock5`.
/// - back: a `CliffStack` every 32 m on the hill side, base at road level, closing the gallery with rock.
/// </summary>
public static class RockShed
{
    public const double PostStep = 16, SlabStep = 12, PostTop = 6.5, SlabBase = 7, BeamReach = 13.2, PostLength = 21.5;

    public static (List<PlacedObject> Objects, string Log) Build(List<PoStructures.Row> rows, IEnumerable<OutEdge> edges, Func<double, double, double>? terrainTrue, double heightFactor)
    {
        var shed = rows.Where(r => r.Role.Contains("Rock shed", StringComparison.OrdinalIgnoreCase)).ToList();
        var res = new List<PlacedObject>();
        if (shed.Count == 0) return (res, "rock shed: no shed pieces in the proposal");
        var road = edges.Where(e => e.Map.Imported && e.Map.Cs2Prefab == "Medium Road")
            .Where(e => shed.Any(r => Enumerable.Range(0, 9).Select(k => e.Curve.Eval(k / 8.0)).Any(p => Math.Sqrt((p.X - r.X) * (p.X - r.X) + (p.Z - r.Z) * (p.Z - r.Z)) < 40))).ToList();
        if (road.Count == 0) return (res, "rock shed: no National Road course under the shed");
        var pts = new List<(double X, double Z, double Y, double Tx, double Tz)>();
        foreach (var e in road)
            for (int k = 0; k <= 64; k++)
            {
                double t = k / 64.0; var p = e.Curve.Eval(t); var q = e.Curve.Eval(Math.Min(1, t + 0.01)); var o = e.Curve.Eval(Math.Max(0, t - 0.01));
                double tx = q.X - o.X, tz = q.Z - o.Z, tl = Math.Sqrt(tx * tx + tz * tz);
                pts.Add((p.X, p.Z, Round9.Y(e.A) + t * (Round9.Y(e.B) - Round9.Y(e.A)), tx / tl, tz / tl));
            }
        var a = shed.MinBy(r => r.Z)!; var b = shed.MaxBy(r => r.Z)!;
        double ax = b.X - a.X, az = b.Z - a.Z, al = Math.Sqrt(ax * ax + az * az); ax /= al; az /= al;
        double S(double x, double z) => (x - a.X) * ax + (z - a.Z) * az;
        var along = pts.Select(p => (P: p, S: S(p.X, p.Z))).Where(q => q.S >= -40 && q.S <= al + 40).OrderBy(q => q.S).ToList();
        (double X, double Z, double Y, double Tx, double Tz) At(double s) => along.MinBy(q => Math.Abs(q.S - s)).P;
        double half = FootprintShift.Cs2Half(road[0]);
        double Terr(double x, double z) => terrainTrue != null ? terrainTrue(x, z) * heightFactor : double.MinValue;
        var m0 = At(al / 2); double nx = -m0.Tz, nz = m0.Tx;   // uphill normal
        if (terrainTrue != null && Terr(m0.X + nx * 30, m0.Z + nz * 30) < Terr(m0.X - nx * 30, m0.Z - nz * 30)) { nx = -nx; nz = -nz; }
        double Yaw(double tx, double tz) => ((Math.Atan2(tx, tz) * 180 / Math.PI) % 360 + 360) % 360;
        PlacedObject O(string id, string prefab, double x, double z, double y, double yaw, string note) =>
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (pylon props)", note);
        // Posts: the beam's local +x has to point uphill: local +x = (cos r, −sin r) in (x, z), so r = atan2(−nz, nx).
        double beamYaw = Math.Atan2(-nz, nx) * 180 / Math.PI;
        // Owner decision (round 34d): as in CS1, posts on the valley edge only, never T-piers on the centreline; the rock slabs (CliffRock6 55 m, CliffRock5
        // 36 m across) span the rest of the 25 m road to the hill-side rock by themselves.
        const bool median = false;
        int posts = 0, floating = 0, sunk = 0;
        for (double s = 0; s <= al + 0.01; s += PostStep)
        {
            var c = At(s); double top = c.Y + PostTop;
            if (median)
            {
                res.Add(O($"shed_post_{++posts}", "ITBP_Pillar Median 01", c.X, c.Z, top, Yaw(c.Tx, c.Tz) + 90, "T-pier on the centreline (road wider than the cantilever)"));
                double gap = Terr(c.X, c.Z) - (top - PostLength); if (gap < -1) floating++; else if (gap > 0.5) sunk++;
            }
            else
            {
                double px = c.X - nx * (half + 1), pz = c.Z - nz * (half + 1);
                res.Add(O($"shed_post_{++posts}", "ITBP_Single-Sided Beam 01", px, pz, top, beamYaw, "valley-edge post, beam over the road toward the hill"));
                double gap = Terr(px, pz) - (top - PostLength); if (gap < -1) floating++; else if (gap > 0.5) sunk++;
            }
        }
        // Roof slabs: long axis (local x) along the road: local +x = (cos r, −sin r) = (tx, tz) → r = atan2(−tz, tx).
        int slabs = 0;
        for (double s = 0; s <= al + 0.01; s += SlabStep)
        {
            var c = At(s); slabs++;
            string prefab = slabs % 3 == 0 ? "CliffRock5" : "CliffRock6";
            double minY = prefab == "CliffRock6" ? -1.10 : -2.57;
            res.Add(O($"shed_slab_{slabs}", prefab, c.X + nx * 2, c.Z + nz * 2, c.Y + SlabBase - minY, Math.Atan2(-c.Tz, c.Tx) * 180 / Math.PI, "rock slab on the beams, long axis along the road"));
        }
        // Back: CliffStack (60.97 × 48.59 × 67.9, min y −21.62) on the hill side every 32 m, base at road level.
        int backs = 0;
        for (double s = 0; s <= al + 0.01; s += 32)
        {
            var c = At(s);
            res.Add(O($"shed_back_{++backs}", "CliffStack", c.X + nx * (half + 18), c.Z + nz * (half + 18), c.Y + 21.62, Math.Atan2(-c.Tz, c.Tx) * 180 / Math.PI, "hill-side rock closing the gallery"));
        }
        return (res, FormattableString.Invariant($"rock shed from pylon props over {al:F0} m of {string.Join(", ", road.Select(e => e.Id))}: {posts} {(median ? "ITBP_Pillar Median 01 on the centreline (road " + (2 * half).ToString("F0") + " m wider than the 13.2 m cantilever)" : "ITBP_Single-Sided Beam 01")} (post base vs terrain: {floating} short by > 1 m, {sunk} sunk > 0.5 m), {slabs} roof slabs (CliffRock6 / CliffRock5) at road + 7 m, {backs} CliffStack on the hill side"));
    }
}
