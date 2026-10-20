// Road QA over the import set (import-all + import-bridges + import-bridges-fixed + import-gaps) and the last live graph.
// node tools/road-qa.mjs [out/x1] [--json out.json] [--label before|after]
// Reports, with coordinates:
//   dead ends (an edge end that is neither a border node nor a junction), genuine (CS1 had a dead end there) or pipeline;
//   mid-air stub pairs (two elevated dead ends within 60 m whose courses run the same way);
//   torn splits (two courses of different lane counts leaving one node < 5° apart and overlapping for more than 10 m);
//   isolated components shorter than 100 m; ramp dead ends within 25 m of a mainline course (should be junctions).
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (n, d) => { const i = args.indexOf(n); return i >= 0 ? args.splice(i, 2)[1] : d; };
const jsonOut = opt("--json", null), label = opt("--label", "now");
const dir = args[0] ?? "out/x1";
const J = f => JSON.parse(readFileSync(f, "utf8"));
const HALF = 7168;

// ---- graph from the import files (bridge files reuse the network's node ids at shared ends; replaced edges are dropped)
const nodes = new Map(), edges = [];
const replaced = new Set();
for (const f of ["import-bridges.json", "import-bridges-fixed.json"]) if (existsSync(join(dir, f))) for (const r of J(join(dir, f)).replaces ?? []) replaced.add(r);
for (const f of ["import-all.json", "import-bridges.json", "import-bridges-fixed.json", "import-gaps.json"]) {
  if (!existsSync(join(dir, f))) continue;
  const k = J(join(dir, f));
  for (const n of k.nodes) if (!nodes.has(n.id)) nodes.set(n.id, { ...n, file: f });
  for (const e of k.edges) if (!(f === "import-all.json" && replaced.has(e.id))) edges.push({ ...e, file: f });
}
const halfW = p => /4 lanes/.test(p) ? 10 : /3 lanes/.test(p) ? 8 : /2 lanes/.test(p) ? 6 : /1 lane/.test(p) ? 4 : /Train/.test(p) ? 6 : /Pavement Path/.test(p) ? 2 : /Gravel/.test(p) ? 4 : /Medium/.test(p) ? 8 : 6;
const lanes = p => (p.match(/(\d) lanes?/) ?? [])[1] ?? p;
const oneway = p => /Oneway/.test(p);
const at = new Map(); for (const e of edges) for (const k of [e.a, e.b]) (at.get(k) ?? at.set(k, []).get(k)).push(e);
const P = id => nodes.get(id);
const bez = (e, t) => { const a = P(e.a), b = P(e.b), c = e.control; if (!c) return [a.x + t * (b.x - a.x), a.z + t * (b.z - a.z)]; const u = 1 - t; return [u*u*u*a.x + 3*u*u*t*c[0][0] + 3*u*t*t*c[1][0] + t*t*t*b.x, u*u*u*a.z + 3*u*u*t*c[0][2] + 3*u*t*t*c[1][2] + t*t*t*b.z]; };
const len = e => { let L = 0, p = bez(e, 0); for (let k = 1; k <= 16; k++) { const q = bez(e, k / 16); L += Math.hypot(q[0] - p[0], q[1] - p[1]); p = q; } return L; };
// point at distance d from node id along e
const along = (e, id, d) => { const L = len(e); const t = Math.min(1, d / Math.max(L, 1e-6)); return bez(e, e.a === id ? t : 1 - t); };
const isBorder = n => n.border || Math.max(Math.abs(n.x), Math.abs(n.z)) >= HALF - 1;
const elevated = e => /elevated|bridge/.test(e.structure ?? "") || /Bridge|Viaduct/.test(e.prefab);
const fmt = (x, z) => `(${Math.round(x)}, ${Math.round(z)})`;

