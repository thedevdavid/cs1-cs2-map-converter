# Falls steps: CS1 terrain under each object vs its top

The steps lie across the river channel; stamped as terrain they would dam it. Use this table to cap each step at the water line. The CS1 water surface is not in this data: it comes from DakotaExport v3 `waterSurface` once that export has run. Heights in metres (CS1 world y).

| id | base prefab | position x, z | terrain under min / median / max | object bottom | object top | top − median terrain | decision |
|---:|---|---|---|---:|---:|---:|---|
| 28 | `2479784985.VC_Rocky_Streambed` | 357, 920 | 129.5 / 133.4 / 141.9 | 119.2 | 155.4 | 22.0 | kept |
| 30 | `2479784985.VC_Rocky_Streambed` | 640, 888 | 129.5 / 135.2 / 146.0 | 118.2 | 154.4 | 19.2 | kept |
| 32 | `2724100634.VC Erosion Rock temp` | 503, 808 | 133.3 / 137.6 / 140.2 | 127.3 | 221.3 | 83.7 | removed: spike |
| 33 | `2724100634.VC Erosion Rock temp` | 579, 822 | 130.5 / 135.3 / 140.2 | 124.3 | 218.2 | 82.9 | removed: spike |
| 47 | `2469281223.VC rounded rock` | 541, 837 | 131.4 / 138.3 / 140.2 | 129.0 | 147.6 | 9.4 | kept |
| 48 | `2469281223.VC rounded rock` | 478, 833 | 130.8 / 137.0 / 140.2 | 129.0 | 147.6 | 10.6 | kept |
| 52 | `2469281223.VC domerock` | 703, 685 | 131.1 / 138.3 / 143.8 | 129.5 | 167.5 | 29.2 | kept |
| 55 | `2469281223.VC domerock` | 569, 888 | 133.3 / 138.4 / 140.2 | 129.5 | 167.5 | 29.2 | kept |
| 57 | `2469281223.VC rock ledge` | 709, 996 | 132.9 / 145.2 / 152.3 | 133.6 | 157.7 | 12.4 | kept |
| 58 | `2469281223.VC rock ledge` | 290, 752 | 130.5 / 139.2 / 145.5 | 133.6 | 157.7 | 18.5 | kept |
| 65 | `1981694823.Mossy Grey Rock NC Single 04` | 450, 673 | 113.5 / 136.4 / 140.0 | 117.2 | 148.2 | 11.8 | kept |
| 66 | `1981694823.Mossy Grey Rock NC Single 03` | 459, 727 | 129.5 / 131.4 / 140.0 | 110.7 | 134.8 | 3.4 | kept |
| 67 | `1981694823.Mossy Grey Rock NC Single 03` | 463, 661 | 109.3 / 136.7 / 140.0 | 118.1 | 142.1 | 5.4 | kept |
| 68 | `1981694823.Mossy Grey Rock NC Single 22` | 466, 652 | 100.1 / 128.2 / 139.8 | 43.4 | 144.3 | 16.1 | kept |
| 69 | `1981694823.Mossy Grey Rock NC Single 03` | 594, 656 | 116.7 / 137.5 / 139.5 | 118.1 | 142.1 | 4.7 | kept |
| 70 | `1981694823.Mossy Grey Rock NC Single 03` | 551, 735 | 123.8 / 132.7 / 140.2 | 110.7 | 134.8 | 2.1 | kept |
| 71 | `1981694823.Mossy Grey Rock NC Single 18` | 663, 637 | 124.0 / 131.6 / 138.2 | 117.0 | 134.7 | 3.0 | kept |
| 72 | `1981694823.Mossy Grey Rock NC Single 04` | 402, 685 | 117.4 / 132.6 / 137.1 | 105.0 | 136.0 | 3.5 | kept |
| 74 | `1981694823.Mossy Grey Rock NC Single 22` | 582, 655 | 106.9 / 135.4 / 139.2 | 43.4 | 144.3 | 8.9 | kept |
| 75 | `1981694823.Mossy Grey Rock NC Single 22` | 404, 600 | 113.0 / 131.0 / 140.1 | 43.4 | 144.3 | 13.3 | kept |
| 79 | `1981694823.Mossy Grey Rock NC Single 22` | 679, 554 | 125.6 / 131.0 / 135.0 | 43.4 | 144.3 | 13.3 | kept |
| 80 | `1981694823.Mossy Grey Rock NC Single 22` | 598, 637 | 114.0 / 134.2 / 139.2 | 43.4 | 144.3 | 10.1 | kept |
| 81 | `1981694823.Mossy Grey Rock NC Single 07` | 666, 531 | 123.6 / 131.2 / 136.2 | 109.8 | 133.2 | 2.0 | kept |
| 82 | `1981694823.Mossy Grey Rock NC Single 20` | 658, 529 | 122.2 / 130.4 / 135.8 | 96.0 | 120.9 | -9.5 | kept |
| 83 | `1981694823.Mossy Grey Rock NC Single 03` | 408, 568 | 112.3 / 124.6 / 134.9 | 102.2 | 126.2 | 1.6 | kept |
| 84 | `1981694823.Mossy Grey Rock NC Single 03` | 407, 538 | 105.4 / 124.9 / 135.1 | 102.2 | 126.2 | 1.3 | kept |
| 85 | `1981694823.Mossy Grey Rock NC Single 20` | 647, 515 | 100.9 / 129.7 / 135.9 | 96.0 | 120.9 | -8.9 | kept |
| 86 | `1981694823.Mossy Grey Rock NC Single 04` | 390, 622 | 129.4 / 134.8 / 141.3 | 117.2 | 148.2 | 13.4 | kept |
| 87 | `1981694823.Mossy Grey Rock NC Single 04` | 374, 668 | 132.0 / 133.7 / 138.4 | 117.2 | 148.2 | 14.5 | kept |
| 88 | `1981694823.Mossy Grey Rock NC Single 21` | 529, 662 | 101.3 / 128.5 / 130.5 | 90.9 | 139.6 | 11.1 | kept |
| 105 | `1981694823.Mossy Grey Rock NC Single 10` | 628, 733 | 131.7 / 131.9 / 132.8 | 130.8 | 140.6 | 8.6 | kept |
| 106 | `1981694823.Mossy Grey Rock NC Single 11` | 581, 746 | 131.3 / 131.6 / 132.1 | 130.4 | 139.7 | 8.2 | kept |
| 107 | `1981694823.Mossy Grey Rock NC Single 11` | 576, 751 | 131.1 / 131.3 / 131.6 | 130.4 | 139.7 | 8.4 | kept |
| 108 | `1981694823.Mossy Grey Rock NC Single 06` | 473, 718 | 130.9 / 132.0 / 133.2 | 127.8 | 141.7 | 9.8 | kept |
| 109 | `1981694823.Mossy Grey Rock NC Single 06` | 458, 716 | 131.7 / 132.5 / 134.0 | 127.8 | 141.7 | 9.2 | kept |
| 110 | `1981694823.Mossy Grey Rock NC Single 14` | 503, 728 | 129.9 / 130.2 / 130.8 | 126.4 | 138.2 | 8.0 | kept |
| 111 | `1981694823.Mossy Grey Rock NC Single 14` | 511, 725 | 129.8 / 130.1 / 130.4 | 126.4 | 138.2 | 8.1 | kept |
| 119 | `1981694823.Mossy Grey Rock NC Single 08` | 461, 635 | 99.9 / 105.0 / 139.5 | 93.2 | 115.1 | 10.1 | kept |
| 120 | `1981694823.Mossy Grey Rock NC Single 08` | 477, 629 | 99.7 / 104.2 / 138.8 | 93.2 | 115.1 | 11.0 | kept |
| 121 | `1981694823.Mossy Grey Rock NC Single 09` | 585, 619 | 97.5 / 118.6 / 138.1 | 92.5 | 114.0 | -4.6 | kept |
| 122 | `1981694823.Mossy Grey Rock NC Single 09` | 569, 636 | 97.1 / 112.0 / 137.9 | 92.5 | 114.0 | 2.0 | kept |
| 127 | `1981694823.Mossy Grey Rock NC Single 03` | -6607, 1120 | 248.5 / 254.8 / 274.1 | 231.5 | 255.5 | 0.8 | kept |
| 128 | `1981694823.Mossy Grey Rock NC Single 18` | -6585, 1148 | 252.8 / 254.7 / 256.4 | 239.1 | 256.7 | 2.0 | kept |
| 129 | `1981694823.Mossy Grey Rock NC Single 03` | -6605, 1094 | 241.7 / 254.8 / 271.5 | 231.6 | 255.7 | 1.0 | kept |
| 130 | `1981694823.Mossy Grey Rock NC Single 03` | -6587, 1174 | 246.6 / 255.1 / 263.3 | 231.6 | 255.7 | 0.7 | kept |
| 131 | `1981694823.Mossy Grey Rock NC Single 22` | -6577, 1091 | 229.5 / 250.0 / 254.7 | 197.9 | 266.7 | 16.7 | kept |
| 132 | `1981694823.Mossy Grey Rock NC Single 22` | -6572, 1142 | 227.4 / 254.5 / 254.7 | 194.3 | 263.1 | 8.5 | kept |
| 133 | `1981694823.Mossy Grey Rock NC Single 22` | -6581, 1094 | 229.6 / 254.1 / 254.7 | 194.3 | 263.1 | 8.9 | kept |
| 134 | `1981694823.Mossy Grey Rock NC Single 21` | -6585, 1042 | 223.3 / 242.4 / 254.2 | 198.7 | 258.0 | 15.6 | kept |
| 135 | `1981694823.Mossy Grey Rock NC Single 05` | -6614, 1046 | 232.1 / 246.5 / 257.6 | 203.5 | 264.4 | 17.9 | kept |
| 136 | `1981694823.Mossy Grey Rock NC Single 05` | -6540, 1163 | 227.9 / 256.1 / 266.5 | 176.6 | 265.9 | 9.8 | kept |
| 137 | `1981694823.Mossy Grey Rock NC Single 04` | -6528, 1146 | 222.6 / 245.5 / 258.4 | 195.4 | 262.5 | 16.9 | kept |
| 138 | `1981694823.Mossy Grey Rock NC Single 03` | -6629, 1187 | 254.7 / 255.0 / 275.6 | 233.0 | 257.1 | 2.1 | kept |
| 139 | `1981694823.Mossy Grey Rock NC Single 20` | -6669, 1194 | 255.3 / 272.5 / 294.8 | 243.7 | 268.6 | -3.9 | kept |
| 140 | `1981694823.Mossy Grey Rock NC Single 20` | -6644, 1169 | 256.8 / 270.8 / 286.8 | 243.7 | 268.6 | -2.2 | kept |
| 141 | `1981694823.Mossy Grey Rock NC Single 22` | -6619, 1067 | 247.6 / 259.7 / 266.6 | 207.7 | 276.4 | 16.7 | kept |
| 147 | `1981694823.Mossy Grey Rock NC Single 17` | -6606, 1209 | 254.7 / 255.6 / 257.2 | 253.5 | 258.7 | 3.1 | kept |
| 148 | `1981694823.Mossy Grey Rock NC Single 12` | -6629, 1206 | 254.7 / 254.7 / 254.8 | 252.5 | 260.7 | 6.0 | kept |
| 149 | `1981694823.Mossy Grey Rock NC Single 12` | -6607, 1154 | 254.7 / 254.7 / 255.0 | 252.5 | 260.7 | 6.0 | kept |
| 150 | `1981694823.Mossy Grey Rock NC Single 12` | -6608, 1208 | 254.7 / 255.0 / 256.5 | 252.5 | 260.7 | 5.7 | kept |
| 151 | `1981694823.Mossy Grey Rock NC Single 17` | -6599, 1164 | 254.7 / 254.8 / 254.9 | 253.5 | 258.7 | 4.0 | kept |
| 156 | `2479784985.VC_Rocky_Streambed` | -6658, 1258 | 254.7 / 256.9 / 270.6 | 250.1 | 264.9 | 8.0 | kept |
| 157 | `2479784985.VC_Rocky_Streambed` | -6696, 1312 | 254.7 / 259.0 / 271.1 | 250.1 | 264.9 | 5.8 | kept |
| 410 | `2469281223.VC domerock` | -6651, 1315 | 254.7 / 264.8 / 280.0 | 250.2 | 288.2 | 23.4 | kept |
| 411 | `2469281223.VC domerock` | -6629, 1282 | 254.7 / 262.7 / 277.8 | 250.2 | 288.2 | 25.5 | kept |
| 548 | `1981694823.Mossy Grey Rock NC Single 11` | -6633, 1139 | 255.4 / 263.3 / 274.7 | 248.9 | 274.7 | 11.4 | kept |
| 549 | `1981694823.Mossy Grey Rock NC Single 09` | -6627, 1107 | 255.9 / 263.8 / 274.2 | 249.4 | 270.9 | 7.2 | kept |
| 582 | `1981694823.Mossy Grey Rock NC Single 08` | -6462, 1150 | 272.3 / 282.1 / 294.3 | 258.5 | 280.4 | -1.7 | kept |
