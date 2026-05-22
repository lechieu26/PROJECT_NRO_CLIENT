# Fix All Ships Spine Assets
# Vấn đề: Tất cả SkeletonData của ships đều thiếu atlasAssets (rỗng []),
# thiếu Atlas asset và Material, file .atlas cần đổi thành .atlas.txt
#
# Script này sẽ:
# 1. Rename .atlas -> .atlas.txt (và .atlas.meta -> .atlas.txt.meta)
# 2. Tạo Material (.mat) cho mỗi ship
# 3. Tạo Atlas asset cho mỗi ship
# 4. Cập nhật SkeletonData để tham chiếu đúng Atlas asset

$shipsRoot = "E:\NRO\SourceCode\PROJECT_NRO_240_mod\Assets\Resources\Spine\Ships"

# Spine shader GUID (lấy từ skin đang hoạt động - Spine/Skeleton)
$spineShaderGuid = "a5cff0acc1f0bf7459293b7a35221fc3"

# SpineAtlasAsset script GUID
$atlasScriptGuid = "22c73ce715b728744b5b39f14be89c76"

# SkeletonDataAsset script GUID  
$skeletonScriptGuid = "03cd8b35ed8341e489e832577d229026"

function New-UnityGuid {
    return [guid]::NewGuid().ToString("N")
}

function Get-GuidFromMeta {
    param([string]$metaPath)
    if (Test-Path $metaPath) {
        $content = Get-Content $metaPath -Raw
        if ($content -match 'guid:\s+([a-f0-9]{32})') {
            return $matches[1]
        }
    }
    return $null
}

$shipDirs = Get-ChildItem -Path $shipsRoot -Directory | Sort-Object Name

$totalFixed = 0
$errors = @()

