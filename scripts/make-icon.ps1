# Generates a simple local app icon (no download) using the brand green
# from docs/COLOR_PALETTE.md, so the Desktop shortcut has something nicer
# than the default PowerShell/batch icon while no real logo exists yet.

Add-Type -AssemblyName System.Drawing

$size = 256
$bmp = New-Object System.Drawing.Bitmap $size, $size
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

# Brand green (approximates oklch(0.62 0.17 150) from docs/COLOR_PALETTE.md)
$green = [System.Drawing.Color]::FromArgb(255, 34, 173, 108)
$brush = New-Object System.Drawing.SolidBrush $green

$rect = New-Object System.Drawing.Rectangle 0, 0, $size, $size
$radius = 56
$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddArc($rect.X, $rect.Y, $radius, $radius, 180, 90)
$path.AddArc($rect.Right - $radius, $rect.Y, $radius, $radius, 270, 90)
$path.AddArc($rect.Right - $radius, $rect.Bottom - $radius, $radius, $radius, 0, 90)
$path.AddArc($rect.X, $rect.Bottom - $radius, $radius, $radius, 90, 90)
$path.CloseFigure()
$g.FillPath($brush, $path)

$font = New-Object System.Drawing.Font("Segoe UI", [float]92, [System.Drawing.FontStyle]::Bold)
$textBrush = [System.Drawing.Brushes]::White
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$g.DrawString("C1", $font, $textBrush, [System.Drawing.RectangleF]::new(0, 4, $size, $size), $format)

$g.Flush()

$iconPath = Join-Path $PSScriptRoot "..\assets\app-icon.ico"
New-Item -ItemType Directory -Path (Split-Path $iconPath) -Force | Out-Null

$hIcon = $bmp.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($hIcon)
$stream = [System.IO.File]::Open($iconPath, [System.IO.FileMode]::Create)
$icon.Save($stream)
$stream.Close()

$g.Dispose()
$bmp.Dispose()

Write-Host "Icon written to $iconPath"
