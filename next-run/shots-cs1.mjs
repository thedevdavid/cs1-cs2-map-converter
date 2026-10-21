// CS1 reference shots at the same cameras as the CS2 set (next-run/shots-a8.sh), via DakotaExport v0.5's camera.json command.
// node next-run/shots-cs1.mjs  → writes %LOCALAPPDATA%\DakotaExport\camera.json, waits for camera.done.json, copies the PNGs to next-run/shots-cs1/.
import { writeFileSync, renameSync, existsSync, readFileSync, mkdirSync, unlinkSync, copyFileSync } from "node:fs";
import { join } from "node:path";
const dir = join(process.env.LOCALAPPDATA, "DakotaExport");
const out = join(process.cwd(), "next-run", "shots-cs1"); mkdirSync(out, { recursive: true });
// name, x, z, yaw, pitch, distance  (CS2 camera list; CS1 and CS2 share x east / z north with the origin at the map centre)
const cams = [
  ["01-br006-truss-arch-side", -3525, -3181, 144, 12, 380], ["02-br009-truss-arch-pair-and-approach", -4420, 50, 150, 25, 650],
  ["03-br005-truss-arch-pair", 437, 200, 60, 20, 650], ["04-detroit-suspension-side", 1533, -3498, 127, 10, 1100],
  ["05-detroit-approach-piers", 1288, -3820, 127, 15, 380], ["06-br003-piers", -1620, -1672, 130, 15, 380],
  ["07-ourense-rail-truss-arch", 778, 2666, 100, 15, 420], ["08-pedrido-rail-truss-arch", -325, -2733, 80, 15, 420],
  ["09-covered-bridge-gravel", 3775, 2919, 0, 20, 220], ["10-dam-and-power-lines", 1300, 4680, 180, 25, 700],
  ["11-substation", 1600, 4544, 0, 35, 260], ["12-interchange-oneway-joins", 2995, 820, 0, 50, 700],
  ["13-crossing-restored", -4224, -60, 0, 50, 500], ["14-dead-end-fix", 817, 4013, 0, 45, 420],
  ["15-interchange-umber-driftwood", 608, -4721, 0, 35, 450], ["16-sand-paint-river", 634, -1505, 0, 45, 900],
  ["17-sand-paint-southeast", 4096, -6127, 0, 45, 1500], ["18-sand-paint-lake", -1666, -1852, 0, 45, 1500],
  ["19-dry-grass-paint-west", -6150, 4397, 0, 45, 1000], ["20-dry-grass-paint-southeast", 4565, -6194, 0, 45, 1000],
  ["21-bare-earth-patch", -34, -2346, 0, 40, 220], ["22-fort-ruins", 5, -2619, 0, 30, 320],
  ["23-po-rocks-cliffs-south", -3000, -5500, 0, 30, 900], ["24-po-rocks-cliffs-west", -6500, 1000, 0, 30, 900],
  ["25-world-map-east-flats", 7000, 0, 90, 15, 3000], ["26-world-map-west-range", -7000, 0, 270, 15, 3000],
  ["W1-water-main-channel", 600, -2750, 0, 35, 700], ["27-detroit-wide", 1532, -3498, 37, 12, 1600],
  ["30-interchange-05-stack", 3493, 2152, 0, 50, 700], ["31-interchange-16", 3015, 1505, 0, 50, 500],
];
const req = { hideUi: true, frames: 20, shots: cams.map(([n, x, z, yaw, pitch, distance]) => ({ x, z, yaw, pitch, distance, out: join(out, n + ".png") })) };
const done = join(dir, "camera.done.json"); if (existsSync(done)) unlinkSync(done);
writeFileSync(join(dir, "camera.json.tmp"), JSON.stringify(req, null, 1)); renameSync(join(dir, "camera.json.tmp"), join(dir, "camera.json"));
console.log(`requested ${cams.length} shots; waiting for ${done}`);
const t0 = Date.now();
while (!existsSync(done)) { if (Date.now() - t0 > 15 * 60e3) { console.log("timeout"); process.exit(1); } await new Promise(r => setTimeout(r, 2000)); }
const d = JSON.parse(readFileSync(done, "utf8"));
console.log(`ok ${d.okCount}/${cams.length}; requestError=${d.requestError}`);
for (const s of d.shots || []) console.log(`${s.ok ? "✓" : "✗"} ${s.out} ${s.bytes || ""} ${s.error || ""} dist=${s.actual?.distance?.toFixed?.(0)}`);
