// Contact sheet of CS1 theme textures (box-downsampled to 256 px tiles, labelled by index in the console).
// Usage: node look/tools/sheet.mjs <out.png> <crp>::<asset> ...
import { readCrpTexture, decode, writePng } from './crp-tex.mjs';

const [out, ...specs] = process.argv.slice(2);
const T = 256, cols = Math.min(5, specs.length), rows = Math.ceil(specs.length / cols);
const sheet = { w: cols * T, h: rows * T, rgba: Buffer.alloc(cols * T * rows * T * 4) };
specs.forEach((spec, i) => {
  const [crp, name] = spec.split('::');
  const img = decode(readCrpTexture(crp, name)), f = img.w / T;
  for (let y = 0; y < T; y++) for (let x = 0; x < T; x++) {
    const s = [0, 0, 0];
    for (let dy = 0; dy < f; dy++) for (let dx = 0; dx < f; dx++) { const q = ((y * f + dy) * img.w + x * f + dx) * 4; s[0] += img.rgba[q]; s[1] += img.rgba[q + 1]; s[2] += img.rgba[q + 2]; }
    const o = (((Math.floor(i / cols) * T + y) * cols * T) + (i % cols) * T + x) * 4;
    sheet.rgba[o] = s[0] / f / f; sheet.rgba[o + 1] = s[1] / f / f; sheet.rgba[o + 2] = s[2] / f / f; sheet.rgba[o + 3] = 255;
  }
  console.log(i, spec.split('/').pop());
});
writePng(out, sheet, 3);
