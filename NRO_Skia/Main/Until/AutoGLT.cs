using System;
using System.Threading;

namespace Mod
{
    public static class AutoGLT
    {
        private const int TIME_SLEEP = 2000;
        private const int MAX_TRIES = 5;

        // ID các loại giáp luyện tập theo thứ tự ưu tiên
        private static readonly int[] GLT_ITEM_IDS = { 1716, 536, 531, 535, 530, 534, 529 };

        // =====================================================================
        // PUBLIC API
        // =====================================================================

        public static void Equip() => Run(isEquip: true);
        public static void Unequip() => Run(isEquip: false);

        private static void Run(bool isEquip)
        {
            var t = new Thread(() =>
            {
                try
                {
                    if (isEquip) EquipGLT();
                    else UseGLT();
                }
                catch (ThreadInterruptedException) { }
                catch (Exception e)
                {
                    Console.WriteLine($"AutoGLT Error: {e.Message}");
                }
            })
            { IsBackground = true, Name = "AutoGLT" };

            t.Start();
        }

        // =====================================================================
        // THÁO GIÁP
        // =====================================================================

        private static void EquipGLT()
        {
            int tries = MAX_TRIES;

            while (tries-- > 0)
            {
                try
                {
                    int bodyId = Char.myCharz().arrItemBody[6].template.id;
                    if (bodyId > 0)
                    {
                        Console.WriteLine("⏳ Chờ tháo giáp...");
                        Service.gI().getItem(5, 6);
                        return; // tháo xong thì thoát
                    }
                }
                catch { }

                Thread.Sleep(TIME_SLEEP);
            }
        }

        // =====================================================================
        // MẶC GIÁP
        // =====================================================================

        public static void UseGLT()
        {

            var bag = Char.myCharz().arrItemBag;
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i]?.template == null) continue;
                if(bag[i].template.type == Item.TYPE_TRAINSUIT)
                {
                    Service.gI().getItem(4, (sbyte)i);
                    return;
                }
            }
        }
    }
}