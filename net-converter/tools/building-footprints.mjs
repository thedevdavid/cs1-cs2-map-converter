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
function inside(b, x, z, margin = 0) {
  const bb = bounds[b.prefab]; if (!bb) return false;
  const dx = x - b.x, dz = z - b.z;
  if (b.rotation == null) { const r = Math.hypot(Math.max(-bb.min[0], bb.max[0]), Math.max(-bb.min[1], bb.max[1])) + margin; return dx * dx + dz * dz <= r * r; }
  const r = b.rotation * Math.PI / 180, u = dx * Math.cos(r) - dz * Math.sin(r), v = dx * Math.sin(r) + dz * Math.cos(r);
  return u >= bb.min[0] - margin && u <= bb.max[0] + margin && v >= bb.min[1] - margin && v <= bb.max[1] + margin;
}
// Round 50 (owner's intent: the CS1 ruin field): the fort's standing stones are moved, not dropped: the nearest point outside every footprint
// with MOVE_MARGIN, searched on rings of 0.25 m up to MOVE_MAX around the CS1 position, yaw kept; dropped only if none is found.
const MOVE_FILES = new Set(["import-fort-props.json"]), MOVE_MARGIN = 1, MOVE_MAX = 6;
function moveOut(o, buildings) {
  for (let rad = 0.25; rad <= MOVE_MAX + 1e-9; rad += 0.25)
    for (let k = 0; k < 64; k++) {
      const a = 2 * Math.PI * k / 64, x = o.x + rad * Math.cos(a), z = o.z + rad * Math.sin(a);
      if (!buildings.some(b => inside(b, x, z, MOVE_MARGIN))) return { x: Math.round(x * 100) / 100, z: Math.round(z * 100) / 100, d: rad };
    }
  return null;
}
function filter(path, buildings, report, write) {
  if (!existsSync(path)) return 0;
  const j = J(path); if (!Array.isArray(j.objects)) return 0;
  let moved = 0;
  const keep = [], gone = [], move = MOVE_FILES.has(path.split(/[\\/]/).pop());
  for (const o of j.objects) {
    if (bounds[o.prefab]?.type === "BuildingPrefab") { keep.push(o); continue; }   // the buildings themselves stay
    const b = buildings.find(b => inside(b, o.x, o.z, move ? MOVE_MARGIN : 0));
    if (!b) { keep.push(o); continue; }
    const m = move ? moveOut(o, buildings) : null;
    if (m) { report.moved ??= []; report.moved.push({ file: path.split(/[\\/]/).slice(-2).join("/"), id: o.id, prefab: o.prefab, from: [o.x, o.z], to: [m.x, m.z], d: m.d, building: b.prefab }); keep.push({ ...o, x: m.x, z: m.z }); moved++; }
    else gone.push({ o, b });
  }
  if ((gone.length || moved) && write) { j.objects = keep; writeFileSync(path, JSON.stringify(j, null, path.includes(`${"trees"}`) ? 0 : 1)); }
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
// Round 50b (bridge 0.9.58 on A23c: OldCastle01 owns 914 sub-objects; ours overlapping them loop between OverrideSystem and Anarchy): no other building
// of our layers may intersect the castle's footprint. Each one that does is moved to the nearest position (rings of 0.5 m up to CASTLE_MOVE) where its
// footprint is clear of the castle's, yaw kept, else dropped. On by default; --keep-castle-lot-ruins keeps them (the CS1 Castle Wall stood there).
const CASTLE = "OldCastle01", CASTLE_MOVE = 15, clearCastle = !args.includes("--keep-castle-lot-ruins");
const corners = (b, x = b.x, z = b.z) => { const bb = bounds[b.prefab], r = (b.rotation ?? 0) * Math.PI / 180, c = Math.cos(r), s = Math.sin(r);
  return [[bb.min[0], bb.min[1]], [bb.max[0], bb.min[1]], [bb.max[0], bb.max[1]], [bb.min[0], bb.max[1]]].map(([u, v]) => [x + u * c + v * s, z - u * s + v * c]); };
function overlap(p, q) {   // two convex quads (SAT)
  for (const poly of [p, q]) for (let i = 0; i < 4; i++) {
    const [x1, z1] = poly[i], [x2, z2] = poly[(i + 1) % 4], nx = z1 - z2, nz = x2 - x1;
    const pr = pts => pts.map(([x, z]) => x * nx + z * nz); const a = pr(p), b = pr(q);
    if (Math.max(...a) < Math.min(...b) || Math.max(...b) < Math.min(...a)) return false;
  }
  return true;
}
function clearCastleLot(dir) {
  const fb = join(dir, "import-fort-buildings.json"); if (!existsSync(fb)) return null;
  const castles = J(fb).objects.filter(o => o.prefab === CASTLE); if (!castles.length) return null;
  const lots = castles.map(c => corners(c)), rep = { moved: [], dropped: [], intersecting: 0 };
  for (const f of ["import-fort-buildings.json", "import-objects.json"]) {
    const p = join(dir, f); if (!existsSync(p)) continue;
    const j = J(p), keep = [];
    for (const o of j.objects) {
      if (o.prefab === CASTLE || bounds[o.prefab]?.type !== "BuildingPrefab" || !lots.some(l => overlap(l, corners(o)))) { keep.push(o); continue; }
      rep.intersecting++;
      if (!clearCastle) { keep.push(o); continue; }
      let to = null;
      for (let rad = 0.5; rad <= CASTLE_MOVE + 1e-9 && !to; rad += 0.5)
        for (let k = 0; k < 64; k++) { const a = 2 * Math.PI * k / 64, x = o.x + rad * Math.cos(a), z = o.z + rad * Math.sin(a); if (!lots.some(l => overlap(l, corners(o, x, z)))) { to = { x: Math.round(x * 100) / 100, z: Math.round(z * 100) / 100, d: rad }; break; } }
      if (to) { rep.moved.push({ file: f, id: o.id, prefab: o.prefab, from: [o.x, o.z], to: [to.x, to.z], d: to.d }); keep.push({ ...o, x: to.x, z: to.z }); }
      else rep.dropped.push({ file: f, id: o.id, prefab: o.prefab, at: [o.x, o.z] });
    }
    if (clearCastle && (keep.length !== j.objects.length || keep.some((o, i) => o !== j.objects[i]))) { j.objects = keep; writeFileSync(p, JSON.stringify(j, null, 1)); }
  }
  return rep;
}
for (const [dir, treeDir] of [[out, join(out, "trees")], [join(out, "x1"), null]]) {
  if (!existsSync(dir)) continue;
  const castle = clearCastleLot(dir);
  if (castle) {
    const cnt = a => Object.entries(a.reduce((m, q) => (m[q.prefab] = (m[q.prefab] ?? 0) + 1, m), {})).map(([k, v]) => `${k} ${v}`).join(", ") || "none";
    writeFileSync(join(dir, "castle-lot-check.md"), [`# Buildings of our layers intersecting the ${CASTLE} footprint (round 50b)`, ``,
      `Switch: ${clearCastle ? "on (default): moved up to " + CASTLE_MOVE + " m out of the castle footprint, else dropped" : "off (--keep-castle-lot-ruins): kept"}.`, ``,
      `Intersecting: **${castle.intersecting}**. Moved: **${castle.moved.length}** (${cnt(castle.moved)}). Dropped: **${castle.dropped.length}** (${cnt(castle.dropped)}).`, ``,
      `| file | id | prefab | from | to | moved m |`, `|---|---|---|---|---|---:|`,
      ...castle.moved.map(m => `| ${m.file} | ${m.id} | ${m.prefab} | (${m.from[0]}, ${m.from[1]}) | (${m.to[0]}, ${m.to[1]}) | ${m.d.toFixed(1)} |`),
      ...castle.dropped.map(m => `| ${m.file} | ${m.id} | ${m.prefab} | (${m.at[0]}, ${m.at[1]}) | dropped | – |`)].join("\n") + "\n");
    console.log(`castle lot ${dir}: ${castle.intersecting} buildings intersect the ${CASTLE} footprint; ${clearCastle ? `moved ${castle.moved.length}, dropped ${castle.dropped.length}` : "kept (--keep-castle-lot-ruins)"} -> castle-lot-check.md`);
  }
  const first = pass(dir, treeDir, true), again = pass(dir, treeDir, false);
  const byBuilding = {}; for (const b of first.buildings) byBuilding[b.prefab] = (byBuilding[b.prefab] ?? 0) + 1;
  const lines = [`# Objects inside building footprints (rounds 49–50)`, ``,
    `Buildings (${first.buildings.length}): ${Object.entries(byBuilding).map(([k, v]) => `${k} ${v}`).join(", ")}. Footprint = geometry bounds x/z from building-bounds.json (GET /prefabs/info), turned with the object's rotation; the dam plant (spawned by the dam course) at its live A-v22 centre with the circle through its corners.`,
    ``, `Removed from the masked layers: **${first.n}**. Check after removal: **${again.n}** left.`, ``, `| file | building | removed |`, `|---|---|---|`,
    ...Object.entries(first.report).filter(([k]) => k !== "moved").map(([k, v]) => { const [f, b] = k.split(" | "); return `| ${f} | ${b} | ${Object.entries(v).map(([p, c]) => `${p} ${c}`).join(", ")} |`; })];
  const mv = first.report.moved ?? [];
  lines.push("", `Moved, not dropped (round 50: the fort's standing stones, to the nearest point outside every footprint + ${MOVE_MARGIN} m, at most ${MOVE_MAX} m from the CS1 position, yaw kept): **${mv.length}**`, "",
    "| stone | prefab | was inside | from | to | moved m |", "|---|---|---|---|---|---:|",
    ...mv.map(m => `| ${m.id} | ${m.prefab} | ${m.building} | (${m.from[0]}, ${m.from[1]}) | (${m.to[0]}, ${m.to[1]}) | ${m.d.toFixed(2)} |`));
  writeFileSync(join(dir, "footprints-check.md"), lines.join("\n") + "\n");
  writeFileSync(join(dir, "footprints-check.json"), JSON.stringify({ removed: first.n, left: again.n, buildings: first.buildings.map(b => ({ prefab: b.prefab, x: b.x, z: b.z, rotation: b.rotation, file: b.file })), report: first.report }, null, 1));
  console.log(`building footprints ${dir}: ${first.buildings.length} buildings, ${first.n} objects removed, ${(first.report.moved ?? []).length} moved, ${again.n} left -> ${join(dir, "footprints-check.md")}`);
}
