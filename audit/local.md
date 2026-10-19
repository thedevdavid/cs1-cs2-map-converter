# Dakota Falls: local file audit

Date 2026-10-19. Read-only audit of the Workshop map, the user's two CS1 saves, the map-editor autosave of the export session, the 2026-10-18 dump, the playset and the exporter source. No pipeline file was changed.

## Method

- **Package tables.** `audit/tools/crp-table.mjs` reads the CRAP header (magic, version, package name, author, main asset, asset count, data offset) and each asset entry (name, checksum, type code, offset, size). `--strings <asset>` lists the length-prefixed strings of an asset, which decodes the metadata assets (they are ColossalFramework-serialised objects: type name, field name, value).
- **Mod data.** The main data asset (type code 52) is not compressed. Its strings are delta-coded: `0x80`, UInt16 BE length, then each character as the byte difference to the previous one, starting at 0x80. That is why plain `grep` finds no prefab names, and why the playset audit called the data "compressed". Right after the manager type list sits the serializable-data dictionary: Int32 BE count, then per entry key string, UInt24 BE length, payload. `audit/tools/crp-moddata.mjs` lists the entries; `--extract <dir>` writes every payload (gzip ones unzipped). The decoder is checked by chaining: every entry ends exactly where the next key starts, and the count matches (22 entries in the map).
- **Payload decoding.** IMT and Node Controller payloads are gzip XML. Procedural Objects is .NET BinaryFormatter split over two keys; `audit/tools/po-summary.cs` (.NET 10, System.Formats.Nrbf) decodes it. Network Skins was decoded by hand, using the modifier enum read from `NetworkSkins.dll` metadata: TerrainSurface=1, Color=2, StreetLight=3, Tree=4, Pillar=5, Catenary=6, RoadDecoration=7.
- **Dependencies.** `audit/tools/deps.mjs` collects every numeric Workshop id from the dump's prefab names (nets, buildings, trees, props), the PO base prefabs, the IMT prefabs and the map's saved mod list. It checks each id against `D:\SteamLibrary\steamapps\workshop\content\255710\<id>` and against `cs1-playset/DakotaFalls-Editor.json`, with titles from `cs1-playset/steam-cache.json`.

Rerun:

```bash
node audit/tools/crp-table.mjs "D:/SteamLibrary/steamapps/workshop/content/255710/2972879554/Dakota Falls.crp"
```
```bash
node audit/tools/crp-moddata.mjs "D:/SteamLibrary/steamapps/workshop/content/255710/2972879554/Dakota Falls.crp" --extract out/mapdata
```
```bash
dotnet run audit/tools/po-summary.cs -- out/mapdata
```
```bash
node audit/tools/deps.mjs C:/Users/dleva/AppData/Local/DakotaExport/map-20261018-132303.json cs1-playset/DakotaFalls-Editor.json D:/SteamLibrary/steamapps/workshop/content/255710 extra.json cs1-playset/steam-cache.json
```

## Files

| File | Path | Size | Modified | Package / main asset | Format |
|---|---|---:|---|---|---|
| Workshop map | `D:\SteamLibrary\steamapps\workshop\content\255710\2972879554\Dakota Falls.crp` | 44 609 684 | 2025-07-03 | `2972879554` / `Dakota Falls` | v10 |
| Save dktareference | `C:\Program Files (x86)\Steam\userdata\79367054\255710\remote\dktareference.crp` | 45 931 097 | 2026-03-21 | `dktareference` | v11 |
| Save dktanew | same folder, `dktanew.crp` | 49 019 025 | 2026-03-30 | `dktanew` | v11 |
| Editor autosave of the export session | `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Maps\AutoSave.crp` | 44 234 007 | 2026-10-18 13:33 (export ran at 13:23) | `2972879554` / `AutoSave` | v12 |

`%LOCALAPPDATA%\…\Saves` is empty; there is no `Addons\MapEditor\Maps`. Other saves in the Steam remote folder (NewSave, test, testwinsettings, maybe, alaska, bruderfalva) were not part of the brief and were not opened.

