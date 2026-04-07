using System;
using System.Collections.Generic;
using System.Text;
using Mod;
using NRO_Skia.Main.Core;
using NRO_Skia.Main.CSKB;

namespace NRO_Skia.Main.CSKB
{
    internal class AutoCSKB
    {
        public static bool IsRunning { get; private set; }
        public static CSKBConfig Config { get; set; } = new CSKBConfig();

        private static bool _configApplied = false;
        private static bool _isFullNotified = false; // Bot Up: Đã báo QLTK là đầy 99 cái
        private static bool _idSent = false;         // Bot Nhận: Đã gửi ID cho QLTK

        public static void ResetApplied()
        {
            _configApplied = false;
            _isFullNotified = false;
            _idSent = false;
        }

        public static bool IsCSKB { get; set; }
        public static int CSKBMap { get; set; }
        public static int CSKBZone { get; set; } = -1;
        public static int CSKBType { get; set; } // 0: Up CSKB, 1: Chứa CSKB

        public static int CharID { get; set; }

        public const int TypeUp = 0;
        public const int TypeContain = 1;

        // ═══════════════════════════════════════════
        // STATE MACHINE
        // ═══════════════════════════════════════════
        public enum TradeState
        {
            Idle,
            Inviting,
            AddingItem,
            Locking,
            Accepting,
            AfterTrade, // Mới: Xử lý sau giao dịch (Về nhà)
            Storing     // Mới: Đang cất đồ vào rương
        }

        public static TradeState CurrentState = TradeState.Idle;
        private static long lastTimeInvite;
        private static long lastTimeTradeAction;
        private static long lastTradeStateTime;
        private static bool isSentItemInState;
        private static long lastTimeRequestChest;

        // ═══════════════════════════════════════════
        // INITIALIZE
        // ═══════════════════════════════════════════
        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Đăng ký nhận ID từ QLTK (QLTK forward từ bot nhận sang bot up)
            GameClient.OnReceived += (type, payload) =>
            {
                if (type == MsgType.CSKB_RECEIVER_ID)
                {
                    if (int.TryParse(payload, out int id))
                    {
                        CharID = id;
                        Logger.Log($"Bot [Up]: Nhan duoc ID acc nhan tu QLTK: {CharID}");
                    }
                }
            };
        }

        public static void ApplyConfig(CSKBConfig config)
        {
            IsCSKB = config.IsCSKB;
            CSKBMap = config.CSKBMap;
            CSKBZone = config.CSKBZone;
            CSKBType = config.CSKBType;

            GameScr.info1.addInfo("Auto CSKB: " + (IsCSKB ? "ON" : "OFF"));
            Logger.Log("[AutoCSKB] Config applied");
        }

