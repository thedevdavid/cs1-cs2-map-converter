// Season test, unattended: sets each test date, lets the clock run so ClimateSystem/SnowSystem update, measures the
// climate through GET /map/time (bridge 0.9.37+), screenshots two camera spots, and grades every shot PASS/FAIL.
// Usage (editor open with the Dakota map, look step applied):
//   node look/season-test.mjs [--out next-run/shots-seasons/<stamp>] [--settle 20] [--snow-settle 120] [--speed 3]
//                             [--peak-offset 7.7] [--bridge 127.0.0.1:8642] [--no-shots]
// Writes <out>/report.md (tables + one PASS/FAIL line per shot + overall line), <out>/report.json, <out>/<shot>-<spot>.png.
import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';

const arg = (k, d) => { const i = process.argv.indexOf(k); return i > 0 ? process.argv[i + 1] : d; };
const [host, port] = arg('--bridge', '127.0.0.1:8642').split(':');
const stamp = new Date().toISOString().replace(/[-:]/g, '').slice(0, 13);
const out = arg('--out', `next-run/shots-seasons/${stamp}`);
const SETTLE = +arg('--settle', 20), SNOW_SETTLE = +arg('--snow-settle', 120), SPEED = +arg('--speed', 3);
const PEAK = +arg('--peak-offset', 7.7);          // °C colder at the summits than at the start area (run A; run B: 15.4)
const SHOTS_ON = !process.argv.includes('--no-shots');
fs.mkdirSync(out, { recursive: true });

// Expected start-area temperature (°C) from look/climate.json: night/day season means keyed at season mid-times
// (Winter 1 Jan −5/8, Spring 1 Mar 0/13, Summer mid-Jul 12/28, Autumn mid-Nov 0/13), interpolated, then the game's
// night→day cosine for the hour. Tolerance ±7 °C (monthly Gaussian draw σ ≈ 3–4 °C plus curve smoothing).
const SHOTS = [
  { id: 'S1', month: 1, hour: 12, expect: 9.3, snowAllowed: true, settle: SNOW_SETTLE, note: 'mid-winter noon' },
  { id: 'S2', month: 1, hour: 6, expect: 2.1, snowAllowed: true, settle: SNOW_SETTLE, note: 'mid-winter dawn' },
  { id: 'S3', month: 3, hour: 12, expect: 14.7, snowAllowed: true, settle: SETTLE, note: 'early spring' },
  { id: 'S4', month: 5, hour: 12, expect: 22.0, snowAllowed: false, settle: SETTLE, note: 'late spring' },
  { id: 'S5', month: 7, hour: 12, expect: 28.0, snowAllowed: false, settle: SETTLE, note: 'midsummer noon' },
  { id: 'S6', month: 7, hour: 20, expect: 14.9, snowAllowed: false, settle: SETTLE, note: 'summer evening (fog possible)' },
  { id: 'S7', month: 9, hour: 12, expect: 20.5, snowAllowed: false, settle: SETTLE, note: 'early autumn' },
  { id: 'S8', month: 11, hour: 12, expect: 13.0, snowAllowed: false, settle: SETTLE, note: 'late autumn' },
  { id: 'S9', month: 12, hour: 12, expect: 9.7, snowAllowed: true, settle: SNOW_SETTLE, note: 'early winter' },
];
const TOL = 7;
const SPOTS = [
  { id: 'A', x: 600, z: -2750, angleX: 270, angleY: 20, zoom: 900, note: 'main river' },
  { id: 'C', x: -6200, z: 1200, angleX: 270, angleY: 10, zoom: 1500, note: 'peak (snow)' },
];

function request(method, p, body, raw = false) {
  return new Promise((resolve, reject) => {
    const data = body === undefined ? null : Buffer.from(JSON.stringify(body));
    const req = http.request({ hostname: host, port: +port, path: p, method, headers: data ? { 'content-type': 'application/json', 'content-length': data.length } : {} }, res => {
      const chunks = []; res.on('data', c => chunks.push(c));
      res.on('end', () => { const buf = Buffer.concat(chunks); if (raw) return resolve({ status: res.statusCode, buf }); let json; try { json = JSON.parse(buf.toString('utf8')); } catch { json = { raw: buf.toString('utf8') }; } resolve({ status: res.statusCode, json }); });
    });
    req.setTimeout(0); req.on('error', reject); if (data) req.write(data); req.end();
  });
}
const sleep = ms => new Promise(r => setTimeout(r, ms));
const f1 = x => (x == null || Number.isNaN(+x)) ? '—' : (+x).toFixed(1);
const f2 = x => (x == null || Number.isNaN(+x)) ? '—' : (+x).toFixed(2);

