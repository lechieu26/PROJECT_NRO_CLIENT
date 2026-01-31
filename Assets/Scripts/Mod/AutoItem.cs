using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using static AutoItem;

public class AutoItem : IActionListener, IChatable
{
    public AutoItem()
    {
        this.list_0 = new List<AutoItem.ItemAuto>();
        this.list_1 = new List<string>();
        this.list_2 = new List<string>();
        this.string_0 = new string[]
        {
            "Nhập delay",
            "giây"
        };
        this.string_1 = new string[]
        {
            "Nhập số lượng bán",
            "số lượng"
        };
        this.string_2 = new string[]
        {
            "Nhập số lượng mua",
            "số lượng"
        };
    }

    public void autoUseItemsFunc()
    {
        if (this.list_0.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < this.list_0.Count; i++)
        {
            AutoItem.ItemAuto itemAuto = this.list_0[i];
            if (mSystem.currentTimeMillis() - itemAuto.lastTimeUse > (long)(itemAuto.delay * 1000) && itemAuto != null)
            {
                itemAuto.lastTimeUse = mSystem.currentTimeMillis();
                int idxItem = ModFunc.GI().FindItemIndex(itemAuto.id);
                Service.gI().useItem(0, 1, (sbyte)idxItem, -1);
                return;
            }
        }
    }

