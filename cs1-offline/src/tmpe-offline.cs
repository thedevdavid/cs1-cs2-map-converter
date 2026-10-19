// TM:PE data of a CS1 map, decoded offline and resolved to segments, lane indices and world positions.
// Usage: dotnet run src/tmpe-offline.cs -- <map.crp> <payloadDir> <dump.json> <outDir>
// Writes tmpe-offline.json and tmpe-summary.md.
//
// TM:PE stores CS1 lane ids (NetManager.m_lanes buffer indices). The dump has no lane ids, so this decodes the
// lane table from the map's NetManager data (ColossalFramework DataSerializer + EncodedArray, layout taken from
// the decompiled NetManager.Data.Serialize of CS1 1.21): lane flags + m_nextLane, node records, segment records
// with m_lanes (first lane). Lane i of a segment = i-th lane in its m_nextLane chain = NetInfo.m_lanes[i].
#:package System.Formats.Nrbf@9.0.0
#pragma warning disable SYSLIB5005
using System.Formats.Nrbf;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var inv = CultureInfo.InvariantCulture;
string crpPath = args[0], payloadDir = args[1], dumpPath = args[2], outDir = args[3];
Directory.CreateDirectory(outDir);

// ---------- data asset ----------
var data = ReadDataAsset(crpPath);
int start = FindDeltaString(data, "NetManager", 1_000_000);
if (start < 0) throw new Exception("NetManager section not found");
var r = new Reader(data, start);

// lanes
const int LaneCount = 262144, NodeCount = 32768, SegmentCount = 36864;
var laneFlags = new ushort[LaneCount];
{ var ea = new EA(r, 2, 1, true); for (int i = 1; i < LaneCount; i++) laneFlags[i] = (ushort)ea.Read(); }
var nextLane = new uint[LaneCount];
for (int i = 1; i < LaneCount; i++) if (laneFlags[i] != 0) nextLane[i] = r.U24();
int createdLanes = laneFlags.Count(f => f != 0);
for (int k = 0; k < 3; k++) { var ea = new EA(r, 1, 0, false); for (int i = 1; i < LaneCount; i++) if (laneFlags[i] != 0) ea.Read(); }   // reservedPrev/Next/Frame
{ var ea = new EA(r, 2, 1, true); for (int i = 1; i < LaneCount; i++) if (laneFlags[i] != 0) ea.Read(); }                          // lastReserveID
// nodes
var nodeFlags = new uint[NodeCount];
{ var ea = new EA(r, 4, 2, true); for (int i = 1; i < NodeCount; i++) { nodeFlags[i] = (uint)ea.Read(); ea.Read(); } }
{ var ea = new EA(r, 8, 4, true); for (int i = 1; i < NodeCount; i++) if (nodeFlags[i] != 0) { ea.Read(); ea.Read(); } }
{ var ea = new EA(r, 1, 0, false); for (int i = 1; i < NodeCount; i++) if (nodeFlags[i] != 0) ea.Read(); }
{ var ea = new EA(r, 2, 1, true); for (int i = 1; i < NodeCount; i++) if (nodeFlags[i] != 0) { ea.Read(); ea.Read(); } }
// node records; m_eventRoute (UInt16) only exists from data version 121007, so try both layouts
Dictionary<int, (float x, float y, float z)>? ParseNodes(bool eventRoute)
{
    int save = r.Pos; var res = new Dictionary<int, (float x, float y, float z)>();
    for (int i = 1; i < NodeCount; i++)
    {
        if (nodeFlags[i] == 0) continue;
        var p = (x: r.F32(), y: r.F32(), z: r.F32());
        if (!(Math.Abs(p.x) <= 20000 && Math.Abs(p.z) <= 20000 && p.y > -500 && p.y < 5000)) { r.Pos = save; return null; }
        res[i] = p;
        int segs = r.U8(); if (segs > 8) { r.Pos = save; return null; }
        for (int k = 0; k < segs; k++) r.U16();
        r.U32(); r.U16(); r.U16(); r.U24(); r.U8(); r.U8(); r.U8(); r.U8(); if (eventRoute) r.U16();
    }
    return res;
}
var nodePos = ParseNodes(false) ?? ParseNodes(true) ?? throw new Exception("node records do not parse");
// The segment record has more version-dependent fields after the ones above; instead of parsing every record,
// resolve each segment's first lane by walking: a lane belongs to the segment whose chain contains it. Chains are
// read from m_nextLane; chain heads are lanes that no other lane points to. The head -> segment link comes from the
// dump (segment start/end nodes + lane count), matched below.
var pointedTo = new HashSet<uint>(); for (uint i = 1; i < LaneCount; i++) if (laneFlags[i] != 0 && nextLane[i] != 0) pointedTo.Add(nextLane[i]);
var chains = new List<List<uint>>();
for (uint i = 1; i < LaneCount; i++)
{
    if (laneFlags[i] == 0 || pointedTo.Contains(i)) continue;
    var c = new List<uint>(); uint l = i; int guard = 0;
    while (l != 0 && guard++ < 64) { c.Add(l); l = nextLane[l]; }
    chains.Add(c);
}

