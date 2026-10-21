// Dry fidelity input (round 36): the network import files as a /graph/export-shaped graph, so tools/fidelity.mjs can compare CS1 with what we would post
// instead of with a live map. Nodes are keyed by position (0.5 m), so courses in different files that meet at one point connect, as in the game.
// node tools/file-graph.mjs [--dir out/x1] [--out <dir>/file-graph.json] [--heightmap ../hm-rebuild/out/DakotaFalls-1to1-4096.png]
import { readFileSync, writeFileSync, existsSync } from "node:fs";
import { join } from "node:path";
import { loadHeightmap } from "./hm-sample.mjs";
const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf(k); return i >= 0 ? args[i + 1] : d; };
const dir = opt("--dir", join("out", "x1"));
const out = opt("--out", join(dir, "file-graph.json"));
const x1 = dir.replace(/[\/]+$/, "").endsWith("x1");
const hm = loadHeightmap(opt("--heightmap", "../hm-rebuild/out/DakotaFalls-1to1-4096.png"), (x1 ? 2048 : 4096) / 65535);
// The /import/network files of tools/import-run.mjs, in its order.
const files = ["import-all.json", "import-bridges.json", "import-bridges-fixed.json", "import-dam.json", "import-access-roads.json", "import-gaps.json", "import-canals.json"];
const segments = [];
for (const f of files) {
  const path = join(dir, f); if (!existsSync(path)) continue;
  const j = JSON.parse(readFileSync(path, "utf8")); const N = new Map(j.nodes.map(n => [n.id, n]));
  const abs = j.options?.heightMode !== "elevation";
  for (const e of j.edges) {
    const a = N.get(e.a), b = N.get(e.b); if (!a || !b) continue;
    const key = n => `p${Math.round(n.x * 2)}_${Math.round(n.z * 2)}`;
    const y = (n, i) => abs ? n.y : hm(n.x, n.z) + (e.elevation?.[i] ?? 0);
    const pa = [a.x, y(a, 0), a.z], pd = [b.x, y(b, 1), b.z];
    const c = e.control?.length === 2 ? e.control : [[a.x + (b.x - a.x) / 3, 0, a.z + (b.z - a.z) / 3], [a.x + 2 * (b.x - a.x) / 3, 0, a.z + 2 * (b.z - a.z) / 3]];
    const curve = [pa, [c[0][0], pa[1] + (pd[1] - pa[1]) / 3, c[0][2]], [c[1][0], pa[1] + 2 * (pd[1] - pa[1]) / 3, c[1][2]], pd];
    segments.push({ id: `${f.replace(/^import-|\.json$/g, "")}:${e.id}`, prefab: e.prefab, a: key(a), b: key(b), length: Math.hypot(b.x - a.x, b.z - a.z),
      elevation: [pa[1] - hm(a.x, a.z), pd[1] - hm(b.x, b.z)], owned: false, curve });
  }
}
writeFileSync(out, JSON.stringify({ source: "import files (tools/file-graph.mjs)", dir, segments }));
console.log(`${segments.length} courses from ${files.filter(f => existsSync(join(dir, f))).length} files -> ${out}`);
