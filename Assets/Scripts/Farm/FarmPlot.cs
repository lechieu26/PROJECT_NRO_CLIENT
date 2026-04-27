using System;

/// <summary>
/// Đại diện cho một ô ruộng trong Cloud Garden phía Client
/// </summary>
public class FarmPlot
{
    public int plotId;              // ID ô ruộng (0-9)
    public bool unlocked;           // Đã mở khóa chưa
    public sbyte currentStage;      // Giai đoạn hiện tại (0-6)
    public sbyte cropType;          // Loại cây (-1 = trống)
    public bool watered;            // Đã tưới nước chưa
    public int posX;                // Vị trí X trên map
    public int posY;                // Vị trí Y trên map

    // Thời gian
    public int serverTimeToHarvest; // Thời gian còn lại từ server (giây)
    public long lastUpdateTime;     // Thời điểm nhận update từ server

    public FarmPlot()
    {
        plotId = -1;
        unlocked = false;
        currentStage = FarmConstants.STAGE_EMPTY;
        cropType = -1;
        watered = false;
        posX = 0;
        posY = 0;
        serverTimeToHarvest = 0;
        lastUpdateTime = 0;
    }

    public FarmPlot(int id)
    {
        plotId = id;
        unlocked = id < FarmConstants.INITIAL_PLOTS;
        currentStage = FarmConstants.STAGE_EMPTY;
        cropType = -1;
        watered = false;
        posX = 0;
        posY = 0;
        serverTimeToHarvest = 0;
        lastUpdateTime = 0;
    }

    /// <summary>
    /// Tính thời gian còn lại (giây)
    /// </summary>
    public int GetRemainingTime()
    {
        if (serverTimeToHarvest <= 0) return 0;
        
        long elapsed = (mSystem.currentTimeMillis() - lastUpdateTime) / 1000;
        int remaining = serverTimeToHarvest - (int)elapsed;
        return remaining > 0 ? remaining : 0;
    }

    /// <summary>
    /// Format thời gian để hiển thị
    /// >= 60 phút: hh:mm
    /// < 60 phút: mm:ss
    /// </summary>
    public string GetTimeDisplay()
    {
        int totalSeconds = GetRemainingTime();
        if (totalSeconds <= 0 && currentStage == FarmConstants.STAGE_MATURE)
        {
            return "Sẵn sàng!";
        }
        if (totalSeconds <= 0) return "";

        int totalMinutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (totalMinutes >= 60)
        {
            // >= 60 phút: hiển thị Xh:Ym (ví dụ: 2h:30m)
            return hours + "h:" + (minutes < 10 ? "0" : "") + minutes + "m";
        }
        else
        {
            // < 60 phút: hiển thị mm:ss
            return totalMinutes + ":" + (seconds < 10 ? "0" : "") + seconds;
        }
    }

    /// <summary>
    /// Kiểm tra ô có trống không
    /// </summary>
    public bool IsEmpty()
    {
        return currentStage == FarmConstants.STAGE_EMPTY || cropType == -1;
    }

    /// <summary>
    /// Kiểm tra có thể thu hoạch không
    /// Cây phải ở stage MATURE VÀ đã hết thời gian chờ, HOẶC cây bị héo (WITHERED)
    /// </summary>
    public bool CanHarvest()
    {
        return (currentStage == FarmConstants.STAGE_MATURE && GetRemainingTime() <= 0)
               || currentStage == FarmConstants.STAGE_WITHERED;
    }

    /// <summary>
    /// Kiểm tra cây có đang bị héo không
    /// </summary>
    public bool IsWithered()
    {
        return currentStage == FarmConstants.STAGE_WITHERED;
    }

    /// <summary>
    /// Kiểm tra đang phát triển
    /// </summary>
    public bool IsGrowing()
    {
        return currentStage > FarmConstants.STAGE_EMPTY && 
               currentStage < FarmConstants.STAGE_MATURE;
    }

