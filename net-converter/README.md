# net-converter

Turns a DakotaExport dump (`dakota-export/cs1/v1`, see `../cs1-exporter/SCHEMA.md`) into batch-import files for the CS2MCP
`POST /import/network` endpoint (`../cs2mcp/IMPORT_SCHEMA.md`), plus water sources, outside connections, a GeoJSON and an
Image Overlay PNG.

## Run

```bash
dotnet run -c Release --project src/NetConverter -- "%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json" --mapping mapping.tsv --out out
```

With no dump argument, it uses the newest `*.json` in `%LOCALAPPDATA%\DakotaExport`. `--inventory` prints only the dump inventory.

```bash
dotnet test
```

## Outputs (`out/`)

| File | Content |
|---|---|
| `import-highways.json`, `import-rail.json`, `import-roads.json` | One import body per layer: `nodes {id,x,y,z,elevation}`, `edges {id,a,b,prefab,control,elevation}`. Extra fields (`structure`, `cs1_prefab`, `cs1_segment`, `border`) are ignored by the importer. Import highways first, then rail, then roads. Run each with `dryRun: true` first. |
| `water-sources.json` | 65 CS1 sources: position, height (target surface, m), type, rate, `inPlayable`. |
| `outside-connections.json` | `borderNodes`: network ends cut at ±7168 m (the CS2 outside connections). `cs1Buildings`: the CS1 connection buildings with the nearest border node of their kind. |
| `network.geojson` | Every edge of every layer (incl. power and skipped), CS2 metres `[x, z]`, with `cs1_prefab`, `cs2_prefab`, `layer`, `structure`, `elev_a/elev_b`. |
| `overlay-4096.png` | 4096² transparent PNG at 3.5 m/px covering ±7168 m, north up. Orange = highways (light orange = ramps), white = Small Road, yellow = Medium Road, tan = Gravel Road, purple = rail, cyan = power lines. |
| `stats.md` | Per-layer counts, CS2 prefabs used, unmapped prefabs, flagged decisions, full mapping table, dump inventory. |

## Transform rules

- x and z unchanged (factor 1.0), y = CS1 world height in metres (Height Scale 2048 m, true metres).
- Mapping: `mapping.tsv`, regex on the CS1 prefab name, first match wins, target `SKIP` or an exact CS2 prefab name.
  CS2 names were checked against `Assets.NAME[...]` keys in the game's `Locale.cok`
  (`Double Train Track`, `Twoway Train Track`, `Oneway Train Track`, `Gravel Road`, `Highway Oneway - N lanes`, …).
  A prefab no rule matches goes to layer `unmapped`. It is not imported. stats.md and the console list it with a heuristic suggestion.
- Clipping: each Bézier is split where it crosses the ±7168 m square (sampling plus bisection, de Casteljau sub-curve). The cut
  node is snapped exactly onto the border. Segments wholly outside are dropped.
- Nodes closer than 0.5 m (within one layer) are merged. Border nodes win, then original CS1 nodes.
- Edges shorter than 4 m are dropped by **collapsing** their two ends into one node, so the graph stays connected (moves ≤ 4 m).
- Edges over 1500 m (importer limit) are split at t = 0.5. The Dakota dump has none.
- Structure comes from the CS1 AI: `*TunnelAI` = tunnel (the USRP/R2 `S` slope and `T` tunnel variants), `*BridgeAI` = elevated, else ground.
  A node touching a bridge or tunnel edge gets `elevation = y − terrain` (current CS1 terrain), snapped to 0 within ±0.5 m.
  Ground-only nodes are 0. Edge `elevation` = `[elev(a), elev(b)]`.
- One-way networks run `a → b` in traffic direction (CS1 `invert` flag honoured).

## Round 2: reconciliation with the live game

```bash
node tools/pull-live-graph.mjs out/live-graph.json
```

```bash
dotnet run -c Release --project src/NetConverter -- "%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json" --mapping mapping.tsv --out out --live out/live-graph.json --baseline out/round1
```

- `tools/pull-live-graph.mjs` tiles ±7368 m with `/city/road-graph?nets=all` (radius ≤ 2000, limit 3000, splits a tile when it is full)
  and writes every live segment and node.
- `out/reconcile.md`: coverage per layer and prefab, before fixes (`out/round1`, the files that were imported) and after.
  Each import edge is sampled at t = 0.1, 0.3, 0.5, 0.7, 0.9. A point counts when it is within 6 m of a live segment. A live
  segment is drawn as the import curve whose ends coincide with it, else as its chord, with the tolerance widened by the sagitta that
  length vs chord implies (the bridge reports no control points).
