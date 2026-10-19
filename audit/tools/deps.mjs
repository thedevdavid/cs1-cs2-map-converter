// Cross-check every Workshop id referenced by the DakotaExport dump (and optional extra id lists)
// against the Workshop folder on disk and the Skyve playset.
// Usage: node deps.mjs <dump.json> <playset.json> <workshopDir> [extra.json] [steam-cache.json] [--json]
//   extra.json: { "<source label>": ["<id or prefab name>", ...], ... }
import fs from 'node:fs';
import path from 'node:path';

const pos = process.argv.slice(2).filter(a => !a.startsWith('--'));
const [dumpPath, playsetPath, wsDir, extraPath, cachePath] = pos;
const titles = cachePath ? JSON.parse(fs.readFileSync(cachePath, 'utf8')).details ?? {} : {};
const dump = JSON.parse(fs.readFileSync(dumpPath, 'utf8'));
const playset = JSON.parse(fs.readFileSync(playsetPath, 'utf8'));
const inPlayset = new Map();
for (const a of playset.Assets ?? []) inPlayset.set(String(a.SteamId), 'asset');
for (const m of playset.Mods ?? []) inPlayset.set(String(m.SteamId), m.Enabled === false ? 'mod (disabled)' : 'mod');

const refs = new Map(); // id -> { sources:Set, count, samples:Set }
function add(name, source, n = 1) {
  const m = /^(\d{6,})[.]/.exec(name) ?? /^(\d{6,})$/.exec(name);
  if (!m) return;
  const r = refs.get(m[1]) ?? { sources: new Set(), count: 0, samples: new Set() };
  r.sources.add(source); r.count += n; if (r.samples.size < 2) r.samples.add(name.slice(m[1].length + 1) || name);
  refs.set(m[1], r);
}
for (const n of dump.nodes ?? []) add(n.prefab, 'net');
for (const s of dump.segments ?? []) add(s.prefab, 'net');
for (const b of dump.buildings ?? []) add(b.prefab, 'building');
for (const [p, v] of Object.entries(dump.trees?.byPrefab ?? {})) add(p, 'tree', v.count);
for (const p of dump.props?.points ?? []) add(p.prefab, 'prop');
for (const [p, v] of Object.entries(dump.props?.byPrefab ?? {})) add(p, 'prop', v.count);
if (extraPath) for (const [src, list] of Object.entries(JSON.parse(fs.readFileSync(extraPath, 'utf8')))) for (const x of list) add(String(x), src);

const rows = [...refs.entries()].map(([id, r]) => {
  const dir = path.join(wsDir, id);
  const onDisk = fs.existsSync(dir);
  return { id, title: titles[id]?.title ?? '', onDisk, playset: inPlayset.get(id)?.trim() ?? '', sources: [...r.sources].join(','), count: r.count, sample: [...r.samples].join(' / ') };
}).sort((a, b) => (a.onDisk && a.playset ? 1 : 0) - (b.onDisk && b.playset ? 1 : 0) || a.id.localeCompare(b.id));

if (process.argv.includes('--json')) console.log(JSON.stringify(rows, null, 1));
else {
  console.log(`| id | title | on disk | in playset | referenced by | count | example |\n|---|---|---|---|---|---:|---|`);
  for (const r of rows) console.log(`| ${r.id} | ${r.title} | ${r.onDisk ? 'yes' : '**NO**'} | ${r.playset || '**NO**'} | ${r.sources} | ${r.count} | ${r.sample} |`);
  const bad = rows.filter(r => !r.onDisk || !r.playset);
  console.error(`${rows.length} ids, ${bad.length} not on disk or not in playset`);
}
