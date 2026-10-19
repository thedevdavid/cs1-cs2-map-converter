// Build the Dakota Falls editor audit + Skyve playset.
// Inputs: steam-cache.json (fetch.mjs), lsm.json (lsm.mjs), Skyve caches in %LOCALAPPDATA%\Skyve-CS1.
// Outputs: audit.json, DakotaFalls-Editor.json (copy), and the playset in Skyve\Profiles.
import fs from 'node:fs';
import path from 'node:path';

const WS = 'D:/SteamLibrary/steamapps/workshop/content/255710';
const LA = process.env.LOCALAPPDATA;
const CS1 = `${LA}/Colossal Order/Cities_Skylines`;
const SKYVE_CACHE = `${LA}/Skyve-CS1`;
const here = f => new URL(f, import.meta.url);
const readJson = f => JSON.parse(fs.readFileSync(f, 'utf8').replace(/^\uFEFF/, ''));

const steam = readJson(here('./steam-cache.json'));
const lsm = readJson(here('./lsm.json'));
const skyveMods = readJson(`${SKYVE_CACHE}/SteamModsCache.json`);
const compat = new Map(readJson(`${SKYVE_CACHE}/CompatibilityDataCache.json`).Packages.map(p => [String(p.SteamId), p]));
const dlcConfig = fs.readFileSync(`${CS1}/Skyve/DlcConfig.xml`, 'utf8');

const MAP_COLLECTION = '2970743270';
const THEME_COLLECTION = '2972871641';
const collection = steam.collections[MAP_COLLECTION].map(c => c.id);
const themeCollection = steam.collections[THEME_COLLECTION].map(c => c.id).filter(id => id !== MAP_COLLECTION);
// Themes named in the theme mix's UsedAssets.txt (2972881605) but absent from both collections.
const themeMixThemes = ['1305157510', '2858527761', '2954236385'];

// Broken / deprecated / removed originals -> the maintained re-upload that is installed.
const SUBSTITUTE = {
  '1094334744': '3071492847', // Procedural Objects -> Procedural Objects (Empty popup fix)
  '2669938594': '3683231517', // [BROKEN] Adaptive Networks -> Adaptive Networks (Emergency Fix)
  '1758376843': '3702700108', // [DEPRECATED] Network Skins -> Network Skins Continued
  // Deprecated mods declared as required items by assets; Skyve names Prop/Tree Control as successors.
  '625608461': '2956854293',  // Adaptive Prop Visibility Distance -> Prop Control
  '767233815': '2956854293',  // Decal Prop Fix -> Prop Control
  '593588108': '2956854293',  // Prop & Tree Anarchy -> Tree Control (trees) + Prop Control (props); both included
  '787611845': '2956854293',  // Prop Snapping -> Prop Control
  '1869561285': '2685506044', // Prop Painter 4.0 (removed) -> Prop Painter: Revisited (rock colours)
  '502750307': '3733110460',  // Extra Landscaping Tools -> ELT Revisited (ships the same NaturalResourcesBrush.dll)
  '445589127': '3680103097',  // Precision Engineering -> Precision Engineering (Harmony)
};

const QOL = {
  '2040656402': 'Harmony (dependency)',
  '2858591409': 'Loading Screen Mod Revisited',
  '2881031511': 'Skyve mod',
  '2041457644': 'Patch Loader Mod (Skyve dependency)',
  '1619685021': 'Move It',
  '2862881785': 'Network Anarchy',
  '2560782729': 'Network Multitool',
  '3680103097': 'Precision Engineering (Harmony)',
  '3733110460': 'Extra Landscaping Tools Revisited',
  '2956854293': 'Prop Control',
  '845665815': 'CSL Map View (export)',
};
// Mods the map description / data imply, beyond the collection.
const IMPLIED = {
  '1637663252': 'TM:PE (map carries TM:PE data)',
  '2960858317': 'Tree Control (740k trees; not Tree Anarchy)',
  '689937287': 'Surface Painter (DakotaExport reads its data; dependency ELT 502750307 is met by ELT Revisited, same NaturalResourcesBrush assembly)',
};
// Load-critical vs visual-only, for mods.
const VISUAL_ONLY = new Set(['2474567113', '3031623332', '2972881605', '1591417160', '1794015399', '1209581656', '1539181199']);

