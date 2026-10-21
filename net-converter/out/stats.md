# net-converter stats

Dump `C:\Users\dleva\AppData\Local\DakotaExport\2972879554_Dakota_Falls-20261019-063123.json`, mapping `mapping.tsv`.

## Output layers

| layer | edges | nodes | border nodes | length km | ground | elevated | tunnel |
|---|---:|---:|---:|---:|---:|---:|---:|
| highways | 1752 | 1734 | 10 | 118.0 | 1090 | 622 | 40 |
| rail | 410 | 419 | 4 | 33.7 | 332 | 66 | 12 |
| roads | 1619 | 1506 | 6 | 124.4 | 1597 | 22 | 0 |
| power | 60 | 62 | 1 | 12.8 | 60 | 0 | 0 |
| skip | 722 | 797 | 12 | 63.8 | 722 | 0 | 0 |

Only highways, rail and roads are written as import files. power, skip and unmapped appear in network.geojson only.

## Transform

- segments in 5532; wholly outside ±7168 m 766; clipped at the border 40 (→ 40 pieces)
- split for > 1500 m: 0; node merges (< 0.5 m): 118; short edges (< 4 m) collapsed into one node: 95; degenerate/duplicate edges dropped: 105

## CS2 prefabs used

| layer | CS2 prefab | edges | length km | from CS1 prefabs |
|---|---|---:|---:|---|
| highways | Highway Oneway - 1 lane | 282 | 15.8 | 2452038713.USRP 1L Interstate Ramp_Data, 2452038713.USRP 1L Interstate Ramp E |
| highways | Highway Oneway - 2 lanes | 901 | 59.7 | 2452038713.USRP 2L Interstate E, 2452038713.USRP 2L Interstate_Data, 2452038713.USRP 2L Interstate B, 2452038713.USRP 2L Interstate T, 2452038713.USRP 2L Interstate S |
| highways | Highway Oneway - 3 lanes | 540 | 40.7 | 2452038713.USRP 3L Interstate_Data, 2452038713.USRP 3L Interstate E, 2452038713.USRP 3L Interstate B, 2452038713.USRP 3L Interstate S |
| highways | Highway Oneway - 4 lanes | 29 | 1.8 | 2452038713.USRP 4L Interstate_Data, 2452038713.USRP 4L Interstate E |
| rail | Double Train Track | 410 | 33.7 | 2610252544.R2USA W GrWo 2x2_Data, 2610252544.R2USA W GrWo 2x2 T0, 2932555116.R2EUR W GrCo 2x2 Pedrido E, 2610252544.R2USA W GrWo 2x2 S0, 2836620152.R2EUR W GrCo 2x2 Ourense SA E, 2610252544.R2USA W GrWo 2x2 E0 |
| roads | Gravel Road | 1257 | 97.1 | Gravel Road, Gravel Road Elevated |
| roads | Medium Road | 39 | 2.5 | 2097539846.USRP 4-Lane National Road_Data, 2098643957.USRP 3-Lane National Road_Data, 2098643957.USRP 3-Lane National Road E |
| roads | Pavement Path | 15 | 0.6 | Pedestrian Gravel |
| roads | Small Road | 308 | 24.1 | 2097530472.USRP 2-Lane National Road_Data, 2097530472.USRP 2-Lane National Road B |

## Unmapped CS1 prefabs (need a human decision)

None: every network prefab in the dump matched a rule.

## Mapping decisions flagged in mapping.tsv

- `2097530472.USRP 2-Lane National Road_Data` → Small Road: DECISION: 2-lane rural road; Small Road keeps lane count (alt: Medium Road for width)
- `2098643957.USRP 3-Lane National Road_Data` → Medium Road: 2+1 lanes, no exact CS2 twin (alt: Medium Road Asymmetric - 1+3 lanes)
- `2097530472.USRP 2-Lane National Road B` → Small Road: DECISION: 2-lane rural road; Small Road keeps lane count (alt: Medium Road for width)
- `2098643957.USRP 3-Lane National Road E` → Medium Road: 2+1 lanes, no exact CS2 twin (alt: Medium Road Asymmetric - 1+3 lanes)

## Full prefab mapping

| CS1 prefab | segments | → CS2 | layer | rule line |
|---|---:|---|---|---:|
| Monorail Oneway Track | 96 | Arc Pedestrian Bridge | decor | 84 |
| Monorail Track - No Pylons | 96 | Arc Pedestrian Bridge | decor | 84 |
| 2452038713.USRP 1L Interstate Ramp E | 84 | Highway Oneway - 1 lane | highways | 11 |
| 2452038713.USRP 1L Interstate Ramp_Data | 167 | Highway Oneway - 1 lane | highways | 11 |
| 2452038713.USRP 2L Interstate B | 26 | Highway Oneway - 2 lanes | highways | 24 |
| 2452038713.USRP 2L Interstate E | 113 | Highway Oneway - 2 lanes | highways | 24 |
| 2452038713.USRP 2L Interstate S | 10 | Highway Oneway - 2 lanes | highways | 24 |
| 2452038713.USRP 2L Interstate T | 26 | Highway Oneway - 2 lanes | highways | 24 |
| 2452038713.USRP 2L Interstate_Data | 665 | Highway Oneway - 2 lanes | highways | 24 |
| 2452038713.USRP 3L Interstate B | 42 | Highway Oneway - 3 lanes | highways | 25 |
| 2452038713.USRP 3L Interstate E | 32 | Highway Oneway - 3 lanes | highways | 25 |
| 2452038713.USRP 3L Interstate S | 2 | Highway Oneway - 3 lanes | highways | 25 |
| 2452038713.USRP 3L Interstate_Data | 535 | Highway Oneway - 3 lanes | highways | 25 |
| 2452038713.USRP 4L Interstate E | 7 | Highway Oneway - 4 lanes | highways | 26 |
| 2452038713.USRP 4L Interstate_Data | 15 | Highway Oneway - 4 lanes | highways | 26 |
| Highway | 5 | Highway Oneway - 3 lanes | highways | 32 |
| Highway Connection | 2 | Highway Oneway - 3 lanes | highways | 31 |
| Two Lane Highway | 5 | Highway Oneway - 2 lanes | highways | 34 |
| Two Lane Highway Twoway Connection | 3 | Highway Twoway - 2 lanes | highways | 33 |
| 2920928696.345 kV H Frame Ceramic Insulator_Data | 66 | High-voltage Line | power | 62 |
| 2610252544.R2USA W GrWo 2x2 B0 | 16 | Double Train Track | rail | 55 |
| 2610252544.R2USA W GrWo 2x2 E0 | 1 | Double Train Track | rail | 55 |
| 2610252544.R2USA W GrWo 2x2 S0 | 6 | Double Train Track | rail | 55 |
| 2610252544.R2USA W GrWo 2x2 T0 | 16 | Double Train Track | rail | 55 |
| 2610252544.R2USA W GrWo 2x2_Data | 409 | Double Train Track | rail | 55 |
| 2836620152.R2EUR W GrCo 2x2 Ourense SA B | 2 | Double Train Track | rail | 55 |
| 2836620152.R2EUR W GrCo 2x2 Ourense SA E | 4 | Double Train Track | rail | 55 |
| 2926036331.R2USA W TrWo 2x2 TRH Girder 02_Data | 7 | Double Train Track | rail | 55 |
| 2932555116.R2EUR W GrCo 2x2 Pedrido B | 3 | Double Train Track | rail | 55 |
| 2932555116.R2EUR W GrCo 2x2 Pedrido E | 4 | Double Train Track | rail | 55 |
| Train Connection Track | 4 | Double Train Track | rail | 58 |
| 2097530472.USRP 2-Lane National Road B | 7 | Small Road | roads | 39 |
| 2097530472.USRP 2-Lane National Road_Data | 372 | Small Road | roads | 39 |
| 2097539846.USRP 4-Lane National Road_Data | 11 | Medium Road | roads | 41 |
| 2098643957.USRP 3-Lane National Road E | 2 | Medium Road | roads | 40 |
| 2098643957.USRP 3-Lane National Road_Data | 23 | Medium Road | roads | 40 |
| Gravel Road | 1485 | Gravel Road | roads | 44 |
| Gravel Road Bridge | 2 | Gravel Road | roads | 44 |
| Gravel Road Elevated | 11 | Gravel Road | roads | 44 |
| Pedestrian Elevated | 7 | Pavement Path | roads | 65 |
| Pedestrian Gravel | 17 | Pavement Path | roads | 65 |
| Ship Connection Path | 1 | Medium Seaway | shippaths | 72 |
| Ship Path | 117 | Medium Seaway | shippaths | 72 |
| 1889660581.Concrete Barrier - raisable_Data | 22 | SKIP | skip | 74 |
| 1889660581.Concrete Barrier_Data | 39 | SKIP | skip | 74 |
| 1889660581.Double Steel Barrier_Data | 1 | SKIP | skip | 74 |
| 2105051034.Terraforming Network Ext 0.5U_Data | 225 | SKIP | skip | 78 |
| 2105051034.Terraforming Network Ext 1U_Data | 2 | SKIP | skip | 78 |
| 2105051034.Terraforming Network Ext 2U_Data | 5 | SKIP | skip | 78 |
| 2105051034.Terraforming Network Ext 8U_Data | 1 | SKIP | skip | 78 |
| 2674210208.Field Network 0_Data | 36 | SKIP | skip | 77 |
| 2674210208.Field Wheat Cut Network 3_Data | 76 | SKIP | skip | 77 |
| 2674210208.Field Wheat Network 3_Data | 102 | SKIP | skip | 77 |
| 2674210208.Field Wheat Young Network_Data | 55 | SKIP | skip | 77 |
| Airplane Connection Path | 4 | SKIP | skip | 83 |
| Airplane Path | 315 | SKIP | skip | 83 |
| Castle Wall 1 | 10 | SKIP | skip | 75 |
| Castle Wall 3 | 2 | SKIP | skip | 75 |
| 2966434181.Retaining Walls - AiO #01_Data | 90 | QuaySmall01 | walls | 68 |
| Canal | 14 | Narrow Boatway | walls | 69 |
| Flood Wall | 2 | Sound Barrier | walls | 70 |
| Quay | 7 | QuaySmall01 | walls | 67 |

