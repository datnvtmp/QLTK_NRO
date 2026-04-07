public static class AutoBuyBua
{
    // ═══════════════════════════════════════════
    // STATES
    // ═══════════════════════════════════════════
    private enum Step
    {
        GoToMap, OpenMenu, WaitMenu1, ClickMenu1, WaitMenu2, ClickMenu2, BuyItem
    }

    // ═══════════════════════════════════════════
    // CONFIG
    // ═══════════════════════════════════════════
    public static int npcId = 21;
    public static int mapId = 44;
    public static string menuLevel1 = "Cửa hàng Bùa";
    public static string menuLevel2 = "Bùa Dùng 8 giờ";

    // ═══════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════
    private const long TIMEOUT = 3000L;
    private const long ACTION_DELAY = 1000L;

    // ═══════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════
    private static bool _isRunning;
    private static Step _step;
    private static long _lastActionTime;

    // ═══════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════
    public static bool IsRunning => _isRunning;

    public static void Start()
    {
        if (_isRunning) return;
        _isRunning = true;
        _step = Step.GoToMap;
        _lastActionTime = 0;
        GameScr.info1.addInfo("Bat dau mua bua 8h!");
    }

    public static void Stop()
    {
        _isRunning = false;
        _step = Step.GoToMap;
        GameCanvas.menu.doCloseMenu();
        GameScr.info1.addInfo("Da dung mua bua!");
    }

    // ═══════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════
    public static void Update()
    {
        if (!_isRunning) return;
        long now = Lib.TimeNow();

        switch (_step)
        {
            case Step.GoToMap: HandleGoToMap(now); break;
            case Step.OpenMenu: HandleOpenMenu(now); break;
            case Step.WaitMenu1: HandleWaitMenu1(now); break;
            case Step.ClickMenu1: HandleClickMenu1(now); break;
            case Step.WaitMenu2: HandleWaitMenu2(now); break;
            case Step.ClickMenu2: HandleClickMenu2(now); break;
            case Step.BuyItem: HandleBuyItem(now); break;
        }
    }

    // ═══════════════════════════════════════════
    // HANDLERS
    // ═══════════════════════════════════════════
    private static void HandleGoToMap(long now)
    {
        if (TileMap.mapID != mapId)
        {
            if (!MainXmap.isXmaping)
            {
                MainXmap.StartGoToMap(mapId);
                GameScr.info1.addInfo("Dang di den map mua bua...");
            }
            return;
        }
        NextStep(Step.OpenMenu, now);
        GameScr.info1.addInfo("Da den map, mo menu...");
    }

    private static void HandleOpenMenu(long now)
    {
        if (!Elapsed(now)) return;
        Service.gI().openMenu(npcId);
        NextStep(Step.WaitMenu1, now);
    }

    private static void HandleWaitMenu1(long now)
    {
        if (GameCanvas.menu.showMenu && GameCanvas.menu.menuItems != null)
        {
            NextStep(Step.ClickMenu1, now);
            return;
        }
        if (IsTimeout(now))
        {
            GameScr.info1.addInfo("Timeout menu 1, thu lai...");
            NextStep(Step.OpenMenu, now);
        }
    }

    private static void HandleClickMenu1(long now)
    {
        int idx = Lib.FindMenuByName(menuLevel1);
        if (idx == -1)
        {
            GameScr.info1.addInfo("Khong tim thay menu '" + menuLevel1 + "'!");
            LogAllMenus();
            Stop();
            return;
        }
        Service.gI().confirmMenu((short)npcId, (sbyte)idx);
        NextStep(Step.WaitMenu2, now);
        GameScr.info1.addInfo("Da click '" + menuLevel1 + "'");
    }

    private static void HandleWaitMenu2(long now)
    {
        if (!Elapsed(now)) return;
        NextStep(Step.ClickMenu2, now);
    }

    private static void HandleClickMenu2(long now)
    {
        Service.gI().confirmMenu((short)npcId, 1);
        NextStep(Step.BuyItem, now);
        GameScr.info1.addInfo("Da click '" + menuLevel2 + "'");
    }

    private static void HandleBuyItem(long now)
    {
        if (!Elapsed(now)) return;
        Service.gI().buyItem(1, 218, 0);
        GameScr.info1.addInfo("Da mua bua!");
        _isRunning = false;
    }

    // ═══════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════
    private static void NextStep(Step step, long now)
    {
        _step = step;
        _lastActionTime = now;
    }

    private static bool Elapsed(long now) => now - _lastActionTime >= ACTION_DELAY;
    private static bool IsTimeout(long now) => now - _lastActionTime > TIMEOUT;

    private static void LogAllMenus()
    {
        if (GameCanvas.menu.menuItems == null) { GameScr.info1.addInfo("Menu null!"); return; }
        for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
        {
            var cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
            if (cmd?.caption != null)
                GameScr.info1.addInfo(i + ": " + cmd.caption);
        }
    }
}