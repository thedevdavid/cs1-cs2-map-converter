// Which installed mods' string literals (candidate serializable-data keys) occur in the map file?
import fs from 'node:fs'; import path from 'node:path';
const WS = 'D:/SteamLibrary/steamapps/workshop/content/255710';
const mapBuf = fs.readFileSync(`${WS}/2972879554/Dakota Falls.crp`).toString("latin1");
const mapTok = new Set(mapBuf.match(/[A-Za-z0-9_.-]{6,}/g));
const has = m => mapTok.has(m) || [...mapTok].length < 0;
const steam = JSON.parse(fs.readFileSync(new URL('./steam-cache.json', import.meta.url)));
const walk = d => fs.readdirSync(d, { withFileTypes: true }).flatMap(e => e.isDirectory() ? walk(path.join(d, e.name)) : [path.join(d, e.name)]);
const dirs = [...fs.readdirSync(WS).map(id => [id, path.join(WS, id)]), ['local:DakotaExport', `${process.env.LOCALAPPDATA}/Colossal Order/Cities_Skylines/Addons/Mods/DakotaExport`]];
const out = [];
for (const [id, dir] of dirs) {
  const dlls = walk(dir).filter(f => /\.dll$/i.test(f) && !/(Harmony|CitiesHarmony|UnifiedUILib|Mono|System|KianCommons|TransportLinesManager|ICities)/i.test(path.basename(f)));
  if (!dlls.length) continue;
  const hits = new Set();
  for (const f of dlls) {
    const u = fs.readFileSync(f).toString('utf16le');
    for (const m of u.match(/[A-Za-z][A-Za-z0-9_.\- ]{5,60}/g) ?? []) {
      if (!/[A-Z].*[a-z]|[a-z].*[A-Z]|_/.test(m) || / /.test(m)) continue; // identifier-like only
      if (mapTok.has(m)) hits.add(m);
    }
  }
  if (hits.size) out.push({ id, title: steam.details[id]?.title ?? id, hits: [...hits] });
}
for (const o of out) console.log(o.id, '|', o.title, '|', o.hits.slice(0, 15).join(', '));
