// Parse the LSM Assets Browser report of the Dakota Falls load.
// Row: [index, type, status, ?, size, flags, workshopId, name, usedByIndices]
// type 0 building, 1 prop, 2 tree, 3 vehicle, 9 network; status 1 ok, 2 missing;
// flags bit 2 = used in city. Needed = used in city, plus every asset used (transitively) by a needed one.
import fs from 'node:fs';

const F = process.env.LOCALAPPDATA + '/Colossal Order/Cities_Skylines/Report/LoadingScreenMod/Dakota Falls - Assets Browser-2026-08-17_06-26-25.htm';
const s = fs.readFileSync(F, 'utf8');
const i = s.indexOf('const d=');
const rows = JSON.parse(s.slice(i + 8, s.indexOf(']]]', i) + 3));

const needed = new Set(rows.filter(r => r[5] & 2).map(r => r[0]));
let changed = true;
while (changed) {
  changed = false;
  for (const r of rows) if (!needed.has(r[0]) && r[8].some(u => needed.has(u))) { needed.add(r[0]); changed = true; }
}
const TYPES = { 0: 'building', 1: 'prop', 2: 'tree', 3: 'vehicle', 9: 'network' };
const byId = {};
for (const r of rows) {
  if (!needed.has(r[0])) continue;
  const id = String(r[6]);
  const e = byId[id] ??= { id, names: [], types: {}, missing: 0, usedInCity: false };
  e.names.push(r[7]);
  e.types[TYPES[r[1]] ?? r[1]] = (e.types[TYPES[r[1]] ?? r[1]] || 0) + 1;
  if (r[2] === 2) e.missing++;
  if (r[5] & 2) e.usedInCity = true;
}
const out = { totalRows: rows.length, neededRows: needed.size, usedInCityRows: rows.filter(r => r[5] & 2).length, ids: byId };
fs.writeFileSync(new URL('./lsm.json', import.meta.url), JSON.stringify(out, null, 1));
console.log(rows.length, 'rows;', needed.size, 'needed rows;', Object.keys(byId).length, 'needed workshop ids');
