// Summarise Procedural Objects stored in a CS1 map/save (BinaryFormatter, decoded with System.Formats.Nrbf).
// Usage: dotnet run po-summary.cs -- <extract dir>
#:package System.Formats.Nrbf@9.0.0
#pragma warning disable SYSLIB5005
using System.Formats.Nrbf;
var dir = args[0]; // folder written by: node crp-moddata.mjs <file.crp> --extract <dir>
var bytes = File.ReadAllBytes(Path.Combine(dir, "ProceduralObjectsDataKey.bin")).Concat(File.ReadAllBytes(Path.Combine(dir, "ProceduralObjectsDataKey1.bin"))).ToArray();
var root = NrbfDecoder.Decode(new MemoryStream(bytes), out var recs, leaveOpen: false);
Console.WriteLine($"root {root.RecordType} {root.TypeName.FullName}");
SZArrayRecord<SerializationRecord> arr = root as SZArrayRecord<SerializationRecord>;
var items = arr.GetArray();
Console.WriteLine($"objects {items.Length}");
var byPrefab = new Dictionary<string,int>(); var byType = new Dictionary<string,int>(); int edited=0, text=0, tex=0, grouped=0, asphalt=0; long verts=0;
foreach (var it in items) {
  if (it is not ClassRecord c) continue;
  string S(string n) { try { return c.GetString(n) ?? "null"; } catch { return "?"; } }
  var p = S("basePrefabName"); var t = S("objectType");
  byPrefab[t+":"+p] = byPrefab.GetValueOrDefault(t+":"+p) + 1; byType[t] = byType.GetValueOrDefault(t)+1;
  if (c.GetRawValue("meshStatus")?.ToString() == "2") edited++;
  try { if (c.GetRawValue("textParam") != null) text++; } catch {}
  try { if (c.GetBoolean("hasCustomTexture")) tex++; } catch {}
  try { if (c.GetBoolean("belongsToGroup")) grouped++; } catch {}
  try { var v = c.GetRawValue("vertices"); if (v is SZArrayRecord<SerializationRecord> va) verts += va.Length; else if (v is SerializationRecord sr && sr is ArrayRecord ar) verts += ar.Lengths[0]; } catch {}
}
Console.WriteLine($"types {string.Join(", ", byType.Select(k=>k.Key+"="+k.Value))}");
Console.WriteLine($"meshStatus==2 (edited mesh) {edited}; textParam {text}; customTexture {tex}; inGroup {grouped}; stored vertices {verts}");
foreach (var kv in byPrefab.OrderByDescending(k=>k.Value)) Console.WriteLine($"  {kv.Value,5}  {kv.Key}");
