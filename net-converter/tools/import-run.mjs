// Whole fresh-map import against the CS2MCP bridge, in the ROUND order, one command.
// node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5] [--skip step,...] [--only step,...] [--with step,...] [--climate <name>] [--dry-run]
//   [--no-verify] [--resume | --fresh] [--verify-only] [--po-structures] [--bridge http://127.0.0.1:8642] [--out out]
// --po-structures (round 20/22): the BR022 pads (ConcreteSet01 154132; BR006 variant a = no kit objects there) after po-rocks; same as --with po-structures.
//   Optional: --with po-br006-arches (BR006 variant b, DIY Stone Arch 135944), po-gantry (Truss Gantry 132188).
//   Default: pillars-off + po-piers (BR001 / BR003 ITBP caps 140978 on Pillar4x16 stacks 154132).
// run-state.json (per scale folder) lists the files imported on this map; --skip / --only / --resume skip those, a plain run (or --fresh) starts over.
// Steps (in order): surfaces network bridges bridges-fixed dam gaps power canals shippaths water lakes connections objects barriers substation po-rocks rock-scatter fort trees
//   resources surfaces power-ends settings look textures. Optional (only with --with or --only): grass, sand-smooth (the earlier smoothed sand), and the pack surfaces in optional/: rock, po-rock, decals, sand-pack (sand-pack paints the same sand as the default sand file with the Dirty Surfaces prefab: use one of the two).
// After the network steps and at the end the bridge's graph export is compared per prefab with the import files (--no-verify skips it).
// Each step POSTs its file(s) and prints one line (created / merged / failed, seconds). A body error (HTTP 400 with problems)
// stops the run and prints the problems; per-item failures are counted and the run goes on. The run report with every
// importId (for undo) goes to <scale dir>/run-<timestamp>.md (x1 → out/x1, x2 → out).
import http from "node:http";
import { existsSync, readdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { createHash } from "node:crypto";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const args = process.argv.slice(2);
const opt = (name, def) => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : def; };
const flag = name => args.includes(name);
const toolDir = dirname(fileURLToPath(import.meta.url));
const scale = opt("--scale", null);
if (!["x1", "x2"].includes(scale)) {
  console.error("usage: node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5] [--skip step,...] [--only step,...] [--with step,...] [--climate <name>] [--dry-run] [--no-verify] [--resume | --fresh] [--verify-only] [--bridge http://127.0.0.1:8642] [--out out]");
  process.exit(2);
}
const sea = Number(opt("--sea", "93"));
const factor = scale === "x2" ? 2 : 1;
const out = opt("--out", "out");
const dir = scale === "x2" ? out : join(out, "x1");
const treesDir = join(out, "trees");
const bridge = new URL(opt("--bridge", "http://127.0.0.1:8642"));
const climate = opt("--climate", null);
const dryRun = flag("--dry-run");
const list = s => (s ? s.split(",").map(x => x.trim()).filter(Boolean) : []);
const skip = new Set(list(opt("--skip", ""))), only = new Set(list(opt("--only", ""))), withOpt = new Set(list(opt("--with", "")));
if (args.includes("--po-structures")) withOpt.add("po-structures");
// Round 26: terrain-paint overrides without regenerating, e.g. --paint sand.strength=0.4,all.softEdge=25,dry.feather=0,dirt.radius=14
//   layers: sand, dry (dry-grass), dirt (bare-earth), lush, all, or one landscape feature n1 … n13 (round 27); keys: strength (main pass), softEdge, feather (feather-pass strength, 0 = no feather pass),
//   radius (dirt strokes, main pass; the feather stroke stays 10 m larger).
const paintOpt = {};
for (const kv of list(opt("--paint", ""))) {
  const m = kv.match(/^(sand|dry|dry-grass|dirt|bare-earth|lush|all|n\d{1,2})\.(strength|softEdge|feather|radius)=([\d.]+)$/);
  if (!m) { console.error(`--paint: cannot read "${kv}"`); process.exit(2); }
  const layer = { "dry-grass": "dry", "bare-earth": "dirt" }[m[1]] ?? m[1];
  (paintOpt[layer] ??= {})[m[2]] = +m[3];
}
const paintLayer = file => /sand\.json$/.test(file) ? "sand" : /dry/.test(file) ? "dry" : /lush/.test(file) ? "lush" : /earth|dirt/.test(file) ? "dirt" : "other";
const paintFeature = file => (file.match(/terrain-paint-(n\d{1,2})-/) ?? [])[1];
function applyPaint(body, file) {
  const o = { ...(paintOpt.all ?? {}), ...(paintOpt[paintLayer(file)] ?? {}), ...(paintOpt[paintFeature(file)] ?? {}) };
  if (Object.keys(o).length === 0) return "";
  const feather = x => /_f$/.test(x.id);
  for (const k of ["polygons", "strokes"]) {
    if (!body[k]) continue;
    if (o.feather === 0) body[k] = body[k].filter(x => !feather(x));
    for (const x of body[k]) {
      if (feather(x)) { if (o.feather != null) x.strength = o.feather; }
      else if (o.strength != null) x.strength = o.strength;
      if (o.softEdge != null && k === "polygons") x.softEdge = o.softEdge;
      if (o.radius != null && k === "strokes") x.radius = feather(x) ? o.radius + 10 : o.radius;
    }
  }
  if (o.strength != null) body.options.strength = o.strength;
  if (o.softEdge != null) body.options.softEdge = o.softEdge;
  return ` (paint overrides ${JSON.stringify(o)})`;
}
const verify = !flag("--no-verify");

