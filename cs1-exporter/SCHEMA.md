# DakotaExport JSON schema (`dakota-export/cs1/v5`)

v5 (exporter 0.4.1, after the coverage audit) adds: `skippedBuildings`, `terrainHeights`, `networkSkins` (+ `networkSkinsReason`), `serializableData`, `mapSettings.themeMixer`, `nodes[].building`, `netPrefabs[].variants` / `.terrain` / `.lanes[].props`, the `sand` and `shore` resource layers, failed Procedural Objects and full edited PO meshes (`-po.json` schema `po/v3`, `-po-meshes.bin`), tree y/flags/scale/ids (`-trees.json` schema `trees/v2`), three height PNGs and the `-sdata/` folder. Props are now always points. Every file is written to `<file>.tmp` and renamed only when complete. Every v4 field is unchanged.

v4 adds Procedural Objects geometry and modules: `<base>-po.json` becomes schema `dakota-export/cs1/po/v2` (every po/v1 field unchanged, plus `geometry`, `modules`, `effects` per object), two new siblings `<base>-po-heights.png` and `<base>-po-heights.json`, and `geometry` / `effectsModule` in the main `proceduralObjects` summary. Every v3 field is unchanged.

v3 adds `waterSurface`, `terrainSurfaces`, `proceduralObjects` (+ `proceduralObjectsReason`), `sectionErrors`, three sibling files and new `stats` keys. Every v1 and v2 field is unchanged.

v2 adds `mapSettings`, `naturalResources`, the sibling `<base>-trees.json` and new `stats` keys. Every v1 field is unchanged.

One file per export: `%LOCALAPPDATA%\DakotaExport\<mapname>-<yyyyMMdd-HHmmss>.json`, UTF-8, no BOM.
Siblings with the same base name:

| File | Content |
|---|---|
| `<base>-trees.json` | every tree as a point (v2); ids, y, flags, scale since v5 (`trees/v2`) |
| `<base>-water.png` | water surface, 1081², 16-bit grey (v3) |
| `<base>-surfaces.png` | terrain surface classes, 1080², 8-bit grey (v3) |
| `<base>-po.json` | Procedural Objects (v3; geometry, modules and effects since v4), only when the mod is active |
| `<base>-po-heights.png` | max PO mesh height per heightmap cell, 1081², 16-bit grey (v4) |
| `<base>-po-heights.json` | the same as sparse `[index, maxY, minY, objectId]` (v4) |
| `<base>-po-meshes.bin` | full-resolution edited PO meshes and failed-container vertices (v5) |
| `<base>-heights-raw.png`, `-heights-raw2.png`, `-heights-block.png` | `RawHeights`, `RawHeights2`, `BlockHeights`, 1081², 16-bit (v5) |
| `<base>-sdata/` | every mod-data blob saved in the map, `NN-<key>.bin` + `index.json` (v5) |
| `<base>.svg` | quick look |
| `<base>.log` | counts, file sizes, time per section, errors, the `export complete` line |

## File naming (v5)

`<base>` = `<mapname>-<yyyyMMdd-HHmmss>`. The map name is the first non-empty of `SimulationMetaData.m_MapName`, `m_CityName`, `m_mapEditorMapPackage` (all null in the map editor), the text in `%LOCALAPPDATA%\DakotaExport\mapname.txt` (created with `dakota-falls` for this port), `workshop-<id>` from `m_WorkshopPublishedFileId`, else `map`. Every file is written as `<file>.tmp` and renamed when complete, so a file without `.tmp` is never truncated.

## Fault tolerance (v3)

Every section is written to a buffer first and spliced into the main object only when it completes. If a section throws, its top-level keys are written as `null`, `sectionErrors.<section>` holds `ExceptionType: message`, the full exception goes to the log, and the export continues with the next section, so the main JSON is always valid. Sections in order: metadata, mapSettings, networks (`nodes`, `segments`, `netPrefabs`), waterSources, buildings, trees, props, naturalResources, treePoints (sibling only), waterSurface, terrainSurfaces, proceduralObjects; the SVG is written last, outside the JSON.

## Conventions

- Units are metres, CS1 world space: **x east, y up, z north**, origin at the map centre.
- The full map is 17 280 m (81 tiles); every coordinate is expected inside x, z ∈ [-8640, 8640].
- The CS1 heightmap is 1081² pixels; raw 16-bit value / 64 = metres (0–1024 m).
- A `Vector3` is written as `[x, y, z]`.
- IDs are the CS1 buffer indices (`NetManager.m_nodes`, `m_segments`, `BuildingManager.m_buildings`,
  `PropManager.m_props`). They are stable only within one export.
- Floats are written round-trip (`"R"`); NaN/Infinity become `null`.
- Large arrays put one element per line.

## Top level

