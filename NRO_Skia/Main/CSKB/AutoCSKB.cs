using System;
using Mod;
using NRO_Skia.Main.Core;

namespace NRO_Skia.Main.CSKB
{
    internal class AutoCSKB
    {
        // ═══════════════════════════════════════════
        // CONFIG & STATE
        // ═══════════════════════════════════════════
        public static bool IsRunning { get; private set; }
        public static CSKBConfig Config { get; set; } = new CSKBConfig();

        public static bool IsCSKB { get; set; }
        public static int CSKBMap { get; set; }
        public static int CSKBZone { get; set; } = -1;
        public static int CSKBType { get; set; } // 0: Up, 1: Chứa
        public static int CharID { get; set; }

        public const int TypeUp = 0;
        public const int TypeContain = 1;
        private const int MAX_STORE_RETRY = 5;

        // ═══════════════════════════════════════════
        // STATE MACHINE
        // ═══════════════════════════════════════════
        public enum TradeState
        {
            Idle, Inviting, AddingItem, Locking, Accepting,
            GoHome, WaitHome, OpeningNpc, SelectingChest, Storing
        }

        public static TradeState CurrentState = TradeState.Idle;

        // Timers
        private static long _lastInvite, _lastAction, _lastStateTime, _lastFullNotifyTime;
        private static long _now;

        // Flags
        private static bool _configApplied, _idSent;
        private static bool _initialized, _sentItem, _npcMenuOpened;
        private static int _storeRetryCount;

        // Chỉ dùng trong AddingItem (Bot Up)
        private static Item _capsunItem;

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════
        private static bool IsTradeOpen => GameCanvas.panel.isShow && GameCanvas.panel.type == 13;

        private static bool IsPostTradeState =>
            CurrentState >= TradeState.GoHome && CurrentState <= TradeState.Storing;

        private static Item FindCSKBInBag(int minQty = 1)
        {
            foreach (var item in Char.myCharz().arrItemBag)
                if (item != null && item.template.id == GameConstants.ID_ITEM_CSKB && item.quantity >= minQty)
                    return item;
            return null;
        }

        private static bool HasCSKBInBox()
        {
            foreach (var item in Char.myCharz().arrItemBox)
                if (item != null && item.template.id == GameConstants.ID_ITEM_CSKB)
                    return true;
            return false;
        }

        private static void GoTo(TradeState state)
        {
            CurrentState = state;
            _lastStateTime = _now;
        }

