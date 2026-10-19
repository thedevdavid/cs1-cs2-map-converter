namespace Cslmap2E2c;

public static class Geometry
{
    /// <summary>Douglas-Peucker in the x,z plane, then drop interior points closer than minSpacing, then densify to maxSegment.</summary>
    public static List<(double X, double Y, double Z)> Simplify(IReadOnlyList<(double X, double Y, double Z)> pts, double tolerance, double minSpacing, double maxSegment)
    {
        var list = pts.ToList();
        if (list.Count > 2 && tolerance > 0)
        {
            var keep = new bool[list.Count];
            keep[0] = keep[^1] = true;
            DouglasPeucker(list, 0, list.Count - 1, tolerance, keep);
            list = list.Where((_, i) => keep[i]).ToList();
        }

        if (list.Count > 2 && minSpacing > 0)
        {
            var spaced = new List<(double, double, double)> { list[0] };
            for (int i = 1; i < list.Count - 1; i++)
                if (Dist(spaced[^1], list[i]) >= minSpacing && Dist(list[i], list[^1]) >= minSpacing)
                    spaced.Add(list[i]);
            spaced.Add(list[^1]);
            list = spaced;
        }

        if (maxSegment > 0)
        {
            var dense = new List<(double X, double Y, double Z)> { list[0] };
            for (int i = 1; i < list.Count; i++)
            {
                var a = list[i - 1];
                var b = list[i];
                int n = (int)Math.Ceiling(Dist(a, b) / maxSegment);
                for (int k = 1; k < n; k++)
                {
                    double t = (double)k / n;
                    dense.Add((a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t));
                }
                dense.Add(b);
            }
            list = dense;
        }
        return list;
    }

    /// <summary>
    /// Clips a polyline to the square |x|, |z| &lt;= half (Liang-Barsky per piece, y interpolated). Returns the inside
    /// runs in order; StartKept/EndKept say whether a run starts/ends at the polyline's original endpoint rather
    /// than at a cut on the border.
    /// </summary>
    public static List<(List<(double X, double Y, double Z)> Points, bool StartKept, bool EndKept)> ClipToSquare(IReadOnlyList<(double X, double Y, double Z)> pts, double half)
    {
        var runs = new List<(List<(double X, double Y, double Z)>, bool, bool)>();
        List<(double X, double Y, double Z)>? run = null;
        bool runStartKept = false;
        for (int i = 0; i + 1 < pts.Count; i++)
        {
            var a = pts[i];
            var b = pts[i + 1];
            if (!ClipPiece(a, b, half, out double t0, out double t1))
            {
                if (run != null) { runs.Add((run, runStartKept, false)); run = null; }
                continue;
            }
            if (run == null)
            {
                run = [Lerp(a, b, t0)];
                runStartKept = i == 0 && t0 == 0;
            }
            run.Add(Lerp(a, b, t1));
            if (t1 < 1) { runs.Add((run, runStartKept, false)); run = null; }
        }
        if (run != null) runs.Add((run, runStartKept, true));
        return runs;
    }

    static (double X, double Y, double Z) Lerp((double X, double Y, double Z) a, (double X, double Y, double Z) b, double t) =>
        t == 0 ? a : t == 1 ? b : (a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);

    static bool ClipPiece((double X, double Y, double Z) a, (double X, double Y, double Z) b, double half, out double t0, out double t1)
    {
        t0 = 0; t1 = 1;
        double dx = b.X - a.X, dz = b.Z - a.Z;
        // Each pair (p, q): the piece is inside where p*t <= q.
        ReadOnlySpan<(double P, double Q)> edges = [(-dx, a.X + half), (dx, half - a.X), (-dz, a.Z + half), (dz, half - a.Z)];
        foreach (var (p, q) in edges)
        {
            if (p == 0) { if (q < 0) return false; continue; }
            double r = q / p;
            if (p < 0) { if (r > t1) return false; if (r > t0) t0 = r; }
            else { if (r < t0) return false; if (r < t1) t1 = r; }
        }
        return t1 > t0 || (t1 == t0 && dx == 0 && dz == 0);
    }

    public static double Dist((double X, double Y, double Z) a, (double X, double Y, double Z) b) =>
        Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));

    static void DouglasPeucker(List<(double X, double Y, double Z)> p, int first, int last, double tol, bool[] keep)
    {
        if (last <= first + 1) return;
        double maxD = -1;
        int idx = -1;
        for (int i = first + 1; i < last; i++)
        {
            double d = PointLineDistance(p[i], p[first], p[last]);
            if (d > maxD) { maxD = d; idx = i; }
        }
        if (maxD <= tol) return;
        keep[idx] = true;
        DouglasPeucker(p, first, idx, tol, keep);
        DouglasPeucker(p, idx, last, tol, keep);
    }

    static double PointLineDistance((double X, double Y, double Z) p, (double X, double Y, double Z) a, (double X, double Y, double Z) b)
    {
        double dx = b.X - a.X, dz = b.Z - a.Z;
        double len2 = dx * dx + dz * dz;
        if (len2 < 1e-12) return Dist(p, a);
        double t = Math.Clamp(((p.X - a.X) * dx + (p.Z - a.Z) * dz) / len2, 0, 1);
        double cx = a.X + t * dx, cz = a.Z + t * dz;
        return Math.Sqrt((p.X - cx) * (p.X - cx) + (p.Z - cz) * (p.Z - cz));
    }
}