## Inventory (CS1 dump)

Nodes 5582, segments 5532, net prefabs 62, water sources 65, outside connections 22.

### Segments per prefab

| CS1 prefab | AI | segments | length km |
|---|---|---:|---:|
| Gravel Road | RoadAI | 1485 | 108.96 |
| 2452038713.USRP 2L Interstate_Data | RoadAI | 665 | 49.11 |
| 2452038713.USRP 3L Interstate_Data | RoadAI | 535 | 44.80 |
| 2610252544.R2USA W GrWo 2x2_Data | TrainTrackAI | 409 | 35.35 |
| Airplane Path | FlightPathAI | 315 | 31.16 |
| 2097530472.USRP 2-Lane National Road_Data | RoadAI | 372 | 29.10 |
| 2920928696.345 kV H Frame Ceramic Insulator_Data | PowerLineAI | 66 | 14.29 |
| 2105051034.Terraforming Network Ext 0.5U_Data | CanalAI | 225 | 13.61 |
| 2674210208.Field Wheat Network 3_Data | RoadAI | 102 | 12.24 |
| Ship Path | ShipPathAI | 117 | 10.90 |
| 2674210208.Field Wheat Cut Network 3_Data | RoadAI | 76 | 9.07 |
| 2452038713.USRP 1L Interstate Ramp_Data | RoadAI | 167 | 7.12 |
| 2674210208.Field Wheat Young Network_Data | RoadAI | 55 | 7.05 |
| 2452038713.USRP 2L Interstate E | RoadBridgeAI | 113 | 6.96 |
| 2966434181.Retaining Walls - AiO #01_Data | RoadAI | 90 | 5.65 |
| 2674210208.Field Network 0_Data | RoadAI | 36 | 4.42 |
| 2452038713.USRP 1L Interstate Ramp E | RoadBridgeAI | 84 | 4.38 |
| 2452038713.USRP 3L Interstate B | RoadBridgeAI | 42 | 3.71 |
| 2452038713.USRP 3L Interstate E | RoadBridgeAI | 32 | 2.59 |
| 2452038713.USRP 2L Interstate B | RoadBridgeAI | 26 | 2.59 |
| 2452038713.USRP 2L Interstate T | RoadTunnelAI | 26 | 2.23 |
| Monorail Oneway Track | MonorailTrackAI | 96 | 1.85 |
| 2610252544.R2USA W GrWo 2x2 T0 | TrainTrackTunnelAI | 16 | 1.44 |
| 2610252544.R2USA W GrWo 2x2 B0 | TrainTrackBridgeAI | 16 | 1.41 |
| 1889660581.Concrete Barrier - raisable_Data | DecorationWallAI | 22 | 1.38 |
| 2105051034.Terraforming Network Ext 2U_Data | CanalAI | 5 | 1.32 |
| Canal | CanalAI | 14 | 1.08 |
| Monorail Track - No Pylons | MonorailTrackAI | 96 | 1.06 |
| 2098643957.USRP 3-Lane National Road_Data | RoadAI | 23 | 0.99 |
| 2452038713.USRP 4L Interstate_Data | RoadAI | 15 | 0.90 |
| Pedestrian Elevated | PedestrianBridgeAI | 7 | 0.80 |
| 2452038713.USRP 2L Interstate S | RoadTunnelAI | 10 | 0.78 |
| 2097539846.USRP 4-Lane National Road_Data | RoadAI | 11 | 0.74 |
| Gravel Road Elevated | RoadBridgeAI | 11 | 0.71 |
| Pedestrian Gravel | PedestrianPathAI | 17 | 0.68 |
| 2097530472.USRP 2-Lane National Road B | RoadBridgeAI | 7 | 0.66 |
| Castle Wall 1 | DecorationWallAI | 10 | 0.65 |
| Quay | QuayAI | 7 | 0.51 |
| 2926036331.R2USA W TrWo 2x2 TRH Girder 02_Data | TrainTrackBridgeAI | 7 | 0.49 |
| 1889660581.Concrete Barrier_Data | DecorationWallAI | 39 | 0.49 |
| 2610252544.R2USA W GrWo 2x2 S0 | TrainTrackTunnelAI | 6 | 0.43 |
| 2452038713.USRP 4L Interstate E | RoadBridgeAI | 7 | 0.42 |
| Train Connection Track | TrainTrackAI | 4 | 0.36 |
| Two Lane Highway | RoadAI | 5 | 0.35 |
| Highway | RoadAI | 5 | 0.35 |
| 2836620152.R2EUR W GrCo 2x2 Ourense SA E | TrainTrackBridgeAI | 4 | 0.34 |
| Airplane Connection Path | FlightPathAI | 4 | 0.31 |
| 2932555116.R2EUR W GrCo 2x2 Pedrido E | TrainTrackBridgeAI | 4 | 0.27 |
| 2932555116.R2EUR W GrCo 2x2 Pedrido B | TrainTrackBridgeAI | 3 | 0.26 |
| Two Lane Highway Twoway Connection | RoadAI | 3 | 0.22 |
| 2836620152.R2EUR W GrCo 2x2 Ourense SA B | TrainTrackBridgeAI | 2 | 0.19 |
| 2105051034.Terraforming Network Ext 1U_Data | CanalAI | 2 | 0.17 |
| Gravel Road Bridge | RoadBridgeAI | 2 | 0.14 |
| Highway Connection | RoadAI | 2 | 0.12 |
| 2452038713.USRP 3L Interstate S | RoadTunnelAI | 2 | 0.12 |
| Castle Wall 3 | DecorationWallAI | 2 | 0.10 |
| 2105051034.Terraforming Network Ext 8U_Data | CanalAI | 1 | 0.09 |
| 2610252544.R2USA W GrWo 2x2 E0 | TrainTrackBridgeAI | 1 | 0.08 |
| 1889660581.Double Steel Barrier_Data | DecorationWallAI | 1 | 0.08 |
| Ship Connection Path | ShipPathAI | 1 | 0.08 |
| 2098643957.USRP 3-Lane National Road E | RoadBridgeAI | 2 | 0.07 |
| Flood Wall | FloodWallAI | 2 | 0.06 |

