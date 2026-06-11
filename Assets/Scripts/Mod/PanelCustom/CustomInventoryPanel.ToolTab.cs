using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
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
        int safeX = layoutSafeX;
        int safeY = panelY + 42;
        int safeW = layoutSafeW;
        int safeH = panelH - 82;
        int gap = 6;
        bool showDetail = selectedToolAction >= 0;
        if (selectedToolGroupIndex == 3)
            showDetail = (selectedAccountSubAction == 1 || selectedAccountSubAction == 2);
        
        bool fullDetail = selectedToolAction == 100;
        int catW = 108;
        int detailW = fullDetail ? (safeW - catW - gap) : (showDetail ? 164 : 0);
        int listW = fullDetail ? 0 : (safeW - catW - gap - (showDetail ? detailW + gap : 0));
        int catX = safeX;
        int listX = catX + catW + gap;
        int detailX = fullDetail ? listX : (listX + listW + gap);

        PaintOldPanelBox(g, catX, safeY, catW, safeH);
        if (!fullDetail)
        {
            PaintOldPanelBox(g, listX, safeY, listW, safeH);
        }
        if (showDetail || fullDetail)
        {
            PaintOldPanelBox(g, detailX, safeY, detailW, safeH);
        }
        mFont.tahoma_7b_dark.drawString(g, "NHÓM", catX + catW / 2, safeY + 6, mFont.CENTER);
        if (!fullDetail)
        {
            mFont.tahoma_7b_dark.drawString(g, "CHỨC NĂNG", listX + listW / 2, safeY + 6, mFont.CENTER);
        }
        PaintToolGroups(g, catX + 6, safeY + 24, catW - 12, safeH - 30);
        if (!fullDetail)
        {
            PaintToolList(g, listX + 6, safeY + 24, listW - 12, safeH - 30);
        }
        if (showDetail || fullDetail)
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
        else if (selectedToolAction == 100) PaintWorldChatDetail(g, x, y, w, h);
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

    private static void PaintWorldChatDetail(mGraphics g, int x, int y, int w, int h)
    {
        Panel p = GameCanvas.panel;
        int inputH = 28;
        int titleH = 20; // Khoảng cách cho tiêu đề "Chat thế giới"
        int msgY = y + titleH; 
        int msgH = h - inputH - titleH - 10; // Tính toán lại vùng tin nhắn để không đè lên input
        
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();

        if (p == null || p.logChat == null || p.logChat.size() == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Chưa có lịch sử chat thế giới", x + w / 2, y + h / 2 - 10, mFont.CENTER);
            mFont.tahoma_7_grey.drawString(g, "Nhấn ô bên dưới để gửi tin", x + w / 2, y + h / 2 + 4, mFont.CENTER);
        }
        else
        {
            MyVector logs = p.logChat;
            int rowH = 32;

            g.setClip(x, msgY, w, msgH);
            g.translate(0, -toolDetailScrollY);

            for (int i = 0; i < logs.size(); i++)
            {
                InfoItem infoItem = (InfoItem)logs.elementAt(i);
                int yy = msgY + i * rowH;

                // Culling
                if (yy + rowH < msgY + toolDetailScrollY || yy > msgY + toolDetailScrollY + msgH) continue;

                int headX = x + 10;
                int headY = yy + 4;
                int textX = headX + 34;

                // Vẽ avatar đầu nhân vật
                if (infoItem.charInfo != null)
                {
                    if (infoItem.charInfo.headICON != -1)
                    {
                        SmallImage.drawSmallImage(g, infoItem.charInfo.headICON, headX, headY, 0, 0);
                    }
                    else
                    {
                        Part part = GameScr.parts[infoItem.charInfo.head];
                        if (part != null && part.pi != null && part.pi.Length > 0)
                        {
                            SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id, 
                                headX + (int)part.pi[Char.CharInfo[0][0][0]].dx, 
                                headY + (int)part.pi[Char.CharInfo[0][0][0]].dy, 0, 0);
                        }
                    }

                    // Tên nhân vật
                    mFont.tahoma_7b_green2.drawString(g, (infoItem.charInfo.isTichXanh ? "     " : string.Empty) + infoItem.charInfo.cName, textX, yy + 2, 0);
                }

                // Nội dung chat
                string msg = "";
                try { 
                    string[] parts = Res.split(infoItem.s, "|", 0);
                    msg = (parts.Length > 2) ? parts[2] : infoItem.s; 
                } catch { msg = infoItem.s; }
                
                mFont font = infoItem.isChatServer ? mFont.tahoma_7_red : mFont.tahoma_7_blue;
                font.drawString(g, TrimText(font, msg, w - 50), textX, yy + 16, 0);
            }

            g.translate(0, toolDetailScrollY);
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH); // Reset clip trước khi vẽ input

        // Áp dụng pattern giống PaintClanTab cho Chat thế giới
        g.setClip(x - 2, y - 2, w + 4, h + 4);
        PaintWorldChatInput(g, x, y + h - inputH - 5, w, inputH);
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
    }

    private static void PaintWorldChatInput(mGraphics g, int x, int y, int w, int h)
    {
        EnsureWorldChatTField();
        int sendW = 50;
        
        // Cập nhật hitbox cho việc click
        worldChatTField.x = x;
        worldChatTField.y = y;
        worldChatTField.width = w - sendW - 5;
        worldChatTField.height = h;

        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();

        // 1. Vẽ ô nhập liệu
        // Vẽ khung: Nếu focus thì dùng màu SELECT_BORDER (0x79A96B)
        g.setColor(worldChatTField.isFocus ? 0x79A96B : 0x9993045);
        g.fillRect(x, y, w - sendW - 5, h, 4); 
        g.setColor(0xFFFFFF); // Nền trắng cho ô nhập
        g.fillRect(x + 1, y + 1, w - sendW - 7, h - 2, 4);

        string draft = GetWorldChatDraft();
        
        g.setClip(x + 2, y + 1, w - sendW - 7, h - 2);
        string displayPlaceholder = "Nhập nội dung...";
        string displayText = string.IsNullOrEmpty(draft) ? displayPlaceholder : draft;
        mFont font = string.IsNullOrEmpty(draft) ? mFont.tahoma_7_grey : mFont.tahoma_7b_dark;
        
        int textX = x + 12 + worldChatTField.offsetX;
        font.drawString(g, TrimText(font, displayText, w - sendW - 25), textX, y + h / 2 - 4, mFont.LEFT);
        
        // Vẽ con trỏ nhấp nháy nếu đang focus
        if (worldChatTField.isFocus && (mSystem.currentTimeMillis() / 500) % 2 == 0)
        {
            int tw = font.getWidth(string.IsNullOrEmpty(draft) ? "" : draft.Substring(0, worldChatTField.caretPos));
            g.setColor(0x000000);
            g.fillRect(textX + tw, y + 5, 1, h - 10);
        }
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);

        // 2. Vẽ nút Gửi
        int btnSendX = x + w - sendW;
        bool isHoverSend = Hit(GameCanvas.px, GameCanvas.py, btnSendX, y, sendW, h);
        g.setColor(isHoverSend ? 0x5EB432 : 0x4C9D27);
        g.fillRect(btnSendX, y, sendW, h, 4);
        mFont.tahoma_7b_white.drawString(g, "Gửi", btnSendX + sendW / 2, y + h / 2 - 4, mFont.CENTER);
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
        int safeX = layoutSafeX;
        int safeY = panelY + 45;
        int safeW = layoutSafeW;
        int safeH = panelH - 82;
        int gap = 6;
        bool showDetail = selectedToolAction >= 0;
        if (selectedToolGroupIndex == 3)
            showDetail = (selectedAccountSubAction == 1 || selectedAccountSubAction == 2);
        
        bool fullDetail = selectedToolAction == 100;
        int catW = 108;
        int detailW = fullDetail ? (safeW - catW - gap) : (showDetail ? 164 : 0);
        int listW = fullDetail ? 0 : (safeW - catW - gap - (showDetail ? detailW + gap : 0));
        int catX = safeX;
        int listX = catX + catW + gap;
        int detailX = fullDetail ? listX : (listX + listW + gap);

        if (GameCanvas.px >= catX && GameCanvas.px <= catX + catW && GameCanvas.py >= safeY && GameCanvas.py <= safeY + safeH)
        {
            int row = (GameCanvas.py - (safeY + 24)) / 29;
            if (row >= 0 && row < TOOL_GROUPS.Length)
            {
                if (!isFire) return true;
                selectedToolGroupIndex = row;
                if (row == 3) p.setTypeAccount();
                selectedToolAction = (row == 2) ? 100 : -1;
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
                    selectedToolAction = 100;
                    selectedToolDetailIndex = -1;
                    toolDetailScrollY = 0;
                    toolDetailScrollTargetY = 0;
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
        else if ((showDetail || selectedToolAction == 100) && GameCanvas.px >= detailX && GameCanvas.px <= detailX + detailW && GameCanvas.py >= safeY && GameCanvas.py <= safeY + safeH)
        {
            if (selectedToolAction == 100)
            {
                int inputH = 28;
                int inputY = safeY + safeH - inputH - 7; // Khớp hoàn toàn với PaintWorldChatDetail (detailY + detailH - inputH - 5)
                int btnSendW = 50;
                int btnSendX = detailX + 6 + detailW - 12 - btnSendW;

                if (isFire)
                {
                    // Vùng click ô nhập liệu
                    if (Hit(GameCanvas.px, GameCanvas.py, detailX + 6, inputY, detailW - 12 - btnSendW - 4, inputH))
                    {
                        EnsureWorldChatTField();
                        worldChatTField.setFocusWithKb(true);
                        SoundMn.gI().panelClick();
                    }
                    // Vùng click nút Gửi
                    else if (Hit(GameCanvas.px, GameCanvas.py, btnSendX, inputY, btnSendW, inputH))
                    {
                        HandleSendWorldChat();
                        SoundMn.gI().panelClick();
                    }
                    else
                    {
                        if (worldChatTField != null) worldChatTField.isFocus = false;
                    }
                }
                return true;
            }

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

    private static void HandleSendWorldChat()
    {
        EnsureWorldChatTField();
        string text = worldChatTField.getText();
        if (!string.IsNullOrEmpty(text))
        {
            Service.gI().chatGlobal(text);
            worldChatTField.setText(string.Empty);
        }
        worldChatTField.isFocus = false;
    }


    private static string GetWorldChatDraft()
    {
        return (worldChatTField != null) ? worldChatTField.getText() : string.Empty;
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

    private static bool ToolActionHasDetail(int action)
    {
        if (action == 7 && selectedAccountSubAction > 0) return true;
        return action == 0 || action == 4 || action == 5 || action == 7 || action == 8 || action == 100;
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
        if (selectedToolAction == 100) return GetCommunicationTitle();
        return "CHI TIẾT";
    }

    private static string GetCommunicationTitle()
    {
        return "CHAT THẾ GIỚI";
    }

    private static void EnsureWorldChatTField()
    {
        if (worldChatTField == null)
        {
            worldChatTField = new TField();
            worldChatTField.setIputType(TField.INPUT_TYPE_ANY);
            worldChatTField.name = "Nhập nội dung...";
            worldChatTField.isFocus = false;
        }
    }

}