function request(method, path, body) {
  return new Promise((resolve, reject) => {
    const data = body === undefined ? null : Buffer.from(typeof body === "string" ? body : JSON.stringify(body));
    const req = http.request({ hostname: bridge.hostname, port: bridge.port, path, method,
      headers: data ? { "content-type": "application/json", "content-length": data.length } : {} }, res => {
      const chunks = []; res.on("data", c => chunks.push(c));
      res.on("end", () => { const text = Buffer.concat(chunks).toString("utf8"); let json; try { json = JSON.parse(text); } catch { json = { raw: text }; } resolve({ status: res.statusCode, json }); });
    });
    req.setTimeout(0); req.on("error", reject);
    if (data) req.write(data);
    req.end();
  });
}

// Water file for the sea level: 97.5 → import-water.json; the variant → import-water-sea<sea × factor>.json.
const seaOut = Math.round(sea * factor * 10) / 10;
const waterFile = sea === 97.5 ? "import-water.json" : `import-water-sea${seaOut}.json`;
const treeFiles = existsSync(treesDir) ? readdirSync(treesDir).filter(f => /^import-trees-\d\d\.json$/.test(f)).sort().map(f => join(treesDir, f)) : [];
const fixedFiles = existsSync(dir) ? readdirSync(dir).filter(f => /^import-trees-fixed-\d\d\.json$/.test(f)).sort().map(f => join(dir, f)) : [];
const p = f => join(dir, f);

// [step, endpoint, files, optional]
// Order (bridge 0.9.24, cs2mcp/NOTES.md round 24): the A-v4 surfaces step removed 309 built segments (92 % of the roads inside field polygons), so
// every surface goes first and the nets are drawn over them. The substation building comes before power-ends (connectTo needs its marker); the
// net count is taken after the last network step, and every later objects / surfaces step must report netGuard.segmentsRemoved = 0.
const steps = [
  // Vanilla paint plan (audit/theme-remake.md §4): fields (Agriculture Surface 01), PO pavement and stand-in pavement (Pavement Surface 01),
  // sand (Sand Surface 01 banks, Sand Surface 02 bare earth and old decal spots), dry grass (Grass Surface 02).
  ["surfaces", "/import/surfaces", ["import-surfaces-fields.json", "import-po-structures.json", "import-surfaces-sand.json"].map(p)],
  // Round 24: terrain material paint (editor Terrain → Materials, bridge 0.9.29): Sand, Dry, Dirt, right after the surfaces. Each file is dry-run
  // first (stamps per item); a file over options.maxStamps is posted in chunks under the cap. No rock layer (the game paints rock on slopes itself).
  ["terrain-paint", "/import/terrain-paint", ["import-terrain-paint-sand.json", "import-terrain-paint-dry-grass.json", "import-terrain-paint-bare-earth.json"].map(p)],
  // Round 27: landscape spots N1–N13 (audit/theme-remake.md §4), plan order; each its own step (--skip paint-n7 etc.), files per brush.
  ...[["paint-n1", ["n1-riparian-lush"]], ["paint-n12", ["n12-fertile-dry"]], ["paint-n13", ["n13-high-ground-dry"]], ["paint-n6", ["n6-savanna-dry", "n6-savanna-dirt"]],
    ["paint-n4", ["n4-hogback-sand"]], ["paint-n5", ["n5-scree-dirt"]], ["paint-n3", ["n3-drawdown-dirt"]], ["paint-n2", ["n2-pit-banks-dirt", "n2-pit-banks-sand"]],
    ["paint-n8", ["n8-playa-rim-lush", "n8-playa-centre-dirt"]], ["paint-n7", ["n7-prairie-dog-dry", "n7-prairie-dog-dirt"]], ["paint-n11", ["n11-ore-dirt", "n11-oil-dirt"]],
    ["paint-n9", ["n9-dirt-roads-dirt"]], ["paint-n10", ["n10-rail-ballast-dirt"]]].map(([name, fs]) => [name, "/import/terrain-paint", fs.map(x => p(`import-terrain-paint-${x}.json`))]),
  ["sand-smooth", "/import/surfaces", [p(join("optional", "import-surfaces-sand-smooth.json"))], true],
  // Pack surfaces kept in optional/ (Terrain Textures, Dirty Surfaces): only with --with.
  ["rock", "/import/surfaces", [p(join("optional", "import-surfaces-rock.json"))], true],
  ["po-rock", "/import/surfaces", [p(join("optional", "import-surfaces-po-rock.json"))], true],
  ["decals", "/import/surfaces", [p(join("optional", "import-surfaces-decals.json"))], true],
  ["sand-pack", "/import/surfaces", [p(join("optional", "import-surfaces-sand-smooth-pack.json"))], true],
  ["network", "/import/network", [p("import-all.json")]],
  ["bridges", "/import/network", [p("import-bridges.json")]],
  ["bridges-fixed", "/import/network", [p("import-bridges-fixed.json")]],
  // Bridge 0.9.23: one dam edge; the game builds the plant and the dam ends from it.
  ["dam", "/import/network", [p("import-dam.json")]],
  // Round 26: Gravel Road access to the dam plant and the substation (RequireRoad).
  ["access", "/import/network", [p("import-access-roads.json")]],
  ["gaps", "/import/network", [p("import-gaps.json")]],
  ["power", "/import/network", [p("import-power.json")]],
  ["canals", "/import/network", [p("import-canals.json")]],
  ["shippaths", "/import/network", [p("import-shippaths.json")]],
  ["substation", "/import/objects", [p("import-substation.json")]],
  ["power-ends", "/import/network", [p("import-power-ends.json")]],
  ["net-count", "netcount", []],
  ["verify-network", "verify", []],
  ["water", "/import/water", [p(waterFile)]],
  ["lakes", "/import/water", [p("import-water-lakes.json")]],
  ["connections", "/import/objects", [p("import-connections.json")]],
  ["objects", "/import/objects", [p("import-objects.json")]],
  ["barriers", "/import/objects", [p("import-walls-props.json")]],
  ["po-rocks", "/import/objects", [p("import-po-rocks.json")]],
  // Round 20 (bridges/PLAN-structures.md): off by default until the owner confirms the kit mods are in the playset (--po-structures).
  ["po-structures", "/import/objects", [p("import-po-structure-objects.json")], true],
  ["po-br006-arches", "/import/objects", [p(join("optional", "import-po-structure-br006-arches.json"))], true],
  ["po-gantry", "/import/objects", [p(join("optional", "import-po-structure-gantry.json"))], true],
  // Round 22: BR001 / BR003 piers by default. The game's pillars on the courses above come back whenever a segment updates, so they are removed
  // again right before the piers go in (POST /net/pillars/remove on the live segments within 3 m of a cap).
  ["pillars-off", "pillarsoff", [p("import-po-structure-piers.json"), p(join("optional", "import-po-structure-br006-arches.json"))]],
  ["po-piers", "/import/objects", [p("import-po-structure-piers.json")]],
  ["rock-scatter", "/import/objects", [p("import-rock-scatter.json")]],
  ["fort", "/import/objects", [p("import-fort.json")]],
  ["trees", "/import/objects", [...treeFiles, ...fixedFiles]],
  ["resources", "/import/resources", [p("import-resources.json")]],
  ["verify-end", "verify", []],
  // Round 28/30: the Detroit test deck option 2, opt-in only (--with detroit-opt2; opt3 crashed the game and is out, optional/ only), after the final check, so a crash (A-v8: native crash in the
  // combined test step) never costs the map. Save the map before trying them.
  ["detroit-opt2", "/import/network", [p(join("optional", "import-detroit-opt2.json"))], true],   // round 31: owner keeps BR001; file in optional/
];
const optional = new Set(steps.filter(s => s[3]).map(s => s[0]));
const wanted = name => (only.size === 0 ? (!optional.has(name) || withOpt.has(name)) : only.has(name)) && !skip.has(name)
  && (!name.startsWith("verify") || (verify && !dryRun)) && (name !== "net-count" || !dryRun);
