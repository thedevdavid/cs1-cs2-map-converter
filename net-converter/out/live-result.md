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

## Round 24: terrain paint in the bridge 0.9.29 format

- import-terrain-paint-sand.json: material Sand, 400 polygons, softEdge 6 m, strength 1 (16.96 km²); offline stamp estimate (port of the bridge's
  FillPolygon) ≈ 232 000, just under the 250 000 cap.
- import-terrain-paint-dry-grass.json: material Dry, 433 polygons, softEdge 6 m (9.72 km²), ≈ 148 000 stamps.
- import-terrain-paint-bare-earth.json: material Dirt, 128 strokes of radius 11 m at the CS1 dirt / mud decals.
- optional/import-terrain-paint-rock.json: mask only, not imported (no rock material).
- Runner: the terrain-paint step is on by default, right after surfaces. Each file is dry-run first with the cap lifted (stamps per item), then posted in
  chunks of at most options.maxStamps. A rejected body or a netGuard removal stops the run. Undo: POST /import/terrain-paint/undo (newest first, last 3 kept).
- Runner note: bridge 0.9.29 reports folded courses as merged; the live-graph re-check of point-check failures is now a fallback.

## Round 25: noPillars only where a pier object set stands (A-v7: BR006 without pylons)

- BR006: `nopillars` is gone from bridges.tsv, so variant (a) gets the game's pillars. Variant (b) (`--with po-br006-arches`) clears them through the runner's
  pillars-off step, which now also takes the arch pieces and Bridge prefab segments.
- Every other noPillars without a pier set behind it is dropped too (`--keep-cs1-nopillars` keeps them): 138 CS1 segments, from the CS1 Network Skins pillar
  flags and the BR001 approach flags. 135 of their courses stand 8–114 m high in A-v6 with nothing under them: the BR001 south approach
  (x 590–1110, z −5250…−4060, up to 114 m near the blue-arch site) and the interchange at x 3430–3690, z 1870–2320. The 4 BR001 3-lane connectors lose the flag.
- noPillars stays on the 26 CS1 segments above the BR001 / BR003 ITBP + Pillar4x16 stacks (48 edges) and on the BR001 suspension deck.
- x1 files against A-v7: geometry unchanged (no node or edge added, removed or moved). Only the `noPillars` field differs: import-all.json 145 edges,
  import-bridges.json 4 (br006_c1_e0…e3), import-bridges-fixed.json 4 (br001_in/out connectors). A re-import onto A7b merges the same geometry, and dropping a flag
  removes nothing. Pillars that noPillars already deleted come back only when the game updates those segments.

## Round 26: owner's A-v7 review (shots next-run/shots-a7) + Detroit test layer

Diagnosis: A against live-graph-20261020-211001-end.json and the run report; B, C, D, E and F-ii per edge against the CS1 dump, the import files and the live graph.

- **A (2995, 820), "completely missing interchange":** the game dropped nothing. Every import-all edge in the 600 m box was live (0.00–0.07 m) and the run had 0 failures.
  - **Cause:** the converter had left out 36 CS1 segments: 11 in the round-12 highway-stub removal and 25 in the round-19 one-way prune.
  - **Fix:** stubs and branch are kept by default now (`--drop-highway-stubs` restores the old removal). The box has 48 edges, A-v7 had 13. Dead ends 9 → 11; the 2 new ones are CS1's own stub ends at (2293, 277) / (2302, 255).
  - **Map-wide:** only 4 edges have no live segment within 2 m.
    - The dam: expected, owned segments.
    - e25368_0 at (−182, 2388): rail, fused with its twin 2.9 m away.
    - gap_e25579_0 and gap_e36563_0: their T-ends sit 56 m and 64 m along the through road. The game re-splits the straight run after snapToSegment (bridge-side, same in A-v6).
