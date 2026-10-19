// Fetch Steam Workshop details for: all installed workshop folders, the map collection,
// the theme-mix collection and LSM-used ids; recurse `children` to a fixed point.
// Output: steam-cache.json  { details: {id: {...}}, collections: {id: [children]} }
import fs from 'node:fs';
import path from 'node:path';

const WS = 'D:/SteamLibrary/steamapps/workshop/content/255710';
const OUT = new URL('./steam-cache.json', import.meta.url);
const cache = fs.existsSync(OUT) ? JSON.parse(fs.readFileSync(OUT, 'utf8')) : { details: {}, collections: {} };

async function post(url, ids, key) {
  const body = new URLSearchParams();
  body.set(key, String(ids.length));
  ids.forEach((id, i) => body.set(`publishedfileids[${i}]`, String(id)));
  for (let attempt = 0; attempt < 4; attempt++) {
    const r = await fetch(url, { method: 'POST', body });
    if (r.ok) return r.json();
    await new Promise(res => setTimeout(res, 2000 * (attempt + 1)));
  }
  throw new Error(`HTTP fail ${url}`);
}

async function details(ids) {
  ids = [...new Set(ids.map(String))].filter(id => !cache.details[id]);
  for (let i = 0; i < ids.length; i += 100) {
    const chunk = ids.slice(i, i + 100);
    const j = await post('https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/', chunk, 'itemcount');
    for (const d of j.response.publishedfiledetails) {
      cache.details[d.publishedfileid] = {
        result: d.result, title: d.title, tags: (d.tags || []).map(t => t.tag),
        file_size: d.file_size, creator: d.creator, time_updated: d.time_updated,
        consumer_app_id: d.consumer_app_id, banned: d.banned, file_type: d.file_type,
      };
    }
  }
}

async function collections(ids) {
  ids = [...new Set(ids.map(String))].filter(id => !cache.collections[id]);
  if (!ids.length) return;
  const j = await post('https://api.steampowered.com/ISteamRemoteStorage/GetCollectionDetails/v1/', ids, 'collectioncount');
  for (const c of j.response.collectiondetails) {
    cache.collections[c.publishedfileid] = (c.children || []).map(ch => ({ id: ch.publishedfileid, filetype: ch.filetype }));
  }
}

const seeds = JSON.parse(process.argv[2] || '[]').map(String);
const installed = fs.readdirSync(WS).filter(n => /^\d+$/.test(n));
await details([...installed, ...seeds]);

// Fixed point: any item may have children (required items); collections expose them via GetCollectionDetails.
let frontier = [...new Set([...installed, ...seeds])];
const seen = new Set();
while (frontier.length) {
  const batch = frontier.filter(id => !seen.has(id));
  batch.forEach(id => seen.add(id));
  if (!batch.length) break;
  for (let i = 0; i < batch.length; i += 100) await collections(batch.slice(i, i + 100));
  const kids = batch.flatMap(id => (cache.collections[id] || []).map(c => c.id));
  await details(kids);
  frontier = kids;
}
fs.writeFileSync(OUT, JSON.stringify(cache, null, 1));
console.log('details', Object.keys(cache.details).length, 'withChildren', Object.values(cache.collections).filter(c => c.length).length);
