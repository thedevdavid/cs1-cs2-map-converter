namespace Cslmap2E2c;

/// <summary>
/// Synthetic network for measuring E2C's placement in the editor, in CS2 game metres (no CS1 scale applied):
///  - X arm: residential, two-way, (-2500, 0) to (2500, 0)  -> expect Small Road
///  - Z arm: secondary 4 lanes, two-way, (0, -2500) to (0, 2500) -> expect Medium Road
///  - Diagonal: motorway one-way 3 lanes from the origin to (3535.5, 3535.5), 5 km at 45 deg into +x +z
///    -> expect Highway Oneway - 3 lanes, traffic flowing away from the origin.
/// Different classes per axis expose an x/z swap; the one-quadrant diagonal exposes a mirror; the one-way
/// direction exposes a reversed point order. All three share the node at the origin.
/// </summary>
public static class Calibration
{
    public const double ArmHalfLength = 2500;
    public const double DiagonalLength = 5000;

    public static Network Build(double spacing)
    {
        var net = new Network { Source = "calibration" };
        long nextNode = 2;
        net.Nodes[1] = (0, 0, 0);

        void AddWay(long wayId, NetClass cls, bool oneWay, int lanes, string label, (double X, double Z) from, (double X, double Z) to, bool throughOrigin)
        {
            var ids = new List<long>();
            var pts = Geometry.Simplify([(from.X, 0, from.Z), (to.X, 0, to.Z)], 0, 0, spacing);
            foreach (var p in pts)
            {
                if (Math.Abs(p.X) < 1e-9 && Math.Abs(p.Z) < 1e-9) { ids.Add(1); continue; }
                var id = nextNode++;
                net.Nodes[id] = p;
                ids.Add(id);
            }
            if (throughOrigin && !ids.Contains(1))
                throw new InvalidOperationException($"{label}: spacing must divide {ArmHalfLength} so the arm has a vertex at the origin.");
            net.Ways.Add(new RoadWay
            {
                Id = wayId, Class = cls, OneWay = oneWay, Lanes = lanes, SourceLanes = lanes,
                Prefab = label, Structure = Structure.Ground, Name = label, NodeIds = ids,
            });
            net.Stats.Predicted($"{label}: {cls.E2cCategory()} lanes={lanes} -> {Converter.PredictE2cPrefab(cls, oneWay, lanes)}");
        }

        double d = DiagonalLength / Math.Sqrt(2);
        AddWay(1, NetClass.Residential, false, 2, "calib X arm", (-ArmHalfLength, 0), (ArmHalfLength, 0), true);
        AddWay(2, NetClass.Secondary, false, 4, "calib Z arm", (0, -ArmHalfLength), (0, ArmHalfLength), true);
        AddWay(3, NetClass.Motorway, true, 3, "calib diagonal", (0, 0), (d, d), true);
        net.Stats.SegmentsTotal = 3;
        return net;
    }
}