for (const n of [...skip, ...only, ...withOpt]) if (![...steps.map(s => s[0]), "settings", "look", "textures"].includes(n)) { console.error(`unknown step: ${n}`); process.exit(2); }

// Graph check: the bridge's export per prefab against the network import files run so far (A-v4 lost 297 built segments after the import).
const imported = {};
async function verifyGraph(tag) {
  const path = join(process.cwd(), dir, `live-graph-${stamp}-${tag}.json`);
  const r = await request("GET", `/graph/export?path=${encodeURIComponent(path)}&curves=true`);
  const live = r.json?.counts?.perPrefab ?? {};
  const lines = Object.entries(imported).map(([k, v]) => `${k} ${live[k] ?? 0}/${v}`);
  const text = `graph ${r.json?.counts?.segments ?? "?"} segments; live/imported edges: ${lines.join(", ")} (${path.split(/[\\/]/).pop()})`;
  console.log(`  ${text}`);
  const re = recheckCoverage(path);
  if (re) console.log(`  ${re}`);
  return text + (re ? `; ${re}` : "");
}

// Pillars off under the PO piers: export the live graph, take the segments that pass within 3 m of an ITBP cap, POST /net/pillars/remove.
// Round 25: also under the BR006 variant (b) arch pieces when --with po-br006-arches (bridges.tsv no longer sets nopillars on BR006).
async function pillarsOff(file, archFile) {
  if (!existsSync(file)) return `${file} missing`;
  const caps = JSON.parse(readFileSync(file, "utf8")).objects.filter(o => o.prefab === "ITBP_Pillar Piers 01");
  if (archFile && existsSync(archFile)) caps.push(...JSON.parse(readFileSync(archFile, "utf8")).objects);
  const path = join(process.cwd(), dir, `live-graph-${stamp}-pillars.json`);
  await request("GET", `/graph/export?path=${encodeURIComponent(path)}&curves=true`);
  if (!existsSync(path)) return "graph export failed";
  const g = JSON.parse(readFileSync(path, "utf8"));
  const bz = (P, t) => { const u = 1 - t; return [0, 2].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]); };
  const ids = new Set();
  for (const sg of g.segments ?? []) {
    if (!sg.curve || !/Highway|Bridge/.test(sg.prefab)) continue;
    const pts = Array.from({ length: 33 }, (_, k) => bz(sg.curve, k / 32));
    if (caps.some(c => pts.some((p, k) => k > 0 && Math.hypot((p[0] + pts[k - 1][0]) / 2 - c.x, (p[1] + pts[k - 1][1]) / 2 - c.z) < 3 + Math.hypot(p[0] - pts[k - 1][0], p[1] - pts[k - 1][1]) / 2))) ids.add(sg.id);
  }
  const r = await request("POST", "/net/pillars/remove", { segments: [...ids] });
  return `${caps.length} caps, ${ids.size} segments: HTTP ${r.status} ${JSON.stringify(r.json).slice(0, 200)}`;
}