        public static void Update()
        {
            Initialize();

            if (!_configApplied && Config != null)
            {
                ApplyConfig(Config);
                _configApplied = true;
            }

            if (!IsCSKB) return;
            IsRunning = false;

            if (MainXmap.isXmaping && CurrentState != TradeState.AfterTrade) return;

            long now = mSystem.currentTimeMillis();

            // ═══════════════════════════════════════════
            // 1. Kiểm tra điều kiện riêng theo Type
            // ═══════════════════════════════════════════
            Item capsunItem = null;
            if (CSKBType == TypeUp)
            {
                // Bot Up: Phải có 99 Capsule mới chạy
                for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                {
                    var item = Char.myCharz().arrItemBag[i];
                    if (item != null && item.template.id == GameConstants.ID_ITEM_CSKB && item.quantity >= 98)
                    {
                        capsunItem = item;
                        IsRunning = true;
                        break;
                    }
                }

                // Nếu đầy 99 cái -> Báo QLTK để bật acc nhận
                if (IsRunning && !_isFullNotified)
                {
                    GameClient.SendNow(MsgType.CSKB_FULL, "");
                    _isFullNotified = true;
                    Logger.Log("Bot [Up]: Da bao QLTK bat acc nhan (CSKB_FULL)");
                }

                if (!IsRunning)
                {
                    _isFullNotified = false; // Reset nếu lỡ tay dùng mất
                    return;
                }
            }
            else
            {
                // Bot Nhận: Luôn chạy nếu bật Auto. Gửi ID cho QLTK khi vào game.
                IsRunning = true;

                if (!_idSent && Char.myCharz() != null && Char.myCharz().charID != 0)
                {
                    GameClient.SendNow(MsgType.CSKB_RECEIVER_ID, Char.myCharz().charID.ToString());
                    _idSent = true;
                    Logger.Log("Bot [Receiver]: Da gui ID ve QLTK: " + Char.myCharz().charID);
                }
            }

            // ═══════════════════════════════════════════
            // 2. Di chuyển đến Map và Zone chỉ định (Cơ bản)
            // ═══════════════════════════════════════════
            if (CurrentState != TradeState.AfterTrade && CurrentState != TradeState.Storing)
            {
                if (TileMap.mapID != CSKBMap)
                {
                    MainXmap.StartGoToMap(CSKBMap);
                    return;
                }
                if (CSKBZone != -1 && TileMap.zoneID != CSKBZone)
                {
                    if (now - MainXmap.LAST_TIME_FINISH_XMAP < 3000) return;
                    Service.gI().requestChangeZone(CSKBZone, -1);
                    lastTimeTradeAction = now;
                    return;
                }
            }

            // ═══════════════════════════════════════════
            // 3. Tìm nhân vật mục tiêu
            // ═══════════════════════════════════════════
            var character = GameScr.findCharInMap(CharID);

            // ═══════════════════════════════════════════
            // 4. Máy trạng thái giao dịch (State Machine)
            // ═══════════════════════════════════════════
            switch (CurrentState)
            {
                case TradeState.Idle:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        if (CSKBType == TypeUp)
                        {
                            if (character != null && now - lastTimeInvite > 5000)
                            {
                                Service.gI().giaodich(0, character.charID, -1, -1);
                                lastTimeInvite = now;
                                CurrentState = TradeState.Inviting;
                                lastTradeStateTime = now;
                            }
                        }
                        else
                        {
                            if (GameScr.gI().popUpYesNo != null && GameScr.gI().popUpYesNo.cmdYes != null)
                            {
                                if (GameScr.gI().popUpYesNo.cmdYes.idAction == 11114)
                                {
                                    Char inviter = (Char)GameScr.gI().popUpYesNo.cmdYes.p;
                                    if (inviter != null)
                                    {
                                        Service.gI().giaodich(1, inviter.charID, -1, -1);
                                        GameScr.gI().popUpYesNo = null;
                                        CurrentState = TradeState.Inviting;
                                        lastTradeStateTime = now;
                                        Logger.Log("Bot [Receiver]: Da chap nhan loi moi tu " + inviter.cName);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        CurrentState = TradeState.AddingItem;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now;
                        isSentItemInState = false;
                    }
                    break;

                case TradeState.Inviting:
                    if (GameCanvas.panel.isShow && GameCanvas.panel.type == 13)
                    {
                        CurrentState = TradeState.AddingItem;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now;
                        isSentItemInState = false;
                    }
                    else if (now - lastTradeStateTime > 15000)
                    {
                        CurrentState = TradeState.Idle;
                    }
                    break;

                case TradeState.AddingItem:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentState = TradeState.Idle;
                        return;
                    }

                    if (CSKBType == TypeUp)
                    {
                        if (GameCanvas.panel.vMyGD.size() > 0)
                        {
                            CurrentState = TradeState.Locking;
                            lastTradeStateTime = now;
                            lastTimeTradeAction = 0;
                        }
                        else if (!isSentItemInState && now - lastTradeStateTime > 1500)
                        {
                            if (capsunItem != null)
                            {
                                int qty = capsunItem.quantity;
                                capsunItem.isSelect = true;
                                Item tradeItem = new Item();
                                tradeItem.template = capsunItem.template;
                                tradeItem.quantity = qty;
                                tradeItem.indexUI = capsunItem.indexUI;
                                tradeItem.itemOption = capsunItem.itemOption;
                                GameCanvas.panel.vMyGD.addElement(tradeItem);

                                Service.gI().giaodich(2, -1, (sbyte)tradeItem.indexUI, tradeItem.quantity);
                                isSentItemInState = true;
                                lastTimeTradeAction = now;
                            }
                        }
                    }
                    else
                    {
                        if (GameCanvas.panel.vFriendGD.size() > 0)
                        {
                            CurrentState = TradeState.Locking;
                            lastTradeStateTime = now;
                            lastTimeTradeAction = now;
                        }
                    }

                    if (now - lastTradeStateTime > 20000) CurrentState = TradeState.Idle;
                    break;

                case TradeState.Locking:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentState = TradeState.Idle;
                        return;
                    }

                    if (now - lastTimeTradeAction > 2000)
                    {
                        GameCanvas.panel.isLock = true;
                        Service.gI().giaodich(5, -1, -1, -1);
                        CurrentState = TradeState.Accepting;
                        lastTradeStateTime = now;
                        lastTimeTradeAction = now;
                    }
                    break;

                case TradeState.Accepting:
                    if (!GameCanvas.panel.isShow || GameCanvas.panel.type != 13)
                    {
                        CurrentState = TradeState.Idle;
                        return;
                    }

                    if (GameCanvas.panel.isLock && GameCanvas.panel.isFriendLock && !GameCanvas.panel.isAccept)
                    {
                        if (now - lastTimeTradeAction > 1500)
                        {
                            GameCanvas.panel.isAccept = true;
                            GameCanvas.endDlg();
                            Service.gI().giaodich(7, -1, -1, -1);
                            GameCanvas.panel.hide();

                            lastTimeTradeAction = now;
                            lastTimeInvite = now;
                            lastTradeStateTime = now;
                            isSentItemInState = false;

                            Logger.Log("Bot: GIAO DICH HOAN TAT!");

                            if (CSKBType == TypeContain)
                            {
                                CurrentState = TradeState.AfterTrade;
                                Logger.Log("Bot [Receiver]: Giao dich xong, chuan bi ve nha cat do...");
                            }
                            else
                            {
                                CurrentState = TradeState.Idle;
                            }
                        }
                    }
                    break;

                // ─── AFTER TRADE: Kiểm tra rương ───────────────
                case TradeState.AfterTrade:

                    // Đã có dữ liệu rương, tìm CSKB
                    bool hasCSKBInBox = false;
                    foreach (var item in Char.myCharz().arrItemBox)
                    {
                        if (item != null && item.template.id == GameConstants.ID_ITEM_CSKB)
                        {
                            hasCSKBInBox = true;
                            break;
                        }
                    }

                    if (hasCSKBInBox)
                    {
                        Logger.Log("Bot [Receiver]: Ruong da co CSKB. Bao QLTK full va thoat...");
                        GameClient.SendNow(MsgType.CSKB_RECEIVER_FULL, "");
                        GameMidlet.instance.exit();
                    }
                    else
                    {
                        Logger.Log("Bot [Receiver]: Ruong chua co CSKB. Bat dau cat do...");
                        CurrentState = TradeState.Storing;
                        lastTimeTradeAction = now;
                    }
                    break;

                // ─── STORING: Cất Capsule vào rương ───────────────────────
                case TradeState.Storing:
                    if(TileMap.mapID != Char.myCharz().cgender + 21)
                    {
                        MainXmap.StartGoToMap(Char.myCharz().cgender + 21);
                        return;
                    }

                    Item cskbInBag = null;
                    for (int i = 0; i < Char.myCharz().arrItemBag.Length; i++)
                    {
                        var item = Char.myCharz().arrItemBag[i];
                        if (item != null && item.template.id == GameConstants.ID_ITEM_CSKB)
                        {
                            cskbInBag = item;
                            break;
                        }
                    }

                    if (cskbInBag != null)
                    {
                        if (now - lastTimeTradeAction > 2000)
                        {
                            Service.gI().useItem(1, 0, (sbyte)cskbInBag.indexUI, -1); // Type 1: Cat vao ruong, From 0: Bag
                            lastTimeTradeAction = now;
                            Logger.Log("Bot [Receiver]: Dang cat CSKB vao ruong...");
                        }
                    }
                    else
                    {
                        // Đã cất xong (không còn trong hành trang)
                        Logger.Log("Bot [Receiver]: Da cat xong. Thoat game...");
                        GameClient.SendNow(MsgType.CSKB_RECEIVER_DONE, "");
                        GameMidlet.instance.exit();
                    }
                    break;
            }
        }
    }
}
