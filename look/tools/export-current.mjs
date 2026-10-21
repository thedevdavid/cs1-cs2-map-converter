// The "current" terrain texture set the runner imports: gain16 (x1.6) for every slot, except Dirt and Extra3,
// which are one step lighter (owner, A-v8: "the dirt texture is very dark. maybe try to lighten it a little bit").
// Needs look/textures/gain16 (node look/tools/export-theme.mjs). Writes look/textures/current/ (14 PNGs + manifest).
// Usage: node look/tools/export-current.mjs [workshopDir]
import fs from 'node:fs';
import path from 'node:path';
import { readCrpTexture, decode, writePng, stats } from './crp-tex.mjs';

const W = process.argv[2] || 'D:/SteamLibrary/steamapps/workshop/content/255710';
const src = 'look/textures/gain16', out = 'look/textures/current';
if (!fs.existsSync(src)) throw new Error(`${src} missing: run node look/tools/export-theme.mjs first`);
fs.mkdirSync(out, { recursive: true });
for (const f of fs.readdirSync(src).filter(f => f.endsWith('.png'))) fs.copyFileSync(path.join(src, f), path.join(out, f));

// one step lighter: x1.9 instead of x1.6, 10 % toward luma (a little less red); same texture, same normal
const STEP = { gain: 1.9, desat: 0.1 };
const lighter = (img) => {
  const r = Buffer.alloc(img.rgba.length);
  for (let i = 0; i < r.length; i += 4) {
    let c = [0, 1, 2].map(k => img.rgba[i + k] * STEP.gain);
    const l = 0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2];
    c = c.map(v => v + (l - v) * STEP.desat);
    for (let k = 0; k < 3; k++) r[i + k] = Math.max(0, Math.min(255, Math.round(c[k])));
    r[i + 3] = 255;
  }
  return { w: img.w, h: img.h, rgba: r };
};
const swaps = [
  { slot: 'Dirt', crp: `${W}/921636136/Springwood.crp`, asset: 'RuinedDiffuseTexture' },
  { slot: 'Extra3', crp: `${W}/1230768185/Seychelles.crp`, asset: 'GravelDiffuseTexture' },
];
const manifest = { base: 'gain16 (x1.6)', step: STEP, slots: {} };
for (const s of swaps) {
  const img = lighter(decode(readCrpTexture(s.crp, s.asset)));
  writePng(path.join(out, `${s.slot}_BaseColor.png`), img, 3);
  manifest.slots[s.slot] = { source: `${path.basename(s.crp)}::${s.asset}`, meanRGB: stats(img).mean.slice(0, 3) };
  console.log(s.slot, JSON.stringify(manifest.slots[s.slot].meanRGB));
}
fs.writeFileSync(path.join(out, 'manifest-current.json'), JSON.stringify(manifest, null, 2) + '\n');
console.log(fs.readdirSync(out).filter(f => f.endsWith('.png')).length, 'PNGs in', out);