// Bridge 0.9.29 reports such folded courses as merged, so this re-check is now only a fallback for older bridges.
// A-v6 (Gravel Road e23168_0, e31265_0, e31665_0, e36006_0): the bridge's point check counts only segments created or recorded in the batch.
// When the game folds a straight course into its straight same-prefab neighbours, it splits the whole run again into equal pieces on the 8 m grid,
// keeping the neighbour's older segment entity. The road is there, but the check misses it. Here every "points along the course" failure is checked
// again against the live graph: all three points (t 0.2 / 0.5 / 0.8) within 2 m of a live segment of the same prefab = present.
function recheckCoverage(path) {
  if (!existsSync(path)) return "";
  const pointFails = rows.flatMap(r => (r.fails ?? []).filter(x => /points along the course/.test(x.error ?? "") && !x.liveCovered).map(x => [r, x]));
  if (pointFails.length === 0) return "";
  const g = JSON.parse(readFileSync(path, "utf8"));
  const bz = (P, t) => { const u = 1 - t; return [0, 2].map(i => u * u * u * P[0][i] + 3 * u * u * t * P[1][i] + 3 * u * t * t * P[2][i] + t * t * t * P[3][i]); };
  const byPrefab = new Map();
  for (const sg of g.segments ?? []) if (sg.curve) (byPrefab.get(sg.prefab) ?? byPrefab.set(sg.prefab, []).get(sg.prefab)).push(Array.from({ length: 65 }, (_, k) => bz(sg.curve, k / 64)));
  const near = (pts, x, z) => pts.some((p, k) => k > 0 && segDist(x, z, pts[k - 1], p) < 2);
  const segDist = (x, z, a, b) => { const dx = b[0] - a[0], dz = b[1] - a[1], l2 = dx * dx + dz * dz, t = l2 > 0 ? Math.max(0, Math.min(1, ((x - a[0]) * dx + (z - a[1]) * dz) / l2)) : 0; return Math.hypot(a[0] + t * dx - x, a[1] + t * dz - z); };
  const bodies = new Map();
  let ok = 0; const still = [];
  for (const [r, x] of pointFails) {
    const body = bodies.get(r.file) ?? bodies.set(r.file, JSON.parse(readFileSync(r.file, "utf8"))).get(r.file);
    const e = (body.edges ?? []).find(y => y.id === x.id); const N = new Map((body.nodes ?? []).map(n => [n.id, n]));
    const a = e && N.get(e.a), b = e && N.get(e.b);
    if (!a || !b) { still.push(x.id); continue; }
    const c = e.control ?? [[a.x, 0, a.z], [b.x, 0, b.z]];
    const P = [[a.x, 0, a.z], [c[0][0], 0, c[0][2]], [c[1][0], 0, c[1][2]], [b.x, 0, b.z]];
    const live = byPrefab.get(x.prefab) ?? [];
    if ([0.2, 0.5, 0.8].every(t => { const [px, pz] = bz(P, t); return live.some(pts => near(pts, px, pz)); })) {
      x.liveCovered = true; x.error += " — present in the live graph (folded into its straight neighbours, re-split by the game): not a failure"; ok++;
    } else still.push(x.id);
  }
  return `point-check failures re-checked on the live graph: ${ok} present, ${still.length} missing${still.length ? ` (${still.join(", ")})` : ""}`;
}

const summary = (endpoint, r) => {
  if (endpoint === "/import/network") { const e = r.edges ?? {}; return { created: e.created ?? 0, merged: e.merged ?? 0, failed: e.failed ?? 0, text: `edges ${e.requested ?? "?"}: created ${e.created ?? 0}, merged ${e.merged ?? 0}, failed ${e.failed ?? 0}${e.notRun ? `, not run ${e.notRun}` : ""}` }; }
  if (endpoint === "/import/water") { const n = (r.sources ?? r.created ?? []).length ?? 0; const f = (r.failed ?? []).length ?? 0; return { created: n, merged: 0, failed: f, text: `sources ${n}${f ? `, failed ${f}` : ""}` }; }
  if (endpoint === "/import/resources") { const l = r.layers ?? []; return { created: l.length, merged: 0, failed: 0, text: `layers ${l.map(x => `${x.layer} ${x.cellsNonZero ?? "?"} non-zero`).join(", ")}` }; }
  const o = r.objects ?? {}; return { created: o.created ?? 0, merged: 0, failed: o.failed ?? 0, text: `objects ${o.requested ?? "?"}: created ${o.created ?? 0}, failed ${o.failed ?? 0}${o.notRun ? `, not run ${o.notRun}` : ""}` };
};
const undoOf = (endpoint, r, name) => endpoint === "/import/network" ? `POST /import/undo?id=${r.importId}` : endpoint === "/import/water" ? `POST /import/water/undo?id=${r.importId}`
  : endpoint === "/import/resources" ? "(no undo: re-import the previous layers)" : `POST /import/objects/undo?id=${r.importId}`;

const started = new Date();
const stamp = started.toISOString().replace(/[-:]/g, "").replace(/\..*/, "").replace("T", "-");
const rows = [], problems = [];
let stopped = null;
const fmt = s => `${s.toFixed(1)} s`;

const ping = await request("GET", "/ping").catch(e => ({ status: 0, json: { error: String(e) } }));
if (ping.status !== 200) { console.error(`bridge not reachable at ${bridge.origin}: ${JSON.stringify(ping.json)}`); process.exit(1); }
console.log(`bridge ${bridge.origin}: ${JSON.stringify(ping.json).slice(0, 160)}`);
console.log(`scale ${scale} (${dir}), sea ${sea} → ${seaOut} (${waterFile})${dryRun ? ", DRY RUN" : ""}`);

