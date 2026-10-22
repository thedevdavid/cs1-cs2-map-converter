// Extra1 (Dry) and Extra2 (Lush) paint brushes graded from the base Grass diffuse so they read clearly against it
// (the CS1 picks were two more grass textures within a few RGB points of the base grass: painting showed nothing).
// Usage: node look/tools/make-extras.mjs --from .s66 --tag .e1   (copies every other file of the --from set under --tag)
import fs from 'node:fs'; import path from 'node:path'; import { fileURLToPath } from 'node:url';
import { readPng, writePng } from './up4k-png.mjs';
const here = path.dirname(fileURLToPath(import.meta.url)), dir = path.join(here, '..', 'textures', 'v3');
const flag = (n, d) => { const i = process.argv.indexOf(n); return i > 0 ? process.argv[i + 1] : d; };
const FROM = flag('--from', '.s66'), TAG = flag('--tag', '.e1');
// per-channel gain on luminance-preserving detail: out = lum * gain + (px - lum) * detail
// --cs1: keep the CS1 textures; Extra1 unchanged, Extra2 (Springwood grass, within a few RGB points of the base grass) only nudged darker/greener
const CS1 = process.argv.includes('--cs1');
const grades = CS1 ? { Extra2: { src: 'Extra2', gain: [0.80, 1.08, 0.78], detail: 1.0, note: 'CS1 Springwood grass, mild lush shift' } } : {
  Extra1: { gain: [1.80, 1.55, 0.95], detail: 0.9, note: 'dry straw grass' },
  Extra2: { gain: [0.72, 1.30, 0.42], detail: 1.0, note: 'lush dark grass' },
};
const grass = readPng(path.join(dir, `Grass_BaseColor${FROM}.png`));
for (const [slot, s] of Object.entries(grades)) {
  const g = s.src ? readPng(path.join(dir, `${s.src}_BaseColor${FROM}.png`)) : grass;
  const o = { w: g.w, h: g.h, rgba: Buffer.from(g.rgba) };
  for (let i = 0; i < g.w * g.h; i++) {
    const r = g.rgba[i * 4], gg = g.rgba[i * 4 + 1], b = g.rgba[i * 4 + 2], l = 0.299 * r + 0.587 * gg + 0.114 * b;
    const px = [r, gg, b];
    for (let k = 0; k < 3; k++) o.rgba[i * 4 + k] = Math.max(0, Math.min(255, Math.round(l * s.gain[k] + (px[k] - l) * s.detail)));
    // alpha (smoothness) stays the grass value
  }
  writePng(path.join(dir, `${slot}_BaseColor${TAG}.png`), o, 4);
  fs.copyFileSync(path.join(dir, `${s.src || 'Grass'}_Normal${FROM}.png`), path.join(dir, `${slot}_Normal${TAG}.png`));
  let sr = 0, sg = 0, sb = 0, n = 0; for (let i = 0; i < g.w * g.h; i += 97) { sr += o.rgba[i * 4]; sg += o.rgba[i * 4 + 1]; sb += o.rgba[i * 4 + 2]; n++; }
  console.log(`${slot} ${s.note}: mean ${(sr / n).toFixed(0)},${(sg / n).toFixed(0)},${(sb / n).toFixed(0)} (grass 59,69,37)`);
}
for (const slot of ['Cliff', 'Dirt', 'Grass', 'Extra3', 'Extra4', ...(CS1 ? ['Extra1'] : [])]) for (const k of ['BaseColor', 'Normal'])
  fs.copyFileSync(path.join(dir, `${slot}_${k}${FROM}.png`), path.join(dir, `${slot}_${k}${TAG}.png`));
console.log(`${fs.readdirSync(dir).filter(f => f.endsWith(`${TAG}.png`)).length} files tagged ${TAG}`);
