namespace HmRebuild;

/// <summary>Fixture for `regrade`: a straight road along +x at z = 0 on a gentle slope, hill to +z with a cut face beyond
/// the bench (terrain rises 1 m/m), valley to −z. New profile = old + up to +3 m (ramp in and out).</summary>
public static class RegradeTest
{
    const int N = Peaks.N;

    public static void Run(Action<string, bool, string> check)
    {
        static double Old(double x) => 200 + 0.01 * x;
        static double Delta(double s) => s < 50 ? 0 : s < 150 ? 3 * (s - 50) / 100 : s < 350 ? 3 : s < 450 ? 3 * (450 - s) / 100 : 0;
        const double hw = 14.4, bench = 13;
        static double Ground(double x, double z) => Old(x) + (z > 14.4 + 13 ? (z - 27.4) * 1.0 : 0);   // flat road + bench, face above
        var terrain = new ushort[N * N];
        for (int r = 0; r < N; r++) for (int c = 0; c < N; c++) terrain[r * N + c] = Peaks.FromM(Ground(-7168 + 3.5 * (c + 0.5), 7168 - 3.5 * (r + 0.5)));
        var pts = Enumerable.Range(0, 63).Select(k => { double s = -8 + 8 * k, x = 1000 + s; return new Regrade.Pt(s, x, 0, Old(x), Old(x) + Delta(s), bench, 0, 1); }).ToList();
        var sp = new Regrade.Spec("fixture", hw, pts);
        var res = Regrade.Run(terrain, sp, new Regrade.Options());
        long outside = 0; for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        check("regrade: bit-identical outside the zone", outside == 0, $"{outside} differ; {res.ChangedPx} changed");
        double At(double x, double z) => Roads.Sample(res.Output, x, z);
        double xm = 1000 + 250;   // mid-section, delta = 3
        check("regrade: centreline and bench at new y", Math.Abs(At(xm, 0) - (Old(xm) + 3)) < 0.05 && Math.Abs(At(xm, hw + bench - 4) - (Old(xm) + 3)) < 0.05 && Math.Abs(At(xm, -hw + 4) - (Old(xm) + 3)) < 0.05,
            $"centre +{At(xm, 0) - Old(xm):F2}, bench +{At(xm, hw + bench - 4) - Old(xm):F2}, valley edge +{At(xm, -hw + 4) - Old(xm):F2} m");
        double face = At(xm, hw + bench + 10) - Ground(xm, hw + bench + 10), far = At(xm, hw + bench + 45) - Ground(xm, hw + bench + 45);
        check("regrade: cut face shifted by delta, then back to existing", Math.Abs(face - 3) < 0.05 && Math.Abs(far) < 0.02, $"face +{face:F2} m, 45 m up +{far:F3} m");
        double apronMid = At(xm, -(hw + 5)) - Ground(xm, -(hw + 5)), apronOut = At(xm, -(hw + 14)) - Ground(xm, -(hw + 14));
        check("regrade: valley apron blends over 10 m", apronMid > 0.5 && apronMid < 2.5 && Math.Abs(apronOut) < 0.02, $"+{apronMid:F2} m at 5 m, +{apronOut:F3} m at 14 m");
        check("regrade: max fill = 3 m, no cut", Math.Abs(res.MaxFillM - 3) < 0.05 && res.MaxCutM < 0.05, $"fill {res.MaxFillM:F2}, cut {res.MaxCutM:F2}");
    }
}