// ---------- dump (geometry, prefab lanes) ----------
var dump = JsonNode.Parse(File.ReadAllText(dumpPath))!;
var dNodes = dump["nodes"]!.AsArray().ToDictionary(n => (int)n!["id"]!, n => n!);
var dSegs = dump["segments"]!.AsArray().ToDictionary(s => (int)s!["id"]!, s => s!);
var prefabs = dump["netPrefabs"]!.AsObject();
int posMatch = 0, posChecked = 0;
foreach (var (id, p) in nodePos)
    if (dNodes.TryGetValue(id, out var dn)) { posChecked++; var q = dn["position"]!.AsArray(); if (Math.Abs((double)q[0]! - p.x) < 0.01 && Math.Abs((double)q[2]! - p.z) < 0.01) posMatch++; }
Console.WriteLine($"NetManager @ {start}: lanes {createdLanes}, nodes {nodePos.Count} (positions equal to the dump: {posMatch}/{posChecked}), chains {chains.Count}");

// lane -> (segment, index): the first segment record gives one verified head; for the rest, read every segment record
// by scanning forward: records start with buildIndex/modifiedIndex (U32, U32) then m_lanes (U24) which must be a chain head
// whose length equals the prefab lane count, followed by start/end node ids equal to the dump's.
var laneOwner = new Dictionary<uint, (int seg, int idx)>(); string chainCheck = "";
var heads = chains.ToDictionary(c => c[0], c => c);
{
    // scan the segment-record area for the pattern [lanes U24][path U24][6 floats][start U16][end U16]
    var bySeg = new Dictionary<int, uint>();
    var want = dSegs.ToDictionary(kv => ((int)kv.Value["startNode"]!, (int)kv.Value["endNode"]!), kv => kv.Key);
    for (int p = r.Pos; p < data.Length - 40 && bySeg.Count < dSegs.Count; p++)
    {
        int sn = (data[p + 30] << 8) | data[p + 31], en = (data[p + 32] << 8) | data[p + 33];
        if (!want.TryGetValue((sn, en), out int sid) || bySeg.ContainsKey(sid)) continue;
        uint head = (uint)((data[p] << 16) | (data[p + 1] << 8) | data[p + 2]);
        if (head == 0 || !heads.ContainsKey(head)) continue;
        bySeg[sid] = head; p += 33;
    }
    foreach (var (sid, head) in bySeg) { var c = heads[head]; for (int i = 0; i < c.Count; i++) laneOwner[c[i]] = (sid, i); }
    int lenOk = bySeg.Count(kv => heads[kv.Value].Count == (int)(prefabs[(string)dSegs[kv.Key]["prefab"]!]?["laneCount"] ?? -1));
    int withLanes = dSegs.Values.Count(ds => (int)(prefabs[(string)ds["prefab"]!]?["laneCount"] ?? 0) > 0);
    Console.WriteLine($"segments resolved to lane chains: {bySeg.Count} of {withLanes} segments with lanes; chain length = prefab lane count for {lenOk}");
    chainCheck = $"{bySeg.Count} of {withLanes} segments with lanes resolved; chain length equals the prefab lane count for {lenOk}";
}

