# Interchange 5 (3493, 2152): CS1 vs A-v8 per highway segment (round 32)

CS1 height = Bézier midpoint above the mean node terrain; CS1 node elevation and flags from the dump (the USRP 'E' prefabs carry the bridge AI); live = A-v8 live-graph-20261021-000201-end.json, elevation per end. Rows: CS1 segments more than 2 m up or with an elevated prefab.

through-road bearings (deg, mod 180, weighted m): [["60",1170.5392141],["50",541.9566092900001],["40",499.29635463],["170",401.4233413],["160",384.3607321]]
CS1 > 6 m above terrain: 32; live ≥ 8 m (pillars) 29, 0.5–8 m (wall band) 2, ground 1

| CS1 segment | prefab | CS1 height above terrain | CS1 node elevation | CS1 node flags | live (elevation per end) |
| 33741 | USRP 1L Interstate Ramp E | 27.4 | 0/0 |  | 65463:3 el 28.8/29.2 |
| 4813 | USRP 1L Interstate Ramp E | 25 | 0/0 |  | 62923:5 el 26.9/27.0 |
| 15344 | USRP 1L Interstate Ramp E | 24.1 | 0/0 |  | 65470:9 el 24.9/24.8 |
| 36686 | USRP 1L Interstate Ramp E | 20.9 | 0/0 |  | 64205:43 el 21.3/21.6 |
| 22886 | USRP 1L Interstate Ramp E | 18.8 | 0/0 |  | 65704:7 el 18.0/17.6 |
| 25769 | USRP 1L Interstate Ramp E | 18.6 | 0/0 |  | 66805:11 el 19.0/18.2 |
| 17372 | USRP 1L Interstate Ramp E | 17.8 | 0/0 |  | 66129:15 el 15.8/16.5 |
| 36781 | USRP 1L Interstate Ramp E | 16.6 | 0/0 |  | 64217:5 el 20.7/20.6 |
| 28071 | USRP 1L Interstate Ramp E | 15 | 0/0 |  | 64713:9 el 15.8/15.6 |
| 5188 | USRP 1L Interstate Ramp E | 14.6 | 0/0 |  | 62947:5 el 10.6/11.0 |
| 6801 | USRP 2L Interstate E | 14.2 | 0/0 |  | 63738:3 el 13.7/13.8 |
| 29775 | USRP 2L Interstate E | 14.1 | 0/0 |  | 65498:5 el 14.5/14.3 |
| 6041 | USRP 1L Interstate Ramp E | 13.2 | 0/13 |  | 63443:7 el 14.3/14.1 |
| 14225 | USRP 1L Interstate Ramp E | 12.5 | 0/0 |  | 64515:7 el 12.9/13.1 |
| 1387 | USRP 2L Interstate E | 11.8 | 0/9 |  | 61617:3 el 13.6/13.1 |
| 21512 | USRP 2L Interstate E | 11.8 | 0/0 |  | 66676:7 el 12.1/11.7 |
| 17669 | USRP 2L Interstate E | 11.6 | 0/0 |  | 66178:19 el 11.7/11.6 |
| 32650 | USRP 1L Interstate Ramp E | 11.6 | 13/9 |  | 64355:35 el 13.2/13.2 |
| 10276 | USRP 1L Interstate Ramp E | 10.6 | 0/0 |  | 64878:11 el 10.7/10.4 |
| 3638 | USRP 1L Interstate Ramp E | 10.2 | 0/0 |  | 62405:3 el 8.5/8.5 |
| 8322 | USRP 1L Interstate Ramp E | 10.1 | 0/0 |  | 64125:7 el 10.2/10.8 |
| 13697 | USRP 2L Interstate E | 10.1 | 0/0 | OnGround | 65340:13 el 14.4/14.8 |
| 31228 | USRP 1L Interstate Ramp E | 10.1 | 0/0 |  | 65957:15 el 10.4/9.9 |
| 10176 | USRP 1L Interstate Ramp E | 9.5 | 0/0 |  | 64875:11 el 8.7/8.6 |
| 19926 | USRP 1L Interstate Ramp E | 9.1 | 0/0 |  | 65976:15 el 9.7/9.8 |
| 4284 | USRP 1L Interstate Ramp E | 8.8 | 0/0 |  | 62902:5 el 10.6/11.6 |
| 20616 | USRP 1L Interstate Ramp E | 8.2 | 0/0 |  | 66555:21 el 9.1/9.3 |
| 12097 | USRP 1L Interstate Ramp E | 7.8 | 0/0 |  | 64960:9 el 7.7/7.3 |
| 33311 | USRP 1L Interstate Ramp E | 7.3 | 0/0 | OnGround | 65299:3 el 10.5/10.0 |
| 26285 | USRP 1L Interstate Ramp E | 7 | 0/0 |  | 66928:15 el 8.0/8.0 |
| 11507 | USRP 1L Interstate Ramp E | 6.6 | 0/5 | OnGround | 65187:7 el 6.7/6.5 |
| 36861 | USRP 1L Interstate Ramp E | 6.1 | 0/0 | OnGround | 64226:37 el  |
| 4510 | USRP 1L Interstate Ramp E | 6 | 0/0 | OnGround | 62975:5 el 6.9/7.3 |
| 34083 | USRP 2L Interstate E | 5.9 | 0/9 | OnGround | 65773:13 el  |
| 9026 | USRP 1L Interstate Ramp E | 5.7 | 0/0 | OnGround | 64723:9 el 6.4/5.8 |
| 21043 | USRP 1L Interstate Ramp E | 5.5 | 0/0 |  | 66597:9 el 6.1/6.0 |
| 26105 | USRP 1L Interstate Ramp E | 4.9 | 0/0 | OnGround | 66672:3 el  |
| 18529 | USRP 1L Interstate Ramp E | 4.8 | 0/0 | OnGround | 65940:21 el 8.0/8.0 |
| 22005 | USRP 1L Interstate Ramp E | 4.8 | 0/0 |  | 65060:11 el 8.1/8.1 |
| 9203 | USRP 2L Interstate E | 4.7 | 0/0 | OnGround | 64275:11 el  |
| 34845 | USRP 2L Interstate E | 4.7 | 0/0 | OnGround | 66183:13 el  |
| 20675 | USRP 1L Interstate Ramp E | 4.5 | 0/0 | OnGround | 66579:5 el 5.3/5.9 |
| 27973 | USRP 1L Interstate Ramp E | 4.5 | 0/0 | OnGround | 65258:3 el  |
| 1638 | USRP 1L Interstate Ramp E | 4.3 | 0/0 | OnGround | 61720:3 el  |
| 14428 | USRP 1L Interstate Ramp E | 4.2 | 0/0 | OnGround | 65384:3 el 5.4/5.0 |
| 21987 | USRP 1L Interstate Ramp | 2.9 | 5/5 | OnGround | 65054:9 el  |
| 8270 | USRP 2L Interstate | 2.8 | 0/0 | OnGround | 64083:5 el  |
| 546 | USRP 1L Interstate Ramp | 2.5 | 0/0 | OnGround | 61500:1 el  |
| 27960 | USRP 1L Interstate Ramp E | 2.5 | 0/0 | OnGround | 65246:11 el  |
| 18755 | USRP 1L Interstate Ramp E | 2.2 | 0/0 | OnGround | 65416:13 el  |
| 35872 | USRP 1L Interstate Ramp | 2.2 | 0/0 | OnGround | 64986:5 el  |
| 36808 | USRP 1L Interstate Ramp E | 2.2 | 0/0 | OnGround | 64220:15 el  |
| 19546 | USRP 1L Interstate Ramp E | 1.8 | 0/0 | OnGround | 65594:3 el  |

