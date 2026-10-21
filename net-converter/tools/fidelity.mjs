// Fidelity check (round 31, owner review of A-v8): top-down SVG comparisons of CS1 vs the live game vs our import files, per interstate interchange and
// per bridge approach, with the CS1 segments that have no live counterpart and the live segments with no CS1 source (midpoint within 2 m).
// node tools/fidelity.mjs [--live out/x1/live-graph-<stamp>-end.json] [--dir out/x1] [--out out/x1/fidelity] [--box x,z,size] [--dump <DakotaExport json>]
import { existsSync, mkdirSync, readdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (n, d) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : d; };
const dir = opt("--dir", join("out", "x1"));
const live = opt("--live", join(dir, readdirSync(dir).filter(f => /^live-graph-\d{8}-\d{6}-end\.json$/.test(f)).sort().at(-1) ?? ""));
const outDir = opt("--out", join(dir, "fidelity"));
const yScale = dir.replace(/[\\/]+$/, "").endsWith("x1") ? 1 : 0.5;   // live heights → CS1 heights
const dumpPath = opt("--dump", join(process.env.LOCALAPPDATA ?? "", "DakotaExport", "2972879554_Dakota_Falls-20261019-063123.json"));
const J = f => JSON.parse(readFileSync(f, "utf8"));
mkdirSync(outDir, { recursive: true });

const short = p => p.replace(/^\d+\./, "").replace(/_Data$/, "");
// Left out on purpose by the converter (stats.md): power lines and pipes, terraforming nets, the monorail decor, CS1 field networks (fields are surfaces),
// barriers / walls / quays, ship and air paths.
const skipCs1 = /Barrier|Wall|Fence|Power Line|kV|Pipe|Cable|Ship Path|Airplane|Quay|Decoration|Trail Light|Terraforming|Monorail|Field .*Network|Field Network/i;
const bz = (P, t) => { const u = 1 - t; return [0, 1, 2].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]); };
const sample = (P, k = 24) => Array.from({ length: k + 1 }, (_, i) => bz(P, i / k));
const bbox = pts => [Math.min(...pts.map(p => p[0])), Math.min(...pts.map(p => p[2])), Math.max(...pts.map(p => p[0])), Math.max(...pts.map(p => p[2]))];

// The converter's fate per CS1 segment (segment-fates.tsv, round 31).
const fates = new Map(existsSync(join(dir, "segment-fates.tsv")) ? readFileSync(join(dir, "segment-fates.tsv"), "utf8").split(/\r?\n/).slice(1).filter(Boolean).map(l => { const [id, , fate] = l.split("\t"); return [+id, fate]; }) : []);
// CS1
const d = J(dumpPath);
const cs1Nodes = new Map(d.nodes.map(n => [n.id, n]));
const cs1 = d.segments.filter(s => !skipCs1.test(s.prefab)).map(s => {
  const a = cs1Nodes.get(s.startNode), b = cs1Nodes.get(s.endNode);
  const pts = sample(s.bezier);
  return { id: s.id, prefab: short(s.prefab), pts, bb: bbox(pts), elev: Math.max(a?.elevation ?? 0, b?.elevation ?? 0), a: s.startNode, b: s.endNode, hw: /Interstate|Highway|Ramp|Freeway/i.test(s.prefab) };
});
// live
const lg = J(live);
const lv = lg.segments.filter(s => s.curve && !/Line|Pipe|Seaway|Boatway|Barrier/.test(s.prefab)).map(s => { const pts = sample(s.curve); return { id: s.id, prefab: s.prefab, pts, bb: bbox(pts), elev: Math.max(...(s.elevation ?? [0])) }; });
// our import files
const imp = [];
for (const f of ["import-all.json", "import-bridges.json", "import-bridges-fixed.json", "import-gaps.json", "import-access-roads.json"]) {
  if (!existsSync(join(dir, f))) continue;
  const j = J(join(dir, f)); const N = new Map(j.nodes.map(n => [n.id, n]));
  for (const e of j.edges) {
    if (/Line|Pipe|Seaway|Boatway|Barrier/.test(e.prefab)) continue;
    const a = N.get(e.a), b = N.get(e.b); if (!a || !b) continue;
    const c = e.control ?? [[a.x + (b.x - a.x) / 3, 0, a.z + (b.z - a.z) / 3], [a.x + 2 * (b.x - a.x) / 3, 0, a.z + 2 * (b.z - a.z) / 3]];
    const pts = sample([[a.x, a.y, a.z], [c[0][0], 0, c[0][2]], [c[1][0], 0, c[1][2]], [b.x, b.y, b.z]]).map((p, i, arr) => [p[0], a.y + (b.y - a.y) * i / (arr.length - 1), p[2]]);
    imp.push({ id: e.id, prefab: e.prefab, pts, bb: bbox(pts), file: f, cs1: e.cs1_segment });
  }
}
const inBox = (bb, x0, z0, x1, z1) => bb[2] >= x0 && bb[0] <= x1 && bb[3] >= z0 && bb[1] <= z1;
const segDist = (p, a, b) => { const dx = b[0] - a[0], dz = b[2] - a[2], l2 = dx * dx + dz * dz; const t = l2 > 0 ? Math.max(0, Math.min(1, ((p[0] - a[0]) * dx + (p[2] - a[2]) * dz) / l2)) : 0; return Math.hypot(a[0] + t * dx - p[0], a[2] + t * dz - p[2]); };
const distTo = (p, s) => { let m = 1e9; for (let i = 1; i < s.pts.length; i++) m = Math.min(m, segDist(p, s.pts[i - 1], s.pts[i])); return m; };
const mid = s => s.pts[Math.floor(s.pts.length / 2)];