// ---------- TM:PE ----------
var cfg = (ClassRecord)NrbfDecoder.Decode(File.OpenRead(Path.Combine(payloadDir, "TrafficManager_v1.0.bin")));
IEnumerable<ClassRecord> List(string name)
{
    if (cfg.GetRawValue(name) is not ClassRecord l || !l.HasMember("_items")) return Array.Empty<ClassRecord>();
    int n = Convert.ToInt32(l.GetRawValue("_size"), inv);
    return ((SZArrayRecord<SerializationRecord>)l.GetRawValue("_items")!).GetArray().Take(n).OfType<ClassRecord>();
}
static object? Raw(ClassRecord c, string m) { var v = c.GetRawValue(m); return v is ClassRecord e && e.HasMember("value__") ? e.GetRawValue("value__") : v; }

JsonObject LaneInfo(uint laneId, bool? atStartNode)
{
    var o = new JsonObject { ["laneId"] = laneId };
    if (!laneOwner.TryGetValue(laneId, out var own)) { o["resolved"] = false; return o; }
    var ds = dSegs[own.seg]; var pf = prefabs[(string)ds["prefab"]!]!; var lane = pf["lanes"]![own.idx]!;
    o["segment"] = own.seg; o["laneIndex"] = own.idx; o["prefab"] = (string)ds["prefab"]!;
    o["laneType"] = (string)lane["type"]!; o["vehicleType"] = (string)lane["vehicleType"]!; o["direction"] = (string)lane["direction"]!;
    o["lanePosition"] = (double)lane["position"]!;
    if (atStartNode is bool s)
    {
        int node = s ? (int)ds["startNode"]! : (int)ds["endNode"]!; o["node"] = node;
        // lane end at the node: node position + lateral offset (lane position, mirrored for inverted segments)
        var bz = ds["bezier"]!.AsArray(); var a = bz[s ? 0 : 3]!.AsArray(); var b = bz[s ? 1 : 2]!.AsArray();
        double dx = (double)b[0]! - (double)a[0]!, dz = (double)b[2]! - (double)a[2]!, L = Math.Sqrt(dx * dx + dz * dz); if (L < 1e-6) { dx = 0; dz = 1; L = 1; }
        dx /= L; dz /= L; if (!s) { dx = -dx; dz = -dz; }   // forward direction of the segment (start -> end) at this end
        double lat = (double)lane["position"]! * ((bool)ds["invert"]! ? -1 : 1);
        o["endPosition"] = new JsonArray(Math.Round((double)a[0]! + dz * lat, 2), Math.Round((double)a[1]!, 2), Math.Round((double)a[2]! - dx * lat, 2));
    }
    return o;
}

