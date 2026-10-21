namespace NetConverter;

/// <summary>
/// Round 34e (owner: "use the vanilla pylon props, and make it partially open", next-run/ref-gallery-1/2.webp, alpine avalanche galleries): the CS1 JP Rock
/// shed (~440 m over the 4-lane National Road at x −6453…−6245, z 677…1069) as an open-sided gallery, one undoable import (`rock-shed`), absolute y, no sink:
/// - hill side closed: ConcreteSet01 `Wall32x16` (32 × 16 × 1 m, pivot centre-bottom) from road level, every 32 m;
/// - roof: square plates at road y + 6.5 m, valley edge on the column line: `BasePlateConcrete62` (153995, 62.5 m) where the road rises ≤ 1.2 m under it,
///   else `BasePlateConcrete32`, else `BasePlateConcrete16` (two across the road);
/// - valley side open: a column every 8 m on the valley edge from the terrain to the slab underside, ConcreteSet01 Pillar2x16 / Pillar2x8 stacked (owner:
///   "use the packs, not the vanilla");
/// - the 11 CS1 `rock_c` positions: cliff pieces behind the wall on the hill side (no rock on the roof).
/// - guardrail: none yet (no vanilla guardrail prop or net confirmed).
/// </summary>
public static class RockShed
{
    public const double Bay = 32, ColumnStep = 8, Clear = 6.5;

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
            new(id, prefab, "object", Math.Round(x, 2), Math.Round(z, 2), Math.Round(y, 2), Math.Round(((yaw % 360) + 360) % 360, 1), "JP Rock shed (open gallery)", note);
        int walls = 0, cols = 0;
        // Roof (owner: packs 154132 and 153995): square plates, the biggest that stays flat enough. A plate of side L over [s0, s0 + L] is used when the road
        // rises at most 1.2 m under it: BasePlateConcrete62 (62.5 m), else two BasePlateConcrete32 halves, else BasePlateConcrete16 (two side by side
        // across the 25 m road). Every plate keeps its valley edge on the column line, so a wider plate reaches back over the wall into the hill.
        var roofAt = new List<(double S0, double S1, double Y)>();
        var plates = new Dictionary<string, int> { ["BasePlateConcrete62"] = 0, ["BasePlateConcrete32"] = 0, ["BasePlateConcrete16"] = 0 };
        double Rise(double s0, double s1) { var ys = Enumerable.Range(0, 9).Select(k => At(s0 + (s1 - s0) * k / 8.0).Y).ToList(); return ys.Max() - ys.Min(); }
        void Roof(double s0, double size)
        {
            string prefab = size > 60 ? "BasePlateConcrete62" : size > 30 ? "BasePlateConcrete32" : "BasePlateConcrete16";
            if (size > 20 && Rise(s0, s0 + size) > 1.2) { double hs = size > 60 ? 32 : 16; Roof(s0, hs); if (s0 + hs < Math.Min(al, s0 + size)) Roof(s0 + hs, size > 60 ? size - hs : hs); return; }
            var c = At(s0 + size / 2); double y = c.Y + Clear;
            int across = (int)Math.Ceiling((2 * half + 1) / size);
            for (int k = 0; k < across; k++)
            {
                double off = -(half + 0.5) + size / 2 + k * size;    // from the road centre toward the hill: valley edge on the column line
                res.Add(O($"shed_roof_{plates.Values.Sum() + 1}", prefab, c.X + nx * off, c.Z + nz * off, y, Yaw(c.Tx, c.Tz), $"roof plate {size:F1} m at road + 6.5 m"));
                plates[prefab]++;
            }
            roofAt.Add((s0, s0 + size, y));
        }
        double s62 = 0;
        for (; s62 + 62.5 <= al + 5; s62 += 62.5) Roof(s62, 62.5);
        for (double s = s62; s < al - 2; s += 32) Roof(s, 32);   // the remainder: no 62.5 m plate past the end of the shed
        for (double s = 0; s < al; s += 32)
        {
            var c = At(s + 16);
            res.Add(O($"shed_wall_{++walls}", "Wall32x16", c.X + nx * (half + 1), c.Z + nz * (half + 1), c.Y, Yaw(c.Tx, c.Tz) - 90, "hill-side wall from road level (32 m side along the road)"));
        }
        // Valley-side columns every 8 m (owner: packs only): ConcreteSet01 Pillar2x16 / Pillar2x8 (pivot centre-bottom) stacked from the slab underside down
        // to the terrain, 16 m pieces while more than 8 m remain, then one 8 m piece; the lowest piece goes into the ground by what is left over.
        int stacks = 0, p16 = 0, p8 = 0;
        for (double s = 0; s <= al + 0.01; s += ColumnStep)
        {
            var c = At(s); double underside = (roofAt.FirstOrDefault(r => s >= r.S0 - 0.01 && s <= r.S1 + 0.01) is { S1: > 0 } r ? r.Y : c.Y + Clear);
            double px = c.X - nx * (half + 0.5), pz = c.Z - nz * (half + 0.5), ground = terrainTrue != null ? Terr(px, pz) : c.Y;
            double top = underside, left = Math.Max(underside - ground, 0.5); int piece = 0; stacks++;
            while (left > 0.05)
            {
                double h = left > 8 ? 16 : 8;
                res.Add(O($"shed_col_{stacks}_{++piece}", h > 8 ? "Pillar2x16" : "Pillar2x8", px, pz, top - h, Yaw(c.Tx, c.Tz), $"valley-side column, piece {piece} from the roof underside down"));
                if (h > 8) p16++; else p8++;
                top -= h; left -= h; cols++;
            }
        }
        // The CS1 rock_c pieces as rock behind the wall on the hill side.
        int rocks = 0;
        foreach (var r in shed)
        {
            var c = At(S(r.X, r.Z));
            res.Add(O($"shed_rock_{r.Id}", "CliffRock5", c.X + nx * (half + 12), c.Z + nz * (half + 12), c.Y + 2.57, Yaw(c.Tx, c.Tz), $"rock behind the hill-side wall at CS1 rock_c {r.Id}"));
            rocks++;
        }
        return (res, FormattableString.Invariant($"rock shed open gallery over {al:F0} m of {string.Join(", ", road.Select(e => e.Id))}: roof {plates["BasePlateConcrete62"]} BasePlateConcrete62 + {plates["BasePlateConcrete32"]} BasePlateConcrete32 + {plates["BasePlateConcrete16"]} BasePlateConcrete16 at road + 6.5 m, {walls} Wall32x16 on the hill side, {stacks} valley-side columns every 8 m ({p16} Pillar2x16 + {p8} Pillar2x8 stacked to the terrain), {rocks} CliffRock5 behind the wall; no guardrail"));
    }
}
