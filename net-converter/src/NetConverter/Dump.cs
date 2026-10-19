using System.Text.Json;

namespace NetConverter;

public sealed record DumpNode(int Id, string Prefab, Vec3 Position, int Elevation, double Terrain, IReadOnlyList<string> Flags)
{
    public bool Has(string flag) => Flags.Contains(flag);
}

public sealed record DumpSegment(int Id, string Prefab, int StartNode, int EndNode, bool Invert, Bezier Bezier, double Length)
{
    public Vec3 StartDirection { get; init; }
    public Vec3 EndDirection { get; init; }
}

public sealed record NetPrefabInfo(string Name, string Service, string SubService, string Ai, double HalfWidth, int ForwardLanes, int BackwardLanes)
{
    public bool OneWay => (ForwardLanes > 0) != (BackwardLanes > 0);
}

public sealed record WaterSource(int Id, string Type, Vec3 Input, Vec3 Output, double TargetHeight, long InputRate, long OutputRate, long Flow);

public sealed record OutsideBuilding(int Id, string Prefab, string Service, string SubService, Vec3 Position, double Angle, bool Incoming, bool Outgoing)
{
    public string Kind => SubService switch
    {
        "PublicTransportTrain" => "train",
        "PublicTransportShip" => "ship",
        "PublicTransportPlane" => "plane",
        _ => Service == "Road" ? "road" : $"{Service}/{SubService}",
    };
}

public sealed class Dump
{
    public Dictionary<int, DumpNode> Nodes { get; } = new();
    public List<DumpSegment> Segments { get; } = new();
    public Dictionary<string, NetPrefabInfo> NetPrefabs { get; } = new();
    public List<WaterSource> WaterSources { get; } = new();
    public List<OutsideBuilding> OutsideConnections { get; } = new();
    public double SeaLevel { get; set; }

    public static Dump Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream, new JsonDocumentOptions { MaxDepth = 256 });
        return From(doc.RootElement);
    }

    public static Dump Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return From(doc.RootElement);
    }

    static Dump From(JsonElement root)
    {
        var d = new Dump();
        if (root.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("coordinates", out var coords)
            && coords.TryGetProperty("seaLevel", out var sea) && sea.ValueKind == JsonValueKind.Number)
            d.SeaLevel = sea.GetDouble();

        foreach (var n in root.GetProperty("nodes").EnumerateArray())
        {
            var flags = n.TryGetProperty("flags", out var f) ? f.EnumerateArray().Select(x => x.GetString()!).ToList() : new List<string>();
            var pos = V(n.GetProperty("position"));
            d.Nodes[n.GetProperty("id").GetInt32()] = new DumpNode(
                n.GetProperty("id").GetInt32(), Str(n, "prefab"), pos,
                n.TryGetProperty("elevation", out var e) ? e.GetInt32() : 0,
                Num(n, "terrain") ?? pos.Y, flags);
        }

        foreach (var s in root.GetProperty("segments").EnumerateArray())
        {
            var bz = s.GetProperty("bezier").EnumerateArray().Select(V).ToArray();
            d.Segments.Add(new DumpSegment(
                s.GetProperty("id").GetInt32(), Str(s, "prefab"),
                s.GetProperty("startNode").GetInt32(), s.GetProperty("endNode").GetInt32(),
                s.TryGetProperty("invert", out var inv) && inv.GetBoolean(),
                new Bezier(bz[0], bz[1], bz[2], bz[3]),
                Num(s, "length") ?? 0)
            {
                StartDirection = s.TryGetProperty("startDirection", out var sd) ? V(sd) : default,
                EndDirection = s.TryGetProperty("endDirection", out var ed) ? V(ed) : default,
            });
        }

        if (root.TryGetProperty("netPrefabs", out var nps))
            foreach (var p in nps.EnumerateObject())
            {
                var v = p.Value;
                d.NetPrefabs[p.Name] = new NetPrefabInfo(p.Name, Str(v, "service"), Str(v, "subService"), Str(v, "ai"),
                    Num(v, "halfWidth") ?? 4, (int)(Num(v, "forwardVehicleLanes") ?? 0), (int)(Num(v, "backwardVehicleLanes") ?? 0));
            }

        if (root.TryGetProperty("waterSources", out var ws))
            foreach (var w in ws.EnumerateArray())
                d.WaterSources.Add(new WaterSource(w.GetProperty("id").GetInt32(), Str(w, "type"),
                    V(w.GetProperty("inputPosition")), V(w.GetProperty("outputPosition")),
                    Num(w, "targetHeight") ?? 0, (long)(Num(w, "inputRate") ?? 0), (long)(Num(w, "outputRate") ?? 0), (long)(Num(w, "flow") ?? 0)));

        if (root.TryGetProperty("buildings", out var bs))
            foreach (var b in bs.EnumerateArray())
            {
                if (!(b.TryGetProperty("outsideConnection", out var oc) && oc.GetBoolean())) continue;
                d.OutsideConnections.Add(new OutsideBuilding(b.GetProperty("id").GetInt32(), Str(b, "prefab"), Str(b, "service"), Str(b, "subService"),
                    V(b.GetProperty("position")), Num(b, "angle") ?? 0,
                    b.TryGetProperty("incoming", out var i) && i.GetBoolean(), b.TryGetProperty("outgoing", out var o) && o.GetBoolean()));
            }
        return d;
    }

    static Vec3 V(JsonElement e)
    {
        var a = e.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Number ? x.GetDouble() : 0).ToArray();
        return new Vec3(a[0], a[1], a[2]);
    }

    static string Str(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : "";
    static double? Num(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;

    /// <summary>
    /// Multiplies every height by f: node y and terrain, Bézier y, water heights. The CS2 map is built with the 2048 m
    /// heightmap at Height Scale 4096, so CS2 heights are 2 × CS1 (f = 2); f = 1 is a 1:1 map.
    /// </summary>
    public void ScaleHeights(double f)
    {
        if (f == 1) return;
        static Vec3 S(Vec3 v, double f) => v with { Y = v.Y * f };
        foreach (var id in Nodes.Keys.ToList())
        {
            var n = Nodes[id];
            Nodes[id] = n with { Position = S(n.Position, f), Terrain = n.Terrain * f };
        }
        for (int i = 0; i < Segments.Count; i++)
        {
            var s = Segments[i]; var b = s.Bezier;
            Segments[i] = s with { Bezier = new Bezier(S(b.A, f), S(b.B, f), S(b.C, f), S(b.D, f)) };
        }
        for (int i = 0; i < WaterSources.Count; i++)
        {
            var w = WaterSources[i];
            WaterSources[i] = w with { Input = S(w.Input, f), Output = S(w.Output, f), TargetHeight = w.TargetHeight * f };
        }
        SeaLevel *= f;
        HeightFactor = f;
    }

    public double HeightFactor { get; private set; } = 1;

    public NetPrefabInfo PrefabInfo(string name) =>
        NetPrefabs.TryGetValue(name, out var p) ? p : new NetPrefabInfo(name, "", "", "", 4, 0, 0);
}
