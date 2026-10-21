# Round 4 converter passes

- Crossings between layers without a shared node: 116. Upper course raised at the crossing (≥ 8 m, CS1 deck height): 48 nodes inserted, 17 ramp nodes adjusted. Already separated: 64. At grade in CS1: 0.
- Bridge profiles: 100 chains of CS1 bridge/elevated pieces, 9 changed, 16 nodes moved (max 3.3 m); grade flips above 1 % removed; single humps kept.
- Twisted courses (control point behind its end or self-intersection): 0; 0 fixed by swapping the control points, 0 made straight.
- Tiny diverge stubs (< 10 m, junction to dead end) collapsed: 0.
- Highway ramp tails snapped onto the course beside them: 0.
- Transitions (no node in (−8, 0) m, portal over one course; no deck node in (0, 2) m beside a raised node): 32 portal nodes set to 0, 0 tunnel nodes set to −12 m, 72 low nodes set to 0, 53 set to 2 m, 5 tunnel nodes capped so depth grows at most 15 % from the portal.
- Heights: CS2 = 2 × CS1 (Height Scale 4096 with the 2048 m heightmap). import files use heightMode "absolute": every node has y; ground nodes also carry elevation 0.

## Crossings

| upper | prefab | lower | prefab | x | z | CS1 Δy m | CS2 Δelev before | upper elev after | action |
|---|---|---|---|---:|---:|---:|---:|---:|---|
| e5616_0 | Highway Oneway - 1 lane | e33002_0 | Highway Oneway - 2 lanes | -1798 | -1818 | 13.7 | 15.9 | 15.9 | separated |
| e15887_0 | Highway Oneway - 1 lane | e26294_0 | Highway Oneway - 2 lanes | -1724 | -1795 | 28.1 | 25.2 | 25.2 | separated |
| e12830_0 | Gravel Road | e10505_0 | Highway Oneway - 3 lanes | -1555 | 4402 | 21.9 | 19.5 | 19.5 | separated |
| e26236_0 | Gravel Road | e36046_0 | Highway Oneway - 3 lanes | -1550 | 4376 | 22.3 | 18.4 | 18.4 | separated |
| e15_0 | Highway Oneway - 2 lanes | e24921_0 | Small Road | -1523 | -1533 | 18.9 | 18.1 | 18.1 | separated |
| e20664_0 | Highway Oneway - 2 lanes | e24921_0 | Small Road | -1512 | -1544 | 18.6 | 17.7 | 17.7 | separated |
| e27153_0 | Double Train Track | e11658_0 | Highway Oneway - 2 lanes | -359 | -2952 | 24.3 | 19.5 | 19.5 | separated |
| e10855_0 | Double Train Track | e23716_0 | Highway Oneway - 2 lanes | -351 | -2900 | 29.6 | 28.7 | 28.7 | separated |
| e30000_0 | Highway Oneway - 1 lane | e16293_0 | Highway Oneway - 1 lane | 181 | 3643 | 24.2 | 21.1 | 21.1 | separated |
| e17314_0 | Highway Oneway - 2 lanes | e2435_0 | Highway Oneway - 2 lanes | 606 | -4739 | 18.5 | 18.1 | 18.1 | separated |
| e21099_0 | Highway Oneway - 2 lanes | e2435_0 | Highway Oneway - 2 lanes | 606 | -4739 | 18.5 | 18.1 | 18.1 | separated |
| e17314_0 | Highway Oneway - 2 lanes | e28152_0 | Highway Oneway - 2 lanes | 606 | -4739 | 18.5 | 18.1 | 18.1 | separated |
| e25034_0 | Highway Oneway - 2 lanes | e33431_0 | Highway Oneway - 2 lanes | 640 | -4681 | 38.3 | 36.9 | 36.9 | separated |
| e12655_0 | Highway Oneway - 2 lanes | e19107_0 | Highway Oneway - 2 lanes | 652 | -4655 | 25.6 | 31.1 | 38.8 | separated |
| e33321_0 | Highway Oneway - 2 lanes | e33431_0 | Highway Oneway - 2 lanes | 656 | -4686 | 38.3 | 36.6 | 36.6 | separated |
| e29970_0 | Highway Oneway - 2 lanes | e19107_0 | Highway Oneway - 2 lanes | 663 | -4669 | 25.0 | 30.8 | 38.7 | separated |
| e14527_0 | Highway Oneway - 2 lanes | e23738_0 | Highway Oneway - 2 lanes | 665 | -4807 | 15.3 | 15.7 | 15.7 | separated |
| e14182_0 | Highway Oneway - 2 lanes | e33431_0 | Highway Oneway - 2 lanes | 668 | -4690 | 38.0 | 35.0 | 35.0 | separated |
| e14182_0 | Highway Oneway - 2 lanes | e19107_0 | Highway Oneway - 2 lanes | 672 | -4682 | 24.3 | 28.6 | 36.8 | separated |
| e4315_0 | Highway Oneway - 2 lanes | e10312_0 | Gravel Road | 1455 | 4344 | 20.7 | 19.6 | 19.6 | separated |
| e36304_0 | Highway Oneway - 2 lanes | e10312_0 | Gravel Road | 1463 | 4330 | 20.7 | 19.5 | 19.5 | separated |
| e28282_0 | Highway Oneway - 3 lanes | e27902_0 | Double Train Track | 2005 | -2863 | 56.6 | 56.5 | 56.5 | separated |
| e26537_0 | Highway Oneway - 3 lanes | e28879_0 | Double Train Track | 2018 | -2872 | 56.0 | 55.9 | 55.9 | separated |
| e20380_0 | Highway Oneway - 1 lane | e4709_0 | Highway Oneway - 2 lanes | 3007 | 1114 | 28.6 | 24.9 | 24.9 | separated |
| e25769_0 | Highway Oneway - 1 lane | e36808_0 | Highway Oneway - 1 lane | 3478 | 2022 | 30.7 | 29.3 | 35.1 | separated |
| e25769_0 | Highway Oneway - 1 lane | e34845_0 | Highway Oneway - 2 lanes | 3478 | 2041 | 27.5 | 25.7 | 39.2 | separated |
| e5188_0 | Highway Oneway - 1 lane | e843_0 | Highway Oneway - 1 lane | 3480 | 2005 | 32.4 | 32.3 | 31.9 | separated |
| e4813_0 | Highway Oneway - 1 lane | e9203_0 | Highway Oneway - 2 lanes | 3481 | 2061 | 28.5 | 27.6 | 44.1 | separated |
| e4813_0 | Highway Oneway - 1 lane | e26105_0 | Highway Oneway - 1 lane | 3491 | 2086 | 36.1 | 34.9 | 50.1 | separated |
| e5188_0 | Highway Oneway - 1 lane | e5138_0 | Highway Oneway - 1 lane | 3493 | 1971 | 20.8 | 22.5 | 27.3 | separated |
| e20616_0 | Highway Oneway - 1 lane | e21530_0 | Highway Oneway - 2 lanes | 3501 | 2135 | 19.3 | 17.0 | 17.0 | separated |
| e33741_0 | Highway Oneway - 1 lane | e21530_0 | Highway Oneway - 2 lanes | 3506 | 2112 | 57.2 | 56.4 | 56.4 | separated |
| e19926_0 | Highway Oneway - 1 lane | e21530_0 | Highway Oneway - 2 lanes | 3509 | 2100 | 19.4 | 17.8 | 17.8 | separated |
| e17669_0 | Highway Oneway - 2 lanes | e21530_0 | Highway Oneway - 2 lanes | 3515 | 2081 | 24.8 | 21.7 | 21.7 | separated |
| e17372_0 | Highway Oneway - 1 lane | e4510_0 | Highway Oneway - 1 lane | 3518 | 2161 | 21.8 | 19.9 | 36.3 | separated |
| e21512_0 | Highway Oneway - 2 lanes | e19647_0 | Highway Oneway - 2 lanes | 3520 | 2066 | 23.4 | 21.1 | 21.1 | separated |
| e36686_0 | Highway Oneway - 1 lane | e19926_0 | Highway Oneway - 1 lane | 3529 | 2117 | 23.2 | 23.8 | 42.2 | separated |
| e36686_0 | Highway Oneway - 1 lane | e17669_0 | Highway Oneway - 2 lanes | 3540 | 2096 | 16.3 | 17.1 | 41.6 | separated |
| e36686_0 | Highway Oneway - 1 lane | e21512_0 | Highway Oneway - 2 lanes | 3551 | 2084 | 16.9 | 17.6 | 41.3 | separated |
| e36686_0 | Highway Oneway - 1 lane | e10276_0 | Highway Oneway - 1 lane | 3564 | 2076 | 20.9 | 20.2 | 40.9 | separated |
| e36781_0 | Highway Oneway - 1 lane | e5294_0 | Highway Oneway - 1 lane | 3616 | 2197 | 43.8 | 40.2 | 40.2 | separated |
| e12097_0 | Highway Oneway - 1 lane | e15892_0 | Highway Oneway - 1 lane | 3624 | 2167 | 18.1 | 16.3 | 16.3 | separated |
| e6801_0 | Highway Oneway - 2 lanes | e15892_0 | Highway Oneway - 1 lane | 3627 | 2152 | 30.7 | 29.3 | 29.3 | separated |
| e29775_0 | Highway Oneway - 2 lanes | e15892_0 | Highway Oneway - 1 lane | 3630 | 2132 | 30.4 | 28.4 | 28.4 | separated |
| e22886_0 | Highway Oneway - 1 lane | e34570_0 | Highway Oneway - 1 lane | 3630 | 2084 | 36.7 | 35.8 | 35.8 | separated |
| e6041_0 | Highway Oneway - 1 lane | e15892_0 | Highway Oneway - 1 lane | 3631 | 2119 | 28.7 | 26.2 | 26.2 | separated |
| e28071_0 | Highway Oneway - 1 lane | e14428_0 | Highway Oneway - 1 lane | 3652 | 2099 | 23.9 | 24.0 | 33.1 | separated |
| e36781_0 | Highway Oneway - 1 lane | e18908_0 | Highway Oneway - 2 lanes | 3657 | 2216 | 35.8 | 29.4 | 29.4 | separated |
| e3638_0 | Highway Oneway - 1 lane | e14428_0 | Highway Oneway - 1 lane | 3663 | 2054 | 17.5 | 17.1 | 20.4 | separated |
| e13697_0 | Highway Oneway - 2 lanes | e26593_0 | Highway Oneway - 2 lanes | 3665 | 2181 | 27.2 | 20.0 | 20.0 | separated |
| e1387_0 | Highway Oneway - 2 lanes | e26593_0 | Highway Oneway - 2 lanes | 3669 | 2160 | 26.6 | 23.0 | 23.0 | separated |
| e32650_0 | Highway Oneway - 1 lane | e26593_0 | Highway Oneway - 2 lanes | 3670 | 2153 | 26.4 | 23.2 | 23.2 | separated |
| e28071_0 | Highway Oneway - 1 lane | e26593_0 | Highway Oneway - 2 lanes | 3676 | 2119 | 31.0 | 29.0 | 29.0 | separated |
| e31228_0 | Highway Oneway - 1 lane | e7896_0 | Highway Oneway - 2 lanes | 3680 | 2095 | 23.9 | 21.0 | 21.0 | separated |
| e28777_0 | Double Train Track | e2899_0 | Highway Oneway - 3 lanes | 3733 | 2844 | 28.7 | 24.0 | 24.0 | separated |
| e16183_0 | Double Train Track | e30536_0 | Highway Oneway - 3 lanes | 3753 | 2841 | 28.5 | 24.3 | 24.3 | separated |
| e24955_0 | Highway Oneway - 2 lanes | e13292_0 | Highway Oneway - 2 lanes | 3875 | 4699 | 16.9 | 16.1 | 16.1 | separated |
| e27946_0 | Highway Oneway - 3 lanes | e29051_0 | Small Road | 6586 | 2244 | 21.0 | 20.7 | 20.7 | separated |
| e11716_0 | Highway Oneway - 3 lanes | e29051_0 | Small Road | 6590 | 2270 | 21.0 | 20.3 | 20.3 | separated |
| e4424_0 | Double Train Track | e12868_0 | Small Road | 6608 | 2389 | 24.7 | 23.0 | 23.0 | separated |
| e9294_0 | Double Train Track | e1430_0 | Highway Oneway - 2 lanes | -109 | -539 | 20.8 | 25.8 | 0.0 | separated (lower course sunk) |
| e9294_0 | Double Train Track | e24715_0 | Highway Oneway - 2 lanes | -101 | -564 | 19.3 | 25.0 | 0.0 | separated (lower course sunk) |
| e22913_0 | Gravel Road | e28255_0 | Double Train Track | 3838 | -4225 | 43.3 | 40.2 | 0.0 | separated (lower course sunk) |
| e3336_0 | Gravel Road | e23907_0 | Double Train Track | 3921 | -4280 | 42.7 | 42.4 | 0.0 | separated (lower course sunk) |
| e568_0 | Highway Oneway - 2 lanes | e9848_0 | Medium Road | -4239 | -85 | 13.5 | 3.3 | 16.0 | upper raised at the crossing |
| e6254_0 | Highway Oneway - 2 lanes | e9848_0 | Medium Road | -4231 | -72 | 13.5 | 2.6 | 16.0 | upper raised at the crossing |
| e3118_0 | Highway Oneway - 1 lane | e29315_0 | Medium Road | -4224 | -60 | 13.5 | 4.3 | 16.0 | upper raised at the crossing |
| e30217_0 | Highway Oneway - 1 lane | e26294_0 | Highway Oneway - 2 lanes | -1747 | -1778 | 14.4 | 12.1 | 16.0 | upper raised at the crossing |
| e15887_0 | Highway Oneway - 1 lane | e30823_0 | Highway Oneway - 1 lane | -1731 | -1800 | 13.7 | 12.6 | 27.4 | upper raised at the crossing |
| e1808_0 | Highway Oneway - 1 lane | e26294_0 | Highway Oneway - 2 lanes | -1729 | -1792 | 14.7 | 12.1 | 16.0 | upper raised at the crossing |
| e26452_0 | Highway Oneway - 1 lane | e20364_0 | Highway Oneway - 2 lanes | -1685 | -1825 | 15.3 | 12.7 | 16.0 | upper raised at the crossing |
| e30706_0 | Double Train Track | e25337_0 | Highway Oneway - 3 lanes | -1441 | 4331 | 24.3 | 3.5 | 16.0 | upper raised at the crossing |
| e30706_0 | Double Train Track | e19619_0 | Highway Oneway - 3 lanes | -1429 | 4302 | 24.2 | 3.3 | 16.0 | upper raised at the crossing |
| e25462_0 | Highway Oneway - 1 lane | e970_0 | Highway Twoway - 4 lanes | -572 | -1317 | 13.7 | 9.5 | 16.0 | upper raised at the crossing |
| e23601_0 | Highway Oneway - 2 lanes | e970_0 | Highway Twoway - 4 lanes | -566 | -1328 | 14.0 | 9.5 | 16.0 | upper raised at the crossing |
| e9348_0 | Highway Oneway - 2 lanes | e13504_0 | Highway Twoway - 4 lanes | -558 | -1341 | 13.9 | 9.4 | 16.0 | upper raised at the crossing |
| e4316_0 | Highway Oneway - 1 lane | e13504_0 | Highway Twoway - 4 lanes | -552 | -1352 | 13.5 | 9.3 | 16.0 | upper raised at the crossing |
| e14974_0 | Highway Oneway - 2 lanes | e35381_0 | Highway Oneway - 1 lane | 77 | 3755 | 13.6 | 14.8 | 16.0 | upper raised at the crossing |
| e21918_0 | Highway Oneway - 1 lane | e501_0 | Highway Oneway - 1 lane | 90 | 43 | 15.3 | 8.7 | 16.0 | upper raised at the crossing |
| e21918_0 | Highway Oneway - 1 lane | e13450_0 | Highway Oneway - 1 lane | 93 | 42 | 15.3 | 8.7 | 16.0 | upper raised at the crossing |
| e5049_0 | Highway Oneway - 2 lanes | e23062_0 | Highway Oneway - 1 lane | 104 | 3734 | 13.9 | 14.1 | 16.0 | upper raised at the crossing |
| e21918_0 | Highway Oneway - 1 lane | e25609_0 | Highway Oneway - 1 lane | 107 | 36 | 15.3 | 8.8 | 16.0 | upper raised at the crossing |
| e21918_0 | Highway Oneway - 1 lane | e3735_0 | Highway Oneway - 1 lane | 125 | 34 | 13.4 | 8.9 | 16.0 | upper raised at the crossing |
| e14932_0 | Double Train Track | e7388_0 | Highway Oneway - 2 lanes | 217 | 2768 | 22.2 | 5.1 | 17.8 | upper raised at the crossing |
| e22426_0 | Highway Oneway - 1 lane | e3338_0 | Highway Oneway - 1 lane | 226 | 131 | 13.1 | 6.1 | 17.4 | upper raised at the crossing |
| e22426_0 | Highway Oneway - 1 lane | e32991_0 | Highway Oneway - 1 lane | 227 | 160 | 13.1 | 3.8 | 18.7 | upper raised at the crossing |
| e22426_0 | Highway Oneway - 1 lane | e31526_0 | Highway Oneway - 1 lane | 229 | 148 | 13.1 | 4.8 | 18.1 | upper raised at the crossing |
| e14932_0 | Double Train Track | e12489_0 | Highway Oneway - 2 lanes | 232 | 2765 | 21.6 | 4.9 | 17.7 | upper raised at the crossing |
| e33976_0 | Highway Oneway - 1 lane | e59_0 | Highway Oneway - 2 lanes | 245 | 3728 | 13.5 | 13.4 | 36.0 | upper raised at the crossing |
| e7553_0 | Highway Oneway - 1 lane | e26235_0 | Highway Oneway - 2 lanes | 299 | 3749 | 14.3 | 14.1 | 36.5 | upper raised at the crossing |
| e19107_0 | Highway Oneway - 2 lanes | e33431_0 | Highway Oneway - 2 lanes | 676 | -4692 | 14.3 | 8.3 | 16.0 | upper raised at the crossing |
| e22671_0 | Highway Oneway - 2 lanes | e28339_0 | Double Train Track | 2500 | 977 | 23.4 | 8.5 | 16.0 | upper raised at the crossing |
| e5111_0 | Highway Oneway - 2 lanes | e28339_0 | Double Train Track | 2502 | 962 | 23.9 | 8.9 | 16.0 | upper raised at the crossing |
| e7805_0 | Highway Oneway - 3 lanes | e16419_0 | Double Train Track | 2505 | 344 | 23.1 | 2.5 | 16.3 | upper raised at the crossing |
| e28200_0 | Highway Oneway - 3 lanes | e16419_0 | Double Train Track | 2509 | 372 | 22.3 | 2.2 | 16.0 | upper raised at the crossing |
| e31840_0 | Highway Oneway - 1 lane | e20735_0 | Highway Oneway - 1 lane | 2964 | 1104 | 15.3 | 14.5 | 21.6 | upper raised at the crossing |
| e20380_0 | Highway Oneway - 1 lane | e3183_0 | Highway Oneway - 2 lanes | 2980 | 1113 | 15.7 | 14.4 | 23.9 | upper raised at the crossing |
| e20380_0 | Highway Oneway - 1 lane | e29767_0 | Highway Oneway - 2 lanes | 2999 | 1115 | 15.3 | 14.6 | 25.7 | upper raised at the crossing |
| e29767_0 | Highway Oneway - 2 lanes | e4709_0 | Highway Oneway - 2 lanes | 3007 | 1132 | 13.5 | 9.8 | 16.0 | upper raised at the crossing |
| e23484_0 | Highway Oneway - 2 lanes | e4709_0 | Highway Oneway - 2 lanes | 3008 | 1172 | 13.5 | 10.1 | 16.0 | upper raised at the crossing |
| e33741_0 | Highway Oneway - 1 lane | e17372_0 | Highway Oneway - 1 lane | 3523 | 2133 | 13.1 | 12.5 | 58.1 | upper raised at the crossing |
| e26285_0 | Highway Oneway - 1 lane | e19647_0 | Highway Oneway - 2 lanes | 3525 | 2052 | 16.5 | 14.8 | 16.0 | upper raised at the crossing |
| e18529_0 | Highway Oneway - 1 lane | e1223_0 | Highway Oneway - 1 lane | 3532 | 1978 | 13.6 | 11.2 | 16.0 | upper raised at the crossing |
| e21043_0 | Highway Oneway - 1 lane | e19817_0 | Highway Oneway - 2 lanes | 3558 | 1987 | 14.0 | 12.4 | 16.0 | upper raised at the crossing |
| e22005_0 | Highway Oneway - 1 lane | e17760_0 | Highway Oneway - 2 lanes | 3633 | 2287 | 18.6 | 15.4 | 16.0 | upper raised at the crossing |
| e13697_0 | Highway Oneway - 2 lanes | e12097_0 | Highway Oneway - 1 lane | 3634 | 2157 | 14.0 | 12.9 | 31.1 | upper raised at the crossing |
| e1387_0 | Highway Oneway - 2 lanes | e12097_0 | Highway Oneway - 1 lane | 3643 | 2141 | 15.1 | 14.8 | 29.8 | upper raised at the crossing |
| e6041_0 | Highway Oneway - 1 lane | e12097_0 | Highway Oneway - 1 lane | 3646 | 2130 | 15.3 | 14.9 | 28.9 | upper raised at the crossing |
| e21729_0 | Gravel Road | e18318_0 | Highway Oneway - 3 lanes | 3766 | 2920 | 13.4 | 7.0 | 16.0 | upper raised at the crossing |
| e21729_0 | Gravel Road | e9761_0 | Highway Oneway - 3 lanes | 3786 | 2917 | 13.2 | 6.9 | 16.0 | upper raised at the crossing |
| e18898_0 | Highway Oneway - 2 lanes | e13068_0 | Highway Oneway - 1 lane | 3874 | 4716 | 17.1 | 15.0 | 16.0 | upper raised at the crossing |
| e18898_0 | Highway Oneway - 2 lanes | e7959_0 | Highway Oneway - 2 lanes | 3876 | 4739 | 15.8 | 13.3 | 16.0 | upper raised at the crossing |
| e24955_0 | Highway Oneway - 2 lanes | e34091_0 | Highway Oneway - 1 lane | 3879 | 4676 | 16.3 | 11.1 | 16.0 | upper raised at the crossing |
| e21897_0 | Highway Oneway - 2 lanes | e11287_0 | Highway Oneway - 1 lane | 4016 | 4861 | 14.2 | 11.1 | 16.0 | upper raised at the crossing |
| e21897_0 | Highway Oneway - 2 lanes | e35667_0 | Highway Oneway - 2 lanes | 4030 | 4854 | 14.9 | 11.8 | 16.0 | upper raised at the crossing |
| e21897_0 | Highway Oneway - 2 lanes | e26454_0 | Highway Oneway - 1 lane | 4044 | 4841 | 15.6 | 12.6 | 16.0 | upper raised at the crossing |

