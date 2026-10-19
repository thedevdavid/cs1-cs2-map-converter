// Minimal 16-bit gray PNG reader + bilinear sampler matching net-converter's Heightmap (row 0 = north, ±7168 m).
import { readFileSync } from "node:fs";
import { inflateSync } from "node:zlib";
export function loadHeightmap(path, metresPerUnit) {
  const b = readFileSync(path); let pos = 8, w = 0, h = 0; const idat = [];
  while (pos < b.length) { const len = b.readUInt32BE(pos), type = b.toString("ascii", pos + 4, pos + 8);
    if (type === "IHDR") { w = b.readUInt32BE(pos + 8); h = b.readUInt32BE(pos + 12); }
    else if (type === "IDAT") idat.push(b.subarray(pos + 8, pos + 8 + len)); else if (type === "IEND") break; pos += 12 + len; }
  const raw = inflateSync(Buffer.concat(idat)), stride = w * 2, v = new Uint16Array(w * h);
  let prev = Buffer.alloc(stride), cur = Buffer.alloc(stride);
  for (let y = 0; y < h; y++) { const f = raw[y * (stride + 1)]; raw.copy(cur, 0, y * (stride + 1) + 1, (y + 1) * (stride + 1));
    for (let i = 0; i < stride; i++) { const a = i >= 2 ? cur[i - 2] : 0, up = prev[i], c = i >= 2 ? prev[i - 2] : 0;
      const p = a + up - c, pa = Math.abs(p - a), pb = Math.abs(p - up), pc = Math.abs(p - c);
      cur[i] = (cur[i] + [0, a, up, (a + up) >> 1, pa <= pb && pa <= pc ? a : pb <= pc ? up : c][f]) & 255; }
    for (let x = 0; x < w; x++) v[y * w + x] = cur[2 * x] << 8 | cur[2 * x + 1]; [prev, cur] = [cur, prev]; }
  const half = 7168, mpp = 2 * half / w;
  return (x, z) => { let px = (x + half) / mpp - 0.5, py = (half - z) / mpp - 0.5; px = Math.max(0, Math.min(w - 1, px)); py = Math.max(0, Math.min(h - 1, py));
    const x0 = Math.floor(px), y0 = Math.floor(py), x1 = Math.min(x0 + 1, w - 1), y1 = Math.min(y0 + 1, h - 1), fx = px - x0, fy = py - y0;
    return (v[y0 * w + x0] * (1 - fx) * (1 - fy) + v[y0 * w + x1] * fx * (1 - fy) + v[y1 * w + x0] * (1 - fx) * fy + v[y1 * w + x1] * fx * fy) * metresPerUnit; };
}
