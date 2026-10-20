# Outside connections check

A border node is matched by following the CS1 network from its clipped segment outwards to the node flagged Outside and taking the CS1 connection building of the same transport within 150 m of it; failing that, the building projected onto the CS2 border (±7168 m) within 300 m. distance = border node to the building's border projection.

## Border nodes of the imported network

| layer | node | prefab | x | z | CS1 connection | distance m |
|---|---|---|---:|---:|---|---:|
| highways | c10624_0 | Highway Oneway - 2 lanes | 4459 | 7168 | 29724 | 281 |
| highways | c15437_0 | Highway Oneway - 3 lanes | 7168 | 2152 | 20804 | 235 |
| highways | c17366_0 | Highway Oneway - 2 lanes | -7168 | 1846 | 41616 | 920 |
| highways | c22660_0 | Highway Oneway - 3 lanes | 1391 | -7168 | 47978 | 217 |
| highways | c32988_0 | Highway Oneway - 3 lanes | 7168 | 2178 | 41809 | 235 |
| highways | c3320_0 | Highway Oneway - 2 lanes | 4475 | 7168 | 1382 | 281 |
| highways | c35891_0 | Highway Oneway - 3 lanes | 1372 | -7168 | 9628 | 217 |
| highways | c36454_0 | Highway Oneway - 3 lanes | -2181 | 7168 | 45798 | 1200 |
| highways | c36711_0 | Highway Oneway - 3 lanes | -2160 | 7168 | 25565 | 1186 |
| highways | c9740_0 | Highway Oneway - 2 lanes | -7168 | 1831 | 31146 | 913 |
| rail | c15951_0 | Double Train Track | -1909 | 7168 | 42176 | 128 |
| rail | c16061_0 | Double Train Track | 7168 | 2300 | 6789 | 234 |
| rail | c34544_0 | Double Train Track | 7168 | -6585 | 7993 | 583 |
| rail | c6866_0 | Double Train Track | -196 | -7168 | 32422 | 216 |
| roads | c1244_0 | Small Road | -1616 | -7168 | 33411 | 1022 |
| roads | c23437_0 | Small Road | 7168 | 2425 | 45251 | 677 |
| roads | c31152_0 | Gravel Road | 7168 | 3456 | 45251 | 355 |
| roads | c3374_0 | Small Road | 5824 | -7168 | 9216 | 1344 |
| roads | c35664_0 | Gravel Road | 7168 | 2034 | 41809 | 91 |
| roads | c9707_0 | Gravel Road | 7168 | 1714 | 20804 | 203 |

## CS1 connections

| id | prefab | type | in/out | CS1 x | CS1 z | border x | border z | border nodes |
|---|---|---|---|---:|---:|---:|---:|---|
| 10384 | Airplane Connection | plane | in/out | -272 | -8653 | -272 | -7168 | object at the border point |
| 16610 | Airplane Connection | plane | in/out | 8653 | 1647 | 7168 | 1647 | object at the border point |
| 22143 | Airplane Connection | plane | in/out | -2561 | 8653 | -2561 | 7168 | object at the border point |
| 36525 | Airplane Connection | plane | in/out | -3263 | 8653 | -3263 | 7168 | object at the border point |
| 1382 | Road Connection | road | in | 4756 | 8647 | 4756 | 7168 | c3320_0 |
| 9216 | Road Connection Small | road | in/out | 7416 | -8646 | 7168 | -7168 | c3374_0 |
| 9628 | Road Connection | road | in | 1588 | -8650 | 1588 | -7168 | c35891_0 |
| 20804 | Road Connection | road | in | 8649 | 1917 | 7168 | 1917 | c9707_0, c15437_0 |
| 25565 | Road Connection | road | in | -3345 | 8649 | -3345 | 7168 | c36711_0 |
| 29724 | Road Connection | road | /out | 4740 | 8645 | 4740 | 7168 | c10624_0 |
| 31146 | Road Connection | road | /out | -8647 | 2744 | -7168 | 2744 | c9740_0 |
| 33411 | Road Connection Small | road | in/out | -594 | -8647 | -594 | -7168 | c1244_0 |
| 41616 | Road Connection | road | in | -8646 | 2766 | -7168 | 2766 | c17366_0 |
| 41809 | Road Connection | road | /out | 8649 | 1943 | 7168 | 1943 | c32988_0, c35664_0 |
| 45251 | Road Connection Small | road | in/out | 8646 | 3102 | 7168 | 3102 | c31152_0, c23437_0 |
| 45798 | Road Connection | road | /out | -3382 | 8648 | -3382 | 7168 | c36454_0 |
| 47978 | Road Connection | road | /out | 1608 | -8648 | 1608 | -7168 | c22660_0 |
| 19314 | Ship Connection | ship | in/out | 5736 | -8653 | 5736 | -7168 | object at the border point |
| 6789 | Train Connection | train | in/out | 8644 | 2065 | 7168 | 2065 | c16061_0 |
| 7993 | Train Connection | train | in/out | 8644 | -7901 | 7168 | -7168 | c34544_0 |
| 32422 | Train Connection | train | in/out | 20 | -8644 | 20 | -7168 | c6866_0 |
| 42176 | Train Connection | train | in/out | -2037 | 8644 | -2037 | 7168 | c15951_0 |
