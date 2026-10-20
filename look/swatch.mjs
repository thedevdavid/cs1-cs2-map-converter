// Writes the surface swatch-test body for POST /import/surfaces (MCP cs2_import_surfaces).
// Usage: node look/swatch.mjs <x> <z> [out.json]   (x, z = south-west corner on flat ground, CS2 metres)
import fs from 'node:fs';

const [x0, z0] = process.argv.slice(2, 4).map(Number);
if (!Number.isFinite(x0) || !Number.isFinite(z0)) {
  console.error('usage: node look/swatch.mjs <x> <z> [out.json]');
  process.exit(1);
}
const prefabs = [
  'Grass Surface 01', 'Grass Surface 02', 'Sand Surface 01', 'Sand Surface 02', 'Agriculture Surface 01',
  'Landfill Surface 01', 'Pavement Surface 01', 'Pavement Surface 02', 'Concrete Surface 01', 'Concrete Surface 02',
];
const size = 60, gap = 10, perRow = 5;
const surfaces = prefabs.map((prefab, i) => {
  const x = x0 + (i % perRow) * (size + gap), z = z0 + Math.floor(i / perRow) * (size + gap);
  return { id: `swatch-${i + 1}`, prefab, polygon: [[x, z], [x + size, z], [x + size, z + size], [x, z + size]] };
});
const body = { name: 'look-swatch', options: { dryRun: false, bypassValidation: true, report: 'compact' }, surfaces };
const text = JSON.stringify(body, null, 2) + '\n';
if (process.argv[4]) fs.writeFileSync(process.argv[4], text); else process.stdout.write(text);
