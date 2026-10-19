// Regenerate everything in cs1-offline/out from the CS1 map .crp.
// Usage: node cs1-offline/run.mjs [--crp <map.crp>] [--dump <DakotaExport main json>] [--workshop <dir>] [--heightmap <cs1 heightmap png>] [--out <dir>]
import fs from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const local = process.env.LOCALAPPDATA ?? path.join(os.homedir(), 'AppData', 'Local');
const opt = {
  crp: 'D:/SteamLibrary/steamapps/workshop/content/255710/2972879554/Dakota Falls.crp',
  dump: path.join(local, 'DakotaExport', 'map-20261018-132303.json'),
  workshop: 'D:/SteamLibrary/steamapps/workshop/content/255710',
  heightmap: path.join(local, 'Colossal Order', 'Cities_Skylines', 'Addons', 'MapEditor', 'Heightmaps', 'DakotaFalls.png'),
  out: path.join(here, 'out'),
};
const argv = process.argv.slice(2);
for (let i = 0; i < argv.length; i += 2) { const k = argv[i].replace(/^--/, ''); if (!(k in opt)) throw new Error(`unknown option ${argv[i]}`); opt[k] = argv[i + 1]; }
for (const k of ['crp', 'dump', 'workshop']) if (!fs.existsSync(opt[k])) throw new Error(`${k} not found: ${opt[k]}`);
const payloads = path.join(opt.out, 'payloads');

function step(title, cmd, args) {
  console.log(`\n== ${title}`);
  const t = Date.now();
  const r = spawnSync(cmd, args, { stdio: ['ignore', 'pipe', 'pipe'], encoding: 'utf8', });
  const out = (r.stdout ?? '') + (r.stderr ?? '');
  process.stdout.write(out.split(/\r?\n/).filter(l => !/warning (CS|SYSLIB|IL)/.test(l)).join('\n'));
  if (r.status !== 0) throw new Error(`${title} failed (exit ${r.status})`);
  console.log(`(${((Date.now() - t) / 1000).toFixed(1)} s)`);
}

fs.mkdirSync(opt.out, { recursive: true });
step('extract mod data', process.execPath, [path.join(here, 'src', 'extract.mjs'), opt.crp, payloads]);
step('procedural objects', 'dotnet', ['run', path.join(here, 'src', 'po-offline.cs'), '--', payloads, opt.workshop, opt.out, ...(fs.existsSync(opt.heightmap) ? [opt.heightmap] : [])]);
step('TM:PE', 'dotnet', ['run', path.join(here, 'src', 'tmpe-offline.cs'), '--', opt.crp, payloads, opt.dump, opt.out]);
step('IMT and Node Controller', process.execPath, [path.join(here, 'src', 'markings.mjs'), payloads, opt.dump, opt.out]);

fs.copyFileSync(path.join(here, 'src', 'audit-corrections.md'), path.join(opt.out, 'audit-corrections.md'));
const files = fs.readdirSync(opt.out).filter(f => fs.statSync(path.join(opt.out, f)).isFile()).sort();
const manifest = { generatedUtc: new Date().toISOString(), inputs: opt, files: Object.fromEntries(files.map(f => [f, fs.statSync(path.join(opt.out, f)).size])) };
fs.writeFileSync(path.join(opt.out, 'manifest.json'), JSON.stringify(manifest, null, 1));
console.log('\n== outputs');
for (const [f, s] of Object.entries(manifest.files)) console.log(`${String(s).padStart(10)}  ${f}`);
