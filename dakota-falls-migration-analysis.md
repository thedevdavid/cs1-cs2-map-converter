# Dakota Falls: CS1 to CS2 migration analysis

Date: 2026-10-18. Scope: how to move the CS1 Workshop map "Dakota Falls" (Steam ID 2972879554) into the Cities: Skylines II map editor with as much automation and AI help as is realistic, including roads, rail, water and outside connections, not only terrain.

Everything below was checked against your machine (CS1 and CS2 installs, your heightmaps, your projects, your playset) and against primary sources (CO dev diaries, the CS2 wiki, mod source on GitHub, mod binaries on disk). Inferences are marked.

---

## 1. Recommendation

Do not try to convert the map file. Treat the job as three layers:

1. **Terrain**: already done by you, with two corrections to decide (height scale, and the squeeze, see §2.1).
2. **Vector layer** (networks, water sources, outside connections): export it from CS1 with a small mod, convert it offline, and push it into the CS2 editor through an importer that uses the game's own tool pipeline. Two importers already exist that you can test before writing one: **Earth2Cities** (installed, alpha, editor-tested by its author, consumes a `roads.json`) and **CS2MCP** (open source, Apache-2.0, local HTTP bridge with road-building and road-graph tools, plus a ready MCP server). The right long-term base is a **fork of CS2MCP's bridge** with a batch import endpoint added; Earth2Cities is the quickest experiment for the road layer. CityMCP, the project you named, is not the one to build on.
3. **Detailing** (740 k trees, props, procedural-object mountains, waterfalls, bridges): manual in the editor, guided by an Image Overlay rendered from the exported data, with brushes and random tree pools instead of individual trees.

The MCP part is real but secondary: an interactive session is the right tool for the long tail (a ramp that did not connect, a water source that floods), not for placing 2 000 segments one tool call at a time. The batch import does the bulk; MCP does the fixes.

---

## 2. What you already have (verified locally)

| Item | State |
|---|---|
| CS1 source | `D:\SteamLibrary\steamapps\workshop\content\255710\2972879554\Dakota Falls.crp` (44.6 MB; a map, not a save). CS1 cloud saves `dktanew.crp` and `dktareference.crp` are cities built on it. |
| CS1 game | Install at `D:\SteamLibrary\steamapps\common\Cities_Skylines`, Move It present, ModTools and CSL Map View not installed. |
| CS1 mod toolchain | `D:\work\mods\train-converter\src\TrainConverter.Cs1Exporter` builds a net35 CS1 mod with .NET 10 and installs it to `Addons\Mods`. Reusable as the skeleton of a network exporter. |
| CS2 base heightmap | `Heightmaps\dktflls-4096.png`: 4096², true 16-bit grayscale. Min 2392, max 65535 (0.34 % of pixels clipped at the ceiling), mean 19350. |
| CS2 world map | `Heightmaps\dktflls-4096_worldmap.png`: 16-bit, centre 1024² equals the base exactly. Exterior is hmap-ext trend output; detail and erosion tickets are still open in that repo and the exterior currently reads as smooth lobes. |
| CS2 map in progress | `Maps\76561198039632782\dktflls.cok` (16.5 MB, 2026-07-25). |
| Active playset "lessgo-withEAI" | 685 enabled mods, including Anarchy, Move It, Image Overlay, Water Features, Tree Controller, Extra Landscaping Tools, Node Controller, Network Tools, Road Builder, Grid Road Generator, Advanced Line Tool, Traffic, Find It, Map Texture Replacer, 529 Tiles. **Earth2Cities is downloaded (package 140919_13) but not enabled in this playset.** Scene Explorer is downloaded but not enabled. |
| CS2 modding toolchain | Installed (`CSII_TOOLPATH` set, `UnityModsProject` present), .NET SDK 10.0.402. |
| Scripting runtimes | Node 26 and .NET 10. **No Python** (only the Store alias). Rust is used in hmap-ext. |

### 2.1 How the base heightmap was made (measured, not assumed)

A spectral check of second differences in `dktflls-4096.png` shows a dominant resampling period of **3.79 px** on both axes (25× and 238× the median power; 4.57 px shows nothing). 4096 / 1081 = 3.789. So the base map is the **full 1081-px CS1 export (17.28 km) resampled to 4096 px**, not a centre crop.

Consequences:

- Horizontal factor CS1 → CS2 is **0.8296** (14 336 / 17 280). Every exported CS1 coordinate is multiplied by it. The CS1 border lands on the CS2 playable border, which suits the 7 road, 3 rail, 1 ship and 4 air connections.
- Pixel values were **not rescaled**. CS1 stores 0–1024 m in 16 bits; the lowest river bed in your file is 2392 = 37.4 m on that scale, consistent with CS1's default 40 m sea level and a carved bed. At CS2's default Height Scale of 4096 m the same file reads 150–4096 m, i.e. **4× too tall**, and with the 0.83× squeeze every slope is 4.8× steeper than in CS1.
- The 0.34 % of pixels at 65535 are CS1's 1024 m ceiling. The author used Procedural Objects for peaks above it, so those summits are flat plateaus in your data.

Decision needed (§8): set the editor's Height Scale to **1024 m** (true CS1 heights, slopes 1.2× steeper), about **850 m** (true CS1 slopes, heights 17 % lower), or keep 4096 m. The editor exposes Height Scale from 200 to 10 000 m, so no file change is needed. The choice becomes one constant in the converter.

Verified with the hmap-ext session (2026-10-18): the world map must be regenerated with `extend --height-scale 1024`, because all synthesis constants are in metres and the current output assumed 4× relief. A 1:1 centre crop would **not** keep the CS1 outer ring: hmap-ext locks the inner and synthesizes everything outside it, and a ring-seeding stage would be new work of the size of the trend stage. Details in `setup-plan.md` §D.

### 2.2 What the CS1 map contains

From the Workshop page and the Loading Screen Mod report of the loaded map:

- Fictional "Mississippi through Colorado" map, Map of the Year 2023, 65-item required collection, author MAVER1CK.
- About 740 000 trees, hundreds of custom props and buildings, Procedural Objects for mountains, waterfalls and many bridges, a plopped dam with power lines.
- Connections: road 7, air 4, ship 1, rail 3.
- 552 network prefabs loaded with the map, 499 flagged as used. Classified by name:

| Category | Count | CS2 fate |
|---|---|---|
| Rail and station tracks (R2USA, R69, R0, yard tracks) | 176 | Map to CS2 Train Track variants; dozens of look-alikes collapse to a few prefabs |
| Fences, walls, barriers, guardrails, curbs | 103 | No network equivalent; skip, redo with CS2 fences and decals |
| Highways and interstates (USRP 1L–6L, vanilla Highway, National Road) | 70 | Map to CS2 Highway one-way 1–5 lanes and two-way variants |
| Surface, terraforming and field networks | 55 | Skip (surfaces become CS2 surface areas) |
| Vanilla-style roads (Basic Road, Four-Lane, Busway, Tram, One-Way) | 52 | Map to CS2 Small / Medium / Large roads |
| Parking-lot networks | 34 | Skip; CS2 parking lots are buildings |
| Sidewalks and paths | 20 | Map to CS2 pathways or skip |
| Power lines | 13 | Map to CS2 High Voltage Line where needed |
| Bridge decoration networks (IMT) | 8 | Skip |
| Other | 21 | Case by case |

About 300 of the 552 prefabs are decorative and should not be imported. The transport skeleton is roughly 120 road and highway prefabs plus the rail family, and those collapse to perhaps 15 CS2 prefabs. The mapping table is small, and you will make the dozen judgement calls (asymmetric interstates with exit lanes have no exact CS2 twin).

---

## 3. Ground truth about the CS2 side

| Fact | Value | Source |
|---|---|---|
| Playable heightmap | 4096², 16-bit PNG/TIFF, 14.336 km, 3.5 m/px | CO dev diary 2, CS2 wiki Terrain |
| World map | 4096², 57.344 km, 14 m/px; centre 1024² must equal the playable map | same |
| Height Scale | default 4096 m, editor range 200–10 000 m | CS2 wiki Terrain |
| Sea Level | editor setting 0–2000 m; water comes from placed sources, not the heightmap | CS2 wiki Water |
| Native vector import | **none**. Only heightmaps and per-resource rasters. Roads are drawn or created by a mod. | CS2 wiki Map Creation |
| Outside connections | drawn to the edge with normal tools; one inbound and one outbound road connection required to publish | CS2 wiki Outside Connections |
| Mods in the editor | Image Overlay works in the editor by design; Anarchy and Water Features work, documented as experimental; Earth2Cities "tested and functional in the map editor" per its author; Move It, Road Builder, CS2MCP: unverified | mod pages and READMEs |
| CS1 export | 1081², 16-bit = 0–1024 m (raw/64), 17.28 km, 16 m/px, origin at centre, x,z in ±8640 m, default sea level 40 m | CS1 wiki, Move It source |