### Asset tables

Type codes as stored: 3 = Texture2D, 52 = game/map data (the simulation stream), 100 = MapMetaData, 101 = SaveGameMetaData.

| File | Asset | Type | Size |
|---|---|---|---:|
| Dakota Falls.crp | `Dakota Falls_SteamPreview` | 3 Texture2D (preview) | 493 849 |
| | `Dakota Falls_Snapshot` | 3 Texture2D (preview) | 179 165 |
| | `Dakota Falls_Data` | 52 map data | 43 932 546 |
| | `Dakota Falls` | 100 MapMetaData | 3 742 |
| dktareference.crp | `dktareference_Data` | 52 game data | 44 939 780 |
| | `dktareference_SteamPreview`, `_Snapshot` | 3 | 390 910, 154 122 |
| | `dktareference` | 101 SaveGameMetaData | 445 939 |
| dktanew.crp | `dktanew_Data` | 52 game data | 48 147 557 |
| | `dktanew_SteamPreview`, `_Snapshot` | 3 | 307 230, 117 448 |
| | `dktanew` | 101 SaveGameMetaData | 446 480 |
| AutoSave.crp | `New Map_Data` | 52 map data | 44 230 716 |
| | `AutoSave` | 100 MapMetaData | 3 076 |

No package has a separate Mods, theme, LUT or custom-data asset. All mod data lives inside the data asset (next table).

### Metadata, decoded

| Field | Dakota Falls.crp (MapMetaData) | AutoSave.crp (MapMetaData) | dktareference / dktanew (SaveGameMetaData) |
|---|---|---|---|
| name | mapName `Dakota Falls` | mapName `New Map` | cityName `Springfield` |
| environment | Sunny | Sunny | Sunny |
| timeStamp | 2023-05-07T23:26:14 +05:30 | 2026-10-18T13:33:06 −07:00 | 2026-03-21T06:46:04 −04:00 / 2026-03-30T05:03:44 −04:00 |
| mapThemeRef | **absent** (no Workshop map theme) | absent | – |
| mods used | 44 ids (list below) | 26 entries, incl. `DakotaExport`, 3071492847 (PO), 3702700108 (NS Continued), 3683231517 (AN fix) | 128 entries; also lists local style/theme folders `dktafix`, `dktflls`, `Lumina_dakotafalls`, `Lumina_dktfix`… |
| other | buildableArea, resource amounts, in/out connection counts, preview refs | same fields | `assets` list (~10 000 loaded assets, not a used-asset list), population, cash, achievementsDisabled |

SimulationMetaData at the head of the data asset:

| | Map | AutoSave | dktareference | dktanew |
|---|---|---|---|---|
| m_gameInstanceIdentifier | fbac126c-3537-44d5-91fd-e60958da8f7d | **same** | 7349d30a-… | 7f10a490-… |
| m_environment | Sunny | Sunny | Sunny | Sunny |
| m_MapName / m_CityName / m_MapThemeMetaData | null | null | null (+ "User/More CitizenUnits x2", "More Vehicles", "More PathUnits x2" mod-option strings) | same |

Mods saved with the Workshop map (MapMetaData `mods`): 2040656402 445589127 530771650 530871278 1094334744 1209581656 1530376523 1591417160 1619685021 1637663252 1758376843 1764637396 1794015399 1890830956 2041457644 2105755179 2133885971 2140418403 2172488844 2255219025 2367735356 2424785481 2443885282 2448994345 2472062376 2487213155 2560782729 2590879367 2616880500 2620852727 2623699944 2632235148 2669938594 2688768140 2778750497 2862881785 2869784351 2911362537 2916526868 2954236385 2956854293 2956939040 2960858317 2966894510.

### Mod data inside the data asset

Sizes in bytes. "–" = key absent.

