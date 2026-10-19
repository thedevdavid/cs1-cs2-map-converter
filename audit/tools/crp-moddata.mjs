// List the mod-data entries (SimulationManager serializable data: key -> byte[]) inside the main data asset
// of a CS1 map or save, plus a few decoded hints. Works on the asset as stored in the .crp (no extraction step).
// Usage: node crp-moddata.mjs <file.crp> [--json] [--extract <dir>]   (--extract writes each payload, gzip ones unzipped)
//
// Encoding notes (found empirically, CS1 1.21, ColossalFramework DataSerializer):
// - strings: 0x80, UInt16 BE length, then each char stored as the byte delta to the previous char (start 0x80)
// - the serializable-data dictionary follows the manager type list: Int32 BE count, then per entry
//   key string, UInt24 BE length, payload. Payloads >16 MB are split by the mods themselves (e.g. PO "...Key1").
import { readTable } from './crp-table.mjs';
import fs from 'node:fs';
import zlib from 'node:zlib';

function readStr(b, p) {
  if (b[p] !== 0x80) return null;
  const L = b.readUInt16BE(p + 1);
  let c = 0x80, s = '';
  for (let k = 0; k < L; k++) { c = (c + b[p + 3 + k]) & 255; if (c < 0x20 || c > 0x7e) return null; s += String.fromCharCode(c); }
  return { s, end: p + 3 + L };
}

export function modData(file) {
  const t = readTable(file);
  const a = t.assets.find(x => x.typeRaw === 52) ?? t.assets.find(x => x.name.endsWith('_Data'));
  const fd = fs.openSync(file, 'r');
  const b = Buffer.alloc(a.size);
  fs.readSync(fd, b, 0, a.size, a.offset);
  fs.closeSync(fd);
  // SimulationMetaData strings at the head (m_gameInstanceIdentifier, environment, names, map theme, ...)
  const head = [];
  for (let p = 0; p < 4096 && head.length < 40; p++) { const r = readStr(b, p); if (r && r.s.length >= 2) { head.push({ at: p, s: r.s }); p = r.end - 1; } }
  // the dictionary starts right after "PathManager+Data, ..." (last manager type); find the first key after it
  const pm = head.find(h => h.s.startsWith('PathManager+Data'));
  let p = pm ? readStr(b, pm.at).end : 0;
  while (p < 8192 && !(b[p] === 0x80 && b[p + 1] === 0x00 && readStr(b, p))) p++;
  const count = b.readInt32BE(p - 4);
  const entries = [];
  for (let i = 0; i < count; i++) {
    const k = readStr(b, p);
    if (!k) { entries.push({ error: `no key at ${p}` }); break; }
    const len = b.readUIntBE(k.end, 3);
    const data = b.subarray(k.end + 3, k.end + 3 + len);
    entries.push({ key: k.s, at: p, size: len, hint: hint(data), data });
    p = k.end + 3 + len;
  }
  return { file, dataAsset: a.name, dataSize: a.size, metaStrings: head.filter(h => !h.s.includes('+Data, ')).map(h => h.s), count, entries };
}

function hint(d) {
  const bf = d.length > 20 && d.readUInt32LE(1) === 1 && d.readInt32LE(5) === -1;
  const txt = d.subarray(0, 4096).toString('latin1').replace(/[^\x20-\x7e]+/g, ' ');
  const words = [...new Set(txt.match(/[A-Za-z][\w.+`\-]{5,}/g) ?? [])].filter(w => !/^(System|mscorlib|Version|Culture|PublicKeyToken)/.test(w));
  const gz = d[0] === 0x1f && d[1] === 0x8b;
  return (bf ? 'BinaryFormatter ' : '') + (gz ? 'gzip ' : '') + words.slice(0, 8).join(' ');
}

const [file, flag, outDir] = process.argv.slice(2);
if (file) {
  const r = modData(file);
  if (flag === '--extract') {
    fs.mkdirSync(outDir, { recursive: true });
    for (const e of r.entries) {
      let d = e.data, name = e.key.replace(/[^\w.-]/g, '_');
      if (d[0] === 0x1f && d[1] === 0x8b) { d = zlib.gunzipSync(d); name += '.unz'; }
      fs.writeFileSync(`${outDir}/${name}.bin`, d);
    }
    console.log(`${r.entries.length} payloads written to ${outDir}`);
  } else if (flag === '--json') console.log(JSON.stringify({ ...r, entries: r.entries.map(({ data, ...e }) => e) }, null, 1));
  else {
    console.log(`${r.file}\n  data asset ${r.dataAsset} (${r.dataSize} B); meta strings: ${r.metaStrings.join(' | ')}\n  ${r.count} mod-data entries`);
    for (const e of r.entries) console.log(`  ${String(e.size ?? '').padStart(10)}  ${e.key ?? e.error}   ${e.hint ?? ''}`);
  }
}