// GET /net/count (bridge 0.9.24): top-level segments per prefab, plus owned segments.
const perPrefab = j => j?.perPrefab ?? j?.counts?.perPrefab ?? j?.prefabs ?? {};
const totalOf = j => j?.segments ?? j?.total ?? j?.counts?.segments ?? Object.values(perPrefab(j)).reduce((a, b) => a + b, 0);
async function netCount() { const r = await request("GET", "/net/count"); return r.status === 200 ? r.json : { error: `HTTP ${r.status}`, raw: r.json }; }
const countLine = j => `${totalOf(j)} segments${j?.owned != null ? `, owned ${typeof j.owned === "object" ? JSON.stringify(j.owned) : j.owned}` : ""}: ${Object.entries(perPrefab(j)).sort((a, b) => b[1] - a[1]).map(([k, v]) => `${k} ${v}`).join(", ")}`;
function diffLine(a, b) {
  const pa = perPrefab(a), pb = perPrefab(b), keys = [...new Set([...Object.keys(pa), ...Object.keys(pb)])].sort();
  const d = keys.map(k => [k, (pb[k] ?? 0) - (pa[k] ?? 0)]).filter(([, v]) => v !== 0);
  return d.length ? d.map(([k, v]) => `${k} ${v > 0 ? "+" : ""}${v}`).join(", ") : "no change";
}
if (flag("--verify-only")) {
  // Compare after 2 minutes idle and after save + reload (cs2mcp/NOTES.md round 24, verification steps 3 and 4).
  const now = await netCount();
  console.log(`net count ${countLine(now)}`);
  const reports = readdirSync(dir).filter(f => /^run-.*\.json$/.test(f)).sort();
  if (reports.length) {
    const last = JSON.parse(readFileSync(join(dir, reports.at(-1)), "utf8"));
    if (last.netCountAfterNetwork) console.log(`vs the network baseline of ${reports.at(-1)}: ${diffLine(last.netCountAfterNetwork, now)}`);
  }
  const vstamp = new Date().toISOString().replace(/[-:]/g, "").replace(/\..*/, "").replace("T", "-");
  writeFileSync(join(dir, `netcount-${vstamp}.json`), JSON.stringify(now, null, 1));
  process.exit(0);
}
let netBaseline = null, netEnd = null, guardStop = null;

// Idempotence (A-v5: the substation was imported twice on a resumed run): run-state.json in the scale folder records every file imported on this
// map with its importId. A resumed run (--skip / --only / --resume) skips the files already there; a plain run starts a new map state (--fresh forces it).
const statePath = join(dir, "run-state.json");
const resume = (skip.size > 0 || only.size > 0 || flag("--resume")) && !flag("--fresh");
let state = { scale, started: new Date().toISOString(), files: {} };
if (resume && existsSync(statePath)) state = JSON.parse(readFileSync(statePath, "utf8"));
else if (!dryRun) writeFileSync(statePath, JSON.stringify(state, null, 1));
const doneKey = file => file.split(/[\\/]/).slice(-2).join("/");
const done = file => state.files[doneKey(file)];
const markDone = (file, importId, name) => { if (dryRun) return; state.files[doneKey(file)] = { step: name, importId: importId ?? null, at: new Date().toISOString() }; writeFileSync(statePath, JSON.stringify(state, null, 1)); };
console.log(resume ? `resuming: ${Object.keys(state.files).length} files already imported on this map (run-state.json since ${state.started})` : "new map state (run-state.json reset)");