| Key | Map | AutoSave | dktareference | dktanew | Mod | What it holds |
|---|---:|---:|---:|---:|---|---|
| ProceduralObjectsDataKey + …Key1 | 16 711 679 + 8 688 994 | 16 711 679 + 8 609 934 | same as AutoSave | same as AutoSave | Procedural Objects | **690** objects in the map (425 building-based, 265 prop-based), **684 with edited meshes** (967 798 stored vertices), 96 with a custom texture, 626 in groups. Saves and AutoSave: **604**. |
| ProceduralObjectsLayerData | 92 | 92 | 92 | 92 | PO | layer names |
| TrafficManager_v1.0 (+ VersionInfo) | 11 808 | 11 808 | 11 808 | 18 313 | TM:PE 11.7.4 | lane connections, arrows, speed limits, priority signs, timed lights |
| NodeMarkup (gzip XML, 1.3 MB unzipped) | 140 168 | 140 270 | 140 270 | 139 671 | Intersection Marking Tool 1.14 | **1 160 node markings + 985 segment markings, 2 875 lines, 91 fillers**: 1 914 line rules are network lines with prefab `1889660581.Single Steel Barrier - raisable` (81.6 km of guardrail along segments), the rest painted (solid, dashed, double); fillers: 60 chevron, 6 stripe, 7 grass islands, 14 Sidewalk-003 textured; no crosswalks (corrected after the full decode in `cs1-offline/out/imt-summary.md`) |
| NodeMarkupUsedPrefabs | 114 | 114 | 114 | 114 | IMT | `1889660581.Single Steel Barrier - raisable_Data`, `2889685220.Sidewalk-003 Decal_Data` |
| NodeController (gzip XML) | 29 045 | 29 091 | 29 091 | 29 422 | Node Controller Renewal 3.4 | **1 067 nodes / 2 162 segment ends** with custom node type, offsets, flat/stretch settings (saves: 1 070) |
| NetworkSkins_APPLIED_SKINS (+ ACTIVE_SELECTION) | 140 136 | 140 136 | 140 136 | 139 983 | Network Skins 2 v1.2 | 15 skins on USRP interstate / national-road bridge and elevated prefabs: **pillars removed on 164 segments**, **street lights removed on 62 segments** (saves: 12 skins) |
| AdaptiveRoads | 1 164 576 | same | same | 1 164 612 | Adaptive Networks | per segment, node and lane AN flag arrays (25 % non-zero) |
| THEME-MIXER-2 / THEME-MIXER-2.5 | 68 / 68 | 68 / 68 | 68 / 68 | 68 / 64 | Theme Mixer 2 / 2.5 | name of the active mix: `ProjectMap4_5dec87…` (TM2), **`DakotaFalls_5dec87…`** (TM 2.5); dktanew: **`dktafix_5dec87…`** |
| MoreTrees | 4 419 514 | 4 419 395 | 4 419 583 | 4 413 324 | Tree Control 0.9 | expanded tree buffer (trees beyond the vanilla 262 144) |
| TreeSnapping | 2 112 698 | ≈ | ≈ | ≈ | Tree Control | per-tree height (UInt16 per slot, 1 048 576 slots) |
| TreeScaling | 4 194 405 | same | same | same | Tree Control | per-tree scale (float per slot); ≈ 49 400 trees ≠ 1.0 |
| TreeControlAnarchyFlags | 131 202 | same | same | same | Tree Control | anarchy bits |
| TreeAnarchy | 19 430 | same | same | same | Tree Anarchy (legacy) | legacy tree data, only Aussie Bushes named |
| PropSnapping / PropPrecision / PropScaling | 29 325 / 22 567 / 262 245 | same | same | same | Prop Snapping, Prop Precision, Prop Control | prop heights, sub-unit positions; **PropScaling is all 1.0** (65 536 × 0x3F800000) |
| ACME | 763 | same | same | same | ACME | camera bookmarks |
| PropPainter | – | 262 267 (near-empty) | 262 267 | 262 267 (colours set) | Prop Painter | absent from the map; written by the active mod; colours only in dktanew |
| SurfaceManager | – | – | 73 018 | 73 018 | Surface Painter | **empty grid** (72 902 of 73 018 bytes = 0xFF; identical in both saves) |
| REPAINT_NET_DATA, PAINTER_COLOR_DATA | – | – | 8, 263 | 62 200, 411 | Repaint, Painter | in-game network/building colours, dktanew only |
| fakeDM/EM/GAM/WM, BOB, AVO, BuildingControl, ZoningAdjusterData2, ABLC, IPT_*, ImprovedPublicTransport, RealisticPopulation, DistrictStylesPlusMod, TransferManagerCE, VehicleSelector, BuildingSpawnPoints, TMPE_Options | – | – | yes | yes | 81 Tiles and gameplay mods | city simulation only |

