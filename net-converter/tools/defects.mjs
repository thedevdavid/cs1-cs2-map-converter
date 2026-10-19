// Defect finder over a live graph: dead ends, facing gap pairs, ramp tails beside segments, wavy elevated chains,
// and wall ends sitting on another wall node without being joined. Writes live-defects.md next to the graph.
// node tools/defects.mjs [out/x1/live-graph.json]
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
const graphPath = process.argv[2] ?? "out/live-graph.json";
const g = JSON.parse(readFileSync(graphPath, "utf8"));
// Walls, ship paths, power and decor are open-ended by nature: no dead-end, gap or tail checks (layer column of mapping.tsv).
const separate = new Map(readFileSync(new URL("../mapping.tsv", import.meta.url), "utf8").split(/\r?\n/)
  .filter(l => l && !l.startsWith("#")).map(l => l.split("\t")).filter(c => ["walls", "shippaths", "power", "decor"].includes(c[2])).map(c => [c[1], c[2]]));
const segs = g.segments, nodes = g.nodes;
const byId = new Map(nodes.map(n => [n.node.index, n]));
const adj = new Map();
for (const s of segs) { for (const [a, b] of [[s.startNode, s.endNode], [s.endNode, s.startNode]]) { if (!adj.has(a)) adj.set(a, []); adj.get(a).push({ s, other: b }); } }
const BORDER = 7168, INSIDE = 60;
const isRoad = p => !/Train Track/.test(p);
const cls = p => /Highway/.test(p) ? "hw" : /Train/.test(p) ? "rail" : "road";
const P = n => n.position;
const dist = (a, b) => Math.hypot(a.x - b.x, a.z - b.z);
// 1. dead ends inside the border
const isSeparate = n => (adj.get(n.node.index) ?? []).some(e => separate.has(e.s.prefab));
const dead = nodes.filter(n => (adj.get(n.node.index)?.length ?? 0) === 1 && !isSeparate(n) && Math.abs(P(n).x) < BORDER - INSIDE && Math.abs(P(n).z) < BORDER - INSIDE);
function dirOf(n) { const e = adj.get(n.node.index)[0]; const o = byId.get(e.other); if (!o) return null; const dx = P(n).x - P(o).x, dz = P(n).z - P(o).z, L = Math.hypot(dx, dz) || 1; return { x: dx / L, z: dz / L, prefab: e.s.prefab }; }
// 2. facing gap pairs
const pairs = []; const used = new Set();
for (const a of dead) { const da = dirOf(a); if (!da) continue; let best = null;
  for (const b of dead) { if (a === b) continue; const db = dirOf(b); if (!db || cls(da.prefab) !== cls(db.prefab)) continue; const d = dist(P(a), P(b)); if (d > 300 || d < 1) continue;
    const vx = (P(b).x - P(a).x) / d, vz = (P(b).z - P(a).z) / d; const ahead = da.x * vx + da.z * vz; const facing = -(db.x * vx + db.z * vz);
    if (ahead > 0.6 && facing > 0.6) { const score = d * (2 - ahead - facing); if (!best || score < best.score) best = { b, d, score }; } }
  if (best && !used.has(a.node.index) && !used.has(best.b.node.index)) { used.add(a.node.index); used.add(best.b.node.index); pairs.push({ a, b: best.b, d: best.d, prefabA: da.prefab, prefabB: dirOf(best.b).prefab }); } }
// 3. ramp tails: dead end within 30 m of a non-adjacent segment chord of same class
function pointSegDist(p, s) { const ax = s.start.x, az = s.start.z, bx = s.end.x, bz = s.end.z; const dx = bx - ax, dz = bz - az; const L2 = dx * dx + dz * dz || 1; let t = ((p.x - ax) * dx + (p.z - az) * dz) / L2; t = Math.max(0, Math.min(1, t)); return Math.hypot(p.x - (ax + t * dx), p.z - (az + t * dz)); }
const tails = [];
for (const n of dead) { const e = adj.get(n.node.index)[0]; let best = null; for (const s of segs) { if (s === e.s) continue; if (cls(s.prefab) !== cls(e.s.prefab)) continue; if (s.startNode === n.node.index || s.endNode === n.node.index) continue; const d = pointSegDist(P(n), s); if (d < 30 && (!best || d < best.d)) best = { s, d }; } if (best) tails.push({ n, d: best.d, prefab: e.s.prefab, near: best.s.prefab }); }
const tailIds = new Set(tails.map(t => t.n.node.index)); const pairIds = used;
// 4. wavy chains: walk degree-2 chains, measure |second difference of y| relative to spacing
const visited = new Set(); const wavy = [];
for (const n of nodes) { const deg = adj.get(n.node.index)?.length ?? 0; if (deg === 2 || visited.has(n.node.index)) continue; // start from junctions/ends
  for (const e of adj.get(n.node.index) ?? []) { let prev = n, cur = byId.get(e.other), chain = [n]; let guard = 0; while (cur && (adj.get(cur.node.index)?.length ?? 0) === 2 && guard++ < 500) { chain.push(cur); visited.add(cur.node.index); const nx = adj.get(cur.node.index).find(x => x.other !== prev.node.index); if (!nx) break; prev = cur; cur = byId.get(nx.other); } if (cur) chain.push(cur);
    if (chain.length < 4) continue; let worst = 0, at = null, flips = 0, lastSign = 0;
    for (let i = 1; i < chain.length - 1; i++) { const y0 = P(chain[i - 1]).y, y1 = P(chain[i]).y, y2 = P(chain[i + 1]).y; const d1 = dist(P(chain[i - 1]), P(chain[i])), d2 = dist(P(chain[i]), P(chain[i + 1])); const g1 = (y1 - y0) / Math.max(d1, 1), g2 = (y2 - y1) / Math.max(d2, 1); const kink = Math.abs(g2 - g1); const sign = Math.sign(g2 - g1); if (sign && lastSign && sign !== lastSign) flips++; if (sign) lastSign = sign; if (kink > worst) { worst = kink; at = chain[i]; } }
    if (worst > 0.06 && flips >= 2) wavy.push({ len: chain.length, worst, flips, at, prefab: e.s.prefab }); } }
