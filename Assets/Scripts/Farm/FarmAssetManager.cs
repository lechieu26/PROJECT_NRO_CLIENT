using System;
using System.Collections.Generic;

/// <summary>
/// Quản lý assets cho Cloud Garden
/// Cache images theo cropType và stage
/// LƯU Ý: Các giai đoạn đầu (seed, sprout1, sprout2) dùng chung cho tất cả loại cây
/// </summary>
public class FarmAssetManager
{
    private static FarmAssetManager instance;

    // Farm icons cache (key: iconName, value: Image)
    private Dictionary<string, Image> farmIcons = new Dictionary<string, Image>();

    // Crop assets cache [cropType][stage] = Image
    // Với early stages (seed, sprout1, sprout2), cropType = -1 (common)
    private Image[,] cropAssets = new Image[FarmConstants.CROP_TYPE_COUNT + 1, FarmConstants.STAGE_COUNT + 1];

    // Common assets cho các giai đoạn đầu (index = CROP_TYPE_COUNT)
    private const int COMMON_INDEX = FarmConstants.CROP_TYPE_COUNT;

    // Plot empty image
    private Image plotEmptyImage;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static FarmAssetManager GI()
    {
        if (instance == null)
        {
            instance = new FarmAssetManager();
        }
        return instance;
    }

    /// <summary>
    /// Lưu farm icon vào cache
    /// </summary>
    public void SaveFarmIcon(string iconName, Image img)
    {
        if (img != null)
        {
            farmIcons[iconName] = img;
            Res.outz("FarmAssetManager: Saved icon " + iconName);
        }
    }

    /// <summary>
    /// Lấy farm icon từ cache
    /// </summary>
    public Image GetFarmIcon(string iconName)
    {
        if (farmIcons.ContainsKey(iconName))
        {
            return farmIcons[iconName];
        }
        return null;
    }

    /// <summary>
    /// Lưu crop asset vào cache
    /// Nếu isCommon = true, lưu vào vị trí common (dùng cho seed, sprout1, sprout2)
    /// </summary>
    public void SaveCropAsset(sbyte cropType, sbyte stage, Image img, bool isCommon = false)
    {
        if (stage >= 0 && stage <= FarmConstants.STAGE_COUNT && img != null)
        {
            int typeIndex = isCommon ? COMMON_INDEX : cropType;
            if (typeIndex >= 0 && typeIndex <= FarmConstants.CROP_TYPE_COUNT)
            {
                cropAssets[typeIndex, stage] = img;
                Res.outz("FarmAssetManager: Saved crop asset type=" + (isCommon ? "common" : cropType.ToString()) + " stage=" + stage);
            }
        }
    }

    /// <summary>
    /// Lưu common crop asset (seed, sprout1, sprout2 dùng chung)
    /// </summary>
    public void SaveCommonCropAsset(sbyte stage, Image img)
    {
        SaveCropAsset(-1, stage, img, true);
    }

