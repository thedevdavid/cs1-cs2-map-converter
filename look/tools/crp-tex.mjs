// Extract a texture from a CS1 .crp (theme) as PNG: DDS DXT1/DXT5, top mip, decoded to RGBA.
// Usage: node look/tools/crp-tex.mjs <file.crp> <assetName> <out.png> [--stats]
// Also exports decode() / writePng() / readCrpTexture() for the other look tools.
import fs from 'node:fs';
import zlib from 'node:zlib';

export function readCrpTexture(crpPath, assetName) {
  const b = fs.readFileSync(crpPath);
  let p = 4;
  const u8 = () => b[p++];
  const str = () => { let len = 0, sh = 0, x; do { x = u8(); len |= (x & 0x7f) << sh; sh += 7; } while (x & 0x80); const s = b.toString('utf8', p, p + len); p += len; return s; };
  const i32 = () => { const v = b.readInt32LE(p); p += 4; return v; };
  const i64 = () => { const v = Number(b.readBigInt64LE(p)); p += 8; return v; };
  p += 2; str(); str(); i32(); str();
  const count = i32(), contentBegin = i64();
  const assets = [];
  for (let i = 0; i < count; i++) assets.push({ name: str(), checksum: str(), type: i32(), offset: i64(), size: i64() });
  const a = assets.find(x => x.name === assetName && x.type === 3);
  if (!a) throw new Error(`${assetName} not in ${crpPath}: ${assets.filter(x => x.type === 3).map(x => x.name).join(', ')}`);
  const blob = b.subarray(contentBegin + a.offset, contentBegin + a.offset + a.size);
  const d = blob.indexOf('DDS ');
  if (d < 0) throw new Error(`${assetName}: no DDS payload`);
  const h = blob.readUInt32LE(d + 12), w = blob.readUInt32LE(d + 16), fourcc = blob.toString('latin1', d + 84, d + 88);
  return { w, h, fourcc, data: blob.subarray(d + 128) };
}

const c565 = v => [((v >> 11) & 31) * 255 / 31, ((v >> 5) & 63) * 255 / 63, (v & 31) * 255 / 31];

export function decode({ w, h, fourcc, data }) {
  if (fourcc !== 'DXT5' && fourcc !== 'DXT1') throw new Error('unsupported ' + fourcc);
  const bs = fourcc === 'DXT1' ? 8 : 16, out = Buffer.alloc(w * h * 4);
  const bw = Math.max(1, w / 4), bh = Math.max(1, h / 4);
  for (let by = 0; by < bh; by++) for (let bx = 0; bx < bw; bx++) {
    const o = (by * bw + bx) * bs;
    const alpha = new Array(16).fill(255);
    let co = o;
    if (bs === 16) {
      const a0 = data[o], a1 = data[o + 1];
      const pal = [a0, a1];
      if (a0 > a1) for (let i = 1; i <= 6; i++) pal.push(((7 - i) * a0 + i * a1) / 7);
      else { for (let i = 1; i <= 4; i++) pal.push(((5 - i) * a0 + i * a1) / 5); pal.push(0, 255); }
      let bits = 0n; for (let i = 0; i < 6; i++) bits |= BigInt(data[o + 2 + i]) << BigInt(8 * i);
      for (let i = 0; i < 16; i++) alpha[i] = pal[Number((bits >> BigInt(3 * i)) & 7n)];
      co = o + 8;
    }
    const v0 = data.readUInt16LE(co), v1 = data.readUInt16LE(co + 2);
    const e0 = c565(v0), e1 = c565(v1);
    const cols = [e0, e1];
    if (bs === 16 || v0 > v1) { cols.push(e0.map((x, i) => (2 * x + e1[i]) / 3), e0.map((x, i) => (x + 2 * e1[i]) / 3)); }
    else { cols.push(e0.map((x, i) => (x + e1[i]) / 2), [0, 0, 0]); }
    const idx = data.readUInt32LE(co + 4);
    for (let i = 0; i < 16; i++) {
      const x = bx * 4 + (i & 3), y = by * 4 + (i >> 2);
      if (x >= w || y >= h) continue;
      const c = cols[(idx >> (2 * i)) & 3], q = (y * w + x) * 4;
      out[q] = c[0]; out[q + 1] = c[1]; out[q + 2] = c[2];
      out[q + 3] = (bs === 8 && v0 <= v1 && ((idx >> (2 * i)) & 3) === 3) ? 0 : alpha[i];
    }
  }
  return { w, h, rgba: out };
}

const crcTable = new Int32Array(256).map((_, n) => { let c = n; for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1; return c; });
const crc32 = buf => { let c = -1; for (let i = 0; i < buf.length; i++) c = crcTable[(c ^ buf[i]) & 255] ^ (c >>> 8); return (c ^ -1) >>> 0; };
const chunk = (type, data) => { const len = Buffer.alloc(4); len.writeUInt32BE(data.length); const td = Buffer.concat([Buffer.from(type, 'latin1'), data]); const crc = Buffer.alloc(4); crc.writeUInt32BE(crc32(td)); return Buffer.concat([len, td, crc]); };

export function writePng(path, { w, h, rgba }, channels = 4) {
  const raw = Buffer.alloc((w * channels + 1) * h);
  for (let y = 0; y < h; y++) {
    raw[y * (w * channels + 1)] = 0;
    for (let x = 0; x < w; x++) for (let c = 0; c < channels; c++) raw[y * (w * channels + 1) + 1 + x * channels + c] = rgba[(y * w + x) * 4 + c];
  }
  const ihdr = Buffer.alloc(13); ihdr.writeUInt32BE(w, 0); ihdr.writeUInt32BE(h, 4); ihdr[8] = 8; ihdr[9] = channels === 4 ? 6 : 2;
  fs.writeFileSync(path, Buffer.concat([Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]), chunk('IHDR', ihdr), chunk('IDAT', zlib.deflateSync(raw, { level: 6 })), chunk('IEND', Buffer.alloc(0))]));
}

export function stats({ w, h, rgba }) {
  const s = [0, 0, 0, 0], mn = [255, 255, 255, 255], mx = [0, 0, 0, 0], n = w * h;
  for (let i = 0; i < n; i++) for (let c = 0; c < 4; c++) { const v = rgba[i * 4 + c]; s[c] += v; if (v < mn[c]) mn[c] = v; if (v > mx[c]) mx[c] = v; }
  return { mean: s.map(x => +(x / n).toFixed(1)), min: mn, max: mx };
}

if ((process.argv[1] ?? '').split('\\').join('/').endsWith('look/tools/crp-tex.mjs')) {
  const [crp, name, out] = process.argv.slice(2);
  if (crp && name && out) {
    const img = decode(readCrpTexture(crp, name));
    writePng(out, img);
    if (process.argv.includes('--stats')) console.log(name, img.w + 'x' + img.h, JSON.stringify(stats(img)));
  }
}