// 5. wall chains: a wall end (one wall segment) within 1 m of another wall node it is not joined to. The 0.0 m pairs
//    may be orphans of failed wall edges.
const wallNodes = nodes.filter(n => (adj.get(n.node.index) ?? []).some(e => separate.get(e.s.prefab) === "walls"));
const wallEnds = wallNodes.filter(n => adj.get(n.node.index).length === 1);
const unjoined = [];
for (const a of wallEnds) {
  let best = null;
  for (const s of segs) {
    if (separate.get(s.prefab) !== "walls" || s.startNode === a.node.index || s.endNode === a.node.index) continue;
    const dx = s.end.x - s.start.x, dz = s.end.z - s.start.z, L2 = dx * dx + dz * dz || 1;
    const t = Math.max(0, Math.min(1, ((P(a).x - s.start.x) * dx + (P(a).z - s.start.z) * dz) / L2));
    const d = Math.hypot(s.start.x + t * dx - P(a).x, s.start.z + t * dz - P(a).z);
    if (d <= 1 && (!best || d < best.d)) best = { s, d, t, y: s.start.y + t * (s.end.y - s.start.y) };
  }
  if (!best) continue;
  const len = Math.sqrt((best.s.end.x - best.s.start.x) ** 2 + (best.s.end.z - best.s.start.z) ** 2);
  const where = best.t * len < 1 || (1 - best.t) * len < 1 ? "on its end node" : `mid-segment (${(best.t * 100).toFixed(0)} %)`;
  unjoined.push({ a, d: best.d, dy: P(a).y - best.y, prefabA: adj.get(a.node.index)[0].s.prefab, prefabB: best.s.prefab, where });
}
const wallDead = wallEnds.filter(n => Math.abs(P(n).x) < BORDER - INSIDE && Math.abs(P(n).z) < BORDER - INSIDE).length;
const fmt = p => `(${p.x.toFixed(0)}, ${p.z.toFixed(0)})`;
let out = `# Live defects (${segs.length} segments, ${nodes.length} nodes)\n\nWalls, ship paths, power and decor (${[...separate.keys()].join(", ")}) are open-ended by nature and left out of the dead-end, gap and tail checks; ${wallDead} wall ends inside the border.\n\n`;
out += `## Dead ends inside the border: ${dead.length}\n\n`;
out += `## Facing gap pairs: ${pairs.length}\n| gap m | prefab | end A | end B |\n|---:|---|---|---|\n` + pairs.sort((p, q) => p.d - q.d).map(p => `| ${p.d.toFixed(0)} | ${p.prefabA} / ${p.prefabB} | ${fmt(P(p.a))} | ${fmt(P(p.b))} |`).join("\n") + "\n\n";
out += `## Ramp tails (dead end within 30 m of an unconnected same-class segment): ${tails.filter(t => !pairIds.has(t.n.node.index)).length}\n| dist m | stub prefab | near | at |\n|---:|---|---|---|\n` + tails.filter(t => !pairIds.has(t.n.node.index)).sort((a, b) => a.d - b.d).map(t => `| ${t.d.toFixed(1)} | ${t.prefab} | ${t.near} | ${fmt(P(t.n))} |`).join("\n") + "\n\n";
const other = dead.filter(n => !pairIds.has(n.node.index) && !tailIds.has(n.node.index));
out += `## Other dead ends: ${other.length}\n| prefab | at |\n|---|---|\n` + other.map(n => `| ${adj.get(n.node.index)[0].s.prefab} | ${fmt(P(n))} |`).join("\n") + "\n\n";
out += `## Wavy chains (kink > 6 % with ≥ 2 sign flips): ${wavy.length}\n| nodes | worst kink | flips | prefab | worst at |\n|---:|---:|---:|---|---|\n` + wavy.sort((a, b) => b.worst - a.worst).slice(0, 25).map(w => `| ${w.len} | ${(w.worst * 100).toFixed(0)} % | ${w.flips} | ${w.prefab} | ${fmt(P(w.at))} y=${P(w.at).y.toFixed(0)} |`).join("\n") + "\n";
out += `\n## Wall ends on another wall, not joined (≤ 1 m): ${unjoined.length}\nThe 0.0 m pairs may be orphans left by failed wall edges.\n| dist m | Δy m | end prefab | other wall | where | at |\n|---:|---:|---|---|---|\n` + unjoined.sort((p, q) => p.d - q.d).map(u => `| ${u.d.toFixed(1)} | ${u.dy.toFixed(1)} | ${u.prefabA} | ${u.prefabB} | ${u.where} | ${fmt(P(u.a))} |`).join("\n") + "\n";
writeFileSync(join(dirname(graphPath), "live-defects.md"), out); console.log(out.slice(0, 6000));
