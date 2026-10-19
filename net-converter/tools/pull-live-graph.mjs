// Pulls every network segment and node from the running game through the CS2MCP bridge
// (/city/road-graph, nets=all) by tiling the playable square, and writes out/live-graph.json.
// Usage: node tools/pull-live-graph.mjs [out/live-graph.json] [--bridge http://127.0.0.1:8642]
import { writeFileSync, mkdirSync } from "node:fs";
import { dirname } from "node:path";

const args = process.argv.slice(2);
const bridgeIdx = args.indexOf("--bridge");
const bridge = bridgeIdx >= 0 ? args.splice(bridgeIdx, 2)[1] : "http://127.0.0.1:8642";
const outPath = args[0] ?? "out/live-graph.json";
const HALF = 7168 + 200; // a little beyond the border catches outside-connection stubs
const LIMIT = 3000;

const segments = new Map();
const nodes = new Map();
let calls = 0;

async function tile(cx, cz, half) {
  // Circle radius covering the square tile [cx±half, cz±half].
  const radius = Math.min(2000, half * Math.SQRT2 + 1);
  const url = `${bridge}/city/road-graph?x=${cx}&z=${cz}&radius=${radius}&limit=${LIMIT}&nets=all`;
  calls++;
  const res = await fetch(url);
  if (!res.ok) throw new Error(`${url}: HTTP ${res.status} ${await res.text()}`);
  const j = await res.json();
  if (j.roadsMatched > j.roads.length || half * Math.SQRT2 > 2000) {
    // Too many segments (or tile too big for one circle): split in four.
    const h = half / 2;
    for (const [dx, dz] of [[-h, -h], [h, -h], [-h, h], [h, h]]) await tile(cx + dx, cz + dz, h);
    return;
  }
  for (const r of j.roads) {
    segments.set(`${r.road.index}:${r.road.version}`, r);
  }
  for (const n of j.junctions) nodes.set(`${n.node.index}:${n.node.version}`, n);
}

const n = 6;
const step = (2 * HALF) / n;
for (let i = 0; i < n; i++)
  for (let k = 0; k < n; k++)
    await tile(-HALF + step * (i + 0.5), -HALF + step * (k + 0.5), step / 2);

const out = {
  pulledAtUtc: new Date().toISOString(),
  bridge,
  calls,
  segments: [...segments.values()],
  nodes: [...nodes.values()],
};
if (out.segments.length === 0) {
  console.error(`no segments returned (map not loaded, or bridge sees an empty network); ${outPath} left untouched`);
  process.exit(3);
}
mkdirSync(dirname(outPath), { recursive: true });
writeFileSync(outPath, JSON.stringify(out));
const byPrefab = {};
for (const s of out.segments) byPrefab[s.prefab] = (byPrefab[s.prefab] ?? 0) + 1;
console.log(`${calls} calls, ${out.segments.length} segments, ${out.nodes.length} nodes -> ${outPath}`);
console.log(byPrefab);
