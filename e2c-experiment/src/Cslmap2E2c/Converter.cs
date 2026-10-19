namespace Cslmap2E2c;

public sealed class ConvertOptions
{
    /// <summary>CS1 metres to CS2 metres (14 336 / 17 280 for the full-extent squeeze).</summary>
    public double Scale { get; set; } = 0.8296;
    public double PlayableWidth { get; set; } = 14336;
    /// <summary>Douglas-Peucker tolerance in metres (0 disables).</summary>
    public double SimplifyTolerance { get; set; } = 0.5;
    /// <summary>Interior points closer than this to the previous kept point are dropped (E2C snaps endpoints within 6 m).</summary>
    public double MinSpacing { get; set; } = 8;
    /// <summary>Insert points so no straight piece is longer than this (0 disables).</summary>
    public double MaxSegment { get; set; } = 0;
    /// <summary>Lane count written for one-way motorways with fewer lanes (E2C cannot pick 1- or 2-lane highways from explicit lanes).</summary>
    public int MotorwayMinLanes { get; set; } = 3;
    /// <summary>Lane count written for motorway links (1 = Small Road Oneway - 1 lane; 3 = Highway Oneway - 3 lanes).</summary>
    public int LinkLanes { get; set; } = 1;
    /// <summary>Cut roads and rail at the playable square (±PlayableWidth/2) so ends sit on the border; drop what lies outside.</summary>
    public bool Clip { get; set; }
    public bool DropTunnels { get; set; }
    public bool ReverseOneWays { get; set; }
}

public sealed class RoadWay
{
    public required long Id { get; init; }
    public required NetClass Class { get; init; }
    public required bool OneWay { get; init; }
    /// <summary>Lane count as written to OSM (already adjusted for E2C's resolver).</summary>
    public required int Lanes { get; init; }
    public required int SourceLanes { get; init; }
    public required string Prefab { get; init; }
    public required Structure Structure { get; init; }
    public string? Name { get; init; }
    /// <summary>OSM node ids; endpoints are shared between ways (derived from CS1 node ids).</summary>
    public required List<long> NodeIds { get; init; }
}

public sealed class RailLine
{
    public required int SegmentId { get; init; }
    public required string Prefab { get; init; }
    public required string ItemClass { get; init; }
    public required Structure Structure { get; init; }
    public required int StartNode { get; init; }
    public required int EndNode { get; init; }
    /// <summary>CS2 metres (scaled x, z) and CS1 height y.</summary>
    public required List<(double X, double Y, double Z)> Points { get; init; }
}

public sealed class Network
{
    /// <summary>OSM node id to CS2 game metres (x, z) and CS1 height y.</summary>
    public Dictionary<long, (double X, double Y, double Z)> Nodes { get; } = new();
    public List<RoadWay> Ways { get; } = new();
    public List<RailLine> Rail { get; } = new();
    public Stats Stats { get; } = new();
    public string Source { get; set; } = "";
}

public sealed class Stats
{
    public int SegmentsTotal;
    public SortedDictionary<string, int> SegmentsByClass { get; } = new();
    public SortedDictionary<string, int> SkippedPrefabs { get; } = new(StringComparer.Ordinal);
    public SortedDictionary<string, int> UnmatchedPrefabs { get; } = new(StringComparer.Ordinal);
    public SortedDictionary<string, int> RoadPrefabs { get; } = new(StringComparer.Ordinal);
    public SortedDictionary<string, int> PredictedE2cPrefab { get; } = new(StringComparer.Ordinal);
    public SortedDictionary<string, int> StructureCounts { get; } = new();
    public int ReversedOneWays;
    public int DroppedTunnels;
    public int DegenerateSegments;
    public int ClippedAway, ClippedAtBorder, RailClippedAway;
    public int PointsIn, PointsOut;
    public int PointsOutsidePlayable;
    public int EdgesNearBoundary;
    public int EdgesShorterThanSnap;

    static void Inc(IDictionary<string, int> d, string k) => d[k] = d.TryGetValue(k, out var v) ? v + 1 : 1;
    internal void Class(string c) => Inc(SegmentsByClass, c);
    internal void Skipped(string p) => Inc(SkippedPrefabs, p);
    internal void Unmatched(string p) => Inc(UnmatchedPrefabs, p);
    internal void Road(string p) => Inc(RoadPrefabs, p);
    internal void Predicted(string p) => Inc(PredictedE2cPrefab, p);
    internal void StructureOf(Structure s) => Inc(StructureCounts, s.ToString());
}

