#:package SixLabors.ImageSharp@3.1.*
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
// Rescale a 2048 m-scale heightmap to the editor's default 4096 m scale (values x0.5), keeping 16-bit grayscale.
var src = args[0]; var dst = args[1];
using var img = Image.Load<L16>(src);
int max=0, min=65535; double sum=0; long n=0;
img.ProcessPixelRows(a => { for (int y=0;y<a.Height;y++){ var r=a.GetRowSpan(y); for(int x=0;x<r.Length;x++){ int v=(r[x].PackedValue+1)/2; r[x]=new L16((ushort)v); if(v>max)max=v; if(v<min)min=v; sum+=v; n++; } } });
img.SaveAsPng(dst, new SixLabors.ImageSharp.Formats.Png.PngEncoder{ BitDepth=SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit16, ColorType=SixLabors.ImageSharp.Formats.Png.PngColorType.Grayscale });
Console.WriteLine($"wrote {dst}: {img.Width}x{img.Height} min={min} ({min*4096.0/65535:F1} m @4096) max={max} ({max*4096.0/65535:F1} m) mean={sum/n*4096/65535:F1} m");