### Node elevation

CS1 `elevation` byte and measured y − terrain:

| bucket | elevation byte | y − terrain |
|---|---:|---:|
| < -20 | 0 | 115 |
| -20..-5 | 0 | 318 |
| -5..-0.5 | 0 | 1067 |
| ±0.5 | 5437 | 2129 |
| 0.5..5 | 66 | 1083 |
| 5..10 | 26 | 199 |
| 10..20 | 32 | 187 |
| 20..40 | 16 | 277 |
| > 40 | 5 | 207 |

Underground nodes 51; nodes with Outside flag 24; nodes beyond ±7168 m 778.

### Water sources

- Natural: 65 (inside ±7168 m: 59; height 178.8–739.5 m)

### Outside connections (CS1 buildings)

- plane: 4 (in 4, out 4)
- road: 13 (in 8, out 8)
- ship: 1 (in 1, out 1)
- train: 4 (in 4, out 4)

## Converter fixes (round 2)

- Hairpins (Bézier length > 1.6 × chord, self-intersection, or a control point behind its end), checked on every imported CS1 segment: **0** found, 0 rebuilt.
- Exact duplicate edges dropped: **0**.
- Structure now from node elevation (not the B/E/S/T prefab suffix): 218 imported edges differ from the suffix-based class (Ground→Elevated 214, Ground→Tunnel 2, Elevated→Ground 2).
- Elevation ramps (Δelevation > 6 m over < 60 m) smoothed to ≤ 8 %: **67** ramps, 30 nodes raised/lowered, 6 still steeper than 12 % (blocked by a junction or the border).

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
| highways | n24717 | e1808_0 | e30217_0 | 0.9 | 12.0 | diverge (tangents spread) |
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


## Other layers

- Power: 60 courses of High-voltage Line in import-power.json (nodes on the ground).
- Trees: 504,894 objects in 11 chunks under out\trees (points (2972879554_Dakota_Falls-20261019-063123-trees.json, trees/v2: y, flags, scale)); outside ±7168 m 158,877; dropped 73,855 (see trees/README.md).
- Pack rocks: no y (their y was dropped on 1112); options.grounding {mode boundsMin, sink 0.15, slopeSink} grounds every object without y from its prefab bounds (bridge 0.9.19).
- Surface-rock cliffs (round 34): 249 CS1 Brown Surface Rock Medium/Large on a ≥ 25° playable slope → CliffStack / CliffRock5 (contour yaw, sunk ≥ 1.5 m downhill); 799 stay rocks on gentler slopes, 0 near a net, 0 in water, 11 dropped as a second piece in a 16 m cell.
- Object y policy: y only > 3 m above the CS2 playable heightmap (10201528-dktflls-1to1.png), never for buildings: 2301 with y; against the old CS1-terrain rule 556 lose y, 0 gain it.
- Objects: 6130 of 8408 CS1 buildings/props got a CS2 target (72.9 %); SKIP 247, MANUAL 7, unmapped 0, outside ±7168 m 2024; 2301 carry y.

