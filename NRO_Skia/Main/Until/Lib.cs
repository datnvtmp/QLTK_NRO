using System;
using System.Text;

public static class Lib
{
    // ═══════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════
    public const int TDLT_ITEM_ICON = 4387;
    public const int ITEM_ID_HOI_THE_LUC_NHO_1 = 212;
    public const int ITEM_ID_HOI_THE_LUC_NHO_2 = 211;
    public const int ITEM_ID_NHAN_THOI_KHONG = 992;
    public const int ITEM_ID_TDLT = 521;
    public static readonly int[] ID_ITEM_NHO = { 212, 211 };
    public static readonly int[] ITEM_ID_PORATA = { 454, 921, 1884 };

    // ═══════════════════════════════════════════
    // TIME
    // ═══════════════════════════════════════════
    public static long TimeNow() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // ═══════════════════════════════════════════
    // TELEPORT
    // ═══════════════════════════════════════════
    public static void TeleportTo(int x, int y)
    {
        try
        {
            Char.myCharz().currentMovePoint = null;
            Char.myCharz().cx = x;
            Char.myCharz().cy = y;
        }
        catch { }
    }

    // ═══════════════════════════════════════════
    // ITEM BAG HELPERS
    // ═══════════════════════════════════════════
    public static bool ExistItemBag(int itemId)
    {
        var bag = Char.myCharz().arrItemBag;
        if (bag == null) return false;
        foreach (var item in bag)
            if (item?.template.id == itemId) return true;
        return false;
    }

    public static Item? GetItemByID(int itemId)
    {
        var bag = Char.myCharz().arrItemBag;
        if (bag == null) return null;
        foreach (var item in bag)
            if (item?.template.id == itemId) return item;
        return null;
    }

    public static int GetItemQuantity(int itemId)
    {
        try
        {
            var bag = Char.myCharz().arrItemBag;
            if (bag == null) return 0;
            foreach (var item in bag)
                if (item?.template.id == itemId) return item.quantity;
            return 0;
        }
        catch { return 0; }
    }

    // ═══════════════════════════════════════════
    // USE ITEM
    // ═══════════════════════════════════════════
    public static void UseItem(int itemId)
    {
        var bag = Char.myCharz().arrItemBag;
        if (bag == null) return;
        foreach (var item in bag)
        {
            if (item?.template.id == itemId)
            {
                Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                break;
            }
        }
    }

    public static bool UseItemBoolean(int itemId)
    {
        var bag = Char.myCharz().arrItemBag;
        if (bag == null) return false;
        foreach (var item in bag)
        {
            if (item?.template.id == itemId)
            {
                Service.gI().useItem(0, 1, (sbyte)item.indexUI, -1);
                return true;
            }
        }
        return false;
    }

    public static bool UseStaminaItem()
    {
        foreach (int id in ID_ITEM_NHO)
            if (UseItemBoolean(id)) return true;
        return false;
    }

    public static bool UsePorata()
    {
        foreach (int id in ITEM_ID_PORATA)
            if (UseItemBoolean(id)) return true;
        return false;
    }

    // ═══════════════════════════════════════════
    // TDLT (Tự động luyện tập)
    // ═══════════════════════════════════════════
    public static bool HasTeleportItem() => ItemTime.isExistItem(TDLT_ITEM_ICON);

    public static void TurnOnTDLT()
    {
        GameScr.info1.addInfo("Bật tự động luyện tập");
        try
        {
            var bag = Char.myCharz().arrItemBag;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i]?.template.id != ITEM_ID_TDLT) continue;

                // Kiểm tra đã bật chưa
                for (int j = 0; j < Char.vItemTime.size(); j++)
                    if (((ItemTime)Char.vItemTime.elementAt(j)).idIcon == TDLT_ITEM_ICON) return;

                GameScr.info1.addInfo("Bật tự động luyện tập1");
                Service.gI().useItem(0, 1, (sbyte)i, -1);
                break;
            }
        }
        catch { }
    }

    // ═══════════════════════════════════════════
    // ITEM TIME
    // ═══════════════════════════════════════════
    public static int GetItemTimeInSeconds(int idIcon) => GetItemTimeById(idIcon)?.coutTime ?? 0;

    public static ItemTime? GetItemTimeById(int id)
    {
        for (int i = 0; i < Char.vItemTime.size(); i++)
        {
            var it = (ItemTime)Char.vItemTime.elementAt(i);
            if (it.idIcon == id) return it;
        }
        return null;
    }

    // ═══════════════════════════════════════════
    // MAGIC TREE
    // ═══════════════════════════════════════════
    public static void UseMagicTree()
    {
        bool hasTree = false;
        foreach (var item in Char.myCharz().arrItemBag)
        {
            if (item?.template.type == 6) { hasTree = true; break; }
        }

        if (!hasTree && GameCanvas.gameTick % 500 == 0)
            Service.gI().requestPean();

        DoUseMagicTree();
    }

    public static bool DoUseMagicTree()
    {
        var bag = Char.myCharz().arrItemBag;
        if (bag == null) return false;

        foreach (var item in bag)
        {
            if (item?.template.type == 6 && GameCanvas.gameTick % 500 == 0)
            {
                Service.gI().useItem(0, 1, -1, item.template.id);
                return true;
            }
        }
        return false;
    }

    // ═══════════════════════════════════════════
    // DISTANCE
    // ═══════════════════════════════════════════
    public static int CalculateShortestDistance(IMapObject a, IMapObject b)
        => Res.distance(a.getX(), a.getY(), b.getX(), b.getY());

    // ═══════════════════════════════════════════
    // TEXT / MENU HELPERS
    // ═══════════════════════════════════════════
    public static string NormalizeText(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var sb = new StringBuilder();
        foreach (char c in input.ToLower())
            if (c >= 32 && c != 127 && c != ' ' && c != '\t' && c != '\n' && c != '\r')
                sb.Append(c);
        return sb.ToString();
    }

    public static int FindMenuByName(string menuName)
    {
        if (GameCanvas.menu.menuItems == null) return -1;

        string search = NormalizeText(menuName);
        for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
        {
            var cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
            if (cmd?.caption == null) continue;
            if (NormalizeText(cmd.caption).Contains(search)) return i;
        }
        return -1;
    }
}