// ---- CS1: node flags and degree (all CS1 segments, imported or not)
const dumpPath = (() => { const d = join(process.env.LOCALAPPDATA ?? "", "DakotaExport"); return existsSync(join(d, "2972879554_Dakota_Falls-20261019-063123.json")) ? join(d, "2972879554_Dakota_Falls-20261019-063123.json") : null; })();
const cs1 = new Map(), cs1deg = new Map(), cs1segs = new Map();
if (dumpPath) {
  const d = J(dumpPath);
  for (const n of d.nodes) cs1.set(n.id, n);
  for (const s of d.segments) { for (const k of [s.startNode, s.endNode]) cs1deg.set(k, (cs1deg.get(k) ?? 0) + 1); cs1segs.set(s.id, s); }
}
const cs1NodeOf = n => { const m = /^n(\d+)$/.exec(n.id); return m ? cs1.get(+m[1]) : null; };

// ---- 1. dead ends
const dead = [];
for (const [id, es] of at) {
  if (es.length !== 1) continue;
  const n = P(id); if (isBorder(n)) continue;
  const c = cs1NodeOf(n);
  const genuine = c ? (cs1deg.get(c.id) ?? 0) <= 1 || c.flags?.includes("End") : false;
  dead.push({ id, x: n.x, z: n.z, y: n.y, prefab: es[0].prefab, edge: es[0].id, elevated: elevated(es[0]), genuine, cs1: c?.id ?? null, cs1deg: c ? cs1deg.get(c.id) ?? 0 : null });
}
// ---- 2. mid-air stub pairs
const stubs = [];
const dirOut = d => { const e = at.get(d.id)[0]; const q = along(e, d.id, 15); const L = Math.hypot(d.x - q[0], d.z - q[1]) || 1; return [(d.x - q[0]) / L, (d.z - q[1]) / L]; };
const elevDead = dead.filter(d => d.elevated && /Highway/.test(d.prefab));
for (let i = 0; i < elevDead.length; i++) for (let j = i + 1; j < elevDead.length; j++) {
  const a = elevDead[i], b = elevDead[j]; const dist = Math.hypot(a.x - b.x, a.z - b.z); if (dist > 60) continue;
  const da = dirOut(a), db = dirOut(b); const dot = da[0] * db[0] + da[1] * db[1];
  if (dot > 0.9) stubs.push({ a, b, dist });
}
// ---- 3. torn splits
const torn = [];
for (const [id, es] of at) {
  if (es.length < 2) continue;
  const n = P(id);
  for (let i = 0; i < es.length; i++) for (let j = i + 1; j < es.length; j++) {
    const e1 = es[i], e2 = es[j]; if (lanes(e1.prefab) === lanes(e2.prefab) || !/Highway/.test(e1.prefab) || !/Highway/.test(e2.prefab)) continue;
    const p1 = along(e1, id, 12), p2 = along(e2, id, 12);
    const v1 = [p1[0] - n.x, p1[1] - n.z], v2 = [p2[0] - n.x, p2[1] - n.z];
    const ang = Math.acos(Math.max(-1, Math.min(1, (v1[0] * v2[0] + v1[1] * v2[1]) / (Math.hypot(...v1) * Math.hypot(...v2) || 1)))) * 180 / Math.PI;
    if (ang >= (process.env.QA_ANGLE ? +process.env.QA_ANGLE : 5)) continue;
    // overlap: how far the two courses stay within their half-widths of each other
    let over = 0; for (let d = 2; d <= 60; d += 2) { const q1 = along(e1, id, d), q2 = along(e2, id, d); if (Math.hypot(q1[0] - q2[0], q1[1] - q2[1]) < (halfW(e1.prefab) + halfW(e2.prefab)) / 2) over = d; else break; }
    if (over > 10) torn.push({ id, x: n.x, z: n.z, e1: e1.id, p1: e1.prefab, e2: e2.id, p2: e2.prefab, ang, over });
  }
}
// ---- 4. isolated short components
const seen = new Set(), isolated = [];
for (const e0 of edges) {
  if (seen.has(e0)) continue;
  const comp = [], q = [e0]; seen.add(e0);
  while (q.length) { const e = q.pop(); comp.push(e); for (const k of [e.a, e.b]) for (const o of at.get(k)) if (!seen.has(o)) { seen.add(o); q.push(o); } }
  const L = comp.reduce((s, e) => s + len(e), 0);
  if (L >= 100 || comp.some(e => isBorder(P(e.a)) || isBorder(P(e.b)))) continue;
  const c = bez(comp[0], 0.5);
  isolated.push({ x: c[0], z: c[1], length: L, edges: comp.map(e => e.id), prefab: comp[0].prefab });
}
// ---- 5. ramp dead ends near a mainline
const hwSamples = edges.filter(e => /Highway/.test(e.prefab)).map(e => ({ e, p: Array.from({ length: 17 }, (_, k) => bez(e, k / 16)) }));
const rampEnds = [];
for (const d of dead.filter(d => /Highway Oneway/.test(d.prefab))) {
  let best = null;
  for (const { e, p } of hwSamples) {
    if (e.a === d.id || e.b === d.id || e.id === d.edge) continue;
    for (const q of p) { const dd = Math.hypot(q[0] - d.x, q[1] - d.z); if (dd < 25 && (!best || dd < best.d)) best = { d: dd, e }; }
  }
  if (best) rampEnds.push({ ...d, near: best.e.id, nearPrefab: best.e.prefab, dist: best.d });
}
// ---- live graph: dead ends as seen in the last live map
let live = null;
if (existsSync(join(dir, "live-graph.json"))) {
  const g = J(join(dir, "live-graph.json")); const deg = new Map(); for (const s of g.segments) for (const k of [s.startNode, s.endNode]) deg.set(k, (deg.get(k) ?? 0) + 1);
  live = { pulled: g.pulledAtUtc, deadEnds: g.nodes.filter(n => deg.get(n.node.index) === 1 && Math.max(Math.abs(n.position.x), Math.abs(n.position.z)) < HALF - 60).length };
}
const res = {
  label, dir, counts: { deadEnds: dead.length, genuine: dead.filter(d => d.genuine).length, pipeline: dead.filter(d => !d.genuine).length, midAirPairs: stubs.length, tornSplits: torn.length, isolatedShort: isolated.length, rampEndsNearMainline: rampEnds.length, liveDeadEnds: live?.deadEnds ?? null },
  pipeline: dead.filter(d => !d.genuine).map(d => ({ at: fmt(d.x, d.z), prefab: d.prefab, edge: d.edge, elevated: d.elevated, cs1deg: d.cs1deg })),
  genuine: dead.filter(d => d.genuine).map(d => ({ at: fmt(d.x, d.z), prefab: d.prefab, edge: d.edge, elevated: d.elevated, cs1deg: d.cs1deg })),
  midAir: stubs.map(s => ({ a: fmt(s.a.x, s.a.z), b: fmt(s.b.x, s.b.z), dist: Math.round(s.dist), prefab: s.a.prefab, genuine: s.a.genuine && s.b.genuine })),
  torn: torn.map(t => ({ at: fmt(t.x, t.z), node: t.id, edges: `${t.e1} ${t.p1} / ${t.e2} ${t.p2}`, angle: +t.ang.toFixed(1), overlap: t.over })),
  isolated: isolated.map(i => ({ at: fmt(i.x, i.z), length: Math.round(i.length), prefab: i.prefab, edges: i.edges.join(",") })),
  rampEnds: rampEnds.map(r => ({ at: fmt(r.x, r.z), prefab: r.prefab, near: `${r.near} ${r.nearPrefab}`, dist: +r.dist.toFixed(1), genuine: r.genuine })),
  live,
};
if (jsonOut) writeFileSync(jsonOut, JSON.stringify(res, null, 1));
console.log(JSON.stringify(res.counts));
