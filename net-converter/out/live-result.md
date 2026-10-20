## Live result, Run A (2026-10-19, reported by the orchestrator)

| import | result |
|---|---|
| network | 4 063 / 4 072 edges; the 9 failures are the known e15883_0, e20430_0b and seven Gravel pieces |
| walls | 90 / 113; the 23 wall-flagged quays fail (bridge investigating) |
| water | 44 sources |
| connections | 25 |
| objects | 6 130 |
| barrier props | 621 |
| trees | 496 318 |
| resources | 4 layers |
| surfaces | 727 |

Five names did not exist in this build and were substituted at import time; the mapping tables now use the substitutes:
Medium Boatway → Narrow Boatway, PineTreeStump01 → LogRandom01, PileGarbageMediumLow01 → TrashbagRandom01,
PileStoneSmallLow01 → RubblePileMixedRandom01, LeisurePier01Small01 → LeisurePier01HeadSmall (kind object, as ElectricalSubstation01).

Defect scan of Run A (`out/x1/live-defects.md`, scanner now skips walls / ship paths / power / decor): 3 facing gaps left, all known — e20430_0b (44 m)
and e15883_0 (68 m) are failed import edges, the Gravel 80 m at (1462, 6173) is the CS1 gap. The Gravel / Narrow Boatway 265 m pair was two CS1 dead ends
(road 23 m above the canal end), not a converter gap. 14 zero-length QuaySmall01 stubs sit on ends of wall-flagged edges (for the bridge analysis).
Pavement Path stubs: 3 now carry snapToSegment for the next import ((4783, −683), (1577, 4694), (737, 4853)); (295, −3026) is an isolated path, nothing to join.

## Run A-v3 (fresh map, tools/import-run.mjs; reports out/x1/run-20261019-165751.md, run-20261019-165831.md)

network 4 063/4 072, gaps 2/8, power 60, walls 90/113, po-walls 98/102, ship paths 100, decor 86/96, water 44, lakes 17 (lake7 rejected), connections 25,
objects 6 130, barriers 621, station, dam + deck, po-rocks 594, scatter 19 020, fort 97, trees 496 368, resources 4, surfaces 869, power-ends 1/2.

