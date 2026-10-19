#!/usr/bin/env node
// Convert a Cities: Skylines 1 colour-correction LUT (1024x32 PNG strip, or a .crp
// asset that embeds one) into a .cube file for the CS2 mod Lumina.
//
// Lumina only applies .cube LUTs in HDRP "External" tonemapping mode. There HDRP
// samples the LUT with LogC-encoded (Unity ARRI EI800) input and uses the result as
// *linear* output, replacing the tonemapper. A CS1 LUT is display-referred (sRGB in,
// sRGB out, applied after CS1's tonemapper), so by default each cube entry is:
//   LogC grid point -> linear -> ACES (Hill fit) -> sRGB -> CS1 LUT -> linear.
// --mode raw writes the CS1 LUT unchanged as a standard sRGB->sRGB .cube (for
// other tools; wrong for Lumina's External mode).
//
// Usage: node cs1lut2cube.mjs <in.png|in.crp> [out.cube] [--mode lumina|raw]
//        [--exposure EV] [--identity]
// No dependencies (Node >= 18).

import fs from "node:fs";
import path from "node:path";
import zlib from "node:zlib";

const PNG_SIG = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
const N = 32; // output .cube size (Lumina resamples everything to 32^3)
// Input strip size M (M tiles of MxM): 32 for most CS1 LUTs, 64 for e.g. More Relight LUTs.

function extractPng(buf) {
  const start = buf.indexOf(PNG_SIG);
  if (start < 0) throw new Error("no PNG found in input");
  // walk chunks to IEND
  let p = start + 8;
  for (;;) {
    const len = buf.readUInt32BE(p);
    const type = buf.toString("latin1", p + 4, p + 8);
    p += 12 + len;
    if (type === "IEND") return buf.subarray(start, p);
    if (p > buf.length) throw new Error("truncated PNG");
  }
}

function decodePng(png) {
  let p = 8, w, h, bitDepth, colorType, interlace;
  const idat = [];
  while (p < png.length) {
    const len = png.readUInt32BE(p);
    const type = png.toString("latin1", p + 4, p + 8);
    const data = png.subarray(p + 8, p + 8 + len);
    if (type === "IHDR") {
      w = data.readUInt32BE(0); h = data.readUInt32BE(4);
      bitDepth = data[8]; colorType = data[9]; interlace = data[12];
    } else if (type === "IDAT") idat.push(data);
    p += 12 + len;
  }
  if (bitDepth !== 8 || interlace !== 0 || ![2, 6].includes(colorType))
    throw new Error(`unsupported PNG (bitDepth ${bitDepth}, colorType ${colorType}, interlace ${interlace})`);
  const bpp = colorType === 6 ? 4 : 3;
  const raw = zlib.inflateSync(Buffer.concat(idat));
  const stride = w * bpp;
  const out = Buffer.alloc(w * h * 3);
  let prev = Buffer.alloc(stride), cur = Buffer.alloc(stride);
  for (let y = 0; y < h; y++) {
    const f = raw[y * (stride + 1)];
    const line = raw.subarray(y * (stride + 1) + 1, (y + 1) * (stride + 1));
    for (let x = 0; x < stride; x++) {
      const a = x >= bpp ? cur[x - bpp] : 0, b = prev[x], c = x >= bpp ? prev[x - bpp] : 0;
      let v = line[x];
      if (f === 1) v += a;
      else if (f === 2) v += b;
      else if (f === 3) v += (a + b) >> 1;
      else if (f === 4) {
        const pa = Math.abs(b - c), pb = Math.abs(a - c), pc = Math.abs(a + b - 2 * c);
        v += pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
      }
      cur[x] = v & 255;
    }
    for (let x = 0; x < w; x++)
      for (let k = 0; k < 3; k++) out[(y * w + x) * 3 + k] = cur[x * bpp + k];
    [prev, cur] = [cur, prev];
  }
  return { w, h, rgb: out };
}

// Strip layout: tile = blue, x in tile = red, PNG row = green (row 0 = green 0;
// checked against three CS1 workshop LUT .crp files).
function stripToGrid({ w, h, rgb }) {
  const M = h;
  if (w !== M * M || ![16, 32, 64].includes(M)) throw new Error(`expected an MxM*M strip (M = 16/32/64), got ${w}x${h}`);
  const g = new Float64Array(M * M * M * 3);
  for (let b = 0; b < M; b++)
    for (let gr = 0; gr < M; gr++)
      for (let r = 0; r < M; r++) {
        const px = (gr * w + b * M + r) * 3;
        const i = ((b * M + gr) * M + r) * 3;
        for (let k = 0; k < 3; k++) g[i + k] = rgb[px + k] / 255;
      }
  g.size = M;
  return g;
}

function identityGrid() {
  const g = new Float64Array(N * N * N * 3);
  for (let b = 0; b < N; b++) for (let gr = 0; gr < N; gr++) for (let r = 0; r < N; r++) {
    const i = ((b * N + gr) * N + r) * 3;
    g[i] = r / (N - 1); g[i + 1] = gr / (N - 1); g[i + 2] = b / (N - 1);
  }
  g.size = N;
  return g;
}

