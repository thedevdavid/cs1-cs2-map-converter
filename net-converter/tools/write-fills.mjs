// Writes out/import-retry.json: fill edges between live dead ends, cut from the imported body's own courses.
// node tools/write-fills.mjs <import-body.json> <live-graph.json> <out.json> <edgeId:liveNodeA:liveNodeB | edgeId> ...
// "edgeId" alone retries the whole course, binding its ends to live nodes within 1 m;
// "edgeId:A:B" builds the part of that course between live nodes A and B (entity indices), bound to them.
import { readFileSync, writeFileSync } from "node:fs";
const [bodyPath, livePath, outPath, ...specs] = process.argv.slice(2);
const body = JSON.parse(readFileSync(bodyPath, "utf8")), live = JSON.parse(readFileSync(livePath, "utf8"));
const N = new Map(body.nodes.map(n => [n.id, n])), E = new Map(body.edges.map(e => [e.id, e]));
const L = new Map(live.nodes.map(n => [n.node.index, n]));
const lerp = (a, b, t) => a.map((v, i) => v + (b[i] - v) * t);
const evalB = (P, t) => { const ab = lerp(P[0], P[1], t), bc = lerp(P[1], P[2], t), cd = lerp(P[2], P[3], t), abc = lerp(ab, bc, t), bcd = lerp(bc, cd, t); return [lerp(abc, bcd, t), [P[0], ab, abc, lerp(abc, bcd, t)], [lerp(abc, bcd, t), bcd, cd, P[3]]]; };
const sub = (P, t0, t1) => { let R = t0 > 0 ? evalB(P, t0)[2] : P; if (t1 < 1) R = evalB(R, (t1 - t0) / (1 - t0))[1]; return R; };
const closestT = (P, x, z) => { let best = 0, bd = Infinity; for (let i = 0; i <= 400; i++) { const p = evalB(P, i / 400)[0]; const d = Math.hypot(p[0] - x, p[2] - z); if (d < bd) { bd = d; best = i / 400; } } return [best, bd]; };
const bindNear = (x, z) => live.nodes.filter(l => Math.hypot(l.position.x - x, l.position.z - z) < 1).sort((a, b) => Math.hypot(a.position.x - x, a.position.z - z) - Math.hypot(b.position.x - x, b.position.z - z))[0];

const nodes = new Map(), edges = [];
const liveNode = l => { const id = `live${l.node.index}`; nodes.set(id, { id, x: l.position.x, y: l.position.y, z: l.position.z, entity: { index: l.node.index, version: l.node.version } }); return id; };
for (const spec of specs) {
  const [eid, la, lb] = spec.split(":");
  const e = E.get(eid); if (!e) throw new Error(`no edge ${eid}`);
  const a = N.get(e.a), b = N.get(e.b);
  const P = [[a.x, a.y, a.z], e.control[0], e.control[1], [b.x, b.y, b.z]];
  if (la) {
    const A = L.get(+la), B = L.get(+lb);
    const [t0, d0] = closestT(P, A.position.x, A.position.z), [t1, d1] = closestT(P, B.position.x, B.position.z);
    const S = sub(P, Math.min(t0, t1), Math.max(t0, t1));
    edges.push({ id: `fill_${eid}`, a: liveNode(t0 < t1 ? A : B), b: liveNode(t0 < t1 ? B : A), prefab: e.prefab, control: [S[1], S[2]].map(p => p.map(v => Math.round(v * 100) / 100)),
      _note: `part of ${eid} between live nodes ${la} and ${lb} (off-course ${d0.toFixed(1)} / ${d1.toFixed(1)} m)` });
  } else {
    const ids = [e.a, e.b].map(k => { const n = N.get(k), l = bindNear(n.x, n.z); if (l) return liveNode(l); nodes.set(k, { ...n }); return k; });
    edges.push({ ...e, a: ids[0], b: ids[1] });
  }
}
writeFileSync(outPath, JSON.stringify({ name: "Dakota Falls fills (dakota-v3)", _comment: "Fill edges for live gaps; nodes with entity attach to the live network.", nodes: [...nodes.values()], edges, options: body.options }, null, 2));
console.log(`${edges.length} edges, ${[...nodes.values()].filter(n => n.entity).length} bound nodes, ${[...nodes.values()].filter(n => !n.entity).length} new`);
console.log(edges.map(e => `${e.id} ${e.prefab} ${e.a} -> ${e.b} ${e._note ?? ""}`).join("\n"));
