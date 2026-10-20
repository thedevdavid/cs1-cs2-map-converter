// Live bridge rules for the prefabs bridges.tsv uses that ../bridges/cs2-bridge-rules.json does not cover (vanilla / DLC; offline unreadable),
// plus the dam net: GET /prefabs/info?name=… → bridge block. Writes bridge-rules-live.json in the shape of cs2-bridge-rules.json "prefabs".
// node tools/fetch-bridge-rules.mjs [--bridge http://127.0.0.1:8642]
import { existsSync, readFileSync, writeFileSync } from "node:fs";
const args = process.argv.slice(2);
const bridge = (i => (i >= 0 ? args[i + 1] : "http://127.0.0.1:8642"))(args.indexOf("--bridge"));
const offline = existsSync("../bridges/cs2-bridge-rules.json") ? new Set(JSON.parse(readFileSync("../bridges/cs2-bridge-rules.json", "utf8")).prefabs.map(p => p.name)) : new Set();
const names = new Set(["Hydroelectric_Power_Plant_01 Dam"]);
for (const l of readFileSync("bridges.tsv", "utf8").split(/\r?\n/)) {
  if (!l || l.startsWith("#")) continue;
  const c = l.split("\t"); names.add(c[2]);
  for (const alt of (c[6] ?? "").split("|")[0].split(/[;,]/)) if (/Bridge|Viaduct|Truss/.test(alt)) names.add(alt.trim());
}
const out = [];
for (const n of [...names].filter(n => !offline.has(n)).sort()) {
  const r = await fetch(`${bridge}/prefabs/info?name=${encodeURIComponent(n)}`).then(r => r.json()).catch(e => ({ error: String(e) }));
  const b = r.bridge;
  if (!b) { console.log(`${n}: ${r.error ?? "no bridge block"}`); continue; }
  const fixed = (b.fixedSegments ?? []).map(f => ({ length: f.length, canCurve: f.canCurve, countRange: f.countRange }));
  const range = b.derived?.edgeLengthRange ?? [0, 1e9];
  out.push({ name: n, class: r.type, segmentLength: b.segmentLength, elevationOnWater: b.elevationOnWater, canCurve: b.canCurve, fixedSegments: fixed,
    edgeLengthRange: { min: range[0], max: range[1] }, minElevatedHeight: b.derived?.minElevatedHeight ?? null,
    importEdge: fixed.length ? { edges: 1, lengthMin: range[0], lengthMax: range[1], nominal: fixed.reduce((s, f) => s + f.length, 0) } : { note: "any length >= min" },
    source: "live /prefabs/info" });
  console.log(`${n}: seg ${b.segmentLength}, fixed ${fixed.map(f => f.length).join("+") || "-"}, range ${range[0]}–${range[1]}, min elevated ${b.derived?.minElevatedHeight}`);
}
writeFileSync("bridge-rules-live.json", JSON.stringify({ generated: new Date().toISOString(), bridge, prefabs: out }, null, 1));
console.log(`${out.length} prefabs -> bridge-rules-live.json`);