- **Systemic causes, all fixed:**
  - **R1:** game-rule limits ran at half strength at x1 (crossing raise 4 m, ramp 4 %). They are now output metres: limits ÷ YScale in ConvertOptions.
  - **R2:** the ramp walk spread raises in height above terrain, so on rising terrain a falling elevation became a rising road. It now takes the lower of the absolute and above-terrain ramps, and never rewrites a junction.
  - **R3:** same-layer crossings were never separated. They now go through the grade-separation pass. A crossing CS1 already cleared by ≥ 8 m is not raised. The round-19 restore pass is off (its split node was clamped back onto the terrain).
- **B (608, −4721):** fixed by R1–R3; ramps are graded at 8 % in output metres.
  - **Not fixed:** B5, curved 3/4-lane elevated pieces sagging 5–9 m mid-span. The cause is in the game or bridge (heights from live terrain); it doesn't depend on our file.
  - **Not fixed:** B4, ramps CS1 itself had at 3–7 m (hand-placed walls). They stay in the wall band.
- **C (−4224, −60):** the ramps over the Medium Road now stand 8.4 m above it (A-v7: 0.7 m), and the stacked restore node is gone.
- **D (BR005):** with two approaches at one deck end, only the through approach is straightened, so the side ramp keeps its tangent and no stub forms. The same applies to BR009.
  - **Unverified:** the carriageway centres are 15.7 m apart (decks 3.7 m apart). Whether the two BXP arches clip needs a live look.
- **E (BR001):** each connector is a curve that leaves the deck end 12° off the axis and arrives parallel to its carriageway. Leg centres are 7 / 12 / 15 / 16.5 m apart at 20 / 40 / 60 / 100 m (A-v7: 16 m only at 140 m). Same elevations.
- **F:**
  - **(i) Access roads:** import-access-roads.json (runner step `access` after the dam) adds Gravel Roads from e1619_0 to the substation front (41 m) and from e33625_0 to 6 m past the dam's east end (144 m). Whether the plant picks up that road is unverified.
  - **(ii) BR002 hump:** this is the bridge south of the dam, and its deck had no lift. The hump came from an unneeded crossing raise spread by the old ramp walk (R2/R3); it is fixed.
  - **(ii) Raised bridges:**
    - BR017, BR033 and BR038 go as one edge: the middle already stands ≥ 8 m, so no lift.
    - BR020 +3.9, BR044 +2.2, BR049 +7.3, BR050 +4.7 and BR053 +8.3 m are lifted, with one climb at ≤ 3 % from up to 300 m back.
    - BR028 and BR045 keep their base prefab: a 3 % climb would need more than 300 m.
- **G:** every PO piece is terrain-like and already baked into the heightmap.
  - **PO rocks:** po-rocks v4 keeps only the boulder class (Boulder01–08 and rzrock). 473 cliff pieces are dropped, 72 in water are dropped, 0 sat on a net corridor; 248 are left.
  - **Canyon road:** the west canyon road problem is the heightmap. road-terrain-mismatch.json and .md list 62 ground courses more than 2 m off the playable heightmap (up to 34.5 m at (−6500, 1000)) for hm-rebuild to carve or fill.
  - **(−3000, −5500):** no roads there, only rocks.
- **H (terrain paint):** main pass strength 0.45, softEdge 20 m. A feather pass at strength 0.2 uses the mask grown by one 16 m cell, smoothed on its own (0 self-intersections). Per-layer overrides without regenerating: `--paint sand.strength=0.4,all.softEdge=25,dry.feather=0,dirt.radius=14`.
- **Detroit test** (import-detroit-test.json, runner step `detroit-test` after the dam, standalone, bypassValidation, undoable):
  - **Option 2:** 200 m NW, 2 × 362 m `BXP Suspension Bridge - Highway Twoway - 6 Lanes` between the CS1 tower positions, plus 150 m `Highway Twoway - 4 lanes` stubs. No vanilla 6-lane two-way highway is known.
  - **Option 3:** 400 m NW, one 976 m `BXP Double Deck Suspension Bridge - Highway` (the game cuts 3 × 325 m), with `… - Lower Highway` 8 m below running the other way. The 8 m is a guess; TODO: measure live. Plus 150 m 3-lane one-way stubs.
  - **Deck height:** as BR001.
  - **Land:** the 976 m river part includes the banks. BR001's own line is about a third over land, the test lines about the same (41 / 46 of 123), so there is no island.

