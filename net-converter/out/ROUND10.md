# Conversion outputs (2026-10-20)

Dump: `C:\Users\dleva\AppData\Local\DakotaExport\2972879554_Dakota_Falls-20261019-063123.json`. Run it all with `node tools/import-run.mjs --scale x1|x2 [--sea 93|97.5]`. Import order (bridges: import-bridges.json right after the network; round 10, owner review of A-v4: no quays, retaining walls, PO walls, monorail decor or dam files; dam by hand, see manual-objects.md): network → gaps → power, canals, ship paths → water (import-water.json at sea level 97.5 / 195, or the variant import-water-sea93.json / -sea186.json at the measured 93 / 186; try the variant first, fall back to 97.5) → connections → objects (incl. barrier props) → trees (shared files + the per-scale fixed-height file) → resources → dam (one network edge) after the bridges → … → surfaces (vanilla paint plan: fields, PO pavement, sand, dry grass; grass optional; the pack surfaces rock, PO rock, decals and pack sand are in optional/, runner --with) → experiments (order 9: net-corridor trees, unmerged decals and grass, instead of or on top of the default files); each file is its own import.
out/ = 2× heights (Height Scale 4096), out/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
| 0 | `out\import-terrain-paint-sand.json` | ROUND 24 terrain paint Sand (CS1 sand), softEdge 6 m (2×) | 743 polygons, 0 strokes | 7.1 MB |
| 0 | `out\import-terrain-paint-dry-grass.json` | ROUND 24 terrain paint Dry (plains + south-facing slopes), softEdge 6 m (2×) | 956 polygons, 0 strokes | 5.0 MB |
| 0 | `out\import-terrain-paint-bare-earth.json` | ROUND 24 terrain paint Dirt (11 m strokes at the CS1 dirt decals) (2×) | 0 polygons, 256 strokes | 0.0 MB |
| 0 | `out\optional\import-terrain-paint-rock.json` | rock mask, NOT imported (no rock material) (2×) | 214 polygons, 0 strokes | 0.2 MB |
| 0 | `out\import-terrain-paint-n1-riparian-lush.json` | ROUND 27 terrain paint N1 Lush (2×) | 153 polygons, 0 strokes | 0.7 MB |
| 0 | `out\import-terrain-paint-n2-pit-banks-dirt.json` | ROUND 27 terrain paint N2 Dirt (2×) | 0 polygons, 13 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n2-pit-banks-sand.json` | ROUND 27 terrain paint N2 Sand (2×) | 0 polygons, 8 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n3-drawdown-dirt.json` | ROUND 27 terrain paint N3 Dirt (2×) | 0 polygons, 134 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n4-hogback-sand.json` | ROUND 27 terrain paint N4 Sand (2×) | 0 polygons, 31644 strokes | 5.1 MB |
| 0 | `out\import-terrain-paint-n5-scree-dirt.json` | ROUND 27 terrain paint N5 Dirt (2×) | 200 polygons, 0 strokes | 0.9 MB |
| 0 | `out\import-terrain-paint-n6-savanna-dirt.json` | ROUND 27 terrain paint N6 Dirt (2×) | 0 polygons, 40029 strokes | 6.4 MB |
| 0 | `out\import-terrain-paint-n6-savanna-dry.json` | ROUND 27 terrain paint N6 Dry (2×) | 1903 polygons, 0 strokes | 20.6 MB |
| 0 | `out\import-terrain-paint-n7-prairie-dog-dirt.json` | ROUND 27 terrain paint N7 Dirt (2×) | 0 polygons, 162 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n7-prairie-dog-dry.json` | ROUND 27 terrain paint N7 Dry (2×) | 2 polygons, 0 strokes | 0.0 MB |
| 0 | `out\import-terrain-paint-n8-playa-centre-dirt.json` | ROUND 27 terrain paint N8 Dirt (2×) | 162 polygons, 0 strokes | 0.6 MB |
| 0 | `out\import-terrain-paint-n8-playa-rim-lush.json` | ROUND 27 terrain paint N8 Lush (2×) | 191 polygons, 0 strokes | 0.8 MB |
| 0 | `out\import-terrain-paint-n9-dirt-roads-dirt.json` | ROUND 27 terrain paint N9 Dirt (2×) | 0 polygons, 12253 strokes | 2.0 MB |
| 0 | `out\import-terrain-paint-n10-rail-ballast-dirt.json` | ROUND 27 terrain paint N10 Dirt (2×) | 0 polygons, 4357 strokes | 0.7 MB |
| 0 | `out\import-terrain-paint-n11-oil-dirt.json` | ROUND 27 terrain paint N11 Dirt (2×) | 425 polygons, 0 strokes | 8.5 MB |
| 0 | `out\import-terrain-paint-n11-ore-dirt.json` | ROUND 27 terrain paint N11 Dirt (2×) | 196 polygons, 0 strokes | 1.8 MB |
| 0 | `out\import-terrain-paint-n12-fertile-dry.json` | ROUND 27 terrain paint N12 Dry (2×) | 475 polygons, 0 strokes | 4.3 MB |
| 0 | `out\import-terrain-paint-n13-high-ground-dry.json` | ROUND 27 terrain paint N13 Dry (2×) | 175 polygons, 0 strokes | 2.7 MB |
| 0 | `out\x1\import-terrain-paint-sand.json` | ROUND 24 terrain paint Sand (CS1 sand), softEdge 6 m (1×) | 743 polygons, 0 strokes | 7.1 MB |
| 0 | `out\x1\import-terrain-paint-dry-grass.json` | ROUND 24 terrain paint Dry (plains + south-facing slopes), softEdge 6 m (1×) | 956 polygons, 0 strokes | 5.0 MB |
| 0 | `out\x1\import-terrain-paint-bare-earth.json` | ROUND 24 terrain paint Dirt (11 m strokes at the CS1 dirt decals) (1×) | 0 polygons, 256 strokes | 0.0 MB |
| 0 | `out\x1\optional\import-terrain-paint-rock.json` | rock mask, NOT imported (no rock material) (1×) | 214 polygons, 0 strokes | 0.2 MB |
| 0 | `out\x1\import-terrain-paint-n1-riparian-lush.json` | ROUND 27 terrain paint N1 Lush (1×) | 153 polygons, 0 strokes | 0.7 MB |
| 0 | `out\x1\import-terrain-paint-n2-pit-banks-dirt.json` | ROUND 27 terrain paint N2 Dirt (1×) | 0 polygons, 13 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n2-pit-banks-sand.json` | ROUND 27 terrain paint N2 Sand (1×) | 0 polygons, 8 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n3-drawdown-dirt.json` | ROUND 27 terrain paint N3 Dirt (1×) | 0 polygons, 134 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n4-hogback-sand.json` | ROUND 27 terrain paint N4 Sand (1×) | 0 polygons, 31644 strokes | 5.1 MB |
| 0 | `out\x1\import-terrain-paint-n5-scree-dirt.json` | ROUND 27 terrain paint N5 Dirt (1×) | 200 polygons, 0 strokes | 0.9 MB |
| 0 | `out\x1\import-terrain-paint-n6-savanna-dirt.json` | ROUND 27 terrain paint N6 Dirt (1×) | 0 polygons, 40029 strokes | 6.4 MB |
| 0 | `out\x1\import-terrain-paint-n6-savanna-dry.json` | ROUND 27 terrain paint N6 Dry (1×) | 1903 polygons, 0 strokes | 20.6 MB |
| 0 | `out\x1\import-terrain-paint-n7-prairie-dog-dirt.json` | ROUND 27 terrain paint N7 Dirt (1×) | 0 polygons, 162 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n7-prairie-dog-dry.json` | ROUND 27 terrain paint N7 Dry (1×) | 2 polygons, 0 strokes | 0.0 MB |
| 0 | `out\x1\import-terrain-paint-n8-playa-centre-dirt.json` | ROUND 27 terrain paint N8 Dirt (1×) | 162 polygons, 0 strokes | 0.6 MB |
| 0 | `out\x1\import-terrain-paint-n8-playa-rim-lush.json` | ROUND 27 terrain paint N8 Lush (1×) | 191 polygons, 0 strokes | 0.8 MB |
| 0 | `out\x1\import-terrain-paint-n9-dirt-roads-dirt.json` | ROUND 27 terrain paint N9 Dirt (1×) | 0 polygons, 12274 strokes | 2.0 MB |
| 0 | `out\x1\import-terrain-paint-n10-rail-ballast-dirt.json` | ROUND 27 terrain paint N10 Dirt (1×) | 0 polygons, 4418 strokes | 0.7 MB |
| 0 | `out\x1\import-terrain-paint-n11-oil-dirt.json` | ROUND 27 terrain paint N11 Dirt (1×) | 425 polygons, 0 strokes | 8.5 MB |
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
| 1 | `out\x1\import-bridges.json` | bridge overrides (right after import-all.json, which leaves these decks out) (1×) | 44 edges, replaces 139 | 0.0 MB |
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
| 5 | `out\import-objects.json` | objects (2×) | 6130 (prop 3040, building 5, plant 3076, tree 9; 1189 with y) | 1.3 MB |
| 5 | `out\import-substation.json` | ROUND 8 substation building TransformerStation01 (2×) | 1 | 0.0 MB |
| 5 | `out\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (2×) | 248 | 0.1 MB |
| 5 | `out\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (2×) | 17557 | 3.2 MB |
| 5 | `out\import-po-structure-objects.json` | ROUND 22 PO structures: BR022 pads (BR006 variant a: no kit objects); runner --po-structures (mod 154132) (2×) | 3 | 0.0 MB |
| 5 | `out\import-po-structure-piers.json` | ROUND 22 BR001 / BR003 piers: ITBP caps (140978) on Pillar4x16 stacks (154132), after pillars-off; on by default (2×) | 201 | 0.0 MB |
| 5 | `out\optional\import-po-structure-br006-arches.json` | ROUND 22 BR006 variant b: 7 StoneArchRailBridge (135944) under the deck, runner --with po-br006-arches (2×) | 7 | 0.0 MB |
| 5 | `out\optional\import-po-structure-gantry.json` | ROUND 20 optional gantry HWY Truss Gantry 2L  A (132188), runner --with po-gantry (2×) | 1 | 0.0 MB |
| 5 | `out\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (2×) | 112 | 0.0 MB |
| 5 | `out\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (2×) | 2 | 0.0 MB |
| 5 | `out\x1\import-walls-props.json` | barrier props (1×) | 621 | 0.1 MB |
| 5 | `out\x1\import-objects.json` | objects (1×) | 6130 (prop 3040, building 5, plant 3076, tree 9; 1189 with y) | 1.3 MB |
| 5 | `out\x1\import-substation.json` | ROUND 8 substation building TransformerStation01 (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-po-rocks.json` | po-rocks-v3: pack rocks and cliffs at the PO mountains / rock work / falls steps, sunk by their live bounds (1×) | 248 | 0.1 MB |
| 5 | `out\x1\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (1×) | 17553 | 3.2 MB |
| 5 | `out\x1\import-po-structure-objects.json` | ROUND 22 PO structures: BR022 pads (BR006 variant a: no kit objects); runner --po-structures (mod 154132) (1×) | 3 | 0.0 MB |
| 5 | `out\x1\import-po-structure-piers.json` | ROUND 22 BR001 / BR003 piers: ITBP caps (140978) on Pillar4x16 stacks (154132), after pillars-off; on by default (1×) | 131 | 0.0 MB |
| 5 | `out\x1\optional\import-po-structure-br006-arches.json` | ROUND 22 BR006 variant b: 7 StoneArchRailBridge (135944) under the deck, runner --with po-br006-arches (1×) | 7 | 0.0 MB |
| 5 | `out\x1\optional\import-po-structure-gantry.json` | ROUND 20 optional gantry HWY Truss Gantry 2L  A (132188), runner --with po-gantry (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-fort.json` | ROUND 11 fort ruins along the CS1 Castle Wall (OldCastle01 keep UNVERIFIED) (1×) | 112 | 0.0 MB |
| 5 | `out\x1\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (1×) | 2 | 0.0 MB |
| 6 | `out\import-trees-fixed-01.json` | trees at fixed height, with y (2×) | 50 | 0.0 MB |
| 6 | `out\x1\import-trees-fixed-01.json` | trees at fixed height, with y (1×) | 50 | 0.0 MB |
| 6 | `out\trees/import-trees-01.json` … `import-trees-10.json` | trees (both scales) | 496,303 in 10 files | |
| 7 | `out\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 7 | `out\x1\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 8 | `out\import-surfaces-fields.json` | field surfaces | 55 polygons | 0.1 MB |
| 8 | `out\import-surfaces-sand.json` | paint plan sand: Sand Surface 01 banks, Sand Surface 02 bare earth + decal spots | 112 polygons | 0.3 MB |
| 8 | `out\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (2×) | 11 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-fields.json` | field surfaces | 55 polygons | 0.1 MB |
| 8 | `out\x1\import-surfaces-sand.json` | paint plan sand: Sand Surface 01 banks, Sand Surface 02 bare earth + decal spots | 112 polygons | 0.3 MB |
| 8 | `out\x1\import-po-structures.json` | ROUND 9 pavement tops of the PO paved blocks (1×) | 11 polygons | 0.0 MB |
| 9 | `out\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\x1\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\trees/experiment-trees-net-corridor-01.json` | EXPERIMENT: trees on ground-net corridors (both scales) | 656 | |

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
