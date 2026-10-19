// Writes fixtures/v3/: a small hand-made dakota-export/cs1/v3 dump with every v2/v3 field the converter reads,
// plus the -trees.json and -po.json siblings. Field shapes follow the round-5 contract; adjust when the exporter's
// SCHEMA.md documents v3. node tools/make-v3-fixture.mjs
import { mkdirSync, writeFileSync } from "node:fs";

const dir = "fixtures/v3";
mkdirSync(dir, { recursive: true });

const node = (id, x, z, y = 100) => ({ id, prefab: "Gravel Road", position: [x, y, z], elevation: 0, terrainOriginal: 100, terrain: 100, flagsRaw: 0, flags: ["OnGround"] });
const seg = (id, a, b, pa, pb) => ({ id, prefab: "Gravel Road", startNode: a, endNode: b, startDirection: [1, 0, 0], endDirection: [-1, 0, 0], invert: false, flagsRaw: 0,
  bezier: [pa, pa.map((v, i) => v + (pb[i] - v) / 3), pa.map((v, i) => v + 2 * (pb[i] - v) / 3), pb], length: Math.hypot(pb[0] - pa[0], pb[2] - pa[2]) });

// Natural resources on a small 8×8 grid (the converter passes the grid through).
const g = 8, layer = f => Array.from({ length: g * g }, (_, i) => f(i % g, Math.floor(i / g)));

// Water surface on the heightmap VERTEX grid (1081, 16 m, x = -8640 + col·16): a river band at 97.6 m along row 540 (z = 0),
// a lake at 150 m near (2000, 2000) with no source, and a pond at source 1.
const W = 1081, cs = 16, idx = (x, z) => Math.round((z + 8640) / cs) * W + Math.round((x + 8640) / cs);
const cells = [];
for (let col = 300; col < 700; col++) for (const row of [539, 540, 541]) cells.push(row * W + col, 97.6, 2.5);
for (let dx = -3; dx <= 3; dx++) for (let dz = -3; dz <= 3; dz++) cells.push(idx(2000 + dx * cs, 2000 + dz * cs), 150, 1.2);
for (let dx = -2; dx <= 2; dx++) for (let dz = -2; dz <= 2; dz++) cells.push(idx(-1000 + dx * cs, 3000 + dz * cs), 203.4, 0.8); // pond at a source

// Terrain surfaces: 1080 cells of 16 m, sparse [index, value] flags: a 3×3 Field (32) block at the centre and a 16-cell Gravel (16) line.
const S = 1080, ts = [];
for (let r = 540; r < 543; r++) for (let c = 540; c < 543; c++) ts.push(r * S + c, 32);
for (let r = 100; r < 116; r++) ts.push(r * S + 100, 16);

