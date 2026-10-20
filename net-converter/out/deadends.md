# Dead ends

Network ends more than 60 m inside the ±7168 m border, explained from the CS1 dump: the CS1 segments meeting within 2 m of the end and what the converter did with each.

Live graph `out\live-graph.json` (2973 nodes).

## Live game (round-1 import, before round-3 fixes): 14

| reason | ends | stub prefabs |
|---|---:|---|
| CS1 dead end (genuine) | 9 | Gravel Road 5, Medium Road 2, Highway Oneway - 3 lanes 2 |
| connected in the data, not joined in the game | 3 | Gravel Road 1, Highway Oneway - 3 lanes 1, Highway Oneway - 4 lanes 1 |
| neighbour collapsed (< 4 m) | 1 | Gravel Road 1 |
| no CS1 node here (game split, or merged/moved by the converter) | 1 | Highway Oneway - 1 lane 1 |

### 20 worst live dead ends

| stub prefab | x | z | reason | CS1 segments here [layer, converter decision] |
|---|---:|---:|---|---|
| Highway Oneway - 3 lanes | 2302 | 255 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, left out: one-way highway stub to a CS1 dead end] |
| Highway Oneway - 3 lanes | 2293 | 277 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, left out: one-way highway stub to a CS1 dead end] |
| Highway Oneway - 3 lanes | 412 | -4474 | connected in the data, not joined in the game | USRP 3L Interstate_Data [highways, imported]; USRP 4L Interstate_Data [highways, imported] |
| Highway Oneway - 4 lanes | 463 | -4520 | connected in the data, not joined in the game | USRP 4L Interstate_Data [highways, imported]; USRP 4L Interstate_Data [highways, imported] |
| Highway Oneway - 1 lane | 3671 | 2153 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Medium Road | -643 | -1188 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Medium Road | -480 | -1481 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Gravel Road | 26 | -2398 | connected in the data, not joined in the game | Pedestrian Gravel [roads, imported]; Gravel Road [roads, imported] |
| Gravel Road | -229 | -2362 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Gravel Road | -3987 | 334 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Gravel Road | -768 | 4363 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Gravel Road | 1104 | 4897 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Gravel Road | 1383 | 6161 | CS1 dead end (genuine) | Gravel Road Bridge [roads, bridge override (import-bridges*.json)] |
| Gravel Road | 1462 | 6173 | neighbour collapsed (< 4 m) | Gravel Road [roads, collapsed (< 4 m)]; Gravel Road [roads, imported] |

## Predicted for the regenerated import-all.json (fresh map): 51

| reason | ends | stub prefabs |
|---|---:|---|
| connected in the data, not joined in the game | 38 | Double Train Track 18, Gravel Road 13, Highway Oneway - 2 lanes 5, Small Road 2 |
| CS1 dead end (genuine) | 12 | Pavement Path 6, Gravel Road 4, Medium Road 2 |
| neighbour collapsed (< 4 m) | 1 | Gravel Road 1 |

| stub prefab | x | z | reason | CS1 segments here |
|---|---:|---:|---|---|
| Highway Oneway - 2 lanes | 595 | 234 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | -4397 | 15 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | -4563 | 145 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | 591 | 249 | connected in the data, not joined in the game | USRP 2L Interstate B [highways, bridge override (import-bridges*.json)]; USRP 2L Interstate_Data [highways, imported] |
| Highway Oneway - 2 lanes | -4571 | 132 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Double Train Track | -250 | -6805 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -398 | -5800 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -423 | -3356 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -1470 | 4404 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 685 | 2683 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Ourense SA E [rail, imported]; R2EUR W GrCo 2x2 Ourense SA B [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 166 | 2777 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 3679 | 2853 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 6492 | 2407 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -345 | -2864 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Pedrido B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Pedrido E [rail, imported] |
| Double Train Track | -1399 | 4230 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 3806 | 2833 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -305 | -6433 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 283 | 2756 | connected in the data, not joined in the game | R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 6872 | 2347 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -304 | -2602 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Pedrido B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Pedrido E [rail, imported] |
| Double Train Track | -465 | -3623 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | -452 | -5436 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 871 | 2649 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Ourense SA B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Ourense SA E [rail, imported] |
| Small Road | -3696 | -2945 | connected in the data, not joined in the game | USRP 2-Lane National Road_Data [roads, imported]; USRP 2-Lane National Road B [roads, bridge override (import-bridges*.json)] |
| Small Road | -3353 | -3417 | connected in the data, not joined in the game | USRP 2-Lane National Road B [roads, bridge override (import-bridges*.json)]; USRP 2-Lane National Road_Data [roads, imported] |
| Gravel Road | -1536 | 4298 | connected in the data, not joined in the game | Gravel Road [roads, imported]; Gravel Road Elevated [roads, bridge override (import-bridges*.json)] |
| Gravel Road | 6847 | 3346 | connected in the data, not joined in the game | Gravel Road [roads, imported]; Gravel Road Elevated [roads, bridge override (import-bridges*.json)] |
| Gravel Road | 6846 | 2619 | connected in the data, not joined in the game | Gravel Road [roads, imported]; Gravel Road Elevated [roads, bridge override (import-bridges*.json)] |
| Gravel Road | 6846 | 2553 | connected in the data, not joined in the game | Gravel Road Elevated [roads, bridge override (import-bridges*.json)]; Gravel Road [roads, imported] |
| Gravel Road | 3737 | 2925 | connected in the data, not joined in the game | Gravel Road [roads, imported]; Gravel Road Bridge [roads, bridge override (import-bridges*.json)] |

