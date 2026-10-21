// Round 43: check the rock shed file against itself and the road, the way fidelity.mjs draws comparisons: a top-down SVG (plates, pillars, rock face,
// road edges) and a valley-side elevation (arc length vs height: road, plate undersides, pillars), plus the numbers: pillar inset from the plate
// edge, pillar top − plate underside, plates' valley edge off the road-centreline offset, step between neighbouring rows.
// node tools/shed-check.mjs [--dir out/x1] [--out <dir>/shed-check]
import { readFileSync, writeFileSync, mkdirSync } from "node:fs";
import { join } from "node:path";
const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 ? args[i + 1] : d; };
const dir = opt("--dir", join("out", "x1")), out = opt("--out", join(dir, "shed-check"));
mkdirSync(out, { recursive: true });
const J = f => JSON.parse(readFileSync(join(dir, f), "utf8"));
const shed = J("import-rock-shed.json").objects, net = J("import-all.json");
const N = new Map(net.nodes.map(n => [n.id, n]));
const road = net.edges.filter(e => /4-Lane National Road/.test(e.cs1_prefab ?? "")).map(e => {
  const a = N.get(e.a), b = N.get(e.b), c = e.control ?? [[a.x + (b.x - a.x) / 3, 0, a.z + (b.z - a.z) / 3], [a.x + 2 * (b.x - a.x) / 3, 0, a.z + 2 * (b.z - a.z) / 3]];
  const P = [[a.x, a.z], [c[0][0], c[0][2]], [c[1][0], c[1][2]], [b.x, b.z]];
  return Array.from({ length: 65 }, (_, k) => { const t = k / 64, u = 1 - t; return [...[0, 1].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]), a.y + t * (b.y - a.y)]; });
}).filter(pts => pts.some(p => p[0] < -6150 && p[0] > -6550 && p[1] > 580 && p[1] < 1150));
const prefabW = { "Highway Twoway - 4 lanes": 10, "Medium Road": 12 };
const half = prefabW[net.edges.find(e => /4-Lane National Road/.test(e.cs1_prefab ?? ""))?.prefab] ?? 10;
// One road polyline ordered along the shed (SE → NW), with arc length.
let line = road.flat().sort((p, q) => (p[1] - q[1]) || (q[0] - p[0]));
const L = []; for (const p of line) { if (L.length && Math.hypot(p[0] - L.at(-1)[0], p[1] - L.at(-1)[1]) < 0.5) continue; L.push([...p, L.length ? L.at(-1)[3] + Math.hypot(p[0] - L.at(-1)[0], p[1] - L.at(-1)[1]) : 0]); }
const proj = (x, z) => { let best = { d: 1e9 }; for (let i = 1; i < L.length; i++) { const a = L[i - 1], b = L[i], dx = b[0] - a[0], dz = b[1] - a[1], l2 = dx * dx + dz * dz, t = Math.max(0, Math.min(1, ((x - a[0]) * dx + (z - a[1]) * dz) / l2)); const px = a[0] + t * dx, pz = a[1] + t * dz, d = Math.hypot(x - px, z - pz); if (d < best.d) { const ln = Math.sqrt(l2); best = { d, s: a[3] + t * ln, y: a[2] + t * (b[2] - a[2]), side: Math.sign((x - a[0]) * dz - (z - a[1]) * dx), tx: dx / ln, tz: dz / ln }; } } return best; };
const plates = shed.filter(o => o.id.startsWith("shed_roof_")), cols = shed.filter(o => o.id.startsWith("shed_col_")), rocks = shed.filter(o => o.id.startsWith("shed_cliff_"));
// Plate frame: local x → (cos r, −sin r), local z → (sin r, cos r); pitch (0.9.50): + tilts the +z end down.
const local = (o, x, z) => { const r = o.rotation * Math.PI / 180, u = (x - o.x) * Math.cos(r) - (z - o.z) * Math.sin(r), v = (x - o.x) * Math.sin(r) + (z - o.z) * Math.cos(r); return [u, v]; };
const underside = (o, v) => o.y - v * Math.tan((o.pitch ?? 0) * Math.PI / 180);
// The valley side: the side of the road the columns are on.
const valleySide = Math.sign(cols.reduce((s, o) => s + proj(o.x, o.z).side, 0));
const nums = { pillars: cols.length, inset: [], topErr: [], edgeOff: [], rowStep: [] };
const stacks = new Map(); for (const o of cols) { const k = o.id.split("_").slice(0, 3).join("_"); if (!stacks.has(k)) stacks.set(k, []); stacks.get(k).push(o); }
for (const [, st] of stacks) {
  const o = st[0], h = Math.max(...st.map(q => q.y + (q.prefab === "Pillar2x16" ? 16 : 8)));
  const cover = plates.filter(p => { const [u, v] = local(p, o.x, o.z); return Math.abs(u) <= 8 && Math.abs(v) <= 8; });
  if (cover.length === 0) { nums.inset.push(-99); continue; }
  // Inset: from the column centre to the nearest plate edge across the road (local x), over the plates that cover it.
  nums.inset.push(Math.max(...cover.map(p => 8 - Math.abs(local(p, o.x, o.z)[0]))));
  nums.topErr.push(h - Math.min(...cover.map(p => underside(p, local(p, o.x, o.z)[1]))));
}
// Valley edge of each row's valley plate against the road-centreline offset (half + 0.5).
const rows = []; for (const p of plates) { const q = proj(p.x, p.z); if (q.side === valleySide) rows.push({ p, q }); }
for (const { p } of rows) for (const v of [-8, 0, 8]) {
  const r = p.rotation * Math.PI / 180, pts = [-8, 8].map(u => [p.x + u * Math.cos(r) + v * Math.sin(r), p.z - u * Math.sin(r) + v * Math.cos(r)]);
  const qs = pts.map(([x, z]) => proj(x, z)).filter(q => q.s > 0.5 && q.s < L.at(-1)[3] - 0.5);   // corners past the ends of the road line do not count
  if (qs.length) nums.edgeOff.push(Math.max(...qs.map(q => q.side === valleySide ? q.d : -q.d)) - (half + 0.5));
}
const valleyRows = rows.map(r => r.p).sort((a, b) => proj(a.x, a.z).s - proj(b.x, b.z).s);
for (let i = 1; i < valleyRows.length; i++) { const a = valleyRows[i - 1], b = valleyRows[i], sa = proj(a.x, a.z).s, sb = proj(b.x, b.z).s, mid = (sa + sb) / 2; nums.rowStep.push(Math.abs(underside(b, mid - sb) - underside(a, mid - sa))); }
const mm = a => a.length ? `${Math.min(...a).toFixed(3)} … ${Math.max(...a).toFixed(3)}` : "–";
const report = [`pillar stacks ${stacks.size} (${cols.length} pieces)`, `pillar centre → nearest plate edge (inset) ${mm(nums.inset)} m`, `pillar top − plate underside ${mm(nums.topErr)} m`,
  `valley plates' outer edge − (road centreline offset ${half + 0.5} m) ${mm(nums.edgeOff)} m`, `underside step between neighbouring rows (at their joint) ${mm(nums.rowStep)} m`, `rock pieces ${rocks.length}`].join("\n");
