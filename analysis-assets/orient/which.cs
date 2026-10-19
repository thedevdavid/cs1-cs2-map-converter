#:package SixLabors.ImageSharp@3.1.*
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// Which file/orientation reproduces the heights the game gave the imported nodes?
var graph = args[0]; var playable = args[1]; var world = args[2];
ushort[] Load(string p, out int W){ using var img = Image.Load<L16>(p); W = img.Width; var d = new ushort[W*W]; int w=W; img.ProcessPixelRows(a => { for (int y=0;y<a.Height;y++){ var r=a.GetRowSpan(y); for(int x=0;x<r.Length;x++) d[y*w+x]=r[x].PackedValue; } }); return d; }
var P = Load(playable, out int WP); var Wm = Load(world, out int WW);
using var doc = JsonDocument.Parse(File.ReadAllText(graph));
var pts = new List<(double x,double z,double y)>();
foreach (var j in doc.RootElement.GetProperty("junctions").EnumerateArray()) { var p=j.GetProperty("position"); pts.Add((p.GetProperty("x").GetDouble(), p.GetProperty("z").GetDouble(), p.GetProperty("y").GetDouble())); }
double S(ushort[] d, int W, double x, double z, double extent, bool row0North, double scaleM) { double cell = extent / W; double px = (x + extent/2)/cell - 0.5; double pz = row0North ? (extent/2 - z)/cell - 0.5 : (z + extent/2)/cell - 0.5; int ix=Math.Clamp((int)Math.Round(px),0,W-1), iz=Math.Clamp((int)Math.Round(pz),0,W-1); return d[iz*W+ix]*scaleM/65535.0; }
void Report(string label, Func<double,double,double> f){ double se=0,sa=0,b=0; int n=0; foreach (var (x,z,y) in pts){ double h=f(x,z); double d=h-y; se+=d*d; sa+=Math.Abs(d); b+=d; n++; } Console.WriteLine($"{label,-58} RMS={Math.Sqrt(se/n),6:F1}  mean|d|={sa/n,6:F1}  bias={b/n,6:F1}"); }
Report("playable png as playable, row0=north, scale 2048", (x,z)=>S(P,WP,x,z,14336,true,2048));
Report("playable png as playable, row0=south, scale 2048", (x,z)=>S(P,WP,x,z,14336,false,2048));
Report("playable png as playable, row0=north, scale 4096", (x,z)=>S(P,WP,x,z,14336,true,4096));
Report("playable png as playable, row0=north, scale 1024", (x,z)=>S(P,WP,x,z,14336,true,1024));
Report("WORLD png imported as playable (14336 m), row0=north, 2048", (x,z)=>S(Wm,WW,x,z,14336,true,2048));
Report("WORLD png imported as playable, row0=south, 2048", (x,z)=>S(Wm,WW,x,z,14336,false,2048));
Report("WORLD png as world (57344 m), row0=north, 2048", (x,z)=>S(Wm,WW,x,z,57344,true,2048));
Console.WriteLine($"game node y: min={pts.Min(p=>p.y):F1} max={pts.Max(p=>p.y):F1} mean={pts.Average(p=>p.y):F1}  (n={pts.Count})");