## Round 27: landscape spots N1–N13 as terrain paint (audit/theme-remake.md §4, revised)

There is one file per feature and brush, `import-terrain-paint-<nN>-<feature>-<brush>.json`, identical at both scales. Each feature is its own runner step
(paint-n1 … paint-n13, on by default, in the plan order N1, N12, N13, N6, N4, N5, N3, N2, N8, N7, N11, N9, N10, all after the round-24 Sand / Dry / Dirt).
Areas are organic polygons with the round-26 feather pass (0.2, one cell out); rings, streaks and dabs are strokes with radius and position jittered,
seeded per feature. `--paint` overrides any layer: brush (sand / dry / dirt / lush), feature (n1 … n13) or all.

| # | file(s) | brush, strength | result (x1) |
|---|---|---|---|
| N1 | n1-riparian-lush | Lush 0.6, soft 30 m | 73 polygons, 1.17 km²: ≤ 150 m from water, ≤ 4 m above it, not sand |
| N12 | n12-fertile-dry | Dry 0.35 | 276 polygons, 15.15 km²: fertility ≥ 0.5, not field / forest |
| N13 | n13-high-ground-dry | Dry 0.5 | 97 polygons, 11.40 km² above the treeline 589 m (99th percentile of tree cells), slope < 30° |
| N6 | n6-savanna-dry + n6-savanna-dirt | Dry 0.4 + Dirt 0.45 dabs r 4–8 | 1 334 polygons, 41.37 km² + 40 029 dabs: foothill band, 1–6 trees per cell |
| N4 | n4-hogback-sand | Sand 0.5 | 31 644 strokes in 3-stroke streaks along the contour: band, 18–38°, east-facing |
| N5 | n5-scree-dirt | Dirt 0.8 | 80 polygons, 0.47 km²: 28–40° within 60 m below ≥ 45° |
| N3 | n3-drawdown-dirt | Dirt 0.7 | 134 strokes: 0–2.5 m above the reservoir (level 157.5 m, flooded at ± 1.5 m from the upstream water) |
| N2 | n2-pit-banks-dirt + n2-pit-banks-sand | Dirt 0.6 / Sand 0.6 | 13 + 8 strokes: 5 small ponds within 1.5 km of the river with flat banks |
| N8 | n8-playa-rim-lush + n8-playa-centre-dirt | Lush 0.5 / Dirt 0.6 | 80 playas (priority-flood fill, depth 0.4–3 m, 0.5–20 ha): 0.58 + 0.92 km² |
| N7 | n7-prairie-dog-dry + n7-prairie-dog-dirt | Dry 0.8 + Dirt 0.5 dabs | 1 town (0.04 km², 162 mounds) |
| N11 | n11-ore-dirt + n11-oil-dirt | Dirt 0.5 / Dirt 0.3 | ore 8.74 km², oil 57.32 km² (CS1 resource ≥ 0.4) |
| N9 | n9-dirt-roads-dirt | Dirt 0.5 | 12 274 strokes along gravel / dirt roads, half-width + 4 m |
| N10 | n10-rail-ballast-dirt | Dirt 0.35 | 4 418 strokes along ground rail with ≤ 6 buildings within 300 m |

- **N7:** only 1 km² qualifies (flat plains with no tree within 100 m). The rate of 1 town per 12 km² gives 0, so the converter places at least one town when 0.5 km² or more qualifies.
- **N11 oil:** the CS1 oil layer covers 57 km² at ≥ 0.4. At strength 0.3 that is a big tint; `--skip paint-n11` or `--paint n11.strength=0.15` to tone it.
- **Policy:** no area Grass Surface of any kind. import-surfaces-grass.json is no longer written and its runner step is gone. The sand area surfaces stay for the comparison.
- **Canyon road:** waiting for hm-rebuild's new inner heightmap. The road-terrain check re-runs against it when it is reported (`--cs2-heightmap <png>`).
