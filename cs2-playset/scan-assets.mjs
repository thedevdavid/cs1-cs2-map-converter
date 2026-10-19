// Scans every pdx_mods item in a playset and dumps its prefabs (from .cok bundles) to JSON.
// Usage: node scan-assets.mjs [playsetName] > out.json
import fs from "node:fs";
import path from "node:path";
import zlib from "node:zlib";

const modsDir = path.join(process.env.USERPROFILE, "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", ".cache", "Mods");
const playsetName = process.argv[2] || "lessgo-withEAI";
const EAI_ID = "80529";

// Minimal zip reader (stored + deflate), reading only what it needs.
function zipEntries(file) {
  const fd = fs.openSync(file, "r");
  const size = fs.fstatSync(fd).size;
  const tailLen = Math.min(size, 65557);
  const tail = Buffer.alloc(tailLen);
  fs.readSync(fd, tail, 0, tailLen, size - tailLen);
  let eocd = -1;
  for (let i = tailLen - 22; i >= 0; i--) if (tail.readUInt32LE(i) === 0x06054b50) { eocd = i; break; }
  if (eocd < 0) { fs.closeSync(fd); return null; }
  let count = tail.readUInt16LE(eocd + 10), cdSize = tail.readUInt32LE(eocd + 12), cdOff = tail.readUInt32LE(eocd + 16);
  if (cdOff === 0xffffffff || count === 0xffff) { // zip64
    const loc = tail.readUInt32LE(eocd - 20) === 0x07064b50 ? Number(tail.readBigUInt64LE(eocd - 12)) : -1;
    const z = Buffer.alloc(56); fs.readSync(fd, z, 0, 56, loc);
    count = Number(z.readBigUInt64LE(32)); cdSize = Number(z.readBigUInt64LE(40)); cdOff = Number(z.readBigUInt64LE(48));
  }
  const cd = Buffer.alloc(cdSize); fs.readSync(fd, cd, 0, cdSize, cdOff);
  const entries = [];
  for (let p = 0, n = 0; n < count && p < cdSize; n++) {
    const method = cd.readUInt16LE(p + 10);
    let csize = cd.readUInt32LE(p + 20), usize = cd.readUInt32LE(p + 24);
    const nl = cd.readUInt16LE(p + 28), xl = cd.readUInt16LE(p + 30), cl = cd.readUInt16LE(p + 32);
    let off = cd.readUInt32LE(p + 42);
    const name = cd.toString("utf8", p + 46, p + 46 + nl);
    // zip64 extra
    let x = p + 46 + nl; const xe = x + xl;
    while (x + 4 <= xe) {
      const id = cd.readUInt16LE(x), len = cd.readUInt16LE(x + 2); let q = x + 4;
      if (id === 1) {
        if (usize === 0xffffffff) { usize = Number(cd.readBigUInt64LE(q)); q += 8; }
        if (csize === 0xffffffff) { csize = Number(cd.readBigUInt64LE(q)); q += 8; }
        if (off === 0xffffffff) { off = Number(cd.readBigUInt64LE(q)); q += 8; }
      }
      x += 4 + len;
    }
    entries.push({ name, method, csize, usize, off });
    p += 46 + nl + xl + cl;
  }
  const read = e => {
    const h = Buffer.alloc(30); fs.readSync(fd, h, 0, 30, e.off);
    const start = e.off + 30 + h.readUInt16LE(26) + h.readUInt16LE(28);
    const b = Buffer.alloc(e.csize); fs.readSync(fd, b, 0, e.csize, start);
    return e.method === 8 ? zlib.inflateRawSync(b) : b;
  };
  return { entries, read, close: () => fs.closeSync(fd) };
}

function parseLoc(buf) {
  const out = {};
  const s = buf.toString("latin1");
  const re = /Assets\.NAME\[([^\]]+)\]([\s\S])/g; let m;
  while ((m = re.exec(s))) {
    const len = m[2].charCodeAt(0), at = m.index + m[0].length;
    out[m[1]] = buf.toString("utf8", at, at + len);
  }
  return out;
}

// Binary prefab format (m_prefabFormat 1): UTF-16 type strings, "name" field as 0x01 + uint32 length + UTF-16.
const u16 = (buf, o) => buf.subarray(o, o + ((buf.length - o) & ~1)).toString("utf16le"); // odd-offset toString crashes Node 26
function parseBinaryPrefab(buf) {
  const types = new Set();
  for (const off of [0, 1]) for (const m of u16(buf, off).matchAll(/Game\.Prefabs\.(\w+)/g)) types.add(m[1]);
  const typeM = u16(buf, 1).match(/Game\.Prefabs\.(\w+)/) || u16(buf, 0).match(/Game\.Prefabs\.(\w+)/);
  let name;
  const key = Buffer.from("name", "utf16le").subarray(0, 9);
  const at = buf.indexOf(key);
  if (at >= 0) {
    const len = buf.readUInt32LE(at + 9);
    if (len > 0 && len < 512) name = buf.toString("utf16le", at + 13, at + 13 + len * 2);
  }
  return { type: typeM ? typeM[1] : "?", name, comps: [...types], meshes: [], bounds: null, group: null, binary: true };
}

