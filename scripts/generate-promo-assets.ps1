Add-Type -AssemblyName System.Drawing

$sourcePath = "C:\Users\misha\.gemini\antigravity\brain\fb7d16e1-e729-4e74-af56-677f04a82a54\codetap_icon_concept_1790752007993.jpg"

if (-not (Test-Path $sourcePath)) {
    Write-Error "Source image not found: $sourcePath"
    exit 1
}

$promoDir = "C:\HelloTap\promo"
$publicDir = "C:\HelloTap\public"

if (-not (Test-Path $promoDir)) {
    New-Item -ItemType Directory -Path $promoDir -Force | Out-Null
}

$srcImg = [System.Drawing.Image]::FromFile($sourcePath)
Write-Host "Source dimensions: $($srcImg.Width)x$($srcImg.Height)"

function Generate-Asset {
    param(
        [System.Drawing.Image]$source,
        [int]$targetW,
        [int]$targetH,
        [string]$outputPath,
        [string]$mode = "cover"
    )

    $bmp = New-Object System.Drawing.Bitmap $targetW, $targetH
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    if ($mode -eq "cover") {
        # Calculate aspect ratios
        $srcRatio = $source.Width / $source.Height
        $tgtRatio = $targetW / $targetH

        if ($srcRatio -gt $tgtRatio) {
            $renderH = $targetH
            $renderW = [int]($targetH * $srcRatio)
            $offsetX = [int](($targetW - $renderW) / 2)
            $offsetY = 0
        } else {
            $renderW = $targetW
            $renderH = [int]($targetW / $srcRatio)
            $offsetX = 0
            $offsetY = [int](($targetH - $renderH) / 2)
        }

        # Clear background with dark cyber color
        $bgBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(11, 15, 25))
        $g.FillRectangle($bgBrush, 0, 0, $targetW, $targetH)
        $bgBrush.Dispose()

        $g.DrawImage($source, $offsetX, $offsetY, $renderW, $renderH)
    }
    elseif ($mode -eq "banner") {
        # Fill futuristic dark gradient background
        $bgBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(10, 14, 26))
        $g.FillRectangle($bgBrush, 0, 0, $targetW, $targetH)
        $bgBrush.Dispose()

        # Draw enlarged ambient blurred/background image
        $g.DrawImage($source, -200, -200, $targetW + 400, $targetH + 400)

        # Semi-transparent dark overlay for contrast
        $overlayBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(170, 8, 12, 22))
        $g.FillRectangle($overlayBrush, 0, 0, $targetW, $targetH)
        $overlayBrush.Dispose()

        # Place the sharp centered / heroic focal graphic
        $iconSize = [int]($targetH * 0.88)
        $posX = [int](($targetW - $iconSize) / 2)
        $posY = [int](($targetH - $iconSize) / 2)

        # Subtle glow ring behind focal center
        $glowPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(60, 6, 182, 212)), 6
        $g.DrawEllipse($glowPen, $posX - 10, $posY - 10, $iconSize + 20, $iconSize + 20)
        $glowPen.Dispose()

        $g.DrawImage($source, $posX, $posY, $iconSize, $iconSize)
    }
    elseif ($mode -eq "square") {
        $g.DrawImage($source, 0, 0, $targetW, $targetH)
    }

    $g.Dispose()
    
    # Save as PNG
    $bmp.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host "Created: $outputPath ($targetW x $targetH)"
}

# 1. Square icons
Generate-Asset -source $srcImg -targetW 1024 -targetH 1024 -outputPath "$promoDir\icon_1024x1024.png" -mode "square"
Generate-Asset -source $srcImg -targetW 512 -targetH 512 -outputPath "$promoDir\icon_512x512.png" -mode "square"
Generate-Asset -source $srcImg -targetW 512 -targetH 512 -outputPath "$publicDir\icon.png" -mode "square"
Generate-Asset -source $srcImg -targetW 192 -targetH 192 -outputPath "$publicDir\icon-192.png" -mode "square"
Generate-Asset -source $srcImg -targetW 64 -targetH 64 -outputPath "$publicDir\favicon.png" -mode "square"

# 2. Covers for Yandex Games (4:3 ratio: 800x600 and 1200x900)
Generate-Asset -source $srcImg -targetW 800 -targetH 600 -outputPath "$promoDir\cover_800x600.png" -mode "cover"
Generate-Asset -source $srcImg -targetW 1200 -targetH 900 -outputPath "$promoDir\cover_1200x900.png" -mode "cover"

# 3. Wide Promo Banner (16:9 ratio: 1920x1080 and 1280x720)
Generate-Asset -source $srcImg -targetW 1920 -targetH 1080 -outputPath "$promoDir\banner_1920x1080.png" -mode "banner"
Generate-Asset -source $srcImg -targetW 1280 -targetH 720 -outputPath "$promoDir\banner_1280x720.png" -mode "banner"

$srcImg.Dispose()
Write-Host "All promo assets successfully generated!"
