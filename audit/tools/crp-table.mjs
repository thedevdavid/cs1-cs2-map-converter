// Read the asset table of a Cities: Skylines 1 .crp package (ColossalFramework.Packaging, format v10)
// and pull readable strings out of the metadata assets.
// Usage: node crp-table.mjs <file.crp> [--json] [--strings <assetName>] [--dump <assetName> <out.bin>]
import fs from 'node:fs';

const TYPE = { 0: 'Unknown', 1: 'Object', 2: 'Material', 3: 'Texture', 4: 'StaticMesh', 5: 'Text', 6: 'Assembly', 7: 'Data', 8: 'Locale', 52: 'GameData (simulation stream)', 80: 'UserLocale' };
// codes inferred from the asset contents of v5 and v10-12 packages (previews = 3, map metadata = 100, save metadata = 101)
const USER = { 100: 'MapMetaData', 101: 'SaveGameMetaData', 102: 'SystemMapMetaData', 103: 'CustomAssetMetaData', 104: 'ColorCorrection', 105: 'DistrictStyleMetaData', 106: 'MapThemeMetaData' };

export function readTable(file) {
  const fd = fs.openSync(file, 'r');
  const head = Buffer.alloc(1 << 20);
  fs.readSync(fd, head, 0, head.length, 0);
  let p = 0;
  const u8 = () => head[p++];
  const len7 = () => { let r = 0, s = 0, c; do { c = u8(); r |= (c & 0x7f) << s; s += 7; } while (c & 0x80); return r; };
  const str = () => { const n = len7(); const v = head.toString('utf8', p, p + n); p += n; return v; };
  const magic = head.toString('latin1', 0, 4); p = 4;
  if (magic !== 'CRAP') throw new Error(`${file}: not a CRAP package (${magic})`);
  const version = head.readUInt16LE(p); p += 2;
  const packageName = str();
  const author = version >= 3 ? str() : null;
  const packageVersion = head.readUInt32LE(p); p += 4;
  const mainAsset = str();
  const count = head.readInt32LE(p); p += 4;
  const dataOffset = Number(head.readBigInt64LE(p)); p += 8;
  const assets = [];
  for (let i = 0; i < count; i++) {
    const name = str(), checksum = str();
    const typeRaw = head.readInt32LE(p); p += 4;
    const offset = Number(head.readBigInt64LE(p)); p += 8;
    const size = Number(head.readBigInt64LE(p)); p += 8;
    assets.push({ name, checksum, typeRaw, type: TYPE[typeRaw] ?? USER[typeRaw] ?? `?${typeRaw}`, offset: dataOffset + offset, size });
  }
  const st = fs.fstatSync(fd);
  fs.closeSync(fd);
  return { file, fileSize: st.size, mtime: st.mtime.toISOString(), magic, version, packageName, author, packageVersion, mainAsset, dataOffset, assets };
}

export function readAsset(file, asset) {
  const fd = fs.openSync(file, 'r');
  const b = Buffer.alloc(asset.size);
  fs.readSync(fd, b, 0, asset.size, asset.offset);
  fs.closeSync(fd);
  return b;
}

// Length-prefixed (7-bit) UTF-8 strings of printable characters, the way BinaryWriter.Write(string) stores them.
export function lpStrings(b, min = 2) {
  const out = [];
  for (let i = 0; i < b.length - 1; i++) {
    const n = b[i];
    if (n < min || n > 127 || i + 1 + n > b.length) continue;
    const s = b.toString('utf8', i + 1, i + 1 + n);
    if (/^[\x20-\x7e]+$/.test(s)) { out.push({ at: i, s }); i += n; }
  }
  return out;
}

if (import.meta.url === `file:///${process.argv[1].replace(/\\/g, '/')}` || process.argv[1].endsWith('crp-table.mjs')) {
  const [file, ...rest] = process.argv.slice(2);
  const t = readTable(file);
  if (rest[0] === '--json') { console.log(JSON.stringify(t, null, 1)); process.exit(0); }
  if (rest[0] === '--strings') {
    const a = t.assets.find(x => x.name === rest[1]);
    for (const s of lpStrings(readAsset(file, a))) console.log(s.at, s.s);
    process.exit(0);
  }
  if (rest[0] === '--dump') {
    const a = t.assets.find(x => x.name === rest[1]);
    fs.writeFileSync(rest[2], readAsset(file, a));
    process.exit(0);
  }
  console.log(`${t.file}\n  size ${t.fileSize}  mtime ${t.mtime}  v${t.version}  package ${t.packageName}  main ${t.mainAsset}  assets ${t.assets.length}`);
  for (const a of t.assets) console.log(`  ${a.type.padEnd(20)} ${String(a.typeRaw).padStart(4)} ${String(a.size).padStart(10)}  ${a.name}`);
}
