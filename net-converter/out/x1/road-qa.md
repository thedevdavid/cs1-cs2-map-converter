# Road QA (tools/road-qa.mjs, 2026-10-20)

Checks on the import set (import-all + import-bridges + import-bridges-fixed + import-gaps), x1; the x2 set has the same nodes and courses.
Columns: the set A-v4 was built from (import-all at 3430d7e + its gaps file), the committed set before this round (e9a3035), and after this round. All three measured with the same rules.

| check | A-v4 set | before | after |
|---|---:|---:|---:|
| dead ends (not border, not junction) | 11 | 11 | 9 |
| … genuine (CS1 dead end too) | 11 | 10 | 9 |
| … pipeline | 0 | 1 | 0 |
| mid-air stub pairs (elevated, same way, ≤ 60 m) | 1 | 0 | 0 |
| torn splits (< 5°, overlap > 10 m) | 0 | 0 | 0 |
| duplicate courses (a rebuild over its original: torn deck) | 4 | 4 | 0 |
| isolated pieces < 100 m | 1 | 1 | 1 |
| ramp dead ends ≤ 25 m from a mainline | 2 | 0 | 0 |

Live A-v4 map (out/x1/live-graph.json, 2026-10-20T12:08:26.849Z): 288 dead-end nodes of all prefabs. Most of what the owner saw there is the 309 segments the map lost after the import (297 of them were live in Run A; out/x1/live-defects.md, "A-v4 against the import set"), not the import set itself.

## What the owner saw, and where it went

- (a) elevated one-way stubs ending in mid-air: the pair at (2293, 277) / (2302, 255) is a CS1 dead end (unfinished 3-lane interchange); left out since round 12 (`--keep-highway-stubs` keeps it). The other mid-air ends on A-v4 are lost continuations (live-defects.md table), not in the import set.
- (b) torn 4-lane / 3-lane split: at (463, −4520) the original 4-lane e15883_0 stayed in import-all.json while import-gaps.json rebuilt it as gap_e15883_3lane between the same nodes; on a fresh map both were built (the duplicate-courses row: 4 such pairs). Fixed: the gap edges now replace their originals (e15883_0, e20430_0b, e25579_0, e36563_0 leave import-all.json).
- (c) ramp ends as square cul-de-sacs: none in the import set (0 ramp dead ends within 25 m of a mainline after the round-12 stub removal); on A-v4 they are the ends of lost segments.
- (d) a short road standing alone ("Foxhollow Street"): no isolated road piece in the import set; the only isolated piece is a CS1 pedestrian path (below). The live one is a remnant of the lost segments; the graph has no road names, a position from the screenshot would pin it.
- The CS1 gravel gap at (1383, 6161)–(1462, 6173) had silently lost its closing edge when BR020 became a bridge (its end node moved to import-bridges.json); the gaps pass now finds bridge nodes, so it is closed again.

## Residual (check in game)

| at | what | reason |
|---|---|---|
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

Near misses below the torn-split threshold (5–12°, overlap > 10 m, normal ramp diverges): (4050, 4550) 2-lane / 1-lane 11°, (2827, 984) 1-lane / 2-lane 11.7°. Re-run: `node tools/road-qa.mjs out/x1 --json out/x1/road-qa-after.json` (QA_ANGLE=12 for the near misses).
