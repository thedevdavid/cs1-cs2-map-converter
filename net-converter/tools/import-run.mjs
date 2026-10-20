// Whole fresh-map import against the CS2MCP bridge, in the ROUND order, one command.
// node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5] [--skip step,...] [--only step,...] [--with step,...] [--climate <name>] [--dry-run]
//   [--no-verify] [--bridge http://127.0.0.1:8642] [--out out]
// Steps: network bridges gaps power canals shippaths water lakes connections objects barriers substation po-rocks rock-scatter fort trees
//   resources surfaces power-ends settings. Optional (only with --with or --only): bridges-fixed (needs the converter run with --bridges-fixed on and the bridge fixedIndex support), sand-smooth, decals.
// After the network steps and at the end the bridge's graph export is compared per prefab with the import files (--no-verify skips it).
// Each step POSTs its file(s) and prints one line (created / merged / failed, seconds). A body error (HTTP 400 with problems)
// stops the run and prints the problems; per-item failures are counted and the run goes on. The run report with every
// importId (for undo) goes to <scale dir>/run-<timestamp>.md (x1 → out/x1, x2 → out).
import http from "node:http";
import { existsSync, readdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (name, def) => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : def; };
const flag = name => args.includes(name);
const scale = opt("--scale", null);
if (!["x1", "x2"].includes(scale)) {
  console.error("usage: node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5] [--skip step,...] [--only step,...] [--with step,...] [--climate <name>] [--dry-run] [--no-verify] [--bridge http://127.0.0.1:8642] [--out out]");
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

// [step, endpoint, files]
const steps = [
  ["network", "/import/network", [p("import-all.json")]],
  ["bridges", "/import/network", [p("import-bridges.json")]],
  ["bridges-fixed", "/import/network", [p("import-bridges-fixed.json")], true],
  ["gaps", "/import/network", [p("import-gaps.json")]],
  ["power", "/import/network", [p("import-power.json")]],
  ["canals", "/import/network", [p("import-canals.json")]],
  ["shippaths", "/import/network", [p("import-shippaths.json")]],
  ["verify-network", "verify", []],
  ["water", "/import/water", [p(waterFile)]],
  ["lakes", "/import/water", [p("import-water-lakes.json")]],
  ["connections", "/import/objects", [p("import-connections.json")]],
  ["objects", "/import/objects", [p("import-objects.json")]],
  ["barriers", "/import/objects", [p("import-walls-props.json")]],
  ["substation", "/import/objects", [p("import-substation.json")]],
  ["po-rocks", "/import/objects", [p("import-po-rocks.json")]],
  ["rock-scatter", "/import/objects", [p("import-rock-scatter.json")]],
  ["fort", "/import/objects", [p("import-fort.json")]],
  ["trees", "/import/objects", [...treeFiles, ...fixedFiles]],
  ["resources", "/import/resources", [p("import-resources.json")]],
  ["surfaces", "/import/surfaces", ["import-surfaces-fields.json", "import-surfaces-rock.json", "import-surfaces-po-rock.json", "import-po-structures.json"].map(p)],
  ["sand-smooth", "/import/surfaces", [p("import-surfaces-sand-smooth.json")], true],
  ["decals", "/import/surfaces", [p("import-surfaces-decals.json")], true],
  ["power-ends", "/import/network", [p("import-power-ends.json")]],
  ["verify-end", "verify", []],
];
const optional = new Set(steps.filter(s => s[3]).map(s => s[0]));
const wanted = name => (only.size === 0 ? (!optional.has(name) || withOpt.has(name)) : only.has(name)) && !skip.has(name)
  && (!name.startsWith("verify") || (verify && !dryRun));
for (const n of [...skip, ...only, ...withOpt]) if (![...steps.map(s => s[0]), "settings"].includes(n)) { console.error(`unknown step: ${n}`); process.exit(2); }

// Graph check: the bridge's export per prefab against the network import files run so far (A-v4 lost 297 built segments after the import).
const imported = {};
async function verifyGraph(tag) {
  const path = join(process.cwd(), dir, `live-graph-${stamp}-${tag}.json`);
  const r = await request("GET", `/graph/export?path=${encodeURIComponent(path)}&curves=true`);
  const live = r.json?.counts?.perPrefab ?? {};
  const lines = Object.entries(imported).map(([k, v]) => `${k} ${live[k] ?? 0}/${v}`);
  const text = `graph ${r.json?.counts?.segments ?? "?"} segments; live/imported edges: ${lines.join(", ")} (${path.split(/[\\/]/).pop()})`;
  console.log(`  ${text}`);
  return text;
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

outer: for (const [name, endpoint, files] of steps) {
  if (!wanted(name)) continue;
  if (endpoint === "verify") { const t0 = Date.now(); const text = await verifyGraph(name.replace("verify-", "")); rows.push({ name, file: "GET /graph/export", status: "ok", secs: (Date.now() - t0) / 1000, text }); continue; }
  for (const file of files) {
    if (!existsSync(file)) { console.log(`- ${name}: ${file} missing, skipped`); rows.push({ name, file, status: "missing" }); continue; }
    const body = JSON.parse(readFileSync(file, "utf8"));
    body.options ??= {};
    if (endpoint === "/import/network") { body.options.nodesFirst = true; body.options.bypassValidation = true; }
    if (endpoint === "/import/objects" || endpoint === "/import/surfaces") body.options.bypassValidation = true;
    if (dryRun) { body.options.dryRun = true; body.dryRun = true; }
    body.name ??= file.split(/[\\/]/).pop().replace(/\.json$/, "");
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
    rows.push({ name, file, status: r.json.completed === false ? `aborted: ${r.json.abortReason}` : "ok", secs: r.json.seconds ?? secs, importId: r.json.importId, undo: undoOf(endpoint, r.json, name), text: s.text, fails });
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

const total = rows.reduce((s, r) => s + (r.secs ?? 0), 0);
const md = [`# Import run ${stamp} (${scale}, sea ${sea} → ${seaOut}${dryRun ? ", dry run" : ""})`, "",
  `Bridge ${bridge.origin}; files from \`${dir}\` (trees from \`${treesDir}\`). ${stopped ? `**Stopped at ${stopped}: body rejected.**` : "Completed."} Total ${fmt(total)}.`, "",
  "Undo in reverse order of the table (each import can only be undone while it is the newest of its kind).", "",
  "| step | file | result | seconds | importId | undo |", "|---|---|---|---:|---|---|",
  ...rows.map(r => `| ${r.name} | \`${r.file}\` | ${r.text ?? r.status}${r.error ? ` ${r.error}` : ""} | ${(r.secs ?? 0).toFixed(1)} | ${r.importId ?? ""} | ${r.undo ?? ""} |`), ""];
for (const r of rows.filter(r => r.fails?.length)) md.push(`## Failures: ${r.name} \`${r.file}\``, "", ...r.fails.map(f => `- ${f.id} ${f.prefab}: ${f.error}`), "");
for (const pr of problems) md.push(`## Rejected body: ${pr.name} \`${pr.file}\` (HTTP ${pr.status})`, "", ...pr.problems.map(x => `- ${typeof x === "string" ? x : JSON.stringify(x)}`), "");
const report = join(dir, `run-${stamp}.md`);
writeFileSync(report, md.join("\n"));
console.log(`report ${report}`);
process.exit(stopped ? 1 : 0);
