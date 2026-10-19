// Classifies failed import edges by what they collide with in the same import body.
// node tools/classify-failures.mjs <report.json> <import.json> [out.md]
// For each failed edge, every other edge whose road surface touches it (centreline distance < sum of half widths, approximate
// CS2 widths below) is a candidate; per candidate:
//   shared node + small angle  -> "parallel diverge"
//   overlaps > 70 % of the shorter course, same prefab -> "duplicate"
//   overlaps > 20 m, no shared node -> "tight parallel"
//   short contact (a crossing) -> "crossing", with the vertical separation at the crossing
import { readFileSync, writeFileSync } from "node:fs";

const [reportPath, importPath, outPath] = process.argv.slice(2);
const R = JSON.parse(readFileSync(reportPath, "utf8"));
const d = JSON.parse(readFileSync(importPath, "utf8"));
const N = new Map(d.nodes.map(n => [n.id, n]));
const E = new Map(d.edges.map(e => [e.id, e]));

// Approximate CS2 road widths (m), for contact only.
const WIDTH = { "Highway Oneway - 1 lane": 8, "Highway Oneway - 2 lanes": 12, "Highway Oneway - 3 lanes": 16, "Highway Oneway - 4 lanes": 19,
  "Highway Oneway - 5 lanes": 23, "Highway Twoway - 2 lanes": 20, "Small Road": 12, "Medium Road": 20, "Large Road": 28, "Gravel Road": 8,
  "Double Train Track": 10, "Twoway Train Track": 6, "Alley": 8, "Pedestrian Street": 12 };
const half = p => (WIDTH[p] ?? 12) / 2;
const ev = (P, t) => { const u = 1 - t; return [0, 1, 2].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]); };
// Deck height = node absolute height (y) interpolated; x/z from the Bézier.
function samples(e, step = 2) {
  const a = N.get(e.a), b = N.get(e.b);
  const P = [[a.x, a.y, a.z], e.control[0], e.control[1], [b.x, b.y, b.z]];
  const chord = Math.hypot(a.x - b.x, a.z - b.z), n = Math.max(2, Math.ceil(chord / step));
  return Array.from({ length: n + 1 }, (_, i) => { const t = i / n, p = ev(P, t); return [p[0], a.y + (b.y - a.y) * t, p[2]]; });
}
const S = new Map(d.edges.map(e => [e.id, samples(e)]));
const cell = 32, grid = new Map();
for (const [id, pts] of S) for (const k of new Set(pts.map(p => `${Math.floor(p[0] / cell)},${Math.floor(p[2] / cell)}`))) (grid.get(k) ?? grid.set(k, []).get(k)).push(id);

function nearest(p, pts) {
  let best = Infinity, y = 0;
  for (let i = 0; i + 1 < pts.length; i++) {
    const [ax, ay, az] = pts[i], [bx, by, bz] = pts[i + 1], dx = bx - ax, dz = bz - az, l2 = dx * dx + dz * dz;
    const t = l2 > 0 ? Math.max(0, Math.min(1, ((p[0] - ax) * dx + (p[2] - az) * dz) / l2)) : 0;
    const d2 = (ax + t * dx - p[0]) ** 2 + (az + t * dz - p[2]) ** 2;
    if (d2 < best) { best = d2; y = ay + t * (by - ay); }
  }
  return [Math.sqrt(best), y];
}
const angleAt = (e, n) => { const p = N.get(n); const c = e.a === n ? e.control[0] : e.control[1]; return Math.atan2(c[2] - p.z, c[0] - p.x); };
const adiff = (a, b) => { let x = (b - a) * 180 / Math.PI; while (x > 180) x -= 360; while (x <= -180) x += 360; return Math.abs(x); };