Not present in any file: Move It (it stores nothing), Ploppable Asphalt, Extra Landscaping Tools (sand goes to vanilla `NaturalResourceManager.m_sand`), Surface Painter content, a map theme asset, a LUT reference.

## Source of truth

1. **The 2026-10-18 export came from the Workshop map `Dakota Falls.crp` (2972879554), loaded in the map editor.**
   - The autosave from that session (13:33, ten minutes after the 13:23 export) has package name `2972879554` and the same `m_gameInstanceIdentifier` (fbac126c…) as the Workshop map.
   - Its SimulationMetaData has `m_MapName` null; MapMetaData `mapName` "Dakota Falls" is a package field that the exporter does not read. That explains `mapName "map"`.
   - The dump's network prefab table matches the map's NetManager prefab table string for string.
   - The dump was written by **exporter 0.1.0** (`schema dakota-export/cs1/v1`): no mapSettings, water surface, terrain surfaces, PO or tree points.
2. **The saves are cities played on the map, not later edits of it.**
   - They are SaveGameMetaData (city "Springfield"), each with its own game-instance id, and they add about 5 200 city strings (zoned buildings, vehicles, policies, 81 Tiles data).
   - Their map content is equal or **poorer**: PO 604 vs 690, IMT lines 2 848 vs 2 875, NS2 skins 12 vs 15 (2 bridge skins and 1 interstate skin fewer), MoreTrees 4 413 324 vs 4 419 514 bytes in dktanew.
   - The only additions are 3 Node Controller nodes and in-game colours (Prop Painter, Repaint).
   - dktanew also switched the Theme Mixer 2.5 mix to the user's own `dktafix` mix.
   - The larger data assets (44.9 MB and 48.1 MB vs 43.9 MB) are citizens, vehicles and city buildings.
   - **Export from the Workshop map, not from a save.** Use dktanew only if the user wants the `dktafix` mix or the in-game colours, which do not affect the CS2 terrain or networks.

## Dependencies

90 Workshop ids are referenced. **Every asset id behind the dump's 4 687 buildings, 740 967 trees, 3 743 props and 5 532 segments, behind all 51 PO base prefabs, and behind the IMT prefabs is on disk and in the playset**: 59 ids, from 1230960704 Pines to 918308740 Tunnel Entrances, incl. 2889685220 Sidewalks & More Decals + PO Textures and 1889660581 Fences Pack.

