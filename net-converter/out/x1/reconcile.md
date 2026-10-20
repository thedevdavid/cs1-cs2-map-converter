# Reconciliation with the live game

Live graph `out\x1\live-graph.json`: 3269 segments, 3106 nodes. Coverage: 5 points per import edge (t = 0.1 … 0.9), each within 6 m of a live segment (chord + sagitta allowance, or the import curve when both ends coincide).

Statuses: covered = all 5 points on a live segment of the mapped prefab; wrong prefab = all 5 on some network; partial = 1–4; missing = none. Retry = missing, or partial with at most 2 points.

### After fixes (current import-*.json)

| layer | prefab | edges | covered | wrong prefab | partial | missing | → retry |
|---|---|---:|---:|---:|---:|---:|---:|
| highways | Highway Oneway - 1 lane | 296 | 296 | 0 | 0 | 0 | 0 |
| highways | Highway Oneway - 2 lanes | 905 | 905 | 0 | 0 | 0 | 0 |
| highways | Highway Oneway - 3 lanes | 537 | 506 | 0 | 6 | 25 | 31 |
| highways | Highway Oneway - 4 lanes | 29 | 29 | 0 | 0 | 0 | 0 |
| rail | Double Train Track | 403 | 396 | 5 | 1 | 1 | 1 |
| roads | Gravel Road | 1261 | 1260 | 0 | 1 | 0 | 0 |
| roads | Medium Road | 39 | 39 | 0 | 0 | 0 | 0 |
| roads | Pavement Path | 15 | 15 | 0 | 0 | 0 | 0 |
| roads | Small Road | 308 | 308 | 0 | 0 | 0 | 0 |
| **highways** | **all** | 1767 | 1736 | 0 | 6 | 25 | 31 |
| **rail** | **all** | 403 | 396 | 5 | 1 | 1 | 1 |
| **roads** | **all** | 1623 | 1622 | 0 | 1 | 0 | 0 |

### Edges not fully covered

| layer | edge | prefab | status | points mapped/any | x | z |
|---|---|---|---|---|---:|---:|
| highways | e1819_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2969 | 795 |
| highways | e3304_0 | Highway Oneway - 3 lanes | Partial | 0/1 | 2987 | 855 |
| highways | e3851_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2943 | 733 |
| highways | e7805_0g0a | Highway Oneway - 3 lanes | Missing | 0/0 | 2462 | 325 |
| highways | e7805_0g0b | Highway Oneway - 3 lanes | Partial | 0/1 | 2489 | 337 |
| highways | e7805_0g1aa | Highway Oneway - 3 lanes | Partial | 0/2 | 2515 | 348 |
| highways | e7805_0g1ab | Highway Oneway - 3 lanes | Missing | 0/0 | 2533 | 356 |
| highways | e7805_0g1b | Highway Oneway - 3 lanes | Missing | 0/0 | 2555 | 366 |
| highways | e10196_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2416 | 331 |
| highways | e11041_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2332 | 294 |
| highways | e12516_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2981 | 720 |
| highways | e12550_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2837 | 533 |
| highways | e12979_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2342 | 272 |
| highways | e14126_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2604 | 414 |
| highways | e15401_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2712 | 438 |
| highways | e15830_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2615 | 392 |
| highways | e20175_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2697 | 458 |
| highways | e20290_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2940 | 653 |
| highways | e20430_0b | Highway Oneway - 3 lanes | Partial | 2/2 | 816 | 4012 |
| highways | e24272_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2906 | 668 |
| highways | e28200_0g0a | Highway Oneway - 3 lanes | Missing | 0/0 | 2548 | 389 |
| highways | e28200_0g0b | Highway Oneway - 3 lanes | Partial | 0/1 | 2524 | 379 |
| highways | e28200_0g1aa | Highway Oneway - 3 lanes | Partial | 0/2 | 2499 | 368 |
| highways | e28200_0g1ab | Highway Oneway - 3 lanes | Missing | 0/0 | 2479 | 359 |
| highways | e28200_0g1b | Highway Oneway - 3 lanes | Missing | 0/0 | 2455 | 348 |
| highways | e32111_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2813 | 551 |
| highways | e32957_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2758 | 501 |
| highways | e33179_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2426 | 309 |
| highways | e34925_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2892 | 590 |
| highways | e36051_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2863 | 607 |
| highways | e36113_0 | Highway Oneway - 3 lanes | Missing | 0/0 | 2778 | 482 |
| rail | e6950_0 | Double Train Track | Missing | 0/0 | 5773 | 2521 |
| rail | e25368_0 | Double Train Track | Partial | 4/4 | -182 | 2388 |
| rail | e30706_0g0a | Double Train Track | WrongPrefab | 1/5 | -1403 | 4242 |
| rail | e30706_0g0b | Double Train Track | WrongPrefab | 0/5 | -1419 | 4279 |
| rail | e30706_0g1 | Double Train Track | WrongPrefab | 0/5 | -1435 | 4317 |
| rail | e30706_0g2a | Double Train Track | WrongPrefab | 0/5 | -1450 | 4354 |
| rail | e30706_0g2b | Double Train Track | WrongPrefab | 2/5 | -1465 | 4392 |
| roads | e35131_0 | Gravel Road | Partial | 4/4 | 6875 | 2817 |

