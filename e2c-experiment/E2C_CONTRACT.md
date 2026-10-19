# Earth2Cities input contract (v0.0.17, PDX 140919)

Date: 2026-10-18. Source: `Earth2Cities.dll` from `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\.cache\Mods\pdx_mods\140919_13\`, decompiled with ilspycmd 11.1 to a scratch folder outside this repo. Nothing below is copied code; identifiers are given so the claims can be rechecked.

Confidence levels:

- **Verified (code)**: read directly in the decompiled source; behaviour follows unambiguously.
- **Inferred**: follows from the code but depends on game behaviour I could not run (CS2 net pipeline, prefab data values).
- **Guess**: plausible, unchecked. The calibration import settles most of these.

## 1. Files and folders

| Fact | Confidence | Where |
|---|---|---|
| The only input the UI path reads is `overpass_roads.osm` in the **mod install folder**, i.e. the folder holding `Earth2Cities.dll`: `...\.cache\Mods\pdx_mods\140919_13\` today. | Verified | `Mod.OnLoad` sets `ModInstallFolder` from the executable asset path; `Setting.ImportRoadsJson` combines it with `overpass_roads.osm` |
| "Step 4: Import Roads" **always** converts that OSM file to `roads.json` in the same folder first, overwriting any `roads.json` you put there, then imports `roads.json`. So the converter's OSM file is the real input; our `roads.json` is for inspection only. | Verified | `Setting.ImportRoadsJson` → `OsmToRoadsJsonConverter.ConvertOsmToRoadsJson` → `RoadImporter.ImportFromJson` |
| `roads.json` is read directly (no OSM step) only by "Import Terrain Then Roads" and "Generate Terrain Then Roads", which are **not shown** in the options UI (see §6). | Verified | `TerrainHeightmapImporter` (reads `roads.json`), `Setting` properties are `internal` |
| The folder name carries the mod version (`_13`). A mod update creates a new folder: copy the OSM file again. | Inferred | PDX cache layout |
| Fallback folder if the asset path lookup fails: `%LOCALAPPDATA%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\Earth2Cities`. The log line `Mod install folder:` says which one is used. | Verified | `Mod.OnLoad` catch branch |
| Log: `Logs\OSMImportMod.Mod.log` under the CS2 user folder (logger name `OSMImportMod.Mod`), plus Unity `Player.log` for the `[OsmConverter]` lines. | Inferred | `LogManager.GetLogger("OSMImportMod.Mod")`; converter uses `Debug.Log` |

## 2. Coordinates: how lat/lon becomes game metres

Two modes exist; the import always uses the first in practice.

**Terrain-aligned mode** (used whenever the four "Overpass Top/Right/Bottom/Left" text settings parse as numbers with top > bottom and right > left, which their defaults `31.5 / -34.5 / 31.0 / -35.0` already satisfy). Verified, `RoadImporter.ImportFromJson`, `TryReadSettingsBbox`, `BuildBoundsFromCenterMeters`, `GeoToTerrainAlignedLocal3D`:

1. Centre = midpoint of the **settings** bbox (not the OSM `<bounds>`, not `roads.json` meta).
2. Square of side W = playable width (14 336 m for `MapSizeKm` 14, the default) is built around the centre in degrees: half-height W/2 / 111 320, half-width W/2 / (111 320 · cos(centre lat)).
3. Each point maps linearly: x = (u − 0.5)·W, z = (v − 0.5)·W with u, v the point's fraction across the square. **Points outside are clamped onto the border.**
4. Origin Lat/Lon, Origin X/Z, Default Scale, rotation, AutoCenterBBox and CenterImportOnFirstPoint are **ignored** in this mode (the log says so).
5. Arithmetic is float32 (lat/lon parsed as double, stored as float).

Net effect: x = (lon − cLon) · 111 320 · cos(cLat), z = (lat − cLat) · 111 320, east = +x, north = +z, no rotation, scale 1. The converter writes exactly the inverse around centre (0, 0), where float32 error is about 2 mm (unit-tested against a model of the formula above). With E2C's Atlantic default centre (31.25, −34.75) the error grows to about 0.5 m.

**Origin mode** (`GeoToMapConverter.GeoToLocal`, equirectangular around OriginLat/OriginLon, × DefaultScale, optional rotation, + OriginX/Z) applies only if the bbox fields fail to parse. Then the `roads.json` meta bbox is used as a terrain-aligned fallback anyway. Not reachable in normal use. Verified.

**Map size** (`MapSizeProfile`): 14/28/57/114 km map widths of 14 336 / 28 672 / 57 344 / 114 688 m, world = 4×. `MapSizeKm` has no UI section attribute and defaults to 14, which matches the vanilla map. Verified.

**Unknown: axis sense in the game.** Whether CS2 +z points to the top of the editor's heightmap preview (the same way CS1 +z does) is not visible in E2C. The calibration import answers it (§8).

## 3. OSM file contract

Verified, `OsmToRoadsJsonConverter`:

- Root element must be `<osm>`; `<bounds minlat minlon maxlat maxlon>` is optional (bounds are otherwise derived from nodes) and feeds only `roads.json` meta.
- `<node id lat lon>`; `<way id>` with `<nd ref>` and `<tag k v>`. Element lookup is by local name, direct children first.
- A way is kept if `highway` maps to a category:

| OSM `highway` | E2C category |
|---|---|
| motorway, motorway_link, **trunk, trunk_link** | motorway |
| primary, primary_link | primary |
| secondary, secondary_link | secondary |
| tertiary, tertiary_link | tertiary |
| residential, living_street, unclassified, service, track, road | residential |
| footway, pedestrian, cycleway, path, steps, bridleway, corridor, construction, proposed, raceway, bus_guideway, escape, anything else | dropped |

  Links lose their link status; trunk becomes a highway-class road.
- `oneway`: yes/1/true/-1 → one-way; `junction=roundabout` → one-way. **`-1` is not reversed**, so the writer must emit points in driving order with `oneway=yes`.
- `lanes`, `lanes:forward`, `lanes:backward`, `lanes:both_ways` → one lane count (one-way: forward, else backward, else lanes; two-way: forward+backward(+both), else lanes). Missing → inferred from category.
- `maxspeed` → `speed_kph` (written, not used for prefab choice). `name` → road name (passed along, not applied to the edge).
- A node referenced by two or more kept ways gets `"junction": true`.

**roads.json schema** (Verified, writer in `OsmToRoadsJsonConverter`, reader `RoadsJsonLoader`):

```text
{ "meta": { "source": str, "crs": "WGS84", "bbox": [minLon, minLat, maxLon, maxLat] },
  "roads": [ { "id": str, "name": str|null, "category": str, "one_way": bool,
               "lanes": int|null, "speed_kph": number|null,
               "points": [ { "lat": num, "lon": num, "junction": true? } ] } ] }
