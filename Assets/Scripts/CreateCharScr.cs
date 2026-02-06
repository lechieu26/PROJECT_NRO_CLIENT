using System;
using Assets.src.g;
using UnityEngine;

public class CreateCharScr : mScreen, IActionListener
{
    public static CreateCharScr instance;
    public static bool isCreateChar = false;

    // Assets
    private Image imgBanner;
    private Image imgBack;
    private Image[] imgPlanet = new Image[3];
    private Image[] imgClass = new Image[3];
    private Image imgPlanetFocus;
    private Image imgClassFocus;
    private Image imgBtnCreate, imgBtnCreateFocus;
    private Image imgInputName, imgInputIcon;
    private Image imgBackground;
    private Image imgChungTocBg;

    // State
    public static TField tAddName;
    public static int indexGender = 0;
    public static int indexHair = 0;

    // Constants from Original CreateCharScr
    // hairID - dùng để vẽ nhân vật preview
    public static int[][] hairID = new int[3][]
    {
        new int[3] { 64, 30, 31 },
        new int[3] { 9, 29, 32 },
        new int[3] { 6, 27, 28 }
    };
    // hairIDOld - dùng khi tạo nhân vật (gửi lên server)
    public static int[][] hairIDOld = new int[3][]
    {
        new int[3] { 64, 30, 31 },
        new int[3] { 9, 29, 32 },
        new int[3] { 6, 27, 28 }
    };
    public static int[] defaultLeg = new int[3] { 2, 11, 17 };
    public static int[] defaultBody = new int[3] { 1, 10, 16 };

    // Planet names
    private string[] planetNames = new string[] { "Trái đất", "Namếc", "Saiyan" };

    private int cf; // Frame animation for char
    private bool layoutCalculated = false;
    private bool isCreating = false; // Chặn double click

    // Layout info structure - lưu vị trí các thành phần UI
    private struct LayoutInfo
    {
        // Back button
        public int backX, backY, backW, backH;
        // Banner
        public int bannerH, titleX, titleY;
        // Chung toc background
        public int chungTocBgX, chungTocBgY, chungTocBgW, chungTocBgH;
        // Planets (3 items)
        public int planetX, planetW, planetH, planetStartY, planetGap;
        // Classes (3 items)
        public int classStartX, classY, classW, classH, classGap;
        // Input field
        public int inputX, inputY, inputW, inputH;
        // Create button
        public int btnX, btnY, btnW, btnH;
        // Character preview area (giữa màn hình)
        public int charPreviewX, charPreviewY, charPreviewW, charPreviewH;
    }
    private LayoutInfo layout;

    public CreateCharScr()
    {
        loadAssets();
        
        // Setup text field
        tAddName = new TField();
        tAddName.name = "";
        tAddName.isFocus = true;
        // Chỉ cho phép nhập chữ và số
        tAddName.setIputType(TField.INPUT_ALPHA_NUMBER_ONLY);
        tAddName.paintFocus = false; // Disable green focus box
        
        // Setup initial focus
        indexGender = 0;
        indexHair = 0;
    }

    public static CreateCharScr gI()
    {
        if (instance == null)
        {
            instance = new CreateCharScr();
        }
        return instance;
    }

    private void loadAssets()
    {
        try
        {
            string pathRoot = "/mainimage/";
            
            imgBanner = GameCanvas.loadImage(pathRoot + "TOP_BG_Title.png");
            imgBack = GameCanvas.loadImage(pathRoot + "TOP_BtBack.png");
            imgPlanetFocus = GameCanvas.loadImage(pathRoot + "Chungtoc_Focus.png");
            imgClassFocus = GameCanvas.loadImage(pathRoot + "Class_Focus.png");
            imgBtnCreate = GameCanvas.loadImage(pathRoot + "Button_Main_3.png");
            imgBtnCreateFocus = GameCanvas.loadImage(pathRoot + "Button_Main_4.png");
            imgInputName = GameCanvas.loadImage(pathRoot + "Name_tfx.png");
            imgInputIcon = GameCanvas.loadImage(pathRoot + "Name_edit.png");
            imgBackground = GameCanvas.loadImage(pathRoot + "background.png"); 
            imgChungTocBg = GameCanvas.loadImage(pathRoot + "Chungtoc_BG.png");
            for (int i = 0; i < 3; i++)
            {
                imgPlanet[i] = GameCanvas.loadImage(pathRoot + "Chungtoc_" + i + ".png");
            }
            loadClassImages();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading assets CreateCharScr: " + ex.Message);
        }
    }