- `out/import-<layer>-retry.json`: only missing edges (or partial with ≤ 2 of 5 points). A node within 1 m of a live node whose
  segments carry a prefab of the same layer gets `entity {index, version}`.
- Hairpin check: length > 1.6 × chord, self-intersection or a control point behind its end. The control points are then rebuilt from the
  dump's start/end directions, swapped, else straight. Logged in stats.md and reconcile.md.
- Structure (ground/elevated/tunnel) comes from node elevation. A node leaves the ground when its CS1 `m_elevation` > 0, it is
  Underground, or it touches a B/E/S/T piece. USRP B/E/S/T suffixes map to the base prefab. `+ Exit Lane` maps to N+1 lanes and `+ 2 Exit Lanes` to N+2.
- Ramp pass (`--ramp-delta 6 --ramp-length 60`, max grade 8 %) raises the low end and the following degree-2 ground chain to
  `e_high − 0.08·s`.
- Exact duplicates (same end nodes, curves ≤ 1 m apart) are dropped. Same-layer pairs < 4 m apart (horizontal and vertical) for > 20 m,
  and edges leaving a node at < 10°, are listed in reconcile.md.

## Round 3: dead ends and near-parallel pairs

- **Import `out/import-all.json`** (all three layers in one body, 3 546 edges, 2.1 MB). It is the only file where junctions between layers
  (8 highway↔Medium Road nodes, 3 rail↔road level crossings) share one node id. The per-layer files stay for partial imports,
  but they leave those junctions as dead ends.
- `out/deadends.md`: every end more than 60 m inside the border, with the CS1 segments meeting there and the converter's decision for
  each (`ConvertResult.Fate`: imported, imported (clipped), SKIP, power, unmapped, outside the border, collapsed (< 4 m), degenerate after merge,
  duplicate, duplicate transition piece). It covers live ends (from `out/live-graph.json`) and the ends predicted for `import-all.json`.
- Node merge (< 0.5 m) is refused when the heights differ by ≥ 2 m. Stacked crossings were being fused into fake junctions.
- Near-parallel pairs (< 10° at a shared node): if the far ends coincide, the pair is a duplicate transition and one is dropped.
  Otherwise it is a diverge/merge, and the tangents are rotated apart to 12° (`MinDivergeAngle`). A through edge keeps its tangent and the ramp turns alone.
- Retry files are written only with `--retry`, because entity ids are valid only in the game session they came from.
- The 4 m collapse stays: the importer rejects edges under 4 m. A collapsed lane-drop piece becomes a prefab change at the merged node.

## Round 4: failed-edge retry, CS2 terrain, deck fit

```bash
curl -s "http://127.0.0.1:8642/import/report?status=failed&limit=200" -o out/import-report-failed.json
```

```bash
node tools/pull-live-graph.mjs out/live-graph.json
```

```bash
dotnet run -c Release --project src/NetConverter -- "%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json" --mapping mapping.tsv --out out --heightmap ../hm-rebuild/out/DakotaFalls-1to1-4096.png --retry-report out/import-report-failed.json
```

- `--heightmap`: the CS2 playable heightmap from hm-rebuild (value / 32 = m). Row order is detected against the CS1 node terrain
  (Dakota: row 0 = north, mean error 0.01 m). Node elevations are then measured against CS2 terrain, so border cuts and other
  interpolated nodes get true heights.
- Deck fit: the importer sets the inner control-point heights to terrain + interpolated end elevation, which bends a flat deck down into
  a canyon (SteepSlope, NotEnoughClearance, OverlapExisting on the canyon viaduct). Raised and sunk courses are split at t = 0.5 until the
  predicted heights are within 2 m of the CS1 deck, or the pieces would be shorter than 15 m. Dakota: 115 courses become 274 pieces.
- `--retry-report`: writes `out/import-retry.json` with the current pieces of every CS1 segment that failed. Nodes are bound to live
  entities (within 1 m, same-layer prefab) when `out/live-graph.json` has segments. Otherwise they are left unbound, so pull the live graph first.
  An empty live graph skips reconcile and dead-end reports.
- `tools/failed-geometry.mjs <report> <import>`: chord, length/chord, end tangents and elevations of failed courses.
- `tools/classify-failures.mjs <report> <import> out/failures.md`: what each failed course touches in the same body (approximate CS2 widths):
  parallel diverge, duplicate, tight parallel, stacked parallel, crossing (Δy at the crossing), or no contact.

## Round 5: grade separation, bridge profiles, fills, water

