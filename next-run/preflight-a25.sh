#!/bin/bash
# A-v25 pre-flight: inputs on disk, bridge build deployed, game closed. One line per check; exits 1 on any FAIL.
# Usage (Git Bash, from anywhere): bash next-run/preflight-a25.sh
# The import file list and the texture slots are read from net-converter/tools/import-run.mjs itself (its steps and slots
# arrays), for the A-v25 build flags --po-structures --with po-gantry.
R="$(cd "$(dirname "$0")/.." && pwd)"
CS2="$LOCALAPPDATA/../LocalLow/Colossal Order/Cities Skylines II"
HM="$CS2/Heightmaps"; DLL="$CS2/Mods/CS2MCP/CS2MCP.dll"
fails=0
ok()   { echo "OK    $*"; }
bad()  { echo "FAIL  $*"; fails=$((fails + 1)); }
info() { echo "INFO  $*"; }
need() { if [ -f "$1" ]; then ok "$2"; else bad "$2 (missing: $1)"; fi; }

# 1. Heightmaps
need "$HM/10211409-dktflls-1to1.png" "playable heightmap 10211409-dktflls-1to1.png"
need "$HM/10270758-dktflls-1to1_worldmap.png" "world map 10270758-dktflls-1to1_worldmap.png"
need "$HM/10270758-dktflls-1to1_water.json" "world map water 10270758-dktflls-1to1_water.json"

# 2. Import files of the runner's default step list (+ po-structures, po-gantry), resolved by the runner's own code
if ! command -v node >/dev/null; then bad "node not on PATH"; else
  ok "node $(node --version)"
  list=$(cd "$R/net-converter" && node -e '
    const fs = require("fs"), { join } = require("path");
    const src = fs.readFileSync("tools/import-run.mjs", "utf8");
    const a = src.indexOf("const steps = ["), b = src.indexOf("\nconst optional", a);
    if (a < 0 || b < 0) { console.log("PARSE steps array not found"); process.exit(0); }
    const dir = join("out", "x1"), treesDir = join("out", "trees"), p = f => join(dir, f);
    const waterFile = "import-water-sea93.json";
    const treeFiles = fs.existsSync(treesDir) ? fs.readdirSync(treesDir).filter(f => /^import-trees-\d\d\.json$/.test(f)).sort().map(f => join(treesDir, f)) : [];
    const fixedFiles = fs.existsSync(dir) ? fs.readdirSync(dir).filter(f => /^import-trees-fixed-\d\d\.json$/.test(f)).sort().map(f => join(dir, f)) : [];
    const steps = new Function("p", "join", "waterFile", "treeFiles", "fixedFiles", src.slice(a, b).replace("const steps =", "return"))(p, join, waterFile, treeFiles, fixedFiles);
    const withOpt = new Set(["po-structures", "po-gantry"]);
    const files = new Set();
    for (const [name, endpoint, fs_, optional] of steps) {
      if (optional && !withOpt.has(name)) continue;
      const use = endpoint === "pillarsoff" ? fs_.slice(0, 1) : fs_;   // the BR006 arch file is read only with --with po-br006-arches
      for (const f of use) files.add(f.replace(/\\/g, "/"));
    }
    console.log("TREES " + treeFiles.length + " " + fixedFiles.length);
    for (const f of files) console.log("FILE " + f);
    const s = src.match(/const slots = (\[\[.*?\]\]);/);
    if (s) for (const [slot, base] of JSON.parse(s[1].replace(/\x27/g, "\""))) console.log("SLOT " + slot + " " + base); else console.log("PARSE slots not found");
  ')
  [ -z "$list" ] && bad "could not read the step list from import-run.mjs"
  nfiles=0; nmiss=0
  while read -r kind a b; do
    case "$kind" in
      PARSE) bad "import-run.mjs: $a $b";;
      TREES) if [ "$a" -gt 0 ] && [ "$b" -gt 0 ]; then ok "tree chunks: $a in out/trees, $b fixed in out/x1"; else bad "tree chunks: $a in out/trees, $b fixed in out/x1"; fi;;
      FILE)  nfiles=$((nfiles + 1)); [ -f "$R/net-converter/$a" ] || { bad "import file missing: net-converter/$a"; nmiss=$((nmiss + 1)); };;
      SLOT)  for suf in BaseColor Normal; do f="$R/look/textures/v3/${b}_${suf}.u1.png"; [ -f "$f" ] || bad "texture missing for slot $a: look/textures/v3/${b}_${suf}.u1.png"; done; slots="$slots $a";;
    esac
  done <<< "$list"
  [ "$nfiles" -gt 0 ] && [ "$nmiss" -eq 0 ] && ok "$nfiles import files of the default steps (+po-structures, po-gantry) present under net-converter/out"
  [ -n "$slots" ] && ok "texture slots checked in look/textures/v3 (.u1):$slots"
fi
need "$R/net-converter/building-bounds.json" "net-converter/building-bounds.json (fort-strip step)"

# 3. Look JSONs (terrain-cs1 carries the Extra3/4 blend values)
for f in water terrain-cs1 climate; do need "$R/look/$f.json" "look/$f.json"; done
grep -q m_Extra4BlurDepth "$R/look/terrain-cs1.json" && ok "terrain-cs1.json has the Extra3/4 splat/blur values" || bad "terrain-cs1.json lacks m_Extra3/4 SplatRange/BlurDepth"

# 4. TM:PE inputs
need "$R/cs1-offline/out/tmpe-offline.json" "cs1-offline/out/tmpe-offline.json"
need "$LOCALAPPDATA/DakotaExport/2972879554_Dakota_Falls-20261019-063123.json" "CS1 dump for TM:PE and fidelity"

# 5. Converter commit
git -C "$R" merge-base --is-ancestor dffafd6 HEAD 2>/dev/null && ok "converter at $(git -C "$R" rev-parse --short HEAD) (contains round 52 dffafd6)" || bad "converter HEAD does not contain dffafd6 (round 52)"

# 6. Bridge build
if grep -q '"0\.9\.63' "$R/cs2mcp/CS2MCP.Bridge/Mod.cs"; then ok "cs2mcp Mod.cs version 0.9.63"; else bad "cs2mcp Mod.cs is not 0.9.63: $(grep -o 'Version = "[^"]*"' "$R/cs2mcp/CS2MCP.Bridge/Mod.cs")"; fi
if [ -f "$DLL" ]; then
  src=$(git -C "$R/cs2mcp" log -1 --format=%ct -- CS2MCP.Bridge); dll=$(stat -c %Y "$DLL")
  if [ "$dll" -gt "$src" ]; then ok "deployed CS2MCP.dll $(date -d @"$dll" '+%F %T') newer than the last CS2MCP.Bridge commit $(date -d @"$src" '+%F %T')"
  else bad "deployed CS2MCP.dll $(date -d @"$dll" '+%F %T') older than the last CS2MCP.Bridge commit $(date -d @"$src" '+%F %T')"; fi
else bad "deployed CS2MCP.dll missing: $DLL"; fi

# 7. Game / bridge state (information only)
if curl -s -m 2 http://127.0.0.1:8642/ping >/dev/null 2>&1; then info "bridge answers on 127.0.0.1:8642 (game running): $(curl -s -m 2 http://127.0.0.1:8642/ping | head -c 120)"
else info "bridge port 8642 not answering (game closed)"; fi

echo "== $([ $fails -eq 0 ] && echo PASS || echo "FAIL ($fails)")"
exit $([ $fails -eq 0 ] && echo 0 || echo 1)
