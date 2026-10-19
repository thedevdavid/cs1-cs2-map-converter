// Geometry of failed import edges: node-tools/failed-geometry.mjs <report.json> <import.json> [regex on error]
import { readFileSync } from "node:fs";
const [reportPath, importPath, filter = "InvalidShape|SteepSlope|Clearance"] = process.argv.slice(2);
const R = JSON.parse(readFileSync(reportPath, "utf8"));
const d = JSON.parse(readFileSync(importPath, "utf8"));
const N = new Map(d.nodes.map(n => [n.id, n]));
const E = new Map(d.edges.map(e => [e.id, e]));
const ev = (p, t) => { const u = 1 - t; return [0, 1, 2].map(i => u * u * u * p[0][i] + 3 * u * u * t * p[1][i] + 3 * u * t * t * p[2][i] + t * t * t * p[3][i]); };
const ang = (v, c) => Math.acos(Math.max(-1, Math.min(1, (v[0] * c[0] + v[1] * c[1]) / (Math.hypot(...v) * Math.hypot(...c) + 1e-9)))) * 180 / Math.PI;
for (const r of R.results.filter(r => new RegExp(filter).test(r.error))) {
  const e = E.get(r.id); const a = N.get(e.a), b = N.get(e.b);
  const P = [[a.x, a.y, a.z], e.control[0], e.control[1], [b.x, b.y, b.z]];
  let len = 0, pr = ev(P, 0); for (let i = 1; i <= 32; i++) { const q = ev(P, i / 32); len += Math.hypot(q[0] - pr[0], q[2] - pr[2]); pr = q; }
  const ch = Math.hypot(a.x - b.x, a.z - b.z), c = [b.x - a.x, b.z - a.z];
  const va = [e.control[0][0] - a.x, e.control[0][2] - a.z], vb = [b.x - e.control[1][0], b.z - e.control[1][2]];
  const cr = v => Math.sign(c[0] * v[1] - c[1] * v[0]);
  const deg = n => { let k = 0; for (const x of d.edges) if (x.a === n || x.b === n) k++; return k; };
  console.log([(r.error.match(/\(([^)]+)\)/) ?? [, r.error.slice(0, 30)])[1].padEnd(36), e.id.padEnd(9), e.prefab.padEnd(25),
    "ch", ch.toFixed(1).padStart(6), "len/ch", (len / ch).toFixed(3), "tan", ang(va, c).toFixed(1), ang(vb, c).toFixed(1), cr(va) * cr(vb) < 0 ? "S" : "C",
    "el", e.elevation.join("/").padEnd(12), "deg", deg(e.a), deg(e.b), e.structure.padEnd(8), e.cs1_prefab.replace(/^\d+\./, ""),
    "@", Math.round(a.x), Math.round(a.z)].join(" "));
}