    /// <summary>
    /// Lấy crop asset từ cache
    /// Tự động lấy common asset cho các giai đoạn đầu
    /// Nếu chưa có, thử load từ local
    /// </summary>
    public Image GetCropAsset(sbyte cropType, sbyte stage)
    {
        if (stage >= 0 && stage <= FarmConstants.STAGE_COUNT)
        {
            int typeIndex;
            
            // Nếu là early stage (seed, sprout1, sprout2), lấy từ common
            if (IsEarlyStage(stage))
            {
                typeIndex = COMMON_INDEX;
            }
            else if (cropType >= 0 && cropType < FarmConstants.CROP_TYPE_COUNT)
            {
                typeIndex = cropType;
            }
            else
            {
                return null;
            }
            
            // Nếu đã có trong cache, trả về
            if (cropAssets[typeIndex, stage] != null)
            {
                return cropAssets[typeIndex, stage];
            }
            
            // Chưa có, thử load từ SmallImage (placeholder)
            Image img = TryLoadCropImageFromLocal(cropType, stage);
            if (img != null)
            {
                cropAssets[typeIndex, stage] = img;
                return img;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Thử load hình ảnh cây trồng từ local (placeholder)
    /// </summary>
    /// <summary>
    /// Thử load hình ảnh cây trồng từ local (placeholder)
    /// </summary>
    /// <summary>
    /// Thử load hình ảnh cây trồng từ local Resources (fallback)
    /// </summary>
    private Image TryLoadCropImageFromLocal(sbyte cropType, sbyte stage)
    {
        try
        {
            // Xác định tên file
            string filename = "";
            bool isCommon = FarmAssetManager.IsEarlyStage(stage);
            
            if (isCommon)
            {
                string stageName = "";
                switch(stage) 
                {
                    case FarmConstants.STAGE_SEED: stageName = "seed"; break;
                    case FarmConstants.STAGE_SPROUT_1: stageName = "sprout1"; break;
                    case FarmConstants.STAGE_SPROUT_2: stageName = "sprout2"; break;
                }
                filename = "crop_common_" + stageName;
            }
            else
            {
                string cropName = "";
                switch(cropType)
                {
                    case FarmConstants.CROP_TOMATO: cropName = "tomato"; break;
                    case FarmConstants.CROP_STARFRUIT: cropName = "starfruit"; break;
                    case FarmConstants.CROP_CORN: cropName = "corn"; break;
                    case FarmConstants.CROP_PUMPKIN: cropName = "pumpkin"; break;
                }
                
                string stageName = "";
                switch(stage)
                {
                    case FarmConstants.STAGE_YOUNG: stageName = "young"; break;
                    case FarmConstants.STAGE_MATURE: stageName = "mature"; break;
                    case FarmConstants.STAGE_WITHERED: stageName = "withered"; break;
                }
                
                if (cropName != "" && stageName != "")
                {
                    filename = "crop_" + cropName + "_" + stageName;
                }
            }
            
            if (!string.IsNullOrEmpty(filename))
            {
                // Thử load từ Resources/farm/x{zoom}/
                string resourcePath = "farm/x" + mGraphics.zoomLevel + "/" + filename;
                UnityEngine.Texture2D texture = UnityEngine.Resources.Load<UnityEngine.Texture2D>(resourcePath);
                
                if (texture == null)
                {
                    // Fallback to x2 or x1 if specific zoom not found
                     resourcePath = "farm/x2/" + filename;
                     texture = UnityEngine.Resources.Load<UnityEngine.Texture2D>(resourcePath);
                }
                
                if (texture != null)
                {
                    Image img = new Image();
                    img.texture = texture;
                    img.w = img.texture.width;
                    img.h = img.texture.height;
                    Image.setTextureQuality(img.texture);
                    Res.outz("Loaded local crop asset: " + resourcePath);
                    return img;
                }
            }
        }
        catch (Exception ex)
        {
            Res.outz("FarmAssetManager: Error loading local crop image - " + ex.Message);
        }
        return null;
    }

    /// <summary>
    /// Kiểm tra có phải early stage không (dùng chung asset)
    /// </summary>
    public static bool IsEarlyStage(sbyte stage)
    {
        return stage == FarmConstants.STAGE_SEED || 
               stage == FarmConstants.STAGE_SPROUT_1 || 
               stage == FarmConstants.STAGE_SPROUT_2;
    }

    /// <summary>
    /// Lưu plot empty image
    /// </summary>
    public void SavePlotEmptyImage(Image img)
    {
        plotEmptyImage = img;
        Res.outz("FarmAssetManager: Saved plot empty image");
    }

    /// <summary>
    /// Lấy plot empty image
    /// </summary>
    public Image GetPlotEmptyImage()
    {
        return plotEmptyImage;
    }

    /// <summary>
    /// Xóa tất cả cache
    /// </summary>
    public void ClearAll()
    {
        farmIcons.Clear();
        cropAssets = new Image[FarmConstants.CROP_TYPE_COUNT + 1, FarmConstants.STAGE_COUNT + 1];
        plotEmptyImage = null;
        Res.outz("FarmAssetManager: Cleared all cache");
    }

    /// <summary>
    /// Kiểm tra có asset không
    /// </summary>
    public bool HasCropAsset(sbyte cropType, sbyte stage)
    {
        return GetCropAsset(cropType, stage) != null;
    }

    /// <summary>
    /// Kiểm tra có icon không
    /// </summary>
    public bool HasFarmIcon(string iconName)
    {
        return farmIcons.ContainsKey(iconName);
    }
}
