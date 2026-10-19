# Next test run: Dakota Falls at Height Scale 2048 and 4096

Draft 2026-10-19. Sections marked **[pending]** fill in when the worker sessions report
(bridge 0.9.7, converter round 5, exporter v2, hm-rebuild v2, hmap-ext v2 world map).

Both runs use the **same heightmap pair**. Only the Height Scale field and the import file set differ. Run A (2048, true heights) is the main deliverable; run B (4096) is the experiment (owner, via the CS2-side audit session, 2026-10-19). Bridge 0.9.10 adds the `wall` edge option for the 21 flat quays; test one edge first.

| | Run A | Run B |
|---|---|---|
| Height Scale on import | **2048** | **4096** |
| Playable heightmap | **`10190615-dktflls-1to1.png`** (peaks v2 + Procedural Object mountains and rock work, max 1 457 m; optional `10190615-dktflls-1to1-falls.png` adds the falls steps, which may dam the river; `10190618-dktflls-1to1-soft.png` is the softened variant to compare: blurred facets plus rock noise); fallbacks `10190442` (peaks only), `10181339` (v1) | same |
| World map | **`10190616-dktflls-1to1_worldmap.png`** (hmap-ext, built from 10190615; seam 46 m max vs 74 m inside; west range 1 540–1 998 m; a rerun with tickets 21–23 will get a newer prefix) | same |
| Import file set | `net-converter/out/x1/` | `net-converter/out/` |
| Sea Level (Water tab) | **97.5 m** | **195 m** |
| Heights in game | true CS1 metres | 2 × CS1 |
| Save name | `dakota-A-2048` | `dakota-B-4096` |

## Before the game starts (you)

1. CS1: load Dakota Falls in the map editor and run the DakotaExport once: **v0.4.2 is installed and is the final version** (schema v5: water surface, Procedural Objects with meshes and Effects Module data incl. failed objects, terrain surfaces, tree height/scale, Network Skins, extra height grids, flagged buildings, raw data dump). Load the **Workshop map** in the map editor, dismiss PO's loading-failure popup **without deleting**, export, **do not save the map**. Tell the orchestrator the new file name.
2. File naming: `<MMddHHmm>-dktflls-1to1*.png`, the prefix is the production time; a world map states which inner it was built from in its session report. The pair above is in place. Confirm the files exist in `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Heightmaps`.
3. Playset DakotaFalls-E2C: CS2MCP (local, **0.9.9 deployed** 2026-10-19; needs the game restart) plus the **14 asset packs** from `cs2-playset/ASSET_PACKS.md` added in Skyve (RZ Rocks, Arid Rocks, Cliffs and Rocks, pd Long Grass, Styx pack, Ferns, Leafy Undergrowth ×2, WildFlower, Bloom, Dirty Surfaces, Terrain Textures ×3). Without them the grass plants, pack rocks and decal surfaces fail the dry run, Anarchy, Scene Explorer, Image Overlay (optional, for `overlay-4096.png`).

## Per run (you, in the editor)

1. Map → New. Terrain tab → Import Heightmap: set **Height Scale** first (2048 for A, 4096 for B), then pick the playable file, then Import World Map with the `_worldmap` file.
2. Water tab → Sea Level: 97.5 (A) or 195 (B) → Apply. (A new map defaults to 511.7 m, which floods everything.)
3. Open the road tool once and turn the **Anarchy toggle on** (highlighted). Leave the tool open during imports or rely on `bypassValidation`.
4. Say "Run A ready" or "Run B ready" in the orchestrator session.

## Per run (orchestrator, through the bridge)

Order (converter `out/ROUND6.md`): network → power, walls, ship paths → water → connections → objects and barrier props → trees → resources → surfaces (fields, decals; grass surfaces only if the grass plants are rejected). Each layer is a separate import name so it can be undone alone. Every layer: dry run first, then real, then the live-graph defect scan where it applies.

