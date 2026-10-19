# Failed edges by collision

Source: `out/import-report-failed.json` against `out/round3-import-all.json` (the body that was imported). Contact = centrelines closer than the sum of both roads' half widths (approximate CS2 widths). The live network was not available, so the other courses of the same body stand in for the live segments; all of them were created or merged.

## Counts (game reason → what the course runs into)

| game reason | classification | edges |
|---|---|---:|
| OverlapExisting | crossing, low clearance | 9 |
| OverlapExisting | tight parallel | 10 |
| OverlapExisting | stacked parallel | 6 |
| OverlapExisting | no contact with another course of this import | 15 |
| OverlapExisting | parallel diverge | 2 |
| OverlapExisting | crossing | 1 |
| InvalidShape | no contact with another course of this import | 7 |
| InvalidShape | tight parallel | 4 |
| SteepSlope | no contact with another course of this import | 10 |
| NotEnoughClearance | crossing, low clearance | 5 |
| NotEnoughClearance | no contact with another course of this import | 2 |
| NotEnoughClearance | stacked parallel | 1 |
| coverage | tight parallel | 2 |
| coverage | no contact with another course of this import | 4 |
| coverage | crossing, low clearance | 2 |

## Edges

| edge | prefab | game reason | classification | nearest courses (prefab, kind, contact length, min Δy) | x | z |
|---|---|---|---|---|---:|---:|
| e59_0 | Highway Oneway - 2 lanes | OverlapExisting | crossing, low clearance | e7553_0 (Highway Oneway - 1 lane, crossing, low clearance, 18 m, Δy 7.1); e33976_0 (Highway Oneway - 1 lane, stacked parallel, 47 m, Δy 6.2) | 208 | 3726 |
| e2266_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e413_0 (Highway Oneway - 3 lanes, tight parallel, 87 m, Δy 0.3); e11900_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0.6) | 291 | -4362 |
| e2435_0 | Highway Oneway - 2 lanes | OverlapExisting | stacked parallel | e17314_0 (Highway Oneway - 2 lanes, stacked parallel, 27 m, Δy 8.7); e21099_0 (Highway Oneway - 2 lanes, crossing, 12 m, Δy 8.9) | 606 | -4739 |
| e2886_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e17800_0 (Highway Oneway - 3 lanes) at 0 m | 1641 | -3342 |
| e3183_0 | Highway Oneway - 2 lanes | OverlapExisting | parallel diverge | e20735_0 (Highway Oneway - 1 lane, parallel diverge, 49 m, Δy 0); e20380_0 (Highway Oneway - 1 lane, stacked parallel, 31 m, Δy 7.6); e31840_0 (Highway Oneway - 1 lane, stacked parallel, 23 m, Δy 7.3) | 2988 | 1127 |
| e4709_0 | Highway Oneway - 2 lanes | OverlapExisting | crossing, low clearance | e33472_0 (Highway Oneway - 2 lanes, crossing, low clearance, 10 m, Δy 6.4); e23484_0 (Highway Oneway - 2 lanes, stacked parallel, 40 m, Δy 6.7); e29767_0 (Highway Oneway - 2 lanes, stacked parallel, 56 m, Δy 6.5) | 3008 | 1177 |
| e5185_0 | Highway Oneway - 4 lanes | OverlapExisting | no contact with another course of this import | nearest e21367_0 (Highway Oneway - 3 lanes) at 0 m | 3752 | 1769 |
| e6031_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e14397_0 (Highway Oneway - 3 lanes) at 0 m | 1783 | -3155 |
| e6891_0 | Highway Oneway - 4 lanes | OverlapExisting | crossing, low clearance | e1403_0 (Highway Oneway - 4 lanes, crossing, low clearance, 18 m, Δy 0) | 516 | -4563 |
| e7316_0 | Highway Oneway - 3 lanes | OverlapExisting | crossing | e27902_0 (Double Train Track, crossing, 4 m, Δy 28.5); e28879_0 (Double Train Track, crossing, 4 m, Δy 28.5) | 1999 | -2870 |
| e7697_0 | Highway Oneway - 2 lanes | InvalidShape | no contact with another course of this import | nearest e36478_0 (Highway Oneway - 2 lanes) at 0 m | -6715 | 1452 |
| e7756_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e1974_0 (Highway Oneway - 3 lanes, tight parallel, 58 m, Δy 0); e36209_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0); e14169_0 (Highway Oneway - 1 lane, crossing, low clearance, 14 m, Δy 0) | -335 | -1117 |
| e8479_0 | Highway Oneway - 3 lanes | InvalidShape | no contact with another course of this import | nearest e26745_0 (Highway Oneway - 3 lanes) at 0 m | 2260 | -2526 |
| e8915_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e13465_0 (Highway Oneway - 3 lanes, tight parallel, 68 m, Δy 0); e303_0 (Highway Oneway - 3 lanes, crossing, low clearance, 8 m, Δy 0); e12088_0 (Highway Oneway - 1 lane, crossing, low clearance, 18 m, Δy 0) | -331 | 3888 |
| e9740_0 | Highway Oneway - 2 lanes | SteepSlope | no contact with another course of this import | nearest e7835_0 (Highway Oneway - 2 lanes) at 0 m | -7168 | 1831 |
| e10238_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e20556_0 (Highway Oneway - 3 lanes) at 0 m | 1282 | -3842 |
| e11287_0 | Highway Oneway - 1 lane | OverlapExisting | crossing, low clearance | e4838_0 (Highway Oneway - 2 lanes, crossing, low clearance, 18 m, Δy 7); e21897_0 (Highway Oneway - 2 lanes, crossing, low clearance, 20 m, Δy 7) | 4030 | 4911 |
| e11900_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e28471_0 (Highway Oneway - 4 lanes, tight parallel, 82 m, Δy 0); e1403_0 (Highway Oneway - 4 lanes, crossing, low clearance, 10 m, Δy 0); e2266_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0.6) | 412 | -4474 |
| e12097_0 | Highway Oneway - 1 lane | NotEnoughClearance, OverlapExisting | crossing, low clearance | e6041_0 (Highway Oneway - 1 lane, crossing, low clearance, 12 m, Δy 7.3); e29775_0 (Highway Oneway - 2 lanes, crossing, low clearance, 16 m, Δy 7.4); e6801_0 (Highway Oneway - 2 lanes, crossing, low clearance, 16 m, Δy 6.8) | 3647 | 2127 |
| e12548_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e33845_0 (Highway Oneway - 3 lanes) at 0 m | 1865 | -3047 |
| e12648_0 | Highway Oneway - 2 lanes | OverlapExisting | crossing, low clearance | e7553_0 (Highway Oneway - 1 lane, crossing, low clearance, 6 m, Δy 7.2); e23083_0 (Highway Oneway - 1 lane, stacked parallel, 23 m, Δy 6.8) | 415 | 3778 |
| e12792_0 | Highway Oneway - 2 lanes | OverlapExisting, NotEnoughClearance | no contact with another course of this import | nearest e22300_0 (Highway Oneway - 2 lanes) at 0 m | 607 | -4821 |
| e13146_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e29898_0 (Highway Oneway - 3 lanes) at 0 m | 1200 | -3950 |
| e13292_0 | Highway Oneway - 2 lanes | OverlapExisting | stacked parallel | e24955_0 (Highway Oneway - 2 lanes, stacked parallel, 26 m, Δy 8.5); e18898_0 (Highway Oneway - 2 lanes, stacked parallel, 24 m, Δy 8.5) | 3860 | 4694 |
| e13589_0 | Highway Oneway - 2 lanes | SteepSlope | no contact with another course of this import | nearest e32174_0 (Highway Oneway - 2 lanes) at 0 m | -5516 | 190 |
| e14397_0 | Highway Oneway - 3 lanes | OverlapExisting, SteepSlope | no contact with another course of this import | nearest e6031_0 (Highway Oneway - 3 lanes) at 0 m | 1745 | -3205 |
| e14428_0 | Highway Oneway - 1 lane | OverlapExisting | crossing, low clearance | e6041_0 (Highway Oneway - 1 lane, crossing, low clearance, 8 m, Δy 7.3); e28071_0 (Highway Oneway - 1 lane, crossing, 16 m, Δy 11.9); e3638_0 (Highway Oneway - 1 lane, crossing, 20 m, Δy 8.5) | 3663 | 2054 |
| e15883_0 | Highway Oneway - 4 lanes | coverage | tight parallel | e1403_0 (Highway Oneway - 4 lanes, tight parallel, 70 m, Δy 0); e28471_0 (Highway Oneway - 4 lanes, crossing, low clearance, 12 m, Δy 0) | 463 | -4520 |
| e15887_0 | Highway Oneway - 1 lane | NotEnoughClearance | crossing, low clearance | e30823_0 (Highway Oneway - 1 lane, crossing, low clearance, 16 m, Δy 6.8); e1808_0 (Highway Oneway - 1 lane, stacked parallel, 22 m, Δy 6.5); e26294_0 (Highway Oneway - 2 lanes, crossing, 20 m, Δy 13.9) | -1765 | -1817 |
| e15892_0 | Highway Oneway - 1 lane | OverlapExisting | stacked parallel | e29775_0 (Highway Oneway - 2 lanes, stacked parallel, 22 m, Δy 14.8); e6801_0 (Highway Oneway - 2 lanes, stacked parallel, 22 m, Δy 15.1); e12097_0 (Highway Oneway - 1 lane, stacked parallel, 24 m, Δy 8.6) | 3632 | 2108 |
| e15986_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e12548_0 (Highway Oneway - 3 lanes) at 0 m | 1825 | -3100 |
| e16124_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e36209_0 (Highway Oneway - 3 lanes, tight parallel, 58 m, Δy 0); e1974_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0) | -305 | -1069 |
| e16165_0 | Highway Oneway - 3 lanes | SteepSlope | no contact with another course of this import | nearest e33711_0 (Highway Oneway - 3 lanes) at 0 m | 1419 | -3635 |
| e16211_0 | Highway Oneway - 2 lanes | InvalidShape | no contact with another course of this import | nearest e6722_0 (Highway Oneway - 2 lanes) at 0 m | -913 | -2291 |
| e16410_0 | Highway Oneway - 4 lanes | OverlapExisting, InvalidShape | no contact with another course of this import | nearest e5703_0 (Highway Oneway - 2 lanes) at 0 m | 701 | -4607 |
| e16506_0 | Highway Oneway - 3 lanes | SteepSlope | no contact with another course of this import | nearest e16165_0 (Highway Oneway - 3 lanes) at 0 m | 1360 | -3712 |
| e17366_0 | Highway Oneway - 2 lanes | SteepSlope | no contact with another course of this import | nearest e15658_0 (Highway Oneway - 2 lanes) at 0 m | -7143 | 1844 |
| e17760_0 | Highway Oneway - 2 lanes | OverlapExisting | stacked parallel | e22005_0 (Highway Oneway - 1 lane, stacked parallel, 33 m, Δy 8); e9026_0 (Highway Oneway - 1 lane, stacked parallel, 23 m, Δy 9.6) | 3638 | 2273 |
| e17800_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e2886_0 (Highway Oneway - 3 lanes) at 0 m | 1588 | -3412 |
| e17897_0 | Highway Oneway - 3 lanes | OverlapExisting, SteepSlope | no contact with another course of this import | nearest e14574_0 (Highway Oneway - 3 lanes) at 0 m | 979 | -4215 |
| e18004_0 | Highway Oneway - 3 lanes | SteepSlope | no contact with another course of this import | nearest e36148_0 (Highway Oneway - 3 lanes) at 0 m | 1104 | -4049 |
| e18149_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e7364_0 (Highway Oneway - 3 lanes, tight parallel, 26 m, Δy 0); e11725_0 (Highway Oneway - 3 lanes, crossing, low clearance, 6 m, Δy 0) | 3785 | 4680 |
| e18161_0 | Highway Oneway - 3 lanes | OverlapExisting | tight parallel | e15515_0 (Highway Oneway - 3 lanes, tight parallel, 78 m, Δy 0.2); e8581_0 (Highway Oneway - 3 lanes, crossing, low clearance, 6 m, Δy 0.2) | -1000 | -1420 |
| e18529_0 | Highway Oneway - 1 lane | NotEnoughClearance | crossing, low clearance | e843_0 (Highway Oneway - 1 lane, crossing, low clearance, 12 m, Δy 6.4); e1223_0 (Highway Oneway - 1 lane, crossing, low clearance, 19 m, Δy 6.5); e5188_0 (Highway Oneway - 1 lane, crossing, 10 m, Δy 10) | 3495 | 1972 |
| e18898_0 | Highway Oneway - 2 lanes | NotEnoughClearance | stacked parallel | e7959_0 (Highway Oneway - 2 lanes, stacked parallel, 36 m, Δy 7.2); e13068_0 (Highway Oneway - 1 lane, stacked parallel, 22 m, Δy 8.5); e5630_0 (Highway Oneway - 2 lanes, crossing, 14 m, Δy 8) | 3885 | 4777 |
| e19076_0 | Highway Oneway - 3 lanes | OverlapExisting | crossing, low clearance | e413_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0.3) | 238 | -4291 |
| e19107_0 | Highway Oneway - 2 lanes | OverlapExisting | crossing, low clearance | e33122_0 (Highway Oneway - 2 lanes, crossing, low clearance, 18 m, Δy 7.1); e14182_0 (Highway Oneway - 2 lanes, stacked parallel, 30 m, Δy 11.4); e33321_0 (Highway Oneway - 2 lanes, stacked parallel, 22 m, Δy 12.3) | 681 | -4721 |
| e20401_0 | Highway Oneway - 3 lanes | SteepSlope | no contact with another course of this import | nearest e26240_0 (Highway Oneway - 3 lanes) at 0 m | 1373 | -3721 |
| e20430_0 | Highway Oneway - 3 lanes | OverlapExisting, InvalidShape | tight parallel | e16466_0 (Highway Oneway - 3 lanes, tight parallel, 81 m, Δy 0); e22842_0 (Highway Oneway - 3 lanes, crossing, low clearance, 6 m, Δy 0) | 868 | 4040 |
| e20494_0 | Highway Oneway - 1 lane | InvalidShape | tight parallel | e20052_0 (Highway Oneway - 1 lane, tight parallel, 28 m, Δy 0); e7993_0 (Highway Oneway - 2 lanes, crossing, low clearance, 18 m, Δy 0); e25924_0 (Highway Oneway - 4 lanes, crossing, low clearance, 14 m, Δy 0) | 3805 | 2241 |
| e20531_0 | Highway Oneway - 3 lanes | OverlapExisting, InvalidShape | tight parallel | e8581_0 (Highway Oneway - 3 lanes, tight parallel, 78 m, Δy 0.2); e15515_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0.2); e34370_0 (Highway Oneway - 1 lane, crossing, low clearance, 14 m, Δy 0) | -924 | -1427 |
| e20556_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e13146_0 (Highway Oneway - 3 lanes) at 0 m | 1240 | -3897 |
| e20735_0 | Highway Oneway - 1 lane | OverlapExisting | parallel diverge | e3183_0 (Highway Oneway - 2 lanes, parallel diverge, 49 m, Δy 0); e31840_0 (Highway Oneway - 1 lane, stacked parallel, 29 m, Δy 6.9); e20380_0 (Highway Oneway - 1 lane, crossing, 16 m, Δy 7.7) | 2977 | 1138 |
| e21401_0 | Highway Oneway - 3 lanes | InvalidShape | no contact with another course of this import | nearest e22393_0 (Highway Oneway - 3 lanes) at 0 m | 2117 | -2742 |
| e21530_0 | Highway Oneway - 2 lanes | OverlapExisting | stacked parallel | e20616_0 (Highway Oneway - 1 lane, stacked parallel, 28 m, Δy 9.3); e33741_0 (Highway Oneway - 1 lane, stacked parallel, 22 m, Δy 28.4); e19926_0 (Highway Oneway - 1 lane, stacked parallel, 22 m, Δy 9.4) | 3499 | 2148 |
| e21574_0 | Highway Oneway - 3 lanes | InvalidShape | no contact with another course of this import | nearest e14233_0 (Highway Oneway - 3 lanes) at 0 m | -1991 | 6572 |
| e21918_0 | Highway Oneway - 1 lane | NotEnoughClearance | crossing, low clearance | e3735_0 (Highway Oneway - 1 lane, crossing, low clearance, 16 m, Δy 6.3); e501_0 (Highway Oneway - 1 lane, crossing, 16 m, Δy 7.7); e13450_0 (Highway Oneway - 1 lane, crossing, 16 m, Δy 7.7) | 63 | 69 |
| e22300_0 | Highway Oneway - 2 lanes | NotEnoughClearance | no contact with another course of this import | nearest e12792_0 (Highway Oneway - 2 lanes) at 0 m | 618 | -4742 |
| e22393_0 | Highway Oneway - 3 lanes | OverlapExisting | no contact with another course of this import | nearest e7621_0 (Highway Oneway - 3 lanes) at 0 m | 2169 | -2673 |
| e22426_0 | Highway Oneway - 1 lane | NotEnoughClearance | crossing, low clearance | e10453_0 (Highway Oneway - 1 lane, crossing, low clearance, 8 m, Δy 6.5); e3338_0 (Highway Oneway - 1 lane, crossing, low clearance, 20 m, Δy 6.5); e31526_0 (Highway Oneway - 1 lane, crossing, low clearance, 16 m, Δy 6.5) | 200 | 101 |
| e22842_0 | Highway Oneway - 3 lanes | OverlapExisting, InvalidShape | tight parallel | e20118_0 (Highway Oneway - 3 lanes, tight parallel, 81 m, Δy 0); e20430_0 (Highway Oneway - 3 lanes, crossing, low clearance, 4 m, Δy 0) | 739 | 3946 |
| e23062_0 | Highway Oneway - 1 lane | OverlapExisting | stacked parallel | e5049_0 (Highway Oneway - 2 lanes, stacked parallel, 37 m, Δy 6.9); e14974_0 (Highway Oneway - 2 lanes, stacked parallel, 22 m, Δy 6.8) | 138 | 3702 |
| e23795_0 | Highway Oneway - 3 lanes | OverlapExisting, SteepSlope | no contact with another course of this import | nearest e36659_0 (Highway Oneway - 3 lanes) at 0 m | 1712 | -3275 |
| e23833_0 | Highway Oneway - 3 lanes | InvalidShape | no contact with another course of this import | nearest e21314_0 (Highway Oneway - 3 lanes) at 0 m | -2144 | 7096 |
| e31685_0 | Highway Oneway - 2 lanes | coverage | tight parallel | e20045_0 (Highway Oneway - 1 lane, tight parallel, 36 m, Δy 0); e16428_0 (Highway Oneway - 1 lane, crossing, low clearance, 18 m, Δy 0) | 3652 | 1881 |
| e23168_0 | Gravel Road | coverage | no contact with another course of this import | nearest e7506_0 (Gravel Road) at 0 m | 6981 | 1715 |
| e25579_0 | Gravel Road | coverage | crossing, low clearance | e2043_0 (Gravel Road, crossing, low clearance, 14 m, Δy 0); e21765_0 (Gravel Road, crossing, low clearance, 14 m, Δy 0) | 6780 | 1411 |
| e31265_0 | Gravel Road | coverage | no contact with another course of this import | nearest e19271_0 (Gravel Road) at 0 m | 6758 | 2945 |
| e31665_0 | Gravel Road | coverage | no contact with another course of this import | nearest e11608_0 (Gravel Road) at 0 m | 6468 | 1003 |
| e36006_0 | Gravel Road | coverage | no contact with another course of this import | nearest e3416_0 (Gravel Road) at 0 m | 6335 | 3361 |
| e36563_0 | Gravel Road | coverage | crossing, low clearance | e12565_0 (Gravel Road, crossing, low clearance, 14 m, Δy 0); e17591_0 (Gravel Road, crossing, low clearance, 14 m, Δy 0) | 5564 | 820 |
