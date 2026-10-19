# net-converter stats

Dump `fixtures/v3/map-v3.json`, mapping `mapping.tsv`.

## Output layers

| layer | edges | nodes | border nodes | length km | ground | elevated | tunnel |
|---|---:|---:|---:|---:|---:|---:|---:|
| highways | 0 | 0 | 0 | 0.0 | 0 | 0 | 0 |
| rail | 0 | 0 | 0 | 0.0 | 0 | 0 | 0 |
| roads | 2 | 3 | 0 | 0.4 | 2 | 0 | 0 |

Only highways, rail and roads are written as import files. power, skip and unmapped appear in network.geojson only.

## Transform

- segments in 2; wholly outside ±7168 m 0; clipped at the border 0 (→ 0 pieces)
- split for > 1500 m: 0; node merges (< 0.5 m): 0; short edges (< 4 m) collapsed into one node: 0; degenerate/duplicate edges dropped: 0

## CS2 prefabs used

| layer | CS2 prefab | edges | length km | from CS1 prefabs |
|---|---|---:|---:|---|
| roads | Gravel Road | 2 | 0.4 | Gravel Road |

## Unmapped CS1 prefabs (need a human decision)

None: every network prefab in the dump matched a rule.

## Mapping decisions flagged in mapping.tsv


## Full prefab mapping

| CS1 prefab | segments | → CS2 | layer | rule line |
|---|---:|---|---|---:|
| Gravel Road | 2 | Gravel Road | roads | 44 |

## Inventory (CS1 dump)

Nodes 3, segments 2, net prefabs 1, water sources 2, outside connections 1.

### Segments per prefab

| CS1 prefab | AI | segments | length km |
|---|---|---:|---:|
| Gravel Road | RoadAI | 2 | 0.41 |

### Node elevation

CS1 `elevation` byte and measured y − terrain:

| bucket | elevation byte | y − terrain |
|---|---:|---:|
| < -20 | 0 | 0 |
| -20..-5 | 0 | 0 |
| -5..-0.5 | 0 | 0 |
| ±0.5 | 3 | 3 |
| 0.5..5 | 0 | 0 |
| 5..10 | 0 | 0 |
| 10..20 | 0 | 0 |
| 20..40 | 0 | 0 |
| > 40 | 0 | 0 |

Underground nodes 0; nodes with Outside flag 0; nodes beyond ±7168 m 0.

### Water sources

- Natural: 2 (inside ±7168 m: 2; height 195.0–400.0 m)

### Outside connections (CS1 buildings)

- road: 1 (in 1, out 1)

## Converter fixes (round 2)

- Hairpins (Bézier length > 1.6 × chord, self-intersection, or a control point behind its end), checked on every imported CS1 segment: **0** found, 0 rebuilt.
- Exact duplicate edges dropped: **0**.
- Structure now from node elevation (not the B/E/S/T prefab suffix): 0 imported edges differ from the suffix-based class ().
- Elevation ramps (Δelevation > 6 m over < 60 m) smoothed to ≤ 8 %: **0** ramps, 0 nodes raised/lowered, 0 still steeper than 12 % (blocked by a junction or the border).

- Node merges refused because the nodes are stacked (height difference ≥ 2 m): **0**.

## Near-parallel pairs resolved (round 3)


| layer | node | edge 1 | edge 2 | angle before ° | after ° | category |
|---|---|---|---|---:|---:|---|

## Acute junctions remaining (< 10°)

Two edges of one layer leaving the same node almost parallel. CS2 builds a long junction there, which looks like a ramp folding back into the highway.


| layer | node | x | z | edge 1 | edge 2 | angle ° |
|---|---|---:|---:|---|---|---:|

## Overlaps (same layer, < 4 m apart for > 20 m)


| layer | edge 1 | edge 2 | close m | shared node | x | z | CS1 prefabs |
|---|---|---|---:|---|---:|---:|---|


## Other layers

- Power: 0 courses of High-voltage Line in import-power.json (nodes on the ground).
- Trees: 3 objects in 1 chunks under out\fixture-v3\trees (points (map-v3-trees.json)); outside ±7168 m 1; dropped 1 (see trees/README.md).
- Objects: 2 of 2 CS1 buildings/props got a CS2 target (100.0 %); SKIP 0, MANUAL 0, unmapped 0, outside ±7168 m 0; 2 carry y.

| CS1 prefab | count | decision |
|---|---:|---|
| 1978360460.Brown Surface Rock A Small_Data | 1 | Rock01, Rock02, Rock03, Rock04, Rock05, Rock06, Rock07, Rock08, Rock09, Rock10 |
| Park Streetlight 01 | 1 | StreetlightSingle01 |

- Outside connections: 0 objects (); border nodes 0, 0 with a CS1 connection (connections-check.md).
- Procedural Objects: 2 listed in manual-objects.md; 1 map through object-mapping.tsv into import-po-candidates.json (UNVERIFIED: scale is not applied, PO meshes are not CS2 assets).
- Water surface: 1,274 wet cells (waterSurface in the dump); 1 source heights refined in import-water.json (water-check.md).
- Terrain surfaces: 25 cells, 2 classes (surfaces-summary.md, surfaces.geojson).
- Resources: import-resources.json from the dump's naturalResources.