Gaps (from the map's saved mod list and the PO data):

| id | title | on disk | in playset | matters? |
|---|---|---|---|---|
| **1094334744** | Procedural Objects (original) | no | no (replaced by 3071492847) | **Yes**: 86 PO objects use base prefab `1094334744.Cube_Data`. 3071492847 ships `NativeCube_Procedural.crp`, which loads as `3071492847.Cube_Data`, and the 86 cubes are **dropped on load**. Proof: the export session's AutoSave and both saves hold 604 objects, and the 86 missing ones are exactly the cubes. Still on the Workshop (Steam result 1). |
| 2954236385 | (removed) original Theme Mixer 2.5 | no | no (3031623332 instead) | no: the mix only stores its name |
| 1758376843, 2669938594 | NS2 (deprecated), AN 3.18 (broken) | no | replaced by 3702700108, 3683231517 | no; data loads with the replacements (NS2 1.2.8391 → 1.3.9594 re-serialised in AutoSave) |
| 445589127 | Precision Engineering (old) | no | no (3680103097 Harmony build is in) | no data |
| 1209581656, 530871278, 1794015399 | Relight, Daylight Classic, Render It! | no | no | visual only; Relight is also needed for LUT `Relight2Natural` (1539181199 More Relight LUTs, **not on disk, not in playset**) |
| 2688768140 | Ploppable Asphalt + | no | no | no: no PA data in the map, no PA props in the dump |
| 2424785481, 2443885282, 2623699944, 2911362537, 2916526868 | other theme mixes | no | no | no (author's library) |
| 2620852727 | (Steam result 9, unknown or removed) | no | no | unknown; no key or prefab refers to it |
| 1764637396, 1890830956, 2105755179, 2133885971, 2172488844, 2255219025, 2367735356, 2448994345, 2487213155, 530771650, 2590879367, 2616880500, 2778750497, 2869784351, 2956939040, 2966894510 | Toggle It, Undo It, FPS Booster, Find It 2, Picker, UUI, Unlimited Outside Connections, YAT, UI Resolution, Prefab Hook, Common Ground, Image Overlay, ACME, Forest Brush, Line Tool, Visibility Control | some on disk | no | no map data, except ACME camera bookmarks |

Checks the brief asked for:
- **Map theme:** none; there is no `mapThemeRef` and m_MapThemeMetaData is null. The look comes from the Theme Mixer 2.5 mix `DakotaFalls` = Workshop 2972881605 (in the playset).
- **LUT:** Relight2Natural, not available. Visual only.
- **PO:** 3071492847 is in, but see the cube gap.
- **NS2 / NC Renewal / IMT / TM:PE / AN / Tree Control / Prop Control:** all in the playset, and their data loads.
- **ELT:** Revisited 3733110460 is in; it has no data of its own.
- **Surface Painter:** in the playset, but there is nothing to read; `terrainSurfaces.sources.surfacePainter` will report no data.
- **Ploppable Asphalt+:** not needed.

## Findings, ranked by visual impact of what the export misses

1. **Procedural Objects: 690 objects, 684 with hand-edited meshes. The exporter cannot carry them, and 86 are already lost in the editor.**
   - Content: 38 pieces of `2931939156.Detroit Bridge`, ~300 cliff rocks (1981694823 Mossy Grey, 2206365654 Paintable, VC rocks), 46 animated waterfalls (2072216451), 35 tunnel portals (918308740), 12 `[US] 3-Lane Highway Pillar`, a rock shed and a truss. The 57 `1251664417.Dam Electricity` objects are zero-size effect emitters (Effects module), not dams (corrected after the full decode; see `cs1-offline/out/po-classes.md`).
   - The 86 cubes are paving: Sidewalks & More textures `Cobble-003_d` (71), `Sidewalk-006_d` (11), `Sidewalk-007_d` (2), `Sidewalk-003_d` (2), e.g. around (440, 127, 199) and (−3 500, 240, −3 200).
   - Exporter v0.3.0 reads PO by reflection and writes placement and bounds only (`vertexCount`, `localBounds`, no vertices). It also only sees what PO loaded, i.e. 604 objects.
   - The full data, including all vertices, textures, colours and groups, is in the .crp and decodes offline (`po-summary.cs`).
2. **Intersection Marking Tool: 1 160 node markings + 985 segment markings, 2 875 lines.**
   - 1 914 of the lines are steel guardrail networks drawn along the interstates (`1889660581.Single Steel Barrier - raisable`).
   - The rest are ~1 000 painted lines (32.7 km) and 91 filled areas; no crosswalks. Decoded to geometry in `cs1-offline/out/imt-markings.geojson`.
   - Not exported. The XML (point = segment end + lane offset) is readable offline.
3. **Tree heights and scales (Tree Control).**
   - `<base>-trees.json` has x, z and prefab only.
   - TreeSnapping holds a height for every tree; trees standing on PO rocks, cliffs and the ~4 500 rock buildings will fall to terrain in CS2.
   - ≈ 49 400 trees have a non-1.0 scale.
   - Not exported.
4. **Network Skins.**
   - 164 bridge or elevated segments have **no pillars**, typically where the Detroit / Hernando de Soto bridge buildings or PO pillars carry the deck. CS2 will add its own pillars there.
   - 62 segments have no street lights.
   - Not exported; offline decode works.
5. **Node Controller: 1 067 nodes / 2 162 segment ends** (custom node types, corner offsets, flat and stretched junctions on the interstates). CS2 builds its own junctions, so this is useful mainly as a hint list for merges and acute junctions. Not exported.
6. **Theme Mixer 2.5 mix `DakotaFalls`** (terrain textures, atmosphere, water colours) and the Relight LUT: not in the dump; `mapSettings.mapTheme` will be null. Low impact for CS2; the mix XML is in 2972881605.
7. **Adaptive Networks flags (1.16 MB), TM:PE (lane connections, arrows, speed limits), Prop Snapping/Precision, ACME, legacy Tree Anarchy.** Low or no visual impact. The dump's props already carry y and `fixedHeight`, and PropScaling is all 1.0.
8. **Exporter version.** The dump in use was written by v0.1.0. Water surface, terrain surfaces (incl. `m_sand`), mapSettings (tile grid, sea level, start tile), tree points and the PO summary need the v0.3.0 run.

## Before the export

1. **File to export from:** the Workshop map `2972879554\Dakota Falls.crp`, loaded in the **map editor** (LoadMap), as on 2026-10-18. Not dktanew or dktareference. Do not save over it from the editor: the save would drop the 86 PO cubes for good, as AutoSave.crp already did.
2. **Run DakotaExport v0.3.0** (installed, not yet run) on that file, so the v3 sections exist.
3. **Playset:**
   - Optional: subscribe to **1094334744** (original PO) **as an asset source only**, with its mod disabled, so that `1094334744.Cube_Data` resolves and PO 3071492847 keeps the 86 cubes. This is untested, and both mods share the `ProceduralObjects` assembly name. If CS1 skips assets of disabled mods, prefer step 4 (offline), which does not depend on it.
   - Optional (visual only): More Relight LUTs 1539181199 + Relight 1209581656.
   - Nothing else is missing for the map's content.
4. **Exporter or offline sections to add** (in this order):
   1. **PO with meshes.** Read `ProceduralObjectsDataKey`+`Key1` from the .crp offline: per object id, base prefab, position, rotation, vertices, texture name, colour, group, layer. The format is proven by `po-summary.cs`. Write OBJ/glTF per object for CS2 asset import, or at least position plus bounds for all 690. Offline is better than reflection here: it keeps the 86 cubes and the vertices.
   2. **IMT markings.** Unzip the `NodeMarkup` XML; export guardrail lines as polylines (for CS2 guardrail / fence networks or props) and fillers as polygons (paved areas).
   3. **Tree height and scale.** Add `y` (TreeSnapping / `TreeInstance.m_posY` with the FixedHeight flag) and scale to `<base>-trees.json`. In game: `TreeInstance.Position` already includes snapped heights.
   4. **Network Skins per segment.** Write a `pillarsRemoved` / `streetLightsRemoved` flag (or the skin index) per segment, so the converter can suppress or adjust CS2 pillars on those bridges.
   5. **Node Controller per node.** Node type and per-end offsets, as converter hints.
   6. **Theme Mixer mix name** in `mapSettings` (`THEME-MIXER-2.5` → `DakotaFalls`) for the record.
5. **Correction for `cs1-playset/AUDIT.md`:**
   - The data asset is not compressed; its strings are delta-coded (see Method).
   - Prop Painter has **no** data in the Workshop map, so the "rock colours" claim does not apply to the map.
   - Surface Painter data is absent from the map and empty in the saves.
