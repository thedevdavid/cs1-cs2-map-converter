I want a Cities: Skylines II mod for custom road markings, eventually something like CS1's Intersection Marking Tool, but built up from the simplest thing that works.

Start by reverse-engineering the game's own marking systems with REA on Game.dll: how lane lines, crosswalks, stop lines and arrows are defined, generated and rendered; how decal objects are placed, rendered and saved; how area surfaces are drawn on terrain; and what each needs serialised to survive save and load. From that, rank the ways to draw a thin white line along an arbitrary curve, easiest first.

Then get one hard-coded solid white line onto a road, visible in the editor and in game, surviving save and load. Make it dashed, then coloured.

Then a tool: click two points on a road, get a line, with width, colour and dash settings, and a way to delete it.

Only once that is solid, grow toward IMT: lines snapped to lane edges at junctions, fillers, guardrails along curves, import and export of a marking set.

Stop after each part and show me before continuing.
