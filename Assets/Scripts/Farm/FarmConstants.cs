using System;

/// <summary>
/// Constants cho hệ thống Cloud Garden phía Client
/// </summary>
public class FarmConstants
{
    // ===================== CROP TYPES =====================
    public const sbyte CROP_TOMATO = 0;      // Cà chua
    public const sbyte CROP_STARFRUIT = 1;   // Khế
    public const sbyte CROP_CORN = 2;        // Ngô
    public const sbyte CROP_PUMPKIN = 3;     // Bí
    public const int CROP_TYPE_COUNT = 4;

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
    public const sbyte MSG_FARM_ASSET = -33;
    public const sbyte MSG_FARM_DATA = -34;
    
    // Sub-types for MSG_FARM_ASSET (-33)
    public const sbyte SUBTYPE_FARM_ASSET = 10;
    public const sbyte SUBTYPE_CROP_ASSET = 11;
    public const sbyte SUBTYPE_FARM_ICON = 12;
    
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
        switch (cropType)
        {
            case CROP_TOMATO: return "Cà chua";
            case CROP_STARFRUIT: return "Khế";
            case CROP_CORN: return "Ngô";
            case CROP_PUMPKIN: return "Bí";
            default: return "Không xác định";
        }
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

    // ===================== SEED ITEM IDs =====================
    public const short SEED_TOMATO_ID = 1872;
    public const short SEED_PUMPKIN_ID = 1873;
    public const short SEED_STARFRUIT_ID = 1874;
    public const short SEED_CORN_ID = 1875;

    public static short GetSeedItemId(sbyte cropType)
    {
        switch (cropType)
        {
            case CROP_TOMATO: return SEED_TOMATO_ID;
            case CROP_STARFRUIT: return SEED_STARFRUIT_ID;
            case CROP_CORN: return SEED_CORN_ID;
            case CROP_PUMPKIN: return SEED_PUMPKIN_ID;
            default: return -1;
        }
    }


    // ===================== HARVEST ITEM IDs =====================
    public const short HARVEST_TOMATO_ID = 1876;
    public const short HARVEST_PUMPKIN_ID = 1877;
    public const short HARVEST_STARFRUIT_ID = 1878;
    public const short HARVEST_CORN_ID = 1879;

    public static short GetHarvestItemId(sbyte cropType)
    {
        switch (cropType)
        {
            case CROP_TOMATO: return HARVEST_TOMATO_ID;
            case CROP_STARFRUIT: return HARVEST_STARFRUIT_ID;
            case CROP_CORN: return HARVEST_CORN_ID;
            case CROP_PUMPKIN: return HARVEST_PUMPKIN_ID;
            default: return -1;
        }
    }
}