foreach ($shipDir in $shipDirs) {
    $shipName = $shipDir.Name
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Processing: $shipName" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    # --- Step 1: Rename .atlas -> .atlas.txt ---
    $atlasFile = Join-Path $shipDir.FullName "$shipName.atlas"
    $atlasTxtFile = Join-Path $shipDir.FullName "$shipName.atlas.txt"
    $atlasMetaFile = Join-Path $shipDir.FullName "$shipName.atlas.meta"
    $atlasTxtMetaFile = Join-Path $shipDir.FullName "$shipName.atlas.txt.meta"

    if ((Test-Path $atlasFile) -and !(Test-Path $atlasTxtFile)) {
        Write-Host "  [1] Renaming .atlas -> .atlas.txt" -ForegroundColor Yellow
        Rename-Item -Path $atlasFile -NewName "$shipName.atlas.txt"
        
        if (Test-Path $atlasMetaFile) {
            # Đọc GUID cũ, tạo meta mới với TextScriptImporter
            $oldGuid = Get-GuidFromMeta $atlasMetaFile
            if ($oldGuid) {
                # Cập nhật meta để Unity nhận diện .atlas.txt là TextAsset
                $newMetaContent = @"
fileFormatVersion: 2
guid: $oldGuid
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
                # Xóa meta cũ và tạo meta mới
                Remove-Item $atlasMetaFile
                Set-Content -Path $atlasTxtMetaFile -Value $newMetaContent -NoNewline
                Write-Host "  [1] Updated meta file (guid: $oldGuid)" -ForegroundColor Green
            }
        }
    } elseif (Test-Path $atlasTxtFile) {
        Write-Host "  [1] .atlas.txt already exists, skipping rename" -ForegroundColor Gray
    } else {
        Write-Host "  [1] ERROR: No .atlas file found!" -ForegroundColor Red
        $errors += "${shipName}: No atlas file"
        continue
    }

    # Lấy GUID của atlas text file
    $atlasTextGuid = Get-GuidFromMeta $atlasTxtMetaFile
    if (-not $atlasTextGuid) {
        # Thử lấy từ meta cũ nếu chưa rename
        $atlasTextGuid = Get-GuidFromMeta $atlasMetaFile
    }
    if (-not $atlasTextGuid) {
        Write-Host "  ERROR: Cannot find atlas text GUID!" -ForegroundColor Red
        $errors += "${shipName}: No atlas text GUID"
        continue
    }

    # Lấy GUID của PNG texture
    $pngMetaFile = Join-Path $shipDir.FullName "$shipName.png.meta"
    $pngGuid = Get-GuidFromMeta $pngMetaFile
    if (-not $pngGuid) {
        Write-Host "  ERROR: Cannot find PNG GUID!" -ForegroundColor Red
        $errors += "${shipName}: No PNG GUID"
        continue
    }

    # Lấy GUID của skeleton JSON
    $jsonMetaFile = Join-Path $shipDir.FullName "$shipName.json.meta"
    $skelMetaFile = Join-Path $shipDir.FullName "$shipName.skel.meta"
    $skeletonJsonGuid = Get-GuidFromMeta $jsonMetaFile
    if (-not $skeletonJsonGuid) {
        $skeletonJsonGuid = Get-GuidFromMeta $skelMetaFile
    }

    Write-Host "  Atlas text GUID: $atlasTextGuid" -ForegroundColor DarkGray
    Write-Host "  PNG GUID: $pngGuid" -ForegroundColor DarkGray
    Write-Host "  Skeleton GUID: $skeletonJsonGuid" -ForegroundColor DarkGray

    # --- Step 2: Tạo Material ---
    $matFile = Join-Path $shipDir.FullName "${shipName}_Material.mat"
    $matMetaFile = Join-Path $shipDir.FullName "${shipName}_Material.mat.meta"
    $matGuid = New-UnityGuid

    if (!(Test-Path $matFile)) {
        Write-Host "  [2] Creating Material..." -ForegroundColor Yellow

        $matContent = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: ${shipName}_Material
  m_Shader: {fileID: 4800000, guid: $spineShaderGuid, type: 3}
  m_Parent: {fileID: 0}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords:
  - _USE8NEIGHBOURHOOD_ON
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {}
  disabledShaderPasses: []
  m_LockedProperties: 
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _MainTex:
        m_Texture: {fileID: 2800000, guid: $pngGuid, type: 3}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    m_Ints: []
    m_Floats:
    - _Cutoff: 0.1
    - _OutlineMipLevel: 0
    - _OutlineOpaqueAlpha: 1
    - _OutlineReferenceTexWidth: 1024
    - _OutlineSmoothness: 1
    - _OutlineWidth: 3
    - _StencilComp: 8
    - _StencilRef: 1
    - _StraightAlphaInput: 0
    - _ThresholdEnd: 0.25
    - _Use8Neighbourhood: 1
    m_Colors:
    - _OutlineColor: {r: 1, g: 1, b: 0, a: 1}
  m_BuildTextureStacks: []
  m_AllowLocking: 1
"@
        Set-Content -Path $matFile -Value $matContent -NoNewline
        
        $matMetaContent = @"
fileFormatVersion: 2
guid: $matGuid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 2100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
        Set-Content -Path $matMetaFile -Value $matMetaContent -NoNewline
        Write-Host "  [2] Material created (guid: $matGuid)" -ForegroundColor Green
    } else {
        Write-Host "  [2] Material already exists, reading GUID..." -ForegroundColor Gray
        $matGuid = Get-GuidFromMeta $matMetaFile
    }

    # --- Step 3: Tạo Atlas asset ---
    $atlasAssetFile = Join-Path $shipDir.FullName "${shipName}_Atlas.asset"
    $atlasAssetMetaFile = Join-Path $shipDir.FullName "${shipName}_Atlas.asset.meta"
    $atlasAssetGuid = New-UnityGuid

    if (!(Test-Path $atlasAssetFile)) {
        Write-Host "  [3] Creating Atlas asset..." -ForegroundColor Yellow

        $atlasContent = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $atlasScriptGuid, type: 3}
  m_Name: ${shipName}_Atlas
  m_EditorClassIdentifier: spine-unity::Spine.Unity.SpineAtlasAsset
  textureLoadingMode: 0
  onDemandTextureLoader: {fileID: 0}
  atlasFile: {fileID: 4900000, guid: $atlasTextGuid, type: 3}
  materials:
  - {fileID: 2100000, guid: $matGuid, type: 2}
"@
        Set-Content -Path $atlasAssetFile -Value $atlasContent -NoNewline
        
        $atlasAssetMetaContent = @"
fileFormatVersion: 2
guid: $atlasAssetGuid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
        Set-Content -Path $atlasAssetMetaFile -Value $atlasAssetMetaContent -NoNewline
        Write-Host "  [3] Atlas asset created (guid: $atlasAssetGuid)" -ForegroundColor Green
    } else {
        Write-Host "  [3] Atlas asset already exists, reading GUID..." -ForegroundColor Gray
        $atlasAssetGuid = Get-GuidFromMeta $atlasAssetMetaFile
    }

    # --- Step 4: Cập nhật SkeletonData ---
    # Tìm file SkeletonData hiện có
    $skeletonFiles = Get-ChildItem -Path $shipDir.FullName -Filter "*_SkeletonData.asset" | Where-Object { $_.Name -notmatch "\.meta$" }
    
    foreach ($skelFile in $skeletonFiles) {
        Write-Host "  [4] Updating SkeletonData: $($skelFile.Name)" -ForegroundColor Yellow
        
        $skelContent = Get-Content $skelFile.FullName -Raw
        
        # Thay thế atlasAssets rỗng bằng tham chiếu đúng
        if ($skelContent -match 'atlasAssets:\s*\[\]') {
            $skelContent = $skelContent -replace 'atlasAssets:\s*\[\]', "atlasAssets:`n  - {fileID: 11400000, guid: $atlasAssetGuid, type: 2}"
            Set-Content -Path $skelFile.FullName -Value $skelContent -NoNewline
            Write-Host "  [4] SkeletonData updated with atlas reference!" -ForegroundColor Green
        } elseif ($skelContent -match 'atlasAssets:') {
            Write-Host "  [4] SkeletonData already has atlasAssets, skipping" -ForegroundColor Gray
        }
    }

    $totalFixed++
    Write-Host "  DONE!" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total ships processed: $totalFixed / $($shipDirs.Count)" -ForegroundColor Green

if ($errors.Count -gt 0) {
    Write-Host "`nErrors:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
}

Write-Host "`n[!] Sau khi chạy xong, hãy mở Unity Editor và đợi nó reimport assets." -ForegroundColor Yellow
Write-Host "[!] Nếu spine vẫn không hiển thị, hãy click vào SkeletonData asset và bấm 'Reload'." -ForegroundColor Yellow
