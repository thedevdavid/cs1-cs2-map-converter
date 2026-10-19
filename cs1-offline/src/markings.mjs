// Intersection Marking Tool and Node Controller data of a CS1 map, decoded offline.
// Usage: node src/markings.mjs <payloadDir> <dump.json> <outDir>
//   payloadDir: from src/extract.mjs (NodeMarkup.unz.bin, NodeController.unz.bin)
//   dump.json:  DakotaExport main JSON of the same map (nodes, segments, netPrefabs give the road geometry)
// Writes imt-markings.geojson, imt-summary.md, nc-summary.md.
//
// Geometry follows IMT 1.15 (decompiled): an entrance is a segment end; its Position is the midpoint of the two
// segment corners, CornerDir runs from the left to the right corner (looking from the node into the segment),
// and enter points sit at the edges of the drive lanes (two points where the gap between lanes is at least
// the mean lane half-width, else one point in the gap), numbered 1..n from left to right. Point id =
// segmentOrNode | index << 16 | type << 24; line id = pointA << 32 | pointB.
// Corners are not stored: they are rebuilt here from the segment edges (Node Controller offsets override),
// so positions near large junctions carry a few metres of error.
import fs from 'node:fs';
import path from 'node:path';

const [payloadDir, dumpPath, outDir] = process.argv.slice(2);
fs.mkdirSync(outDir, { recursive: true });
const dump = JSON.parse(fs.readFileSync(dumpPath, 'utf8'));
const imtXml = fs.readFileSync(path.join(payloadDir, 'NodeMarkup.unz.bin'), 'utf8');
const ncXml = fs.readFileSync(path.join(payloadDir, 'NodeController.unz.bin'), 'utf8');

