# Reconciliation with the live game

Live graph `out\live-graph.json`: 3138 segments, 2973 nodes. Coverage: 5 points per import edge (t = 0.1 … 0.9), each within 6 m of a live segment (chord + sagitta allowance, or the import curve when both ends coincide).

Statuses: covered = all 5 points on a live segment of the mapped prefab; wrong prefab = all 5 on some network; partial = 1–4; missing = none. Retry = missing, or partial with at most 2 points.

### After fixes (current import-*.json)

| layer | prefab | edges | covered | wrong prefab | partial | missing | → retry |
|---|---|---:|---:|---:|---:|---:|---:|
| highways | Highway Oneway - 1 lane | 282 | 281 | 1 | 0 | 0 | 0 |
| highways | Highway Oneway - 2 lanes | 901 | 898 | 3 | 0 | 0 | 0 |
| highways | Highway Oneway - 3 lanes | 540 | 540 | 0 | 0 | 0 | 0 |
| highways | Highway Oneway - 4 lanes | 29 | 29 | 0 | 0 | 0 | 0 |
| rail | Double Train Track | 410 | 409 | 0 | 1 | 0 | 0 |
| roads | Gravel Road | 1257 | 1257 | 0 | 0 | 0 | 0 |
| roads | Highway Twoway - 2 lanes | 308 | 0 | 308 | 0 | 0 | 0 |
| roads | Highway Twoway - 4 lanes | 39 | 0 | 39 | 0 | 0 | 0 |
| roads | Pavement Path | 15 | 0 | 0 | 1 | 14 | 15 |
| **highways** | **all** | 1752 | 1748 | 4 | 0 | 0 | 0 |
| **rail** | **all** | 410 | 409 | 0 | 1 | 0 | 0 |
| **roads** | **all** | 1619 | 1257 | 347 | 1 | 14 | 15 |

### Edges not fully covered

