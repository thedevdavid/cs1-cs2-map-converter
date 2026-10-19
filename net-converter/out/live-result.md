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
