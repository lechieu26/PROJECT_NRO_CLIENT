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
    // Với early stages (seed, sprout1, sprout2), cropType = COMMON_INDEX
    private const int MAX_CROP_TYPES = 32; // Giới hạn hợp lý cho mobile
    private Image[,] cropAssets = new Image[MAX_CROP_TYPES + 1, FarmConstants.STAGE_COUNT + 1];

    // Common assets cho các giai đoạn đầu (index = MAX_CROP_TYPES)
    private const int COMMON_INDEX = MAX_CROP_TYPES;

    // Plot empty image
    private Image plotEmptyImage;

    // Plot selected image (viền highlight khi chọn ô đất)
    private Image plotSelectedImage;

    // Raw data cache for thread-safe image creation (Main Thread only)
    private Dictionary<string, sbyte[]> pendingData = new Dictionary<string, sbyte[]>();

    // Missing assets cache to avoid repeated failed load attempts
    private Dictionary<string, bool> missingAssets = new Dictionary<string, bool>();

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
    /// Lưu farm icon vào cache (chỉ lưu data, sẽ tạo Image ở Main Thread)
    /// </summary>
    public void SaveFarmIcon(string iconName, sbyte[] data)
    {
        if (data != null && data.Length > 0)
        {
            string key = "icon_" + iconName;
            pendingData[key] = data;
            
            // Xóa khỏi cache Image cũ nếu có để force reload từ data mới
            if (farmIcons.ContainsKey(iconName))
                farmIcons.Remove(iconName);
                
            if (missingAssets.ContainsKey(key))
                missingAssets.Remove(key);
                
            Res.outz("FarmAssetManager: Queued icon data " + iconName);
        }
    }

    /// <summary>
    /// Lấy farm icon từ cache
    /// </summary>
    public Image GetFarmIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return null;

        // 1. Kiểm tra RAM Cache (Image đã tạo)
        if (farmIcons.ContainsKey(iconName))
        {
            return farmIcons[iconName];
        }

        // 2. Kiểm tra dữ liệu đang chờ tạo Image (Thread safety)
        string dataKey = "icon_" + iconName;
        if (pendingData.ContainsKey(dataKey))
        {
            try
            {
                sbyte[] data = pendingData[dataKey];
                Image img = Image.createImage(data, 0, data.Length);
                if (img != null)
                {
                    farmIcons[iconName] = img;
                    pendingData.Remove(dataKey);
                    return img;
                }
            }
            catch (Exception ex)
            {
                Res.outz("FarmAssetManager: Error creating icon " + iconName + " - " + ex.Message);
            }
        }

        // 3. Kiểm tra danh sách đã biết là thiếu
        if (missingAssets.ContainsKey(dataKey))
        {
            return null;
        }

        // 4. Kiểm tra file RMS
        Image rmsImg = LoadRMSImage("farm_icon_" + iconName + ".png");
        if (rmsImg == null)
            rmsImg = LoadRMSImage("farm_" + iconName + ".png");

        if (rmsImg != null)
        {
            farmIcons[iconName] = rmsImg;
            return rmsImg;
        }

        // 5. Nếu vẫn không thấy, đánh dấu là thiếu để không thử lại
        missingAssets[dataKey] = true;
        return null;
    }

    /// <summary>
    /// Lưu crop asset vào cache (chỉ lưu data, sẽ tạo Image ở Main Thread)
    /// </summary>
    public void SaveCropAsset(sbyte cropType, sbyte stage, sbyte[] data, bool isCommon = false)
    {
        if (stage >= 0 && stage <= FarmConstants.STAGE_COUNT && data != null && data.Length > 0)
        {
            // Dùng key cố định (typeIndex, stage) để tránh timing issue
            // (data có thể đến trước cropTemplate info)
            int typeIndex = (isCommon || cropType == -1) ? COMMON_INDEX : cropType;
            if (typeIndex < 0 || typeIndex > MAX_CROP_TYPES) return;

            string dataKey = "crop_" + typeIndex + "_" + stage;
            pendingData[dataKey] = data;

            // Clear Image cache cũ
            cropAssets[typeIndex, stage] = null;
                
            // Clear missing flags
            string filename = GetCropFilename(cropType, stage);
            if (!string.IsNullOrEmpty(filename) && missingAssets.ContainsKey(filename))
                missingAssets.Remove(filename);
            if (missingAssets.ContainsKey(dataKey))
                missingAssets.Remove(dataKey);
                    
            Res.outz("FarmAssetManager: Queued crop data key=" + dataKey + " len=" + data.Length);
        }
    }

    /// <summary>
    /// Lưu common crop asset (seed, sprout1, sprout2 dùng chung)
    /// </summary>
    public void SaveCommonCropAsset(sbyte stage, sbyte[] data)
    {
        SaveCropAsset(-1, stage, data, true);
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
            else if (cropType >= 0 && cropType < MAX_CROP_TYPES)
            {
                typeIndex = cropType;
            }
            else
            {
                return null;
            }
            
            // 1. Nếu đã có trong RAM, trả về
            if (cropAssets[typeIndex, stage] != null)
            {
                return cropAssets[typeIndex, stage];
            }

            // 2. Kiểm tra dữ liệu đang chờ tạo Image (Thread safety)
            // Dùng key cố định (typeIndex, stage) khớp với SaveCropAsset
            string dataKey = "crop_" + typeIndex + "_" + stage;
            if (pendingData.ContainsKey(dataKey))
            {
                try
                {
                    sbyte[] data = pendingData[dataKey];
                    Image cropImg = Image.createImage(data, 0, data.Length);
                    if (cropImg != null)
                    {
                        cropAssets[typeIndex, stage] = cropImg;
                        pendingData.Remove(dataKey);
                        return cropImg;
                    }
                }
                catch (Exception ex)
                {
                    Res.outz("FarmAssetManager: Error creating crop image key=" + dataKey + " - " + ex.Message);
                    pendingData.Remove(dataKey);
                }
            }
            
            // 3. Kiểm tra danh sách đã biết là thiếu
            if (missingAssets.ContainsKey(dataKey))
            {
                return null;
            }
            
            // 4. Chưa có trong RAM, thử load từ bộ nhớ Cache đệm RMS
            // RMS key: "farm_crop_tomato_mature.png" (lưu bởi Controller case -74 sub-type 5)
            string filename = GetCropFilename(cropType, stage);
            if (!string.IsNullOrEmpty(filename))
            {
                Image rmsImg = LoadRMSImage("farm_" + filename + ".png");
                if (rmsImg == null)
                    rmsImg = LoadRMSImage("farm_" + filename);
                if (rmsImg != null)
                {
                    cropAssets[typeIndex, stage] = rmsImg;
                    return rmsImg;
                }
            }
            
            // 5. Chưa có trong RMS, thử load từ resources local của APK/PC build
            Image img = TryLoadCropImageFromLocal(cropType, stage);
            if (img != null)
            {
                cropAssets[typeIndex, stage] = img;
                return img;
            }

            // 6. Nếu vẫn không thấy, đánh dấu là thiếu để không thử lại mỗi frame
            missingAssets[dataKey] = true;
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
            string filename = GetCropFilename(cropType, stage);
            if (string.IsNullOrEmpty(filename)) return null;

            // Thử load từ Resources/farm/x{zoom}/
            string resourcePath = "farm/x" + mGraphics.zoomLevel + "/" + filename;
            UnityEngine.Texture2D texture = UnityEngine.Resources.Load<UnityEngine.Texture2D>(resourcePath);
            
            if (texture == null)
            {
                // Fallback to x2 if specific zoom not found
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
    /// Lưu plot empty image (chỉ lưu data)
    /// </summary>
    public void SavePlotEmptyImage(sbyte[] data)
    {
        if (data != null && data.Length > 0)
        {
            pendingData["plot_empty_data"] = data;
            plotEmptyImage = null;
            if (missingAssets.ContainsKey("plot_empty"))
                missingAssets.Remove("plot_empty");
            Res.outz("FarmAssetManager: Queued plot empty data");
        }
    }

    /// <summary>
    /// Lấy plot empty image
    /// </summary>
    public Image GetPlotEmptyImage()
    {
        if (plotEmptyImage != null)
            return plotEmptyImage;
            
        if (pendingData.ContainsKey("plot_empty_data"))
        {
            try
            {
                sbyte[] data = pendingData["plot_empty_data"];
                plotEmptyImage = Image.createImage(data, 0, data.Length);
                pendingData.Remove("plot_empty_data");
                if (plotEmptyImage != null) return plotEmptyImage;
            }
            catch (Exception ex)
            {
                Res.outz("FarmAssetManager: Error creating plot empty image - " + ex.Message);
                pendingData.Remove("plot_empty_data");
            }
        }

        if (missingAssets.ContainsKey("plot_empty"))
            return null;

        Image rmsImg = LoadRMSImage("farm_plot_empty.png");
        if (rmsImg != null)
        {
            plotEmptyImage = rmsImg;
            return rmsImg;
        }
            
        missingAssets["plot_empty"] = true;
        return null;
    }

    /// <summary>
    /// Lưu plot selected image (chỉ lưu data)
    /// </summary>
    public void SavePlotSelectedImage(sbyte[] data)
    {
        if (data != null && data.Length > 0)
        {
            pendingData["plot_selected_data"] = data;
            plotSelectedImage = null;
            if (missingAssets.ContainsKey("plot_selected"))
                missingAssets.Remove("plot_selected");
            Res.outz("FarmAssetManager: Queued plot selected data");
        }
    }

    /// <summary>
    /// Lấy plot selected image
    /// </summary>
    public Image GetPlotSelectedImage()
    {
        if (plotSelectedImage != null)
            return plotSelectedImage;
            
        if (pendingData.ContainsKey("plot_selected_data"))
        {
            sbyte[] data = pendingData["plot_selected_data"];
            plotSelectedImage = Image.createImage(data, 0, data.Length);
            pendingData.Remove("plot_selected_data");
            if (plotSelectedImage != null) return plotSelectedImage;
        }

        if (missingAssets.ContainsKey("plot_selected"))
            return null;

        Image rmsImg = LoadRMSImage("farm_plot_selected.png");
        if (rmsImg != null)
        {
            plotSelectedImage = rmsImg;
            return rmsImg;
        }
            
        missingAssets["plot_selected"] = true;
        return null;
    }

    /// <summary>
    /// Xóa tất cả cache
    /// </summary>
    public void ClearAll()
    {
        farmIcons.Clear();
        missingAssets.Clear();
        cropAssets = new Image[MAX_CROP_TYPES + 1, FarmConstants.STAGE_COUNT + 1];
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
        return GetFarmIcon(iconName) != null;
    }
}