outer: for (const [name, endpoint, files] of steps) {
  if (!wanted(name)) continue;
  if (endpoint === "netcount") {
    const t0 = Date.now(); netBaseline = await netCount();
    const text = `net count after the network steps: ${countLine(netBaseline)}`; console.log(`  ${text}`);
    rows.push({ name, file: "GET /net/count", status: "ok", secs: (Date.now() - t0) / 1000, text }); continue;
  }
  if (endpoint === "pillarsoff") { const t0 = Date.now(); const text = dryRun ? "dry run: not sent" : await pillarsOff(files[0], wanted("po-br006-arches") ? files[1] : null); console.log(`  ${text}`); rows.push({ name, file: "POST /net/pillars/remove", status: "ok", secs: (Date.now() - t0) / 1000, text }); continue; }
  if (endpoint === "verify") { const t0 = Date.now(); const text = await verifyGraph(name.replace("verify-", "")); rows.push({ name, file: "GET /graph/export", status: "ok", secs: (Date.now() - t0) / 1000, text }); continue; }
  for (const file of files) {
    if (!existsSync(file)) { console.log(`- ${name}: ${file} missing, skipped`); rows.push({ name, file, status: "missing" }); continue; }
    if (done(file)) { const d = done(file); console.log(`- ${name}: ${file} already imported (${d.importId ?? "no id"}, ${d.at}), skipped`); rows.push({ name, file, status: `already imported (${d.importId ?? ""})`, importId: d.importId }); continue; }
    const body = JSON.parse(readFileSync(file, "utf8"));
    body.options ??= {};
    if (endpoint === "/import/network") { body.options.nodesFirst = true; body.options.bypassValidation = true; }
    if (endpoint === "/import/objects" || endpoint === "/import/surfaces") body.options.bypassValidation = true;
    if (dryRun) { body.options.dryRun = true; body.dryRun = true; }
    body.name ??= file.split(/[\\/]/).pop().replace(/\.json$/, "");
    if (endpoint === "/import/terrain-paint") {
      // Dry run with the cap lifted to get every item's stamp count, then chunks of at most maxStamps (stops on a rejected body or netGuard like any step).
      const overrides = applyPaint(body, file);
      if (overrides) console.log(`  ${name} ${file.split(/[\\/]/).pop()}${overrides}`);
      const cap = body.options.maxStamps ?? 250000;
      const dr = await request("POST", endpoint, { ...body, options: { ...body.options, dryRun: true, maxStamps: 1e9 } });
      if (dr.status !== 200) {
        console.log(`✗ ${name} ${file}: dry run HTTP ${dr.status}`);
        for (const pr of (dr.json?.problems ?? [dr.json?.error ?? dr.json]).slice(0, 50)) console.log(`    ${typeof pr === "string" ? pr : JSON.stringify(pr)}`);
        problems.push({ name, file, status: dr.status, problems: dr.json?.problems ?? [dr.json] });
        rows.push({ name, file, status: `dry run HTTP ${dr.status} rejected` }); stopped = `${name} (${file}) dry run`; break outer;
      }
      const per = new Map((dr.json.items ?? []).map(i => [i.id, i.stamps]));
      const items = [...(body.polygons ?? []).map(x => ["polygons", x]), ...(body.strokes ?? []).map(x => ["strokes", x])];
      const chunks = []; let cur = [], sum = 0;
      for (const it of items) { const n = per.get(it[1].id) ?? 1; if (cur.length && sum + n > cap) { chunks.push(cur); cur = []; sum = 0; } cur.push(it); sum += n; }
      if (cur.length) chunks.push(cur);
      console.log(`  ${name} ${file.split(/[\\/]/).pop()}: dry run ${dr.json.stamps} stamps for ${items.length} items -> ${chunks.length} request(s) of at most ${cap}`);
      if (dryRun) { rows.push({ name, file, status: "dry run ok", text: `${dr.json.stamps} stamps, ${items.length} items, ${chunks.length} request(s)` }); continue; }
      for (const [ci, ch] of chunks.entries()) {
        const part = { ...body, name: `${body.name}${chunks.length > 1 ? `-${ci + 1}` : ""}`, polygons: ch.filter(c => c[0] === "polygons").map(c => c[1]), strokes: ch.filter(c => c[0] === "strokes").map(c => c[1]) };
        const t1 = Date.now();
        const r = await request("POST", endpoint, part);
        const secs = (Date.now() - t1) / 1000;
        if (r.status !== 200) {
          console.log(`✗ ${name} ${file} part ${ci + 1}: HTTP ${r.status} ${JSON.stringify(r.json).slice(0, 300)}`);
          problems.push({ name, file, status: r.status, problems: r.json?.problems ?? [r.json] });
          rows.push({ name, file, status: `HTTP ${r.status}`, secs }); stopped = `${name} (${file}) part ${ci + 1}`; break outer;
        }
        const ng = r.json.netGuard;
        const text = `part ${ci + 1}/${chunks.length}: items ${r.json.items}, stamps ${r.json.stamps}${ng ? `; netGuard removed ${ng.segmentsRemoved ?? 0}` : ""}`;
        console.log(`✓ ${name} ${file.split(/[\\/]/).pop()} ${text} (${fmt(secs)})`);
        rows.push({ name, file, status: "ok", secs, importId: r.json.importId, undo: "POST /import/terrain-paint/undo (newest first, last 3 kept)", text });
        if (ng && (ng.segmentsRemoved ?? 0) > 0) { guardStop = { name, file, netGuard: ng }; stopped = `${name} (${file}): netGuard removed ${ng.segmentsRemoved} segments`; break outer; }
      }
      markDone(file, null, name);
      continue;
    }
    const t0 = Date.now();
    const r = await request("POST", endpoint, body);
    const secs = (Date.now() - t0) / 1000;
    if (r.status === 400 || (r.status >= 400 && r.json?.problems)) {
      console.log(`✗ ${name} ${file}: HTTP ${r.status}, body rejected`);
      for (const pr of (r.json?.problems ?? [r.json?.error ?? r.json]).slice(0, 50)) console.log(`    ${typeof pr === "string" ? pr : JSON.stringify(pr)}`);
      problems.push({ name, file, status: r.status, problems: r.json?.problems ?? [r.json] });
      rows.push({ name, file, status: `HTTP ${r.status} rejected`, secs });
      stopped = `${name} (${file})`;
      break outer;
    }
    if (r.status !== 200) {
      console.log(`✗ ${name} ${file}: HTTP ${r.status} ${JSON.stringify(r.json).slice(0, 300)}`);
      rows.push({ name, file, status: `HTTP ${r.status}`, secs, error: JSON.stringify(r.json).slice(0, 300) });
      continue;
    }
    if (dryRun) { console.log(`✓ ${name} ${file}: dry run ${JSON.stringify(r.json).slice(0, 200)} (${fmt(secs)})`); rows.push({ name, file, status: "dry run ok", secs }); continue; }
    const s = summary(endpoint, r.json);
    if (endpoint === "/import/network") for (const e of body.edges ?? []) imported[e.prefab] = (imported[e.prefab] ?? 0) + 1;
    const fails = (r.json.failedEdges ?? r.json.failedObjects ?? []).slice(0, 20);
    console.log(`${s.failed ? "!" : "✓"} ${name} ${file.split(/[\\/]/).pop()}: ${s.text} (${fmt(r.json.seconds ?? secs)})${r.json.completed === false ? ` ABORTED: ${r.json.abortReason}` : ""}`);
    markDone(file, r.json.importId, name);
    const ng = r.json.netGuard;
    const guardText = ng ? `; netGuard removed ${ng.segmentsRemoved ?? 0}${ng.geometryGone ? ` (geometry gone ${ng.geometryGone})` : ""}` : "";
    rows.push({ name, file, status: r.json.completed === false ? `aborted: ${r.json.abortReason}` : "ok", secs: r.json.seconds ?? secs, importId: r.json.importId, undo: undoOf(endpoint, r.json, name), text: s.text + guardText, fails });
    // Stop at the first objects / surfaces step that removed built segments (netGuard, bridge 0.9.24).
    if (ng && (ng.segmentsRemoved ?? 0) > 0) {
      console.log(`✗ ${name} ${file}: netGuard: ${ng.segmentsRemoved} segments removed (geometry gone ${ng.geometryGone ?? "?"}); run stopped`);
      for (const b of (ng.batches ?? []).slice(0, 20)) console.log(`    batch: items ${JSON.stringify(b.items ?? []).slice(0, 400)}${b.lost ? `; lost ${JSON.stringify(b.lost).slice(0, 400)}` : ""}`);
      guardStop = { name, file, netGuard: ng };
      stopped = `${name} (${file}): netGuard removed ${ng.segmentsRemoved} segments`;
      break outer;
    }
  }
}

