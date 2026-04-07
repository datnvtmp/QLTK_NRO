using System;

public static class MainXmap
{
    // ═══════════════════════════════════════════
    // PUBLIC STATE
    // ═══════════════════════════════════════════
    public static bool isXmaping;
    public static int IdMapEnd;
    public static bool isEatChicken = true;
    public static float customMapDelay = 2.5f;
    public static bool teleDirect;
    public static long LAST_TIME_FINISH_XMAP;
    public static long TIME_TICK_XMAP = 50;
    public static bool xmapErrr;

    // ── Private state ─────────────────────────
    private static bool _isUseCapsule = true;
    private static bool _isOpeningPanel;
    private static long _lastTimeOpenedPanel;
    private static long _lastWaitTime;
    private static long _lastErrorTime;
    private static long _lastItemUseTime;
    private static bool _isUsingItem;
    private static bool _findNpc29to27;
    private static long _lastMapChangeTime;
    private static int _lastProcessedMap = -1;
    private static bool _isProcessingMapChange;

    // Capsule state machine
    private enum CapsuleState { Idle, Used, SelectingMap, WaitingMapChange }
    private static CapsuleState _capsuleState = CapsuleState.Idle;
    private static int _capsuleRetryCount;
    private static int _capsuleSelectedMapID;
    private static long _capsuleSelectTime;
    private const int MAX_CAPSULE_RETRY = 3;
    private const int MAP_CHANGE_TIMEOUT_MS = 4000;

    private static readonly XmapPathfinder _pathfinder = XmapPathfinder.GetInstance();

    // ═══════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════
    public static void Update()
    {
        if (!isXmaping) return;

        GameClient.Send(MsgType.DATA_INGAME, $"[ Di Chuyển ] {TileMap.mapID} => {IdMapEnd}");
        ModPro.UpdateConfirmNpc();

        if (TileMap.mapID == IdMapEnd) { FinishXmap(); return; }

        if (TryEatChicken() || !ShouldUpdate()) return;

        // Track map change — bắt đầu đếm delay NGAY khi map đổi
        if (TileMap.mapID != _lastProcessedMap)
        {
            _lastProcessedMap = TileMap.mapID;
            _lastMapChangeTime = mSystem.currentTimeMillis();
            _isProcessingMapChange = true;  // chờ delay trước khi đi tiếp
        }

        if (_isProcessingMapChange)
        {
            if (mSystem.currentTimeMillis() - _lastMapChangeTime < (long)(customMapDelay * 1000)) return;
            _isProcessingMapChange = false; // delay xong mới clear
        }

        if (!HandleFutureSpecialCase())
            UpdateXmap(IdMapEnd);
    }

    // ═══════════════════════════════════════════
    // EAT CHICKEN
    // ═══════════════════════════════════════════
    public static bool TryEatChicken()
    {
        if (!isEatChicken) return false;

        int map = TileMap.mapID;
        if (map != 21 && map != 22 && map != 23) return false;

        for (int i = 0; i < GameScr.vItemMap.size(); i++)
        {
            var item = (ItemMap)GameScr.vItemMap.elementAt(i);
            if ((item.itemMapID == Char.myCharz().charID || item.itemMapID == -1)
                && item.template.id == 74)
            {
                Char.myCharz().itemFocus = item;
                if (mSystem.currentTimeMillis() - _lastWaitTime > 600)
                {
                    _lastWaitTime = mSystem.currentTimeMillis();
                    Service.gI().pickItem(Char.myCharz().itemFocus.itemMapID);
                }
                return true;
            }
        }
        return false;
    }

    // ═══════════════════════════════════════════
    // SHOULD UPDATE
    // ═══════════════════════════════════════════
    private static bool ShouldUpdate()
    {
        if (!isXmaping) return false;
        if (mSystem.currentTimeMillis() - _lastWaitTime <= 300) return false;
        if (Char.ischangingMap || Controller.isStopReadMessage) return false;
        return GameCanvas.gameTick % TIME_TICK_XMAP == 0;
    }