const rows = [], counts = {};
for (const r of R.results) {
  const e = E.get(r.id); if (!e) continue;
  const reasons = (r.error.match(/\(([^)]+)\)/)?.[1] ?? "coverage").split(", ");
  const pts = S.get(e.id);
  const cands = new Set();
  for (const p of pts) for (let i = -1; i <= 1; i++) for (let j = -1; j <= 1; j++)
    for (const id of grid.get(`${Math.floor(p[0] / cell) + i},${Math.floor(p[2] / cell) + j}`) ?? []) if (id !== e.id) cands.add(id);
  const hits = [];
  for (const id of cands) {
    const o = E.get(id), op = S.get(id);
    const reach = half(e.prefab) + half(o.prefab);
    const close = pts.map(p => [p, ...nearest(p, op)]).filter(([, dist]) => dist < reach);
    if (close.length === 0) continue;
    const step = Math.hypot(N.get(e.a).x - N.get(e.b).x, N.get(e.a).z - N.get(e.b).z) / (pts.length - 1);
    const len = close.length * step;
    const dy = Math.min(...close.map(([p, , y]) => Math.abs(p[1] - y)));
    const shared = [e.a, e.b].find(n => n === o.a || n === o.b);
    const otherLen = Math.hypot(N.get(o.a).x - N.get(o.b).x, N.get(o.a).z - N.get(o.b).z);
    const thisLen = step * (pts.length - 1);
    let kind;
    if (shared && adiff(angleAt(e, shared), angleAt(o, shared)) > 150) continue; // the next course of the same road
    if (shared && len <= reach) continue; // only the common junction
    if (shared && adiff(angleAt(e, shared), angleAt(o, shared)) < 15) kind = "parallel diverge";
    else if (len > 0.7 * Math.min(thisLen, otherLen) && o.prefab === e.prefab && dy < 3
      && close.map(c => c[1]).sort((x, y) => x - y)[close.length >> 1] < 3) kind = "duplicate";
    else if (len > 20) kind = dy < 4 ? "tight parallel" : "stacked parallel";
    // tight parallel = road surfaces touch side by side over > 20 m
    else kind = dy < 7.5 ? "crossing, low clearance" : "crossing";
    hits.push({ id, prefab: o.prefab, kind, len: Math.round(len), dy: Math.round(dy * 10) / 10, shared: !!shared });
  }
  const order = ["duplicate", "parallel diverge", "tight parallel", "crossing, low clearance", "stacked parallel", "crossing"];
  hits.sort((a, b) => order.indexOf(a.kind) - order.indexOf(b.kind));
  let main = hits[0]?.kind;
  if (!main) {
    // Distance to the nearest other course, to show how far "nothing" is.
    let best = Infinity, bestId = "";
    for (const [id, op] of S) if (id !== e.id) for (const p of pts.filter((_, i) => i % 4 === 0)) { const [dist] = nearest(p, op); if (dist < best) { best = dist; bestId = id; } }
    main = "no contact with another course of this import";
    hits.push({ id: bestId, prefab: E.get(bestId)?.prefab, kind: "nearest", len: 0, dy: 0, dist: Math.round(best) });
  }
  for (const reason of reasons) { counts[reason] ??= {}; counts[reason][main] = (counts[reason][main] ?? 0) + 1; }
  const a = N.get(e.a);
  rows.push(`| ${r.id} | ${e.prefab} | ${reasons.join(", ")} | ${main} | ${hits.slice(0, 3).map(h => h.kind === "nearest" ? `nearest ${h.id} (${h.prefab}) at ${h.dist} m` : `${h.id} (${h.prefab}, ${h.kind}, ${h.len} m, Δy ${h.dy})`).join("; ")} | ${Math.round(a.x)} | ${Math.round(a.z)} |`);
}

let md = `# Failed edges by collision\n\nSource: \`${reportPath}\` against \`${importPath}\` (the body that was imported). Contact = centrelines closer than the sum of both roads' half widths (approximate CS2 widths). The live network was not available, so the other courses of the same body stand in for the live segments; all of them were created or merged.\n\n## Counts (game reason → what the course runs into)\n\n| game reason | classification | edges |\n|---|---|---:|\n`;
for (const [reason, m] of Object.entries(counts)) for (const [k, v] of Object.entries(m)) md += `| ${reason} | ${k} | ${v} |\n`;
md += `\n## Edges\n\n| edge | prefab | game reason | classification | nearest courses (prefab, kind, contact length, min Δy) | x | z |\n|---|---|---|---|---|---:|---:|\n${rows.join("\n")}\n`;
if (outPath) writeFileSync(outPath, md);
console.log(Object.entries(counts).map(([r, m]) => `${r}: ${Object.entries(m).map(([k, v]) => `${k} ${v}`).join(", ")}`).join("\n"));