function compare(name, title, x0, z0, x1, z1) {
  const c = cs1.filter(s => inBox(s.bb, x0, z0, x1, z1)), l = lv.filter(s => inBox(s.bb, x0 - 5, z0 - 5, x1 + 5, z1 + 5)), m = imp.filter(s => inBox(s.bb, x0, z0, x1, z1));
  const inside = s => { const p = mid(s); return p[0] >= x0 && p[0] <= x1 && p[2] >= z0 && p[2] <= z1; };
  const missing = c.filter(inside).filter(s => !l.some(q => distTo(mid(s), q) < 2));
  const extra = l.filter(inside).filter(q => !c.some(s => distTo(mid(q), s) < 2));
  const notInFile = c.filter(inside).filter(s => !m.some(q => distTo(mid(s), q) < 2));
  // A CS1 segment the converter replaced on purpose (bridge override: the merged BR001 deck) is not a fidelity loss.
  const intended = s => /bridge override/.test(fates.get(s.id) ?? "") && !l.some(q => distTo(mid(s), q) < 2);
  // Height: the live segment under a CS1 segment's midpoint (x1 = true heights = CS1 heights) more than 4 m above or below CS1 (a flattened flyover, a lifted road).
  const ys = (q, p) => { let best = 1e9, y = 0; for (const r of q.pts) { const dd = Math.hypot(r[0] - p[0], r[2] - p[2]); if (dd < best) { best = dd; y = r[1]; } } return y; };
  const height = c.filter(inside).flatMap(s => { const p = mid(s); const q = l.filter(q => distTo(p, q) < 2).sort((a, b) => distTo(p, a) - distTo(p, b))[0]; if (!q) return []; const dy = ys(q, p) * yScale - p[1]; return Math.abs(dy) > 4 ? [{ s, q, dy }] : []; });
  const W = 1000, sc = W / Math.max(x1 - x0, z1 - z0), X = p => ((p[0] - x0) * sc).toFixed(1), Z = p => ((z1 - p[2]) * sc).toFixed(1);   // north up
  const path = s => "M" + s.pts.map(p => `${X(p)},${Z(p)}`).join("L");
  const svg = [`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${W} ${W + 150}" font-family="sans-serif" font-size="13">`, `<rect width="${W}" height="${W + 150}" fill="#f7f6f2"/>`,
    `<text x="10" y="${W + 20}" font-weight="bold">${title}</text>`,
    `<text x="10" y="${W + 40}">box (${x0.toFixed(0)}, ${z0.toFixed(0)}) – (${x1.toFixed(0)}, ${z1.toFixed(0)}), north up, ${(x1 - x0).toFixed(0)} m. CS1 ${c.length} segments, live ${l.length}, our files ${m.length}.</text>`,
    `<g fill="none" stroke-linecap="round">`];
  for (const s of c) svg.push(`<path d="${path(s)}" stroke="${s.elev > 0 ? "#6b8fbf" : "#b9b4a6"}" stroke-width="${s.hw ? 9 : 6}" opacity="0.7"><title>CS1 ${s.id} ${s.prefab} el ${s.elev}</title></path>`);
  for (const s of m) svg.push(`<path d="${path(s)}" stroke="#2e8b57" stroke-width="3.5"><title>file ${s.id} ${s.prefab} (${s.file})</title></path>`);
  for (const s of l) svg.push(`<path d="${path(s)}" stroke="#c0392b" stroke-width="1.6"${s.elev > 0.5 ? ' stroke-dasharray="6 3"' : ""}><title>live ${s.id} ${s.prefab} el ${s.elev.toFixed(1)}</title></path>`);
  for (const s of missing) svg.push(`<path d="${path(s)}" stroke="#ff8c00" stroke-width="12" opacity="0.45"><title>MISSING LIVE: CS1 ${s.id} ${s.prefab}</title></path>`);
  for (const h of height) svg.push(`<path d="${path(h.s)}" stroke="#1a1aff" stroke-width="12" opacity="0.35"><title>HEIGHT: CS1 ${h.s.id} ${h.s.prefab} y ${mid(h.s)[1].toFixed(1)}, live ${h.q.id} ${(h.dy > 0 ? "+" : "") + h.dy.toFixed(1)} m</title></path>`);
  for (const s of extra) svg.push(`<path d="${path(s)}" stroke="#d000d0" stroke-width="8" opacity="0.4"><title>NO CS1 SOURCE: live ${s.id} ${s.prefab}</title></path>`);
  svg.push(`</g>`);
  const leg = [["#b9b4a6", "CS1 ground"], ["#6b8fbf", "CS1 elevated"], ["#2e8b57", "our import files"], ["#c0392b", "live (dashed = elevated)"], ["#ff8c00", "CS1 without live counterpart"], ["#d000d0", "live without CS1 source"], ["#1a1aff", "height off CS1 by > 4 m"]];
  leg.forEach(([col, t], i) => svg.push(`<rect x="${10 + 140 * i}" y="${W + 60}" width="18" height="10" fill="${col}"/><text x="${32 + 140 * i}" y="${W + 70}">${t}</text>`));
  svg.push(`<text x="10" y="${W + 100}">CS1 without live counterpart: ${missing.length} (${missing.slice(0, 8).map(s => s.id).join(", ")}${missing.length > 8 ? " …" : ""}); not in our files: ${notInFile.length}</text>`);
  svg.push(`<text x="10" y="${W + 140}">height off CS1 by more than 4 m: ${height.length} (${height.slice(0, 6).map(h => `${h.s.id} ${h.dy > 0 ? "+" : ""}${h.dy.toFixed(0)}`).join(", ")}${height.length > 6 ? " …" : ""})</text>`);
  svg.push(`<text x="10" y="${W + 120}">live without CS1 source: ${extra.length} (${extra.slice(0, 6).map(s => s.id).join(", ")}${extra.length > 6 ? " …" : ""})</text>`, `</svg>`);
  writeFileSync(join(outDir, name + ".svg"), svg.join("\n"));
  const lost = missing.filter(s => !intended(s));
  const verdict = lost.length === 0 && extra.length === 0 && height.length === 0 ? "OK: layout and heights match CS1 (2 m in plan, 4 m in height)"
    : `CHECK: ${lost.length} CS1 segments without live counterpart${missing.length > lost.length ? ` (+${missing.length - lost.length} replaced by a bridge override)` : ""}, ${extra.length} live without CS1 source, ${height.length} more than 4 m off the CS1 height`;
  const md = [`# ${title}`, "", verdict, "", `![](${name}.svg)`, "", "| CS1 segment | prefab | elevation | in our files | live | converter fate |", "|---|---|---:|---|---|---|",
    ...missing.map(s => `| ${s.id} | ${s.prefab} | ${s.elev} | ${notInFile.includes(s) ? "no" : "yes"} | missing | ${fates.get(s.id) ?? "?"} |`), "",
    "| live segment | prefab | elevation |", "|---|---|---:|", ...extra.map(s => `| ${s.id} | ${s.prefab} | ${s.elev.toFixed(1)} |`), "",
    "| CS1 segment | prefab | CS1 y | live | live − CS1 m |", "|---|---|---:|---|---:|", ...height.map(h => `| ${h.s.id} | ${h.s.prefab} | ${mid(h.s)[1].toFixed(1)} | ${h.q.id} ${h.q.prefab} | ${h.dy.toFixed(1)} |`), ""];
  writeFileSync(join(outDir, name + ".md"), md.join("\n"));
  return { name, title, verdict, missing: lost.length, extra: extra.length, height: height.length, notInFile: notInFile.length };
}

