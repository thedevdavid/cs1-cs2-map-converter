# Conversion outputs (2026-10-20)

Dump: `C:\Users\dleva\AppData\Local/DakotaExport/2972879554_Dakota_Falls-20261019-063123.json`. Run it all with `node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5]`. Import order (bridges: import-bridges.json right after the network; round 10, owner review of A-v4: no quays, retaining walls, PO walls, monorail decor or dam files; dam by hand, see manual-objects.md): network → gaps → power, canals, ship paths → water (import-water.json at sea level 97.5 / 195, or the variant import-water-sea93.json / -sea186.json at the measured 93 / 186; try the variant first, fall back to 97.5) → connections → objects (incl. barrier props) → trees (shared files + the per-scale fixed-height file) → resources → surfaces (vanilla only: fields, sand, PO pavement; grass optional; the pack surfaces rock, PO rock, decals and pack sand are in optional/, runner --with) → experiments (order 9: net-corridor trees, unmerged decals and grass, instead of or on top of the default files); each file is its own import.
out/ = 2× heights (Height Scale 4096), out/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
| 1 | `out\import-all.json` | network (2×) | 3827 edges, 3696 nodes | 2.0 MB |
| 1 | `out\import-bridges.json` | bridge overrides (right after import-all.json, which leaves these decks out) (2×) | 56 edges, replaces 152 | 0.0 MB |
| 1 | `out\import-bridges-fixed.json` | fixed-span bridges: BR001 merged suspension (one 976 m edge) + BR022 arches, right after import-bridges.json (2×) | 9 edges, replaces 151 | 0.0 MB |
| 1 | `out\import-gaps.json` | ROUND 9 network gaps (snapToSegment onto the live network) (2×) | 6 edges | 0.0 MB |
| 1 | `out\import-gaps-e15883-taper.json` | ROUND 9 alternative for e15883_0 (4-lane taper); import instead of its gap_e15883_3lane (2×) | 2 edges | 0.0 MB |
| 1 | `out\x1\import-all.json` | network (1×) | 3827 edges, 3696 nodes | 2.0 MB |
| 1 | `out\x1\import-bridges.json` | bridge overrides (right after import-all.json, which leaves these decks out) (1×) | 56 edges, replaces 152 | 0.0 MB |
| 1 | `out\x1\import-bridges-fixed.json` | fixed-span bridges: BR001 merged suspension (one 976 m edge) + BR022 arches, right after import-bridges.json (1×) | 9 edges, replaces 151 | 0.0 MB |
| 1 | `out\x1\import-gaps.json` | ROUND 9 network gaps (snapToSegment onto the live network) (1×) | 6 edges | 0.0 MB |
| 1 | `out\x1\import-gaps-e15883-taper.json` | ROUND 9 alternative for e15883_0 (4-lane taper); import instead of its gap_e15883_3lane (1×) | 2 edges | 0.0 MB |
| 2 | `out\import-power.json` | power (2×) | 58 edges | 0.0 MB |
| 2 | `out\import-canals.json` | canals and flood walls (2×) | 16 edges: Narrow Boatway 14, Sound Barrier 2 | 0.0 MB |
| 2 | `out\import-shippaths.json` | ship paths (2×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam, connectTo high voltage (bridge 0.9.13; undo the replaced edges first) (2×) | 3 edges, replaces e5094_0, e32855_0 | 0.0 MB |
| 2 | `out\x1\import-power.json` | power (1×) | 58 edges | 0.0 MB |
| 2 | `out\x1\import-canals.json` | canals and flood walls (1×) | 16 edges: Narrow Boatway 14, Sound Barrier 2 | 0.0 MB |
| 2 | `out\x1\import-shippaths.json` | ship paths (1×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\x1\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam, connectTo high voltage (bridge 0.9.13; undo the replaced edges first) (1×) | 3 edges, replaces e5094_0, e32855_0 | 0.0 MB |
| 3 | `out\import-water.json` | water (2×), seaLevel 195 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\import-water-sea186.json` | water VARIANT (2×), seaLevel 186: import instead of import-water.json to try the measured level | 45 sources | 0.0 MB |
| 3 | `out\import-water-lakes.json` | ROUND 9 lake sources for unexplained wet bodies (after the water file) (2×) | 18 sources | 0.0 MB |
| 3 | `out\x1\import-water.json` | water (1×), seaLevel 97.5 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\x1\import-water-sea93.json` | water VARIANT (1×), seaLevel 93: import instead of import-water.json to try the measured level | 44 sources | 0.0 MB |
| 3 | `out\x1\import-water-lakes.json` | ROUND 9 lake sources for unexplained wet bodies (after the water file) (1×) | 18 sources | 0.0 MB |
| 4 | `out\import-connections.json` | outside connections | 25 | 0.0 MB |
| 4 | `out\x1\import-connections.json` | outside connections | 25 | 0.0 MB |
| 5 | `out\import-walls-props.json` | barrier props (2×) | 621 | 0.1 MB |
| 5 | `out\import-objects.json` | objects (2×) | 6130 (prop 3040, building 5, plant 3076, tree 9; 1189 with y) | 1.3 MB |
| 5 | `out\import-substation.json` | ROUND 8 substation building TransformerStation01 (2×) | 1 | 0.0 MB |
| 5 | `out\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (2×) | 793 | 0.2 MB |
| 5 | `out\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (2×) | 17555 | 3.2 MB |
| 5 | `out\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (2×) | 112 | 0.0 MB |
| 5 | `out\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (2×) | 2 | 0.0 MB |
| 5 | `out\x1\import-walls-props.json` | barrier props (1×) | 621 | 0.1 MB |
| 5 | `out\x1\import-objects.json` | objects (1×) | 6130 (prop 3040, building 5, plant 3076, tree 9; 1189 with y) | 1.3 MB |
| 5 | `out\x1\import-substation.json` | ROUND 8 substation building TransformerStation01 (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (1×) | 793 | 0.2 MB |
| 5 | `out\x1\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (1×) | 17555 | 3.2 MB |
| 5 | `out\x1\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (1×) | 112 | 0.0 MB |
| 5 | `out\x1\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (1×) | 2 | 0.0 MB |
| 6 | `out\import-trees-fixed-01.json` | trees at fixed height, with y (2×) | 50 | 0.0 MB |
| 6 | `out\x1\import-trees-fixed-01.json` | trees at fixed height, with y (1×) | 50 | 0.0 MB |
| 6 | `out\trees/import-trees-01.json` … `import-trees-10.json` | trees (both scales) | 496,318 in 10 files | |
| 7 | `out\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 7 | `out\x1\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 8 | `out\import-surfaces-fields.json` | field surfaces | 56 polygons | 0.1 MB |
| 8 | `out\import-surfaces-sand-smooth.json` | sand (vanilla Sand Surface 01, smoothed blobs) | 387 polygons | 0.2 MB |
| 8 | `out\import-surfaces-grass.json` | grass surfaces (optional alternative to the grass plants) | 51 polygons | 0.0 MB |
| 8 | `out\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (2×) | 79 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-fields.json` | field surfaces | 56 polygons | 0.1 MB |
| 8 | `out\x1\import-surfaces-sand-smooth.json` | sand (vanilla Sand Surface 01, smoothed blobs) | 387 polygons | 0.2 MB |
| 8 | `out\x1\import-surfaces-grass.json` | grass surfaces (optional alternative to the grass plants) | 51 polygons | 0.0 MB |
| 8 | `out\x1\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (1×) | 79 polygons | 0.0 MB |
| 9 | `out\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\x1\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\trees/experiment-trees-net-corridor-01.json` | EXPERIMENT: trees on ground-net corridors (both scales) | 641 | |

Reports (both folders): `water-check.md`, `surfaces-summary.md`, `surfaces.geojson`, `manual-objects.md`, `connections-check.md`, `stats.md`.

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
