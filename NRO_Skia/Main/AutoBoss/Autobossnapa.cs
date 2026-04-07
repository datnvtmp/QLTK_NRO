using System;

namespace Mod
{
    public static class AutoBossNapa
    {
        // ── Constants ─────────────────────────────────────────────────────────
        private static readonly int[] MAP_LIST = { 68, 69, 70, 71, 72, 64, 65, 63, 66, 67, 73, 74, 75, 76, 77 };

        private static readonly string[] BOSS_NAMES = { "Mập", "đầu đinh", "Kuku", "Rambo" };

        private const int MAX_ZONE = 14;
        private const int START_ZONE = 2;
        private const int ITEM_ID_THIEN_SU = 1070;
        private const long ZONE_CHANGE_DELAY = 6500L;
        private const long ATTACK_COOLDOWN = 440L;
        private const long TELEPORT_COOLDOWN = 4000L;
        private const long BOSS_TIMEOUT = 60000L;
        private const int TELEPORT_DISTANCE = 30;

        private const int STATE_GO_TO_MAP = 0;
        private const int STATE_WAIT_ZONE = 1;
        private const int STATE_ATTACK_BOSS = 2;
        private const int STATE_NEXT_ZONE = 3;

        // ── State ─────────────────────────────────────────────────────────────
        public static bool IsRunning { get; private set; }

        private static int _currentState;
        private static int _currentMapIndex;
        private static int _currentZone = START_ZONE;
        private static long _zoneChangeTimer;
        private static long _bossAttackStartTime;
        private static long _lastAttackTime;
        private static long _lastTeleportTime;
        private static int _initialItemCount;
        private static int _targetQuantity;
        private static bool _farmReverse;

        // =====================================================================
        // PUBLIC API
        // =====================================================================

        public static void Start(int targetQuantity, bool farmReverse)
        {
            _targetQuantity = targetQuantity;
            _farmReverse = farmReverse;
            _initialItemCount = GetItemQuantity();

            if (_farmReverse) ReverseMapList();

            IsRunning = true;
            _currentState = STATE_GO_TO_MAP;
            _currentMapIndex = 0;
            _currentZone = START_ZONE;
            _lastAttackTime = 0L;
            _lastTeleportTime = 0L;

            Lib.TurnOnTDLT();

            Console.WriteLine("Bắt đầu farm boss Nappa");
            Console.WriteLine($"Mục tiêu : {_targetQuantity} mảnh");
            Console.WriteLine($"Hiện có  : {_initialItemCount} mảnh");
            Console.WriteLine($"Farm     : {(_farmReverse ? "ngược" : "thuận")}");

            GameClient.Send($"QUANTITY:{_initialItemCount}");
        }

        public static void Stop()
        {
            IsRunning = false;
            MainXmap.isXmaping = false;
            ResetState();
            Console.WriteLine("Đã dừng farm boss Nappa");
        }

        public static void Update()
        {
            if (MainXmap.isXmaping || !IsRunning) return;

            try
            {
                if (Char.myCharz().meDead)
                {
                    ResetState();
                    Service.gI().returnTownFromDead();
                    return;
                }

                switch (_currentState)
                {
                    case STATE_GO_TO_MAP: HandleGoToMap(); break;
                    case STATE_WAIT_ZONE: HandleWaitZone(); break;
                    case STATE_ATTACK_BOSS: HandleAttackBoss(); break;
                    case STATE_NEXT_ZONE: HandleNextZone(); break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"AutoBossNapa Error: {e.Message}");
            }
        }

        // =====================================================================
        // STATE HANDLERS
        // =====================================================================

        private static void HandleGoToMap()
        {
            int targetMapId = MAP_LIST[_currentMapIndex];

            if (TileMap.mapID == targetMapId)
            {
                Service.gI().requestChangeZone(_currentZone, -1);
                _zoneChangeTimer = Now() + ZONE_CHANGE_DELAY;
                _currentState = STATE_WAIT_ZONE;
                Console.WriteLine($"Đã đến map {targetMapId}, chuyển khu {_currentZone}");
            }
            else
            {
                Console.WriteLine($"Đang đi đến map {targetMapId}");
                MainXmap.StartGoToMap(targetMapId);
            }
        }

