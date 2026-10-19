# Trees for POST /import/objects

Source: rasters (trees.total 1081 grid + trees.byPrefab 270 grid). 492,993 trees in 10 files of ≤ 50,000 (import-trees-01.json, import-trees-02.json, import-trees-03.json, import-trees-04.json, import-trees-05.json, import-trees-06.json, import-trees-07.json, import-trees-08.json, import-trees-09.json, import-trees-10.json).
No y: the game samples terrain, so the same files serve the 2× (out/) and the 1× (out/x1/) map.

- Outside ±7168 m (dropped): 158,906
- Points where ≈3 m spacing could not be kept (dense cells, placed anyway): 54,424
- Points in 64 m cells without a species list (got the global mix): 1,054
- Crop cells (Corn Stalk, for fertility): 235 cells of 64 m
- Masked, terrain below sea level: 10,194
- Masked, inside the CS1 water surface: 0
- Masked, on a ground-level net corridor (half width + 2 m): 5,271

## Per CS2 prefab

| CS2 prefab | trees |
|---|---:|
| PineTree01 | 198,887 |
| SpruceTree01 | 89,838 |
| EuropeanSpruce01 | 75,213 |
| BirchTree01 | 48,637 |
| OakTree01 | 29,983 |
| GreenBushWild01 | 18,227 |
| GreenBushWild02 | 11,281 |
| NA_HickoryTree01 | 9,806 |
| NA_LindenTree01 | 6,577 |
| FlowerBushWild02 | 2,288 |
| FlowerBushWild01 | 2,256 |

## Per CS1 species (decision from tree-mapping.tsv)

| CS1 prefab | CS1 trees | dropped (SKIP / CROP / outside mapping) | → CS2 |
|---|---:|---:|---|
| mp9-BorealPine | 94,511 | 0 | SpruceTree01 0.6, EuropeanSpruce01 0.4 |
| PD - Pine short_Data | 84,596 | 0 | PineTree01 1 |
| PD - Pine Tall_Data | 70,329 | 0 | PineTree01 1 |
| Corn Stalk _Data | 69,884 | 69,884 | CROP |
| PD - Pine Regular _Data | 65,579 | 0 | PineTree01 1 |
| PD Large EverGreen 2_Data | 43,514 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| PD Large EverGreen 3_Data | 43,315 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| PD Large EverGreen_Data | 42,940 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| mp9-LiveOaknvl | 42,213 | 0 | OakTree01 1 |
| PD - pencil pine_Data | 42,049 | 0 | PineTree01 1 |
| Bare Tree E_Data | 28,476 | 0 | BirchTree01 1 |
| Bare Tree D_Data | 28,352 | 0 | BirchTree01 1 |
| Semi Bare Tree Red_Data | 14,127 | 0 | NA_HickoryTree01 1 |
| Filler Bush small 1_Data | 14,107 | 0 | GreenBushWild01 0.6, GreenBushWild02 0.4 |
| Semi Bare Tree Gold_Data | 13,361 | 0 | BirchTree01 1 |
| Filler Bush Medium 1_Data | 10,218 | 0 | GreenBushWild01 0.6, GreenBushWild02 0.4 |
| Semi Bare Tree Green_Data | 9,474 | 0 | NA_LindenTree01 1 |
| pd Aussie Bushes 1_Data | 8,342 | 0 | GreenBushWild01 0.5, FlowerBushWild01 0.25, FlowerBushWild02 0.25 |
| pd Aussie Bushes 2_Data | 2,521 | 0 | GreenBushWild01 0.5, FlowerBushWild01 0.25, FlowerBushWild02 0.25 |
| Deadwood 01_Data | 2,103 | 2,103 | SKIP |
| Detail Shrub Cluster 03_Data | 1,268 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub 06 Cluster_Data | 1,261 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub 05 Cluster_Data | 1,243 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Filler Bush Large 1_Data | 1,237 | 0 | GreenBushWild02 1 |
| Detail Shrub Cluster 01_Data | 1,230 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub Cluster 02_Data | 1,225 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub Cluster 04_Data | 1,146 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| pd Filler Grass 2_Data | 800 | 800 | SKIP |
| Detail Shrub 06_Data | 585 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| pd Filler Grass_Data | 543 | 543 | SKIP |
| RiverReeds Detail Edition cluste_Data | 273 | 273 | SKIP |
| mp9-WildHedgevnl | 145 | 0 | GreenBushWild02 1 |
