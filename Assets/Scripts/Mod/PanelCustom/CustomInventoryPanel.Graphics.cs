using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{

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
        PaintCloseButton(g);
    }

    private static void PaintTopTabs(mGraphics g)
    {
        int[] visibleTabs = GetVisibleTabs();
        int tabW = 60;
        int tabH = 18;
        int gap = 2;
        int totalTabsW = visibleTabs.Length * tabW + (visibleTabs.Length - 1) * gap;
        int tabX = panelX + (panelW - totalTabsW) / 2;
        int tabY = panelY + 22;
        for (int vi = 0; vi < visibleTabs.Length; vi++)
        {
            int logicalIndex = visibleTabs[vi];
            int x = tabX + vi * (tabW + gap);
            bool active = logicalIndex == topTab;
            
            // Vẽ theo phong cách RaisedTabBox từ bản backup
            // 1. Viền ngoài
            g.setColor(9993045);
            g.fillRect(x, tabY, tabW, tabH, 3);
            
            // 2. Nền chính
            g.setColor(active ? 0xFFF1CF : 16770503);
            g.fillRect(x + 1, tabY + 1, tabW - 2, tabH - 2, 3);
            
            // 3. Viền bóng ở trên (Highlight)
            g.setColor(0xFFFFFF);
            g.fillRect(x + 2, tabY + 2, tabW - 4, 3, 3);

            if (active)
            {
                // 4. Vùng chọn (Inner)
                g.setColor(SELECT_BG);
                g.fillRect(x + 3, tabY + 4, tabW - 6, tabH - 7, 3);
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
        int statsFrameW = 246;

        PaintOldPanelBox(g, statsFrameX, statsFrameY, statsFrameW, 65);
        PaintCharacterStats(g, statsFrameX + 18, statsFrameY + 3, statsFrameW - 36);
        PaintInventoryCurrency(g);
    }

    private static void PaintOldPanelBox(mGraphics g, int x, int y, int w, int h)
    {
        // Khung kiểu panel cũ: viền nâu nhẹ + nền sáng, bo góc 5px.
        g.setColor(9993045);
        g.fillRect(x, y, w, h, 5);
        g.setColor(15196114);
        g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
        // Viền bóng ở trên (Highlight)
        g.setColor(0xFFFFFF);
        g.fillRect(x + 2, y + 2, w - 4, 3, 5);
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
        int viewW = 246;
        int y = panelY + panelH - 50;
        PaintOldPanelBox(g, rightX - 11, y - 4, viewW, 22);
        int x1 = rightX - 6;
        int x2 = rightX + 74;
        int x3 = rightX + 154;
        DrawCurrency(g, Panel.imgXu, FormatGold(c.xu), x1, y + 2, 0xD6A000, 17, 80);
        DrawCurrency(g, Panel.imgLuong, FormatStat((long)c.luong), x2, y + 2, 0x1E9F4B, 18, 70);
        DrawCurrency(g, Panel.imgLuongKhoa, FormatStat((long)c.luongKhoa), x3, y + 2, 0xD03D7C, 15, 70);
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

    private static void PaintOldTextCell(mGraphics g, int x, int y, int w, int h, bool selected)
    {
        g.setColor(selected ? SELECT_BORDER : 9993045);
        g.fillRect(x, y, w, h, 5);
        g.setColor(selected ? SELECT_BG : 15196114);
        g.fillRect(x + 1, y + 1, w - 2, h - 2, 5);
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

    public static void PaintOldTextCellBridge(mGraphics g, int x, int y, int w, int h, bool selected)
    {
        PaintOldTextCell(g, x, y, w, h, selected);
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
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();

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
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
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
        int oldClipX = g.getClipX();
        int oldClipY = g.getClipY();
        int oldClipW = g.getClipWidth();
        int oldClipH = g.getClipHeight();
        g.setClip(dx, dy, dw, dh);
        g.drawRegion(img, sx, sy, sw, sh, 0, dx, dy, 0);
        g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);
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

}
