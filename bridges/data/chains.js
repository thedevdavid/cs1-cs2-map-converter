const fs=require('fs'),zlib=require('zlib');
const base=process.argv[2];const d=JSON.parse(fs.readFileSync(base+'.json','utf8'));
function png16(f){const b=fs.readFileSync(f);let p=8,w,h,ct,bd,idat=[];while(p<b.length){const len=b.readUInt32BE(p),t=b.toString('ascii',p+4,p+8),dd=b.slice(p+8,p+8+len);if(t=='IHDR'){w=dd.readUInt32BE(0);h=dd.readUInt32BE(4);bd=dd[8];ct=dd[9]}if(t=='IDAT')idat.push(dd);p+=12+len}
 const raw=zlib.inflateSync(Buffer.concat(idat));const bpp=bd/8*(ct==0?1:ct==4?2:ct==2?3:4),stride=w*bpp;const out=new Uint16Array(w*h);let prev=Buffer.alloc(stride);
 for(let y=0;y<h;y++){const ft=raw[y*(stride+1)],line=Buffer.from(raw.slice(y*(stride+1)+1,(y+1)*(stride+1)));for(let i=0;i<stride;i++){const a=i>=bpp?line[i-bpp]:0,up=prev[i],c=i>=bpp?prev[i-bpp]:0;let v=line[i];if(ft==1)v+=a;else if(ft==2)v+=up;else if(ft==3)v+=(a+up)>>1;else if(ft==4){const pp=a+up-c,pa=Math.abs(pp-a),pb=Math.abs(pp-up),pc=Math.abs(pp-c);v+=(pa<=pb&&pa<=pc)?a:(pb<=pc?up:c)}line[i]=v&255}
  for(let x=0;x<w;x++)out[y*w+x]=line[x*bpp]<<8|line[x*bpp+1];prev=line}return {w,h,out}}
