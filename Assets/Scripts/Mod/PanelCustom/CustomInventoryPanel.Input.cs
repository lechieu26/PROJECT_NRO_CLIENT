using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{

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

        int rightX = layoutRightX;
        int rightY = panelY + 66;
        int gap = 4;
        int cols = layoutBagCols;
        int viewW = layoutBagGridW;
        int viewH = panelH - 132;
        int maxScroll = GetBagMaxScroll(viewH);
        
        bool inside = GameCanvas.px >= rightX && GameCanvas.px <= rightX + viewW && GameCanvas.py >= rightY && GameCanvas.py <= rightY + viewH;
        
        if ((inside || draggingBag) && GameCanvas.isPointerDown)
        {
            if (!draggingBag)
            {
                bagDragStartY = GameCanvas.py;
                draggingBag = true;
                bagDragged = false;
                downWhenRunning = (bagScrollRun != 0);
                bagScrollRun = 0;
                bagScrollCmdy = 0;
                bagScrollCmvy = 0;
                bagScrollTargetY = bagScrollY;
            }
            else
            {
                if (System.Math.Abs(GameCanvas.py - bagDragStartY) > 5)
                {
                    bagDragged = true;
                    globalDragged = true;
                }

                if (bagDragged)
                {
                    int dy = GameCanvas.py - dragLastY[0];
                    if (maxScroll == 0 || (bagScrollY <= 0 && dy > 0) || (bagScrollY >= maxScroll && dy < 0))
                    {
                        bagElasticY += dy / 2;
                        if (bagElasticY > 28) bagElasticY = 28;
                        if (bagElasticY < -28) bagElasticY = -28;
                    }
                    else
                    {
                        bagScrollTargetY -= dy;
                        if (bagScrollTargetY < 0) bagScrollTargetY = 0;
                        if (bagScrollTargetY > maxScroll) bagScrollTargetY = maxScroll;
                        
                        bagScrollY = bagScrollTargetY;
                    }
                }
            }
        }
        
        if (!GameCanvas.isPointerJustRelease || !draggingBag)
        {
            ApplyBagScrollRun(maxScroll);
            return;
        }
        
        int releaseDelta = GameCanvas.py - dragLastY[0];
        GameCanvas.isPointerJustRelease = false;
        if (System.Math.Abs(releaseDelta) < 20 && System.Math.Abs(GameCanvas.py - bagDragStartY) < 20 && !downWhenRunning)
        {
            pendingSelectionClick = true;
            bagScrollRun = 0;
            bagScrollTargetY = bagScrollY;
        }
        else if (!downWhenRunning)
        {
            if (bagScrollY < 0)
            {
                bagScrollTargetY = 0;
            }
            else if (bagScrollY > maxScroll)
            {
                bagScrollTargetY = maxScroll;
            }
            else
            {
                int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
                if (force > 15) force = 15;
                if (force < -15) force = -15;
                bagScrollRun = -force * 150;
            }
        }
        
        draggingBag = false;
        bagDragged = false;
        dragTime = 0;
        GameCanvas.isPointerJustRelease = false;
        ApplyBagScrollRun(maxScroll);
    }

    private static void ApplyBagScrollRun(int maxScroll)
    {
        if (!draggingBag && bagElasticY != 0)
        {
            bagElasticY = bagElasticY * 3 / 4;
            if (bagElasticY > -1 && bagElasticY < 1)
            {
                bagElasticY = 0;
            }
        }

        if (bagScrollRun != 0 && !draggingBag)
        {
            bagScrollTargetY += bagScrollRun / 100;
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
            bagScrollRun = bagScrollRun * 9 / 10;
            if (bagScrollRun < 100 && bagScrollRun > -100)
            {
                bagScrollRun = 0;
            }
        }
        
        if (bagScrollY != bagScrollTargetY && !draggingBag)
        {
            bagScrollCmvy = bagScrollTargetY - bagScrollY << 2;
            bagScrollCmdy += bagScrollCmvy;
            bagScrollY += bagScrollCmdy >> 4;
            bagScrollCmdy &= 15;
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
                if (System.Math.Abs(dy) > 5)
                {
                    globalDragged = true;
                }
                popupScrollTargetY -= dy;
                
                // Giới hạn đàn hồi khi đang kéo
                if (popupScrollTargetY < -60) popupScrollTargetY = -60;
                if (popupScrollTargetY > maxScroll + 60) popupScrollTargetY = maxScroll + 60;
                
                popupScrollY = popupScrollTargetY;
            }
        }
        else if (GameCanvas.isPointerJustRelease && popupDragged)
        {
            int releaseDelta = GameCanvas.py - dragLastY[0];
            GameCanvas.isPointerJustRelease = false;
            if (System.Math.Abs(releaseDelta) < 20 && System.Math.Abs(GameCanvas.py - popupDragStartY) < 20)
            {
                // Tap nhẹ vào popup (thường là để đóng)
                pendingSelectionClick = true;
                popupScrollRun = 0;
            }
            else
            {
                int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
                if (force > 15) force = 15;
                if (force < -15) force = -15;
                popupScrollRun = -force * 150;
            }
            popupDragged = false;
            GameCanvas.isPointerJustRelease = false;
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

        int safeY = panelY + 45;
        int safeH = panelH - 82;
        int detailH = safeH - 30;

        // Recalculate detail area for hit testing
        int safeX = layoutSafeX;
        int safeW = layoutSafeW;
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
        else if (selectedToolAction == 100)
        {
            count = (p != null && p.logChat != null) ? p.logChat.size() : 0;
            rowH = 32;
            detailH -= 34; 
        }

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
            int releaseDelta = GameCanvas.py - dragLastY[0];
            GameCanvas.isPointerJustRelease = false;
            if (System.Math.Abs(releaseDelta) < 20 && System.Math.Abs(GameCanvas.py - toolDetailDragStartY) < 20)
            {
                pendingSelectionClick = true;
                toolDetailScrollRun = 0;
            }
            toolDetailDragged = false;
            GameCanvas.isPointerJustRelease = false;
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
        int gap = 2;
        int maxTabW = (panelW - 20 - (visibleTabs.Length - 1) * gap) / visibleTabs.Length;
        int tabW = (maxTabW > 60) ? 60 : maxTabW;
        if (tabW < 36) tabW = 36;
        int tabH = 18;
        int totalTabsW = visibleTabs.Length * tabW + (visibleTabs.Length - 1) * gap;
        int tabX = panelX + (panelW - totalTabsW) / 2;
        int tabY = panelY + 22;
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
                modScrollY = 0;
                modScrollTargetY = 0;
                modScrollRun = 0;
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
        int rightX = layoutRightX;
        int y = panelY + 44;
        int gridW = layoutBagGridW;
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

    private static void UpdateSelectedItemPosition()
    {
        if (GameCanvas.panel == null || GameCanvas.panel.cp == null || selectedItemInfo == null)
        {
            return;
        }

        int rightX = layoutRightX;
        int rightY = panelY + 66;
        int gap = 4;
        int cols = layoutBagCols;
        int newY = -1;

        if (topTab == 4)
        {
            if (rightSubTab == 0 && selectedBagIndex >= 0)
            {
            int col = selectedBagIndex % cols;
            int row = selectedBagIndex / cols;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
            else if (rightSubTab == 1 && selectedBoxIndex >= 0)
            {
                int col = selectedBoxIndex % cols;
                int row = selectedBoxIndex / cols;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
            else if (rightSubTab == 2 && selectedAutoIndex >= 0)
            {
                int col = selectedAutoIndex % cols;
                int row = selectedAutoIndex / cols;
                selectedItemX = rightX + col * (34 + gap);
                newY = rightY + row * (26 + gap) - bagScrollY;
            }
        }
        else if (topTab == 3 && selectedSkillIndex >= 0)
        {
            int colW = (layoutSafeW - 6) / 3;
            int col = selectedSkillIndex / 6;
            int row = selectedSkillIndex % 6;
            selectedItemX = layoutSafeX + col * (colW + 3) + 3;
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
        int rightX = layoutRightX;
        int rightY = panelY + 66;
        int gap = 4;
        int cols = layoutBagCols;
        int bagViewW = layoutBagGridW;
        int bagViewH = panelH - 132;
        if (GameCanvas.px >= rightX && GameCanvas.px <= rightX + bagViewW && GameCanvas.py >= rightY && GameCanvas.py <= rightY + bagViewH)
        {
            int localX = GameCanvas.px - rightX;
            int localY = GameCanvas.py - rightY + bagScrollY;
            int col = localX / (34 + gap);
            int row = localY / (26 + gap);
            if (col >= 0 && col < cols && localX % (34 + gap) < 34)
            {
                int index = row * cols + col;
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
        int leftX = layoutSafeX;
        int leftY = panelY + 42;
        int leftW = layoutLeftFrameW;
        int frameX = layoutLeftFrameX;
        int frameY = panelY + 42;
        int frameW = layoutLeftFrameW;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 19;
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
            int bottomY = frameY + 159;
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

}