var conns = new JsonArray(); int unresolved = 0;
foreach (var c in List("LaneConnections"))
{
    uint src = Convert.ToUInt32(Raw(c, "sourceLaneId"), inv), dst = Convert.ToUInt32(Raw(c, "targetLaneId"), inv);
    bool startNode = Convert.ToBoolean(Raw(c, "sourceStartNode"), inv);
    var s = LaneInfo(src, startNode);
    int? node = s["node"]?.GetValue<int>();
    bool? dstStart = null;
    if (node != null && laneOwner.TryGetValue(dst, out var dOwn)) dstStart = (int)dSegs[dOwn.seg]["startNode"]! == node;
    var t = LaneInfo(dst, dstStart);
    if (s["resolved"] != null || t["resolved"] != null) unresolved++;
    conns.Add(new JsonObject { ["node"] = node, ["group"] = Raw(c, "group")?.ToString(), ["source"] = s, ["target"] = t });
}
var arrows = new JsonArray();
string ArrowNames(uint f) { var n = new List<string>(); if ((f & 32) != 0) n.Add("Left"); if ((f & 16) != 0) n.Add("Forward"); if ((f & 64) != 0) n.Add("Right"); return string.Join("+", n); }
foreach (var a in List("LaneArrows"))
{
    uint lid = Convert.ToUInt32(Raw(a, "laneId"), inv), f = Convert.ToUInt32(Raw(a, "arrows"), inv);
    var li = LaneInfo(lid, null);
    // arrows apply at the lane's end node in its travel direction
    if (li["segment"] != null) { var ds = dSegs[li["segment"]!.GetValue<int>()]; bool fwd = ((string)li["direction"]!).StartsWith("Forward") ^ (bool)ds["invert"]!; li = LaneInfo(lid, !fwd); }
    arrows.Add(new JsonObject { ["arrowsRaw"] = f, ["arrows"] = ArrowNames(f), ["lane"] = li });
}
var other = new JsonObject();
foreach (var name in cfg.MemberNames)
{
    var v = cfg.GetRawValue(name);
    if (v is ClassRecord l && l.HasMember("_size")) other[name] = Convert.ToInt32(l.GetRawValue("_size"), inv);
    else if (v is ClassRecord d && d.HasMember("KeyValuePairs")) other[name] = d.GetRawValue("KeyValuePairs") is ArrayRecord kv ? kv.Lengths[0] : 0;
}

var root = new JsonObject
{
    ["schema"] = "dakota-offline/cs1/tmpe/v1",
    ["source"] = "TrafficManager_v1.0 (TM:PE 11.7.4 Configuration) of the map's data asset; lane ids resolved through the decoded NetManager lane table",
    ["coordinates"] = "CS1 world metres, x east, y up, z north; endPosition = lane end at the node, on the node's cross line (no corner offset)",
    ["listSizes"] = other,
    ["laneConnections"] = conns,
    ["laneArrows"] = arrows,
};
File.WriteAllText(Path.Combine(outDir, "tmpe-offline.json"), root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));

var md = new StringBuilder();
md.AppendLine("# TM:PE data: summary");
md.AppendLine();
md.AppendLine("Decoded from the map's `TrafficManager_v1.0` data (TM:PE 11.7.4 `Configuration`, BinaryFormatter). Generated by `src/tmpe-offline.cs`; details in `tmpe-offline.json`.");
md.AppendLine();
md.AppendLine("| list | entries |"); md.AppendLine("|---|---:|");
foreach (var (k, v) in other) md.AppendLine($"| {k} | {v} |");
md.AppendLine();
md.AppendLine($"Lane ids resolved through the NetManager lane table decoded from the map: {laneOwner.Count} lanes on {laneOwner.Values.Select(x => x.seg).Distinct().Count()} segments; node positions equal to the dump for {posMatch}/{posChecked} nodes; {chainCheck}. Unresolved TM:PE entries: {unresolved}.");
md.AppendLine();
md.AppendLine("## Lane connections by node");
md.AppendLine();
md.AppendLine("| node | x | z | connections | from segment:lane → to segment:lane |"); md.AppendLine("|---:|---:|---:|---:|---|");
foreach (var g in conns.GroupBy(c => c!["node"]?.GetValue<int>() ?? -1).OrderBy(g => g.Key))
{
    var p = g.Key >= 0 && nodePos.TryGetValue(g.Key, out var np) ? np : (0f, 0f, 0f);
    md.AppendLine($"| {g.Key} | {p.Item1:0} | {p.Item3:0} | {g.Count()} | {string.Join(", ", g.Select(c => $"{c!["source"]!["segment"]}:{c["source"]!["laneIndex"]}→{c["target"]!["segment"]}:{c["target"]!["laneIndex"]}"))} |");
}
md.AppendLine();
md.AppendLine("## Lane arrows");
md.AppendLine();
md.AppendLine("| node | segment | lane | arrows |"); md.AppendLine("|---:|---:|---:|---|");
foreach (var a in arrows) md.AppendLine($"| {a!["lane"]!["node"]} | {a["lane"]!["segment"]} | {a["lane"]!["laneIndex"]} | {a["arrows"]} |");
File.WriteAllText(Path.Combine(outDir, "tmpe-summary.md"), md.ToString(), new UTF8Encoding(false));
Console.WriteLine($"TM:PE: {conns.Count} lane connections, {arrows.Count} lane arrows, unresolved {unresolved}");

