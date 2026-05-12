using Assets.src.g;
using System;
using UnityEngine;

public partial class CustomInventoryPanel
{

    private static void StartOriginalPotentialMenu(int statIndex, int rowY)
    {
        if (GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.selected = statIndex;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        Char c = Char.myCharz();
        if (c == null)
        {
            return;
        }
        MyVector actions = new MyVector(string.Empty);
        string statName = string.Empty;
        int incValue = 1;
        long cost1 = 0L;
        long cost10 = 0L;
        long cost100 = 0L;
        int num = 1000;
        if (statIndex == 0)
        {
            statName = mResources.HP;
            incValue = c.hpFrom1000TiemNang;
            cost1 = (long)c.cHPGoc + num;
            cost10 = 10L * (2L * ((long)c.cHPGoc + num) + 180L) / 2L;
            cost100 = 100L * (2L * ((long)c.cHPGoc + num) + 1980L) / 2L;
        }
        else if (statIndex == 1)
        {
            statName = mResources.KI;
            incValue = c.mpFrom1000TiemNang;
            cost1 = (long)c.cMPGoc + num;
            cost10 = 10L * (2L * ((long)c.cMPGoc + num) + 180L) / 2L;
            cost100 = 100L * (2L * ((long)c.cMPGoc + num) + 1980L) / 2L;
        }
        else if (statIndex == 2)
        {
            statName = mResources.hit_point;
            incValue = c.damFrom1000TiemNang;
            cost1 = (long)c.cDamGoc * c.expForOneAdd;
            cost10 = 10L * (2L * (long)c.cDamGoc + 9L) / 2L * c.expForOneAdd;
            cost100 = 100L * (2L * (long)c.cDamGoc + 99L) / 2L * c.expForOneAdd;
        }
        else if (statIndex == 3)
        {
            statName = mResources.armor;
            incValue = 1;
            cost1 = 2L * (c.cDefGoc + 5L) / 2L * 100000L;
            cost10 = 10L * (2L * (c.cDefGoc + 5L) + 9L) / 2L * 100000L;
            cost100 = 100L * (2L * (c.cDefGoc + 5L) + 99L) / 2L * 100000L;
        }
        else if (statIndex == 4)
        {
            int idx = c.cCriticalGoc;
            if (idx > Panel.t_tiemnang.Length - 1)
            {
                idx = Panel.t_tiemnang.Length - 1;
            }
            long cost = Panel.t_tiemnang[idx];
            if (c.cTiemNang < cost)
            {
                GameCanvas.startOKDlg(mResources.not_enough_potential_point1 + Res.formatNumber2(c.cTiemNang) + mResources.not_enough_potential_point2 + Res.formatNumber2(cost));
                return;
            }
            GameCanvas.startYesNoDlg(mResources.use_potential_point_for1 + Res.formatNumber(cost) + mResources.use_potential_point_for2 + c.criticalFrom1000Tiemnang + mResources.for_crit, new Command(mResources.increase_upper, panel, 9000, null), new Command(mResources.CANCEL, panel, 4007, null));
            return;
        }
        if (c.cTiemNang < cost1)
        {
            GameCanvas.startOKDlg(mResources.not_enough_potential_point1 + c.cTiemNang + mResources.not_enough_potential_point2 + cost1);
            return;
        }
        actions.addElement(new Command(mResources.increase_upper + "\n" + incValue + " " + statName + "\n-" + Res.formatNumber2(cost1), panel, 9000, null));
        if (c.cTiemNang >= cost10)
        {
            actions.addElement(new Command(mResources.increase_upper + "\n" + (incValue * 10) + " " + statName + "\n-" + Res.formatNumber2(cost10), panel, 9006, null));
        }
        if (c.cTiemNang >= cost100)
        {
            actions.addElement(new Command(mResources.increase_upper + "\n" + (incValue * 100) + " " + statName + "\n-" + Res.formatNumber2(cost100), panel, 9007, null));
        }
        actions.addElement(new Command(ModFunc.strInCrease, ModFunc.GI(), 100, statIndex.ToString() + "-" + false.ToString()));
        GameCanvas.menu.startAt(actions, panelX, rowY + 40);
    }

    private static void StartOriginalSkillMenu(SkillTemplate template, Skill skill, int index)
    {
        if (template == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        MyVector actions = new MyVector(string.Empty);
        Skill nextSkill = null;
        if (skill != null)
        {
            if (skill.point == template.maxPoint)
            {
                actions.addElement(new Command(mResources.make_shortcut, panel, 9003, skill.template));
                actions.addElement(new Command(mResources.CLOSE, 2));
            }
            else
            {
                nextSkill = template.skills[skill.point];
                actions.addElement(new Command(mResources.UPGRADE, panel, 9002, nextSkill));
                actions.addElement(new Command(mResources.make_shortcut, panel, 9003, skill.template));
            }
        }
        else if (template.skills != null && template.skills.Length > 0)
        {
            nextSkill = template.skills[0];
            actions.addElement(new Command(mResources.learn, panel, 9004, nextSkill));
        }
        int menuX = panelX + panelW / 2 - actions.size() * 22;
        int menuY = panelY + panelH - 18;
        GameCanvas.menu.startAt(actions, menuX, menuY);
        panel.addSkillDetail(template, skill, nextSkill);
    }

    private static void StartPetBodyItemMenu(Item item, int petBodyIndex)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.type = 21;
        panel.currentTabIndex = 0;
        panel.selected = petBodyIndex;
        panel.currItem = item;
        MyVector actions = new MyVector();
        actions.addElement(new Command(mResources.MOVEOUT, panel, 2006, item));
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartOriginalItemMenu(Item item, bool isBodySlot, int inventorySelected)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.customSelectInventoryItem(inventorySelected);
        panel.currItem = item;
        MyVector actions = new MyVector();
        if (isBodySlot)
        {
            actions.addElement(new Command(mResources.GETOUT, panel, 2002, item));
        }
        else if (item.isTypeBody())
        {
            actions.addElement(new Command(mResources.USE, panel, 2000, item));
            if (Char.myCharz().havePet)
            {
                actions.addElement(new Command(mResources.MOVEFORPET, panel, 2005, item));
            }
            if (Char.myCharz().havePet2)
            {
                actions.addElement(new Command(ModFunc.strUseForPet2, panel, 2007, item));
            }
        }
        else
        {
            actions.addElement(new Command(mResources.USE, panel, 2001, item));
            if (Char.myCharz().havePet)
            {
                actions.addElement(new Command(mResources.MOVEFORPET, panel, 2005, item));
            }
            if (Char.myCharz().havePet2)
            {
                actions.addElement(new Command(ModFunc.strUseForPet2, panel, 2007, item));
            }
        }
        actions.addElement(new Command(mResources.THROW, panel, 2003, item));
        Char.myCharz().setPartTemp(item.headTemp, item.bodyTemp, item.legTemp, item.bagTemp);
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartOriginalBoxMenu(Item item, int boxIndex)
    {
        if (item == null || GameCanvas.panel == null)
        {
            return;
        }
        Panel panel = GameCanvas.panel;
        panel.X = panelX;
        panel.Y = panelY;
        panel.W = panelW;
        panel.H = panelH;
        panel.customSelectInventoryItem(boxIndex);
        panel.currItem = item;
        MyVector actions = new MyVector();
        actions.addElement(new Command(mResources.GETOUT, panel, 1000, item));
        panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        StartItemActionMenu(actions);
    }

    private static void StartAutoItemMenu(Item item, int bagIndex)
    {
        if (item == null || item.template == null)
        {
            return;
        }
        AutoItem.ItemAuto auto = new AutoItem.ItemAuto(item.template.id, (short)bagIndex, item.getFullName());
        MyVector actions = new MyVector();
        if (AutoItem.mAutoItem.method_1(item.template.id))
        {
            actions.addElement(new Command("Tắt Auto", AutoItem.mAutoItem, 2, item.template.id));
        }
        else
        {
            actions.addElement(new Command("Auto dùng", AutoItem.mAutoItem, 1, auto));
        }
        actions.addElement(new Command("Bán", AutoItem.mAutoItem, 3, new AutoItem.ItemAuto(item.template.id, (short)bagIndex, false, true)));
        if (GameCanvas.panel != null)
        {
            GameCanvas.panel.currItem = item;
            GameCanvas.panel.customShowItemDetail(item, selectedItemX, selectedItemY, panelX, panelY, panelW, panelH);
        }
        StartItemActionMenu(actions);
    }

    private static void StartItemActionMenu(MyVector actions)
    {
        // Tính vị trí menu gắn với khung thông tin item (cp)
        Panel panel = GameCanvas.panel;
        int menuTotalW = actions.size() * 60;
        int menuX;
        int menuY;
        if (panel != null && panel.cp != null)
        {
            // Đặt menu ngay bên dưới khung thông tin item, canh giữa theo khung cp
            menuX = panel.cp.cx + panel.cp.sayWidth / 2 - menuTotalW / 2;
            menuY = panel.cp.cy + panel.cp.ch + 2;
        }
        else
        {
            // Fallback: đặt theo vị trí item slot
            int itemSlotH = (selectedBodyIndex >= 0) ? 24 : 26;
            menuX = selectedItemX + 17 - menuTotalW / 2;
            menuY = selectedItemY + itemSlotH + 2;
        }
        // Clamp menuX trong panel
        if (menuX < panelX + 4)
        {
            menuX = panelX + 4;
        }
        if (menuX + menuTotalW > panelX + panelW - 4)
        {
            menuX = panelX + panelW - 4 - menuTotalW;
        }
        GameCanvas.menu.startAt(actions, menuX, menuY);
    }

    private static void PaintOriginalItemDetail(mGraphics g)
    {
        if (GameCanvas.panel == null || selectedItemInfo == null)
        {
            return;
        }
        GameCanvas.panel.paintDetail(g);
    }

    private static bool TrySelectBodySlot(Item[] body, int bodyIndex, int x, int y, int w, int h, bool isFire)
    {
        Item clicked = GetItem(body, bodyIndex);
        if (!Hit(GameCanvas.px, GameCanvas.py, x, y, w, h) || clicked == null)
        {
            return false;
        }
        selectedBodyIndex = bodyIndex;
        selectedBagIndex = -1;
        selectedBoxIndex = -1;
        selectedAutoIndex = -1;
        selectedItemInfo = clicked;
        selectedItemX = x;
        selectedItemY = y;
        if (isFire)
        {
            StartOriginalItemMenu(clicked, true, bodyIndex);
            SoundMn.gI().panelClick();
        }
        else
        {
            if (GameCanvas.panel != null)
            {
                GameCanvas.panel.customSelectInventoryItem(bodyIndex);
            }
        }
        return true;
    }

}
