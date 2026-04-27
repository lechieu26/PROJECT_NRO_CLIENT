using System;
using System.Collections.Generic;
/// <summary>
/// Constants cho hệ thống Cloud Garden phía Client
/// </summary>
public class FarmConstants
{
    // ===================== CROP TEMPLATES =====================
    public class CropTemplateInfo
    {
        public sbyte id;
        public string name;
        public short seedItemId;
        public short harvestItemId;
        public string imgYoung;
        public string imgMature;
        public string imgWithered;
    }

    public static Dictionary<sbyte, CropTemplateInfo> cropTemplates = new Dictionary<sbyte, CropTemplateInfo>();

    public static void AddCropTemplate(CropTemplateInfo info)
    {
        cropTemplates[info.id] = info;
    }

    public static int GetCropTypeCount()
    {
        return cropTemplates.Count;
    }
    // ===================== GROWTH STAGES =====================
    public const sbyte STAGE_EMPTY = 0;      // Đất trống
    public const sbyte STAGE_SEED = 1;       // Hạt giống
    public const sbyte STAGE_SPROUT_1 = 2;   // Mầm 1
    public const sbyte STAGE_SPROUT_2 = 3;   // Mầm 2
    public const sbyte STAGE_YOUNG = 4;      // Cây non
    public const sbyte STAGE_MATURE = 5;     // Trưởng thành
    public const sbyte STAGE_WITHERED = 6;   // Héo
    public const int STAGE_COUNT = 6;

    // ===================== MAP IDs =====================
    // Sử dụng lại map nhà theo gender
    public const int MAP_CLOUD_GARDEN_TD = 39;  // Trái Đất (gender 0)
    public const int MAP_CLOUD_GARDEN_NM = 40;  // Namếc (gender 1)
    public const int MAP_CLOUD_GARDEN_XD = 41;  // Xayda (gender 2)

    // ===================== PLOT CONFIG =====================
    public const int INITIAL_PLOTS = 5; // Số ô mở khóa ban đầu
    public const int MAX_PLOTS = 10;

    // ===================== MESSAGE TYPES =====================
    public const sbyte MSG_FARM_ASSET = -58;   // Đã đổi sang -58 để tránh xung đột với Offline Map (-33) và Mabu Power (-115)
    public const sbyte MSG_FARM_DATA = -34;
    
    // Sub-types for MSG_FARM_ASSET (-115)
    public const sbyte SUBTYPE_FARM_ASSET = 10;
    public const sbyte SUBTYPE_CROP_ASSET = 11;
    public const sbyte SUBTYPE_FARM_ICON = 12;
    public const sbyte SUBTYPE_CROP_TEMPLATE = 13;
    
    // Sub-types for MSG_FARM_DATA (-34)
    public const sbyte SUBTYPE_PLOT_UPDATE = 10;
    public const sbyte DATA_UPDATE_SINGLE = 0;
    public const sbyte DATA_UPDATE_FULL = 1;
    public const sbyte DATA_OPEN_SEED_PANEL = 2;  // Server yêu cầu mở panel chọn hạt
    public const sbyte DATA_CLOSE_DIALOG = 3;    // Server yêu cầu đóng dialog
    public const sbyte DATA_HARVEST_SUCCESS = 4; // Hiệu ứng thu hoạch

    // ===================== HELPER METHODS =====================

    /// <summary>
    /// Kiểm tra map có phải Cloud Garden không
    /// </summary>
    public static bool IsCloudGardenMap(int mapId)
    {
        return mapId >= MAP_CLOUD_GARDEN_TD && mapId <= MAP_CLOUD_GARDEN_XD;
    }

    /// <summary>
    /// Lấy tên loại cây
    /// </summary>
    public static string GetCropName(sbyte cropType)
    {
        if (cropTemplates.ContainsKey(cropType))
        {
            return cropTemplates[cropType].name;
        }
        return "Không xác định";
    }

    /// <summary>
    /// Lấy tên giai đoạn
    /// </summary>
    public static string GetStageName(sbyte stage)
    {
        switch (stage)
        {
            case STAGE_EMPTY: return "Đất trống";
            case STAGE_SEED: return "Hạt giống";
            case STAGE_SPROUT_1: return "Mầm 1";
            case STAGE_SPROUT_2: return "Mầm 2";
            case STAGE_YOUNG: return "Cây non";
            case STAGE_MATURE: return "Thu hoạch";
            case STAGE_WITHERED: return "Héo";
            default: return "Không xác định";
        }
    }

    // ===================== SEED/HARVEST IDs (Removed hardcoded constants) =====================

    public static short GetSeedItemId(sbyte cropType)
    {
        if (cropTemplates.ContainsKey(cropType))
        {
            return cropTemplates[cropType].seedItemId;
        }
        return -1;
    }

    public static bool IsSeedItem(short itemId)
    {
        foreach (var crop in cropTemplates.Values)
        {
            if (crop.seedItemId == itemId)
            {
                return true;
            }
        }
        return false;
    }

    public static short GetHarvestItemId(sbyte cropType)
    {
        if (cropTemplates.ContainsKey(cropType))
        {
            return cropTemplates[cropType].harvestItemId;
        }
        return -1;
    }
}
