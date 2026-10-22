// make-up4k-set.mjs [--tag .u1] [--w 0.6] [--base .e2]
// Cliff/Dirt/Extra3 diffuse: ESRGAN colour-matched (per-channel mean/std) to Lanczos, blended w*esrgan + (1-w)*lanczos,
// alpha = base diffuse alpha upscaled 2x bilinear (wrap). Other set files copied from the base tag. Verifies + writes compare strips.
import fs from 'node:fs';
import { readPng, writePng } from './up4k-png.mjs';
const arg = (k, d) => { const i = process.argv.indexOf(k); return i > 0 ? process.argv[i + 1] : d; };
const TAG = arg('--tag', '.u1'), W = +arg('--w', 0.6), BASE = arg('--base', '.e2');
const D = 'look/textures', V3 = `${D}/v3`, R = 300, X0 = 874, Y0 = 874; // strip region as up4k-check.mjs
const UP = ['Cliff', 'Dirt', 'Extra3'], ALL = ['Grass', 'Dirt', 'Cliff', 'Extra1', 'Extra2', 'Extra3', 'Extra4'];

function stats(im) {
  const n = im.w * im.h, m = [0, 0, 0], s = [0, 0, 0];
  for (let i = 0; i < n; i++) for (let k = 0; k < 3; k++) { const v = im.rgba[i * 4 + k]; m[k] += v; s[k] += v * v; }
  return m.map((v, k) => { const mu = v / n; return [mu, Math.sqrt(Math.max(0, s[k] / n - mu * mu))]; });
}
const meanA = im => { let a = 0; for (let i = 3; i < im.rgba.length; i += 4) a += im.rgba[i]; return a / (im.w * im.h); };
function up2Alpha(im) { // bilinear 2x, wrap-around, pixel-centre aligned
  const w = im.w * 2, h = im.h * 2, out = new Float32Array(w * h), A = (x, y) => im.rgba[((((y % im.h) + im.h) % im.h) * im.w + (((x % im.w) + im.w) % im.w)) * 4 + 3];
  for (let y = 0; y < h; y++) { const sy = (y + 0.5) / 2 - 0.5, y0 = Math.floor(sy), fy = sy - y0;
    for (let x = 0; x < w; x++) { const sx = (x + 0.5) / 2 - 0.5, x0 = Math.floor(sx), fx = sx - x0;
      out[y * w + x] = (A(x0, y0) * (1 - fx) + A(x0 + 1, y0) * fx) * (1 - fy) + (A(x0, y0 + 1) * (1 - fx) + A(x0 + 1, y0 + 1) * fx) * fy; } }
  return { w, h, a: out };
}
function seam({ w, h, rgba }) { // identical to up4k-check.mjs
  const px = (x, y, k) => rgba[(y * w + x) * 4 + k], d = (ax, ay, bx, by) => { let s = 0; for (let k = 0; k < 3; k++) s += Math.abs(px(ax, ay, k) - px(bx, by, k)); return s / 3; };
  let lr = 0, tb = 0, ic = 0, ir = 0;
  for (let i = 0; i < h; i++) lr += d(0, i, w - 1, i); for (let i = 0; i < w; i++) tb += d(i, 0, i, h - 1);
  for (let y = 0; y < h; y += 4) for (let x = 1; x < w - 1; x++) ic += d(x, y, x + 1, y);
  for (let y = 1; y < h - 1; y++) for (let x = 0; x < w; x += 4) ir += d(x, y, x, y + 1);
  return `LR=${(lr / h).toFixed(2)} col=${(ic / ((w - 2) * h / 4)).toFixed(2)} TB=${(tb / w).toFixed(2)} row=${(ir / ((h - 2) * w / 4)).toFixed(2)}`;
}

fs.mkdirSync(`${D}/up4k/compare`, { recursive: true });
for (const slot of UP) {
  const f = `${slot}_BaseColor.png`, es = readPng(`${D}/up4k/${f}`), lz = readPng(`${D}/up4k/lanczos/${f}`), base = readPng(`${V3}/${slot}_BaseColor${BASE}.png`);
  if (es.w !== lz.w || es.h !== lz.h || base.w * 2 !== lz.w) throw new Error(`${slot}: size mismatch ${es.w} ${lz.w} ${base.w}`);
  const se = stats(es), sl = stats(lz), al = up2Alpha(base), n = es.w * es.h, rgba = Buffer.alloc(n * 4);
  const g = [0, 1, 2].map(k => se[k][1] > 1e-6 ? sl[k][1] / se[k][1] : 1);
  for (let i = 0; i < n; i++) {
    for (let k = 0; k < 3; k++) { const m = (es.rgba[i * 4 + k] - se[k][0]) * g[k] + sl[k][0]; rgba[i * 4 + k] = Math.max(0, Math.min(255, Math.round(W * m + (1 - W) * lz.rgba[i * 4 + k]))); }
    rgba[i * 4 + 3] = Math.max(0, Math.min(255, Math.round(al.a[i])));
  }
  const out = { w: es.w, h: es.h, rgba };
  writePng(`${V3}/${slot}_BaseColor${TAG}.png`, out, 4);
  // verify from disk
  const chk = readPng(`${V3}/${slot}_BaseColor${TAG}.png`), so = stats(chk);
  console.log(`${slot}: ${chk.w}x${chk.h}x${chk.ch ?? '?'} meanRGB lanczos=${sl.map(v => v[0].toFixed(1)).join('/')} out=${so.map(v => v[0].toFixed(1)).join('/')} | alpha ${BASE}=${meanA(base).toFixed(2)} ${TAG}=${meanA(chk).toFixed(2)} | seam ${seam(chk)}`);
  // compare strip: src (current, 2x nearest) | lanczos | out
  const src = readPng(`${D}/current/${f}`), panels = [[src, X0, Y0, 2], [lz, 2 * X0, 2 * Y0, 1], [chk, 2 * X0, 2 * Y0, 1]];
  const S = 2 * R, SW = S * panels.length, strip = Buffer.alloc(SW * S * 4);
  panels.forEach(([im, x0, y0, z], p) => { for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) { const o = ((y0 + (y / z | 0)) * im.w + x0 + (x / z | 0)) * 4; im.rgba.copy(strip, (y * SW + p * S + x) * 4, o, o + 4); } });
  writePng(`${D}/up4k/compare/${slot}-blend.png`, { w: SW, h: S, rgba: strip }, 3);
}
for (const slot of ALL) for (const kind of ['BaseColor', 'Normal']) {
  if (kind === 'BaseColor' && UP.includes(slot)) continue;
  fs.copyFileSync(`${V3}/${slot}_${kind}${BASE}.png`, `${V3}/${slot}_${kind}${TAG}.png`);
}
const tagged = fs.readdirSync(V3).filter(n => n.endsWith(`${TAG}.png`));
console.log(`${TAG} files: ${tagged.length}${tagged.length === 14 ? ' OK' : ' MISMATCH'}`);
