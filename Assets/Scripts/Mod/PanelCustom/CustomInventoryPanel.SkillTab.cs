using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
    private static void PaintSkillTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 45;
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
                if (System.Math.Abs(GameCanvas.py - skillDragStartY) > 5)
                {
                    skillDragged = true;
                    globalDragged = true;
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
        int releaseDelta = GameCanvas.py - dragLastY[0];
        GameCanvas.isPointerJustRelease = false;
        if (System.Math.Abs(releaseDelta) < 20 && System.Math.Abs(GameCanvas.py - skillDragStartY) < 20 && !downWhenRunning)
        {
            pendingSelectionClick = true;
            skillScrollRun = 0;
            skillScrollTargetY = skillScrollY;
        }
        else if (!downWhenRunning)
        {
            int force = GameCanvas.py - dragLastY[0] + (dragLastY[0] - dragLastY[1]) + (dragLastY[1] - dragLastY[2]);
            if (force > 15) force = 15;
            if (force < -15) force = -15;
            skillScrollRun = -force * 150;
        }
        draggingSkill = false;
        skillDragged = false;
        GameCanvas.isPointerJustRelease = false;
        ApplySkillScrollRun(maxScroll);
    }

    private static bool TryHandleSkillClick(bool isFire)
    {
        Char c = Char.myCharz();
        if (c == null)
        {
            return false;
        }
        int safeX = panelX + 24;
        int safeY = panelY + 45;
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

}
