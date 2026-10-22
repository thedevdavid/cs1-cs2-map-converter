// Removes exactly the objects /debug/override-loop reports: per building, a demolish-area polygon = the building's
// rotated prefab bounds, restricted to the looping prefabs. Rotation comes from our import files (nearest entry).
import fs from "node:fs";
const B = "http://127.0.0.1:8642", root = "D:/work/mods/csl2-editor/net-converter/out/x1/";
const get = async u => (await fetch(B + u)).json();
const post = async (u, b) => (await fetch(B + u, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(b) })).json();
const rep = JSON.parse(fs.readFileSync(root + "override-loop-a22b.json", "utf8"));
const placed = [];
for (const f of ["import-fort.json", "import-objects.json", "import-dam.json", "import-substation.json", "import-connections.json"]) {
  if (!fs.existsSync(root + f)) continue;
  const o = JSON.parse(fs.readFileSync(root + f, "utf8"));
  for (const it of (o.objects || [])) if (it.prefab && typeof it.x === "number") placed.push(it);
}
const sizes = {};
let dryTotal = 0, realTotal = 0; const plan = [];
for (const b of rep.buildings) {
  if (!sizes[b.prefab]) { const i = await get("/prefabs/info?name=" + encodeURIComponent(b.prefab)); sizes[b.prefab] = i.geometry?.size; }
  const s = sizes[b.prefab]; if (!s) { console.log("no size for", b.prefab); continue; }
  let best = null, bd = 1e9;
  for (const p of placed) { if (p.prefab !== b.prefab) continue; const d = Math.hypot(p.x - b.position.x, p.z - b.position.z); if (d < bd) { bd = d; best = p; } }
  const yaw = (best && bd < 5 ? (best.rotation || 0) : 0) * Math.PI / 180;
  const hw = s.x / 2 + 0.5, hd = s.z / 2 + 0.5, c = Math.cos(yaw), sn = Math.sin(yaw);
  const poly = [[-hw, -hd], [hw, -hd], [hw, hd], [-hw, hd]].map(([lx, lz]) => [+(b.position.x + lx * c + lz * sn).toFixed(2), +(b.position.z - lx * sn + lz * c).toFixed(2)]);
  const prefabs = Object.keys(b.objects), expect = Object.values(b.objects).reduce((a, n) => a + n, 0);
  const body = { polygon: poly, kinds: ["object", "tree"], prefabs, dryRun: true };
  const dry = await post("/build/demolish-area", body);
  plan.push({ b, body, expect, dry: dry.total });
  dryTotal += dry.total || 0;
  console.log(b.prefab, b.position.x.toFixed(0), b.position.z.toFixed(0), "rot", (yaw * 180 / Math.PI).toFixed(1), "matchDist", bd.toFixed(1), "expect", expect, "dry", dry.total, dry.error || "");
}
console.log("loop objects", rep.objectsInLoop, "dry total", dryTotal);
if (dryTotal > rep.objectsInLoop * 1.3 + 5) { console.log("ABORT: dry run would remove far more than the loop count"); process.exit(2); }
for (const p of plan) { if (!p.dry) continue; const r = await post("/build/demolish-area", { ...p.body, dryRun: false }); realTotal += r.objectsDeleted || 0; }
console.log("removed", realTotal);
await new Promise(r => setTimeout(r, 6000));
const again = await get("/debug/override-loop");
fs.writeFileSync(root + "override-loop-a22b-after.json", JSON.stringify(again));
console.log("after: objectsInLoop", again.objectsInLoop, JSON.stringify(again.buildings?.slice(0, 3) || []).slice(0, 300));
process.exit(again.objectsInLoop === 0 ? 0 : 3);
