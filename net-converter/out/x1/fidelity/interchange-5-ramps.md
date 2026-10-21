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
