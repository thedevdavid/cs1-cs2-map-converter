# CS1 → CS2 map converter

A generic, open-source tool for converting a Cities: Skylines 1 map into a Cities: Skylines II map in the map editor, at true heights, with roads, bridges, trees, props, water, climate, terrain textures and TM:PE data carried over. **Dakota Falls** (Steam Workshop 2972879554) is the reference map: it was converted end to end and is the worked example. The engine is being made map-agnostic, with everything map-specific moving into a per-map profile (roadmap: issue #3).

## Pipeline

1. **`cs1-exporter/`** — DakotaExport, a CS1 mod that dumps the map (networks, trees, props, PO structures, IMT/TM:PE data) to JSON.
2. **`hm-rebuild/`** — rebuilds the CS1 1081 px heightmap as a CS2 4096 px 16-bit heightmap plus world map.
3. **`net-converter/`** — turns the export into CS2 import files and drives the whole build (`node tools/import-run.mjs`), including prefab mapping, bridges, the avalanche gallery, trees, water, TM:PE and the post-save look.
4. **CS2MCP bridge fork** — the HTTP bridge that runs inside the CS2 editor and performs the imports: <https://github.com/thedevdavid/cities-skylines-2-mcp> (branch `editor-import`, nested here as `cs2mcp/`, ignored by this repo). Endpoints are documented in its `MAP_SETTINGS.md`.
5. **`look/`** — climate, water and terrain render settings and the CS1-derived terrain textures (`look/tools/*.mjs` regenerate them; see `look/README.md`).

## Where things are

- `setup-plan.md` — the full progress log and decisions, newest at the bottom.
- `docs/UPSTREAMING.md` — issue text and staged PR plan for contributing the bridge fork upstream.
- `next-run/RUNBOOK.md` — how to run a build round, the screenshot audit and the publishing steps.
- `MAP-DESCRIPTION.md` — the Paradox Mods page text and required-mods list.
- `cs2-playset/PLAYSET-AUDIT.md`, `cs1-playset/` — mod dependency audits.
- `docs/bridge-dev-notes.md` — the bridge fork's round-by-round development journal for this port (moved out of `cs2mcp/NOTES.md`).
- `bridges/`, `analysis-assets/`, `audit/`, `docs/` — research, asset mappings, audits, glossary and ADRs.

## Status

The converter works end to end on the reference map: Dakota Falls run A is accepted and its publishing save `Dakota Falls.cok` is prepared (not yet shared). The roadmap to a generic converter (bridge client and tests, identical-output check, fix passes, per-map profile, generic CS1 exporter and a second map as the acceptance test) is spec issue [#3](https://github.com/thedevdavid/cs1-cs2-map-converter/issues/3), with tickets in priority order. Bridge fork: <https://github.com/thedevdavid/cities-skylines-2-mcp> (branch `editor-import`); upstreaming plan: `docs/UPSTREAMING.md`. Dakota Falls leftovers: deploy bridge 0.9.63, share from the editor, run B (4096) and a second climate variant.
