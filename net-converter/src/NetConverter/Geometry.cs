namespace NetConverter;

public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 a, double s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vec3 Lerp(Vec3 a, Vec3 b, double t) => a + (b - a) * t;
    public double DistXZ(Vec3 o) => Math.Sqrt((X - o.X) * (X - o.X) + (Z - o.Z) * (Z - o.Z));
}

/// <summary>Cubic Bézier [A, B, C, D].</summary>
public readonly record struct Bezier(Vec3 A, Vec3 B, Vec3 C, Vec3 D)
{
    public Vec3 Eval(double t)
    {
        var ab = Vec3.Lerp(A, B, t); var bc = Vec3.Lerp(B, C, t); var cd = Vec3.Lerp(C, D, t);
        var abc = Vec3.Lerp(ab, bc, t); var bcd = Vec3.Lerp(bc, cd, t);
        return Vec3.Lerp(abc, bcd, t);
    }

    /// <summary>de Casteljau split at t.</summary>
    public (Bezier Left, Bezier Right) Split(double t)
    {
        var ab = Vec3.Lerp(A, B, t); var bc = Vec3.Lerp(B, C, t); var cd = Vec3.Lerp(C, D, t);
        var abc = Vec3.Lerp(ab, bc, t); var bcd = Vec3.Lerp(bc, cd, t);
        var p = Vec3.Lerp(abc, bcd, t);
        return (new Bezier(A, ab, abc, p), new Bezier(p, bcd, cd, D));
    }

    /// <summary>The part of the curve between t0 and t1 (0 ≤ t0 &lt; t1 ≤ 1).</summary>
    public Bezier Sub(double t0, double t1)
    {
        var right = t0 > 0 ? Split(t0).Right : this;
        if (t1 >= 1) return right;
        var u = (t1 - t0) / (1 - t0);
        return right.Split(u).Left;
    }

    public Bezier Reverse() => new(D, C, B, A);

    /// <summary>Horizontal (x/z) arc length, sampled.</summary>
    public double LengthXZ(int samples = 32)
    {
        double len = 0; var prev = A;
        for (int i = 1; i <= samples; i++) { var p = Eval(i / (double)samples); len += prev.DistXZ(p); prev = p; }
        return len;
    }

    public Bezier WithEnds(Vec3 a, Vec3 d) => new(a, B + (a - A), C + (d - D), d);
}

public static class Clipper
{
    /// <summary>Signed distance-like border function: ≤ 0 inside the square [-half, half]².</summary>
    public static double Outside(Vec3 p, double half) => Math.Max(Math.Abs(p.X), Math.Abs(p.Z)) - half;

    /// <summary>Moves a point that lies (numerically) on the border exactly onto it.</summary>
    public static Vec3 SnapToBorder(Vec3 p, double half)
    {
        if (Math.Abs(p.X) >= Math.Abs(p.Z)) return p with { X = Math.CopySign(half, p.X) };
        return p with { Z = Math.CopySign(half, p.Z) };
    }

    /// <summary>
    /// Parameter intervals of the curve that lie inside the square. A curve that is wholly inside returns [(0,1)].
    /// Border crossings are found by sampling and refined by bisection.
    /// </summary>
    public static List<(double T0, double T1)> InsideIntervals(Bezier b, double half, int samples = 64)
    {
        var result = new List<(double, double)>();
        double? start = Outside(b.A, half) <= 0 ? 0 : null;
        double prevT = 0; bool prevIn = start.HasValue;
        for (int i = 1; i <= samples; i++)
        {
            double t = i / (double)samples;
            bool isIn = Outside(b.Eval(t), half) <= 0;
            if (isIn != prevIn)
            {
                double lo = prevT, hi = t;
                for (int k = 0; k < 50; k++)
                {
                    double mid = (lo + hi) / 2;
                    bool midIn = Outside(b.Eval(mid), half) <= 0;
                    if (midIn == prevIn) lo = mid; else hi = mid;
                }
                double cut = (lo + hi) / 2;
                if (isIn) start = cut; else { result.Add((start!.Value, cut)); start = null; }
            }
            prevT = t; prevIn = isIn;
        }
        if (start.HasValue) result.Add((start.Value, 1));
        return result;
    }
}