if (!stopped && wanted("settings")) {
  // Map settings: theme North American (name from the running game), lat 40.0, lon −105.3, sea level, climate if given.
  const themes = await request("GET", "/prefabs?category=theme&limit=100").catch(() => ({ json: {} }));
  const names = JSON.stringify(themes.json).match(/"[^"]*North[ _-]?America[^"]*"/gi)?.map(s => JSON.parse(s)) ?? [];
  const set = { latitude: 40.0, longitude: -105.3, seaLevel: seaOut, ...(names[0] ? { theme: names[0] } : { theme: "North American" }), ...(climate ? { climate } : {}) };
  const t0 = Date.now();
  const r = dryRun ? { status: 200, json: { applied: [], note: "dry run: not sent" } } : await request("POST", "/map/settings", set);
  const secs = (Date.now() - t0) / 1000;
  const line = `settings ${JSON.stringify(set)}: HTTP ${r.status}, applied ${JSON.stringify(r.json.applied ?? [])}${r.json.problems?.length ? `, problems ${JSON.stringify(r.json.problems)}` : ""}`;
  console.log(`${r.status === 200 && !r.json.problems?.length ? "✓" : "!"} ${line}`);
  rows.push({ name: "settings", file: "POST /map/settings", status: `HTTP ${r.status}`, secs, text: line });
}

if (!stopped && wanted("look")) {
  // Look: climate, water rendering, terrain rendering (audit/theme-remake.md §5–6; ../look/*.json from the theme session, else net-converter/look/).
  // GET /map/look/schema first; edit fields the schema does not list are dropped with a warning. --climate overrides the climate prefab.
  const t0 = Date.now();
  const schema = await request("GET", "/map/look/schema").catch(() => ({ status: 0, json: {} }));
  const known = new Set();
  const walk = v => { if (Array.isArray(v)) v.forEach(walk); else if (v && typeof v === "object") for (const [k, x] of Object.entries(v)) { if ((k === "name" || k === "field") && typeof x === "string") known.add(x.replace(/^m_/, "").toLowerCase()); walk(x); } };
  walk(schema.json);
  const norm = k => k.replace(/^m_/, "").toLowerCase();
  const sources = [];
  // Round 29 (A-v8): terrain-cs1.json, not terrain.json. terrain.json selects the read-only vanilla DefaultTerrainRenderSettings and so throws away the
  // editable CS1 copy and its textures; terrain-cs1.json makes / edits the copy (legacy texture format, CS1 tiling).
  for (const name of ["climate", "water", "terrain-cs1"]) {
    const file = [join(toolDir, "..", "..", "look", `${name}.json`), join(toolDir, "..", "look", `${name}.json`)].find(existsSync);
    if (file) sources.push([name, file]);
  }
  const lines = [];
  for (const [name, file] of sources) {
    const raw = readFileSync(file, "utf8"), body = JSON.parse(raw); delete body._source;   // read now, every run: the current file, never run-state
    const ver = `sha1 ${createHash("sha1").update(raw).digest("hex").slice(0, 8)}, ${statSync(file).mtime.toISOString().slice(0, 16)}`;
    if (name === "climate" && climate) body.climate = climate;
    const dropped = [];
    for (const editKey of ["climateEdit", "waterEdit", "terrainEdit"])
      if (body[editKey] && known.size > 0)
        for (const k of Object.keys(body[editKey])) if (!known.has(norm(k))) { dropped.push(`${editKey}.${k}`); delete body[editKey][k]; }
    if (dryRun) { lines.push(`${name}: ${file} [${ver}] (dry run, not sent)${dropped.length ? `; unknown: ${dropped.join(", ")}` : ""}`); continue; }
    const r = await request("POST", "/map/settings", body);
    const line = `${name}: ${file} [${ver}] HTTP ${r.status}, applied ${(r.json.applied ?? []).length}${r.json.problems?.length ? `, problems ${JSON.stringify(r.json.problems).slice(0, 300)}` : ""}${dropped.length ? `; skipped (not in /map/look/schema): ${dropped.join(", ")}` : ""}`;
    console.log(`${r.status === 200 && !r.json.problems?.length ? "✓" : "!"} look ${line}`);
    lines.push(line);
  }
  if (schema.status !== 200) lines.push(`/map/look/schema HTTP ${schema.status}: fields not pre-checked`);
  if (sources.length === 0) lines.push("no look/*.json found");
  rows.push({ name: "look", file: "POST /map/settings (look)", status: "ok", secs: (Date.now() - t0) / 1000, text: lines.join(" | ") });
}

