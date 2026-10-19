#:package SixLabors.ImageSharp@3.1.*
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// Does dktflls-1to1-4096.png match CS1 terrain heights at CS1 node positions, and under which row orientation?
var png = args[0]; var dump = args[1];
using var img = Image.Load<L16>(png);
int W = img.Width;
var data = new ushort[W*W];
img.ProcessPixelRows(a => { for (int y=0;y<a.Height;y++){ var r=a.GetRowSpan(y); for(int x=0;x<r.Length;x++) data[y*W+x]=r[x].PackedValue; } });
using var doc = JsonDocument.Parse(File.ReadAllText(dump));
var nodes = new List<(double x,double z,double t)>();
foreach (var n in doc.RootElement.GetProperty("nodes").EnumerateArray()) {
  var p = n.GetProperty("position"); double x=p[0].GetDouble(), z=p[2].GetDouble(); double t = n.GetProperty("terrain").GetDouble();
  if (Math.Abs(x) < 7100 && Math.Abs(z) < 7100) nodes.Add((x,z,t));
}
double Sample(double x, double z, bool row0North, bool flipX) {
  double px = (x + 7168.0) / 3.5 - 0.5; if (flipX) px = (7168.0 - x)/3.5 - 0.5;
  double pz = row0North ? (7168.0 - z)/3.5 - 0.5 : (z + 7168.0)/3.5 - 0.5;
  int ix = (int)Math.Round(px), iz = (int)Math.Round(pz);
  ix = Math.Clamp(ix,0,W-1); iz = Math.Clamp(iz,0,W-1);
  return data[iz*W+ix] * 2048.0 / 65535.0;
}
foreach (var (label, north, fx) in new[]{("row0=north, x east", true, false), ("row0=south, x east", false, false), ("row0=north, x flipped", true, true), ("row0=south, x flipped", false, true)}) {
  double se=0, sa=0, bias=0; int n=0;
  foreach (var (x,z,t) in nodes) { double h = Sample(x,z,north,fx); double d = h - t; se += d*d; sa += Math.Abs(d); bias += d; n++; }
  Console.WriteLine($"{label,-24} n={n} RMS={Math.Sqrt(se/n):F1} m  mean|d|={sa/n:F1} m  bias={bias/n:F1} m");
}