| Key | Type | Meaning |
|---|---|---|
| `schema` | string | `dakota-export/cs1/v3` |
| `metadata` | object | see below |
| `mapSettings` | object | map-level settings (v2) |
| `nodes` | array | network nodes |
| `segments` | array | network segments |
| `netPrefabs` | object | prefab name → network prefab info, for every prefab used by a node or segment |
| `waterSources` | array | water sources from the water simulation |
| `buildings` | array | top-level (plopped) buildings and all outside connections |
| `trees` | object | count rasters, no individual trees |
| `props` | object | points if fewer than 50 000 props, otherwise count rasters |
| `naturalResources` | object | 512² resource layers (v2) |
| `waterSurface` | object | measured water surface on the 1081 grid (v3) |
| `terrainSurfaces` | object | surface classes on the 1080 grid (v3) |
| `proceduralObjects` | object or null | summary of `<base>-po.json` (v3) |
| `proceduralObjectsReason` | string | only when `proceduralObjects` is null because the mod is absent or inactive (v3) |
| `sectionErrors` | object | section → error; empty when every section succeeded (v3) |
| `skippedBuildings` | array | buildings left out of `buildings` (sub-buildings, Untouchable/pillars), flagged (v5) |
| `terrainHeights` | object | the three height PNGs and their ranges (v5) |
| `networkSkins` | object or null | Network Skins per segment/node (v5); `networkSkinsReason` when null |
| `serializableData` | object | index of the `-sdata/` dump (v5) |
| `stats` | object | name → count summary (also in the log) |

## `metadata`

| Key | Source |
|---|---|
| `mapName` | `SimulationMetaData.m_MapName`, else `m_CityName`, else `m_mapEditorMapPackage` (v2), else `"map"` |
| `metaMapName`, `metaCityName`, `environment` | raw `SimulationMetaData` fields |
| `loadMode` | `LoadMap`, `NewMap`, `LoadGame` or `NewGame` |
| `exporterVersion` | mod version |
| `gameVersion` | `BuildConfig.applicationVersionFull` (fallback `Application.version`) |
| `exportedAtUtc` | ISO 8601 |
| `coordinates` | `units`, `axes`, `origin`, `halfExtent` 8640, `extent` 17280, `heightmapPixels` 1081, `heightmapRawToMetres` 1/64, `seaLevel` (`WaterSimulation.m_currentSeaLevel`) |

The extent is an **assumption** (the vanilla 81-tile map size), not read from the game.

## `mapSettings` (v2)

Anything not readable is `null`; nothing is guessed.

| Key | Source |
|---|---|
| `environment` | `SimulationMetaData.m_environment` |
| `startingDateTime` | `m_startingDateTime`, `yyyy-MM-ddTHH:mm:ss` |
| `invertTraffic`, `disableAchievements` | `MetaBool` enum names (`Undefined`, `False`, `True`) |
| `mapEditorMapPackage` | `m_mapEditorMapPackage` |
| `seaLevel` | `WaterSimulation.m_currentSeaLevel` (same as `metadata.coordinates.seaLevel`) |
| `gameAreas` | see below, or `null` |
| `mapTheme` | see below, or `null` when the map has no `m_MapThemeMetaData` |

`gameAreas` (from `GameAreaManager.instance`):

| Key | Source |
|---|---|
| `gridSize` | `sqrt(m_areaGrid.Length)`; 5 in vanilla, larger if a tile-count mod resized the grid |
| `vanillaGridSize`, `cellSize` | `AREAGRID_RESOLUTION` (5), `AREAGRID_CELL_SIZE` (1920 m) |
| `startTile`, `startTileX`, `startTileZ` | `GetStartTile(out x, out z)`; `startTile = z*5 + x` (the private `m_startTile`) |
| `areaCount`, `maxAreaCount` | `m_areaCount`, `m_maxAreaCount` |
| `unlocked` | tile indices with `m_areaGrid[i] != 0` (same test as `IsUnlocked`) |
| `areaGridRaw` | `m_areaGrid` as written by the game |

Tile index = `z*gridSize + x`; the grid is centred on the map, tile (0,0) is south-west.

`themeMixer` (v5, Theme Mixer 2.5, Workshop 3031623332): read from the private static `ThemeMixer.Themes.ThemeManager._instance` (the public `Instance` getter would create one) → `CurrentMix` (`ThemeMix.ID`, `.Name`, `.Lut`, `.Terrain`) and the private `MixID` (the id saved in the map under key `THEME-MIXER-2.5`). Shape: `{ loaded, assembly, mixIdSaved, mixId, mixName, lut, terrain }`, where `terrain` is a generic dump of `ThemeMix.Terrain` (per texture: `ThemeID`, tiling `Float`, colour offset `Vector3`, detail `Bool`). When the mod is absent: `{ loaded: false, reason }`. For Dakota Falls the mix is "Dakota Falls" (Workshop 2972881605, `ThemeMix.xml`): it sets how each surface class looks, not where.

