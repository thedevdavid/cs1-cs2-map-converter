// Read-only: proposes a CS2 replacement per PO structure object (cs1-offline/out/po-offline.json).
// Output: bridges/data/po-structures-proposal.json and a markdown table on stdout. Coordinates are CS1 metres (x, z unchanged in CS2);
// yBottom is the bottom of the PO mesh bounds; y2 = 2 × yBottom for the x2 set (height factor 2).
const fs = require('fs');
const objs = JSON.parse(fs.readFileSync('cs1-offline/out/po-offline.json', 'utf8')).objects
  .filter(o => ['bridge/pillar', 'tunnel/shed', 'paved block'].includes(o.subClass));
const ids = (...a) => new Set(a.flat());
const range = (a, b) => Array.from({ length: b - a + 1 }, (_, i) => a + i);
function rule(o) {
  const id = o.id, sz = o.boundsMax.map((v, i) => v - o.boundsMin[i]), bp = o.basePrefab;
  if (/Detroit Bridge/.test(bp)) return ['BR001 approach pier', 'ITBP_Pillar Piers 01', '140978', 'object, only if the deck gets noPillars; else leave to CS2 pillars'];
  if (/3-Lane Highway Pillar/.test(bp)) return ['BR003 hammerhead pier', 'ITBP_Pillar Piers 01', '140978', 'object, only with noPillars on BR003'];
  if (/artdeco-truss01/.test(bp)) return id === 690 ? ['BR009 green truss arch', 'BXP Truss Arch Bridge - Highway Oneway - 2 Lanes (net, per PLAN.md); object fallback PittTrussBridge', '92245 / 142603', 'net swap on BR009; fallback one object at the group centre'] : ['BR009 truss (stacked copy of 690)', 'no object (covered by 690)', '', ''];
  if (/R69 Tunnel Entrance 1/.test(bp) && o.color[2] > 0.8) return id === 503 ? ['BR001 blue arch under the 3L approach', 'BXP Truss Arch Bridge - Highway Oneway - 3 Lanes (net on that approach edge); object fallback PittTrussBridge', '92245 / 142603', 'net swap; fallback object'] : ['BR001 blue arch (twin of 503)', 'no object (covered by 503)', '', ''];
  if (/R69 Tunnel Entrance 1/.test(bp)) return id === 27 ? ['BR006 red deck arch', 'StoneArchRailBridge or OlympicBridge (owner choice, PLAN.md Q3)', '135944 / 136168', 'object under the plain Small Road deck'] : ['BR006 red arch (twin of 27)', 'no object (covered by 27)', '', ''];
  if (range(1, 20).includes(id)) return ['dam body / crest (R69 blocks)', 'no equivalent needed: the hand-placed HydroelectricPowerPlant01 + dam deck replace it', '', ''];
  if (id === 620) return ['sign-gantry-like frame over the 2L interstate', 'HWY Truss Gantry 2L  A', '132188', 'object, yaw to be checked against the prefab axis'];
  if (id === 619 || id === 621) return ['gantry post (part of 620)', 'no object (covered by 620)', '', ''];
  if (/R69 Tunnel Entrance Block 2/.test(bp)) return ['abutment block at a short interstate viaduct next to a tunnel mouth', 'leave to the game (CS2 builds its own abutment walls); optional Wall32x16', '154132', 'optional object'];
  if (/rock_c/.test(bp)) return ['JP Rock shed piece over the 4-lane national road', 'no equivalent (no CS2 rock shed found); optional CliffSideTall overhang', '137334 (in E2C)', 'optional object with Anarchy'];
  if (/Cube/.test(bp)) {
    const g = o.group && o.group.rootId;
    if (g === 27 && sz[0] < 5) return ['BR006 spandrel column', 'Pillar1x16 (dimensions [U])', '154132', 'object; exclude from pavement/quay'];
    if (g === 27) return ['BR006 viaduct pier', 'Pillar4x16 (dimensions [U])', '154132', 'object; exclude from pavement/quay'];
    if (g === 25) return ['BR022 arch footing pad', 'BasePlateConcrete32 (dimensions [U])', '154132', 'object; exclude from pavement'];
    if (g === 288) return ['BR005 Hernando-Desoto bearing / column', 'no object (the truss-arch net brings its own supports)', '', 'exclude from pavement/quay'];
    return ['bridge abutment / end block', 'keep current handling (pavement top) or leave to the game', '', ''];
  }
  return ['?', 'no equivalent', '', ''];
}
const out = objs.map(o => { const [role, prefab, mod, how] = rule(o); const sz = o.boundsMax.map((v, i) => +(v - o.boundsMin[i]).toFixed(0));
  const c = [0, 2].map(i => +((o.boundsMin[i] + o.boundsMax[i]) / 2).toFixed(1));
  return { id: o.id, subClass: o.subClass, basePrefab: o.basePrefab.replace(/^\d+\./, ''), loads: o.loadsInPlayset, role, prefab, mod, how,
    x: c[0], z: c[1], yaw: +(o.yawDegrees ?? 0).toFixed(1), yBottom: +o.boundsMin[1].toFixed(1), y2: +(2 * o.boundsMin[1]).toFixed(1), size: sz }; });
fs.writeFileSync('bridges/data/po-structures-proposal.json', JSON.stringify(out, null, 0).replace(/\},\{/g, '},\n{'));
// Collapse consecutive objects with the same role and prefab into one row.
const rows = []; for (const r of out) { const k = r.role + '|' + r.prefab; const last = rows[rows.length - 1];
  if (last && last.k === k) last.items.push(r); else rows.push({ k, items: [r] }); }
console.log('| PO ids | CS1 piece | role | proposed CS2 | mod | x, z (CS1 = CS2) | yaw | y bottom CS1 → x2 | size m |\n|---|---|---|---|---|---|---:|---|---|');
for (const { items } of rows) { const r = items[0], n = items.length;
  const xs = items.map(i => i.x), zs = items.map(i => i.z), ys = items.map(i => i.yBottom);
  const pos = n === 1 ? `${r.x}, ${r.z}` : `${Math.min(...xs).toFixed(0)}…${Math.max(...xs).toFixed(0)}, ${Math.min(...zs).toFixed(0)}…${Math.max(...zs).toFixed(0)}`;
  const y = n === 1 ? `${r.yBottom} → ${r.y2}` : `${Math.min(...ys).toFixed(0)}…${Math.max(...ys).toFixed(0)} → ×2`;
  console.log(`| ${items.map(i => i.id).join(', ')} | ${r.basePrefab}${n > 1 ? ' ×' + n : ''} | ${r.role} | ${r.prefab} | ${r.mod} | ${pos} | ${r.yaw} | ${y} | ${r.size.join('×')} |`); }
