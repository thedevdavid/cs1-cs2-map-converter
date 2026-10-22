// wrap-pad.mjs <in> <pad> <out> : pad a seamless tile on every side with wrap-around pixels (RGB out)
import { readPng, writePng } from './up4k-png.mjs';
const [inp, padS, out] = process.argv.slice(2), pad = +padS, s = readPng(inp);
const W = s.w + 2 * pad, H = s.h + 2 * pad, rgba = Buffer.alloc(W * H * 4);
for (let y = 0; y < H; y++) { const sy = ((y - pad) % s.h + s.h) % s.h;
  for (let x = 0; x < W; x++) { const sx = ((x - pad) % s.w + s.w) % s.w; s.rgba.copy(rgba, (y * W + x) * 4, (sy * s.w + sx) * 4, (sy * s.w + sx) * 4 + 4); } }
writePng(out, { w: W, h: H, rgba }, 3); console.log(`${inp} ${s.w}x${s.h} -> ${out} ${W}x${H}`);