### Retry files

Not written (pass --retry; entity ids are only valid for the running game session).

## Converter fixes (round 2)

- Hairpins (Bézier length > 1.6 × chord, self-intersection, or a control point behind its end), checked on every imported CS1 segment: **0** found, 0 rebuilt.
- Exact duplicate edges dropped: **0**.
- Structure now from node elevation (not the B/E/S/T prefab suffix): 177 imported edges differ from the suffix-based class (Ground→Elevated 171, Elevated→Ground 6).
- Elevation ramps (Δelevation > 6 m over < 60 m) smoothed to ≤ 8 %: **14** ramps, 18 nodes raised/lowered, 5 still steeper than 12 % (blocked by a junction or the border).

- Node merges refused because the nodes are stacked (height difference ≥ 2 m): **9**.

## Near-parallel pairs resolved (round 3)

- diverge (tangents spread): 21

| layer | node | edge 1 | edge 2 | angle before ° | after ° | category |
|---|---|---|---|---:|---:|---|
| rail | n21444 | e5748_0 | e12499_0 | 6.6 | 12.0 | diverge (tangents spread) |
| rail | n12247 | e25368_0 | e31426_0 | 2.9 | 12.0 | diverge (tangents spread) |
| rail | n12468 | e10974_0 | e29479_0 | 8.4 | 12.0 | diverge (tangents spread) |
| rail | n21726 | e18896_0 | e28929_0 | 0.0 | 12.0 | diverge (tangents spread) |
| roads | n10794 | e27640_0 | e33942_0 | 0.6 | 12.0 | diverge (tangents spread) |
| roads | n29712 | e10075_0 | e13594_0 | 1.0 | 12.0 | diverge (tangents spread) |
| highways | n27056 | e501_0 | e13450_0 | 1.4 | 12.0 | diverge (tangents spread) |
| highways | n17002 | e1009_0 | e2218_0 | 6.1 | 12.0 | diverge (tangents spread) |
| highways | n14204 | e28080_0 | e31965_0 | 9.8 | 12.0 | diverge (tangents spread) |
| highways | n30207 | e1638_0 | e26105_0b | 7.9 | 12.0 | diverge (tangents spread) |
| highways | n24717 | e1808_0g0 | e30217_0g0 | 0.9 | 12.0 | diverge (tangents spread) |
| highways | n30535 | e8119_0 | e25129_0 | 0.5 | 12.0 | diverge (tangents spread) |
| highways | n18884 | e3338_0 | e36804_0 | 0.8 | 12.0 | diverge (tangents spread) |
| highways | n30303 | e8272_0 | e9411_0 | 8.9 | 12.0 | diverge (tangents spread) |
| highways | n26558 | e27960_0 | e35790_0 | 0.0 | 12.0 | diverge (tangents spread) |
| highways | n7208 | e18755_0 | e26448_0 | 9.4 | 12.0 | diverge (tangents spread) |
| highways | n31799 | e16285_0 | e18863_0 | 2.4 | 12.0 | diverge (tangents spread) |
| highways | n20924 | e18751_0 | e26674_0 | 0.9 | 12.0 | diverge (tangents spread) |
| highways | n16847 | e19493_0 | e32327_0 | 5.7 | 12.0 | diverge (tangents spread) |
| highways | n6721 | e19786_0 | e29495_0 | 8.6 | 12.0 | diverge (tangents spread) |
| highways | n16119 | e28976_0 | e36589_0 | 0.5 | 12.0 | diverge (tangents spread) |

