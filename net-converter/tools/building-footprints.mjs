// Round 49 (bridge 0.9.56 GET /debug/override-loop on A-v22: 96 objects in the OverrideSystem / Anarchy PreventOverride loop): no object of a masked layer
// may stand inside the footprint of a building the layers create. Footprint = the building prefab's geometry bounds in x/z (building-bounds.json, from
// GET /prefabs/info), turned with the object's rotation (the bridge's RotateY: local x → (cos, −sin), local z → (sin, cos)); the test is the object's
// position, as the bridge's skipInsideBuildings does. Rewrites the masked files without those objects and writes footprints-check.md / .json per folder;
// a second pass confirms 0 left.
// node tools/building-footprints.mjs [--out out]   (handles <out>, <out>/x1 and the shared <out>/trees)
import { readFileSync, writeFileSync, existsSync, readdirSync } from "node:fs";
import { join } from "node:path";
const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 ? args[i + 1] : d; };
const out = opt("--out", "out");
const bounds = JSON.parse(readFileSync("building-bounds.json", "utf8")).buildings;
// The dam's plant is spawned by the dam course (import-dam.json), not placed by us: its centre as measured live on A-v22 (override-loop-a22b.json); its
// rotation is unknown there, so its footprint is the circle through the bounds' corners.
const DAM_PLANT = { prefab: "HydroelectricPowerPlant01", x: 1381.35, z: 4691.62, rotation: null };
const BUILDING_FILES = ["import-fort-buildings.json", "import-objects.json", "import-substation.json"];
const MASKED_FILES = ["import-rock-scatter.json", "import-po-rocks.json", "import-objects.json", "import-walls-props.json", "import-rock-shed.json", "import-fort-props.json"];
const J = f => JSON.parse(readFileSync(f, "utf8"));
function buildingsIn(dir) {
  const list = [];
  for (const f of BUILDING_FILES) {
    const p = join(dir, f); if (!existsSync(p)) continue;
    for (const o of J(p).objects) if (bounds[o.prefab]?.type === "BuildingPrefab") list.push({ ...o, file: f });
  }
  list.push({ ...DAM_PLANT, file: "import-dam.json (spawned plant, live A-v22 position)" });
  return list;
}
function inside(b, x, z) {
  const bb = bounds[b.prefab]; if (!bb) return false;
  const dx = x - b.x, dz = z - b.z;
  if (b.rotation == null) { const r = Math.hypot(Math.max(-bb.min[0], bb.max[0]), Math.max(-bb.min[1], bb.max[1])); return dx * dx + dz * dz <= r * r; }
  const r = b.rotation * Math.PI / 180, u = dx * Math.cos(r) - dz * Math.sin(r), v = dx * Math.sin(r) + dz * Math.cos(r);
  return u >= bb.min[0] && u <= bb.max[0] && v >= bb.min[1] && v <= bb.max[1];
}
function filter(path, buildings, report, write) {
  if (!existsSync(path)) return 0;
  const j = J(path); if (!Array.isArray(j.objects)) return 0;
  const keep = [], gone = [];
  for (const o of j.objects) {
    if (bounds[o.prefab]?.type === "BuildingPrefab") { keep.push(o); continue; }   // the buildings themselves stay
    const b = buildings.find(b => inside(b, o.x, o.z));
    if (b) gone.push({ o, b }); else keep.push(o);
  }
  if (gone.length && write) { j.objects = keep; writeFileSync(path, JSON.stringify(j, null, path.includes(`${"trees"}`) ? 0 : 1)); }
  for (const { o, b } of gone) {
    const k = `${path.split(/[\\/]/).slice(-2).join("/")} | ${b.prefab} (${b.x.toFixed(1)}, ${b.z.toFixed(1)})`;
    report[k] ??= {}; report[k][o.prefab] = (report[k][o.prefab] ?? 0) + 1;
  }
  return gone.length;
}
function pass(dir, treeDir, write) {
  const buildings = buildingsIn(dir), report = {}; let n = 0;
  const files = [...MASKED_FILES.map(f => join(dir, f)), ...readdirSync(dir).filter(f => /^import-trees-fixed-\d+\.json$/.test(f)).map(f => join(dir, f)),
    ...(treeDir && existsSync(treeDir) ? readdirSync(treeDir).filter(f => /^import-trees-\d+\.json$/.test(f)).map(f => join(treeDir, f)) : [])];
  for (const f of files) n += filter(f, buildings, report, write);
  return { buildings, report, n };
}
for (const [dir, treeDir] of [[out, join(out, "trees")], [join(out, "x1"), null]]) {
  if (!existsSync(dir)) continue;
  const first = pass(dir, treeDir, true), again = pass(dir, treeDir, false);
  const byBuilding = {}; for (const b of first.buildings) byBuilding[b.prefab] = (byBuilding[b.prefab] ?? 0) + 1;
  const lines = [`# Objects inside building footprints (round 49)`, ``,
    `Buildings (${first.buildings.length}): ${Object.entries(byBuilding).map(([k, v]) => `${k} ${v}`).join(", ")}. Footprint = geometry bounds x/z from building-bounds.json (GET /prefabs/info), turned with the object's rotation; the dam plant (spawned by the dam course) at its live A-v22 centre with the circle through its corners.`,
    ``, `Removed from the masked layers: **${first.n}**. Check after removal: **${again.n}** left.`, ``, `| file | building | removed |`, `|---|---|---|`,
    ...Object.entries(first.report).map(([k, v]) => { const [f, b] = k.split(" | "); return `| ${f} | ${b} | ${Object.entries(v).map(([p, c]) => `${p} ${c}`).join(", ")} |`; })];
  writeFileSync(join(dir, "footprints-check.md"), lines.join("\n") + "\n");
  writeFileSync(join(dir, "footprints-check.json"), JSON.stringify({ removed: first.n, left: again.n, buildings: first.buildings.map(b => ({ prefab: b.prefab, x: b.x, z: b.z, rotation: b.rotation, file: b.file })), report: first.report }, null, 1));
  console.log(`building footprints ${dir}: ${first.buildings.length} buildings, ${first.n} objects removed, ${again.n} left -> ${join(dir, "footprints-check.md")}`);
}
