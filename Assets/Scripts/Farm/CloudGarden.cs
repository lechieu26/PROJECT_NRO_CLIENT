using System;
using System.Collections.Generic;

/// <summary>
/// Quản lý toàn bộ khu vườn của player phía Client
/// Vị trí các ô đất được quản lý tập trung trên Server và gửi về Client
/// </summary>
public class CloudGarden
{
    private static CloudGarden instance;

    public FarmPlot[] plots;        // Danh sách ô ruộng
    public int unlockedPlots;       // Số ô đã mở khóa
    public bool isInitialized;      // Đã khởi tạo chưa (từ server)
    private bool yAdjustApplied;    // Đã apply offset Y cho mặt đất chưa
    
    // Logic hiển thị click
    public int focusedPlotId = -1; // ID ô đang được bấm vào

    // Layout config
    private const int PLOT_WIDTH = 48;
    private const int PLOT_HEIGHT = 48;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static CloudGarden GI()
    {
        if (instance == null)
        {
            instance = new CloudGarden();
        }
        return instance;
    }

    public CloudGarden()
    {
        plots = new FarmPlot[FarmConstants.MAX_PLOTS];
        for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
        {
            plots[i] = new FarmPlot(i);
            // Vị trí sẽ được set từ Server qua UpdateFullGarden()
        }
        unlockedPlots = FarmConstants.INITIAL_PLOTS;
        isInitialized = false;
        yAdjustApplied = false;
    }

