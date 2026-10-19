# Conversion outputs (2026-10-19)

Dump: `C:\Users\dleva\AppData\Local/DakotaExport/2972879554_Dakota_Falls-20261019-063123.json`. Import order: network → power, walls, ship paths → water (import-water.json at sea level 97.5 / 195, or the variant import-water-sea93.json / -sea186.json at the measured 93 / 186; try the variant first, fall back to 97.5) → connections → objects (incl. barrier props) → trees (shared files + the per-scale fixed-height file) → resources → surfaces (fields, rock, decals, sand; grass optional) → experiments (order 9: net-corridor trees, unmerged decals and grass, instead of or on top of the default files); each file is its own import.
out/ = 2× heights (Height Scale 4096), out/x1/ = true heights (Height Scale 2048); same nodes and courses in both.

| order | file | content | count | size |
|---:|---|---|---|---:|
| 1 | `out\import-all.json` | network (2×) | 4072 edges, 3915 nodes | 2.1 MB |
| 1 | `out\x1\import-all.json` | network (1×) | 4072 edges, 3915 nodes | 2.1 MB |
| 2 | `out\import-power.json` | power (2×) | 60 edges | 0.0 MB |
| 2 | `out\import-walls.json` | walls: quays, retaining walls, canals, flood walls (2×) | 113 edges: QuaySmall01 97, Narrow Boatway 14, Sound Barrier 2 | 0.1 MB |
| 2 | `out\import-shippaths.json` | ship paths (2×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam (undo the replaced edges first) (2×) | 2 edges, replaces e5094_0, e32855_0 | 0.0 MB |
| 2 | `out\x1\import-power.json` | power (1×) | 60 edges | 0.0 MB |
| 2 | `out\x1\import-walls.json` | walls: quays, retaining walls, canals, flood walls (1×) | 113 edges: QuaySmall01 97, Narrow Boatway 14, Sound Barrier 2 | 0.1 MB |
| 2 | `out\x1\import-shippaths.json` | ship paths (1×) | 100 edges: Medium Seaway 100 | 0.1 MB |
| 2 | `out\x1\import-power-ends.json` | ROUND 8 power-line ends at the substation and the dam (undo the replaced edges first) (1×) | 2 edges, replaces e5094_0, e32855_0 | 0.0 MB |
| 3 | `out\import-water.json` | water (2×), seaLevel 195 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\import-water-sea186.json` | water VARIANT (2×), seaLevel 186: import instead of import-water.json to try the measured level | 45 sources | 0.0 MB |
| 3 | `out\x1\import-water.json` | water (1×), seaLevel 97.5 | 41 sources, 0 refined | 0.0 MB |
| 3 | `out\x1\import-water-sea93.json` | water VARIANT (1×), seaLevel 93: import instead of import-water.json to try the measured level | 44 sources | 0.0 MB |
| 4 | `out\import-connections.json` | outside connections | 25 | 0.0 MB |
| 4 | `out\x1\import-connections.json` | outside connections | 25 | 0.0 MB |
| 5 | `out\import-walls-props.json` | barrier props (2×) | 621 | 0.1 MB |
| 5 | `out\import-objects.json` | objects (2×) | 6130 (prop 3038, building 5, object 2, plant 3076, tree 9; 2301 with y) | 1.3 MB |
| 5 | `out\import-substation.json` | ROUND 8 substation building (placeholder prefab until the bridge reports it) (2×) | 1 | 0.0 MB |
| 5 | `out\import-dam.json` | ROUND 8 dam, variant A (yaw −CS1 angle) (2×) | 1 | 0.0 MB |
| 5 | `out\import-dam-B.json` | ROUND 8 dam, variant B (mirrored yaw); import A or B (2×) | 1 | 0.0 MB |
| 5 | `out\import-po-rocks.json` | ROUND 8 pack cliff objects at the PO mountains / rock work / falls steps (2×) | 594 | 0.1 MB |
| 5 | `out\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (2×) | 19020 | 3.5 MB |
| 5 | `out\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (2×) | 2 | 0.0 MB |
| 5 | `out\x1\import-walls-props.json` | barrier props (1×) | 621 | 0.1 MB |
| 5 | `out\x1\import-objects.json` | objects (1×) | 6130 (prop 3038, building 5, object 2, plant 3076, tree 9; 2301 with y) | 1.3 MB |
| 5 | `out\x1\import-substation.json` | ROUND 8 substation building (placeholder prefab until the bridge reports it) (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-dam.json` | ROUND 8 dam, variant A (yaw −CS1 angle) (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-dam-B.json` | ROUND 8 dam, variant B (mirrored yaw); import A or B (1×) | 1 | 0.0 MB |
| 5 | `out\x1\import-po-rocks.json` | ROUND 8 pack cliff objects at the PO mountains / rock work / falls steps (1×) | 594 | 0.1 MB |
| 5 | `out\x1\import-rock-scatter.json` | ROUND 8 vanilla rock scatter (1×) | 19020 | 3.5 MB |
| 5 | `out\x1\import-po-candidates.json` | Procedural Object candidates, UNVERIFIED (1×) | 2 | 0.0 MB |
| 6 | `out\import-trees-fixed-01.json` | trees at fixed height, with y (2×) | 50 | 0.0 MB |
| 6 | `out\x1\import-trees-fixed-01.json` | trees at fixed height, with y (1×) | 50 | 0.0 MB |
| 6 | `out\trees/import-trees-01.json` … `import-trees-10.json` | trees (both scales) | 496,318 in 10 files | |
| 7 | `out\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 7 | `out\x1\import-resources.json` | natural resources | fertility, ore, oil, groundwater | 2.4 MB |
| 8 | `out\import-surfaces-fields.json` | field surfaces | 56 polygons | 0.1 MB |
| 8 | `out\import-surfaces-rock.json` | rock surfaces (slope >= 40°) | 151 polygons | 0.0 MB |
| 8 | `out\import-surfaces-decals.json` | decal surfaces (Dirty Surfaces Pack) | 41 polygons | 0.0 MB |
| 8 | `out\import-surfaces-sand.json` | sand surfaces (CS1 terrainSurfaces) | 479 polygons | 0.2 MB |
| 8 | `out\import-surfaces-grass.json` | grass surfaces (optional alternative to the grass plants) | 51 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-fields.json` | field surfaces | 56 polygons | 0.1 MB |
| 8 | `out\x1\import-surfaces-rock.json` | rock surfaces (slope >= 40°) | 151 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-decals.json` | decal surfaces (Dirty Surfaces Pack) | 41 polygons | 0.0 MB |
| 8 | `out\x1\import-surfaces-sand.json` | sand surfaces (CS1 terrainSurfaces) | 479 polygons | 0.2 MB |
| 8 | `out\x1\import-surfaces-grass.json` | grass surfaces (optional alternative to the grass plants) | 51 polygons | 0.0 MB |
| 9 | `out\import-surfaces-decals-unmerged.json` | EXPERIMENT: decal squares, not merged | 128 polygons | 0.0 MB |
| 9 | `out\import-surfaces-grass-unmerged.json` | EXPERIMENT: grass clusters, not merged | 53 polygons | 0.0 MB |
| 9 | `out\x1\import-surfaces-decals-unmerged.json` | EXPERIMENT: decal squares, not merged | 128 polygons | 0.0 MB |
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
