# Next test run: Dakota Falls at Height Scale 2048 and 4096

Draft 2026-10-19. Sections marked **[pending]** fill in when the worker sessions report
(bridge 0.9.7, converter round 5, exporter v2, hm-rebuild v2, hmap-ext v2 world map).

Both runs use the **same heightmap pair**. Only the Height Scale field and the import file set differ. Run A (2048, true heights) is the main deliverable; run B (4096) is the experiment (owner, via the CS2-side audit session, 2026-10-19). Bridge 0.9.10 adds the `wall` edge option for the 21 flat quays; test one edge first.

| | Run A | Run B |
|---|---|---|
| Height Scale on import | **2048** | **4096** |
| Playable heightmap | **`10190843-dktflls-1to1.png`** (A-v3: peaks + PO mountains/rock work + falls steps capped at the CS1 water line); comparison `10190843-dktflls-1to1-soft.png` (softened mountains, pair it with world map 10190910) | same |
| World map | **ticket-28 successor from hmap-ext when it lands (flats + gullies); until then `10200633-dktflls-1to1_worldmap.png`** (Import World Map can be redone later on the same map without touching the playable area) (hmap-ext f2e2e35, tickets 01–27: massifs, 8 hill groups, creek network cutting ~21 m, 29–33 % flats; heavier north foothills); `10200636-dktflls-1to1_worldmap.png` is the same with the lighter north of 10191247; soft variant `10200635-dktflls-1to1-soft_worldmap.png` | same |
| Import file set | `net-converter/out/x1/` | `net-converter/out/` |
| Sea Level (Water tab) | **93 m** first (measured CS1 lower river 92.8 m); 97.5 m if banks look dry | **186 m** first; 195 m fallback |
| Heights in game | true CS1 metres | 2 × CS1 |
| Save name | `dakota-A6-2048` (A-v6, 2026-10-20) | `dakota-B-4096` |

## Before the game starts (you)

1. CS1: load Dakota Falls in the map editor and run the DakotaExport once: **v0.4.2 is installed and is the final version** (schema v5: water surface, Procedural Objects with meshes and Effects Module data incl. failed objects, terrain surfaces, tree height/scale, Network Skins, extra height grids, flagged buildings, raw data dump). Load the **Workshop map** in the map editor, dismiss PO's loading-failure popup **without deleting**, export, **do not save the map**. Tell the orchestrator the new file name.
2. File naming: `<MMddHHmm>-dktflls-1to1*.png`, the prefix is the production time; a world map states which inner it was built from in its session report. The pair above is in place. Confirm the files exist in `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Heightmaps`.
3. Playset DakotaFalls-E2C: CS2MCP (local, **0.9.9 deployed** 2026-10-19; needs the game restart) plus the **14 asset packs** from `cs2-playset/ASSET_PACKS.md` added in Skyve (RZ Rocks, Arid Rocks, Cliffs and Rocks, pd Long Grass, Styx pack, Ferns, Leafy Undergrowth ×2, WildFlower, Bloom, Dirty Surfaces, Terrain Textures ×3). Without them the grass plants, pack rocks and decal surfaces fail the dry run, Anarchy, Scene Explorer, Image Overlay (optional, for `overlay-4096.png`).

## Per run (you, in the editor)

1. Map → New. Terrain tab → Import Heightmap: set **Height Scale** first (2048 for A, 4096 for B), then pick the playable file, then Import World Map with the `_worldmap` file.
2. Water tab → Sea Level: 97.5 (A) or 195 (B) → Apply. (A new map defaults to 511.7 m, which floods everything.)
3. Open the road tool once and turn the **Anarchy toggle on** (highlighted), or rely on `bypassValidation` (A-v5 ran without the toggle, no losses).
3b. **Unpause the editor simulation** (play button bottom-left) before judging water: sources only flow while the clock runs; A-v5 showed 0 wet cells on the streams until it was unpaused, 290 within a minute. Let it run a few minutes and save again, since the water state is saved.
4. Say "Run A ready" or "Run B ready" in the orchestrator session.

## Per run (orchestrator, through the bridge)

**One command (A-v6):** `node net-converter/tools/import-run.mjs --scale x1 --sea 93 --po-structures --with po-gantry` (run B: `--scale x2 --sea 186`; climate comes from `look/climate.json`, override with `--climate`). Order since A-v5: surfaces first, then nets (network, gaps, bridges, dam, power, canals, ship paths), then water, connections, objects, barriers, station, cliffs, scatter, fort, trees, resources, power ends, settings, look. The runner stops at the first step whose `netGuard.segmentsRemoved` is not 0 and prints `/net/count` after the nets and at the end.

**A-v6 (2026-10-20):** bridge **0.9.26** must be the running build (closed surface polygons, so the surface step no longer deletes nets; Traffic-style TM:PE pre-validation) — restart the game after Skyve shows it. Converter round 19 + 20 files in `out/x1`: footpaths and dam walkway gone, organic sand/dry-grass polygons and true field circles, straightened BR005/BR009 approaches, interchange head-on joins and the (−4224,−60) crossing restored, nine low bridges at 8.5 m, BR007/BR010 single truss-arch edges, 304 m dam edge with `flow: invert`. `--po-structures` places the 72 PO structure objects (BR006 StoneArchRailBridge + ConcreteSet01 columns/piers, BR022 pads; mods 154132 + 135944 in the playset since 2026-10-20) after po-rocks; `--with po-gantry` adds the one Truss Gantry (132188). `po-piers` (ITBP 140978) stays off until `noPillars` is verified on BR001/BR003. The look step re-posts `look/climate.json` and the corrected `look/water.json`. After the save: `node cs2mcp/scripts/import-tmpe.mjs --dry-run --body net-converter/out/x1/import-all.json`, then the real import, save again as `dakota-A6b-2048`.

