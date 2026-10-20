namespace HmRebuild;

/// <summary>Fixture for `roads`: a road at plane height through a 30 m rock block and over a 12 m dip, crossing a river strip
/// whose bed (85 m) is below the 93 m water level.</summary>
public static class RoadsTest
{
    const int N = Peaks.N;

    public static void Run(Action<string, bool, string> check)
    {
        static double Plane(double x, double z) => 150 + 0.002 * x;
        var terrain = new ushort[N * N];
        for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
            {
                double x = -7168 + 3.5 * (c + 0.5), z = 7168 - 3.5 * (r + 0.5), h = Plane(x, z);
                if (Math.Abs(x - 1000) < 80 && Math.Abs(z) < 120) h += 30;          // baked rock over the road
                if (Math.Abs(x - 1400) < 60 && Math.Abs(z) < 120) h -= 12;          // dip under the road
                if (Math.Abs(x - 1700) < 40) h = 85;                                 // river bed below the 93 m water level
                terrain[r * N + c] = Peaks.FromM(h);
            }
        var xs = Enumerable.Range(0, 41).Select(k => 800.0 + 25 * k).ToArray();   // x 800..1800 along z = 0
        var course = new Roads.Course("t1", "Medium Road", 8, xs, xs.Select(_ => 0.0).ToArray(), xs.Select(x => Plane(x, 0)).ToArray(), 30);
        var o = new Roads.Options();
        var res = Roads.Run(terrain, [course], o);
        long outside = 0;
        for (int i = 0; i < terrain.Length; i++) if (!res.Zone[i] && res.Output[i] != terrain[i]) outside++;
        var cr = res.Courses[0];
        check("roads: bit-identical outside corridors", outside == 0, $"{outside} differ; {res.ChangedPx} changed");
        // Residual on the dry points only (the river crossing stays a bridge gap).
        double maxDry = 0;
        foreach (var x in xs.Where(x => Math.Abs(x - 1700) > 60)) maxDry = Math.Max(maxDry, Math.Abs(Roads.Sample(res.Output, x, 0) - Plane(x, 0)));
        check("roads: corridor at road height through rock and dip", maxDry < 0.1 && cr.MaxBeforeM > 25, $"dry residual max {maxDry:F3} m (before {cr.MaxBeforeM:F1} m)");
        double shoulder = Roads.Sample(res.Output, 1000, 8 + 4) - Plane(1000, 0), outer = Roads.Sample(res.Output, 1000, 8 + 6 + 25) - Plane(1000, 0);
        check("roads: shoulders flat, grading back to rock beyond", Math.Abs(shoulder) < 0.1 && outer > 29, $"+{shoulder:F2} m at the shoulder, +{outer:F1} m 25 m beyond the core");
        int riverChanged = 0;
        for (int r = 0; r < N; r++) for (int c = 0; c < N; c++) { double x = -7168 + 3.5 * (c + 0.5); if (Math.Abs(x - 1700) < 39 && res.Output[r * N + c] != terrain[r * N + c]) riverChanged++; }
        check("roads: river bed below 93 m untouched", riverChanged == 0 && res.RiverHeldPx > 0, $"{riverChanged} river px changed; {res.RiverHeldPx} held");
    }
}