    private void loadClassImages()
    {
        string pathRoot = "/mainimage/";
        try {
            for (int i = 0; i < 3; i++)
            {
                imgClass[i] = GameCanvas.loadImage(pathRoot + "Class_" + indexGender + "_" + i + ".png");
            }
        } catch (Exception) { }
    }

    public override void switchToMe()
    {
        GameCanvas.menu.showMenu = false;
        GameCanvas.endDlg();
        base.switchToMe();
        if(tAddName != null) 
        {
            tAddName.setText("");
            // Trên PC, tự động focus vào ô nhập tên
            if (Main.isPC)
            {
                tAddName.isFocus = true;
            }
        }
        indexGender = 0;
        indexHair = 0;
        isCreating = false; // Reset cờ tạo nhân vật
        loadClassImages();
        layoutCalculated = false; // Force recalculate
    }

    // Xử lý nhập bàn phím trên PC
    public override void keyPress(int keyCode)
    {
        if (tAddName != null)
        {
            // Chỉ cho phép nhập chữ, chặn số (0-9)
            // TField.INPUT_ALPHA_NUMBER_ONLY đã chặn ký tự đặc biệt, giờ chặn thêm số
            if (keyCode >= 48 && keyCode <= 57)
            {
                return;
            }
            tAddName.keyPressed(keyCode);
        }
    }

    // Get image width safely
    private int getImgW(Image img, int defaultVal)
    {
        if (img == null) return defaultVal;
        return img.getWidth();
    }

    // Get image height safely
    private int getImgH(Image img, int defaultVal)
    {
        if (img == null) return defaultVal;
        return img.getHeight();
    }

    // Tính toán layout dựa trên kích thước màn hình và ảnh thực tế
    private void calculateLayout()
    {
        // Back button - góc trên trái, trên banner
        layout.backW = getImgW(imgBack, 40);
        layout.backH = getImgH(imgBack, 30);
        layout.backX = 0;
        layout.backY = 0;

        // Banner
        layout.bannerH = getImgH(imgBanner, 30);
        layout.titleX = layout.backX + layout.backW + 10;
        layout.titleY = layout.bannerH / 2;

        // Planets - bên trái, xếp dọc bắt đầu từ dưới banner
        layout.planetW = getImgW(imgPlanet[0], 60);
        layout.planetH = getImgH(imgPlanet[0], 60);
        layout.planetX = 10;
        layout.planetStartY = layout.bannerH + 10;
        layout.planetGap = 5;

        // Classes - bên phải, xếp ngang
        layout.classW = getImgW(imgClass[0], 140);
        layout.classH = getImgH(imgClass[0], 200);
        layout.classGap = 10;
        int totalClassWidth = 3 * layout.classW + 2 * layout.classGap;
        layout.classStartX = GameCanvas.w - totalClassWidth - 20;
        // Căn giữa theo chiều dọc trong vùng còn lại (dưới banner)
        layout.classY = layout.bannerH + (GameCanvas.h - layout.bannerH - layout.classH) / 2;

        // Character preview area - ở giữa màn hình (giữa planets và class cards)
        int leftAreaRight = layout.planetX + layout.planetW + 80; // Sau tên hành tinh
        layout.charPreviewW = 100;
        layout.charPreviewH = 100;
        layout.charPreviewX = leftAreaRight + (layout.classStartX - leftAreaRight - layout.charPreviewW) / 2;
        layout.charPreviewY = layout.bannerH + 20; // Bắt đầu từ sau banner

        // Input field - NGAY DƯỚI CHARACTER PREVIEW
        layout.inputW = getImgW(imgInputName, 150);
        layout.inputH = getImgH(imgInputName, 30);
        // Căn giữa theo horizontal với character preview
        layout.inputX = layout.charPreviewX + (layout.charPreviewW - layout.inputW) / 2;
        layout.inputY = layout.charPreviewY + layout.charPreviewH + 60; // 60px dưới char preview (để có chỗ cho nhân vật)

        // Create button - ngay dưới input field
        layout.btnW = getImgW(imgBtnCreate, 80);
        layout.btnH = getImgH(imgBtnCreate, 30);
        layout.btnX = layout.inputX + (layout.inputW - layout.btnW) / 2;
        layout.btnY = layout.inputY + layout.inputH + 5;
    }

