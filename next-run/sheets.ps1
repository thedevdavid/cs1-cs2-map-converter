# Contact sheets for a review round: CS1 reference (left) vs CS2 run (right), five numbered pairs per sheet.
# Usage: powershell -File next-run/sheets.ps1 -Run a9   (expects next-run/shots-cs1/small/cs1-<name>.jpg and next-run/shots-<Run>/small/<name>.jpg)
param([string]$Run = "a9")
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot; $root = Join-Path $root "next-run"; $out = "$root\sheets-$Run"; New-Item -ItemType Directory -Force $out | Out-Null
$cs1 = "$root\shots-cs1\small"; $cs2 = "$root\shots-$Run\small"
$pairs = @(
 @("01-br006-truss-arch-side","01 BR006 truss arch"), @("02-br009-truss-arch-pair-and-approach","02 BR009 pair + approach"), @("03-br005-truss-arch-pair","03 BR005 pair"),
 @("04-detroit-suspension-side","04 Detroit side"), @("05-detroit-approach-piers","05 Detroit approach"), @("06-br003-piers","06 BR003 piers"),
 @("07-ourense-rail-truss-arch","07 Ourense rail arch"), @("08-pedrido-rail-truss-arch","08 Pedrido rail arch"), @("09-covered-bridge-gravel","09 covered bridge"),
 @("10-dam-and-power-lines","10 dam + power"), @("11-substation","11 substation"), @("12-interchange-oneway-joins","12 fork (2995,820)"),
 @("30-interchange-05-stack","30 stack interchange 5"), @("13-crossing-restored","13 crossing (-4224,-60)"), @("14-dead-end-fix","14 dead-end fix"),
 @("15-interchange-umber-driftwood","15 Umber/Driftwood"), @("16-sand-paint-river","16 sand river"), @("17-sand-paint-southeast","17 sand southeast"),
 @("18-sand-paint-lake","18 sand lake"), @("19-dry-grass-paint-west","19 dry grass west"), @("20-dry-grass-paint-southeast","20 dry grass southeast"),
 @("21-bare-earth-patch","21 bare earth"), @("22-fort-ruins","22 fort"), @("23-po-rocks-cliffs-south","23 cliffs south"),
 @("24-po-rocks-cliffs-west","24 rocks west"), @("25-world-map-east-flats","25 world map east"), @("26-world-map-west-range","26 world map west"),
 @("W1-water-v1","W1 water (v1)"))
$cw = 960; $ch = 600; $lab = 34; $rowsPerSheet = 5
$font = New-Object System.Drawing.Font("Segoe UI", 20, [System.Drawing.FontStyle]::Bold); $white = [System.Drawing.Brushes]::White; $black = [System.Drawing.Brushes]::Black
$enc = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }; $ep = New-Object System.Drawing.Imaging.EncoderParameters 1; $ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), 80L
function Cell($g, $path, $x, $y, $title) { $g.FillRectangle($black, $x, $y, $cw, $ch + $lab); if (Test-Path $path) { $img = [System.Drawing.Image]::FromFile($path); $g.DrawImage($img, $x, $y + $lab, $cw, $ch); $img.Dispose() } else { $g.DrawString("(missing)", $font, $white, $x + 20, $y + $lab + 20) }; $g.DrawString($title, $font, $white, $x + 8, $y + 2) }
$sheet = 0; for ($i = 0; $i -lt $pairs.Count; $i += $rowsPerSheet) { $sheet++; $n = [Math]::Min($rowsPerSheet, $pairs.Count - $i); $bmp = New-Object System.Drawing.Bitmap ($cw * 2), (($ch + $lab) * $n); $g = [System.Drawing.Graphics]::FromImage($bmp); $g.InterpolationMode = 'HighQualityBicubic'; for ($r = 0; $r -lt $n; $r++) { $p = $pairs[$i + $r]; $y = $r * ($ch + $lab); Cell $g "$cs1\cs1-$($p[0]).jpg" 0 $y "CS1  $($p[1])"; Cell $g "$cs2\$($p[0]).jpg" $cw $y "$($Run.ToUpper())  $($p[1])" }; $bmp.Save("$out\sheet-$sheet-pairs.jpg", $enc, $ep); $g.Dispose(); $bmp.Dispose() }
Get-ChildItem "$out\*.jpg" | Select-Object Name, @{n='KB';e={[int]($_.Length/1KB)}}
