// lanczos2x.mjs <in> <out> : wrap-aware separable Lanczos3 2x upscale of a seamless tile (RGB out)
import { readPng, writePng } from './up4k-png.mjs';
const [inp, out] = process.argv.slice(2), s = readPng(inp);
const L = x => x === 0 ? 1 : Math.abs(x) >= 3 ? 0 : 3 * Math.sin(Math.PI * x) * Math.sin(Math.PI * x / 3) / (Math.PI * Math.PI * x * x);
// output pixel o maps to source u = (o+0.5)/2-0.5; phases: even o -> base floor(u), frac .75; odd -> frac .25
const taps = [0.75, 0.25].map(fr => { const w = []; for (let t = -2; t <= 3; t++) w.push(L(t - fr)); const sum = w.reduce((a, b) => a + b); return w.map(v => v / sum); });
const base = o => Math.floor((o + 0.5) / 2 - 0.5);
function pass(src, w, h, horiz) { // upsample along one axis, Float32 RGB planes
  const W = horiz ? 2 * w : w, H = horiz ? h : 2 * h, dst = new Float32Array(W * H * 3), n = horiz ? w : h;
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) { const o = horiz ? x : y, b = base(o), tw = taps[o & 1];
    for (let k = 0; k < 3; k++) { let a = 0;
      for (let t = 0; t < 6; t++) { const q = ((b - 2 + t) % n + n) % n; a += tw[t] * src[((horiz ? y * w + q : q * w + x)) * 3 + k]; }
      dst[(y * W + x) * 3 + k] = a; } }
  return dst; }
const f0 = new Float32Array(s.w * s.h * 3); for (let i = 0; i < s.w * s.h; i++) for (let k = 0; k < 3; k++) f0[i * 3 + k] = s.rgba[i * 4 + k];
const f2 = pass(pass(f0, s.w, s.h, true), 2 * s.w, s.h, false), W = 2 * s.w, H = 2 * s.h, rgba = Buffer.alloc(W * H * 4);
for (let i = 0; i < W * H; i++) for (let k = 0; k < 3; k++) rgba[i * 4 + k] = Math.max(0, Math.min(255, Math.round(f2[i * 3 + k])));
writePng(out, { w: W, h: H, rgba }, 3); console.log(`${inp} -> ${out} ${W}x${H}`);