const dump = {
  schema: "dakota-export/cs1/v3",
  metadata: { mapName: "fixture-v3", coordinates: { seaLevel: 97.5 } },
  mapSettings: { seaLevel: 97.5 },
  nodes: [node(1, -100, 0), node(2, 100, 0), node(3, 300, 50)],
  segments: [seg(10, 1, 2, [-100, 100, 0], [100, 100, 0]), seg(11, 2, 3, [100, 100, 0], [300, 100, 50])],
  netPrefabs: { "Gravel Road": { service: "Road", subService: "None", ai: "RoadAI", halfWidth: 5, segmentCount: 2, forwardVehicleLanes: 1, backwardVehicleLanes: 1 } },
  waterSources: [
    { id: 1, type: "Natural", typeRaw: 1, inputPosition: [-1000, 200, 3000], outputPosition: [-1000, 200, 3000], targetRaw: 13000, targetHeight: 200, inputRate: 4096, outputRate: 4096, water: 0, pollution: 0, flow: 1 },
    { id: 2, type: "Natural", typeRaw: 1, inputPosition: [0, 97, 10], outputPosition: [0, 97, 10], targetRaw: 6240, targetHeight: 97.5, inputRate: 16383, outputRate: 16383, water: 0, pollution: 0, flow: 1 },
  ],
  buildings: [
    { id: 5, prefab: "1978360460.Brown Surface Rock A Small_Data", ai: "DecorationBuildingAI", position: [50, 100, 50], angle: 0, outsideConnection: false },
    { id: 6, prefab: "Road Connection", service: "Road", subService: "None", ai: "OutsideConnectionAI", position: [8640, 100, 0], angle: 0, outsideConnection: true, incoming: true, outgoing: true },
  ],
  trees: {
    count: 4,
    raster: { size: 270, cellSize: 64 },
    total: { size: 1081, cellSize: 15.985199, cells: [541 * 1081 + 541, 4] },
    byPrefab: { "mp9-LiveOaknvl": { count: 4, cells: [135 * 270 + 135, 4] } },
  },
  props: { encoding: "points", points: [{ id: 7, prefab: "Park Streetlight 01", position: [10, 100, 10], angle: 0, fixedHeight: false }] },
  naturalResources: {
    grid: { size: g, cellSize: 2160, originX: -8640, originZ: -8640, rowAxis: "+z", cellCount: g * g },
    layers: { ore: layer((c, r) => (c === 2 ? 200 : 0)), oil: layer((c, r) => (r === 5 ? 120 : 0)), forest: layer(() => 50),
      fertility: layer((c, r) => (c + r) * 10), pollution: null, water: null },
  },
  waterSurface: { source: "fixture", grid: { size: W, cellSize: cs, originX: -8640, originZ: -8640, rowAxis: "+z", note: "vertex grid" }, threshold: 0.05,
    encoding: "cells: flat [index, surfaceMetres, depthMetres, ...]", png: "map-v3-water.png", stats: { wetCells: cells.length / 3 }, cells },
  terrainSurfaces: { grid: { size: S, cellSize: 16, originX: -8640, originZ: -8640, rowAxis: "+z" },
    legend: { "0": "None", "1": "Clip", "2": "PavementA", "4": "PavementB", "8": "Ruined", "16": "Gravel", "32": "Field", "64": "RuinedWeak", "128": "Sand" },
    encoding: "cells: sparse flat [index, value, ...] for value != 0", cells: ts, notes: [] },
  proceduralObjects: { file: "map-v3-po.json", schema: "dakota-export/cs1/po/v1", count: 3, byType: { PROP: 2, BUILDING: 1 } },
  sectionErrors: {},
  stats: {},
};
writeFileSync(`${dir}/map-v3.json`, JSON.stringify(dump));
writeFileSync(`${dir}/map-v3-trees.json`, JSON.stringify({ schema: "dakota-export/cs1/trees/v1", count: 5,
  prefabs: ["mp9-LiveOaknvl", "1953870276.Corn Stalk _Data"], points: [10, 20, 0, 30, 40, 0, 50, 60, 1, 9000, 0, 0, -20, -20, 0] }));
writeFileSync(`${dir}/map-v3-po.json`, JSON.stringify({ schema: "dakota-export/cs1/po/v1", count: 3, objects: [
  { id: 1, baseInfoType: "PROP", basePrefab: "1978360460.Brown Surface Rock A Large_Data", position: [120, 130, 80], rotation: [0, 0.7071068, 0, 0.7071068], yawDegrees: null, scale: 1 },
  { id: 2, baseInfoType: "PROP", basePrefab: "Cube", position: [-50, 110, 60], rotation: [0, 0, 0, 1], yawDegrees: 45, scale: 3 },
  { id: 3, baseInfoType: "BUILDING", basePrefab: "Castle Ruins 01", position: [10, 100, -30], rotation: [0, 0, 0, 1], yawDegrees: 0, scale: 0 },
], groups: [], layers: [] }));
console.log(`wrote ${dir}/map-v3.json, map-v3-trees.json, map-v3-po.json`);
