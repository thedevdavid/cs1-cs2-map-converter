// Packed Extra-slot "Normal" maps for the CS2 terrain importer: RG = source tangent normal XY, B/A = height or AO.
// The game reads the four Extra slots' Normal texture as a packed map; which of B/A is height is unknown, so the
// layout is selectable per slot for an A/B test. Height = diffuse luminance (v3 *.s66 BaseColor), box blur r=2 with
// wrap-around (tile stays seamless), percentile-normalized (p1 -> 0, p99 -> 255). current/ and *.s66 are never written.
// Layouts: ah = A height, B 255 (AO neutral); bh = B height, A 128.
// Usage: node look/tools/make-packed.mjs --tag .h1 [--layout ah] [--slots Extra1,Extra2,Extra3,Extra4] [--slot-layout Extra1=ah,Extra3=bh]
// Slots not in --slots get their Normal copied unchanged (24-bit). All 7 BaseColor.s66 files are copied to <slot>_BaseColor<tag>.png.
import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import { fileURLToPath } from 'node:url';
import { writePng } from './crp-tex.mjs';

const here = path.dirname(fileURLToPath(import.meta.url));
const flag = (n) => { const i = process.argv.indexOf(n); return i > 0 ? process.argv[i + 1] : undefined; };
const TAG = flag('--tag'); if (!TAG) throw new Error('--tag required (e.g. --tag .h1)');
const LAYOUT = flag('--layout') || 'ah';
const SLOTS = (flag('--slots') || 'Extra1,Extra2,Extra3,Extra4').split(',').filter(Boolean);
const perSlot = Object.fromEntries((flag('--slot-layout') || '').split(',').filter(Boolean).map(s => s.split('=')));
const tex = path.join(here, '..', 'textures'), src = path.join(tex, 'current'), out = path.join(tex, 'v3');
const ALL = ['Cliff', 'Dirt', 'Grass', 'Extra1', 'Extra2', 'Extra3', 'Extra4'];
for (const l of [LAYOUT, ...Object.values(perSlot)]) if (l !== 'ah' && l !== 'bh') throw new Error(`unknown layout ${l}`);

// minimal PNG reader: 8-bit RGB/RGBA, non-interlaced (same as make-v3.mjs)
function readPng(file) {
  const b = fs.readFileSync(file); let p = 8, w, h, ct; const idat = [];
  while (p < b.length) {
    const len = b.readUInt32BE(p), type = b.toString('latin1', p + 4, p + 8), d = b.subarray(p + 8, p + 8 + len);
    if (type === 'IHDR') { w = d.readUInt32BE(0); h = d.readUInt32BE(4); ct = d[9]; if (d[8] !== 8 || d[12]) throw new Error(`${file}: unsupported PNG`); }
    else if (type === 'IDAT') idat.push(d);
    p += 12 + len;
  }
  const ch = ct === 6 ? 4 : ct === 2 ? 3 : 0; if (!ch) throw new Error(`${file}: colour type ${ct}`);
  const raw = zlib.inflateSync(Buffer.concat(idat)), stride = w * ch, px = Buffer.alloc(stride * h), rgba = Buffer.alloc(w * h * 4);
  for (let y = 0; y < h; y++) {
    const f = raw[y * (stride + 1)], line = raw.subarray(y * (stride + 1) + 1, (y + 1) * (stride + 1)), o = y * stride;
    for (let x = 0; x < stride; x++) {
      const a = x >= ch ? px[o + x - ch] : 0, up = y ? px[o - stride + x] : 0, c = x >= ch && y ? px[o - stride + x - ch] : 0;
      let v = line[x];
      if (f === 1) v += a; else if (f === 2) v += up; else if (f === 3) v += (a + up) >> 1;
      else if (f === 4) { const pp = a + up - c, pa = Math.abs(pp - a), pb = Math.abs(pp - up), pc = Math.abs(pp - c); v += pa <= pb && pa <= pc ? a : pb <= pc ? up : c; }
      px[o + x] = v & 255;
    }
  }
  for (let i = 0; i < w * h; i++) { for (let k = 0; k < 3; k++) rgba[i * 4 + k] = px[i * ch + k]; rgba[i * 4 + 3] = ch === 4 ? px[i * ch + 3] : 255; }
  return { w, h, rgba, ch };
}

