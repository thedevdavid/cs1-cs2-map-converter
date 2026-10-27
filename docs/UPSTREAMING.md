# Upstreaming the editor-import bridge to LancerComet/cities-skylines-2-mcp

Status 2026-10-27: our fork is at <https://github.com/thedevdavid/cities-skylines-2-mcp>, branch `editor-import`, 82 commits ahead of upstream `master` (72bd7e0), upstream's security hardening already merged in, no map-specific content left in tree, history or commit messages. Nothing has been posted upstream yet. This document holds the issue text and the PR plan so it can be done later without re-deriving anything.

Ground rules that make this acceptable to a maintainer of a 1 500-line project:

- Issue first, PRs only after a reply. The maintainer just redesigned the request layer; they will want a say in the shape.
- Each PR builds green on its own, changes nothing in play mode behaviour, and keeps the new routes behind the editor-mode check so a city player never sees them.
- Every PR branches from upstream `master`, not from `editor-import`, and cherry-picks or re-applies only its own files. Our fork keeps everything regardless of what upstream takes.
- Docs go with the code they describe, trimmed to the endpoints in that PR.
- Commit trailers: keep the `Co-Authored-By: Claude …` lines; they are honest provenance.

## 1. Issue text (post as a GitHub issue on upstream)

Title: **Map-editor support: bulk import, map settings and publishing endpoints (offering a fork for upstream)**

Body:

> I have been using CS2MCP as the automation layer for a Cities: Skylines 1 → 2 map port and ended up extending the bridge for the **map editor** (`gameMode: Editor`), where the play-mode tools do not apply. The fork is at https://github.com/thedevdavid/cities-skylines-2-mcp (branch `editor-import`, Apache-2.0 like upstream). It adds, all behind an editor-mode check so play mode is unchanged:
>
> - **Bulk import with undo:** `/import/network` (nodes, edges, lane/elevation data, bridges), `/import/objects` (trees, props, buildings with pin/pitch/roll, skip-inside-buildings and strip-owned-sub-object options), `/import/water` (sources, sea level), `/import/surfaces`, `/import/resources`, `/import/tmpe` (lane arrows, speed limits, junction rules → Traffic mod via a satellite assembly), each with `/import/status`, `/import/report` and `/import/undo`.
> - **Map settings and look:** `/map/settings` (height scale, sea level, theme, climate, water and terrain render settings, per-field edits), `/map/terrain-textures` (import PNGs into the terrain layers through the game's own importer), `/map/look/save-prefabs`, `/map/time`.
> - **Editor workflow:** `/map/save`, `/map/load`, `/map/starting-tiles`, `/map/camera-start`, `/map/preview`, `/screenshot?ui=false`, `/build/demolish-area`, `/build/strip-subobjects`, `/graph/export` and `/graph/diff`, `/objects/near`, `/entity/inspect`.
> - **Generic hardening we needed along the way:** 16 MB request bodies and a 15-minute timeout for long imports, switching to the default tool before state-changing POSTs, texture asset reuse keyed by file content, a request journal in CS2MCP.log.
>
> It is about 15 000 lines across 34 new and 14 changed files, which is too much for one PR. I would like to contribute it in stages (generic fixes → editor foundation → look/terrain → import pipeline), each building green on its own, or keep it as a documented fork if you would rather keep upstream play-mode only. Which do you prefer, and do you have conventions I should follow for route naming and the MCP tool list?
>
> Endpoint docs: `MAP_SETTINGS.md` and `IMPORT_SCHEMA.md` in the fork. Happy to split differently if that suits the project better.

## 2. PR plan

Branch names are suggestions. Sizes are from `git diff --stat origin/master...editor-import`.

### PR 1 — generic fixes (small, no new features)

Branch `fix/bridge-limits-and-journal`. Files: `HttpBridgeServer.cs` (body limit, timeout), `BridgeSystem.cs` / `RequestHandlers.cs` (default-tool switch before POSTs, request journal), `ScreenshotRunner.cs` (UI-free screenshot option), `RequestHandlers.TerrainTextures.cs` is **not** here (it is a feature).

Content: 16 MB body limit and 15-minute per-request timeout; the DefaultToolSystem switch before any state-changing POST (fixes "tool still active" failures); request journal lines in CS2MCP.log; `/screenshot?ui=false&width&height&save=`. Each item one commit. PR text: what failed before, what changes, no behaviour change for existing routes.

### PR 2 — editor foundation

Branch `feat/editor-mode-basics`. Files: `Mod.cs` / `BridgeSystem.cs` (editor-mode detection and the gate), `RequestHandlers.MapSettings.cs`, `RequestHandlers.MapSave.cs`, `RequestHandlers.MapLoad.cs`, `RequestHandlers.StartTiles.cs`, `RequestHandlers.Checklist.cs`, `RequestHandlers.Diagnostics.cs`, `RequestHandlers.EditorInvoke.cs` (optional, debug only), `MAP_SETTINGS.md` trimmed to these routes, MCP server tools `cs2_map_settings`, `cs2_save_game`/load, `cs2_map_checklist`.

Content: `GET/POST /map/settings`, `/map/save`, `/map/load`, `/map/starting-tiles`, `/map/camera-start`, `/map/preview`, `/map/checklist`, `/map/time`. PR text: the editor exposes none of this to scripts today; show the publish-prep flow as the example.

### PR 3 — look and terrain

Branch `feat/editor-look-terrain`. Files: `RequestHandlers.MapLook.cs`, `RequestHandlers.SaveLookPrefabs.cs`, `RequestHandlers.TerrainTextures.cs`, `RequestHandlers.TerrainPaint.cs`, `RequestHandlers.TerrainPaintRaster.cs`, `RequestHandlers.Water.cs`, `RequestHandlers.Resources.cs`, `RequestHandlers.Surfaces.cs`, `SURFACES.md`, the look sections of `MAP_SETTINGS.md`.

Content: climate, water and terrain render settings read/edit, `/map/look/schema`, terrain texture import with content-hashed asset reuse, terrain paint (brush and raster), water sources and sea level, map resources, surfaces. PR text must state the two facts that cost us days: a 24-bit PNG imports with alpha 1.0 and the terrain shader reads diffuse alpha as smoothness; the game's texture hash ignores pixel content, hence the SHA-1 in the asset name.

### PR 4 — import pipeline

Branch `feat/editor-import`. Files: `BridgeImportToolSystem.cs` (+ `.CourseObjects.cs`), `BridgeObjectToolSystem.cs`, `BridgeTmpeImportSystem.cs`, `RequestHandlers.Import.cs`, `RequestHandlers.Objects.cs`, `RequestHandlers.ObjectsNear.cs`, `RequestHandlers.Tmpe.cs`, `RequestHandlers.Graph.cs`, `RequestHandlers.GraphDiff.cs`, `RequestHandlers.SubNets.cs`, `RequestHandlers.PrefabRefs.cs`, `RequestHandlers.BridgeRules.cs`, `RequestHandlers.DemolishArea.cs`, `RequestHandlers.StripSubObjects.cs`, `RequestHandlers.OverrideLoop.cs`, `NetGuard.cs`, `OverrideSampler.cs`, `AnarchyInterop.cs`, `CS2MCP.Traffic/*` (satellite assembly, loads only when the Traffic mod is present), `IMPORT_SCHEMA.md`, `PREFAB_NAMES.md`, `fixtures/*`, `scripts/import-network.mjs`, `scripts/import-tmpe.mjs`, MCP tools `cs2_import_*`, `cs2_undo_*`, `cs2_graph_export`.

This is the big one (about 9 000 lines). Offer it as an optional module: a single `#if`/feature flag or a separate assembly `CS2MCP.Editor` if the maintainer prefers, so upstream can take PRs 1–3 and leave this one as the fork's own. PR text: the import JSON schema, the undo model, the known game interactions (OverrideSystem vs Anarchy loop, GroundHeightSystem re-grounding hence `pin`, lot-masked trees), and the fixtures as the smoke test.

### Order and dependencies

1 → 2 → 3 → 4. PR 2 needs nothing from 1 but reads cleaner after it. PR 3 needs the editor gate from 2. PR 4 needs 2 (save/load for its tests) and the satellite-assembly pattern from 3 is not required.

## 3. Before each PR

- `git fetch origin && git checkout -b <branch> origin/master`, then bring the files over from `editor-import` (`git checkout editor-import -- <files>` plus hand-edits where a file is shared), build with `dotnet build CS2MCP.Bridge/CS2MCP.Bridge.csproj -c Release -p:SkipDeploy=true`, 0 warnings.
- Run the fork's fixtures against a fresh editor map for PR 4 (`scripts/import-network.mjs fixtures/ten-highway-segments.json`).
- Re-read the docs that ship with the PR for any leftover round numbers or map names (`grep -ri "dakota\|A-v[0-9]" .` must stay empty).
- Push to the fork, open the PR against `LancerComet:master`, end the body with the Claude Code attribution line.

## 4. What stays in the fork only

Our development journal (`docs/bridge-dev-notes.md` in this repo), the per-map scripts in `next-run/`, the `CS1_DUMP_DIR` convention for the TM:PE import script, and anything the maintainer declines.
