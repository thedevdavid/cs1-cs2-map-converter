# Dead ends

Network ends more than 60 m inside the ±7168 m border, explained from the CS1 dump: the CS1 segments meeting within 2 m of the end and what the converter did with each.

Live graph `out\x1\live-graph.json` (3322 nodes).

## Live game (round-1 import, before round-3 fixes): 264

| reason | ends | stub prefabs |
|---|---:|---|
| no CS1 node here (game split, or merged/moved by the converter) | 115 | QuaySmall01 78, Highway Oneway - 2 lanes 17, Hydroelectric_Power_Plant_01 Dam 8, Gravel Road 4, High-voltage Line 2, Arc Pedestrian Bridge 2, Invisible Road Path - 2xTwoway 2, Highway Oneway - 3 lanes 1, Small Road 1 |
| connected in the data, not joined in the game | 110 | Gravel Road 58, Highway Oneway - 2 lanes 22, QuaySmall01 11, Small Road 8, Pavement Path 4, Highway Oneway - 1 lane 3, Medium Road 2, Medium Seaway 1, Highway Oneway - 3 lanes 1 |
| CS1 dead end (genuine) | 33 | QuaySmall01 17, Arc Pedestrian Bridge 6, Gravel Road 2, Medium Road 2, Highway Oneway - 3 lanes 2, High-voltage Line 1, Medium Seaway 1, Pavement Path 1, Hydroelectric_Power_Plant_01 Dam 1 |
| neighbour collapsed (< 4 m) | 6 | Arc Pedestrian Bridge 6 |

### 20 worst live dead ends

| stub prefab | x | z | reason | CS1 segments here [layer, converter decision] |
|---|---:|---:|---|---|
| Highway Oneway - 1 lane | 213 | 143 | connected in the data, not joined in the game | USRP 1L Interstate Ramp_Data [highways, imported]; USRP 1L Interstate Ramp_Data [highways, imported] |
| Highway Oneway - 1 lane | 203 | 160 | connected in the data, not joined in the game | USRP 1L Interstate Ramp_Data [highways, imported]; USRP 1L Interstate Ramp_Data [highways, imported] |
| Highway Oneway - 2 lanes | 279 | 146 | connected in the data, not joined in the game | USRP 1L Interstate Ramp_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)]; USRP 1L Interstate Ramp_Data [highways, imported] |
| Highway Oneway - 2 lanes | 287 | 164 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | 595 | 234 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | 591 | 249 | connected in the data, not joined in the game | USRP 2L Interstate B [highways, bridge override (import-bridges*.json)]; USRP 2L Interstate_Data [highways, imported] |
| Highway Oneway - 2 lanes | 614 | 239 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | 632 | 260 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 3 lanes | 2302 | 255 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, left out: one-way highway stub to a CS1 dead end] |
| Highway Oneway - 3 lanes | 2293 | 277 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, left out: one-way highway stub to a CS1 dead end] |
| Highway Oneway - 2 lanes | -4299 | -31 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate_Data [highways, imported] |
| Highway Oneway - 2 lanes | -4307 | -45 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate_Data [highways, imported] |
| Highway Oneway - 1 lane | -4347 | 20 | connected in the data, not joined in the game | USRP 1L Interstate Ramp_Data [highways, imported]; USRP 1L Interstate Ramp_Data [highways, imported] |
| Highway Oneway - 2 lanes | -4455 | 73 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | -4488 | 76 | connected in the data, not joined in the game | USRP 2L Interstate B [highways, bridge override (import-bridges*.json)]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | -4536 | 108 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | -4527 | 121 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | -4571 | 132 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | -4563 | 145 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 3 lanes | 799 | 4000 | connected in the data, not joined in the game | USRP 3L Interstate E [highways, imported]; USRP 3L Interstate E [highways, imported] |

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