if (!stopped && wanted("textures")) {
  // Round 29: the 14 CS1 terrain textures (look/README.md "A-v8 file list") into the terrain render settings copy, right after the look step:
  // POST /map/terrain-textures (bridge 0.9.32+, background job), then poll /map/terrain-textures/status until it is no longer running.
  // --skip textures leaves the map's textures alone; --textures-dir <folder> picks another set (default look/textures/current: gain16 with the dirt one step lighter, owner after A-v8;
  // fallbacks look/textures/gain16, look/textures/raw).
  const t0 = Date.now();
  const tdir = opt("--textures-dir", ["current", "gain16", "raw"].map(x => join(toolDir, "..", "..", "look", "textures", x)).find(existsSync) ?? "");
  const slots = [["Rock", "Cliff"], ["Dirt", "Dirt"], ["Grass", "Grass"], ["Extra1", "Extra1"], ["Extra2", "Extra2"], ["Extra3", "Extra3"], ["Extra4", "Extra4"]];
  const textures = [], missing = [];
  for (const [slot, base] of slots) {
    const t = { slot };
    for (const [key, suffix] of [["diffuse", "BaseColor"], ["normal", "Normal"]]) {
      const file = join(tdir, `${base}_${suffix}.png`);
      if (existsSync(file)) t[key] = file.replace(/\\/g, "/"); else missing.push(`${base}_${suffix}.png`);
    }
    if (t.diffuse || t.normal) textures.push(t);
  }
  let text;
  if (!tdir || textures.length === 0) text = `no texture files (${tdir || "none of look/textures/current, gain16, raw exists: run node look/tools/export-theme.mjs, then export-current.mjs"})`;
  else if (dryRun) text = `${textures.length} slots from ${tdir} (dry run, not sent)${missing.length ? `; missing ${missing.join(", ")}` : ""}`;
  else {
    const r = await request("POST", "/map/terrain-textures", { textures });
    if (r.status !== 200 && r.status !== 202) text = `POST HTTP ${r.status} ${JSON.stringify(r.json).slice(0, 300)}`;
    else {
      console.log(`  textures: job ${r.json.job ?? "?"}, ${r.json.queued ?? textures.length} files queued from ${tdir}`);
      let st = null;
      for (let i = 0; i < 300; i++) {                                    // up to 10 min, every 2 s
        await new Promise(res => setTimeout(res, 2000));
        const q = await request("GET", "/map/terrain-textures/status").catch(() => ({ status: 0, json: {} }));
        st = q.json ?? {};
        if (st.state && st.state !== "running" && st.state !== "queued") break;
      }
      const log = st?.log ?? st?.files ?? [];
      for (const e of log) console.log(`  ${/ok|done|imported/i.test(e.result ?? "") ? "✓" : "✗"} texture ${e.field ?? e.slot ?? "?"} ${String(e.file ?? "").split(/[\\/]/).pop()}: ${e.result ?? ""}${e.error ? ` (${e.error})` : ""}`);
      text = `${st?.state ?? "no status"}: done ${st?.done ?? "?"}, failed ${st?.failed ?? "?"} of ${log.length || textures.length} files from ${tdir}${missing.length ? `; missing ${missing.join(", ")}` : ""}`;
    }
  }
  console.log(`${/failed [1-9]|HTTP|no texture/.test(text) ? "!" : "✓"} textures ${text}`);
  rows.push({ name: "textures", file: "POST /map/terrain-textures", status: "ok", secs: (Date.now() - t0) / 1000, text });
}

if (!dryRun && netBaseline && wanted("net-count")) {
  netEnd = await netCount();
  const text = `net count at the end: ${countLine(netEnd)}; vs after the network: ${diffLine(netBaseline, netEnd)}`;
  console.log(`  ${text}`);
  rows.push({ name: "net-count-end", file: "GET /net/count", status: "ok", secs: 0, text });
}

const total = rows.reduce((s, r) => s + (r.secs ?? 0), 0);
const md = [`# Import run ${stamp} (${scale}, sea ${sea} → ${seaOut}${dryRun ? ", dry run" : ""})`, "",
  `Bridge ${bridge.origin}; files from \`${dir}\` (trees from \`${treesDir}\`). ${stopped ? `**Stopped at ${stopped}: body rejected.**` : "Completed."} Total ${fmt(total)}.`, "",
  "Undo in reverse order of the table (each import can only be undone while it is the newest of its kind).", "",
  "| step | file | result | seconds | importId | undo |", "|---|---|---|---:|---|---|",
  ...rows.map(r => `| ${r.name} | \`${r.file}\` | ${r.text ?? r.status}${r.error ? ` ${r.error}` : ""} | ${(r.secs ?? 0).toFixed(1)} | ${r.importId ?? ""} | ${r.undo ?? ""} |`), ""];
for (const r of rows.filter(r => r.fails?.length)) md.push(`## Failures: ${r.name} \`${r.file}\``, "", ...r.fails.map(f => `- ${f.id} ${f.prefab}: ${f.error}`), "");
if (netBaseline) md.push("## Net counts (GET /net/count)", "", `- after the network steps: ${countLine(netBaseline)}`, ...(netEnd ? [`- at the end: ${countLine(netEnd)}`, `- difference: ${diffLine(netBaseline, netEnd)}`] : []), "",
  "Check again with `node tools/import-run.mjs --scale ${scale} --verify-only` after 2 minutes idle and after save + reload; both must equal the network count.", "");
if (guardStop) md.push(`## netGuard stop: ${guardStop.name} \`${guardStop.file}\``, "", "```json", JSON.stringify(guardStop.netGuard, null, 1).slice(0, 20000), "```", "");
for (const pr of problems) md.push(`## Rejected body: ${pr.name} \`${pr.file}\` (HTTP ${pr.status})`, "", ...pr.problems.map(x => `- ${typeof x === "string" ? x : JSON.stringify(x)}`), "");
const report = join(dir, `run-${stamp}.md`);
writeFileSync(report, md.join("\n"));
writeFileSync(report.replace(/\.md$/, ".json"), JSON.stringify({ stamp, scale, sea, netCountAfterNetwork: netBaseline, netCountEnd: netEnd, guardStop, rows }, null, 1));
console.log(`report ${report}`);
process.exit(stopped ? 1 : 0);