### 3.1 The three candidate importers, examined from source or binary

**CityMCP (willmakesrandomshit, GitHub; also PDX Mods 159045).** Created 2026-09-13, 4 commits, 0 stars, no README. The local HTTP server on port 2828 exposes only status, money, time, speed, camera and screenshot. The real command surface (`BuildRoad`, `PlaceBuilding`, terrain brushes, zoning, 39 commands) is reachable only through a WebSocket to a third-party relay at `relay.wilbot.link`, paired by a six-character code, pushing telemetry every 5 s, with no off switch in the relay code. `BuildRoad` creates one isolated straight edge per call (`CreationDefinition` + `NetCourse` + `Updated`, no node attachment, no curve). Verdict: a proof that the recipe works from a mod; not a base to build on.

**CS2MCP (LancerComet/cities-skylines-2-mcp, GitHub, Apache-2.0).** First commit 2026-07-17, last 2026-10-01, 42 stars, 12 forks. A C# bridge mod with a loopback HTTP server on `127.0.0.1:8642` whose requests run on the main thread, a custom `ToolBaseSystem` that goes through the game's native creation pipeline (definitions ported from Line Tool, the Clear → None → Apply handshake, placement validation with the vanilla error icons), and a TypeScript stdio MCP server with 65 tools: `cs2_build_road` (one segment per call, optional single control point `cx,cz`, elevations `e1,e2` clamped to -30…60 m, prefab by exact name, 8–1500 m), `cs2_connect_road` (snaps to existing junctions or splits segments), `cs2_replace_road`, `cs2_upgrade_road`, `cs2_road_graph`, `cs2_terrain`, `cs2_find_prefabs`, `cs2_screenshot`, `cs2_save_game`, and many in-game-only tools. Limits: no batch endpoint, no water sources, no terraforming, handlers are gated by a city lookup that may not exist in the editor (unverified), editor use untested. Verdict: the best base. Fork it, add a batch import endpoint that takes our node-and-edge JSON, make the city gate optional, add water sources, verify in the editor.

**Earth2Cities (PDX Mods 140919, BussianRanana, v0.0.17 of 2026-06-25, 1 019 subscribers, closed source).** Real-world terrain plus OSM road import. From its binary: a browser BBox tool, DEM download, and a road path `overpass_roads.osm → roads.json → OsmRoadToolSystem` with intersection snapping on a grid (`IntersectionSnapCellSizeMeters`, tolerance), segment splitting, boundary-connection margin, optional `SnapElevationToExistingRoad`, curated road profiles mapping OSM highway classes to CS2 prefabs, move/rotate/undo of tagged imported roads, and a documented option "apply prebuilt local terrain heightmaps, then import roads.json on top". Its page states it is tested in the map editor and that imported roads are saved into the map file. Fields in `roads.json` include `roads`, `points`, `lat`, `lon`, `category`, `lanes`, `junction`, `bbox`, `crs`; coordinates are geographic, so the converter would write synthetic lat/lon around a chosen origin. Limits per its author: simplified roundabouts and complex intersections, snapping trouble in mountains, roads only (no rail), alpha. Verdict: the cheapest experiment for the road layer, probably good enough for the highway and arterial skeleton, probably not for interchanges, and a dead end for rail and water.

**Also seen:** `WFMinerva/cs2-real-map-toolkit` (MIT) lays rail in-game with the same recipe and documents the dead ends (nodes do not merge by coordinate, attach with `CoursePos.m_Entity`; `FreeHeight` on an attached end breaks heights; sub-nets need `OwnerDefinition`). `lucarager/CS2-NetworkTools` has a clean 40-line `NetCourseEmitter`. `Ti4goc/GridRoadGenerator` keeps the preview alive six frames for validation. All three are open source and compile against the game.

### 3.2 The creation recipe (what any importer must do)

Verified across five mods: create an entity with `CreationDefinition { m_Prefab, m_Flags }`, `NetCourse { m_Curve, m_StartPosition, m_EndPosition, m_Length, m_Elevation, m_FixedIndex = -1 }` and `Updated`, through `ToolOutputBarrier`, from a `ToolBaseSystem` that is the active tool and drives `applyMode` Clear → None → Apply. `GenerateNodesSystem`, `GenerateEdgesSystem` and `ApplyNetSystem` turn it into permanent nodes and edges that save normally. Attach to existing nodes by setting `CoursePos.m_Entity`; never rely on coordinates. Sample Y from `TerrainSystem.GetHeightData()`. Water sources are created with the Water Features archetype (`WaterSourceData` + `Transform` + `Created`, then `Updated`). The heightmap itself is imported through the editor UI; no mod writes it. Writing a `.cok` directly is out: it is a zip of zstd-compressed versioned ECS streams with no public schema, and one wrong byte gives an unloadable map with no diagnostics.

