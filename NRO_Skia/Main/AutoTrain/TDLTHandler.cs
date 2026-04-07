namespace Mod
{
    /// <summary>
    /// Tự động bật Tự Động Luyện Tập (TDLT) khi cần.
    /// </summary>
    public static class TDLTHandler
    {
        public static void TurnOn()
        {
            try
            {
                var bag = Char.myCharz().arrItemBag;
                for (int i = 0; i < bag.Length; i++)
                {
                    if (bag[i]?.template.id != Lib.ITEM_ID_TDLT) continue;

                    if (IsAlreadyActive()) return;

                    Service.gI().useItem(0, 1, (sbyte)i, -1);
                    break;
                }
            }
            catch { }
        }

        private static bool IsAlreadyActive()
        {
            for (int i = 0; i < Char.vItemTime.size(); i++)
                if (((ItemTime)Char.vItemTime.elementAt(i)).idIcon == Lib.TDLT_ITEM_ICON)
                    return true;
            return false;
        }
    }
}