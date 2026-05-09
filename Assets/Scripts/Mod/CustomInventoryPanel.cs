using Assets.src.g;
using System;
using UnityEngine;

public class CustomInventoryPanel
{
    public static bool isShow;
    public static bool suppressFlagUI;
    public static Image closeImg = GameCanvas.loadImage("/mainImage/myTexture2der.png");

    private const int DEBUG_LAYER = 2;
    private const int SELECT_BG = 0xDDEFCB;
    private const int SELECT_BORDER = 0x79A96B;

    private static readonly string[] TOP_TABS = new string[]
    {
        "Nhiệm Vụ", "Nhân Vật", "Đệ Tử", "Kĩ Năng", "Hành Trang", "Bang Hội", "Mod", "Chức Năng"
    };

    // Bản 1: taskId < 12 – chưa mở Bang Hội (tương tự mainTab1)
    private static readonly int[] VISIBLE_TABS_1 = new int[] { 0, 1, 2, 3, 4, 6, 7 };
    // Bản 2: taskId >= 12 – đã mở Bang Hội (tương tự mainTab2)
    private static readonly int[] VISIBLE_TABS_2 = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };

    private static int[] GetVisibleTabs()
    {
        Char me = Char.myCharz();
        if (me != null && me.taskMaint != null && me.taskMaint.taskId >= 12)
        {
            return VISIBLE_TABS_2;
        }
        return VISIBLE_TABS_1;
    }

    private static int panelX;
    private static int panelY;
    private static int panelW;
    private static int panelH;
    private static int bagScrollY;
    private static int bagScrollTargetY;
    private static int bagScrollRun;
    private static int bagScrollCmdy;
    private static int bagScrollCmvy;
    private static int skillScrollY;
    private static int skillScrollTargetY;
    private static int skillScrollRun;
    private static int skillScrollCmdy;
    private static int skillScrollCmvy;
    private static int lastSkillDragY;
    private static int skillDragStartY;
    private static int lastDragY;
    private static int firstDragY;
    private static int firstDragX;
    private static int dragTime;
    private static readonly int[] dragLastY = new int[3];
    private static bool draggingBag;
    private static int bagScrollYBeforeDrag;
    private static bool downWhenRunning;
    private static bool draggingSkill;
    private static bool skillDragged;
    private static bool globalDragged;
    private static int clanMsgScrollY;
    private static int clanLogicScrollY;
    private static int selectedClanMsgIndex = -1;
    private static int selectedClanLogicIndex = -1;
    private static int selectedClanMenuIndex;
    private static int selectedToolGroupIndex;
    private static int selectedToolAction = -1;
    private static int selectedToolDetailIndex = -1;
    private static int toolDetailScrollY;
    private static int toolDetailScrollTargetY;
    private static int toolDetailScrollRun;
    private static bool toolDetailDragged;
    private static int toolDetailDragStartY;
    private static int toolDetailScrollYBeforeDrag;
    private static GameInfo selectedGameInfoPopup;
    private static int topTab = 4;
    private static int rightSubTab;
    private static int petSubTab;
    private static int selectedBoxIndex = -1;
    private static int selectedAutoIndex = -1;
    private static int selectedSkillIndex = -1;
    private static int selectedBagIndex = -1;
    private static int selectedBodyIndex = -1;
    private static Item selectedItemInfo;
    private static int selectedItemX;
    private static int selectedItemY;
    private static readonly string[] RIGHT_TABS = new string[] { "Hành Trang", "Rương Đồ", "Auto Item" };
    private static readonly string[] PET_TABS = new string[] { "Hành Trang", "Kỹ Năng", "Trạng Thái" };
    private static long lastToggleTime;
    private static long lastPetInfoRequestTime;
    private static Effect previewProtectEffect;
    private static Effect previewFreezeEffect;
    private static Image imgPanel9;
    private static Image imgPanelBg;
    private static Image imgTopTab;
    private static Image imgTopTabActive;
    private static Image imgSubTab;
    private static Image imgSubTabActive;
    private static Image imgEquipFrame;
    private static Image imgStatsFrame;
    private static Image imgCurrencyFrame;
    private static Image imgTaskFrame;
    private static bool triedLoadPanel9;
    private static bool triedLoadPanelBg;
    private static bool triedLoadTabs;

    private static int abs(int v) { return v > 0 ? v : -v; }
    private static int min(int a, int b) { return a < b ? a : b; }
    private static int max(int a, int b) { return a > b ? a : b; }
    private static bool triedLoadEquipFrame;
    private static bool triedLoadStatsFrame;
    private static bool triedLoadCurrencyFrame;
    private static bool triedLoadTaskFrame;
    private static bool isDownOutside;
    private static int openMapId = -1;
    private static int openZoneId = -1;
    private static long suppressFusionHighlightUntil;
    private static int pendingPetStatus = -1;
    private static long pendingPetStatusUntil;
    private static bool hasPorataInBag;
    private static bool isPorataFusionActive;
    private static int taskMapClickedIndex = -1;
    private static int taskMapScrollX;
    private static int taskMapScrollY;
    private static int taskMapContentX;
    private static int taskMapContentY;
    private static int taskMapContentW;
    private static int taskMapContentH;
    private static int popupScrollY;
    private static int popupScrollTargetY;
    private static int popupScrollRun;
    private static int popupScrollCmdy;
    private static int popupScrollCmvy;
    private static int popupDragStartY;
    private static bool popupDragged;

    public static void Show()
    {
        isShow = true;
        openMapId = TileMap.mapID;
        openZoneId = TileMap.zoneID;
        pendingPetStatus = -1;
        pendingPetStatusUntil = 0L;
        SyncPetStateFlags();
    }

    public static void Hide()
    {
        isShow = false;
        pendingPetStatus = -1;
        pendingPetStatusUntil = 0L;
        hasPorataInBag = false;
        isPorataFusionActive = false;
    }

    private static void ClosePanelState(bool playSound)
    {
        Hide();
        if (GameCanvas.panel != null)
        {
            GameCanvas.panel.cp = null;
        }
        GameCanvas.menu.showMenu = false;
        ResetSelection();
        GameCanvas.clearAllPointerEvent();
        if (playSound)
        {
            SoundMn.gI().panelClick();
        }
    }

    private static bool ShouldAutoClosePanel()
    {
        if (!isShow)
        {
            return false;
        }

        // Đóng ngay khi đang chuyển map/khu để tránh panel "kẹt" qua scene mới.
        if (Char.ischangingMap || Char.isLoadingMap)
        {
            return true;
        }

        // Đổi map/khu xong thì đóng panel.
        if (openMapId != -1 && (TileMap.mapID != openMapId || TileMap.zoneID != openZoneId))
        {
            return true;
        }

        // Gọi rồng: đóng panel để không đè giao diện sự kiện.
        GameScr scr = GameScr.gI();
        return scr != null && scr.isRongThanXuatHien;
    }

    public static void Toggle()
    {
        isShow = !isShow;
    }

    public static void Update()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                long now = mSystem.currentTimeMillis();
                if (now - lastToggleTime > 250)
                {
                    lastToggleTime = now;
                    Toggle();
                    GameScr.info1.addInfo(isShow ? "Mở Panel Skin" : "Tắt Panel Skin", 0);
                }
            }
            if (!isShow)
            {
                return;
            }

            // Nếu có Dialog hoặc Menu gốc hoặc bất kỳ Popup nào đang mở, không xử lý input của CustomPanel
            if (GameCanvas.currentDialog != null || GameCanvas.menu.showMenu || (GameCanvas.panel != null && GameCanvas.panel.cp != null) || 
                ChatLogPopup.gI().IsPointerInPopup() || FriendPopup.gI().IsPointerInPopup() || EnemyPopup.gI().IsPointerInPopup())
            {
                if (ChatLogPopup.gI().IsPointerInPopup() || FriendPopup.gI().IsPointerInPopup() || EnemyPopup.gI().IsPointerInPopup())
                {
                    // Vẫn cần ResetSelection để tránh "dính" highlight khi click vào popup
                    ResetSelection();
                }
                return;
            }

            SyncPetStateFlags();

            // 1. Lưu trạng thái và Tracking (Phải ở đầu)
            bool isDown = GameCanvas.isPointerDown;
            bool isRelease = GameCanvas.isPointerJustRelease;
            int px = GameCanvas.px;
            int py = GameCanvas.py;
            ComputeLayout();
            if (selectedGameInfoPopup != null)
            {
                UpdatePopupScroll();
                if (isRelease)
                {
                    TryHandleGameInfoPopupClick(true);
                }
                BlockGameInput();
                return;
            }
            if (selectedToolAction >= 0)
            {
                UpdateToolDetailScroll();
            }
            if (ShouldAutoClosePanel())
            {
                ClosePanelState(false);
                return;
            }
            bool pointerInPanel = IsPointerInPanel();

            if (isDown)
            {
                dragTime++;
                if (dragTime == 1)
                {
                    firstDragY = py;
                    firstDragX = px;
                    for (int i = 0; i < dragLastY.Length; i++) dragLastY[i] = py;
                    globalDragged = false;
                    isDownOutside = !pointerInPanel;
                    // Chỉ reset selection nếu không phải đang click vào menu/popup
                    bool isMenuShown = GameCanvas.menu.showMenu || (GameCanvas.panel != null && GameCanvas.panel.cp != null);
                    if (!isMenuShown)
                    {
                        ResetSelection();
                    }
                }
                if (dragTime > 1 && (System.Math.Abs(py - firstDragY) > 10 || System.Math.Abs(px - firstDragX) > 10))
                {
                    if (!globalDragged)
                    {
                        globalDragged = true;
                        ResetSelection();
                    }
                }
            }
            else
            {
                dragTime = 0;
                // Không reset ngay khi vừa nhả chuột, vì nhánh isRelease cần đọc cờ này
                // để xử lý đóng panel khi click bắt đầu từ bên ngoài.
                if (!isRelease)
                {
                    isDownOutside = false;
                }
            }

            // 2. Trạng thái tương tác
            bool wasInteracting = pointerInPanel || draggingBag || draggingSkill;

            // 3. Cập nhật Scroll
            if (topTab == 4 || (topTab == 2 && petSubTab == 0))
            {
                UpdateBagScroll();
                UpdateSelectedItemPosition();
            }
            else if (topTab == 3)
            {
                UpdateSkillScroll();
                UpdateSelectedItemPosition();
            }
            
            if (GameCanvas.panel != null)
            {
                if (GameCanvas.panel.chatTField != null && GameCanvas.panel.chatTField.isShow)
                {
                    GameCanvas.panel.chatTField.update();
                    if (GameCanvas.panel.chatTField.left != null && (GameCanvas.keyPressed[12] || mScreen.getCmdPointerLast(GameCanvas.panel.chatTField.left)))
                    {
                        GameCanvas.panel.chatTField.left.performAction();
                    }
                    if (GameCanvas.panel.chatTField.right != null && (GameCanvas.keyPressed[13] || mScreen.getCmdPointerLast(GameCanvas.panel.chatTField.right)))
                    {
                        GameCanvas.panel.chatTField.right.performAction();
                    }
                    if (GameCanvas.panel.chatTField.center != null && (GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25] || mScreen.getCmdPointerLast(GameCanvas.panel.chatTField.center)))
                    {
                        GameCanvas.panel.chatTField.center.performAction();
                    }
                    if (GameCanvas.keyAsciiPress != 0)
                    {
                        GameCanvas.panel.chatTField.keyPressed(GameCanvas.keyAsciiPress);
                        GameCanvas.keyAsciiPress = 0;
                    }
                    GameCanvas.clearKeyHold();
                    GameCanvas.clearKeyPressed();
                    return;
                }
                if (topTab == 5 && GameCanvas.panel.tabIcon != null && GameCanvas.panel.tabIcon.isShow)
                {
                    GameCanvas.panel.tabIcon.update();
                    GameCanvas.panel.tabIcon.updateKey();
                    return;
                }
            }

            if (isDown && dragTime > 1)
            {
                int dyTotal = System.Math.Abs(py - firstDragY);
                int dxTotal = System.Math.Abs(px - firstDragX);
                if (dyTotal > 10 || dxTotal > 10)
                {
                    ResetSelection();
                }
            }

            // 5. Xử lý Selection
            if (isRelease)
            {
                // Tính toán hitbox cho nút Close (tâm tại panelX + panelW - 5, panelY + 28)
                int closeBtnX = panelX + panelW - 17; // (panelX + panelW - 5) - 12
                int closeBtnY = panelY + 16;         // (panelY + 28) - 12
                if (GameCanvas.isPointerHoldIn(closeBtnX, closeBtnY, 24, 24))
                {
                    ClosePanelState(true);
                    return;
                }
                
                // Đóng panel khi click ra ngoài (Tap outside), tránh đóng khi click vào các popup
                if (!pointerInPanel && !globalDragged && isDownOutside && 
                    !ChatLogPopup.gI().IsPointerInPopup() && !FriendPopup.gI().IsPointerInPopup() && !EnemyPopup.gI().IsPointerInPopup())
                {
                    ClosePanelState(true);
                    return;
                }

                if (wasInteracting && !globalDragged)
                {
                    HandlePanelSelection(true);
                    BlockGameInput();
                }

                // Reset tracking flags sau khi nhả
                globalDragged = false;
                isDownOutside = false;
            }
            else if (isDown && wasInteracting)
            {
                // Chỉ cập nhật visual selection nếu KHÔNG đang drag và KHÔNG đang chạy quán tính
                if (!globalDragged && bagScrollRun == 0 && skillScrollRun == 0)
                {
                    // Tăng delay lên 5 frames để chắc chắn người dùng muốn chọn chứ không phải scroll
                    if (dragTime > 5)
                    {
                        HandlePanelSelection(false);
                    }
                }
                else
                {
                    // Nếu đang drag, xóa ngay selection hiện tại để tránh nhầm lẫn
                    ResetSelection();
                }

                if (topTab != 3 || skillDragged || globalDragged)
                {
                    BlockGameInput();
                }
            }

            // 6. Cập nhật lịch sử pointer cho frame sau
            if (isDown)
            {
                for (int i = dragLastY.Length - 1; i > 0; i--)
                {
                    dragLastY[i] = dragLastY[i - 1];
                }
                dragLastY[0] = py;
            }
        }
        catch (System.Exception ex)
        {
            isShow = false;
            Debug.Log("CustomInventoryPanel.Update error: " + ex.Message);
        }
    }

    public static void Paint(mGraphics g)
    {
        if (!isShow)
        {
            return;
        }
        try
        {
            ComputeLayout();
            int oldTx = g.getTranslateX();
            int oldTy = g.getTranslateY();
            g.translate(-oldTx, -oldTy);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
            PaintTheme(g);
            if (topTab == 0)
            {
                PaintTaskTab(g);
            }
            else if (topTab == 1)
            {
                PaintCharacterTab(g);
            }
            else if (topTab == 2)
            {
                PaintPetTab(g);
            }
            else if (topTab == 3)
            {
                PaintSkillTab(g);
            }
            else if (topTab == 5)
            {
                PaintClanTab(g);
            }
            else if (topTab == 6)
            {
                PaintModTab(g);
            }
            else if (topTab == 7)
            {
                PaintToolTab(g);
            }
            else if (topTab == 4 && DEBUG_LAYER >= 2)
            {
                PaintEmptySlots(g);
                PaintOriginalItemDetail(g);
            }
            if (topTab == 1 || topTab == 2)
            {
                PaintOriginalItemDetail(g);
            }
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
            if (GameCanvas.panel != null && GameCanvas.panel.chatTField != null && GameCanvas.panel.chatTField.isShow)
            {
                GameCanvas.panel.chatTField.paint(g);
            }
            PaintGameInfoPopup(g);
            if (GameCanvas.panel != null && GameCanvas.panel.tabIcon != null && GameCanvas.panel.tabIcon.isShow)
            {
                GameCanvas.panel.tabIcon.paint(g);
            }
            g.translate(oldTx, oldTy);
        }
        catch (System.Exception ex)
        {
            isShow = false;
            Debug.Log("CustomInventoryPanel.Paint error: " + ex.Message);
        }
    }

    private static void ComputeLayout()
    {
        panelW = (GameCanvas.w - 28 < 540) ? (GameCanvas.w - 28) : 540;
        // Tăng chiều cao panel để phần tab trên cùng và nội dung có thêm khoảng thở.
        panelH = (GameCanvas.h - 20 < 360) ? (GameCanvas.h - 20) : 360;
        if (panelW < 420)
        {
            panelW = 420;
        }
        if (panelH < 310)
        {
            panelH = 310;
        }
        panelX = (GameCanvas.w - panelW) / 2;
        // Không kéo panel lên trên nữa để tránh tab top bị nhô khỏi khung.
        panelY = (GameCanvas.h - panelH) / 2;
        if (panelY < 6)
        {
            panelY = 6;
        }
    }

    private static void UpdateBagScroll()
    {
        bool isInfoVisible = (GameCanvas.panel != null && GameCanvas.panel.cp != null) || GameCanvas.menu.showMenu;
        if (isInfoVisible)
        {
            if (GameCanvas.isPointerDown && dragTime > 2 && (System.Math.Abs(GameCanvas.py - firstDragY) > 10 || System.Math.Abs(GameCanvas.px - firstDragX) > 10))
            {
                if (GameCanvas.panel != null) GameCanvas.panel.cp = null;
                GameCanvas.menu.showMenu = false;
                selectedItemInfo = null;
                GameCanvas.clearAllPointerEvent();
            }
            draggingBag = false;
            return;
        }

        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightY = panelY + 66;
        int viewW = safeW / 2;
        int viewH = panelY + panelH - 36 - rightY;
        int maxScroll = GetBagMaxScroll(viewH);
        bool inside = GameCanvas.px >= rightX - 10 && GameCanvas.px <= rightX + viewW + 10 && GameCanvas.py >= rightY - 10 && GameCanvas.py <= rightY + viewH + 10;
        if ((inside || draggingBag) && GameCanvas.isPointerDown)
        {
            if (!draggingBag)
            {
                draggingBag = true;
                bagScrollYBeforeDrag = bagScrollY;
                downWhenRunning = (bagScrollRun != 0);
                bagScrollRun = 0;
            }
            else
            {
                int dy = GameCanvas.py - dragLastY[0];
                bagScrollTargetY -= dy;
                
                // Giới hạn biên cứng khi đang drag (cho phép kéo lố 60px để tạo cảm giác đàn hồi)
                if (bagScrollTargetY < -60) bagScrollTargetY = -60;
                if (bagScrollTargetY > maxScroll + 60) bagScrollTargetY = maxScroll + 60;

                // Trong lúc drag, ép bagScrollY đi theo TargetY ngay lập tức để bám chuột (không smoothing)
                bagScrollY = bagScrollTargetY;
            }
        }
        if (!GameCanvas.isPointerJustRelease || !draggingBag)
        {
            ApplyBagScrollRun(maxScroll);
            return;
        }
        if (System.Math.Abs(GameCanvas.py - firstDragY) <= 20 && System.Math.Abs(GameCanvas.px - firstDragX) <= 20)
        {
            bagScrollY = bagScrollYBeforeDrag;
            bagScrollTargetY = bagScrollY;
            bagScrollRun = 0;
            draggingBag = false;
            return;
        }
        GameCanvas.isPointerJustRelease = false;
        if (!downWhenRunning)
        {
            int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
            if (force > 15) force = 15;
            if (force < -15) force = -15;
            bagScrollRun = -force * 150;
        }
        draggingBag = false;
        dragTime = 0;
        ApplyBagScrollRun(maxScroll);
    }

    private static void ApplyBagScrollRun(int maxScroll)
    {
        if (bagScrollRun != 0 && !draggingBag)
        {
            bagScrollTargetY += bagScrollRun / 100;
            bagScrollRun = bagScrollRun * 9 / 10;
            if (bagScrollRun < 100 && bagScrollRun > -100)
            {
                bagScrollRun = 0;
            }

            if (bagScrollTargetY < 0)
            {
                bagScrollTargetY = 0;
                bagScrollRun = 0;
            }
            else if (bagScrollTargetY > maxScroll)
            {
                bagScrollTargetY = maxScroll;
                bagScrollRun = 0;
            }
            else
            {
                bagScrollY = bagScrollTargetY;
            }
        }
        
        if (!draggingBag)
        {
            // T- `Tt snap lAi vA biA?n
            if (bagScrollTargetY < 0) bagScrollTargetY = 0;
            if (bagScrollTargetY > maxScroll) bagScrollTargetY = maxScroll;

            if (bagScrollY != bagScrollTargetY)
            {
                bagScrollCmvy = bagScrollTargetY - bagScrollY << 2;
                bagScrollCmdy += bagScrollCmvy;
                bagScrollY += bagScrollCmdy >> 4;
                bagScrollCmdy &= 15;
            }
        }
    }

    private static void UpdatePopupScroll()
    {
        if (selectedGameInfoPopup == null) return;
        
        int w = 240;
        int popupH = 160;
        int cx = GameCanvas.w / 2;
        int cy = GameCanvas.h / 2;
        int popupX = cx - w / 2;
        int popupY = cy - popupH / 2;

        int textW = w - 20;
        string[] mainLines = mFont.tahoma_7b_dark.splitFontArray(selectedGameInfoPopup.main, textW);
        string[] contentLines = mFont.tahoma_7.splitFontArray(selectedGameInfoPopup.content, textW);
        
        int totalContentH = (mainLines.Length + contentLines.Length) * 12 + 10;
        int viewH = popupH - 40;
        int maxScroll = totalContentH - viewH;
        if (maxScroll < 0) maxScroll = 0;

        // Xử lý lăn chuột
        if (GameCanvas.pXYScrollMouse != 0)
        {
            popupScrollTargetY -= GameCanvas.pXYScrollMouse * 10;
            GameCanvas.pXYScrollMouse = 0;
        }

        bool inside = Hit(GameCanvas.px, GameCanvas.py, popupX, popupY + 30, w, popupH - 40);
        if (GameCanvas.isPointerDown)
        {
            if (!popupDragged)
            {
                if (inside)
                {
                    popupDragged = true;
                    popupDragStartY = GameCanvas.py;
                    popupScrollRun = 0;
                }
            }
            else
            {
                int dy = GameCanvas.py - dragLastY[0];
                popupScrollTargetY -= dy;
                
                // Giới hạn đàn hồi khi đang kéo
                if (popupScrollTargetY < -60) popupScrollTargetY = -60;
                if (popupScrollTargetY > maxScroll + 60) popupScrollTargetY = maxScroll + 60;
                
                popupScrollY = popupScrollTargetY;
            }
        }
        else if (GameCanvas.isPointerJustRelease && popupDragged)
        {
            int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
            if (force > 15) force = 15;
            if (force < -15) force = -15;
            popupScrollRun = -force * 150;
            popupDragged = false;
        }

        ApplyPopupScrollRun(maxScroll);
    }

    private static void ApplyPopupScrollRun(int maxScroll)
    {
        if (popupScrollRun != 0 && !popupDragged)
        {
            popupScrollTargetY += popupScrollRun / 100;
            popupScrollRun = popupScrollRun * 9 / 10;
            if (popupScrollRun < 100 && popupScrollRun > -100)
            {
                popupScrollRun = 0;
            }
        }
        
        if (!popupDragged)
        {
            if (popupScrollTargetY < 0) popupScrollTargetY = 0;
            if (popupScrollTargetY > maxScroll) popupScrollTargetY = maxScroll;

            if (popupScrollY != popupScrollTargetY)
            {
                popupScrollCmvy = popupScrollTargetY - popupScrollY << 2;
                popupScrollCmdy += popupScrollCmvy;
                popupScrollY += popupScrollCmdy >> 4;
                popupScrollCmdy &= 15;
            }
        }
    }

    private static void UpdateToolDetailScroll()
    {
        if (selectedToolAction < 0) return;

        int safeY = panelY + 52;
        int safeH = panelH - 118;
        int detailH = safeH - 30;

        // Recalculate detail area for hit testing
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int gap = 6;
        int catW = 108;
        int detailW = 164;
        int listW = safeW - catW - gap - (detailW + gap);
        int detailX = safeX + catW + gap + listW + gap;

        int rowH = 26;
        int count = 0;
        Panel p = GameCanvas.panel;
        if (selectedToolAction == 0 && Panel.vGameInfo != null) count = Panel.vGameInfo.size();
        else if (selectedToolAction == 4 && p.vFlag != null) count = p.vFlag.size();
        else if (selectedToolAction == 5 && GameScr.gI().zones != null) count = GameScr.gI().zones.Length;
        else if (selectedToolAction == 7 && selectedAccountSubAction == 1 && p.vFriend != null) { count = p.vFriend.size(); rowH = 28; }
        else if (selectedToolAction == 7 && selectedAccountSubAction == 2 && p.vEnemy != null) { count = p.vEnemy.size(); rowH = 28; }
        else if (selectedToolAction == 7 && selectedAccountSubAction <= 0 && Panel.strAccount != null) count = Panel.strAccount.Length;
        else if (selectedToolAction == 8 && Panel.strCauhinh != null) count = Panel.strCauhinh.Length;

        int totalH = count * rowH;
        int maxScroll = totalH - detailH;
        if (maxScroll < 0) maxScroll = 0;

        if (GameCanvas.pXYScrollMouse != 0)
        {
            toolDetailScrollTargetY -= GameCanvas.pXYScrollMouse * 20;
            GameCanvas.pXYScrollMouse = 0;
        }

        bool inside = GameCanvas.px >= detailX && GameCanvas.px <= detailX + detailW && GameCanvas.py >= safeY + 24 && GameCanvas.py <= safeY + safeH;

        if (GameCanvas.isPointerDown)
        {
            if (!toolDetailDragged)
            {
                if (inside)
                {
                    toolDetailDragged = true;
                    toolDetailDragStartY = GameCanvas.py;
                    toolDetailScrollYBeforeDrag = toolDetailScrollY;
                    toolDetailScrollRun = 0;
                }
            }
            else
            {
                int dy = toolDetailDragStartY - GameCanvas.py;
                if (abs(dy) > 5) globalDragged = true;
                toolDetailScrollTargetY = toolDetailScrollYBeforeDrag + dy;
            }
        }
        else if (GameCanvas.isPointerJustRelease && toolDetailDragged)
        {
            toolDetailDragged = false;
        }

        if (toolDetailScrollTargetY < 0) toolDetailScrollTargetY = 0;
        if (toolDetailScrollTargetY > maxScroll) toolDetailScrollTargetY = maxScroll;

        if (!toolDetailDragged && toolDetailScrollY != toolDetailScrollTargetY)
        {
            toolDetailScrollRun = (toolDetailScrollTargetY - toolDetailScrollY) >> 2;
            if (toolDetailScrollRun == 0) toolDetailScrollRun = (toolDetailScrollTargetY > toolDetailScrollY) ? 1 : -1;
            toolDetailScrollY += toolDetailScrollRun;
        }
    }

    private static void UpdateSkillScroll()
    {
        bool isInfoVisible = (GameCanvas.panel != null && GameCanvas.panel.cp != null) || GameCanvas.menu.showMenu;
        if (isInfoVisible)
        {
            if (GameCanvas.isPointerDown && dragTime > 2 && (System.Math.Abs(GameCanvas.py - firstDragY) > 10 || System.Math.Abs(GameCanvas.px - firstDragX) > 10))
            {
                if (GameCanvas.panel != null) GameCanvas.panel.cp = null;
                GameCanvas.menu.showMenu = false;
                selectedItemInfo = null;
                GameCanvas.clearAllPointerEvent();
            }
            draggingSkill = false;
            return;
        }

        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int listY = panelY + 47;
        int listH = panelY + panelH - 36 - listY;
        int maxScroll = GetSkillMaxScroll(listH);
        bool inside = GameCanvas.px >= safeX + 6 && GameCanvas.px <= safeX + safeW - 12 && GameCanvas.py >= listY && GameCanvas.py <= listY + listH;
        if ((inside || draggingSkill) && GameCanvas.isPointerDown)
        {
            if (!draggingSkill)
            {
                draggingSkill = true;
                skillDragged = false;
                skillDragStartY = GameCanvas.py;
                downWhenRunning = (skillScrollRun != 0);
                skillScrollRun = 0;
                skillScrollCmdy = 0;
                skillScrollCmvy = 0;
            }
            else
            {
                if (System.Math.Abs(GameCanvas.py - skillDragStartY) > 4)
                {
                    skillDragged = true;
                }
                if (skillDragged)
                {
                    int dy = GameCanvas.py - dragLastY[0];
                    skillScrollTargetY -= dy;
                    
                    if (skillScrollTargetY < -60) skillScrollTargetY = -60;
                    if (skillScrollTargetY > maxScroll + 60) skillScrollTargetY = maxScroll + 60;

                    // Bám chuột tuyệt đối khi đang drag
                    skillScrollY = skillScrollTargetY;
                }
            }
            if (skillDragged)
            {
                BlockGameInput();
            }
        }
        if (!GameCanvas.isPointerJustRelease || !draggingSkill)
        {
            ApplySkillScrollRun(maxScroll);
            return;
        }
        if (System.Math.Abs(GameCanvas.py - skillDragStartY) <= 20)
        {
            skillScrollRun = 0;
            skillScrollTargetY = skillScrollY;
            draggingSkill = false;
            return;
        }
        GameCanvas.isPointerJustRelease = false;
        int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
        if (force > 15) force = 15;
        if (force < -15) force = -15;
        skillScrollRun = -force * 150;
        draggingSkill = false;
        ApplySkillScrollRun(maxScroll);
    }

    private static void ApplySkillScrollRun(int maxScroll)
    {
        if (skillScrollRun != 0 && !draggingSkill)
        {
            skillScrollTargetY += skillScrollRun / 100;
            skillScrollRun = skillScrollRun * 9 / 10;
            if (skillScrollRun < 100 && skillScrollRun > -100)
            {
                skillScrollRun = 0;
            }

            if (skillScrollTargetY < 0)
            {
                skillScrollTargetY = 0;
                skillScrollRun = 0;
            }
            else if (skillScrollTargetY > maxScroll)
            {
                skillScrollTargetY = maxScroll;
                skillScrollRun = 0;
            }
            else
            {
                skillScrollY = skillScrollTargetY;
            }
        }
        
        if (!draggingSkill)
        {
            if (skillScrollTargetY < 0) skillScrollTargetY = 0;
            if (skillScrollTargetY > maxScroll) skillScrollTargetY = maxScroll;

            if (skillScrollY != skillScrollTargetY)
            {
                skillScrollCmvy = skillScrollTargetY - skillScrollY << 2;
                skillScrollCmdy += skillScrollCmvy;
                skillScrollY += skillScrollCmdy >> 4;
                skillScrollCmdy &= 15;
            }
        }
        if (skillScrollY < 0)
        {
            skillScrollY = 0;
        }
        if (skillScrollY > maxScroll)
        {
            skillScrollY = maxScroll;
        }
        if (skillScrollTargetY < 0)
        {
            skillScrollTargetY = 0;
        }
        if (skillScrollTargetY > maxScroll)
        {
            skillScrollTargetY = maxScroll;
        }
    }

    private static int GetSkillMaxScroll(int viewH)
    {
        int rows = 6;
        int contentH = rows * 40;
        int max = contentH - viewH;
        return (max > 0) ? max : 0;
    }

    private static int GetFullSkillCount(Char ch)
    {
        if (ch == null)
        {
            return 0;
        }
        if (ch.nClass != null && ch.nClass.skillTemplates != null)
        {
            return ch.nClass.skillTemplates.Length + 6;
        }
        return ((ch.vSkill != null) ? ch.vSkill.size() : 0) + 6;
    }

    private static int GetBagMaxScroll(int viewH)
    {
        Item[] items = GetRightTabItems();
        int count = (items != null) ? items.Length : 0;
        int rows = count / 6 + ((count % 6 != 0) ? 1 : 0);
        int contentH = rows * (26 + 4);
        int max = contentH - viewH;
        return (max > 0) ? max : 0;
    }

    private static Item[] GetRightTabItems()
    {
        if (Char.myCharz() == null)
        {
            return null;
        }
        if (rightSubTab == 1)
        {
            return Char.myCharz().arrItemBox;
        }
        return Char.myCharz().arrItemBag;
    }

    public static bool IsPointerInPanel()
    {
        return isShow && GameCanvas.px >= panelX && GameCanvas.px <= panelX + panelW && GameCanvas.py >= panelY && GameCanvas.py <= panelY + panelH;
    }

    private static void BlockGameInput()
    {
        GameCanvas.clearAllPointerEvent();
        mScreen.keyTouch = -1;
        mScreen.keyMouse = -1;
        for (int i = 0; i < GameCanvas.keyHold.Length; i++)
        {
            GameCanvas.keyHold[i] = false;
        }
        for (int j = 0; j < GameCanvas.keyPressed.Length; j++)
        {
            GameCanvas.keyPressed[j] = false;
        }
    }

    private static bool TryHandleTopTabClick(bool isFire)
    {
        int[] visibleTabs = GetVisibleTabs();
        int tabW = 60;
        int tabH = 24;
        int gap = 1;
        int totalTabsW = visibleTabs.Length * tabW + (visibleTabs.Length - 1) * gap;
        int tabX = panelX + (panelW - totalTabsW) / 2;
        int tabY = panelY + 18;
        for (int vi = 0; vi < visibleTabs.Length; vi++)
        {
            int x = tabX + vi * (tabW + gap);
            if (Hit(GameCanvas.px, GameCanvas.py, x, tabY, tabW, tabH))
            {
                if (!isFire) return true; // Chỉ chặn, không đổi tab khi chưa thả chuột
                topTab = visibleTabs[vi];
                bagScrollY = 0;
                bagScrollTargetY = 0;
                bagScrollRun = 0;
                skillScrollY = 0;
                skillScrollTargetY = 0;
                skillScrollRun = 0;
                skillScrollCmdy = 0;
                skillScrollCmvy = 0;
                draggingSkill = false;
                downWhenRunning = false;
                selectedBagIndex = -1;
                selectedBoxIndex = -1;
                selectedAutoIndex = -1;
                selectedSkillIndex = -1;
                selectedBodyIndex = -1;
                selectedClanMsgIndex = -1;
                selectedClanLogicIndex = -1;
                selectedItemInfo = null;
                if (GameCanvas.panel != null)
                {
                    GameCanvas.panel.cp = null;
                }
                if (visibleTabs[vi] == 2)
                {
                    RequestPetInfoIfNeeded(true);
                }
                SoundMn.gI().panelClick();
                return true;
            }
        }
        return false;
    }

    private static bool TryHandleRightSubTabClick(bool isFire)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int y = panelY + 44;
        int gridW = 34 * 6 + 4 * 5;
        int tw = gridW / RIGHT_TABS.Length;
        for (int i = 0; i < RIGHT_TABS.Length; i++)
        {
            int x = rightX + i * tw;
            if (Hit(GameCanvas.px, GameCanvas.py, x, y, tw - 2, 18))
            {
                if (!isFire) return true;
                if (rightSubTab != i)
                {
                    rightSubTab = i;
                    bagScrollY = 0;
                    bagScrollTargetY = 0;
                    bagScrollRun = 0;
                    selectedBagIndex = -1;
                    selectedBoxIndex = -1;
                    selectedAutoIndex = -1;
                    selectedItemInfo = null;
                    if (GameCanvas.panel != null)
                    {
                        GameCanvas.panel.cp = null;
                    }
                }
                SoundMn.gI().panelClick();
                return true;
            }
        }
        return false;
    }

    private static bool TryHandlePetSubTabClick(bool isFire)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightW = 34 * 6 + 4 * 5;
        int y = panelY + 44;
        int tw = rightW / PET_TABS.Length;
        for (int i = 0; i < PET_TABS.Length; i++)
        {
            int x = rightX + i * tw;
            if (Hit(GameCanvas.px, GameCanvas.py, x, y, tw - 2, 18))
            {
                if (!isFire) return true;
                petSubTab = i;
                SoundMn.gI().panelClick();
                return true;
            }
        }
        return false;
    }

    private static bool TryHandleSkillClick(bool isFire)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return false;
        }
        int safeX = panelX + 24;
        int safeY = panelY + 42;
        int safeW = panelW - 48;
        int listY = panelY + 52;
        int listH = panelY + panelH - 36 - listY;
        int colGap = 3;
        int colW = (safeW - colGap * 2) / 3;
        int listX = safeX;
        if (GameCanvas.py < listY || GameCanvas.py > listY + listH)
        {
            return false;
        }
        int col = -1;
        for (int i = 0; i < 3; i++)
        {
            int x = listX + i * (colW + colGap);
            if (GameCanvas.px >= x && GameCanvas.px <= x + colW)
            {
                col = i;
                break;
            }
        }
        if (col < 0)
        {
            return false;
        }
        int localY = GameCanvas.py - listY + skillScrollY;
        int row = localY / 40;
        if (row >= 6) return false;
        int index = col * 6 + row;
        if (index < 0 || index >= GetFullSkillCount(c))
        {
            return false;
        }
        selectedSkillIndex = index;
        if (!isFire) return true;
        SoundMn.gI().panelClick();
        if (index >= 0 && index <= 4)
        {
            StartOriginalPotentialMenu(index, listY + row * 40 - skillScrollY);
            return true;
        }
        if (index == 5)
        {
            Service.gI().speacialSkill(0);
            return true;
        }
        if (index >= 6)
        {
            SkillTemplate template = GetSkillTemplateAt(c, index);
            Skill skill = GetLearnedSkill(c, template);
            StartOriginalSkillMenu(template, skill, index);
        }
        return true;
    }

    private static void StartOriginalPotentialMenu(int statIndex, int rowY)
    {
        if (GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.selected = statIndex;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        MyVector actions = new MyVector(string.Empty);
        string statName = string.Empty;
        int incValue = 1;
        long cost1 = 0L;
        long cost10 = 0L;
        long cost100 = 0L;
        int num = 1000;
        if (statIndex == 0)
        {
            statName = mResources.HP;
            incValue = c.hpFrom1000TiemNang;
            cost1 = (long)c.cHPGoc + num;
            cost10 = 10L * (2L * ((long)c.cHPGoc + num) + 180L) / 2L;
            cost100 = 100L * (2L * ((long)c.cHPGoc + num) + 1980L) / 2L;
        }
        else if (statIndex == 1)
        {
            statName = mResources.KI;
            incValue = c.mpFrom1000TiemNang;
            cost1 = (long)c.cMPGoc + num;
            cost10 = 10L * (2L * ((long)c.cMPGoc + num) + 180L) / 2L;
            cost100 = 100L * (2L * ((long)c.cMPGoc + num) + 1980L) / 2L;
        }
        else if (statIndex == 2)
        {
            statName = mResources.hit_point;
            incValue = c.damFrom1000TiemNang;
            cost1 = (long)c.cDamGoc * c.expForOneAdd;
            cost10 = 10L * (2L * (long)c.cDamGoc + 9L) / 2L * c.expForOneAdd;
            cost100 = 100L * (2L * (long)c.cDamGoc + 99L) / 2L * c.expForOneAdd;
        }
        else if (statIndex == 3)
        {
            statName = mResources.armor;
            incValue = 1;
            cost1 = 2L * (c.cDefGoc + 5L) / 2L * 100000L;
            cost10 = 10L * (2L * (c.cDefGoc + 5L) + 9L) / 2L * 100000L;
            cost100 = 100L * (2L * (c.cDefGoc + 5L) + 99L) / 2L * 100000L;
        }
        else if (statIndex == 4)
        {
            int idx = c.cCriticalGoc;
            if (idx > Panel.t_tiemnang.Length - 1)
            {
                idx = Panel.t_tiemnang.Length - 1;
            }
            long cost = Panel.t_tiemnang[idx];
            if (c.cTiemNang < cost)
            {
                GameCanvas.startOKDlg(mResources.not_enough_potential_point1 + Res.formatNumber2(c.cTiemNang) + mResources.not_enough_potential_point2 + Res.formatNumber2(cost));
                return;
            }
            GameCanvas.startYesNoDlg(mResources.use_potential_point_for1 + Res.formatNumber(cost) + mResources.use_potential_point_for2 + c.criticalFrom1000Tiemnang + mResources.for_crit, new Command(mResources.increase_upper, panel, 9000, null), new Command(mResources.CANCEL, panel, 4007, null));
            return;
        }
        if (c.cTiemNang < cost1)
        {
            GameCanvas.startOKDlg(mResources.not_enough_potential_point1 + c.cTiemNang + mResources.not_enough_potential_point2 + cost1);
            return;
        }
        actions.addElement(new Command(mResources.increase_upper + "\n" + incValue + " " + statName + "\n-" + Res.formatNumber2(cost1), panel, 9000, null));
        if (c.cTiemNang >= cost10)
        {
            actions.addElement(new Command(mResources.increase_upper + "\n" + (incValue * 10) + " " + statName + "\n-" + Res.formatNumber2(cost10), panel, 9006, null));
        }
        if (c.cTiemNang >= cost100)
        {
            actions.addElement(new Command(mResources.increase_upper + "\n" + (incValue * 100) + " " + statName + "\n-" + Res.formatNumber2(cost100), panel, 9007, null));
        }
        actions.addElement(new Command(ModFunc.strInCrease, ModFunc.GI(), 100, statIndex.ToString() + "-" + false.ToString()));
        GameCanvas.menu.startAt(actions, panelX, rowY + 40);
    }

    private static void StartOriginalSkillMenu(SkillTemplate template, Skill skill, int index)
    {
        if (template == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        MyVector actions = new MyVector(string.Empty);
        Skill nextSkill = null;
        if (skill != null)
        {
            if (skill.point == template.maxPoint)
            {
                actions.addElement(new Command(mResources.make_shortcut, panel, 9003, skill.template));
                actions.addElement(new Command(mResources.CLOSE, 2));
            }
            else
            {
                nextSkill = template.skills[skill.point];
                actions.addElement(new Command(mResources.UPGRADE, panel, 9002, nextSkill));
                actions.addElement(new Command(mResources.make_shortcut, panel, 9003, skill.template));
            }
        }
        else if (template.skills != null && template.skills.Length > 0)
        {
            nextSkill = template.skills[0];
            actions.addElement(new Command(mResources.learn, panel, 9004, nextSkill));
        }
        int menuX = panelX + panelW / 2 - actions.size() * 22;
        int menuY = panelY + panelH - 18;
        GameCanvas.menu.startAt(actions, menuX, menuY);
        panel.addSkillDetail(template, skill, nextSkill);
    }

    private static void HandlePanelSelection(bool isFire)
    {
        if (globalDragged || downWhenRunning)
        {
            return;
        }

        if (TryHandleTopTabClick(isFire))
        {
            return;
        }

        if (topTab == 4 && TryHandleRightSubTabClick(isFire))
        {
            return;
        }
        
        if (topTab == 2 && TryHandlePetSubTabClick(isFire))
        {
            return;
        }

        if (topTab == 0)
        {
            TryHandleTaskMapClick(isFire);
            return;
        }
        if (topTab == 2)
        {
            HandlePetSelection(isFire);
            return;
        }
        if (topTab == 3)
        {
            TryHandleSkillClick(isFire);
            return;
        }
        if (topTab == 5)
        {
            TryHandleClanClick(isFire);
            return;
        }
        if (topTab == 6)
        {
            TryHandleModClick(isFire);
            return;
        }
        if (topTab == 7)
        {
            TryHandleToolClick(isFire);
            return;
        }
        if (topTab == 1)
        {
            HandleCharacterSelection(isFire);
            return;
        }
        if (topTab != 4)
        {
            return;
        }
        Item[] bag = (Char.myCharz() != null) ? Char.myCharz().arrItemBag : null;
        Item[] body = (Char.myCharz() != null) ? Char.myCharz().arrItemBody : null;
        Item[] box = (Char.myCharz() != null) ? Char.myCharz().arrItemBox : null;
        Item[] rightItems = (rightSubTab == 1) ? box : bag;
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightY = panelY + 66;
        int gap = 4;
        int bagViewW = 34 * 6 + gap * 5;
        int bagViewH = 26 * 7 + gap * 6;
        if (GameCanvas.px >= rightX && GameCanvas.px <= rightX + bagViewW && GameCanvas.py >= rightY && GameCanvas.py <= rightY + bagViewH)
        {
            int localX = GameCanvas.px - rightX;
            int localY = GameCanvas.py - rightY + bagScrollY;
            int col = localX / (34 + gap);
            int row = localY / (26 + gap);
            if (col >= 0 && col < 6 && localX % (34 + gap) < 34)
            {
                int index = row * 6 + col;
                Item clicked = GetItem(rightItems, index);
                
                selectedBagIndex = (rightSubTab == 0) ? index : -1;
                selectedBoxIndex = (rightSubTab == 1) ? index : -1;
                selectedAutoIndex = (rightSubTab == 2) ? index : -1;
                selectedBodyIndex = -1;
                selectedItemInfo = clicked;
                selectedItemX = rightX + col * (34 + gap);
                selectedItemY = rightY + row * (26 + gap) - bagScrollY;
                
                if (clicked != null)
                {
                    if (isFire)
                    {
                        if (rightSubTab == 1)
                        {
                            StartOriginalBoxMenu(clicked, index);
                        }
                        else if (rightSubTab == 2)
                        {
                            StartAutoItemMenu(clicked, index);
                        }
                        else
                        {
                            int inventorySelected = ((body != null) ? body.Length : 0) + index;
                            StartOriginalItemMenu(clicked, false, inventorySelected);
                        }
                    }
                    else
                    {
                        // Chỉ cập nhật highlight visually, KHÔNG hiển thị popup info ngay lập tức khi chạm
                        if (GameCanvas.panel != null)
                        {
                            int inventorySelected = ((body != null) ? body.Length : 0) + index;
                            GameCanvas.panel.customSelectInventoryItem(inventorySelected);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
        int leftX = safeX;
        int leftY = panelY + 42;
        int leftW = safeW / 2 - 16;
        int frameX = safeX - 3;
        int frameY = panelY + 46;
        int frameW = 246;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 8;
        int bodyGapY = 27;
        for (int i = 0; i < 5; i++)
        {
            int y = bodyTopY + i * bodyGapY;
            Item clickedLeft = GetItem(body, i);
            if (Hit(GameCanvas.px, GameCanvas.py, bodyLeftX, y, 36, 24))
            {
                selectedBodyIndex = i;
                selectedBagIndex = -1;
                selectedItemInfo = clickedLeft;
                selectedItemX = bodyLeftX;
                selectedItemY = y;
                if (clickedLeft != null)
                {
                    if (isFire)
                    {
                        StartOriginalItemMenu(clickedLeft, true, i);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(i);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
            Item clickedRight = GetItem(body, i + 5);
            if (Hit(GameCanvas.px, GameCanvas.py, bodyRightX, y, 36, 24))
            {
                selectedBodyIndex = i + 5;
                selectedBagIndex = -1;
                selectedItemInfo = clickedRight;
                selectedItemX = bodyRightX;
                selectedItemY = y;
                if (clickedRight != null)
                {
                    if (isFire)
                    {
                        StartOriginalItemMenu(clickedRight, true, i + 5);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(i + 5);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            int x = centerX - 75 + i * (36 + 2);
            int bodyIndex = i + 10;
            Item clickedBottom = GetItem(body, bodyIndex);
            int bottomY = frameY + 148;
            if (Hit(GameCanvas.px, GameCanvas.py, x, bottomY, 36, 24))
            {
                selectedBodyIndex = bodyIndex;
                selectedBagIndex = -1;
                selectedItemInfo = clickedBottom;
                selectedItemX = x;
                selectedItemY = bottomY;
                if (clickedBottom != null)
                {
                    if (isFire)
                    {
                        StartOriginalItemMenu(clickedBottom, true, bodyIndex);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(bodyIndex);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
    }

    private static Char GetPrimaryPet()
    {
        Char me = Char.myCharz();
        if (me == null) return Char.myPetz();
        if (me.havePet2 && !me.havePet) return Char.MyPet2z();
        // Nếu có cả 2, mặc định hiển thị Pet 1, hoặc có thể thêm nút chuyển đổi sau.
        return Char.myPetz();
    }

    private static bool IsPrimaryPet2()
    {
        Char me = Char.myCharz();
        return me != null && me.havePet2 && !me.havePet;
    }

    private static string[] GetPetStatusOptions()
    {
        Char me = Char.myCharz();
        bool hasFusionForever = me != null && me.cgender == 1;
        return hasFusionForever
            ? new string[] { mResources.follow, mResources.defend, mResources.attack, mResources.gohome, mResources.fusion, mResources.fusionForever }
            : new string[] { mResources.follow, mResources.defend, mResources.attack, mResources.gohome, mResources.fusion };
    }

    private static void ApplyPetStatusSelection(int selectedStatus)
    {
        Char pet = GetPrimaryPet();
        if (pet == null)
        {
            return;
        }
        // Trạng thái hợp thể vĩnh viễn cần xác nhận như panel gốc.
        if (selectedStatus == 5)
        {
            pendingPetStatus = 5;
            pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;
            GameCanvas.startYesNoDlg(
                mResources.sure_fusion,
                new Command(mResources.YES, IsPrimaryPet2() ? 888352 : 888351),
                new Command(mResources.NO, 2001)
            );
            return;
        }
        // Nút "Hợp thể" đang bật: bấm lại sẽ tách hợp thể.
        if (selectedStatus == 4)
        {
            if (isPorataFusionActive)
            {
                isPorataFusionActive = false;
                RequestDetachFusion();
                return;
            }

            // Chưa hợp thể: ưu tiên dùng bông tai để hợp thể.
            int porataIndex = hasPorataInBag ? FindPorataBagIndex() : -1;
            if (porataIndex >= 0)
            {
                Service.gI().useItem(0, 1, (sbyte)porataIndex, -1);
                suppressFusionHighlightUntil = 0L;
                pendingPetStatus = 4;
                pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;
                isPorataFusionActive = true;
                return;
            }
        }
        // Đang hợp thể thì phải tắt hợp thể trước, chưa cho đổi state ngay.
        if (selectedStatus != 4 && isPorataFusionActive)
        {
            isPorataFusionActive = false;
            RequestDetachFusion();
            pendingPetStatus = pet.petStatus;
            pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;
            GameScr.info1.addInfo("Đã yêu cầu tắt hợp thể, hãy chọn lại trạng thái sau khi tách xong", 0);
            return;
        }
        if (IsPrimaryPet2())
        {
            Service.gI().pet2Status((sbyte)selectedStatus);
        }
        else
        {
            Service.gI().petStatus((sbyte)selectedStatus);
        }
        pet.petStatus = (sbyte) selectedStatus;
        pendingPetStatus = selectedStatus;
        pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;
    }

    private static void RequestDetachFusion()
    {
        int porataIndex = FindPorataBagIndex();
        if (porataIndex >= 0)
        {
            Service.gI().useItem(0, 1, (sbyte)porataIndex, -1);
        }
        else
        {
            // Fallback an toàn khi không có bông tai trong túi.
            Service.gI().funsion(6);
        }
        // Tránh giữ highlight "Hợp thể" quá lâu khi vừa bấm tách.
        suppressFusionHighlightUntil = mSystem.currentTimeMillis() + 2500L;
    }

    private static int FindPorataBagIndex()
    {
        Char me = Char.myCharz();
        Item[] bag = (me != null) ? me.arrItemBag : null;
        if (bag == null)
        {
            return -1;
        }
        int[] porataIds = new int[] { 454, 921, 2104, 1255 };
        for (int i = 0; i < bag.Length; i++)
        {
            Item item = bag[i];
            if (item == null || item.template == null)
            {
                continue;
            }
            int templateId = item.template.id;
            for (int j = 0; j < porataIds.Length; j++)
            {
                if (templateId == porataIds[j])
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static void SyncPetStateFlags()
    {
        Char me = Char.myCharz();
        if (me == null)
        {
            hasPorataInBag = false;
            isPorataFusionActive = false;
            return;
        }
        hasPorataInBag = FindPorataBagIndex() >= 0;
        long now = mSystem.currentTimeMillis();
        if (pendingPetStatus == 4 && now < pendingPetStatusUntil)
        {
            isPorataFusionActive = true;
            return;
        }
        isPorataFusionActive = me.isFusion;
    }

    private static void GetPetStatusLayout(int x, int y, int w, out int rowX, out int rowY, out int rowW, out int rowH, out int toggleW, out int toggleH)
    {
        rowX = x + 8;
        rowY = y + 12;
        rowW = w - 16;
        rowH = 22;
        toggleW = 24;
        toggleH = 14;
    }

    private static bool HasLoadedPetInfo(Char pet)
    {
        if (pet == null)
        {
            return false;
        }
        if (pet.arrItemBody != null && pet.arrItemBody.Length > 0)
        {
            return true;
        }
        return pet.cPower > 0L || pet.cHPFull > 0L || (pet.cName != null && pet.cName.Length > 0);
    }

    private static void RequestPetInfoIfNeeded(bool force)
    {
        Char master = Char.myCharz();
        if (master == null)
        {
            return;
        }

        long now = mSystem.currentTimeMillis();
        
        // Tránh spam request quá nhanh (dưới 2 giây)
        if (!force && now - lastPetInfoRequestTime < 2000)
        {
            return;
        }

        Char pet = GetPrimaryPet();
        
        // Nếu force (click tab) hoặc chưa tải xong dữ liệu đệ tử
        if (force || !HasLoadedPetInfo(pet))
        {
            // Chỉ gửi request nếu đã 10s trôi qua kể từ lần cuối (tránh lag server)
            if (force || now - lastPetInfoRequestTime > 10000)
            {
                lastPetInfoRequestTime = now;
                Service.gI().petInfo();
                if (master.havePet2)
                {
                    Service.gI().PetInfo2();
                }
            }
        }
    }

    private static void HandlePetSelection(bool isFire)
    {
        if (TryHandlePetSubTabClick(isFire))
        {
            return;
        }
        if (petSubTab == 2)
        {
            TryHandlePetStatusClick(isFire);
            return;
        }
        if (petSubTab != 0)
        {
            return;
        }
        Char master = Char.myCharz();
        Char pet = GetPrimaryPet();
        Item[] masterBag = (master != null) ? master.arrItemBag : null;
        Item[] masterBody = (master != null) ? master.arrItemBody : null;
        Item[] petBody = (pet != null) ? pet.arrItemBody : null;
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightY = panelY + 66;
        int gap = 4;
        int bagViewW = 34 * 6 + gap * 5;
        int bagViewH = 26 * 7 + gap * 6;
        if (GameCanvas.px >= rightX && GameCanvas.px <= rightX + bagViewW && GameCanvas.py >= rightY && GameCanvas.py <= rightY + bagViewH)
        {
            int localX = GameCanvas.px - rightX;
            int localY = GameCanvas.py - rightY + bagScrollY;
            int col = localX / (34 + gap);
            int row = localY / (26 + gap);
            if (col >= 0 && col < 6 && localX % (34 + gap) < 34)
            {
                int index = row * 6 + col;
                Item clicked = GetItem(masterBag, index);
                
                selectedBagIndex = index;
                selectedBoxIndex = -1;
                selectedAutoIndex = -1;
                selectedBodyIndex = -1;
                selectedItemInfo = clicked;
                selectedItemX = rightX + col * (34 + gap);
                selectedItemY = rightY + row * (26 + gap) - bagScrollY;
                
                if (clicked != null)
                {
                    if (isFire)
                    {
                        int inventorySelected = ((masterBody != null) ? masterBody.Length : 0) + index;
                        StartOriginalItemMenu(clicked, false, inventorySelected);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            int inventorySelected = ((masterBody != null) ? masterBody.Length : 0) + index;
                            GameCanvas.panel.customSelectInventoryItem(inventorySelected);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
        int leftX = safeX;
        int leftY = panelY + 42;
        int frameX = leftX - 3;
        int frameY = leftY + 4;
        int frameW = 246;
        int frameH = 188;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 19;
        int bodyGapY = 27;
        for (int i = 0; i < 5; i++)
        {
            int y = bodyTopY + i * bodyGapY;
            Item clickedLeft = GetItem(petBody, i);
            if (Hit(GameCanvas.px, GameCanvas.py, bodyLeftX, y, 36, 24))
            {
                selectedBodyIndex = i;
                selectedBagIndex = -1;
                selectedItemInfo = clickedLeft;
                selectedItemX = bodyLeftX;
                selectedItemY = y;
                if (clickedLeft != null)
                {
                    if (isFire)
                    {
                        StartPetBodyItemMenu(clickedLeft, i);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(i);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
            Item clickedRight = GetItem(petBody, i + 5);
            if (Hit(GameCanvas.px, GameCanvas.py, bodyRightX, y, 36, 24))
            {
                selectedBodyIndex = i + 5;
                selectedBagIndex = -1;
                selectedItemInfo = clickedRight;
                selectedItemX = bodyRightX;
                selectedItemY = y;
                if (clickedRight != null)
                {
                    if (isFire)
                    {
                        StartPetBodyItemMenu(clickedRight, i + 5);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(i + 5);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
        int bottomY = frameY + 159;
        int bottomW = 36;
        int bottomH = 24;
        int bottomGap = 2;
        for (int i = 0; i < 4; i++)
        {
            int x = centerX - 75 + i * (36 + 2);
            int bodyIndex = i + 10;
            Item clickedBottom = GetItem(petBody, bodyIndex);
            if (Hit(GameCanvas.px, GameCanvas.py, x, bottomY, 36, 24))
            {
                selectedBodyIndex = bodyIndex;
                selectedBagIndex = -1;
                selectedItemInfo = clickedBottom;
                selectedItemX = x;
                selectedItemY = bottomY;
                if (clickedBottom != null)
                {
                    if (isFire)
                    {
                        StartPetBodyItemMenu(clickedBottom, bodyIndex);
                    }
                    else
                    {
                        if (GameCanvas.panel != null)
                        {
                            GameCanvas.panel.customSelectInventoryItem(bodyIndex);
                        }
                    }
                }
                if (isFire) SoundMn.gI().panelClick();
                return;
            }
        }
    }

    private static bool TryHandlePetStatusClick(bool isFire)
    {
        Char pet = GetPrimaryPet();
        if (pet == null)
        {
            return false;
        }
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightW = 34 * 6 + 4 * 5;
        int contentY = panelY + 66;
        string[] options = GetPetStatusOptions();
        int rowX;
        int rowYStart;
        int rowW;
        int rowH;
        int toggleW;
        int toggleH;
        GetPetStatusLayout(rightX, contentY, rightW, out rowX, out rowYStart, out rowW, out rowH, out toggleW, out toggleH);
        for (int i = 0; i < options.Length; i++)
        {
            int rowY = rowYStart + i * rowH;
            int toggleX = rowX + rowW - toggleW - 6;
            int toggleY = rowY + (rowH - toggleH) / 2;
            bool hitRow = Hit(GameCanvas.px, GameCanvas.py, rowX, rowY, rowW, rowH - 1);
            bool hitToggle = Hit(GameCanvas.px, GameCanvas.py, toggleX, toggleY, toggleW, toggleH);
            if (!hitRow && !hitToggle)
            {
                continue;
            }
            if (!isFire)
            {
                return true;
            }
            int newStatus = i;
            if (pet.petStatus != newStatus)
            {
                ApplyPetStatusSelection(newStatus);
            }
            else
            {
                // Luôn giữ đúng 1 trạng thái bật, click lại trạng thái đang bật chỉ sync lại.
                ApplyPetStatusSelection(newStatus);
            }
            SoundMn.gI().panelClick();
            return true;
        }
        return false;
    }

    private static void HandleCharacterSelection(bool isFire)
    {
        Char me = Char.myCharz();
        Item[] body = (me != null) ? me.arrItemBody : null;
        int safeX = panelX + 24;
        int safeY = panelY + 42;
        int safeW = panelW - 48;
        int frameMargin = 2;
        int frameOffsetY = 6;
        int topX = safeX + frameMargin;
        int topY = safeY + frameMargin + frameOffsetY;
        int topW = safeW - frameMargin * 2;
        int topH = 160;
        int slotW = 36;
        int slotH = 24;
        int slotGapY = 27;
        int leftSlotX = topX + 22;
        int rightSlotX = topX + topW - 22 - slotW;
        int slotStartY = topY + 12;
        for (int i = 0; i < 5; i++)
        {
            int y = slotStartY + i * slotGapY;
            if (TrySelectBodySlot(body, i, leftSlotX, y, slotW, slotH, isFire))
            {
                return;
            }
            if (TrySelectBodySlot(body, i + 5, rightSlotX, y, slotW, slotH, isFire))
            {
                return;
            }
        }
        int centerX = topX + topW / 2;
        int bottomY = topY + topH - 32;
        int bottomGap = 6;
        int bottomCount = 4;
        int bottomStartX = centerX - (bottomCount * slotW + (bottomCount - 1) * bottomGap) / 2;
        for (int i = 0; i < bottomCount; i++)
        {
            int bodyIndex = i + 10;
            int x = bottomStartX + i * (slotW + bottomGap);
            if (TrySelectBodySlot(body, bodyIndex, x, bottomY, slotW, slotH, isFire))
            {
                return;
            }
        }
    }

    private static bool TrySelectBodySlot(Item[] body, int bodyIndex, int x, int y, int w, int h, bool isFire)
    {
        Item clicked = GetItem(body, bodyIndex);
        if (!Hit(GameCanvas.px, GameCanvas.py, x, y, w, h) || clicked == null)
        {
            return false;
        }
        selectedBodyIndex = bodyIndex;
        selectedBagIndex = -1;
        selectedBoxIndex = -1;
        selectedAutoIndex = -1;
        selectedItemInfo = clicked;
        selectedItemX = x;
        selectedItemY = y;
        if (isFire)
        {
            StartOriginalItemMenu(clicked, true, bodyIndex);
            SoundMn.gI().panelClick();
        }
        else
        {
            if (GameCanvas.panel != null)
            {
                GameCanvas.panel.customSelectInventoryItem(bodyIndex);
            }
        }
        return true;
    }

    private static void ResetSelection()
    {
        selectedBagIndex = -1;
        selectedBoxIndex = -1;
        selectedAutoIndex = -1;
        selectedSkillIndex = -1;
        selectedBodyIndex = -1;
        selectedClanMsgIndex = -1;
        selectedClanLogicIndex = -1;
        // selectedToolAction = -1;       // Do not reset here, handled by TryHandleToolClick
        // selectedToolDetailIndex = -1;  // Do not reset here, handled by TryHandleToolClick
        selectedItemInfo = null;
        if (GameCanvas.panel != null)
        {
            // KHÔNG reset currItem và selected nếu menu đang hiện 
            // vì lệnh thực thi từ menu sẽ cần tham chiếu đến item hiện tại
            if (!GameCanvas.menu.showMenu)
            {
                GameCanvas.panel.currItem = null;
                GameCanvas.panel.selected = -1;
            }
            // Thông tin mô tả (cp) có thể reset nếu không phải đang click vào chính nó
            GameCanvas.panel.cp = null;
        }
    }


    private static bool Hit(int px, int py, int x, int y, int w, int h)
    {
        return px >= x && px <= x + w && py >= y && py <= y + h;
    }

    private static void PaintOriginalItemDetail(mGraphics g)
    {
        if (GameCanvas.panel == null || selectedItemInfo == null)
        {
            return;
        }
        GameCanvas.panel.paintDetail(g);
    }

    private static void StartPetBodyItemMenu(Item item, int petBodyIndex)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.type = 21;
        panel.currentTabIndex = 0;
        panel.selected = petBodyIndex;
        panel.currItem = item;
        MyVector actions = new MyVector();
        actions.addElement(new Command(mResources.MOVEOUT, panel, 2006, item));
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartOriginalItemMenu(Item item, bool isBodySlot, int inventorySelected)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.customSelectInventoryItem(inventorySelected);
        panel.currItem = item;
        MyVector actions = new MyVector();
        if (isBodySlot)
        {
            actions.addElement(new Command(mResources.GETOUT, panel, 2002, item));
        }
        else if (item.isTypeBody())
        {
            actions.addElement(new Command(mResources.USE, panel, 2000, item));
            if (Char.myCharz().havePet)
            {
                actions.addElement(new Command(mResources.MOVEFORPET, panel, 2005, item));
            }
            if (Char.myCharz().havePet2)
            {
                actions.addElement(new Command(ModFunc.strUseForPet2, panel, 2007, item));
            }
        }
        else
        {
            actions.addElement(new Command(mResources.USE, panel, 2001, item));
            if (Char.myCharz().havePet)
            {
                actions.addElement(new Command(mResources.MOVEFORPET, panel, 2005, item));
            }
            if (Char.myCharz().havePet2)
            {
                actions.addElement(new Command(ModFunc.strUseForPet2, panel, 2007, item));
            }
        }
        actions.addElement(new Command(mResources.THROW, panel, 2003, item));
        Char.myCharz().setPartTemp(item.headTemp, item.bodyTemp, item.legTemp, item.bagTemp);
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartOriginalBoxMenu(Item item, int boxIndex)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.customSelectInventoryItem(boxIndex);
        panel.currItem = item;
        MyVector actions = new MyVector();
        actions.addElement(new Command(mResources.GETOUT, panel, 1000, item));
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartAutoItemMenu(Item item, int bagIndex)
    {
        if (item == null || item.template == null)
        {
            return;
        }
        AutoItem.ItemAuto auto = new AutoItem.ItemAuto(item.template.id, (short)bagIndex, item.getFullName());
        MyVector actions = new MyVector();
        if (AutoItem.mAutoItem.method_1(item.template.id))
        {
            actions.addElement(new Command("Tắt Auto", AutoItem.mAutoItem, 2, item.template.id));
        }
        else
        {
            actions.addElement(new Command("Auto dùng", AutoItem.mAutoItem, 1, auto));
        }
        actions.addElement(new Command("Bán", AutoItem.mAutoItem, 3, new AutoItem.ItemAuto(item.template.id, (short)bagIndex, false, true)));
        if (GameCanvas.panel != null)
        {
            GameCanvas.panel.currItem = item;
            GameCanvas.panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        }
        StartItemActionMenu(actions);
    }

    private static void StartItemActionMenu(MyVector actions)
    {
        // Tính vị trí menu gắn với khung thông tin item (cp)
        Panel panel = GameCanvas.panel;
        int menuTotalW = actions.size() * 60;
        int menuX;
        int menuY;
        if (panel != null && panel.cp != null)
        {
            // Đặt menu ngay bên dưới khung thông tin item, canh giữa theo khung cp
            menuX = panel.cp.cx + panel.cp.sayWidth / 2 - menuTotalW / 2;
            menuY = panel.cp.cy + panel.cp.ch + 2;
        }
        else
        {
            // Fallback: đặt theo vị trí item slot
            int itemSlotH = (selectedBodyIndex >= 0) ? 24 : 26;
            menuX = selectedItemX + 17 - menuTotalW / 2;
            menuY = selectedItemY + itemSlotH + 2;
        }
        // Clamp menuX trong panel
        if (menuX < panelX + 4)
        {
            menuX = panelX + 4;
        }
        if (menuX + menuTotalW > panelX + panelW - 4)
        {
            menuX = panelX + panelW - 4 - menuTotalW;
        }
        GameCanvas.menu.startAt(actions, menuX, menuY);
    }

    private static void UpdateSelectedItemPosition()
    {
        if (GameCanvas.panel == null || GameCanvas.panel.cp == null || selectedItemInfo == null)
        {
            return;
        }

        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightY = panelY + 66;
        int gap = 4;
        int newY = -1;

        if (topTab == 4)
        {
            if (rightSubTab == 0 && selectedBagIndex >= 0)
            {
                int col = selectedBagIndex % 6;
                int row = selectedBagIndex / 6;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
            else if (rightSubTab == 1 && selectedBoxIndex >= 0)
            {
                int col = selectedBoxIndex % 6;
                int row = selectedBoxIndex / 6;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
            else if (rightSubTab == 2 && selectedAutoIndex >= 0)
            {
                int col = selectedAutoIndex % 6;
                int row = selectedAutoIndex / 6;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
        }
        else if (topTab == 3 && selectedSkillIndex >= 0)
        {
            int colW = (safeW - 6) / 3;
            int col = selectedSkillIndex / 6;
            int row = selectedSkillIndex % 6;
            selectedItemX = safeX + col * (colW + 3) + 3;
            newY = panelY + 52 + row * 40 - skillScrollY;
        }

        if (newY != -1)
        {
            selectedItemY = newY;
            int y = selectedItemY - GameCanvas.panel.cp.ch / 2;
            if (y < panelY + 28)
            {
                y = panelY + 28;
            }
            GameCanvas.panel.cp.cy = y;
            // Cập nhật vị trí menu button theo khung cp
            if (GameCanvas.menu != null && GameCanvas.menu.showMenu)
            {
                int menuTotalW = GameCanvas.menu.menuItems.size() * 60;
                GameCanvas.menu.menuX = GameCanvas.panel.cp.cx + GameCanvas.panel.cp.sayWidth / 2 - menuTotalW / 2;
                if (GameCanvas.menu.menuX < panelX + 4)
                {
                    GameCanvas.menu.menuX = panelX + 4;
                }
                if (GameCanvas.menu.menuX + menuTotalW > panelX + panelW - 4)
                {
                    GameCanvas.menu.menuX = panelX + panelW - 4 - menuTotalW;
                }
                GameCanvas.menu.menuY = GameCanvas.panel.cp.cy + GameCanvas.panel.cp.ch + 2;
            }
        }
    }

    private static void PaintTheme(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 18;
        int safeW = panelW - 48;
        int safeH = panelH - 36;
        int splitX = safeX + safeW / 2;
        int bodyY = safeY + 54;
        int bodyH = safeH - 62;

        if (!PaintPanelBg(g) && !PaintPanelFrame9(g))
        {
            Fill(g, panelX + 3, panelY + 4, panelW, panelH, 0x2D1B0B);
            Fill(g, panelX, panelY, panelW, panelH, 0x6D431B);
            Fill(g, panelX + 2, panelY + 2, panelW - 4, panelH - 4, 0xF5D6A3);
            Fill(g, panelX + 8, panelY + 30, panelW - 16, panelH - 39, 0xFFE8BF);
        }

        PaintTopTabs(g);
        if (topTab == 4)
        {
            PaintSubTabs(g);
            PaintTitleBars(g);
        }
        PaintCloseButton(g);
    }

    private static void PaintTopTabs(mGraphics g)
    {
        int[] visibleTabs = GetVisibleTabs();
        int tabW = 60;
        int tabH = 20;
        int gap = 1;
        int totalTabsW = visibleTabs.Length * tabW + (visibleTabs.Length - 1) * gap;
        int tabX = panelX + (panelW - totalTabsW) / 2;
        int tabY = panelY + 22;
        for (int vi = 0; vi < visibleTabs.Length; vi++)
        {
            int logicalIndex = visibleTabs[vi];
            int x = tabX + vi * (tabW + gap);
            bool active = logicalIndex == topTab;
            
            // Vẽ nền và viền đồng nhất cho tất cả các tab
            int bgColor = active ? 0xBEEA8D : 0xF8DDA8;
            int borderColor = active ? 0x4C9D27 : 0x7B4B1F;
            
            // Vẽ viền bo góc bằng fillRect lớn (giả lập drawRect bo góc)
            g.setColor(borderColor);
            g.fillRect(x, tabY, tabW, tabH, 5);
            
            // Vẽ nền nhỏ hơn 1px để hiện viền
            g.setColor(bgColor);
            g.fillRect(x + 1, tabY + 1, tabW - 2, tabH - 2, 5);

            if (active)
            {
                g.setColor(SELECT_BORDER);
                g.fillRect(x + 1, tabY + 1, tabW - 2, tabH - 2, 5);
                g.setColor(SELECT_BG);
                g.fillRect(x + 2, tabY + 2, tabW - 4, tabH - 4, 5);
            }

            string label = TOP_TABS[logicalIndex];
            int textY = tabY + (tabH - mFont.tahoma_7b_dark.getHeight()) / 2;
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, tabW - 6), x + tabW / 2, textY, mFont.CENTER);
        }
    }

    private static void PaintSubTabs(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int y = panelY + 44;
        int slot = 26;
        int gap = 4;
        int gridW = 34 * 6 + gap * 5;
        int tw = gridW / RIGHT_TABS.Length;
        for (int i = 0; i < RIGHT_TABS.Length; i++)
        {
            int x = rightX + i * tw;
            bool active = i == rightSubTab;
            Image tabImg = GetTabImage(active, true);
            if (tabImg != null)
            {
                g.drawImage(tabImg, x, y, 0);
            }
            else
            {
                Fill(g, x, y, tw - 2, 18, active ? 0xDFF6FF : 0xF8DDA8);
                g.setColor(active ? 0x2B8AA8 : 0xA36B2E);
                g.drawRect(x + 1, y + 1, tw - 4, 16);
            }
            mFont.tahoma_7b_dark.drawString(g, RIGHT_TABS[i], x + tw / 2, y + 4, mFont.CENTER);
        }
    }

    private static void PaintTitleBars(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int leftX = safeX;
        int leftW = safeW / 2 - 16;
        int statsFrameX = leftX - 3;
        int statsFrameY = panelY + 232;
        int statsFrameW = 247;

        PaintOldPanelBox(g, statsFrameX, statsFrameY, statsFrameW, 65);
        PaintCharacterStats(g, statsFrameX + 18, statsFrameY + 3, statsFrameW - 36);
        PaintInventoryCurrency(g);
    }

    private static void PaintCharacterTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 42;
        int safeW = panelW - 48;
        int safeH = panelH - 82;
        int frameMargin = 2;
        int frameGap = 2;
        int frameOffsetY = 6;
        int topX = safeX + frameMargin;
        int topY = safeY + frameMargin + frameOffsetY;
        int topW = safeW - frameMargin * 2;
        int topH = 160;
        int infoX = topX;
        int infoY = topY + topH + frameGap;
        int infoW = topW;
        int infoH = safeH - topH - frameGap - frameMargin * 2;
        PaintOldPanelBox(g, topX, topY, topW, topH);
        PaintOldPanelBox(g, infoX, infoY, infoW, infoH - 28);

        Char me = Char.myCharz();
        if (me == null)
        {
            mFont.tahoma_7b_dark.drawString(g, "Nhân vật", topX + topW / 2, topY + 14, mFont.CENTER);
            return;
        }

        Item[] body = me.arrItemBody;
        int slotW = 36;
        int slotH = 24;
        int slotGapY = 27;
        int leftSlotX = topX + 22;
        int rightSlotX = topX + topW - 22 - slotW;
        int slotStartY = topY + 12;
        int centerX = topX + topW / 2;
        int previewY = topY + 104;

        mFont.tahoma_7b_dark.drawString(g, me.cName, centerX, topY + 10, mFont.CENTER);
            PaintCharacterPreview(g, me, centerX, previewY, false);

        for (int i = 0; i < 5; i++)
        {
            int y = slotStartY + i * slotGapY;
            bool isSelectedLeft = (selectedBodyIndex == i);
            PaintSlotRect(g, leftSlotX, y, slotW, slotH, isSelectedLeft);
            PaintItemInSlot(g, GetItem(body, i), leftSlotX, y, slotW, slotH, isSelectedLeft);

            int rightIndex = i + 5;
            bool isSelectedRight = (selectedBodyIndex == rightIndex);
            PaintSlotRect(g, rightSlotX, y, slotW, slotH, isSelectedRight);
            PaintItemInSlot(g, GetItem(body, rightIndex), rightSlotX, y, slotW, slotH, isSelectedRight);
        }

        int bottomY = topY + topH - 32;
        int bottomGap = 6;
        int bottomCount = 4;
        int bottomStartX = centerX - (bottomCount * slotW + (bottomCount - 1) * bottomGap) / 2;
        for (int i = 0; i < bottomCount; i++)
        {
            int bodyIndex = i + 10;
            int x = bottomStartX + i * (slotW + bottomGap);
            bool isSelected = (selectedBodyIndex == bodyIndex);
            PaintSlotRect(g, x, bottomY, slotW, slotH, isSelected);
            PaintItemInSlot(g, GetItem(body, bodyIndex), x, bottomY, slotW, slotH, isSelected);
        }

        mFont.tahoma_7b_dark.drawString(g, "THÔNG TIN NHÂN VẬT", infoX + infoW / 2, infoY + 7, mFont.CENTER);
        PaintCharacterStatsCompact(g, infoX + 16, infoY + 20, infoW - 32);
    }

    private static void PaintCharacterStatsCompact(mGraphics g, int x, int y, int w)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        mFont.tahoma_7b_dark.drawString(g, "SM: " + FormatStat(c.cPower), x + w / 2, y, mFont.CENTER);
        string[] labels = new string[] { "HP", "KI", "SĐ", "Giáp", "Crit", "Né", "TN", "Choáng", "Lạnh", "Hút HP", "Hút KI", "PST" };
        string[] values = new string[]
        {
            FormatStat(c.cHPFull),
            FormatStat(c.cMPFull),
            FormatStat(c.cDamFull),
            FormatStat(c.cDefull),
            c.cCriticalFull + "%",
            c.cMiss + "%",
            Res.formatNumber2(c.cLevelPercent / 100.0) + "%",
            c.khangTDHS ? "Có" : "Không",
            c.isKhongLanh ? "Có" : "Không",
            c.tlHutHp + "%",
            c.tlHutMp + "%",
            c.tlPst + "%"
        };
        int colW = w / 3;
        int rowH = 10;
        int startY = y + 12;
        for (int i = 0; i < labels.Length; i++)
        {
            int col = i / 4;
            int row = i % 4;
            int xx = x + col * colW;
            int yy = startY + row * rowH;
            string text = labels[i] + ": " + values[i];
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, text, colW - 4), xx, yy, mFont.LEFT);
        }
    }

    private static void PaintOldPanelBox(mGraphics g, int x, int y, int w, int h)
    {
        // Khung kiểu panel cũ: viền nâu nhẹ + nền sáng, bo góc 5px.
        g.setColor(9993045);
        g.fillRect(x, y, w, h, 5);
        g.setColor(15196114);
        g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
    }

    private static void PaintCharacterStats(mGraphics g, int x, int y, int w)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        int colW = w / 2;
        int rowH = 8;
        mFont.tahoma_7b_dark.drawString(g, "SM: " + FormatStat(c.cPower), x + w / 2, y, mFont.CENTER);
        int startY = y + 8;
        string[] leftLabel = new string[] { "HP", "KI", "SĐ", "Giáp", "Crit", "Né" };
        string[] leftValue = new string[]
        {
            FormatStat(c.cHPFull),
            FormatStat(c.cMPFull),
            FormatStat(c.cDamFull),
            FormatStat(c.cDefull),
            c.cCriticalFull + "%",
            c.cMiss + "%"
        };
        string[] rightLabel = new string[] { "TN", "Choáng", "Lạnh", "Hút HP", "Hút KI", "PST" };
        string[] rightValue = new string[]
        {
            Res.formatNumber2(c.cLevelPercent / 100.0) + "%",
            c.khangTDHS ? "Có" : "Không",
            c.isKhongLanh ? "Có" : "Không",
            c.tlHutHp + "%",
            c.tlHutMp + "%",
            c.tlPst + "%"
        };

        int leftLabelX = x;
        int leftValueX = x + 27;
        int rightLabelX = x + colW + 2;
        int rightValueX = rightLabelX + 31;
        for (int i = 0; i < 6; i++)
        {
            int yy = startY + i * rowH;
            mFont.tahoma_7b_dark.drawString(g, leftLabel[i] + ":", leftLabelX, yy, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, leftValue[i], leftValueX, yy, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, rightLabel[i] + ":", rightLabelX, yy, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, rightValue[i], rightValueX, yy, mFont.LEFT);
        }
    }

    private static string FormatStat(double value)
    {
        return NinjaUtil.getMoneys(value);
    }

    private static string FormatStat(long value)
    {
        return NinjaUtil.getMoneys(value);
    }

    private static string FormatGold(long value)
    {
        if (value >= 10000000000L)
        {
            string text = (value / 1000000000.0).ToString("0.###");
            return text.Replace('.', ',') + "b";
        }
        return FormatStat(value);
    }

    private static void PaintInventoryCurrency(mGraphics g)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int viewW = 34 * 6 + 4 * 5;
        int y = panelY + 66 + (26 * 7 + 4 * 6) + 8;
        int groupW = viewW / 3;
        PaintOldPanelBox(g, rightX - 5, y - 4, viewW + 10, 22);
        int x1 = rightX;
        int x2 = rightX + groupW + 20;
        int x3 = rightX + groupW * 2 + 26;
        DrawCurrency(g, Panel.imgXu, FormatGold(c.xu), x1 + 6, y + 2, 0xD6A000, 17, x2 - x1 - 22);
        DrawCurrency(g, Panel.imgLuong, FormatStat((long)c.luong), x2 + 1, y + 2, 0x1E9F4B, 18, x3 - x2 - 18);
        DrawCurrency(g, Panel.imgLuongKhoa, FormatStat((long)c.luongKhoa), x3 + 6, y + 2, 0xD03D7C, 15, panelX + panelW - 24 - x3 - 13);
    }

    private static void DrawCurrency(mGraphics g, Image icon, string text, int x, int y, int fallbackColor, int textOffset, int maxTextW)
    {
        if (icon != null)
        {
            g.drawImage(icon, x, y + 5, mGraphics.VCENTER | mGraphics.LEFT);
        }
        else
        {
            Fill(g, x, y + 1, 6, 6, fallbackColor);
            g.setColor(0x6B3A12);
            g.drawRect(x, y + 1, 5, 5);
        }
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, text, maxTextW), x + textOffset, y, mFont.LEFT);
    }

    private static void PaintCloseButton(mGraphics g)
    {
        int x = panelX + panelW - 2;
        int y = panelY + 10;

        if (closeImg != null)
            g.drawImage(closeImg, x-3, y+18, mGraphics.VCENTER | mGraphics.HCENTER);
    }

    private static void PaintSkillTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 42;
        int safeW = panelW - 48;
        int listY = panelY + 52;
        int listH = panelY + panelH - 36 - listY;
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        int colGap = 3;
        int colW = (safeW - colGap * 2) / 3;
        int columnsX = safeX;
        int columnsY = listY - 4;
        int columnsH = (listH + 8 < 248) ? (listH + 8) : 248;
        for (int i = 0; i < 3; i++)
        {
            PaintOldPanelBox(g, columnsX + i * (colW + colGap), columnsY, colW, columnsH);
        }
        g.setClip(safeX, listY, safeW, listH);
        PaintSkillList(g, Char.myCharz(), safeX, safeW, listY, listY, listY + listH);
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
        PaintBagScrollBar(g, safeX + safeW - 8, listY, listH, GetSkillMaxScroll(listH));
    }

    private static void PaintSkillList(mGraphics g, Char ch, int x, int w, int y, int top, int bottom)
    {
        int count = GetFullSkillCount(ch);
        if (ch == null || count == 0)
        {
            mFont.tahoma_7b_dark.drawString(g, "Chưa có kỹ năng", x + w / 3, top + 30, mFont.CENTER);
            return;
        }
        int colGap = 3;
        int colW = (w - colGap * 2) / 3;
        int cellW = colW;
        int rowH = 40;
        for (int i = 0; i < count; i++)
        {
            int col = i / 6;
            int row = i % 6;
            int boxX = x + col * (colW + colGap);
            int xx = boxX + 3;
            int yy = y + row * rowH - skillScrollY;
            if (yy + 40 < top)
            {
                continue;
            }
            if (yy > bottom)
            {
                break;
            }
            if (i < 6)
            {
                PaintSkillStatCell(g, i, xx, yy, cellW, 40, selectedSkillIndex == i);
                continue;
            }
            SkillTemplate template = GetSkillTemplateAt(ch, i);
            Skill skill = GetLearnedSkill(ch, template);
            if (template == null && skill == null)
            {
                continue;
            }
            PaintSkillCell(g, skill, template, xx, yy, cellW, 40, selectedSkillIndex == i);
        }
    }

    private static void PaintSkillCellFrame(mGraphics g, int x, int y, int w, int h, bool special)
    {
        int textX = x + 29;
        int textW = w - 35;
        g.setColor(special ? 0xD8A33E : 0xC89A55);
        g.fillRect(textX, y + 3, textW, h - 6, 5);
        g.setColor(special ? 0xFFF6C4 : 0xFFF1CF);
        g.fillRect(textX + 1, y + 4, textW - 2, h - 8, 5);
        if (GameScr.imgSkill != null)
        {
            g.drawImage(GameScr.imgSkill, x, y + (h - 28) / 2, 0);
        }
        else
        {
            Fill(g, x, y + 6, 28, 28, 0xF3D18A);
        }
    }

    private static void PaintSkillStatCell(mGraphics g, int type, int x, int y, int w, int h, bool selected)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        PaintSkillCellFrame(g, x, y, w, h, type == 5 || selected);
        string title = string.Empty;
        string info = string.Empty;
        int icon = 567;
        if (type == 0)
        {
            icon = 567;
            title = "HP gốc: " + NinjaUtil.getMoneys(c.cHPGoc);
            info = NinjaUtil.getMoneys(c.cHPGoc + 1000L) + " tiềm năng: tăng " + c.hpFrom1000TiemNang;
        }
        else if (type == 1)
        {
            icon = 569;
            title = "KI gốc: " + NinjaUtil.getMoneys(c.cMPGoc);
            info = NinjaUtil.getMoneys(c.cMPGoc + 1000L) + " tiềm năng: tăng " + c.mpFrom1000TiemNang;
        }
        else if (type == 2)
        {
            icon = 568;
            title = "Sức đánh gốc: " + NinjaUtil.getMoneys(c.cDamGoc);
            info = NinjaUtil.getMoneys(c.cDamGoc * 100L) + " tiềm năng: tăng " + c.damFrom1000TiemNang;
        }
        else if (type == 3)
        {
            icon = 721;
            title = "Giáp gốc: " + NinjaUtil.getMoneys((long)c.cDefGoc);
            info = NinjaUtil.getMoneys((long)(500000 + c.cDefGoc * 100000)) + " tiềm năng: tăng " + c.defFrom1000TiemNang;
        }
        else if (type == 4)
        {
            icon = 719;
            title = "Chí mạng gốc: " + c.cCriticalGoc + "%";
            int idx = c.cCriticalGoc;
            if (idx > Panel.t_tiemnang.Length - 1)
            {
                idx = Panel.t_tiemnang.Length - 1;
            }
            info = Res.formatNumber2(Panel.t_tiemnang[idx]) + " tiềm năng: tăng " + c.criticalFrom1000Tiemnang;
        }
        else
        {
            icon = Panel.spearcialImage;
            title = (Panel.specialInfo != null && Panel.specialInfo.Length > 0) ? Panel.specialInfo : "Nội tại";
            info = string.Empty;
        }
        SmallImage.drawSmallImage(g, icon, x + 4, y + (h - 20) / 2, 0, 0);
        
        int infoLines = 0;
        string[] wrapped = null;
        if (info.Length > 0)
        {
            wrapped = mFont.tahoma_7_green2.splitFontArray(info, w - 43);
            infoLines = (wrapped.Length < 2) ? wrapped.Length : 2;
        }
        int totalLines = 1 + infoLines;
        int textY = y + (h - totalLines * 11) / 2;
        
        mFont.tahoma_7b_blue.drawString(g, TrimText(mFont.tahoma_7b_blue, title, w - 43), x + 33, textY, mFont.LEFT);
        if (infoLines > 0)
        {
            for (int i = 0; i < infoLines; i++)
            {
                mFont.tahoma_7_green2.drawString(g, wrapped[i], x + 33, textY + 11 + i * 11, mFont.LEFT);
            }
        }
    }

    private static SkillTemplate GetSkillTemplateAt(Char ch, int index)
    {
        if (ch == null)
        {
            return null;
        }
        index -= 6;
        if (index < 0)
        {
            return null;
        }
        if (ch.nClass != null && ch.nClass.skillTemplates != null && index >= 0 && index < ch.nClass.skillTemplates.Length)
        {
            return ch.nClass.skillTemplates[index];
        }
        if (ch.vSkill != null && index >= 0 && index < ch.vSkill.size())
        {
            Skill skill = (Skill)ch.vSkill.elementAt(index);
            return (skill != null) ? skill.template : null;
        }
        return null;
    }

    private static Skill GetLearnedSkill(Char ch, SkillTemplate template)
    {
        if (ch == null || template == null || ch.vSkill == null)
        {
            return null;
        }
        for (int i = 0; i < ch.vSkill.size(); i++)
        {
            Skill skill = (Skill)ch.vSkill.elementAt(i);
            if (skill != null && skill.template != null && skill.template.id == template.id)
            {
                return skill;
            }
        }
        return null;
    }

    private static void PaintSkillCell(mGraphics g, Skill skill, SkillTemplate template, int x, int y, int w, int h, bool selected)
    {
        PaintSkillCellFrame(g, x, y, w, h, selected);
        if (skill != null)
        {
            skill.paint(x + 15, y + h / 2, g);
        }
        else if (template != null)
        {
            SmallImage.drawSmallImage(g, template.iconId, x + 15, y + h / 2, 0, StaticObj.VCENTER_HCENTER);
        }
        string name = (template != null) ? template.name : ((skill != null && skill.template != null) ? skill.template.name : "Skill");
        string info = (skill != null) ? ("Lv " + skill.point + " KI " + skill.manaUse + " CD " + skill.strTimeReplay() + "s") : "Chưa học";
        
        int textY = y + (h - 22) / 2;
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, name, w - 43), x + 33, textY, mFont.LEFT);
        mFont.tahoma_7_grey.drawString(g, TrimText(mFont.tahoma_7_grey, info, w - 43), x + 33, textY + 11, mFont.LEFT);
    }

    private static void PaintSkillCell(mGraphics g, Skill skill, int x, int y, int w, int h)
    {
        PaintSkillCell(g, skill, (skill != null) ? skill.template : null, x, y, w, h, false);
    }

    private static void PaintModTab(mGraphics g)
    {
        Panel p = GameCanvas.panel;
        if (p == null)
        {
            return;
        }
        if (p.type != 26)
        {
            p.SetTypeModFunc();
        }
        SoundMn.gI().GetStrModFunc();

        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 118;


        int gap = 6;
        int catW = 126;
        int listW = safeW - catW - gap;
        int catX = safeX;
        int listX = catX + catW + gap;

        PaintOldPanelBox(g, catX, safeY, catW, safeH);
        PaintOldPanelBox(g, listX, safeY, listW, safeH);
        mFont.tahoma_7b_dark.drawString(g, "NHÓM MOD", catX + catW / 2, safeY + 6, mFont.CENTER);
        mFont.tahoma_7b_dark.drawString(g, "CHỨC NĂNG MOD", listX + listW / 2, safeY + 6, mFont.CENTER);

        PaintModCategoryList(g, catX + 6, safeY + 24, catW - 12, safeH - 30);
        PaintModFunctionList(g, listX + 6, safeY + 24, listW - 12, safeH - 30);
    }

    private static void PaintModCategoryList(mGraphics g, int x, int y, int w, int h)
    {
        string[][] tabs = Panel.boxMod;
        if (tabs == null)
        {
            return;
        }
        int rowH = 29;
        for (int i = 0; i < tabs.Length; i++)
        {
            int yy = y + i * rowH;
            bool selected = GameCanvas.panel != null && i == GameCanvas.panel.currentTabIndex;
            PaintOldTextCell(g, x, yy, w, rowH - 4, selected);
            string line1 = tabs[i][0];
            string line2 = (tabs[i].Length > 1) ? tabs[i][1] : string.Empty;
            mFont font = selected ? mFont.tahoma_7b_green2 : mFont.tahoma_7b_dark;
            string title = line1 + " " + line2;
            font.drawString(g, title, x + w / 2, yy + 8, mFont.CENTER);
        }
    }

    private static void PaintModFunctionList(mGraphics g, int x, int y, int w, int h)
    {
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        g.setClip(x, y, w, h);
        int rowH = 26;
        if (Panel.strModFunc == null || Panel.strModFunc.Length == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Chưa có chức năng", x + w / 2, y + 35, mFont.CENTER);
        }
        else
        {
            for (int i = 0; i < Panel.strModFunc.Length; i++)
            {
                int yy = y + i * rowH;
                if (yy > y + h)
                {
                    break;
                }
                string raw = Panel.strModFunc[i];
                bool enabled = raw != null && raw.StartsWith("[x]");
                string label = CleanModLabel(raw);
                PaintOldTextCell(g, x, yy, w, rowH - 4, i == selectedAutoIndex);
                PaintModToggle(g, x + 7, yy + 6, enabled);
                mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, w - 48), x + 32, yy + 6, mFont.LEFT);
            }
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }

    private static void PaintOldTextCell(mGraphics g, int x, int y, int w, int h, bool selected)
    {
        g.setColor(selected ? SELECT_BORDER : 9993045);
        g.fillRect(x, y, w, h, 5);
        g.setColor(selected ? SELECT_BG : 15196114);
        g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
    }

    private static void PaintModToggle(mGraphics g, int x, int y, bool enabled)
    {
        g.setColor(enabled ? 4825130 : 9671571);
        g.fillRect(x, y, 18, 10, 5);
        g.setColor(0xFFFFFF);
        g.fillRect(enabled ? x + 9 : x + 1, y + 1, 8, 8, 4);
    }

    private static string CleanModLabel(string raw)
    {
        if (raw == null)
        {
            return string.Empty;
        }
        if (raw.StartsWith("[x]") || raw.StartsWith("[  ]"))
        {
            return raw.Substring(raw.IndexOf(']') + 1).Trim();
        }
        return raw.Trim();
    }

    private static bool TryHandleModClick(bool isFire)
    {
        Panel p = GameCanvas.panel;
        if (p == null)
        {
            return false;
        }
        if (p.type != 26)
        {
            if (isFire) p.SetTypeModFunc();
            else return true;
        }
        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 88;
        int gap = 6;
        int catW = 126;
        int listW = safeW - catW - gap;
        int catX = safeX + 6;
        int catY = safeY + 24;
        int catInnerW = catW - 12;
        int listX = safeX + catW + gap + 6;
        int listY = safeY + 24;
        int listInnerW = listW - 12;
        int innerH = safeH - 30;

        if (GameCanvas.px >= catX && GameCanvas.px <= catX + catInnerW && GameCanvas.py >= catY && GameCanvas.py <= catY + innerH)
        {
            int row = (GameCanvas.py - catY) / 29;
            if (Panel.boxMod != null && row >= 0 && row < Panel.boxMod.Length)
            {
                if (!isFire) return true;
                p.currentTabIndex = row;
                p.selected = GameCanvas.isTouch ? -1 : 0;
                selectedAutoIndex = -1;
                SoundMn.gI().GetStrModFunc();
                SoundMn.gI().panelClick();
                return true;
            }
        }

        if (GameCanvas.px >= listX && GameCanvas.px <= listX + listInnerW && GameCanvas.py >= listY && GameCanvas.py <= listY + innerH)
        {
            int row = (GameCanvas.py - listY) / 26;
            if (Panel.strModFunc != null && row >= 0 && row < Panel.strModFunc.Length)
            {
                selectedAutoIndex = row;
                p.selected = row;
                if (isFire)
                {
                    p.DoFireModFunc();
                    SoundMn.gI().panelClick();
                }
                return true;
            }
        }
        return false;
    }

    private static readonly string[] TOOL_GROUPS = new string[] { "Hệ thống", "Di chuyển", "Giao tiếp", "Tài khoản" };

    private static void PaintToolTab(mGraphics g)
    {
        Panel p = GameCanvas.panel;
        if (p == null)
        {
            return;
        }
        if (selectedToolAction < 0)
        {
            SoundMn.gI().getSoundOption();
            if (selectedToolGroupIndex == 3 && (Panel.strAccount == null || Panel.strAccount.Length == 0))
                p.setTypeAccount();
        }
        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 118;
        int gap = 6;
        bool showDetail = selectedToolAction >= 0;
        if (selectedToolGroupIndex == 3)
            showDetail = (selectedAccountSubAction == 1 || selectedAccountSubAction == 2);
        int catW = 108;
        int detailW = showDetail ? 164 : 0;
        int listW = safeW - catW - gap - (showDetail ? detailW + gap : 0);
        int catX = safeX;
        int listX = catX + catW + gap;
        int detailX = listX + listW + gap;

        PaintOldPanelBox(g, catX, safeY, catW, safeH);
        PaintOldPanelBox(g, listX, safeY, listW, safeH);
        if (showDetail)
        {
            PaintOldPanelBox(g, detailX, safeY, detailW, safeH);
        }
        mFont.tahoma_7b_dark.drawString(g, "NHÓM", catX + catW / 2, safeY + 6, mFont.CENTER);
        mFont.tahoma_7b_dark.drawString(g, "CHỨC NĂNG", listX + listW / 2, safeY + 6, mFont.CENTER);
        PaintToolGroups(g, catX + 6, safeY + 24, catW - 12, safeH - 30);
        PaintToolList(g, listX + 6, safeY + 24, listW - 12, safeH - 30);
        if (showDetail)
        {
            mFont.tahoma_7b_dark.drawString(g, GetToolDetailTitle(), detailX + detailW / 2, safeY + 6, mFont.CENTER);
            PaintToolDetail(g, detailX + 6, safeY + 24, detailW - 12, safeH - 30);
        }
    }

    private static void PaintToolGroups(mGraphics g, int x, int y, int w, int h)
    {
        int rowH = 29;
        for (int i = 0; i < TOOL_GROUPS.Length; i++)
        {
            int yy = y + i * rowH;
            bool selected = i == selectedToolGroupIndex;
            PaintOldTextCell(g, x, yy, w, rowH - 4, selected);
            mFont font = selected ? mFont.tahoma_7b_green2 : mFont.tahoma_7b_dark;
            font.drawString(g, TOOL_GROUPS[i], x + w / 2, yy + 8, mFont.CENTER);
        }
    }

    private static void PaintToolList(mGraphics g, int x, int y, int w, int h)
    {
        int rowH = 26;
        // Group 3 (Tài khoản): hiện trực tiếp các mục từ Panel.strAccount + các action bổ sung (9, 10, 11)
        if (selectedToolGroupIndex == 3)
        {
            MyVector items = new MyVector();
            MyVector actions = new MyVector();
            
            if (Panel.strAccount != null)
            {
                for (int i = 0; i < Panel.strAccount.Length; i++)
                {
                    items.addElement(Panel.strAccount[i]);
                    actions.addElement(new int[] { i, -1 }); // { accountSubAction, toolAction }
                }
            }
            
            int[] extras = { 9, 10, 11 };
            foreach (int act in extras)
            {
                int idx = GetToolOriginalIndexByAction(act);
                if (idx >= 0)
                {
                    string l = GetToolOriginalLabel(idx);
                    if (!string.IsNullOrEmpty(l))
                    {
                        bool isDup = false;
                        if (Panel.strAccount != null)
                        {
                            for (int j = 0; j < Panel.strAccount.Length; j++)
                                if (Panel.strAccount[j] == l) isDup = true;
                        }
                        if (!isDup)
                        {
                            items.addElement(l);
                            actions.addElement(new int[] { -1, act });
                        }
                    }
                }
            }

            if (items.size() == 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Không có chức năng", x + w / 2, y + 35, mFont.CENTER);
                return;
            }

            for (int i = 0; i < items.size(); i++)
            {
                int yy = y + i * rowH;
                if (yy > y + h) break;
                
                string label = (string)items.elementAt(i);
                int[] actData = (int[])actions.elementAt(i);
                bool isSelected = (actData[0] >= 0) ? (selectedAccountSubAction == actData[0]) : (selectedToolAction == actData[1]);

                PaintOldTextCell(g, x, yy, w, rowH - 4, isSelected);
                mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, w - 18), x + 10, yy + 6, mFont.LEFT);
            }
            return;
        }
        int count = GetToolGroupCount(selectedToolGroupIndex);
        if (count == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Không có chức năng", x + w / 2, y + 35, mFont.CENTER);
            return;
        }
        for (int i = 0; i < count; i++)
        {
            int yy = y + i * rowH;
            if (yy > y + h)
            {
                break;
            }
            int originalIndex = GetToolOriginalIndex(selectedToolGroupIndex, i);
            int action = GetToolActionFromGroupRow(selectedToolGroupIndex, i);
            string label = (action == 100) ? "Chat thế giới" : GetToolOriginalLabel(originalIndex);
            PaintOldTextCell(g, x, yy, w, rowH - 4, selectedToolAction == action);
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, w - 18), x + 10, yy + 6, mFont.LEFT);
        }
    }

    private static int GetToolGroupCount(int group)
    {
        if (group == 0) return 3;
        if (group == 1) return 2;
        if (group == 2) return 1;
        if (group == 3)
        {
            int count = (Panel.strAccount != null) ? Panel.strAccount.Length : 0;
            int[] extras = { 9, 10, 11 };
            foreach (int act in extras)
            {
                int idx = GetToolOriginalIndexByAction(act);
                if (idx >= 0)
                {
                    string l = GetToolOriginalLabel(idx);
                    if (!string.IsNullOrEmpty(l))
                    {
                        bool isDup = false;
                        if (Panel.strAccount != null)
                        {
                            for (int j = 0; j < Panel.strAccount.Length; j++)
                                if (Panel.strAccount[j] == l) isDup = true;
                        }
                        if (!isDup) count++;
                    }
                }
            }
            return count;
        }
        return 0;
    }

    private static int GetToolOriginalIndex(int group, int row)
    {
        if (group == 0)
        {
            if (row == 0) return GetToolOriginalIndexByAction(0);
            if (row == 1) return GetToolOriginalIndexByAction(8);
            return GetToolOriginalIndexByAction(3);
        }
        if (group == 1)
        {
            if (row == 0) return GetToolOriginalIndexByAction(4);
            return GetToolOriginalIndexByAction(5);
        }
        if (group == 2)
        {
            return GetToolOriginalIndexByAction(6);
        }
        if (group == 3)
        {
            if (row == 0) return GetToolOriginalIndexByAction(7);
            if (row == 1) return GetToolOriginalIndexByAction(9);
            if (row == 2) return GetToolOriginalIndexByAction(10);
            return GetToolOriginalIndexByAction(11);
        }
        return -1;
    }

    private static int GetToolOriginalIndexByAction(int action)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return -1;
        }
        bool pet1 = c.havePet;
        bool pet2 = c.havePet2;
        if (!pet1 && !pet2)
        {
            if (action <= 3) return action;
            if (action >= 4 && action <= 10) return action;
            return -1;
        }
        if (pet1 && pet2)
        {
            if (action <= 3) return action;
            if (action >= 4 && action <= 10) return action + 2;
            if (action == 11) return 12;
            return -1;
        }
        if (action <= 3) return action;
        if (action >= 4 && action <= 10) return action + 1;
        if (action == 11) return 11;
        return -1;
    }

    private static string GetToolOriginalLabel(int originalIndex)
    {
        if (Panel.strTool == null || originalIndex < 0 || originalIndex >= Panel.strTool.Length)
        {
            return string.Empty;
        }
        return Panel.strTool[originalIndex];
    }

    private static int GetToolActionFromGroupRow(int group, int row)
    {
        if (group == 0)
        {
            if (row == 0) return 0;
            if (row == 1) return 8;
            return 3;
        }
        if (group == 1)
        {
            return row == 0 ? 4 : 5;
        }
        if (group == 2)
        {
            return 100;
        }
        if (group == 3)
        {
            if (row == 0) return 7;
            if (row == 1) return 9;
            if (row == 2) return 10;
            return 11;
        }
        return -1;
    }

    // Sub-action cho Tài khoản: khi chọn Bạn bè/Kẻ thù thì hiển thị danh sách trong box 3
    private static int selectedAccountSubAction = -1; // -1: chưa chọn, 1: bạn bè, 2: kẻ thù
    private static int friendEnemySelected = -1;
    private static int friendEnemyDragStartY;
    private static bool friendEnemyDragged;

    private static bool ToolActionHasDetail(int action)
    {
        if (action == 7 && selectedAccountSubAction > 0) return true;
        return action == 0 || action == 4 || action == 5 || action == 7 || action == 8;
    }

    private static string GetToolDetailTitle()
    {
        if (selectedToolAction == 0) return "THÔNG BÁO";
        if (selectedToolAction == 4) return "ĐỔI CỜ";
        if (selectedToolAction == 5) return "ĐỔI KHU";
        if (selectedToolAction == 7)
        {
            if (selectedAccountSubAction == 1) return "BẠN BÈ";
            if (selectedAccountSubAction == 2) return "KẺ THÙ";
            return "TÀI KHOẢN";
        }
        if (selectedToolAction == 8) return "CẤU HÌNH";
        return "CHI TIẾT";
    }

    private static void PaintToolDetail(mGraphics g, int x, int y, int w, int h)
    {
        Panel p = GameCanvas.panel;
        if (p == null) return;

        if (selectedToolAction == 0) PaintGameInfoDetailList(g, x, y, w, h);
        else if (selectedToolAction == 4) PaintFlagDetail(g, x, y, w, h);
        else if (selectedToolAction == 5) PaintZoneDetail(g, x, y, w, h);
        else if (selectedToolAction == 7)
        {
            if (selectedAccountSubAction == 1)
                PaintFriendEnemyDetail(g, x, y, w, h, p.vFriend, 0);
            else if (selectedAccountSubAction == 2)
                PaintFriendEnemyDetail(g, x, y, w, h, p.vEnemy, 1);
            else
                PaintStringArrayDetail(g, Panel.strAccount, x, y, w, h);
        }
        else if (selectedToolAction == 8) PaintStringArrayDetail(g, Panel.strCauhinh, x, y, w, h);
    }

    private static void PaintFriendEnemyDetail(mGraphics g, int x, int y, int w, int h, MyVector list, int feType)
    {
        if (list == null || list.size() == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Đang tải dữ liệu...", x + w / 2, y + h / 2, mFont.CENTER);
            return;
        }

        int rowH = 28;
        int iconColW = 24;
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();

        g.setClip(x, y, w, h);
        g.translate(0, -toolDetailScrollY);

        for (int i = 0; i < list.size(); i++)
        {
            InfoItem infoItem = (InfoItem)list.elementAt(i);
            int rowY = y + i * rowH;
            if (rowY + rowH < y + toolDetailScrollY || rowY > y + toolDetailScrollY + h) continue;

            int iconX = x;
            int textBgX = x + iconColW;
            int textBgW = w - iconColW;
            int rh = rowH - 1;

            g.setColor(i == friendEnemySelected ? 0x919100 : 0x989355);
            g.fillRect(iconX, rowY, iconColW, rh);
            g.setColor(i == friendEnemySelected ? 0xF9F5CA : 0xE7E3D2);
            g.fillRect(textBgX, rowY, textBgW, rh);

            if (infoItem.charInfo != null)
            {
                if (infoItem.charInfo.headICON != -1)
                {
                    SmallImage.drawSmallImage(g, infoItem.charInfo.headICON, iconX, rowY, 0, 0);
                }
                else
                {
                    Part part = GameScr.parts[infoItem.charInfo.head];
                    if (part != null && part.pi != null && part.pi.Length > 0)
                    {
                        SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id,
                            iconX + (int)part.pi[Char.CharInfo[0][0][0]].dx,
                            rowY + 3 + (int)part.pi[Char.CharInfo[0][0][0]].dy, 0, 0);
                    }
                }

                if (infoItem.isOnline)
                {
                    mFont.tahoma_7b_green.drawString(g, infoItem.charInfo.cName, textBgX + 5, rowY, 0);
                    mFont.tahoma_7_blue.drawString(g, infoItem.s, textBgX + 5, rowY + 11, 0);
                }
                else
                {
                    mFont.tahoma_7_grey.drawString(g, infoItem.charInfo.cName, textBgX + 5, rowY, 0);
                    mFont.tahoma_7_grey.drawString(g, infoItem.s, textBgX + 5, rowY + 11, 0);
                }
            }
        }

        g.translate(0, toolDetailScrollY);
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }

    private static void PaintFlagDetail(mGraphics g, int x, int y, int w, int h)
    {
        Panel p = GameCanvas.panel;
        if (p.vFlag == null || p.vFlag.size() == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Không có cờ", x + w / 2, y + 35, mFont.CENTER);
            return;
        }

        int rowH = 26;
        int startY = y;
        g.setClip(x, startY, w, h);
        g.translate(0, -toolDetailScrollY);
        
        for (int i = 0; i < p.vFlag.size(); i++)
        {
            int yy = startY + i * rowH;
            if (yy + rowH < startY + toolDetailScrollY || yy > startY + toolDetailScrollY + h) continue;
            
            bool isSelected = (selectedToolDetailIndex == i);
            
            g.setColor(isSelected ? SELECT_BG : 15723751);
            g.fillRect(x + 4, yy, w - 8, rowH - 2, 5);
            
            Item item = (Item)p.vFlag.elementAt(i);
            if (item != null)
            {
                SmallImage.drawSmallImage(g, (int)item.template.iconID, x + 16, yy + rowH / 2, 0, 3);
                mFont.tahoma_7b_dark.drawString(g, item.template.name, x + 32, yy + 2, mFont.LEFT);
                if (item.itemOption != null && item.itemOption.Length > 0)
                {
                    mFont.tahoma_7_blue.drawString(g, item.itemOption[0].getOptionString(), x + 32, yy + 12, mFont.LEFT);
                }
            }
        }
        g.translate(0, toolDetailScrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private static void PaintZoneDetail(mGraphics g, int x, int y, int w, int h)
    {
        int[] zones = GameScr.gI().zones;
        int[] pts = GameScr.gI().pts;
        if (zones == null || zones.Length == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Đang tải dữ liệu...", x + w / 2, y + 35, mFont.CENTER);
            return;
        }

        int rowH = 26;
        int startY = y;
        g.setClip(x, startY, w, h);
        g.translate(0, -toolDetailScrollY);
        
        for (int i = 0; i < zones.Length; i++)
        {
            int yy = startY + i * rowH;
            if (yy + rowH < startY + toolDetailScrollY || yy > startY + toolDetailScrollY + h) continue;
            
            bool isSelected = (selectedToolDetailIndex == i);
            
            g.setColor(isSelected ? SELECT_BG : 15723751);
            g.fillRect(x + 4, yy, w - 8, rowH - 2, 5);
            
            int color = 0x00FF00; // Green
            if (pts[i] == 1) color = 0xFFFF00; // Yellow
            else if (pts[i] == 2) color = 0xFF0000; // Red
            
            g.setColor(color);
            g.fillRect(x + 8, yy + 6, 12, 12, 5);
            
            mFont.tahoma_7b_dark.drawString(g, "Khu " + zones[i], x + 26, yy + 6, mFont.LEFT);
            string countStr = GameScr.gI().numPlayer[i] + "/" + GameScr.gI().maxPlayer[i];
            mFont.tahoma_7_blue.drawString(g, countStr, x + w - 10, yy + 6, mFont.RIGHT);
        }
        g.translate(0, toolDetailScrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private static void PaintGameInfoDetailList(mGraphics g, int x, int y, int w, int h)
    {
        int rowH = 26;
        if (Panel.vGameInfo == null || Panel.vGameInfo.size() == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Chưa có thông báo", x + w / 2, y + 35, mFont.CENTER);
            return;
        }
        g.setClip(x, y, w, h);
        g.translate(0, -toolDetailScrollY);
        for (int i = 0; i < Panel.vGameInfo.size(); i++)
        {
            int yy = y + i * rowH;
            if (yy + rowH < y + toolDetailScrollY || yy > y + toolDetailScrollY + h) continue;
            GameInfo info = (GameInfo)Panel.vGameInfo.elementAt(i);
            PaintOldTextCell(g, x, yy, w, rowH - 4, selectedToolDetailIndex == i);
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, info.main, w - 18), x + 10, yy + 6, mFont.LEFT);
            if (!info.hasRead && GameCanvas.gameTick % 20 > 10)
            {
                g.drawImage(Panel.imgNew, x + w - 12, yy + 11, 3);
            }
        }
        g.translate(0, toolDetailScrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private static void TryHandleGameInfoPopupClick(bool isFire)
    {
        if (selectedGameInfoPopup == null || !isFire) return;
        
        int w = 240;
        int popupH = 160;
        int cx = GameCanvas.w / 2;
        int cy = GameCanvas.h / 2;
        int popupX = cx - w / 2;
        int popupY = cy - popupH / 2;

        int closeBtnX = popupX + w - 16;
        int closeBtnY = popupY + 16;
        
        if (GameCanvas.px >= closeBtnX - 12 && GameCanvas.px <= closeBtnX + 12 && GameCanvas.py >= closeBtnY - 12 && GameCanvas.py <= closeBtnY + 12)
        {
            selectedGameInfoPopup = null;
            popupScrollY = 0;
            popupScrollTargetY = 0;
            popupScrollRun = 0;
            SoundMn.gI().panelClick();
            return;
        }
        
        if (GameCanvas.px < popupX || GameCanvas.px > popupX + w || GameCanvas.py < popupY || GameCanvas.py > popupY + popupH)
        {
            selectedGameInfoPopup = null;
            popupScrollY = 0;
            popupScrollTargetY = 0;
            popupScrollRun = 0;
            SoundMn.gI().panelClick();
            return;
        }
    }

    private static void PaintGameInfoPopup(mGraphics g)
    {
        if (selectedGameInfoPopup == null) return;
        
        int w = 240;
        int popupH = 160;
        int cx = GameCanvas.w / 2;
        int cy = GameCanvas.h / 2;
        int popupX = cx - w / 2;
        int popupY = cy - popupH / 2;

        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        int oldTx = g.getTranslateX();
        int oldTy = g.getTranslateY();

        PaintOldPanelBox(g, popupX, popupY, w, popupH);

        mFont.tahoma_7b_dark.drawString(g, "CHI TIẾT THÔNG BÁO", cx, popupY + 8, mFont.CENTER);

        if (closeImg != null)
        {
            g.drawImage(closeImg, popupX + w - 16, popupY + 16, 3);
        }

        int textX = popupX + 10;
        int textW = w - 20;
        int clipY = popupY + 30;
        int clipH = popupH - 40;

        g.setClip(popupX + 4, clipY, w - 8, clipH);
        g.translate(0, -popupScrollY);

        int currentY = clipY;
        string[] mainLines = mFont.tahoma_7b_dark.splitFontArray(selectedGameInfoPopup.main, textW);
        for (int i = 0; i < mainLines.Length; i++)
        {
            mFont.tahoma_7b_dark.drawString(g, mainLines[i], textX, currentY, mFont.LEFT);
            currentY += 12;
        }
        currentY += 4;
        string[] contentLines = mFont.tahoma_7.splitFontArray(selectedGameInfoPopup.content, textW);
        for (int i = 0; i < contentLines.Length; i++)
        {
            mFont.tahoma_7.drawString(g, contentLines[i], textX, currentY, mFont.LEFT);
            currentY += 12;
        }

        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
        g.translate(-g.getTranslateX() + oldTx, -g.getTranslateY() + oldTy);
    }

    private static void PaintStringArrayDetail(mGraphics g, string[] values, int x, int y, int w, int h)
    {
        int rowH = 26;
        if (values == null || values.Length == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Không có tuỳ chọn", x + w / 2, y + 35, mFont.CENTER);
            return;
        }
        g.setClip(x, y, w, h);
        g.translate(0, -toolDetailScrollY);
        for (int i = 0; i < values.Length; i++)
        {
            int yy = y + i * rowH;
            if (yy + rowH < y + toolDetailScrollY || yy > y + toolDetailScrollY + h) continue;
            string raw = values[i];
            bool hasToggle = raw != null && (raw.StartsWith("[x]") || raw.StartsWith("[  ]"));
            bool enabled = raw != null && raw.StartsWith("[x]");
            string label = hasToggle ? CleanModLabel(raw) : raw;
            PaintOldTextCell(g, x, yy, w, rowH - 4, selectedToolDetailIndex == i);
            if (hasToggle)
            {
                PaintModToggle(g, x + 7, yy + 6, enabled);
                mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, w - 48), x + 32, yy + 6, mFont.LEFT);
            }
            else
            {
                mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, label, w - 18), x + 10, yy + 6, mFont.LEFT);
            }
        }
        g.translate(0, toolDetailScrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private static bool TryHandleToolClick(bool isFire)
    {
        Panel p = GameCanvas.panel;
        if (p == null || globalDragged)
        {
            return false;
        }
        SoundMn.gI().getSoundOption();
        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 118;
        int gap = 6;
        bool showDetail = selectedToolAction >= 0;
        if (selectedToolGroupIndex == 3)
            showDetail = (selectedAccountSubAction == 1 || selectedAccountSubAction == 2);
        int catW = 108;
        int detailW = showDetail ? 164 : 0;
        int listW = safeW - catW - gap - (showDetail ? detailW + gap : 0);
        int catX = safeX;
        int listX = catX + catW + gap;
        int detailX = listX + listW + gap;

        if (GameCanvas.px >= catX && GameCanvas.px <= catX + catW && GameCanvas.py >= safeY && GameCanvas.py <= safeY + safeH)
        {
            int row = (GameCanvas.py - (safeY + 24)) / 29;
            if (row >= 0 && row < TOOL_GROUPS.Length)
            {
                if (!isFire) return true;
                selectedToolGroupIndex = row;
                if (row == 3) p.setTypeAccount();
                selectedToolAction = -1;
                selectedToolDetailIndex = -1;
                selectedAccountSubAction = -1;
                SoundMn.gI().panelClick();
                return true;
            }
        }
        else if (GameCanvas.px >= listX && GameCanvas.px <= listX + listW && GameCanvas.py >= safeY && GameCanvas.py <= safeY + safeH)
        {
            int row = (GameCanvas.py - (safeY + 24)) / 26;
            
            // Group 3 (Tài khoản): click trực tiếp vào các mục đã lọc
            if (selectedToolGroupIndex == 3)
            {
                MyVector actions = new MyVector();
                if (Panel.strAccount != null)
                {
                    for (int i = 0; i < Panel.strAccount.Length; i++)
                        actions.addElement(new int[] { i, -1 });
                }
                int[] extras = { 9, 10, 11 };
                foreach (int act in extras)
                {
                    int idx = GetToolOriginalIndexByAction(act);
                    if (idx >= 0)
                    {
                        string l = GetToolOriginalLabel(idx);
                        if (!string.IsNullOrEmpty(l))
                        {
                            bool isDup = false;
                            if (Panel.strAccount != null)
                            {
                                for (int j = 0; j < Panel.strAccount.Length; j++)
                                    if (Panel.strAccount[j] == l) isDup = true;
                            }
                            if (!isDup) actions.addElement(new int[] { -1, act });
                        }
                    }
                }

                if (row >= 0 && row < actions.size())
                {
                    if (!isFire) return true;
                    int[] actData = (int[])actions.elementAt(row);
                    
                    if (actData[0] >= 0) // Mục trong strAccount
                    {
                        int subAct = actData[0];
                        if (subAct == 1) // Bạn bè
                        {
                            selectedAccountSubAction = subAct;
                            selectedToolAction = 7;
                            toolDetailScrollY = 0;
                            toolDetailScrollTargetY = 0;
                            friendEnemySelected = -1;
                            if (p.vFriend == null || p.vFriend.size() == 0)
                                Service.gI().friend(0, -1);
                        }
                        else if (subAct == 2) // Kẻ thù
                        {
                            selectedAccountSubAction = subAct;
                            selectedToolAction = 7;
                            toolDetailScrollY = 0;
                            toolDetailScrollTargetY = 0;
                            friendEnemySelected = -1;
                            if (p.vEnemy == null || p.vEnemy.size() == 0)
                                Service.gI().enemy(0, -1);
                        }
                        else
                        {
                            selectedAccountSubAction = subAct;
                            selectedToolAction = -1;
                            p.selected = subAct;
                            p.doFireAccount();
                            if (subAct == 4) Hide(); // Chỉ đóng khi nạp tiền
                        }
                    }
                    else // Extra action (9, 10, 11)
                    {
                        int action = actData[1];
                        int origIdx = GetToolOriginalIndexByAction(action);
                        selectedAccountSubAction = -1;
                        selectedToolAction = action;
                        p.selected = origIdx;
                        p.doFireTool();
                    }
                    SoundMn.gI().panelClick();
                    return true;
                }
                return false;
            }

            int originalIndex = GetToolOriginalIndex(selectedToolGroupIndex, row);
            if (originalIndex >= 0 && Panel.strTool != null && originalIndex < Panel.strTool.Length)
            {
                if (!isFire) return true;
                int action = GetToolActionFromGroupRow(selectedToolGroupIndex, row);
                selectedToolDetailIndex = -1;
                if (ToolActionHasDetail(action))
                {
                    selectedToolAction = action;
                    selectedAccountSubAction = -1;
                    toolDetailScrollY = 0;
                    toolDetailScrollTargetY = 0;
                    toolDetailScrollRun = 0;
                    if (action == 0)
                    {
                        p.selected = -1;
                    }
                    else if (action == 4)
                    {
                        suppressFlagUI = true;
                        Service.gI().getFlag(0, 0);
                    }
                    else if (action == 5)
                    {
                        Service.gI().openUIZone();
                    }
                    else if (action == 8)
                    {
                        p.setTypeOption();
                        SoundMn.gI().getStrOption();
                    }
                    SoundMn.gI().panelClick();
                    return true;
                }
                if (action == 100)
                {
                    ChatLogPopup.gI().Toggle();
                    SoundMn.gI().panelClick();
                    return true;
                }
                selectedToolAction = -1;
                p.selected = originalIndex;
                p.doFireTool();
                if (action == 3) ClosePanelState(false);
                SoundMn.gI().panelClick();
                return true;
            }
        }
        else if (showDetail && GameCanvas.px >= detailX && GameCanvas.px <= detailX + detailW && GameCanvas.py >= safeY && GameCanvas.py <= safeY + safeH)
        {
            int feRowH = (selectedToolAction == 7 && selectedAccountSubAction > 0) ? 28 : 26;
            int scrollOffset = (selectedToolAction == 7 && selectedAccountSubAction > 0) ? toolDetailScrollY : toolDetailScrollY;
            int row = (GameCanvas.py + scrollOffset - (safeY + 24)) / feRowH;
            
            int maxRow = 0;
            if (selectedToolAction == 0 && Panel.vGameInfo != null) maxRow = Panel.vGameInfo.size();
            else if (selectedToolAction == 4 && p.vFlag != null) maxRow = p.vFlag.size();
            else if (selectedToolAction == 5 && GameScr.gI().zones != null) maxRow = GameScr.gI().zones.Length;
            else if (selectedToolAction == 7 && selectedAccountSubAction == 1 && p.vFriend != null) maxRow = p.vFriend.size();
            else if (selectedToolAction == 7 && selectedAccountSubAction == 2 && p.vEnemy != null) maxRow = p.vEnemy.size();
            else if (selectedToolAction == 7 && selectedAccountSubAction <= 0 && Panel.strAccount != null) maxRow = Panel.strAccount.Length;
            else if (selectedToolAction == 8 && Panel.strCauhinh != null) maxRow = Panel.strCauhinh.Length;

            if (row >= 0 && row < maxRow)
            {
                selectedToolDetailIndex = row;
                if (!isFire) return true;
                
                if (selectedToolAction == 0)
                {
                    p.selected = row;
                    p.doFireGameInfo();
                    selectedGameInfoPopup = (GameInfo)Panel.vGameInfo.elementAt(row);
                    popupScrollY = 0;
                    popupScrollTargetY = 0;
                    popupScrollRun = 0;
                    SoundMn.gI().panelClick();
                    return true;
                }
                if (selectedToolAction == 4)
                {
                    Service.gI().getFlag(1, (sbyte)row);
                    SoundMn.gI().panelClick();
                    return true;
                }
                if (selectedToolAction == 5)
                {
                    Service.gI().requestChangeZone(row, -1);
                    SoundMn.gI().panelClick();
                    return true;
                }
                if (selectedToolAction == 7)
                {
                    if (selectedAccountSubAction <= 0)
                    {
                        // Đang ở danh sách chính Tài khoản
                        if (row == 1)
                        {
                            selectedAccountSubAction = 1; // Bạn bè
                            toolDetailScrollY = 0;
                            toolDetailScrollTargetY = 0;
                            friendEnemySelected = -1;
                            if (p.vFriend == null || p.vFriend.size() == 0)
                                Service.gI().friend(0, -1);
                        }
                        else if (row == 2)
                        {
                            selectedAccountSubAction = 2; // Kẻ thù
                            toolDetailScrollY = 0;
                            toolDetailScrollTargetY = 0;
                            friendEnemySelected = -1;
                            if (p.vEnemy == null || p.vEnemy.size() == 0)
                                Service.gI().enemy(0, -1);
                        }
                        else
                        {
                            p.selected = row;
                            p.doFireAccount();
                        }
                    }
                    else
                    {
                        // Đang xem danh sách Bạn bè/Kẻ thù trong box 3
                        friendEnemySelected = row;
                        p.currInfoItem = row;
                        p.selected = row;
                        
                        // Tính toán lại tọa độ Box 3 chính xác để menu hiện đúng vị trí
                        int _detailW = 164;
                        int _listW = safeW - catW - gap - (_detailW + gap);
                        int _detailX = safeX + catW + gap + _listW + gap;
                        
                        p.X = _detailX;
                        p.Y = safeY + 24;
                        p.wScroll = _detailW;
                        p.hScroll = safeH - 30;
                        p.yScroll = p.Y;
                        p.cmy = toolDetailScrollY;
                        p.ITEM_HEIGHT = 28;

                        if (selectedAccountSubAction == 1)
                        {
                            p.type = 11;
                            p.vFriend = p.vFriend;
                            p.doFireFriend();
                        }
                        else
                        {
                            p.type = 16;
                            p.vEnemy = p.vEnemy;
                            p.doFireEnemy();
                        }
                    }
                    SoundMn.gI().panelClick();
                    return true;
                }
                if (selectedToolAction == 8)
                {
                    p.selected = row;
                    p.doFireOption();
                    SoundMn.gI().getStrOption();
                    //ClosePanelState(false);
                    SoundMn.gI().panelClick();
                    return true;
                }
            }
        }
        return false;
    }

    private static void PaintClanTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 118;
        int gap = 6;
        int msgW = safeW / 2 - gap / 2;
        int logicW = safeW - msgW - gap;
        int msgX = safeX;
        int logicX = msgX + msgW + gap;
        PaintOldPanelBox(g, msgX, safeY, msgW, safeH);
        PaintOldPanelBox(g, logicX, safeY, logicW, safeH);
        mFont.tahoma_7b_dark.drawString(g, "TIN NHẮN BANG", msgX + msgW / 2, safeY + 6, mFont.CENTER);
        PaintClanMessages(g, msgX + 6, safeY + 24, msgW - 12, safeH - 30);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
        PaintClanMenu(g, logicX + 6, safeY + 6, logicW - 12, 22);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
        PaintClanLogic(g, logicX + 6, safeY + 34, logicW - 12, safeH - 40);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    private static void PaintClanMenu(mGraphics g, int x, int y, int w, int h)
    {
        string[] tabs = GetClanMenuLabels();
        if (tabs == null || tabs.Length == 0)
        {
            return;
        }
        int tabW = w / tabs.Length;
        for (int i = 0; i < tabs.Length; i++)
        {
            int xx = x + i * tabW;
            bool selected = i == selectedClanMenuIndex;
            g.setColor(selected ? SELECT_BG : 15723751);
            g.fillRect(xx, y, tabW - 2, h, 5);
            mFont menuFont = mFont.tahoma_7b_dark;
            string label = tabs[i];
            int spaceIndex = label.IndexOf(' ');
            if (spaceIndex > 0)
            {
                menuFont.drawString(g, label.Substring(0, spaceIndex), xx + tabW / 2, y + 1, mFont.CENTER);
                menuFont.drawString(g, label.Substring(spaceIndex + 1), xx + tabW / 2, y + 10, mFont.CENTER);
            }
            else
            {
                menuFont.drawString(g, label, xx + tabW / 2, y + 5, mFont.CENTER);
            }
        }
    }

    private static string[] GetClanMenuLabels()
    {
        if (HasClanData())
        {
            return new string[] { "Thành viên", "Khẩu hiệu", "Biểu tượng" };
        }
        return new string[] { "Tìm bang", "Lập bang" };
    }

    private static bool HasClanData()
    {
        Char c = Char.myCharz();
        Panel p = GameCanvas.panel;
        return (c != null && c.clan != null) || (p != null && p.myMember != null && p.myMember.size() > 0);
    }

    private static MyVector GetClanMembers(Panel p)
    {
        if (p == null)
        {
            return null;
        }
        if (p.myMember != null && p.myMember.size() > 0)
        {
            return p.myMember;
        }
        return p.member;
    }

    private static void PaintClanMessages(mGraphics g, int x, int y, int w, int h)
    {
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        g.setClip(x, y, w, h);
        int rowH = 32;
        int count = ClanMessage.vMessage.size();
        if (count == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Chưa có tin nhắn bang hội", x + w / 2, y + 35, mFont.CENTER);
        }
        for (int i = 0; i < count; i++)
        {
            int yy = y + i * rowH - clanMsgScrollY;
            if (yy + rowH < y)
            {
                continue;
            }
            if (yy > y + h)
            {
                break;
            }
            ClanMessage cm = (ClanMessage)ClanMessage.vMessage.elementAt(i);
            if (cm != null)
            {
                cm.update();
                g.setColor((i == selectedClanMsgIndex) ? SELECT_BG : 15723751);
                g.fillRect(x, yy, w, rowH - 4, 5);
                int panelWScroll = GameCanvas.panel.wScroll;
                GameCanvas.panel.wScroll = w;
                cm.paint(g, x + 2, yy + 2);
                GameCanvas.panel.wScroll = panelWScroll;
            }
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
        PaintBagScrollBar(g, x + w - 4, y, h, GetClanMessageMaxScroll(h));
    }

    private static void PaintClanLogic(mGraphics g, int x, int y, int w, int h)
    {
        Panel p = GameCanvas.panel;
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        g.setClip(x, y, w, h);
        int rowH = 32;
        int count = GetClanLogicCount(p);
        if (p == null || count == 0)
        {
            string reason = HasClanData() ? "Chưa có dữ liệu thành viên" : "Chưa có danh sách bang";
            mFont.tahoma_7b_dark.drawString(g, reason, x + w / 2, y + 35, mFont.CENTER);
        }
        for (int i = 0; i < count; i++)
        {
            int yy = y + i * rowH - clanLogicScrollY;
            if (yy + rowH < y)
            {
                continue;
            }
            if (yy > y + h)
            {
                break;
            }
            PaintClanLogicRow(g, p, i, x, yy, w, rowH, i == selectedClanLogicIndex);
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
        PaintBagScrollBar(g, x + w - 4, y, h, GetClanLogicMaxScroll(h));
    }

    private static void PaintClanLogicRow(mGraphics g, Panel p, int row, int x, int y, int w, int h, bool selected)
    {
        g.setColor(selected ? SELECT_BG : 15723751);
        g.fillRect(x + 24, y, w - 24, h - 4, 5);
        g.setColor(selected ? SELECT_BORDER : 15723751);
        g.fillRect(x, y, 24, h - 4, 5);
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        if (!HasClanData())
        {
            if (p == null || p.clans == null || row < 0 || row >= p.clans.Length)
            {
                return;
            }
            Clan clan = p.clans[row];
            if (clan == null)
            {
                return;
            }
            int oldClanClipX = g.getClipX();
            int oldClanClipY = g.getClipY();
            int oldClanClipW = g.getClipWidth();
            int oldClanClipH = g.getClipHeight();
            g.setClip(x, y, 24, h - 4);
            if (ClanImage.isExistClanImage(clan.imgID) && ClanImage.getClanImage((short)clan.imgID).idImage != null)
            {
                SmallImage.drawSmallImage(g, ClanImage.getClanImage((short)clan.imgID).idImage[0], x + 12, y + (h - 4) / 2, 0, StaticObj.VCENTER_HCENTER);
            }
            g.setClip(oldClanClipX, oldClanClipY, oldClanClipW, oldClanClipH);
            int clanTextX = x + 36;
            int clanTextW = w - 76;
            string name = (clan.name.Length <= 23) ? clan.name : (clan.name.Substring(0, 23) + "...");
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, name, clanTextW), clanTextX, y + 2, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, clan.slogan, clanTextW), clanTextX, y + 14, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, clan.currMember + "/" + clan.maxMember, x + w - 8, y + 2, mFont.RIGHT);
            return;
        }
        MyVector members = GetClanMembers(p);
        if (members == null || row < 0 || row >= members.size())
        {
            return;
        }
        Member member = (Member)members.elementAt(row);
        if (member == null)
        {
            return;
        }
        if (member.headICON != -1)
        {
            SmallImage.drawSmallImage(g, member.headICON, x + 2, y + 9, 0, 0);
        }
        else if (member.head >= 0 && member.head < GameScr.parts.Length)
        {
            Part part = GameScr.parts[(int)member.head];
            if (part != null && part.pi != null)
            {
                SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id, x + 2 + (int)part.pi[Char.CharInfo[0][0][0]].dx, y + 12 + (int)part.pi[Char.CharInfo[0][0][0]].dy, 0, 0);
            }
        }
        int textX = x + 32;
        int textW = w - 74;
        int oldTextClipX = g.getClipX();
        int oldTextClipY = g.getClipY();
        int oldTextClipW = g.getClipWidth();
        int oldTextClipH = g.getClipHeight();
        g.setClip(textX - 2, y, textW + 4, h - 4);
        g.setColor(selected ? SELECT_BG : 15723751);
        g.fillRect(textX - 2, y, textW + 4, h - 4, 5);
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, member.name, textW), textX, y + 2, mFont.LEFT);
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, mResources.power + ": " + member.powerPoint, textW), textX, y + 14, mFont.LEFT);
        g.setClip(oldTextClipX, oldTextClipY, oldTextClipW, oldTextClipH);
        mFont.tahoma_7b_dark.drawString(g, member.clanPoint.ToString(), x + w - 8, y + 8, mFont.RIGHT);
    }

    private static int GetClanLogicCount(Panel p)
    {
        if (p == null)
        {
            return 0;
        }
        if (HasClanData())
        {
            MyVector members = GetClanMembers(p);
            return (members != null) ? members.size() : 0;
        }
        return (p.clans != null) ? p.clans.Length : 0;
    }

    private static int GetClanMessageMaxScroll(int viewH)
    {
        int contentH = ClanMessage.vMessage.size() * 32;
        int max = contentH - viewH;
        return (max > 0) ? max : 0;
    }

    private static int GetClanLogicMaxScroll(int viewH)
    {
        int contentH = GetClanLogicCount(GameCanvas.panel) * 32;
        int max = contentH - viewH;
        return (max > 0) ? max : 0;
    }

    private static void EnsurePanelChatField(Panel p)
    {
        if (p == null || p.chatTField != null)
        {
            return;
        }
        p.chatTField = new ChatTextField();
        p.chatTField.tfChat.y = GameCanvas.h - 35 - ChatTextField.gI().tfChat.height;
        p.chatTField.initChatTextField();
        p.chatTField.parentScreen = GameCanvas.panel;
    }

    private static void HandleClanMenuAction(int menuIndex)
    {
        Panel p = GameCanvas.panel;
        Char c = Char.myCharz();
        if (p == null || c == null)
        {
            return;
        }
        if (HasClanData())
        {
            if (menuIndex == 0)
            {
                p.member = null;
                p.isSearchClan = false;
                p.isViewMember = true;
                p.isMessage = false;
                selectedClanLogicIndex = -1;
                clanLogicScrollY = 0;
            }
            else if (menuIndex == 1)
            {
                EnsurePanelChatField(p);
                p.chagenSlogan();
            }
            else if (menuIndex == 2)
            {
                Service.gI().getClan(3, -1, null);
            }
            return;
        }
        if (menuIndex == 0)
        {
            p.isSearchClan = true;
            p.isViewMember = false;
            p.isMessage = false;
            Service.gI().searchClan(string.Empty);
            selectedClanLogicIndex = -1;
            clanLogicScrollY = 0;
        }
        else if (menuIndex == 1)
        {
            EnsurePanelChatField(p);
            p.chatTField.strChat = mResources.clan_name;
            p.chatTField.tfChat.name = mResources.clan_name;
            p.chatTField.to = string.Empty;
            p.chatTField.isShow = true;
            p.chatTField.tfChat.isFocus = true;
            p.chatTField.tfChat.setIputType(TField.INPUT_TYPE_ANY);
            if (GameCanvas.isTouch)
            {
                p.chatTField.tfChat.doChangeToTextBox();
            }
            if (!Main.isPC)
            {
                p.chatTField.startChat(p, string.Empty);
            }
        }
    }

    private static void HandleClanListAction(int row)
    {
        Panel p = GameCanvas.panel;
        if (p == null || p.clans == null || row < 0 || row >= p.clans.Length)
        {
            return;
        }
        Clan clan = p.clans[row];
        if (clan == null)
        {
            return;
        }
        p.currClan = clan;
        MyVector cmds = new MyVector();
        if (Char.myCharz() != null && Char.myCharz().clan == null)
        {
            cmds.addElement(new Command(mResources.request_join_clan, p, 4000, clan));
        }
        cmds.addElement(new Command(mResources.view_clan_member, p, 4001, clan));
        int menuY = panelY + 78 + row * 32 - clanLogicScrollY;
        GameCanvas.menu.startAt(cmds, panelX + panelW / 2, menuY);
        p.addClanDetail(clan);
    }

    private static void HandleClanMemberAction(int row)
    {
        Panel p = GameCanvas.panel;
        MyVector members = GetClanMembers(p);
        if (p == null || members == null || row < 0 || row >= members.size())
        {
            return;
        }
        Member mem = (Member)members.elementAt(row);
        if (mem == null)
        {
            return;
        }
        p.currMem = mem;
        MyVector cmds = new MyVector();
        Char c = Char.myCharz();
        if (p.member != null && (p.myMember == null || p.myMember.size() == 0))
        {
            cmds.addElement(new Command(mResources.CLOSE, p, 8000, null));
        }
        else if (c != null)
        {
            if (c.charID == mem.ID || c.role == 2)
            {
                cmds.addElement(new Command(mResources.CLOSE, p, 8000, mem));
            }
            if (c.role < 2 && c.charID != mem.ID)
            {
                if (mem.role == 0 || mem.role == 1)
                {
                    cmds.addElement(new Command(mResources.CLOSE, p, 8000, mem));
                }
                if (mem.role == 2)
                {
                    cmds.addElement(new Command(mResources.create_clan_co_leader, p, 5002, mem));
                }
                if (c.role == 0)
                {
                    cmds.addElement(new Command(mResources.create_clan_leader, p, 5001, mem));
                    if (mem.role == 1)
                    {
                        cmds.addElement(new Command(mResources.disable_clan_mastership, p, 5003, mem));
                    }
                }
            }
            if (c.role < mem.role)
            {
                cmds.addElement(new Command(mResources.kick_clan_mem, p, 5004, mem));
            }
        }
        cmds.addElement(new Command(ModFunc.strTeleportTo, p, 8004, mem.ID));
        int menuY = panelY + 78 + row * 32 - clanLogicScrollY;
        GameCanvas.menu.startAt(cmds, panelX + panelW / 2, menuY);
        p.addClanMemberDetail(mem);
    }

    private static void HandleClanMessageAction(int row)
    {
        if (row < 0 || row >= ClanMessage.vMessage.size())
        {
            return;
        }
        Panel p = GameCanvas.panel;
        ClanMessage msg = (ClanMessage)ClanMessage.vMessage.elementAt(row);
        if (p == null || msg == null)
        {
            return;
        }
        p.currMess = msg;
        if (msg.type == 0)
        {
            MyVector cmds = new MyVector();
            cmds.addElement(new Command(mResources.CLOSE, p, 8000, msg));
            int menuY = panelY + 68 + row * 32 - clanMsgScrollY;
            GameCanvas.menu.startAt(cmds, panelX + 24, menuY);
            p.addMessageDetail(msg);
        }
        else if (msg.type == 1)
        {
            if (msg.playerId != Char.myCharz().charID)
            {
                Service.gI().clanDonate(msg.id);
            }
        }
        else if (msg.type == 2 && msg.option != null)
        {
            MyVector cmds = new MyVector();
            cmds.addElement(new Command(msg.option[0], new ClanJoinMessageCommand(msg.id, 1), 1, null));
            if (msg.option.Length > 1)
            {
                cmds.addElement(new Command(msg.option[1], new ClanJoinMessageCommand(msg.id, 0), 1, null));
            }
            int menuY = panelY + 68 + row * 32 - clanMsgScrollY;
            GameCanvas.menu.startAt(cmds, panelX + 24, menuY);
        }
    }

    private class ClanJoinMessageCommand : IActionListener
    {
        private readonly int id;
        private readonly sbyte action;

        public ClanJoinMessageCommand(int id, sbyte action)
        {
            this.id = id;
            this.action = action;
        }

        public void perform(int idAction, object p)
        {
            Service.gI().joinClan(this.id, this.action);
        }
    }

    private static bool TryHandleClanClick(bool isFire)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 52;
        int safeW = panelW - 48;
        int safeH = panelH - 88;
        int gap = 6;
        int msgW = safeW / 2 - gap / 2;
        int logicW = safeW - msgW - gap;
        int msgX = safeX + 6;
        int logicX = safeX + msgW + gap + 6;
        int listY = safeY + 34;
        int listH = safeH - 40;
        int rowH = 32;
        string[] menuLabels = GetClanMenuLabels();
        int menuY = safeY + 6;
        int menuH = 22;
        int logicInnerW = logicW - 12;
        if (GameCanvas.px >= logicX && GameCanvas.px <= logicX + logicInnerW && GameCanvas.py >= menuY && GameCanvas.py <= menuY + menuH)
        {
            int tabW = logicInnerW / menuLabels.Length;
            int menuIndex = (GameCanvas.px - logicX) / tabW;
            if (menuIndex < 0) menuIndex = 0;
            if (menuIndex >= menuLabels.Length) menuIndex = menuLabels.Length - 1;
            if (!isFire) return true;
            selectedClanMenuIndex = menuIndex;
            HandleClanMenuAction(menuIndex);
            SoundMn.gI().panelClick();
            return true;
        }
        if (GameCanvas.py < listY || GameCanvas.py > listY + listH)
        {
            return false;
        }
        if (GameCanvas.px >= msgX && GameCanvas.px <= msgX + msgW - 12)
        {
            int row = (GameCanvas.py - listY + clanMsgScrollY) / rowH;
            if (row >= 0 && row < ClanMessage.vMessage.size())
            {
                if (!isFire) return true;
                selectedClanMsgIndex = row;
                HandleClanMessageAction(row);
                SoundMn.gI().panelClick();
                return true;
            }
        }
        if (GameCanvas.px >= logicX && GameCanvas.px <= logicX + logicW - 12)
        {
            int row = (GameCanvas.py - listY + clanLogicScrollY) / rowH;
            if (row >= 0 && row < GetClanLogicCount(GameCanvas.panel))
            {
                selectedClanLogicIndex = row;
                if (GameCanvas.panel != null)
                {
                    GameCanvas.panel.selected = row + 2;
                }
                if (!isFire) return true;
                if (!HasClanData())
                {
                    HandleClanListAction(row);
                }
                else
                {
                    HandleClanMemberAction(row);
                }
                SoundMn.gI().panelClick();
                return true;
            }
        }
        return false;
    }

    private static void PaintPetTab(mGraphics g)
    {
        Char me = Char.myCharz();
        if (me == null) return;

        bool hasPet = me.havePet || me.havePet2;
        
        // Yêu cầu dữ liệu nếu cần
        RequestPetInfoIfNeeded(false);
        
        Char pet = hasPet ? GetPrimaryPet() : null;
        if (pet != null)
        {
            // Bỏ SyncCharPartsFromItems vì nó ghi đè dữ liệu server gây lỗi lệch bộ phận.
            // Để server tự cập nhật ngoại hình đệ tử (giống player) sẽ chính xác hơn.
            
            // Đảm bảo có bộ phận tối thiểu để vẽ nếu server chưa gửi dữ liệu
            if (pet.head == -1 || pet.head == 0) pet.setDefaultPart();
        }

        int safeX2 = panelX + 24;
        int safeW2 = panelW - 48;
        int leftX = safeX2;
        int leftW = safeW2 / 2 - 16;
        int rightX2 = safeX2 + safeW2 / 2 + 10;
        int rightW2 = 34 * 6 + 4 * 5;

        PaintPetLeft(g, pet, leftX, leftW);
        PaintPetInfoFrame(g, pet);
        PaintPetSubTabs(g, rightX2, panelY + 44, rightW2);

        if (!hasPet)
        {
            mFont.tahoma_7b_dark.drawString(g, "Chưa có đệ tử", rightX2 + rightW2 / 2, panelY + 126, mFont.CENTER);
        }
        else if (petSubTab == 0)
        {
            PaintPetBag(g, pet, rightX2, panelY + 66);
        }
        else if (petSubTab == 1)
        {
            PaintPetSkills(g, pet, rightX2, panelY + 66, rightW2);
        }
        else if (petSubTab == 2)
        {
            PaintPetStatus(g, pet, rightX2, panelY + 66, rightW2);
        }

        PaintInventoryCurrency(g);
    }

    private static void PaintPetLeft(mGraphics g, Char pet, int leftX, int leftW)
    {
        int leftY = panelY + 42;
        int frameX = leftX - 3;
        int frameY = leftY + 4;
        int frameW = 246;
        int frameH = 188;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 19;
        int bodyGapY = 27;
        PaintOldPanelBox(g, frameX, frameY, frameW, frameH);
        Item[] body = (pet != null) ? pet.arrItemBody : null;
        for (int i = 0; i < 5; i++)
        {
            int y = bodyTopY + i * bodyGapY;
            bool isSelectedLeft = (selectedBodyIndex == i);
            PaintSlotRect(g, bodyLeftX, y, 36, 24, isSelectedLeft);
            PaintItemInSlot(g, GetItem(body, i), bodyLeftX, y, 36, 24, isSelectedLeft);
            bool isSelectedRight = (selectedBodyIndex == i + 5);
            PaintSlotRect(g, bodyRightX, y, 36, 24, isSelectedRight);
            PaintItemInSlot(g, GetItem(body, i + 5), bodyRightX, y, 36, 24, isSelectedRight);
        }
        if (pet != null)
        {
            PaintCharacterPreview(g, pet, centerX, frameY + 132, true);
        }
        int bottomY = frameY + 159;
        int bottomW = 36;
        int bottomH = 24;
        int bottomGap = 2;
        for (int i = 0; i < 4; i++)
        {
            int x = centerX - 75 + i * (bottomW + bottomGap);
            bool isSelected = (selectedBodyIndex == i + 10);
            PaintSlotRect(g, x, bottomY, bottomW, bottomH, isSelected);
            PaintItemInSlot(g, GetItem(body, i + 10), x, bottomY, bottomW, bottomH, isSelected);
        }
    }

    private static void PaintCharacterPreview(mGraphics g, Char ch, int x, int y, bool applyCostumePartsForPreview)
    {
        if (ch == null)
        {
            return;
        }
        int oldCx = ch.cx;
        int oldCy = ch.cy;
        int oldCdir = ch.cdir;
        int oldCf = ch.cf;
        int oldFy = ch.fy;
        sbyte oldMonkey = ch.isMonkey;
        bool oldFusion = ch.isFusion;
        short oldHead = (short) ch.head;
        short oldBody = (short) ch.body;
        short oldLeg = (short) ch.leg;
        
        int[] effXs, effYs;
        CapturePreviewEffectPositions(ch, out effXs, out effYs);

        try
        {
            ch.cx = x;
            ch.cy = y;
            ch.fy = 0;
            ch.cdir = 1;
            ch.isMonkey = 0;
            ch.isFusion = false;
            ch.cf = ((GameCanvas.gameTick / 8) % 2 == 0) ? 0 : 1;
            if (applyCostumePartsForPreview)
            {
                SyncCharPartsFromItems(ch);
            }
            
            PositionPreviewCharEffects(ch);
            PaintPreviewCharBoundEffects(g, ch, 0);
            PaintPreviewMobMe(g, ch);
            
            ch.paintCharBody(g, ch.cx, ch.cy, ch.cdir, ch.cf, true);
            
            PaintPreviewCharBoundEffects(g, ch, 1);
            PaintCharacterPreviewAuxEffects(g, ch, x, y);
        }
        catch (Exception ex)
        {
            Cout.LogError("Loi paint character preview: " + ex.ToString());
        }
        finally
        {
            ch.cx = oldCx;
            ch.cy = oldCy;
            ch.cdir = oldCdir;
            ch.cf = oldCf;
            ch.fy = oldFy;
            ch.isMonkey = oldMonkey;
            ch.isFusion = oldFusion;
            ch.head = oldHead;
            ch.body = oldBody;
            ch.leg = oldLeg;
            RestorePreviewEffectPositions(ch, effXs, effYs);
        }
    }

    private static void SyncCharPartsFromItems(Char ch)
    {
        if (ch == null || ch.arrItemBody == null)
        {
            return;
        }

        bool overrideHead = false;
        bool overrideBody = false;
        bool overrideLeg = false;
        short costumeHead = (short)ch.head;
        short costumeBody = (short)ch.body;
        short costumeLeg = (short)ch.leg;

        // 1. Ưu tiên lấy part từ Cải trang đang trang bị (type tóc/cải trang)
        Item costume = FindEquippedCostume(ch.arrItemBody);
        if (costume != null)
        {
            if (costume.headTemp != -1)
            {
                costumeHead = (short)costume.headTemp;
                overrideHead = true;
            }
            if (costume.bodyTemp != -1)
            {
                costumeBody = (short)costume.bodyTemp;
                overrideBody = true;
            }
            if (costume.legTemp != -1)
            {
                costumeLeg = (short)costume.legTemp;
                overrideLeg = true;
            }

            if (costume.itemOption != null)
            {
                for (int j = 0; j < costume.itemOption.Length; j++)
                {
                    ItemOption opt = costume.itemOption[j];
                    if (opt == null || opt.optionTemplate == null) continue;
                    if (opt.optionTemplate.id == 127)
                    {
                        costumeHead = (short)opt.param;
                        overrideHead = true;
                    }
                    if (opt.optionTemplate.id == 128)
                    {
                        costumeBody = (short)opt.param;
                        overrideBody = true;
                    }
                    if (opt.optionTemplate.id == 129)
                    {
                        costumeLeg = (short)opt.param;
                        overrideLeg = true;
                    }
                }
            }
        }

        // 2. Ghi part từ cải trang trước, từng bộ phận độc lập
        if (overrideHead) ch.head = costumeHead;
        if (overrideBody) ch.body = costumeBody;
        if (overrideLeg) ch.leg = costumeLeg;

        // 3. Bộ phận nào không bị cải trang ghi đè thì lấy từ trang bị gốc
        for (int i = 0; i < ch.arrItemBody.Length; i++)
        {
            Item item = ch.arrItemBody[i];
            if (item == null || item.template == null) continue;

            int part = item.template.part;
            if (part == -1) continue;

            if (item.template.type == 0 && !overrideBody)
                ch.body = (short)part;
            else if (item.template.type == 1 && !overrideLeg)
                ch.leg = (short)part;
            else if ((item.template.type == 2 || item.template.type == 6) && !overrideHead)
                ch.head = (short)part;
        }

        // 4. Fallback để không bị trống
        if (ch.head == -1) ch.head = 0;
        if (ch.body == -1) ch.body = 0;
        if (ch.leg == -1) ch.leg = 0;
    }

    private static Item FindEquippedCostume(Item[] bodyItems)
    {
        if (bodyItems == null)
        {
            return null;
        }
        // Ưu tiên item type 5 (hair/cải trang). Không cứng theo index vì mỗi server có thể map slot khác nhau.
        for (int i = 0; i < bodyItems.Length; i++)
        {
            Item item = bodyItems[i];
            if (item == null || item.template == null)
            {
                continue;
            }
            if (item.template.type == 5)
            {
                return item;
            }
        }
        // Fallback: item nào có temp part thì dùng để preview.
        for (int i = 0; i < bodyItems.Length; i++)
        {
            Item item = bodyItems[i];
            if (item == null)
            {
                continue;
            }
            if (item.headTemp != -1 || item.bodyTemp != -1 || item.legTemp != -1)
            {
                return item;
            }
        }
        return null;
    }

    private static void CapturePreviewEffectPositions(Char ch, out int[] xs, out int[] ys)
    {
        xs = null;
        ys = null;
        if (ch == null || ch.vEffChar == null)
        {
            return;
        }
        int count = ch.vEffChar.size();
        xs = new int[count];
        ys = new int[count];
        for (int i = 0; i < count; i++)
        {
            Effect effect = (Effect)ch.vEffChar.elementAt(i);
            if (effect == null)
            {
                continue;
            }
            xs[i] = effect.x;
            ys[i] = effect.y;
        }
    }

    private static void RestorePreviewEffectPositions(Char ch, int[] xs, int[] ys)
    {
        if (ch == null || ch.vEffChar == null || xs == null || ys == null)
        {
            return;
        }
        int count = ch.vEffChar.size();
        if (count > xs.Length)
        {
            count = xs.Length;
        }
        for (int i = 0; i < count; i++)
        {
            Effect effect = (Effect)ch.vEffChar.elementAt(i);
            if (effect == null)
            {
                continue;
            }
            effect.x = xs[i];
            effect.y = ys[i];
        }
    }

    private static void PositionPreviewCharEffects(Char ch)
    {
        if (ch == null || ch.vEffChar == null)
        {
            return;
        }
        for (int i = 0; i < ch.vEffChar.size(); i++)
        {
            Effect effect = (Effect)ch.vEffChar.elementAt(i);
            if (effect == null)
            {
                continue;
            }
            if (effect.typeEff == 5)
            {
                effect.trans = (ch.cdir != 1) ? 1 : 0;
                effect.x = (ch.cdir == 1) ? (ch.cx - 15) : (ch.cx + 15);
                effect.y = (ch.isMonkey == 0) ? (ch.cy - 25) : (ch.cy - 35);
            }
            else
            {
                effect.x = ch.cx;
                effect.y = ch.cy;
            }
        }
    }

    private static void PaintPreviewCharBoundEffects(mGraphics g, Char ch, int layer)
    {
        if (ch == null || ch.vEffChar == null)
        {
            return;
        }
        PositionPreviewCharEffects(ch);
        for (int i = 0; i < ch.vEffChar.size(); i++)
        {
            Effect effect = (Effect)ch.vEffChar.elementAt(i);
            if (effect == null || effect.layer != layer)
            {
                continue;
            }
            bool canPaint = true;
            if (effect.isStand == 0)
            {
                canPaint = ch.statusMe == 1 || ch.statusMe == 6;
            }
            if (canPaint)
            {
                effect.paint(g);
            }
        }
    }

    private static void PaintPreviewMobMe(mGraphics g, Char ch)
    {
        if (ch == null || ch.mobMe == null)
        {
            return;
        }
        ch.mobMe.xFirst = (ch.cdir != 1) ? (ch.cx + 30) : (ch.cx - 30);
        ch.mobMe.yFirst = ch.cy - 60;
        ch.mobMe.x = ch.mobMe.xFirst;
        ch.mobMe.y = ch.mobMe.yFirst;
        ch.mobMe.dir = ch.cdir;
        ch.mobMe.paint(g);
    }

    private static void PaintCharacterPreviewAuxEffects(mGraphics g, Char ch, int x, int y)
    {
        if (ch == null)
        {
            return;
        }
        if (ch.protectEff)
        {
            PaintLocalPreviewEffect(g, ref previewProtectEffect, 33, x, y - 34);
        }
        if (ch.isFreez)
        {
            PaintLocalPreviewEffect(g, ref previewFreezeEffect, 113, x, y - 24);
        }
        if (ch.sleepEff)
        {
            SmallImage.drawSmallImage(g, 290, x, y - 58, 0, mGraphics.BOTTOM | mGraphics.HCENTER);
        }
    }

    private static void PaintLocalPreviewEffect(mGraphics g, ref Effect effect, int effectId, int x, int y)
    {
        if (effect == null || effect.effId != effectId)
        {
            effect = new Effect(effectId, x, y, 1, -1, -1);
        }
        effect.x = x;
        effect.y = y;
        effect.update();
        effect.paint(g);
    }

    private static void PaintPetSubTabs(mGraphics g, int x, int y, int w)
    {
        int tw = w / PET_TABS.Length;
        for (int i = 0; i < PET_TABS.Length; i++)
        {
            bool active = i == petSubTab;
            Image tabImg = GetTabImage(active, true);
            int tx = x + i * tw;
            if (tabImg != null)
            {
                g.drawImage(tabImg, tx, y, 0);
            }
            else
            {
                Fill(g, tx, y, tw - 2, 18, active ? 0xDFF6FF : 0xF8DDA8);
                g.setColor(active ? 0x2B8AA8 : 0xA36B2E);
                g.drawRect(tx + 1, y + 1, tw - 4, 16);
            }
            mFont.tahoma_7b_dark.drawString(g, PET_TABS[i], tx + tw / 2, y + 4, mFont.CENTER);
        }
    }

    private static void PaintPetBag(mGraphics g, Char pet, int x, int y)
    {
        Item[] items = (Char.myCharz() != null) ? Char.myCharz().arrItemBag : null;
        int gap = 4;
        int viewW = 34 * 6 + gap * 5;
        int viewH = 26 * 7 + gap * 6;
        int total = (items != null) ? items.Length : 42;
        int rows = (total + 5) / 6;
        if (rows < 7)
        {
            rows = 7;
        }
        g.setClip(x, y, viewW, viewH);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                int index = row * 6 + col;
                int xx = x + col * (34 + gap);
                int yy = y + row * (26 + gap) - bagScrollY;
                bool isSelected = (selectedBagIndex == index);
                PaintSlotRect(g, xx, yy, 34, 26, isSelected);
                PaintItemInSlot(g, GetItem(items, index), xx, yy, 34, 26, isSelected);
                if (yy + 26 < y || yy > y + viewH)
                {
                    continue;
                }
                PaintSlot(g, xx, yy, 26, selectedBagIndex == index && selectedBodyIndex < 0);
                PaintItemInSlot(g, GetItem(items, index), xx, yy, 34, 26);
            }
        }
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
        PaintBagScrollBar(g, x + viewW + 3, y, viewH, GetBagMaxScroll(viewH));
    }

    private static void PaintPetSkills(mGraphics g, Char pet, int x, int y, int w)
    {
        Skill[] skills = pet.arrPetSkill;
        int colW = w / 2;
        for (int i = 0; skills != null && i < skills.Length; i++)
        {
            Skill skill = skills[i];
            if (skill == null || skill.template == null)
            {
                continue;
            }
            int xx = x + (i % 2) * colW;
            int yy = y + (i / 2) * 46;
            PaintSkillCell(g, skill, xx, yy, colW - 4, 40);
        }
    }

    private static void PaintPetInfoFrame(mGraphics g, Char pet)
    {
        int safeX = panelX + 24;
        int leftX = safeX;
        int statsFrameX = leftX - 3;
        int statsFrameY = panelY + 242;
        int statsFrameW = 247;
        PaintOldPanelBox(g, statsFrameX, statsFrameY, statsFrameW, 58);
        if (pet == null)
        {
            mFont.tahoma_7b_dark.drawString(g, "Thông tin đệ tử", statsFrameX + statsFrameW / 2, statsFrameY + 9, mFont.CENTER);
            mFont.tahoma_7_grey.drawString(g, "Chưa có dữ liệu", statsFrameX + statsFrameW / 2, statsFrameY + 31, mFont.CENTER);
            return;
        }
        int x = statsFrameX + 22;
        int y = statsFrameY + 5;
        string[] lines = new string[]
        {
            "HP: " + FormatStat(pet.cHPFull),
            "KI: " + FormatStat(pet.cMPFull),
            "SĐ: " + FormatStat(pet.cDamFull),
            "Giáp: " + FormatStat(pet.cDefull),
            "Crit: " + pet.cCriticalFull + "%",
            "SM: " + FormatStat(pet.cPower)
        };
        for (int i = 0; i < lines.Length; i++)
        {
            int col = i / 3;
            int row = i % 3;
            mFont.tahoma_7b_dark.drawString(g, lines[i], x + col * 96, y + row * 11, mFont.LEFT);
        }
    }

    private static void PaintPetStatus(mGraphics g, Char pet, int x, int y, int w)
    {
        if (pet == null)
        {
            mFont.tahoma_7b_dark.drawString(g, "Chưa có dữ liệu đệ tử", x + 6, y + 6, mFont.LEFT);
            return;
        }
        string[] options = GetPetStatusOptions();
        int activeStatus = pet.petStatus;
        bool shouldHighlightFusion = isPorataFusionActive && mSystem.currentTimeMillis() >= suppressFusionHighlightUntil;
        if (shouldHighlightFusion)
        {
            activeStatus = 4;
        }
        else
        {
            long now = mSystem.currentTimeMillis();
            if (pendingPetStatus >= 0 && now < pendingPetStatusUntil)
            {
                activeStatus = pendingPetStatus;
            }
            else if (pendingPetStatus >= 0)
            {
                pendingPetStatus = -1;
                pendingPetStatusUntil = 0L;
            }
        }
        if (activeStatus < 0 || activeStatus >= options.Length)
        {
            activeStatus = 0;
        }
        int rowX;
        int rowY;
        int rowW;
        int rowH;
        int toggleW;
        int toggleH;
        GetPetStatusLayout(x, y, w, out rowX, out rowY, out rowW, out rowH, out toggleW, out toggleH);
        mFont.tahoma_7b_dark.drawString(g, "Chọn trạng thái đệ tử", x + 6, y, mFont.LEFT);
        for (int i = 0; i < options.Length; i++)
        {
            bool enabled = i == activeStatus;
            int itemY = rowY + i * rowH;
            PaintOldTextCell(g, rowX, itemY, rowW, rowH - 2, enabled);
            mFont.tahoma_7b_dark.drawString(g, options[i], rowX + 8, itemY + 7, mFont.LEFT);
            PaintStatusToggle(g, rowX + rowW - toggleW - 6, itemY + (rowH - toggleH) / 2, toggleW, toggleH, enabled);
        }
    }

    private static void PaintStatusToggle(mGraphics g, int x, int y, int w, int h, bool enabled)
    {
        g.setColor(enabled ? 4825130 : 9671571);
        g.fillRect(x, y, w, h, 6);
        g.setColor(0xFFFFFF);
        int knobW = w / 2 - 1;
        int knobH = h - 2;
        int knobX = enabled ? (x + w - knobW - 1) : (x + 1);
        g.fillRect(knobX, y + 1, knobW, knobH, 4);
    }

    private static void TryHandleTaskMapClick(bool isFire)
    {
        if (Panel.imgMap == null || !Panel.isPaintMap)
        {
            return;
        }
        int px = GameCanvas.px;
        int py = GameCanvas.py;
        // Kiểm tra click trong vùng bản đồ
        if (px < taskMapContentX || px > taskMapContentX + taskMapContentW ||
            py < taskMapContentY || py > taskMapContentY + taskMapContentH)
        {
            return;
        }
        if (!isFire)
        {
            return;
        }
        // Chuyển đổi tọa độ click sang tọa độ bản đồ
        int mapClickX = px - taskMapContentX + taskMapScrollX;
        int mapClickY = py - taskMapContentY + taskMapScrollY;
        // Tìm vị trí gần nhất trên bản đồ
        taskMapClickedIndex = -1;
        for (int i = 0; i < Panel.mapX[(int)TileMap.planetID].Length; i++)
        {
            int mx = Panel.mapX[(int)TileMap.planetID][i];
            int my = Panel.mapY[(int)TileMap.planetID][i];
            if (Res.inRect(mx - 15, my - 15, 30, 30, mapClickX, mapClickY))
            {
                taskMapClickedIndex = i;
                break;
            }
        }
    }

    private static void PaintTaskTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 42;
        int safeW = panelW - 48;
        int safeH = panelH - 82;
        Task task = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;

        int gap = 6;
        int frameY = safeY + 4;
        int frameH = 253;

        // Box 1: Thông tin nhiệm vụ (bên trái)
        int taskBoxX = panelX + 21;
        int taskBoxW = (safeW - gap) / 2 + 10;
        PaintOldPanelBox(g, taskBoxX, frameY, taskBoxW, frameH);

        // Box 2: Bản đồ (bên phải)
        int mapBoxX = taskBoxX + taskBoxW + gap;
        int mapBoxW = 499 - taskBoxW - gap;
        PaintOldPanelBox(g, mapBoxX, frameY, mapBoxW, frameH);

        // === Vẽ nội dung Box 1: Thông tin nhiệm vụ ===
        int contentX = taskBoxX + 8;
        int contentW = taskBoxW - 16;
        mFont.tahoma_7b_dark.drawString(g, "NHIỆM VỤ", taskBoxX + taskBoxW / 2, frameY + 8, mFont.CENTER);

        if (task == null)
        {
            mFont.tahoma_7b_dark.drawString(g, "Chưa có nhiệm vụ", taskBoxX + taskBoxW / 2, frameY + 50, mFont.CENTER);
        }
        else
        {
            int y = frameY + 26;
            int bottomLimit = frameY + frameH - 12;
            if (task.names != null)
            {
                for (int i = 0; i < task.names.Length && y < bottomLimit - 20; i++)
                {
                    mFont.tahoma_7b_green2.drawString(g, task.names[i], contentX, y, mFont.LEFT);
                    y += 11;
                }
            }
            y += 4;
            if (task.details != null)
            {
                for (int i = 0; i < task.details.Length && y < bottomLimit - 40; i++)
                {
                    mFont.tahoma_7b_dark.drawString(g, task.details[i], contentX, y, mFont.LEFT);
                    y += 10;
                }
            }
            y += 6;
            PaintTaskProgress(g, task, contentX, y, contentW, bottomLimit);
        }

        // === Vẽ nội dung Box 2: Bản đồ ===
        mFont.tahoma_7b_dark.drawString(g, mResources.map, mapBoxX + mapBoxW / 2, frameY + 8, mFont.CENTER);

        taskMapContentX = mapBoxX + 3;
        taskMapContentY = frameY + 22;
        taskMapContentW = mapBoxW - 6;
        taskMapContentH = frameH - 28;

        // Load bản đồ nếu chưa có
        if (Panel.imgMap == null && Panel.isPaintMap)
        {
            try
            {
                Panel.imgMap = GameCanvas.loadImageRMS("/img/map" + TileMap.planetID.ToString() + ".png");
                TileMap.lastPlanetId = TileMap.planetID;
            }
            catch (System.Exception) { }
        }

        if (Panel.imgMap != null && Panel.isPaintMap)
        {
            // Tính vị trí nhân vật trên bản đồ
            int charMapX = -1;
            int charMapY = -1;
            for (int i = 0; i < Panel.mapId[(int)TileMap.planetID].Length; i++)
            {
                if (TileMap.mapID == Panel.mapId[(int)TileMap.planetID][i])
                {
                    charMapX = Panel.mapX[(int)TileMap.planetID][i];
                    charMapY = Panel.mapY[(int)TileMap.planetID][i];
                    break;
                }
            }

            // Tính offset để center bản đồ vào vị trí nhân vật
            int imgW = Panel.imgMap.getWidth();
            int imgH = Panel.imgMap.getHeight();
            int scrollX = 0;
            int scrollY = 0;
            if (charMapX >= 0)
            {
                scrollX = charMapX - taskMapContentW / 2;
                scrollY = charMapY - taskMapContentH / 2;
            }
            if (scrollX < 0) scrollX = 0;
            if (scrollY < 0) scrollY = 0;
            if (scrollX > imgW - taskMapContentW) scrollX = imgW - taskMapContentW;
            if (scrollY > imgH - taskMapContentH) scrollY = imgH - taskMapContentH;
            if (scrollX < 0) scrollX = 0;
            if (scrollY < 0) scrollY = 0;

            // Lưu scroll offset để dùng trong click handler
            taskMapScrollX = scrollX;
            taskMapScrollY = scrollY;

            // Clip và vẽ bản đồ
            int oldClipX = g.getClipX();
            int oldClipY = g.getClipY();
            int oldClipW = g.getClipWidth();
            int oldClipH = g.getClipHeight();
            g.setClip(taskMapContentX, taskMapContentY, taskMapContentW, taskMapContentH);
            g.translate(-scrollX, -scrollY);
            g.drawImage(Panel.imgMap, taskMapContentX, taskMapContentY, 0);

            // Vẽ vị trí nhiệm vụ nhấp nháy (sao vàng)
            int taskMapIdVal = GameScr.getTaskMapId();
            int taskPointIdx = -1;
            if (taskMapIdVal != -1)
            {
                for (int i = 0; i < Panel.mapId[(int)TileMap.planetID].Length; i++)
                {
                    if (Panel.mapId[(int)TileMap.planetID][i] == taskMapIdVal)
                    {
                        taskPointIdx = i;
                        break;
                    }
                }
                if (taskPointIdx >= 0 && GameCanvas.gameTick % 4 > 0)
                {
                    g.drawImage(ItemMap.imageFlare,
                        taskMapContentX + Panel.mapX[(int)TileMap.planetID][taskPointIdx],
                        taskMapContentY + Panel.mapY[(int)TileMap.planetID][taskPointIdx], 3);
                }
            }

            // Vẽ vị trí nhân vật (icon đầu + tên map hiện tại)
            if (charMapX >= 0)
            {
                int head = Char.myCharz().head;
                Part part = GameScr.parts[head];
                SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id,
                    taskMapContentX + charMapX, taskMapContentY + charMapY + 5, 0, 3);

                int nameAlign = mFont.CENTER;
                if (charMapX <= 40) nameAlign = mFont.LEFT;
                if (charMapX >= imgW - 40) nameAlign = mFont.RIGHT;
                mFont.tahoma_7b_yellow.drawString(g, TileMap.mapName,
                    taskMapContentX + charMapX, taskMapContentY + charMapY - 12, nameAlign, mFont.tahoma_7_grey);
            }

            // Vẽ vị trí đã click (icon bàn tay + tên địa điểm)
            if (taskMapClickedIndex >= 0 && taskMapClickedIndex < Panel.mapX[(int)TileMap.planetID].Length)
            {
                int clickedPx = Panel.mapX[(int)TileMap.planetID][taskMapClickedIndex] + taskMapContentX;
                int clickedPy = Panel.mapY[(int)TileMap.planetID][taskMapClickedIndex] + taskMapContentY;
                int clickedMapId = Panel.mapId[(int)TileMap.planetID][taskMapClickedIndex];

                // Vẽ icon bàn tay
                if (Panel.imgBantay != null)
                {
                    g.drawImage(Panel.imgBantay, clickedPx, clickedPy, StaticObj.TOP_RIGHT);
                }

                // Vẽ tên địa điểm
                if (TileMap.mapNames != null && clickedMapId >= 0 && clickedMapId < TileMap.mapNames.Length)
                {
                    int clickAlign = mFont.CENTER;
                    if (clickedPx - taskMapContentX + scrollX <= 30) clickAlign = mFont.LEFT;
                    if (clickedPx - taskMapContentX + scrollX >= imgW - 30) clickAlign = mFont.RIGHT;
                    mFont.tahoma_7b_yellow.drawString(g, TileMap.mapNames[clickedMapId],
                        clickedPx, clickedPy - 12, clickAlign, mFont.tahoma_7_grey);
                }
            }

            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);

            // Vẽ mũi tên chỉ hướng nhiệm vụ ngoài viewport (không cần translate)
            if (taskPointIdx >= 0)
            {
                int tpx = Panel.mapX[(int)TileMap.planetID][taskPointIdx];
                int tpy = Panel.mapY[(int)TileMap.planetID][taskPointIdx];
                if (tpx < scrollX)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 5,
                        taskMapContentX + 5, taskMapContentY + taskMapContentH / 2 - 4, 0);
                }
                if (tpx > scrollX + taskMapContentW)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 6,
                        taskMapContentX + taskMapContentW - 5, taskMapContentY + taskMapContentH / 2 - 4, StaticObj.TOP_RIGHT);
                }
                if (tpy < scrollY)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 1,
                        taskMapContentX + taskMapContentW / 2, taskMapContentY + 5, StaticObj.TOP_CENTER);
                }
                if (tpy > scrollY + taskMapContentH)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 0,
                        taskMapContentX + taskMapContentW / 2, taskMapContentY + taskMapContentH - 5, StaticObj.BOTTOM_HCENTER);
                }
            }
        }
        else
        {
            // Không có bản đồ
            mFont.tahoma_7_grey.drawString(g, "Không có bản đồ", mapBoxX + mapBoxW / 2, frameY + frameH / 2, mFont.CENTER);
        }
    }

    private static void PaintTaskProgress(mGraphics g, Task task, int x, int y, int w, int bottom)
    {
        if (task.subNames == null)
        {
            return;
        }
        for (int i = 0; i < task.subNames.Length && y < bottom; i++)
        {
            string name = task.subNames[i];
            if (name == null || name.Length == 0)
            {
                continue;
            }
            bool active = i == task.index;
            bool done = i < task.index;
            string progress = string.Empty;
            if (task.counts != null && i < task.counts.Length && task.counts[i] > 0)
            {
                int cur = active ? task.count : (done ? task.counts[i] : 0);
                progress = " (" + cur + "/" + task.counts[i] + ")";
            }
            int bulletColor = done ? 0x4CAF50 : (active ? 0xD89C21 : 0x8C6A3D);
            Fill(g, x, y + 3, 5, 5, bulletColor);
            mFont font = active ? mFont.tahoma_7b_dark : mFont.tahoma_7b_dark;
            font.drawString(g, name + progress, x + 10, y, mFont.LEFT);
            y += 11;
        }
    }

    private static void PaintEmptySlots(mGraphics g)
    {
        PaintBodySlots(g);
        PaintBagSlots(g);
    }

    private static void PaintBodySlots(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int leftX = safeX;
        int leftY = panelY + 42;
        int leftW = safeW / 2 - 16;
        int frameX = safeX - 3;
        int frameY = panelY + 46;
        int frameW = 246;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 8;
        int bodyGapY = 27;
        int slot = 26;
        int gapY = 30;
        PaintOldPanelBox(g, frameX, frameY, frameW, 179);
        Item[] body = (Char.myCharz() != null) ? Char.myCharz().arrItemBody : null;
        for (int i = 0; i < 5; i++)
        {
            int lx = bodyLeftX;
            int rx = bodyRightX;
            int y = bodyTopY + i * bodyGapY;
            PaintSlotRect(g, lx, y, 36, 24, selectedBodyIndex == i);
            PaintItemInSlot(g, GetItem(body, i), lx, y, 36, 24);
            PaintSlotRect(g, rx, y, 36, 24, selectedBodyIndex == i + 5);
            PaintItemInSlot(g, GetItem(body, i + 5), rx, y, 36, 24);
        }
        Char me = Char.myCharz();
        if (me != null)
        {
            PaintCharacterPreview(g, me, centerX, frameY + 121, false);
        }
        int bottomY = frameY + 148;
        int bottomW = 36;
        int bottomH = 24;
        int bottomGap = 2;
        for (int i = 0; i < 4; i++)
        {
            int x = centerX - 75 + i * (bottomW + bottomGap);
            PaintSlotRect(g, x, bottomY, bottomW, bottomH, selectedBodyIndex == i + 10);
            PaintItemInSlot(g, GetItem(body, i + 10), x, bottomY, bottomW, bottomH);
        }
    }

    private static void PaintBagSlots(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int rightX = safeX + safeW / 2 + 10;
        int rightY = panelY + 66;
        int slot = 26;
        int gap = 4;
        Item[] bag = (Char.myCharz() != null) ? Char.myCharz().arrItemBag : null;
        Item[] box = (Char.myCharz() != null) ? Char.myCharz().arrItemBox : null;
        Item[] items = (rightSubTab == 1) ? box : bag;
        int selectedIndex = (rightSubTab == 1) ? selectedBoxIndex : ((rightSubTab == 2) ? selectedAutoIndex : selectedBagIndex);
        int count = (items != null) ? items.Length : 0;
        int viewW = 34 * 6 + gap * 5;
        int viewH = 26 * 7 + gap * 6;
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        g.setClip(rightX, rightY, viewW, viewH);
        for (int i = 0; i < count; i++)
        {
            int col = i % 6;
            int row = i / 6;
            int x = rightX + col * (34 + gap);
            int y = rightY + row * (slot + gap) - bagScrollY;
            if (y + slot < rightY || y > rightY + viewH)
            {
                continue;
            }
            PaintSlot(g, x, y, slot, selectedIndex == i);
            PaintItemInSlot(g, GetItem(items, i), x, y, 34, slot);
            if (rightSubTab == 2 && GetItem(items, i) != null && AutoItem.mAutoItem.method_1(GetItem(items, i).template.id))
            {
                mFont.tahoma_7b_green.drawString(g, "A", x + 28, y + 16, mFont.CENTER);
            }
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
        PaintBagScrollBar(g, rightX + viewW + 3, rightY, viewH, GetBagMaxScroll(viewH));
    }

    private static void PaintSelectedItemInfo(mGraphics g)
    {
        Item item = null;
        if (selectedBagIndex >= 0 && Char.myCharz() != null)
        {
            item = GetItem(Char.myCharz().arrItemBag, selectedBagIndex);
        }
        else if (selectedBodyIndex >= 0)
        {
            Char owner = (topTab == 2) ? GetPrimaryPet() : Char.myCharz();
            if (owner != null)
            {
                item = GetItem(owner.arrItemBody, selectedBodyIndex);
            }
        }
        if (item == null || item.template == null)
        {
            return;
        }
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int leftX = safeX;
        int leftY = panelY + 42;
        int leftW = safeW / 2 - 16;
        int boxW = leftW - 4;
        int boxH = 52;
        int x = leftX + 2;
        int y = panelY + panelH - boxH - 14;
        Fill(g, x, y, boxW, boxH, 0xFFF1CF);
        g.setColor(0x7B4D1F);
        g.drawRect(x, y, boxW - 1, boxH - 1);
        PaintSlotRect(g, x + 6, y + 9, 34, 26, false);
        PaintItemInSlot(g, item, x + 6, y + 9, 34, 26);
        string name = item.template.name;
        if (name == null || name.Length == 0)
        {
            name = "Item";
        }
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, name, boxW - 54), x + 46, y + 6, mFont.LEFT);
        if (item.quantity > 1)
        {
            mFont.tahoma_7_yellow.drawString(g, "x" + item.quantity, x + boxW - 5, y + 6, mFont.RIGHT);
        }
        string optText = "Chưa có chỉ số";
        if (item.itemOption != null)
        {
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                if (item.itemOption[i] == null)
                {
                    continue;
                }
                string opt = item.itemOption[i].getOptionString();
                if (opt != null && opt.Length > 0)
                {
                    optText = opt;
                    break;
                }
            }
        }
        mFont.tahoma_7b_dark.drawString(g, TrimText(mFont.tahoma_7b_dark, optText, boxW - 54), x + 46, y + 22, mFont.LEFT);
        mFont.tahoma_7_yellow.drawString(g, selectedBagIndex >= 0 ? "Hành trang" : "Trang bị", x + 46, y + 36, mFont.LEFT);
    }

    private static void PaintSlot(mGraphics g, int x, int y, int size, bool selected)
    {
        PaintSlotRect(g, x, y, 34, size, selected);
    }

    private static void PaintSlotRect(mGraphics g, int x, int y, int w, int h, bool selected)
    {
        // Viền vàng đậm khi được chọn (giống item bag: 16383818)
        if (selected)
        {
            g.setColor(16383818);
            g.fillRect(x - 1, y - 1, w + 2, h + 2, 5);
            
            g.setColor(SELECT_BG);
            g.fillRect(x, y, w, h, 5);
        }
        else
        {
            g.setColor(6047789, 0.3f);
            g.fillRect(x, y, w, h, 5);
        }
    }

    private static void PaintBagScrollBar(mGraphics g, int x, int y, int h, int maxScroll)
    {
        if (maxScroll <= 0)
        {
            return;
        }
        Fill(g, x, y, 2, h, 0xB99A73);
        int thumbH = h * h / (h + maxScroll);
        if (thumbH < 14)
        {
            thumbH = 14;
        }
        int thumbY = y + bagScrollY * (h - thumbH) / maxScroll;
        Fill(g, x - 1, thumbY, 4, thumbH, 0x7B5A34);
    }

    private static Item GetItem(Item[] items, int index)
    {
        if (items == null || index < 0 || index >= items.Length)
        {
            return null;
        }
        return items[index];
    }

    private static void PaintItemInSlot(mGraphics g, Item item, int x, int y, int w, int h, bool isSelected = false)
    {
        if (item == null || item.template == null)
        {
            return;
        }
        if (GameCanvas.panel != null && topTab != 1)
        {
            GameCanvas.panel.customPaintEffectItem(g, item, x, y, w, h);
        }
        
        // Icon dao động khi được chọn (sử dụng thời gian hệ thống để mượt hơn)
        float offset = 0f;
        if (isSelected)
        {
            double time = (double)(mSystem.currentTimeMillis() % 10000L) / 1000.0;
            offset = (float)System.Math.Sin(time * 10.0) * 3.0f;
        }
            
        SmallImage.drawSmallImage(g, (int)item.template.iconID, x + w / 2, y + h / 2 + (int)offset, 0, 3);
        if (item.quantity > 1)
        {
            mFont.tahoma_7_yellow.drawString(g, string.Empty + item.quantity.ToString(), x + w, y + h - mFont.tahoma_7_yellow.getHeight(), mFont.RIGHT);
        }
    }

    private static Image GetTabImage(bool active, bool small)
    {
        if (!triedLoadTabs)
        {
            triedLoadTabs = true;
            try
            {
                imgTopTab = GameCanvas.loadImage("/custom_ui/top_tab.png");
                imgTopTabActive = GameCanvas.loadImage("/custom_ui/top_tab_active.png");
                imgSubTab = GameCanvas.loadImage("/custom_ui/subtab.png");
                imgSubTabActive = GameCanvas.loadImage("/custom_ui/subtab_active.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load tab images error: " + ex.Message);
                imgTopTab = null;
                imgTopTabActive = null;
                imgSubTab = null;
                imgSubTabActive = null;
            }
        }
        if (small)
        {
            return active ? imgSubTabActive : imgSubTab;
        }
        return active ? imgTopTabActive : imgTopTab;
    }

    private static Image GetEquipFrameImage()
    {
        if (!triedLoadEquipFrame)
        {
            triedLoadEquipFrame = true;
            try
            {
                imgEquipFrame = GameCanvas.loadImage("/custom_ui/equip_frame.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load equip frame error: " + ex.Message);
                imgEquipFrame = null;
            }
        }
        return imgEquipFrame;
    }

    private static Image GetStatsFrameImage()
    {
        if (!triedLoadStatsFrame)
        {
            triedLoadStatsFrame = true;
            try
            {
                imgStatsFrame = GameCanvas.loadImage("/custom_ui/stats_frame.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load stats frame error: " + ex.Message);
                imgStatsFrame = null;
            }
        }
        return imgStatsFrame;
    }

    private static Image GetCurrencyFrameImage()
    {
        if (!triedLoadCurrencyFrame)
        {
            triedLoadCurrencyFrame = true;
            try
            {
                imgCurrencyFrame = GameCanvas.loadImage("/custom_ui/currency_frame.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load currency frame error: " + ex.Message);
                imgCurrencyFrame = null;
            }
        }
        return imgCurrencyFrame;
    }

    private static Image GetTaskFrameImage()
    {
        if (!triedLoadTaskFrame)
        {
            triedLoadTaskFrame = true;
            try
            {
                imgTaskFrame = GameCanvas.loadImage("/custom_ui/task_frame.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load task frame error: " + ex.Message);
                imgTaskFrame = null;
            }
        }
        return imgTaskFrame;
    }

    private static string TrimText(mFont font, string text, int maxWidth)
    {
        if (font == null || text == null || text.Length == 0 || maxWidth <= 0)
        {
            return string.Empty;
        }
        if (font.getWidth(text) <= maxWidth)
        {
            return text;
        }
        string suffix = "...";
        int suffixW = font.getWidth(suffix);
        int len = text.Length;
        while (len > 0 && font.getWidth(text.Substring(0, len)) + suffixW > maxWidth)
        {
            len--;
        }
        return (len > 0) ? text.Substring(0, len) + suffix : suffix;
    }

    private static void Fill(mGraphics g, int x, int y, int w, int h, int color)
    {
        g.setColor(color);
        g.fillRect(x, y, w, h);
    }

    private static bool PaintPanelBg(mGraphics g)
    {
        if (!triedLoadPanelBg)
        {
            triedLoadPanelBg = true;
            try
            {
                imgPanelBg = GameCanvas.loadImage("/custom_ui/panel_bg.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load panel_bg error: " + ex.Message);
                imgPanelBg = null;
            }
        }
        if (imgPanelBg == null)
        {
            return false;
        }
        g.drawImage(imgPanelBg, panelX, panelY, 0);
        return true;
    }

    private static bool PaintPanelFrame9(mGraphics g)
    {
        if (!triedLoadPanel9)
        {
            triedLoadPanel9 = true;
            try
            {
                imgPanel9 = GameCanvas.loadImage("/custom_ui/panel_9slice.png");
            }
            catch (System.Exception ex)
            {
                Debug.Log("Load panel_9slice error: " + ex.Message);
                imgPanel9 = null;
            }
        }
        if (imgPanel9 == null)
        {
            return false;
        }
        Paint9Slice(g, imgPanel9, panelX, panelY, panelW, panelH, 16);
        return true;
    }

    private static void Paint9Slice(mGraphics g, Image img, int x, int y, int w, int h, int s)
    {
        if (img == null || w < s * 2 || h < s * 2)
        {
            return;
        }
        int oldClipX = g.clipX;
        int oldClipY = g.clipY;
        int oldClipW = g.clipW;
        int oldClipH = g.clipH;

        DrawPart(g, img, 0, 0, s, s, x, y, s, s);
        DrawPart(g, img, s * 2, 0, s, s, x + w - s, y, s, s);
        DrawPart(g, img, 0, s * 2, s, s, x, y + h - s, s, s);
        DrawPart(g, img, s * 2, s * 2, s, s, x + w - s, y + h - s, s, s);

        TilePart(g, img, s, 0, s, s, x + s, y, w - s * 2, s);
        TilePart(g, img, s, s * 2, s, s, x + s, y + h - s, w - s * 2, s);
        TilePart(g, img, 0, s, s, s, x, y + s, s, h - s * 2);
        TilePart(g, img, s * 2, s, s, s, x + w - s, y + s, s, h - s * 2);
        TilePart(g, img, s, s, s, s, x + s, y + s, w - s * 2, h - s * 2);

        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }

    private static void TilePart(mGraphics g, Image img, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
    {
        if (dw <= 0 || dh <= 0)
        {
            return;
        }
        int oldClipX = g.clipX;
        int oldClipY = g.clipY;
        int oldClipW = g.clipW;
        int oldClipH = g.clipH;
        g.setClip(dx, dy, dw, dh);
        for (int yy = dy; yy < dy + dh; yy += sh)
        {
            for (int xx = dx; xx < dx + dw; xx += sw)
            {
                g.drawRegion(img, sx, sy, sw, sh, 0, xx, yy, 0);
            }
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }

    private static void DrawPart(mGraphics g, Image img, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
    {
        int oldClipX = g.clipX;
        int oldClipY = g.clipY;
        int oldClipW = g.clipW;
        int oldClipH = g.clipH;
        g.setClip(dx, dy, dw, dh);
        g.drawRegion(img, sx, sy, sw, sh, 0, dx, dy, 0);
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }
}
