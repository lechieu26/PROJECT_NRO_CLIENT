using Assets.src.g;
using System;
using UnityEngine;

public class ChatLogPopup : IActionListener, IChatable
{
    public static bool isShow;
    private static ChatLogPopup instance;

    private int x, y, w, h;
    private TField tfChat;
    private Command cmdSend;
    
    private int scrollY, scrollTargetY;
    private int ITEM_HEIGHT = 28;
    private int lastDragY;
    private bool isDragging;
    private int cmyLim;

    public static ChatLogPopup gI()
    {
        if (instance == null) instance = new ChatLogPopup();
        return instance;
    }

    public ChatLogPopup()
    {
        w = 300;
        h = 240;
        x = (GameCanvas.w - w) / 2;
        y = (GameCanvas.h - h) / 2;

        tfChat = new TField();
        tfChat.width = w - 70;
        tfChat.height = 20;
        tfChat.x = x + 10;
        tfChat.y = y + h - 28;
        tfChat.isFocus = true;
        tfChat.setMaxTextLenght(100);

        cmdSend = new Command("Gửi", this, 2, null);
        cmdSend.w = 50;
        cmdSend.hw = 25;
        cmdSend.type = 2; // Button nhỏ 50px
    }

    public void Toggle()
    {
        isShow = !isShow;
        if (isShow)
        {
            tfChat.setFocusWithKb(true);
            // Reset position if needed
            x = (GameCanvas.w - w) / 2;
            y = (GameCanvas.h - h) / 2;
            tfChat.x = x + 10;
            tfChat.y = y + h - 28;
        }
    }

    public void Update()
    {
        if (!isShow) return;

        // Kiểm tra đóng popup khi click ra ngoài ngay từ đầu Update
        if (GameCanvas.isPointerJustRelease && !IsPointerInPopup() && !tfChat.isFocus)
        {
            isShow = false;
            return;
        }

        tfChat.update();
        
        cmdSend.x = x + w - 55;
        cmdSend.y = y + h - 30;
        if (cmdSend.isPointerPressInside())
        {
            cmdSend.performAction();
        }

        if (GameCanvas.keyAsciiPress != 0)
        {
            if (GameCanvas.keyAsciiPress == 10 || GameCanvas.keyAsciiPress == -5) // Enter
            {
                perform(2, null);
            }
            else
            {
                tfChat.keyPressed(GameCanvas.keyAsciiPress);
            }
            GameCanvas.keyAsciiPress = 0;
        }

        if (GameCanvas.isPointerDown)
        {
            if (GameCanvas.isPointer(x, y + 25, w, h - 60))
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

        // Mouse wheel scroll
        if (GameCanvas.pXYScrollMouse != 0)
        {
            scrollTargetY -= GameCanvas.pXYScrollMouse * 30;
            GameCanvas.pXYScrollMouse = 0;
        }

        if (GameCanvas.panel == null || GameCanvas.panel.logChat == null) return;
        MyVector logs = GameCanvas.panel.logChat;
        cmyLim = logs.size() * ITEM_HEIGHT - (h - 60);
        if (cmyLim < 0) cmyLim = 0;

        if (scrollTargetY < 0) scrollTargetY = 0;
        if (scrollTargetY > cmyLim) scrollTargetY = cmyLim;

        if (scrollY != scrollTargetY)
        {
            scrollY += (scrollTargetY - scrollY) / 4;
            if (Mathf.Abs(scrollTargetY - scrollY) < 1) scrollY = scrollTargetY;
        }

        // Bỏ kiểm tra keyPressed cũ nếu có
    }

    public void Paint(mGraphics g)
    {
        if (!isShow) return;

        // Vẽ nền popup
        PopUp.paintPopUp(g, x, y, w, h, -1, true);
        
        mFont.tahoma_7b_white.drawString(g, "Chat thế giới", x + w / 2, y + 7, mFont.CENTER);

        // Nút Đóng (X)
        int closeX = x + w - 20;
        int closeY = y + 5;
        g.setColor(16711680);
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
        int clipH = h - 60;

        g.setClip(clipX, clipY, clipW, clipH);
        g.translate(0, -scrollY);
        if (GameCanvas.panel == null || GameCanvas.panel.logChat == null)
        {
            g.translate(0, scrollY);
            return;
        }
        MyVector logs = GameCanvas.panel.logChat;
        for (int i = 0; i < logs.size(); i++)
        {
            InfoItem infoItem = (InfoItem)logs.elementAt(i);
            int rowY = clipY + i * ITEM_HEIGHT;

            // Culling
            if (rowY + ITEM_HEIGHT < clipY + scrollY || rowY > clipY + scrollY + clipH) continue;

            int headX = x + 10;
            int headY = rowY;
            int textX = headX + 26;

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
                        SmallImage.drawSmallImage(g, (int)part.pi[Char.CharInfo[0][0][0]].id, headX + (int)part.pi[Char.CharInfo[0][0][0]].dx, headY + (int)part.pi[Char.CharInfo[0][0][0]].dy, 0, 0);
                    }
                }

                // Tên nhân vật
                mFont.tahoma_7b_green2.drawString(g, (infoItem.charInfo.isTichXanh ? "     " : string.Empty) + infoItem.charInfo.cName, textX, rowY, 0);
            }

            // Nội dung chat
            string msg = "";
            try { 
                string[] parts = Res.split(infoItem.s, "|", 0);
                msg = (parts.Length > 2) ? parts[2] : infoItem.s; 
            } catch { msg = infoItem.s; }
            
            if (!infoItem.isChatServer)
                mFont.tahoma_7_blue.drawString(g, msg, textX, rowY + 12, 0);
            else
                mFont.tahoma_7_red.drawString(g, msg, textX, rowY + 12, 0);
        }

        g.translate(0, scrollY);
        g.setClip(0, 0, GameCanvas.w, GameCanvas.h);

        // Vẽ ô nhập text
        tfChat.paint(g);

        // Vẽ nút Gửi bằng Command style của game
        cmdSend.paint(g);
    }

    public bool IsPointerInPopup()
    {
        if (!isShow) return false;
        return GameCanvas.isPointer(x, y, w, h);
    }

    public void perform(int idAction, object p)
    {
        if (idAction == 1) isShow = false;
        if (idAction == 2)
        {
            string text = tfChat.getText();
            if (!string.IsNullOrEmpty(text))
            {
                Service.gI().chatGlobal(text);
                tfChat.setText("");
                // Cuộn xuống cuối khi gửi tin mới
                scrollTargetY = cmyLim;
            }
        }
    }

    public void onChatFromMe(string text, string to) { }
    public void onCancelChat() { }
}