- Crossings between layers without a shared node: where CS1 had ≥ 4 m between the decks (`GradeSeparation`), the upper course gets a node
  at the crossing at its CS1 deck height. The node is at least `--min-bridge-elevation` (default 8 m) above terrain and the lower course. It
  is pinned, and the ramp walk spreads it (8 % roads, 4 % track). Crossings at grade in CS1 are listed in `out/round4.md`.
- Bridge profiles: along chains of CS1 bridge/elevated pieces, no node sits more than 1.5 m off its neighbours' chord. The ends are fixed and pinned nodes are kept.
- Twisted-course check (control point behind an end, or self-intersection): swap the controls, else make the course straight.
  Tiny stubs (< 10 m, from a junction to a dead end) are collapsed. Highway dead ends within 30 m of a course they run along are snapped onto it.
- `out/import-water.json`: `{seaLevel, sources:[{id,x,z,height,radius,type,rate,inPlayable,belowSeaLevel}]}`, radius = clamp(√rate, 20, 150) m.
  `--sea-level 97.5` sets the flag.
- `tools/live-defects.mjs out/live-graph.json`: lists live gaps (dead ends facing each other within 250 m), dead ends within 100 m of another layer,
  and tails beside a same-layer segment.
- `tools/write-fills.mjs <body> <live> out/import-retry.json e123_0 e456_0:liveA:liveB`: writes whole courses, or the part of a course between two live
  nodes, bound to live entities.
- `tools/pull-live-graph.mjs` no longer overwrites the file when the bridge returns no segments.

## Round 6: CS2 heights = 2 × CS1, absolute output, tunnel transitions

- The CS2 map is the 2048 m heightmap at Height Scale 4096, so every CS2 height is 2 × CS1. `--height-factor 2` (default) scales y, terrain,
  Béziers and water heights when the dump loads. `--height-factor 1` gives a 1:1 map. All passes then work in CS2 metres.
- `--height-mode absolute` (default): every node has `y` = terrain + elevation. Ground nodes also carry `elevation: 0`. Raised and sunk nodes get their height
  from `y`, and edges carry no `elevation`. `--height-mode elevation` restores the old form.
- Deck smoothing: grade flips above 1 % along bridge/viaduct chains are removed. Single humps (overpasses) and arches stay, and so do crossing nodes (pinned).
- Transitions: no node in (−8, 0) m. A shallow node next to the surface is the portal (0), and other shallow nodes go to −12 m. Walking in from a portal,
  depth grows at most 15 % per metre (but at least 8 m). No deck node in (0, 2) m beside a raised node. Deck fit skips tunnel courses.
- `import-water.json` heights are scaled too (178–740 m). `--sea-level` is in CS1 metres (97.5 → 195).
- `out/test-e15883-halves.json` and `out/test-e15883-3lane.json`: the two test variants of the refused piece, ends bound to the live dead ends.

## Round 7: mid-span sag, merge clearance

- CS2 inserts its own node in the middle of raised spans of about 80 m or more. Its height comes from the game, not from our nodes, and on high
  spans it sits 5–8 m below the deck (measured on the v8 import: 11 courses, −7.4 to −7.8 m). The deck fit now also splits raised courses
  longer than `MaxElevatedSpan` (75 m) at the deck height. Dakota: 165 extra splits.
- A deck/span split node keeps `SplitClearance` (10 m) from any course meeting the split course at an end. Otherwise the half next to a
  shallow merge lies inside the other road and is dropped (e32650_0b). Dakota: 20 splits skipped.
- `tools/live-deck-dips.mjs <live> <import body>`: height of game-inserted nodes against our deck.
- `tools/hm-sample.mjs`: the heightmap sampler for Node scripts.

## Round 8: all layers (power, trees, objects, connections, resources), two scales

```bash
dotnet run -c Release --project src/NetConverter -- "%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json" --mapping mapping.tsv --out out --heightmap ../hm-rebuild/out/DakotaFalls-1to1-4096.png --live out/live-graph.json --baseline out/none
```

```bash
dotnet run -c Release --project src/NetConverter -- "%LOCALAPPDATA%/DakotaExport/map-20261018-132303.json" --mapping mapping.tsv --out out/x1 --heightmap ../hm-rebuild/out/DakotaFalls-1to1-4096.png --height-factor 1.0 --sea-level 97.5 --trees-dir out/trees
```

- The network is always converted at `--topology-factor 2` and written at `--height-factor`. out/ and out/x1/ therefore have identical nodes and courses, and only the heights differ.
- Power: `High-voltage Line` (Locale.cok) in `import-power.json`. Nodes are on the ground and stay out of import-all.json.
- Trees (`tree-mapping.tsv`, weighted groups, age profiles): `trees/import-trees-NN.json`, ≤ 50 000 each, no y. Source is `<dump>-trees.json`
  (dakota-export/cs1/trees/v1) when present, else the rasters. The 16 m cells give density, scattered with ≈3 m spacing (seed `--tree-seed`), and the 64 m cells give
  the species multiset. Corn Stalk is CROP: dropped, and its cells feed the synthetic fertility layer.