    // ═══════════════════════════════════════════
    // FUTURE MAP SPECIAL CASE
    // ═══════════════════════════════════════════
    private static bool HandleFutureSpecialCase()
    {
        if (!DataXmap.IsFutureMap(IdMapEnd)) return false;
        if (Char.myCharz().taskMaint.taskId <= 24) { xmapErrr = true; return true; }
        if (GameScr.findNPCInMap(38) != null) { _findNpc29to27 = false; return false; }

        switch (TileMap.mapID)
        {
            case 27: UpdateXmap(28); _findNpc29to27 = false; return true;
            case 28: UpdateXmap(_findNpc29to27 ? 27 : 29); return true;
            case 29: _findNpc29to27 = true; UpdateXmap(28); return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════
    // UPDATE XMAP — tìm đường + di chuyển
    // ═══════════════════════════════════════════
    public static void UpdateXmap(int targetMapID)
    {
        // Map 999: portal theo gender, update mỗi tick để luôn đúng
        DataXmap.linkMaps[999] = new System.Collections.Generic.List<NextMap>
        {
            new NextMap(24 + Char.myCharz().cgender, 10, "OK")
        };

        if (!CheckSpecialMapRequirements()) return;

        var path = _pathfinder.FindPath(targetMapID, TileMap.mapID,
            Char.myCharz().cPower,
            Char.myCharz().taskMaint.taskId > 30);

        if (path == null) { HandlePathNotFound(targetMapID); return; }
        if (path.Length < 2) return; // Đã ở đích

        if (TryUseCapsule(path)) return;
        if (CheckClanRequirement(path)) return;
        if (_isUsingItem && mSystem.currentTimeMillis() - _lastItemUseTime < 500) return;

        if (_isUsingItem && TileMap.mapID == 160) _isUsingItem = false;

        GotoNextMap(path[1]);
    }

    // ─── Kiểm tra yêu cầu đặc biệt trước khi tìm đường ───────────────────
    private static bool CheckSpecialMapRequirements()
    {
        // Map 160: cần nhẫn thời không (item 992)
        if (IdMapEnd == 160 && !_isUsingItem)
        {
            if (!Lib.ExistItemBag(992)) { xmapErrr = true; return false; }
            _isUsingItem = true;
            _lastItemUseTime = mSystem.currentTimeMillis();
            Lib.UseItem(992);
            return false;
        }

        // Map 181: cần bình hút năng lượng (item 1852)
        if (IdMapEnd == 181 && !Lib.ExistItemBag(1852)) { xmapErrr = true; return false; }

        return true;
    }

    // ═══════════════════════════════════════════
    // PATH NOT FOUND
    // ═══════════════════════════════════════════
    private static void HandlePathNotFound(int targetID)
    {
        long now = mSystem.currentTimeMillis();
        if (now - _lastErrorTime < 1000) return;
        _lastErrorTime = now;

        string msg = _pathfinder.GetPathErrorMessage(targetID, TileMap.mapID,
            Char.myCharz().cPower,
            Char.myCharz().taskMaint.taskId > 30);
        GameScr.info1.addInfo(msg);
        xmapErrr = true;
    }

    // ═══════════════════════════════════════════
    // CAPSULE
    // Retry tối đa MAX_CAPSULE_RETRY lần khi server throttle ("từ từ đừng vội").
    // Phát hiện: sau requestMapSelect, đợi MAP_CHANGE_TIMEOUT_MS.
    // Nếu map không đổi → server từ chối → thử lại từ Idle.
    // ═══════════════════════════════════════════
    private static bool TryUseCapsule(int[] path)
    {
        if (!_isUseCapsule) return false;

        switch (_capsuleState)
        {
            case CapsuleState.Idle:
                if (path.Length <= 4) return false;
                if (!TryActivateCapsule())
                {
                    _isUseCapsule = false; // Không có capsule trong túi
                    return false;
                }
                return true;

            case CapsuleState.Used:
                {
                    long elapsed = mSystem.currentTimeMillis() - _lastTimeOpenedPanel;
                    if (elapsed < 500) return true;
                    if (GameCanvas.panel.mapNames == null)
                    {
                        if (elapsed > 2000)
                        {
                            // Panel không mở → retry nếu còn lượt
                            return RetryOrGiveUp();
                        }
                        return true;
                    }
                    _capsuleState = CapsuleState.SelectingMap;
                    return true;
                }

            case CapsuleState.SelectingMap:
                return TrySelectMapFromPanel(path);

            case CapsuleState.WaitingMapChange:
                {
                    // Map đổi thành công
                    if (TileMap.mapID != _capsuleSelectedMapID)
                    {
                        _isUseCapsule = false;
                        _capsuleState = CapsuleState.Idle;
                        return false; // Để UpdateXmap chạy bình thường từ map mới
                    }
                    // Server throttle — chưa đổi map sau timeout
                    long waited = mSystem.currentTimeMillis() - _capsuleSelectTime;
                    if (waited > MAP_CHANGE_TIMEOUT_MS)
                    {
                        GameScr.info1.addInfo($"[Capsule] Server chậm, thử lại... ({_capsuleRetryCount + 1}/{MAX_CAPSULE_RETRY})");
                        return RetryOrGiveUp();
                    }
                    return true; // Vẫn đang chờ
                }

            default:
                return false;
        }
    }

    // Thử lại: reset về Idle để dùng capsule lần nữa, hoặc give up nếu hết lượt
    private static bool RetryOrGiveUp()
    {
        _capsuleState = CapsuleState.Idle;
        if (++_capsuleRetryCount >= MAX_CAPSULE_RETRY)
        {
            GameScr.info1.addInfo("[Capsule] Hết lượt thử, chạy bộ.");
            _isUseCapsule = false;
            return false;
        }
        // Đợi 1 chút trước khi thử lại tránh spam
        _lastTimeOpenedPanel = mSystem.currentTimeMillis();
        return false; // Tick này chạy bộ, tick sau thử capsule lại
    }

    private static bool TryActivateCapsule()
    {
        foreach (var item in Char.myCharz().arrItemBag)
        {
            if (item == null) continue;
            if (item.template.id == 194 || item.template.id == 193)
            {
                _capsuleState = CapsuleState.Used;
                _lastTimeOpenedPanel = mSystem.currentTimeMillis();
                GameCanvas.panel.mapNames = null;
                Service.gI().useItem(0, 1, -1, (short)item.template.id);
                return true;
            }
        }
        return false;
    }

    private static bool TrySelectMapFromPanel(int[] path)
    {
        var mapNames = GameCanvas.panel.mapNames;
        if (mapNames == null)
        {
            // Panel đóng bất ngờ → retry
            return RetryOrGiveUp();
        }

        for (int i = path.Length - 1; i >= 1; i--)
        {
            string target = TileMap.mapNames[path[i]];
            for (int j = 0; j < mapNames.Length; j++)
            {
                if (mapNames[j].Contains(target))
                {
                    // Ghi nhận map đang chờ đổi, chuyển sang state chờ xác nhận
                    _capsuleSelectedMapID = TileMap.mapID; // map hiện tại, chờ nó ĐỔI
                    _capsuleSelectTime = mSystem.currentTimeMillis();
                    _capsuleState = CapsuleState.WaitingMapChange;
                    Service.gI().requestMapSelect(j);
                    return true;
                }
            }
        }

        // Map đích không có trong panel capsule → give up
        GameScr.info1.addInfo("[Capsule] Map đích không hỗ trợ capsule.");
        _isUseCapsule = false;
        _capsuleState = CapsuleState.Idle;
        return false;
    }
    // ═══════════════════════════════════════════
    // CLAN CHECK
    // ═══════════════════════════════════════════
    private static bool CheckClanRequirement(int[] path)
    {
        if (path == null || path.Length == 0) return true;
        if (TileMap.mapID != path[0] || Char.ischangingMap || Controller.isStopReadMessage) return true;
        if (Char.myCharz().clan != null) return false;

        if (DataXmap.IsKhiGasMap(IdMapEnd) ||
            DataXmap.IsManhVoBT(IdMapEnd) ||
            DataXmap.IsClanMap(IdMapEnd))
        {
            xmapErrr = true;
            return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════
    // GOTO NEXT MAP
    // ═══════════════════════════════════════════
    private static void GotoNextMap(int nextMapID)
    {
        var nextMap = _pathfinder.FindNextMapToGo(TileMap.mapID, nextMapID);
        nextMap?.GotoMap();
    }

    // ═══════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════
    public static void StartGoToMap(int mapID)
    {
        isXmaping = true;
        IdMapEnd = mapID;
        _lastProcessedMap = -1;
        _isProcessingMapChange = false;
        _capsuleState = CapsuleState.Idle;
        _isUseCapsule = true;
        _capsuleRetryCount = 0;
        _capsuleSelectedMapID = -1;
    }

    public static void FinishXmap()
    {
        isXmaping = false;
        _capsuleState = CapsuleState.Idle;
        _isOpeningPanel = false;
        xmapErrr = false;
        _lastProcessedMap = -1;
        _isProcessingMapChange = false;
        LAST_TIME_FINISH_XMAP = Lib.TimeNow();
    }

    // ═══════════════════════════════════════════
    // MAP NAVIGATION HELPERS
    // ═══════════════════════════════════════════
    public static void LoadMapLeft() => LoadMap(0);
    public static void LoadMapCenter() => LoadMap(2);
    public static void LoadMapRight() => LoadMap(1);

    private static void LoadMap(int position)
    {
        if (DataXmap.IsNRDMap(TileMap.mapID)) { TeleportInNRDMap(position); return; }

        var (left, center, right) = LoadWaypoints();
        int[] wp = position switch { 0 => left, 1 => right, _ => center };

        int tx = wp[0] != 0 ? wp[0] : position switch
        {
            0 => 60,
            1 => TileMap.pxw - 60,
            _ => TileMap.pxw / 2
        };
        int ty = wp[1] != 0 ? wp[1] : GetYGround(tx);

        TeleportTo(tx, ty);
        Service.gI().charMove();

        if (TileMap.mapID != 7 && TileMap.mapID != 14 && TileMap.mapID != 0)
            Service.gI().requestChangeMap();
        else
            Service.gI().getMapOffline();

        Char.ischangingMap = true;
    }

    private static (int[] left, int[] center, int[] right) LoadWaypoints()
    {
        var left = new int[2]; var center = new int[2]; var right = new int[2];
        int count = TileMap.vGo.size();

        for (int i = 0; i < count; i++)
        {
            var wp = (Waypoint)TileMap.vGo.elementAt(i);
            if (wp.maxX < 60) { left[0] = wp.minX + 15; left[1] = wp.maxY; }
            else if (wp.minX > TileMap.pxw - 60) { right[0] = wp.maxX - 15; right[1] = wp.maxY; }
            else { center[0] = wp.minX + 15; center[1] = wp.maxY; }
        }

        // Nếu chỉ có 2 waypoint, phân biệt trái/phải theo maxX
        if (count == 2)
        {
            var wp1 = (Waypoint)TileMap.vGo.elementAt(0);
            var wp2 = (Waypoint)TileMap.vGo.elementAt(1);
            bool w1IsLeft = wp1.maxX < wp2.maxX;
            var wL = w1IsLeft ? wp1 : wp2;
            var wR = w1IsLeft ? wp2 : wp1;
            left = new[] { wL.minX + 15, wL.maxY };
            right = new[] { wR.maxX - 15, wR.maxY };
        }

        return (left, center, right);
    }

    private static void TeleportInNRDMap(int position)
    {
        switch (position)
        {
            case 0: TeleportTo(60, GetYGround(60)); break;
            case 1: TeleportTo(TileMap.pxw - 60, GetYGround(TileMap.pxw - 60)); break;
            case 2:
                for (int i = 0; i < GameScr.vNpc.size(); i++)
                {
                    var npc = (Npc)GameScr.vNpc.elementAt(i);
                    if (npc.template.npcTemplateId >= 30 && npc.template.npcTemplateId <= 36)
                    {
                        Char.myCharz().npcFocus = npc;
                        TeleportTo(npc.cx, npc.cy - 3);
                        break;
                    }
                }
                break;
        }
    }

    public static int GetYGround(int x)
    {
        int y = 50;
        for (int i = 0; i < 30; i++)
        {
            y += 24;
            if (TileMap.tileTypeAt(x, y, 2))
                return y - y % 24;
        }
        return y;
    }

    // Single source of truth cho TeleportTo — NextMap.TeleportTo đã bị xóa, gọi về đây
    public static void TeleportTo(int x, int y)
    {
        Char.myCharz().cx = x;
        Char.myCharz().cy = y;
        Service.gI().charMove();
        if (!GameScr.canAutoPlay)
        {
            Char.myCharz().cy = y + 1; Service.gI().charMove();
            Char.myCharz().cy = y; Service.gI().charMove();
        }
    }
}