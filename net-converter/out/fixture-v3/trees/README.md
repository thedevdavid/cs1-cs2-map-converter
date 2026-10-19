# Trees for POST /import/objects

Source: points (map-v3-trees.json). 3 trees in 1 files of ≤ 50,000 (import-trees-01.json).
No y: the game samples terrain, so the same files serve the 2× (out/) and the 1× (out/x1/) map.

- Outside ±7168 m (dropped): 1
- Points where ≈3 m spacing could not be kept (dense cells, placed anyway): 0
- Points in 64 m cells without a species list (got the global mix): 0
- Crop cells (Corn Stalk, for fertility): 1 cells of 64 m

## Per CS2 prefab

| CS2 prefab | trees |
|---|---:|
| OakTree01 | 3 |

## Per CS1 species (decision from tree-mapping.tsv)

| CS1 prefab | CS1 trees | dropped (SKIP / CROP / outside mapping) | → CS2 |
|---|---:|---:|---|
| mp9-LiveOaknvl | 4 | 0 | OakTree01 1 |
| Corn Stalk _Data | 1 | 1 | CROP |

## Exact points vs rasters

Exact: 3 placed (outside 1). Raster-based: 4 placed (outside 0). Difference -1.

| CS2 prefab | exact | rasters |
|---|---:|---:|
| OakTree01 | 3 | 4 |