- Objects (`object-mapping.tsv`; SKIP, MANUAL): `import-objects.json`. rotation = −CS1 angle in degrees (sign unverified). y = CS1 y × factor only
  when the object is > 2 m off the CS1 terrain (many rock decorations sit on cliff faces). MANUAL items go to `manual-objects.md`.
- Outside connections: `import-connections.json`, one object per border node. A border node is matched by tracing the CS1 network outwards to the node
  flagged Outside, else by the border projection within 300 m. Air and ship connections sit at the CS1 position clamped to the border. `connections-check.md` covers both directions.
- Resources: `import-resources.json` from the dump's `naturalResources` (schema v2) when present, else `import-resources-synthetic.json`
  (fertility from corn density).
- `out/ROUND5.md`: file list, counts and import order.

## Round 9: v2/v3 dump fields, one command

```bash
node tools/run-all.mjs
```

This runs both scales on the newest dump in `%LOCALAPPDATA%\DakotaExport` (or pass a path) and writes `out/ROUND.md`. Fixture: `node tools/make-v3-fixture.mjs`
writes `fixtures/v3/` (a hand-made v3 dump with siblings), and `node tools/run-all.mjs fixtures/v3/map-v3.json --out out/fixture-v3` runs it.

- `waterSurface` (v3; fallback `<base>-water.png`, surface × 64, row 0 north, depth = surface − CS1 terrain) produces `water-check.md`:
  - each source against the surface at it and the median surface within 100 m;
  - the largest wet body against the sea level;
  - wet patches above sea level with no source within 400 m.
  Sources off by more than 1 m get the ring median in `import-water.json` (`refinedFrom`).
- `naturalResources` (v2) produces `import-resources.json` with fertility, ore, oil, plus groundwater from `water` only when it has non-zero cells; forest is omitted. The synthetic file is
  deleted once real layers exist.
- `<base>-trees.json` (v2): exact positions and species. The tree README compares the count with the raster-based set.
- `<base>-po.json` (v3): grouped by base prefab in `manual-objects.md`. The subset that maps through object-mapping.tsv goes to `import-po-candidates.json`
  (UNVERIFIED, with y, scale not applied). The parser accepts several key spellings until the exporter documents v3.
- `terrainSurfaces` (v3, may be null) produces `surfaces-summary.md` (legend, area per class, largest patches) and `surfaces.geojson` (cell-boundary
  polygons per class, Douglas–Peucker 4 m; counter-clockwise outer rings, clockwise holes).

## Round 10: surfaces and the skip review

- `import-surfaces-fields.json`: CS1 field networks become Agriculture Surface 01. Each segment's half-width is swept along its Bézier on a 4 m grid, touching footprints merge,
  pieces are cut through holes (CS2 areas have none), and rings are traced and simplified (2 m, ≤ 500 points, finer when a ring would self-intersect).
  The footprints raise fertility to ≥ 220 in the real or the synthetic resources layer.
- `import-surfaces-grass.json` (optional): Landscape Grass props clustered with DBSCAN (12 m, ≥ 5), convex hull padded 3 m.
- `import-surfaces.json`: v3 terrainSurfaces through `surface-mapping.tsv`.
- Pedestrian Gravel / Elevated become Pavement Path (roads layer). Quay becomes QuayMedium01 in `import-quays.json` (unverified). Modern Fence becomes FenceResidentialPieceLow01.
  The skipped classes and their reasons are in `stats.md` → "Skip review".

## Round 11: v3 shapes from the exporter, walls and ship paths, ruins

- v3 parsing follows cs1-exporter/SCHEMA.md:
  - `waterSurface` and `-water.png` sit on the heightmap vertex grid (x = −8640 + col·16).
  - `terrainSurfaces` uses sparse flag values (2 PavementA … 128 Sand) on a 1080 × 16 m grid.
  - PO is read from `proceduralObjects.file`, using `yawDegrees` (else the quaternion); a `scale` of 0 means 1.
  - Every v3 section may be null; `sectionErrors` is printed and noted in stats.md.
- New layers with their own import files: `walls` (`import-walls.json`, CS1 elevations: Quay → QuayMedium01, Retaining Walls → QuaySmall01, Canal → Medium Boatway,
  Flood Wall → Sound Barrier) and `shippaths` (`import-shippaths.json`, Ship Path → Medium Seaway, ground, clipped so it ends on the border).
  Barrier segments become ConcreteBarrier01 / SafetyBarrier01 props every 3 m (`import-walls-props.json`).