        private static void HandleWaitZone()
        {
            if (Now() < _zoneChangeTimer) return;

            if (TileMap.zoneID == _currentZone)
            {
                _currentState = STATE_ATTACK_BOSS;
                _bossAttackStartTime = Now();
                Console.WriteLine($"Đã vào khu {_currentZone}, bắt đầu tìm boss");
            }
            else
            {
                Service.gI().requestChangeZone(_currentZone, -1);
                _zoneChangeTimer = Now() + ZONE_CHANGE_DELAY;
                Console.WriteLine($"Chờ vào khu {_currentZone}");
            }
        }

        private static void HandleAttackBoss()
        {
            var boss = FindBossNappa();

            if (boss != null)
            {
                AttackBoss(boss);
                _bossAttackStartTime = Now();
                return;
            }

            // Không thấy boss
            if (Now() - _bossAttackStartTime > BOSS_TIMEOUT)
            {
                Console.WriteLine("Timeout 60s không thấy boss, next khu");
                _currentState = STATE_NEXT_ZONE;
                return;
            }

            if (GameCanvas.gameTick % 180 != 0) return;

            int earned = GetItemQuantity() - _initialItemCount;
            AutoManager.SendState($"Mảnh găng: {earned}/{_targetQuantity}");

            if (earned >= _targetQuantity)
            {
                Console.WriteLine($"Đã đủ {_targetQuantity} mảnh, dừng farm");
                Stop();
                GameClient.Send("KILL:");
                return;
            }

            _currentState = STATE_NEXT_ZONE;
        }

        private static void HandleNextZone()
        {
            _currentZone++;

            if (_currentZone > MAX_ZONE)
            {
                _currentZone = START_ZONE;
                _currentMapIndex++;

                if (_currentMapIndex >= MAP_LIST.Length)
                {
                    _currentMapIndex = 0;
                    Console.WriteLine("Đã hết map, quay lại từ đầu");
                }

                _currentState = STATE_GO_TO_MAP;
                Console.WriteLine($"Chuyển sang map {MAP_LIST[_currentMapIndex]}");
            }
            else
            {
                Service.gI().requestChangeZone(_currentZone, -1);
                _zoneChangeTimer = Now() + ZONE_CHANGE_DELAY;
                _currentState = STATE_WAIT_ZONE;
                Console.WriteLine($"Chuyển khu {_currentZone}");
            }
        }

        // =====================================================================
        // ATTACK
        // =====================================================================

        private static void AttackBoss(Char boss)
        {
            var me = Char.myCharz();
            me.charFocus = boss;
            me.mobFocus = null;
            me.npcFocus = null;

            int dx = boss.cx - me.cx;
            int dy = boss.cy - me.cy;
            int distance = (int)Math.Sqrt(dx * dx + dy * dy);

            if (distance > TELEPORT_DISTANCE)
            {
                long now = Now();
                if (now - _lastTeleportTime > TELEPORT_COOLDOWN)
                {
                    MainXmap.TeleportTo(boss.cx, boss.cy);
                    _lastTeleportTime = now;
                    Console.WriteLine($"Teleport đến boss (khoảng cách: {distance})");
                }
            }

            long nowMs = Now();
            if (nowMs - _lastAttackTime < ATTACK_COOLDOWN) return;

            var targets = new MyVector();
            targets.addElement(boss);
            Service.gI().sendPlayerAttack(new MyVector(), targets, -1);
            _lastAttackTime = nowMs;
        }

        // =====================================================================
        // HELPERS
        // =====================================================================

        private static Char FindBossNappa()
        {
            var map = GameScr.vCharInMap;
            if (map == null) return null;

            for (int i = 0; i < map.size(); i++)
            {
                var c = (Char)map.elementAt(i);
                if (c?.cName != null && c.cHP > 0 && IsBossNappa(c.cName))
                    return c;
            }
            return null;
        }

        private static bool IsBossNappa(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var bossName in BOSS_NAMES)
                if (name.IndexOf(bossName, StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static int GetItemQuantity()
        {
            try
            {
                foreach (var item in Char.myCharz().arrItemBag)
                    if (item?.template.id == ITEM_ID_THIEN_SU) return item.quantity;
                return 0;
            }
            catch { return 0; }
        }

        private static void ResetState()
        {
            _currentState = STATE_GO_TO_MAP;
            _currentZone = START_ZONE;
        }

        private static void ReverseMapList()
        {
            Array.Reverse(MAP_LIST);
        }

        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}