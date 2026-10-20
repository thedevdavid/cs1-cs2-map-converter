# Detroit crossing (BR001): alternatives to test

Owner review of A-v7 (`next-run/shots-a7/04-detroit-suspension-side.png`): the current `BXP SuspensionBridge03 - Six Lane Highway` works, but the owner is not sure about it and wants another bridge tested.

**CS1 original:** two one-way 3-lane interstate carriageways, about 16 m apart. Two blue towers (Detroit Bridge [Ploppable], Ambassador-style) stand 723 m apart at (1314, −3786) and (1751, −3210). That is one long main span with short side spans, a steel stiffening truss, and a deck about 30 m over the water at Height Scale 2048. The `B` part is about 1.4 km per carriageway.

**Current A-v7:** `BXP SuspensionBridge03 - Six Lane Highway`, one two-way deck 28 m wide. Fixed spans are 168 + 320 + 320 + 168 = 976 m, so the towers stand 640 m apart. The carriageways merge onto it through 140 m connectors. Look: tan towers, stiffening truss, a single main span.

**Sources:**
- **Live, read in the running editor on 2026-10-20 (bridge 0.9.29):** names from `cs2_find_prefabs category=net query=Bridge` (119 matches) and the span rules from `cs2_prefab_info`. All the names below exist and are placeable in the DakotaFalls-E2C playset.
- **Unverified [U]:** lane counts and looks, inferred from width, name and flags.
- **Import contract** (cs2mcp/IMPORT_SCHEMA.md "Bridge prefabs"):
  - **Fixed-span prefab:** one edge from abutment to abutment; the game cuts it into the spans and scales them in proportion.
  - **Non-fixed prefab:** one edge of any length at or above the minimum; the game cuts it into ⌈L / max⌉ equal pieces.

## Shortlist