public static class Converter
{
    const long InteriorIdBase = 1_000_000_000;

    public static Network Convert(CslMap map, Classifier classifier, ConvertOptions opt)
    {
        var net = new Network { Source = map.City };
        long nextInterior = InteriorIdBase;
        long nextWay = 1;
        var st = net.Stats;

        foreach (var seg in map.Segments)
        {
            st.SegmentsTotal++;
            var (baseName, structure) = Classifier.SplitPrefab(seg.Prefab, seg.ItemClass);
            if (map.Nodes.TryGetValue(seg.StartNode, out var sn) && sn.Underground || map.Nodes.TryGetValue(seg.EndNode, out var en) && en.Underground)
                structure = Structure.Tunnel;
            var (cls, rule) = classifier.Classify(seg.ItemClass, baseName);
            st.Class(cls.ToString());
            if (rule == null) st.Unmatched(seg.Prefab);
            if (cls == NetClass.Skip) { st.Skipped(seg.Prefab); continue; }

            var pts = CollectPoints(map, seg, opt.Scale);
            if (pts.Count < 2) { st.DegenerateSegments++; continue; }

            if (cls == NetClass.Rail)
            {
                var railRuns = opt.Clip ? Geometry.ClipToSquare(pts, opt.PlayableWidth / 2) : [(pts, true, true)];
                if (railRuns.Count == 0) st.RailClippedAway++;
                foreach (var (railPts, _, _) in railRuns)
                    net.Rail.Add(new RailLine
                    {
                        SegmentId = seg.Id, Prefab = seg.Prefab, ItemClass = seg.ItemClass, Structure = structure,
                        StartNode = seg.StartNode, EndNode = seg.EndNode, Points = railPts,
                    });
                continue;
            }

            if (structure == Structure.Tunnel && opt.DropTunnels) { st.DroppedTunnels++; continue; }
            st.StructureOf(structure);
            st.Road(seg.Prefab);

            map.SegmentTypes.TryGetValue(seg.Prefab, out var lanes);
            var (fwd, bwd) = Classifier.CountLanes(lanes, baseName);
            bool oneWay = (fwd == 0) != (bwd == 0);
            int sourceLanes = fwd + bwd;
            long startId = seg.StartNode + 1, endId = seg.EndNode + 1;
            bool reverse = oneWay && (fwd == 0) != opt.ReverseOneWays;
            if (reverse)
            {
                pts.Reverse();
                (startId, endId) = (endId, startId);
                st.ReversedOneWays++;
            }

            st.PointsIn += pts.Count;
            var runs = opt.Clip ? Geometry.ClipToSquare(pts, opt.PlayableWidth / 2) : [(pts, true, true)];
            if (opt.Clip)
            {
                if (runs.Count == 0) { st.ClippedAway++; continue; }
                if (runs.Count > 1 || !runs[0].StartKept || !runs[0].EndKept) st.ClippedAtBorder++;
            }
            int written = E2cLanes(cls, oneWay, sourceLanes, opt);

            foreach (var (runPts, startKept, endKept) in runs)
            {
                var simplified = Geometry.Simplify(runPts, opt.SimplifyTolerance, opt.MinSpacing, opt.MaxSegment);
                if (simplified.Count < 2 || Geometry.Dist(simplified[0], simplified[^1]) < 1e-6) { st.DegenerateSegments++; continue; }
                st.PointsOut += simplified.Count;

                var ids = new List<long>(simplified.Count);
                for (int i = 0; i < simplified.Count; i++)
                {
                    // Cut points on the border get fresh ids; CS1 node ids are kept only where the CS1 node itself survives.
                    long id = i == 0 && startKept ? startId : i == simplified.Count - 1 && endKept ? endId : nextInterior++;
                    // Shared CS1 nodes keep the first position seen so every way references identical coordinates.
                    if (!net.Nodes.ContainsKey(id)) net.Nodes[id] = simplified[i];
                    ids.Add(id);
                }

                net.Ways.Add(new RoadWay
                {
                    Id = nextWay++, Class = cls, OneWay = oneWay, Lanes = written, SourceLanes = sourceLanes,
                    Prefab = seg.Prefab, Structure = structure, Name = seg.Name, NodeIds = ids,
                });
                st.Predicted($"{cls.E2cCategory()} {(oneWay ? "oneway" : "twoway")} lanes={written} -> {PredictE2cPrefab(cls, oneWay, written)}");
            }
        }

        CountPlacementRisks(net, opt);
        return net;
    }

