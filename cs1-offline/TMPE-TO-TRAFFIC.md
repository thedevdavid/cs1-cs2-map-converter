# TM:PE (CS1) → Traffic (CS2, Paradox Mods 80095)

Can the map's TM:PE settings be reused in CS2 through [Traffic](https://mods.paradoxplaza.com/mods/80095/Windows) (krzychu124, source: https://github.com/krzychu124/Traffic, checked at commit ce1551f, 2026-09-14)? **Yes for everything the map actually uses, but only through an importer that does not exist yet**: Traffic has no file import.

## What the map has (`out/tmpe-summary.md`, `out/tmpe-offline.json`)

| TM:PE feature | entries | Traffic equivalent |
|---|---:|---|
| Lane connections | 68 at 26 nodes | **Lane Connector** (`GeneratedConnection`) |
| Lane arrows | 39 at 15 nodes (36 Forward, 3 Right) | none as such; expressible as lane connections (allow only the targets in the arrow directions) |
| Priority signs, timed lights, toggled lights, speed limits, vehicle restrictions, parking restrictions, junction restrictions | 0 | (Traffic has priorities: RightOfWay / Yield / Stop) |

All 68 connections are on the USRP interstates: lane drops, merges and ramp splits (for example 3L → 2L, ramp → 2L). 14 of the 15 arrow nodes also have connections. Every entry is resolved to segment, lane index, node and world position: TM:PE stores CS1 lane ids, so `src/tmpe-offline.cs` decodes the lane table from the map's NetManager data. All 5 582 node positions match the dump exactly, and 4 879 of 4 881 lane chains have the prefab's lane count.

## How Traffic stores the data (CS2 ECS, saved in the map/save)

- On the **node** entity: tag `ModifiedConnections` and buffer `ModifiedLaneConnections { laneIndex, carriagewayAndGroup, lanePosition, edgeEntity, modifiedConnections }`, one per modified source lane.
- Per source lane, a separate entity with `DataOwner(node)`, `PrefabRef(fake prefab)` and buffer `GeneratedConnection { sourceEntity, targetEntity (edges), laneIndexMap, carriagewayAndGroupIndexMap, lanePositionMap, method (PathMethod Road/Track), isUnsafe }`.
- Priorities: buffer `LanePriority { laneIndex (lane, group, carriageway), priority, isEnd }` with tag `ModifiedPriorities`.
- Traffic's own `TLEDataMigrationSystem` builds exactly these entities from another mod's data (Traffic Lights Enhancement lane directions), so writing them from outside is a supported pattern.
- Caveats from Traffic's README:
  - Custom connections reset when the road composition changes or when Road Builder touches the road.
  - Intersections with custom connections don't regenerate when a road is added later.
  - The map editor is "not tested thoroughly", though settings are expected to save into the map.

## Import path

1. Import the network first (net-converter + bridge). The bridge's import report maps CS1 segment ids (`e<segment>_…`) to CS2 edges, and CS1 node ids to CS2 nodes.
2. Run the importer after the network has settled. It would be a small CS2 mod referencing `Traffic.dll`, or an extension of the cs2mcp bridge. Per node in `tmpe-offline.json`:
   - find the CS2 node;
   - for each connection, pick the CS2 edge for the source and target segment;
   - pick the CS2 sub-lane whose end position is nearest to `source.endPosition` / `target.endPosition`. CS1 and CS2 lane indices differ, so match by position, not by index.
3. Create the components above, following `TLEDataMigrationSystem.MigrateCustomLaneDirectionsJob`.
4. Turn lane arrows into connections: for the arrow lane, keep only the targets whose turn direction is in the arrow set.
5. Save the map, reload it, and check the intersections in Traffic's Lane Connector.

Risks:
- Traffic's internals (component layouts, `DataMigrationVersion`) can change between releases, so pin the Traffic version the importer was built against.
- CS2 interstate junction geometry differs from CS1, so some lane counts won't line up. Report those instead of guessing.
- 68 connections is small. Re-creating them by hand in Traffic's Lane Connector, with `tmpe-summary.md` as the checklist, is a realistic fallback (26 intersections).
