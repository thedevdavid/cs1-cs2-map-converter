const fs=require('fs');const base=process.argv[2];
const d=JSON.parse(fs.readFileSync(base+'.json','utf8'));const ch=JSON.parse(fs.readFileSync(process.argv[3],'utf8'));
const po=JSON.parse(fs.readFileSync('cs1-offline/out/po-offline.json','utf8')).objects.filter(o=>o.subClass=='bridge/pillar');
const S=new Map(d.segments.map(s=>[s.id,s]));
const bez=(s,t)=>{const [a,b,c,e]=s.bezier,u=1-t;return [0,1,2].map(k=>u*u*u*a[k]+3*u*u*t*b[k]+3*u*t*t*c[k]+t*t*t*e[k])};
const pts=g=>g.segIds.flatMap(id=>{const s=S.get(id);const n=Math.max(2,Math.ceil(s.length/10));return Array.from({length:n+1},(_,i)=>bez(s,i/n))});
const cls=g=>{const p=Object.keys(g.prefabs).join('|');return /Monorail/.test(p)?'monorail':/R2|Train/.test(p)?'rail':/Pedestrian/.test(p)?'path':'road'};
ch.forEach(g=>{g.P=pts(g);g.cls=cls(g)});
const par=ch.map((_,i)=>i);const f=x=>{while(par[x]!=x)x=par[x]=par[par[x]];return x};
const near=(A,B,r)=>{let k=0;for(const a of A){for(const b of B){if((a[0]-b[0])**2+(a[2]-b[2])**2<r*r){k++;break}}}return k/A.length};
for(let i=0;i<ch.length;i++)for(let j=i+1;j<ch.length;j++){const a=ch[i],b=ch[j];if(a.cls!=b.cls)continue;
 if(a.bbox[2]+60<b.bbox[0]||b.bbox[2]+60<a.bbox[0]||a.bbox[3]+60<b.bbox[1]||b.bbox[3]+60<a.bbox[1])continue;
 const [s,l]=a.P.length<b.P.length?[a,b]:[b,a];const r=a.cls=='monorail'?120:45;if(near(s.P,l.P,r)>=0.6)par[f(i)]=f(j)}
const M=new Map();ch.forEach((g,i)=>{const r=f(i);(M.get(r)||M.set(r,[]).get(r)).push(g)});
const bridges=[];for(const gs of M.values()){const P=gs.flatMap(g=>g.P);const pre={};gs.forEach(g=>Object.entries(g.prefabs).forEach(([k,v])=>pre[k]=(pre[k]||0)+v));
 const cr={};gs.forEach(g=>Object.entries(g.crosses).forEach(([k,v])=>cr[k]=(cr[k]||0)+v));const pp={};gs.forEach(g=>Object.entries(g.pillarPrefabs).forEach(([k,v])=>pp[k]=(pp[k]||0)+v));
 const pon={};po.filter(o=>P.some(q=>(q[0]-o.position[0])**2+(q[2]-o.position[2])**2<80*80)).forEach(o=>pon[o.basePrefab]=(pon[o.basePrefab]||0)+1);
 const xs=P.map(q=>q[0]),zs=P.map(q=>q[2]);
 bridges.push({chains:gs.map(g=>g.id),carriageways:gs.length,cls:gs[0].cls,segs:gs.reduce((a,g)=>a+g.segs,0),len:Math.max(...gs.map(g=>g.len)),lenAll:gs.reduce((a,g)=>a+g.len,0),maxT:Math.max(...gs.map(g=>g.maxAboveTerrain)),wet:Math.max(...gs.map(g=>g.wetLen)),maxW:gs.map(g=>g.maxAboveWater).filter(x=>x!=null).reduce((a,b)=>Math.max(a,b),-1e9),
 pillars:gs.reduce((a,g)=>a+g.pillars,0),pillarPrefabs:pp,prefabs:pre,crosses:cr,bld:[...new Set(gs.flatMap(g=>g.nearBridgeBuildings))],po:pon,
 bbox:[Math.min(...xs),Math.min(...zs),Math.max(...xs),Math.max(...zs)].map(Math.round),y:[Math.round(Math.min(...P.map(q=>q[1]))),Math.round(Math.max(...P.map(q=>q[1])))],segIds:gs.flatMap(g=>g.segIds)})}
bridges.forEach(b=>{if(b.maxW<-1e8)b.maxW=null;let sh=1;if(b.bld.length||Object.keys(b.po).length)sh=3;if(b.cls=='monorail')sh=3;b.score=Math.round(b.len*Math.max(b.maxT,1)*sh/1000)});
bridges.sort((a,b)=>b.score-a.score);bridges.forEach((b,i)=>b.id='BR'+String(i+1).padStart(3,'0'));
fs.writeFileSync(process.argv[4],JSON.stringify(bridges.map(({segIds,...r})=>({...r,segIds})),null,0).replace(/\},\{/g,'},\n{'));
const sp=n=>n.replace(/^\d+\./,'').replace(/_Data$/,'');
for(const b of bridges)console.log([b.id,b.score,b.cls,b.carriageways,b.segs,b.len,b.lenAll,b.maxT,b.wet,b.maxW,b.pillars,`(${Math.round((b.bbox[0]+b.bbox[2])/2)},${Math.round((b.bbox[1]+b.bbox[3])/2)})`,b.y.join('-'),Object.entries(b.prefabs).map(([k,v])=>sp(k)+'×'+v).join('; '),Object.entries(b.crosses).map(([k,v])=>sp(k)).join('; '),Object.entries(b.pillarPrefabs).map(([k,v])=>sp(k)+'×'+v).join('; '),b.bld.map(sp).join(','),Object.entries(b.po).map(([k,v])=>sp(k)+'×'+v).join('; ')].join('\t'));
console.log('PO not assigned:',po.filter(o=>!bridges.some(b=>b.po[o.basePrefab]&&b.bbox[0]-80<=o.position[0]&&o.position[0]<=b.bbox[2]+80&&b.bbox[1]-80<=o.position[2]&&o.position[2]<=b.bbox[3]+80)).map(o=>sp(o.basePrefab)+'@'+o.position.map(Math.round).join(',')).join(' | '));