    public void onChatFromMe(string text, string to)
    {
        ChatTextField chatTextField = ChatTextField.gI();
        if (chatTextField.tfChat.getText() == null || chatTextField.tfChat.getText().Equals(string.Empty) || text.Equals(string.Empty) || text == null)
        {
            chatTextField.isShow = false;
            return;
        }
        if (chatTextField.strChat.Equals(this.string_0[0]))
        {
            try
            {
                int delay = int.Parse(chatTextField.tfChat.getText());
                this.gclass6_0.delay = delay;
                GameScr.info1.addInfo(string.Concat(new string[]
                {
                    "Auto ",
                    this.gclass6_0.name,
                    ": ",
                    delay.ToString(),
                    " giây"
                }), 0);
                this.list_0.Add(this.gclass6_0);
            }
            catch
            {
                GameScr.info1.addInfo("Delay Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
            }
            ModFunc.ResetTF();
            return;
        }
        if (chatTextField.strChat.Equals(this.string_2[0]))
        {
            try
            {
                int quantity = int.Parse(chatTextField.tfChat.getText());
                this.gclass6_0.quantity = quantity;
                new Thread(new ThreadStart(this.method_11)).Start();
            }
            catch
            {
                GameScr.info1.addInfo("Số Lượng Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
            }
            ModFunc.ResetTF();
            return;
        }
        if (!chatTextField.strChat.Equals(this.string_1[0]))
        {
            return;
        }
        try
        {
            int quantity2 = int.Parse(chatTextField.tfChat.getText());
            this.gclass6_0.quantity = quantity2;
            new Thread(new ThreadStart(this.method_12)).Start();
        }
        catch
        {
            GameScr.info1.addInfo("Số Lượng Không Hợp Lệ, Vui Lòng Nhập Lại!", 0);
        }
        ModFunc.ResetTF();
    }

    public void onCancelChat()
    {
    }

    public void perform(int idAction, object p)
    {
        switch (idAction)
        {
            case 1:
                GameCanvas.panel.hide();
                this.method_3((AutoItem.ItemAuto)p);
                return;
            case 2:
                this.method_2((int)p);
                return;
            case 3:
                this.method_4((AutoItem.ItemAuto)p);
                return;
            case 4:
                this.list_1.Add(((Item)p).getFullName());
                return;
            case 5:
                this.list_2.Add(((Item)p).getFullName());
                return;
            case 6:
                this.list_1.Remove(((Item)p).getFullName());
                return;
            case 7:
                this.list_2.Remove(((Item)p).getFullName());
                return;
            default:
                return;
        }
    }
    public bool method_1(int itemID)
    {
        for (int i = 0; i < this.list_0.Count; i++)
        {
            if (this.list_0[i].id == itemID)
            {
                return true;
            }
        }
        return false;
    }

    private void method_2(int int_0)
    {
        for (int i = 0; i < this.list_0.Count; i++)
        {
            if (this.list_0[i].id == int_0)
            {
                this.list_0.RemoveAt(i);
                return;
            }
        }
    }

    public const int TIME_USE_ITEM_BUFF = 600; // 600s = 10m
    public const int TIME_USE_NORMAL = 1;
    private void method_3(AutoItem.ItemAuto gclass6_1)
    {
        this.gclass6_0 = gclass6_1;
        this.gclass6_0.delay = TIME_USE_NORMAL;
        int[] itemBuff = { 380, 381, 1150, 382, 1152, 383, 1153, 384, 1154, 385, 1155, 663, 664, 665, 667, 752, 753, 764, 880, 881, 882};
        foreach(int idx in itemBuff)
        {
            if (this.gclass6_0.id == idx)
            {
                this.gclass6_0.delay = TIME_USE_ITEM_BUFF;
                break;
            }
        }  
        this.list_0.Add(this.gclass6_0);
    }

    private void method_4(AutoItem.ItemAuto gclass6_1)
    {
        this.gclass6_0 = gclass6_1;
        GameCanvas.panel.isShow = false;
        if (gclass6_1.isSell)
        {
            ModFunc.InputTF(this, this.string_1[0], this.string_1[1], TField.INPUT_TYPE_NUMERIC);
            return;
        }
        ModFunc.InputTF(this, this.string_2[0], this.string_2[1], TField.INPUT_TYPE_NUMERIC);
    }
    private void method_5(AutoItem.ItemAuto gclass6_1)
    {
        Thread.Sleep(100);
        short index = gclass6_1.index;
        while (gclass6_1.quantity > 0)
        {
            if (global::Char.myCharz().arrItemBag[(int)index] != null)
            {
                if (global::Char.myCharz().arrItemBag[(int)index] == null || global::Char.myCharz().arrItemBag[(int)index].template.id == (short)gclass6_1.id)
                {
                    Service.gI().saleItem(0, 1, (short)global::Char.myCharz().arrItemBag[(int)index].indexUI);
                    Thread.Sleep(100);
                    Service.gI().saleItem(1, 1, (short)global::Char.myCharz().arrItemBag[(int)index].indexUI);
                    Thread.Sleep(1000);
                    gclass6_1.quantity--;
                    if (global::Char.myCharz().xu <= 1963100000L)
                    {
                        continue;
                    }
                    GameScr.info1.addInfo("Xong!", 0);
                    return;
                }
            }
            GameScr.info1.addInfo("Không Tìm Thấy Item!", 0);
            return;
        }
        GameScr.info1.addInfo("Xong!", 0);
    }
    private void method_6(AutoItem.ItemAuto gclass6_1)
    {
        while (gclass6_1.quantity > 0 && !GameScr.gI().isBagFull())
        {
            Service.gI().buyItem((sbyte)((!gclass6_1.isGold) ? 1 : 0), gclass6_1.id, 0);
            gclass6_1.quantity--;
            Thread.Sleep(1000);
        }
        GameScr.info1.addInfo("Xong!", 0);
    }

    public List<string> method_9()
    {
        return this.list_1;
    }

    public List<string> method_10()
    {
        return this.list_2;
    }

    [CompilerGenerated]
    private void method_11()
    {
        this.method_6(this.gclass6_0);
    }

    [CompilerGenerated]
    private void method_12()
    {
        this.method_5(this.gclass6_0);
    }

    public static AutoItem mAutoItem = new AutoItem();

    private List<AutoItem.ItemAuto> list_0;

    private AutoItem.ItemAuto gclass6_0;

    private List<string> list_1;

    private List<string> list_2;

    private bool bool_0;

    private string[] string_0;

    private string[] string_1;

    private string[] string_2;

    public class ItemAuto
    {
        public ItemAuto()
        {
        }

        public ItemAuto(int id, short index, string name)
        {
            this.id = id;
            this.index = index;
            this.name = name;
        }
        public ItemAuto(int id, short index, bool isGold, bool isSell)
        {
            this.id = id;
            this.isGold = isGold;
            this.index = index;
            this.isSell = isSell;
        }

        public int id;

        public string name;

        public int quantity;

        public short index;

        public bool isGold;

        public bool isSell;

        public int delay;

        public long lastTimeUse;
    }

}
