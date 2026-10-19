# DakotaExport coverage audit (v0.3.0, before the last export)

Date: 2026-10-19. Scope: `cs1-exporter/DakotaExport` (v0.3.0, schema `dakota-export/cs1/v3`) against what CS1 1.21.1-f9 and the map's mods store for Dakota Falls (Workshop 2972879554).

How this was checked:
- Decompiled CS1 `Assembly-CSharp.dll`, `ColossalManaged.dll` (`DataSerializer`) and 16 mod assemblies from the playset's Workshop folders with ilspycmd.
- Parsed the map file itself (`Dakota Falls.crp`, asset `Dakota Falls_Data`). The custom-data dictionary that `SimulationManager.Data.Serialize` writes is at offset 676 659: a count of **22**, then key/blob pairs. The keys are `WriteUniqueString`, 8-bit delta-coded. Every blob was extracted, and the gzip/XML ones were decoded.
- Read the last CS1 session log (`Cities_Data/output_log.txt`, 2026-10-18 13:43) and the v0.1.0 dump `%LOCALAPPDATA%\DakotaExport\map-20261018-132303.json` for real counts.

## Summary

- **One real data loss.** 86 of the map's 690 Procedural Objects fail to load in the current playset. PO drops them from its live list, so the exporter never sees them. The log shows 86 `NullReferenceException` in `ProceduralObject..ctor`. PO then logged "Saved 604 procedural objects" on the editor autosave (690 − 86 = 604). The likely cause is that the map references `1094334744.Cube_Data` / `1094334744.Square_Data`, the cube and square assets of the original PO item, which is not installed. The re-upload 3071492847 ships its own copies under a different package name (log line 11136: "Loading custom asset Cube_Data from 3071492847"), and PO has no name remap. Fix this before the export (see "Before the export", item 1).
- **Everything else important is either exported or still recoverable offline.** All 22 mod-data blobs are inside the map `.crp`, which stays on disk, and I extracted them all without CS1. The gzip/XML ones (Node Controller, IMT) decode directly.
- **Tree scale is used and not exported.** Tree Control scaling is set on about 70 600 tree slots (1.86× to 2.96×). The tree sibling file has no tree ids, so the offline scale blob cannot be joined to it. Small fix.
- **The code review found no correctness bug in the untestable paths.** The constants, grids, locks, APIs and PO member names the exporter uses all match the assemblies. The remaining findings are omissions and documentation gaps.

## 1. Map data stored by CS1, by manager

"Map use" is what Dakota Falls plausibly has, from the v0.1.0 dump, the key table below and the editor's capabilities.