    /// <summary>
    /// Khởi tạo lại garden
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
        {
            plots[i] = new FarmPlot(i);
            // Vị trí sẽ được set từ Server qua UpdateFullGarden()
        }
        unlockedPlots = FarmConstants.INITIAL_PLOTS;
        isInitialized = false;
        yAdjustApplied = false;
    }

    /// <summary>
    /// Cập nhật một ô từ server
    /// </summary>
    public void UpdateSinglePlot(int plotId, bool unlocked, sbyte stage, sbyte cropType, 
                                  int timeToHarvest, bool watered)
    {
        if (plotId >= 0 && plotId < FarmConstants.MAX_PLOTS)
        {
            plots[plotId].UpdateFromServer(unlocked, stage, cropType, timeToHarvest, watered);
            Res.outz("CloudGarden: Updated plot " + plotId);
            Res.outz("chieu.lq: [Client] CloudGarden visual update plot " + plotId);
        }
    }

    /// <summary>
    /// Cập nhật toàn bộ garden từ server (khi vào map)
    /// Server gửi vị trí các ô đất tập trung để quản lý
    /// </summary>
    public void UpdateFullGarden(Message msg)
    {
        try
        {
            unlockedPlots = msg.reader().readInt();
            Res.outz("CloudGarden.UpdateFullGarden: unlockedPlots=" + unlockedPlots + " available=" + msg.reader().available());
            
            // Validate bounds
            if (unlockedPlots < 0) unlockedPlots = 0;
            if (unlockedPlots > FarmConstants.MAX_PLOTS) unlockedPlots = FarmConstants.MAX_PLOTS;

            if (plots == null) plots = new FarmPlot[FarmConstants.MAX_PLOTS];

            for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
            {
                int plotId = msg.reader().readInt();
                bool unlocked = msg.reader().readBool();
                sbyte stage = msg.reader().readByte();
                sbyte cropType = msg.reader().readByte();
                int timeToHarvest = msg.reader().readInt();
                bool watered = msg.reader().readBool();
                short posX = msg.reader().readShort();
                short posY = msg.reader().readShort();

                if (i == 0) // Log chi tiết plot đầu tiên để debug
                {
                    Res.outz("CloudGarden: Plot[0] id=" + plotId + " unlocked=" + unlocked +
                             " stage=" + stage + " crop=" + cropType + " posX=" + posX + " posY=" + posY);
                }

                // Validate tọa độ - bảo vệ khỏi dữ liệu bị lỗi
                if (posX < 0 || posX > 1000 || posY < 0 || posY > 1000)
                {
                    Res.outz("CloudGarden: Invalid pos for plot " + plotId + 
                             " posX=" + posX + " posY=" + posY + " - skipping");
                    continue;
                }

                if (plotId >= 0 && plotId < FarmConstants.MAX_PLOTS && plots[plotId] != null)
                {
                    plots[plotId].UpdateFromServer(unlocked, stage, cropType, timeToHarvest, watered);
                    // Sử dụng vị trí từ server (server quản lý tập trung)
                    plots[plotId].posX = posX;
                    plots[plotId].posY = posY;
                }
            }

            isInitialized = true;
            yAdjustApplied = false; // Reset để tính lại offset
            Res.outz("CloudGarden: Initialized with " + unlockedPlots + " unlocked plots");
        }
        catch (Exception ex)
        {
            Res.outz("CloudGarden: Error updating garden - " + ex.Message);
            // Không set isInitialized = true nếu lỗi để tránh vẽ data sai
        }
    }

    /// <summary>
    /// Dịch chuyển posY tất cả ô đất sao cho đường giữa 2 hàng = mặt đất nhân vật
    /// Đường giữa = trung điểm giữa (đáy hàng trên) và (đỉnh hàng dưới)
    ///           = (minPosY + maxPosY - PLOT_HEIGHT) / 2
    /// </summary>
    private void AdjustPlotsToGround()
    {
        if (yAdjustApplied) return;

        Char myChar = Char.myCharz();
        if (myChar == null) return; // Nhân vật chưa khởi tạo

        int groundY = myChar.cy;
        if (groundY <= 0) return; // Nhân vật chưa có vị trí hợp lệ

        // Tìm min/max posY để xác định 2 hàng
        int minY = int.MaxValue;
        int maxY = int.MinValue;
        int validCount = 0;

        for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
        {
            if (plots[i] != null && plots[i].posY > 0)
            {
                if (plots[i].posY < minY) minY = plots[i].posY;
                if (plots[i].posY > maxY) maxY = plots[i].posY;
                validCount++;
            }
        }

        if (validCount == 0 || minY == int.MaxValue)
        {
            // Nếu không có ô nào có vị trí (có thể do chưa nhận đủ data), 
            // đánh dấu là đã apply để không lặp lại loop này mỗi frame cho đến khi có update mới
            yAdjustApplied = true; 
            return;
        }

        // Đường giữa 2 hàng:
        // - Hàng trên (anchor BOTTOM): đáy ở minY
        // - Hàng dưới (anchor BOTTOM): đỉnh ở maxY - PLOT_HEIGHT
        // - Trung điểm = (minY + maxY - PLOT_HEIGHT) / 2
        int midLine = (minY + maxY - PLOT_HEIGHT) / 2;

        int offset = groundY - midLine;

        // Apply offset vào posY từng ô
        for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
        {
            if (plots[i] != null)
            {
                plots[i].posY += offset;
            }
        }

        yAdjustApplied = true;
        Res.outz("CloudGarden: Adjusted Y offset=" + offset + " groundY=" + groundY + " midLine=" + midLine + " map=" + TileMap.mapID);
    }


    // Vị trí các ô được server gửi về qua UpdateFullGarden

    /// <summary>
    /// Xử lý khi click vào map
    /// </summary>
    public void OnClick(int x, int y)
    {
        FarmPlot p = GetPlotAt(x, y);
        if (p != null)
        {
            // Toggle hoặc set focus
            if (focusedPlotId == p.plotId) focusedPlotId = -1;
            else focusedPlotId = p.plotId;
        }
        else
        {
            focusedPlotId = -1; // Click ra ngoài thì bỏ chọn
        }
    }

    /// <summary>
    /// Lấy ô ruộng tại vị trí click
    /// Tọa độ click (x,y) và posX/posY đều ở hệ logical world-space
    /// Kích thước hit area lấy từ ảnh plot (logical = physical / zoomLevel)
    /// </summary>
    public FarmPlot GetPlotAt(int x, int y)
    {
        // Lấy kích thước LOGICAL từ ảnh plot_empty
        // image.getWidth() = image.w / zoomLevel (logical pixels)
        // image.w = texture physical pixels (đã nhân zoom)
        int plotW = PLOT_WIDTH;
        int plotH = PLOT_HEIGHT;
        Image plotImg = FarmAssetManager.GI().GetPlotEmptyImage();
        if (plotImg != null)
        {
            plotW = plotImg.getWidth();   // logical width = physical / zoomLevel
            plotH = plotImg.getHeight();  // logical height = physical / zoomLevel
        }

        for (int i = 0; i < FarmConstants.MAX_PLOTS; i++)
        {
            if (plots[i] != null && plots[i].ContainsPoint(x, y, plotW, plotH))
            {
                return plots[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Lấy ô theo ID
    /// </summary>
    public FarmPlot GetPlot(int plotId)
    {
        if (plotId >= 0 && plotId < FarmConstants.MAX_PLOTS)
        {
            return plots[plotId];
        }
        return null;
    }

    /// <summary>
    /// Đếm số ô có thể thu hoạch
    /// </summary>
    public int CountHarvestable()
    {
        int count = 0;
        for (int i = 0; i < unlockedPlots; i++)
        {
            if (plots[i].CanHarvest())
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Đếm số ô trống
    /// </summary>
    public int CountEmpty()
    {
        int count = 0;
        for (int i = 0; i < unlockedPlots; i++)
        {
            if (plots[i].IsEmpty())
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Đếm số ô đang trồng
    /// </summary>
    public int CountGrowing()
    {
        int count = 0;
        for (int i = 0; i < unlockedPlots; i++)
        {
            if (plots[i].IsGrowing())
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Lấy thông tin tóm tắt
    /// </summary>
    public string GetSummary()
    {
        return string.Format("Ô mở: {0}/{1} | Trống: {2} | Đang trồng: {3} | Thu hoạch: {4}",
            unlockedPlots, FarmConstants.MAX_PLOTS,
            CountEmpty(), CountGrowing(), CountHarvestable());
    }

    /// <summary>
    /// Vẽ khu vườn
    /// </summary>
    public void Paint(mGraphics g)
    {
        if (!isInitialized || g == null)
        {
            // Log chỉ 1 lần mỗi 5 giây để tránh spam
            if (GameCanvas.gameTick % 300 == 0)
                Res.outz("CloudGarden.Paint SKIP: isInit=" + isInitialized + " g=" + (g != null));
            return;
        }

        // Chỉ tính offset Y khi nhân vật đã có vị trí đúng
        if (!yAdjustApplied && Char.myCharz() != null && Char.myCharz().cy > 0)
        {
            AdjustPlotsToGround();
        }

        try
        {
            // Vẽ các ô đã mở khóa
            for (int i = 0; i < unlockedPlots && i < FarmConstants.MAX_PLOTS; i++)
            {
                if (plots[i] != null)
                    PaintPlot(g, plots[i]);
            }

            // Vẽ các ô chưa mở khóa (với icon khóa)
            for (int i = unlockedPlots; i < FarmConstants.MAX_PLOTS; i++)
            {
                if (plots[i] != null)
                    PaintLockedPlot(g, plots[i]);
            }
            
            // Vẽ hiệu ứng thu hoạch
            if (harvestEffects != null)
            {
                for (int i = 0; i < harvestEffects.Count; i++)
                {
                    if (harvestEffects[i] != null)
                        harvestEffects[i].Paint(g);
                }
            }
        }
        catch (Exception ex)
        {
            Res.outz("CloudGarden.Paint error: " + ex.Message);
        }
    }

    /// <summary>
    /// Vẽ một ô ruộng
    /// </summary>
    private void PaintPlot(mGraphics g, FarmPlot plot)
    {
        // Animation bobbing - nhấp nhô theo timetick chung cho các UI bay
        int yOffset = (GameCanvas.gameTick % 10 > 5) ? 1 : 0;
        
        // Độ cao của các UI bay (Cloud, Harvest icon) so với đáy ô đất
        // PLOT_HEIGHT (48) + 12 = 60px
        int floatingHeight = PLOT_HEIGHT + 12;

        // 1. LUÔN vẽ ô đất trước
        Image plotEmpty = FarmAssetManager.GI().GetPlotEmptyImage();
        if (plotEmpty != null)
        {
            g.drawImage(plotEmpty, plot.posX, plot.posY, mGraphics.BOTTOM | mGraphics.HCENTER);
        }
        else
        {
            // Fallback khi chưa có hình ảnh - vẽ text placeholder để debug
            mFont.tahoma_7b_white.drawString(g, "[" + plot.plotId + "]", 
                plot.posX, plot.posY - PLOT_HEIGHT / 2, mFont.CENTER);
        }

        // 1b. Vẽ highlight nếu ô đang được chọn (dùng ảnh plot_selected.png)
        if (plot.plotId == focusedPlotId)
        {
            Image plotSelected = FarmAssetManager.GI().GetPlotSelectedImage();
            if (plotSelected != null)
            {
                g.drawImage(plotSelected, plot.posX, plot.posY, mGraphics.BOTTOM | mGraphics.HCENTER);
            }
        }

        // 2. Nếu có cây trồng, vẽ cây lên trên ô đất
        if (!plot.IsEmpty())
        {
            // Dùng GetVisualStage() để lấy stage theo thời gian
            sbyte visualStage = plot.GetVisualStage();
            Image cropImg = FarmAssetManager.GI().GetCropAsset(plot.cropType, visualStage);
            if (cropImg != null)
            {
                // Offset Y tùy theo stage: cây trưởng thành lớn hơn nên cần nằm sát đất
                int cropOffsetY = -10;
                if (visualStage >= FarmConstants.STAGE_YOUNG)
                {
                    cropOffsetY = -5; // Seed/Sprout nhỏ, đẩy lên chút
                }
                
                g.drawImage(cropImg, plot.posX, plot.posY + cropOffsetY, mGraphics.BOTTOM | mGraphics.HCENTER);
            }
            else
            {
                if (GameCanvas.gameTick % 300 == 0)
                {
                    Res.outz("Missing crop asset: Type=" + plot.cropType + " VisualStage=" + visualStage + " RealStage=" + plot.currentStage);
                }
            }
        }

        // 3. Vẽ icon thu hoạch hoặc icon héo
        if (plot.IsWithered())
        {
            // Cây bị héo - vẽ chữ "Héo!" nhấp nháy màu đỏ
            if (GameCanvas.gameTick % 20 > 5) // Nhấp nháy
            {
                mFont.tahoma_7b_white.drawString(g, "Héo!", 
                    plot.posX, plot.posY - floatingHeight + yOffset, 
                    mFont.CENTER, mFont.tahoma_7_red);
            }
        }
        else if (plot.CanHarvest())
        {
            Image harvestIcon = FarmAssetManager.GI().GetFarmIcon("harvest");
            if (harvestIcon != null)
            {
                 // Vẽ cùng độ cao với khung mây
                g.drawImage(harvestIcon, plot.posX, plot.posY - floatingHeight + yOffset, 
                           mGraphics.BOTTOM | mGraphics.HCENTER);
            }
        }

        // 4. Vẽ timer nếu đang phát triển HOẶC đang chín muồi (không vẽ khi héo)
        // Chỉ vẽ khi có focus (được bấm vào)
        if (plot.plotId == focusedPlotId && !plot.IsWithered() && (plot.IsGrowing() || (plot.currentStage == FarmConstants.STAGE_MATURE && plot.GetRemainingTime() > 0)))
        {
            // Vẽ khung rau củ (đưa vào đây theo yêu cầu)
            Image frame = FarmAssetManager.GI().GetFarmIcon("khung_raucu");
            int drawY = plot.posY - floatingHeight + yOffset;
            int centerX = plot.posX;
            int frameCenterY = drawY;

            if (frame != null)
            {
                g.drawImage(frame, centerX, drawY, mGraphics.BOTTOM | mGraphics.HCENTER);
                frameCenterY = drawY - (frame.h / 2);
            }

            string timeText = plot.GetTimeDisplay();
            if (!string.IsNullOrEmpty(timeText))
            {
                // Vẽ timer ngay dưới khung mây (hoặc trong khung rau củ)
                // Điều chỉnh Y để nằm trên/trong khung
                mFont.tahoma_7b_white.drawString(g, timeText, 
                    plot.posX, plot.posY - floatingHeight + 5, 
                    mFont.CENTER, mFont.tahoma_7_grey);
            }

             // Vẽ icon thu hoạch - đặt ở tâm khung
            short harvestItemId = FarmConstants.GetHarvestItemId(plot.cropType);
            if (harvestItemId != -1)
            {
                    Image harvestIcon = FarmAssetManager.GI().GetFarmIcon(harvestItemId.ToString());
                    if (harvestIcon != null)
                    {
                        g.drawImage(harvestIcon, centerX, frameCenterY + 23, mGraphics.VCENTER | mGraphics.HCENTER);
                    }
                    else
                    {
                        ItemTemplate temp = ItemTemplates.get(harvestItemId);
                        if (temp != null)
                        {
                            SmallImage.drawSmallImage(g, temp.iconID, centerX, frameCenterY + 23, 0, mGraphics.VCENTER | mGraphics.HCENTER);
                        }
                    }
            }
        }

        // 5. Vẽ arrow down hoặc khung hạt giống nếu ô mở khóa
        if (plot.unlocked)
        {
             // Đã đưa logic vẽ khung rau củ vào mục 4, chỉ giữ lại arrow cho ô trống
            if (plot.IsEmpty())
            {
                // Hiệu ứng nhấp nháy: luân phiên giữa arrow_down_1 và arrow_down_2
                string arrowKey = (GameCanvas.gameTick % 20 > 10) ? "arrow_down_1" : "arrow_down_2";
                Image arrow = FarmAssetManager.GI().GetFarmIcon(arrowKey);
                if (arrow != null)
                {
                    // Vẽ mũi tên chỉ xuống đất
                    g.drawImage(arrow, plot.posX, plot.posY - PLOT_HEIGHT + 30 + yOffset, 
                               mGraphics.BOTTOM | mGraphics.HCENTER);
                }
            }
        }
    }

    /// <summary>
    /// Vẽ ô bị khóa - vẽ ô đất trống + icon khóa nhấp nháy
    /// </summary>
    private void PaintLockedPlot(mGraphics g, FarmPlot plot)
    {
        // 1. Vẽ ô đất trống trước
        Image plotEmpty = FarmAssetManager.GI().GetPlotEmptyImage();
        if (plotEmpty != null)
        {
            g.drawImage(plotEmpty, plot.posX, plot.posY, mGraphics.BOTTOM | mGraphics.HCENTER);
        }

        // 2. Vẽ icon khóa nhấp nháy (luân phiên giữa khoa_0 và khoa_1)
        string lockKey = (GameCanvas.gameTick % 20 > 10) ? "khoa_0" : "khoa_1";
        Image lockedIcon = FarmAssetManager.GI().GetFarmIcon(lockKey);
        if (lockedIcon != null)
        {
            // Animation bobbing - nhấp nhô theo timetick
            int yOffset = (GameCanvas.gameTick % 10 > 5) ? 1 : 0;
            // Vẽ icon khóa ở giữa ô đất
            g.drawImage(lockedIcon, plot.posX, plot.posY - PLOT_HEIGHT/2 + yOffset, 
                       mGraphics.VCENTER | mGraphics.HCENTER);
        }
        else
        {
            // Fallback: thử lấy icon "locked" chung
            lockedIcon = FarmAssetManager.GI().GetFarmIcon("locked");
            if (lockedIcon != null)
            {
                g.drawImage(lockedIcon, plot.posX, plot.posY, mGraphics.BOTTOM | mGraphics.HCENTER);
            }
            else
            {
                // Vẽ placeholder nếu không có icon
                mFont.tahoma_7b_white.drawString(g, "🔒", plot.posX, plot.posY - PLOT_HEIGHT/2, mFont.CENTER);
            }
        }
    }

    /// <summary>
    /// Update logic (gọi mỗi frame)
    /// </summary>
    public void Update()
    {
        if (!isInitialized) return;

        try
        {
            // Kiểm tra các ô có hết thời gian chưa
            for (int i = 0; i < unlockedPlots && i < FarmConstants.MAX_PLOTS; i++)
            {
                if (plots[i] != null && plots[i].IsGrowing() && plots[i].GetRemainingTime() <= 0)
                {
                    // Client-side auto-maturation:
                    // Nếu hết giờ mà chưa có update server, tự chuyển sang MATURE để hiện icon thu hoạch
                    plots[i].currentStage = FarmConstants.STAGE_MATURE;
                }
            }

            // Update harvest effects
            if (harvestEffects != null)
            {
                for (int i = harvestEffects.Count - 1; i >= 0; i--)
                {
                    if (harvestEffects[i] != null && harvestEffects[i].Update())
                    {
                        harvestEffects.RemoveAt(i);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Res.outz("CloudGarden.Update error: " + ex.Message);
        }
    }

    // ===================== HARVEST EFFECTS =====================
    private List<HarvestEffect> harvestEffects = new List<HarvestEffect>();

    public void ShowHarvestEffect(int plotId, sbyte cropType, int quantity)
    {
        FarmPlot plot = GetPlot(plotId);
        if (plot != null)
        {
            // Center of the plot
            int x = plot.posX;
            int y = plot.posY - PLOT_HEIGHT / 2 - 30;
            harvestEffects.Add(new HarvestEffect(x, y, cropType, quantity));
        }
    }

    private class HarvestEffect
    {
        public int x, y;
        public int startY;
        public sbyte cropType;
        public int quantity;
        public int timer;

        public HarvestEffect(int x, int y, sbyte cropType, int quantity)
        {
            this.x = x;
            this.y = y;
            this.startY = y;
            this.cropType = cropType;
            this.quantity = quantity;
            this.timer = 0;
        }

        /// <summary>
        /// Returns true if finished
        /// </summary>
        public bool Update()
        {
            timer++;
            // Fly up speed
            if (timer < 20) y -= 2;
            else if (timer < 40) y -= 1;
            
            return timer > 60;
        }

        public void Paint(mGraphics g)
        {
             // Draw Harvest Item Icon
             short harvestItemId = FarmConstants.GetHarvestItemId(cropType);
             if (harvestItemId != -1)
             {
                 // Try to load item template to get icon ID
                 ItemTemplate temp = ItemTemplates.get(harvestItemId);
                 if (temp != null)
                 {
                      SmallImage.drawSmallImage(g, temp.iconID, x, y, 0, mGraphics.VCENTER | mGraphics.HCENTER);
                 }
                 else
                 {
                     // Fallback to crop asset if template not found
                      Image icon = FarmAssetManager.GI().GetCropAsset(cropType, FarmConstants.STAGE_MATURE);
                      if (icon != null)
                         g.drawImage(icon, x, y, mGraphics.VCENTER | mGraphics.HCENTER);
                 }
             }

            // Draw Quantity Text with Outline
            // +quantity
            mFont.tahoma_7b_yellow.drawString(g, "+" + quantity, x, y - 25, mFont.CENTER);
        }
    }
}