## After round 32 (canonical dump, flyover lift + clearance guard + elevated flag)

ELEVATED-FLAG 77 of 178 highway edges in the 900 m box carry "elevated": true:
e1387_0 (CS1 1387), e1638_0 (CS1 1638), e3638_0 (CS1 3638), e4284_0a (CS1 4284), e4284_0b (CS1 4284), e4510_0 (CS1 4510), e4813_0a (CS1 4813), e4813_0b (CS1 4813), e5188_0a (CS1 5188), e5188_0b (CS1 5188), e6041_0a (CS1 6041), e6041_0b (CS1 6041), e6801_0a (CS1 6801), e6801_0b (CS1 6801), e8322_0 (CS1 8322), e9026_0a (CS1 9026), e9026_0b (CS1 9026), e9203_0a (CS1 9203), e9203_0b (CS1 9203), e10176_0 (CS1 10176), e10276_0 (CS1 10276), e11507_0 (CS1 11507), e12097_0 (CS1 12097), e13697_0g0a (CS1 13697), e13697_0g0b (CS1 13697), e13697_0g1 (CS1 13697), e14225_0 (CS1 14225), e14428_0 (CS1 14428), e15344_0a (CS1 15344), e15344_0b (CS1 15344), e17372_0a (CS1 17372), e17372_0b (CS1 17372), e17669_0a (CS1 17669), e17669_0b (CS1 17669), e18529_0g0 (CS1 18529), e18529_0g1 (CS1 18529), e18755_0 (CS1 18755), e19546_0 (CS1 19546), e19926_0 (CS1 19926), e20616_0 (CS1 20616), e20675_0 (CS1 20675), e21043_0g0 (CS1 21043), e21043_0g1 (CS1 21043), e21512_0a (CS1 21512), e21512_0b (CS1 21512), e22005_0a (CS1 22005), e22005_0b (CS1 22005), e22886_0a (CS1 22886), e22886_0b (CS1 22886), e25769_0 (CS1 25769), e26105_0a (CS1 26105), e26105_0b (CS1 26105), e26285_0g0 (CS1 26285), e26285_0g1 (CS1 26285), e27960_0 (CS1 27960), e27973_0a (CS1 27973), e27973_0b (CS1 27973), e28071_0 (CS1 28071), e29775_0 (CS1 29775), e31228_0a (CS1 31228), e31228_0b (CS1 31228), e32650_0 (CS1 32650), e33311_0a (CS1 33311), e33311_0b (CS1 33311), e33741_0g0 (CS1 33741), e33741_0g1 (CS1 33741), e34083_0a (CS1 34083), e34083_0b (CS1 34083), e34845_0a (CS1 34845), e34845_0b (CS1 34845), e36686_0a (CS1 36686), e36686_0b (CS1 36686), e36781_0a (CS1 36781), e36781_0b (CS1 36781), e36808_0 (CS1 36808), e36861_0a (CS1 36861), e36861_0b (CS1 36861)

