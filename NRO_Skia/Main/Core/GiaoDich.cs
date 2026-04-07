using System;
using System.Collections.Generic;
using System.Text;

namespace NRO_Skia.Main.Core
{
    internal class GiaoDich
    {
        public static long lastTimeTradeAction;
        public static long lastTimeInvite;
        public static bool isSentItem;

        public enum TradeStateGD
        {
            Idle,
            Inviting,
            AddingItem,
            Locking,
            Accepting
        }

        public static TradeStateGD CurrentTradeStateGD = TradeStateGD.Idle;
        public static long lastTradeStateTime;
        public static bool isSentItemInState;





        public static void TestGD()
        {
            // 1. Kiểm tra vật phẩm (Đổi sang CSKB ID 380 để test chuẩn)
            Item capsunItem = null;
            for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
            {
                var item = Char.myCharz().arrItemBag[i];
                if (item != null && (item.template.id == 221))
                {
                    capsunItem = item;
                    break;
                }
            }

            // 2. Tìm nhân vật mục tiêu
            var character = GameScr.findCharInMap(161244479);
            if (character == null)
            {
                if (CurrentTradeStateGD != TradeStateGD.Idle)
                {
                    Logger.Log("Bot: Khong tim thay doi phuong, reset ve Idle.");
                    CurrentTradeStateGD = TradeStateGD.Idle;
                }
                return;
            }

            long now = mSystem.currentTimeMillis();

            // 3. Máy trạng thái (State Machine)
            switch (CurrentTradeStateGD)
            {
                case TradeStateGD.Idle:
                    // Nếu bảng chưa mở, bắt đầu mời
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        if (now - lastTimeInvite > 5000)
                        {
                            Logger.Log("Bot [Idle -> Inviting]: Gui loi moi giao dich toi " + character.cName);
                            Service.gI().giaodich(0, character.charID, -1, -1);
                            lastTimeInvite = now;
                            CurrentTradeStateGD = TradeStateGD.Inviting;
                            lastTradeStateTime = now;
                        }
                    }
                    else
                    {
                        // Nếu bảng đã mở từ trước, nhảy thẳng vào bước bỏ đồ
                        Logger.Log("Bot [Idle -> AddingItem]: Da mo bang tu truoc.");
                        CurrentTradeStateGD = TradeStateGD.AddingItem;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now; // <--- Sửa lỗi spam: Reset thời gian ở đây
                        isSentItemInState = false;
                    }
                    break;

                case TradeStateGD.Inviting:
                    if (GameCanvas.panel.isShow && GameCanvas.panel.type == 13)
                    {
                        Logger.Log("Bot [Inviting -> AddingItem]: Bang giao dich da mo.");
                        CurrentTradeStateGD = TradeStateGD.AddingItem;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now; // <--- SỬA LỖI: Cập nhật đúng thời điểm mở bảng
                        isSentItemInState = false; // Reset cờ gửi đồ cho phiên mới
                    }
                    else if (now - lastTradeStateTime > 15000)
                    {
                        Logger.Log("Bot [Inviting -> Idle]: Qua thoi gian cho doi phuong dong y, reset.");
                        CurrentTradeStateGD = TradeStateGD.Idle;
                    }
                    break;

                case TradeStateGD.AddingItem:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentTradeStateGD = TradeStateGD.Idle;
                        return;
                    }

                    if (GameCanvas.panel.vMyGD.size() > 0)
                    {
                        Logger.Log("Bot [AddingItem -> Locking]: Da thay do tren bang.");
                        CurrentTradeStateGD = TradeStateGD.Locking;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = 0;
                    }
                    else if (!isSentItemInState && now - lastTradeStateTime > 1500) // Đợi 1.5s cho bảng ổn định
                    {
                        if (capsunItem != null)
                        {
                            int qty = capsunItem.quantity;
                            Logger.Log($"Bot: Dang bo vat pham ID={capsunItem.template.id}, Index={capsunItem.indexUI}, Qty={qty}");

                            // === BẮT BUỘC: Thêm vào danh sách local TRƯỚC (giống logic game Panel.cs:8805-8811) ===
                            capsunItem.isSelect = true;
                            Item tradeItem = new Item();
                            tradeItem.template = capsunItem.template;
                            tradeItem.quantity = qty;
                            tradeItem.indexUI = capsunItem.indexUI;
                            tradeItem.itemOption = capsunItem.itemOption;
                            GameCanvas.panel.vMyGD.addElement(tradeItem);

                            // Rồi mới gửi packet lên server
                            Service.gI().giaodich(2, -1, (sbyte)tradeItem.indexUI, tradeItem.quantity);
                            isSentItemInState = true;
                            lastTimeTradeAction = now;
                        }
                        else
                        {
                            Logger.Log("Bot: Khong tim thay vat pham trong tui de bo vao!");
                            CurrentTradeStateGD = TradeStateGD.Idle;
                        }
                    }
                    else if (now - lastTimeTradeAction > 15000)
                    {
                        Logger.Log("Bot [AddingItem]: Qua lau khong thay do hien len, reset de thu lai.");
                        CurrentTradeStateGD = TradeStateGD.Idle;
                    }
                    break;

                case TradeStateGD.Locking:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentTradeStateGD = TradeStateGD.Idle;
                        return;
                    }

                    if (now - lastTimeTradeAction > 2000)
                    {
                        Logger.Log("Bot: Dang nhan XAC NHAN (Khoa do) va chuyen sang Accepting...");
                        GameCanvas.panel.isLock = true;
                        Service.gI().giaodich(5, -1, -1, -1);
                        // Chuyển thẳng sang Accepting, không đợi frame sau
                        CurrentTradeStateGD = TradeStateGD.Accepting;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now;
                    }
                    break;

                case TradeStateGD.Accepting:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentTradeStateGD = TradeStateGD.Idle;
                        return;
                    }

                    if (GameCanvas.panel.isLock && GameCanvas.panel.isFriendLock && !GameCanvas.panel.isAccept)
                    {
                        if (now - lastTimeTradeAction > 1500)
                        {
                            Logger.Log("Bot: Dang nhan DONG Y GIAO DICH (Ket thuc)...");
                            GameCanvas.panel.isAccept = true;
                            GameCanvas.endDlg();
                            Service.gI().giaodich(7, -1, -1, -1);
                            GameCanvas.panel.hide();

                            // === QUAN TRỌNG: Reset tất cả timer để bot không mời lại ngay ===
                            lastTimeTradeAction = now;
                            lastTimeInvite = now; // Chờ 5 giây mới được mời lại
                            lastTradeStateTime = now;
                            isSentItemInState = false;
                            CurrentTradeStateGD = TradeStateGD.Idle;
                            Logger.Log("Bot: GIAO DICH HOAN TAT! Cho 5 giay truoc khi bat dau phien moi.");
                        }
                    }
                    break;

            }
        }
    }
}
