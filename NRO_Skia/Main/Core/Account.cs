public static class Account
{
    // ═══════════════════════════════════════════
    // STATES
    // ═══════════════════════════════════════════
    private static class State
    {
        public const int IDLE = 0;
        public const int SELECTING_SERVER = 1;
        public const int CONNECTING = 2;
        public const int WAITING = 3;
        public const int LOGGING_IN = 4;
        public const int IN_GAME = 5;
        public const int DISCONNECTED = 6;
        public const int RETRY_DELAY = 7;

        public static string GetName(int s) => s switch
        {
            IDLE => "IDLE",
            SELECTING_SERVER => "SELECTING_SERVER",
            CONNECTING => "CONNECTING",
            WAITING => "WAITING",
            LOGGING_IN => "LOGGING_IN",
            IN_GAME => "IN_GAME",
            DISCONNECTED => "DISCONNECTED",
            RETRY_DELAY => "RETRY_DELAY",
            _ => "UNKNOWN"
        };
    }

    // ═══════════════════════════════════════════
    // CONFIG
    // ═══════════════════════════════════════════
    private const long CONNECT_TIMEOUT = 15_000;          // 15s chờ kết nối server
    private const long UI_WAIT_TIME = 1_000;          // 1s chờ UI sẵn sàng
    private const long LOGIN_TIMEOUT = 45_000;          // 45s chờ vào game sau login
    private const long LOGIN_COOLDOWN = 35_000;          // 35s giữa 2 lần login
    private const long RETRY_DELAY_TIME = 30_000;          // 30s chờ trước khi reconnect
    private const long REPORT_INTERVAL = 5_000;          // 5s log 1 lần khi chờ retry
    private const long CONNECTION_CHECK_INTERVAL = 3_000;          // 3s check kết nối 1 lần
    private const long POST_MAP_BUFFER = 3_000;          // 3s buffer sau đổi map
    private const long STUCK_TIMEOUT = 10 * 60 * 1_000L;// 10 phút kẹt → exit

    // ═══════════════════════════════════════════
    // ACCOUNT INFO
    // ═══════════════════════════════════════════
    public static string Id { get; private set; } = "";
    public static string Acc { get; private set; } = "";
    public static string Pass { get; private set; } = "";
    public static string ServerName { get; private set; } = "";
    public static string Proxy { get; private set; } = "";
    public static int Server { get; private set; } = 0;
    public static bool HasAccount => !string.IsNullOrEmpty(Acc);

    // ═══════════════════════════════════════════
    // INTERNAL STATE
    // ═══════════════════════════════════════════
    private static int _state = State.IDLE;
    private static long _stateStartTime = 0;

    private static bool _serverSelected = false;
    private static int _totalRetry = 0;
    private static long _lastLoginTime = 0;
    private static long _lastConnCheck = 0;
    private static long _lastReportTime = 0;

    // Đổi map — tránh false disconnect khi load map
    private static bool _wasLoadingMap = false;
    private static long _mapLoadDoneAt = 0;

    // Thời gian server hiện "xin chờ Xs" — trừ ra khi tính LOGIN_TIMEOUT
    private static long _waitScreenTime = 0;

    // Stuck guard — bao lâu không ở trong màn hình game
    private static long _notInGameSince = 0;

    // ═══════════════════════════════════════════
    // LOAD
    // ═══════════════════════════════════════════
    public static void Load(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--id": Id = args[i + 1]; break;
                case "--user": Acc = args[i + 1]; break;
                case "--pass": Pass = args[i + 1]; break;
                case "--server": ServerName = args[i + 1]; break;
                case "--proxy": Proxy = args[i + 1]; break;
            }
        }
        //Logger.Log($"[Account] Id={Id} Acc={Acc} Server={ServerName}");
    }

    // ═══════════════════════════════════════════
    // UPDATE — gọi mỗi frame
    // ═══════════════════════════════════════════
    public static void Update()
    {
        if (!HasAccount) return;

        long now = TimeNow();

        CheckStuckGuard(now);

        if (_state == State.IN_GAME)
            CheckConnectionInGame(now);

        switch (_state)
        {
            case State.IDLE: HandleIdle(); break;
            case State.SELECTING_SERVER: HandleSelectServer(); break;
            case State.CONNECTING: HandleConnecting(); break;
            case State.WAITING: HandleWaiting(); break;
            case State.LOGGING_IN: HandleLoggingIn(); break;
            case State.DISCONNECTED: HandleDisconnected(); break;
            case State.RETRY_DELAY: HandleRetryDelay(now); break;
        }
    }

    // ═══════════════════════════════════════════
    // STUCK GUARD
    // Nếu kẹt ở màn hình login/server quá 10 phút → tự exit
    // IsInGame() = true  → GameScr, NPC, InputScr, PopUp... (đang chơi)
    // IsInGame() = false → login, chọn server, splash (chưa vào game)
    // ═══════════════════════════════════════════
    private static void CheckStuckGuard(long now)
    {
        if (IsInGame())
        {
            _notInGameSince = 0;
            return;
        }

        if (_notInGameSince == 0)
            _notInGameSince = now;

        if (now - _notInGameSince >= STUCK_TIMEOUT)
        {
            //Logger.Log("[Account] Kẹt 10 phút không vào game → exit");
            GameClient.Send(MsgType.DATA_INGAME, "Kẹt 10 phút, tự tắt...");
            Environment.Exit(0);
        }
    }

    // ═══════════════════════════════════════════
    // CHECK KẾT NỐI KHI IN_GAME
    // ═══════════════════════════════════════════
    private static void CheckConnectionInGame(long now)
    {
        if (now - _lastConnCheck < CONNECTION_CHECK_INTERVAL) return;
        long elapsed = now - _lastConnCheck;
        _lastConnCheck = now;

        // ── Đang load map → bỏ qua, tránh false disconnect ──────
        bool loadingMap = Char.isLoadingMap;
        if (_wasLoadingMap && !loadingMap)
        {
            _mapLoadDoneAt = now;
            //Logger.Log("[Account] Load map xong → buffer 3s");
        }
        _wasLoadingMap = loadingMap;

        if (loadingMap) { _mapLoadDoneAt = 0; return; }
        if (_mapLoadDoneAt > 0)
        {
            if (now - _mapLoadDoneAt < POST_MAP_BUFFER) return;
            _mapLoadDoneAt = 0;
        }

        // ── Server hiện "xin chờ Xs" → không đếm timeout ────────
        if (GameCanvas.isWait())
        {
            _waitScreenTime += elapsed;
            return;
        }

        // ── Đang trong game (GameScr, NPC, InputScr...) → OK ─────
        if (IsInGame()) return;

        // ── Mất kết nối thật ─────────────────────────────────────
        if (!Session_ME.gI().isConnected())
        {
            //Logger.Log("[Account] Mất kết nối → DISCONNECTED");
            ChangeState(State.DISCONNECTED);
            return;
        }

        // ── Kẹt ở SelectChar hoặc màn hình trung gian sau login ──
        // realElapsed = thời gian thực (không tính giây server bắt chờ)
        long realElapsed = Elapsed() - _waitScreenTime;
        if (realElapsed > LOGIN_TIMEOUT)
        {
            //Logger.Log($"[Account] Timeout {LOGIN_TIMEOUT / 1000}s → DISCONNECTED");
            ChangeState(State.DISCONNECTED);
        }
    }

    // ═══════════════════════════════════════════
    // STATE HANDLERS
    // ═══════════════════════════════════════════
    private static void HandleIdle()
    {
        // Phải đang đứng ở màn hình ServerListScreen
        if (GameCanvas.serverScreen == null) return;
        if (GameCanvas.currentScreen != GameCanvas.serverScreen) return;

        // Màn hình chưa load xong (đang tải dữ liệu)
        if (!ServerListScreen.loadScreen) return;

        // Dữ liệu chưa sẵn sàng
        if (!ServerListScreen.bigOk) return;

        ResolveServer();
        _totalRetry++;
        //Logger.Log($"[Account] Auto-login lần {_totalRetry}, server='{ServerName}' idx={Server}");
        GameClient.Send(MsgType.DATA_INGAME, $"Auto login lần {_totalRetry}...");
        ChangeState(State.SELECTING_SERVER);
    }

    private static void HandleSelectServer()
    {
        try
        {
            if (!IsValidServer(Server)) { ChangeState(State.RETRY_DELAY); return; }

            if (!_serverSelected)
            {
                if (ServerListScreen.ipSelect != Server)
                {
                    ServerListScreen.ipSelect = Server;
                    GameCanvas.serverScreen?.selectServer();
                    GameClient.Send(MsgType.DATA_INGAME, "Đang chọn server...");
                }
                _serverSelected = true;
                return;
            }

            ChangeState(State.CONNECTING);
        }
        catch { ChangeState(State.RETRY_DELAY); }
    }

    private static void HandleConnecting()
    {
        if (Session_ME.gI().isConnected())
        {
            GameClient.Send(MsgType.DATA_INGAME, "Đã kết nối server");
            ChangeState(State.WAITING);
            return;
        }

        if (IsTimeout(CONNECT_TIMEOUT))
        {
            GameClient.Send(MsgType.DATA_INGAME, "Timeout kết nối, thử lại...");
            try { Session_ME.gI().close(); } catch { }
            ChangeState(State.RETRY_DELAY);
        }
    }

    private static void HandleWaiting()
    {
        if (IsUIReady() || Elapsed() >= UI_WAIT_TIME)
            ChangeState(State.LOGGING_IN);
    }

    private static void HandleLoggingIn()
    {
        if (GameCanvas.loginScr == null)
            GameCanvas.loginScr = new LoginScr();

        if (string.IsNullOrEmpty(Acc)) return;

        long cooldownLeft = LOGIN_COOLDOWN - (TimeNow() - _lastLoginTime);
        if (_lastLoginTime > 0 && cooldownLeft > 0)
        {
            GameClient.Send(MsgType.DATA_INGAME, $"Cooldown login: {cooldownLeft / 1000}s");
            return;
        }

        try
        {
            if (!Session_ME.gI().isConnected()) { GameCanvas.connect(); return; }

            GameClient.Send(MsgType.DATA_INGAME, "Đang login...");
            GameCanvas.serverScreen.Login_New();

            _lastLoginTime = TimeNow();
            _lastConnCheck = TimeNow();
            _waitScreenTime = 0;
            ChangeState(State.IN_GAME);
        }
        catch { ChangeState(State.RETRY_DELAY); }
    }

    private static void HandleDisconnected()
    {
        GameClient.Send(MsgType.DATA_INGAME, "Mất kết nối, chuẩn bị reconnect...");
        try { Session_ME.gI().close(); } catch { }
        try { Session_ME2.gI().close(); } catch { }
        try { GameCanvas.serverScreen?.switchToMe(); } catch { }
        ChangeState(State.RETRY_DELAY);
    }

    private static void HandleRetryDelay(long now)
    {
        long remaining = RETRY_DELAY_TIME - Elapsed();

        if (remaining <= 0)
        {
            GameClient.Send(MsgType.DATA_INGAME, "Reconnect ngay...");
            ChangeState(State.IDLE);
            return;
        }

        if (now - _lastReportTime >= REPORT_INTERVAL)
        {
            _lastReportTime = now;
            GameClient.Send(MsgType.DATA_INGAME, $"Reconnect sau: {remaining / 1000}s");
            GameClient.Send(MsgType.RETRY_INFO, $"{_totalRetry}|{remaining / 1000}");
        }
    }

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════
    public static void SwitchAccount(string newAcc, string newPass, string newServer = null)
    {
        Acc = newAcc;
        Pass = newPass;

        bool sameServer = string.IsNullOrEmpty(newServer) || newServer == ServerName;
        if (!string.IsNullOrEmpty(newServer)) ServerName = newServer;

        _lastLoginTime = 0;

        if (Session_ME.gI().isConnected() && sameServer)
        {
            //Logger.Log("[Account] SwitchAccount: dùng kết nối cũ");
            ChangeState(State.LOGGING_IN);
        }
        else
        {
            ChangeState(State.DISCONNECTED);
        }
    }

    public static void ResolveServer()
    {
        if (string.IsNullOrEmpty(ServerName) || ServerListScreen.nameServer == null) return;
        for (int i = 0; i < ServerListScreen.nameServer.Length; i++)
        {
            if (ServerListScreen.nameServer[i] == ServerName) { Server = i; return; }
        }
        Server = 0;
    }

    // ═══════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════

    // Đang trong game = mọi màn hình KHÔNG phải login/server/splash
    // Bao gồm: GameScr, NPC, InputScr, PopUp, Shop...
    private static bool IsInGame()
    {
        var s = GameCanvas.currentScreen;
        if (s == null) return false;
        if (s == GameCanvas.serverScreen) return false;
        if (s == GameCanvas.loginScr) return false;
        if (s == GameCanvas.serverScr) return false;
        if (s == GameCanvas._SelectCharScr) return false;
        if (s is SplashScr) return false;
        return true;
    }

    private static bool IsValidServer(int idx) =>
        idx >= 0
        && ServerListScreen.nameServer != null
        && idx < ServerListScreen.nameServer.Length;

    private static bool IsUIReady() =>
        GameCanvas.serverScreen != null && GameCanvas.loginScr != null;

    private static void ChangeState(int newState)
    {
        if (_state == newState) return;
        //Logger.Log($"[Account] {State.GetName(_state)} → {State.GetName(newState)}");
        _state = newState;
        _stateStartTime = TimeNow();

        if (newState == State.IDLE)
            _serverSelected = false;

        if (newState != State.IN_GAME)
        {
            _wasLoadingMap = false;
            _mapLoadDoneAt = 0;
            _waitScreenTime = 0;
        }
    }

    private static long Elapsed() => TimeNow() - _stateStartTime;
    private static bool IsTimeout(long ms) => Elapsed() > ms;
    private static long TimeNow() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}