// Extract the mod-data payloads (SimulationManager serializable data) from a CS1 map/save .crp.
// Usage: node src/extract.mjs <file.crp> <outDir>
// Writes <outDir>/<key>.bin (gzip payloads unzipped to <key>.unz.bin) and <outDir>/index.json.
//
// Format notes (CS1 1.21, found empirically; see audit/local.md):
// - .crp: "CRAP", UInt16 version, strings (7-bit length prefix) package name, author (v>=3), UInt32 version,
//   main asset, Int32 count, Int64 data offset, then per asset: name, checksum, Int32 type, Int64 offset, Int64 size.
// - The map/save data asset (type 52) is a ColossalFramework DataSerializer stream. Its strings are 0x80,
//   UInt16 BE length, then each char as the byte delta to the previous one (start 0x80).
// - After the manager type list: Int32 BE entry count, then per entry key string, UInt24 BE length, payload.
import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';

export function readTable(file) {
  const fd = fs.openSync(file, 'r');
  const head = Buffer.alloc(1 << 20);
  fs.readSync(fd, head, 0, head.length, 0);
  fs.closeSync(fd);
  let p = 0;
  const len7 = () => { let r = 0, s = 0, c; do { c = head[p++]; r |= (c & 0x7f) << s; s += 7; } while (c & 0x80); return r; };
  const str = () => { const n = len7(); const v = head.toString('utf8', p, p + n); p += n; return v; };
  if (head.toString('latin1', 0, 4) !== 'CRAP') throw new Error(`${file}: not a CS1 package`);
  p = 4;
  const version = head.readUInt16LE(p); p += 2;
  const packageName = str();
  if (version >= 3) str();
  p += 4;
  const mainAsset = str();
  const count = head.readInt32LE(p); p += 4;
  const dataOffset = Number(head.readBigInt64LE(p)); p += 8;
  const assets = [];
  for (let i = 0; i < count; i++) {
    const name = str(); str();
    const type = head.readInt32LE(p); p += 4;
    const offset = Number(head.readBigInt64LE(p)); p += 8;
    const size = Number(head.readBigInt64LE(p)); p += 8;
    assets.push({ name, type, offset: dataOffset + offset, size });
  }
  return { version, packageName, mainAsset, assets };
}

function readStr(b, p) {
  if (b[p] !== 0x80) return null;
  const L = b.readUInt16BE(p + 1);
  let c = 0x80, s = '';
  for (let k = 0; k < L; k++) { c = (c + b[p + 3 + k]) & 255; if (c < 0x20 || c > 0x7e) return null; s += String.fromCharCode(c); }
  return { s, end: p + 3 + L };
}

export function modData(file) {
  const t = readTable(file);
  const a = t.assets.find(x => x.type === 52);
  if (!a) throw new Error(`${file}: no data asset (type 52)`);
  const b = Buffer.alloc(a.size);
  const fd = fs.openSync(file, 'r'); fs.readSync(fd, b, 0, a.size, a.offset); fs.closeSync(fd);
  let p = 0, pm = null;
  for (; p < 8192; p++) { const r = b[p] === 0x80 && readStr(b, p); if (r && r.s.startsWith('PathManager+Data')) { pm = r; break; } }
  if (!pm) throw new Error('manager type list not found');
  p = pm.end;
  while (p < 16384 && !(b[p] === 0x80 && b[p + 1] === 0 && readStr(b, p))) p++;
  const count = b.readInt32BE(p - 4);
  const entries = [];
  for (let i = 0; i < count; i++) {
    const k = readStr(b, p);
    if (!k) throw new Error(`entry ${i}: no key at ${p}`);
    const len = b.readUIntBE(k.end, 3);
    entries.push({ key: k.s, size: len, data: b.subarray(k.end + 3, k.end + 3 + len) });
    p = k.end + 3 + len;
  }
  return { dataAsset: a.name, dataSize: a.size, entries };
}

if (process.argv[1] && path.resolve(process.argv[1]) === path.resolve(new URL(import.meta.url).pathname.replace(/^\/(\w:)/, '$1'))) {
  const [file, outDir] = process.argv.slice(2);
  fs.mkdirSync(outDir, { recursive: true });
  const r = modData(file);
  const index = [];
  for (const e of r.entries) {
    let d = e.data, name = e.key.replace(/[^\w.-]/g, '_');
    if (d[0] === 0x1f && d[1] === 0x8b) { d = zlib.gunzipSync(d); name += '.unz'; }
    fs.writeFileSync(path.join(outDir, `${name}.bin`), d);
    index.push({ key: e.key, file: `${name}.bin`, storedBytes: e.size, writtenBytes: d.length });
  }
  fs.writeFileSync(path.join(outDir, 'index.json'), JSON.stringify({ source: file, dataAsset: r.dataAsset, dataSize: r.dataSize, entries: index }, null, 1));
  console.log(`${index.length} mod-data payloads from ${r.dataAsset} -> ${outDir}`);
}