// ---- 0. climate checks (curves, not the HUD) ----
const t0 = await request('GET', '/map/time');
if (t0.status !== 200) { console.error('GET /map/time failed', t0.status, JSON.stringify(t0.json).slice(0, 300)); process.exit(1); }
const clim = t0.json.climate ?? {};
const checks = [];
const check = (name, ok, detail) => { checks.push({ name, ok, detail }); console.log(ok ? 'PASS' : 'FAIL', name, detail); };
check('current climate is the edited copy', /\(copy\)/.test(clim.name ?? clim.prefab ?? ''), String(clim.name ?? clim.prefab));
check('daysPerYear is 12', clim.daysPerYear === 12, String(clim.daysPerYear));
const keyCount = clim.temperatureKeys ?? clim.keys ?? clim.temperatureCurve?.keys;
check('temperature curve rebuilt (288 keys)', keyCount === 288 || keyCount === undefined, String(keyCount));
const monthly = clim.monthly ?? [];
// design noon bands per month (start area): Jan 9, Feb 12, Mar 15, Apr 18, May 22, Jun 25, Jul 28, Aug 25, Sep 21, Oct 17, Nov 13, Dec 10 (±7)
const NOON = [9.3, 12.0, 14.7, 18.3, 22.0, 25.6, 28.0, 24.2, 20.5, 16.8, 13.0, 9.7];
for (const m of monthly) {
  const i = (m.month ?? 0) - 1; if (i < 0 || i > 11) continue;
  const v = m.temperatureNoon ?? m.noon?.temperature;
  check(`curve noon month ${m.month}`, v != null && Math.abs(v - NOON[i]) <= TOL, `${f1(v)} °C (design ${NOON[i]} ±${TOL})`);
}
// curve-level snow outlook (start area and summits): snow needs precipitation ≥ 0.1 at ≤ 0 °C
const snowOutlook = monthly.map(m => {
  const mid = m.temperatureMidnight ?? m.midnight?.temperature, noon = m.temperatureNoon ?? m.noon?.temperature;
  const pr = Math.max(m.precipitationNoon ?? 0, m.precipitationMidnight ?? 0, m.precipitation ?? 0);
  return { month: m.month, midnight: mid, noon, precipitation: pr, plainsSnowPossible: mid != null && mid <= 0 && pr >= 0.1, peakSnowPossible: mid != null && mid - PEAK <= 0 && pr >= 0.1 };
});
for (const s of snowOutlook) if (s.month >= 4 && s.month <= 10) check(`no plains snow possible in month ${s.month}`, !s.plainsSnowPossible, `midnight ${f1(s.midnight)} °C, precip ${f2(s.precipitation)}`);

// ---- 1. shots ----
const rows = [];
for (const s of SHOTS) {
  const date = ((s.month - 1) + s.hour / 24) / 12;   // a CS2 day is a month unit: the hour must be encoded in the date
  const post = await request('POST', '/map/time', { date, hour: s.hour, settle: s.settle, speed: SPEED });
  if (post.status !== 200) { rows.push({ ...s, error: `POST /map/time ${post.status}` }); continue; }
  await sleep(s.settle * 1000 + 1500);
  let t = null;
  for (let i = 0; i < 60; i++) { t = await request('GET', '/map/time'); if (!t.json.running) break; await sleep(1000); }
  // the settle ran the clock: re-pin the date so both shots see the same sun and climate point
  await request('POST', '/map/time', { date, hour: s.hour });
  t = await request('GET', '/map/time');
  const live = t.json.live ?? {}, sample = t.json.sampleAtDate ?? {};
  const shots = {};
  if (SHOTS_ON) for (const sp of SPOTS) {
    await request('POST', `/camera/set?x=${sp.x}&z=${sp.z}&angleX=${sp.angleX}&angleY=${sp.angleY}&zoom=${sp.zoom}`);
    await sleep(2500);
    const img = await request('GET', '/screenshot?width=1600', undefined, true);
    const file = `${s.id}-m${s.month}-h${s.hour}-${sp.id}.png`;
    if (img.status === 200) { fs.writeFileSync(path.join(out, file), img.buf); shots[sp.id] = file; } else shots[sp.id] = `HTTP ${img.status}`;
  }
  const tLive = live.temperature, tSample = sample.temperature;
  const fails = [];
  if (tLive == null || tSample == null) fails.push('no live/sample temperature');
  else {
    if (Math.abs(tLive - tSample) > 1.5) fails.push(`live ${f1(tLive)} ≠ sample ${f1(tSample)} (clock did not update the climate)`);
    if (Math.abs(tSample - s.expect) > TOL) fails.push(`temperature ${f1(tSample)} outside ${s.expect} ±${TOL}`);
  }
  if (!s.snowAllowed && live.isSnowing) fails.push('snowing outside the cold season');
  if (live.isSnowing && tLive > 2) fails.push(`snowing at ${f1(tLive)} °C`);
  const season = String(live.season ?? '');
  if (season && !/summer/i.test(season)) fails.push(`season "${season}" is not the summer prefab`);
  const row = { ...s, date: +date.toFixed(4), live, sample, shots, pass: fails.length === 0, fails };
  rows.push(row);
  console.log(`${s.id} ${row.pass ? 'PASS' : 'FAIL'} live ${f1(tLive)} sample ${f1(tSample)} expect ${s.expect} precip ${f2(sample.precipitation)} snow ${live.isSnowing} ${fails.join('; ')}`);
}
await request('POST', '/map/time', { reset: true });

