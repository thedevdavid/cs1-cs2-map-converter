// Mesh bounds of every object prefab the import files use (and the PO rock / scatter sets), from the running bridge:
// GET /prefabs/info?name=<prefab> → geometry.{size, boundsMin, boundsMax}. Writes prefab-bounds.json for the converter.
// node tools/fetch-bounds.mjs [--bridge http://127.0.0.1:8642] [--out prefab-bounds.json]
import { existsSync, readdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (n, d) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : d; };
const bridge = opt("--bridge", "http://127.0.0.1:8642"), outFile = opt("--out", "prefab-bounds.json");
const names = new Set();
for (const dir of ["out/x1", "out/trees"].filter(existsSync))
  for (const f of readdirSync(dir).filter(f => f.startsWith("import-") && f.endsWith(".json") && !f.startsWith("import-trees")))
    for (const o of JSON.parse(readFileSync(join(dir, f), "utf8")).objects ?? []) names.add(o.prefab);
const range = (p, a, b, w = 2) => Array.from({ length: b - a + 1 }, (_, i) => `${p}${String(a + i).padStart(w, "0")}`);
for (const n of [...range("Rock", 1, 30), ...range("Boulder", 1, 8), ...range("StandingStone", 1, 8), ...Array.from({ length: 8 }, (_, i) => `rzrock${i + 1}`),
  ...Array.from({ length: 5 }, (_, i) => `RZaridrock${i + 1}`), "CliffRock3", "CliffRock5", "CliffRock6", "CliffStack", "CliffSide2", "CliffSide2Left", "CliffSide2Right",
  "CliffSideTall", "CliffSlim", "CliffFreeStanding", "RuinsStoneFormation01", "RuinsStoneFormation02", "RuinsStoneCairns01", "RuinsStoneCairns02", "RuinsStoneCairns03",
  "OldCastle01", "RuinsCastle01", "LeisurePier01HeadSmall", "ConcreteBarrier01", "SafetyBarrier01"]) names.add(n);
const prev = existsSync(outFile) ? JSON.parse(readFileSync(outFile, "utf8")).prefabs ?? {} : {};
const res = { ...prev }; let ok = 0, missing = [];
for (const n of [...names].sort()) {
  const r = await fetch(`${bridge}/prefabs/info?name=${encodeURIComponent(n)}`).then(r => r.json()).catch(e => ({ error: String(e) }));
  const g = r.geometry;
  if (g?.boundsMin && g?.boundsMax) { res[n] = { min: g.boundsMin, max: g.boundsMax, type: r.type }; ok++; } else missing.push(`${n}: ${r.error ?? "no geometry"}`);
}
writeFileSync(outFile, JSON.stringify({ source: `${bridge}/prefabs/info`, fetchedAt: new Date().toISOString(), prefabs: res }, null, 1));
console.log(`${ok} prefabs with bounds -> ${outFile}${missing.length ? `; no bounds: ${missing.join("; ")}` : ""}`);