    public override void update()
    {

        
        // Calculate layout on first update when screen dimensions are ready
        if (!layoutCalculated && GameCanvas.w > 0 && GameCanvas.h > 0)
        {
            calculateLayout();
            layoutCalculated = true;
        }

        // Animation frame
        if (GameCanvas.gameTick % 10 > 5) cf = 0; else cf = 1;

        if(tAddName != null) tAddName.update();

        // *** QUAN TRỌNG: Xử lý pointer TRƯỚC khi clearKeyPressed() vì clear sẽ reset isPointerJustRelease ***
        if (GameCanvas.isPointerJustRelease)
        {
            int xP = GameCanvas.px;
            int yP = GameCanvas.py;

            // Check Back Button (top left corner) - mở rộng vùng click
            int backRight = layout.backX + layout.backW + 10;
            int backBottom = layout.backY + layout.backH + 5;
            if (xP >= layout.backX && xP <= backRight && 
                yP >= layout.backY && yP <= backBottom)
            {
                doExit();
                return;
            }

            // Check Planet Selection (Left, vertical) - mở rộng vùng click bao gồm cả tên hành tinh
            for (int i = 0; i < 3; i++)
            {
                // Cập nhật tọa độ theo logic vẽ mới (offset +50, +20)
                int pX = layout.planetX + 50;
                int pY = layout.planetStartY + i * (layout.planetH + layout.planetGap) + 20;
                
                int clickWidth = layout.planetW + 60; // Vùng click rộng hơn để cover cả text
                int pRight = pX + clickWidth;
                // pBottom cũng cần +20 như pY
                int pBottom = pY + layout.planetH; 
                
                if (xP >= pX && xP <= pRight && 
                    yP >= pY && yP <= pBottom)
                {
                    if (indexGender != i)
                    {
                        indexGender = i;
                        indexHair = 0; 
                        loadClassImages();
                        calculateLayout();
                    }
                    return;
                }
            }

            // Check Class Selection (Right, horizontal)
            for (int i = 0; i < 3; i++)
            {
                int cX = layout.classStartX + i * (layout.classW + layout.classGap);
                int cRight = cX + layout.classW;
                int cBottom = layout.classY + layout.classH;
                if (xP >= cX && xP <= cRight && 
                    yP >= layout.classY && yP <= cBottom)
                {
                    indexHair = i;
                    return;
                }
            }

            // Check Create Button (below input)
            int btnRight = layout.btnX + layout.btnW;
            int btnBottom = layout.btnY + layout.btnH;
            if (xP >= layout.btnX && xP <= btnRight && 
                yP >= layout.btnY && yP <= btnBottom)
            {
                doCreateChar();
                return;
            }

            // Check Input Focus (center)
            int inputRight = layout.inputX + layout.inputW;
            int inputBottom = layout.inputY + layout.inputH;
            if (xP >= layout.inputX && xP <= inputRight && 
                yP >= layout.inputY && yP <= inputBottom)
            {
                tAddName.isFocus = true;
                tAddName.setFocusWithKb(true);
                return;
            }
            
            
            // Nếu click ra ngoài input field -> unfocus
            tAddName.isFocus = false;
        }
        
        // Clear key states SAU khi đã xử lý pointer
        GameCanvas.clearKeyHold();
        GameCanvas.clearKeyPressed();
    }