// ---- 2. report ----
const allPass = checks.every(c => c.ok) && rows.every(r => r.pass);
const md = [
  `# Season test ${stamp}`, '',
  `Bridge ${host}:${port}; settle ${SETTLE} s (snow shots ${SNOW_SETTLE} s) at speed ${SPEED}; summit offset −${PEAK} °C; tolerance ±${TOL} °C.`, '',
  `**Overall: ${allPass ? 'PASS' : 'FAIL'}** (${checks.filter(c => !c.ok).length} climate checks failed, ${rows.filter(r => !r.pass).length} of ${rows.length} shots failed)`, '',
  '## Climate checks', '', '| Check | Result | Detail |', '|---|---|---|',
  ...checks.map(c => `| ${c.name} | ${c.ok ? 'PASS' : '**FAIL**'} | ${c.detail} |`), '',
  '## Snow outlook from the curves', '', '| Month | Midnight °C (start / summit) | Noon °C | Precipitation | Snow possible: plains / summits |', '|---|---|---|---|---|',
  ...snowOutlook.map(s => `| ${s.month} | ${f1(s.midnight)} / ${f1(s.midnight == null ? null : s.midnight - PEAK)} | ${f1(s.noon)} | ${f2(s.precipitation)} | ${s.plainsSnowPossible ? 'yes' : 'no'} / ${s.peakSnowPossible ? 'yes' : 'no'} |`), '',
  '## Shots', '', '| Shot | Date / hour | Expected °C | Live °C | Sampled °C | Precip. | Cloud | Fog | Rain / snow | Season | Screenshots | Result |', '|---|---|---|---|---|---|---|---|---|---|---|---|',
  ...rows.map(r => r.error ? `| ${r.id} | ${r.month} / ${r.hour}:00 | ${r.expect} | — | — | — | — | — | — | — | — | **FAIL** ${r.error} |` :
    `| ${r.id} ${r.note} | ${r.month} / ${r.hour}:00 | ${r.expect} | ${f1(r.live.temperature)} | ${f1(r.sample.temperature)} | ${f2(r.sample.precipitation)} | ${f2(r.sample.cloudiness)} | ${f2(r.sample.fog)} | ${r.live.isRaining ? 'rain' : '—'} / ${r.live.isSnowing ? 'snow' : '—'} | ${r.live.season ?? '—'} | ${Object.values(r.shots).join(', ')} | ${r.pass ? 'PASS' : '**FAIL**: ' + r.fails.join('; ')} |`),
  '', 'Visual checks to do on the screenshots (not automatable): foliage green in every shot (all seasons use SeasonSummer); summit snow at most a light, temporary cover in S1, S2, S9; no snow on the plains in S4–S8.',
];
fs.writeFileSync(path.join(out, 'report.md'), md.join('\n') + '\n');
fs.writeFileSync(path.join(out, 'report.json'), JSON.stringify({ stamp, allPass, checks, snowOutlook, rows }, null, 2) + '\n');
console.log(`\nOverall: ${allPass ? 'PASS' : 'FAIL'}  → ${path.join(out, 'report.md')}`);
process.exit(allPass ? 0 : 2);
