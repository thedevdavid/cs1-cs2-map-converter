#:package SixLabors.ImageSharp@3.1.*
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

var files = args;
var outDir = Environment.GetEnvironmentVariable("OUT_DIR") ?? ".";
var loaded = new Dictionary<string, ushort[]>();
var dims = new Dictionary<string, (int w, int h)>();
foreach (var f in files) {
  var info = Image.Identify(f);
  var meta = info.Metadata.GetPngMetadata();
  Console.WriteLine($"== {Path.GetFileName(f)}: {info.Width}x{info.Height}, png bitDepth={meta.BitDepth}, colorType={meta.ColorType}, bpp={info.PixelType.BitsPerPixel}, size={new FileInfo(f).Length/1e6:F1} MB");
  using var img = Image.Load<L16>(f);
  int w = img.Width, h = img.Height;
  var data = new ushort[w*h];
  img.ProcessPixelRows(acc => { for (int y=0;y<acc.Height;y++){ var row=acc.GetRowSpan(y); for(int x=0;x<row.Length;x++) data[y*w+x]=row[x].PackedValue; } });
  loaded[f]=data; dims[f]=(w,h);
  var hist = new long[65536]; foreach (var v in data) hist[v]++;
  int min=0; while(hist[min]==0) min++; int max=65535; while(hist[max]==0) max--;
  double mean = 0; foreach (var v in data) mean += v; mean /= data.Length;
  long total=data.LongLength;
  int Pct(double p){ long t=(long)(p*total); long c=0; for(int i=0;i<65536;i++){ c+=hist[i]; if(c>=t) return i;} return 65535; }
  int mode=0; for(int i=0;i<65536;i++) if(hist[i]>hist[mode]) mode=i;
  int distinct=0; for(int i=0;i<65536;i++) if(hist[i]>0) distinct++;
  Console.WriteLine($"   raw16: min={min} max={max} mean={mean:F1} p1={Pct(0.01)} p5={Pct(0.05)} p50={Pct(0.5)} p95={Pct(0.95)} p99={Pct(0.99)} mode={mode} ({100.0*hist[mode]/total:F2}%) distinct={distinct} zeros={100.0*hist[0]/total:F2}% at65535={100.0*hist[65535]/total:F2}%");
  Console.WriteLine($"   @4096m scale: min={min*4096.0/65535:F1} max={max*4096.0/65535:F1} mean={mean*4096/65535:F1} mode={mode*4096.0/65535:F1} p5={Pct(0.05)*4096.0/65535:F1} p95={Pct(0.95)*4096.0/65535:F1}");
  Console.WriteLine($"   @1024m scale: min={min*1024.0/65535:F1} max={max*1024.0/65535:F1} mean={mean*1024/65535:F1} mode={mode*1024.0/65535:F1}");
  double top=0,bot=0,left=0,right=0; for(int x=0;x<w;x++){top+=data[x]; bot+=data[(h-1)*w+x];} for(int y=0;y<h;y++){left+=data[y*w]; right+=data[y*w+w-1];}
  Console.WriteLine($"   edge means: top={top/w:F0} bottom={bot/w:F0} left={left/h:F0} right={right/h:F0}");
  // 8-bit-step detection: are values multiples of 257 (8-bit upscaled)?
  long mult257=0; for(int i=0;i<65536;i+=257) mult257+=hist[i];
  Console.WriteLine($"   fraction of values that are exact 8-bit multiples (v%257==0): {100.0*mult257/total:F2}%");
  using var prev = img.Clone(ctx => ctx.Resize(1024, 1024));
  var p8 = new Image<L8>(1024,1024);
  double range = Math.Max(1, max-min);
  prev.ProcessPixelRows(p8, (src,dst)=>{ for(int y=0;y<src.Height;y++){ var s=src.GetRowSpan(y); var d=dst.GetRowSpan(y); for(int x=0;x<s.Length;x++) d[x]=new L8((byte)Math.Clamp((s[x].PackedValue-min)/range*255,0,255)); } });
  var outPath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(f)+"_preview.png");
  p8.Save(outPath); Console.WriteLine($"   preview -> {outPath}");
}
if (files.Length==2 && dims[files[0]].w==4096 && dims[files[1]].w==4096) {
  var a = loaded[files[0]]; var b = loaded[files[1]];
  double sad=0; long n=0; double maxd=0; double ma=0, mb=0;
  for(int y=0;y<1024;y++) for(int x=0;x<1024;x++){ double s=0; for(int dy=0;dy<4;dy++) for(int dx=0;dx<4;dx++) s+=a[(y*4+dy)*4096 + x*4+dx]; s/=16; double bv=b[(1536+y)*4096 + 1536+x]; double d=Math.Abs(s-bv); sad+=d; n++; if(d>maxd) maxd=d; ma+=s; mb+=bv; }
  Console.WriteLine($"== worldmap center(1024^2 @1536) vs base box-downsampled: mean abs diff={sad/n:F1} raw16 (={sad/n*4096/65535:F2} m @4096m), max diff={maxd:F0}; base mean={ma/n:F1} center mean={mb/n:F1} ratio={mb/ma:F3}");
  // seam check: ring just outside center vs just inside
  double inside=0, outside=0; long ni=0, no=0;
  for(int i=1536;i<2560;i++){ inside+=b[1536*4096+i]+b[2559*4096+i]+b[i*4096+1536]+b[i*4096+2559]; ni+=4; outside+=b[1535*4096+i]+b[2560*4096+i]+b[i*4096+1535]+b[i*4096+2560]; no+=4; }
  Console.WriteLine($"   seam: mean just-inside ring={inside/ni:F0}, just-outside ring={outside/no:F0}");
}
