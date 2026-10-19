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
| Highway Oneway - 3 lanes | 2302 | 255 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, imported] |
| Highway Oneway - 3 lanes | 2293 | 277 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, imported] |
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
| Gravel Road | 1383 | 6161 | CS1 dead end (genuine) | Gravel Road Bridge [roads, imported] |
| Gravel Road | 1462 | 6173 | neighbour collapsed (< 4 m) | Gravel Road [roads, collapsed (< 4 m)]; Gravel Road [roads, imported] |

## Predicted for the regenerated import-all.json (fresh map): 16

| reason | ends | stub prefabs |
|---|---:|---|
| CS1 dead end (genuine) | 15 | Pavement Path 6, Gravel Road 5, Highway Oneway - 3 lanes 2, Medium Road 2 |
| neighbour collapsed (< 4 m) | 1 | Gravel Road 1 |

| stub prefab | x | z | reason | CS1 segments here |
|---|---:|---:|---|---|
| Gravel Road | 1462 | 6173 | neighbour collapsed (< 4 m) | Gravel Road [roads, collapsed (< 4 m)]; Gravel Road [roads, imported] |