function sampleGrid(g, rgb) {
  const M = g.size;
  const c = rgb.map((v) => Math.min(Math.max(v, 0), 1) * (M - 1));
  const i0 = c.map(Math.floor), i1 = i0.map((v) => Math.min(v + 1, M - 1)), t = c.map((v, k) => v - i0[k]);
  const at = (r, gr, b, k) => g[((b * M + gr) * M + r) * 3 + k];
  const res = [0, 0, 0];
  for (let k = 0; k < 3; k++) {
    let acc = 0;
    for (let dz = 0; dz < 2; dz++) for (let dy = 0; dy < 2; dy++) for (let dx = 0; dx < 2; dx++) {
      const wgt = (dx ? t[0] : 1 - t[0]) * (dy ? t[1] : 1 - t[1]) * (dz ? t[2] : 1 - t[2]);
      acc += wgt * at(dx ? i1[0] : i0[0], dy ? i1[1] : i0[1], dz ? i1[2] : i0[2], k);
    }
    res[k] = acc;
  }
  return res;
}

// Unity LogC (ARRI EI800) as used by HDRP's LutBuilder3D
const LOGC = { cut: 0.011361, a: 5.555556, b: 0.047996, c: 0.244161, d: 0.386036, e: 5.301883, f: 0.092819 };
const logCToLinear = (x) =>
  x > LOGC.e * LOGC.cut + LOGC.f ? (Math.pow(10, (x - LOGC.d) / LOGC.c) - LOGC.b) / LOGC.a : (x - LOGC.f) / LOGC.e;

// ACES filmic fit by Stephen Hill (sRGB primaries in/out)
function acesHill([r, g, b]) {
  const mul = (m, v) => m.map((row) => row[0] * v[0] + row[1] * v[1] + row[2] * v[2]);
  const IN = [[0.59719, 0.35458, 0.04823], [0.076, 0.90834, 0.01566], [0.0284, 0.13383, 0.83777]];
  const OUT = [[1.60475, -0.53108, -0.07367], [-0.10208, 1.10813, -0.00605], [-0.00327, -0.07276, 1.07602]];
  const rrt = (v) => (v * (v + 0.0245786) - 0.000090537) / (v * (0.983729 * v + 0.432951) + 0.238081);
  return mul(OUT, mul(IN, [r, g, b]).map(rrt)).map((v) => Math.min(Math.max(v, 0), 1));
}

const toSrgb = (v) => (v <= 0.0031308 ? 12.92 * v : 1.055 * Math.pow(v, 1 / 2.4) - 0.055);
const toLinear = (v) => (v <= 0.04045 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4));

function main() {
  const args = process.argv.slice(2);
  const opt = (name, def) => { const i = args.indexOf(name); return i >= 0 ? args.splice(i, 2)[1] : def; };
  const flag = (name) => { const i = args.indexOf(name); return i >= 0 ? (args.splice(i, 1), true) : false; };
  const mode = opt("--mode", "lumina");
  const exposure = Math.pow(2, parseFloat(opt("--exposure", "0")));
  const identity = flag("--identity");
  const [input, outArg] = identity ? [undefined, args[0]] : args;
  if (!input && !identity) {
    console.error("usage: node cs1lut2cube.mjs <in.png|in.crp> [out.cube] [--mode lumina|raw] [--exposure EV] [--identity]");
    process.exit(2);
  }
  const grid = identity ? identityGrid() : stripToGrid(decodePng(extractPng(fs.readFileSync(input))));
  const title = identity ? "Identity" : path.basename(input).replace(/\.(png|crp)$/i, "");
  const out = outArg ?? `${title.replace(/[^\w.-]+/g, "_")}.cube`;

  const lines = [
    `# Converted from CS1 LUT "${title}" by cs1lut2cube.mjs (mode ${mode})`,
    mode === "lumina"
      ? "# Input: Unity LogC (HDRP External tonemapping). Output: linear, ACES (Hill) + CS1 LUT baked in."
      : "# Input/output: sRGB (display-referred), CS1 LUT unchanged.",
    `TITLE "${title}"`,
    `LUT_3D_SIZE ${N}`,
    "DOMAIN_MIN 0.0 0.0 0.0",
    "DOMAIN_MAX 1.0 1.0 1.0",
  ];
  for (let b = 0; b < N; b++) for (let gr = 0; gr < N; gr++) for (let r = 0; r < N; r++) {
    let v;
    if (mode === "raw") {
      v = sampleGrid(grid, [r, gr, b].map((x) => x / (N - 1)));
    } else {
      const lin = [r, gr, b].map((x) => logCToLinear(x / (N - 1)) * exposure);
      v = sampleGrid(grid, acesHill(lin).map(toSrgb)).map(toLinear);
    }
    lines.push(v.map((x) => x.toFixed(6)).join(" "));
  }
  fs.writeFileSync(out, lines.join("\n") + "\n");
  console.log(`wrote ${out} (${grid.size}^3 in, ${N}^3 out, mode ${mode})`);
}

main();
