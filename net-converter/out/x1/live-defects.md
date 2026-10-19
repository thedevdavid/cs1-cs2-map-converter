# Live defects (3418 segments, 3273 nodes)

Walls, ship paths, power and decor (High-voltage Line, QuaySmall01, Narrow Boatway, Sound Barrier, Medium Seaway, Arc Pedestrian Bridge) are open-ended by nature and left out of the dead-end, gap and tail checks; 72 wall ends inside the border.

## Dead ends inside the border: 20

## Facing gap pairs: 3
| gap m | prefab | end A | end B |
|---:|---|---|---|
| 44 | Highway Oneway - 3 lanes / Highway Oneway - 3 lanes | (837, 4022) | (799, 4000) |
| 68 | Highway Oneway - 4 lanes / Highway Oneway - 3 lanes | (463, -4520) | (412, -4474) |
| 80 | Gravel Road / Gravel Road | (1462, 6173) | (1383, 6161) |

## Ramp tails (dead end within 30 m of an unconnected same-class segment): 7
| dist m | stub prefab | near | at |
|---:|---|---|---|
| 7.6 | Pavement Path | Small Road | (4783, -683) |
| 9.2 | Pavement Path | Pavement Path | (373, -3031) |
| 10.9 | Pavement Path | Gravel Road | (1577, 4694) |
| 12.8 | Pavement Path | Gravel Road | (737, 4853) |
| 16.0 | Pavement Path | Pavement Path | (295, -3026) |
| 23.8 | Highway Oneway - 3 lanes | Highway Oneway - 3 lanes | (2302, 255) |
| 23.8 | Highway Oneway - 3 lanes | Highway Oneway - 3 lanes | (2293, 277) |

## Other dead ends: 7
| prefab | at |
|---|---|
| Gravel Road | (-3987, 334) |
| Gravel Road | (-229, -2362) |
| Medium Road | (-643, -1188) |
| Medium Road | (-480, -1481) |
| Gravel Road | (-768, 4363) |
| Gravel Road | (1104, 4897) |
| Pavement Path | (4787, -709) |

## Wavy chains (kink > 6 % with ≥ 2 sign flips): 132
| nodes | worst kink | flips | prefab | worst at |
|---:|---:|---:|---|---|
| 9 | 51 % | 3 | Medium Road | (-6295, 741) y=301 |
| 9 | 51 % | 3 | Highway Oneway - 2 lanes | (-6295, 741) y=301 |
| 17 | 50 % | 4 | Highway Oneway - 4 lanes | (944, -4288) y=139 |
| 17 | 50 % | 4 | Highway Oneway - 3 lanes | (944, -4288) y=139 |
| 11 | 50 % | 3 | Highway Oneway - 3 lanes | (927, -4283) y=139 |
| 11 | 50 % | 3 | Highway Oneway - 4 lanes | (927, -4283) y=139 |
| 40 | 41 % | 18 | Highway Oneway - 3 lanes | (1334, -3747) y=152 |
| 40 | 41 % | 18 | Highway Oneway - 3 lanes | (1334, -3747) y=152 |
| 7 | 39 % | 3 | Medium Road | (-4136, 87) y=155 |
| 7 | 39 % | 3 | Medium Road | (-4136, 87) y=155 |
| 41 | 38 % | 22 | Highway Oneway - 3 lanes | (1347, -3757) y=152 |
| 41 | 38 % | 22 | Highway Oneway - 3 lanes | (1347, -3757) y=152 |
| 41 | 38 % | 21 | Highway Oneway - 2 lanes | (-5516, 190) y=223 |
| 41 | 38 % | 21 | Highway Oneway - 2 lanes | (-5516, 190) y=223 |
| 59 | 37 % | 27 | High-voltage Line | (1012, 3828) y=132 |
| 59 | 37 % | 27 | High-voltage Line | (1012, 3828) y=132 |
| 57 | 29 % | 28 | Double Train Track | (-304, -2602) y=115 |
| 57 | 29 % | 28 | Double Train Track | (-304, -2602) y=115 |
| 9 | 29 % | 4 | Highway Oneway - 1 lane | (3505, 2110) y=200 |
| 9 | 29 % | 4 | Highway Oneway - 1 lane | (3505, 2110) y=200 |
| 9 | 27 % | 2 | Gravel Road | (-1437, 5257) y=249 |
| 9 | 27 % | 2 | Gravel Road | (-1437, 5257) y=249 |
| 7 | 26 % | 3 | Highway Oneway - 1 lane | (3542, 2151) y=200 |
| 7 | 26 % | 3 | Highway Oneway - 1 lane | (3542, 2151) y=200 |
| 20 | 25 % | 9 | Highway Oneway - 2 lanes | (-7145, 1828) y=316 |

## Wall ends on another wall, not joined (≤ 1 m): 14
The 0.0 m pairs may be orphans left by failed wall edges.
| dist m | Δy m | end prefab | other wall | where | at |
|---:|---:|---|---|---|
| 0.0 | 0.3 | QuaySmall01 | QuaySmall01 | on its end node | (-2058, -1500) |
| 0.0 | 0.0 | QuaySmall01 | QuaySmall01 | on its end node | (-2118, -1425) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (-2177, -1349) |
| 0.0 | 0.0 | QuaySmall01 | QuaySmall01 | on its end node | (-389, -1218) |
| 0.0 | 0.0 | QuaySmall01 | QuaySmall01 | on its end node | (-588, -1335) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (-431, -1258) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (636, -4634) |
| 0.0 | 0.0 | QuaySmall01 | QuaySmall01 | on its end node | (679, -4684) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (675, -4808) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (3504, 1976) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (3675, 1971) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (3527, 1973) |
| 0.0 | 0.0 | QuaySmall01 | QuaySmall01 | on its end node | (3916, 4826) |
| 0.0 | -0.0 | QuaySmall01 | QuaySmall01 | on its end node | (3969, 4876) |
