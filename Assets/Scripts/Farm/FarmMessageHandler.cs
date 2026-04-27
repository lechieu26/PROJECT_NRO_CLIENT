using System;

/// <summary>
/// Xử lý message farm từ Server
/// Gọi các method này trong Controller.onMessage()
/// </summary>
public class FarmMessageHandler
{
    private static FarmMessageHandler instance;

    public static FarmMessageHandler GI()
    {
        if (instance == null)
        {
            instance = new FarmMessageHandler();
        }
        return instance;
    }

    /// <summary>
    /// Xử lý message -33 (Farm Assets)
    /// </summary>
    public void HandleFarmAssetMessage(Message msg)
    {
        try
        {
            sbyte subType = msg.reader().readByte();

            switch (subType)
            {
                case FarmConstants.SUBTYPE_FARM_ASSET: // 10
                    ReadFarmAsset(msg);
                    break;

                case FarmConstants.SUBTYPE_CROP_ASSET: // 11
                    ReadCropAsset(msg);
                    break;

                case FarmConstants.SUBTYPE_FARM_ICON: // 12
                    ReadFarmIcon(msg);
                    break;

                case FarmConstants.SUBTYPE_CROP_TEMPLATE: // 13
                    ReadCropTemplate(msg);
                    break;

                default:
                    Res.outz("FarmMessageHandler: Unknown sub-type " + subType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Res.outz("FarmMessageHandler: Error handling asset message - " + ex.Message);
        }
    }

    /// <summary>
    /// Xử lý message -34 (Farm Data)
    /// </summary>
    public void HandleFarmDataMessage(Message msg)
    {
        try
        {
            sbyte subType = msg.reader().readByte();

            if (subType == FarmConstants.SUBTYPE_PLOT_UPDATE) // 10
            {
                ProcessFarmData(msg);
            }
        }
        catch (Exception ex)
        {
            Res.outz("FarmMessageHandler: Error handling data message - " + ex.Message);
        }
    }

    /// <summary>
    /// Xử lý message -34 khi subType (10) đã được đọc ở Controller
    /// Gọi từ Controller.cs khi b12 == 10
    /// </summary>
    public void HandleFarmDataDirect(Message msg)
    {
        try
        {
            ProcessFarmData(msg);
        }
        catch (Exception ex)
        {
            Res.outz("FarmMessageHandler: Error handling farm data direct - " + ex.Message);
        }
    }

    /// <summary>
    /// Logic xử lý farm data chung (đọc dataType và dispatch)
    /// </summary>
    private void ProcessFarmData(Message msg)
    {
        int available = msg.reader().available();
        sbyte dataType = msg.reader().readByte();
        Res.outz("FarmMessageHandler: ProcessFarmData dataType=" + dataType + " available=" + available);

        switch (dataType)
        {
            case FarmConstants.DATA_UPDATE_SINGLE: // 0
                ReadSinglePlotUpdate(msg);
                break;

            case FarmConstants.DATA_UPDATE_FULL: // 1
                ReadFullGardenUpdate(msg);
                break;

            case FarmConstants.DATA_OPEN_SEED_PANEL: // 2
                int plotId = msg.reader().readInt();
                // Mở panel chọn hạt giống, Client tự lọc từ inventory
                GameCanvas.panel.setTypeFarmSeed(plotId);
                GameCanvas.panel.show();
                Res.outz("FarmMessageHandler: Open seed panel for plot " + plotId);
                break;

            case 3: // DATA_CLOSE_DIALOG - Server yêu cầu đóng dialog
                CloseCurrentDialog();
                Res.outz("FarmMessageHandler: Server requested close dialog");
                break;

            case FarmConstants.DATA_HARVEST_SUCCESS: // 4 - Hiệu ứng thu hoạch
                int hPlotId = msg.reader().readInt();
                sbyte hCropType = msg.reader().readByte();
                int hQuantity = msg.reader().readInt();
                CloudGarden.GI().ShowHarvestEffect(hPlotId, hCropType, hQuantity);
                break;

            default:
                Res.outz("FarmMessageHandler: Unknown data type " + dataType);
                break;
        }
    }

    /// <summary>
    /// Đọc farm asset chung
    /// </summary>
    private void ReadFarmAsset(Message msg)
    {
        short assetId = msg.reader().readShort();
        int length = msg.reader().readInt();
        sbyte[] imageData = new sbyte[length];
        msg.reader().read(ref imageData, 0, length);

        if (assetId == 2000) // ICON_PLOT_EMPTY
        {
            FarmAssetManager.GI().SavePlotEmptyImage(imageData);
        }
        else if (assetId == 2001) // ICON_PLOT_SELECTED
        {
            FarmAssetManager.GI().SavePlotSelectedImage(imageData);
        }
        Res.outz("FarmMessageHandler: Loaded farm asset ID=" + assetId);
    }

    /// <summary>
    /// Đọc crop stage asset
    /// </summary>
    private void ReadCropAsset(Message msg)
    {
        sbyte cropType = msg.reader().readByte();
        sbyte stage = msg.reader().readByte();
        int length = msg.reader().readInt();
        sbyte[] imageData = new sbyte[length];
        msg.reader().read(ref imageData, 0, length);

        FarmAssetManager.GI().SaveCropAsset(cropType, stage, imageData);
        Res.outz("FarmMessageHandler: Loaded crop asset type=" + cropType + " stage=" + stage);
    }

    /// <summary>
    /// Đọc farm icon
    /// </summary>
    private void ReadFarmIcon(Message msg)
    {
        short iconId = msg.reader().readShort();
        string iconName = msg.reader().readUTF();
        int length = msg.reader().readInt();
        sbyte[] imageData = new sbyte[length];
        msg.reader().read(ref imageData, 0, length);

        FarmAssetManager.GI().SaveFarmIcon(iconName, imageData);
        Res.outz("FarmMessageHandler: Loaded farm icon " + iconName);
    }

    /// <summary>
    /// Đọc crop template
    /// </summary>
    private void ReadCropTemplate(Message msg)
    {
        sbyte count = msg.reader().readByte();
        for (int i = 0; i < count; i++)
        {
            FarmConstants.CropTemplateInfo info = new FarmConstants.CropTemplateInfo();
            info.id = msg.reader().readByte();
            info.name = msg.reader().readUTF();
            info.seedItemId = msg.reader().readShort();
            info.harvestItemId = msg.reader().readShort();
            info.imgYoung = msg.reader().readUTF();
            info.imgMature = msg.reader().readUTF();
            info.imgWithered = msg.reader().readUTF();
            FarmConstants.AddCropTemplate(info);
        }
        Res.outz("FarmMessageHandler: Loaded " + count + " crop templates");
    }

    /// <summary>
    /// Đọc update cho một ô
    /// </summary>
    private void ReadSinglePlotUpdate(Message msg)
    {
        int plotId = msg.reader().readInt();
        sbyte stage = msg.reader().readByte();
        sbyte cropType = msg.reader().readByte();
        int timeToHarvest = msg.reader().readInt();
        bool watered = msg.reader().readBool();

        CloudGarden.GI().UpdateSinglePlot(plotId, true, stage, cropType, timeToHarvest, watered);
        Res.outz("FarmMessageHandler: Updated plot " + plotId);
        Res.outz("chieu.lq: [Client] Received UpdateSinglePlot: plotId=" + plotId + " stage=" + stage + " time=" + timeToHarvest);
        
        // Chỉ đóng menu và ChatPopup sau khi thu hoạch (stage = EMPTY)
        // Không đóng khi gieo hạt vì panel seed tự xử lý
        if (stage == FarmConstants.STAGE_EMPTY)
        {
            CloseCurrentDialog();
        }
    }

    /// <summary>
    /// Đọc update toàn bộ garden
    /// </summary>
    private void ReadFullGardenUpdate(Message msg)
    {
        CloudGarden.GI().UpdateFullGarden(msg);
        Res.outz("FarmMessageHandler: Full garden updated");
        Res.outz("chieu.lq: [Client] Received FullGardenUpdate");
        // Không đóng dialog ở đây - sẽ gây đóng menu khi vào map
        // Server sẽ gửi DATA_CLOSE_DIALOG (3) khi cần đóng dialog
    }

    /// <summary>
    /// Đóng menu và ChatPopup hiện tại
    /// Gọi sau khi thu hoạch hoặc gieo hạt thành công
    /// </summary>
    private void CloseCurrentDialog()
    {
        try
        {
            // Đóng menu
            if (GameCanvas.menu != null)
            {
                GameCanvas.menu.showMenu = false;
            }
            
            // Đóng ChatPopup an toàn
            ChatPopup.currChatPopup = null;
            ChatPopup.serverChatPopUp = null;
            Char.chatPopup = null;
            
            // Ẩn InfoDlg và reset dialog state
            InfoDlg.hide();
            GameCanvas.endDlg();
            
            Res.outz("FarmMessageHandler: Closed current dialog safely");
        }
        catch (Exception ex)
        {
            Res.outz("FarmMessageHandler: Error closing dialog - " + ex.Message);
        }
    }
}