- Ruins: Abandoned Factory → RuinsOldMill01, Pier Ruins → LeisurePier01Small01, Ship Wreck → RubblePileWood02 (nearest names; reasons in object-mapping.tsv).
- Waiting for cs2-playset/ASSET_PACKS.md: Landscape Grass 1:1 grass props, pack rocks, pack decals. These need only object-mapping.tsv rows.

## Round 12: asset packs (cs2-playset/ASSET_PACKS.md)

- Landscape Grass 01–06 map 1:1 to pack plants (Styx grass tufts, ferns, pd Long Grass, trimmed undergrowth, PD-YellowFlower, pd Bloom) with kind `plant`
  and age `adult`, keeping CS1 position and rotation. The grass-surface clusters stay as the optional alternative.
- Rocks: Arid / RZ / Cliffs and Rocks pack rocks mixed 50/50 with vanilla per CS1 class; Modular C formations go by footprint (see object-mapping.tsv).
  The CS1 lots (32–72 m) are much larger than the Arid rocks; the switch is noted in the mapping.
- R69 Big Decal Dirt/Mud become 16 × 16 m Dirty Surfaces Pack squares (`import-surfaces-decals.json`): Dirt → PDO23 Dirty Surface 01, Mud → PDO23 Soil Surface 01.
- `node tools/run-all.mjs` writes `out/ROUND6.md` (= ROUND.md), the final file list.

## Round 13: final CS1 export (schema v5)

- Trees from the trees/v2 point file: exact position, species and `scale` → age (< 0.6 seedling, < 0.85 sapling, ≤ 1.2 adult, else elderly). FixedHeight trees (flag 0x20)
  carry y and go to the per-scale `import-trees-fixed-NN.json` in out/ and out/x1/; the rest stay in the shared `trees/` chunks. The mask drops trees below the sea level,
  in CS1 water (real water surface) or on a ground-net corridor (the latter to the experiment file).
- Resources come only from `naturalResources`; the synthetic file is no longer written.
- `water-check.md` ends with a sea-level recommendation from the largest body's median surface.
- `terrainSurfaces` are written per class (`import-surfaces-sand.json`, …) through `surface-mapping.tsv`, unioned, hole-free and simplified; `import-surfaces.json` is gone.
- PO: `manual-objects.md` groups the objects by class (rules from cs1-offline po-classes.md, ids cross-checked with `--po-offline`), lists Effects Module positions as
  "falls foam", and `import-po-candidates.json` holds only structures that map through object-mapping.tsv.
- Network skins: IMPORT_SCHEMA.md has no edge field for them, so edges carry informational `noPillars` / `noStreetLights` fields, stats.md has a Network Skins row
  and manual-objects.md lists the segments.
- `node tools/run-all.mjs` writes `out/ROUND7.md` (= ROUND.md).
- Run A name fixes (live game build): Canal → Narrow Boatway (only Pier and Narrow exist), Pier Ruins → LeisurePier01HeadSmall, Fallen Pine Stump → LogRandom01,
  Trashpile → TrashbagRandom01, Dirt Pile → RubblePileMixedRandom01; the pier and ElectricalSubstation01 have kind `object` (the bridge classifies them so).
- `tools/defects.mjs <live-graph.json>` writes live-defects.md next to the graph. Walls, ship paths, power and decor (layer column of mapping.tsv) are left out of
  the dead-end, gap and tail checks; wall ends lying on another wall without being joined get their own table (Run A: 14 zero-length quay stubs, all on an end of a
  wall-flagged edge).
- Pavement Path dead ends within 20 m (Δy ≤ 3 m) of a ground road, gravel road or path get `snapToSegment` {radius, prefabContains} (`PathSnap`, stats in stats.md).
  Run A: 3 of the 4 reported stubs; the 4th, at (295, −3026), is the end of an isolated three-piece path whose "neighbour" is its own chain.

## Round 8: owner review of Run A (`Round8.cs`; each its own import file, both scales)

- Object y: compared with the playable CS2 heightmap (`--cs2-heightmap`, default LocalLow `10190615-dktflls-1to1.png`, value/32 = true metres). y only where the
  object is > 3 m above it; buildings never get y. 556 objects lose y against the old CS1-terrain rule, none gain it (2 857 → 2 301).
- `import-substation.json`: CS1 b39573 (SEC Extra Large Substation) → `Transformer Station` (PLACEHOLDER until the bridge reports the prefab), kind building, no y.
- `import-dam.json` (variant A, yaw 261.7 = −CS1 angle) and `import-dam-B.json` (B, 98.3): `Hydroelectric Power Plant` (placeholder) at the centre of the four CS1
  Dam Electricity pieces (1549.5, 4690). Import one of them.