| Manager / data | What CS1 stores for a map | Exported? | Map use | Notes |
|---|---|---|---|---|
| NetManager: nodes | position, flags, `m_elevation`, `m_building` (pillar), lane links | **partly**: position, elevation, flags, terrain under node | 5 582 nodes | `m_building` (the node's own pillar or pylon) is not exported. Custom node heights are just `m_position.y`, which is exported. NCR per-node data is in its own blob (§2). |
| NetManager: segments | nodes, directions, flags, `m_averageLength`, `m_nameSeed` | **fully** for geometry (Bézier, invert, flagsRaw, length) | 5 532 segments, 62 prefabs, 922 inverted | Segment names (`m_nameSeed` plus InstanceManager custom names) are not exported. That is low value: the editor gives no naming UI, and CS2 names roads itself. |
| NetManager: lanes | runtime, rebuilt from segments by `NetAI.UpdateLanes` | n/a | – | Lane geometry follows from the segment and its prefab. Per-prefab lanes are exported in `netPrefabs`. |
| NetInfo (prefab) | class, AI, width, lanes | **partly** | 62 | Not exported: elevated/bridge/slope/tunnel variant links (`RoadAI.m_elevatedInfo` …), lane props (street trees, lights), terrain flags (`m_flattenTerrain`, `m_lowerTerrain`, `m_clipTerrain`, `m_createPavement/Gravel/Ruining`). These help prefab mapping; optional. |
| BuildingManager | every building incl. sub-buildings and network-owned pillars | **partly**: top level plus outside connections; skips `m_parentBuilding != 0` and `Untouchable` | 4 687 exported (4 545 decoration rocks, 112 manually placed pillars, 4 dams, 3 ploppable bridges, ruins, 22 outside connections), 301 skipped | Node pillars are created `Untouchable \| FixedHeight` (`NetNode.cs:2346`), so the 301 skipped are mostly network pillars (CS2 makes its own) plus sub-buildings of multi-part assets. The landmarks (dams, Detroit and Hernando-de-Soto bridges) are top-level buildings and **are** exported. A sub-building is regenerated from its parent prefab, so skipping it loses nothing a stand-in needs; exporting skipped ones with `parentBuilding` would only help hand placement. |
| TreeManager | position (0.26 m quantised), `m_posY`, flags (FixedHeight, Burning, FireDamage, Hidden, GrowState), prefab index; Tree Control's expanded buffer (`MoreTrees`) | **partly**: x, z, prefab (sibling) plus rasters | 740 967 trees, 32 prefabs | Not exported: **y for FixedHeight trees** (trees snapped onto rocks or bridges, Tree Control `TreeSnapping`), **scale** (Tree Control, see §2), flags. Burning and fire damage don't matter for a map. |
| PropManager | position (Prop Control precision applied by its `Position` patch), y, angle, flags, prefab; Prop Control scale | **fully** for position, angle, fixedHeight | 3 743 props (44 prefabs, 128 decals, 14 fixed-height) | Prop Control's `PropScaling` holds 65 535 × 1.0 (at most one prop differs), so prop scale **is not used** by this map. Precision offsets (`PropPrecision`) reach the export through the patched getter, because `buffer[i].Position` runs on the array element in place. |
| TerrainManager | `m_rawHeights` (1081²), block-height offsets, dirt buffer | **no** raw heights in the dump; nodes sample `m_rawHeights` (`terrainOriginal`) and `m_rawHeights2` (`terrain`); water uses `BlockHeights` | yes | The heightmap comes from the editor's PNG export, a separate pipeline. `m_rawHeights2` (terrain with object terraforming) and `m_blockHeights` are not on disk anywhere, and exporting them costs 3 × 2.3 MB. Detail surface and patch surface maps are runtime only (`terrainSurfaces.objects` covers them). |
| WaterSimulation | sources, sea level, water buffer (depth, pollution, velocity) | **fully** for sources, sea level, depth; not pollution or velocity | 65 sources, sea level 92 m | Velocities are not needed: CS2 rebuilds flow from its own sources. |
| NaturalResourceManager | ore, oil, sand, forest, tree, fertility, pollution, water, shore, burned, destroyed (512²) | **partly**: six layers; sand only thresholded inside `terrainSurfaces` | yes | `m_sand` raw (0–255) and `m_shore` are not written as layers; trivial to add. |
| DistrictManager | districts, parks, industry areas | no | none | The map editor has no district, park or industry tools. |
| GameAreaManager | start tile, area grid (5 × 5), area count | **fully** | yes | 81 Tiles is not in the playset, so the vanilla 5 × 5 grid applies. |
| WeatherManager / theme | weather state; `SimulationMetaData.m_MapThemeMetaData` | **fully** for theme and environment | yes | Theme Mixer 2.5 overrides visuals (key `THEME-MIXER-2.5` = mix id `DakotaFalls_5dec87fb…`); the mix itself is the theme-mix asset (audit-theme session). |
| ZoneManager | zone blocks generated along roads | no | blocks only, nothing painted | Map editor cannot zone; CS2 regenerates. |
| TransportManager | lines, stops | no | none | Lines are not in maps. Ship and air paths are networks and are exported as segments. |
| PathManager, ImmaterialResource, Electricity, Water, Citizen, Vehicle, Economy, Disaster, Event | simulation state | no | none / runtime | Not map content. |
| InstanceManager | custom names | no | unlikely | See segment names. |
| SimulationMetaData | names, environment, start date, invertTraffic, theme | **fully** | yes | `m_MapName`/`m_CityName` are null in the editor (v0.1.0 wrote `"map"`). |
| Editor-only map metadata | `MapMetaData` asset `Dakota Falls` (3 742 B), preview `Dakota Falls_SteamPreview`, `Dakota Falls_Snapshot` | no | yes | Separate assets in the `.crp`, readable offline with `audit/tools/crp-table.mjs`. The description is on the Workshop page. Nothing for the exporter. |

## 2. Mod data in the map

The authoritative list is the 22 keys in the map's custom-data dictionary, in file order. I walked the key chain end to end, and each key's offset plus its blob length lands exactly on the next key. A mod listed in the playset whose key is absent stores nothing in this map.

| Key | Bytes | Format | Mod |
|---|---:|---|---|
| `ProceduralObjectsDataKey` + `ProceduralObjectsDataKey1` | 16 711 679 + 8 688 994 | BinaryFormatter `ProceduralObjectContainer[]`, split at the 16 MB array limit | Procedural Objects |
| `ProceduralObjectsLayerData` | 92 | BinaryFormatter | PO layers (log: "No layer found") |
| `TrafficManager_VersionInfo_v1.0`, `TrafficManager_v1.0` | 424, 11 808 | BinaryFormatter | TM:PE |
| `NodeMarkup`, `NodeMarkupUsedPrefabs` | 140 168, 114 | gzip XML; plain XML | Intersection Marking Tool |
| `NodeController` | 29 045 | gzip XML | Node Controller Renewal |
| `ACME` | 763 | binary | camera mod (not in playset; camera bookmarks) |
| `TreeAnarchy` | 19 430 | DataSerializer | legacy Tree Anarchy (read by Tree Control) |
| `PropSnapping`, `PropScaling`, `PropPrecision` | 29 325, 262 245, 22 567 | DataSerializer | legacy PTA keys, read by Prop Control |
| `THEME-MIXER-2`, `THEME-MIXER-2.5` | 68, 68 | BinaryFormatter / string | Theme Mixer |
| `NetworkSkins_ACTIVE_SELECTION`, `NetworkSkins_APPLIED_SKINS` | 138, 140 136 | DataSerializer | Network Skins |
| `AdaptiveRoads` | 1 164 576 | AN SimpleDataSerializer | Adaptive Networks |
| `TreeSnapping`, `TreeScaling`, `MoreTrees`, `TreeControlAnarchyFlags` | 2 112 698, 4 194 405, 4 419 514, 131 202 | DataSerializer | Tree Control |

Absent, so nothing to read: Surface Painter (`SurfaceManager`), Extra Landscaping Tools, Prop Painter (`PropPainter`), Network Anarchy, Move It, Railway Replacer, Hide It, ROTTERdam, Precision Engineering, Network Multitool, Effects Module (its data lives inside the PO containers).

| Mod | Where its data lives | Exporter reads it? | What is lost now | v0.4.0 by reflection |
|---|---|---|---|---|
| **Procedural Objects** (3071492847) | `ProceduralObjectsLogic.instance.proceduralObjects` (live), failed ones in static `PopupStart.loading_failures[].containers` (key = `ProceduralObjectContainer`), count in `logic.failedToLoadObjects` | **partly**: 604 live objects with transform, prefab, mesh status, bounds, texture name, layer, group, colour | **86 objects that failed to load** (not even counted). **Edited mesh vertices** (meshStatus 2) of every object: only `vertexCount` and local bounds are written. Text parameters and module data (Effects Module). | Failed containers: **small** (public fields `position`, `rotation`, `basePrefabName`, `objectType`, `scale`, `meshStatus`, `vertices` (`SerializableVector3[]`), `serializedMeshData`, `customTextureName`, `color`, `layerId`, `groupRootId`). Edited meshes as vertices + triangles + UV for meshStatus 2: **medium** (live: `m_mesh.vertices/triangles/uv`; failed: container vertices plus the base mesh's triangles, or `serializedMeshData`). |
| **Tree Control** (2960858317) | vanilla `TreeInstance.m_posY` + `FixedHeight` (snapping); `TreeControl.Patches.TreeInstancePatches.s_scalingData` (`byte[]`, 50 = 1.0, × 0.02); expanded `m_trees` | positions yes (expanded buffer iterated); **y, FixedHeight, scale no** | Scale on ≈ 70 600 slots (49 403 at 1.86, 14 860 at 2.50, 2 938 at 2.96, 2 340 at 2.24, 1 029 at 2.20, 119 at 0.74), heights of snapped trees | **small**: add `y`, `flags`, `scale` (or the buffer id) to the tree sibling |
| **Prop Control** (2956854293) | `PropControl.Patches.PropInstancePatches.PrecisionData` (dictionary, applied by the `Position` patch), `ScalingData` (`float[65536]`) | precision **yes** (implicitly); scale no | nothing material: every scale is 1.0 | trivial; optional |
| **Network Skins Continued** (3702700108) | static `NetworkSkins.Skins.NetworkSkinManager.SegmentSkins` / `NodeSkins` (`NetworkSkin[]` by segment/node id); each skin: `Prefab`, `Modifiers` (Tree, StreetLight, Pillar, Catenary, Color, TerrainSurface, RoadDecoration, CustomData; public readonly fields) | **no** | Per-segment removed or replaced street lights and trees, pillar types, colour, hidden markings, on USRP interstates and national roads (names seen in the blob) | **small** |
| **Node Controller Renewal** (2472062376) | key `NodeController` (gzip XML: 1 067 nodes, 2 162 segment ends; offset, shift, rotate, slope, twist, stretch, no-markings, collision) | **no** | nothing irrecoverable: decoded offline from the `.crp` | none needed. Mostly not expressible in CS2. |
| **Intersection Marking Tool** (2140418403) | key `NodeMarkup` (gzip XML: 1 160 marked nodes, 2 875 lines, 91 fillers, crosswalks); uses network `1889660581.Single Steel Barrier - raisable_Data` and prop `2889685220.Sidewalk-003 Decal_Data` | **no** | The **barrier network lines and decal props IMT renders itself** (they are not in NetManager/PropManager); filler areas. The XML is offline, but the geometry needs IMT's entrance-point maths. | Computed trajectories (`IMT.Manager` markings → lines → `ITrajectory`, fillers, crosswalk borders) sampled to polylines: **medium–large**. Value medium-low for CS2. |
| **Adaptive Networks** (3683231517) | key `AdaptiveRoads`; per-segment/node/lane flags. The session log shows "Serializing 0 lanes" | no | AN toggles on RAILWAY 2 style networks (which props and wires show) | **medium**; low value |
| **TM:PE** (1637663252) | key `TrafficManager_v1.0` (lane connections, arrows, speed limits, priority signs, timed lights, crosswalks) | no | Traffic rules. They don't matter for a CS2 map, which auto-generates them. | skip (offline BinaryFormatter if ever wanted) |
| **Theme Mixer 2.5** (3031623332) | key `THEME-MIXER-2.5` = mix id; the textures and LUT are in the theme-mix asset | theme base yes (`mapTheme`) | nothing for the map data | skip (audit-theme) |
| **Extra Landscaping Tools Revisited** (3733110460) | writes vanilla arrays only (sand, resources, terrain) | yes, via vanilla | nothing | – |
| **Surface Painter** | not in map | – | – | – |
| **Ploppable Asphalt** | no mod data; PO objects with `isPloppableAsphalt` and props whose mesh is `ploppableasphalt-*` | yes: `ploppableAsphalt` flag + colour per PO object; props by prefab | – | – |
| **Prop Line Tool / Prop Anarchy / Prop Precision** | positions end up in PropManager; legacy keys read by Prop Control | yes | – | – |
| **Move It** | no data | – | – | – |
| **Effects Module [PO]** (2632235148) | inside PO containers (`modulesData`) | count only | light/effect modules on PO objects | small (dump `m_modules` key/values); low value |

## 3. Correctness review (paths that need CS1 to test)

Verified correct against the decompiled code:

- **Bézier middle points.** `MapExporter.cs:268-272` calls `NetSegment.CalculateMiddlePoints(a, m_startDirection, d, m_endDirection, smoothStart, smoothEnd)` with smooth = the node's `Middle` flag. `NetAI.UpdateLanes` uses the same rule for lanes (`CalculateCorner` sets `smooth = (flags & Middle) != 0`, `NetSegment.cs:2878`), and both directions point into the segment, as the game passes them. On a straight segment the flag only moves b/c along the line (0.15 vs 0.3 × length); the geometry is identical.
- **Invert.** The flag is read correctly (`MapExporter.cs:282`). See doc gap D1.
- **Terrain sampling.** `SampleOriginalRawHeightSmooth` reads `m_rawHeights` (the saved heightmap, without object terraforming). `SampleRawHeightSmooth` reads `m_rawHeights2` (with buildings and networks applied). Both use `x/16 + 540` and return metres. The schema labels are right.
- **Extent.** `TerrainManager.RAW_RESOLUTION = 1080`, `RAW_CELL_SIZE = 16` give a half extent of 8 640. `GameAreaManager.TOTAL_AREA_RESOLUTION = 9` × `AREAGRID_CELL_SIZE = 1920` = 17 280. `RESOURCEGRID 512 × 33.75` = 17 280. The ±8 640 assumption is the game's real frame.
- **1081 vs 1080 grids.** Heights and water are `(1080+1)²` vertices (`WaterSimulation` allocates `(num+1)*(num+1)`, num = 1080). `m_rawSurface` is `new SurfaceCell[1166400]` = 1080², indexed `z*1080+x`. The exporter uses each correctly.
- **Natural resources.** Fields `m_ore, m_oil, m_forest, m_fertility, m_pollution, m_water, m_sand` exist on `ResourceCell`. The lock (`Monitor.TryEnter(m_naturalResources, SYNCHRONIZE_TIMEOUT)` spin) is the game's own (`NaturalResourceManager.cs:646`).
- **Water surface.** `BeginRead()` returns the front buffer `m_waterBuffers[~(m_waterFrameIndex >> 6) & 1]` under `m_simulationBufferLock`. `Cell.m_height` is depth in raw units. `TerrainManager.GetSurfaceHeight2` computes the surface the same way (`m_blockHeights + m_height`). Sea level `m_currentSeaLevel` is the serialized value.
- **Procedural Objects member names.** All match `ProceduralObjects.dll` from 3071492847: `ProceduralObjectsLogic.instance` (public static field), `proceduralObjects` (`List<ProceduralObject>`), `groups` (`List<POGroup>`), `layerManager`; `ProceduralObject.{id, baseInfoType, basePrefabName, m_position, m_rotation, m_scale, meshStatus (byte), m_mesh, customTexture, renderDistance, renderDistLocked, m_visibility, layer, group, isRootOfGroup, m_color, tilingFactor (int), isPloppableAsphalt, m_textParameters, m_modules, disableCastShadows, flipFaces}`; `POGroup.{root, objects}`; `Layer.{m_name, m_id (uint), m_isHidden}`. meshStatus 1 = base mesh, 2 = edited, as documented. PO fills the list in `ProceduralObjectsLogic.Start` on the frame after `OnLevelLoaded`, well inside the exporter's 10-frame delay.
- **Trees.** `m_trees.m_buffer` is Tree Control's expanded array (it replaces the buffer during deserialization and fills it from `MoreTrees`). The `Created`/`Deleted` test and `m_infoIndex` → `Info` are correct, and `TreeInstance.Position` is the vanilla getter.
- **Surfaces.** `TerrainManager.RawSurface` and the `SurfaceCell` channels (`m_clipped, m_ruined, m_gravel, m_field, m_pavementA, m_pavementB`) exist as used. Raw cell (c, r) has its corner at world (−8640 + 16c, −8640 + 16r).
- **Threading.** `ThreadingExtensionBase.OnUpdate` runs on the main thread, as `FindObjectOfType` and `Mesh` access need.

### Bugs and gaps

| # | Severity | Where | Problem | Fix |
|---|---|---|---|---|
| B1 | **High** (data loss) | `ProceduralObjectsExporter.cs:575-612`, summary `:551-558` | Only the live list is read. Objects PO failed to load (86 of 690 in the 2026-10-18 session) are silently absent. The summary's `failed` counts exporter exceptions only, which suggests nothing is missing. | Read `ProceduralObjectsLogic.failedToLoadObjects` and static `ProceduralObjects.Classes.PopupStart.loading_failures` (`missing_asset`, `containers` keys), and write each failed container (fields in §2) to `-po.json` as `failedObjects[]` with `missingAsset`. Report `poLoadFailures` in the summary and the log. Independently, make the base asset resolve (item 1 below). |
| B2 | Medium | `TreePointsWriter.cs:64-71` | Only x, z and prefab per tree: no y, flags or scale. Without the buffer id the offline `TreeScaling` blob (indexed by tree id) cannot be joined. | Write `y` (`m_posY/64`), `flags` (FixedHeight at least), Tree Control scale (`s_scalingData[i]` by reflection, `null` when absent) and the buffer id, or one parallel array per field. Bump to `trees/v2`. |
| B3 | Medium (missing content) | `ProceduralObjectsExporter.cs:642-653` | Edited PO meshes are reduced to a vertex count and bounds. Cube/Square-based POs are pure custom geometry and can't be rebuilt from bounds. | Write a binary sibling `-po-meshes.bin` (per edited object: id, vertices float32[], triangles int32[], uv float32[]) plus an index in `-po.json`. |
| B4 | Low | `MapExporter.cs:30, 738` | Over 50 000 props, positions degrade to rasters. Not hit here (3 743). | Always write points (65 535 maximum). |
| B5 | Low | `MapExporter.cs:648` | Skipped buildings leave no trace except a count. | Export them with `parentBuilding`, `untouchable`, and the owning node when `NetNode.m_building` points at them; also add `building` to nodes. |
| B6 | Low | `MapExporter.cs:69`, `TreePointsWriter.cs:44` | `treePoints` has no JSON keys. A failure mid-write leaves a truncated `-trees.json` that looks complete. | Write to `*.tmp` and rename on success (same for `-po.json` and the PNGs). |
| B7 | Low | `TerrainSurfaceExporter.cs:340-353` | The majority histogram has 4 slots, though the comment says 7. A 5th distinct value is dropped. No effect here (Surface Painter absent). | Use 8 slots or a small array. |
| B8 | Cosmetic | `MapExporter.cs:191-202` | In the editor `m_MapName`/`m_CityName` are null, so the files are called `map-…`. | Fall back to the loaded package name, or hard-code `dakota-falls` for this run. |
| D1 | Doc | `SCHEMA.md` `segments[].invert`, `netPrefabs.lanes[].direction` | With `Invert`, `NetAI.UpdateLanes` mirrors each lane's lateral position (`num4 = 1 - num4`) as well as flipping direction. `direction` is `m_direction`, not `m_finalDirection` (they differ only with left-hand traffic, which this map does not use). | Document it: the converter must negate lane offsets on inverted segments. |
| D2 | Doc | `SCHEMA.md` `flagsRaw` | `NetNode.Flags`/`Building.Flags` use bit 31. Written as signed int. No negative values occur in the v0.1.0 dump. | Note "read as uint32". |

## 4. Before the export: v0.4.0 list, by value for the CS2 port

1. **Recover the 86 failed Procedural Objects.**
   - (a) In the exporter, implement B1 so failed containers are written no matter what.
   - (b) Make them load. Subscribe to the original PO item 1094334744, keep its mod disabled (only its `Cube`/`Square` assets are needed; never two PO dlls), and load the map. The log must show "Loading custom asset Cube_Data from 1094334744" and no "Failed to load a Procedural Object". Unverified: whether CS1/LSM loads assets from a disabled mod's folder. If not, rely on (a).
   - **Do not save the map in the editor** with objects missing (the 2026-10-18 autosave already dropped them), and dismiss PO's "loading failures" popup without deleting.
2. **Tree y, flags, scale and id** in the tree sibling (B2). This is the only way to carry the ≈ 70 600 scaled trees and the snapped-tree heights into the dump.
3. **Edited PO meshes** (B3), including the failed objects' container vertices. These are irreplaceable custom shapes (walls, rock work, tunnel blocks, the PO waterfalls).
4. **Network Skins per segment** (small reflection, §2): street light, tree, pillar and catenary overrides and colour on the USRP roads. They decide which CS2 road variant or upgrade to use.
5. **Extra height grids as 16-bit PNGs** on the water PNG's frame: `RawHeights` (should equal the editor export; a consistency check), `RawHeights2` (with road and building terraforming) and `BlockHeights` (what the water runs on, dams included). Small; lets the converter pick cut/fill versus pristine terrain.
6. **Keep skipped buildings, flagged** (B5) and add `NetNode.m_building`. This separates CS1-generated pillars from the 112 hand-placed ones.
7. **Raw custom-data dump**: write every `m_serializableDataStorage` entry to `<base>-sdata/<key>.bin` with an index (key, length). This is insurance only: the same bytes are in the `.crp` (offline extraction proven, see the notes folder), but it pins the exact loaded state next to the dump.
8. Small extras: `m_sand` and `m_shore` as resource layers; `netPrefabs` variant links (elevated/bridge/slope/tunnel → base), lane props and terrain flags; always-points props (B4); temp-file writes (B6).
9. **Skip.** TM:PE, Adaptive Networks, Node Controller, Theme Mixer, ACME: low value for a CS2 map, and their raw data stays in the `.crp`; Node Controller and IMT XML decode offline today. IMT computed geometry (barrier lines, fillers) is the only one with visible CS2 value (the steel barriers), at medium–large cost. Do it only if the barriers matter.

Things the single export does **not** need to capture, because the `.crp` holds them permanently: all 22 mod blobs, the vanilla manager data, the map metadata asset and the preview images.
