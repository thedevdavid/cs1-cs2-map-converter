// Live bounds of the kit prefabs the PO structure layer uses (bridges/PLAN-structures.md): GET /prefabs/info?name=… → geometry.boundsMin/Max.
// Writes po-structure-bounds.json; a prefab the running game does not have (mod not in the playset) is listed with "missing".
// node tools/fetch-po-structure-bounds.mjs [--bridge http://127.0.0.1:8642]
import { existsSync, readFileSync, writeFileSync } from "node:fs";
// A prefab the game does not have keeps an earlier measured entry from po-structure-bounds.json.
const prev = existsSync("po-structure-bounds.json") ? new Map(JSON.parse(readFileSync("po-structure-bounds.json", "utf8")).prefabs.filter(p => !p.missing).map(p => [p.name, p])) : new Map();
const args = process.argv.slice(2);
const bridge = (i => (i >= 0 ? args[i + 1] : "http://127.0.0.1:8642"))(args.indexOf("--bridge"));
const names = ["StoneArchRailBridge", "Pillar1x16", "Pillar4x16", "BasePlateConcrete32", "HWY Truss Gantry 2L  A", "ITBP_Pillar Piers 01"];
const out = [];
for (const n of names) {
  // encodeURIComponent: spaces as %20 (the bridge does not read "+" as a space; the gantry name has a double space).
  const r = await fetch(`${bridge}/prefabs/info?name=${encodeURIComponent(n)}`).then(r => r.json()).catch(e => ({ error: String(e) }));
  const g = r.geometry;
  if (!g && prev.has(n)) { out.push(prev.get(n)); console.log(`${n}: not in the game, kept ${prev.get(n).source ?? "earlier"} entry`); continue; }
  if (!g) { out.push({ name: n, missing: r.error ?? "no geometry" }); console.log(`${n}: ${r.error ?? "no geometry"}`); continue; }
  out.push({ name: n, type: r.type, boundsMin: g.boundsMin, boundsMax: g.boundsMax, size: g.size });
  console.log(`${n}: min y ${g.boundsMin.y}, max y ${g.boundsMax.y}, size ${g.size.x} × ${g.size.y} × ${g.size.z}`);
}
writeFileSync("po-structure-bounds.json", JSON.stringify({ generated: new Date().toISOString(), bridge, prefabs: out }, null, 1));
console.log(`${out.filter(p => !p.missing).length}/${out.length} prefabs measured -> po-structure-bounds.json`);