    static List<(double X, double Y, double Z)> CollectPoints(CslMap map, CslSegment seg, double scale)
    {
        var pts = seg.Points.Select(p => (p.X * scale, p.Y, p.Z * scale)).ToList();
        // Use node positions for the endpoints so segments meeting at a node share exact coordinates.
        if (map.Nodes.TryGetValue(seg.StartNode, out var a))
        {
            if (pts.Count == 0) pts.Add((a.X * scale, a.Y, a.Z * scale)); else pts[0] = (a.X * scale, a.Y, a.Z * scale);
        }
        if (map.Nodes.TryGetValue(seg.EndNode, out var b))
        {
            if (pts.Count < 2) pts.Add((b.X * scale, b.Y, b.Z * scale)); else pts[^1] = (b.X * scale, b.Y, b.Z * scale);
        }
        return pts;
    }

    /// <summary>
    /// Lane count to write so that E2C's resolver lands on a prefab with the right directionality.
    /// With explicit lanes &lt;= 2 E2C excludes every "Highway" prefab, and a one-way 2-lane request has no
    /// one-way candidate at all (it falls back to two-way Small Road). See E2C_CONTRACT.md §4.
    /// </summary>
    public static int E2cLanes(NetClass cls, bool oneWay, int lanes, ConvertOptions opt)
    {
        bool motorway = cls.E2cCategory() == "motorway";
        if (cls == NetClass.MotorwayLink && opt.LinkLanes > 0) return opt.LinkLanes;
        if (oneWay)
        {
            if (motorway) return Math.Clamp(Math.Max(lanes, opt.MotorwayMinLanes), 1, 4);
            return lanes >= 3 ? 3 : 1;
        }
        if (motorway) return Math.Clamp(Math.Max(lanes, 3), 3, 4);
        return Math.Clamp(lanes, 2, 6);
    }

    /// <summary>Offline prediction of E2C's curated prefab (inferred from its resolver, not verified in game).</summary>
    public static string PredictE2cPrefab(NetClass cls, bool oneWay, int lanes)
    {
        bool motorway = cls.E2cCategory() == "motorway";
        if (oneWay)
            return lanes switch
            {
                1 => "Small Road Oneway - 1 lane",
                2 => "Small Road (two-way fallback!)",
                3 => motorway ? "Highway Oneway - 3 lanes" : "Small Road Oneway - 3 lanes",
                _ => "Highway Oneway - 4 lanes",
            };
        return lanes switch
        {
            <= 2 => "Small Road",
            3 => motorway ? "Highway Twoway - 3 lanes" : "Small Road Asymmetric",
            4 => motorway ? "Highway Twoway - 4 lanes" : "Medium Road",
            5 => "Medium Road Asymmetric - 2+3 lanes",
            _ => "Large Road",
        };
    }

    static void CountPlacementRisks(Network net, ConvertOptions opt)
    {
        double half = opt.PlayableWidth / 2;
        double boundary = half - 600; // E2C swaps any edge with an end beyond this for a Highway prefab.
        foreach (var p in net.Nodes.Values)
            if (Math.Abs(p.X) > half || Math.Abs(p.Z) > half) net.Stats.PointsOutsidePlayable++;
        foreach (var w in net.Ways)
            for (int i = 0; i + 1 < w.NodeIds.Count; i++)
            {
                var a = net.Nodes[w.NodeIds[i]];
                var b = net.Nodes[w.NodeIds[i + 1]];
                if (Math.Abs(a.X) > boundary || Math.Abs(a.Z) > boundary || Math.Abs(b.X) > boundary || Math.Abs(b.Z) > boundary)
                    net.Stats.EdgesNearBoundary++;
                if (Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z)) < 6)
                    net.Stats.EdgesShorterThanSnap++;
            }
    }
}
