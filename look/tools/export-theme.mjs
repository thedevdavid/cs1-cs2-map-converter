// Export the CS1 "Dakota Falls" theme-mix terrain textures as PNG sets for CS2's terrain render settings.
// Writes look/textures/<variant>/<Slot>_BaseColor.png and <Slot>_Normal.png (Map Texture Replacer pack names),
// variant "raw" (as stored in CS1) and "gain16" (RGB x1.6; CS1's terrain shader brightens its textures).
// Usage: node look/tools/export-theme.mjs [workshopDir]
import fs from 'node:fs';
import path from 'node:path';
import { readCrpTexture, decode, writePng, stats } from './crp-tex.mjs';

const W = process.argv[2] || 'D:/SteamLibrary/steamapps/workshop/content/255710';
const crp = {
  seychelles: `${W}/1230768185/Seychelles.crp`,
  springwood: `${W}/921636136/Springwood.crp`,
  owl: `${W}/1367973971/Owl's Albedo.crp`,
  coastal: `${W}/1594406264/r69-coastal.crp`,
};
// slot -> CS1 source. normal: 'rg' / 'ba' = channels of Owl's CliffSandNormal (cliff in RG, sand in BA); 'gen' = from luminance.
const slots = [
  { slot: 'Grass', src: [crp.seychelles, 'GrassDiffuseTexture'], normal: 'gen', note: 'mix grass (Seychelles)' },
  { slot: 'Cliff', src: [crp.springwood, 'CliffDiffuseTexture'], normal: 'rg', note: 'mix cliff (Springwood) + Owl\'s cliff normal' },
  { slot: 'Dirt', src: [crp.springwood, 'RuinedDiffuseTexture'], normal: 'gen', note: 'mix ruined (Springwood)' },
  { slot: 'Extra1', src: [crp.coastal, 'GrassDiffuseTexture'], normal: 'gen', note: 'brush "Dry": r69 Coastal grass (olive-brown, the driest grass among the mix themes)' },
  { slot: 'Extra2', src: [crp.springwood, 'GrassDiffuseTexture'], normal: 'gen', note: 'brush "Lush": Springwood grass' },
  { slot: 'Extra3', src: [crp.seychelles, 'GravelDiffuseTexture'], normal: 'gen', note: 'brush "Dirt": mix gravel (Seychelles)' },
  { slot: 'Extra4', src: [crp.owl, 'SandDiffuseTexture'], normal: 'ba', note: 'brush "Sand": mix sand (Owl\'s Albedo) + Owl\'s sand normal' },
];

const out = 'look/textures';
let cliffSand = null;
const getCliffSand = () => cliffSand ??= decode(readCrpTexture(crp.owl, 'CliffSandNormalTexture'));

function normalFromPacked(src, which, w, h) {
  const n = { w, h, rgba: Buffer.alloc(w * h * 4) };
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const sx = Math.floor(x * src.w / w), sy = Math.floor(y * src.h / h), q = (sy * src.w + sx) * 4;
    const nx = src.rgba[q + (which === 'rg' ? 0 : 2)] / 127.5 - 1, ny = src.rgba[q + (which === 'rg' ? 1 : 3)] / 127.5 - 1;
    const nz = Math.sqrt(Math.max(0, 1 - nx * nx - ny * ny)), o = (y * w + x) * 4;
    n.rgba[o] = Math.round((nx + 1) * 127.5); n.rgba[o + 1] = Math.round((ny + 1) * 127.5); n.rgba[o + 2] = Math.round((nz + 1) * 127.5); n.rgba[o + 3] = 255;
  }
  return n;
}

function normalFromLuma(img, strength = 2.0) {
  const { w, h, rgba } = img, L = new Float32Array(w * h);
  for (let i = 0; i < w * h; i++) L[i] = (0.299 * rgba[i * 4] + 0.587 * rgba[i * 4 + 1] + 0.114 * rgba[i * 4 + 2]) / 255;
  const at = (x, y) => L[((y + h) % h) * w + ((x + w) % w)];
  const n = { w, h, rgba: Buffer.alloc(w * h * 4) };
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const dx = (at(x + 1, y - 1) + 2 * at(x + 1, y) + at(x + 1, y + 1)) - (at(x - 1, y - 1) + 2 * at(x - 1, y) + at(x - 1, y + 1));
    const dy = (at(x - 1, y + 1) + 2 * at(x, y + 1) + at(x + 1, y + 1)) - (at(x - 1, y - 1) + 2 * at(x, y - 1) + at(x + 1, y - 1));
    let nx = -dx * strength, ny = -dy * strength, nz = 1; const len = Math.hypot(nx, ny, nz); nx /= len; ny /= len; nz /= len;
    const o = (y * w + x) * 4;
    n.rgba[o] = Math.round((nx + 1) * 127.5); n.rgba[o + 1] = Math.round((ny + 1) * 127.5); n.rgba[o + 2] = Math.round((nz + 1) * 127.5); n.rgba[o + 3] = 255;
  }
  return n;
}

const gain = (img, g) => { const r = Buffer.from(img.rgba); for (let i = 0; i < r.length; i += 4) for (let c = 0; c < 3; c++) r[i + c] = Math.min(255, Math.round(r[i + c] * g)); return { ...img, rgba: r }; };

const manifest = [];
for (const v of ['raw', 'gain16']) fs.mkdirSync(path.join(out, v), { recursive: true });
for (const s of slots) {
  const img = decode(readCrpTexture(...s.src));
  const normal = s.normal === 'gen' ? normalFromLuma(img) : normalFromPacked(getCliffSand(), s.normal, img.w, img.h);
  writePng(path.join(out, 'raw', `${s.slot}_BaseColor.png`), img, 3);
  writePng(path.join(out, 'gain16', `${s.slot}_BaseColor.png`), gain(img, 1.6), 3);
  for (const v of ['raw', 'gain16']) writePng(path.join(out, v, `${s.slot}_Normal.png`), normal, 3);
  const st = stats(img);
  manifest.push({ slot: s.slot, source: `${path.basename(s.src[0])}::${s.src[1]}`, size: `${img.w}x${img.h}`, meanRGBA: st.mean, normal: s.normal, note: s.note });
  console.log(s.slot.padEnd(7), `${img.w}x${img.h}`, JSON.stringify(st.mean), s.note);
}
fs.writeFileSync(path.join(out, 'manifest.json'), JSON.stringify(manifest, null, 2) + '\n');
