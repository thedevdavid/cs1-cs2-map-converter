# Fidelity check: CS1 vs live (live-graph-20261021-144500-end.json) vs our files

Each image: CS1 segments wide and pale (blue = elevated), our import files green, the live game thin red (dashed = elevated); orange = a CS1 segment with no live
segment within 2 m of its midpoint, magenta = a live segment with no CS1 segment within 2 m. Hover a line for its id and prefab.

| image | verdict |
|---|---|
| [Interchange 1 at (-602, -1295), 10 highway junction nodes](interchange_01_-602_-1295.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 2 at (179, 101), 6 highway junction nodes](interchange_02_179_101.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 3 at (3929, 4552), 4 highway junction nodes](interchange_03_3929_4552.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 4 at (-267, 3858), 2 highway junction nodes](interchange_04_-267_3858.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 5 at (3493, 2152), 15 highway junction nodes](interchange_05_3493_2152.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 6 at (-1675, -1796), 6 highway junction nodes](interchange_06_-1675_-1796.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 7 at (252, 3431), 2 highway junction nodes](interchange_07_252_3431.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 8 at (662, -4494), 4 highway junction nodes](interchange_08_662_-4494.md) | CHECK: 6 CS1 segments without live counterpart, 8 live without CS1 source, 0 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 9 at (2872, 913), 5 highway junction nodes](interchange_09_2872_913.md) | CHECK: 3 CS1 segments without live counterpart, 2 live without CS1 source, 0 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 10 at (682, -5037), 2 highway junction nodes](interchange_10_682_-5037.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 11 at (-4155, -75), 6 highway junction nodes](interchange_11_-4155_-75.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 2 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 12 at (735, 3953), 2 highway junction nodes](interchange_12_735_3953.md) | CHECK: 2 CS1 segments without live counterpart, 1 live without CS1 source, 3 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 13 at (4063, 5043), 2 highway junction nodes](interchange_13_4063_5043.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Interchange 14 at (-6463, 1078), 1 highway junction nodes](interchange_14_-6463_1078.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 9 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 15 at (-6198, 627), 1 highway junction nodes](interchange_15_-6198_627.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 8 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Interchange 16 at (3015, 1505), 1 highway junction nodes](interchange_16_3015_1505.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 1 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR001 and its approaches at (1532, -3499)](bridge_BR001_1532_-3499.md) | CHECK: 0 CS1 segments without live counterpart (+26 replaced by a bridge override), 8 live without CS1 source, 0 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR005 and its approaches at (435, 197)](bridge_BR005_435_197.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR006 and its approaches at (-3525, -3181)](bridge_BR006_-3525_-3181.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 1 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR007 and its approaches at (778, 2666)](bridge_BR007_778_2666.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR009 and its approaches at (-4480, 80)](bridge_BR009_-4480_80.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 3 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR010 and its approaches at (-325, -2733)](bridge_BR010_-325_-2733.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR012 and its approaches at (-425, -5618)](bridge_BR012_-425_-5618.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR015 and its approaches at (-278, -6619)](bridge_BR015_-278_-6619.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR017 and its approaches at (-444, -3489)](bridge_BR017_-444_-3489.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR019 and its approaches at (-1553, 4393)](bridge_BR019_-1553_4393.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 4 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR020 and its approaches at (1300, 6149)](bridge_BR020_1300_6149.md) | CHECK: 0 CS1 segments without live counterpart (+2 replaced by a bridge override), 1 live without CS1 source, 2 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR023 and its approaches at (6729, 2369)](bridge_BR023_6729_2369.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 7 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR026 and its approaches at (6635, 2384)](bridge_BR026_6635_2384.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 7 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR033 and its approaches at (6540, 2399)](bridge_BR033_6540_2399.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 8 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR035 and its approaches at (225, 2767)](bridge_BR035_225_2767.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR038 and its approaches at (6824, 2354)](bridge_BR038_6824_2354.md) | CHECK: 3 CS1 segments without live counterpart, 0 live without CS1 source, 7 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR042 and its approaches at (3742, 2843)](bridge_BR042_3742_2843.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR044 and its approaches at (3775, 2919)](bridge_BR044_3775_2919.md) | OK: layout and heights match CS1 (2 m in plan, 4 m in height) |
| [Bridge BR049 and its approaches at (6847, 3303)](bridge_BR049_6847_3303.md) | CHECK: 1 CS1 segments without live counterpart, 0 live without CS1 source, 12 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR050 and its approaches at (6786, 3457)](bridge_BR050_6786_3457.md) | CHECK: 0 CS1 segments without live counterpart, 0 live without CS1 source, 12 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
| [Bridge BR053 and its approaches at (6846, 2586)](bridge_BR053_6846_2586.md) | CHECK: 2 CS1 segments without live counterpart, 1 live without CS1 source, 7 more than 4 m off the CS1 height, 0 live dead ends CS1 did not have |