| # | prefab (exact) | source | span rules (live) | deck | fits Detroit how | look vs CS1 Ambassador-style |
|---|---|---|---|---|---|---|
| **B** (DECISIONS.md) | `Suspension Bridge - Highway Oneway - 3 lanes` ×2, side by side | vanilla | non-fixed, `segmentLength` 256 → edges 153.6–358.4 m, straight (`canCurve` false), `MiddlePillars`, min 10 m over water | **one-way**, 16 m wide, 3 lanes: lane-exact per carriageway, **no merge** | One edge per carriageway over the ~976 m river part. The game cuts it into 3 equal spans of about 325 m, i.e. a chain of towers rather than one 723 m main span. The two decks need ≥ 18 m between centrelines (CS1 has about 16 m), so shift each carriageway 1–2 m outward over the bridge | two parallel slim suspension chains with towers every ~325 m: more towers and shorter spans than CS1, but twin decks as in CS1 [U look] |
| **1** | `Golden Gate Bridge` | vanilla | fixed 350 + 640 + 640 + 350 m (each 0.6–1.4 ×) → **one edge 1188–2772 m**; towers at L × 350/1980 and L × 1630/1980; straight; `elevationRange` max 100 | two-way, 25 m wide; a road, not a highway; lanes [U] (probably 3 + 3) | Merge as now, at the minimum length 1188 m (side spans 210 m, **towers 768 m apart**, nearest to CS1's 723 m). That is 212 m longer than now, so the 140 m connectors shrink to about 35 m each, or the bridge ends move outward onto the approaches | **closest proportions**: two tall towers, one long main span, short side spans. But international-orange Art Deco towers, not blue steel |
| **2** | `BXP Suspension Bridge - Highway Twoway - 6 Lanes` | Bridge Expansion Pack 92245 | non-fixed, `segmentLength` 320 → edges 192–448 m, **curves allowed**, `MiddlePillars` | two-way highway, 28 m wide, 6 lanes (3 + 3) | Merge as now. A 976 m edge is cut into 3 spans of about 325 m (towers at each node [U]). To mimic CS1, use 2 edges of 362 m centred on the CS1 towers' midpoint (1532, −3498) plus plain approaches: towers at about 0 / 362 / 724 m, i.e. **CS1's tower positions plus one mid-river tower** | highway deck as now, but a multi-span chain (like the Bay Bridge western spans) instead of one main span; the BXP highway suspension mesh (not the B&P one) [U look] |
| **3** | `BXP Double Deck Suspension Bridge - Highway` (+ `BXP Double Deck Suspension Bridge - Lower Highway` drawn under it) | Bridge Expansion Pack 92245 | fixed span 320 m repeated (count unbounded) → each span 192–448 m; **max slope 5 %**; `elevationRange` 10–200; straight | upper deck one-way highway 24 m wide (lanes [U], probably 3–4); the lower deck is a separate net | Each carriageway gets its own deck, stacked: northbound on top, southbound below. This needs the converter to stack the carriageways vertically over ~1 km (new, not done anywhere yet) and ramps of ≤ 5 % to separate them. Spans of about 325 m | double-deck stiffening-truss suspension (Bay Bridge / Verrazzano-like), the most "steel truss" of all; lane-exact per deck, but stacked carriageways are a departure from CS1 [U look]. Highest effort |

### Considered and not shortlisted
- **`SuspensionBridge03`** (vanilla B&P "Six-Lane Suspension Bridge"): the same fixed spans as the current BXP copy (168 + 320 + 320 + 168 m, 25 m wide, a road instead of a highway). It would look the same as A-v7.
- **`SuspensionBridge04`** (vanilla B&P six-lane one-way suspension): the same spans, but 6 lanes one-way. Two side by side would double the lane count, and each still has a single 640 m main span.
- **`Grand Bridge`** (vanilla): fixed 130 + 170 + 170 + 130 m, so 360–840 m in total and a main span ≤ 476 m; 19 m two-way road, about 4 lanes. Too short and too narrow.
- **`Cable-stayed Bridge - XL Road Divided - 8 Lanes`**: 8 lanes, 40 m wide, edges ≤ 179 m. It is a cable-stayed type and the lanes don't match.
- **`BXP ExtradosedBridge03 - Highway`**: edges 192–448 m, 20 m two-way (about 2 + 2). An extradosed type with too few lanes.
- **Not available:** `Cable-Stayed Bridge - Highway Oneway - 3 Lanes` and `Extradosed Bridge - Highway Oneway - 3 Lanes` appear in Locale.cok, but not as placeable nets in this build. Searching for "Cable" and "Bridge" does not list them.
- **No blue option exists.** None of the candidates has a selectable colour [U: a recolour mod is not in the playset].

## Recommendation for the next run
Test **option 1 (`Golden Gate Bridge`)** first. It is the only alternative that keeps a single long main span (768 m vs CS1 723 m), and it uses the same converter path as A-v7: fixed spans, merged carriageways, length 1188 m instead of 976 m.

If the owner prefers twin carriageways over the single-span shape, **option B** is the next one. It needs no merge, only a 1–2 m outward shift per carriageway and one non-fixed edge each. Option 2 is the cheap fallback, the same as A-v7 with a different prefab and edge length. Option 3 is a larger converter change and only worth it if the owner likes the double-deck truss look.

## Converter changes per option (net-converter BR001 override, `bridges.tsv`)

| option | override |
|---|---|
| B | prefab `Suspension Bridge - Highway Oneway - 3 lanes`, no `merge`, one edge per carriageway over the river part (~976 m), carriageways offset to ≥ 18 m apart over the bridge |
| 1 | prefab `Golden Gate Bridge`, `fixed:350,640,640,350;merge`, length 1188 m centred on (1532, −3498) on the tower line; check that the connectors (~35 m) still meet the approaches' minimum lengths |
| 2 | prefab `BXP Suspension Bridge - Highway Twoway - 6 Lanes`, `merge`, two edges of ~362 m between the CS1 tower positions (or one 976 m edge, cut by the game into 3) |
| 3 | prefab pair `BXP Double Deck Suspension Bridge - Highway` / `… - Lower Highway`, carriageways stacked; ramps ≤ 5 %; new converter logic |