// Top-down SVG.
const all = [...plates, ...cols, ...rocks]; const xs = all.map(o => o.x), zs = all.map(o => o.z);
const x0 = Math.min(...xs) - 40, x1 = Math.max(...xs) + 40, z0 = Math.min(...zs) - 40, z1 = Math.max(...zs) + 40, S = 2;
const X = x => ((x - x0) * S).toFixed(1), Z = z => ((z1 - z) * S).toFixed(1);
const rect = (o, w, d, cls) => { const r = o.rotation * Math.PI / 180; const pts = [[-w, -d], [w, -d], [w, d], [-w, d]].map(([u, v]) => `${X(o.x + u * Math.cos(r) + v * Math.sin(r))},${Z(o.z - u * Math.sin(r) + v * Math.cos(r))}`); return `<polygon class="${cls}" points="${pts.join(" ")}"/>`; };
const edgeLine = off => `<polyline class="road" points="${L.map((p, i) => { const q = L[Math.min(i + 1, L.length - 1)], o = L[Math.max(i - 1, 0)], dx = q[0] - o[0], dz = q[1] - o[1], l = Math.hypot(dx, dz) || 1; return `${X(p[0] - dz / l * off)},${Z(p[1] + dx / l * off)}`; }).join(" ")}"/>`;
const css = `<style>.plate{fill:#ccc;fill-opacity:.35;stroke:#666;stroke-width:.6}.col{fill:#c00}.rock{fill:#8a6;fill-opacity:.3;stroke:#563}.road{fill:none;stroke:#03c;stroke-width:1}.axis{fill:none;stroke:#03c;stroke-dasharray:4 3}text{font:12px sans-serif}</style>`;
writeFileSync(join(out, "top.svg"), `<svg xmlns="http://www.w3.org/2000/svg" width="${((x1 - x0) * S).toFixed(0)}" height="${((z1 - z0) * S).toFixed(0)}">${css}
${rocks.map(o => rect(o, 18, 18, "rock")).join("")}${plates.map(o => rect(o, 8, 8, "plate")).join("")}${edgeLine(half)}${edgeLine(-half)}<polyline class="axis" points="${L.map(p => `${X(p[0])},${Z(p[1])}`).join(" ")}"/>
${cols.map(o => rect(o, 1, 1, "col")).join("")}<text x="10" y="20">${report.split("\n").join(" | ")}</text></svg>`);
// Valley-side elevation: arc length s (x) vs height (y, ×4).
const ss = all.map(o => proj(o.x, o.z).s), sMin = Math.min(...ss) - 20, sMax = Math.max(...ss) + 20, yMin = Math.min(...cols.map(o => o.y)) - 5, yMax = Math.max(...plates.map(o => o.y)) + 10, VY = 4;
const SX = s => ((s - sMin) * S).toFixed(1), SY = y => ((yMax - y) * VY).toFixed(1);
const plateSeg = o => { const q = proj(o.x, o.z), r = o.rotation * Math.PI / 180, dirAlong = Math.sign(Math.sin(r) * q.tx + Math.cos(r) * q.tz) || 1; return `<line class="road" style="stroke:#666;stroke-width:2" x1="${SX(q.s - 8)}" y1="${SY(underside(o, -8 * dirAlong))}" x2="${SX(q.s + 8)}" y2="${SY(underside(o, 8 * dirAlong))}"/>`; };
writeFileSync(join(out, "valley.svg"), `<svg xmlns="http://www.w3.org/2000/svg" width="${((sMax - sMin) * S).toFixed(0)}" height="${((yMax - yMin) * VY).toFixed(0)}">${css}
<polyline class="road" points="${L.map(p => `${SX(p[3])},${SY(p[2])}`).join(" ")}"/>${valleyRows.map(plateSeg).join("")}
${cols.map(o => { const q = proj(o.x, o.z), h = o.prefab === "Pillar2x16" ? 16 : 8; return `<rect class="col" x="${SX(q.s - 1)}" y="${SY(o.y + h)}" width="${2 * S}" height="${(h * VY).toFixed(1)}" fill-opacity=".6"/>`; }).join("")}
<text x="10" y="20">valley side, height ×${VY}: road (blue), valley-row plate undersides (grey), columns (red)</text></svg>`);
writeFileSync(join(out, "numbers.txt"), report + "\n");
console.log(report + `\n-> ${join(out, "top.svg")}, ${join(out, "valley.svg")}`);
