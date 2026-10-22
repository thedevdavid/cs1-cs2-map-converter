namespace NetConverter;

/// <summary>
/// A course that runs inside another net's footprint is dropped by the game (IMPORT_SCHEMA.md, notes from the first full run):
/// every sample closer to that segment's centre line than the two half-widths. Run A-v3: 20 QuaySmall01 walls and 9 decor
/// pieces beside highways. Each such course is moved sideways, away from the highway (as the bridge did live); where the move
/// lands it inside another highway (walls between carriageways) it stays and is listed as impossible.
/// </summary>
public static class FootprintShift
{
    /// <summary>CS2 half-widths from the bridge's drop messages (two half-widths: 1 lane + quay 5.5, 2 lanes 7.5, 3 lanes 9.5, 3 + 4 lanes 18).</summary>
    public static double Cs2Half(string? prefab, double fallback) => prefab switch
    {
        "Highway Oneway - 1 lane" => 4, "Highway Oneway - 2 lanes" => 6, "Highway Oneway - 3 lanes" => 8, "Highway Oneway - 4 lanes" => 10,
        "Highway Twoway - 4 lanes" => 10, "Highway Twoway - 2 lanes" => 6, "Highway Twoway - 3 lanes" => 8,   // round 43/47: GET /prefabs/info widths 20, 12, 16 m
        "QuaySmall01" or "Arc Pedestrian Bridge" => 1.5,
        _ => fallback,
    };

    public static double Cs2Half(OutEdge e) => Cs2Half(e.Map.Cs2Prefab, e.Info.HalfWidth);

    public static (double X, double Z)[] Samples(Bezier b, int n = 16) => Enumerable.Range(0, n + 1).Select(k => { var p = b.Eval(k / (double)n); return (p.X, p.Z); }).ToArray();

    static (double D, double Nx, double Nz) Dist((double X, double Z) p, IEnumerable<(double X, double Z)[]> polys)
    {
        double best = double.MaxValue, bx = 0, bz = 0;
        foreach (var poly in polys)
            for (int i = 1; i < poly.Length; i++)
            {
                var (ax, az) = poly[i - 1]; double dx = poly[i].X - ax, dz = poly[i].Z - az, l2 = dx * dx + dz * dz;
                double t = l2 > 0 ? Math.Clamp(((p.X - ax) * dx + (p.Z - az) * dz) / l2, 0, 1) : 0;
                double qx = ax + t * dx, qz = az + t * dz, d = Math.Sqrt((p.X - qx) * (p.X - qx) + (p.Z - qz) * (p.Z - qz));
                if (d < best) { best = d; bx = p.X - qx; bz = p.Z - qz; }
            }
        double l = Math.Sqrt(bx * bx + bz * bz);
        return (best, l > 0 ? bx / l : 0, l > 0 ? bz / l : 0);
    }

    /// <summary>Highway courses the game would merge into one segment: same prefab, joined through nodes where exactly two courses meet.</summary>
    public sealed record Chain(OutEdge E, List<(double X, double Z)[]> P, HashSet<OutNode> Nodes);

    public static List<Chain> Highways(ConvertResult res)
    {
        var hw = res.Layer(Layers.Highways).ToList();
        var at = new Dictionary<OutNode, List<OutEdge>>(ReferenceEqualityComparer.Instance);
        foreach (var e in hw) foreach (var n in new[] { e.A, e.B }) (at.TryGetValue(n, out var l) ? l : at[n] = new()).Add(e);
        var seen = new HashSet<OutEdge>(ReferenceEqualityComparer.Instance);
        var chains = new List<Chain>();
        OutEdge? Next(OutEdge e, OutNode n) => at[n].Count == 2 && at[n].Single(o => o != e) is var o && o.Map.Cs2Prefab == e.Map.Cs2Prefab ? o : null;
        foreach (var e0 in hw)
        {
            if (!seen.Add(e0)) continue;
            var members = new List<OutEdge> { e0 };
            foreach (var start in new[] { e0.A, e0.B })
            {
                var (e, n) = (e0, start);
                while (Next(e, n) is { } o && seen.Add(o)) { members.Add(o); n = o.A == n ? o.B : o.A; e = o; }
            }
            chains.Add(new Chain(e0, members.Select(m => Samples(m.Curve, 16)).ToList(), members.SelectMany(m => new[] { m.A, m.B }).ToHashSet<OutNode>(ReferenceEqualityComparer.Instance)));
        }
        return chains;
    }

