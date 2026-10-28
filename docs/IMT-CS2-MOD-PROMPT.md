# Agent prompt: a CS2 road-marking mod (IMT equivalent) fed from the Dakota Falls data

Copy everything below the line into a fresh agent session started in `D:\work\mods\cs` (not inside this repo). Written 2026-10-28.

---

You are building **a Cities: Skylines II mod that draws road markings and roadside guardrails from data**: the CS2 counterpart of the CS1 Intersection Marking Tool (IMT). The first consumer is the Dakota Falls map port, whose CS1 markings are already decoded; the mod itself must be generic (no map names in its code or data model) and open source. Work locally, in a new repo at `D:\work\mods\cs\cs2-road-markings` (create it, `git init`, commit as you go). Never run anything in the cloud. The game is started and closed only by the owner; ask for a restart when you need one, and say "restart needed for correct results: yes/no" whenever you deploy a build.

## What exists (read these first, in this order)

All paths are under `D:\work\mods\cs\csl2-editor\` unless stated.

1. `cs1-offline/IMT-FOR-CS2.md` — the decoded IMT XML format, point/line id scheme, style codes and the geometry rules taken from the decompiled IMT 1.15.
2. `cs1-offline/out/imt-summary.md` and `cs1-offline/out/imt-markings.geojson` (1.9 MB, schema `dakota-offline/cs1/imt/v1`): 2 000 LineStrings (986 `barrier`, 1 014 `paint`) and 87 filler Polygons. Properties: `kind` (line | filler), `category` (barrier | network | paint), `style` (Solid, Dashed, DoubleSolid, Network; fillers: Chevron, Texture, Grass, Stripe), `styleCode`, `prefab` (CS1 prefab of a network line, e.g. `1889660581.Single Steel Barrier - raisable_Data`), `color` (`#rrggbbaa`), `width` (m), `markingType` (node | segment), `markingId`, `lineId`, `lengthM`. Coordinates are `[x east, z north, y height]` in CS1 metres, origin at the map centre. 81.6 km of guardrail, 32.7 km of paint, 0.44 ha of fillers.
3. `cs1-offline/src/markings.mjs` — the decoder (regenerate with `node cs1-offline/run.mjs`; raw XML stays in `cs1-offline/out/payloads/NodeMarkup.unz.bin`). The decompiled IMT 1.15 source is regenerated with `ilspycmd` from `D:\SteamLibrary\steamapps\workshop\content\255710\2140418403\IntersectionMarkingTool.dll` when you need the original rendering rules (dash lengths, chevron spacing, line offsets).
4. Coordinate contract with CS2 (from `net-converter/src/NetConverter/Outputs.cs` and `Converter.cs`): CS1 x/z metres carry over unchanged (factor 1.0), the playable map is clipped at ±7168 m, heights are CS1 world heights at CS2 Height Scale 2048 (the map in use: playable heightmap `10211409-dktflls-1to1.png`). Verify the z sign against an imported road before trusting it: `net-converter/out/x1/import-all.json` holds the road network the game already contains, in the same frame as the GeoJSON.
5. What CS2 already has for this map: `net-converter/README.md` lines 216–217 and 324 — CS1 **barrier networks** (not IMT lines) became `ConcreteBarrier01` / `SafetyBarrier01` props every 3 m (`import-walls-props.json`). The IMT guardrails, painted lines and fillers are **not** in the CS2 map at all; `mapping.tsv` row 74 and `dakota-falls-migration-analysis.md` ("Bridge decoration networks (IMT): Skip") are the gap this mod closes.
6. The bridge fork `cs2mcp/` (branch `editor-import`, deployed as 0.9.63): an HTTP server inside the game at `http://127.0.0.1:8642`. Read its `README.md`, `IMPORT_SCHEMA.md` (`POST /import/objects` with pin/yaw, `/import/surfaces` polygons, undo endpoints), `SURFACES.md` (how area entities and the `Clip Surface` work), and `RequestHandlers.Objects.cs` / `BridgeObjectToolSystem.cs` for how the fork creates entities through `ToolOutputBarrier`. Use it for prototypes and for measuring the game; it is also the pattern for ECS-safe entity creation.
7. CS2 modding skills in this environment: `cs2-modding:cs2-modding-setup`, `cs2-modding:cs2-mod-project`, `cs2-modding:cs2-modding`, `cs2-modding:cs2-modding-ui`. Load them before creating the project. Decompiled `Game.dll` (CS2 1.6.x) is the ground truth for rendering systems; `tools-bin/` in the csl2-editor repo is where portable tools go (git-ignored).

