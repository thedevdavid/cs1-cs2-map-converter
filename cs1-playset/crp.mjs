// Minimal CS1 .crp (CRAP) header reader: lists assets with type/offset/size.
import fs from 'node:fs';
const b = fs.readFileSync(process.argv[2]);
let p = 0;
const u8 = () => b[p++];
const str = () => { let len = 0, shift = 0, x; do { x = u8(); len |= (x & 0x7f) << shift; shift += 7; } while (x & 0x80); const s = b.toString('utf8', p, p + len); p += len; return s; };
const u16 = () => { const v = b.readUInt16LE(p); p += 2; return v; };
const i32 = () => { const v = b.readInt32LE(p); p += 4; return v; };
const i64 = () => { const v = Number(b.readBigInt64LE(p)); p += 8; return v; };
const magic = b.toString('latin1', 0, 4); p = 4;
const version = u16();
const pkgName = str(); const author = str(); const pkgVersion = i32(); const main = str(); const count = i32(); const contentBegin = i64();
console.log({ magic, version, pkgName, author, pkgVersion, main, count, contentBegin });
const assets = [];
for (let i = 0; i < count; i++) assets.push({ name: str(), checksum: str(), type: i32(), offset: i64(), size: i64() });
for (const a of assets) {
  const start = contentBegin + a.offset;
  console.log(a.type, a.name, a.size, b.subarray(start, start + 16).toString('hex'));
}
if (process.argv[3]) { const a = assets.find(x => x.name === process.argv[3]); fs.writeFileSync(process.argv[4], b.subarray(contentBegin + a.offset, contentBegin + a.offset + a.size)); console.log('wrote', a.name); }
