namespace NetConverter;

/// <summary>
/// Organic outlines for raster-derived surfaces (owner review of A-v5: "surfaces must not be rectangles"). Each ring (already traced from a mask closed and
/// opened by two cells and simplified to half a cell, so the 16 m staircase becomes diagonals) gets three Chaikin corner-cutting passes, is resampled
/// along its length, and is displaced along its outward normal by a smooth edge noise of 6–10 m amplitude (two sines of random phase and wavelength, capped
/// at 15 % of the patch size, reduced or skipped if the ring would self-intersect). No straight raster edge or 90° corner remains. Patches under 1 500 m²
/// are dropped. Deterministic for a seed.
/// </summary>
public static class SmoothRings
{
    public const double MinArea = 1500;

    static List<(double X, double Z)> Chaikin(List<(double X, double Z)> r)
    {
        var o = new List<(double, double)>(r.Count * 2);
        for (int i = 0; i < r.Count; i++)
        {
            var a = r[i]; var b = r[(i + 1) % r.Count];
            o.Add((0.75 * a.X + 0.25 * b.X, 0.75 * a.Z + 0.25 * b.Z));
            o.Add((0.25 * a.X + 0.75 * b.X, 0.25 * a.Z + 0.75 * b.Z));
        }
        return o;
    }

    static List<(double X, double Z)> Resample(List<(double X, double Z)> r, double step)
    {
        double per = 0; for (int i = 0; i < r.Count; i++) { var a = r[i]; var b = r[(i + 1) % r.Count]; per += Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z)); }
        int n = Math.Max(8, (int)Math.Round(per / step)); double d = per / n;
        var o = new List<(double, double)>(n); double acc = 0, next = 0; int seg = 0;
        var p0 = r[0];
        while (o.Count < n && seg < r.Count * 2)
        {
            var a = r[seg % r.Count]; var b = r[(seg + 1) % r.Count];
            double l = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
            while (next <= acc + l && o.Count < n) { double t = l > 0 ? (next - acc) / l : 0; o.Add((a.X + t * (b.X - a.X), a.Z + t * (b.Z - a.Z))); next += d; }
            acc += l; seg++;
        }
        return o;
    }

    public static List<List<(double X, double Z)>> Smooth(IEnumerable<List<(double X, double Z)>> rings, ulong seed, double half = 7168)
    {
        var res = new List<List<(double X, double Z)>>();
        int k = 0;
        foreach (var r0 in rings)
        {
            k++;
            if (r0.Count < 3 || Math.Abs(Surfaces.Area(r0)) < MinArea) continue;
            var r = r0;
            for (int p = 0; p < 3; p++) r = Chaikin(r);
            double area = Math.Abs(Surfaces.Area(r));
            double per = 0; for (int i = 0; i < r.Count; i++) { var a = r[i]; var b = r[(i + 1) % r.Count]; per += Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z)); }
            var smooth = Resample(r, Math.Max(8, per / 1900));
            var rng = new Rng(Rng.Mix(seed, (ulong)k));
            double amp = Math.Min(6 + 4 * rng.NextDouble(), 0.15 * Math.Sqrt(area));
            double l1 = 60 + 60 * rng.NextDouble(), l2 = 25 + 15 * rng.NextDouble(), f1 = rng.NextDouble() * 2 * Math.PI, f2 = rng.NextDouble() * 2 * Math.PI;
            double sign = Surfaces.Area(smooth) > 0 ? 1 : -1;   // outward normal side
            List<(double X, double Z)>? best = null;
            for (int attempt = 0; attempt < 3 && best == null; attempt++, amp /= 2)
            {
                var o = new List<(double X, double Z)>(smooth.Count); double s = 0;
                for (int i = 0; i < smooth.Count; i++)
                {
                    var prev = smooth[(i - 1 + smooth.Count) % smooth.Count]; var cur = smooth[i]; var next = smooth[(i + 1) % smooth.Count];
                    double tx = next.X - prev.X, tz = next.Z - prev.Z, tl = Math.Sqrt(tx * tx + tz * tz); if (tl < 1e-9) { o.Add(cur); continue; }
                    double nx = tz / tl * sign, nz = -tx / tl * sign;
                    if (i > 0) s += Math.Sqrt((cur.X - smooth[i - 1].X) * (cur.X - smooth[i - 1].X) + (cur.Z - smooth[i - 1].Z) * (cur.Z - smooth[i - 1].Z));
                    double off = amp * (0.6 * Math.Sin(2 * Math.PI * s / l1 + f1) + 0.4 * Math.Sin(2 * Math.PI * s / l2 + f2));
                    o.Add((cur.X + nx * off, cur.Z + nz * off));
                }
                if (!Surfaces.SelfIntersects(o)) best = o;
            }
            best ??= smooth;
            var clipped = Surfaces.ClipSquare(best, half);
            if (clipped.Count >= 3 && Math.Abs(Surfaces.Area(clipped)) >= MinArea)
                res.Add(clipped.Select(p => (Math.Round(p.X, 2), Math.Round(p.Z, 2))).ToList());
        }
        return res;
    }

    /// <summary>Least-squares (Kåsa) circle through points: centre, radius and the RMS of the radial residuals.</summary>
    public static (double Cx, double Cz, double R, double Rms) FitCircle(IReadOnlyList<(double X, double Z)> pts)
    {
        double mx = pts.Average(p => p.X), mz = pts.Average(p => p.Z);
        double suu = 0, svv = 0, suv = 0, suuu = 0, svvv = 0, suvv = 0, svuu = 0;
        foreach (var (x, z) in pts)
        {
            double u = x - mx, v = z - mz;
            suu += u * u; svv += v * v; suv += u * v; suuu += u * u * u; svvv += v * v * v; suvv += u * v * v; svuu += v * u * u;
        }
        double det = suu * svv - suv * suv;
        if (Math.Abs(det) < 1e-9) return (mx, mz, 0, double.MaxValue);
        double b1 = 0.5 * (suuu + suvv), b2 = 0.5 * (svvv + svuu);
        double uc = (b1 * svv - b2 * suv) / det, vc = (suu * b2 - suv * b1) / det;
        double cx = mx + uc, cz = mz + vc;
        double r = Math.Sqrt(uc * uc + vc * vc + (suu + svv) / pts.Count);
        double rms = Math.Sqrt(pts.Average(p => Math.Pow(Math.Sqrt((p.X - cx) * (p.X - cx) + (p.Z - cz) * (p.Z - cz)) - r, 2)));
        return (cx, cz, r, rms);
    }
}