`mapTheme` (from `SimulationMetaData.m_MapThemeMetaData`): `name`, `mapThemeRef`, `environment`,
`longitude`, `latitude`, `sunSize`, `sunAnisotropy`, `rayleigh` (the game's field is `rayleight`),
`mie`, `exposure`, `waterClean`, `waterDirty`, `waterUnder` (RGBA floats `[r,g,b,a]`),
`minTemperatureDay`, `maxTemperatureDay`, `minTemperatureNight`, `maxTemperatureNight`.

## `naturalResources` (v2)

Copied from `NaturalResourceManager.instance.m_naturalResources` (`ResourceCell[]`) while holding
`Monitor` on that array, acquired with the game's own spin
`while (!Monitor.TryEnter(m_naturalResources, SimulationManager.SYNCHRONIZE_TIMEOUT)) {}`.

```json
"naturalResources": {
  "grid": { "size": 512, "cellSize": 33.75, "originX": -8640, "originZ": -8640,
            "rowAxis": "+z", "note": "index = row*512 + col, col along +x, row 0 = south", "cellCount": 262144 },
  "layers": { "ore": [...], "oil": [...], "forest": [...], "fertility": [...], "pollution": [...], "water": [...],
              "sand": [...], "shore": [...] }   // sand, shore since v5
}
```

- `size` and `cellSize` come from `NaturalResourceManager.RESOURCEGRID_RESOLUTION` and `RESOURCEGRID_CELL_SIZE` (verified: 512 and 33.75).
- The game indexes cells as `z*512 + x` with `x = floor(pos.x/33.75 + 256)`, so row 0 is the south edge, same as the tree rasters.
- Each layer is a dense row-major array of 262 144 ints 0–255 (the `byte` fields `m_ore`, `m_oil`, `m_forest`, `m_fertility`, `m_pollution`, `m_water`, and since v5 `m_sand`, `m_shore`), one grid row per line. An all-zero layer is `null`.
- `stats` gets `naturalResources.<layer>.sum` per layer.

## `<base>-trees.json` (v2, schema `dakota-export/cs1/trees/v1`)

Every tree in `TreeManager.m_trees.m_buffer` with `Created` and not `Deleted` (burning, fire-damaged and fixed-height trees included).

```json
{ "schema": "dakota-export/cs1/trees/v1", "count": 740967,
  "prefabs": ["<TreeInfo.name>", ...],
  "points": [x, z, prefabIdx, x, z, prefabIdx, ...] }
```

- `x`, `z` are CS1 world metres rounded to 0.1 (written as `-1234.5`, or `12` when the tenth is 0).
- `prefabIdx` indexes `prefabs`; the table is built in buffer order.
- 1 000 points per line. `stats.treePoints` and `stats.treePointPrefabs` in the main JSON repeat the totals.

Since v5 the file is schema `dakota-export/cs1/trees/v2`: the v1 keys are unchanged and four parallel arrays follow, one value per tree in the same order as `points`:

| Key | Content |
|---|---|
| `ids` | `TreeManager.m_trees` buffer index (Tree Control's expanded buffer); joins to the `TreeScaling`/`TreeSnapping` blobs in `-sdata/` |
| `y` | `TreeInstance.Position.y` (= `m_posY / 64`), metres, 0.1. For `FixedHeight` trees (snapped onto rocks or bridges) this is the real height; otherwise the terrain height |
| `flags` | `TreeInstance.m_flags` as an integer (1 Created, 4 Hidden, 0x10 Single, 0x20 FixedHeight, 0x40 FireDamage, 0x80 Burning, 0xF00 GrowState) |
| `scale` | Tree Control scale = `TreeControl.Patches.TreeInstancePatches.s_scalingData[id] × 0.02` (byte 50 = 1.0), written with two decimals; `null` when Tree Control is not loaded (`scaleNote` says why) |

`scaleSource`, `scaleNote`, `yNote`, `flagsNote` and `arrays` describe the fields. `stats` adds `treePoints.fixedHeight` and `treePoints.scaled`.

## `waterSurface` (v3)

Read directly from the simulation, not through the `Sample…WithWater` helpers (they interpolate and time-lerp):

- depth: `WaterSimulation.BeginRead()` returns the current front buffer of `WaterSimulation.Cell` (1081², index `z*1081+x`); `Cell.m_height` is the water **depth** in raw units (1/64 m). The exporter copies every depth and calls `EndRead()` straight away (the same read-lock pair `TerrainManager` uses).
- terrain: `TerrainManager.BlockHeights` (raw, 1/64 m), the height the water simulation runs on.
- surface = `(BlockHeights + m_height) / 64`. For depths under 1 m the game's renderer uses its private `m_rawHeights2` instead of `BlockHeights`; the difference is centimetres.

```json
"waterSurface": {
  "source": "...",
  "grid": { "size": 1081, "cellSize": 16, "originX": -8640, "originZ": -8640, "rowAxis": "+z", "note": "..." },
  "threshold": 0.05, "thresholdNote": "a cell is wet when Cell.m_height >= 4 raw (0.0625 m)",
  "surfaceDefinition": "...", "encoding": "...", "png": "<base>-water.png",
  "stats": { "wetCells": 0, "bodies": 0, "minSurface": 0, "medianSurface": 0, "maxSurface": 0,
             "largestBodyCells": 0, "largestBodyMedianSurface": 0, "seaLevel": 40 },
  "cells": [index, surfaceMetres, depthMetres, ...]
}
```

- Grid: the heightmap **vertex** grid. `index = row*1081 + col`, sample point `x = -8640 + col*16`, `z = -8640 + row*16`, row 0 = south: exactly one CS1 heightmap PNG pixel per cell. This differs slightly from `trees.total`, whose 1081 bins are 17280/1081 = 15.985 m wide.
- `cells`: wet cells only (depth > 0.05 m), values rounded to 0.01 m.
- `bodies`: 4-connected components of wet cells; `largestBody*` describes the one with the most cells (usually the sea or the main river system).
- `stats` (main JSON): `waterSurface.wetCells`, `waterSurface.bodies`, `waterSurface.largestBodyCells`.

`<base>-water.png`: 1081 × 1081, 16-bit greyscale, **row 0 = north** (the CS1 heightmap orientation), value = surface × 64 (the raw heightmap scale) on wet cells, 0 on dry cells. It overlays a CS1 heightmap export 1:1; subtract the heightmap to get depth.

## `terrainSurfaces` (v3)

What CS1 actually persists for ground painting, checked against the decompiled game and mods:

| Candidate | Persisted? | Used |
|---|---|---|
| `TerrainManager` serialized data | only `m_rawHeights`, block-height offsets and the dirt buffer; **no surface data** | – |
| `TerrainManager.m_detailSurface`, `m_rawSurface`, patch `m_surfaceMapA/B` | no (runtime only); rebuilt on load by `TerrainModify` from the surfaces of buildings, networks and props | yes, as `objects` (runtime but deterministic) |
| `NaturalResourceManager` `m_sand` (map editor sand brush; also Extra Landscaping Tools) | yes, serialized with the other resource bytes | yes, as `sand` |
| Surface Painter mod (Workshop 689937287): `SurfacePainter.SurfaceManager.m_surfaces`, saved as serializable data `"SurfaceManager"` | yes | yes, as `surfacePainter`, by reflection when the mod is active |

Values use `TerrainModify.Surface`, so one legend covers every source:

| Value | Name | PNG grey |
|---|---|---|
| 0 | None | 0 |
| 1 | Clip (terrain hidden under an object) | 40 |
| 2 | PavementA | 70 |
| 4 | PavementB | 100 |
| 8 | Ruined | 130 |
| 16 | Gravel | 160 |
| 32 | Field | 190 |
| 64 | RuinedWeak | 215 |
| 128 | Sand (exporter's own code) | 245 |

```json
"terrainSurfaces": {
  "grid": { "size": 1080, "cellSize": 16, "originX": -8640, "originZ": -8640, "rowAxis": "+z", "note": "..." },
  "legend": { "0": "None", "1": "Clip", "...": "...", "128": "Sand" },
  "priority": "...", "encoding": "cells: sparse flat [index, value, ...] for value != 0",
  "png": "<base>-surfaces.png", "pngGrey": { "None": 0, "...": 0 },
  "sources": { "surfacePainter": null, "objects": {}, "sand": {} },
  "cellsBySource": { "objects": 0 }, "counts": { "<value>": 0 },
  "notes": ["what was missing and why"],
  "cells": [index, value, ...],
  "surfacePainterNative": {}
}
```

- Grid: `TerrainManager.RawSurface` (1080², index `z*1080+x`), sample point `x = -8640 + col*16`, `z = -8640 + row*16`, row 0 = south.
- Per cell the first non-zero source wins: Surface Painter (majority of the 4×4 4-m cells starting at the sample point; at least 8 of the 16 must be painted) → objects (the strongest of the six `SurfaceCell` channels, if ≥ 128) → sand (`m_sand` ≥ 128 at the nearest 33.75 m resource cell; value 128).
- `sources.surfacePainter`: `assembly`, `eightyOneTiles`, `nativeWidth` (4320 with 81 Tiles, else 2400), `nativeOffsetCells` (0 or 960), `nativeCounts` (value → cells; +256 = painted with "override existing"). It is `null` when the mod is not loaded, not active or holds no data; `notes` says which.
- `surfacePainterNative` (only when Surface Painter data was read): the full 4 m data on the 4320 frame (`index = row*4320 + col`, cell covers `[-8640 + col*4, +4)`), as row runs `[startIndex, length, value, ...]`, zeros omitted.
- `stats` (main JSON): `terrainSurfaces.cells` and `terrainSurfaces.<source>`.

`<base>-surfaces.png`: 1080 × 1080, 8-bit greyscale, **row 0 = north**, grey per the table above.

## `proceduralObjects` (v3)

Read by reflection from the Procedural Objects mod (assembly `ProceduralObjects`, Workshop 3071492847); there is no compile-time reference. If the type `ProceduralObjects.ProceduralObjectsLogic` is not loaded, or it has no instance, the main JSON has `"proceduralObjects": null` and `"proceduralObjectsReason": "<why>"`, and no `-po.json` is written.

Main JSON summary:

```json
"proceduralObjects": { "file": "<base>-po.json", "schema": "dakota-export/cs1/po/v2",
  "count": 0, "edited": 0, "failed": 0, "firstError": null, "groups": 0,
  "byType": { "PROP": 0, "BUILDING": 0 }, "byPrefab": { "PROP:<basePrefab>": 0 } }
```

`stats` gets `proceduralObjects`, `proceduralObjects.edited`, `proceduralObjects.failed` and `proceduralObjects.prefab.<TYPE>:<basePrefab>`.

`<base>-po.json` (schema `dakota-export/cs1/po/v2` since v4): `assembly`, `coordinates`, `meshStatusLegend`, `geometryNote` (v4), `effectsModule` (v4: `loaded`, `assembly`, `reason`), `objects[]`, `groups[]` (`index`, `rootId`, `memberCount`), `layers[]` (`id`, `name`), `count`, `failed`, `firstError`. One object per line:

| Key | Source field on `ProceduralObjects.Classes.ProceduralObject` |
|---|---|
| `id` | `id` |
| `baseInfoType` | `baseInfoType` (`PROP` or `BUILDING`) |
| `basePrefab` | `basePrefabName` |
| `position` | `m_position` (world) |
| `rotation` | `m_rotation` as `[x,y,z,w]` |
| `yawDegrees`, `eulerDegrees` | `m_rotation.eulerAngles` (`.y`, and all three) |
| `scale` | `m_scale` (PO applies it to the vertices on load; often 0 or 1) |
| `meshStatus`, `meshEdited` | `meshStatus` (1 = base mesh, 2 = edited/custom), `meshStatus == 2` |
| `vertexCount`, `localBounds` | `m_mesh.vertexCount`, `m_mesh.bounds` (`center`, `size`, object space); no vertices are exported |
| `customTexture` | `customTexture.name`, or null |
| `renderDistance`, `renderDistLocked` | same names |
| `visibility` | `m_visibility` enum name |
| `layer` | `layer` → `{ id: m_id, name: m_name, hidden: m_isHidden }`, or null |
| `group`, `isRootOfGroup` | index into `ProceduralObjectsLogic.groups`; `isRootOfGroup` |
| `color` | `m_color` RGBA |
| `tilingFactor`, `ploppableAsphalt`, `hasText`, `moduleCount`, `disableCastShadows`, `flipFaces` | `tilingFactor`, `isPloppableAsphalt`, `m_textParameters != null`, `m_modules.Count`, same names |

A missing field is written as null (or -1 for integers); an object that throws is skipped and counted in `failed`.


### PO geometry, modules and effects (v4)

Exact members used (verified with ilspycmd on the installed `ProceduralObjects.dll`, Workshop 3071492847, and `EffectsPOModule.dll`, Workshop 2632235148):

| Purpose | Member |
|---|---|
| object list | `ProceduralObjects.ProceduralObjectsLogic.instance` (static field) → `proceduralObjects` (`List<ProceduralObject>`), `groups` |
| transform | `ProceduralObject.m_position` (`Vector3`), `m_rotation` (`Quaternion`). PO draws with `Graphics.DrawMesh(m_mesh, m_position, m_rotation, …)`, so there is no extra scale: world = `m_position + m_rotation * v` |
| mesh | `ProceduralObject.m_mesh` (`UnityEngine.Mesh`): `.vertices` (object space), `.triangles` (all sub-meshes) |
| fallback | `ProceduralObject.vertices` (`ProceduralObjects.Classes.Vertex[]`, `.Position` object space) when the mesh is not readable; then there are no triangles |
| modules | `ProceduralObject.m_modules` (`List<POModule>`); `POModule.ModuleType` (`POModuleType.TypeID`, `.Name`), `.enabled`, `._get_data(false)` (builds a string dictionary, no side effects) |
| effects | `EffectsPOModule.EffectsModule` (subclass of `POModule`): `effect` (`ParticleEffect`, its `.name`), `offset`, `direction`, `Width`, `Height`, `magnitude`, `acceleration`, `emitRate`; the mod renders at `parent.m_position + parent.m_rotation * offset` |

Per object, three new keys:

```json
"geometry": {
  "source": "m_mesh",              // or "vertices" (fallback, no triangles)
  "fullVertexCount": 12345, "fullTriangleCount": 20000,
  "triangleStride": 11,            // every 11th triangle kept; 0 when there are no triangles
  "vertexStride": 0,               // used only for the vertex-only fallback
  "vertices": [x, y, z, ...],      // WORLD metres, rounded to 0.01, at most 2 000 vertices
  "vertexCount": 1998,
  "triangles": [i0, i1, i2, ...],  // indices into the dumped vertices; null for the fallback
  "readNote": null,                // why the fallback was used, if it was
  "rasterized": true,
  "skipReason": null               // "decal/texture-only quad (<= 2 triangles)" when not rasterized
},
"modules": [ { "class": "EffectsPOModule.EffectsModule", "typeId": "...", "name": "...", "enabled": true,
               "data": { "TypeID": "...", "enabled": "True", "effectName": "...", ... } } ],
"effects": [ { "prefab": "<ParticleEffect name>", "position": [x,y,z], "rotation": [x,y,z,w],
               "direction": [x,y,z], "localOffset": [x,y,z], "width": 4, "height": 3,
               "magnitude": 2, "acceleration": 1, "emitRate": 2, "enabled": true } ]
```

- Decimation: if the mesh has more than 2 000 vertices, the exporter keeps every *k*-th triangle (smallest *k* that keeps the referenced vertices ≤ 2 000) and remaps indices. The result is a sparse sample of the surface for shape and extent, not a watertight mesh; use `<base>-po-heights.*` for terrain work.
- `geometry` is `{ "error": "..." }` when the object's mesh could not be read; the object itself is still listed.
- `effects` is `null` plus `"effectsReason"` on every object when the Effects Module assembly is not loaded; otherwise an array (empty when the object has no effect module). `rotation` is the parent object's rotation; `direction` is the emission direction in world space.
- Since exporter 0.4.2: `modules[].data` comes from `POModule._get_data(true)` (exactly the dictionary PO saves; for Effects Module the keys are `TypeID`, `enabled`, `effectName`, `width`, `height`, `magnitude`, `acceleration`, `direction`, `offset`, `emitRate`). Each effect also carries `worldPosition` (= `position`, `m_position + m_rotation * offset`) and `worldDirection` (= `direction`, `m_rotation * direction`). If the Effects Module class is not loaded but a module's data has `effectName`, `effects` is rebuilt from that data (entries have `"source": "moduleData"`, plus `offsetParsed`/`directionParsed`, and `effectsReason` says so).
- `failedObjects[].effects` (0.4.2): the same effect entries rebuilt from the failed container's `modulesData` and its `position`/`rotation`. PO writes the vectors as `"(x, y, z)"` with the game's culture; the exporter parses current culture first, then invariant.
- `emitRate` (byte): 0 = every frame, 1 = every 2 frames, 2 = 0.1 s, 3 = 0.3 s, 4 = 0.6 s, n > 4 = n − 4 s.
- Counts per effect name: main `proceduralObjects.byEffect` and `stats["proceduralObjects.effect.<name>"]` (live and failed objects together).
- Main summary additions: `proceduralObjects.geometry` = `{ objectsWithGeometry, geometryFailed, firstGeometryError, skippedQuadObjects, rasterizedObjects, vertexOnlyObjects, rasterizedTriangles, dumpedVertices, heightsPng, heightsPngCompression, heightsJson, heightsError, coveredCells }` and `proceduralObjects.effectsModule` = `{ loaded, reason, effects }`.
- `stats` additions: `proceduralObjects.geometry`, `.geometryFailed`, `.skippedQuads`, `.effects`, `.heightCells`.

### Failed objects and full meshes (v5, `-po.json` schema `po/v3`)

- `poLoadFailures`: `ProceduralObjectsLogic.failedToLoadObjects` (uint; −1 if unreadable). The 2026-10-18 session had 86.
- `failedObjects[]`: every container in the static `ProceduralObjects.Classes.PopupStart.loading_failures` (groups with `missing_asset` and `containers`, a `Dictionary<ProceduralObjectContainer, Exception>`). Per object: `id`, `missingAsset`, `exception`, `objectType`, `basePrefab`, `position`, `rotation` `[x,y,z,w]`, `yawDegrees`, `scale`, `meshStatus`, `tilingFactor`, `customTextureName`, `hasCustomTexture`, `renderDistance`, `renderDistLocked`, `layerId`, `groupRootId`, `belongsToGroup`, `color`, `visibility`, `flipFaces`, `disableCastShadows`, `hasText`, `modulesData` (Effects Module settings etc.), `containerVertexCount`, `meshBin` (when the container holds full vertices: world = position + rotation × (vertex × scale if scale ≠ 0), as PO loads them; no triangles, since the base mesh is missing), `meshDelta` (newer format: `{ positions: [x,y,z,...] local, indices: [[...], ...] }`, moved vertices → indices into the base prefab mesh). `failedObjectsNote` says how many were found.
- `meshesFile`: `{ file: "<base>-po-meshes.bin", format, entries }`. Live objects with `meshStatus` 2 get `geometry.meshBin = { offset, vertexCount, triangleIndexCount, uvCount }` pointing at their full mesh (world xyz, all triangles, UV0).
- Main summary adds `poLoadFailures`, `failedObjectsWritten`, `failedObjectsNote`, `meshesFile`, `meshEntries`; `stats` adds `proceduralObjects.loadFailures`, `.failedObjectsWritten`, `.meshEntries`.

`<base>-po-meshes.bin`, little-endian: header `DKPOMSH1` (8 bytes) + int32 version 1; then per entry at its `offset`: int32 id, int32 kind (0 live edited object, 1 failed container), int32 vertexCount, float32[3 × vertexCount] world xyz, int32 triangleIndexCount, int32[triangleIndexCount], int32 uvCount, float32[2 × uvCount].

### `<base>-po-heights.png` / `.json` (v4)

Every PO triangle (objects with ≤ 2 triangles, i.e. decals and texture quads, are skipped) is rasterized on the waterSurface grid: 1081², index `row*1081 + col`, a cell is the 16 m square centred on `x = -8640 + col*16`, `z = -8640 + row*16`, row 0 = south. A triangle contributes its barycentric height at every sample point it covers, its three vertices, and points every 4 m along its edges (so thin strips and vertical faces such as cliffs and waterfall steps still mark their cells). Vertex-only objects contribute their vertices.

- PNG: 1081 × 1081, 16-bit grey, **row 0 = north**, value = round(maxY × 64) clamped to 1…65535 on covered cells, 0 elsewhere (same scale as the CS1 heightmap; heights above 1024 m clip at 65535).
- JSON (schema `dakota-export/cs1/po-heights/v1`): `grid`, `encoding`, `png`, `pngEncoding`, `objectsRasterized`, `vertexOnlyObjects`, `trianglesRasterized`, `skippedQuadObjects`, `coveredCells`, `cells: [index, maxY, minY, objectId, ...]` (world metres, 0.01 m; `objectId` = PO `id` that owns `maxY`), `cellsAbovePngRange` (cells clipped in the PNG; the JSON keeps the true height).

## `nodes[]`

| Key | Type | Source |
|---|---|---|
| `id` | int | node buffer index |
| `prefab` | string | `NetNode.Info.name` |
| `position` | Vector3 | `NetNode.m_position` (y is the node's absolute height) |
| `elevation` | int | `NetNode.m_elevation` (byte; height above ground for elevated/bridge, depth for tunnels) |
| `terrainOriginal` | float | `TerrainManager.SampleOriginalRawHeightSmooth(position)`, metres |
| `terrain` | float | `TerrainManager.SampleRawHeightSmooth(position)`, metres (current terrain) |
| `flagsRaw` | int | `NetNode.m_flags`, written as signed int32; read as **uint32** (bit 31 is a flag) |
| `flags` | string[] | the set flags among End, Middle, Bend, Junction, Untouchable, Outside, Double, OnGround, Underground, Transition, LevelCrossing, OneWayIn, OneWayOut, Water |

| `building` | int | `NetNode.m_building` (v5): the node's own pillar/pylon building id, 0 if none; see `skippedBuildings` |

Only nodes with `Created` and without `Deleted` are written.

## `segments[]`

| Key | Type | Source |
|---|---|---|
| `id` | int | segment buffer index |
| `prefab` | string | `NetSegment.Info.name` |
| `startNode`, `endNode` | int | node ids |
| `startDirection`, `endDirection` | Vector3 | `m_startDirection`, `m_endDirection` (unit vectors pointing *into* the segment from each end) |
| `invert` | bool | `NetSegment.Flags.Invert`: the prefab is laid end → start. With `Invert`, `NetAI.UpdateLanes` flips each lane's direction **and mirrors its lateral position** (`netPrefabs.lanes[].position` → `-position`), so the converter must negate lane offsets on inverted segments |
| `flagsRaw` | int | `NetSegment.m_flags` (signed int32; read as uint32) |
| `bezier` | Vector3[4] | cubic Bézier `[a, b, c, d]`: a = start node position, d = end node position, b and c from `NetSegment.CalculateMiddlePoints(a, startDir, d, endDir, smoothStart, smoothEnd, out b, out c)` where `smoothStart/End` = the node has the `Middle` flag (same rule as Move It and the game's own segment code) |
| `length` | float | `m_averageLength` |

## `netPrefabs{name}`

| Key | Source |
|---|---|
| `service`, `subService`, `level` | `NetInfo.m_class` (`ItemClass.Service` etc., as enum names) |
| `ai` | `NetInfo.m_netAI` type name (`RoadAI`, `TrainTrackAI`, `PowerLineAI`, …) |
| `halfWidth` | `m_halfWidth` |
| `segmentCount` | segments using the prefab in this map |
| `laneCount` | `m_lanes.Length` |
| `forwardVehicleLanes`, `backwardVehicleLanes` | `m_forwardVehicleLaneCount`, `m_backwardVehicleLaneCount` |
| `pedestrianLanes` | lanes with `LaneType.Pedestrian` |
| `lanes[]` | per lane: `type`, `vehicleType`, `direction` (enum names; this is `m_direction`, not `m_finalDirection`, which differs only with left-hand traffic), `position` (lateral offset), `width`, `verticalOffset`, `speedLimit`; since v5 `props[]`: `{ prop, tree, position, angle, segmentOffset, repeatDistance, probability }` from `m_laneProps.m_props` (street lights, trees, signs) |
| `variants` (v5) | every `NetInfo`-typed public field of the prefab's AI, name → prefab name or null: e.g. `m_elevatedInfo`, `m_bridgeInfo`, `m_slopeInfo`, `m_tunnelInfo` (RoadAI, TrainTrackAI, …). A variant prefab used in the map appears as its own `netPrefabs` entry |
| `terrain` (v5) | `flattenTerrain`, `lowerTerrain`, `clipTerrain`, `createPavement`, `createGravel`, `createRuining`, `clipSegmentEnds` |

## `waterSources[]`

Copied from `TerrainManager.instance.WaterSimulation.m_waterSources` (a `FastList<WaterSource>`) under its own lock; entries with `m_type == 0` are free slots and skipped.

| Key | Source |
|---|---|
| `id` | 1-based source id (buffer index + 1, as the game uses it) |
| `type`, `typeRaw` | `m_type`: 1 Natural, 2 Facility (pump/drain), 3 Cleaner |
| `inputPosition`, `outputPosition` | `m_inputPosition`, `m_outputPosition` (identical for natural sources) |
| `targetRaw`, `targetHeight` | `m_target` in raw heightmap units, and / 64 in metres (the simulation compares it to the raw height buffer) |
| `inputRate`, `outputRate`, `water`, `pollution`, `flow` | the corresponding `uint` fields, unscaled |

## `buildings[]`

Buildings with `Created` and not `Deleted`. Sub-buildings (`m_parentBuilding != 0`) and `Untouchable` buildings are skipped, **except** outside connections, which are always kept.

| Key | Source |
|---|---|
| `id`, `prefab` | buffer index, `Info.name` |
| `service`, `subService`, `level` | `BuildingInfo.m_class` |
| `ai` | `m_buildingAI` type name |
| `position`, `angle` | `m_position`, `m_angle` (radians) |
| `width`, `length` | footprint in 8 m cells |
| `flagsRaw`, `untouchable` | `m_flags` |
| `outsideConnection` | `Info.m_buildingAI is OutsideConnectionAI` |
| `incoming`, `outgoing`, `netNode` | only on outside connections: `Building.Flags.Incoming/Outgoing`, `m_netNode` |

The transport type of an outside connection is its `service`/`subService` (Road, PublicTransport/Train, /Ship, /Plane). `flagsRaw` is signed int32; read as uint32.

## `skippedBuildings[]` (v5)

The buildings left out of `buildings`, in their own array so `buildings` keeps its v1 meaning. Same keys as `buildings` (`id`, `prefab`, `service`, `subService`, `level`, `ai`, `position`, `angle`, `width`, `length`, `flagsRaw`, `untouchable`) plus:

| Key | Meaning |
|---|---|
| `parentBuilding` | `m_parentBuilding` (non-zero for sub-buildings of multi-part assets) |
| `ownerNode` | the node whose `m_building` is this building (CS1-generated pillar or pylon), or null |
| `reason` | `sub-building`, `network node building (pillar/pylon)` or `untouchable` |

Hand-placed pillars are top-level buildings and stay in `buildings`; CS1-generated ones have an `ownerNode`. `stats.buildingsSkippedOwnedByNode` counts the latter.

## `terrainHeights` (v5)

```json
"terrainHeights": { "pngEncoding": "...", "grids": {
  "raw":   { "source": "TerrainManager.RawHeights ...",  "file": "<base>-heights-raw.png",   "compression": "deflate", "minRaw": 0, "maxRaw": 0, "minMetres": 0, "maxMetres": 0 },
  "raw2":  { "source": "TerrainManager.RawHeights2 ...", "file": "<base>-heights-raw2.png",  "...": "...", "cellsDifferentFromRaw": 0 },
  "block": { "source": "TerrainManager.BlockHeights ...","file": "<base>-heights-block.png", "...": "...", "cellsDifferentFromRaw": 0 } } }
```

All three are 1081 × 1081, 16-bit grey, row 0 = north, value = raw height (metres × 64), the same frame as `-water.png` and the CS1 heightmap export. `raw` is the saved heightmap (should equal the editor's PNG export), `raw2` includes building and network terraforming, `block` is what the water simulation runs on (dams included). A grid that cannot be read has `file: null` and `error`.

## `networkSkins` (v5)

Network Skins Continued (Workshop 3702700108) by reflection: static `NetworkSkins.Skins.NetworkSkinManager.SegmentSkins` and `NodeSkins` (`NetworkSkin[]` indexed by segment/node id). Each `NetworkSkin` has `Prefab` and a private `_modifiers` list; every modifier's public fields are dumped generically.

```json
"networkSkins": { "assembly": "...", "encoding": "...",
  "skins": [ { "index": 0, "prefab": "<NetInfo>", "modifiers": [
      { "type": "StreetLightModifier", "fields": { "StreetLight": null, "RepeatDistance": 40 } },
      { "type": "TreeModifier", "fields": { "Position": "Left", "Tree": { "unityObject": "<TreeInfo>", "type": "TreeInfo" }, "RepeatDistance": 20 } },
      { "type": "PillarModifier", "fields": { "Type": "Bridge", "Pillar": null } },
      { "type": "ColorModifier", "fields": { "Color": [r, g, b, a] } } ],
    "terrain": { "createPavement": false, "createGravel": false, "createRuining": false } } ],
  "segments": [segmentId, skinIndex, ...],
  "nodes": [nodeId, skinIndex, ...] }
```

Modifier types: `StreetLightModifier` (`StreetLight`, `RepeatDistance`), `TreeModifier` (`Position`, `Tree`, `RepeatDistance`), `PillarModifier` (`Type`, `Pillar`), `CatenaryModifier` (`Catenary`), `ColorModifier` (`Color`, Color32 0–255), `TerrainSurfaceModifier` (`GroundType`), `RoadDecorationModifier` (`nodeMarkingsHidden`, `arrowsHidden`, `signsHidden`, `decorationHidden`, `transportStopsHidden`, `trafficLightsHidden`, `levelCrossingsHidden`), `CustomDataCollectionModifier` (`Data`). A null prefab field means the item was removed. Unity objects are written as `{ unityObject: name, type }`. `null` + `networkSkinsReason` when the mod is absent or inactive. `stats`: `networkSkins.skins`, `.segments`, `.nodes`.

## `serializableData` and `<base>-sdata/` (v5)

Every entry of `SimulationManager.m_serializableDataStorage` (the mod data saved in the map: PO, Tree Control, Network Skins, Node Controller, IMT, TM:PE, Adaptive Networks, Theme Mixer, …), copied under `Monitor` on the dictionary (the game's own lock), written byte for byte to `<base>-sdata/NN-<key>.bin` (NN = position in key order, key sanitised to `[A-Za-z0-9._-]`), plus `<base>-sdata/index.json` (schema `dakota-export/cs1/sdata/v1`). Main JSON:

```json
"serializableData": { "dir": "<base>-sdata", "index": "<base>-sdata/index.json",
  "entries": [ { "key": "TreeScaling", "file": "17-TreeScaling.bin", "length": 4194405, "sha256": "...", "error": null, "textPreview": null } ],
  "count": 22, "totalBytes": 0, "failed": 0 }
```

`textPreview` shows short printable blobs (e.g. the Theme Mixer mix id). These are insurance: the same bytes are in the map `.crp`.

## `trees`

Individual trees are not written. Counts are binned on the map square.

```json
"trees": {
  "count": 740000, "outOfRange": 0,
  "raster": { "size": 270, "cellSize": 64, "origin": "...", "encoding": "..." },
  "total":  { "size": 1081, "cellSize": 15.985, "cells": [i0, n0, i1, n1, ...] },
  "byPrefab": { "<TreeInfo.name>": { "count": 1234, "cells": [i0, n0, ...] } }
}
```

- `cells` is a sparse flat array of `(index, count)` pairs, sorted by index; empty cells are omitted.
- `index = row * size + col`, `col = floor((x + 8640) / cellSize)`, `row = floor((z + 8640) / cellSize)`; row 0 is the **south** edge.
- `byPrefab` uses the `raster.size` grid (270, 64 m); `total` uses the 1081 heightmap grid.
- Trees outside the square are counted in `outOfRange` only.

## `props`

`encoding` is `"points"` when fewer than 50 000 props exist, else `"raster"`. Since v5 props are always written as points (CS1 holds at most 65 536), so `encoding` is always `"points"`.

- points: `points[]` with `id`, `prefab`, `position`, `angle` (radians), `fixedHeight`.
- raster: `raster` header and `byPrefab` exactly as for trees.

## SVG

`<base>.svg`: viewBox `-8640 -8640 17280 17280`, SVG y = −z (north up). One `<g data-service="Service/SubService">` per service, each segment a cubic path from the same Bézier points, stroke = prefab width (minimum 4 m). Blue dots are water sources, red dots outside-connection buildings, red squares nodes with the `Outside` flag. A legend is in the top-left corner.