    /// <summary>
    /// The highway chain whose footprint swallows the whole course (every sample inside the two half-widths), if any. Planar by
    /// default, as the game check: Run A-v3 dropped quays raised 4–8 m and elevated decor pieces too.
    /// </summary>
    public static (OutEdge H, double MinD, double Need, double Nx, double Nz)? Inside((double X, double Z)[] course, double half, double y,
        IReadOnlyList<Chain> highways, ISet<OutNode> exclude, double topologyFactor, double maxDy = double.PositiveInfinity)
    {
        (OutEdge, double, double, double, double)? best = null;
        foreach (var c in highways)
        {
            if (c.Nodes.Overlaps(exclude)) continue;
            if (Math.Abs((c.E.A.Position.Y + c.E.B.Position.Y) / 2 - y) / topologyFactor > maxDy) continue;
            double need = Cs2Half(c.E) + half;
            var ds = course.Select(p => Dist(p, c.P)).ToArray();
            if (ds.Any(d => d.D >= need)) continue;
            double min = ds.Min(d => d.D);
            double nx = ds.Average(d => d.Nx), nz = ds.Average(d => d.Nz), l = Math.Sqrt(nx * nx + nz * nz);
            if (l < 1e-6) continue;
            if (best == null || min < best.Value.Item2) best = (c.E, min, need, nx / l, nz / l);
        }
        return best;
    }

    /// <summary>Drop messages in run reports: id → (the prefab it ran inside, the sideways move the bridge asks for).</summary>
    public static Dictionary<string, (string Prefab, double Move)> ParseDrops(IEnumerable<string> reportPaths)
    {
        var rx = new System.Text.RegularExpressions.Regex(@"^- (\S+) .*?: dropped by the game: the course runs inside the existing '(.+?)' segment .*?move it at least ([\d.]+) m sideways");
        var res = new Dictionary<string, (string, double)>();
        foreach (var p in reportPaths)
            foreach (var line in File.ReadLines(p))
                if (rx.Match(line) is { Success: true } m) res[m.Groups[1].Value] = (m.Groups[2].Value, double.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture));
        return res;
    }

    public sealed record Result(List<string> Shifted, List<string> Impossible);

    /// <summary>
    /// With drop messages (run reports): each dropped course moves the asked distance + 1 m away from the nearest chain of the named
    /// prefab. Without: every course a highway chain at a similar height (≤ 6 m) swallows moves until it clears the two half-widths
    /// + 1 m. Either way a course that the move puts inside another chain (walls between carriageways) stays and is listed as impossible.
    /// </summary>
    public static Result Apply(ConvertResult res, IEnumerable<OutEdge> movable, double topologyFactor, IReadOnlyDictionary<string, (string Prefab, double Move)>? drops = null)
    {
        var hw = Highways(res);
        var moved = new HashSet<OutNode>(ReferenceEqualityComparer.Instance);
        var shifted = new List<string>(); var impossible = new List<string>();
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var e in movable.ToList())
        {
            var ex = new HashSet<OutNode>(ReferenceEqualityComparer.Instance) { e.A, e.B };
            double half = Cs2Half(e), y = (e.A.Position.Y + e.B.Position.Y) / 2;
            var course = Samples(e.Curve);
            double s, nx, nz; string what;
            if (drops != null)
            {
                if (!drops.TryGetValue(e.Id, out var d)) continue;
                var cands = hw.Where(c => c.E.Map.Cs2Prefab == d.Prefab && !c.Nodes.Overlaps(ex)).Select(c => (c, ds: course.Select(p => Dist(p, c.P)).ToArray())).ToList();
                if (cands.Count == 0) continue;
                var near = cands.MinBy(q => q.ds.Average(x => x.D));
                nx = near.ds.Average(x => x.Nx); nz = near.ds.Average(x => x.Nz); double l = Math.Sqrt(nx * nx + nz * nz);
                if (l < 1e-6) continue;
                nx /= l; nz /= l;
                s = d.Move + 1;
                what = string.Create(ci, $"{e.Id} {e.Map.Cs2Prefab} beside {near.c.E.Id} {d.Prefab} ({near.ds.Min(x => x.D):F1} m; run report: move {d.Move:F1} m)");
            }
            else
            {
                if (Inside(course, half, y, hw, ex, topologyFactor, 6) is not { } h) continue;
                s = h.Need + 1 - h.MinD; nx = h.Nx; nz = h.Nz;
                what = string.Create(ci, $"{e.Id} {e.Map.Cs2Prefab} beside {h.H.Id} {h.H.Map.Cs2Prefab} ({h.MinD:F1} m, needs {h.Need:F1})");
            }
            var off = new Vec3(nx * s, 0, nz * s);
            var curve = new Bezier(e.Curve.A + off, e.Curve.B + off, e.Curve.C + off, e.Curve.D + off);
            var again = Inside(Samples(curve), half, y, hw, ex, topologyFactor, drops != null ? double.PositiveInfinity : 6);
            if (again != null) { impossible.Add(string.Create(ci, $"{what}: shifted {s:F1} m it lands inside {again.Value.H.Id} {again.Value.H.Map.Cs2Prefab} (between carriageways), left as is")); continue; }
            foreach (var n in new[] { e.A, e.B })
            {
                if (!moved.Add(n)) continue;
                n.Position = n.Position + off;
                foreach (var o in res.Edges.Where(o => o != e && (o.A == n || o.B == n))) o.Curve = o.Curve.WithEnds(o.A.Position, o.B.Position);
            }
            e.Curve = new Bezier(e.A.Position, curve.B, curve.C, e.B.Position);
            shifted.Add(string.Create(ci, $"{what}: moved {s:F1} m"));
        }
        return new Result(shifted, impossible);
    }
}
