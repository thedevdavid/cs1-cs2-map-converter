# Half-size JPEGs for the sheets: powershell -File next-run/small.ps1 -Dir next-run/shots-a11  → <Dir>/small/<name>.jpg
param([string]$Dir = "next-run/shots-a11", [int]$Quality = 82)
Add-Type -AssemblyName System.Drawing
$out = Join-Path $Dir "small"; New-Item -ItemType Directory -Force $out | Out-Null
$codec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq "image/jpeg" }
$ep = New-Object System.Drawing.Imaging.EncoderParameters 1
$ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality, [long]$Quality)
Get-ChildItem "$Dir\*.png" | ForEach-Object {
  $img = [System.Drawing.Image]::FromFile($_.FullName)
  $w = [int]($img.Width / 2); $h = [int]($img.Height / 2)
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  $g = [System.Drawing.Graphics]::FromImage($bmp); $g.InterpolationMode = "HighQualityBicubic"; $g.DrawImage($img, 0, 0, $w, $h); $g.Dispose()
  $dst = Join-Path $out ($_.BaseName + ".jpg"); $bmp.Save($dst, $codec, $ep); $bmp.Dispose(); $img.Dispose()
  "{0} {1}x{2} {3} KB" -f $_.BaseName, $w, $h, [int]((Get-Item $dst).Length / 1KB)
}