- `import-power-ends.json`: both 345 kV dead ends sit near the substation (45 / 57 m; no CS1 line reaches the dam). The nearer circuit is rerouted to the substation,
  the other to the dam: each new course runs from the existing pylon node (snapToSegment radius 2) to the building centre moved 8 m toward the line (fallback
  until the bridge reports the connection point). `replaces` lists the edges to undo first (e5094_0, e32855_0).
- `import-po-rocks.json`: pack cliffs at the PO mountains / rock work / falls steps of cs1-offline po-offline.json, by footprint in the object frame (3–12 m Arid rocks /
  Boulders, 12–36 m rzrock, 36–80 m CliffRock5/6/Stack, 80–160 m by height and aspect, > 160 m a CliffFreeStanding at the summit plus CliffSide pieces every 60 m
  down the steepest flank), PO yaw, no y, largest first, cap 2 500.
- `import-rock-scatter.json`: Rock01–10 (70 %), Rock11–30 (25 %), Boulder01–08 (5 %), one dart per 10 m cell with p = 100/500 within 60 m of every rock/cliff object
  and inside the rock surfaces, ≥ 6 m apart and from a course's edge, none in CS1 water. Boulder02–07 are not in our name lists yet (orchestrator's list).
- `node tools/run-all.mjs` writes `out/ROUND8.md` (= ROUND.md); ROUND7.md stays as the Run A record.
- Bridge live tests (round 8b): substation → `TransformerStation01` (48 × 56 m), dam → `HydroelectricPowerPlant01` (48 × 72 m) at rotation 261.7 (variant B dropped).
  `import-dam-deck.json` (absolute heights, after import-dam.json): one `Hydroelectric_Power_Plant_01 Dam` edge from (1440, 4673) to (1576, 4694) at crest y 172 (× 2 = 344).
  `import-power-ends.json` end nodes carry `connectTo` {voltage high, radius 60} (bridge 0.9.13), so they attach to the building's high-voltage marker node.

## Round 9 (`Round9.cs`, `tools/import-run.mjs`; both scales)

- `node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5] [--skip step,...] [--only step,...] [--climate <name>] [--dry-run] [--bridge URL]` runs a whole
  fresh-map import in the ROUND order (network, gaps, power, walls, po-walls, shippaths, decor, water (the file for the sea level), lakes, connections, objects,
  barriers, substation, dam (+ deck), po-rocks, rock-scatter, fort, trees (+ fixed), resources, surfaces, power-ends, settings). One line per file; a body error
  (HTTP 400 with problems) stops the run; the report with every importId and its undo call goes to out/x1 (x1) or out (x2) as `run-<timestamp>.md`.
  Settings: theme named like "North America" in the game's list, latitude 40.0, longitude −105.3, sea level × scale, climate if given.
- `import-surfaces-po-rock.json`: rock texture (`gaga TTP - Pebbles 2L`, surface-mapping.tsv `po rock`) on every cell of cs1-offline v2
  `po-terrain-like-cs2-4096.png` > 0, hole-free, 4 m, patches ≥ 100 m². The 40° slope layer stays separate.
- `import-fort.json`: wall props along the 12 CS1 Castle Wall Béziers from `fort-walls.tsv` (UNVERIFIED names, lengths, rotation offset).
- `import-po-structures.json` (pavement tops of the 85 PO paved blocks; sloped tops use the plan footprint) and `import-po-walls.json` (QuaySmall01 along the long
  sides of blocks > 4 m tall, at the top height, absolute).
- `import-water-lakes.json`: one natural source per wet body above sea level + 1 m with no CS1 source within 400 m (median level, radius √(area/π), 20–300 m);
  bodies whose surface spans > 3 m are flagged SLOPING REACH in the note.
- `import-gaps.json`: e15883_0 as 3-lane (taper alternative in `import-gaps-e15883-taper.json`), e20430_0b bowed 1 m off the parallel carriageway, the CS1 gravel gap,
  e25579_0 / e36563_0 (Run A lost their T-junction node), and connectors for the three Pavement Path stubs (snap radius 20). Ends join the live net by snapToSegment.
  e15232_0, e17832_0 and e28650_0 were built in Run A (a curved segment sits on each) although the bridge reported them failed.

## Round 10: Run A-v3 fixes

