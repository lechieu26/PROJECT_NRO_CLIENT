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
    // Với early stages (seed, sprout1, sprout2), cropType = 255 (common)
    private Image[,] cropAssets = new Image[256, FarmConstants.STAGE_COUNT + 1];

    // Common assets cho các giai đoạn đầu (index = 255)
    private const int COMMON_INDEX = 255;

    // Plot empty image
    private Image plotEmptyImage;

    // Plot selected image (viền highlight khi chọn ô đất)
    private Image plotSelectedImage;

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
        // Kiểm tra RAM Cache
        if (farmIcons.ContainsKey(iconName))
        {
            return farmIcons[iconName];
        }

        // Kiểm tra file RMS (VD "farm_icon_water.png" hoặc "farm_water.png" - phụ thuộc log file RMS lưu bằng controller)
        Image rmsImg = LoadRMSImage("farm_icon_" + iconName + ".png");
        if (rmsImg == null)
            rmsImg = LoadRMSImage("farm_" + iconName + ".png");

        if (rmsImg != null)
        {
            SaveFarmIcon(iconName, rmsImg);
            return rmsImg;
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
            if (typeIndex >= 0 && typeIndex < 256)
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
            else if (cropType >= 0 && cropType < 255)
            {
                typeIndex = cropType;
            }
            else
            {
                return null;
            }
            
            // Nếu đã có trong RAM, trả về
            if (cropAssets[typeIndex, stage] != null)
            {
                return cropAssets[typeIndex, stage];
            }
            
            // Chưa có trong RAM, thử load từ bộ nhớ Cache đệm RMS (lưu từ Controller -74)
            string filename = GetCropFilename(cropType, stage);
            if (!string.IsNullOrEmpty(filename))
            {
                Image rmsImg = LoadRMSImage("farm_" + filename);
                if (rmsImg != null)
                {
                    cropAssets[typeIndex, stage] = rmsImg;
                    return rmsImg;
                }
            }
            
            // Chưa có trong RMS, thử load từ resources local của APK/PC build
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
    /// Hàm load hình từ RMS Cache Storage Memory
    /// </summary>
    private Image LoadRMSImage(string rmsName)
    {
        try
        {
            sbyte[] fileData = Rms.loadRMS(rmsName);
            if (fileData != null && fileData.Length > 0)
            {
                return Image.createImage(fileData, 0, fileData.Length);
            }
        }
        catch (Exception ex)
        {
            Res.outz("FarmAssetManager: Error loading RMS " + rmsName + " - " + ex.Message);
        }
        return null;
    }
    
    /// <summary>
    /// Helper trả về tên crop theo quy chuẩn (vd: crop_tomato_mature)
    /// </summary>
    private string GetCropFilename(sbyte cropType, sbyte stage)
    {
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
            if (FarmConstants.cropTemplates.ContainsKey(cropType))
            {
                FarmConstants.CropTemplateInfo info = FarmConstants.cropTemplates[cropType];
                switch (stage)
                {
                    case FarmConstants.STAGE_YOUNG: filename = info.imgYoung; break;
                    case FarmConstants.STAGE_MATURE: filename = info.imgMature; break;
                    case FarmConstants.STAGE_WITHERED: filename = info.imgWithered; break;
                }
            }
        }
        
        // Safety fallback if filename is still empty
        if (string.IsNullOrEmpty(filename))
        {
            filename = "crop_" + cropType + "_" + stage;
        }
        
        return filename;
    }

    /// <summary>
    /// Thử load hình ảnh cây trồng từ local Resources (fallback)
    /// </summary>
    private Image TryLoadCropImageFromLocal(sbyte cropType, sbyte stage)
    {
        try
        {
            // Xác định tên file
            string filename = GetCropFilename(cropType, stage);
            
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
        if (plotEmptyImage != null)
            return plotEmptyImage;
            
        Image rmsImg = LoadRMSImage("farm_plot_empty.png");
        if (rmsImg != null)
        {
            plotEmptyImage = rmsImg;
            return rmsImg;
        }
            
        return plotEmptyImage;
    }

    /// <summary>
    /// Lưu plot selected image
    /// </summary>
    public void SavePlotSelectedImage(Image img)
    {
        plotSelectedImage = img;
        Res.outz("FarmAssetManager: Saved plot selected image");
    }

    /// <summary>
    /// Lấy plot selected image
    /// </summary>
    public Image GetPlotSelectedImage()
    {
        if (plotSelectedImage != null)
            return plotSelectedImage;
            
        Image rmsImg = LoadRMSImage("farm_plot_selected.png");
        if (rmsImg != null)
        {
            plotSelectedImage = rmsImg;
            return rmsImg;
        }
            
        return plotSelectedImage;
    }

    /// <summary>
    /// Xóa tất cả cache
    /// </summary>
    public void ClearAll()
    {
        farmIcons.Clear();
        cropAssets = new Image[256, FarmConstants.STAGE_COUNT + 1];
        plotEmptyImage = null;
        plotSelectedImage = null;
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