// =====================================================================
static byte[] ReadDataAsset(string file)
{
    using var br = new BinaryReader(File.OpenRead(file));
    if (new string(br.ReadChars(4)) != "CRAP") throw new InvalidDataException(file);
    int version = br.ReadUInt16(); br.ReadString(); if (version >= 3) br.ReadString(); br.ReadUInt32(); br.ReadString();
    int count = br.ReadInt32(); long dataOffset = br.ReadInt64();
    for (int i = 0; i < count; i++)
    {
        br.ReadString(); br.ReadString(); int type = br.ReadInt32(); long off = br.ReadInt64(), size = br.ReadInt64();
        if (type == 52) { br.BaseStream.Seek(dataOffset + off, SeekOrigin.Begin); return br.ReadBytes((int)size); }
    }
    throw new InvalidDataException("no data asset");
}
// DataSerializer string: 0x80, UInt16 BE length, chars as byte deltas starting from 0x80
static int FindDeltaString(byte[] b, string s, int from)
{
    var pat = new byte[3 + s.Length]; pat[0] = 0x80; pat[1] = (byte)(s.Length >> 8); pat[2] = (byte)s.Length;
    int prev = 0x80; for (int i = 0; i < s.Length; i++) { pat[3 + i] = (byte)(s[i] - prev); prev = s[i]; }
    int idx = b.AsSpan(from).IndexOf(pat);
    return idx < 0 ? -1 : from + idx + pat.Length;
}

class Reader
{
    readonly byte[] b; public int Pos;
    public Reader(byte[] b, int pos) { this.b = b; Pos = pos; }
    public int U8() => b[Pos++];
    public int I8() => (sbyte)b[Pos++];
    public int U16() { int v = (b[Pos] << 8) | b[Pos + 1]; Pos += 2; return v; }
    public int I16() => (short)U16();
    public uint U24() { uint v = (uint)((b[Pos] << 16) | (b[Pos + 1] << 8) | b[Pos + 2]); Pos += 3; return v; }
    public uint U32() { uint v = (uint)((b[Pos] << 24) | (b[Pos + 1] << 16) | (b[Pos + 2] << 8) | b[Pos + 3]); Pos += 4; return v; }
    public int I32() => (int)U32();
    public ulong U64() { ulong hi = U32(), lo = U32(); return (hi << 32) | lo; }
    public float F32() => BitConverter.Int32BitsToSingle(I32());
}

// ColossalFramework EncodedArray reader. Control byte: >= 128 -> repeat previous delta (c - 127) times;
// 64..127 -> (c - 63) values, each delta-of-delta of closeBytes; else (c + 1) full values of fullBytes.
// Byte arrays have no close mode (< 128 = full).
class EA
{
    readonly Reader r; readonly int full, close; readonly bool hasClose;
    long prev, delta; int same, closeN, fullN;
    public EA(Reader r, int fullBytes, int closeBytes, bool hasClose) { this.r = r; full = fullBytes; close = closeBytes; this.hasClose = hasClose; }
    public long Read()
    {
        while (true)
        {
            if (same > 0) { prev += delta; same--; return Wrap(prev); }
            if (closeN > 0) { delta += close == 1 ? r.I8() : close == 2 ? r.I16() : r.I32(); prev += delta; closeN--; return Wrap(prev); }
            if (fullN > 0) { long v = full switch { 1 => r.U8(), 2 => r.U16(), 4 => r.U32(), _ => (long)r.U64() }; delta = v - prev; prev = v; fullN--; return Wrap(prev); }
            int c = r.U8();
            if (c >= 128) same = c - 127;
            else if (hasClose && c >= 64) closeN = c - 63;
            else fullN = c + 1;
        }
    }
    long Wrap(long v) => full switch { 1 => v & 0xFF, 2 => v & 0xFFFF, 4 => v & 0xFFFFFFFFL, _ => v };
}