---

## 4. Paths considered

| Path | Code to write | Fidelity | Main risk | Reuses |
|---|---|---|---|---|
| A. Manual rebuild with Image Overlay from a CS1 render | none | your hand | your time | Image Overlay setup, hmap-ext water helper |
| B1. Export → convert → **Earth2Cities** import | exporter + converter | roads good, interchanges rough, no rail | alpha mod, geographic projection, closed source | E2C already installed |
| B2. Export → convert → **CS2MCP fork** batch import | exporter + converter + bridge changes | high, explicit node graph | editor behaviour of the bridge; a few days of pipeline work | CS2MCP (Apache-2.0), WFMinerva lessons, your toolchain |
| B3. Export → convert → **own importer from scratch** | more | same as B2 | same as B2 plus rewriting what CS2MCP already has | NetworkTools emitter |
| C. **MCP-driven interactive** recreation | small, on top of B2 | high for fixes, hopeless for bulk | LLM cost per segment | CS2MCP's server |
| D. Write the `.cok` directly | very high | n/a | unloadable map | none |
| E. OSM as the intermediate format | medium | medium | the only CS2 OSM consumer is E2C | none beyond B1 |
| F. "REA" | unknown | | | I could not identify it; see §8 |

Path E collapses into B1: there is no CS1 → OSM exporter that still works, and no CS2 OSM consumer other than Earth2Cities, so "OSM" just means writing E2C's input file ourselves. CS1's own node graph is richer than OSM ways anyway (explicit nodes, Bézier control points, elevation per node), so our interchange format should be our own JSON, with an OSM/roads.json writer as one output among several.

---

## 5. Recommended plan

**Phase 0, decisions (you):** height scale; keep the full-extent squeeze (recommended, it is baked into the world map and puts the connections on the border) or re-import a centre crop; map only or also the `dktanew` city; what REA is.

