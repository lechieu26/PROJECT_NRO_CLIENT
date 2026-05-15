using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
    private static void PaintCharacterTab(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeY = panelY + 45;
        int safeW = panelW - 48;
        int safeH = panelH - 82;
        int frameMargin = 2;
        int frameGap = 2;
        int topX = safeX + frameMargin;
        int topY = safeY;
        int topW = safeW - frameMargin * 2;
        int topH = 165;
        int infoX = topX;
        int infoY = topY + topH + frameGap;
        int infoW = topW;
        int infoH = safeH - topH;
        PaintOldPanelBox(g, topX, topY, topW, topH);
        PaintOldPanelBox(g, infoX, infoY, infoW, infoH);

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
        PaintCharacterStatsCompact(g, me, infoX + 16, infoY + 20, infoW - 32);
    }

    private static void PaintCharacterStatsCompact(mGraphics g, Char c, int x, int y, int w)
    {
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
        bool oldUseSpine = ch.useSpine;
        int oldSpineId = ch.spineId;
        bool oldIsPreviewSpine = ch.isPreviewSpine;
        
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
            
            if (ch.useSpine)
            {
                ch.isPreviewSpine = true;
                SpineCharacterManager.Instance.PaintPreviewSpine(g, ch.cx, ch.cy);
            }
            else
            {
                ch.paintCharBody(g, ch.cx, ch.cy, ch.cdir, ch.cf, true);
            }
            
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
            ch.useSpine = oldUseSpine;
            ch.spineId = oldSpineId;
            ch.isPreviewSpine = oldIsPreviewSpine;
            RestorePreviewEffectPositions(ch, effXs, effYs);
        }
    }

    private static void HandleCharacterSelection(bool isFire)
    {
        Char me = Char.myCharz();
        Item[] body = (me != null) ? me.arrItemBody : null;
        int safeX = panelX + 24;
        int safeY = panelY + 45;
        int safeW = panelW - 48;
        int frameMargin = 2;
        int topX = safeX + frameMargin;
        int topY = safeY;
        int topW = safeW - frameMargin * 2;
        int topH = 165;
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

}
