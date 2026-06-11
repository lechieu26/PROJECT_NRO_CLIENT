using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
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

    // --- Computed layout fields (recalculated every frame in ComputeLayout) ---
    private static int layoutSafeX;      // panelX + margin
    private static int layoutSafeW;      // panelW - margin*2
    private static int layoutLeftFrameX; // Left column frame X
    private static int layoutLeftFrameW; // Left column frame width
    private static int layoutRightX;     // Right column content X
    private static int layoutRightFrameX;// Right column frame X
    private static int layoutRightFrameW;// Right column frame width
    private static int layoutBagCols;    // Number of bag grid columns (5 or 6)
    private static int layoutBagGridW;   // Total bag grid pixel width
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
    private static bool bagDragged;
    private static int bagDragStartY;
    private static int bagScrollYBeforeDrag;
    private static bool downWhenRunning;
    private static bool draggingSkill;
    private static bool skillDragged;
    private static bool globalDragged;
    private static bool pendingSelectionClick;
    private static int bagElasticY;
    private static int clanMsgScrollY;
    private static int clanLogicScrollY;
    private static int selectedClanMsgIndex = -1;
    private static int selectedClanLogicIndex = -1;
    private static int selectedClanMenuIndex;
    private static bool isEditingSlogan;
    private static TField sloganTField;
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
    private static sbyte lastConfirmedPetStatus = -1;
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

            // ƯU TIÊN 1: Nếu có Dialog (như bảng hỏi Rời bang), dừng tất cả input của Panel để nhường cho Dialog
            if (GameCanvas.currentDialog != null)
            {
                return;
            }

            // Xử lý logic cho TabClanIcon nếu đang hiển thị
            if (GameCanvas.panel != null && GameCanvas.panel.tabIcon != null && GameCanvas.panel.tabIcon.isShow)
            {
                GameCanvas.panel.tabIcon.update();
                GameCanvas.panel.tabIcon.updateKey();
                return; 
            }

            SyncPetStateFlags();

            // 1. Lưu trạng thái và Tracking (Phải ở đầu)
            bool isDown = GameCanvas.isPointerDown;
            bool isRelease = GameCanvas.isPointerJustRelease;
            int px = GameCanvas.px;
            int py = GameCanvas.py;
            ComputeLayout();
            
            // 2. Cập nhật và xử lý Input cho các TField (Chat bang, Chat thế giới, Slogan)
            TField currentFocus = null;
            
            // Ràng buộc focus theo Tab hiện tại để tránh xung đột
            if (topTab == 5) // Tab Bang Hội
            {
                if (clanChatTField != null && clanChatTField.isFocus) currentFocus = clanChatTField;
                else if (selectedClanMenuIndex == 1 && sloganTField != null && sloganTField.isFocus) currentFocus = sloganTField;
                
                // Tắt focus của worldChat nếu đang ở tab Bang
                if (worldChatTField != null) worldChatTField.isFocus = false;
            }
            else if (topTab == 7) // Tab Công cụ (Chat thế giới)
            {
                if (worldChatTField != null && worldChatTField.isFocus) currentFocus = worldChatTField;
                
                // Tắt focus của clanChat/slogan nếu đang ở tab Công cụ
                if (clanChatTField != null) clanChatTField.isFocus = false;
                if (sloganTField != null) sloganTField.isFocus = false;
            }
            else
            {
                // Nếu ở các tab khác, tắt toàn bộ focus
                if (clanChatTField != null) clanChatTField.isFocus = false;
                if (worldChatTField != null) worldChatTField.isFocus = false;
                if (sloganTField != null) sloganTField.isFocus = false;
            }

            // Chỉ update các field có khả năng hiển thị để tối ưu và tránh lỗi con trỏ
            if (topTab == 5)
            {
                if (clanChatTField != null) clanChatTField.update();
                if (selectedClanMenuIndex == 1 && sloganTField != null) sloganTField.update();
            }
            else if (topTab == 7)
            {
                if (worldChatTField != null) worldChatTField.update();
            }

            // 3. Tiêu thụ phím nhấn để chặn phím tắt toàn cục (P, B, C...) khi Panel đang mở
            if (GameCanvas.keyAsciiPress != 0)
            {
                int k = GameCanvas.keyAsciiPress;
                if (currentFocus != null)
                {
                    // Nếu đang focus ô nhập liệu, xử lý gõ phím
                    if (k == 10 || k == -5) // Enter hoặc phím chọn
                    {
                        if (currentFocus == clanChatTField) HandleSendClanChat();
                        else if (currentFocus == worldChatTField) HandleSendWorldChat();
                        else if (currentFocus == sloganTField) HandleSaveSlogan();
                        SoundMn.gI().panelClick();
                    }
                    else
                    {
                        currentFocus.keyPressed(k);
                    }
                }
                // LUÔN LUÔN xóa phím sau khi xử lý (hoặc để chặn nếu không có focus)
                GameCanvas.keyAsciiPress = 0;
            }

            // 4. Xử lý các phím chức năng đặc biệt (Xóa, Enter/OK từ keyPressed)
            if (currentFocus != null)
            {
                // Phím Xóa (Backspace)
                if (GameCanvas.keyPressed[14] || GameCanvas.keyPressed[8] || GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21])
                {
                    currentFocus.keyPressed(8);
                    GameCanvas.keyPressed[14] = false;
                    GameCanvas.keyPressed[8] = false;
                    GameCanvas.keyPressed[(!Main.isPC) ? 2 : 21] = false;
                    GameCanvas.clearKeyPressed();
                }

                // Phím Enter/OK bổ sung từ keyPressed
                if (GameCanvas.keyPressed[(!Main.isPC) ? 5 : 25])
                {
                    if (currentFocus == clanChatTField) HandleSendClanChat();
                    else if (currentFocus == worldChatTField) HandleSendWorldChat();
                    else if (currentFocus == sloganTField) HandleSaveSlogan();
                    SoundMn.gI().panelClick();
                    GameCanvas.clearKeyPressed();
                }
            }

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
            if (topTab == 6)
            {
                UpdateModScroll();
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

                if ((wasInteracting && !globalDragged && GameCanvas.isPointerJustRelease) || pendingSelectionClick)
                {
                    HandlePanelSelection(true);
                    BlockGameInput();
                }

                // Reset tracking flags sau khi nhả
                globalDragged = false;
                pendingSelectionClick = false;
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
        // --- Panel size: fill available space, cap at 540×330 ---
        panelW = (GameCanvas.w - 16 < 540) ? (GameCanvas.w - 16) : 540;
        panelH = (GameCanvas.h - 16 < 330) ? (GameCanvas.h - 16) : 330;
        if (panelW < 280) panelW = 280;
        if (panelH < 220) panelH = 220;
        panelX = (GameCanvas.w - panelW) / 2;
        panelY = (GameCanvas.h - panelH) / 2;
        if (panelY < 4) panelY = 4;
        if (panelX < 2) panelX = 2;

        // --- Computed sub-layout ---
        int margin = (panelW >= 400) ? 24 : (panelW >= 320 ? 16 : 10);
        layoutSafeX = panelX + margin;
        layoutSafeW = panelW - margin * 2;

        int halfW = layoutSafeW / 2;
        int colGap = 6;

        // Right column: determine frame size first to fill remaining space
        layoutRightFrameX = layoutSafeX + halfW + colGap / 2;
        layoutRightFrameW = layoutSafeW - halfW - colGap / 2;

        int slotW = 34;
        int slotGap = 4;
        
        // Luôn giữ 6 cột (hoặc 5 nếu cực kỳ hẹp)
        int tryGridW6 = slotW * 6 + slotGap * 5; // 224
        int tryGridW5 = slotW * 5 + slotGap * 4; // 186
        
        if (layoutRightFrameW >= tryGridW6 + 10)
        {
            layoutBagCols = 6;
            layoutBagGridW = tryGridW6;
        }
        else
        {
            layoutBagCols = 5;
            layoutBagGridW = tryGridW5;
        }

        // Center the grid inside the right frame
        layoutRightX = layoutRightFrameX + (layoutRightFrameW - layoutBagGridW) / 2;

        // Left column: fill remaining space on the left, but respect margin
        layoutLeftFrameX = layoutSafeX;
        layoutLeftFrameW = layoutRightFrameX - layoutLeftFrameX - colGap;
        if (layoutLeftFrameW < 140) layoutLeftFrameW = 140;
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

    private static Char GetPrimaryPet()
    {
        Char me = Char.myCharz();
        if (me == null) return Char.myPetz();
        if (me.havePet2 && !me.havePet) return Char.MyPet2z();
        // Nếu có cả 2, mặc định hiển thị Pet 1, hoặc có thể thêm nút chuyển đổi sau.
        return Char.myPetz();
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

    private static readonly string[] TOOL_GROUPS = new string[] { "Hệ thống", "Di chuyển", "Chat thế giới", "Tài khoản" };

    // Sub-action cho Tài khoản: khi chọn Bạn bè/Kẻ thù thì hiển thị danh sách trong box 3
    private static int selectedAccountSubAction = -1; // -1: chưa chọn, 1: bạn bè, 2: kẻ thù
    private static int friendEnemySelected = -1;
    private static int friendEnemyDragStartY;
    private static bool friendEnemyDragged;

    private static TField worldChatTField;
    private static TField clanChatTField;

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

    private class ClanChatAction : IActionListener
    {
        public void perform(int idAction, object p)
        {
            if (idAction == 0) HandleSendClanChat();
            else if (idAction == 1)
            {
                if (GameCanvas.panel != null && GameCanvas.panel.chatTField != null)
                    GameCanvas.panel.chatTField.isShow = false;
            }
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

    private class LeaveClanAction : IActionListener
    {
        public void perform(int idAction, object p)
        {
            Service.gI().clanMessage(2, null, -1);
            GameCanvas.endDlg();
        }
    }
}
