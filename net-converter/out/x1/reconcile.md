# Reconciliation with the live game

Live graph `out\x1\live-graph.json`: 3353 segments, 3322 nodes. Coverage: 5 points per import edge (t = 0.1 … 0.9), each within 6 m of a live segment (chord + sagitta allowance, or the import curve when both ends coincide).

Statuses: covered = all 5 points on a live segment of the mapped prefab; wrong prefab = all 5 on some network; partial = 1–4; missing = none. Retry = missing, or partial with at most 2 points.

### After fixes (current import-*.json)

| layer | prefab | edges | covered | wrong prefab | partial | missing | → retry |
|---|---|---:|---:|---:|---:|---:|---:|
| highways | Highway Oneway - 1 lane | 283 | 280 | 0 | 3 | 0 | 3 |
| highways | Highway Oneway - 2 lanes | 956 | 896 | 2 | 31 | 27 | 52 |
| highways | Highway Oneway - 3 lanes | 565 | 564 | 0 | 1 | 0 | 1 |
| highways | Highway Oneway - 4 lanes | 32 | 31 | 1 | 0 | 0 | 0 |
| rail | Double Train Track | 496 | 495 | 0 | 1 | 0 | 0 |
| roads | Gravel Road | 1291 | 1060 | 0 | 119 | 112 | 217 |
| roads | Medium Road | 42 | 35 | 0 | 2 | 5 | 7 |
| roads | Pavement Path | 48 | 36 | 0 | 7 | 5 | 10 |
| roads | Small Road | 325 | 311 | 3 | 6 | 5 | 8 |
| **highways** | **all** | 1836 | 1771 | 3 | 35 | 27 | 56 |
| **rail** | **all** | 496 | 495 | 0 | 1 | 0 | 0 |
| **roads** | **all** | 1706 | 1442 | 3 | 134 | 127 | 242 |

### Edges not fully covered

