# CS1 → CS2 map conversion

Converts the Cities: Skylines 1 map "Dakota Falls" into a Cities: Skylines 2 map: the converter turns the CS1 export into import files, and the runner imports them into the CS2 map editor through the bridge.

## Game and tooling

**Bridge**:
The CS2MCP mod running inside the CS2 editor, an HTTP server at 127.0.0.1:8642 with versioned behaviour ("bridge 0.9.62").
_Avoid_: CS2MCP server, game server, the mod

**Bridge deck**:
A road or rail bridge structure on the map, identified by a BR id (BR001 …).
_Avoid_: bridge (on its own)

## Importing

**Import file**:
One JSON file the converter writes, holding the items for one import.
_Avoid_: output, layer file

**Import**:
One POST of an import file to the bridge; it returns an importId and can be undone while it is the newest of its kind.
_Avoid_: upload, batch

**Step**:
A named entry in the run order; it posts zero or more import files, or is a check such as net-count or verify.
_Avoid_: layer (for runner entries), phase

**Layer**:
A network class in the converter's mapping (Highways, Walls, Power, Rail …).
_Avoid_: step

**Import manifest**:
The list of import files one conversion wrote for a scale set, each tagged with the step that posts it.
_Avoid_: file list, ROUND.md

**Paint layer**:
A terrain material brush set (sand, dry, dirt, lush) or a landscape spot N1–N13.
_Avoid_: layer (on its own)

## Network and heights

**Course**:
One network piece the converter writes, with an id like e20430_0b.
_Avoid_: edge (that is the JSON field), segment

**Segment**:
A live network entity in the game, as the bridge's graph export and net count report it.
_Avoid_: course, edge

**Drop**:
A course the game did not build, reported back with the segment it ran inside and how far to move it.
_Avoid_: failure (on its own), missing road

**Scale set**:
One of the two output folders: x1 at true heights (Height Scale 2048) or x2 at doubled heights (Height Scale 4096).
_Avoid_: scale, variant

**Topology height**:
The height space the converter's passes work in (topology factor 2).
_Avoid_: CS1 height

**Output height**:
The height written to import files: topology height × height factor ÷ topology factor.
_Avoid_: game height, true height

## Iterations

**Round**:
A numbered change set the owner requested from the converter (round 8 … 52).
_Avoid_: version, iteration

**Run**:
One execution of the import runner against the bridge, recorded in one run report.
_Avoid_: attempt, round

**Attempt**:
One fresh-map build in the game, numbered A-vN; it can take several runs (build, post-save, resumes).
_Avoid_: run, version
