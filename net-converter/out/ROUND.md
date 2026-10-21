# Conversion outputs (2026-10-21)

Dump: `C:\Users\dleva\AppData\Local\DakotaExport\2972879554_Dakota_Falls-20261019-063123.json`. Run it all with `node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5]`. Import order (bridges: import-bridges.json right after the network; round 10, owner review of A-v4: no quays, retaining walls, PO walls, monorail decor or dam files; dam by hand, see manual-objects.md): network → gaps → power, canals, ship paths → water (import-water.json at sea level 97.5 / 195, or the variant import-water-sea93.json / -sea186.json at the measured 93 / 186; try the variant first, fall back to 97.5) → connections → objects (incl. barrier props) → trees (shared files + the per-scale fixed-height file) → resources → dam (one network edge) after the bridges → … → surfaces (vanilla paint plan: fields, PO pavement, sand, dry grass; grass optional; the pack surfaces rock, PO rock, decals and pack sand are in optional/, runner --with) → experiments (order 9: net-corridor trees, unmerged decals and grass, instead of or on top of the default files); each file is its own import.
out/ = 2× heights (Height Scale 4096), out/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
| 0 | `out\import-terrain-paint-sand.json` | ROUND 24 terrain paint Sand (CS1 sand), softEdge 6 m (2×) | 574 polygons, 0 strokes | 4.2 MB |
| 0 | `out\import-terrain-paint-dry-grass.json` | ROUND 24 terrain paint Dry (plains + south-facing slopes), softEdge 6 m (2×) | 947 polygons, 0 strokes | 4.8 MB |
| 0 | `out\import-terrain-paint-bare-earth.json` | ROUND 24 terrain paint Dirt (11 m strokes at the CS1 dirt decals) (2×) | 0 polygons, 256 strokes | 0.0 MB |
| 0 | `out\optional\import-terrain-paint-rock.json` | rock mask, NOT imported (no rock material) (2×) | 214 polygons, 0 strokes | 0.2 MB |
| 0 | `out\import-terrain-paint-n1-riparian-lush.json` | ROUND 27 terrain paint N1 Lush (2×) | 153 polygons, 0 strokes | 0.7 MB |
| 0 | `out\import-terrain-paint-n2-pit-banks-dirt.json` | ROUND 27 terrain paint N2 Dirt (2×) | 0 polygons, 13 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n2-pit-banks-sand.json` | ROUND 27 terrain paint N2 Sand (2×) | 0 polygons, 8 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n3-drawdown-dirt.json` | ROUND 27 terrain paint N3 Dirt (2×) | 0 polygons, 134 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n4-hogback-sand.json` | ROUND 27 terrain paint N4 Sand (2×) | 0 polygons, 31644 strokes | 5.1 MB |
| 0 | `out\import-terrain-paint-n5-scree-dirt.json` | ROUND 27 terrain paint N5 Dirt (2×) | 200 polygons, 0 strokes | 0.9 MB |
| 0 | `out\import-terrain-paint-n6-savanna-dirt.json` | ROUND 27 terrain paint N6 Dirt (2×) | 0 polygons, 40027 strokes | 6.4 MB |
| 0 | `out\import-terrain-paint-n6-savanna-dry.json` | ROUND 27 terrain paint N6 Dry (2×) | 1903 polygons, 0 strokes | 20.6 MB |
| 0 | `out\import-terrain-paint-n7-prairie-dog-dirt.json` | ROUND 27 terrain paint N7 Dirt (2×) | 0 polygons, 215 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n7-prairie-dog-dry.json` | ROUND 27 terrain paint N7 Dry (2×) | 2 polygons, 0 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n8-playa-centre-dirt.json` | ROUND 27 terrain paint N8 Dirt (2×) | 162 polygons, 0 strokes | 0.6 MB |
| 0 | `out\import-terrain-paint-n8-playa-rim-lush.json` | ROUND 27 terrain paint N8 Lush (2×) | 191 polygons, 0 strokes | 0.8 MB |
| 0 | `out\import-terrain-paint-n9-dirt-roads-dirt.json` | ROUND 27 terrain paint N9 Dirt (2×) | 0 polygons, 12253 strokes | 2.0 MB |
| 0 | `out\import-terrain-paint-n10-rail-ballast-dirt.json` | ROUND 27 terrain paint N10 Dirt (2×) | 0 polygons, 4357 strokes | 0.7 MB |
| 0 | `out\import-terrain-paint-n11-oil-dirt.json` | ROUND 27 terrain paint N11 Dirt (2×) | 425 polygons, 0 strokes | 8.5 MB |
| 0 | `out\import-terrain-paint-n11-ore-dirt.json` | ROUND 27 terrain paint N11 Dirt (2×) | 196 polygons, 0 strokes | 1.8 MB |
| 0 | `out\import-terrain-paint-n12-fertile-dry.json` | ROUND 27 terrain paint N12 Dry (2×) | 475 polygons, 0 strokes | 4.3 MB |
| 0 | `out\import-terrain-paint-n13-high-ground-dry.json` | ROUND 27 terrain paint N13 Dry (2×) | 175 polygons, 0 strokes | 2.7 MB |
| 0 | `out\x1\import-terrain-paint-sand.json` | ROUND 24 terrain paint Sand (CS1 sand), softEdge 6 m (1×) | 574 polygons, 0 strokes | 4.2 MB |
| 0 | `out\x1\import-terrain-paint-dry-grass.json` | ROUND 24 terrain paint Dry (plains + south-facing slopes), softEdge 6 m (1×) | 947 polygons, 0 strokes | 4.8 MB |
| 0 | `out\x1\import-terrain-paint-bare-earth.json` | ROUND 24 terrain paint Dirt (11 m strokes at the CS1 dirt decals) (1×) | 0 polygons, 256 strokes | 0.0 MB |
| 0 | `out\x1\optional\import-terrain-paint-rock.json` | rock mask, NOT imported (no rock material) (1×) | 214 polygons, 0 strokes | 0.2 MB |
| 0 | `out\x1\import-terrain-paint-n1-riparian-lush.json` | ROUND 27 terrain paint N1 Lush (1×) | 153 polygons, 0 strokes | 0.7 MB |
| 0 | `out\x1\import-terrain-paint-n2-pit-banks-dirt.json` | ROUND 27 terrain paint N2 Dirt (1×) | 0 polygons, 13 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n2-pit-banks-sand.json` | ROUND 27 terrain paint N2 Sand (1×) | 0 polygons, 8 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n3-drawdown-dirt.json` | ROUND 27 terrain paint N3 Dirt (1×) | 0 polygons, 134 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n4-hogback-sand.json` | ROUND 27 terrain paint N4 Sand (1×) | 0 polygons, 31644 strokes | 5.1 MB |
| 0 | `out\x1\import-terrain-paint-n5-scree-dirt.json` | ROUND 27 terrain paint N5 Dirt (1×) | 200 polygons, 0 strokes | 0.9 MB |
| 0 | `out\x1\import-terrain-paint-n6-savanna-dirt.json` | ROUND 27 terrain paint N6 Dirt (1×) | 0 polygons, 40027 strokes | 6.4 MB |
| 0 | `out\x1\import-terrain-paint-n6-savanna-dry.json` | ROUND 27 terrain paint N6 Dry (1×) | 1903 polygons, 0 strokes | 20.6 MB |
| 0 | `out\x1\import-terrain-paint-n7-prairie-dog-dirt.json` | ROUND 27 terrain paint N7 Dirt (1×) | 0 polygons, 215 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n7-prairie-dog-dry.json` | ROUND 27 terrain paint N7 Dry (1×) | 2 polygons, 0 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n8-playa-centre-dirt.json` | ROUND 27 terrain paint N8 Dirt (1×) | 162 polygons, 0 strokes | 0.6 MB |
| 0 | `out\x1\import-terrain-paint-n8-playa-rim-lush.json` | ROUND 27 terrain paint N8 Lush (1×) | 191 polygons, 0 strokes | 0.8 MB |
| 0 | `out\x1\import-terrain-paint-n9-dirt-roads-dirt.json` | ROUND 27 terrain paint N9 Dirt (1×) | 0 polygons, 12266 strokes | 2.0 MB |
| 0 | `out\x1\import-terrain-paint-n10-rail-ballast-dirt.json` | ROUND 27 terrain paint N10 Dirt (1×) | 0 polygons, 4418 strokes | 0.7 MB |
| 0 | `out\x1\import-terrain-paint-n11-oil-dirt.json` | ROUND 27 terrain paint N11 Dirt (1×) | 428 polygons, 0 strokes | 8.5 MB |
| 0 | `out\x1\import-terrain-paint-n11-ore-dirt.json` | ROUND 27 terrain paint N11 Dirt (1×) | 196 polygons, 0 strokes | 1.8 MB |
| 0 | `out\x1\import-terrain-paint-n12-fertile-dry.json` | ROUND 27 terrain paint N12 Dry (1×) | 475 polygons, 0 strokes | 4.3 MB |
| 0 | `out\x1\import-terrain-paint-n13-high-ground-dry.json` | ROUND 27 terrain paint N13 Dry (1×) | 175 polygons, 0 strokes | 2.7 MB |
| 1 | `out\import-all.json` | network (2×) | 3781 edges, 3648 nodes | 1.9 MB |
| 1 | `out\import-bridges.json` | bridge overrides (right after import-all.json, which leaves these decks out) (2×) | 54 edges, replaces 150 | 0.0 MB |
| 1 | `out\import-bridges-fixed.json` | fixed-span bridges: BR001 Detroit suspension (one edge) + its 3-lane connectors, right after import-bridges.json (2×) | 5 edges, replaces 55 | 0.0 MB |
| 1 | `out\import-dam.json` | dam: one Hydroelectric_Power_Plant_01 Dam edge (bridge 0.9.23 builds the plant), after the bridges (2×) | 1 edges, replaces 0 | 0.0 MB |
| 1 | `out\import-gaps.json` | ROUND 9 network gaps (snapToSegment onto the live network) (2×) | 4 edges | 0.0 MB |
| 1 | `out\import-gaps-e15883-taper.json` | ROUND 9 alternative for e15883_0 (4-lane taper); import instead of its gap_e15883_3lane (2×) | 2 edges | 0.0 MB |
| 1 | `out\x1\import-all.json` | network (1×) | 3793 edges, 3658 nodes | 1.9 MB |
| 1 | `out\x1\import-bridges.json` | bridge overrides (right after import-all.json, which leaves these decks out) (1×) | 43 edges, replaces 139 | 0.0 MB |
| 1 | `out\x1\import-bridges-fixed.json` | fixed-span bridges: BR001 Detroit suspension (one edge) + its 3-lane connectors, right after import-bridges.json (1×) | 5 edges, replaces 55 | 0.0 MB |
| 1 | `out\x1\import-dam.json` | dam: one Hydroelectric_Power_Plant_01 Dam edge (bridge 0.9.23 builds the plant), after the bridges (1×) | 1 edges, replaces 0 | 0.0 MB |
| 1 | `out\x1\import-gaps.json` | ROUND 9 network gaps (snapToSegment onto the live network) (1×) | 4 edges | 0.0 MB |
| 1 | `out\x1\import-gaps-e15883-taper.json` | ROUND 9 alternative for e15883_0 (4-lane taper); import instead of its gap_e15883_3lane (1×) | 2 edges | 0.0 MB |
| 2 | `out\import-power.json` | power (2×) | 58 edges | 0.0 MB |
| 2 | `out\import-canals.json` | canals and flood walls (2×) | 16 edges: Narrow Boatway 14, Sound Barrier 2 | 0.0 MB |
| 2 | `out\import-shippaths.json` | ship paths (2×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam (A-v6: dam end on the plant marker, extra pylon), connectTo high voltage radius 60 (2×) | 4 edges | 0.0 MB |
| 2 | `out\x1\import-power.json` | power (1×) | 58 edges | 0.0 MB |
| 2 | `out\x1\import-canals.json` | canals and flood walls (1×) | 16 edges: Narrow Boatway 14, Sound Barrier 2 | 0.0 MB |
| 2 | `out\x1\import-shippaths.json` | ship paths (1×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\x1\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam (A-v6: dam end on the plant marker, extra pylon), connectTo high voltage radius 60 (1×) | 4 edges | 0.0 MB |
| 3 | `out\import-water.json` | water (2×), seaLevel 195 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\import-water-sea186.json` | water VARIANT (2×), seaLevel 186: import instead of import-water.json to try the measured level | 45 sources | 0.0 MB |
| 3 | `out\import-water-lakes.json` | ROUND 9 lake sources for unexplained wet bodies (after the water file) (2×) | 18 sources | 0.0 MB |
| 3 | `out\x1\import-water.json` | water (1×), seaLevel 97.5 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\x1\import-water-sea93.json` | water VARIANT (1×), seaLevel 93: import instead of import-water.json to try the measured level | 44 sources | 0.0 MB |
| 3 | `out\x1\import-water-lakes.json` | ROUND 9 lake sources for unexplained wet bodies (after the water file) (1×) | 18 sources | 0.0 MB |
| 4 | `out\import-connections.json` | outside connections | 25 | 0.0 MB |
| 4 | `out\x1\import-connections.json` | outside connections | 25 | 0.0 MB |
| 5 | `out\import-walls-props.json` | barrier props (2×) | 621 | 0.1 MB |
| 5 | `out\import-objects.json` | objects (2×) | 6119 (prop 3029, building 5, plant 3076, tree 9; 1073 with y) | 1.3 MB |
| 5 | `out\import-substation.json` | ROUND 8 substation building TransformerStation01 (2×) | 1 | 0.0 MB |
| 5 | `out\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (2×) | 613 | 0.1 MB |
| 5 | `out\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (2×) | 17557 | 3.2 MB |
| 5 | `out\import-po-structure-objects.json` | ROUND 22 PO structures: BR022 pads (BR006 variant a: no kit objects); runner --po-structures (mod 154132) (2×) | 3 | 0.0 MB |
| 5 | `out\import-po-structure-piers.json` | ROUND 22 BR001 / BR003 piers: ITBP caps (140978) on Pillar4x16 stacks (154132), after pillars-off; on by default (2×) | 201 | 0.0 MB |
| 5 | `out\optional\import-po-structure-br006-arches.json` | ROUND 22 BR006 variant b: 7 StoneArchRailBridge (135944) under the deck, runner --with po-br006-arches (2×) | 7 | 0.0 MB |
| 5 | `out\optional\import-po-structure-gantry.json` | ROUND 20 optional gantry HWY Truss Gantry 2L  A (132188), runner --with po-gantry (2×) | 1 | 0.0 MB |
| 5 | `out\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (2×) | 112 | 0.0 MB |
| 5 | `out\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (2×) | 2 | 0.0 MB |
| 5 | `out\x1\import-walls-props.json` | barrier props (1×) | 621 | 0.1 MB |
| 5 | `out\x1\import-objects.json` | objects (1×) | 6119 (prop 3029, building 5, plant 3076, tree 9; 1073 with y) | 1.3 MB |
| 5 | `out\x1\import-substation.json` | ROUND 8 substation building TransformerStation01 (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (1×) | 613 | 0.1 MB |
| 5 | `out\x1\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (1×) | 17553 | 3.2 MB |
| 5 | `out\x1\import-po-structure-objects.json` | ROUND 22 PO structures: BR022 pads (BR006 variant a: no kit objects); runner --po-structures (mod 154132) (1×) | 3 | 0.0 MB |
| 5 | `out\x1\import-po-structure-piers.json` | ROUND 22 BR001 / BR003 piers: ITBP caps (140978) on Pillar4x16 stacks (154132), after pillars-off; on by default (1×) | 131 | 0.0 MB |
| 5 | `out\x1\optional\import-po-structure-br006-arches.json` | ROUND 22 BR006 variant b: 7 StoneArchRailBridge (135944) under the deck, runner --with po-br006-arches (1×) | 7 | 0.0 MB |
| 5 | `out\x1\optional\import-po-structure-gantry.json` | ROUND 20 optional gantry HWY Truss Gantry 2L  A (132188), runner --with po-gantry (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (1×) | 112 | 0.0 MB |
| 5 | `out\x1\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (1×) | 2 | 0.0 MB |
| 6 | `out\import-trees-fixed-01.json` | trees at fixed height, with y (2×) | 50 | 0.0 MB |
| 6 | `out\x1\import-trees-fixed-01.json` | trees at fixed height, with y (1×) | 50 | 0.0 MB |
| 6 | `out\trees/import-trees-01.json` … `import-trees-11.json` | trees (both scales) | 504,894 in 11 files | |
| 7 | `out\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 7 | `out\x1\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 8 | `out\import-surfaces-fields.json` | field surfaces | 55 polygons | 0.1 MB |
| 8 | `out\import-surfaces-sand.json` | paint plan sand: Sand Surface 01 banks, Sand Surface 02 bare earth + decal spots | 99 polygons | 0.3 MB |
| 8 | `out\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (2×) | 11 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-fields.json` | field surfaces | 55 polygons | 0.1 MB |
| 8 | `out\x1\import-surfaces-sand.json` | paint plan sand: Sand Surface 01 banks, Sand Surface 02 bare earth + decal spots | 99 polygons | 0.3 MB |
| 8 | `out\x1\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (1×) | 11 polygons | 0.0 MB |
| 9 | `out\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\x1\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\trees/experiment-trees-net-corridor-01.json` | EXPERIMENT: trees on ground-net corridors (both scales) | 726 | |

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

## Round 28: Detroit test split (A-v8 crashed natively during the combined `detroit-test` step)

- import-detroit-test.json is gone. It is replaced by import-detroit-opt2.json (option 2: 2 × 362 m BXP twoway 6-lane spans + 2 plain stubs, 5 nodes) and
  import-detroit-opt3.json (option 3: the double-deck upper and lower decks + 4 stubs, 8 nodes), at both scales.
- Runner: `detroit-opt2` / `detroit-opt3` are opt-in only (`--with detroit-opt2` or `--with detroit-opt3`) and come after `verify-end`, so a crash there never costs
  the map. Save the map first, and try opt2 alone before opt3 (opt3 holds the Lower Highway net the crash is suspected on).

## Round 29: look / textures steps; the 3 A-v8 network failures

- Runner `look`: posts look/terrain-cs1.json, no longer terrain.json. terrain.json selected the read-only vanilla DefaultTerrainRenderSettings and dropped the
  CS1 copy with its textures.
- Runner `textures` (new, right after `look`; `--skip textures` leaves them alone; `--textures-dir` for another set, default look/textures/gain16, fallback
  look/textures/raw): POST /map/terrain-textures with the 14 files, slots Rock (Cliff_*), Dirt, Grass, Extra1–4. It polls /map/terrain-textures/status every 2 s
  (up to 10 min) until the job is no longer running, then prints done / failed and one line per file. Tested against a mock bridge only (14 queued, 14 done).
- The look group stays last: settings → look → textures, then the final net count.
- A-v8 failures (live-graph-20261020-231951-end.json). None is missing. All three run along live segments of their prefab; only the ends differ:
  - e31685_0 Highway Oneway - 2 lanes, (3652, 1881) → (3695, 1841), a 4-way junction with two 1-lane ramps and a 4-lane road: t 0.2 / 0.5 lie 0.00 m on live
    64346:35. That piece is only 29.7 m long (our course is 59 m); at t 0.8 the nearest pieces are the 1-lane ramp 3.5 m away and the 4-lane road. The game moved the
    junction about 20 m along the carriageway (acute ramp joins).
  - e34504_0 Highway Oneway - 2 lanes, (3810, 2322) → (3755, 2261), the same type of junction: t 0.5 / 0.8 lie 0.00 m on live 65990:19 (45.7 m of an 82 m course);
    t 0.2 is 2.9 m off a 1-lane ramp.
  - e35131_0 Gravel Road, (6910, 2817) → (6846, 2817), curved 64 m: live 97595:9 follows it within 0.45 m at t 0.2 but runs 6.2 m off the middle (the game
    straightened the curve), and the far end now belongs to two 128 m pieces 8.5 m off (a straight run re-split at the junction).
  These are game-side relocations at acute or folded junctions, not losses. The runner's 2 m re-check is strict on purpose and lists them as missing; bridge 0.9.33
  marks the folds as merged.

## Round 30: Detroit option 3 out

`import-detroit-opt3` crashed the game natively twice, even with the lower deck stripped: the `BXP Double Deck Suspension Bridge - Highway` upper deck alone
is enough. Its file now lives only in optional/import-detroit-opt3.json, with a CRASHES note, and has no runner step. `import-detroit-opt2` imported fine
(2 spans created, 2 stubs merged) and stays the opt-in `--with detroit-opt2` step after verify-end.

## Round 31: fidelity check (owner review of A-v8: "that interchange is completely messed up")

`tools/fidelity.mjs` draws a top-down SVG per interstate interchange (CS1 nodes where ≥ 3 segments meet and one is a highway or ramp, clustered within 350 m;
16 clusters) and per bridge override with its approaches (21). Each shows CS1 (blue where elevated), our import files (green) and the live A-v8 graph (red, dashed
where elevated). It flags:
- orange: a CS1 segment with no live segment within 2 m of its midpoint;
- magenta: a live segment with no CS1 source;
- blue: live more than 4 m off the CS1 height at that point.
Each image has its own .md with the tables and, for every missing CS1 segment, the converter's recorded fate. The fates are now written to segment-fates.tsv.
Index with a verdict per image: out/x1/fidelity/index.md (37 images; 20 OK).

**(2995, 820)** (interchange 9 at (2872, 913) and 16 at (3015, 1505) cover it): OK. All 64 CS1 segments in the 800 m box have a live counterpart within
2 m, live and CS1 agree within 4 m in height everywhere, and there are 0 live segments without a CS1 source. Prefabs map one to one (USRP 1L/2L/3L/4L
Interstate → Highway Oneway 1/2/3/4 lanes). Junctions: 5 with ≥ 3 segments in both, 0 dead ends in both. In A-v7 the branch was missing: the round-12 stub
removal and the round-19 prune had taken out 36 CS1 segments, and round 26 put them back, so A-v8 has the CS1 layout. A difference the owner sees beyond
this (lane markings, how CS2 draws the merges, the USRP assets' looks) is not in the network data; a CS1 screenshot from the same camera would show what is meant.

Remaining CHECKs, by cause:
- **Our deliberate changes:** the BR001 carriageways replaced by the merged deck (28 CS1 segments); the round-26 crossing raises (+4 to +5 m: interchange
  11, BR009, BR019); the gravel trestles lifted to 8.5 m with 3 % climbs (BR023, 026, 033, 038, 049, 050, 053: 7–12 segments each). If CS1 heights matter more
  than the bridge rule there, those lifts can go back to CS1 heights with the plain base road.
- **The heightmap** (hm-rebuild's carve is pending): the canyon road, interchanges 14 and 15 (National Road 11–24 m, 2L Interstate 4–8 m).
- **Bridge-side:** interchange 8 / the BR001 south approach. CS1 segments 14574 / 35648 are at y 139.8–140.7 in our file (= CS1), but the live
  64661:3 / 64743:3 stand at y 168.7–178.3 (elevation ≈ 110 m). The importer does not keep our absolute heights there, the same mechanism as the sagging curved decks.
- **Fate list:** 16370 and 19410 lie outside the border, 31152 collapsed (< 4 m), 16061 was folded into its neighbour; e35131 is the A-v8 junction the game moved
  (round 29).

Detroit: the owner keeps the BXP SuspensionBridge03; import-detroit-opt2.json now lives only in optional/, and the `detroit-opt2` step (opt-in only) points there.

## Round 32: CS1 flyovers on pillars (owner: the CS1 reference shot next-run/cs1-reference-interchange-12.jpg)

- **The shot is interchange 5 at (3493, 2152).** It is the only stack: 139 highway segments, 68 ramps, 32 of them more than 6 m above the terrain, up to 27.4 m (CS1
  33741). Interchanges 9 and 16 near (2995, 820) have at most 5 and 3 such segments, none above 12 m.
  - Matching CS2 camera (estimate): pivot (3493, 2152), yaw 330° (looking north-north-west, across the main through road at bearing 60°), pitch 45°,
    zoom about 900.
- **Per segment:** out/x1/fidelity/interchange-5-ramps.md compares CS1 height, CS1 node elevation and flags with the A-v8 live elevation. Of the 32 CS1
  segments more than 6 m up, 29 were ≥ 8 m live (pillars), 2 sat in the 0.5–8 m band (retaining walls) and 1 on the ground. Below 6 m, most of the CS1
  'Ramp E' / '2L Interstate E' pieces at 4–6 m were 5–8 m live, so walls.
- **Rule** (FlyoverLift.cs, map-wide, before the bridge overrides): every course that was an elevated (bridge-AI) piece in CS1 goes to ≥ 8.5 m above the terrain
  (output metres).
  - Nodes where such a chain meets a ground or tunnel course, border nodes and tunnel nodes stay fixed.
  - The lift ramps from the nearest fixed node at ≤ 8 % along the elevated courses. No node is lowered.
  - Rail is not included.
  - **Threshold assumption** (the bridge session has not answered): the A-v6/A-v7 /prefabs/info notes show Highway Oneway elevationLimit 4 m, and CS2 builds
    walls at 1–2 × that, so 8 m is the pillar threshold (= minElevatedHeight 8 for bridges); 8.5 m gives a margin.
- **Result:**
  - x1: 174 nodes lifted on 250 CS1-elevated courses, mean +1.9 m, max +5.3 m.
  - x2: 47 nodes on 85 courses (the doubled heights put most of them above 8.5 m already).
  - The list is in flyover-lifts.md, per scale.
  - Interchange 5 afterwards: 67 of its 71 CS1-elevated courses reach ≥ 8 m. The other 4 are the touch-down pieces the 8 % grade keeps lower: e9026_0a 7.1/7.2,
    e9203_0b 6.9/3.8, e14428_0 −1.0/7.2, e34845_0b 7.9/6.9 (estimated against the live terrain).

## Round 32b: bridge 0.9.39 "elevated" flag; control-point heights fixed

- **`elevated` flag:** every course that was a CS1 elevated (bridge-AI) road piece and is still elevated after the flyover lift gets `"elevated": true` (bridge 0.9.39).
  - The bridge then stores 8.05 m on its segments and inner nodes, so CS2 builds pillars instead of retaining walls, also on the touch-down pieces the 8 %
    grade keeps below 8 m.
  - Count: x1 505 edges, x2 479. Rail is not flagged.
  - Unverified in game: the bridge has not tested the flag yet, and a later net-tool edit of such a segment reverts it.
- **Control-point y:** written in output metres like the nodes. They were topology heights before (2 × the node y at x1); the ratio is now 1.000 at both scales.
  The BR001 fork control points in import-bridges-fixed.json no longer have y 0: they sit on the straight grade between the ends. The bridge ignores control
  y today.

## Round 32c: interchange 5 check, clearance guard, dump pinned

- **Dump:** run-all picked the newest JSON in DakotaExport. Since 2026-10-20 18:37 that was a CS1 autosave from the owner's shot session (38 segments differ from
  the map dump), so commits 63962cf and 6a98d1e were generated from it. run-all now takes only `<id>_Dakota_Falls-<stamp>.json`; this round is regenerated
  from 2972879554_Dakota_Falls-20261019-063123.json again.
- **Clearance guard (FlyoverLift):** a lifted course keeps min(its CS1 clearance, 7 m) under any course crossing above it, at 7 crossings at x1.
- **Interchange 5** (3493, 2152), full tables in out/x1/fidelity/interchange-5-ramps.md:
  - 77 of its 178 highway edges carry `elevated: true`.
  - 38 crossings without a shared node: the least clearance is now 7.0 m (e6041_0b over e12097_0 at (3646, 2130); CS1 7.7 m, before the guard 5.8 m). None is
    under 6 m.
  - Levels at the centre: ground 171–174 m, +1 at 180–187 m, +2 at 189–198 m, a top ramp at 200–201 m (CS1 33741 at 27 m), so 3–4 levels as in CS1.
- **Cameras:** the owner's (3493, 2152), yaw 0, pitch 50, distance 700. To show the levels: (3493, 2152), yaw 45 (from the south-west, looking north-east
  along the main road at 60°), pitch 30, distance 600.

## Round 33: the A-v9 network failures (run-20261021-025202, 10 against 3 in A-v8)

Diffed against live-graph-20261021-025202-end.json and against the A-v8 files (1a79162):
- **Dropped as "runs inside the existing segment"** (a new bridge 0.9.39 check; all three were built in A-v8):
  - e31224_0ba, 4 lanes: a real overlap with the parallel 3-lane carriageway e25_0 (16.3 m apart, 18 m needed). Its run-report drop is now read and it moves
    3.2 m clear (the asked 2.2 m + 1 m).
  - e22564_0b, 2 lanes, and e31224_0bb, 4 lanes: the segment the game names is their sibling at the same node, a merge or diverge (e9919_0 1-lane, and the
    2-lane legs e15395_0a / e27293_0). Moving them would bend the junction; not moved and listed in stats.md for the bridge. This looks like a false positive
    of the new check at shared nodes. e22564_0b and its sibling also went up 1.6–2.5 m with the round-32 flyover lift.
- **Point-check 0 or 1 of 3** (e16410_0a, e24407_0ba, e24407_0bb, e35437_0b): high BR001 south-approach pieces (live neighbours 19–41 m up). Their heights are
  the same as in A-v8, where they were built. The one change is the round-32 `elevated: true` flag, and they are gone from the live graph.
  - Change: the flag is now written only where an end is below 8 m (where it changes the look). x1: 505 → 155 edges, x2: 479 → 93. None of the four carries it.
  - For the bridge session: check what the flag does to segments already above 8.05 m.
- **The three known ones** (e31685_0, e34504_0, e35131_0) are the junctions the game moved (round 29).
- **The run-report reader:** it now covers highway courses (FootprintShift with the drop messages), only where a separate course of the named prefab is
  closer than the two half-widths + 0.5 m.

## Round 34: owner's A-v9 review (14 / 15 disconnected ramps, 17 island trees, 22 / 24 cliffs, 24 the covered road)

**A. Dead ends.** The map-wide list from live-graph-20261021-025202-end.json: 21 live degree-1 nodes off the border, 10 of them not dead ends in CS1 (within 30 m).
The fidelity tool now counts these per box and circles them in the SVG.

| live dead end | cause | fix |
|---|---|---|
| (709, 3928) spot 14 | e22564_0b dropped: "inside" its merge sibling e9919_0 (1-lane) | its free end e22564_0d0 moves 2.7 m away from the sibling; the merge node n4291 stays |
| (884, −4339) spot 15 | e31224_0ba dropped beside the parallel 3-lane carriageway e25_0 (16.3 m, 18 needed) | moved 3.2 m clear (round 33) |
| e31224_0bb, between them | dropped "inside" its diverge sibling e15395_0a | its free end e31224_0bd1 moves 2.2 m away; the diverge node n27662 stays |
| (723, −4579), (782, −4501), (808, −4467), (1086, −4100), (1117, −4059) spot 15 | e16410_0a, e35437_0b, e24407_0ba / _0bb vanished in A-v9, the first run with the `elevated` flag on 20–40 m viaduct pieces | the flag is now only on courses with an end below 8 m (round 33); these four no longer carry it |
| (1431, 4697), (1581, 4564) | the round-26 access roads to the dam plant and the substation end at the buildings | expected |
| (1440, 2585) | rail e28929_0 built 5.1 m off its line, the same in A-v8 and A-v9 | left; separate case |

Fidelity on the A-v9 graph (before these fixes): box 14 has 1 such dead end, box 15 has 4, 22 and 24 have 0. Verification needs A-v10.

**B. Island trees (17).**
- A tree is masked only where the playable heightmap (10201528) is below the 93 m sea + 0.5. The CS1 water-surface raster masks only there too, and the old
  rule (CS1 terrain below 97.5 m, the default sea level) is gone.
- Within 300 m of (4096, −6127): 23 trees kept, 38 still masked (on terrain below 93.5 m).
- Sand on islands (CS1 water raster, playable terrain ≥ 95 m) is no longer painted, as an area surface or as terrain paint. Island tops stay grass.

**C. Cliffs (22, 24).**
- **PO cliff pieces** come back where the playable slope is ≥ 35° (the steepest within half the footprint), one per 16 m cell, yawed along the contour, and sunk
  1.5 m + half the footprint × tan slope. None on a net corridor; none where the playable terrain is below 93.5 m (cliff walls rise out of the river).
  365 pieces map-wide: fort bluff 10, canyon 20, the south rock work 32.
- **CS1 Brown Surface Rock Medium / Large** (the author's rock dressing, mapped to small rocks so far) become CliffStack / CliffRock5 on ≥ 25° slopes, with the same
  yaw, sink and corridor rules: 249 pieces. 35° qualified only 61 on the smoother playable heightmap.

**D. The covered road (24).**
- import-rock-shed.json (runner step `rock-shed`): 14 ConcreteSet01 Wall32x16 along the uphill side of the National Road and 14 Pillar4x16 on the downhill
  side, every 32 m over 444 m, bases at road height, between the first and the last rock_c piece.
- No CS2 tunnel: hm-rebuild is carving this corridor down to the road (road-terrain-mismatch), so there is no rock cover for one.
- Wall32x16's long axis along the road is assumed; check in game.

## Round 34b: the rock shed as a ConcreteSet01 gallery (bridges/ROCK-SHED.md fallback (b))

import-rock-shed.json is one import, name `rock-shed`, runner step after po-piers, on by default. Absolute y, no grounding or sink; ConcreteSet01 pivots are centre-bottom.
It runs along the CS1 road line over 444 m (e10437_0, e13009_0, e20651_0, e24116_0, e31333_0, e32978_0, e34976_0):
- **Roof:** BasePlateConcrete32 bays centred over the road at road y + 6 m, each at its own height. Where two bays would step more than 1 m, two
  BasePlateConcrete16 halves instead: x1 6 full + 16 half bays, x2 4 + 20. The road is steeper than 5 % there, so mostly halves.
- **Valley side:** 15 Pillar2x8 under the slab edges, top at the slab underside; Pillar2x16 where the terrain is more than 8 m below the slab.
- **Hill side:** 14 Wall32x16 from road level, 32 m side along the road.
- **Rock face:** 11 CliffRock5 on top of the wall line at the CS1 rock_c positions.
- No tunnel fallback.

## Round 34c: the rock shed from pylon props (owner: "custom made out of pylon props"; plates and walls gone)

import-rock-shed.json, the same `rock-shed` import, absolute y, no sink, over 444 m of the National Road line:
- **Posts:** 28, every 16 m, post top at road y + 6.5 m. The 4-lane road is 25 m wide (CS2 Medium Road), more than the 13.2 m reach of
  `ITBP_Single-Sided Beam 01`, so by the rule they are `ITBP_Pillar Median 01` T-piers on the centreline (27 m across, crossbeam across the road).
  - All 28 posts (21.5 m) reach down into the ground; none is short. Single-Sided Beam 02–05 and Pillar Median 02–04 are not measured, so variant 01 throughout.
- **Roof:** 37 rock slabs every 12 m resting at road y + 7 m, long axis along the road: CliffRock6 (60.7 × 8.5 × 55.3 flat slab), every third a CliffRock5.
  The CS1 rock_c positions are spread over the whole length.
- **Back:** 14 CliffStack on the hill side every 32 m, base at road level.
- **Alternative if the owner wants the CS1 look of posts on the valley edge only:** Single-Sided Beams there, with two-thirds of the roof width hanging from the
  slabs alone.

## Round 34d: rock shed posts on the valley edge only (owner decision)

- 28 `ITBP_Single-Sided Beam 01` posts on the valley edge every 16 m, cantilever pointing uphill, post top at road y + 6.5 m. No T-piers on the centreline.
- The 37 roof slabs span the rest of the 25 m road to the hill-side rock by themselves (CliffRock6 is 55 m and CliffRock5 36 m across), so one slab per bay.
- 14 CliffStack on the hill side, as before.
- All 28 posts reach into the slope (none short). Beam variants 02–05: to measure in A-v10.

## Round 34e: rock shed as an open-sided avalanche gallery (owner, next-run/ref-gallery-1/2.webp)

import-rock-shed.json, the same `rock-shed` import, absolute y, no sink, over 444 m:
- **Hill side closed:** 14 Wall32x16 from road level.
- **Roof:** BasePlateConcrete32 bays at road + 6.5 m, BasePlateConcrete16 halves where a bay would step more than 1 m. x1: 6 full + 16 half bays, x2: 4 + 20.
- **Valley side open:** 56 columns every 8 m on the valley edge, top at the roof underside. The prop is one constant, `RockShed.Column` + `ColumnHeight`
  (src/NetConverter/RockShed.cs). It is ConcreteSet01 `Pillar2x8` until the vanilla pillar prop is confirmed, so the file imports as it is.
- **Rock:** 11 CliffRock5 behind the wall at the CS1 rock_c positions; no rock roof slabs, no ITBP.
- **Guardrail:** none (no vanilla guardrail prop or net confirmed).
- **Candidates to check live** (`/prefabs?category=object&query=Pillar`, also `query=Column`, `query=Guard`, `query=Rail`, `query=Barrier`; then `/prefabs/info`
  for size and boundsMin of each):
  - the vanilla road / highway / train pillar objects the elevated nets spawn; their names are unknown here; any prefab whose name contains "Pillar" and is not
    ITBP, ConcreteSet01, Pier01–03 or a *NetPillar of a bridge;
  - wanted: about 1–2 m square and 6–10 m tall, pivot at the base;
  - for the guardrail: any object or net with "Guardrail", "Guard Rail", "Crash Barrier" or "Highway Barrier" in the name.

## Round 34f: gallery columns are the vanilla `Vertical Pillar`

The 56 valley-side columns (every 8 m) are now `Vertical Pillar`, the vanilla road stack pillar (PREFAB_NAMES.md, bridge 0.9.42), set in RockShed.Column.
- y = the roof slab underside, yaw = road heading. The bridge passes elevation = y − ground, so each reaches the terrain by itself.
- The rest is unchanged: roof x1 6 + 16 / x2 4 + 20 bays at road + 6.5 m, 14 Wall32x16, 11 CliffRock5 behind the wall, no guardrail.
- Total 103 objects at x1, 105 at x2.
- To check at the start of A-v10: the measured size of Vertical Pillar against the 8 m pitch and the 6.5 m roof offset.

## Round 34g: gallery roof from ConcreteSet01 (154132; BasePlateConcrete62 is part of it, no separate mod)

The roof is square plates at road + 6.5 m, each with its valley edge on the column line (a wider plate reaches back over the wall into the hill):
- BasePlateConcrete62 (ConcreteSet01, 62.5 m) where the road rises ≤ 1.2 m under it, else two BasePlateConcrete32, else BasePlateConcrete16, two side by side
  across the 25 m road. The road there climbs about 5–8 %, so mostly 16 m plates.
- No plate runs past the end of the shed.
- Counts:
  - x1: 1 × 62 + 4 × 32 + 36 × 16 plates, 14 Wall32x16 (154132), 56 Vertical Pillar every 8 m (y = roof underside), 11 CliffRock5. 122 objects.
  - x2 (doubled heights, steeper): 0 × 62 + 2 × 32 + 50 × 16. 133 objects.

## Round 34h: gallery from the packs only (owner: "use the packs, not the vanilla")

No `Vertical Pillar`, no ITBP. Valley-side columns are ConcreteSet01 Pillar2x16 / Pillar2x8 (pivot centre-bottom), stacked from the slab underside down to the
terrain: 16 m pieces while more than 8 m remain, then one 8 m piece. The bottom piece goes into the ground by what is left over.
- Roof (62 / 32 / 16 rule), 14 Wall32x16 and 11 CliffRock5: as in 35f11c8.
- x1: 1 × BasePlateConcrete62 + 4 × 32 + 36 × 16, 14 Wall32x16, 56 Pillar2x8 (one per column), 11 CliffRock5. 122 objects.
- x2: 0 × 62 + 2 × 32 + 50 × 16, 14 Wall32x16, 56 columns (1 Pillar2x16 + 55 Pillar2x8), 11 CliffRock5. 133 objects.

## Round 35: empty import files; N7 back

- **Runner:** a file with no items (edges, objects, surfaces, polygons, strokes, sources all empty) is skipped with "- step: file is empty, skipped" instead
  of being posted; the bridge answers 400 to an empty body. Checked against the mock bridge.
- **N7 prairie dogs** were empty in A-v10. Cause: the round-34 tree mask keeps more trees (the playable 93.5 m rule instead of CS1 water / 97.5 m), so the
  open flat plains with no tree within 100 m shrank from 1.0 to 0.47 km², under the 0.5 km² floor for placing at least one town. The floor is now 0.15 km²
  (a town is 3–15 ha): 1 town, 0.05 km² Dry + 215 Dirt mounds.

## Round 35b: runner phases; the A-v10 failures

**Runner phases.** A-v8 and A-v10 both crashed natively seconds after the terrain render settings post on a fresh, unsaved map; loaded saves never did.
- `--phase build` (default): every import step, settings, look climate + water. No terrain-cs1, no textures.
- `--phase post-save`: only look terrain-cs1 + textures. It keeps run-state.json (it resumes, never resets).
- `--phase all`: the old single pass.
- `--save <name>`: POST /map/save?name= at the end of the build phase. The bridge has only /game/save, which is city-gated (409 in the editor); until /map/save
  exists the runner prints "not available, save the map by hand now" and goes on.
- The runbook: build → save → reload → post-save → save. Checked against the mock bridge.

**A-v10 failures** (run-20261021-121517 + -122852, live-graph-20261021-122852-end.json), per course, the nearest live segment of the same prefab at t 0.2 / 0.5 / 0.8:
- **Spot 14 (817, 4013):** no dead end CS1 does not have. The round-34 sibling move worked.
- **Spot 15 (608, −4721):** 6 such dead ends: (723, −4579), (782, −4501), (808, −4467), (929, −4308), (960, −4266), (976, −4245).
- **The BR001 south-approach deck pieces are really missing** (not folded): e16410_0a, e24407_0ba / _0bb, e31688_0aa / _0ab / _0bb, e35437_0b, e35648_0a / _0ba / _0bb.
  - Nothing of their prefab within 2 m; the nearest is the opposite carriageway, 16.3 m away.
  - Not an overlap: 16.5 m is more than the bridge's 16 m for two 3-lane carriageways, and the bridge reported no overlap drop for them.
  - What they share: deck-fit split pieces (`_0a` / `_0b`), 20–110 m up, noPillars (the BR001 piers stand under them), no `elevated` flag since round 33.
  - They first went missing in A-v9 (bridge 0.9.39, and the first run with control-point y in output metres instead of 2 × node y), and more in A-v10.
  - **For the bridge session:** the batch log for e35648_0ba / e24407_0ba (does the game reject the definition, e.g. elevation range, or remove it after
    building?), and whether 0.9.39+ uses the control-point y.
- **Folded or moved junctions as before:** e31685_0, e34504_0 (2 of 3 points on a live 2-lane piece) and e35131_0 (curve straightened, 0.4 m at t 0.2).

**Carriageway spread** (CarriagewaySpread.cs): parallel one-way highway courses closer than their two CS2 half-widths (the bridge's drop threshold) are pushed
apart to the half-widths + 0.3 m: 17 pairs, about 55 nodes, converged in 3 passes. It does not touch the BR001 pieces above (16.5 m is enough).

## Round 35c: A-v11 failures; runner save polling

**A-v11** (run-20261021-131042, live-graph-20261021-131042-end.json), the 12 point-check failures:
- **Present but folded or moved junctions:** e31685_0, e34504_0 (2 of 3 points lie 0.0 m on a live 2-lane piece) and e35131_0 (0.4 m at t 0.2, the
  curve straightened).
- **Missing:** e16410_0a, e24407_0ba / _0bb, e31688_0ab / _0bb, e35437_0b, e35648_0a / _0ba / _0bb.
  - Nothing of any prefab lies within 12 m of their midpoints.
  - The same set as A-v10, without e31688_0aa (built this time).
- **Spot 14 (817, 4013):** 0 live dead ends CS1 does not have.
- **Spot 15 (608, −4721):** 6: (723, −4579), (782, −4501), (808, −4467), (944, −4288), (960, −4266), (976, −4245).
- **The round-35b carriageway spread** did not touch these pieces (their carriageways are 16.5 m apart, enough), so it cannot fix them.
- **What changed for them since A-v8**, where they were built: diffing the import files, their nodes, structure and noPillars are identical. The only
  difference is the control-point y: A-v8 had the old 2 × node y (~280 m), A-v9 on the corrected node y (~140 m). Bridge 0.9.39 arrived at the same
  time.
  - Suggested test on the saved map: post one of them (e35648_0ba) with the old and with the new control y, or ask the bridge how its build uses control y.

**Runner `--save`:** a 202 answer is now followed by polling GET /map/save/status every second until done or failed (up to 5 min). It prints the package
size and the time, e.g. "saved as … (59.3 MB, 2.0 s)" against the mock.

## Round 36: the BR001 south-approach dead-end chain (A-v11, interchange 8 / Detroit approach)

**Symptom** (live-graph-20261021-135800-end.json, save dakota-A11d-2048). Two sets of dead ends:
- interchange_08: (723, −4579), (782, −4501), (808, −4467), (944, −4288), (960, −4266), (976, −4245), (1054, −4142);
- BR001: (1086, −4100), (1117, −4059).

They are the ends of nine pieces on one carriageway of the viaduct, 20–77 m up, that the game never built:
- e16410_0a, e35437_0b;
- e31688_0ab / _0bb;
- e35648_0a / _0ba / _0bb;
- e24407_0ba / _0bb.

The pieces between them are built, so the course alternates between built and missing.

**Ruled out:**
- *Control-point y.* The bridge reads only x/z of `control` (RequestHandlers.Import.cs: `new float2(c[0], c[2])`).
- *A file change.* These edges, and every course within 60 m, are byte-identical between A-v9 (895da65) and A-v10 (a9afdd9). Yet e31688 and e35648 were built in A-v9 and lost in A-v10, so batch order and timing decide which pieces survive.
- *Height.* The opposite carriageway is built at the same heights, up to 77 m.

**Root cause.** The two carriageways run only 0.3 m clear of each other.
- CS1 runs the USRP carriageways 16.3 m apart, centre to centre.
- The two CS2 decks need 16 m of half-widths (3 + 3 lanes: 8 + 8; 4 + 2 lanes: 10 + 6).
- Every missing piece is the later-built half of such a 0.3 m pair: its opposite piece was posted in an earlier batch (batches of 64, file order).
- 9 of the 15 pairs at 0.3 m clearance lost the later piece. No pair with 2 m or more lost one. For example, e27498 beside the 2-lane e15395 has 2.3 m, and e10926 beside the 4-lane e31224 has 17.3 m between centres but was posted first.
- The round-35 spread did not move these pairs, because 16.3 m is more than the 16 m threshold.
- The bridge gives only "only 0/1/2 of 3 points along the course lie on a segment of this prefab", with no overlap diagnosis. `DescribeOverlap` skips segments of the same prefab, which covers both 3-lane pairs.
- This is inferred from the import files and the live graph, without a game test. The game was kept on A11d with no imports.

**Fix** (CarriagewaySpread.cs):
- The opposite carriageways of one road (parallel within 15°, traffic in opposite directions, no shared node) are pushed to the two half-widths + 2 m (`Margin`), each by half.
- Same-direction pairs (a ramp beside its mainline after a diverge) keep the old rule: moved only when they overlap, to + 0.3 m. Junctions are not bent.
- x1: 98 pairs in pass 1, 172 nodes moved, the largest 1.71 m (n3337 at (3605, 2554)), most under 1 m. Node and edge sets are unchanged.
- All nine BR001 pairs are now 18.0 m centre to centre (2.0 m clear), at both scales.

**Dry fidelity check** (`node tools/file-graph.mjs`, then `node tools/fidelity.mjs --live out/x1/file-graph.json --out out/x1/fidelity-files`):
- New tool, tools/file-graph.mjs: it turns the /import/network files into a /graph/export-shaped graph, keyed by position, so fidelity.mjs can compare CS1 with what we would post.
- interchange_08 and BR001: 0 dead ends and 0 height errors, as with the previous files. The files never had the gaps; the game made them.
- Interchange 8 now lists 6 CS1 segments "without counterpart" in our files (was 1: 1403):
  - 1403, 10926, 11900, 15883, 28471, 31224;
  - all present, but more than 2 m from the CS1 course in plan: the spread on top of earlier moves (e31224_0ba, + 3.2 m in round 33).
- BR001: "+26 replaced by a bridge override" (was 28), the same proximity effect.
- 142 tests pass.

**For A-v12:**
- Check the nine ids above in the post-network graph. A piece still missing at 2 m clearance would point to something else: pillars of the earlier carriageway, or the game's own merge of collinear pieces.
- **For the bridge session:** `DescribeOverlap` should not skip same-prefab segments that are not the course's own collinear neighbours. Then a parallel carriageway overlap is named instead of reported as a bare point-check failure.

## Round 37: the ConcreteSet01 avalanche gallery (owner's A-v11 review, next-run/shots-a11/fix/24-01..07)

**What was wrong:**
- The yaw sign was right. In 24-01 the asymmetric Wall32x16 pieces and the 62.5 m plate's edges run along the road, which matches the bridge's `quaternion.RotateY(yaw)` (local +z → (sin, cos)). The yaw is now derived per piece, as asked.
- The layout was the problem:
  - The 62.5 m plate is 2.5 times the road strip, so it reached over the cliff and into the hill.
  - Every plate took the unsmoothed road y at its own centre, positioned by a chord projection instead of along the road. That gave steps, gaps on the outer side of the curve, and mixed 16 / 32 / 62 m sizes.
  - The 16 m walls stood on the road, so they rose 9.5 m above the roof at 6.5 m. They were 32 m long at a 32 m pitch, which left gaps on the curve.
  - The rocks were set at road + 2.6 m and showed above the wall and roof.

**New layout** (RockShed.cs, ConcreteSet01 only, all pivot centre-bottom per next-run/concreteset01-bounds.txt):
- **Placement along the road.** Every piece is placed by its arc length along the road and turned to the road bearing at its centre (walls: the chord of the hill edge between their ends).
- **One height profile.** The road y is smoothed over 48 m. Roof = profile + 6.5 m; columns, walls and rocks take their heights from it.
- **Roof.** BasePlateConcrete16 only (no 62.5 m plate), two across:
  - one slab with its valley edge on the column line, one with its hill edge on the wall;
  - they overlap in the middle;
  - a row every 8 m, overlapping 8 m along.
  - The road really climbs and drops about 10 m under the shed (x1 260 → 270 → 259 → 274 m), and slabs can only turn about Y, so the roof is terraced. The largest step between rows is 1.49 m in x1 (2.98 m at 2×).
- **Hill-side wall.** Wall32x16 every 16 m (half overlap), top flush with the roof. The rest goes down into the hill cut.
- **Valley side.** A Pillar2x16 / Pillar2x8 stack every 8 m of road under the slab's valley edge, from the roof underside to the terrain.
- **Rocks.** The 11 CliffRock5 sit on the terrain 7 m behind the wall, at most roof − 4 m. In x1 they are 7–9 m below the roof.

**Counts** (x1): 110 BasePlateConcrete16, 28 Wall32x16, 55 Pillar2x8, 1 Pillar2x16, 11 CliffRock5. The extent is x −6467 … −6233, z 668 … 1075, y 252 … 279.

**Bounds check** (written into stats.md):
- The roof covers 99.9 % of the road strip from the column line to the wall, sampled at 1 m. The 13 open samples are at the two ends.
- The wall covers 100 % of the hill edge.
- Not checked: the CliffRock5 height. If a rock still shows above the wall, lower its y or move it further back.

## Round 38: the avalanche gallery against the reference photos (owner's A-v12 review of 24)

Owner: "24 doesnt look like the example inspo images". Interchange 8 / BR001 are accepted. The reference photos are next-run/ref-gallery-1.webp (inside) and ref-gallery-2.webp (outside). Kept from round 37: roof at the 48 m smoothed profile + 6.5 m, placement by arc length, hill wall flush with the roof.

1. **Colonnade:** a Pillar1x8 (1 × 8 × 1 m, ConcreteSet01) stack every 4 m along the road under the valley slab edge, from the fascia underside down to the terrain. 111 stacks, 111 pieces in x1 (114 at 2×). Was a Pillar2x8 every 8 m.
2. **Roof cover:**
   - The 11 CS1 rock_c pieces are back ON the roof as CliffRock5, base (bounds min y −2.57) at the roof top. CliffRock5 is 36 × 17 × 36 m, wider than the 26.5 m roof, so each one is turned to the road and set toward the hill (centre 20.6 m in from the column line): its valley side ends 1.5 m inside the roof edge, and the rest reaches over the wall into the hill.
   - Over the whole roof top: a jittered 2.5 m scatter, inset 1.5 m from the valley edge.
     - 1180 GreenBushWild01/02 (kind tree, adult);
     - 778 stones from the rock-scatter layer's Rock01…Rock30.
   - Every base sits at the roof top: the highest slab row covering the point, + 0.2 m.
   - Check: 0 cover pieces reach past the valley edge.
3. **Fascia:** 4 more BasePlateConcrete16 stacked under each valley-side slab at 0.2 m steps, a 1 m concrete edge (220 objects).
4. **Foreground:** a new tree-mask rule, the rock shed's open valley side. The ring runs from just under the slab edge out 20 m from the column line, 10 m past each end, and masks 93 trees. The same ring is in out/clear-rock-shed.json and out/x1/clear-rock-shed.json (surfaces format, one polygon, prefab "none (clear area)") for demolishing the trees already placed. It is not a surface import.
5. **Portals:** left as they are. TunnelEntrance18 (26 × 14 × 16 m, 10 m opening) would have to sink 3.5 m below the road for its opening to meet the roof at + 6.5 m. It would still stand 4 m above the hill wall, which is flush with the roof.

**Counts** (x1): 2438 objects: 110 roof slabs + 220 fascia plates, 28 Wall32x16, 111 Pillar1x8, 11 CliffRock5, 1180 bushes, 778 stones.

**Bounds check** (stats.md):
- The roof covers 99.9 % of the road strip; the 13 open 1 m samples are at the ends.
- The largest step between roof rows is 1.49 m in x1 (2.98 m at 2×). The round-37 figure of 2.98 m for x1 was a bug in the check: deduplicating heights skipped a row. It now compares row to row.
- The wall covers 100 % of the hill edge.
