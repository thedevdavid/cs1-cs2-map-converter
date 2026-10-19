#:package SixLabors.ImageSharp@3.1.*
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
ushort[] Load(string p){ using var img=Image.Load<L16>(p); var d=new ushort[4096*4096]; img.ProcessPixelRows(a=>{ for(int y=0;y<4096;y++){ var r=a.GetRowSpan(y); for(int x=0;x<4096;x++) d[y*4096+x]=r[x].PackedValue; } }); return d; }
var a=Load(args[0]); var b=Load(args[1]); double sad=0; double maxd=0;
for(int y=0;y<1024;y++) for(int x=0;x<1024;x++){ double s=0; for(int dy=0;dy<4;dy++) for(int dx=0;dx<4;dx++) s+=a[(y*4+dy)*4096+x*4+dx]; s/=16; double d=Math.Abs(s-b[(1536+y)*4096+1536+x]); sad+=d; if(d>maxd) maxd=d; }
Console.WriteLine($"hs4096 pair: world-map centre vs playable box-downsampled: mean abs diff={sad/(1024.0*1024):F2} raw, max={maxd:F1} raw ({maxd*4096/65535:F2} m)");
