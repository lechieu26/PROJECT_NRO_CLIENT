using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{

    private static int GetBagMaxScroll(int viewH)
    {
        Item[] items = GetRightTabItems();
        int count = (items != null) ? items.Length : 0;
        int rows = count / 6 + ((count % 6 != 0) ? 1 : 0);
        int contentH = rows * (26 + 4);
        int max = contentH - viewH;
        return (max > 0) ? max : 0;
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
        int viewH = panelH - 66 - (rightY - panelY); // Dừng trên vùng info (khoảng 198px)
        if (viewH < 0) viewH = 0;

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
            int y = rightY + row * (slot + gap) - bagScrollY + bagElasticY;
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

    private static void PaintEmptySlots(mGraphics g)
    {
        int safeX = panelX + 24;
        int safeW = panelW - 48;
        int frameY = panelY + 42;
        int frameW = 246;
        int frameH = panelH - 30 - (panelY + 42);
        
        // 1. Vẽ 3 khung nền chính
        PaintOldPanelBox(g, safeX - 3, frameY, frameW, 188); // Box Trang bị (Trái trên)
        PaintOldPanelBox(g, safeX - 3, panelY + 232, 246, 65); // Box Chỉ số (Trái dưới)
        PaintOldPanelBox(g, safeX + safeW / 2 + 10 - 11, frameY, frameW, frameH); // Box Hành trang (Phải)

        // 2. Vẽ SubTabs và TitleBars trên nền box
        PaintSubTabs(g);
        PaintTitleBars(g);

        // 3. Vẽ nội dung items
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
        int frameY = panelY + 42;
        int frameW = 246;
        int centerX = frameX + frameW / 2;
        int bodyLeftX = frameX + 22;
        int bodyRightX = frameX + frameW - 22 - 36;
        int bodyTopY = frameY + 19;
        int bodyGapY = 27;
        int slot = 26;
        int gapY = 30;
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
            PaintCharacterPreview(g, me, centerX, frameY + 132, true);
        }
        int bottomY = frameY + 159;
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

    private static void PaintSlot(mGraphics g, int x, int y, int size, bool selected)
    {
        PaintSlotRect(g, x, y, 34, size, selected);
    }

    private static void PaintSlotRect(mGraphics g, int x, int y, int w, int h, bool selected)
    {
        if (selected)
        {
            g.setColor(SELECT_BG);
            g.fillRect(x - 1, y - 1, w + 2, h + 2, 3);
        }
        g.setColor(6047789, 0.3f);
        g.fillRect(x, y, w, h, 3);
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
        mFont.tahoma_7_yellow.drawString(g, selectedBagIndex >= 0 ? "Hành trang" : "Trang bị", x + 46, y + 36, mFont.LEFT);
    }

    private static void SyncCharPartsFromItems(Char ch)
    {
        if (ch == null || ch.arrItemBody == null)
        {
            return;
        }

        // 1. Xác định ID để tra cứu cache Spine
        int searchId = ch.charID;
        Char myPet = Char.myPetz();
        Char myPet2 = (Char.myCharz() != null) ? Char.MyPet2z() : null;
        if (ch == myPet || ch == myPet2)
        {
            if (Char.myCharz() != null)
            {
                searchId = -Char.myCharz().charID;
                // Đồng bộ ID đệ tử để SpineCharacterManager có thể tìm thấy dữ liệu xem trước
                ch.charID = searchId;
            }
        }

        // 2. Nhận diện trạng thái Spine
        bool hasSpineItem = false;
        int newItemSpineId = -1;
        for (int i = 0; i < ch.arrItemBody.Length; i++)
        {
            Item item = ch.arrItemBody[i];
            if (item != null && item.template != null && item.template.type == 80)
            {
                hasSpineItem = true;
                newItemSpineId = item.template.part;
                break;
            }
        }

        Item costume = FindEquippedCostume(ch.arrItemBody);

        if (hasSpineItem)
        {
            ch.useSpine = true;
            ch.spineId = newItemSpineId;
        }
        else if (costume != null)
        {
            // Nếu đang mặc cải trang thường (type 5), ưu tiên hiển thị dạng legacy
            ch.useSpine = false;
        }
        else
        {
            // Nếu không có vật phẩm chỉ định, đồng bộ với trạng thái thế giới (cache)
            if (SpineMessageHandler.playerSkinCache.TryGetValue(searchId, out int cachedId))
            {
                ch.useSpine = true;
                ch.spineId = cachedId;
            }
            else
            {
                ch.useSpine = false;
            }
        }

        // Nếu là Spine, không cần xử lý bộ phận legacy (head/body/leg)
        if (ch.useSpine) return;

        // 3. Xử lý bộ phận Legacy
        bool overrideHead = false;
        bool overrideBody = false;
        bool overrideLeg = false;
        
        // Khởi tạo từ giá trị hiện tại của nhân vật (giúp giữ đúng ngoại hình server đã set)
        short costumeHead = (short)ch.head;
        short costumeBody = (short)ch.body;
        short costumeLeg = (short)ch.leg;

        // Ưu tiên Cải trang (type 5)
        if (costume != null)
        {
            // Một khi đã mặc cải trang, ta coi như toàn bộ ngoại hình bị ghi đè 
            // để tránh các item áo/quần hiện đè lên (gây lỗi lệch đầu/thân)
            overrideHead = true;
            overrideBody = true;
            overrideLeg = true;

            if (costume.headTemp != -1) costumeHead = (short)costume.headTemp;
            if (costume.bodyTemp != -1) costumeBody = (short)costume.bodyTemp;
            if (costume.legTemp != -1) costumeLeg = (short)costume.legTemp;

            if (costume.itemOption != null)
            {
                for (int j = 0; j < costume.itemOption.Length; j++)
                {
                    ItemOption opt = costume.itemOption[j];
                    if (opt == null || opt.optionTemplate == null) continue;
                    if (opt.optionTemplate.id == 127) costumeHead = (short)opt.param;
                    if (opt.optionTemplate.id == 128) costumeBody = (short)opt.param;
                    if (opt.optionTemplate.id == 129) costumeLeg = (short)opt.param;
                }
            }
        }

        // Chỉ áp dụng part từ trang bị nếu bộ phận đó CHƯA bị cải trang chiếm dụng
        for (int i = 0; i < ch.arrItemBody.Length; i++)
        {
            Item item = ch.arrItemBody[i];
            if (item == null || item.template == null) continue;

            int part = item.template.part;
            if (part == -1) continue;

            if (item.template.type == 0 && !overrideBody)
                costumeBody = (short)part;
            else if (item.template.type == 1 && !overrideLeg)
                costumeLeg = (short)part;
            else if ((item.template.type == 2 || item.template.type == 6) && !overrideHead)
                costumeHead = (short)part;
        }

        // Cập nhật lại cho nhân vật
        ch.head = costumeHead;
        ch.body = costumeBody;
        ch.leg = costumeLeg;

        // Fallback để tránh lỗi IndexOutOfRangeException khi vẽ
        if (ch.head <= -1) ch.head = 0;
        if (ch.body <= -1) ch.body = 0;
        if (ch.leg <= -1) ch.leg = 0;
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

}