CROSSINGS 38 (no shared node) between highway courses in the box; Δy ours (x1) min 7.0; under 6 m: 0; CS1 under 6 m: 0

| at | upper | lower | ours Δy | CS1 Δy |
|---|---|---|---:|---:|
| (3646, 2130) | e6041_0b 1 lane y 191.4 | e12097_0 1 lane y 184.4 | 7.0 | 7.7 |
| (3643, 2141) | e1387_0 2 lanes y 191.8 | e12097_0 1 lane y 184.5 | 7.4 | 7.6 |
| (3634, 2157) | e13697_0g0b 2 lanes y 192.3 | e12097_0 1 lane y 184.6 | 7.6 | 7.0 |
| (3523, 2133) | e33741_0g0 1 lane y 201.0 | e17372_0b 1 lane y 193.1 | 7.9 | 6.6 |
| (3663, 2054) | e3638_0 1 lane y 188.9 | e14428_0 1 lane y 180.8 | 8.2 | 8.7 |
| (3624, 2167) | e12097_0 1 lane y 184.7 | e15892_0 1 lane y 176.5 | 8.2 | 9.0 |
| (3540, 2096) | e36686_0a 1 lane y 193.9 | e17669_0a 2 lanes y 185.7 | 8.2 | 8.1 |
| (3532, 1978) | e18529_0g1 1 lane y 181.9 | e1223_0 1 lane y 173.7 | 8.2 | 6.8 |
| (3633, 2287) | e22005_0a 1 lane y 187.3 | e17760_0 2 lanes y 178.9 | 8.4 | 9.3 |
| (3558, 1987) | e21043_0g0 1 lane y 181.5 | e19817_0 2 lanes y 173.1 | 8.4 | 7.0 |
| (3551, 2084) | e36686_0b 1 lane y 194.1 | e21512_0b 2 lanes y 185.7 | 8.4 | 8.5 |
| (3525, 2052) | e26285_0g1 1 lane y 180.9 | e19647_0 2 lanes y 172.3 | 8.5 | 8.2 |
| (3509, 2100) | e19926_0 1 lane y 181.1 | e21530_0 2 lanes y 171.8 | 9.3 | 9.7 |
| (3501, 2135) | e20616_0 1 lane y 180.9 | e21530_0 2 lanes y 171.4 | 9.5 | 9.7 |
| (3564, 2076) | e36686_0b 1 lane y 194.2 | e10276_0 1 lane y 183.9 | 10.4 | 10.4 |
| (3493, 1971) | e5188_0b 1 lane y 189.3 | e5138_0 1 lane y 178.8 | 10.4 | 10.4 |
| (3518, 2161) | e17372_0b 1 lane y 191.6 | e4510_0 1 lane y 180.9 | 10.7 | 10.8 |
| (3520, 2066) | e21512_0a 2 lanes y 183.6 | e19647_0 2 lanes y 172.2 | 11.4 | 11.7 |
| (3652, 2099) | e28071_0 1 lane y 194.5 | e14428_0 1 lane y 183.0 | 11.5 | 11.9 |
| (3529, 2117) | e36686_0a 1 lane y 193.5 | e19926_0 1 lane y 181.9 | 11.6 | 11.6 |
| (3680, 2095) | e31228_0a 1 lane y 191.1 | e7896_0 2 lanes y 179.3 | 11.8 | 11.9 |
| (3515, 2081) | e17669_0b 2 lanes y 184.0 | e21530_0 2 lanes y 172.0 | 12.1 | 12.4 |
| (3670, 2153) | e32650_0 1 lane y 192.5 | e26593_0 2 lanes y 179.6 | 12.8 | 13.2 |
| (3669, 2160) | e1387_0 2 lanes y 192.6 | e26593_0 2 lanes y 179.7 | 12.9 | 13.3 |
| (3478, 2022) | e25769_0 1 lane y 193.1 | e36808_0 1 lane y 180.1 | 13.0 | 15.3 |
| (3478, 2041) | e25769_0 1 lane y 194.5 | e34845_0b 2 lanes y 181.2 | 13.3 | 13.8 |
| (3665, 2181) | e13697_0g0b 2 lanes y 193.2 | e26593_0 2 lanes y 179.8 | 13.4 | 13.6 |
| (3631, 2119) | e6041_0b 1 lane y 190.0 | e15892_0 1 lane y 176.4 | 13.7 | 14.4 |
| (3481, 2061) | e4813_0b 1 lane y 196.0 | e9203_0a 2 lanes y 181.7 | 14.3 | 14.3 |
| (3630, 2132) | e29775_0 2 lanes y 190.9 | e15892_0 1 lane y 176.4 | 14.5 | 15.2 |
| (3627, 2152) | e6801_0a 2 lanes y 191.3 | e15892_0 1 lane y 176.5 | 14.8 | 15.4 |
| (3676, 2119) | e28071_0 1 lane y 194.6 | e26593_0 2 lanes y 179.4 | 15.2 | 15.5 |
| (3480, 2005) | e5188_0a 1 lane y 191.8 | e843_0 1 lane y 175.9 | 15.9 | 16.2 |
| (3657, 2216) | e36781_0a 1 lane y 197.0 | e18908_0 2 lanes y 180.0 | 17.0 | 17.9 |
| (3491, 2086) | e4813_0a 1 lane y 197.9 | e26105_0a 1 lane y 180.8 | 17.2 | 18.1 |
| (3630, 2084) | e22886_0b 1 lane y 194.4 | e34570_0 1 lane y 176.7 | 17.8 | 18.4 |
| (3616, 2197) | e36781_0b 1 lane y 197.9 | e5294_0 1 lane y 177.2 | 20.7 | 21.9 |
| (3506, 2112) | e33741_0g1 1 lane y 199.9 | e21530_0 2 lanes y 171.6 | 28.2 | 28.6 |

LEVELS at the centre (≤ 200 m): heights of crossing courses 171, 172, 173, 174, 176, 177, 179, 180, 181, 182, 183, 184, 185, 186, 187, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 200, 201

LEVELS at the centre (≤ 200 m): heights of crossing courses 171, 172, 173, 174, 176, 177, 179, 180, 181, 182, 183, 184, 185, 186, 187, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 200, 201
