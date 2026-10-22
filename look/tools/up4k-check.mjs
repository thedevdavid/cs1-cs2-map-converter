// up4k-check.mjs <slot>... : seam metrics for up4k/<slot> + lanczos/<slot>, and compare strips (src 2x nearest | lanczos | esrgan)
import fs from 'node:fs';
import { readPng, writePng } from './up4k-png.mjs';
const D = 'look/textures', R = 300, X0 = 874, Y0 = 874; // source region (300x300 at centre)
function seam({ w, h, rgba, ch }) {
  const px = (x, y, k) => rgba[(y * w + x) * 4 + k], d = (ax, ay, bx, by) => { let s = 0; for (let k = 0; k < 3; k++) s += Math.abs(px(ax, ay, k) - px(bx, by, k)); return s / 3; };
  let lr = 0, tb = 0, ic = 0, ir = 0;
  for (let i = 0; i < h; i++) lr += d(0, i, w - 1, i); for (let i = 0; i < w; i++) tb += d(i, 0, i, h - 1);
  for (let y = 0; y < h; y += 4) for (let x = 1; x < w - 1; x++) ic += d(x, y, x + 1, y);
  for (let y = 1; y < h - 1; y++) for (let x = 0; x < w; x += 4) ir += d(x, y, x, y + 1);
  return `${w}x${h} ch=${ch} LR=${(lr / h).toFixed(2)} col=${(ic / ((w - 2) * h / 4)).toFixed(2)} TB=${(tb / w).toFixed(2)} row=${(ir / ((h - 2) * w / 4)).toFixed(2)}`;
}
for (const slot of process.argv.slice(2)) {
  const f = `${slot}_BaseColor.png`, src = readPng(`${D}/current/${f}`), panels = [[src, X0, Y0, 2]];
  for (const dir of ['up4k/lanczos', 'up4k']) if (fs.existsSync(`${D}/${dir}/${f}`)) { const im = readPng(`${D}/${dir}/${f}`); console.log(`${slot} ${dir}: ${seam(im)}`); panels.push([im, 2 * X0, 2 * Y0, 1]); }
  const S = 2 * R, W = S * panels.length, rgba = Buffer.alloc(W * S * 4);
  panels.forEach(([im, x0, y0, z], p) => { for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) im.rgba.copy(rgba, (y * W + p * S + x) * 4, ((y0 + (y / z | 0)) * im.w + x0 + (x / z | 0)) * 4, ((y0 + (y / z | 0)) * im.w + x0 + (x / z | 0)) * 4 + 4); });
  writePng(`${D}/up4k/compare/${slot}.png`, { w: W, h: S, rgba }, 3);
}