function parsePrefab(buf) {
  if (buf[0] !== 0x7b && !(buf[0] === 0xef && buf[3] === 0x7b)) return parseBinaryPrefab(buf);
  const txt = buf.toString("utf8");
  const type = (txt.match(/"\$type":\s*"0\|Game\.Prefabs\.(\w+)/) || [])[1] || "?";
  const name = (txt.match(/^\s{4}"name":\s*"([^"]*)"/m) || [])[1];
  const comps = [...new Set([...txt.matchAll(/"\|?\d*\|?Game\.Prefabs\.(\w+), Game"/g)].map(m => m[1]))];
  const meshes = [...txt.matchAll(/"m_Mesh":\s*\$fstrref:"CID:([0-9a-f]+)"/g)].map(m => m[1]);
  let bounds = null;
  const b = txt.match(/"m_Bounds":[\s\S]*?"x":\s*([-\d.eE]+),\s*"y":\s*([-\d.eE]+),\s*"z":\s*([-\d.eE]+)[\s\S]*?"x":\s*([-\d.eE]+),\s*"y":\s*([-\d.eE]+),\s*"z":\s*([-\d.eE]+)/);
  if (b) bounds = b.slice(1, 7).map(Number);
  const group = (txt.match(/"m_Group":\s*\$fstrref:"([^"]+)"/) || [])[1];
  return { type, name, comps, meshes, bounds, group };
}

const cfg = JSON.parse(fs.readFileSync(path.join(modsDir, "playset_config.json"), "utf8"));
const cache = JSON.parse(fs.readFileSync(path.join(modsDir, "pdx_mods_cache.json"), "utf8"));
const byId = new Map(cache.items.map(i => [i.id, i]));
const playset = cfg.playsets.find(p => p.presentation?.name === playsetName);
const installed = fs.readdirSync(path.join(modsDir, "pdx_mods"));

const walk = (dir, base = dir, acc = []) => {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    if (e.name === ".cpatch" || e.name === ".metadata") continue;
    const p = path.join(dir, e.name);
    if (e.isDirectory()) walk(p, base, acc); else acc.push(path.relative(base, p).replace(/\\/g, "/"));
  }
  return acc;
};

const result = [];
for (const m of playset.mods) {
  const meta = byId.get(m.sourceId) || {};
  const dir = installed.find(d => d.startsWith(m.sourceId + "_"));
  const files = dir ? walk(path.join(modsDir, "pdx_mods", dir)) : [];
  const deps = (meta.dependencies || []).map(d => d.id).filter(Boolean);
  const looseDirs = [...new Set(files.filter(f => f.includes("/")).map(f => f.split("/")[0]))];
  const eaiLoose = looseDirs.filter(d => /^Custom|^(Decals|Surfaces|NetLanes|Props)$/i.test(d));
  const item = {
    id: m.sourceId, enabled: m.isEnabled, name: meta.displayName, dir, deps,
    tags: (meta.tags || []).map(t => t.id).filter(Boolean),
    hasDll: files.some(f => /\.dll$/i.test(f)), coks: files.filter(f => /\.cok$/i.test(f)),
    looseDirs, eaiRequired: deps.includes(EAI_ID) || eaiLoose.length > 0, eaiLoose, prefabs: [],
  };
  for (const c of item.coks) {
    const z = zipEntries(path.join(modsDir, "pdx_mods", dir, c));
    if (!z) continue;
    try {
      const loc = {}, cidOf = {}, raw = [];
      for (const e of z.entries) {
        if (/_en-US\.loc$/.test(e.name)) Object.assign(loc, parseLoc(z.read(e)));
        else if (/\.Prefab\.cid$/.test(e.name)) cidOf[e.name.slice(0, -4)] = z.read(e).toString("latin1").replace(/^﻿/, "").trim();
        else if (e.name === "package.deps") { try { item.packageDeps = JSON.parse(z.read(e).toString("utf8").replace(/^﻿/, "")).modDependencies; } catch {} }
      }
      for (const e of z.entries) if (/\.Prefab$/.test(e.name)) raw.push({ file: e.name, cid: cidOf[e.name], ...parsePrefab(z.read(e)) });
      const byCid = new Map(raw.map(r => [r.cid, r]));
      for (const r of raw) {
        if (r.type === "RenderPrefab") continue;
        const mesh = r.meshes.map(c => byCid.get(c)).filter(Boolean);
        const meshComps = [...new Set(mesh.flatMap(x => x.comps))];
        const bs = mesh.map(x => x.bounds).filter(Boolean);
        const bounds = bs.length ? [Math.min(...bs.map(b => b[0])), Math.min(...bs.map(b => b[1])), Math.min(...bs.map(b => b[2])), Math.max(...bs.map(b => b[3])), Math.max(...bs.map(b => b[4])), Math.max(...bs.map(b => b[5]))] : null;
        item.prefabs.push({ name: r.name, type: r.type, display: loc[r.name], comps: r.comps, meshComps, bounds, cok: c });
      }
    } finally { z.close(); }
  }
  result.push(item);
}
process.stdout.write(JSON.stringify(result, null, 1));
