using Assets.src.g;
using System;
using UnityEngine;

public class ListPopup : IActionListener
{
    protected int x, y, w, h;
    protected int scrollY, scrollTargetY;
    protected int ITEM_HEIGHT = 28;
    protected int lastDragY;
    protected bool isDragging;
    protected int cmyLim;
    public bool isShow;
    protected int selected = -1;
    
    protected string title;
    protected MyVector list;
    protected int type; // 0: Friend, 1: Enemy

    public ListPopup(string title, int type)
    {
        this.title = title;
        this.type = type;
        w = 280;
        h = 240;
        x = (GameCanvas.w - w) / 2;
        y = (GameCanvas.h - h) / 2;
    }

    public virtual void Toggle()
    {
        isShow = !isShow;
        if (isShow)
        {
            x = (GameCanvas.w - w) / 2;
            y = (GameCanvas.h - h) / 2;
            scrollTargetY = 0;
            scrollY = 0;
            RefreshData();
        }
    }

    protected virtual void RefreshData()
    {
        if (GameCanvas.panel == null) return;
        if (type == 0)
        {
            list = GameCanvas.panel.vFriend;
            if (list == null || list.size() == 0) Service.gI().friend(0, -1);
        }
        else
        {
            list = GameCanvas.panel.vEnemy;
            if (list == null || list.size() == 0) Service.gI().enemy(0, -1);
        }
    }

    public virtual void Update()
    {
        if (!isShow) return;

        // Kiểm tra đóng popup khi click ra ngoài ngay từ đầu Update
        if (GameCanvas.isPointerJustRelease && !IsPointerInPopup())
        {
            isShow = false;
            // Không set isPointerJustRelease = false ở đây để các thành phần bên dưới (như panel chính) vẫn nhận được sự kiện nếu cần
            return;
        }

        if (GameCanvas.isPointerDown)
        {
            if (GameCanvas.isPointer(x, y + 25, w, h - 35))
            {
                if (!isDragging)
                {
                    isDragging = true;
                    lastDragY = GameCanvas.py;
                }
                else
                {
                    int dy = GameCanvas.py - lastDragY;
                    scrollTargetY -= dy;
                    lastDragY = GameCanvas.py;
                }
            }
        }
        else
        {
            isDragging = false;
        }

        if (GameCanvas.pXYScrollMouse != 0)
        {
            scrollTargetY -= GameCanvas.pXYScrollMouse * 30;
            GameCanvas.pXYScrollMouse = 0;
        }

        RefreshData();
        if (list == null) return;

        cmyLim = list.size() * ITEM_HEIGHT - (h - 40);
        if (cmyLim < 0) cmyLim = 0;

        if (scrollTargetY < 0) scrollTargetY = 0;
        if (scrollTargetY > cmyLim) scrollTargetY = cmyLim;

        if (scrollY != scrollTargetY)
        {
            scrollY += (scrollTargetY - scrollY) / 4;
            if (Mathf.Abs(scrollTargetY - scrollY) < 1) scrollY = scrollTargetY;
        }
    }

    public virtual void Paint(mGraphics g)
    {
        if (!isShow) return;

        PopUp.paintPopUp(g, x, y, w, h, -1, true);
        mFont.tahoma_7b_white.drawString(g, title, x + w / 2, y + 7, mFont.CENTER);

        // Close button
        int closeX = x + w - 20;
        int closeY = y + 5;
        g.setColor(0xFF0000);
        g.fillRect(closeX, closeY, 15, 15);
        mFont.tahoma_7b_white.drawString(g, "X", closeX + 8, closeY + 1, mFont.CENTER);
        if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(closeX - 5, closeY - 5, 25, 25))
        {
            isShow = false;
            GameCanvas.isPointerJustRelease = false;
        }

        int clipX = x + 5;
        int clipY = y + 25;
        int clipW = w - 10;
        int clipH = h - 35;

        g.setClip(clipX, clipY, clipW, clipH);
        g.translate(0, -scrollY);

        if (list == null || list.size() == 0)
        {
            mFont.tahoma_7_grey.drawString(g, "Đang tải dữ liệu...", x + w / 2, clipY + clipH / 2, mFont.CENTER);
        }
        else
        {
            int iconColW = 24;
            for (int i = 0; i < list.size(); i++)
            {
                InfoItem infoItem = (InfoItem)list.elementAt(i);
                int rowY = clipY + i * ITEM_HEIGHT;
                if (rowY + ITEM_HEIGHT < clipY + scrollY || rowY > clipY + scrollY + clipH) continue;

                int iconX = clipX;
                int textBgX = clipX + iconColW;
                int textBgW = clipW - iconColW;
                int rowH = ITEM_HEIGHT - 1;

                // Nền ô icon (màu đậm hơn, giống game gốc 0x989355 / 0x919100)
                g.setColor(i == selected ? 0x919100 : 0x989355);
                g.fillRect(iconX, rowY, iconColW, rowH);

                // Nền ô text (màu sáng, giống game gốc 0xE7E3D2 / 0xF9F5CA)
                g.setColor(i == selected ? 0xF9F5CA : 0xE7E3D2);
                g.fillRect(textBgX, rowY, textBgW, rowH);

                if (infoItem.charInfo != null)
                {
                    // Icon vẽ giống game gốc - tọa độ trùng với ô icon
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

                    // Text giống game gốc
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

                // Handle click for row menu
                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(clipX, rowY, clipW, ITEM_HEIGHT))
                {
                    selected = i;
                    OnRowClick(infoItem, i);
                    GameCanvas.isPointerJustRelease = false;
                }
            }
        }

        g.translate(0, scrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);
    }

    protected virtual void OnRowClick(InfoItem item, int index)
    {
        if (GameCanvas.panel == null) return;

        // Đồng bộ tọa độ để Menu của GameCanvas tính toán vị trí hiển thị chính xác
        GameCanvas.panel.X = x;
        GameCanvas.panel.yScroll = y + 25;
        GameCanvas.panel.cmy = scrollY;
        GameCanvas.panel.ITEM_HEIGHT = ITEM_HEIGHT;
        GameCanvas.panel.currInfoItem = index;
        GameCanvas.panel.selected = index;

        if (type == 0) // Friend
        {
            GameCanvas.panel.type = 11;
            GameCanvas.panel.vFriend = list;
            GameCanvas.panel.doFireFriend();
        }
        else // Enemy
        {
            GameCanvas.panel.type = 16;
            GameCanvas.panel.vEnemy = list;
            GameCanvas.panel.doFireEnemy();
        }
    }

    public void perform(int idAction, object p) { }

    public bool IsPointerInPopup()
    {
        if (!isShow) return false;
        return GameCanvas.isPointer(x, y, w, h);
    }
}