**A-v5 acceptance (bridge round 24):** segment counts per prefab must match (a) right after the net steps, (b) after 2 minutes of idle editor time, (c) after save + reload (`node net-converter/tools/import-run.mjs --verify-only`). Save under a name with no leading or trailing space. It runs every layer below in order, prints one line per layer, stops on a body error and writes `out/<scale>/run-<timestamp>.md` with every importId. The numbered list stays as the reference and for manual re-runs of single layers.

Order (converter `out/ROUND6.md`): network → power, walls, ship paths → water → connections → objects and barrier props → trees → resources → surfaces (fields, decals; grass surfaces only if the grass plants are rejected). Each layer is a separate import name so it can be undone alone. Every layer: dry run first, then real, then the live-graph defect scan where it applies.

1. `cs2_ping`: version 0.9.7, mode Editor. `cs2_terrain`: heightScale 2048 or 4096, min ≈ 38 m (A) / 76 m (B).
2. Network: `import-all.json` (4 072 edges, 3 915 nodes, heightMode absolute), options `nodesFirst`, `bypassValidation: true`. Expect ≈ 4 000 edges created+merged, 0 rejected batches.
3. Power, walls, ship paths via `cs2_import_network`: `import-power.json` (60 edges, `High-voltage Line`), `import-walls.json` (113 edges: QuaySmall01 90, QuayMedium01 7, Medium Boatway 14 for canals, Sound Barrier 2), `import-shippaths.json` (100 edges of `Medium Seaway`, ending on the border so the game can tag the ship outside connection).
4. Water: `import-water-sea93.json` (A) / `import-water-sea186.json` (B) via `cs2_import_water` (41 natural sources after the audit rule; dry run, then real); the 97.5/195 files are the fallback pair. `water-check.md` has the measured CS1 surface per source.
5. Outside connections: `import-connections.json` via `cs2_import_objects` (25 entries: road + rail are **verification only**, the game tags border nodes itself; 4 air + 1 ship are placed on the border facing outward). Any `failed` road/rail entry means that net does not end exactly on ±7168.
6. Objects: first the rotation test (one asymmetric building at rotation 0 and one at 90; 0 must face north, 90 east), then `import-objects.json` (6 130: 3 038 props incl. pack and vanilla rocks, 3 076 grass plants from the packs, 7 buildings incl. ruins/wreck/factory stand-ins) and `import-walls-props.json` (621 barrier and fence props). The dams and the two CS1 bridge models stay in `out/manual-objects.md` for hand placement.
7. Trees: the bridge's 2 000-tree timing test first, then `out/trees/import-trees-01..10.json` (496 318 trees from the exact CS1 points, water and net corridors masked; shared by both runs) plus the per-scale `import-trees-fixed-01.json` (50 fixed-height trees with y). Expect roughly 20 s per 50 000 at the test rate; adjust `batchSize`.
8. Resources: `import-resources.json` (real CS1 layers: fertility, ore, oil, groundwater); read back with `cs2_map_resources`.
8b. Surfaces via `cs2_import_surfaces` (pack textures per audit/theme.md): `import-surfaces-sand.json` (479 polygons from CS1's painted sand), `import-surfaces-fields.json` (56), `import-surfaces-decals.json` (41); optional: `import-surfaces-rock.json` (151, slope ≥ 40°), `import-surfaces-grass.json` (51, only if the grass plants are rejected); `-unmerged` files are experiments. `import-surfaces-fields.json` (67 Agriculture Surface polygons), `import-surfaces-decals.json` (128 Dirty Surfaces Pack polygons for the CS1 dirt/mud decals); `import-surfaces-grass.json` (53) only as the fallback for grass.
9. Map settings: `cs2_map_settings` sets theme North American, climate, then latitude 40.0 / longitude −105.3, sea level, height scale, left-hand traffic off, camera start. **Climate: try both** on the same map (owner decision): the River Valley / Colorado climate with winter snow, and a milder no-snow climate that keeps CS1's summer look; read the exact climate prefab names from the editor first (audit/theme.md), switch with `cs2_map_settings`, and judge in game before saving. By hand per `cs2mcp/MAP_SETTINGS.md`: map name, description, preview, starting tiles, content prerequisites.
10. `pull-live-graph.mjs` + `defects.mjs` → `out/live-defects.md`; screenshots of the known trouble spots (river bridge, e15883, tunnel transitions).
11. Save as `dakota-A-2048` / `dakota-B-4096`. **No leading or trailing space in the name**: a map saved as " dakota-A4-2048" froze on load until the name was fixed inside the file.
12. **Last, after the save:** TM:PE lane connections → Traffic (PDX 80095, pinned 0.2.13; the mod is in the playset): `cs2_import_tmpe` dry run, import, check junctions in Traffic's Lane Connector, save again under a new name, reload, check again (bridge 0.9.11, untested; data `cs1-offline/out/tmpe-offline.json`).

## Review checklist (you)

- Water: lower river at sea level, upper reaches fed by sources, no flooding of the interchanges.
- Power lines follow the CS1 corridor and connect to the dam / substation positions listed in `manual-objects.md`.
- Every road and rail that reaches the border has an outside connection; 4 air + 1 ship exist.
- Tree density reads like CS1 (dense pine on the western slopes, oaks along the river, open plains east).
- Rocks sit on the slopes they came from; landmarks oriented correctly.
- Resources visible in the editor's resource overlay where CS1 had cornfields.
- Peaks: no flat 1024 m plateaus left; no seam at the playable/world-map edge.
