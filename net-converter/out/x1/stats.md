# net-converter stats

Dump `C:\Users\dleva\AppData\Local/DakotaExport/2972879554_Dakota_Falls-20261019-063123.json`, mapping `mapping.tsv`.

## Output layers

| layer | edges | nodes | border nodes | length km | ground | elevated | tunnel |
|---|---:|---:|---:|---:|---:|---:|---:|
| highways | 1870 | 1845 | 10 | 121.7 | 1026 | 804 | 40 |
| rail | 496 | 496 | 4 | 36.0 | 297 | 176 | 23 |
| roads | 1706 | 1585 | 6 | 126.8 | 1592 | 114 | 0 |
| power | 60 | 62 | 1 | 12.8 | 60 | 0 | 0 |
| skip | 722 | 797 | 12 | 63.8 | 722 | 0 | 0 |

Only highways, rail and roads are written as import files. power, skip and unmapped appear in network.geojson only.

## Transform

- segments in 5532; wholly outside ±7168 m 766; clipped at the border 40 (→ 40 pieces)
- split for > 1500 m: 0; node merges (< 0.5 m): 118; short edges (< 4 m) collapsed into one node: 95; degenerate/duplicate edges dropped: 105

## CS2 prefabs used

| layer | CS2 prefab | edges | length km | from CS1 prefabs |
|---|---|---:|---:|---|
| highways | Highway Oneway - 1 lane | 283 | 15.9 | 2452038713.USRP 1L Interstate Ramp_Data, 2452038713.USRP 1L Interstate Ramp E |
| highways | Highway Oneway - 2 lanes | 956 | 60.7 | 2452038713.USRP 2L Interstate E, 2452038713.USRP 2L Interstate_Data, 2452038713.USRP 2L Interstate B, 2452038713.USRP 2L Interstate T, 2452038713.USRP 2L Interstate S |
| highways | Highway Oneway - 3 lanes | 599 | 43.2 | 2452038713.USRP 3L Interstate_Data, 2452038713.USRP 3L Interstate B, 2452038713.USRP 3L Interstate E, 2452038713.USRP 3L Interstate S |
| highways | Highway Oneway - 4 lanes | 32 | 1.8 | 2452038713.USRP 4L Interstate_Data, 2452038713.USRP 4L Interstate E |
| rail | Double Train Track | 496 | 36.0 | 2610252544.R2USA W GrWo 2x2_Data, 2932555116.R2EUR W GrCo 2x2 Pedrido B, 2610252544.R2USA W GrWo 2x2 T0, 2932555116.R2EUR W GrCo 2x2 Pedrido E, 2610252544.R2USA W GrWo 2x2 B0, 2926036331.R2USA W TrWo 2x2 TRH Girder 02_Data, 2610252544.R2USA W GrWo 2x2 S0, 2836620152.R2EUR W GrCo 2x2 Ourense SA B, 2836620152.R2EUR W GrCo 2x2 Ourense SA E, 2610252544.R2USA W GrWo 2x2 E0 |
| roads | Gravel Road | 1291 | 98.1 | Gravel Road, Gravel Road Elevated, Gravel Road Bridge |
| roads | Medium Road | 42 | 2.5 | 2097539846.USRP 4-Lane National Road_Data, 2098643957.USRP 3-Lane National Road_Data, 2098643957.USRP 3-Lane National Road E |
| roads | Pavement Path | 48 | 1.5 | Pedestrian Gravel, Pedestrian Elevated |
| roads | Small Road | 325 | 24.7 | 2097530472.USRP 2-Lane National Road_Data, 2097530472.USRP 2-Lane National Road B |

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
- Structure now from node elevation (not the B/E/S/T prefab suffix): 383 imported edges differ from the suffix-based class (Ground→Elevated 364, Ground→Tunnel 13, Elevated→Ground 6).
- Elevation ramps (Δelevation > 6 m over < 60 m) smoothed to ≤ 8 %: **44** ramps, 266 nodes raised/lowered, 5 still steeper than 12 % (blocked by a junction or the border).

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
| highways | n6721 | e19786_0a | e29495_0 | 8.6 | 12.0 | diverge (tangents spread) |
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