const H=png16(base+'-heights-raw.png'),W=png16(base+'-water.png');
function samp(g,x,z){const c=Math.round((x+8640)/16),r=Math.round((8640-z)/16);if(c<0||r<0||c>=g.w||r>=g.h)return 0;return g.out[r*g.w+c]/64}
const P=d.netPrefabs,nodes=new Map(d.nodes.map(n=>[n.id,n]));
const isBr=n=>{const p=P[n];if(!p)return false;return /BridgeAI$/.test(p.ai)||/ (B|E)0?$/.test(n)||/Elevated|Bridge/.test(n)};
const isMono=n=>/Monorail/.test(n);
const bez=(s,t)=>{const [a,b,c,e]=s.bezier,u=1-t;return [0,1,2].map(k=>u*u*u*a[k]+3*u*u*t*b[k]+3*u*t*t*c[k]+t*t*t*e[k])};
const segs=d.segments.filter(s=>isBr(s.prefab)||isMono(s.prefab));
// ground node: OnGround flag and |y-terrain|<1
const ground=id=>{const n=nodes.get(id);return !n||n.flags.includes("OnGround")||Math.abs(n.position[1]-n.terrain)<1.5};
const par=new Map();const f=x=>{while(par.get(x)!==x){par.set(x,par.get(par.get(x)));x=par.get(x)}return x};
segs.forEach(s=>par.set(s.id,s.id));
const byNode=new Map();for(const s of segs)for(const nid of [s.startNode,s.endNode]){if(ground(nid))continue;const k=nid+(isMono(s.prefab)?'m':'');(byNode.get(k)||byNode.set(k,[]).get(k)).push(s.id)}
for(const ids of byNode.values())for(let i=1;i<ids.length;i++)par.set(f(ids[i]),f(ids[0]));
const groups=new Map();for(const s of segs){const r=f(s.id);(groups.get(r)||groups.set(r,[]).get(r)).push(s)}
// other segments for crossing detection: grid index of polylines
const others=d.segments.filter(s=>!isBr(s.prefab)&&!isMono(s.prefab)&&/Road|Train|Highway|Pedestrian|Ship/.test(P[s.prefab]?.ai+P[s.prefab]?.service+P[s.prefab]?.subService)&&!/Field|Terraform|Retaining|Airplane|Power/.test(s.prefab));
const pts=s=>{const n=Math.max(2,Math.ceil(s.length/8));return Array.from({length:n+1},(_,i)=>bez(s,i/n))};
const grid=new Map();const gk=(x,z)=>Math.floor(x/32)+','+Math.floor(z/32);
for(const s of others){const p=pts(s);for(const q of p){const k=gk(q[0],q[2]);(grid.get(k)||grid.set(k,new Set()).get(k)).add(s)}}
const allB=d.buildings.concat(d.skippedBuildings);const pil=allB.filter(b=>/Pillar|Pylon/i.test(b.prefab)&&!/kV|Power/.test(b.prefab));
const bridgeBld=d.buildings.filter(b=>/Bridge_Data/.test(b.prefab));
const res=[];
for(const [r,ss] of groups){let len=0,maxT=-1e9,maxW=null,wetLen=0,minx=1e9,maxx=-1e9,minz=1e9,maxz=-1e9,cross=new Map(),prefabs={},ptsAll=[];
 for(const s of ss){len+=s.length;prefabs[s.prefab]=(prefabs[s.prefab]||0)+1;const p=pts(s);const step=s.length/(p.length-1);
  for(const q of p){ptsAll.push(q);minx=Math.min(minx,q[0]);maxx=Math.max(maxx,q[0]);minz=Math.min(minz,q[2]);maxz=Math.max(maxz,q[2]);const t=samp(H,q[0],q[2]),w=samp(W,q[0],q[2]);maxT=Math.max(maxT,q[1]-t);if(w>0){wetLen+=step;maxW=Math.max(maxW??-1e9,q[1]-w)}
   const c=grid.get(gk(q[0],q[2]));if(c)for(const o of c){const op=pts(o);for(const oq of op){const dx=oq[0]-q[0],dz=oq[2]-q[2];if(dx*dx+dz*dz<36&&q[1]-oq[1]>4){cross.set(o.prefab,(cross.get(o.prefab)||0)+1);break}}}}}
 // node heights
 const nids=new Set(ss.flatMap(s=>[s.startNode,s.endNode]));let maxN=0;for(const id of nids){const n=nodes.get(id);if(n)maxN=Math.max(maxN,n.position[1]-n.terrain)}
 const near=(b)=>ptsAll.some(q=>(q[0]-b.position[0])**2+(q[2]-b.position[2])**2<225);
 const pl=pil.filter(b=>b.position[0]>=minx-15&&b.position[0]<=maxx+15&&b.position[2]>=minz-15&&b.position[2]<=maxz+15&&near(b));
 const plc={};pl.forEach(b=>plc[b.prefab]=(plc[b.prefab]||0)+1);
 const bb=bridgeBld.filter(b=>ptsAll.some(q=>(q[0]-b.position[0])**2+(q[2]-b.position[2])**2<150*150)).map(b=>b.prefab+'#'+b.id);
 const ends=[...nids].filter(id=>ground(id)).length;
 res.push({id:r,segs:ss.length,segIds:ss.map(s=>s.id),prefabs,len:+len.toFixed(0),maxAboveTerrain:+maxT.toFixed(1),maxNodeElev:+maxN.toFixed(1),wetLen:+wetLen.toFixed(0),maxAboveWater:maxW==null?null:+maxW.toFixed(1),bbox:[minx,minz,maxx,maxz].map(v=>Math.round(v)),centre:[Math.round((minx+maxx)/2),Math.round((minz+maxz)/2)],y:[Math.round(Math.min(...ptsAll.map(q=>q[1]))),Math.round(Math.max(...ptsAll.map(q=>q[1])))],pillars:pl.length,pillarPrefabs:plc,crosses:Object.fromEntries([...cross].sort((a,b)=>b[1]-a[1]).slice(0,4)),nearBridgeBuildings:bb,groundEnds:ends})}
res.sort((a,b)=>b.len*Math.max(b.maxAboveTerrain,1)-a.len*Math.max(a.maxAboveTerrain,1));
fs.writeFileSync(process.argv[3],JSON.stringify(res,null,1));
console.log('groups',res.length);for(const g of res.slice(0,200))console.log(g.id,g.segs,g.len,g.maxAboveTerrain,g.wetLen,g.maxAboveWater,g.pillars,JSON.stringify(g.centre),Object.keys(g.prefabs).map(p=>p.replace(/^\d+\./,'')).join('|'),JSON.stringify(g.crosses).slice(0,120),g.nearBridgeBuildings.join(','));
console.log('bridgeBuildings',JSON.stringify(bridgeBld.map(b=>[b.id,b.prefab,b.position.map(Math.round),b.angle,b.width,b.length])));
