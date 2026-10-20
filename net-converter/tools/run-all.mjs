// One command for a full conversion: both scales, then out/ROUND.md (file list, counts, import order).
// node tools/run-all.mjs [dump.json] [--heightmap ../hm-rebuild/out/DakotaFalls-1to1-4096.png] [--out out] [--water-variant 93.0]
// Default dump: the newest *.json in %LOCALAPPDATA%\DakotaExport that is not a -trees/-po sibling.
import { execFileSync } from "node:child_process";
import { existsSync, readdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";

const args = process.argv.slice(2);
const opt = (name, def) => { const i = args.indexOf(name); return i >= 0 ? args.splice(i, 2)[1] : def; };
const heightmap = opt("--heightmap", "../hm-rebuild/out/DakotaFalls-1to1-4096.png");
const out = opt("--out", "out");
const waterVariant = opt("--water-variant", "93.0"); // second sea level (CS1 m) next to 97.5
let dump = args[0];
if (!dump) {
  const dir = join(process.env.LOCALAPPDATA ?? "", "DakotaExport");
  dump = readdirSync(dir).filter(f => f.endsWith(".json") && !/-(trees|po)\.json$/.test(f))
    .map(f => join(dir, f)).sort((a, b) => statSync(b).mtimeMs - statSync(a).mtimeMs)[0];
}
console.log(`dump ${dump}`);

const run = extra => execFileSync("dotnet", ["run", "-c", "Release", "--project", "src/NetConverter", "--", dump, "--mapping", "mapping.tsv", "--water-variant", waterVariant,
  "--heightmap", heightmap, ...extra], { stdio: ["ignore", "inherit", "inherit"] });
const live = join(out, "live-graph.json");
run(["--out", out, ...(existsSync(live) ? ["--live", live, "--baseline", join(out, "none")] : [])]);
run(["--out", join(out, "x1"), "--height-factor", "1.0", "--sea-level", "97.5", "--trees-dir", join(out, "trees")]);

// Summary.
const J = f => JSON.parse(readFileSync(f, "utf8"));
const size = f => `${(statSync(f).size / 1048576).toFixed(1)} MB`;
const rows = [];
const add = (order, file, what, count) => { if (existsSync(file)) rows.push(`| ${order} | \`${file}\` | ${what} | ${count} | ${size(file)} |`); };
for (const [dir, scale] of [[out, "2×"], [join(out, "x1"), "1×"]]) {
  const p = f => join(dir, f);
  if (existsSync(p("import-all.json"))) { const a = J(p("import-all.json")); add(1, p("import-all.json"), `network (${scale})`, `${a.edges.length} edges, ${a.nodes.length} nodes`); }
  if (existsSync(p("import-power.json"))) add(2, p("import-power.json"), `power (${scale})`, `${J(p("import-power.json")).edges.length} edges`);
  const byPrefab = edges => Object.entries(edges.reduce((m, e) => (m[e.prefab] = (m[e.prefab] || 0) + 1, m), {})).map(([k, v]) => `${k} ${v}`).join(", ");
  for (const [f, what] of [["import-canals.json", "canals and flood walls"], ["import-shippaths.json", "ship paths"]])
    if (existsSync(p(f))) { const j = J(p(f)); add(2, p(f), `${what} (${scale})`, `${j.edges.length} edges: ${byPrefab(j.edges)}`); }
  if (existsSync(p("import-walls-props.json"))) add(5, p("import-walls-props.json"), `barrier props (${scale})`, `${J(p("import-walls-props.json")).objects.length}`);
  for (const [f, what] of [["import-surfaces-fields.json", "field surfaces"], ["import-surfaces-sand.json", "paint plan sand: Sand Surface 01 banks, Sand Surface 02 bare earth + decal spots"], ["import-surfaces-grass-dry.json", "paint plan dry grass (Grass Surface 02)"], ["import-surfaces-pavement.json", "paint plan pavement under stand-in buildings"], ["import-surfaces-grass.json", "grass surfaces (optional alternative to the grass plants)"]])
    if (existsSync(p(f))) add(8, p(f), what, `${J(p(f)).surfaces.length} polygons`);
  if (existsSync(p("import-water.json"))) { const w = J(p("import-water.json")); add(3, p("import-water.json"), `water (${scale}), seaLevel ${w.seaLevel}`, `${w.sources.length} sources, ${w.sources.filter(s => s.refinedFrom).length} refined`); }
  for (const vf of existsSync(dir) ? readdirSync(dir).filter(x => /^import-water-sea[\d.]+\.json$/.test(x)) : []) { const w = J(p(vf)); add(3, p(vf), `water VARIANT (${scale}), seaLevel ${w.seaLevel}: import instead of import-water.json to try the measured level`, `${w.sources.length} sources`); }
  if (existsSync(p("import-connections.json"))) add(4, p("import-connections.json"), "outside connections", `${J(p("import-connections.json")).objects.length}`);
  if (existsSync(p("import-objects.json")))
  {
    const o = J(p("import-objects.json")).objects;
    const kinds = Object.entries(o.reduce((m, x) => (m[x.kind] = (m[x.kind] || 0) + 1, m), {})).map(([k, v]) => `${k} ${v}`).join(", ");
    add(5, p("import-objects.json"), `objects (${scale})`, `${o.length} (${kinds}; ${o.filter(x => "y" in x).length} with y)`);
  }
  for (const ff of existsSync(dir) ? readdirSync(dir).filter(x => x.startsWith("import-trees-fixed-")) : []) add(6, p(ff), `trees at fixed height, with y (${scale})`, `${J(p(ff)).objects.length}`);
  // Round 8 (owner review of Run A): each its own import, to add to the live map.
  for (const [f, order, what] of [["import-power-ends.json", 2, "ROUND 8 power-line ends at the substation and the dam (A-v6: dam end on the plant marker, extra pylon), connectTo high voltage radius 60"], ["import-dam-deck.json", 2, "ROUND 8 dam deck network, absolute heights (after import-dam.json)"],
    ["import-substation.json", 5, "ROUND 8 substation building TransformerStation01"], ["import-po-rocks.json", 5, "po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds"],
    ["import-rock-scatter.json", 5, "ROUND 8 vanilla rock scatter"],
    ["import-po-structure-objects.json", 5, "ROUND 20 PO structures: BR006 arch (StoneArchRailBridge), ConcreteSet01 columns / piers, BR022 pads; runner --po-structures (mods 154132, 135944)"],
    [join("optional", "import-po-structure-gantry.json"), 5, "ROUND 20 optional gantry HWY Truss Gantry 2L  A (132188), runner --with po-gantry"],
    [join("optional", "import-po-structure-piers.json"), 5, "ROUND 20 optional ITBP piers BR001 / BR003 (140978), only with noPillars, runner --with po-piers"]])
    if (existsSync(p(f))) { const j = J(p(f)); add(order, p(f), `${what} (${scale})`, j.objects ? `${j.objects.length}` : `${j.edges.length} edges${j.replaces ? `, replaces ${j.replaces.join(", ")}` : ""}`); }
  // Bridges (bridges.tsv).
  for (const [f, order, what] of [["import-bridges.json", 1, "bridge overrides (right after import-all.json, which leaves these decks out)"], ["import-bridges-fixed.json", 1, "fixed-span bridges: BR001 Detroit suspension (one edge) + its 3-lane connectors, right after import-bridges.json"], ["import-dam.json", 1, "dam: one Hydroelectric_Power_Plant_01 Dam edge (bridge 0.9.23 builds the plant), after the bridges"]])
    if (existsSync(p(f))) { const j = J(p(f)); add(order, p(f), `${what} (${scale})`, `${j.edges.length} edges, replaces ${j.replaces?.length ?? 0}`); }
  // Round 9.
  for (const [f, order, what] of [["import-gaps.json", 1, "ROUND 9 network gaps (snapToSegment onto the live network)"], ["import-gaps-e15883-taper.json", 1, "ROUND 9 alternative for e15883_0 (4-lane taper); import instead of its gap_e15883_3lane"],
    ["import-po-walls.json", 2, "ROUND 9 QuaySmall01 sides of the PO paved blocks"], ["import-water-lakes.json", 3, "ROUND 9 lake sources for unexplained wet bodies (after the water file)"],
    ["import-fort.json", 5, "ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED)"]])
    if (existsSync(p(f))) { const j = J(p(f)); add(order, p(f), `${what} (${scale})`, j.objects ? `${j.objects.length}` : j.sources ? `${j.sources.length} sources` : `${j.edges.length} edges`); }
  for (const [f, what] of [["import-po-structures.json", "ROUND 9 pavement tops of the PO paved blocks"]])
    if (existsSync(p(f))) add(8, p(f), `${what} (${scale})`, `${J(p(f)).surfaces.length} polygons`);
  if (existsSync(p("import-po-candidates.json"))) add(5, p("import-po-candidates.json"), `Procedural Object candidates, UNVERIFIED (${scale})`, `${J(p("import-po-candidates.json")).objects.length}`);
  if (existsSync(p("import-resources.json"))) add(7, p("import-resources.json"), "natural resources", Object.keys(J(p("import-resources.json")).layers).join(", "));
  else if (existsSync(p("import-resources-synthetic.json"))) add(7, p("import-resources-synthetic.json"), "SYNTHETIC fertility (no naturalResources in the dump)", "fertility");
}
for (const dir of [out, join(out, "x1")])
  for (const [f, what] of [["import-surfaces-decals-unmerged.json", "EXPERIMENT: decal squares, not merged"], ["import-surfaces-grass-unmerged.json", "EXPERIMENT: grass clusters, not merged"]])
    if (existsSync(join(dir, f))) add(9, join(dir, f), what, `${J(join(dir, f)).surfaces.length} polygons`);
const tdir = join(out, "trees");
const ef = existsSync(tdir) ? readdirSync(tdir).filter(f => f.startsWith("experiment-trees-")).sort() : [];
if (ef.length) rows.push(`| 9 | \`${tdir}/${ef[0]}\` | EXPERIMENT: trees on ground-net corridors (both scales) | ${ef.reduce((s, f) => s + J(join(tdir, f)).objects.length, 0).toLocaleString("en")} | |`);
const tf = existsSync(tdir) ? readdirSync(tdir).filter(f => /^import-trees-\d\d\.json$/.test(f)).sort() : [];
if (tf.length) rows.push(`| 6 | \`${tdir}/${tf[0]}\` … \`${tf.at(-1)}\` | trees (both scales) | ${tf.reduce((s, f) => s + J(join(tdir, f)).objects.length, 0).toLocaleString("en")} in ${tf.length} files | |`);
rows.sort((a, b) => parseInt(a.slice(2)) - parseInt(b.slice(2)));
const extras = ["water-check.md", "surfaces-summary.md", "surfaces.geojson", "manual-objects.md", "connections-check.md", "stats.md"].filter(f => existsSync(join(out, f)));
const summary = (`# Conversion outputs (${new Date().toISOString().slice(0, 10)})

Dump: \`${dump}\`. Run it all with \`node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5]\`. Import order (bridges: import-bridges.json right after the network; round 10, owner review of A-v4: no quays, retaining walls, PO walls, monorail decor or dam files; dam by hand, see manual-objects.md): network → gaps → power, canals, ship paths → water (import-water.json at sea level 97.5 / 195, or the variant import-water-sea93.json / -sea186.json at the measured 93 / 186; try the variant first, fall back to 97.5) → connections → objects (incl. barrier props) → trees (shared files + the per-scale fixed-height file) → resources → dam (one network edge) after the bridges → … → surfaces (vanilla paint plan: fields, PO pavement, sand, dry grass; grass optional; the pack surfaces rock, PO rock, decals and pack sand are in optional/, runner --with) → experiments (order 9: net-corridor trees, unmerged decals and grass, instead of or on top of the default files); each file is its own import.
${out}/ = 2× heights (Height Scale 4096), ${out}/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
${rows.join("\n")}

Reports (both folders): ${extras.map(f => `\`${f}\``).join(", ")}.
`);
// Live results reported back from the game (hand-written, kept across reruns).
const liveResult = existsSync(join(out, "live-result.md")) ? "\n" + readFileSync(join(out, "live-result.md"), "utf8") : "";
for (const name of ["ROUND.md", "ROUND10.md"]) writeFileSync(join(out, name), summary + liveResult);
console.log(`summary: ${join(out, "ROUND10.md")}`);
