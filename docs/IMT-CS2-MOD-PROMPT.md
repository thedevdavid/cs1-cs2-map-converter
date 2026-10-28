Build a Cities: Skylines II mod that draws custom road markings, grown from the simplest thing that works toward an Intersection Marking Tool equivalent. Load the skills `reverse-engineer-anything` and `cs2-modding:*` first.

## 1. REA the game's own markings

Open `Game.dll` (`…\Cities Skylines II\Cities2_Data\Managed\`) with the REA tools and find, with decompiled evidence:

- Where lane lines, crosswalks, stop lines and arrows come from: the lane / lane-marking prefabs and components in `Game.Net` and `Game.Prefabs`, the systems that generate their geometry, and the rendering path in `Game.Rendering` that puts them on the road surface.
- How decal-type `ObjectPrefab`s are placed, rendered and saved, and whether a decal can be stretched or repeated along a curve.
- How `Game.Areas` surfaces are triangulated and rendered on terrain.
- What is serialised for each, so a created entity survives save and load.

Write `docs/REA.md`: one page per mechanism (type names, system order, the smallest component set an entity needs), ending with a ranked list of routes for "a 10 cm white line along an arbitrary curve", easiest first.

## 2. One line on the road

Using the easiest route: a mod with one debug hotkey that draws a solid white line between two hard-coded points on a road, visible in the editor and in game, surviving save and load. Screenshot as proof. Then dashed, then coloured.

## 3. A marking tool

Click two points on a road to get a line; settings for width, colour, dash pattern; click a line to delete it. Data model: a list of polylines with a style. UI via the game's tool panel conventions.

## 4. Toward IMT

In order, each only after the previous is solid: lines snapped to lane edges at a junction (IMT's "points"), fillers (chevron, stripe, solid polygon), guardrails along a curve (fence props or a net lane object, whichever step 1 showed works), JSON import/export of a marking set.

Stop after each step with a short report and wait for a go.
