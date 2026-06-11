using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
    private static void PaintClanTab(mGraphics g)
    {
        int safeX = layoutSafeX;
        int safeY = panelY + 45;
        int safeW = layoutSafeW;
        int safeH = panelH - 82;
        int gap = 6;
        int msgW = safeW / 2 - gap / 2;
        int logicW = safeW - msgW - gap;
        int msgX = safeX;
        int logicX = msgX + msgW + gap;
        
        int inputH = 25;
        int msgListH = safeH - inputH - 32; 
        
        PaintOldPanelBox(g, msgX, safeY, msgW, safeH);
        PaintOldPanelBox(g, logicX, safeY, logicW, safeH);
        
        mFont.tahoma_7b_dark.drawString(g, "TIN NHẮN BANG", msgX + msgW / 2, safeY + 6, mFont.CENTER);
        
        // Vẽ danh sách tin nhắn
        PaintClanMessages(g, msgX + 6, safeY + 24, msgW - 12, msgListH);
        
        // Vẽ ô chat ở dưới cùng
        g.setClip(msgX + 4, safeY + 4, msgW - 8, safeH - 8);
        PaintClanChatInput(g, msgX + 6, safeY + safeH - inputH - 6, msgW - 12, inputH);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
        
        PaintClanMenu(g, logicX + 6, safeY + 6, logicW - 12, 22);
        PaintClanLogic(g, logicX + 6, safeY + 34, logicW - 12, safeH - 40);
    }

    private static void PaintClanChatInput(mGraphics g, int x, int y, int w, int h)
    {
        EnsureClanChatTField();
        int btnAskW = 28;
        int btnSendW = 28;
        int gap = 2;
        int tfW = w - btnAskW - btnSendW - gap * 2;
        
        // 1. Vẽ nút Xin đậu (bên trái)
        int btnAskX = x;
        bool isHoverAsk = Hit(GameCanvas.px, GameCanvas.py, btnAskX, y, btnAskW, h);
        g.setColor(isHoverAsk ? 0xFFA500 : 0xCD853F);
        g.fillRect(btnAskX, y, btnAskW, h, 4);
        mFont.tahoma_7_white.drawString(g, "Xin", btnAskX + btnAskW / 2, y + 1, mFont.CENTER);
        mFont.tahoma_7_white.drawString(g, "đậu", btnAskX + btnAskW / 2, y + 10, mFont.CENTER);

        // 2. Vẽ ô nhập liệu (giữa) - Vẽ thủ công để tránh TField.paint lấn chiếm clip
        clanChatTField.x = x + btnAskW + gap;
        clanChatTField.y = y;
        clanChatTField.width = tfW;
        clanChatTField.height = h;
        
        g.setColor(0x000000);
        g.fillRect(clanChatTField.x, clanChatTField.y, clanChatTField.width, clanChatTField.height, 4);
        g.setColor(0xFFFFFF);
        g.fillRect(clanChatTField.x + 1, clanChatTField.y + 1, clanChatTField.width - 2, clanChatTField.height - 2, 4);
        
        string text = clanChatTField.getText();
        int textX = clanChatTField.x + 4 + clanChatTField.offsetX;
        
        g.setClip(clanChatTField.x + 2, clanChatTField.y + 1, clanChatTField.width - 4, clanChatTField.height - 2);
        if (string.IsNullOrEmpty(text))
        {
            mFont.tahoma_7_grey.drawString(g, clanChatTField.name, clanChatTField.x + 4, clanChatTField.y + h / 2 - 4, mFont.LEFT);
        }
        else
        {
            mFont.tahoma_7b_dark.drawString(g, text, textX, clanChatTField.y + h / 2 - 4, mFont.LEFT);
        }
        
        // Vẽ con trỏ nhấp nháy khi focus
        if (clanChatTField.isFocus && (mSystem.currentTimeMillis() / 500) % 2 == 0)
        {
            string textBeforeCaret = text.Substring(0, clanChatTField.caretPos);
            int tw = mFont.tahoma_7b_dark.getWidth(textBeforeCaret);
            g.setColor(0x000000);
            g.fillRect(textX + tw, clanChatTField.y + 4, 1, h - 8);
        }
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);

        // 3. Vẽ nút Gửi (bên phải)
        int btnSendX = x + w - btnSendW;
        bool isHoverSend = Hit(GameCanvas.px, GameCanvas.py, btnSendX, y, btnSendW, h);
        g.setColor(isHoverSend ? 0x5EB432 : 0x4C9D27);
        g.fillRect(btnSendX, y, btnSendW, h, 4);
        mFont.tahoma_7_white.drawString(g, "Gửi", btnSendX + btnSendW / 2, y + h / 2 - 4, mFont.CENTER);
    }

    private static void PaintClanMenu(mGraphics g, int x, int y, int w, int h)
    {
        string[] tabs = GetClanMenuLabels();
        if (tabs == null || tabs.Length == 0) return;
        
        int nTabs = tabs.Length;
        int tabW = w / nTabs;
        for (int i = 0; i < nTabs; i++)
        {
            int xx = x + i * tabW;
            // Tab cuối cùng lấy phần còn lại để khớp hoàn toàn với chiều rộng w
            int currTabW = (i == nTabs - 1) ? (x + w - xx) : tabW;
            
            bool selected = (i == selectedClanMenuIndex);
            
            // Vẽ nền nút
            g.setColor(selected ? SELECT_BG : 15723751);
            g.fillRect(xx, y, currTabW - 1, h, 5);
            
            // Viền bóng ở trên (Highlight)
            g.setColor(0xFFFFFF);
            g.fillRect(xx + 1, y + 1, currTabW - 3, 2, 5);
            
            // Vẽ viền nếu không được chọn
            if (!selected)
            {
                g.setColor(0xCCCCCC);
                g.drawRect(xx, y, currTabW - 1, h);
            }

            mFont menuFont = selected ? mFont.tahoma_7b_dark : mFont.tahoma_7_blue;
            string label = tabs[i];
            
            // Vẽ chữ (tự động xuống dòng nếu có dấu cách và tab hẹp)
            int spaceIndex = label.IndexOf(' ');
            if (spaceIndex > 0 && currTabW < 50)
            {
                menuFont.drawString(g, label.Substring(0, spaceIndex), xx + currTabW / 2, y + 1, mFont.CENTER);
                menuFont.drawString(g, label.Substring(spaceIndex + 1), xx + currTabW / 2, y + 10, mFont.CENTER);
            }
            else
            {
                menuFont.drawString(g, label, xx + currTabW / 2, y + h / 2 - 4, mFont.CENTER);
            }
        }
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
        if (HasClanData() && selectedClanMenuIndex == 1)
        {
            PaintClanInfo(g, x, y, w, h);
            return;
        }

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
        if (c == null) return;
        
        if (!HasClanData())
        {
            if (p == null || p.clans == null || row < 0 || row >= p.clans.Length) return;
            Clan clan = p.clans[row];
            if (clan == null) return;
            
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
        if (members == null || row < 0 || row >= members.size()) return;
        Member member = (Member)members.elementAt(row);
        if (member == null) return;
        
        // Vẽ Icon thành viên
        int iconX = x;
        int iconY = y + 7; 
        if (member.headICON != -1)
        {
            SmallImage.drawSmallImage(g, member.headICON, iconX, iconY, 0, StaticObj.VCENTER_HCENTER);
        }
        else if (member.head >= 0)
        {
            Part part = (member.head < GameScr.parts.Length) ? GameScr.parts[(int)member.head] : null;
            if (part != null && part.pi != null && part.pi.Length > 0)
            {
                SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id, iconX + (int)part.pi[Char.CharInfo[0][0][0]].dx, iconY + (int)part.pi[Char.CharInfo[0][0][0]].dy, 0, 0);
            }
        }
        
        int textX = x + 32;
        int textW = w - 74;
        
        mFont nameFont = mFont.tahoma_7b_dark;
        if (member.role == 0) nameFont = mFont.tahoma_7b_red;
        else if (member.role == 1) nameFont = mFont.tahoma_7b_blue;

        nameFont.drawString(g, TrimText(nameFont, member.name, textW), textX, y + 2, mFont.LEFT);
        mFont.tahoma_7_blue.drawString(g, TrimText(mFont.tahoma_7_blue, mResources.power + ": " + member.powerPoint, textW), textX, y + 14, mFont.LEFT);
        mFont.tahoma_7b_dark.drawString(g, member.clanPoint.ToString(), x + w - 8, y + 8, mFont.RIGHT);
    }

    private static void PaintClanInfo(mGraphics g, int x, int y, int w, int h)
    {
        Char c = Char.myCharz();
        if (c == null || c.clan == null) return;
        Clan clan = c.clan;
        
        int currY = y + 10;
        int rowH = 18;
        
        mFont.tahoma_7b_dark.drawString(g, "Tên bang: " + clan.name, x + 10, currY, mFont.LEFT);
        currY += rowH;
        mFont.tahoma_7b_dark.drawString(g, "Thành viên: " + clan.currMember + "/" + clan.maxMember, x + 10, currY, mFont.LEFT);
        currY += rowH;
        
        mFont.tahoma_7b_dark.drawString(g, "Khẩu hiệu:", x + 10, currY, mFont.LEFT);
        currY += 14;
        
        string[] sloganLines = mFont.tahoma_7_blue.splitFontArray(clan.slogan, w - 20);
        for (int i = 0; i < sloganLines.Length; i++)
        {
            mFont.tahoma_7_blue.drawString(g, sloganLines[i], x + 15, currY, mFont.LEFT);
            currY += 15;
        }
        
        currY += 10;
        if (Char.myCharz().role == 0) // Chỉ Chủ bang mới có quyền sửa
        {
            if (!isEditingSlogan)
            {
                int btnW = 85;
                int btnH = 22;
                int gap = 10;
                int totalW = btnW * 2 + gap;
                int startX = x + (w - totalW) / 2;
                
                // Nút Sửa khẩu hiệu
                bool isHoverSlogan = Hit(GameCanvas.px, GameCanvas.py, startX, currY, btnW, btnH);
                g.setColor(isHoverSlogan ? 0x5EB432 : 0x4C9D27);
                g.fillRect(startX, currY, btnW, btnH, 4);
                mFont.tahoma_7b_white.drawString(g, "Sửa slogan", startX + btnW / 2, currY + btnH / 2 - 4, mFont.CENTER);
                
                // Nút Biểu tượng
                int iconBtnX = startX + btnW + gap;
                bool isHoverIcon = Hit(GameCanvas.px, GameCanvas.py, iconBtnX, currY, btnW, btnH);
                g.setColor(isHoverIcon ? 0x5EB432 : 0x4C9D27);
                g.fillRect(iconBtnX, currY, btnW, btnH, 4);
                mFont.tahoma_7b_white.drawString(g, "Biểu tượng", iconBtnX + btnW / 2, currY + btnH / 2 - 4, mFont.CENTER);
            }
            else
            {
                EnsureSloganTField();
                int btnW = 35;
                int btnH = 22;
                int tfW = w - (btnW + 2) * 2 - 10;
                int tfX = x + 5;
                
                sloganTField.x = tfX;
                sloganTField.y = currY;
                sloganTField.width = tfW;
                sloganTField.height = btnH;
                
                // Vẽ ô nhập thủ công
                g.setColor(0x000000);
                g.fillRect(sloganTField.x, sloganTField.y, sloganTField.width, sloganTField.height, 4);
                g.setColor(0xFFFFFF);
                g.fillRect(sloganTField.x + 1, sloganTField.y + 1, sloganTField.width - 2, sloganTField.height - 2, 4);
                
                string sloganText = sloganTField.getText();
                int textX = sloganTField.x + 4 + sloganTField.offsetX;
                
                g.setClip(sloganTField.x + 2, sloganTField.y + 1, sloganTField.width - 4, sloganTField.height - 2);
                
                // Sử dụng tahoma_8b để đảm bảo hiển thị tiếng Việt tốt nhất
                if (string.IsNullOrEmpty(sloganText))
                {
                    mFont.tahoma_7_grey.drawString(g, sloganTField.name, sloganTField.x + 4, sloganTField.y + btnH / 2 - 4, mFont.LEFT);
                }
                else
                {
                    mFont.tahoma_8b.drawString(g, sloganText, textX, sloganTField.y + btnH / 2 - 4, mFont.LEFT);
                }
                
                if (sloganTField.isFocus && (mSystem.currentTimeMillis() / 500) % 2 == 0)
                {
                    // Tính toán vị trí con trỏ dựa trên số ký tự phía trước
                    string textBeforeCaret = sloganText.Substring(0, sloganTField.caretPos);
                    int caretX = mFont.tahoma_8b.getWidth(textBeforeCaret);
                    g.setColor(0x000000);
                    g.fillRect(textX + caretX, sloganTField.y + 4, 1, btnH - 8);
                }
                g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
                
                // Nút Lưu
                int saveBtnX = tfX + tfW + 2;
                bool isHoverSave = Hit(GameCanvas.px, GameCanvas.py, saveBtnX, currY, btnW, btnH);
                g.setColor(isHoverSave ? 0x5EB432 : 0x4C9D27);
                g.fillRect(saveBtnX, currY, btnW, btnH, 4);
                mFont.tahoma_7_white.drawString(g, "Lưu", saveBtnX + btnW / 2, currY + btnH / 2 - 4, mFont.CENTER);
                
                // Nút Đóng
                int closeBtnX = saveBtnX + btnW + 2;
                bool isHoverClose = Hit(GameCanvas.px, GameCanvas.py, closeBtnX, currY, btnW, btnH);
                g.setColor(isHoverClose ? 0xCD5C5C : 0xB22222);
                g.fillRect(closeBtnX, currY, btnW, btnH, 4);
                mFont.tahoma_7_white.drawString(g, "Đóng", closeBtnX + btnW / 2, currY + btnH / 2 - 4, mFont.CENTER);
            }
        }
    }

    private static bool TryHandleClanClick(bool isFire)
    {
        int safeX = layoutSafeX;
        int safeY = panelY + 45;
        int safeW = layoutSafeW;
        int safeH = panelH - 82; // Đồng bộ với PaintClanTab
        int gap = 6;
        int msgW = safeW / 2 - gap / 2;
        int logicW = safeW - msgW - gap;
        int msgX = safeX; // Đồng bộ với PaintClanTab
        int logicX = msgX + msgW + gap; // Đồng bộ với PaintClanTab
        int listY = safeY + 34;
        int listH = safeH - 40;
        int rowH = 32;
        string[] menuLabels = GetClanMenuLabels();
        int menuY = safeY + 6;
        int menuH = 22;
        int logicInnerW = logicW - 12;
        // 1. Kiểm tra click vào Menu Bang Hội (Thành viên, Khẩu hiệu...)
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

        // 2. Kiểm tra click vào vùng Chat Bang Hội (Dưới cùng cột bên trái)
        if (GameCanvas.px >= msgX && GameCanvas.px <= msgX + msgW)
        {
            int inputH = 25;
            int inputY = safeY + safeH - inputH - 6;
            int btnW = 28;
            int innerGap = 2;
            int tfX = msgX + 6 + btnW + innerGap;
            int tfW = msgW - 12 - btnW * 2 - innerGap * 2;
            int btnSendX = msgX + msgW - btnW - 6;

            if (isFire)
            {
                // Click nút Xin đậu
                if (Hit(GameCanvas.px, GameCanvas.py, msgX + 6, inputY, btnW, inputH))
                {
                    Service.gI().clanMessage(1, null, -1);
                    SoundMn.gI().panelClick();
                    return true;
                }
                // Click nút Gửi
                else if (Hit(GameCanvas.px, GameCanvas.py, btnSendX, inputY, btnW, inputH))
                {
                    HandleSendClanChat();
                    SoundMn.gI().panelClick();
                    return true;
                }
                // Click vào ô nhập liệu
                else if (Hit(GameCanvas.px, GameCanvas.py, tfX, inputY, tfW, inputH))
                {
                    EnsureClanChatTField();
                    clanChatTField.setFocusWithKb(true);
                    SoundMn.gI().panelClick();
                    return true;
                }
                else
                {
                    // Click ra ngoài ô chat trong vùng msgX
                    if (clanChatTField != null) clanChatTField.isFocus = false;
                }
            }
            // Block click nếu đang ở trong vùng input (hover)
            if (GameCanvas.py >= inputY - 2 && GameCanvas.py <= inputY + inputH + 2) return true;
        }

        // 3. Kiểm tra click vào danh sách (Tin nhắn hoặc Thành viên hoặc Thông tin)
        if (HasClanData() && selectedClanMenuIndex == 1)
        {
            // Đối với tab Thông tin, cho phép click xuống tận cùng vùng an toàn (safeH) để không hụt nút Lưu khi slogan dài
            if (GameCanvas.px >= logicX && GameCanvas.px <= logicX + logicInnerW && GameCanvas.py >= listY && GameCanvas.py <= safeY + safeH)
            {
                Char c = Char.myCharz();
                if (c != null && c.clan != null)
                {
                    string[] sloganLines = mFont.tahoma_7_blue.splitFontArray(c.clan.slogan, logicInnerW - 20);
                    int btnY = listY + 10 + 18 + 18 + 14 + sloganLines.Length * 15 + 10;
                    
                    // Chỉ Chủ bang (role == 0) mới được tương tác với nút sửa
                    bool isLeader = Char.myCharz().role == 0;
                    if (isLeader)
                    {
                        if (!isEditingSlogan)
                        {
                            int btnW = 85;
                            int btnH = 22;
                            int btnGap = 10;
                            int totalW = btnW * 2 + btnGap;
                            int startX = logicX + (logicInnerW - totalW) / 2;
                            
                            if (isFire)
                            {
                                // Click Sửa slogan
                                if (Hit(GameCanvas.px, GameCanvas.py, startX, btnY, btnW, btnH))
                                {
                                    SoundMn.gI().panelClick();
                                    isEditingSlogan = true;
                                    EnsureSloganTField();
                                    sloganTField.setText(c.clan.slogan);
                                    sloganTField.setFocusWithKb(true);
                                }
                                // Click Đổi biểu tượng
                                else if (Hit(GameCanvas.px, GameCanvas.py, startX + btnW + btnGap, btnY, btnW, btnH))
                                {
                                    SoundMn.gI().panelClick();
                                    Service.gI().getClan(3, -1, null);
                                    if (GameCanvas.panel != null && GameCanvas.panel.tabIcon != null)
                                    {
                                        GameCanvas.panel.tabIcon.show(isGetName: false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int sBtnW = 35;
                            int sBtnH = 22;
                            int tfW = logicInnerW - (sBtnW + 2) * 2 - 10;
                            int tfX = logicX + 5;
                            int saveBtnX = tfX + tfW + 2;
                            int closeBtnX = saveBtnX + sBtnW + 2;
                            
                            if (isFire)
                            {
                                if (Hit(GameCanvas.px, GameCanvas.py, tfX, btnY, tfW, sBtnH))
                                {
                                    sloganTField.setFocusWithKb(true);
                                }
                                else if (Hit(GameCanvas.px, GameCanvas.py, saveBtnX, btnY, sBtnW, sBtnH))
                                {
                                    HandleSaveSlogan();
                                    SoundMn.gI().panelClick();
                                }
                                else if (Hit(GameCanvas.px, GameCanvas.py, closeBtnX, btnY, sBtnW, sBtnH))
                                {
                                    isEditingSlogan = false;
                                    SoundMn.gI().panelClick();
                                }
                                else
                                {
                                    if (sloganTField != null) sloganTField.isFocus = false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }

        if (GameCanvas.py < listY || GameCanvas.py > listY + listH)
        {
            return false;
        }

        if (GameCanvas.px >= msgX && GameCanvas.px <= msgX + msgW - 12)
        {
            // Click vào danh sách tin nhắn
            int inputH = 25;
            int msgListH = safeH - inputH - 32;
            if (GameCanvas.py <= listY + msgListH)
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

    private static void HandleSendClanChat()
    {
        EnsureClanChatTField();
        string text = clanChatTField.getText();
        if (!string.IsNullOrEmpty(text))
        {
            Service.gI().clanMessage(0, text, -1);
            clanChatTField.setText(string.Empty);
            // reset caret và offset
            clanChatTField.caretPos = 0;
            clanChatTField.setOffset(0);
        }
        // Giữ focus để người dùng có thể chat tiếp tục, giống như các game hiện đại
        clanChatTField.isFocus = true;
    }

    private static void HandleSaveSlogan()
    {
        Char c = Char.myCharz();
        if (c != null && c.clan != null && sloganTField != null)
        {
            string newSlogan = sloganTField.getText();
            if (!string.IsNullOrEmpty(newSlogan))
            {
                Service.gI().getClan(4, (sbyte)c.clan.imgID, newSlogan);
            }
        }
        isEditingSlogan = false;
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
                selectedClanMenuIndex = 0;
            }
            else if (menuIndex == 1)
            {
                selectedClanMenuIndex = 1;
            }
            else if (menuIndex == 2)
            {
                // Rời bang bây giờ là index 2
                GameCanvas.startYesNoDlg("Bạn có chắc chắn muốn rời bang hội không?", new Command("Có", new LeaveClanAction(), 0, null), new Command("Không", GameCanvas.instance, 8882, null));
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
            GameCanvas.menu.startAt(cmds, layoutSafeX, menuY);
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
            GameCanvas.menu.startAt(cmds, layoutSafeX, menuY);
        }
    }


    private static void EnsureClanChatTField()
    {
        if (clanChatTField == null)
        {
            clanChatTField = new TField();
            clanChatTField.setIputType(TField.INPUT_TYPE_ANY);
            clanChatTField.name = "Nhập nội dung...";
            clanChatTField.isFocus = false;
        }
    }

    private static void EnsureSloganTField()
    {
        if (sloganTField == null)
        {
            sloganTField = new TField();
            sloganTField.setIputType(TField.INPUT_TYPE_ANY);
            sloganTField.name = "Nhập khẩu hiệu...";
            sloganTField.isFocus = false;
            sloganTField.setMaxTextLenght(250);
        }
    }

    private static string[] GetClanMenuLabels()
    {
        if (HasClanData())
        {
            return new string[] { "Thành viên", "Thông tin", "Rời bang" };
        }
        return new string[] { "Tìm bang", "Lập bang" };
    }

    private static bool HasClanData()
    {
        Char c = Char.myCharz();
        Panel p = GameCanvas.panel;
        return (c != null && c.clan != null) || (p != null && p.myMember != null && p.myMember.size() > 0);
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

    private static void OpenClanChatInput()
    {
        Panel p = GameCanvas.panel;
        if (p == null) return;
        EnsurePanelChatField(p);
        p.chatTField.strChat = "Chat Bang";
        p.chatTField.tfChat.name = "Nội dung";
        p.chatTField.to = string.Empty;
        p.chatTField.isShow = true;
        p.chatTField.tfChat.isFocus = true;
        p.chatTField.tfChat.setIputType(TField.INPUT_TYPE_ANY);
        if (GameCanvas.isTouch)
        {
            p.chatTField.tfChat.doChangeToTextBox();
        }
        ClanChatAction action = new ClanChatAction();
        p.chatTField.center = new Command("Gửi", action, 0, null);
        p.chatTField.left = new Command(mResources.CLOSE, action, 1, null);
    }

}
