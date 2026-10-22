# Trees for POST /import/objects

Source: points (2972879554_Dakota_Falls-20261019-063123-trees.json, trees/v2: y, flags, scale). 504,909 trees in 11 files of ≤ 50,000 (import-trees-01.json, import-trees-02.json, import-trees-03.json, import-trees-04.json, import-trees-05.json, import-trees-06.json, import-trees-07.json, import-trees-08.json, import-trees-09.json, import-trees-10.json, import-trees-11.json).
No y: the game samples terrain, so the same files serve the 2× (out/) and the 1× (out/x1/) map.

- Outside ±7168 m (dropped): 158,877
- Points where ≈3 m spacing could not be kept (dense cells, placed anyway): 0
- Points in 64 m cells without a species list (got the global mix): 0
- Crop cells (Corn Stalk, for fertility): 138 cells of 64 m
- FixedHeight trees (on rocks or bridges, with y): 74 in the point file (before clipping and masking), written per scale as import-trees-fixed-NN.json in out/ and out/x1/
- Tree Control scale → age (trees with scale ≠ 1; the rest use the mapping age profile): elderly 70,104 (scale < 0.6 seedling, < 0.85 sapling, ≤ 1.2 adult, else elderly)
- Masked, CS2 terrain below sea level 93 m + 0.5: 2,472
- Masked, inside the CS1 water surface and below 93.5 m: 0
- Masked, on a ground-level net corridor (half width + 2 m): 726
- Masked, the rock shed's open valley side (20 m strip, 10 m past each end): 78

## Per CS2 prefab

| CS2 prefab | trees |
|---|---:|
| PineTree01 | 200,207 |
| SpruceTree01 | 91,130 |
| EuropeanSpruce01 | 76,358 |
| OakTree01 | 30,660 |
| GreenBushWild01 | 21,675 |
| NA_LindenTree01 | 16,762 |
| NA_HickoryTree01 | 16,703 |
| GreenBushWild02 | 13,430 |
| NA_LondonPlaneTree01 | 10,112 |
| EU_AlderTree01 | 10,079 |
| EU_ChestnutTree01 | 9,918 |
| EU_PoplarTree01 | 2,845 |
| FlowerBushWild01 | 2,576 |
| FlowerBushWild02 | 2,504 |

## Per CS1 species (decision from tree-mapping.tsv)

| CS1 prefab | CS1 trees | dropped (SKIP / CROP / outside mapping) | → CS2 |
|---|---:|---:|---|
| mp9-BorealPine | 94,358 | 0 | SpruceTree01 0.6, EuropeanSpruce01 0.4 |
| PD - Pine short_Data | 84,669 | 0 | PineTree01 1 |
| PD - Pine Tall_Data | 70,374 | 0 | PineTree01 1 |
| Corn Stalk _Data | 70,104 | 70,104 | CROP |
| PD - Pine Regular _Data | 65,521 | 0 | PineTree01 1 |
| PD Large EverGreen 2_Data | 43,527 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| PD Large EverGreen 3_Data | 43,249 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| PD Large EverGreen_Data | 43,052 | 0 | EuropeanSpruce01 0.5, SpruceTree01 0.5 |
| mp9-LiveOaknvl | 42,097 | 0 | OakTree01 1 |
| PD - pencil pine_Data | 41,944 | 0 | PineTree01 1 |
| Bare Tree E_Data | 28,301 | 0 | EU_ChestnutTree01 0.5, NA_LindenTree01 0.5 |
| Bare Tree D_Data | 28,300 | 0 | EU_AlderTree01 0.5, NA_LondonPlaneTree01 0.5 |
| Filler Bush small 1_Data | 14,195 | 0 | GreenBushWild01 0.6, GreenBushWild02 0.4 |
| Semi Bare Tree Red_Data | 14,108 | 0 | NA_HickoryTree01 1 |
| Semi Bare Tree Gold_Data | 13,389 | 0 | NA_HickoryTree01 0.7, EU_PoplarTree01 0.3 |
| Filler Bush Medium 1_Data | 10,237 | 0 | GreenBushWild01 0.6, GreenBushWild02 0.4 |
| Semi Bare Tree Green_Data | 9,452 | 0 | NA_LindenTree01 1 |
| pd Aussie Bushes 1_Data | 8,403 | 0 | GreenBushWild01 0.5, FlowerBushWild01 0.25, FlowerBushWild02 0.25 |
| pd Aussie Bushes 2_Data | 2,541 | 0 | GreenBushWild01 0.5, FlowerBushWild01 0.25, FlowerBushWild02 0.25 |
| Deadwood 01_Data | 2,146 | 2,146 | SKIP |
| Detail Shrub Cluster 03_Data | 1,274 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub 06 Cluster_Data | 1,268 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub Cluster 01_Data | 1,254 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Filler Bush Large 1_Data | 1,246 | 0 | GreenBushWild02 1 |
| Detail Shrub 05 Cluster_Data | 1,238 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub Cluster 02_Data | 1,229 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| Detail Shrub Cluster 04_Data | 1,148 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| pd Filler Grass 2_Data | 814 | 814 | SKIP |
| Detail Shrub 06_Data | 589 | 0 | GreenBushWild01 0.5, GreenBushWild02 0.5 |
| pd Filler Grass_Data | 534 | 534 | SKIP |
| RiverReeds Detail Edition cluste_Data | 257 | 257 | SKIP |
| mp9-WildHedgevnl | 149 | 0 | GreenBushWild02 1 |

## Exact points vs rasters

Exact: 504,909 placed (outside 158,877). Raster-based: 508,458 placed (outside 158,906). Difference -3,549.

| CS2 prefab | exact | rasters |
|---|---:|---:|
| PineTree01 | 200,207 | 200,828 |
| SpruceTree01 | 91,130 | 91,460 |
| EuropeanSpruce01 | 76,358 | 76,910 |
| OakTree01 | 30,660 | 31,114 |
| GreenBushWild01 | 21,675 | 21,868 |
| NA_LindenTree01 | 16,762 | 16,835 |
| NA_HickoryTree01 | 16,703 | 16,708 |
| GreenBushWild02 | 13,430 | 13,962 |
| NA_LondonPlaneTree01 | 10,112 | 10,218 |
| EU_AlderTree01 | 10,079 | 10,242 |
| EU_ChestnutTree01 | 9,918 | 10,238 |
| EU_PoplarTree01 | 2,845 | 2,880 |
| FlowerBushWild01 | 2,576 | 2,587 |
| FlowerBushWild02 | 2,504 | 2,608 |
