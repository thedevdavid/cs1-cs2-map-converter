// A-v8 follow-up: lighter / forest-floor variants of the two slots that read too dark, as v2 PNGs.
// Writes look/textures/v2/<name>_BaseColor.png (+ the matching _Normal.png copied from gain16).
// Usage: node look/tools/export-v2.mjs [workshopDir]
import fs from 'node:fs';
import path from 'node:path';
import { readCrpTexture, decode, writePng, stats } from './crp-tex.mjs';

const W = process.argv[2] || 'D:/SteamLibrary/steamapps/workshop/content/255710';
const sey = `${W}/1230768185/Seychelles.crp`, sw = `${W}/921636136/Springwood.crp`;
const out = 'look/textures/v2';
fs.mkdirSync(out, { recursive: true });

const load = (crp, name) => decode(readCrpTexture(crp, name));
// mix a and b (same size) with weight wa, multiply by gain, desaturate toward luma by d (0..1)
function grade(a, b, wa, gain, d) {
  const r = Buffer.alloc(a.rgba.length);
  for (let i = 0; i < r.length; i += 4) {
    let c = [0, 1, 2].map(k => (b ? a.rgba[i + k] * wa + b.rgba[i + k] * (1 - wa) : a.rgba[i + k]) * gain);
    const l = 0.299 * c[0] + 0.587 * c[1] + 0.114 * c[2];
    c = c.map(v => v + (l - v) * d);
    for (let k = 0; k < 3; k++) r[i + k] = Math.max(0, Math.min(255, Math.round(c[k])));
    r[i + 3] = 255;
  }
  return { w: a.w, h: a.h, rgba: r };
}

const grass = load(sey, 'GrassDiffuseTexture'), ruined = load(sw, 'RuinedDiffuseTexture'), gravel = load(sey, 'GravelDiffuseTexture');
const variants = [
  { name: 'Dirt-forestfloor', img: grade(grass, ruined, 0.6, 1.8, 0.1), normal: 'Dirt', why: 'CS2 shows the base Dirt layer under forest and scattered trees, where CS1 had grass with the forest tint: 60 % Seychelles grass + 40 % Springwood ruined, x1.8, slight desaturation (olive-brown forest floor)' },
  { name: 'Dirt-lighter', img: grade(ruined, null, 1, 2.2, 0.2), normal: 'Dirt', why: 'the same Springwood ruined, x2.2 and 20 % desaturated (lighter, less red soil)' },
  { name: 'Extra3-lighter', img: grade(gravel, null, 1, 2.4, 0.25), normal: 'Extra3', why: 'Dirt brush (Seychelles gravel) x2.4 and 25 % desaturated: grey-tan gravel/scree as on real talus and gravel-pit banks' },
];
const manifest = [];
for (const v of variants) {
  writePng(path.join(out, `${v.name}_BaseColor.png`), v.img, 3);
  fs.copyFileSync(`look/textures/gain16/${v.normal}_Normal.png`, path.join(out, `${v.name}_Normal.png`));
  const st = stats(v.img);
  manifest.push({ file: `${v.name}_BaseColor.png`, meanRGB: st.mean.slice(0, 3), why: v.why });
  console.log(v.name.padEnd(17), JSON.stringify(st.mean.slice(0, 3)));
}
fs.writeFileSync(path.join(out, 'manifest-v2.json'), JSON.stringify(manifest, null, 2) + '\n');