function walk(dir, base = dir, out = []) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) walk(p, base, out); else out.push(path.relative(base, p));
  }
  return out;
}
function localFiles(id) {
  const dir = path.join(WS, id);
  if (!fs.existsSync(dir)) return null;
  const files = walk(dir);
  return { crp: files.filter(f => /\.crp$/i.test(f)), dll: files.filter(f => /\.dll$/i.test(f)), all: files.length };
}
function title(id) { return steam.details[id]?.title ?? skyveMods[id]?.Name ?? compat.get(id)?.Name ?? '(unknown)'; }
function tags(id) { return steam.details[id]?.tags ?? skyveMods[id]?.Tags ?? []; }
function requires(id) {
  const fromSteam = skyveMods[id]?.RequiredPackageIds ?? [];
  const fromCompat = (compat.get(id)?.Interactions ?? []).filter(i => i.Type === 7).flatMap(i => i.Packages);
  return [...new Set([...fromSteam, ...fromCompat].map(String))];
}
function kind(id, files) {
  const t = tags(id);
  if (t.includes('Map')) return 'map';
  if (t.includes('Theme Mix')) return 'theme-mix';
  if (t.includes('Map Theme')) return 'theme';
  if (t.includes('Color Correction LUT')) return 'lut';
  if (files?.dll.length || t.includes('Mod')) return 'mod';
  return 'asset';
}

// Gather items with the reason each is included.
const items = new Map();
function add(id, reason) {
  id = String(id);
  const sub = SUBSTITUTE[id];
  if (sub) {
    const orig = items.get(id) ?? { id, reasons: [], substitutedBy: sub };
    orig.reasons.push(reason); orig.substitutedBy = sub; items.set(id, orig);
    return add(sub, `replaces ${id} (${title(id)})`);
  }
  const e = items.get(id);
  if (e) { if (!e.reasons.includes(reason)) e.reasons.push(reason); return false; }
  items.set(id, { id, reasons: [reason] });
  return true;
}
collection.forEach(id => add(id, 'map collection 2970743270'));
themeCollection.forEach(id => add(id, 'theme-mix collection 2972871641'));
themeMixThemes.forEach(id => add(id, 'theme mix UsedAssets.txt'));
for (const id of Object.keys(lsm.ids)) add(id, 'LSM: used by the map');
for (const [id, why] of Object.entries(IMPLIED)) add(id, why);
for (const [id, why] of Object.entries(QOL)) add(id, `QoL: ${why}`);

// Required-items closure to a fixed point (Steam required items + Skyve compatibility "required").
let changed = true;
while (changed) {
  changed = false;
  for (const e of [...items.values()]) {
    if (e.substitutedBy) continue;
    for (const r of requires(e.id)) if (add(r, `required by ${e.id} (${title(e.id)})`)) changed = true;
  }
}

const STABILITY_BROKEN = 5; // Skyve CS1: '[BROKEN] Adaptive Networks', 'Node controller V2.11', 'Network Extensions 2' all carry 5.
const audit = [];
for (const e of items.values()) {
  const files = localFiles(e.id);
  const c = compat.get(e.id);
  const statuses = (c?.Statuses ?? []).map(s => s.LocaleKey.replace('Status_', '') + (s.Packages?.length ? `->${s.Packages.join('/')}` : ''));
  const k = kind(e.id, files);
  audit.push({
    id: e.id, title: title(e.id), kind: k, tags: tags(e.id), reasons: e.reasons,
    substitutedBy: e.substitutedBy ?? null,
    present: !!files && (files.crp.length + files.dll.length > 0),
    crp: files?.crp.length ?? 0, dll: files?.dll.length ?? 0,
    removedFromWorkshop: steam.details[e.id]?.result === 9 || !!skyveMods[e.id]?.IsRemoved,
    skyveStability: c?.Stability ?? null, skyveStatuses: statuses,
    skyveBroken: c?.Stability === STABILITY_BROKEN,
    loadCritical: k === 'mod' ? !VISUAL_ONLY.has(e.id) && !e.reasons.every(r => r.startsWith('QoL')) : null,
    lsm: lsm.ids[e.id] ? { types: lsm.ids[e.id].types, missingAssets: lsm.ids[e.id].missing, usedInCity: lsm.ids[e.id].usedInCity } : null,
  });
}
audit.sort((a, b) => a.kind.localeCompare(b.kind) || a.title.localeCompare(b.title));