    /// <summary>
    /// Lấy stage hiển thị dựa trên thời gian còn lại
    /// Tỷ lệ 1:2:2:4:1 = 10 phần (seed:sprout1:sprout2:young:mature)
    /// </summary>
    /// <summary>
    /// Lấy stage hiển thị
    /// Fix: Sử dụng trực tiếp currentStage từ Server
    /// Nếu cây bị héo, trả về STAGE_WITHERED để load asset withered.png
    /// </summary>
    public sbyte GetVisualStage()
    {
        if (IsEmpty()) return FarmConstants.STAGE_EMPTY;
        
        // Nếu cây bị héo, hiển thị asset héo
        if (currentStage == FarmConstants.STAGE_WITHERED)
        {
            return FarmConstants.STAGE_WITHERED;
        }
        
        // Nếu server bảo là mature hoặc client tính toán hết giờ
        if (currentStage == FarmConstants.STAGE_MATURE || GetRemainingTime() <= 0) 
        {
            return FarmConstants.STAGE_MATURE;
        }
        
        // Trả về stage hiện tại do server gửi
        return currentStage;
    }

    /// <summary>
    /// Lấy image để hiển thị
    /// </summary>
    public Image GetDisplayImage()
    {
        if (!unlocked)
        {
            // Ô chưa mở khóa - có thể hiện icon khóa
            return FarmAssetManager.GI().GetFarmIcon("locked");
        }

        if (IsEmpty())
        {
            return FarmAssetManager.GI().GetPlotEmptyImage();
        }

        return FarmAssetManager.GI().GetCropAsset(cropType, currentStage);
    }

    /// <summary>
    /// Lấy tên trạng thái
    /// </summary>
    public string GetStatusText()
    {
        if (!unlocked) return "Đã khóa";
        if (IsEmpty()) return "Trống";
        if (IsWithered()) return "Bị héo!";
        if (CanHarvest()) return "Thu hoạch";
        if (IsGrowing()) return FarmConstants.GetStageName(currentStage);
        
        // Trường hợp đặc biệt: Đang ở MATURE nhưng chưa hết giờ
        if (currentStage == FarmConstants.STAGE_MATURE && GetRemainingTime() > 0)
        {
            return "Đang chín";
        }
        return "";
    }

    /// <summary>
    /// Cập nhật từ server
    /// </summary>
    public void UpdateFromServer(bool isUnlocked, sbyte stage, sbyte crop, int timeToHarvest, bool isWatered)
    {
        this.unlocked = isUnlocked;
        this.currentStage = stage;
        this.cropType = crop;
        this.serverTimeToHarvest = timeToHarvest;
        this.watered = isWatered;
        this.lastUpdateTime = mSystem.currentTimeMillis();
    }

    /// <summary>
    /// Kiểm tra điểm click có nằm trong ô hình thoi (isometric diamond) không
    /// Ô đất được vẽ với anchor BOTTOM | HCENTER tại (posX, posY)
    /// 
    /// Hình thoi có 4 đỉnh:
    ///         (posX, posY - height)     ← đỉnh trên
    ///  (posX - w/2, posY - h/2)   (posX + w/2, posY - h/2)  ← 2 cạnh
    ///         (posX, posY)              ← đáy dưới
    /// 
    /// Thuật toán: Manhattan distance từ tâm hình thoi
    /// |dx| / halfW + |dy| / halfH <= 1.0
    /// </summary>
    public bool ContainsPoint(int x, int y, int width, int height)
    {
        // Tâm hình thoi (anchor BOTTOM | HCENTER → đáy = posY, tâm Y = posY - height/2)
        float centerX = posX;
        float centerY = posY - height / 2.0f;

        // Nửa chiều rộng và nửa chiều cao của hình thoi
        float halfW = width / 2.0f;
        float halfH = height / 2.0f;

        // Tránh chia cho 0
        if (halfW <= 0 || halfH <= 0) return false;

        // Khoảng cách tương đối từ tâm
        float dx = x - centerX;
        if (dx < 0) dx = -dx;
        float dy = y - centerY;
        if (dy < 0) dy = -dy;

        // Kiểm tra Manhattan distance: điểm nằm trong thoi khi tổng tỷ lệ <= 1
        return (dx / halfW + dy / halfH) <= 1.0f;
    }
}