// separable box blur, radius r, wrap-around
function boxBlur(a, w, h, r) {
  const t = new Float32Array(w * h), o = new Float32Array(w * h), n = 2 * r + 1;
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) { let s = 0; for (let k = -r; k <= r; k++) s += a[y * w + ((x + k) % w + w) % w]; t[y * w + x] = s / n; }
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) { let s = 0; for (let k = -r; k <= r; k++) s += t[(((y + k) % h + h) % h) * w + x]; o[y * w + x] = s / n; }
  return o;
}

function heightMap(diffuse, w, h) {
  const lum = new Float32Array(w * h);
  for (let i = 0; i < w * h; i++) lum[i] = 0.299 * diffuse[i * 4] + 0.587 * diffuse[i * 4 + 1] + 0.114 * diffuse[i * 4 + 2];
  const b = boxBlur(lum, w, h, 2), s = Float32Array.from(b).sort();
  const lo = s[Math.floor(0.01 * (s.length - 1))], hi = s[Math.floor(0.99 * (s.length - 1))], span = Math.max(hi - lo, 1e-6);
  const out = new Uint8Array(w * h);
  for (let i = 0; i < w * h; i++) out[i] = Math.max(0, Math.min(255, Math.round((b[i] - lo) / span * 255)));
  return out;
}

fs.mkdirSync(out, { recursive: true });
const rows = [];
for (const slot of ALL) {
  fs.copyFileSync(path.join(out, `${slot}_BaseColor.s66.png`), path.join(out, `${slot}_BaseColor${TAG}.png`));
  const dst = path.join(out, `${slot}_Normal${TAG}.png`);
  if (!SLOTS.includes(slot)) { fs.copyFileSync(path.join(src, `${slot}_Normal.png`), dst); rows.push({ slot, layout: 'copy' }); continue; }
  const layout = perSlot[slot] || LAYOUT;
  const n = readPng(path.join(src, `${slot}_Normal.png`)), d = readPng(path.join(out, `${slot}_BaseColor.s66.png`));
  if (n.w !== d.w || n.h !== d.h) throw new Error(`${slot}: normal ${n.w}x${n.h} vs diffuse ${d.w}x${d.h}`);
  const hm0 = heightMap(d.rgba, d.w, d.h);
  // --height-range lo,hi remaps the 0..255 height into [lo,hi] (e.g. 128,255 to test whether a channel is a blend height)
  const hr = (process.argv[process.argv.indexOf('--height-range') + 1] || '').split(',').map(Number); const [hlo, hhi] = hr.length === 2 && process.argv.includes('--height-range') ? hr : [0, 255];
  const hm = hm0.map(v => Math.round(hlo + v / 255 * (hhi - hlo)));
  for (let i = 0; i < n.w * n.h; i++) {
    if (layout === 'ah') { n.rgba[i * 4 + 2] = 255; n.rgba[i * 4 + 3] = hm[i]; }
    else { n.rgba[i * 4 + 2] = hm[i]; n.rgba[i * 4 + 3] = 128; }
  }
  writePng(dst, n, 4);
  rows.push({ slot, layout });
}
// verify from disk
for (const r of rows) {
  const v = readPng(path.join(out, `${r.slot}_Normal${TAG}.png`)); let sb = 0, sa = 0, mn = 255, mx = 0; const hc = r.layout === 'bh' ? 2 : 3;
  for (let i = 0; i < v.w * v.h; i++) { sb += v.rgba[i * 4 + 2]; sa += v.rgba[i * 4 + 3]; const hv = v.rgba[i * 4 + hc]; if (hv < mn) mn = hv; if (hv > mx) mx = hv; }
  Object.assign(r, { w: v.w, ch: v.ch, meanB: (sb / (v.w * v.h)).toFixed(1), meanA: (sa / (v.w * v.h)).toFixed(1), h: r.layout === 'copy' ? '-' : `${mn}/${mx}` });
}
const n14 = fs.readdirSync(out).filter(f => f.endsWith(`${TAG}.png`)).length;
console.log(`${n14} files tagged ${TAG} | ` + rows.map(r => `${r.slot} ${r.layout} w${r.w} ch${r.ch} B${r.meanB} A${r.meanA} h${r.h}`).join(' | '));
