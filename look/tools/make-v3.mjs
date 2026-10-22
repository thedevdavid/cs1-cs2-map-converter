// v3 terrain textures: look/textures/current with an alpha channel on the 7 BaseColor PNGs (32-bit RGBA, RGB unchanged).
// Why: the CS2 importer keeps 3-channel PNGs uncompressed and the GPU samples alpha = 1.0, which the terrain shader
// (legacy mode, CS1 convention) reads as smoothness -> mirror-like terrain. CS1's theme textures carry that alpha.
// Alpha = the CS1 source alpha per pixel (re-read from the theme .crp, same sources as export-theme.mjs) when the source
// has real alpha content and the same size; a flat-255 source (no alpha content) or a missing/mismatched source gets a
// uniform value. Normal PNGs are copied unchanged (24-bit tangent normals). current/ is never written.
// Usage: node look/textures/make-v3.mjs [workshopDir]
import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import { fileURLToPath } from 'node:url';
import { readCrpTexture, decode, writePng } from './crp-tex.mjs';

const here = path.dirname(fileURLToPath(import.meta.url));
const W = process.argv[2] || 'D:/SteamLibrary/steamapps/workshop/content/255710';
const tex = path.join(here, '..', 'textures');
const src = path.join(tex, 'current'), out = path.join(tex, 'v3');
const crp = {
  seychelles: `${W}/1230768185/Seychelles.crp`, springwood: `${W}/921636136/Springwood.crp`,
  owl: `${W}/1367973971/Owl's Albedo.crp`, coastal: `${W}/1594406264/r69-coastal.crp`,
};
// uniform fallback per slot (Grass 60, Dirt 50, Cliff 90, Extra* = manifest CS1 mean alpha clamped 40..110; Extra2 is a
// grass slot whose source alpha is flat 255, so it takes the Grass value)
const slots = {
  Grass: { src: [crp.seychelles, 'GrassDiffuseTexture'], uniform: 60, cap: 60 },
  Cliff: { src: [crp.springwood, 'CliffDiffuseTexture'], uniform: 60, cap: 60 },
  Dirt: { src: [crp.springwood, 'RuinedDiffuseTexture'], uniform: 50, cap: 60 },
  Extra1: { src: [crp.coastal, 'GrassDiffuseTexture'], uniform: 98, cap: 60 },
  Extra2: { src: [crp.springwood, 'GrassDiffuseTexture'], uniform: 60, cap: 60 },
  Extra3: { src: [crp.seychelles, 'GravelDiffuseTexture'], uniform: 110, cap: 60 },
  Extra4: { src: [crp.owl, 'SandDiffuseTexture'], uniform: 110, cap: 60 },
};

// minimal PNG reader: 8-bit RGB/RGBA, non-interlaced (what crp-tex writePng produces)
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

fs.mkdirSync(out, { recursive: true });
const rows = [];
for (const [slot, s] of Object.entries(slots)) {
  const img = readPng(path.join(src, `${slot}_BaseColor.png`));
  let mode = `uniform ${s.uniform}`, alpha = null;
  try {
    const cs1 = decode(readCrpTexture(...s.src));
    let mn = 255; for (let i = 3; i < cs1.rgba.length; i += 4) if (cs1.rgba[i] < mn) mn = cs1.rgba[i];
    if (cs1.w !== img.w || cs1.h !== img.h) mode += ` (CS1 size ${cs1.w}x${cs1.h} differs)`;
    else if (mn === 255) mode += ' (CS1 alpha flat 255)';
    else { alpha = cs1.rgba; mode = 'CS1 alpha per pixel'; }
  } catch (e) { mode += ` (CS1 source unreadable: ${e.message})`; }
  for (let i = 3; i < img.rgba.length; i += 4) { const a = alpha ? alpha[i] : s.uniform; img.rgba[i] = s.cap ? Math.min(a, s.cap) : a; }
  if (s.cap) mode += ` capped at ${s.cap}`;
  writePng(path.join(out, `${slot}_BaseColor.png`), img, 4);
  fs.copyFileSync(path.join(src, `${slot}_Normal.png`), path.join(out, `${slot}_Normal.png`)); // normals unchanged (24-bit)
  rows.push({ slot, mode });
}
// verify from disk: 4 channels, mean alpha
for (const r of rows) {
  const v = readPng(path.join(out, `${r.slot}_BaseColor.png`)); let sa = 0;
  for (let i = 3; i < v.rgba.length; i += 4) sa += v.rgba[i];
  r.size = `${v.w}x${v.h}`; r.channels = v.ch; r.meanA = +(sa / (v.w * v.h)).toFixed(1);
}
fs.writeFileSync(path.join(out, 'manifest-v3.json'), JSON.stringify({ base: 'look/textures/current', alpha: rows }, null, 2) + '\n');
console.log(rows.map(r => `${r.slot} ${r.size} ch${r.channels} meanA=${r.meanA} [${r.mode}]`).join(' | '));