        private static bool SelectMenuItem(string keyword)
        {
            if (GameCanvas.menu.menuItems == null) return false;
            for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
            {
                try
                {
                    var cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                    if (cmd?.caption != null && cmd.caption.ToLower().Contains(keyword))
                    {
                        GameCanvas.menu.menuSelectedItem = i;
                        GameCanvas.menu.performSelect();
                        GameCanvas.menu.doCloseMenu();
                        Logger.Log($"Bot [Receiver]: Da chon '{cmd.caption}'.");
                        GameClient.SendNow(MsgType.DATA_INGAME, $"Bot [Receiver]: Da chon '{cmd.caption}'.");
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        // ═══════════════════════════════════════════
        // INITIALIZE & CONFIG
        // ═══════════════════════════════════════════
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            GameClient.OnReceived += (type, payload) =>
            {
                if (type == MsgType.CSKB_RECEIVER_ID && int.TryParse(payload, out int id))
                {
                    CharID = id;
                    Logger.Log($"Bot [Up]: Nhan duoc ID acc nhan tu QLTK: {CharID}");
                    GameClient.SendNow(MsgType.DATA_INGAME, $"Bot [Up]: Da nhan ID {CharID} tu QLTK.");
                }
            };
        }

        public static void ResetApplied()
        {
            _configApplied = false;
            _idSent = false;
        }

        public static void ApplyConfig(CSKBConfig config)
        {
            IsCSKB = config.IsCSKB;
            CSKBMap = config.CSKBMap;
            CSKBZone = config.CSKBZone;
            CSKBType = config.CSKBType;
            GameScr.info1.addInfo("Auto CSKB: " + (IsCSKB ? "ON" : "OFF"));
        }

        // ═══════════════════════════════════════════
        // UPDATE (entry point)
        // ═══════════════════════════════════════════
        public static void Update()
        {
            Initialize();
            if (!_configApplied && Config != null) { ApplyConfig(Config); _configApplied = true; }
            if (!IsCSKB) return;

            IsRunning = false;
            if (MainXmap.isXmaping && !IsPostTradeState) return;

            _now = mSystem.currentTimeMillis();
            _capsunItem = null;

            // 1. Kiểm tra điều kiện theo Type
            if (CSKBType == TypeUp)
            {
                _capsunItem = FindCSKBInBag(99);
                IsRunning = _capsunItem != null;

                if (IsRunning && _now - _lastFullNotifyTime > 10000)
                {
                    GameClient.SendNow(MsgType.CSKB_FULL, "");
                    _lastFullNotifyTime = _now;
                    Logger.Log("Bot [Up]: Da bao QLTK bat acc nhan (CSKB_FULL)");
                    GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Up]: Da bao QLTK bat acc nhan (CSKB_FULL)");
                }
                if (!IsRunning) return;
            }
            else
            {
                IsRunning = true;
                if (!_idSent && Char.myCharz()?.charID != 0)
                {
                    GameClient.SendNow(MsgType.CSKB_RECEIVER_ID, Char.myCharz().charID.ToString());
                    _idSent = true;
                    Logger.Log("Bot [Receiver]: Da gui ID ve QLTK: " + Char.myCharz().charID);
                    GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Da gui ID " + Char.myCharz().charID + " ve QLTK.");
                }

                // Nếu inventory đã có CSKB (chưa cất từ lần trước) → đi cất trước, ko nhận trade
                if (CurrentState == TradeState.Idle && FindCSKBInBag() != null)
                {
                    CurrentState = TradeState.GoHome;
                    _storeRetryCount = 0;
                    _npcMenuOpened = false;
                    Logger.Log("Bot [Receiver]: Inventory da co CSKB! Di cat truoc khi nhan trade moi...");
                    GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Inventory đã có CSKB! Đi cất trước khi nhận trade mới...");
                }
            }

            // 2. Di chuyển đến map giao dịch (chỉ khi chưa post-trade)
            if (!IsPostTradeState)
            {
                if (TileMap.mapID != CSKBMap)
                {
                    if (_now - _lastAction < 3000) return; // Tránh spam
                    MainXmap.StartGoToMap(CSKBMap);
                    _lastAction = _now;
                    return;
                }
                if (CSKBZone != -1 && TileMap.zoneID != CSKBZone)
                {
                    if (_now - MainXmap.LAST_TIME_FINISH_XMAP < 3000) return;
                    if (_now - _lastAction < 5000) return; // Tránh spam đổi khu
                    Service.gI().requestChangeZone(CSKBZone, -1);
                    _lastAction = _now;
                    return;
                }
            }

            // 3. State Machine
            switch (CurrentState)
            {
                case TradeState.Idle:       HandleIdle(); break;
                case TradeState.Inviting:   HandleInviting(); break;
                case TradeState.AddingItem: HandleAddingItem(); break;
                case TradeState.Locking:    HandleLocking(); break;
                case TradeState.Accepting:  HandleAccepting(); break;

                // Post-trade (Bot Nhận)
                case TradeState.GoHome:         HandleGoHome(); break;
                case TradeState.WaitHome:       HandleWaitHome(); break;
                case TradeState.OpeningNpc:     HandleOpeningNpc(); break;
                case TradeState.SelectingChest: HandleSelectingChest(); break;
                case TradeState.Storing:        HandleStoring(); break;
            }
        }

        // ═══════════════════════════════════════════
        // TRADE STATES
        // ═══════════════════════════════════════════
        private static void HandleIdle()
        {
            if (!IsTradeOpen)
            {
                if (CSKBType == TypeUp)
                {
                    var target = GameScr.findCharInMap(CharID);
                    if (target != null && _now - _lastInvite > 5000)
                    {
                        Service.gI().giaodich(0, target.charID, -1, -1);
                        _lastInvite = _now;
                        GoTo(TradeState.Inviting);
                    }
                }
                else
                {
                    var popup = GameScr.gI().popUpYesNo;
                    if (popup?.cmdYes != null && popup.cmdYes.idAction == 11114)
                    {
                        Char inviter = (Char)popup.cmdYes.p;
                        if (inviter != null)
                        {
                            Service.gI().giaodich(1, inviter.charID, -1, -1);
                            GameScr.gI().popUpYesNo = null;
                            GoTo(TradeState.Inviting);
                            Logger.Log("Bot [Receiver]: Da chap nhan loi moi tu " + inviter.cName);
                            GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Da chap nhan loi moi tu " + inviter.cName);
                        }
                    }
                }
            }
            else
            {
                GoTo(TradeState.AddingItem);
                _lastAction = _now;
                _sentItem = false;
            }
        }

        private static void HandleInviting()
        {
            if (IsTradeOpen)
            {
                GoTo(TradeState.AddingItem);
                _lastAction = _now;
                _sentItem = false;
            }
            else if (_now - _lastStateTime > 15000)
                CurrentState = TradeState.Idle;
        }

        private static void HandleAddingItem()
        {
            if (!IsTradeOpen) { CurrentState = TradeState.Idle; return; }

            if (CSKBType == TypeUp)
            {
                if (GameCanvas.panel.vMyGD.size() > 0)
                {
                    GoTo(TradeState.Locking);
                    _lastAction = 0;
                }
                else if (!_sentItem && _now - _lastStateTime > 1500 && _capsunItem != null)
                {
                    _capsunItem.isSelect = true;
                    var tradeItem = new Item
                    {
                        template = _capsunItem.template,
                        quantity = Math.Min(_capsunItem.quantity, 99),
                        indexUI = _capsunItem.indexUI,
                        itemOption = _capsunItem.itemOption
                    };
                    GameCanvas.panel.vMyGD.addElement(tradeItem);
                    Service.gI().giaodich(2, -1, (sbyte)tradeItem.indexUI, tradeItem.quantity);
                    _sentItem = true;
                    _lastAction = _now;
                }
            }
            else
            {
                if (GameCanvas.panel.vFriendGD.size() > 0)
                {
                    GoTo(TradeState.Locking);
                    _lastAction = _now;
                }
            }

            if (_now - _lastStateTime > 20000) CurrentState = TradeState.Idle;
        }

        private static void HandleLocking()
        {
            if (!IsTradeOpen) { CurrentState = TradeState.Idle; return; }

            if (_now - _lastAction > 2000)
            {
                GameCanvas.panel.isLock = true;
                Service.gI().giaodich(5, -1, -1, -1);
                GoTo(TradeState.Accepting);
                _lastAction = _now;
            }
        }

        private static void HandleAccepting()
        {
            if (!IsTradeOpen) { CurrentState = TradeState.Idle; return; }

            if (GameCanvas.panel.isLock && GameCanvas.panel.isFriendLock
                && !GameCanvas.panel.isAccept && _now - _lastAction > 1500)
            {
                GameCanvas.panel.isAccept = true;
                GameCanvas.endDlg();
                Service.gI().giaodich(7, -1, -1, -1);
                GameCanvas.panel.hide();

                _lastAction = _lastInvite = _now;
                _sentItem = false;
                Logger.Log("Bot: GIAO DICH HOAN TAT!");


                if (CSKBType == TypeContain)
                {
                    GoTo(TradeState.GoHome);
                    _storeRetryCount = 0;
                    _npcMenuOpened = false;
                    Logger.Log("Bot [Receiver]: Giao dich xong, chuan bi ve nha cat do...");
                    GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Giao dịch xong, chuẩn bị về nhà cất đồ...");
                }
                else
                    CurrentState = TradeState.Idle;
            }
        }

        // ═══════════════════════════════════════════
        // POST-TRADE STATES (Bot Nhận)
        // ═══════════════════════════════════════════
        private static void HandleGoHome()
        {
            int homeMap = Char.myCharz().cgender + 21;
            if (TileMap.mapID != homeMap)
            {
                if (!MainXmap.isXmaping && _now - _lastAction > 3000) // Tránh spam
                {
                    MainXmap.StartGoToMap(homeMap);
                    _lastAction = _now;
                    Logger.Log($"Bot [Receiver]: Dang ve nha (map {homeMap})...");
                    GameClient.SendNow(MsgType.DATA_INGAME, $"Bot [Receiver]: Đang về nhà (map {homeMap})...");
                }
                return;
            }
            Logger.Log("Bot [Receiver]: Da ve nha. Cho 1.5s...");
            GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Đã về nhà. Chờ 1.5s...");
            GoTo(TradeState.WaitHome);
        }

        private static void HandleWaitHome()
        {
            if (_now - _lastStateTime > 1500)
            {
                Logger.Log("Bot [Receiver]: Het delay. Mo NPC 3...");
                GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Hết delay. Mở NPC 3...");
                GoTo(TradeState.OpeningNpc);
                _npcMenuOpened = false;
            }
        }

        private static void HandleOpeningNpc()
        {
            // Rương đã mở (panel type 2) → chuyển thẳng sang cất đồ
            if (GameCanvas.panel.isShow && GameCanvas.panel.type == 2)
            {
                GoTo(TradeState.Storing);
                _lastAction = _now;
                Logger.Log("Bot [Receiver]: Ruong do da mo. Bat dau cat do...");
                GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Rương đã mở. Bắt đầu cất đồ...");
                return;
            }

            if (!_npcMenuOpened)
            {
                Service.gI().openMenu(3);
                _npcMenuOpened = true;
                _lastStateTime = _now;
                Logger.Log("Bot [Receiver]: Da gui lenh mo NPC 3 (ruong do).");
                GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Đã gửi lệnh mở NPC 3 (rương đồ).");
            }
            else if (_now - _lastStateTime > 5000)
            {
                _npcMenuOpened = false;
                _lastStateTime = _now;
                Logger.Log("Bot [Receiver]: Timeout mo ruong do, thu lai...");
                GameClient.SendNow(MsgType.DATA_INGAME, "Bot [Receiver]: Timeout mở rương đồ, thử lại...");
            }
        }

        private static void HandleSelectingChest()
        {
            if (GameCanvas.menu.showMenu)
            {
                if (_now - _lastStateTime > 1000)
                {
                    if (SelectMenuItem("rương"))
                    {
                        GoTo(TradeState.Storing);
                        _lastAction = _now;
                    }
                    else
                    {
                        Logger.Log("Bot [Receiver]: Khong tim thay 'Ruong do' trong menu!");
                        GameCanvas.menu.doCloseMenu();
                        GoTo(TradeState.OpeningNpc);
                        _npcMenuOpened = false;
                    }
                }
            }
            else if (_now - _lastStateTime > 5000)
            {
                GoTo(TradeState.OpeningNpc);
                _npcMenuOpened = false;
            }
        }

        private static void HandleStoring()
        {
            if (_now - _lastAction < 2000) return;

            Item cskb = FindCSKBInBag();
            bool boxHas = HasCSKBInBox();

            // TH1: Hành trang không có CSKB (không có từ đầu hoặc đã cất xong do chuyển vào rương) → DONE
            if (cskb == null)
            {
                Logger.Log("Bot [Receiver]: Da cat xong CSKB vao ruong (khong con trong tui). Thoat game...");
                GameClient.SendNow(MsgType.CSKB_RECEIVER_DONE, "");
                GameMidlet.instance.exit();
            }
            // TH2 & TH4: Hành trang còn CSKB mà Rương đã có sẵn CSKB (rương chỉ chứa đc 1 ô CSKB) → FULL
            else if (boxHas)
            {
                Logger.Log("Bot [Receiver]: Ruong da co CSKB (nhung tui van con)! Bao QLTK full va thoat...");
                GameScr.info1.addInfo("Rương đã có CSKB! Thoát...");
                GameClient.SendNow(MsgType.CSKB_RECEIVER_FULL, "");
                GameMidlet.instance.exit();
            }
            // TH3: Hành trang còn CSKB, Rương chưa có → Thử cất vào rương
            else if (_storeRetryCount >= MAX_STORE_RETRY)
            {
                Logger.Log($"Bot [Receiver]: Ruong day! Thu {MAX_STORE_RETRY} lan khong duoc. Bao QLTK va thoat...");
                GameScr.info1.addInfo("Rương đầy (hết chỗ)! Thoát...");
                GameClient.SendNow(MsgType.CSKB_RECEIVER_FULL, "");
                GameMidlet.instance.exit();
            }
            else
            {
                _storeRetryCount++;
                Service.gI().getItem(1, (sbyte)cskb.indexUI);
                _lastAction = _now;
                Logger.Log($"Bot [Receiver]: Cat CSKB vao ruong (lan {_storeRetryCount}/{MAX_STORE_RETRY})...");
            }
        }
    }
}
