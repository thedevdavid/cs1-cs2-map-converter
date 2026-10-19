# Objects to place by hand

Positions in CS2 metres (y = CS1 y × 1); rotation = yaw degrees, 0 = +z, clockwise from above (sign unverified).

| CS1 prefab | source | x | y | z | rotation | why |
|---|---|---:|---:|---:|---:|---|
| Dam Electricity_Data | building 7278 | 1556.2 | 165.6 | 4679.1 | 261.7 | CS2 hydro dam (Hydroelectric_Power_Plant_01 Dam) is placed across the river by hand |
| Hernando-Desoto Bridge_Data | building 9019 | 498.3 | 135.5 | 214.9 | 164.4 | CS1 bridge model placed as a building; in CS2 build TrussArchBridge01 / ExtradosedBridge01 along the road instead |
| Dam Electricity_Data | building 23159 | 1546.7 | 165.6 | 4677.8 | 261.7 | CS2 hydro dam (Hydroelectric_Power_Plant_01 Dam) is placed across the river by hand |
| Dam Electricity_Data | building 32789 | 1543.3 | 166.2 | 4701.0 | 261.7 | CS2 hydro dam (Hydroelectric_Power_Plant_01 Dam) is placed across the river by hand |
| Dam Electricity_Data | building 35536 | 1551.8 | 166.0 | 4702.2 | 261.7 | CS2 hydro dam (Hydroelectric_Power_Plant_01 Dam) is placed across the river by hand |
| Detroit Bridge_Data | building 42069 | 1313.7 | 90.1 | -3786.3 | 217.2 | CS1 bridge model placed as a building; in CS2 build TrussArchBridge01 / ExtradosedBridge01 along the road instead |
| Detroit Bridge_Data | building 47598 | 1751.2 | 90.1 | -3209.8 | 37.2 | CS1 bridge model placed as a building; in CS2 build TrussArchBridge01 / ExtradosedBridge01 along the road instead |

## Monorail arch bridge below the dam (decorative)

192 CS1 monorail segments (2.91 km) form the white multi-arch "vanilla asset bridge": x 944…1345, z 4061…4290, y 139…159 m (CS1; × 1 here).

They are written as a decorative network in import-decor-monorail.json (Arc Pedestrian Bridge, exact elevations). For a closer look in CS2, build the crossing by hand as **Tied Arch Bridge - 4 lanes** or **Truss Arch Bridge - 4 lanes** (Bridges & Ports) along the same line, or place several **Arc Pedestrian Bridge** spans.

## Air and ship connections

Emitted as objects in import-connections.json at the CS1 position clamped to the border; place by hand if the bridge cannot.

| id | prefab | x | z | rotation | note |
|---|---|---:|---:|---:|---|
| oc_plane_10384 | Airplane Outside Connection | -272 | -7168 | 182 | CS1 Airplane Connection at (-272, -8653) clamped to the border; may need hand placement |
| oc_plane_16610 | Airplane Outside Connection | 7168 | 1647 | 77 | CS1 Airplane Connection at (8653, 1647) clamped to the border; may need hand placement |
| oc_ship_19314 | Ship Outside Connection - Twoway | 4718 | -7168 | 146 | on the seaway border end c33538_0, facing outward (CS1 Ship Connection at (5736, -8653)); drop this object if the seaway spawns its own marker (NOTES.md item 6) |
| oc_plane_22143 | Airplane Outside Connection | -2561 | 7168 | 340 | CS1 Airplane Connection at (-2561, 8653) clamped to the border; may need hand placement |
| oc_plane_36525 | Airplane Outside Connection | -3263 | 7168 | 336 | CS1 Airplane Connection at (-3263, 8653) clamped to the border; may need hand placement |
