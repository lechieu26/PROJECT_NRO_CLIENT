# Script sửa cấu hình Texture của các Ships Spine để tránh bị mờ, nhòe và lỗi màu
# 1. Đặt alphaIsTransparency thành 0 (vì Spine dùng Premultiplied Alpha)
# 2. Đặt textureCompression thành 0 (Uncompressed) để giữ chất lượng tối đa không bị vỡ hạt, mờ
# 3. Đảm bảo dùng đường dẫn tương đối để tương thích

$shipsRoot = "Assets\Resources\Spine\Ships"
if (-not (Test-Path $shipsRoot)) {
    $shipsRoot = "..\Assets\Resources\Spine\Ships"
}

if (-not (Test-Path $shipsRoot)) {
    Write-Error "Khong tim thay thu muc Ships tai: $shipsRoot"
    exit
}

Write-Host "Tim kiem cac file texture .png.meta trong: $shipsRoot" -ForegroundColor Cyan

$metaFiles = Get-ChildItem -Path $shipsRoot -Filter "*.png.meta" -Recurse

$totalFixed = 0

foreach ($file in $metaFiles) {
    $filePath = $file.FullName
    $relativeName = $file.FullName.Replace((Get-Location).Path, "")
    Write-Host "Dang xu ly: $relativeName" -ForegroundColor Yellow
    
    $content = Get-Content -Path $filePath -Raw
    $modified = $false
    
    # 1. Chuyển alphaIsTransparency sang 0
    if ($content -match 'alphaIsTransparency:\s*1') {
        $content = $content -replace 'alphaIsTransparency:\s*1', 'alphaIsTransparency: 0'
        $modified = $true
        Write-Host "  -> Da tat alphaIsTransparency (dat ve 0)" -ForegroundColor Green
    }
    
    # 2. Chuyển textureCompression sang 0 cho platformSettings và mac/default settings
    if ($content -match 'textureCompression:\s*[^0\s]') {
        $content = $content -replace 'textureCompression:\s*[^0\s]+', 'textureCompression: 0'
        $modified = $true
        Write-Host "  -> Da tat textureCompression (dat ve 0)" -ForegroundColor Green
    }
    
    if ($modified) {
        Set-Content -Path $filePath -Value $content -NoNewline
        $totalFixed++
        Write-Host "  -> Da luu thay doi file meta!" -ForegroundColor Cyan
    } else {
        Write-Host "  -> File da dung cau hinh, khong can sua." -ForegroundColor Gray
    }
}

Write-Host "`nHoan thanh! Da sua $totalFixed / $($metaFiles.Count) file meta texture." -ForegroundColor Green
Write-Host "[!] Vui long mo Unity Editor va doi no reimport lai texture cua Ships." -ForegroundColor Yellow
