# CS1 → CS2 map converter (Dakota Falls)

Ports the Cities: Skylines 1 Workshop map **Dakota Falls** (Steam 2972879554) into the Cities: Skylines II map editor at true heights (Height Scale 2048), with roads, bridges, trees, props, water, climate, terrain textures and TM:PE data carried over.

## Pipeline

1. **`cs1-exporter/`** — DakotaExport, a CS1 mod that dumps the map (networks, trees, props, PO structures, IMT/TM:PE data) to JSON.
2. **`hm-rebuild/`** — rebuilds the CS1 1081 px heightmap as a CS2 4096 px 16-bit heightmap plus world map.
3. **`net-converter/`** — turns the export into CS2 import files and drives the whole build (`node tools/import-run.mjs`), including prefab mapping, bridges, the avalanche gallery, trees, water, TM:PE and the post-save look.
4. **CS2MCP bridge fork** — the HTTP bridge that runs inside the CS2 editor and performs the imports: <https://github.com/thedevdavid/cities-skylines-2-mcp> (branch `editor-import`, nested here as `cs2mcp/`, ignored by this repo). Endpoints are documented in its `MAP_SETTINGS.md`.
5. **`look/`** — climate, water and terrain render settings and the CS1-derived terrain textures (`look/tools/*.mjs` regenerate them; see `look/README.md`).

## Where things are

- `setup-plan.md` — the full progress log and decisions, newest at the bottom.
- `next-run/RUNBOOK.md` — how to run a build round, the screenshot audit and the publishing steps.
- `MAP-DESCRIPTION.md` — the Paradox Mods page text and required-mods list.
- `cs2-playset/PLAYSET-AUDIT.md`, `cs1-playset/` — mod dependency audits.
- `docs/bridge-dev-notes.md` — the bridge fork's round-by-round development journal for this port (moved out of `cs2mcp/NOTES.md`).
- `bridges/`, `analysis-assets/`, `audit/`, `docs/` — research, asset mappings, audits, glossary and ADRs.

## Status

Run A is accepted and the publishing save `Dakota Falls.cok` is prepared (not yet shared). Remaining: deploy bridge 0.9.63, share from the editor, optional playset trim, run B (4096) and a second climate variant.
