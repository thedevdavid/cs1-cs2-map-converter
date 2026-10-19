// Adds the "DakotaFalls-E2C" playset to CS2's playset_config.json.
// Usage: node add-playset.mjs            -> apply (backs up first)
//        node add-playset.mjs --dry-run  -> write result to ./playset_config.preview.json only
import fs from "node:fs";
import path from "node:path";
import crypto from "node:crypto";
import { execSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const modsDir = path.join(process.env.USERPROFILE, "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", ".cache", "Mods");
const cfgPath = path.join(modsDir, "playset_config.json");
const dryRun = process.argv.includes("--dry-run");

const NAME = "DakotaFalls-E2C";
const MODS = [
  ["140919", "Earth2Cities"],
  ["74604", "Anarchy"],
  ["74417", "Unified Icon Library (Anarchy dependency)"],
  ["74539", "Image Overlay"],
  ["74285", "Scene Explorer"],
];

// 1. Refuse while anything that rewrites the config is running.
const tasks = execSync("tasklist /fo csv /nh", { encoding: "utf8" });
const blockers = tasks.split(/\r?\n/).map(l => l.split('","')[0].replace(/^"/, ""))
  .filter(n => /^(Cities2|Paradox Launcher|ParadoxLauncher|Skyve.*|dowser)\.exe$/i.test(n));
if (blockers.length && !dryRun) {
  console.error("Close these first:", [...new Set(blockers)].join(", "));
  process.exit(1);
}

// 2. Load and check the config.
const raw = fs.readFileSync(cfgPath, "utf8");
const cfg = JSON.parse(raw);
if (cfg.playsets.some(p => p.presentation?.name === NAME)) {
  console.log(`Playset "${NAME}" already exists; nothing to do.`);
  process.exit(0);
}
const installed = fs.readdirSync(path.join(modsDir, "pdx_mods"));
const missing = MODS.filter(([id]) => !installed.some(d => d.startsWith(id + "_")));
if (missing.length) {
  console.error("Not installed:", missing.map(m => m.join(" ")).join(", "));
  process.exit(1);
}

// 3. Build the entry in the same shape as existing ones.
const now = new Date().toISOString().replace(/\.\d{3}Z$/, ".000Z");
const entry = {
  id: crypto.randomUUID(),
  isOffdisk: false,
  isImmutable: false,
  isAutoSorted: false,
  createdAt: now,
  modifiedAt: now,
  modifiedWith: "Cities: Skylines II",
  presentation: { name: NAME, description: "Dakota Falls map editor + Earth2Cities calibration" },
  mods: MODS.map(([id]) => ({ source: "pdx_mods", sourceId: id, isEnabled: true })),
};
const before = JSON.stringify({ active: cfg.activePlaysetId, playsets: cfg.playsets });
cfg.playsets.push(entry);
const out = JSON.stringify(cfg, null, raw.includes("\n  ") ? 2 : undefined);

// 4. Validate before writing.
const re = JSON.parse(out);
if (JSON.stringify({ active: re.activePlaysetId, playsets: re.playsets.slice(0, -1) }) !== before)
  throw new Error("Existing playsets or activePlaysetId changed");

if (dryRun) {
  fs.writeFileSync(path.join(here, "playset_config.preview.json"), out);
  console.log("Dry run OK; blockers seen:", blockers.join(", ") || "none", "; new id", entry.id);
  process.exit(0);
}
const stamp = new Date().toISOString().replace(/[:.]/g, "-");
fs.copyFileSync(cfgPath, path.join(here, `playset_config.backup-${stamp}.json`));
fs.writeFileSync(cfgPath, out);
JSON.parse(fs.readFileSync(cfgPath, "utf8"));
console.log(`Added "${NAME}" (${entry.id}); backup playset_config.backup-${stamp}.json`);