| layer | edge | prefab | status | points mapped/any | x | z |
|---|---|---|---|---|---:|---:|
| highways | e3183_0 | Highway Oneway - 2 lanes | WrongPrefab | 3/5 | 2968 | 1098 |
| highways | e19786_0 | Highway Oneway - 2 lanes | WrongPrefab | 4/5 | 3411 | 1985 |
| highways | e32650_0 | Highway Oneway - 1 lane | WrongPrefab | 3/5 | 3671 | 2153 |
| highways | e34504_0 | Highway Oneway - 2 lanes | WrongPrefab | 3/5 | 3785 | 2295 |
| rail | e25368_0 | Double Train Track | Partial | 4/4 | -182 | 2388 |
| roads | e98_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2999 | -3905 |
| roads | e166_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1636 | -1449 |
| roads | e328_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1255 | -5672 |
| roads | e482_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2729 | -4278 |
| roads | e512_0 | Pavement Path | Partial | 0/1 | 32 | -2425 |
| roads | e574_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4353 | -1181 |
| roads | e702_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4585 | -855 |
| roads | e824_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4792 | -3032 |
| roads | e890_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2745 | -323 |
| roads | e970_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -590 | -1283 |
| roads | e1025_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1020 | -1916 |
| roads | e1078_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4098 | -2389 |
| roads | e1138_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5959 | -5023 |
| roads | e1244_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1628 | -7135 |
| roads | e1408_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6385 | 1792 |
| roads | e1532_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3941 | -2608 |
| roads | e1698_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3162 | -3680 |
| roads | e1734_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4364 | -2306 |
| roads | e1801_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4402 | -1112 |
| roads | e1889_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4305 | -1249 |
| roads | e1904_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5778 | -6043 |
| roads | e2121_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1358 | -6330 |
| roads | e2242_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6266 | 1617 |
| roads | e2447_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1321 | -5542 |
| roads | e2519_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4211 | 11 |
| roads | e2528_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5177 | -3503 |
| roads | e2582_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2075 | -1023 |
| roads | e2641_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5572 | -3986 |
| roads | e2654_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2781 | -4205 |
| roads | e2709_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4130 | -2004 |
| roads | e2731_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4148 | -1634 |
| roads | e2797_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5744 | -4197 |
| roads | e2945_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4261 | -1690 |
| roads | e3195_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4165 | -2136 |
| roads | e3374_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5826 | -7140 |
| roads | e4485_0 | Pavement Path | Missing | 0/0 | 33 | -2679 |
| roads | e4740_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4319 | -1003 |
| roads | e4840_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5329 | -102 |
| roads | e5256_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6082 | -6694 |
| roads | e5700_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5822 | -5529 |
| roads | e5903_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -456 | -1386 |
| roads | e5928_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3807 | 54 |
| roads | e5934_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6658 | 2462 |
| roads | e6055_0 | Pavement Path | Missing | 0/0 | 16 | -2686 |
| roads | e6154_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2133 | -956 |
| roads | e6440_0a | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4168 | 32 |
| roads | e6440_0b | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4184 | 7 |
| roads | e6548_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1758 | -4992 |
| roads | e6569_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4399 | -2545 |
| roads | e7126_0 | Pavement Path | Missing | 0/0 | 367 | -3024 |
| roads | e7145_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5815 | 952 |
| roads | e7147_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5987 | -4673 |
| roads | e7290_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5124 | -3438 |
| roads | e7461_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5811 | -6107 |
| roads | e7524_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3370 | -89 |
| roads | e7525_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4439 | -2465 |
| roads | e8113_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4231 | -2129 |
| roads | e8297_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4135 | -189 |
| roads | e8335_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4936 | -605 |
| roads | e8408_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2888 | -252 |
| roads | e8509_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3108 | -3755 |
| roads | e8560_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3053 | -3830 |
| roads | e8580_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3833 | -2757 |
| roads | e8708_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6482 | 1935 |
| roads | e8751_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1685 | -6625 |
| roads | e8810_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4303 | -806 |
| roads | e8813_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4396 | -590 |
| roads | e8893_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4142 | -1865 |
| roads | e8930_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1839 | -1328 |
| roads | e9080_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5854 | -4334 |
| roads | e9128_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4478 | -2648 |
| roads | e9146_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1351 | -6020 |
| roads | e9277_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2050 | -4897 |
| roads | e9600_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1484 | -5404 |
| roads | e9628_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1111 | -1875 |
| roads | e9721_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4139 | -1562 |
| roads | e9729_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3202 | -144 |
| roads | e9771_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4207 | -1386 |
| roads | e9800_0 | Pavement Path | Missing | 0/0 | 37 | -2642 |
| roads | e9848_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4249 | -102 |
| roads | e9926_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5749 | -5982 |
| roads | e10089_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5070 | -3373 |
| roads | e10131_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4081 | -194 |
| roads | e10173_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1741 | -6676 |
| roads | e10217_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4625 | -2828 |
| roads | e10242_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5021 | -568 |
| roads | e10366_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4415 | -1474 |
| roads | e10382_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4541 | -916 |
| roads | e10422_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4196 | -167 |
| roads | e10435_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2426 | -4696 |
| roads | e10437_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6274 | 710 |
| roads | e10456_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4169 | -1801 |
| roads | e10493_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2629 | -4420 |
| roads | e10522_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3217 | -3605 |
| roads | e10646_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4041 | 162 |
| roads | e10725_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5840 | -5464 |
| roads | e10901_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4188 | -1883 |
| roads | e10929_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5371 | -3740 |
| roads | e10970_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2890 | -4055 |
| roads | e11334_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3721 | -2910 |
| roads | e11385_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4057 | 218 |
| roads | e11432_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1199 | -1836 |
| roads | e11675_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5837 | -7072 |
| roads | e11739_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1765 | -6741 |
| roads | e11849_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1620 | -5124 |
| roads | e11882_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2132 | -4876 |
| roads | e12070_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1700 | -6866 |
| roads | e12141_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5736 | -5857 |
| roads | e12223_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4316 | -887 |
| roads | e12287_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4360 | -1268 |
| roads | e12316_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5884 | -6245 |
| roads | e12553_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5330 | 90 |
| roads | e12582_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4346 | -770 |
| roads | e12622_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4332 | -1183 |
| roads | e12687_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6560 | 2089 |
| roads | e12868_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6606 | 2379 |
| roads | e12936_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6019 | 1253 |
| roads | e13007_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4959 | -3237 |
| roads | e13009_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6416 | 976 |
| roads | e13070_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4049 | -2459 |
| roads | e13170_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5521 | 518 |
| roads | e13242_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5968 | 1177 |
| roads | e13290_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5575 | 597 |
| roads | e13343_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1661 | -6998 |
| roads | e13504_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -534 | -1384 |
| roads | e13540_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5921 | -6316 |
| roads | e13550_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5905 | -5224 |
| roads | e13564_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5333 | -3695 |
| roads | e13646_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5685 | -4125 |
| roads | e13686_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -480 | -2181 |
| roads | e13882_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2530 | -4562 |
| roads | e13896_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1434 | -5454 |
| roads | e13908_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5923 | -5158 |
| roads | e14091_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5847 | -6174 |
| roads | e14106_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5966 | -6856 |
| roads | e14208_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5458 | -3847 |
| roads | e14439_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4497 | -978 |
| roads | e14627_0 | Pavement Path | Missing | 0/0 | 16 | -2563 |
| roads | e14669_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1275 | -5602 |
| roads | e14683_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2962 | -223 |
| roads | e14726_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4328 | -266 |
| roads | e14776_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4161 | -1717 |
| roads | e14794_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4463 | -2420 |
| roads | e15020_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4432 | -2592 |
| roads | e15050_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5939 | -4491 |
| roads | e15057_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4344 | -421 |
| roads | e15138_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5768 | -5730 |
| roads | e15425_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3889 | 82 |
| roads | e15602_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5232 | -3571 |
| roads | e15697_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1905 | -1224 |
| roads | e15809_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1552 | -6542 |
| roads | e15829_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6574 | 2165 |
| roads | e16121_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -494 | -1414 |
| roads | e16191_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4423 | -2339 |
| roads | e16242_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2193 | -886 |
| roads | e16458_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4307 | -2258 |
| roads | e16613_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5865 | 1025 |
| roads | e16689_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4147 | -1501 |
| roads | e16784_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -934 | -1957 |
| roads | e17013_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6170 | 1475 |
| roads | e17043_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -616 | -2114 |
| roads | e17156_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1973 | -4909 |
| roads | e17330_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6428 | 1856 |
| roads | e17477_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4698 | -729 |
| roads | e17505_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4770 | -679 |
| roads | e17696_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1671 | -6933 |
| roads | e17776_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2944 | -3980 |
| roads | e17824_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3887 | -2682 |
| roads | e17850_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4265 | 33 |
| roads | e17969_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1529 | -5346 |
| roads | e18033_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5959 | -6391 |
| roads | e18056_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1403 | -1683 |
| roads | e18065_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5627 | 674 |
| roads | e18179_0a | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4158 | 50 |
| roads | e18179_0b | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4141 | 78 |
| roads | e18236_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -2310 | -748 |
| roads | e18414_0 | Pavement Path | Missing | 0/0 | 4785 | -696 |
| roads | e18498_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6002 | -6472 |
| roads | e18537_0a | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3313 | -3473 |
| roads | e18537_0b | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3344 | -3430 |
| roads | e18630_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -2244 | -826 |
| roads | e18890_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1264 | -5747 |
| roads | e18907_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1562 | -5273 |
| roads | e18960_0 | Pavement Path | Missing | 0/0 | 302 | -3028 |
| roads | e18990_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6928 | 2419 |
| roads | e19400_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -631 | -1253 |
| roads | e19560_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5803 | -5598 |
| roads | e19810_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2482 | -4632 |
| roads | e19818_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5994 | -4846 |
| roads | e19899_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3628 | -5 |
| roads | e19990_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5015 | -3305 |
| roads | e19996_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5857 | -5399 |
| roads | e19998_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4133 | -2321 |
| roads | e20013_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4736 | -2964 |
| roads | e20072_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5674 | 743 |
| roads | e20176_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2483 | -546 |
| roads | e20478_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1585 | -5196 |
| roads | e20484_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5866 | -6989 |
| roads | e20485_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6071 | 1329 |
| roads | e20500_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -2432 | -606 |
| roads | e20518_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5405 | -3782 |
| roads | e20588_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5288 | -3639 |
| roads | e20651_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6383 | 907 |
| roads | e20853_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4214 | -2044 |
| roads | e21034_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1356 | -6179 |
| roads | e21165_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4281 | -156 |
| roads | e21169_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4261 | -2197 |
| roads | e21236_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -284 | -2279 |
| roads | e21352_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4092 | 160 |
| roads | e21520_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6616 | 2444 |
| roads | e21713_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4681 | -2896 |
| roads | e21722_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6077 | -6625 |
| roads | e21976_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4395 | -1340 |
| roads | e22104_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3286 | -117 |
| roads | e22321_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4365 | -513 |
| roads | e22348_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5982 | -4934 |
| roads | e22532_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1316 | -5874 |
| roads | e22639_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4451 | -1044 |
| roads | e22657_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1876 | -1268 |
| roads | e23173_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2361 | -4753 |
| roads | e23270_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4340 | -338 |
| roads | e23272_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6045 | -6554 |
| roads | e23368_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4171 | -2187 |
| roads | e23370_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5516 | -3918 |
| roads | e23437_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 7146 | 2420 |
| roads | e23642_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1648 | -7065 |
| roads | e23759_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5276 | -451 |
| roads | e23794_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1938 | -1184 |
| roads | e24113_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5721 | 813 |
| roads | e24116_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6229 | 648 |
| roads | e24156_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6311 | 1684 |
| roads | e24186_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5329 | -6 |
| roads | e24564_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -665 | -1274 |
| roads | e24607_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1349 | -6256 |
| roads | e24853_0 | Pavement Path | Missing | 0/0 | -9 | -2665 |
| roads | e24921_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1518 | -1538 |
| roads | e25074_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -548 | -2148 |
| roads | e25185_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4295 | -846 |
| roads | e25265_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3271 | -3530 |
| roads | e25471_0 | Pavement Path | Missing | 0/0 | 19 | -2600 |
| roads | e25631_0 | Pavement Path | Missing | 0/0 | 40 | -2472 |
| roads | e25671_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4852 | -641 |
| roads | e25705_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5735 | -5920 |
| roads | e25901_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3995 | -2534 |
| roads | e26018_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2814 | -286 |
| roads | e26050_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 7082 | 2411 |
| roads | e26099_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6871 | 2428 |
| roads | e26137_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4848 | -3100 |
| roads | e26193_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5901 | -4410 |
| roads | e26311_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1279 | -1800 |
| roads | e26359_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4327 | -935 |
| roads | e26712_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1705 | -5030 |
| roads | e26786_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6120 | 1402 |
| roads | e26953_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6826 | 2435 |
| roads | e27142_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4159 | -2248 |
| roads | e27203_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5786 | -5664 |
| roads | e27286_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -2365 | -684 |
| roads | e27325_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5916 | 1101 |
| roads | e27375_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6058 | -6758 |
| roads | e27461_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6349 | 1739 |
| roads | e27477_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6996 | 2410 |
| roads | e27535_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5799 | -4265 |
| roads | e27634_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1826 | -4957 |
| roads | e27783_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4145 | -2076 |
| roads | e27788_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4307 | -205 |
| roads | e27800_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5332 | 169 |
| roads | e27995_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3119 | -171 |
| roads | e28042_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1469 | -1583 |
| roads | e28072_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5627 | -4054 |
| roads | e28128_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1380 | -5498 |
| roads | e28392_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6221 | 1551 |
| roads | e28443_0 | Pavement Path | Missing | 0/0 | -21 | -2605 |
| roads | e28490_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2213 | -4844 |
| roads | e28551_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1485 | -6506 |
| roads | e28585_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -770 | -2037 |
| roads | e28846_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4458 | -2376 |
| roads | e28940_0 | Pavement Path | Missing | 0/0 | 335 | -3032 |
| roads | e29025_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6596 | 2308 |
| roads | e29051_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6585 | 2237 |
| roads | e29134_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4202 | -1970 |
| roads | e29213_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -688 | -2078 |
| roads | e29315_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4210 | -36 |
| roads | e29320_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4209 | -1742 |
| roads | e29342_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5975 | -4610 |
| roads | e29367_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -496 | -1453 |
| roads | e29443_0 | Pavement Path | Missing | 0/0 | -10 | -2572 |
| roads | e29474_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4414 | -1407 |
| roads | e29525_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5467 | 438 |
| roads | e29554_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4635 | -789 |
| roads | e29616_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -238 | -2302 |
| roads | e29648_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4247 | -142 |
| roads | e29848_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5962 | -4559 |
| roads | e29904_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1336 | -5942 |
| roads | e29920_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1291 | -5814 |
| roads | e29967_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3716 | 24 |
| roads | e30001_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4175 | -1800 |
| roads | e30053_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -351 | -2245 |
| roads | e30088_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1383 | -6399 |
| roads | e30228_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3037 | -198 |
| roads | e30301_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4526 | -2707 |
| roads | e30500_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4129 | -1933 |
| roads | e30506_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1437 | -1624 |
| roads | e30614_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4318 | -1093 |
| roads | e30807_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5371 | 292 |
| roads | e31120_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3453 | -62 |
| roads | e31128_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2579 | -4491 |
| roads | e31215_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1620 | -6581 |
| roads | e31251_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3779 | -2831 |
| roads | e31254_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2836 | -4130 |
| roads | e31333_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6314 | 774 |
| roads | e31394_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1971 | -1145 |
| roads | e31425_0 | Pavement Path | Missing | 0/0 | 43 | -2515 |
| roads | e31653_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5413 | 359 |
| roads | e31684_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2676 | -365 |
| roads | e31766_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4403 | -2503 |
| roads | e31827_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4368 | -1595 |
| roads | e31857_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4256 | -1317 |
| roads | e31860_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2018 | -1090 |
| roads | e32130_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2543 | -478 |
| roads | e32483_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1662 | -5069 |
| roads | e32569_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5912 | -6915 |
| roads | e32624_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6013 | -6808 |
| roads | e32771_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1357 | -6101 |
| roads | e32791_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -415 | -2213 |
| roads | e32931_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -3541 | -33 |
| roads | e32978_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6349 | 840 |
| roads | e33245_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1701 | -1416 |
| roads | e33296_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5328 | -198 |
| roads | e33708_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5884 | -5301 |
| roads | e33726_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5321 | -383 |
| roads | e33770_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2607 | -418 |
| roads | e33804_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1348 | -1748 |
| roads | e33936_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5197 | -492 |
| roads | e33992_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1427 | -6458 |
| roads | e34215_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5938 | -5103 |
| roads | e34238_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -3961 | 115 |
| roads | e34353_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -628 | -1215 |
| roads | e34701_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4574 | -2766 |
| roads | e34750_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1746 | -6808 |
| roads | e34806_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5109 | -530 |
| roads | e34816_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5346 | 232 |
| roads | e34887_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4400 | -1537 |
| roads | e34892_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4167 | -1447 |
| roads | e34971_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1899 | -4929 |
| roads | e34976_0 | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -6449 | 1047 |
| roads | e34979_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5328 | -294 |
| roads | e34982_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 4903 | -3168 |
| roads | e35068_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1774 | -1385 |
| roads | e35405_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -853 | -1996 |
| roads | e35527_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5768 | 882 |
| roads | e35617_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5995 | -4756 |
| roads | e35760_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2679 | -4349 |
| roads | e35888_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4387 | -718 |
| roads | e35977_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4404 | -656 |
| roads | e36303_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -2289 | -4803 |
| roads | e36355_0a | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4130 | 97 |
| roads | e36355_0b | Highway Twoway - 4 lanes | WrongPrefab | 0/5 | -4112 | 126 |
| roads | e36621_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6752 | 2447 |
| roads | e36635_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 6530 | 2011 |
| roads | e36679_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | 5750 | -5795 |
| roads | e36836_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -4320 | -1646 |
| roads | e36849_0 | Highway Twoway - 2 lanes | WrongPrefab | 0/5 | -1575 | -1490 |

### Retry files

Not written (pass --retry; entity ids are only valid for the running game session).

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