// ---------- small XML reader (flat attributes, nested elements) ----------
function parseXml(s) {
  const root = { tag: '#root', attrs: {}, children: [] }, stack = [root];
  for (const m of s.matchAll(/<(\/?)([\w.]+)((?:\s+[\w.]+="[^"]*")*)\s*(\/?)>/g)) {
    if (m[1]) { stack.pop(); continue; }
    const el = { tag: m[2], attrs: {}, children: [] };
    for (const a of m[3].matchAll(/([\w.]+)="([^"]*)"/g)) el.attrs[a[1]] = a[2].replace(/&quot;/g, '"').replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&');
    stack.at(-1).children.push(el);
    if (!m[4]) stack.push(el);
  }
  return root;
}

// ---------- vector helpers (CS1 world: x east, y up, z north) ----------
const add = (a, b) => [a[0] + b[0], a[1] + b[1], a[2] + b[2]];
const sub = (a, b) => [a[0] - b[0], a[1] - b[1], a[2] - b[2]];
const mul = (a, s) => [a[0] * s, a[1] * s, a[2] * s];
const lerp = (a, b, t) => add(a, mul(sub(b, a), t));
const len2 = a => Math.hypot(a[0], a[2]);
const norm2 = a => { const l = len2(a) || 1; return [a[0] / l, 0, a[2] / l]; };
const right = d => [d[2], 0, -d[0]];   // right of a horizontal direction (x east, z north)
const bez = (B, t) => { const u = 1 - t; return [0, 1, 2].map(k => u * u * u * B[0][k] + 3 * u * u * t * B[1][k] + 3 * u * t * t * B[2][k] + t * t * t * B[3][k]); };
const bezTan = (B, t) => { const u = 1 - t; return [0, 1, 2].map(k => 3 * u * u * (B[1][k] - B[0][k]) + 6 * u * t * (B[2][k] - B[1][k]) + 3 * t * t * (B[3][k] - B[2][k])); };
const round = (v, d = 2) => Math.round(v * 10 ** d) / 10 ** d;

// ---------- network ----------
const nodes = new Map(dump.nodes.map(n => [n.id, n]));
const segs = new Map(dump.segments.map(s => [s.id, s]));
const prefabs = dump.netPrefabs;
const segsAt = new Map();
for (const s of dump.segments) for (const n of [s.startNode, s.endNode]) (segsAt.get(n) ?? segsAt.set(n, []).get(n)).push(s);

// Node Controller: per segment end offset (O) overrides the rebuilt corner offset
const nc = parseXml(ncXml).children[0];
const ncOffset = new Map();   // `${node}:${seg}` -> metres
for (const n of nc?.children ?? []) for (const se of n.children.filter(c => c.tag === 'SE')) ncOffset.set(`${n.attrs.Id}:${se.attrs.Id}`, +se.attrs.O || 0);

// samples of a segment from one end, as arc length -> position, tangent pointing away from the node
const sampleCache = new Map();
function samples(seg, fromStart) {
  const key = `${seg.id}:${fromStart}`;
  if (sampleCache.has(key)) return sampleCache.get(key);
  const B = seg.bezier, out = []; let acc = 0, prev = null;
  for (let i = 0; i <= 64; i++) {
    const t = fromStart ? i / 64 : 1 - i / 64;
    const p = bez(B, t); let d = bezTan(B, t); if (!fromStart) d = mul(d, -1);
    if (prev) acc += len2(sub(p, prev));
    out.push({ s: acc, p, d: norm2(d), t }); prev = p;
  }
  sampleCache.set(key, out); return out;
}
const at = (smp, s) => {
  if (s <= 0) return smp[0];
  for (let i = 1; i < smp.length; i++) if (smp[i].s >= s) { const a = smp[i - 1], b = smp[i], f = (s - a.s) / Math.max(1e-9, b.s - a.s); return { s, p: lerp(a.p, b.p, f), d: norm2(lerp(a.d, b.d, f)), t: a.t + (b.t - a.t) * f }; }
  return smp.at(-1);
};
const halfWidth = seg => prefabs[seg.prefab]?.halfWidth ?? 4;
const heading = d => Math.atan2(d[0], d[2]);

// first intersection (arc length on edge A) of two offset edges
function edgeHit(segA, startA, sideA, segB, startB, sideB) {
  const A = samples(segA, startA), B = samples(segB, startB), ha = halfWidth(segA), hb = halfWidth(segB);
  const ea = A.map(q => add(q.p, mul(right(q.d), sideA * ha))), eb = B.map(q => add(q.p, mul(right(q.d), sideB * hb)));
  for (let i = 1; i < ea.length; i++) for (let j = 1; j < eb.length; j++) {
    const p = ea[i - 1], r = sub(ea[i], p), q = eb[j - 1], s = sub(eb[j], q);
    const den = r[0] * s[2] - r[2] * s[0]; if (Math.abs(den) < 1e-9) continue;
    const qp = sub(q, p), t = (qp[0] * s[2] - qp[2] * s[0]) / den, u = (qp[0] * r[2] - qp[2] * r[0]) / den;
    if (t >= 0 && t <= 1 && u >= 0 && u <= 1) return A[i - 1].s + t * (A[i].s - A[i - 1].s);
  }
  return 0;
}

// entrance = one segment end at a node
const entCache = new Map();
function entrance(nodeId, segId) {
  const key = `${nodeId}:${segId}`;
  if (entCache.has(key)) return entCache.get(key);
  const seg = segs.get(segId), node = nodes.get(nodeId);
  if (!seg || !node) { entCache.set(key, null); return null; }
  const isStart = seg.startNode === nodeId;
  const smp = samples(seg, isStart), d0 = smp[0].d;
  let offL = 0, offR = 0;
  const others = (segsAt.get(nodeId) ?? []).filter(o => o.id !== segId);
  if (others.length >= 2) {
    // neighbours by heading: left (counter-clockwise) and right (clockwise) of this segment's direction
    const h0 = heading(d0);
    const rel = others.map(o => { const os = o.startNode === nodeId; const h = heading(samples(o, os)[0].d); let dh = h - h0; while (dh <= 0) dh += 2 * Math.PI; return { o, os, dh }; });
    const cw = rel.reduce((a, b) => (b.dh < a.dh ? b : a)), ccw = rel.reduce((a, b) => (b.dh > a.dh ? b : a));
    offR = edgeHit(seg, isStart, +1, cw.o, cw.os, -1);
    offL = edgeHit(seg, isStart, -1, ccw.o, ccw.os, +1);
  }
  const ncO = ncOffset.get(key);
  if (ncO) { offL = Math.max(offL, ncO); offR = Math.max(offR, ncO); }
  const maxS = smp.at(-1).s * 0.5;
  offL = Math.min(offL, maxS); offR = Math.min(offR, maxS);
  const hw = halfWidth(seg);
  const qL = at(smp, offL), qR = at(smp, offR);
  const leftCorner = add(qL.p, mul(right(qL.d), -hw)), rightCorner = add(qR.p, mul(right(qR.d), hw));
  const position = lerp(leftCorner, rightCorner, 0.5);
  const cornerDir = norm2(sub(rightCorner, leftCorner));
  const normal = norm2(add(qL.d, qR.d));
  const ratio = Math.max(0.2, Math.abs(normal[0] * cornerDir[2] - normal[2] * cornerDir[0]));   // sin(angle(normal, corner))
  // drive lanes (IMT: road -> Car|Bicycle|Tram|Trolleybus, track -> train-like, path -> pedestrian), lateral to the right of the into-segment direction
  const pf = prefabs[seg.prefab] ?? { lanes: [], ai: '' };
  const ai = pf.ai ?? '';
  const isRoad = /^Road(Bridge|Tunnel)?AI$|RoadBaseAI|Damaged/.test(ai), isTrack = /TrainTrack|MetroTrack/.test(ai), isPath = /^Pedestrian/.test(ai);
  const lanes = (pf.lanes ?? []).filter(l => isRoad ? /Car|Bicycle|Tram|Trolleybus/.test(l.vehicleType ?? '')
    : isTrack ? /Train|Metro|Monorail/.test(l.vehicleType ?? '') : isPath ? (/Bicycle/.test(l.vehicleType ?? '') || /Pedestrian/.test(l.type ?? '')) : false)
    .map(l => ({ lat: l.position * (seg.invert ? -1 : 1) * (isStart ? 1 : -1), hw: l.width / 2 }))
    .sort((a, b) => a.lat - b.lat);
  const pts = [];
  for (let i = 0; i <= lanes.length && lanes.length; i++) {
    const L = lanes[i - 1], R = lanes[i];
    if (!L) pts.push(R.lat - R.hw);
    else if (!R) pts.push(L.lat + L.hw);
    else {
      const gap = Math.abs((R.lat - R.hw) - (L.lat + L.hw));
      if (gap >= (L.hw + R.hw) * 0.5) { pts.push(L.lat + L.hw, R.lat - R.hw); }
      else pts.push(((L.lat + L.hw) + (R.lat - R.hw)) / 2);
    }
  }
  const e = { nodeId, segId, isStart, position, cornerDir, ratio, normal, into: d0, pointLat: pts, offL, offR, y: position[1] };
  entCache.set(key, e); return e;
}
const pointPos = (e, idx, offset = 0) => {
  const lat = e.pointLat[idx - 1];
  if (lat === undefined) return null;
  return add(e.position, mul(e.cornerDir, (lat + offset) / e.ratio));
};

// ---------- styles ----------
const REG = { 257: 'Solid', 258: 'Dashed', 259: 'DoubleSolid', 260: 'DoubleDashed', 261: 'SolidAndDashed', 262: 'SharkTeeth', 263: 'DoubleDashedAsym', 264: 'ZigZag', 385: 'Pavement', 401: 'Prop', 402: 'Tree', 403: 'Text', 404: 'Decal', 417: 'Network', 510: 'Empty' };
const FILL = { 1025: 'Stripe', 1026: 'Grid', 1027: 'Solid', 1028: 'Chevron', 1029: 'Decal', 1030: 'Asphalt', 1153: 'Pavement', 1154: 'Grass', 1155: 'Gravel', 1156: 'Ruined', 1157: 'Cliff', 1158: 'Texture' };
const STOP = { 513: 'Solid', 514: 'Dashed', 515: 'DoubleSolid', 516: 'DoubleDashed', 517: 'SolidAndDashed', 518: 'SharkTeeth', 519: 'Pavement' };
// stored code = StyleType - (item + 1) for lines (item 256) and StyleType - 513 for fillers (checked against the attribute sets)
const lineStyle = (lineType, code) => lineType === 512 ? 'Stop' + (STOP[code + 513] ?? code) : lineType === 2048 ? 'Crosswalk' + code : (REG[code + 257] ?? `code${code}`);
const fillStyle = code => FILL[code + 513] ?? `code${code}`;
const argb = c => '#' + (Number(c) >>> 0).toString(16).padStart(8, '0');

// ---------- decode markings ----------
const features = [];
const stats = { nodeMarkings: 0, segmentMarkings: 0, lines: 0, rules: 0, fillers: 0, unresolvedPoints: 0, unresolvedLines: 0, zeroLength: 0, byStyle: {}, fillerByStyle: {}, barrierKm: 0, paintKm: 0, fillerAreaM2: 0, prefabs: {} };
const imt = parseXml(imtXml).children[0];

function polyLen(c) { let l = 0; for (let i = 1; i < c.length; i++) l += Math.hypot(c[i][0] - c[i - 1][0], c[i][2] - c[i - 1][2]); return l; }
function subPoly(c, t0, t1) {
  // c: polyline, t in [0,1] by arc length
  const L = polyLen(c); if (L === 0) return c;
  const want = [Math.min(t0, t1) * L, Math.max(t0, t1) * L]; const out = []; let acc = 0;
  for (let i = 1; i < c.length; i++) {
    const seg = Math.hypot(c[i][0] - c[i - 1][0], c[i][2] - c[i - 1][2]); const a = acc, b = acc + seg;
    if (b >= want[0] && a <= want[1]) {
      if (!out.length) out.push(lerp(c[i - 1], c[i], seg ? (want[0] - a) / seg : 0));
      if (b <= want[1]) out.push(c[i]); else { out.push(lerp(c[i - 1], c[i], seg ? (want[1] - a) / seg : 0)); break; }
    }
    acc = b;
  }
  return t0 <= t1 ? out : out.reverse();
}
function hitT(c, d) {   // first intersection of polyline c with polyline d, as arc-length fraction of c
  const L = polyLen(c); let acc = 0;
  for (let i = 1; i < c.length; i++) {
    const p = c[i - 1], r = sub(c[i], p), segL = Math.hypot(r[0], r[2]);
    for (let j = 1; j < d.length; j++) {
      const q = d[j - 1], s = sub(d[j], q); const den = r[0] * s[2] - r[2] * s[0]; if (Math.abs(den) < 1e-12) continue;
      const qp = sub(q, p), t = (qp[0] * s[2] - qp[2] * s[0]) / den, u = (qp[0] * r[2] - qp[2] * r[0]) / den;
      if (t >= -1e-6 && t <= 1 + 1e-6 && u >= -1e-6 && u <= 1 + 1e-6) return L ? (acc + t * segL) / L : 0;
    }
    acc += segL;
  }
  return null;
}

for (const mk of imt?.children ?? []) {
  const isNode = mk.tag === 'M', isSeg = mk.tag === 'S' && mk.attrs.Id !== undefined;
  if (!isNode && !isSeg) continue;
  const mid = +mk.attrs.Id;
  isNode ? stats.nodeMarkings++ : stats.segmentMarkings++;
  const pOff = new Map(mk.children.filter(c => c.tag === 'P').map(c => [+c.attrs.Id, +c.attrs.O || 0]));
  // point id -> entrance + index
  const resolve = pid => {
    const enterId = pid & 0xffff, idx = (pid >>> 16) & 0xff;
    const e = isNode ? entrance(mid, enterId) : entrance(enterId, mid);
    if (!e) return null;
    const p = pointPos(e, idx, pOff.get(pid & 0xffffff) ?? pOff.get(pid) ?? 0);
    return p ? { e, p, idx } : null;
  };
  const lineGeom = new Map();   // lineId -> polyline
  const lineOf = id => {
    if (lineGeom.has(id)) return lineGeom.get(id);
    const big = BigInt(id), a = Number(big >> 32n), b = Number(big & 0xffffffffn);
    const A = resolve(a), B = resolve(b); let c = null;
    if (A && B) {
      if (A.e === B.e) c = [A.p, B.p];
      else if (isSeg) {
        // along the segment: offset curve between the two ends
        const seg = segs.get(mid); const sA = A.e.isStart ? A : B, sB = A.e.isStart ? B : A;
        const latA = (sA.p && dot2(sub(sA.p, sA.e.position), sA.e.cornerDir)), latB = -dot2(sub(sB.p, sB.e.position), sB.e.cornerDir);
        const smp = samples(seg, true), L = smp.at(-1).s, s0 = sA.e === A.e ? 0 : 0;
        const from = Math.max(sA.e.offL, sA.e.offR) * 0.5 + Math.min(sA.e.offL, sA.e.offR) * 0.5, to = L - (Math.max(sB.e.offL, sB.e.offR) * 0.5 + Math.min(sB.e.offL, sB.e.offR) * 0.5);
        c = [];
        for (let i = 0; i <= 32; i++) { const f = i / 32, q = at(smp, from + (to - from) * f); c.push(add(q.p, mul(right(q.d), latA + (latB - latA) * f))); }
        c[0] = sA.p; c[c.length - 1] = sB.p;
        if (sA !== A) c.reverse();
      } else {
        // through the junction: cubic Bézier, tangents pointing into the node
        const d = len2(sub(B.p, A.p)) / 3, b1 = add(A.p, mul(A.e.into, -d)), b2 = add(B.p, mul(B.e.into, -d));
        c = []; for (let i = 0; i <= 16; i++) c.push(bez([A.p, b1, b2, B.p], i / 16));
      }
    } else stats.unresolvedPoints += (A ? 0 : 1) + (B ? 0 : 1);
    lineGeom.set(id, c); return c;
  };
  const lines = mk.children.filter(c => c.tag === 'L');
  for (const l of lines) {
    stats.lines++;
    const c = lineOf(l.attrs.Id), lineType = +l.attrs.T;
    if (!c) { stats.unresolvedLines++; continue; }
    for (const r of l.children.filter(x => x.tag === 'R')) {
      stats.rules++;
      const es = r.children.filter(x => x.tag === 'E'), st = r.children.find(x => x.tag === 'S');
      const tOf = e => {
        if (+e.attrs.T === 0) { const pid = +e.attrs.P, big = BigInt(l.attrs.Id); return pid === Number(big >> 32n) ? 0 : pid === Number(big & 0xffffffffn) ? 1 : null; }
        const other = lineOf(e.attrs.L); return other ? hitT(c, other) : null;
      };
      let t0 = es[0] ? tOf(es[0]) : 0, t1 = es[1] ? tOf(es[1]) : 1;
      if (t0 === null) t0 = 0; if (t1 === null) t1 = 1;
      const coords = subPoly(c, t0, t1).filter((p, i, a) => i === 0 || Math.hypot(p[0] - a[i - 1][0], p[2] - a[i - 1][2]) > 0.01);
      if (coords.length < 2 || polyLen(coords) < 0.1) { stats.zeroLength++; continue; }
      const style = lineStyle(lineType, +st.attrs.T), prefab = st.attrs.PRF ?? null;
      const category = style === 'Network' ? (/barrier|guard|rail|fence|wall/i.test(prefab ?? '') ? 'barrier' : 'network') : 'paint';
      const L = polyLen(coords) / 1000;
      if (category === 'barrier') stats.barrierKm += L; else stats.paintKm += L;
      stats.byStyle[style] = (stats.byStyle[style] ?? 0) + 1;
      if (prefab) stats.prefabs[prefab] = (stats.prefabs[prefab] ?? 0) + 1;
      features.push({ type: 'Feature', geometry: { type: 'LineString', coordinates: coords.map(p => [round(p[0]), round(p[2]), round(p[1])]) },
        properties: { kind: 'line', category, style, styleCode: +st.attrs.T, prefab, color: st.attrs.C ? argb(st.attrs.C) : null, width: st.attrs.W ? round(+st.attrs.W, 3) : null,
          markingType: isNode ? 'node' : 'segment', markingId: mid, lineId: l.attrs.Id, lengthM: round(L * 1000, 1) } });
    }
  }
  for (const f of mk.children.filter(c => c.tag === 'F')) {
    stats.fillers++;
    const st = f.children.find(x => x.tag === 'S'), vs = f.children.filter(x => x.tag === 'V');
    const verts = vs.map(v => {
      if (+v.attrs.T === 0) { const r = resolve(+v.attrs.P); return r ? { p: r.p, lines: [v.attrs.L] } : null; }
      const a = lineOf(v.attrs.L1), b = lineOf(v.attrs.L2); if (!a || !b) return null;
      const t = hitT(a, b); return t === null ? null : { p: subPoly(a, 0, t).at(-1), lines: [v.attrs.L1, v.attrs.L2] };
    });
    if (verts.some(v => !v)) { stats.unresolvedPoints++; continue; }
    const ring = [];
    for (let i = 0; i < verts.length; i++) {
      const a = verts[i], b = verts[(i + 1) % verts.length];
      const shared = a.lines.find(x => x && b.lines.includes(x));
      const c = shared ? lineOf(shared) : null;
      if (c) {
        const ta = nearestT(c, a.p), tb = nearestT(c, b.p);
        ring.push(...subPoly(c, ta, tb).slice(0, -1));
      } else ring.push(a.p);
    }
    ring.push(ring[0]);
    const style = fillStyle(+st.attrs.T);
    const area = Math.abs(ring.reduce((s, p, i) => i ? s + (ring[i - 1][0] * p[2] - p[0] * ring[i - 1][2]) : 0, 0)) / 2;
    stats.fillerByStyle[style] = (stats.fillerByStyle[style] ?? 0) + 1; stats.fillerAreaM2 += area;
    features.push({ type: 'Feature', geometry: { type: 'Polygon', coordinates: [ring.map(p => [round(p[0]), round(p[2]), round(p[1])])] },
      properties: { kind: 'filler', style, styleCode: +st.attrs.T, texture: st.attrs.DCL ?? null, elevation: st.attrs.E ? round(+st.attrs.E, 2) : null,
        color: st.attrs.C ? argb(st.attrs.C) : null, markingType: isNode ? 'node' : 'segment', markingId: mid, areaM2: round(area, 1) } });
  }
}
function dot2(a, b) { return a[0] * b[0] + a[2] * b[2]; }
function nearestT(c, p) {
  const L = polyLen(c); let best = 1e18, bt = 0, acc = 0;
  for (let i = 1; i < c.length; i++) {
    const a = c[i - 1], r = sub(c[i], a), l = Math.hypot(r[0], r[2]); const f = l ? Math.max(0, Math.min(1, dot2(sub(p, a), r) / (l * l))) : 0;
    const q = add(a, mul(r, f)), d = Math.hypot(q[0] - p[0], q[2] - p[2]); if (d < best) { best = d; bt = L ? (acc + f * l) / L : 0; } acc += l;
  }
  return bt;
}

fs.writeFileSync(path.join(outDir, 'imt-markings.geojson'), JSON.stringify({
  type: 'FeatureCollection',
  name: 'dakota-falls-imt-markings',
  crs_note: 'CS1 world metres, not geographic: coordinates are [x east, z north, y height]; origin = map centre',
  schema: 'dakota-offline/cs1/imt/v1',
  features,
}).replace(/\},\{"type":"Feature"/g, '},\n{"type":"Feature"'));

// ---------- summaries ----------
const md = [];
md.push('# Intersection Marking Tool: summary', '');
md.push(`Decoded from the map's \`NodeMarkup\` data (IMT ${imt?.attrs.V}). Generated by \`src/markings.mjs\`; geometry in \`imt-markings.geojson\` (CS1 metres, [x, z, y]).`, '');
md.push('| item | count |', '|---|---:|');
md.push(`| node markings (junctions) | ${stats.nodeMarkings} |`, `| segment markings (along one segment) | ${stats.segmentMarkings} |`, `| lines | ${stats.lines} |`, `| line rules (features) | ${stats.rules} |`, `| fillers (polygons) | ${stats.fillers} |`);
md.push(`| barrier length | ${stats.barrierKm.toFixed(1)} km |`, `| painted line length | ${stats.paintKm.toFixed(1)} km |`, `| filler area | ${(stats.fillerAreaM2 / 1e4).toFixed(2)} ha |`);
md.push(`| rules dropped as zero length (both points at the same spot, e.g. across a two-segment node) | ${stats.zeroLength} |`, `| unresolved points / lines | ${stats.unresolvedPoints} / ${stats.unresolvedLines} |`, '');
md.push('Line styles (per rule):', '', '| style | rules |', '|---|---:|');
for (const [k, v] of Object.entries(stats.byStyle).sort((a, b) => b[1] - a[1])) md.push(`| ${k} | ${v} |`);
md.push('', 'Network-line prefabs:', '', '| prefab | rules |', '|---|---:|');
for (const [k, v] of Object.entries(stats.prefabs)) md.push(`| \`${k}\` | ${v} |`);
md.push('', 'Filler styles:', '', '| style | fillers |', '|---|---:|');
for (const [k, v] of Object.entries(stats.fillerByStyle).sort((a, b) => b[1] - a[1])) md.push(`| ${k} | ${v} |`);
md.push('', '## Method and accuracy', '',
  '- Point positions follow IMT 1.15 (decompiled `Entrance`, `NetInfoPointSource`): points at drive-lane edges on the corner line of each segment end, numbered left to right looking from the node into the segment; point `O` offsets are applied.',
  '- Segment corners are not stored in the save. They are rebuilt from the intersection of neighbouring segment edges (Bézier from the dump, prefab half-width); Node Controller offsets override. Error: about 0 m at two-segment nodes, a few metres at large junctions (no `m_minCornerOffset`, no pavement width in the dump).',
  '- Segment-marking lines follow the segment Bézier with the lateral offset interpolated between both ends; junction lines are cubic Béziers between the two points with tangents into the node; rules are cut at line intersections.',
  '- Style names come from IMT\'s `Style.StyleType` enum; the XML stores `StyleType - 257` for lines and `StyleType - 513` for fillers.',
  '- `category`: `barrier` = network line whose prefab is a barrier/guardrail/fence, `network` = other network lines, `paint` = painted lines.');
fs.writeFileSync(path.join(outDir, 'imt-summary.md'), md.join('\n') + '\n');

const NT = ['Middle', 'Bend', 'Stretch', 'Crossing', 'UTurn', 'Custom', 'End'];
const MODE = { 1: 'Flat', 2: 'Slope', 4: 'FreeForm' };
const ncRows = [], typeCount = {};
for (const n of nc?.children ?? []) {
  const node = nodes.get(+n.attrs.Id), t = NT[+n.attrs.T] ?? n.attrs.T; typeCount[t] = (typeCount[t] ?? 0) + 1;
  const ses = n.children.filter(c => c.tag === 'SE');
  const f = (k, d = 0) => ses.map(s => (+s.attrs[k] || 0)).some(v => v !== d) ? ses.map(s => round(+s.attrs[k] || 0, 2)).join(' / ') : '';
  ncRows.push(`| ${n.attrs.Id} | ${node ? round(node.position[0], 0) : ''} | ${node ? round(node.position[2], 0) : ''} | ${node?.prefab?.replace(/^\d+\./, '') ?? ''} | ${t} | ${ses.length} | ${ses.map(s => s.attrs.Id).join(' / ')} | ${ses.map(s => MODE[s.attrs.M] ?? s.attrs.M).join(' / ')} | ${f('O')} | ${f('S')} | ${f('RA')} | ${f('SA')} | ${f('ST', 1)} |`);
}
const nmd = ['# Node Controller: per-node summary', '',
  `Decoded from the map's \`NodeController\` data (Node Controller ${nc?.attrs.V}). ${ncRows.length} nodes: ${Object.entries(typeCount).map(([k, v]) => `${k} ${v}`).join(', ')}. Values per segment end, in the order of the segment column (offset/shift in metres, rotate/slope in degrees, stretch as a factor; empty = default).`, '',
  '| node | x | z | net | type | ends | segments | mode | offset | shift | rotate | slope | stretch |', '|---:|---:|---:|---|---|---:|---|---|---|---|---|---|---|', ...ncRows];
fs.writeFileSync(path.join(outDir, 'nc-summary.md'), nmd.join('\n') + '\n');
console.log(JSON.stringify({ ...stats, barrierKm: round(stats.barrierKm, 2), paintKm: round(stats.paintKm, 2), fillerAreaM2: round(stats.fillerAreaM2, 0), ncNodes: ncRows.length, ncTypes: typeCount, features: features.length }));
