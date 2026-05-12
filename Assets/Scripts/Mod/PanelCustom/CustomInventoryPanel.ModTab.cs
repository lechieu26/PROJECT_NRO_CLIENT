using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
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
        int safeY = panelY + 45;
        int safeW = panelW - 48;
        int safeH = panelH - 82;


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

    private static void PaintModToggle(mGraphics g, int x, int y, bool enabled)
    {
        g.setColor(enabled ? 4825130 : 9671571);
        g.fillRect(x, y, 18, 10, 5);
        g.setColor(0xFFFFFF);
        g.fillRect(enabled ? x + 9 : x + 1, y + 1, 8, 8, 4);
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
        int safeY = panelY + 45;
        int safeW = panelW - 48;
        int safeH = panelH - 118;
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

}