| CS1 prefab | count | decision |
|---|---:|---|
| 1978360460.Brown Surface Rock A Large_Data | 1463 | rzrock1, rzrock7, Rock21, Rock22, Rock23, Rock24, Rock25, Rock26, Rock27, Rock28, Rock29, Rock30 |
| 2803411105.Landscape Grass 01_Data | 1325 | Grass02 (Styx's), Grass05 (Styx's), gaga - Fern 1, PD-YellowFlower |
| 1988876538.Brown Modular C Formation 04_Data | 1102 | CliffStack, CliffRock6, CliffRock5 |
| 1978360460.Brown Surface Rock A Small_Data | 912 | RZaridrock3, RZaridrock4, Rock01, Rock02, Rock03, Rock04, Rock05, Rock06, Rock07, Rock08, Rock09, Rock10 |
| 2803411105.Landscape Grass 02_Data | 665 | Grass03 (Styx's), Grass06 (Styx's) |
| 2803411105.Landscape Grass 04_Data | 526 | gaga - Fern 1, gaga - Fern 2, Grass02 (Styx's) |
| 2803411105.Landscape Grass 03_Data | 342 | Grass01 (Styx's), Grass04 (Styx's), pd Bloom |
| 1978360460.Brown Surface Rock B Small_Data | 306 | RZaridrock3, RZaridrock4, Rock01, Rock02, Rock03, Rock04, Rock05, Rock06, Rock07, Rock08, Rock09, Rock10 |
| 1978360460.Brown Surface Rock A Medium_Data | 245 | RZaridrock1, RZaridrock2, RZaridrock5, Rock11, Rock12, Rock13, Rock14, Rock15, Rock16, Rock17, Rock18, Rock19, Rock20 |
| 2803411105.Landscape Grass 06_Data | 218 | pd Long Grass, ReikUndergrowth01Trimmed, ReikUndergrowth02Trimmed |
| 1978360460.Brown Surface Rock B Large_Data | 192 | rzrock1, rzrock7, Rock21, Rock22, Rock23, Rock24, Rock25, Rock26, Rock27, Rock28, Rock29, Rock30 |
| 1988121829.Grey Breakwater Rocks 01_Data | 172 | rzrock2, rzrock3, rzrock4, rzrock5, rzrock8 |
| 1978360460.Brown Surface Rock B Medium_Data | 137 | RZaridrock1, RZaridrock2, RZaridrock5, Rock11, Rock12, Rock13, Rock14, Rock15, Rock16, Rock17, Rock18, Rock19, Rock20 |
| 2074426936.Fallen Pine Stump 02_Data | 116 | LogRandom01 |
| 767694644.R69 Big Decal Dirt_Data | 113 | SKIP: no dirt/mud decal without EAI: converted to Dirty Surfaces Pack area surfaces (import-surfaces-decals.json) |
| 2074426936.Fallen Pine 02 TC_Data | 71 | Log01, Log02, Log03, Log04 |
| 2097539846.[US] 4-Lane Highway Pillar_Data | 42 | SKIP: CS2 places pillars for its own bridges |
| 2074426936.Fallen Pine 07 LP TC_Data | 40 | Log01, Log02, Log03, Log04 |
| 2074426936.Fallen Pine Stump 01_Data | 34 | LogRandom01 |
| 2074426936.Fallen Pine 07 LP_Data | 31 | Log01, Log02, Log03, Log04 |
| 2098643957.[US] 3-Lane Highway Pillar_Data | 28 | SKIP: CS2 places pillars for its own bridges |
| 2074426936.Fallen Pine 02_Data | 28 | Log01, Log02, Log03, Log04 |
| 2452038713.USRP 2L Interstate Pillar_Data | 25 | SKIP: CS2 places pillars for its own bridges |
| 2074426936.Fallen Pine 04_Data | 25 | Log01, Log02, Log03, Log04 |
| Park Streetlight 01 | 24 | StreetlightSingle01 |
| Warning Light Red | 24 | WarningLight01 |
| Modern Fence 02 | 22 | FenceResidentialPieceLow01 |
| Destroyed Road Prop 03 | 18 | RubblePileConcrete01, RubblePileConcrete02 |
| Trashpile | 18 | TrashbagRandom01 |
| 2074426936.Fallen Pine 03_Data | 15 | Log01, Log02, Log03, Log04 |
| 2074426936.Fallen Pine 06 LP_Data | 11 | Log01, Log02, Log03, Log04 |
| 2074426936.Fallen Pine 05 LP_Data | 10 | Log01, Log02, Log03, Log04 |
| 2074426936.Fallen Pine 01_Data | 9 | Log01, Log02, Log03, Log04 |
| High Vegetation 02 | 9 | GreenBushWild02 |
| 2452038713.USRP 3L Interstate Pillar_Data | 8 | SKIP: CS2 places pillars for its own bridges |
| Destroyed Road Prop 01 | 5 | RubblePileConcrete01, RubblePileConcrete02 |
| Flood Light Blue | 5 | SKIP: no floodlight prop in CS2 |
| 1251664417.Dam Electricity_Data | 4 | MANUAL: CS2 hydro dam (Hydroelectric_Power_Plant_01 Dam) is placed across the river by hand |
| 1988876538.Brown Modular C Formation 02_Data | 4 | CliffStack, CliffRock6, CliffRock5 |
| Destroyed Road Prop 02 | 4 | RubblePileConcrete01, RubblePileConcrete02 |
| 874716774.R69 Big Decal Mud 4_Data | 4 | SKIP: no dirt/mud decal without EAI: converted to Dirty Surfaces Pack area surfaces (import-surfaces-decals.json) |
| 874716774.R69 Big Decal Mud 1_Data | 4 | SKIP: no dirt/mud decal without EAI: converted to Dirty Surfaces Pack area surfaces (import-surfaces-decals.json) |
| 874716774.R69 Big Decal Mud 3_Data | 4 | SKIP: no dirt/mud decal without EAI: converted to Dirty Surfaces Pack area surfaces (import-surfaces-decals.json) |
| 1988876538.Brown Modular C Formation 01_Data | 3 | CliffRock3 |
| 2926036331.R2USA W TrWo 2x2 TRH Girder 02_Data_bridgePillar | 3 | SKIP: CS2 places pillars for its own bridges |
| Wooden Footbridge Pillar 12 | 3 | SKIP: CS2 places pillars for its own bridges |
| Dirt Pile 02 | 3 | RubblePileMixedRandom01 |
| Dirt Pile 01 | 3 | RubblePileMixedRandom01 |
| 874716774.R69 Big Decal Mud 2_Data | 3 | SKIP: no dirt/mud decal without EAI: converted to Dirty Surfaces Pack area surfaces (import-surfaces-decals.json) |
| Castle Ruins 02 | 2 | RuinsCastle01 |
| 1988876538.Brown Modular C Formation 03_Data | 2 | CliffRock3 |
| Pier Ruins 01 | 2 | StandingStone01 |
| 2931939156.Detroit Bridge_Data | 2 | MANUAL: CS1 bridge model placed as a building; in CS2 build TrussArchBridge01 / ExtradosedBridge01 along the road instead |
| Stop Sign | 2 | NA_StopSign01 |
| Bench 01 | 2 | GardenBench01 |
| Wooden Footbridge Pillar 6 | 1 | SKIP: CS2 places pillars for its own bridges |
| 1310328404.Hernando-Desoto Bridge_Data | 1 | MANUAL: CS1 bridge model placed as a building; in CS2 build TrussArchBridge01 / ExtradosedBridge01 along the road instead |
| GravelBridgePillar | 1 | SKIP: CS2 places pillars for its own bridges |
| 2926036331.R2USA W TrWo 1x2 TRH Girder 02_Data_bridgePillar | 1 | SKIP: CS2 places pillars for its own bridges |
| Castle Ruins 01 | 1 | RuinsCastle01 |
| Abandoned Factory 01 | 1 | RuinsOldMill01 |
| 2916095473.SEC Extra Large Substation_Data | 1 | TransformerStation01 |
| Ship Wreck 01 | 1 | RubblePileWood02 |
| StreetLamp02 | 1 | StreetlightSingle02 |
| Park Sign 02 | 1 | ParkSign02 |
| Park Sign 01 | 1 | ParkSign01 |
| Concrete Debris 02 | 1 | RubblePileConcrete01, RubblePileConcrete02 |
| Park Bench 01 | 1 | GardenBench01 |
| Concrete Debris 03 | 1 | RubblePileConcrete01, RubblePileConcrete02 |
| Park Trashbin 01 | 1 | SKIP: no matching bin name verified |
| 2074426936.Fallen Pine 01 TC_Data | 1 | Log01, Log02, Log03, Log04 |
| Dirt Pile 03 | 1 | RubblePileMixedRandom01 |
| Garbage Bin | 1 | SKIP: no matching bin name verified |

- Outside connections: 25 objects (road 16, rail 4, air 4, ship 1); border nodes 20, 20 with a CS1 connection (connections-check.md).
- Procedural Objects: 604 listed in manual-objects.md; 2 map through object-mapping.tsv into import-po-candidates.json (UNVERIFIED: scale is not applied, PO meshes are not CS2 assets).
- Water surface: 83,194 wet cells (waterSurface in the dump); source levels refined in import-water.json (water-check.md).
- Terrain surfaces: 85,890 cells, 1 classes (surfaces-summary.md, surfaces.geojson).
- Barrier props (import-walls-props.json): 621 ConcreteBarrier01 / SafetyBarrier01 pieces every 3 m along 62 CS1 barrier segments (piece length unverified).
- Field surfaces (import-surfaces-fields.json): 55 polygons, 3.153 km², max 48 points; footprints also raise fertility to ≥ 220.
- Grass surfaces from 3076 Landscape Grass props (import-surfaces-grass.json, optional): 51 polygons, 0.026 km², max 130 points.
- Rock surfaces (import-surfaces-rock.json): DakotaFalls-1to1-4096.png (|err| vs CS1 nodes 0.01 m), steep area 0.63 km² in 145 patches >= 400 m²; 151 polygons kept (0.60 km²), 0 smallest dropped.
- Round 8 substation: 1 (b39573 TransformerStation01 at (1594, 4549), rotation 318.04, no y) -> import-substation.json.
- Round 8 dam (network, bridge 0.9.23): dam edge 304 m heading 79.2° (CS1 81.2°), crest 165.6 m × 2; west abutment (1127, 4639) terrain 165.2 m (elevation 0.4), east (1425, 4696) terrain 165.6 m (elevation -0.0); middle terrain 153.6 m (12.0 below the crest); 14/15 samples over CS1 water; flow invert; profile dam-profile.md -> import-dam.json.
- Round 8 power ends: substation: CS1 end n12755 (1633, 4526), replaces e5094_0, new course n26177 -> (1600.5, 4544.0), 85 m; dam: CS1 end n16152 (1651, 4542), replaces e32855_0, new course n2881 -> (1555.8, 4685.1), 222 m -> import-power-ends.json.
- Round 8 PO rocks v5 (boulders, and cliff pieces on ≥ 35° slopes one per 16 m cell; left out: cliff class dropped 0, net corridor 42, water 72, cliff under 35° 57, cliff, cell taken 9, cliff pieces kept 365): 607 objects (CliffStack 50, CliffSideTall 49, CliffRock6 47, CliffSide2 45, CliffSide2Right 45, CliffRock5 42, CliffSlim 42, CliffSide2Left 39, rzrock7 26, rzrock8 26, rzrock6 25, rzrock1 21, rzrock5 20, rzrock3 18, rzrock2 17, Boulder08 16, rzrock4 15, Boulder03 13, Boulder01 11, Boulder05 11, Boulder04 9, Boulder02 8, Boulder07 8, Boulder06 4; at the rock shed 7 in the shed box (x −6520…−6180, z 620…1130): 1 re-seated (yaw along the contour, mesh bottom 0.75 m under the lowest footprint terrain), 6 left out (footprint on a roof slab or the road)) from 399 PO pieces, 0 over the cap of 1500 dropped, 64 PO objects > 60 m above their surroundings with CliffSide pieces down the steepest flank, 83 re-picked for the 80 m prefab/heading rule; with y 613; sink depth (mesh bottom below terrain): < 1 m 25, 1–5 m 223, 10–20 m 13, 20–50 m 110, 50–100 m 72, > 100 m 170 -> import-po-rocks.json (import name po-rocks-v3).
- Round 8 PO cliffs v1 (not written; their positions feed the rock scatter): 594 objects from 399 PO pieces (mountain 239, rock work 92, falls steps 68); bands > 160 m flank 267, > 160 m summit 111, 36–80 m 120, 80–160 m 75, 12–36 m 20, 3–12 m 1; 0 over the cap of 2500 dropped -> import-po-rocks.json.
- Round 8 rock scatter: in water left out 75, slopes > 45° left out 1467, slopes 25–45° (lowered by the bridge's slopeSink) 5166; at the rock shed 124 in the shed box (x −6520…−6180, z 620…1130): 118 re-seated (yaw along the contour, mesh bottom 0.75 m under the lowest footprint terrain), 6 left out (footprint on a roof slab or the road); 17551 (small 12249, medium 4450, boulders 852) around 3099 rock/cliff objects (r 60 m) and in 151 rock polygons; rejected road 296, spacing 935, water 3970, border 35 -> import-rock-scatter.json.
- Round 9 PO rock surfaces: 63 polygons (4.79 km² of 4.79 km² footprint, 48 patches, 0 dropped), gaga TTP - Pebbles 2L -> import-surfaces-po-rock.json.
- Round 9 fort: 112 objects along the 12 CS1 Castle Wall segments (RuinsStoneFormation01 34, RuinsStoneFormation02 29, RuinsStoneCairns01 11, RuinsStoneCairns03 10, RuinsStoneCairns02 8, StandingStone05 4, StandingStone06 3, StandingStone07 3, StandingStone08 3, StandingStone01 2, StandingStone04 2, OldCastle01 1, StandingStone02 1, StandingStone03 1), keep OldCastle01 at (5, -2619) (UNVERIFIED placeable) -> import-fort.json; RuinsCastle01 stays in import-objects.json.
- Round 9 PO walls left out (dropped by the game inside a highway in the run reports): pb646_side23.
- Round 9 PO paved blocks: 11 blocks (group roots 27, 288 left out: BR006 and BR005 bridge parts, see PO structures) -> 11 pavement polygons (import-po-structures.json); PO walls dropped (owner review of A-v4).
- Round 9 rock shed open gallery, CS1 shell extent 492 m of road from (-6235.4, 655.3) to (-6462.7, 1077.6) (A-v14: 445 m from (-6249.8, 673.9) to (-6457.6, 1066.5)) (e10437_0, e13009_0, e20651_0, e24116_0, e31333_0, e32978_0, e34976_0): 96 BasePlateConcrete16 in 32 rows every 15.35 m, one layer, on one smooth profile (rise -1.11…5.32° toward +z, file pitch = −rise; outside the steep stretches |rise| max 1.11° and change between neighbouring rows max 0.99°, overall change max 3.56°; the profile follows the road where the road itself is steeper than 3° over 48 m: row 1 road 18.8° at (-6240.5, 661.5), row 2 road 17.3° at (-6249.7, 673.7), row 3 road 15.2° at (-6258.7, 686.2), row 4 road 13.5° at (-6267.3, 698.9), row 5 road 12.0° at (-6275.8, 711.7), row 6 road 10.4° at (-6284.1, 724.6), row 7 road 7.9° at (-6292.2, 737.6), row 8 road 4.7° at (-6300.1, 750.8), row 15 road 3.1° at (-6351.9, 844.9), row 16 road 5.0° at (-6359.0, 858.6), row 17 road 7.9° at (-6365.9, 872.3), row 18 road 10.8° at (-6372.7, 886.0), row 19 road 12.5° at (-6379.5, 899.8), row 20 road 10.9° at (-6386.2, 913.6), row 21 road 5.2° at (-6392.9, 927.5), row 22 road 3.9° at (-6399.5, 941.3), row 23 road 12.8° at (-6406.1, 955.2), row 24 road 18.1° at (-6412.6, 969.1), row 25 road 18.6° at (-6419.1, 983.0), row 26 road 14.9° at (-6425.6, 996.9), row 27 road 8.7° at (-6432.1, 1010.8), row 30 road 3.2° at (-6451.3, 1052.7)), underside >= the highest road point under each row + 6.6 m (clearance over the road per row: level min 6.60 max 25.87 m, pitched min 6.60 max 26.34 m), into the cut face: row depth from the column line min 40.5 m, max 44.5 m (0 rows stopped at the cap, axis + 40 m = 52.9 m); column line → terrain at the roof top per row: min 38.5 m, max 42.5 m (widest at row 3), the cap at 52.9 m; hill-side wall only at gaps: 0 Wall32x16 + 0 Wall32x32 on the 40 m line; 82 valley-side columns every 6 m (174 Pillar1x8, bases 0.5 m in the ground; 24 single pieces deeper, up to 1.3 m, where the underside is under 7.5 m above the terrain); bare roof; no portals; check: roof covers 99.9 % of the road strip (13 of 12792 1 m samples open), largest step between roof rows 1.45 m, 0 of 492 1 m samples along the hill edge open (terrain below the roof, no wall) -> import-rock-shed.json.
- Round 9 rock shed clear strip: 236 points -> clear-rock-shed.json (tree mask uses the same ring).
- Round 9 PO structures: BR006 variant b: 7 StoneArchRailBridge at 28 m pitch along yaw 324 from (-3528.5, -3175.8), rotation 234.
- Round 9 PO structures: main (BR006 variant a, no kit objects at BR006): 3 objects (BasePlateConcrete32 3), 3 grounded.
- Round 9 PO structures: BR006 variant b: 7 StoneArchRailBridge; gantry 1 (grounded).
- Round 9 PO structures: piers: 50 caps + 151 Pillar4x16 stack pieces (cap to terrain up to 89.2 m), 0 caps without a course within 6 m.
- Round 9 PO structures: not measured, pivot assumed at the base: BasePlateConcrete32, HWY Truss Gantry 2L  A.
- Round 9 PO structures: 185 proposal rows -> import-po-structure-objects.json 3, import-po-structure-piers.json 201, optional/import-po-structure-br006-arches.json 7, optional/import-po-structure-gantry.json 1.
- Round 9 lake lake7: centre terrain 138.23 m vs level 134.94 m, moved to (6272, 4032) terrain 134.26 m.
- Round 9 lake lake11: centre terrain 135.55 m vs level 135.84 m, moved to (6944, 2640) terrain 134.79 m.
- Round 9 lake lake16: centre terrain 138.44 m vs level 138.91 m, moved to (640, 1008) terrain 136.86 m.
- Round 9 lakes: 26 wet bodies above sea level 97.5 + 1 m with no CS1 source within 400 m; 18 sources (outside the map 4, single cells 4) -> import-water-lakes.json (append after import-water*.json).
- Round 9 gaps: 4 edges -> import-gaps.json, written before the network (e15883_0: 3-lane course 68 m (alternative: 4-lane taper 12 m + 3-lane, import-gaps-e15883-taper.json); gravel gap n895–n26596: 80 m; e25579_0: rebuilt, junction end snaps radius 6; e36563_0: rebuilt, junction end snaps radius 6; path stub n7811: 9.3 m, inside the road footprint; left to its snapToSegment in import-all.json; 3 originals left out of import-all.json (e15883_0, e25579_0, e36563_0)).
- Decal surfaces (import-surfaces-decals.json; PDO23 Dirty Surface 01 / PDO23 Soil Surface 01): 41 polygons, 0.022 km², max 298 points.
- Sand: the 16 m-cell surfaces are dropped (owner review of A-v4); optional import-surfaces-sand-smooth.json: 387 blobs from 610 of 905 patches >= 2000 m² (21.54 km²).
- Terrain surfaces (import-surfaces.json): 479 polygons, 16.935 km², max 323 points: Sand → PDO23 Dirty Sand Surface 01 (479 polygons).
- Terrain paint import-terrain-paint-sand.json: material Sand, CS1 painted sand (all Sand cells): 321 polygons, 0 strokes, 9.94 km², softEdge 20 m.
- Terrain paint optional\import-terrain-paint-rock.json: material (none: rock is automatic on steep slopes), slope ≥ 40° and the PO rock / cliff footprints: 214 polygons, 0 strokes, 5.39 km², softEdge 0 m, not imported.
- Terrain paint import-terrain-paint-dry-grass.json: material Dry, dry open ground: east / south-east plains (22502 cells) and south-facing slopes (20825 cells): 426 polygons, 0 strokes, 9.19 km², softEdge 20 m.
- Terrain paint import-terrain-paint-bare-earth.json: material Dirt, CS1 dirt / mud decals: 0 polygons, 128 strokes, 0.05 km², softEdge 0 m.
- Terrain paint import-terrain-paint-n1-riparian-lush.json: Lush 0.6, 73 polygons, 1.17 km², 7834 cells (N1 cottonwood / willow bottoms).
- Terrain paint import-terrain-paint-n12-fertile-dry.json: Dry 0.35, 276 polygons, 15.15 km², 59080 cells (N12 CS1 fertile land (yellower)).
- Terrain paint import-terrain-paint-n13-high-ground-dry.json: Dry 0.5, 97 polygons, 11.42 km², 45868 cells (N13 bare high ground above the treeline 588 m).
- Terrain paint import-terrain-paint-n6-savanna-dry.json: Dry 0.4, 1334 polygons, 41.37 km², 133390 cells (N6 ponderosa savanna floor).
- Terrain paint import-terrain-paint-n6-savanna-dirt.json: Dirt 0.45, 40027 strokes (N6 bare soil dabs between the pines).
- Terrain paint import-terrain-paint-n4-hogback-sand.json: Sand 0.5, 31644 strokes (N4 hogback sandstone streaks along the contour (30066 cells)).
- Terrain paint import-terrain-paint-n5-scree-dirt.json: Dirt 0.8, 80 polygons, 0.47 km², 2260 cells (N5 talus / scree aprons below cliffs).
- Terrain paint N3: reservoir level 157.5 m (2912 wet cells at ± 1.5 m).
- Terrain paint import-terrain-paint-n3-drawdown-dirt.json: Dirt 0.7, 134 strokes (N3 reservoir drawdown ring, 0–2.5 m above the level).
- Terrain paint import-terrain-paint-n2-pit-banks-dirt.json: Dirt 0.6, 13 strokes (N2 gravel-pit pond banks, 5 ponds).
- Terrain paint import-terrain-paint-n2-pit-banks-sand.json: Sand 0.6, 8 strokes (N2 gravel-pit pond shore).
- Terrain paint import-terrain-paint-n8-playa-rim-lush.json: Lush 0.5, 70 polygons, 0.58 km², 2802 cells (N8 playa rims (80 playas)).
- Terrain paint import-terrain-paint-n8-playa-centre-dirt.json: Dirt 0.6, 82 polygons, 0.92 km², 3736 cells (N8 playa centres (dried mud)).
- Terrain paint N7 eligibility: free 705502, flat plains 237808, no tree within 100 m 13395, fertility < 0.5 1849.
- Terrain paint import-terrain-paint-n7-prairie-dog-dry.json: Dry 0.8, 1 polygons, 0.05 km², 188 cells (N7 prairie dog towns (1)).
- Terrain paint import-terrain-paint-n7-prairie-dog-dirt.json: Dirt 0.5, 215 strokes (N7 prairie dog mounds (30 % cover)).
- Terrain paint import-terrain-paint-n11-ore-dirt.json: Dirt 0.5, 104 polygons, 8.74 km², 34188 cells (N11 CS1 ore ground).
- Terrain paint import-terrain-paint-n11-oil-dirt.json: Dirt 0.3, 302 polygons, 57.32 km², 226055 cells (N11 CS1 oil ground (well pads)).
- Terrain paint import-terrain-paint-n9-dirt-roads-dirt.json: Dirt 0.5, 12253 strokes (N9 dirt / gravel road and track edges).
- Terrain paint import-terrain-paint-n10-rail-ballast-dirt.json: Dirt 0.35, 4357 strokes (N10 rail ballast outside towns).
- Paint plan S1 bank sand: 120 cells, 9 polygons, 0.03 km².
- Paint plan S3 bare earth + S4 dirt decals: 13364 cells, 90 polygons, 2.56 km².
- Paint plan G1 fertile + G2 dry plains: 60018 cells, 151 polygons, 14.65 km².
- Paint plan S2 submerged sand (none): 23813 cells, 0 polygons, 0.00 km².
- Paint plan P1 stand-in pavement: 0 cells, 0 polygons, 0.00 km².
- Resources: import-resources.json from the dump's naturalResources.

## Skip review (rounds 10-12)

| class | CS1 source | count | mapped | kept skipped | decision |
|---|---|---:|---:|---:|---|
| Field networks | network segments | 269 | 210 as surfaces | 59 (outside ±7168) | → 55 Agriculture Surface 01 polygons (import-surfaces-fields.json) + fertility; not imported as networks |
| Landscape Grass | props | 3076 | 3076 | 0 | → 1:1 pack grass plants (Styx tufts, ferns, pd Long Grass, undergrowth, flowers; kind plant, age adult) in import-objects.json; 51 Grass Surface 01 clusters remain as the optional alternative |
| Rocks (Surface Rock / Modular C / Breakwater) | buildings | 4538 | 4538 | 0 | → pack rocks (Arid, RZ, Cliffs and Rocks) mixed with vanilla Rock01-30, per class in object-mapping.tsv |
| Decals (R69 dirt/mud) | props | 128 | 41 as surfaces | 87 (outside ±7168) | → 16 m squares of PDO23 Dirty Surface 01 (dirt) / PDO23 Soil Surface 01 (mud), import-surfaces-decals.json |
| Pedestrian Gravel / Elevated | network segments | 24 | 15 | 9 | → Pavement Path (roads layer, elevations kept); CS2 has no gravel path |
| Quay | network segments | 7 | 7 | 0 | → QuayMedium01 (walls layer, import-walls.json) |
| Canal | network segments | 14 | 14 | 0 | → Narrow Boatway (walls layer); CS2 boatways are canals |
| Flood Wall | network segments | 2 | 2 | 0 | → Sound Barrier (walls layer), the only wall network by name |
| Retaining Walls AiO | network segments | 90 | 90 | 0 | → QuaySmall01 (walls layer); no retaining-wall network, quay wall is the nearest (unverified inland) |
| Concrete / Steel barriers | network segments | 62 | 61 as props | 1 (outside ±7168) | → 621 ConcreteBarrier01 / SafetyBarrier01 props every 3 m (import-walls-props.json) |
| Castle Wall | network segments | 12 | 0 | 12 | NO TWIN, kept skipped: searched Assets.NAME for Wall/Castle/Rampart/Fort/Ruin (only garden walls EU_Wall*, RuinsCastle01 building, Sound Barrier) |
| Ship paths | network segments | 118 | 100 | 18 | → Medium Seaway (import-shippaths.json), clipped to end on the border |
| Airplane paths | network segments | 319 | 0 | 319 | kept skipped: the game routes planes |
| Ruins / wreck / factory | buildings | 7 | 7 | 0 | → LeisurePier01HeadSmall / RubblePileWood02 / RuinsOldMill01 / RuinsCastle01 (nearest by name) |
| Terraforming Network | network segments | 233 | 0 | 233 | kept skipped: CS1 terrain-shaping helper (terrain is in the heightmap) |
| Pillars | buildings | 112 | 0 | 112 | kept skipped: CS2 bridges spawn their own pillars |
| Modern Fence 02 | props | 22 | 22 | 0 | → FenceResidentialPieceLow01 (piece length unverified) |
| Network Skins (v5) | segments | 88 | 88 flagged | 0 | note: no pillars on 26, no street lights on 62; edge fields noPillars / noStreetLights (no bridge field yet), list in manual-objects.md |

## CS2 audit fixes (audit/cs2-side.md, before the run)

- Short border edges folded into their neighbour (< 10 m): 1.
- Gravel Road border ends: 9; kept as outside connections 3 (one per CS1 connection: c31152_0, c9707_0, c35664_0); pulled 1 m inside 6: c4947_0 (7168, 690), c1237_0 (7168, 3968), c17246_0 (7168, 1202), c19544_0 (7168, 3712), c26692_0 (7168, 530), c17018_0 (7168, 4064).
- Boatways (CS1 canals): 14; kept where water stands (CS1 water surface, or a source within 150 m above the ground) 14, dropped 0.
- Flat QuaySmall01 edges (cross-slope < 0.4 m at × 2): 8, wall both 1, right 4, left 3; their nodes raised to ≥ 4 m (one side) / ≥ 8 m (both) as the fallback.
- One-way highway stubs to CS1 dead ends left out: 0 courses.
- One-way courses pruned after the stub removal (they met head-on at a node with no exit or no entry): 0.
- Run reports read for game drops: run-20261019-165751.md, run-20261019-165831.md, run-20261019-215737.md, run-20261020-140800.md, run-20261020-140845.md, run-20261020-140910.md, run-20261020-192927.md, run-20261020-193334.md, run-20261020-193751.md, run-20261020-211001.md, run-20261020-231951.md, run-20261021-000201.md, run-20261021-025202.md, run-20261021-121517.md, run-20261021-122852.md, run-20261021-131042.md, run-20261021-134229.md, run-20261021-143023.md, run-20261021-144033.md, run-20261021-150720.md, run-20261021-151724.md, run-20261021-152938.md, run-20261021-153923.md, run-20261021-162625.md, run-20261021-163621.md, run-a7.md (36 dropped courses).
- e20430_0b clear of its parallel carriageway: e20430_0b Highway Oneway - 3 lanes beside e16466_0a Highway Oneway - 3 lanes (15.5 m; run report: move 0.9 m): moved 1.9 m
- Carriageway spread: pass 1: 99 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 227 nodes moved apart.
- Carriageway spread: pass 2: 14 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 49 nodes moved apart.
- Carriageway spread: pass 3: 5 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 18 nodes moved apart.
- Carriageway spread: pass 4: 4 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 14 nodes moved apart.
- Carriageway spread: pass 5: 4 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 14 nodes moved apart.
- Carriageway spread: pass 6: 2 parallel carriageway pairs closer than their two half-widths (+ 2 m for opposite carriageways), 8 nodes moved apart.
- Highway course dropped beside its node-sharing sibling e9919_0 (Highway Oneway - 1 lane): e22564_0b's free end e22564_0d0 moved 2.7 m away from it; the shared node n4291 stays.
- Highway course dropped by the game, no node-sharing 'Highway Oneway - 3 lanes' sibling found, not moved: e31224_0ba.
- Highway course dropped beside its node-sharing sibling e15395_0a (Highway Oneway - 2 lanes): e31224_0bb's free end e31224_0bd1 moved 2.2 m away from it; the shared node n27662 stays.
- Courses inside a highway footprint (walls, decor) moved sideways by the two CS2 half-widths + 1 m: 0; impossible (between carriageways) 0.
- Seaways stay at the riverbed (pass --seaway-at-water to write them at the water surface once the dig test shows digging).
- Pavement Path dead ends snapped onto the segment beside them (snapToSegment, ≤ 20 m, Δy ≤ 3 m): 3: n7811 (4783, -683) → e17505_0 Small Road, 9.3 m, Δy 0.0 m, radius 11; n21342 (737, 4853) → e36560_0 Gravel Road, 12.8 m, Δy 0.0 m, radius 15; n13342 (1577, 4694) → e21878_0 Gravel Road, 9.6 m, Δy 0.1 m, radius 12.
- noPillars dropped (no pier object set behind them; --keep-cs1-nopillars keeps them): 138 CS1 segments: 172, 779, 1387, 1638, 1677, 2029, 2653, 2804, 2886, 3139, 3389, 3500, 3598, 3638, 3944, 4284, 4813, 4838, 5188, 5703, 6031, 6041, 6338, 6801, 7008, 8145, 8322, 8562, 9026, 9203, 9467, 10176, 10238, 10276, 10815, 10906, 11467, 11507, 11661, 11735, 11895, 12548, 12655, 12729, 12774, 12792, 13146, 13697, 14182, 14225, 14397, 14527, 14574, 14892, 15108, 15344, 15414, 15768, 15986, 16165, 16410, 16506, 16555, 17085, 17314, 17372, 17669, 17800, 17815, 18179, 18529, 18755, 18923, 19546, 20401, 20556, 20616, 20675, 21043, 21099, 21338, 21512, 21897, 21906, 22005, 22300, 22886, 22934, 23013, 23795, 24934, 25034, 25769, 26105, 26134, 26240, 26285, 26635, 27061, 27293, 27395, 27612, 27684, 27870, 27973, 27996, 28071, 29014, 29685, 29775, 29898, 29970, 31228, 32165, 32650, 32706, 33197, 33311, 33321, 33371, 33711, 33741, 33803, 33845, 34021, 34083, 34456, 34845, 35437, 35648, 36148, 36355, 36595, 36659, 36686, 36781, 36784, 36861.

## Bridges (bridges.tsv)

25 bridge overrides; 54 edges in import-bridges.json, 5 in import-bridges-fixed.json (on). No prefab name is verified in game yet; --bridges off keeps every CS1 bridge as the plain mapped road. Rule checks: the bridge's 0.9.22 pre-checks computed offline against ../bridges/cs2-bridge-rules.json (bridge.lowElevation and vanilla prefabs are left to the runner's dry run). noPillars: BR006 deck, BR001 connectors and 4 BR001 deck segments outside the merged span.

| bridge | prefab | plan | chains | edges | lengths m | file | notes | rule checks (cs2-bridge-rules.json) |
|---|---|---|---:|---:|---|---|---|---|
| BR001 | BXP SuspensionBridge03 - Six Lane Highway | fixed:168,320,320,168;merge | 2 | 1 | 976 | fixed | Detroit: two 3-lane carriageways merged onto the 6-lane two-way suspension, towers 640 m apart (CS1 723 m); tower spacing 640 m vs CS1 724 m; carriageway n27934: 26 courses between e29898_0d0 and e11895_0d0 replaced; connectors 141 / 140 m; carriageway n14554: 29 courses between e33845_0d0 and e36148_0d0 replaced; connectors 140 / 141 m; clearance over e3593_0 Medium Seaway: 248.2 m (deck − course, output metres); deck over the water: min 116.3 m | fixed br001_deck: 976 m within 586–1366 m (nominal 976) |
| BR004 | (dropped) | drop | 0 | 0 |  | bridges | dam-crest walkway (Pedestrian Elevated, 799 m): dropped (owner review of A-v5: an unwanted footpath along the dam); 31 courses dropped, 798 m; footpaths that only led onto it: 2 (118 m) | ok |
| BR005 | BXP Truss Arch Bridge - Highway Oneway - 2 Lanes | count:2 | 2 | 4 | 4×164 | bridges | Hernando-Desoto: two ~150 m arches per carriageway ("M"); br005_c1: 2 approaches meet at n18884: only the through one is straightened; br005_c1: approach e3338_0 met the deck at 6°, its end now runs straight onto the deck; br005_c2: 2 approaches meet at n26229: only the through one is straightened | ok |
| BR009 | BXP Truss Arch Bridge - Highway Oneway - 2 Lanes | span:179.2 | 2 | 4 | 4×105 | bridges | green truss arch over the gorge; the owner asked for one edge per carriageway, but the deck chain is 210 m straight (> 1.4 × 128 = 179 m), so two; br009_c1: 2 approaches meet at n11662: only the through one is straightened | ok |
| BR006 | Truss Arch Bridge - Small Road - 2 Lanes | span:179.2 | 1 | 4 | 4×146 | bridges | red deck arch over the dry valley (round 25: game pillars; variant b's arch objects clear them through the runner's pillars-off step) (vanilla truss arch; BXP equivalent if this name fails) | ok |
| BR007 | TrussArchBridge01 | fixed:128,128;count:1 | 1 | 1 | 189 | bridges | Ourense: truss arch over the main span, ONE edge abutment to abutment (live rules: fixed 128 + 128 m, edge 153.6–358.4 m); approaches stay Double Train Track | fixed br007_c1_e0: 189 m within 154–358 m (nominal 256) |
| BR010 | TrussArchBridge01 | fixed:128,128;count:1 | 1 | 1 | 265 | bridges | Pedrido: truss arch over the bow-string part, ONE edge abutment to abutment (fixed 128 + 128 m, edge 153.6–358.4 m) | fixed br010_c1_e0: 265 m within 154–358 m (nominal 256) |
| BR023 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×48 | bridges | TRH through girder (96 m: 2 edges) | ok |
| BR026 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×48 | bridges | TRH through girder (96 m: 2 edges) | ok |
| BR033 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×48 | bridges | TRH through girder | ok |
| BR035 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×59 | bridges | TRH through girder | ok |
| BR038 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×48 | bridges | TRH through girder | ok |
| BR042 | BXP Truss Viaduct - Train | span:89.6 | 1 | 2 | 2×64 | bridges | TRH through girder | ok |
| BR011 | BXP Truss Viaduct - Train |  | 0 | 0 |  | bridges | R2USA B0 rail viaduct, one edge per span; no converted edges (outside the map or skipped) | ok |
| BR012 | BXP Truss Viaduct - Train | span:89.6 | 1 | 5 | 5×74 | bridges | R2USA B0 rail viaduct | ok |
| BR015 | BXP Truss Viaduct - Train | span:89.6 | 1 | 5 | 5×75 | bridges | R2USA B0 rail viaduct | ok |
| BR017 | BXP Truss Viaduct - Train | span:89.6 | 1 | 4 | 4×68 | bridges | R2USA B0 rail viaduct | ok |
| BR028 | BXP Truss Viaduct - Train | span:89.6 | 1 | 3 | 3×63 | bridges | R2USA B0 rail viaduct | ok |
| BR019 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 3 | 3×64 | bridges | gravel trestle | ok |
| BR020 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 2 | 2×84 | bridges | gravel trestle; br020_c1: CS1 chain curves up to 7 m off the straight line (171 m along, 168 m straight); br020_c1: deck 7.4 m over the water at (1258, 6142) (< 10 m, m_ElevationOnWater [U]) | bridge.waterClearance (warning) br020_c1_e0: 7.3 m < 10 m over the water; bridge.waterClearance (warning) br020_c1_e1: 7.6 m < 10 m over the water |
| BR044 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 1 | 78 | bridges | gravel trestle | ok |
| BR045 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 2 | 2×63 | bridges | gravel trestle; br045_c1: deck lifted +2.3 m to 8.5 m above the terrain; 1 approach nodes re-graded at ≤ 3 % (track 2 %) | ok |
| BR049 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 1 | 86 | bridges | gravel trestle; br049_c1: deck lifted +6.1 m to 8.5 m above the terrain; 9 approach nodes re-graded at ≤ 3 % (track 2 %) | ok |
| BR050 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 1 | 81 | bridges | gravel trestle; br050_c1: deck lifted +5.4 m to 8.5 m above the terrain; 13 approach nodes re-graded at ≤ 3 % (track 2 %) | ok |
| BR053 | Wooden Covered Bridge - 2 lanes | span:89.6 | 1 | 1 | 67 | bridges | gravel trestle; br053_c1: deck lifted +8.1 m to 8.5 m above the terrain; 13 approach nodes re-graded at ≤ 3 % (track 2 %) | ok |
- Detroit test: BR001 line itself: 43/123 samples of the river part over land; best NW offsets 175 m 40, 200 m 41, 150 m 41, 275 m 43, 375 m 45.
- Detroit test: option 2 at 200 m NW: 2 × 362 m BXP twoway 6-lane spans + 2 × 150 m stubs; 41/123 samples of the river part over land (no CS1 water), deck 75.8 m above the lowest terrain gap.
- Detroit test: option 3 at 400 m NW: upper 976 m + lower 976 m (8.0 m lower, guess) + 4 × 150 m stubs; 46/123 samples of the river part over land (no CS1 water), deck 75.9 m above the lowest terrain gap.
- Gaps: 4 edges in import-gaps.json; originals left out of import-all.json: e15883_0, e25579_0, e36563_0.
- Road QA fixes: pipeline dead ends snapped 0, stubs < 40 m left out 0, residual 0; isolated pieces < 100 m left out 0, kept (isolated in CS1 too) 1.
  - isolated, kept: (367, -3024) Pavement Path, 83 m (e7126_0, e28940_0, e18960_0): isolated in CS1 too
- Water sources: 65 → 41 (12 changes, water-check.md).
- EXPERIMENT trees: the 726 net-corridor trees are in experiment-trees-net-corridor-01.json (import after the main tree files to try them; undo by its name).
- Island check (4096, -6127), 300 m: 23 trees kept; masked CS2 terrain below sea level 93 m + 0.5 38, inside the CS1 water surface and below 93.5 m 0, on a ground-level net corridor (half width + 2 m) 0, the rock shed's open valley side (20 m strip, 10 m past each end) 0.
- Trees masked (same set for both scales): CS2 terrain below sea level 93 m + 0.5 2,472; inside the CS1 water surface and below 93.5 m 0; on a ground-level net corridor (half width + 2 m) 726; the rock shed's open valley side (20 m strip, 10 m past each end) 93; total 3,291, 504,944 remain.
- Field circles: 7 overlapping pairs shrunk to touch (0.5 m gap) instead of a union, so they stay circles.
- Fields: 55 circles (closed CS1 rings, and open arcs fitted by least squares, RMS < 6 m), 0 swept arcs left (smoothed).
- Grass surfaces: 2 overlapping pairs merged in 2 groups (53 → 51).
- Decal surfaces: 227 overlapping pairs merged in 11 groups (128 → 41).
