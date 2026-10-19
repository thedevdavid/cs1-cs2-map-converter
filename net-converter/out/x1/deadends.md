# Dead ends

Network ends more than 60 m inside the ±7168 m border, explained from the CS1 dump: the CS1 segments meeting within 2 m of the end and what the converter did with each.

Live graph `out\x1\live-graph.json` (3273 nodes).

## Live game (round-1 import, before round-3 fixes): 78

| reason | ends | stub prefabs |
|---|---:|---|
| CS1 dead end (genuine) | 64 | QuaySmall01 29, Narrow Boatway 14, Gravel Road 5, Pavement Path 5, High-voltage Line 3, Sound Barrier 3, Medium Road 2, Highway Oneway - 3 lanes 2, Medium Seaway 1 |
| connected in the data, not joined in the game | 11 | QuaySmall01 8, Highway Oneway - 3 lanes 2, Highway Oneway - 4 lanes 1 |
| neighbour collapsed (< 4 m) | 2 | Sound Barrier 1, Gravel Road 1 |
| no CS1 node here (game split, or merged/moved by the converter) | 1 | Highway Oneway - 3 lanes 1 |

### 20 worst live dead ends

| stub prefab | x | z | reason | CS1 segments here [layer, converter decision] |
|---|---:|---:|---|---|
| Highway Oneway - 3 lanes | 2302 | 255 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, imported] |
| Highway Oneway - 3 lanes | 2293 | 277 | CS1 dead end (genuine) | USRP 3L Interstate_Data [highways, imported] |
| Highway Oneway - 3 lanes | 799 | 4000 | connected in the data, not joined in the game | USRP 3L Interstate E [highways, imported]; USRP 3L Interstate E [highways, imported] |
| Highway Oneway - 3 lanes | 837 | 4022 | no CS1 node here (game split, or merged/moved by the converter) |  |
| Highway Oneway - 3 lanes | 412 | -4474 | connected in the data, not joined in the game | USRP 3L Interstate_Data [highways, imported]; USRP 4L Interstate_Data [highways, imported] |
| Highway Oneway - 4 lanes | 463 | -4520 | connected in the data, not joined in the game | USRP 4L Interstate_Data [highways, imported]; USRP 4L Interstate_Data [highways, imported] |
| Medium Seaway | 405 | 3 | CS1 dead end (genuine) | Ship Path [shippaths, shippaths] |
| Medium Road | -643 | -1188 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Medium Road | -480 | -1481 | CS1 dead end (genuine) | USRP 4-Lane National Road_Data [roads, imported] |
| Sound Barrier | 436 | 182 | CS1 dead end (genuine) | Flood Wall [walls, walls] |
| Sound Barrier | 428 | 212 | CS1 dead end (genuine) | Flood Wall [walls, walls] |
| QuaySmall01 | -389 | -1218 | CS1 dead end (genuine) | Retaining Walls - AiO #01_Data [walls, walls] |
| High-voltage Line | -315 | -1340 | CS1 dead end (genuine) | 345 kV H Frame Ceramic Insulator_Data [power, power] |
| QuaySmall01 | -431 | -1258 | connected in the data, not joined in the game | Retaining Walls - AiO #01_Data [walls, walls]; Retaining Walls - AiO #01_Data [walls, walls] |
| QuaySmall01 | -588 | -1335 | CS1 dead end (genuine) | Retaining Walls - AiO #01_Data [walls, walls] |
| QuaySmall01 | -775 | -1400 | CS1 dead end (genuine) | Retaining Walls - AiO #01_Data [walls, walls] |
| Gravel Road | -229 | -2362 | CS1 dead end (genuine) | Gravel Road [roads, imported] |
| Pavement Path | 295 | -3026 | CS1 dead end (genuine) | Pedestrian Gravel [roads, imported] |
| Pavement Path | 373 | -3031 | CS1 dead end (genuine) | Pedestrian Gravel [roads, imported] |
| QuaySmall01 | -3046 | -428 | CS1 dead end (genuine) | Retaining Walls - AiO #01_Data [walls, walls] |

## Predicted for the regenerated import-all.json (fresh map): 16

| reason | ends | stub prefabs |
|---|---:|---|
| CS1 dead end (genuine) | 15 | Pavement Path 6, Gravel Road 5, Highway Oneway - 3 lanes 2, Medium Road 2 |
| neighbour collapsed (< 4 m) | 1 | Gravel Road 1 |

| stub prefab | x | z | reason | CS1 segments here |
|---|---:|---:|---|---|
| Gravel Road | 1462 | 6173 | neighbour collapsed (< 4 m) | Gravel Road [roads, collapsed (< 4 m)]; Gravel Road [roads, imported] |