**Phase 1, CS1 exporter (1–2 days).** Clone the TrainConverter exporter into a `DakotaExport` CS1 mod that runs under `LoadMode.LoadMap` in the CS1 map editor. For each segment: prefab name, start/end node ids, positions, Bézier control points from `NetSegment.CalculateMiddlePoints`, node elevation, underground and outside flags. Plus water sources from `TerrainManager.WaterSimulation`, outside-connection buildings, plopped buildings (the dam), and trees and props as a density raster rather than 740 000 points. Output `dakota-cs1.json` plus a quick SVG so you can see the result before anything touches CS2. (ModTools' in-game C# script editor is the no-compile alternative; it is not installed, and the compiled mod is more repeatable.)

**Phase 2, converter (1–2 days, Rust next to hmap-ext or a .NET console).** Multiply x/z by 0.8296, convert heights by the chosen height scale, apply the CS1 → CS2 prefab table (seeded from §2.2, "skip" for decorative classes), merge coincident nodes, simplify, classify segments as ground/elevated/tunnel against `dktflls-4096.png`, and write: `import.json` for the bridge, `roads.json` for Earth2Cities, GeoJSON for QGIS or a browser viewer, and a 4096-px overlay PNG for Image Overlay. Produce the overlay first so Path A exists from day one.

**Phase 3a, Earth2Cities experiment (half a day).** Enable it in a dedicated editor playset, feed it the generated `roads.json` on top of your terrain, judge the highways and arterials. If it is good enough for the road skeleton, keep it for that layer and move on.

**Phase 3b, CS2MCP fork (3–5 days), in milestones:**

1. Bridge builds and answers `cs2_ping` inside the map editor; city-dependent handlers skipped there.
2. Ten connected highway segments from a hand-written `import.json`, visible, saved, reloaded.
3. Elevated, bridge and tunnel segments; node merging across a corridor using explicit node entities.
4. Full import by layer (highways, rail, main roads) with undo-by-layer.
5. Water sources (Water Features archetype) and outside connections stretched to the border.
6. The existing MCP server now gives you the interactive layer for free (Path C): road graph queries, terrain samples, screenshots, connect/replace/upgrade for fix-ups.

**Phase 4, editor work (manual, overlay-guided):** tree brushing with random pools, peaks above CS1's 1024 m ceiling, waterfalls and bridges that were Procedural Objects, dam and landmarks, surfaces.

**Phase 5, world map polish:** finish hmap-ext detail and erosion so the exterior stops reading as smooth lobes.

---

## 6. Risks and unknowns

- **Editor behaviour of the CS2MCP bridge.** Its handlers look up the city entity; the editor may have none. Milestone 1 settles it.
- **Node connection semantics.** Consecutive courses must share node entities; coordinate overlap is not enough (WFMinerva, confirmed in three mods). Our exporter gives us the node graph, which is exactly what is needed.
- **Earth2Cities internals.** Closed source, alpha, geographic coordinates with its own projection; expect to calibrate the origin and scale with a synthetic test file before trusting it.
- **Prefab mapping judgement.** Asymmetric interstates and exit lanes have no exact twin; 10–20 % of segments will need a manual retouch.
- **Editor stability with 685 mods.** Use a dedicated editor playset: Anarchy, Image Overlay, Water Features, Scene Explorer, the importer, nothing else.
- **Procedural Objects content** (mountains above 1024 m, waterfalls, bridges) is in PO's own data, not in any manager; it is recreated by hand.
- **Trees.** 740 000 individual trees is beyond comfortable; a density raster and brushes are the substitute.
- **Height scale.** Left at 4096 m, every imported bridge and ramp inherits 4× vertical exaggeration.

---

## 7. What I would build first

The CS1 exporter: cheap, reuses your project, zero risk to the CS2 side, and its JSON immediately yields the Image Overlay PNG and the Earth2Cities test file. The CS2MCP fork's milestone 1 (bridge alive in the editor) can start in parallel because it needs no data.

---

## 8. Questions for you

1. What is "REA"? No CS1/CS2 tool, mod or agent by that name turned up in several targeted searches.
2. Which Height Scale is `dktflls.cok` using today, and which do you want (1024 m recommended)?
3. Keep the full-extent squeeze (0.8296) or re-import a 1:1 centre crop?
4. Map only, or also the city from `dktanew.crp`?

---

## 9. Appendix: sources

- CO Modding Dev Diary 2, Map Editor: https://www.paradoxinteractive.com/games/cities-skylines-ii/modding/dev-diary-2-map-editor
- CS2 wiki, Map Creation (Terrain, Water, Outside Connections, Settings and Polish, Sharing): https://cs2.paradoxwikis.com/Map_Creation
- CS2 wiki, Modding Toolchain, Launch Parameters, Developer mode: https://cs2.paradoxwikis.com/Modding_Toolchain
- CS1 wiki, Map Editor and Heightmap: https://skylines.paradoxwikis.com/Map_Editor , https://skylines.paradoxwikis.com/Heightmap
- CS2MCP: https://github.com/LancerComet/cities-skylines-2-mcp
- CityMCP: https://github.com/willmakesrandomshit/CityMCP
- Earth2Cities: https://mods.paradoxplaza.com/mods/140919/Windows
- cs2-real-map-toolkit (lessons on the net pipeline): https://github.com/WFMinerva/cs2-real-map-toolkit
- CS2-NetworkTools (NetCourseEmitter): https://github.com/lucarager/CS2-NetworkTools
- GridRoadGenerator: https://github.com/Ti4goc/GridRoadGenerator
- Line Tool (definitions code): https://github.com/algernon-A/LineTool-CS2
- Water Features (water source archetype, editor tool): https://github.com/yenyang/Water_Features
- Anarchy (NetworkDefinitionSystem): https://github.com/yenyang/Anarchy
- Image Overlay: https://github.com/algernon-A/ImageOverlay
- Scene Explorer: https://github.com/krzychu124/SceneExplorer
- Move It CS1 (field names, export format): https://github.com/Quboid/CS-MoveIt
- ModTools (in-game C# scripting): https://github.com/bloodypenguin/Skylines-ModTools
- CSL Map View and converters: https://github.com/gansaku/CSLMapView , https://github.com/delick/cslmap_converter
- CS2 OSM Toolkit (overlay rendering only): https://github.com/Osyanne/cs2-minneapolis-osm-toolkit
- Cities2-MCP (docs and scaffolding only): https://github.com/mayor-modder/Cities2-MCP
- Dakota Falls Workshop page: https://steamcommunity.com/sharedfiles/filedetails/?id=2972879554
- Local evidence: `analysis-assets/heightmap-analyze.cs`, `analysis-assets/heightmap-resample-period.cs`, previews in `analysis-assets/`.