const results = [];
const boxArg = opt("--box", null);
if (boxArg) {
  const [x, z, size] = boxArg.split(",").map(Number);
  results.push(compare(`box_${Math.round(x)}_${Math.round(z)}`, `Box at (${x}, ${z})`, x - size / 2, z - size / 2, x + size / 2, z + size / 2));
} else {
  // Interstate interchanges: CS1 nodes where three or more segments meet and one of them is a highway / ramp; clustered within 350 m.
  const deg = new Map(), hwAt = new Set();
  for (const s of cs1) for (const n of [s.a, s.b]) { deg.set(n, (deg.get(n) ?? 0) + 1); if (s.hw) hwAt.add(n); }
  const pts = [...deg].filter(([n, k]) => k >= 3 && hwAt.has(n)).map(([n]) => cs1Nodes.get(n)).filter(Boolean).map(n => [n.position[0], n.position[2]]);
  const cl = []; const used = new Array(pts.length).fill(false);
  for (let i = 0; i < pts.length; i++) {
    if (used[i]) continue; const group = [i]; used[i] = true;
    for (let k = 0; k < group.length; k++) for (let j = 0; j < pts.length; j++) if (!used[j] && Math.hypot(pts[j][0] - pts[group[k]][0], pts[j][1] - pts[group[k]][1]) < 350) { used[j] = true; group.push(j); }
    cl.push(group.map(g => pts[g]));
  }
  cl.forEach((g, i) => {
    const xs = g.map(p => p[0]), zs = g.map(p => p[1]); const cx = (Math.min(...xs) + Math.max(...xs)) / 2, cz = (Math.min(...zs) + Math.max(...zs)) / 2;
    const size = Math.max(800, Math.max(...xs) - Math.min(...xs) + 300, Math.max(...zs) - Math.min(...zs) + 300);
    results.push(compare(`interchange_${String(i + 1).padStart(2, "0")}_${Math.round(cx)}_${Math.round(cz)}`, `Interchange ${i + 1} at (${cx.toFixed(0)}, ${cz.toFixed(0)}), ${g.length} highway junction nodes`, cx - size / 2, cz - size / 2, cx + size / 2, cz + size / 2));
  });
  // Bridge approaches: one box per bridges.tsv override (edges brNNN_* in the bridge files), 800 m around the deck.
  const br = new Map();
  for (const s of imp.filter(s => /^br\d{3}/.test(s.id))) { const k = s.id.slice(0, 5).toUpperCase(); const b = br.get(k) ?? [1e9, 1e9, -1e9, -1e9]; br.set(k, [Math.min(b[0], s.bb[0]), Math.min(b[1], s.bb[1]), Math.max(b[2], s.bb[2]), Math.max(b[3], s.bb[3])]); }
  for (const [k, b] of [...br].sort()) {
    const cx = (b[0] + b[2]) / 2, cz = (b[1] + b[3]) / 2, size = Math.max(800, b[2] - b[0] + 400, b[3] - b[1] + 400);
    results.push(compare(`bridge_${k}_${Math.round(cx)}_${Math.round(cz)}`, `Bridge ${k} and its approaches at (${cx.toFixed(0)}, ${cz.toFixed(0)})`, cx - size / 2, cz - size / 2, cx + size / 2, cz + size / 2));
  }
}
const index = [`# Fidelity check: CS1 vs live (${live.split(/[\\/]/).pop()}) vs our files`, "",
  "Each image: CS1 segments wide and pale (blue = elevated), our import files green, the live game thin red (dashed = elevated); orange = a CS1 segment with no live",
  "segment within 2 m of its midpoint, magenta = a live segment with no CS1 segment within 2 m. Hover a line for its id and prefab.", "",
  "| image | verdict |", "|---|---|", ...results.map(r => `| [${r.title}](${r.name}.md) | ${r.verdict} |`), ""];
writeFileSync(join(outDir, "index.md"), index.join("\n"));
console.log(`${results.length} comparisons -> ${outDir} (OK ${results.filter(r => r.missing === 0 && r.extra === 0 && r.height === 0).length})`);
for (const r of results) console.log(`${r.name}: ${r.verdict}`);