| layer | edge | prefab | status | points mapped/any | x | z |
|---|---|---|---|---|---:|---:|
| highways | e2084_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6593 | 1294 |
| highways | e4420_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6642 | 1351 |
| highways | e6447_0a | Highway Oneway - 2 lanes | Missing | 0/0 | 4237 | 5913 |
| highways | e6447_0ba | Highway Oneway - 2 lanes | Partial | 2/2 | 4244 | 5951 |
| highways | e6456_0a | Highway Oneway - 2 lanes | Partial | 4/4 | 611 | 239 |
| highways | e7163_0 | Highway Oneway - 2 lanes | Missing | 0/0 | 3388 | 2007 |
| highways | e7697_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6692 | 1417 |
| highways | e7777_0aa | Highway Oneway - 2 lanes | Missing | 0/0 | -4395 | 33 |
| highways | e7777_0ab | Highway Oneway - 2 lanes | Missing | 0/0 | -4416 | 47 |
| highways | e7777_0baa | Highway Oneway - 2 lanes | Missing | 0/0 | -4435 | 60 |
| highways | e7777_0bab | Highway Oneway - 2 lanes | Partial | 2/2 | -4449 | 69 |
| highways | e9203_0a | Highway Oneway - 2 lanes | Partial | 3/3 | 3472 | 2056 |
| highways | e9203_0b | Highway Oneway - 2 lanes | Missing | 0/0 | 3429 | 2031 |
| highways | e9387_0ba | Highway Oneway - 2 lanes | Partial | 1/1 | -5021 | 194 |
| highways | e9387_0bb | Highway Oneway - 2 lanes | Missing | 0/0 | -5044 | 192 |
| highways | e9740_0 | Highway Oneway - 2 lanes | Partial | 1/1 | -7156 | 1829 |
| highways | e12352_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -4369 | 15 |
| highways | e13697_0a | Highway Oneway - 2 lanes | Partial | 2/2 | 3682 | 2194 |
| highways | e14049_0 | Highway Oneway - 2 lanes | Partial | 2/2 | -5065 | 190 |
| highways | e14409_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -4377 | 2 |
| highways | e15154_0 | Highway Oneway - 2 lanes | Partial | 3/3 | -6475 | 1116 |
| highways | e15276_0a | Highway Oneway - 2 lanes | Partial | 1/1 | 4230 | 5964 |
| highways | e15276_0b | Highway Oneway - 2 lanes | Missing | 0/0 | 4221 | 5916 |
| highways | e15883_0 | Highway Oneway - 4 lanes | WrongPrefab | 0/5 | 437 | -4497 |
| highways | e16239_0 | Highway Oneway - 2 lanes | Missing | 0/0 | 3396 | 1993 |
| highways | e16808_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6553 | 1237 |
| highways | e17033_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6630 | 1361 |
| highways | e17366_0 | Highway Oneway - 2 lanes | Partial | 1/1 | -7155 | 1845 |
| highways | e17815_0bb | Highway Oneway - 2 lanes | Partial | 2/2 | 280 | 162 |
| highways | e18373_0 | Highway Oneway - 1 lane | Partial | 1/1 | -4366 | 28 |
| highways | e19525_0aa | Highway Oneway - 2 lanes | Partial | 2/2 | -5047 | 176 |
| highways | e20387_0ab | Highway Oneway - 2 lanes | Partial | 1/1 | -5976 | 510 |
| highways | e20387_0ba | Highway Oneway - 2 lanes | Missing | 0/0 | -5955 | 497 |
| highways | e20387_0bb | Highway Oneway - 2 lanes | Partial | 2/2 | -5935 | 485 |
| highways | e20430_0b | Highway Oneway - 3 lanes | Partial | 2/2 | 817 | 4011 |
| highways | e21320_0aa | Highway Oneway - 2 lanes | Partial | 1/1 | -5934 | 503 |
| highways | e21320_0ab | Highway Oneway - 2 lanes | Partial | 1/1 | -5953 | 515 |
| highways | e21483_0 | Highway Oneway - 2 lanes | Missing | 0/0 | 3330 | 1955 |
| highways | e21493_0 | Highway Oneway - 2 lanes | Partial | 1/1 | -4322 | -16 |
| highways | e21812_0aa | Highway Oneway - 2 lanes | Partial | 1/1 | -4477 | 69 |
| highways | e21812_0aba | Highway Oneway - 2 lanes | Missing | 0/0 | -4457 | 56 |
| highways | e21812_0abb | Highway Oneway - 2 lanes | Missing | 0/0 | -4444 | 47 |
| highways | e21812_0ba | Highway Oneway - 2 lanes | Missing | 0/0 | -4424 | 34 |
| highways | e21812_0bb | Highway Oneway - 2 lanes | Missing | 0/0 | -4404 | 20 |
| highways | e23931_0ba | Highway Oneway - 2 lanes | WrongPrefab | 1/5 | -4537 | 128 |
| highways | e23931_0bb | Highway Oneway - 2 lanes | WrongPrefab | 3/5 | -4556 | 141 |
| highways | e24659_0a | Highway Oneway - 2 lanes | Partial | 3/3 | 4217 | 5813 |
| highways | e24659_0b | Highway Oneway - 2 lanes | Missing | 0/0 | 4228 | 5866 |
| highways | e25170_0ba | Highway Oneway - 2 lanes | Partial | 1/1 | 619 | 257 |
| highways | e25170_0bb | Highway Oneway - 2 lanes | Partial | 2/2 | 598 | 251 |
| highways | e27167_0 | Highway Oneway - 2 lanes | Partial | 2/2 | -5064 | 174 |
| highways | e27572_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6679 | 1426 |
| highways | e28824_0 | Highway Oneway - 2 lanes | Partial | 1/1 | -4330 | -30 |
| highways | e31526_0 | Highway Oneway - 1 lane | Partial | 2/2 | 248 | 153 |
| highways | e31696_0 | Highway Oneway - 2 lanes | Partial | 1/1 | -6486 | 1111 |
| highways | e32115_0 | Highway Oneway - 2 lanes | Missing | 0/0 | -6519 | 1178 |
| highways | e32444_0 | Highway Oneway - 2 lanes | Missing | 0/0 | 3322 | 1968 |
| highways | e32930_0a | Highway Oneway - 2 lanes | Partial | 1/1 | 3731 | 2209 |
| highways | e32930_0b | Highway Oneway - 2 lanes | Partial | 1/1 | 3761 | 2235 |
| highways | e32991_0 | Highway Oneway - 1 lane | Partial | 1/1 | 243 | 160 |
| highways | e34552_0aa | Highway Oneway - 2 lanes | Partial | 3/3 | -4565 | 128 |
| highways | e34552_0ab | Highway Oneway - 2 lanes | Partial | 2/2 | -4546 | 115 |
| highways | e34845_0a | Highway Oneway - 2 lanes | Missing | 0/0 | 3438 | 2018 |
| highways | e34845_0b | Highway Oneway - 2 lanes | Partial | 1/3 | 3485 | 2045 |
| highways | e35268_0a | Highway Oneway - 2 lanes | Partial | 1/1 | 4212 | 5869 |
| rail | e25368_0 | Double Train Track | Partial | 4/4 | -182 | 2388 |
| roads | e158_0 | Gravel Road | Missing | 0/0 | 5062 | -4058 |
| roads | e173_0 | Gravel Road | Missing | 0/0 | 3020 | -1955 |
| roads | e322_0 | Gravel Road | Partial | 1/1 | 2883 | -2831 |
| roads | e346_0 | Gravel Road | Missing | 0/0 | 6076 | 219 |
| roads | e389_0 | Gravel Road | Missing | 0/0 | 3288 | -3127 |
| roads | e445_0 | Gravel Road | Missing | 0/0 | 2607 | -2629 |
| roads | e632_0ba | Pavement Path | Partial | 1/1 | 1418 | 4670 |
| roads | e632_0bb | Pavement Path | Partial | 2/3 | 1437 | 4673 |
| roads | e677_0 | Gravel Road | Partial | 1/1 | 5229 | -4031 |
| roads | e744_0 | Gravel Road | Partial | 4/4 | 4184 | -3468 |
| roads | e815_0 | Gravel Road | Partial | 1/1 | 3967 | -3746 |
| roads | e1012_0 | Gravel Road | Missing | 0/0 | 1619 | 5542 |
| roads | e1036_0 | Gravel Road | Missing | 0/0 | 6532 | 903 |
| roads | e1783_0 | Gravel Road | Partial | 1/1 | 4946 | -3831 |
| roads | e1893_0 | Gravel Road | Missing | 0/0 | 897 | 5508 |
| roads | e2099_0 | Gravel Road | Partial | 1/1 | 3218 | -2166 |
| roads | e2140_0 | Gravel Road | Partial | 1/1 | 4810 | -4220 |
| roads | e2259_0 | Gravel Road | Missing | 0/0 | 3494 | -1878 |
| roads | e2380_0 | Gravel Road | Partial | 1/1 | 4365 | -3196 |
| roads | e2474_0 | Gravel Road | Partial | 1/1 | 5914 | -66 |
| roads | e2686_0 | Gravel Road | Missing | 0/0 | 5011 | -3966 |
| roads | e2803_0 | Gravel Road | Missing | 0/0 | 3921 | -4040 |
| roads | e2889_0 | Gravel Road | Missing | 0/0 | 3539 | -1627 |
| roads | e3015_0 | Gravel Road | Missing | 0/0 | 3470 | -2052 |
| roads | e3023_0 | Gravel Road | Partial | 1/1 | 5947 | 180 |
| roads | e3088_0 | Gravel Road | Missing | 0/0 | 2775 | -2967 |
| roads | e3173_0 | Gravel Road | Partial | 1/1 | 2870 | -2753 |
| roads | e3201_0 | Gravel Road | Partial | 1/1 | 3680 | -2018 |
| roads | e3261_0 | Gravel Road | Missing | 0/0 | 6332 | 1095 |
| roads | e3326_0 | Gravel Road | Partial | 1/1 | 3370 | -1937 |
| roads | e3397_0 | Gravel Road | Missing | 0/0 | -1389 | 5261 |
| roads | e3416_0 | Gravel Road | Partial | 1/1 | 6335 | 3415 |
| roads | e3430_0 | Gravel Road | Partial | 1/1 | 6360 | 3457 |
| roads | e3575_0 | Gravel Road | Missing | 0/0 | 2684 | -3103 |
| roads | e3604_0 | Gravel Road | Missing | 0/0 | 6104 | 180 |
| roads | e3659_0 | Gravel Road | Partial | 1/1 | 6160 | -108 |
| roads | e3880_0 | Gravel Road | Missing | 0/0 | 3004 | -2079 |
| roads | e3915_0 | Gravel Road | Missing | 0/0 | 3106 | -2706 |
| roads | e4043_0 | Gravel Road | Partial | 1/1 | 4206 | -3221 |
| roads | e4090_0 | Gravel Road | Partial | 1/1 | 4860 | -4331 |
| roads | e4102_0a | Gravel Road | Partial | 3/3 | 6425 | 1203 |
| roads | e4102_0ba | Gravel Road | Missing | 0/0 | 6372 | 1203 |
| roads | e4102_0bb | Gravel Road | Partial | 2/2 | 6341 | 1203 |
| roads | e4107_0 | Gravel Road | Missing | 0/0 | 4106 | -3481 |
| roads | e4177_0 | Gravel Road | Missing | 0/0 | 3333 | -2876 |
| roads | e4398_0 | Gravel Road | Missing | 0/0 | 3783 | -3885 |
| roads | e4485_0 | Pavement Path | Partial | 1/1 | 33 | -2679 |
| roads | e4605_0 | Gravel Road | Partial | 2/2 | 4014 | -3009 |
| roads | e4913_0 | Gravel Road | Partial | 2/2 | 6846 | 2654 |
| roads | e5154_0 | Gravel Road | Missing | 0/0 | 4909 | -4083 |
| roads | e5167_0 | Gravel Road | Partial | 1/1 | 4136 | -3233 |
| roads | e5501_0 | Gravel Road | Missing | 0/0 | -1294 | 5269 |
| roads | e5706_0 | Gravel Road | Partial | 1/1 | 6591 | 3240 |
| roads | e5779_0 | Gravel Road | Missing | 0/0 | 6846 | 2586 |
| roads | e5861_0 | Gravel Road | Missing | 0/0 | 6161 | 1204 |
| roads | e5934_0 | Small Road | Partial | 1/1 | 6658 | 2462 |
| roads | e5995_0 | Gravel Road | Partial | 1/1 | 6332 | 1168 |
| roads | e6055_0 | Pavement Path | Partial | 1/1 | 16 | -2686 |
| roads | e6079_0 | Gravel Road | Partial | 1/1 | 6591 | 3418 |
| roads | e6088_0 | Gravel Road | Missing | 0/0 | -778 | 4377 |
| roads | e6216_0 | Gravel Road | Partial | 1/1 | 3801 | -3773 |
| roads | e6250_0 | Gravel Road | Missing | 0/0 | 3426 | -1682 |
| roads | e6273_0 | Gravel Road | Missing | 0/0 | -2936 | 2879 |
| roads | e6492_0a | Gravel Road | Missing | 0/0 | 1329 | 6153 |
| roads | e6492_0ba | Gravel Road | Missing | 0/0 | 1360 | 6158 |
| roads | e6492_0bb | Gravel Road | Missing | 0/0 | 1378 | 6161 |
| roads | e6877_0 | Gravel Road | Missing | 0/0 | 6360 | 1003 |
| roads | e6977_0 | Gravel Road | Missing | 0/0 | 6846 | 2542 |
| roads | e7025_0 | Gravel Road | Partial | 1/1 | 6589 | 1532 |
| roads | e7126_0 | Pavement Path | Missing | 0/0 | 367 | -3024 |
| roads | e7317_0 | Gravel Road | Missing | 0/0 | 6479 | 3457 |
| roads | e7447_0 | Gravel Road | Partial | 1/1 | 3187 | -1928 |
| roads | e7556_0 | Gravel Road | Partial | 1/1 | 5190 | -3791 |
| roads | e7610_0 | Gravel Road | Missing | 0/0 | 3455 | -1641 |
| roads | e7877_0 | Gravel Road | Partial | 1/1 | 6443 | 716 |
| roads | e7885_0 | Gravel Road | Partial | 2/2 | 5915 | 36 |
| roads | e7961_0 | Gravel Road | Missing | 0/0 | 3269 | -3166 |
| roads | e7969_0 | Gravel Road | Partial | 1/1 | 3828 | -4012 |
| roads | e8335_0 | Small Road | Partial | 3/3 | 4936 | -605 |
| roads | e8537_0 | Gravel Road | Missing | 0/0 | 1631 | 5447 |
| roads | e8576_0 | Gravel Road | Partial | 1/1 | 3936 | -4132 |
| roads | e8628_0 | Gravel Road | Partial | 1/1 | 2896 | -2915 |
| roads | e8708_0 | Small Road | Missing | 0/0 | 6482 | 1935 |
| roads | e8908_0 | Gravel Road | Missing | 0/0 | 6132 | 1175 |
| roads | e9074_0 | Gravel Road | Partial | 1/1 | 950 | 4894 |
| roads | e9357_0 | Gravel Road | Partial | 1/1 | 3018 | -2162 |
| roads | e9365_0 | Gravel Road | Partial | 3/3 | 6076 | 308 |
| roads | e9656_0 | Gravel Road | Missing | 0/0 | 6296 | 1003 |
| roads | e10126_0 | Gravel Road | Partial | 1/1 | 4165 | -3195 |
| roads | e10287_0 | Gravel Road | Partial | 1/1 | 4139 | -3032 |
| roads | e10437_0 | Medium Road | Missing | 0/0 | -6274 | 710 |
| roads | e10481_0 | Gravel Road | Missing | 0/0 | 2905 | -2703 |
| roads | e10697_0 | Gravel Road | Missing | 0/0 | 5024 | -4041 |
| roads | e10807_0 | Gravel Road | Partial | 1/1 | 3133 | -2873 |
| roads | e10937_0 | Gravel Road | Missing | 0/0 | 3453 | -1848 |
| roads | e11008_0 | Gravel Road | Partial | 1/1 | 4825 | -4096 |
| roads | e11310_0 | Gravel Road | Partial | 1/1 | 3411 | -1891 |
| roads | e11608_0 | Gravel Road | Missing | 0/0 | 6503 | 1003 |
| roads | e11645_0 | Gravel Road | Missing | 0/0 | 2856 | -2667 |
| roads | e11712_0 | Gravel Road | Partial | 1/1 | 4013 | -3982 |
| roads | e12128_0 | Gravel Road | Missing | 0/0 | 2919 | -3053 |
| roads | e12353_0 | Gravel Road | Missing | 0/0 | 2993 | -2688 |
| roads | e12391_0 | Gravel Road | Partial | 1/1 | 2696 | -2979 |
| roads | e12729_0aa | Small Road | WrongPrefab | 3/5 | -3358 | -3411 |
| roads | e12804_0 | Gravel Road | Missing | 0/0 | 3130 | -2180 |
| roads | e13009_0 | Medium Road | Missing | 0/0 | -6416 | 976 |
| roads | e13132_0 | Gravel Road | Partial | 1/1 | 3622 | -1614 |
| roads | e13569_0 | Gravel Road | Partial | 4/4 | 3678 | -1739 |
| roads | e13795_0 | Gravel Road | Partial | 4/4 | 3145 | -3150 |
| roads | e13936_0 | Gravel Road | Partial | 1/1 | 4080 | -2998 |
| roads | e14549_0 | Gravel Road | Partial | 1/1 | 4779 | -4034 |
| roads | e14556_0 | Gravel Road | Partial | 1/1 | 6304 | 1203 |
| roads | e14627_0 | Pavement Path | Partial | 1/1 | 16 | -2563 |
| roads | e14701_0 | Gravel Road | Partial | 0/1 | 6846 | 2475 |
| roads | e14885_0 | Gravel Road | Missing | 0/0 | 3561 | -1867 |
| roads | e15012_0 | Gravel Road | Partial | 2/2 | 3392 | -2976 |
| roads | e15335_0 | Gravel Road | Partial | 3/3 | 6058 | -108 |
| roads | e15569_0 | Gravel Road | Partial | 1/1 | 6335 | 3230 |
| roads | e15698_0 | Gravel Road | Partial | 1/1 | 5024 | -3818 |
| roads | e15881_0 | Gravel Road | Partial | 1/1 | 4757 | -3899 |
| roads | e15919_0 | Gravel Road | Partial | 1/1 | 2620 | -2713 |
| roads | e16153_0 | Gravel Road | Partial | 1/1 | 2854 | -2954 |
| roads | e16316_0 | Gravel Road | Partial | 1/1 | 3242 | -2049 |
| roads | e16402_0 | Gravel Road | Missing | 0/0 | 3103 | -1941 |
| roads | e16621_0 | Gravel Road | Missing | 0/0 | -759 | 4422 |
| roads | e16655_0 | Gravel Road | Missing | 0/0 | 5107 | -3805 |
| roads | e16674_0 | Gravel Road | Missing | 0/0 | 1055 | 4896 |
| roads | e16793_0 | Gravel Road | Missing | 0/0 | 3890 | -4002 |
| roads | e16902_0 | Gravel Road | Partial | 1/1 | 3771 | -3810 |
| roads | e17243_0 | Gravel Road | Missing | 0/0 | 4769 | -3972 |
| roads | e17330_0 | Small Road | Partial | 4/4 | 6428 | 1856 |
| roads | e17477_0 | Small Road | Missing | 0/0 | 4698 | -729 |
| roads | e17505_0 | Small Road | Partial | 3/3 | 4770 | -679 |
| roads | e17538_0 | Gravel Road | Missing | 0/0 | 6224 | 1003 |
| roads | e17692_0 | Gravel Road | Partial | 1/1 | 6132 | 1032 |
| roads | e17921_0 | Gravel Road | Partial | 1/1 | 3664 | -1650 |
| roads | e18096_0 | Gravel Road | Partial | 1/1 | 2697 | -3186 |
| roads | e18124_0 | Gravel Road | Missing | 0/0 | 3588 | -2033 |
| roads | e18265_0 | Gravel Road | Missing | 0/0 | 6463 | 3201 |
| roads | e18414_0 | Pavement Path | Missing | 0/0 | 4785 | -696 |
| roads | e18537_0a | Small Road | Partial | 1/1 | -3313 | -3473 |
| roads | e18537_0b | Small Road | Missing | 0/0 | -3344 | -3430 |
| roads | e18960_0 | Pavement Path | Missing | 0/0 | 302 | -3028 |
| roads | e19145_0 | Gravel Road | Missing | 0/0 | 6332 | 1032 |
| roads | e19264_0 | Gravel Road | Missing | 0/0 | 2991 | -1995 |
| roads | e19284_0 | Pavement Path | Partial | 4/4 | 771 | 4830 |
| roads | e19366_0 | Gravel Road | Missing | 0/0 | 4048 | -3247 |
| roads | e19374_0 | Gravel Road | Partial | 1/1 | 5023 | -4305 |
| roads | e19778_0 | Gravel Road | Partial | 1/1 | 2906 | -2978 |
| roads | e20001_0 | Gravel Road | Missing | 0/0 | 3220 | -3138 |
| roads | e20097_0 | Gravel Road | Partial | 1/1 | 4008 | -3772 |
| roads | e20500_0 | Medium Road | Partial | 0/1 | -2432 | -606 |
| roads | e20630_0 | Gravel Road | Partial | 1/1 | 1476 | 6176 |
| roads | e20651_0 | Medium Road | Missing | 0/0 | -6383 | 907 |
| roads | e20671_0 | Gravel Road | Partial | 1/1 | 4783 | -3857 |
| roads | e20761_0 | Gravel Road | Missing | 0/0 | 4858 | -3845 |
| roads | e20766_0 | Gravel Road | Partial | 3/3 | 6447 | 3565 |
| roads | e20789_0 | Gravel Road | Missing | 0/0 | 3884 | -3760 |
| roads | e20829_0 | Gravel Road | Partial | 2/2 | 4020 | -3847 |
| roads | e20854_0 | Gravel Road | Missing | 0/0 | 3380 | -2901 |
| roads | e20871_0 | Gravel Road | Missing | 0/0 | 3529 | -2042 |
| roads | e20919_0 | Gravel Road | Partial | 3/3 | 1597 | 5733 |
| roads | e21070_0 | Gravel Road | Partial | 1/1 | 6374 | 3201 |
| roads | e21170_0 | Gravel Road | Missing | 0/0 | -745 | 4502 |
| roads | e21243_0 | Gravel Road | Missing | 0/0 | 3245 | -2891 |
| roads | e21380_0 | Gravel Road | Missing | 0/0 | 6132 | 1104 |
| roads | e21729_0g0 | Gravel Road | Partial | 2/2 | 3803 | 2914 |
| roads | e21737_0 | Gravel Road | Missing | 0/0 | 3440 | -1765 |
| roads | e21899_0 | Gravel Road | Partial | 1/1 | 3055 | -2192 |
| roads | e21937_0 | Gravel Road | Partial | 1/1 | 3385 | -2030 |
| roads | e22084_0 | Gravel Road | Missing | 0/0 | 2593 | -2546 |
| roads | e22123_0 | Gravel Road | Missing | 0/0 | 3536 | -1911 |
| roads | e22296_0 | Gravel Road | Missing | 0/0 | 3942 | -3993 |
| roads | e22902_0 | Gravel Road | Partial | 1/1 | 3372 | -3113 |
| roads | e22952_0 | Gravel Road | Missing | 0/0 | 6232 | 1203 |
| roads | e23282_0 | Gravel Road | Partial | 4/4 | 5248 | -3928 |
| roads | e23638_0 | Gravel Road | Missing | 0/0 | 4992 | -4069 |
| roads | e23722_0 | Gravel Road | Missing | 0/0 | 5032 | -4095 |
| roads | e23777_0 | Gravel Road | Missing | 0/0 | 6488 | 803 |
| roads | e23831_0 | Gravel Road | Partial | 1/1 | 6377 | 1715 |
| roads | e23972_0 | Gravel Road | Partial | 1/1 | 2670 | -3020 |
| roads | e24077_0 | Gravel Road | Missing | 0/0 | 3131 | -2666 |
| roads | e24544_0 | Gravel Road | Partial | 1/1 | 4796 | -4137 |
| roads | e24682_0 | Gravel Road | Partial | 1/1 | 4368 | -2995 |
| roads | e24887_0 | Gravel Road | Missing | 0/0 | 6447 | 3489 |
| roads | e25135_0 | Gravel Road | Missing | 0/0 | 3171 | -3110 |
| roads | e25248_0 | Gravel Road | Missing | 0/0 | 5146 | -4044 |
| roads | e25342_0 | Gravel Road | Partial | 1/1 | 2933 | -3141 |
| roads | e25389_0 | Gravel Road | Partial | 1/1 | 3144 | -2943 |
| roads | e25403_0 | Gravel Road | Partial | 1/1 | 6332 | 839 |
| roads | e25416_0 | Gravel Road | Partial | 1/1 | -771 | 4539 |
| roads | e25579_0 | Gravel Road | Partial | 4/4 | 6745 | 1411 |
| roads | e25631_0 | Pavement Path | Partial | 1/1 | 40 | -2472 |
| roads | e25671_0 | Small Road | Missing | 0/0 | 4852 | -641 |
| roads | e26134_0ba | Small Road | WrongPrefab | 3/5 | -3679 | -2969 |
| roads | e26134_0bb | Small Road | WrongPrefab | 3/5 | -3692 | -2951 |
| roads | e26192_0 | Gravel Road | Missing | 0/0 | 3068 | -2676 |
| roads | e26342_0 | Gravel Road | Partial | 2/2 | -34 | -2358 |
| roads | e26508_0 | Gravel Road | Partial | 1/1 | 6356 | 803 |
| roads | e26636_0 | Gravel Road | Missing | 0/0 | 6419 | 803 |
| roads | e26785_0 | Gravel Road | Partial | 1/1 | 3654 | -1852 |
| roads | e26880_0 | Gravel Road | Partial | 1/1 | 3283 | -3254 |
| roads | e26953_0 | Small Road | Partial | 1/1 | 6826 | 2435 |
| roads | e27008_0 | Gravel Road | Missing | 0/0 | 4935 | -4319 |
| roads | e27165_0 | Gravel Road | Missing | 0/0 | 3551 | -2003 |
| roads | e27286_0 | Medium Road | Partial | 1/1 | -2365 | -684 |
| roads | e27329_0 | Gravel Road | Partial | 1/1 | 4006 | -3497 |
| roads | e27343_0 | Gravel Road | Missing | 0/0 | 3207 | -2654 |
| roads | e27381_0 | Gravel Road | Partial | 1/1 | 3797 | -3974 |
| roads | e27569_0 | Gravel Road | Missing | 0/0 | 2802 | -2930 |
| roads | e28009_0 | Gravel Road | Missing | 0/0 | 6532 | 975 |
| roads | e28030_0 | Gravel Road | Missing | 0/0 | 6422 | 3457 |
| roads | e28238_0 | Gravel Road | Missing | 0/0 | 3370 | -2838 |
| roads | e28703_0 | Gravel Road | Missing | 0/0 | 6332 | 975 |
| roads | e28940_0 | Pavement Path | Missing | 0/0 | 335 | -3032 |
| roads | e29009_0 | Gravel Road | Partial | 4/4 | 6203 | 36 |
| roads | e29183_0 | Gravel Road | Missing | 0/0 | 2706 | -2492 |
| roads | e29226_0 | Gravel Road | Missing | 0/0 | -724 | 4463 |
| roads | e29297_0 | Gravel Road | Missing | 0/0 | 6444 | 778 |
| roads | e29617_0 | Gravel Road | Missing | 0/0 | 6332 | 911 |
| roads | e29800_0 | Gravel Road | Partial | 1/1 | 6559 | 3457 |
| roads | e29850_0 | Gravel Road | Partial | 1/1 | 6160 | 1004 |
| roads | e30028_0 | Gravel Road | Missing | 0/0 | 2733 | -2730 |
| roads | e30067_0 | Gravel Road | Partial | 1/1 | 3295 | -2639 |
| roads | e30866_0 | Gravel Road | Partial | 1/1 | 6202 | -66 |
| roads | e30913_0 | Gravel Road | Missing | 0/0 | 1608 | 5638 |
| roads | e30940_0 | Gravel Road | Partial | 1/1 | 2658 | -2743 |
| roads | e31262_0 | Gravel Road | Partial | 1/1 | 2790 | -2478 |
| roads | e31333_0 | Medium Road | Missing | 0/0 | -6314 | 774 |
| roads | e31425_0 | Pavement Path | Missing | 0/0 | 43 | -2515 |
| roads | e31556_0 | Gravel Road | Partial | 1/1 | 3716 | -1969 |
| roads | e31605_0 | Gravel Road | Missing | 0/0 | 6591 | 3329 |
| roads | e31665_0 | Gravel Road | Missing | 0/0 | 6432 | 1003 |
| roads | e31667_0 | Gravel Road | Partial | 1/1 | 3344 | -2675 |
| roads | e31675_0 | Gravel Road | Partial | 2/2 | 5044 | -4169 |
| roads | e32055_0 | Gravel Road | Partial | 1/1 | 3961 | -3261 |
| roads | e32109_0 | Gravel Road | Partial | 1/1 | 6552 | 3201 |
| roads | e32124_0 | Gravel Road | Missing | 0/0 | 6532 | 831 |
| roads | e32134_0 | Gravel Road | Missing | 0/0 | 2821 | -2716 |
| roads | e32350_0 | Gravel Road | Partial | 2/2 | 1306 | 6146 |
| roads | e32433_0 | Gravel Road | Partial | 1/1 | 4995 | -3866 |
| roads | e32648_0 | Gravel Road | Partial | 1/1 | 3415 | -2061 |
| roads | e32673_0 | Gravel Road | Missing | 0/0 | 3358 | -2763 |
| roads | e32973_0 | Gravel Road | Missing | 0/0 | 4285 | -3208 |
| roads | e32978_0 | Medium Road | Missing | 0/0 | -6349 | 840 |
| roads | e32994_0 | Gravel Road | Partial | 1/1 | 2842 | -2579 |
| roads | e33438_0 | Gravel Road | Missing | 0/0 | 3157 | -3026 |
| roads | e33942_0 | Gravel Road | Partial | 4/4 | 894 | 4896 |
| roads | e34111_0 | Gravel Road | Missing | 0/0 | 6037 | 180 |
| roads | e34336_0 | Gravel Road | Partial | 1/1 | 2788 | -2847 |
| roads | e34354_0 | Gravel Road | Missing | 0/0 | 3119 | -2789 |
| roads | e34422_0 | Gravel Road | Missing | 0/0 | 2623 | -2505 |
| roads | e34922_0 | Gravel Road | Partial | 1/1 | 3701 | -1877 |
| roads | e34946_0 | Gravel Road | Partial | 1/1 | 5232 | -3828 |
| roads | e35264_0a | Gravel Road | Missing | 0/0 | 6432 | 1715 |
| roads | e35264_0b | Gravel Road | Partial | 1/1 | 6467 | 1715 |
| roads | e35592_0 | Gravel Road | Partial | 3/3 | 6588 | 1439 |
| roads | e36006_0 | Gravel Road | Missing | 0/0 | 6335 | 3313 |
| roads | e36406_0 | Gravel Road | Partial | 1/1 | 3227 | -1957 |
| roads | e36458_0 | Gravel Road | Partial | 1/1 | 3170 | -2903 |
| roads | e36527_0 | Gravel Road | Partial | 1/1 | 6174 | 180 |
| roads | e36563_0 | Gravel Road | Partial | 4/4 | 5535 | 820 |
| roads | e36621_0 | Small Road | Missing | 0/0 | 6752 | 2447 |
| roads | e36675_0 | Gravel Road | Missing | 0/0 | 4153 | -3120 |

### Retry files

Not written (pass --retry; entity ids are only valid for the running game session).

## Converter fixes (round 2)

- Hairpins (Bézier length > 1.6 × chord, self-intersection, or a control point behind its end), checked on every imported CS1 segment: **0** found, 0 rebuilt.
- Exact duplicate edges dropped: **0**.
- Structure now from node elevation (not the B/E/S/T prefab suffix): 371 imported edges differ from the suffix-based class (Ground→Elevated 352, Ground→Tunnel 13, Elevated→Ground 6).
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