Failures, classified (round 10 fixes in brackets):
- Game drops (course inside another net's footprint): walls 17, decor 10, po-walls 1, gaps 2.
  [walls / decor: moved the asked distance + 1 m away from the named highway: 10 walls + 4 decor move; 7 walls + 6 decor would land inside the other
  carriageway and stay (listed in stats.md as IMPOSSIBLE). pb646_side23 left out. gap_e15883_3lane bowed 4.5 m off the parallel 4-lane;
  gap_n7811_path dropped (its path end snaps by itself in import-all.json; same for n13342).]
- Geometry / detection ("only N of 3 points lie on a segment of this prefab"): network 9 (e15883_0, e20430_0b, seven Gravel), walls 3 (e835_0, e23249_0,
  e23938_0), po-walls 3 (pb693/pb694), gaps 4 (gap_e20430_offset, gap_e25579_0, gap_e36563_0, gap_n13342_path). Not a footprint drop: the game built
  something else or nothing there; check in game (Run A showed three such Gravel pieces built as curves).
- power-ends: e5094_0_to_substation overlapped the original e5094_0 [import-power.json now leaves out e5094_0 and e32855_0].
- lake7: level 134.94 m under the CS2 terrain 138.23 m at the body centre [lakes now sit on the body's lowest CS2-terrain cell (lake7, 11, 16 moved);
  a lake whose level is not 0.5 m above even that is dropped with a note]. The CS2 heightmap default is now the newest <MMddHHmm>-dktflls-1to1.png
  (10190843, the one A-v3 used).
- Fort piece lengths: not reported by the bridge yet; fort-walls.tsv still holds the guesses (all 97 props were created).

Owner review of A-v3 (round 11): the v1 PO cliffs (CliffFreeStanding / CliffSide, 140–195 m) stood exposed above the stamped terrain → `po-rocks-v2`
(626 pieces: 239 sunk, 387 on the terrain). The dam's high-voltage course ran through the station's connector → it now bends round the station
through an extra pylon. Undo on the live A-v3 map: `POST /import/objects/undo?id=obj-20261019-165834-6` (v1 po-rocks); the next build is a fresh map.

Fort (round 11b): the EU_Wall pieces measured 3 × 0.9 m (garden walls), so import-fort.json is now a ruined stone line: 112 objects
(RuinsStoneFormation01/02 63 every 12 m, RuinsStoneCairns01–03 29 inside the line, StandingStone01–08 19 at the ends, OldCastle01 keep at (5, −2619), unverified).
fort-walls.tsv is retired. RuinsCastle01 stays in import-objects.json. A real rampart needs a PDX Mods wall pack (none in the playset).

## A-v4 (owner review, round 12)

Live graph (out/x1/live-graph.json, export live-graph-export.json, 2026-10-20): 309 import edges have no live segment (220 Gravel Road, 58 Highway 2-lane); 297 of them
were live in Run A, the run reported 9 failures, and no imported object, network or surface explains most of them: lost after the import (game side or an edit), see
out/x1/live-defects.md ("A-v4 against the import set"). The runner now exports the graph after the network steps and at the end to pin the step down.
Mid-air highway stubs: 44 of the 46 highway dead ends are such lost continuations; the other two are the CS1 dead-end 3-lane pair at (2293, 277) / (2302, 255), left out
from this round. The 4-lane split with broken geometry needs a position from the screenshot (12 candidates listed).

## Round 15: dam, look, vanilla paint plan; road-QA residual for the owner

Dam: import-dam.json is one Hydroelectric_Power_Plant_01 Dam edge (bridge 0.9.23 builds the plant and the dam ends), level at the crest (172 m x1 / 344 m x2), flow auto; the east abutment moved 164 m out to where the terrain reaches the crest, the west one stays at the CS1 quay end 6.9 m above the terrain (dam.elevation warning: the terrain there never reaches 172 m within 250 m). Look: the runner's look step POSTs ../look/climate.json, water.json, terrain.json after the settings (fields not in /map/look/schema skipped).

Road QA residual (out/x1/road-qa.md, check in game): every remaining dead end is a CS1 dead end too:

| (-768, 4363) | dead end, Gravel Road | CS1 dead end too (CS1 degree 1) |
| (373, -3031) | dead end, Pavement Path | CS1 dead end too (CS1 degree 1) |
| (1104, 4897) | dead end, Gravel Road | CS1 dead end too (CS1 degree 1) |
| (4787, -709) | dead end, Pavement Path | CS1 dead end too (CS1 degree 1) |
| (295, -3026) | dead end, Pavement Path | CS1 dead end too (CS1 degree 1) |
| (-3987, 334) | dead end, Gravel Road | CS1 dead end too (CS1 degree 1) |
| (-480, -1481) | dead end, Medium Road | CS1 dead end too (CS1 degree 1) |
| (-229, -2362) | dead end, Gravel Road | CS1 dead end too (CS1 degree 1) |
| (-643, -1188) | dead end, Medium Road | CS1 dead end too (CS1 degree 1) |
| (367, -3024) | isolated Pavement Path, 83 m (e7126_0,e28940_0,e18960_0) | isolated in CS1 too (kept) |

## A-v5 (live, 2026-10-20; reports out/x1/run-20261020-140800.md and run-20261020-140845.md)

Every bridge prefab name resolved in game; the surfaces-first order worked. The bridges body was rejected by the bridge's pre-checks (patched inline for that run):
TrussArchBridge01 is a fixed-span prefab (bridge.fixed.chained on BR007 / BR010), and nine edges were below 8 m (bridge.lowElevation). The follow-up run stopped at
the dam: bridge.lowElevation 6.9 m. Round 17 fixes: BR007 / BR010 one TrussArchBridge01 edge each (189 / 265 m, live rules 153.6–358.4 m); BR017, BR020, BR044
decks lifted to 8.5 m; BR038 (approach would reach 6.9 %) and the gravel trestles BR045, BR049, BR050, BR053 (0–3 m) keep their base prefab; the dam edge is
180 m (its fixed spans allow 179.2–204.8 m) with the crest at 175.5 m so its middle is 8.5 m above the terrain; the terrain along the dam axis is nearly flat
(167–169 m), so both abutments stand 6.8–7.7 m above it (dam.elevation warning). A ground-level dam there needs a valley at least 8 m deep under it (heightmap).

Dam (round 18): A-v5 built plant + 11 owned spans on dry ground east of the channel, with the plant at the west end. The dam is now one 304 m edge across the
CS1 water from (1127, 4639) to (1425, 4696), crest 165.6 m (x1; 331.2 m x2), flow invert, abutments on the ground (0.4 / 0.0 m); out/x1/dam-profile.md has
the terrain and water profile (CS1 water 157.8 m upstream / north, 154–156 m downstream). The A-v5 +1.6 m patch is superseded by the regenerated file.

## Round 19: owner review of A-v5 (live graph out/x1/live-graph.json / live-graph-export.json, A-v5 map)

1. BR022 arch decor dropped (bridges/DECISIONS.md decision 2), BR004 dam-crest walkway dropped (31 courses, 798 m) with the footpaths that only led onto it;
   import-bridges-fixed.json holds only the Detroit suspension (1 deck edge + 4 connectors).
2. Raster surfaces (sand, dry grass): mask closed + opened by 2 cells, simplified to half a cell, 3 Chaikin passes, 6–10 m smooth edge noise, patches ≥ 1 500 m²
   (sand 112 polygons, dry grass 159, no self-intersection, no axis-aligned right angle, ≤ 1 214 points). Dry grass is Grass Surface 01 until the swatch test.
   Fields: 55 true circles (closed CS1 rings and least-squares fits of open arcs), 0 cell blobs.
3. Bridge checks (stats.md → Bridges): no bridge failed the twin-overlap (< 1.2 × width) or the course-through-deck (< 12 m in plan, < 6 m in height) checks;
   approach tangents over 4° were straightened onto the deck: BR005 (6°, 6°, 15°) and BR009 (42°, e18373_0: the likely knot).
4. (a) live highway dead ends that CS1 did not have: 2, both at e20430_0b (799, 4000) / (837, 4022): it ran 15.5 m beside its parallel carriageway (two 3-lane
   half-widths need 16 m); it is now moved 1.9 m away in the network itself (no separate rebuild). (b) one-way head-on joins: 2, at (2993, 886) and (2998, 755):
   the round-12 stub removal had taken the 3-lane outflow of two merges; the unfinished CS1 branch now goes as a whole (66 one-way courses, 4.2 km, x 2745–3695,
   z 755–2382; --keep-highway-stubs restores branch and stubs). (c) crossings flatter than in CS1: 1 at (−4224, −60) (1-lane highway over Medium Road
   e29315_0a: 1.8 m apart, CS1 6.7 m; the CS1 road dipped between its nodes): the road gets a node at its CS1 height. None on the live map between highways.
5. Low bridges: all nine low spans raised to 8.5 m (BR017 +1.0, BR020 +1.2, BR038 +2.6, BR044 +0.8, BR045 +5.4, BR049 +7.3, BR050 +7.4, BR053 +8.3), approaches
   regraded at ≤ 6 % through junctions (0–5 nodes each); no fallback needed.
6. BR007 / BR010: one TrussArchBridge01 edge each, 189 / 265 m, inside the fixed sequence range 153.6–358.4 m (128 + 128, each 76.8–179.2).
Residual: the 9 genuine CS1 dead ends and the isolated 83 m CS1 path in out/x1/road-qa.md; dam abutments per dam-profile.md.

## Round 20: PO structures (bridges/PLAN-structures.md), A-v6 file set

1. Paved blocks: PO group roots 27 (BR006 viaduct) and 288 (BR005 bearings) are no longer pavement: 11 blocks → 11 polygons in import-po-structures.json (was 79).
2. import-po-structure-objects.json (both scales, /import/objects, kind object, rotation = PO yaw, options.grounding boundsMin with sink 0): 72 objects.
   - BR006: one StoneArchRailBridge at (−3528.5, −3175.8), the midpoint of the arch meshes 27 / 274, yaw 324, y = arch foot (199.3 × scale). Its live bounds
     reach 100 m below the pivot (a buried foundation), so it gets y instead of grounding. It is 28 × 40 × 19 m against the CS1 arch's 139 × 62 × 190 m.
   - BR006: 34 Pillar4x16 piers (grounded) and 34 Pillar1x16 spandrel columns (32 stand on the arch, so they get y = PO bottom × scale, pivot assumed at the base
     until ConcreteSet01 is in the running game; 2 grounded).
   - BR022: 3 BasePlateConcrete32 pads (grounded). BR022's arch decor is dropped (round 19); the pads stay per the proposal.
   - optional/import-po-structure-gantry.json: 1 HWY Truss Gantry 2L  A at (608.3, −4721.4), grounded (runner --with po-gantry).
   - optional/import-po-structure-piers.json: 50 ITBP_Pillar Piers 01 (BR001 38, BR003 12), a 3.6 m cap with its top at the PO pier top
     (runner --with po-piers, only once noPillars works).
   - Portals, rock shed, dam blocks, abutments, BR005 bearings: nothing.
   Bounds: po-structure-bounds.json (tools/fetch-po-structure-bounds.mjs). The running game has StoneArchRailBridge and ITBP_Pillar Piers 01; ConcreteSet01 and the
   gantry pack are not loaded, so a dry run rejects them (400 unknown prefab) until they are in the playset.
3. Runner: the steps po-structures, po-gantry and po-piers come after po-rocks, all off by default; `--po-structures` = `--with po-structures`. The look step reads
   look/climate.json, water.json and terrain.json at run time on every run (never via run-state), resolved from the script folder, and logs each file's sha1 and
   time. The dry run read water.json at 15:34, the version with waterScatteringColor, and the schema dropped no key.
4. A-v6 file set: the round-19 files (import-all 3730 edges, bridges 54, bridges-fixed 5 (Detroit), dam 1, gaps 4, surfaces with the smoothed paint plan,
   po-structures 11) plus the round-20 PO structure files above. Run:
   `node tools/import-run.mjs --scale x1|x2 --sea 93|97.5 [--po-structures] [--with po-gantry,po-piers]`.

## Round 21: A-v6 fixes

1. import-power-ends.json (both scales): dam_end now sits on the plant's High-voltage Marker as measured on A-v6. The position is stored in the dam edge's frame:
   22.0 m back from the east end and 27.0 m to the reservoir side, which gives (1398.66, 4718.33), y = crest (165.6 / 331.2), with connectTo radius 60.
   An extra pylon dam_pylon at (1470, 4655) splits the line into spans of 203 m and 95 m. bypassValidation is set in the file. The dead "replaces" lists are gone.
   Both files match next-run/power-ends-a6.json, except that they carry no connectTo index (entity ids differ per map).
2. Gravel Road point-check failures (e23168_0, e31265_0, e31665_0, e36006_0): offline, all four are straight, axis-aligned, flat 48–96 m farm-grid courses
   in the east (x 6335–6981). No other course comes within 12 m, so they are not merged under another prefab. e31665_0 and e36006_0 lie on the field circles
   field36 / field34 (Agriculture Surface 01); e23168_0 and e31265_0 lie on no surface. Waiting on the A-v6 live graph export.

## Round 22: A-v6 review

1. Gravel Road point-check failures (e23168_0, e31265_0, e31665_0 2/3 points, e36006_0 1/3), root cause from live-graph-20261020-193334-end.json and
   run-20261020-192927.json: the roads are there. All three check points (t 0.2 / 0.5 / 0.8) of each course lie 0.01–0.04 m from a live Gravel Road.
   - The game folded each straight course into its straight Gravel Road neighbours and split the run again into equal 8 m-grid pieces. Example: e19271_0 88 m
     + e31265_0 80 m + e11921_0 88 m = 256 m between two junctions became 98704:45 + 98400:33 of 128 m each. Others: 104 + 104, 104 + 96, 128 + 128.
   - The courses' inner nodes moved: n14925, n4745 and n32228 have no live node.
   - The bridge's check (BridgeImportToolSystem, kCourseSamples) only counts segments that are new since the batch snapshot or recorded for it. The game
     kept the neighbour's older segment entity for part of the run, so those samples counted as missing. No other prefab, no surface, nothing missing.
   - Fix (runner): every "points along the course" failure is checked again against the live graph export (verify steps); covered courses are reported as
     present, not as failures. Offline on the A-v6 data: 4 present, 0 missing. Bridge-side fix suggested: also count the segments the batch updated.
2. BR001 / BR003 piers (import-po-structure-piers.json, on by default):
   - Each of the 50 PO piers gets an ITBP_Pillar Piers 01 cap with its top at the deck soffit (deck y of the course above, which is 0.1–1.3 m away in plan,
     − 2 m deck depth), and a Pillar4x16 stack from the cap down to the terrain: 81 pieces at x1 (up to 41.8 m), 151 at x2 (up to 89.2 m).
   - The 26 CS1 segments above them get noPillars in import-all.json.
   - Runner step pillars-off, right before po-piers: live graph export, highway segments within 3 m of a cap, POST /net/pillars/remove.
3. Dry grass: import-surfaces-grass-dry.json is no longer written (swatch verdict, look/README.md). import-surfaces-pavement.json is off the runner's list.
4. BR006: the main file import-po-structure-objects.json is variant (a), no kit objects at BR006 (3 BR022 pads only), and the runner default.
   Variant (b) is optional/import-po-structure-br006-arches.json (--with po-br006-arches): 7 StoneArchRailBridge at 28 m pitch along the axis from
   (−3479, −3244) to (−3578, −3108), rotation 234 (the 28 m side along yaw 324), tops at the soffit of the 263.3 m deck (x1), legs sunk.
5. Gantry: unchanged (optional, --with po-gantry).

## Round 23: terrain material paint (editor Terrain Inspector → MATERIALS), data ready, format pending

The same files are written at both scales (x/z only): `import-terrain-paint-<material>.json`, each with `material` (a PLACEHOLDER, `materialPlaceholder: true`)
and `polygons: [{id, strength 1, polygon [[x, z], …], note}]`. Runner step `terrain-paint` after surfaces, optional (`--with terrain-paint`) until the bridge
publishes `/import/terrain-paint`; then only `TerrainPaint.Write` changes (polygons as they are, or brush strokes sampled from them).

| file | placeholder material | source | polygons | km² |
|---|---|---|---:|---:|
| import-terrain-paint-sand.json | Sand | every CS1 Sand cell (the 479-polygon source set), 16 m grid, close/open 2 cells, organic outlines as in round 19, ≥ 1 500 m² | 400 | 16.96 |
| import-terrain-paint-rock.json | Rock | slope ≥ 40° (true-height heightmap) + the PO rock / cliff footprints | 214 | 5.39 |
| import-terrain-paint-dry-grass.json | DryGrass | open dry ground (CS1 fertility < 0.5, no tree within 48 m, not sand / field / water / road): east and south-east plains (bearing 45°–180° from the centre, ≤ start mean + 60 m, slope < 12°) and south-facing slopes (8°–35°, downhill within 60° of south) | 433 | 9.72 |
| import-terrain-paint-bare-earth.json | Dirt | a round 11 m patch at each of the 128 CS1 R69 dirt / mud decals | 128 | 0.05 |

The CS1 data has no grass class (only Sand was painted). The dry-grass mask therefore stands on the CS1 theme's tints (fertile = yellower, forest floor = darker,
audit/theme-remake.md §1), the fertility raster, tree density and the heightmap aspect. Area surfaces (sand, PO pavement, fields) are unchanged.