## Stage 0 — research and decision (one report, no code yet)

Answer, with evidence from Paradox Mods (https://mods.paradoxplaza.com) and decompiled game code, and stop for the owner's decision:

- Is there already a CS2 mod that draws custom lane lines, fillers or guardrails from a tool or from data (candidates to check by name: any port of "Intersection Marking Tool" by MacSergey, "Road Builder", "Extended Road Upgrades", "Traffic", "Placemaker", "Decal" or "Road Wear" mods, "Lane Markings")? If one exists with a file or API surface we can feed, the project becomes an importer for it and Stages 1–2 shrink accordingly.
- How does CS2 render its own lane markings and road decals (`Game.Net` lane-marking rendering, `DecalPrefab`/`Decal` objects, `Game.Rendering` batching)? Which of these three routes can draw a 10 cm-wide dashed white line along an arbitrary Bézier on terrain or on a road surface, and survive save/load: (a) decal object entities (an `ObjectPrefab` with a decal mesh, placed per dash, like the barrier props), (b) area/surface entities (`SURFACES.md`: polygon areas, no holes, triangulated by the game), (c) a custom `RenderSystem` with its own mesh and material (how Road Builder or Lumina-style mods inject rendering).
- Guardrails: CS2 fence-type nets, `SafetyBarrier01`-style props, or a network prefab with a lane object (what the game uses for its own highway barriers). Which one follows a curve without per-piece gaps, snaps to terrain, and saves in a map?
- Fillers (chevron, stripe, grass, texture polygons): surfaces (b) for Grass/Texture, generated geometry for Chevron/Stripe.

Deliver `docs/STAGE0.md` in the new repo: one table (route × line/filler/guardrail × renders / saves / editable later / effort) and a recommendation. Expected size: a day of work. The owner answers before Stage 1.

## Stage 1 — data model and importer

- Define the mod's own serialisable format (JSON) that is a superset of the GeoJSON above: lines with style, width, colour, dash pattern, an optional prefab; fillers with style; guardrails with a prefab name; everything in CS2 world metres. Write `node tools/imt-geojson-to-markings.mjs` in the new repo that converts `imt-markings.geojson` to it, applying the coordinate contract (and the z-sign check) and dropping features outside ±7168 m with a count.
- Importer: a mod-side HTTP route in the style of the bridge fork (so the existing runner can call it as a step) **or** an in-game "Import markings file" button in the editor UI; pick what Stage 0 recommended. Entities created through `ToolOutputBarrier` with a proper `PrefabRef`, so the game's own serialisation saves them; an undo that removes the last import.
- Acceptance: the guardrails of one interstate junction (pick a `markingId` with both `barrier` and `paint` rules) appear in the editor within 0.5 m of the GeoJSON, survive save → load, and are deleted by the undo. Screenshots through `GET /screenshot` on the bridge, before/after.

## Stage 2 — full Dakota Falls pass

- Import all 2 087 features on a copy of the accepted map (`dakota-A24d-2048`; never save over it, save as `dakota-A24d-markings`). Report counts, dropped features and reasons, frame-time cost (`/ping` timing and the game's own profiler), file-size delta of the save.
- Fidelity check script: sample 200 random lines and compare rendered entity positions (`/objects/near` or `/entity/inspect`) with the source; p50/p95 error in metres.
- Visual review shots with the bridge camera (`/camera/set`) of six junctions, sent to the owner.

## Stage 3 — tool UI (only after the owner accepts Stage 2)

An in-editor tool to add, move and delete markings by hand, reusing the mod's data model; mirrors IMT's point-and-rule idea only as far as CS2's UI allows. Out of scope until asked.

## Rules

- Generic mod, open source (MIT or Apache-2.0, pick one and say why); no "Dakota" in the mod's code, assets, data model or docs; the Dakota files are test inputs referenced by path in the README only.
- Commit everything after each step, messages ending with the line `Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>`. Push nothing until the owner names a remote.
- Build with `dotnet build -c Release`; deploy to the game's Mods folder only when the owner is between runs; keep the bridge fork untouched unless you need a new endpoint (then add it on `editor-import` in `cs2mcp/`, map-neutral, with a doc line, and tell the owner "restart needed: yes").
- No desktop automation; the owner drives the game UI. Use the bridge for everything you can measure or place.
- Report after each stage in under 40 lines: what works, what does not, the numbers, the next decision for the owner.
