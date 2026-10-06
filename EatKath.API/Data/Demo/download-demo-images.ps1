# ==========================================================
# Recreates the local CraveDine demo photo library (development only).
#
#   pwsh EatKath.API/Data/Demo/download-demo-images.ps1
#
# Reads demo-image-attribution.json (Wikimedia Commons sources and
# licences), downloads each photo, centre-crops it to 16:10 and saves it
# as 1200x750 JPEG under EatKath.API/wwwroot/uploads/demo (git-ignored).
# Existing files are skipped. Requires Windows (System.Drawing).
# ==========================================================

param(
    [string]$Attribution = (Join-Path $PSScriptRoot "demo-image-attribution.json"),
    [string]$Dest = (Join-Path $PSScriptRoot "..\..\wwwroot\uploads\demo")
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$headers = @{ "User-Agent" = "CraveDineDevSeed/1.0 (local development demo dataset)" }
New-Item -ItemType Directory -Force $Dest | Out-Null

$doc = Get-Content $Attribution -Raw | ConvertFrom-Json
$jpeg = [Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq "image/jpeg"
$params = New-Object Drawing.Imaging.EncoderParameters 1
$params.Param[0] = New-Object Drawing.Imaging.EncoderParameter ([Drawing.Imaging.Encoder]::Quality), 82L

$saved = 0; $skipped = 0; $failed = 0

foreach ($image in $doc.images) {
    $target = Join-Path $Dest (Split-Path $image.file -Leaf)

    if (Test-Path $target) { $skipped++; continue }

    $tmp = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString("N") + ".jpg")
    $url = "https://commons.wikimedia.org/wiki/Special:FilePath/" + [uri]::EscapeDataString($image.title) + "?width=1600"

    try {
        Invoke-WebRequest $url -Headers $headers -OutFile $tmp -TimeoutSec 90 | Out-Null

        $img = [Drawing.Image]::FromFile($tmp)
        $tw = 1200; $th = 750; $ratio = $tw / $th; $r = $img.Width / $img.Height

        if ($r -gt $ratio) {
            $sw = [int]($img.Height * $ratio)
            $rect = New-Object Drawing.Rectangle ([int](($img.Width - $sw) / 2)), 0, $sw, $img.Height
        }
        else {
            $sh = [int]($img.Width / $ratio)
            $rect = New-Object Drawing.Rectangle 0, ([int](($img.Height - $sh) / 2)), $img.Width, $sh
        }

        $bmp = New-Object Drawing.Bitmap $tw, $th
        $g = [Drawing.Graphics]::FromImage($bmp)
        $g.InterpolationMode = "HighQualityBicubic"; $g.PixelOffsetMode = "HighQuality"
        $g.DrawImage($img, (New-Object Drawing.Rectangle 0, 0, $tw, $th), $rect, [Drawing.GraphicsUnit]::Pixel)
        $bmp.Save($target, $jpeg, $params)
        $g.Dispose(); $bmp.Dispose(); $img.Dispose()
        $saved++
    }
    catch {
        Write-Warning "Failed: $($image.file) ($($_.Exception.Message))"
        $failed++
    }
    finally {
        if (Test-Path $tmp) { Remove-Item $tmp -Force }
    }

    Start-Sleep -Milliseconds 250
}

"Saved $saved, skipped $skipped (already present), failed $failed -> $Dest"