```

## 4. Prefab choice

Verified, `RoadPrefabResolver` (`CuratedRoadProfiles`, `ResolveForRoad`, `ResolveHighwayForConnection`, `FindDefaultRoadPrefab`). Only 17 vanilla prefabs are candidates, matched by exact name: Small Road, Small Road Asymmetric, Small Road Oneway - 1 lane / - 3 lanes, Medium Road, Medium Road Asymmetric - 2+3 / - 1+3 lanes, Large Road, Large Road Asymmetric, Alley (service only), Highway Twoway - 2/3/4 lanes, Highway Oneway - 1/2/3/4 lanes. Custom and Road Builder prefabs are never used.

Scoring per road: one-way mismatch +1000, |lane difference| × 100, |speed difference|, +250 for a non-plain prefab when the category is not motorway. With explicit lanes only candidates with exactly that lane count are considered (target capped at 6). Target speed per category: motorway 100, primary 70, secondary 55, tertiary 40, else 30.

Consequences that drive the converter's lane policy (Inferred from that code):

| Written (category, direction, lanes) | Expected prefab |
|---|---|
| motorway one-way 3 / 4 | Highway Oneway - 3 / - 4 lanes |
| motorway one-way **1 or 2** | **no highway**: explicit lanes ≤ 2 exclude every name containing "highway"; 1 → Small Road Oneway - 1 lane; **2 → two-way Small Road** (the only 2-lane candidate left) |
| motorway one-way, lanes omitted | Highway Oneway - 3 lanes |
| motorway two-way 3 / 4 | Highway Twoway - 3 / - 4 lanes (speed decides against Small Road Asymmetric / Medium Road) |
| any one-way 2 lanes | two-way Small Road (fallback) |
| two-way 2 / 3 / 4 / 5 / 6 | Small Road / Small Road Asymmetric / Medium Road / Medium Road Asymmetric - 2+3 / Large Road |

So E2C cannot produce Highway Oneway - 1 or - 2 lanes, nor Highway Twoway - 2 lanes, from file input. The converter therefore writes 3 lanes for one-way interstates with fewer (`--motorway-min-lanes`), 1 lane for ramps (`--link-lanes`, gives Small Road Oneway - 1 lane), and 1 or 3 for other one-way roads. The "Highway beats Small Road at equal lanes" rows depend on the prefabs' `RoadData.m_SpeedLimit` values and units, which I did not read from game data: **Guess**, check in the calibration (diagonal must be a highway).

**Border override** (Verified, `IsNearBoundary`, `BoundaryConnectionMarginMeters` = 600): any edge with an end within 600 m of the playable border is replaced by a Highway prefab of the same direction (lanes clamped 1–4; this path does allow 1- and 2-lane highways). With the 0.8296 squeeze the CS1 border is the CS2 border, so every CS1 road reaching the outer 723 CS1-metres becomes a highway there. The converter counts these edges in `stats.txt`.

## 5. Geometry and elevation

Verified, `RoadImporter.ImportFromJson`, `SimplifyPolyline`, `SnapEndpointToGrid`, `OsmRoadToolSystem.CreateEdgeRequest`:

- Every pair of consecutive points becomes **one straight edge** (control points on the chord). No curves are fitted, so polyline density sets how smooth curves look and how many nodes the map gets.
- Points closer than 1.5 m to a neighbour are dropped (junction points kept). The "straight within 2°" removal never fires in practice: the angle it tests is 180° for a straight line. Pieces shorter than 1 m are skipped; duplicate undirected pieces (0.1 m grid) are skipped.
- **Endpoint snapping:** each edge end is moved onto any already-placed edge end within **6 m** (6 m grid cells). Parallel carriageways or ramp gores closer than 6 m get welded together. The converter drops interior points closer than 8 m and reports edges shorter than 6 m.
- **Elevation is not imported.** Each end's height comes from the terrain (`TerrainHeightSampler.TrySampleHeight` via the game's `TerrainSystem.GetHeightData` / `TerrainUtils.SampleHeight`). `ImportElevation` (500, hidden) is used only where sampling fails, or replaced by the height of the first existing road node if `SnapElevationToExistingRoad` (default on, hidden) finds one. Bridges, viaducts and tunnels are laid on the ground surface. Verified.
- The course is created with `CoursePos.m_Entity = null` (no attachment to existing nodes) and `m_Elevation` set to the absolute terrain height. Whether the net pipeline merges coincident ends within one batch into shared nodes, and whether that elevation value lifts roads, is **Inferred/Guess**: the author's screenshots show connected networks on terrain; the calibration origin junction answers it.
- All queued edges become `CreationDefinition` + `NetCourse` + `OwnerDefinition` + `Updated` entities in one frame through `ToolOutputBarrier`, from `OsmRoadToolSystem` (a `ToolBaseSystem`, tool id "OSM Road Tool") made the active tool; apply mode goes None → Apply once the temps appear. Thousands of edges therefore apply in one batch. Performance for ~9 000 edges: **unknown**.

## 6. UI (Options → Earth2Cities → Main) and what is actually visible

The game builds mod option pages from **public** instance properties only (`Game.UI.Menu.AutomaticSettings`, `GetProperties(Instance | Public)`; checked in the installed `Game.dll`). Earth2Cities declares several commands `internal`, so they do not appear. Verified.

| Control | Visible | Effect |
|---|---|---|
| Step 1: Open BBox Map Tool | yes | **Resets the four bbox fields to the Atlantic default**, starts a local web page to pick a bbox. Do not use for this experiment. |
| Overpass Top / Right / Bottom / Left | yes | Text fields; their centre is the projection centre (§2). |
| Step 2: Generate Terrain From BBox | yes | Downloads DEM and overwrites the terrain. Do not use. |
| Step 3: Download Roads from OSM | yes | Overwrites `overpass_roads.osm` from the internet. Do not use. |
| **Step 4: Import Roads** | yes | OSM → roads.json → queue edges (§1). |
| Move Imported Offset X / Z (±1000 m), Y (±500 m), **Move Imported Roads** | yes | Shifts every node and edge carrying the import tag (`RoadCleaner.OffsetTaggedImportedRoads`). |
| Rotate Imported Degrees (±180), **Rotate Imported Roads** | yes | Rotates tagged roads about their centroid (`RotateTaggedImportedRoads`). |
| Default Scale, Origin Lat/Lon/X/Z | yes | Ignored in terrain-aligned mode. |
| Terrain Height Scale, tile zoom, offsets, contrast | yes | Terrain generation only. |
| Operation Active / Progress / Status | yes | Status display. |
| Enable Debug Logging (Debug tab) | yes | Also registers the diagnostics systems `RoadCreationMonitor` and `ImportedRoadLifecycleSystem` (takes effect at next game start). |
| **Undo Import** | **no** (internal) | Would delete tagged roads, and if none are tagged, **every road in the map** (`RoadCleaner.DeleteAllRoads`). |
| Import Terrain Then Roads, Import Configured Terrain, Generate Terrain Then Roads, Import Elevation, Snap Elevation, Auto Center, Center On First Point, Dump Road Prefabs | no (internal) | Not reachable without editing the mod's settings file (not recommended). |

**Tagging** (Verified, `OsmRoadToolSystem.StartImportBatchIfNeeded` / `TagImportedEntitiesForCurrentBatch`): at the start of a batch E2C snapshots all existing road edges; when the batch ends, every road edge not in the snapshot and its two nodes get `ImportedRoadTag`. Roads you draw during the import window are tagged too. `ImportedRoadTag` is an empty `IComponentData` with no serialization interface, so the tag is most likely **lost on save/reload** (Inferred): Move/Rotate work only in the session of the import.

**Undo path** therefore: none inside E2C. Save the map under a new name before importing, and reload it to undo. Bulldoze for local fixes.

## 7. Terrain-related settings that matter here

Verified, `Setting.SetDefaults`: `TerrainHeightScale` 1000 and `GeneratedTerrainVerticalOffset` 500 apply only to E2C's own terrain import paths, not to roads. Road heights follow whatever terrain is loaded, so import the Dakota heightmap (Height Scale 1024 m) in the editor first.

## 8. What the calibration import must confirm

| Question | Expected if this contract is right |
|---|---|
| Scale | X arm ends at x = ±2500 m, Z arm at z = ±2500 m (5000 m each), diagonal end at (3535.5, 3535.5). |
| Rotation / mirror | Small Road along x, Medium Road along z, diagonal in the +x +z quadrant, heading away from the origin. |
| Junction | One node at (0, 0) joining all three; vertices every 250 m become nodes. |
| Prefabs | Small Road, Medium Road, Highway Oneway - 3 lanes. |
| Elevation | Roads sit on terrain; nothing floats at 500 m or at 2× terrain height. |
| Tag / move | Move by +100 X moves exactly these roads. |