1. `cs2_ping`: version 0.9.7, mode Editor. `cs2_terrain`: heightScale 2048 or 4096, min ≈ 38 m (A) / 76 m (B).
2. Network: `import-all.json` (round 5: 4 025 edges, 3 866 nodes, heightMode absolute; `import-retry.json` is for the old v8 map only), options `nodesFirst`, `bypassValidation: true`. Expect ≈ 4 000 edges created+merged, 0 rejected batches.
3. Power, walls, ship paths via `cs2_import_network`: `import-power.json` (60 edges, `High-voltage Line`), `import-walls.json` (113 edges: QuaySmall01 90, QuayMedium01 7, Medium Boatway 14 for canals, Sound Barrier 2), `import-shippaths.json` (100 edges of `Medium Seaway`, ending on the border so the game can tag the ship outside connection).
4. Water: `import-water.json` sources via `cs2_import_water` (65 natural sources; dry run, then real). Tune by eye afterwards.
5. Outside connections: `import-connections.json` via `cs2_import_objects` (28 entries: 19 road + 4 rail are **verification only**, the game tags border nodes itself; 4 air + 1 ship are placed on the border facing outward). Any `failed` road/rail entry means that net does not end exactly on ±7168.
6. Objects: first the rotation test (one asymmetric building at rotation 0 and one at 90; 0 must face north, 90 east), then `import-objects.json` (6 130: 3 038 props incl. pack and vanilla rocks, 3 076 grass plants from the packs, 7 buildings incl. ruins/wreck/factory stand-ins) and `import-walls-props.json` (621 barrier and fence props). The dams and the two CS1 bridge models stay in `out/manual-objects.md` for hand placement.
7. Trees: the bridge's 2 000-tree timing test first, then `out/trees/import-trees-01..11.json` (508 458 trees: pine/spruce 369 k, birch/oak/linden/hickory 98 k, wild bushes 41 k; shared by both runs). Expect roughly 20 s per 50 000 at the test rate; adjust `batchSize`.
8. Resources: `import-resources.json` once the v3 dump exists, else `import-resources-synthetic.json` (fertility from the cornfields only); read back with `cs2_map_resources` (fertility, ore, oil; forest is ignored by the game, fish follows water).
8b. Surfaces via `cs2_import_surfaces` (pack textures per audit/theme.md; `import-surfaces-rock.json` automatic rock pass on steep slopes is optional, judge it in game): `import-surfaces-fields.json` (67 Agriculture Surface polygons), `import-surfaces-decals.json` (128 Dirty Surfaces Pack polygons for the CS1 dirt/mud decals); `import-surfaces-grass.json` (53) only as the fallback for grass.
8c. Optional, TM:PE lane connections → Traffic (PDX 80095, pinned 0.2.13): only with the Traffic mod in the playset; `cs2_import_tmpe` dry run, import, check junctions in Traffic's Lane Connector, save, reload, check again (bridge 0.9.11, untested; data `cs1-offline/out/tmpe-offline.json`).
9. Map settings: `cs2_map_settings` sets theme North American, climate, then latitude 40.0 / longitude −105.3, sea level, height scale, left-hand traffic off, camera start. **Climate: try both** on the same map (owner decision): the River Valley / Colorado climate with winter snow, and a milder no-snow climate that keeps CS1's summer look; read the exact climate prefab names from the editor first (audit/theme.md), switch with `cs2_map_settings`, and judge in game before saving. By hand per `cs2mcp/MAP_SETTINGS.md`: map name, description, preview, starting tiles, content prerequisites.
10. `pull-live-graph.mjs` + `defects.mjs` → `out/live-defects.md`; screenshots of the known trouble spots (river bridge, e15883, tunnel transitions).
11. Save as `dakota-A-2048` / `dakota-B-4096`.

## Review checklist (you)

- Water: lower river at sea level, upper reaches fed by sources, no flooding of the interchanges.
- Power lines follow the CS1 corridor and connect to the dam / substation positions listed in `manual-objects.md`.
- Every road and rail that reaches the border has an outside connection; 4 air + 1 ship exist.
- Tree density reads like CS1 (dense pine on the western slopes, oaks along the river, open plains east).
- Rocks sit on the slopes they came from; landmarks oriented correctly.
- Resources visible in the editor's resource overlay where CS1 had cornfields.
- Peaks: no flat 1024 m plateaus left; no seam at the playable/world-map edge.
