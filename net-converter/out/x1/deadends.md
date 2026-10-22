# Dead ends

Network ends more than 60 m inside the ±7168 m border, explained from the CS1 dump: the CS1 segments meeting within 2 m of the end and what the converter did with each.

Live graph `out\x1\live-graph.json` (3106 nodes).

## Live game (round-1 import, before round-3 fixes): 44

| reason | ends | stub prefabs |
|---|---:|---|
| CS1 dead end (genuine) | 29 | Narrow Boatway 14, Gravel Road 4, Pavement Path 3, Sound Barrier 3, Medium Road 2, High-voltage Line 1, Medium Seaway 1, BXP Arc Pedestrian Bridge Revised 1 |
| no CS1 node here (game split, or merged/moved by the converter) | 9 | Invisible Road Path - 1xOneway 2, Invisible Road Path - 2xTwoway 2, High-voltage Line 2, Hydroelectric_Power_Plant_01 Dam 2, Highway Oneway - 3 lanes 1 |
| neighbour collapsed (< 4 m) | 3 | BXP Arc Pedestrian Bridge Revised 2, Sound Barrier 1 |
| connected in the data, not joined in the game | 3 | Double Train Track 2, Highway Oneway - 3 lanes 1 |

### 20 worst live dead ends

| stub prefab | x | z | reason | CS1 segments here [layer, converter decision] |
|---|---:|---:|---|---|
| Highway Oneway - 3 lanes | 799 | 4000 | connected in the data, not joined in the game | USRP 3L Interstate E [highways, imported]; USRP 3L Interstate E [highways, imported] |
| Highway Oneway - 3 lanes | 837 | 4022 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Double Train Track | 5726 | 2528 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 5923 | 2497 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2_Data [rail, imported] |
| Medium Seaway | 405 | 3 | CS1 dead end (genuine) | Ship Path [shippaths, shippaths] |
| Medium Road | -643 | -1188 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Medium Road | -480 | -1481 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Sound Barrier | 436 | 182 | CS1 dead end (genuine) | Flood Wall [walls, walls] |
| Sound Barrier | 428 | 212 | CS1 dead end (genuine) | Flood Wall [walls, walls] |
| High-voltage Line | -315 | -1340 | CS1 dead end (genuine) | 345 kV H Frame Ceramic Insulator_Data [power, power] |
| Invisible Road Path - 1xOneway | -38 | -2525 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Gravel Road | -229 | -2362 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Invisible Road Path - 1xOneway | -81 | -2525 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Pavement Path | 295 | -3026 | CS1 dead end (genuine) | Pedestrian Gravel [roads, imported] |
| Pavement Path | 373 | -3031 | CS1 dead end (genuine) | Pedestrian Gravel [roads, imported] |
| Gravel Road | -3987 | 334 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Gravel Road | -768 | 4363 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Sound Barrier | 1211 | 4198 | neighbour collapsed (< 4 m) | Monorail Oneway Track [decor, decor]; Monorail Track - No Pylons [decor, collapsed (< 4 m)]; Monorail Oneway Track [decor, decor]; Monorail Track - No Pylons [decor, degenerate after merge]; Flood Wall [walls, walls] |
| Sound Barrier | 1212 | 4222 | CS1 dead end (genuine) | Flood Wall [walls, walls] |
| Pavement Path | 4787 | -709 | CS1 dead end (genuine) | Pedestrian Gravel [roads, imported] |

## Predicted for the regenerated import-all.json (fresh map): 55

| reason | ends | stub prefabs |
|---|---:|---|
| connected in the data, not joined in the game | 36 | Double Train Track 16, Gravel Road 13, Highway Oneway - 2 lanes 5, Highway Twoway - 2 lanes 2 |
| CS1 dead end (genuine) | 12 | Gravel Road 4, Pavement Path 4, Highway Oneway - 3 lanes 2, Highway Twoway - 4 lanes 2 |
| no CS1 node here (game split, or merged/moved by the converter) | 6 | Highway Oneway - 3 lanes 5, Highway Oneway - 4 lanes 1 |
| neighbour collapsed (< 4 m) | 1 | Gravel Road 1 |

| stub prefab | x | z | reason | CS1 segments here |
|---|---:|---:|---|---|
| Highway Oneway - 2 lanes | 595 | 234 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 4 lanes | 465 | -4518 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Twoway - 2 lanes | -3696 | -2945 | connected in the data, not joined in the game | USRP 2-Lane National Road_Data [roads, imported]; USRP 2-Lane National Road B [roads, bridge override (import-bridges*.json)] |
| Highway Oneway - 3 lanes | 1919 | -3004 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 3 lanes | 414 | -4472 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 2 lanes | -4397 | 15 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Twoway - 2 lanes | -3353 | -3417 | connected in the data, not joined in the game | USRP 2-Lane National Road B [roads, bridge override (import-bridges*.json)]; USRP 2-Lane National Road_Data [roads, imported] |
| Highway Oneway - 2 lanes | -4563 | 145 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 2 lanes | 591 | 249 | connected in the data, not joined in the game | USRP 2L Interstate B [highways, bridge override (import-bridges*.json)]; USRP 2L Interstate_Data [highways, imported] |
| Highway Oneway - 2 lanes | -4571 | 132 | connected in the data, not joined in the game | USRP 2L Interstate_Data [highways, imported]; USRP 2L Interstate B [highways, bridge override (import-bridges*.json)] |
| Highway Oneway - 3 lanes | 1160 | -4005 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 3 lanes | 1905 | -2993 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 3 lanes | 1145 | -3994 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Double Train Track | -250 | -6805 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -398 | -5800 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -423 | -3356 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 685 | 2683 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Ourense SA E [rail, imported]; R2EUR W GrCo 2x2 Ourense SA B [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 166 | 2777 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 3679 | 2853 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | 6492 | 2407 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -345 | -2864 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Pedrido B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Pedrido E [rail, imported] |
| Double Train Track | 3806 | 2833 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -305 | -6433 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 283 | 2756 | connected in the data, not joined in the game | R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 6872 | 2347 | connected in the data, not joined in the game | R2USA W GrWo 2x2_Data [rail, imported]; R2USA W TrWo 2x2 TRH Girder 02_Data [rail, bridge override (import-bridges*.json)] |
| Double Train Track | -304 | -2602 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Pedrido B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Pedrido E [rail, imported] |
| Double Train Track | -465 | -3623 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | -452 | -5436 | connected in the data, not joined in the game | R2USA W GrWo 2x2 B0 [rail, bridge override (import-bridges*.json)]; R2USA W GrWo 2x2_Data [rail, imported] |
| Double Train Track | 871 | 2649 | connected in the data, not joined in the game | R2EUR W GrCo 2x2 Ourense SA B [rail, bridge override (import-bridges*.json)]; R2EUR W GrCo 2x2 Ourense SA E [rail, imported] |
| Gravel Road | -1536 | 4298 | connected in the data, not joined in the game | Gravel Road [roads, imported]; Gravel Road Elevated [roads, bridge override (import-bridges*.json)] |

