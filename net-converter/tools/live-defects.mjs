// Live-network defects: dead ends facing each other (gaps), dead ends near a crossing with another layer, ramp tails
// beside a same-layer course. node tools/live-defects.mjs out/live-graph.json > out/live-defects.json
import { readFileSync } from "node:fs";
const g = JSON.parse(readFileSync(process.argv[2] ?? "out/live-graph.json", "utf8"));
const HALF = 7168, layer = p => p.includes("Train") ? "rail" : p.startsWith("Highway") ? "highways" : "roads";
const segsAt = new Map();
for (const s of g.segments) for (const k of [s.startNode, s.endNode]) (segsAt.get(k) ?? segsAt.set(k, []).get(k)).push(s);
const ends = g.nodes.filter(n => n.segmentsConnected === 1 && Math.max(Math.abs(n.position.x), Math.abs(n.position.z)) < HALF - 60 && segsAt.has(n.node.index));
const P = n => [n.position.x, n.position.z];
function distSeg(p, s) {
  const ax = s.start.x, az = s.start.z, dx = s.end.x - ax, dz = s.end.z - az, l2 = dx * dx + dz * dz;
  const t = l2 ? Math.max(0, Math.min(1, ((p[0] - ax) * dx + (p[1] - az) * dz) / l2)) : 0;
  return [Math.hypot(ax + t * dx - p[0], az + t * dz - p[1]), t];
}
const out = { gaps: [], nearCrossing: [], rampTails: [], deadEnds: ends.length };
const info = ends.map(n => {
  const s = segsAt.get(n.node.index)[0];
  const other = s.startNode === n.node.index ? s.end : s.start;
  const dir = [n.position.x - other.x, n.position.z - other.z], L = Math.hypot(...dir);
  return { n, s, dir: [dir[0] / L, dir[1] / L], layer: layer(s.prefab) };
});
const used = new Set();
for (const a of info) {
  let best = null;
  for (const b of info) {
    if (a === b || a.layer !== b.layer) continue;
    const v = [b.n.position.x - a.n.position.x, b.n.position.z - a.n.position.z], d = Math.hypot(...v);
    if (d > 250 || d < 1) continue;
    const ahead = (v[0] * a.dir[0] + v[1] * a.dir[1]) / d, back = -(v[0] * b.dir[0] + v[1] * b.dir[1]) / d;
    if (ahead > 0.8 && back > 0.8 && (!best || d < best.d)) best = { b, d };
  }
  if (best) {
    const key = [a.n.node.index, best.b.n.node.index].sort().join("-");
    if (!used.has(key)) { used.add(key); out.gaps.push({ a: a.n.node, b: best.b.n.node, prefabA: a.s.prefab, prefabB: best.b.s.prefab, gap: Math.round(best.d), x: Math.round(a.n.position.x), z: Math.round(a.n.position.z) }); }
  }
  // Crossing with another layer within 100 m ahead.
  let cross = null;
  for (const s of g.segments) {
    if (layer(s.prefab) === a.layer) continue;
    const [d] = distSeg(P(a.n), s);
    if (d < 100 && (!cross || d < cross.d)) cross = { d, prefab: s.prefab };
  }
  if (cross) out.nearCrossing.push({ node: a.n.node, prefab: a.s.prefab, other: cross.prefab, distance: Math.round(cross.d), x: Math.round(a.n.position.x), z: Math.round(a.n.position.z) });
  // Ramp tail: same-layer segment within 5-30 m, not connected.
  let tail = null;
  for (const s of g.segments) {
    if (layer(s.prefab) !== a.layer || s === a.s || s.startNode === a.n.node.index || s.endNode === a.n.node.index) continue;
    const [d, t] = distSeg(P(a.n), s);
    if (d <= 30 && (!tail || d < tail.d)) tail = { d, s, t };
  }
  if (tail) out.rampTails.push({ node: a.n.node, prefab: a.s.prefab, stubLength: a.s.length, beside: tail.s.prefab, besideSegment: tail.s.road, distance: Math.round(tail.d * 10) / 10, x: Math.round(a.n.position.x), z: Math.round(a.n.position.z) });
}
console.log(JSON.stringify(out, null, 1));
