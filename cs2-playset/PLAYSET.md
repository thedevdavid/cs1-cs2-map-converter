# CS2 playset: DakotaFalls-E2C

**Status (2026-10-19):** You created the playset by hand in Skyve. It is active and synced (local id `dc6468a3-0cba-4b9f-9378-9c870be083f8`, Paradox playset 12050958). It holds the 5 base mods. The 14 asset packs below are **not added yet**: Skyve.exe was running, and Skyve's sync overwrites direct edits to `playset_config.json` (tried on 2026-10-18, playset dropped). Add them in Skyve.

## Base mods (in the playset)

| id | Mod | Installed | Why |
|---|---|---|---|
| 140919 | Earth2Cities – Real World Cities Import [alpha] | 140919_13 | requested |
| 74604 | Anarchy | 74604_40 | requested |
| 74417 | Unified Icon Library | 74417_17 | declared dependency of Anarchy |
| 74539 | Image Overlay | 74539_20 | requested |
| 74285 | Scene Explorer | 74285_22 | requested |

## Asset packs to add (compiled .cok, no EAI, no dependencies)

Prefab names and footprints are in [ASSET_PACKS.md](ASSET_PACKS.md).

| id | Pack | Installed | Used for |
|---|---|---|---|
| 140176 | RZ Rocks | 140176_3 | rocks / boulders |
| 146553 | Arid Rocks | 146553_1 | small–large rocks |
| 137334 | Cliffs and Rocks Pack | 137334_5 | cliffs, big boulders |
| 132403 | pd Long Grass | 132403_3 | tall grass |
| 134056 | Asset-Dependency-Pack (Styx's) | 134056_15 | grass tufts (Grass01–06) |
| 146871 | Ferns | 146871_4 | ferns |
| 159309 | Leafy Undergrowth (Light) | 159309_1 | undergrowth |
| 159384 | Leafy Undergrowth (Dark) | 159384_1 | undergrowth |
| 134147 | PD-WildFlower | 134147_2 | wildflowers |
| 132771 | pd Bloom | 132771_3 | flowers |
| 148077 | Dirty Surfaces Pack | 148077_4 | dirt/soil/sand/stone area surfaces |
| 144834 | Terrain Textures Pack: Rocks | 144834_3 | rock, pebble and riverbed surfaces |
| 144550 | Terrain Textures Pack - Grass | 144550_2 | grass surfaces |
| 143898 | Terrain Textures Pack - Ground | 143898_4 | ground, mud and dirt surfaces |

No compiled dirt, mud or tyre-mark **decals** exist in lessgo-withEAI; all of them need EAI. The four surface packs stand in for them.

**Steps in Skyve:** select the DakotaFalls-E2C playset → Mods (or the Add/Subscribe view) → search each id above → add and enable it → Skyve syncs the playset. All 14 are already installed, so nothing downloads.

Do not edit `playset_config.json` by hand while Skyve or the game can sync. `add-playset.mjs` is kept only as a record of the failed direct-write attempt.

## Activate / switch back
- **In game:** Paradox Mods → Playsets → DakotaFalls-E2C → Activate. Use the same path with **lessgo-withEAI** to return.
- **In Skyve:** use the playset dropdown at the top left.

## Notes
- **Local mods:** CS2MCP lives in the legacy `...\Cities Skylines II\Mods\` folder and loads with every playset.
- **Regenerating the asset list:** `node scan-assets.mjs > scan.json && node make-asset-doc.mjs scan.json`. This rewrites ASSET_PACKS.md and LESSGO_INVENTORY.md.
- **Backup:** `playset_config.backup-2026-10-18T20-47-59-286Z.json` is the config from before the 2026-10-18 write attempt.