// DLC: the map page lists Mass Transit (547502) as required.
const owned = [...dlcConfig.matchAll(/<unsignedInt>(\d+)<\/unsignedInt>/g)].map(m => m[1]);
const ownedSection = dlcConfig.split('<OwnedDLCs>')[1] ?? '';
const dlc = [{ appId: '547502', name: 'Mass Transit', ownedPerSkyve: ownedSection.includes('<unsignedInt>547502</unsignedInt>'), removedInSkyve: /<RemovedDLCs>[\s\S]*547502[\s\S]*<\/RemovedDLCs>/.test(dlcConfig) }];

// Playset.
const usable = audit.filter(a => !a.substitutedBy && a.present && !a.skyveBroken);
const mods = usable.filter(a => a.dll > 0).map(a => ({ Enabled: true, RelativePath: `%WORKSHOP%\\${a.id}`, SteamId: Number(a.id) }));
mods.unshift({ Enabled: true, RelativePath: '%LOCALAPPDATA%\\Addons\\Mods\\DakotaExport', SteamId: 0 });
const assets = usable.flatMap(a => localFiles(a.id).crp.map(f => ({ RelativePath: `%WORKSHOP%\\${a.id}\\${f.split(path.sep).join('\\')}`, SteamId: Number(a.id) })));
const yo = readJson(`${CS1}/Skyve/Profiles/yo.json`);
const playset = {
  Name: 'DakotaFalls-Editor', Temporary: false, Assets: assets, Mods: mods, ExcludedDLCs: [],
  LaunchSettings: { ...yo.LaunchSettings, SaveToLoad: null, LoadSaveGame: false, MapToLoad: null, StartNewGame: false, NewAsset: false, LoadAsset: false },
  LsmSettings: { ...yo.LsmSettings },
  AutoSave: false, IsFavorite: false, Color: null, LastUsed: new Date().toISOString(), Usage: -1, Author: 0, ProfileId: 0,
  UnsavedChanges: false, BannerBytes: null, Public: false,
};

// Validate: every referenced path exists.
const resolve = rp => rp.replace('%WORKSHOP%', WS).replace('%LOCALAPPDATA%', CS1).replaceAll('\\', '/');
const bad = [...mods, ...assets].filter(x => !fs.existsSync(resolve(x.RelativePath)));
if (bad.length) { console.error('MISSING PATHS', bad); process.exit(1); }
if (!fs.existsSync(`${CS1}/Addons/Mods/DakotaExport/DakotaExport.dll`)) throw new Error('DakotaExport.dll missing');

const text = JSON.stringify(playset, null, 2);
JSON.parse(text);
fs.writeFileSync(here('./DakotaFalls-Editor.json'), text);
const target = `${CS1}/Skyve/Profiles/DakotaFalls-Editor.json`;
if (process.argv.includes('--write-profile')) fs.writeFileSync(target, text);
fs.writeFileSync(here('./audit.json'), JSON.stringify({ generated: new Date().toISOString(), dlc, items: audit, playset: { mods: mods.length, assetFiles: assets.length, workshopItems: usable.length } }, null, 1));

const missing = audit.filter(a => !a.substitutedBy && !a.present);
console.log('items', audit.length, 'usable', usable.length, 'mods', mods.length, 'asset files', assets.length);
console.log('by kind', Object.entries(Object.groupBy(usable, a => a.kind)).map(([k, v]) => `${k}:${v.length}`).join(' '));
console.log('MISSING:'); missing.forEach(a => console.log(' ', a.id, a.kind, a.title, a.removedFromWorkshop ? '(removed)' : '', '|', a.reasons.join('; ')));
console.log('BROKEN/flagged:'); audit.filter(a => a.skyveBroken || a.skyveStatuses.length).forEach(a => console.log(' ', a.id, a.title, a.skyveStability, a.skyveStatuses.join(',')));
console.log('DLC', JSON.stringify(dlc));