    public override void paint(mGraphics g)
    {
        // Đảm bảo layout đã được tính
        if (!layoutCalculated && GameCanvas.w > 0 && GameCanvas.h > 0)
        {
            calculateLayout();
            layoutCalculated = true;
        }

        // Background
        if (imgBackground != null) 
            g.drawImage(imgBackground, 0, 0, 0);
        else 
            GameCanvas.paintBGGameScr(g);

        // Draw Banner at top
        if (imgBanner != null) 
        {
            g.drawImage(imgBanner, 0, 0, 0);
        }

        // Draw Back Button at top left corner
        if (imgBack != null) g.drawImage(imgBack, layout.backX, layout.backY, 0);

        // Draw title text "Tạo Nhân Vật" on banner, after back button
        mFont.tahoma_7b_white.drawString(g, "Tạo Nhân Vật", layout.titleX, layout.titleY - 5, 0);

			// Vẽ background cho hành tinh (nếu có)
            if (imgChungTocBg != null)
            {
                // Căn giữa background với icon hành tinh (tâm trùng nhau)
                int bgX = layout.planetX + (layout.planetW - layout.chungTocBgW) / 2;
                int bgY = layout.planetStartY; 
                g.drawImage(imgChungTocBg, bgX+15, bgY, 0);
            }

        // Draw Planets (Left side, vertical stack)
        for (int i = 0; i < 3; i++)
        {
            int pY = layout.planetStartY + i * (layout.planetH + layout.planetGap);
            
            // Draw focus highlight if selected
            if (i == indexGender && imgPlanetFocus != null)
            {
                int focusW = getImgW(imgPlanetFocus, layout.planetW);
                int focusH = getImgH(imgPlanetFocus, layout.planetH);
                g.drawImage(imgPlanetFocus, layout.planetX - (focusW - layout.planetW) / 2 + 50, pY - (focusH - layout.planetH) / 2 + 20, 0);
            }
            
            // Draw planet icon
            if (imgPlanet[i] != null)
                g.drawImage(imgPlanet[i], layout.planetX + 50, pY + 20, 0);
            
            // Draw planet name to the right of icon
            mFont.tahoma_7_white.drawString(g, planetNames[i], layout.planetX + layout.planetW + 57, pY + layout.planetH / 2 + 15, 0);
        }

        // Draw Name Input near center
        if (imgInputName != null)
        {
             g.drawImage(imgInputName, layout.inputX, layout.inputY, 0);
        }
        if (imgInputIcon != null)
        {
            int iconH = getImgH(imgInputIcon, 20);
            g.drawImage(imgInputIcon, layout.inputX + 5, layout.inputY + layout.inputH / 2 - iconH / 2, 0);
        }
        
        // Draw text directly in input frame
        if(tAddName != null) {
            tAddName.x = layout.inputX + 25;
            tAddName.y = layout.inputY + 5;
            tAddName.width = layout.inputW - 30;
            tAddName.height = layout.inputH - 10;
            
            string text = tAddName.getText();
            int textX = layout.inputX + 25;
            int textY = layout.inputY + layout.inputH / 2 - 5;
            
            if (string.IsNullOrEmpty(text))
            {
                // Placeholder text khi chưa nhập
                if (!tAddName.isFocus)
                {
                    mFont.tahoma_7_grey.drawString(g, "Nhập tên nhân vật", textX, textY, 0);
                }
            }
            else
            {
                mFont.tahoma_7_white.drawString(g, text, textX, textY, 0);
            }
            // Vẽ cursor khi đang focus (nhấp nháy mỗi 500ms)
            if (tAddName.isFocus)
            {
                bool showCursor = (GameCanvas.gameTick % 30) < 15; // Nhấp nháy
                if (showCursor)
                {
                    // Tính vị trí cursor ở cuối text
                    int textWidth = 0;
                    if (!string.IsNullOrEmpty(text))
                    {
                        textWidth = mFont.tahoma_7_white.getWidth(text);
                    }
                    // Cursor sát ngay sau text (bỏ +2)
                    int cursorX = textX + textWidth;
                    int cursorY = textY - 2;
                    int cursorHeight = mFont.tahoma_7_white.getHeight() + 2;
                    
                    // Vẽ hình chữ nhật mỏng làm cursor (2px width)
                    g.setColor(0xFFFFFF); // Màu trắng
                    g.fillRect(cursorX, cursorY, 2, cursorHeight);
                }
            }
        }

        // Draw Create Button BELOW input
        Image btnImg = (GameCanvas.isPointerHoldIn(layout.btnX, layout.btnY, layout.btnW, layout.btnH) && GameCanvas.isPointerDown) ? imgBtnCreateFocus : imgBtnCreate;
        if (btnImg != null)
        {
            g.drawImage(btnImg, layout.btnX, layout.btnY, 0); 
            mFont.tahoma_7b_white.drawString(g, "Tạo mới", layout.btnX + layout.btnW / 2, layout.btnY + layout.btnH / 2 - 5, mFont.CENTER);
        }

        // Draw Class Cards (Right side, horizontal row)
        for (int i = 0; i < 3; i++)
        {
            int cX = layout.classStartX + i * (layout.classW + layout.classGap);
            
            // Draw focus highlight if selected
            if (i == indexHair && imgClassFocus != null)
            {
                int focusW = getImgW(imgClassFocus, layout.classW);
                int focusH = getImgH(imgClassFocus, layout.classH);
                g.drawImage(imgClassFocus, cX - (focusW - layout.classW) / 2, layout.classY - (focusH - layout.classH) / 2, 0);
            }
            
            // Draw class card
            if (imgClass[i] != null)
            {
                g.drawImage(imgClass[i], cX, layout.classY, 0);
            }
        }

        // Draw Character Preview (giữa màn hình - vùng hình chữ nhật đỏ trong design)
        // Logic giống hệt CreateCharOld.cs
        try
        {
            // Lấy các part dựa trên lựa chọn hiện tại - sử dụng hairID cho vẽ (giống code gốc)
            int headId = hairID[indexGender][indexHair]; // Dùng hairID để vẽ, hairIDOld khi tạo char
            int legId = defaultLeg[indexGender];
            int bodyId = defaultBody[indexGender];



            // Vị trí vẽ nhân vật (căn giữa trong preview area, gần với input field)
            int charX = layout.charPreviewX + layout.charPreviewW / 2;
            int charY = layout.inputY - 60; // Đặt nhân vật ngay trên input field, cách 60px

            // Vẽ bóng
            if (TileMap.bong != null)
            {
                g.drawImage(TileMap.bong, charX, charY + 50, 3);
            }

            // Vẽ nhân vật - giống hệt logic trong CreateCharOld.cs
            Part partHead = GameScr.parts[headId];
            Part partLeg = GameScr.parts[legId];
            Part partBody = GameScr.parts[bodyId];

            // Vẽ head
            if (partHead != null && partHead.pi != null)
            {
                SmallImage.drawSmallImage(g, partHead.pi[Char.CharInfo[cf][0][0]].id, 
                    charX + Char.CharInfo[cf][0][1] + partHead.pi[Char.CharInfo[cf][0][0]].dx, 
                    charY - Char.CharInfo[cf][0][2] + partHead.pi[Char.CharInfo[cf][0][0]].dy + 45, 0, 0);
            }
            
            // Vẽ leg
            if (partLeg != null && partLeg.pi != null)
            {
                SmallImage.drawSmallImage(g, partLeg.pi[Char.CharInfo[cf][1][0]].id, 
                    charX + Char.CharInfo[cf][1][1] + partLeg.pi[Char.CharInfo[cf][1][0]].dx, 
                    charY - Char.CharInfo[cf][1][2] + partLeg.pi[Char.CharInfo[cf][1][0]].dy + 45, 0, 0);
            }
            
            // Vẽ body
            if (partBody != null && partBody.pi != null)
            {
                SmallImage.drawSmallImage(g, partBody.pi[Char.CharInfo[cf][2][0]].id, 
                    charX + Char.CharInfo[cf][2][1] + partBody.pi[Char.CharInfo[cf][2][0]].dx, 
                    charY - Char.CharInfo[cf][2][2] + partBody.pi[Char.CharInfo[cf][2][0]].dy + 45, 0, 0);
            }
        }
        catch (Exception ex)
        {
            // Log lỗi nếu có
            // if (GameCanvas.gameTick % 120 == 0) Debug.Log($"[CreateCharScr] Error drawing character: {ex.Message}");
        }
    }

    private void doCreateChar()
    {
        if (isCreating) return; // Đang tạo thì bỏ qua

        if (tAddName.getText().Trim().Length < 5)
        {
            GameCanvas.startOKDlg("Tên nhân vật quá ngắn");
            return;
        }

        isCreating = true; // Đánh dấu đang tạo
        string name = tAddName.getText();
        // Quay lại dùng hairIDOld để gửi lên server
        int hair = hairIDOld[indexGender][indexHair];
        Service.gI().createChar(name, indexGender, hair);
        
        // Sau 1 khoảng thời gian reset isCreating? 
        // Thường thì sau khi gửi message sẽ có response chuyển màn hình hoặc báo lỗi.
        // Nếu server không phản hồi thì sẽ bị kẹt -> Cần mechanism unlock hoặc để người dùng bấm Back.
    }

    private void doExit()
    {
         GameCanvas.instance.doResetToLoginScr(GameCanvas.serverScreen);
    }

    public void perform(int idAction, object p)
    {
        // Handle dialog actions if any
    }
}
