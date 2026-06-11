using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{
    private static void PaintTaskTab(mGraphics g)
    {
        int safeX = layoutSafeX;
        int safeY = panelY + 45;
        int safeW = layoutSafeW;
        int safeH = panelH - 82;
        Task task = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;

        int gap = 6;
        int frameY = safeY;
        int frameH = 253;

        // Box 1: Thông tin nhiệm vụ (bên trái)
        int taskBoxX = panelX + 21;
        int taskBoxW = (safeW - gap) / 2 + 10;
        PaintOldPanelBox(g, taskBoxX, frameY, taskBoxW, frameH);

        // Box 2: Bản đồ (bên phải)
        int mapBoxX = taskBoxX + taskBoxW + gap;
        int mapBoxW = 499 - taskBoxW - gap;
        PaintOldPanelBox(g, mapBoxX, frameY, mapBoxW, frameH);

        // === Vẽ nội dung Box 1: Thông tin nhiệm vụ ===
        int contentX = taskBoxX + 30;
        int contentW = taskBoxW - 50;
        mFont.tahoma_7b_dark.drawString(g, "NHIỆM VỤ", taskBoxX + taskBoxW / 2, frameY + 8, mFont.CENTER);

        if (task == null)
        {
            mFont.tahoma_7b_dark.drawString(g, "Chưa có nhiệm vụ", taskBoxX + taskBoxW / 2, frameY + 50, mFont.CENTER);
        }
        else
        {
            int y = frameY + 26;
            int bottomLimit = frameY + frameH - 12;
            if (task.names != null)
            {
                for (int i = 0; i < task.names.Length && y < bottomLimit - 20; i++)
                {
                    mFont.tahoma_7b_green2.drawString(g, task.names[i], contentX, y, mFont.LEFT);
                    y += 11;
                }
            }
            y += 4;
            if (task.details != null)
            {
                for (int i = 0; i < task.details.Length && y < bottomLimit - 40; i++)
                {
                    mFont.tahoma_7b_dark.drawString(g, task.details[i], contentX, y, mFont.LEFT);
                    y += 10;
                }
            }
            y += 6;
            PaintTaskProgress(g, task, contentX, y, contentW, bottomLimit);
        }

        // === Vẽ nội dung Box 2: Bản đồ ===
        mFont.tahoma_7b_dark.drawString(g, mResources.map, mapBoxX + mapBoxW / 2, frameY + 8, mFont.CENTER);

        taskMapContentX = mapBoxX + 3;
        taskMapContentY = frameY + 22;
        taskMapContentW = mapBoxW - 6;
        taskMapContentH = frameH - 28;

        // Load bản đồ nếu chưa có
        if (Panel.imgMap == null && Panel.isPaintMap)
        {
            try
            {
                Panel.imgMap = GameCanvas.loadImageRMS("/img/map" + TileMap.planetID.ToString() + ".png");
                TileMap.lastPlanetId = TileMap.planetID;
            }
            catch (System.Exception) { }
        }

        if (Panel.imgMap != null && Panel.isPaintMap)
        {
            // Tính vị trí nhân vật trên bản đồ
            int charMapX = -1;
            int charMapY = -1;
            for (int i = 0; i < Panel.mapId[(int)TileMap.planetID].Length; i++)
            {
                if (TileMap.mapID == Panel.mapId[(int)TileMap.planetID][i])
                {
                    charMapX = Panel.mapX[(int)TileMap.planetID][i];
                    charMapY = Panel.mapY[(int)TileMap.planetID][i];
                    break;
                }
            }

            // Tính offset để center bản đồ vào vị trí nhân vật
            int imgW = Panel.imgMap.getWidth();
            int imgH = Panel.imgMap.getHeight();
            int scrollX = 0;
            int scrollY = 0;
            if (charMapX >= 0)
            {
                scrollX = charMapX - taskMapContentW / 2;
                scrollY = charMapY - taskMapContentH / 2;
            }
            if (scrollX < 0) scrollX = 0;
            if (scrollY < 0) scrollY = 0;
            if (scrollX > imgW - taskMapContentW) scrollX = imgW - taskMapContentW;
            if (scrollY > imgH - taskMapContentH) scrollY = imgH - taskMapContentH;
            if (scrollX < 0) scrollX = 0;
            if (scrollY < 0) scrollY = 0;

            // Lưu scroll offset để dùng trong click handler
            taskMapScrollX = scrollX;
            taskMapScrollY = scrollY;

            // Clip và vẽ bản đồ
            int oldClipX = g.getClipX();
            int oldClipY = g.getClipY();
            int oldClipW = g.getClipWidth();
            int oldClipH = g.getClipHeight();
            g.setClip(taskMapContentX, taskMapContentY, taskMapContentW, taskMapContentH);
            g.translate(-scrollX, -scrollY);
            g.drawImage(Panel.imgMap, taskMapContentX, taskMapContentY, 0);

            // Vẽ vị trí nhiệm vụ nhấp nháy (sao vàng)
            int taskMapIdVal = GameScr.getTaskMapId();
            int taskPointIdx = -1;
            if (taskMapIdVal != -1)
            {
                for (int i = 0; i < Panel.mapId[(int)TileMap.planetID].Length; i++)
                {
                    if (Panel.mapId[(int)TileMap.planetID][i] == taskMapIdVal)
                    {
                        taskPointIdx = i;
                        break;
                    }
                }
                if (taskPointIdx >= 0 && GameCanvas.gameTick % 4 > 0)
                {
                    g.drawImage(ItemMap.imageFlare,
                        taskMapContentX + Panel.mapX[(int)TileMap.planetID][taskPointIdx],
                        taskMapContentY + Panel.mapY[(int)TileMap.planetID][taskPointIdx], 3);
                }
            }

            // Vẽ vị trí nhân vật (icon đầu + tên map hiện tại)
            if (charMapX >= 0)
            {
                int head = Char.myCharz().head;
                Part part = GameScr.parts[head];
                SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id,
                    taskMapContentX + charMapX, taskMapContentY + charMapY + 5, 0, 3);

                int nameAlign = mFont.CENTER;
                if (charMapX <= 40) nameAlign = mFont.LEFT;
                if (charMapX >= imgW - 40) nameAlign = mFont.RIGHT;
                mFont.tahoma_7b_yellow.drawString(g, TileMap.mapName,
                    taskMapContentX + charMapX, taskMapContentY + charMapY - 12, nameAlign, mFont.tahoma_7_grey);
            }

            // Vẽ vị trí đã click (icon bàn tay + tên địa điểm)
            if (taskMapClickedIndex >= 0 && taskMapClickedIndex < Panel.mapX[(int)TileMap.planetID].Length)
            {
                int clickedPx = Panel.mapX[(int)TileMap.planetID][taskMapClickedIndex] + taskMapContentX;
                int clickedPy = Panel.mapY[(int)TileMap.planetID][taskMapClickedIndex] + taskMapContentY;
                int clickedMapId = Panel.mapId[(int)TileMap.planetID][taskMapClickedIndex];

                // Vẽ icon bàn tay
                if (Panel.imgBantay != null)
                {
                    g.drawImage(Panel.imgBantay, clickedPx, clickedPy, StaticObj.TOP_RIGHT);
                }

                // Vẽ tên địa điểm
                if (TileMap.mapNames != null && clickedMapId >= 0 && clickedMapId < TileMap.mapNames.Length)
                {
                    int clickAlign = mFont.CENTER;
                    if (clickedPx - taskMapContentX + scrollX <= 30) clickAlign = mFont.LEFT;
                    if (clickedPx - taskMapContentX + scrollX >= imgW - 30) clickAlign = mFont.RIGHT;
                    mFont.tahoma_7b_yellow.drawString(g, TileMap.mapNames[clickedMapId],
                        clickedPx, clickedPy - 12, clickAlign, mFont.tahoma_7_grey);
                }
            }

            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(oldClipX, oldClipY, oldClipW, oldClipH);

            // Vẽ mũi tên chỉ hướng nhiệm vụ ngoài viewport (không cần translate)
            if (taskPointIdx >= 0)
            {
                int tpx = Panel.mapX[(int)TileMap.planetID][taskPointIdx];
                int tpy = Panel.mapY[(int)TileMap.planetID][taskPointIdx];
                if (tpx < scrollX)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 5,
                        taskMapContentX + 5, taskMapContentY + taskMapContentH / 2 - 4, 0);
                }
                if (tpx > scrollX + taskMapContentW)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 6,
                        taskMapContentX + taskMapContentW - 5, taskMapContentY + taskMapContentH / 2 - 4, StaticObj.TOP_RIGHT);
                }
                if (tpy < scrollY)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 1,
                        taskMapContentX + taskMapContentW / 2, taskMapContentY + 5, StaticObj.TOP_CENTER);
                }
                if (tpy > scrollY + taskMapContentH)
                {
                    g.drawRegion(Mob.imgHP, 0, 0, 9, 6, 0,
                        taskMapContentX + taskMapContentW / 2, taskMapContentY + taskMapContentH - 5, StaticObj.BOTTOM_HCENTER);
                }
            }
        }
        else
        {
            // Không có bản đồ
            mFont.tahoma_7_grey.drawString(g, "Không có bản đồ", mapBoxX + mapBoxW / 2, frameY + frameH / 2, mFont.CENTER);
        }
    }

    private static void PaintTaskProgress(mGraphics g, Task task, int x, int y, int w, int bottom)
    {
        if (task.subNames == null)
        {
            return;
        }
        for (int i = 0; i < task.subNames.Length && y < bottom; i++)
        {
            string name = task.subNames[i];
            if (name == null || name.Length == 0)
            {
                continue;
            }
            bool active = i == task.index;
            bool done = i < task.index;
            string progress = string.Empty;
            if (task.counts != null && i < task.counts.Length && task.counts[i] > 0)
            {
                int cur = active ? task.count : (done ? task.counts[i] : 0);
                progress = " (" + cur + "/" + task.counts[i] + ")";
            }
            int bulletColor = done ? 0x4CAF50 : (active ? 0xD89C21 : 0x8C6A3D);
            Fill(g, x, y + 3, 5, 5, bulletColor);
            mFont font = active ? mFont.tahoma_7b_dark : mFont.tahoma_7b_dark;
            font.drawString(g, name + progress, x + 10, y, mFont.LEFT);
            y += 11;
        }
    }

    private static void TryHandleTaskMapClick(bool isFire)
    {
        if (Panel.imgMap == null || !Panel.isPaintMap)
        {
            return;
        }
        int px = GameCanvas.px;
        int py = GameCanvas.py;
        // Kiểm tra click trong vùng bản đồ
        if (px < taskMapContentX || px > taskMapContentX + taskMapContentW ||
            py < taskMapContentY || py > taskMapContentY + taskMapContentH)
        {
            return;
        }
        if (!isFire)
        {
            return;
        }
        // Chuyển đổi tọa độ click sang tọa độ bản đồ
        int mapClickX = px - taskMapContentX + taskMapScrollX;
        int mapClickY = py - taskMapContentY + taskMapScrollY;
        // Tìm vị trí gần nhất trên bản đồ
        taskMapClickedIndex = -1;
        for (int i = 0; i < Panel.mapX[(int)TileMap.planetID].Length; i++)
        {
            int mx = Panel.mapX[(int)TileMap.planetID][i];
            int my = Panel.mapY[(int)TileMap.planetID][i];
            if (Res.inRect(mx - 15, my - 15, 30, 30, mapClickX, mapClickY))
            {
                taskMapClickedIndex = i;
                break;
            }
        }
    }

}
