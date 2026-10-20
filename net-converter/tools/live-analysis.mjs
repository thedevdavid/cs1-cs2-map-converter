// Live map vs import set, appended to <dir>/live-defects.md (run tools/defects.mjs first):
// lost segments (import edges with no live segment at their midpoint), highway dead ends classified, 4-lane split nodes.
// node tools/live-analysis.mjs [out/x1] [--export out/x1/live-graph-export.json] [--baseline out/live-graph.json]
import { existsSync, readFileSync, appendFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (n, d) => { const i = args.indexOf(n); return i >= 0 ? args.splice(i, 2)[1] : d; };
const exportPath = opt("--export", null), baselinePath = opt("--baseline", "out/live-graph.json");
const dir = args[0] ?? "out/x1";
const J = f => JSON.parse(readFileSync(f, "utf8"));
const imp = J(join(dir, "import-all.json")); const N = new Map(imp.nodes.map(n => [n.id, n]));
const deg = new Map(); for (const e of imp.edges) for (const k of [e.a, e.b]) deg.set(k, (deg.get(k) ?? 0) + 1);
const ex = J(exportPath ?? join(dir, "live-graph-export.json"));
const bez = (c, t) => { const u = 1 - t; return [u * u * u * c[0][0] + 3 * u * u * t * c[1][0] + 3 * u * t * t * c[2][0] + t * t * t * c[3][0], u * u * u * c[0][2] + 3 * u * u * t * c[1][2] + 3 * u * t * t * c[2][2] + t * t * t * c[3][2]]; };
const H = new Map(); const K = (x, z) => `${Math.floor(x / 30)},${Math.floor(z / 30)}`;
for (const s of ex.segments) for (let k = 0; k <= 24; k++) { const [x, z] = bez(s.curve, k / 24); (H.get(K(x, z)) ?? H.set(K(x, z), []).get(K(x, z))).push({ x, z, p: s.prefab }); }
const near = (p, pref) => { let b = 1e9; const cx = Math.floor(p[0] / 30), cz = Math.floor(p[1] / 30); for (let dx = -1; dx <= 1; dx++) for (let dz = -1; dz <= 1; dz++) for (const q of H.get(`${cx + dx},${cz + dz}`) ?? []) if (!pref || q.p === pref) b = Math.min(b, Math.hypot(q.x - p[0], q.z - p[1])); return b; };
const mid = e => { const a = N.get(e.a), b = N.get(e.b), c = e.control; return bez([[a.x, 0, a.z], c[0], c[1], [b.x, 0, b.z]], 0.5); };
const lost = imp.edges.filter(e => near(mid(e), e.prefab) >= 6 && near(mid(e), null) >= 6).map(e => ({ id: e.id, prefab: e.prefab, p: mid(e) }));
let inBase = null;
if (existsSync(baselinePath)) {
  const b = J(baselinePath);
  const sd = (p, s) => { const ax = s.start.x, az = s.start.z, dx = s.end.x - ax, dz = s.end.z - az, l2 = dx * dx + dz * dz || 1; const t = Math.max(0, Math.min(1, ((p[0] - ax) * dx + (p[1] - az) * dz) / l2)); return Math.hypot(ax + t * dx - p[0], az + t * dz - p[1]); };
  inBase = lost.filter(l => b.segments.some(s => sd(l.p, s) < 10)).length;
}
writeFileSync(join(dir, "live-lost-edges.json"), JSON.stringify(lost.map(l => ({ id: l.id, prefab: l.prefab, x: Math.round(l.p[0]), z: Math.round(l.p[1]) }))));
const byPrefab = Object.entries(lost.reduce((m, l) => (m[l.prefab] = (m[l.prefab] ?? 0) + 1, m), {})).map(([k, v]) => `${k} ${v}`).join(", ");
const clusters = Object.entries(lost.reduce((m, l) => { const k = `(${Math.round(l.p[0] / 1000)} km, ${Math.round(l.p[1] / 1000)} km)`; m[k] = (m[k] ?? 0) + 1; return m; }, {})).sort((a, b) => b[1] - a[1]).slice(0, 10);
// Highway dead ends in the live graph.
const at = new Map(); for (const s of ex.segments) for (const k of [s.a, s.b]) (at.get(k) ?? at.set(k, []).get(k)).push(s);
const pos = new Map(ex.nodes.map(n => [n.id, n]));
const rows = [];
for (const [id, segs] of at) {
  if (segs.length !== 1 || !/^Highway/.test(segs[0].prefab)) continue;
  const n = pos.get(id); const x = n.position?.x ?? n.x, z = n.position?.z ?? n.z, y = n.position?.y ?? n.y;
  if (Math.max(Math.abs(x), Math.abs(z)) > 7108) continue;
  const m = imp.nodes.map(q => ({ q, d: Math.hypot(q.x - x, q.z - z) })).sort((a, b) => a.d - b.d)[0];
  const d = m.d < 5 ? deg.get(m.q.id) : 0;
  const lostHere = imp.edges.filter(e => (e.a === m.q.id || e.b === m.q.id) && lost.some(l => l.id === e.id)).map(e => e.id);
  rows.push(`| (${x.toFixed(0)}, ${z.toFixed(0)}) | ${y.toFixed(1)} | ${segs[0].prefab} | ${m.d < 5 ? m.q.id : "-"} | ${d ?? 0} | ${d >= 2 ? `lost continuation: ${lostHere.join(", ") || "(merged elsewhere)"}` : d === 1 ? "CS1 dead end (left out by the converter from round 10 if a one-way stub)" : "no import node: game-made"} |`);
}
// 4-lane split nodes (a 4-lane meeting two or more other highway segments).
const splits = [];
for (const [id, segs] of at) {
  if (segs.length < 3 || !segs.some(s => s.prefab === "Highway Oneway - 4 lanes")) continue;
  const n = pos.get(id); splits.push(`(${(n.position?.x ?? n.x).toFixed(0)}, ${(n.position?.z ?? n.z).toFixed(0)}): ${segs.map(s => s.prefab.replace("Highway Oneway - ", "")).join(" + ")}`);
}
const md = [``, `## A-v4 against the import set (tools/live-analysis.mjs, ${new Date().toISOString().slice(0, 10)})`, ``,
  `Live: ${ex.segments.length} segments (${ex.exportedAt ?? ""}). Import: ${imp.edges.length} edges in import-all.json.`, ``,
  `### Lost segments: ${lost.length}`, ``,
  `Import edges with no live segment of any prefab within 6 m of their midpoint (list: live-lost-edges.json). ${inBase != null ? `${inBase} of them were live in Run A (${baselinePath}), so they were lost after the import, not refused by it: the run reported only 9 failed edges, and no imported object, network or surface lies near most of them (game side, or an edit after the import; tools/import-run.mjs now exports the graph after the network steps and at the end to catch the step).` : ""}`, ``,
  `By prefab: ${byPrefab}. Largest clusters: ${clusters.map(([k, v]) => `${k} ${v}`).join(", ")}.`, ``,
  `### Highway dead ends: ${rows.length}`, ``,
  "Stubs ending in mid-air with the dead-end warning and ramp ends with a square end cap are these ends.", ``,
  `| at | y | prefab | import node | import degree | cause |`, `|---|---:|---|---|---:|---|`, ...rows.sort(), ``,
  `### 4-lane split nodes: ${splits.length}`, ``,
  `Candidates for the broken node on the 4-lane split ("Sheffield Highway"; the graph has no road names, so the owner's screenshot position is needed to pick one): ${splits.join("; ")}.`, ``];
appendFileSync(join(dir, "live-defects.md"), md.join("\n"));
console.log(`lost ${lost.length}${inBase != null ? ` (${inBase} live in Run A)` : ""}, highway dead ends ${rows.length}, 4-lane splits ${splits.length} -> ${join(dir, "live-defects.md")}`);
