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