- `FootprintShift`: walls and decor that the game dropped inside a highway's footprint (drop messages in `--run-reports`, default out/x1/run-*.md) move the
  asked distance + 1 m away from the nearest course of the named highway; highway courses that the game would merge count as one chain. A move that would land
  inside another chain (between carriageways) is skipped and listed as IMPOSSIBLE in stats.md. Without run reports, the geometric rule (two CS2 half-widths + 1 m,
  highways within 6 m height) applies.
- import-power.json leaves out the courses import-power-ends.json replaces (`_note`).
- Lakes sit on the body's lowest CS2-terrain cell when the centre cell is not 0.5 m below the level; otherwise dropped with a note.
- The CS2 heightmap default is the newest `<MMddHHmm>-dktflls-1to1.png` in the game's Heightmaps folder.
- Gaps: gap_e15883_3lane bowed clear of the parallel 4-lane; path connectors under 10 m are left out (inside the road's footprint; the path end snaps in import-all.json).

## Round 11: PO rocks v2, dam power course

- `import-po-rocks.json` (import name `po-rocks-v2`, `PoRocksV2.cs`): the objects only add rock texture to the stamped terrain. Pieces of height H ≥ 12 m
  (ASSET_PACKS.md footprints) get absolute y = terrain − (H − v), v = clamp(0.25 H, 8, 35) m visible (terrain from the CS2 heightmap, × 2 for the x2 set);
  smaller rocks sit on the terrain. Summit piece per PO object from CliffRock5/6 / CliffStack (≥ 36 m), rzrock (12–36 m) or Boulder (< 12 m); extra pieces
  inside large footprints are rzrock / Boulder; one CliffSide* / CliffFreeStanding only on PO mountains > 120 m above the ring around them. No two pieces
  within 80 m share prefab and heading (± 20°). Cap 700, largest first, inside the map. The v1 positions still feed the rock scatter.
  Live A-v3 map: undo v1 first with `POST /import/objects/undo?id=obj-20261019-165834-6`.
- `import-power-ends.json`: a course that passes another target closer than 77 m (TransformerStation01 half-diagonal 37 m + 40 m) bends through an extra pylon
  on the far side. The dam course now runs n2881 → dam_via (87 m from the station) → dam_end, both legs ≥ 79 m clear; the station course is unchanged.
- Fort (round 11b): EU_Wall01/03 are 0.9 m garden walls (live measurement), so `import-fort.json` places RuinsStoneFormation01/02 every 12 m along each
  Castle Wall Bézier (curve heading), StandingStone01–08 at the segment ends, RuinsStoneCairns01–03 every 25 m 3–6 m inside the line (towards the centroid
  of the walls within 300 m) and OldCastle01 at the largest group's centroid (unverified as placeable). `fort-walls.tsv` is gone.

## Round 12 (orchestrator round 10): owner review of A-v4

- Dropped from the file set and the runner: quays and retaining walls (`import-walls.json` → `import-canals.json` with the 14 Narrow Boatway canals and the 2 Sound
  Barrier flood walls), `import-po-walls.json`, `import-decor-monorail.json`, `import-dam.json` and `import-dam-deck.json` (dam by hand: manual-objects.md → Dam),
  the 16 m-cell sand surfaces (optional `import-surfaces-sand-smooth.json`: patches ≥ 2 000 m², 12 m round buffer, 2 majority passes on 4 m; runner `--with sand-smooth`)
  and the decal squares (runner `--with decals`).
- `tools/fetch-bounds.mjs` writes `prefab-bounds.json` (live mesh bounds from the bridge). Pack rocks in import-objects.json sit at y = terrain − bounds.min.y − 15 % of the
  height; po-rocks v3 (`PoRocksV2.cs`, import name `po-rocks-v3`): visible v = clamp(0.45 H, 15, 70) for pieces ≥ 12 m, 0.6 H below, y = terrain − bounds.min.y − (H − v)
  (v2 ignored bounds.min.y, and the cliff pivots sit up to 130 m above the mesh bottom); PO objects > 60 m above their surroundings get CliffSide pieces every 50 m down the
  steepest flank; cap 1 500.
- Barriers: the piece's long axis is local x (bounds), so yaw = course heading + 90°.
- Rock scatter: none where the CS2 terrain is below sea level; slopes 25–45° lowered by tan(slope) × half the footprint; over 45° left out.
- Pier Ruins → StandingStone01 (LeisurePier01HeadSmall renders untextured in the editor).
- One-way highway chains that end in a CS1 dead end are left out up to the first junction (two 3-lane stubs at (2293, 277) / (2302, 255), 34 courses); `--keep-highway-stubs` keeps them.
- `tools/import-run.mjs`: `--with` for optional steps, a graph export compared per prefab with the imported edges after the network steps and at the end (`--no-verify` skips).
- `tools/live-analysis.mjs`: lost segments, highway dead ends and 4-lane splits of a live map against the import set, appended to live-defects.md.
- Round 12b: the converter no longer computes y for pack rocks, po-rocks, scatter or fort pieces. Those files carry `options.grounding`
  {mode boundsMin, sink 0.15, slopeSink true} (bridge 0.9.19 grounds each object without y from its prefab bounds); po-rocks pieces carry a per-object
  `sink` (fraction of the height, (H − v) / H: median 0.40, max 0.59) for the v3 visible-height rule. import-objects.json keeps y on the 1 189 objects that
  stand on structures (> 3 m above the CS2 terrain), assuming the bridge grounds only objects without y. prefab-bounds.json stays for the v3 sink fractions.

## Round 13 (orchestrator round 11): bridge replacements (`bridges.tsv`, `Bridges.cs`)

- `bridges.tsv`: bridge id (../bridges/data/cs1-bridges.json segIds) → deck selector → CS2 prefab → plan (`fixed:`, `count:`, `span:`, `merge`, `fixedfile`) → seg
  (`[U]` = assumed) → notes → kept alternative. It overrides mapping.tsv for those segments only.
- Each selected deck chain (split at junctions) becomes straight, constant-grade edges between its end nodes (deck height = the end nodes' heights, already × scale),
  no span split, no smoothing; ends shared with the network join it by snapToSegment (radius 2). → `import-bridges.json`, imported right after import-all.json
  (which leaves these decks out).
- BR001: one two-way centreline of 168 + 320 + 320 + 168 m (fixedIndex 0–3) on the CS1 tower axis, centred between the towers; each carriageway splits off at its
  first node ≥ 100 m beyond a deck end, the carriageway courses in between are replaced; clearance and height over water reported. BR022: BXP Arc Pedestrian
  Bridge Revised along the monorail arch chains. Both in `import-bridges-fixed.json`, off by default (`--bridges-fixed on` removes the replaced courses from
  import-all.json; runner `--with bridges-fixed`).
- `--bridges off` keeps every CS1 bridge as the plain mapped road. stats.md → "Bridges": prefab, plan, edges and lengths, length / curvature / over-water checks.
- Round 13b (bridge 0.9.22 fixed-span contract): `import-bridges-fixed.json` holds BR001 as ONE `BXP SuspensionBridge03 - Six Lane Highway` edge abutment to
  abutment (976 m, no inner nodes, no fixedIndex: CourseSplitSystem cuts the spans and places the towers) with the four 3-lane connectors to the split nodes
  outside it, and BR022 as one `BXP Arc Pedestrian Bridge Revised` edge per arc chain (not a fixed-span prefab; the game cuts 446 m into 10 pieces). On by
  default (`--bridges-fixed off` to keep BR001 as plain highway); the runner imports it right after import-bridges.json.
- Rule checks (stats.md → Bridges, last column): the bridge's pre-checks computed offline against ../bridges/cs2-bridge-rules.json (`Bridges.Validate`):
  bridge.length.short, bridge.fixed.repeat / .short / .chained, the game's split count, bridge.waterClearance. Vanilla prefabs and bridge.lowElevation are not in
  the offline rules; the runner's dry run checks them.
- noPillars (PLAN.md item 4): BR006 deck edges, the four BR001 connectors and BR001's deck courses outside the merged span in import-all.json.

## Round 14: road QA, vanilla surfaces

- `tools/road-qa.mjs [out/x1] [--json f] [--label l]`: dead ends (genuine vs pipeline against CS1), mid-air elevated stub pairs, torn splits, duplicate courses
  (a rebuild over its original), isolated pieces < 100 m and ramp dead ends near a mainline, over import-all + bridges + gaps (snapping nodes count as joined).
  Report with before / after counts and the residual list: `out/x1/road-qa.md`.
- Fixes: the gap rebuilds now replace their originals in import-all.json (e15883_0, e20430_0b, e25579_0, e36563_0; on A-v4 both were built and tore the
  4-lane / 3-lane split); the gaps pass also finds bridge-deck nodes (the CS1 gravel gap at BR020); a converter pass snaps pipeline dead ends onto a course
  within 25 m or drops stubs < 40 m and drops isolated pieces < 100 m that CS1 connected (none left in this map; reported in stats.md).
- Surfaces are vanilla only: fields (Agriculture Surface 01), sand as Sand Surface 01 (smoothed blobs, on by default), PO pavement (Pavement Surface 01); grass
  optional. Terrain Textures rock (40° and PO footprints), Dirty Surfaces decals and the pack sand moved to `out/optional/` (runner `--with rock,po-rock,decals,sand-pack`).
