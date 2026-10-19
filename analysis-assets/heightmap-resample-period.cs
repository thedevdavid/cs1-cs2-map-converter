#:package SixLabors.ImageSharp@3.1.*
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// Detect the resampling grid period in an upsampled heightmap via Goertzel power of 2nd differences.
using var img = Image.Load<L16>(args[0]);
int w = img.Width, h = img.Height;
var data = new ushort[w*h];
img.ProcessPixelRows(acc => { for (int y=0;y<acc.Height;y++){ var row=acc.GetRowSpan(y); for(int x=0;x<row.Length;x++) data[y*w+x]=row[x].PackedValue; } });
double Power(double period, bool rows) {
  double total=0; int n=0;
  for (int line=64; line<4096-64; line+=16) {
    // second differences along the line
    var d2 = new double[w-2];
    for (int i=1;i<w-1;i++) { double a = rows? data[line*w+i-1] : data[(i-1)*w+line]; double b = rows? data[line*w+i] : data[i*w+line]; double c = rows? data[line*w+i+1] : data[(i+1)*w+line]; d2[i-1]=a-2*b+c; }
    double wr = 2*Math.PI/period; double coeff=2*Math.Cos(wr); double s0=0,s1=0,s2=0;
    for (int i=0;i<d2.Length;i++){ s0 = d2[i] + coeff*s1 - s2; s2=s1; s1=s0; }
    double p = s1*s1 + s2*s2 - coeff*s1*s2; total += p; n++;
  }
  return total/n;
}
foreach (var rows in new[]{true,false}) {
  Console.WriteLine(rows? "== along rows (x)" : "== along columns (y)");
  var results = new List<(double p,double pow)>();
  for (double P=2.2; P<=8.0; P+=0.01) results.Add((P, Power(P, rows)));
  double median = results.Select(r=>r.pow).OrderBy(v=>v).ElementAt(results.Count/2);
  foreach (var r in results.OrderByDescending(r=>r.pow).Take(8)) Console.WriteLine($"   period {r.p:F2} px  power/median = {r.pow/median:F1}");
  foreach (var P in new[]{3.789, 4.571, 3.0, 4.0, 1081.0/4096*16}) Console.WriteLine($"   candidate period {P:F3}: power/median = {Power(P,rows)/median:F1}");
}
