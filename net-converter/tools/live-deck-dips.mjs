// Game-inserted nodes (live, degree 2, no node of ours within 2 m) lying on our courses: how far is their height
// from our deck (linear between our end heights)? node tools/live-deck-dips.mjs out/live-graph.json out/import-all.json
import { readFileSync } from "node:fs";
const g = JSON.parse(readFileSync(process.argv[2], "utf8")), d = JSON.parse(readFileSync(process.argv[3], "utf8"));
const N = new Map(d.nodes.map(n => [n.id, n]));
const ev = (P, t) => { const u = 1 - t; return [0, 1, 2].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]); };
const grid = new Map();
for (const n of d.nodes) { const k = `${Math.floor(n.x / 4)},${Math.floor(n.z / 4)}`; (grid.get(k) ?? grid.set(k, []).get(k)).push(n); }
const hasOurs = (x, z) => { for (let i = -1; i <= 1; i++) for (let j = -1; j <= 1; j++) for (const n of grid.get(`${Math.floor(x / 4) + i},${Math.floor(z / 4) + j}`) ?? []) if (Math.hypot(n.x - x, n.z - z) < 2) return true; return false; };
const egrid = new Map();
const polys = d.edges.map(e => {
  const a = N.get(e.a), b = N.get(e.b);
  const P = [[a.x, a.y, a.z], e.control[0], e.control[1], [b.x, b.y, b.z]];
  return { e, a, b, pts: Array.from({ length: 41 }, (_, i) => { const t = i / 40, p = ev(P, t); return { t, x: p[0], z: p[2], y: a.y + (b.y - a.y) * t }; }) };
});
for (const p of polys) for (const k of new Set(p.pts.map(q => `${Math.floor(q.x / 64)},${Math.floor(q.z / 64)}`))) (egrid.get(k) ?? egrid.set(k, []).get(k)).push(p);
const rows = [];
for (const n of g.nodes) {
  if (n.segmentsConnected !== 2) continue;
  const { x, z, y } = n.position;
  if (hasOurs(x, z)) continue;
  let best = null;
  for (const p of egrid.get(`${Math.floor(x / 64)},${Math.floor(z / 64)}`) ?? []) for (const q of p.pts) { const dd = Math.hypot(q.x - x, q.z - z); if (dd < 3 && (!best || dd < best.dd)) best = { dd, p, q }; }
  if (!best) continue;
  const raised = !("elevation" in best.p.a) || !("elevation" in best.p.b);
  rows.push({ dev: y - best.q.y, raised, id: best.p.e.id, prefab: best.p.e.prefab, x, z, t: best.q.t, len: Math.hypot(best.p.a.x - best.p.b.x, best.p.a.z - best.p.b.z) });
}
const r = rows.filter(r => r.raised);
console.log(`game-inserted degree-2 nodes on our courses: ${rows.length}; on raised courses: ${r.length}; |dev| > 2 m: ${r.filter(x => Math.abs(x.dev) > 2).length}; > 5 m: ${r.filter(x => Math.abs(x.dev) > 5).length}`);
for (const x of r.filter(x => Math.abs(x.dev) > 2).sort((a, b) => a.dev - b.dev))
  console.log(`${x.id} ${x.prefab} dev ${x.dev.toFixed(1)} m at t=${x.t.toFixed(2)} of ${x.len.toFixed(0)} m, (${x.x.toFixed(0)}, ${x.z.toFixed(0)})`);
