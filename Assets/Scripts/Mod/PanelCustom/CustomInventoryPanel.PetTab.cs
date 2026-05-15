using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
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
            // Đảm bảo có bộ phận tối thiểu để vẽ nếu server chưa gửi dữ liệu
            if (pet.head == -1 || pet.head == 0) pet.setDefaultPart();
        }

        int safeX2 = panelX + 24;
        int safeW2 = panelW - 48;
        int leftX = safeX2;
        int leftW = safeW2 / 2 - 16;
        int rightX2 = safeX2 + safeW2 / 2 + 10;
        int rightW2 = 34 * 6 + 4 * 5;

        // Vẽ khung nền bên phải cho Inventory/Skills đệ tử
        PaintOldPanelBox(g, rightX2 - 11, panelY + 42, 246, panelH - 30 - (panelY + 42));

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
        int frameY = leftY;
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
            // Vẽ nhân vật đệ tử. Sử dụng trạng thái thực tế từ map (false) để tránh lỗi tính toán lại bộ phận.
            PaintCharacterPreview(g, pet, centerX, frameY + 132, false);
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
        int cols = 6;
        int viewW = 34 * cols + gap * (cols - 1);
        int viewH = panelH - 66 - (y - panelY);
        if (viewH < 0) viewH = 0;

        int total = (items != null) ? items.Length : 42;
        int rows = (total + (cols - 1)) / cols;
        if (rows < 7)
        {
            rows = 7;
        }
        g.setClip(x, y, viewW, viewH);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                int xx = x + col * (34 + gap);
                int yy = y + row * (26 + gap) - bagScrollY + bagElasticY;
                if (yy + 26 < y || yy > y + viewH)
                {
                    continue;
                }
                bool isSelected = (selectedBagIndex == index && selectedBodyIndex < 0);
                PaintSlotRect(g, xx, yy, 34, 26, isSelected);
                PaintItemInSlot(g, GetItem(items, index), xx, yy, 34, 26, isSelected);
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
        int statsFrameY = panelY + 232;
        int statsFrameW = 246;
        PaintOldPanelBox(g, statsFrameX, statsFrameY, statsFrameW, 65);
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
        long now = mSystem.currentTimeMillis();

        // 1. Tự động đồng bộ khi phát hiện thay đổi từ server
        if (pet.petStatus != lastConfirmedPetStatus)
        {
            lastConfirmedPetStatus = pet.petStatus;
            // CHỈ xóa trạng thái chờ nếu thực tế từ server đã khớp với mong muốn
            if (pendingPetStatus != -1 && pet.petStatus == (sbyte)pendingPetStatus)
            {
                pendingPetStatus = -1;
                pendingPetStatusUntil = 0L;
            }
        }

        // 2. Xác định trạng thái hiển thị (Ưu tiên pending -> Fusion -> Thực tế)
        if (pendingPetStatus >= 0)
        {
            // Nếu đã hết thời gian chờ mà server chưa update, quay về thực tế
            if (now >= pendingPetStatusUntil)
            {
                pendingPetStatus = -1;
                pendingPetStatusUntil = 0L;
                activeStatus = pet.petStatus;
            }
            else
            {
                activeStatus = (sbyte)pendingPetStatus;
            }
        }
        else if (isPorataFusionActive && now >= suppressFusionHighlightUntil)
        {
            activeStatus = 4;
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
        //mFont.tahoma_7b_dark.drawString(g, "Trạng thái", x + 26, y, mFont.LEFT);
        for (int i = 0; i < options.Length; i++)
        {
            bool enabled = i == activeStatus;
            int itemY = rowY + i * rowH;
            PaintOldTextCell(g, rowX, itemY, rowW, rowH - 2, enabled);
            mFont.tahoma_7b_dark.drawString(g, options[i], rowX + 28, itemY + 4, mFont.LEFT);
            PaintStatusToggle(g, rowX + rowW - toggleW - 6, itemY + (rowH - toggleH) / 2, toggleW-2, toggleH - 3, enabled);
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
        int bagViewH = panelH - 132;
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
        int frameY = leftY;
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
                return;
            }
        }
        // Đang hợp thể thì phải tắt hợp thể trước, chưa cho đổi state ngay.
        if (selectedStatus != 4 && isPorataFusionActive)
        {
            RequestDetachFusion();
            GameScr.info1.addInfo("Đang yêu cầu tách hợp thể...", 0);
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
        // Cung cấp phản hồi thị giác ngay lập tức nhưng không thay đổi dữ liệu gốc.
        // UI sẽ tự động quay lại nếu Server không cập nhật trạng thái trong vòng 2.5s.
        pendingPetStatus = selectedStatus;
        pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;
    }

    public static void OnPetStatusReceived()
    {
        Char pet = GetPrimaryPet();
        if (pet != null)
        {
            // Chỉ xóa trạng thái chờ nếu Server đã cập nhật ĐÚNG trạng thái đó (xác nhận thành công)
            if (pendingPetStatus != -1 && pet.petStatus == (sbyte)pendingPetStatus)
            {
                pendingPetStatus = -1;
                pendingPetStatusUntil = 0L;
            }
            lastConfirmedPetStatus = pet.petStatus;
        }
    }

    private static void RequestDetachFusion()
    {
        Char pet = GetPrimaryPet();
        // Tránh giữ highlight "Hợp thể" quá lâu khi vừa bấm tách.
        suppressFusionHighlightUntil = mSystem.currentTimeMillis() + 2500L;
        // Đặt trạng thái chờ hiển thị là "Về nhà" để phản hồi tức thì
        pendingPetStatus = 3;
        pendingPetStatusUntil = mSystem.currentTimeMillis() + 2500L;

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
        }
        else
        {
            isPorataFusionActive = me.isFusion;
        }
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

}