## Acute junctions remaining (< 10°)

Two edges of one layer leaving the same node almost parallel. CS2 builds a long junction there, which looks like a ramp folding back into the highway.


| layer | node | x | z | edge 1 | edge 2 | angle ° |
|---|---|---:|---:|---|---|---:|

## Overlaps (same layer, < 4 m apart for > 20 m)

- rail: 4 pairs (4 share a node, i.e. a shallow diverge; 0 unconnected)
- roads: 1 pairs (1 share a node, i.e. a shallow diverge; 0 unconnected)
- highways: 7 pairs (7 share a node, i.e. a shallow diverge; 0 unconnected)

| layer | edge 1 | edge 2 | close m | shared node | x | z | CS1 prefabs |
|---|---|---|---:|---|---:|---:|---|
| rail | e25368_0 | e31426_0 | 38 | yes | -182 | 2388 | 2610252544.R2USA W GrWo 2x2_Data / 2610252544.R2USA W GrWo 2x2_Data |
| rail | e18896_0 | e28929_0 | 35 | yes | 1401 | 2588 | 2610252544.R2USA W GrWo 2x2_Data / 2610252544.R2USA W GrWo 2x2_Data |
| highways | e7747_0 | e23246_0 | 25 | yes | 204 | 3486 | 2452038713.USRP 1L Interstate Ramp_Data / 2452038713.USRP 1L Interstate Ramp_Data |
| highways | e27960_0 | e35790_0 | 25 | yes | 3591 | 2353 | 2452038713.USRP 1L Interstate Ramp E / 2452038713.USRP 1L Interstate Ramp_Data |
| rail | e5748_0 | e12499_0 | 23 | yes | -490 | 2975 | 2610252544.R2USA W GrWo 2x2_Data / 2610252544.R2USA W GrWo 2x2_Data |
| highways | e22957_0 | e23275_0 | 23 | yes | -1654 | -1722 | 2452038713.USRP 1L Interstate Ramp E / 2452038713.USRP 1L Interstate Ramp E |
| roads | e27640_0 | e33942_0 | 22 | yes | 869 | 4891 | Gravel Road / Gravel Road |
| highways | e28080_0 | e31965_0 | 21 | yes | 251 | 3442 | 2452038713.USRP 1L Interstate Ramp_Data / 2452038713.USRP 1L Interstate Ramp_Data |
| rail | e10974_0 | e29479_0 | 21 | yes | 21 | 2804 | 2610252544.R2USA W GrWo 2x2_Data / 2610252544.R2USA W GrWo 2x2_Data |
| highways | e1638_0 | e26105_0b | 21 | yes | 3440 | 2065 | 2452038713.USRP 1L Interstate Ramp E / 2452038713.USRP 1L Interstate Ramp E |
| highways | e501_0 | e13450_0 | 21 | yes | 93 | 58 | 2452038713.USRP 1L Interstate Ramp_Data / 2452038713.USRP 1L Interstate Ramp_Data |
| highways | e31526_0 | e32991_0 | 21 | yes | 248 | 153 | 2452038713.USRP 1L Interstate Ramp_Data / 2452038713.USRP 1L Interstate Ramp_Data |

