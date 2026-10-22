// crop-down.mjs <in> <crop> <factor> <out> : crop <crop> px off each side, then box-downsample by <factor> (RGB out)
import { readPng, writePng } from './up4k-png.mjs';
const [inp, cS, fS, out] = process.argv.slice(2), c = +cS, f = +fS, s = readPng(inp);
const W = (s.w - 2 * c) / f, H = (s.h - 2 * c) / f; if (W % 1 || H % 1) throw new Error('size not divisible');
const rgba = Buffer.alloc(W * H * 4), n = f * f;
for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) for (let k = 0; k < 3; k++) {
  let a = 0; for (let j = 0; j < f; j++) { const r = (c + y * f + j) * s.w + c + x * f; for (let i = 0; i < f; i++) a += s.rgba[(r + i) * 4 + k]; }
  rgba[(y * W + x) * 4 + k] = Math.round(a / n); }
writePng(out, { w: W, h: H, rgba }, 3); console.log(`${inp} ${s.w}x${s.h} -> ${out} ${W}x${H}`);
