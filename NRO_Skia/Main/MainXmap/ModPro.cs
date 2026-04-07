using System;

public static class ModPro
{
    // ═══════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════
    public static short idNpcService;
    public static string nameIndex1 = "";
    public static string nameIndex2 = "";
    public static string nameIndex3 = "";
    public static string s1SubName = "";
    public static string s2SubName = "";
    public static string s3SubName = "";
    public static string s1SubName2 = "";
    public static string s2SubName2 = "";
    public static string s3SubName2 = "";
    public static bool confirming;
    public static long delayConfirm;
    public static bool runningOpenNpc;
    public static int currentStep;

    private static long _confirmStartTime;
    private static long _lastStepTime;
    private static bool _nextInfoSuKien;
    private static string _infoSuKien = "";

    private const int CONFIRM_TIMEOUT = 5000;
    private const int STEP_DELAY = 1500;

    // ═══════════════════════════════════════════
    // START
    // ═══════════════════════════════════════════
    public static void StartConfirmNpc(short npcId)
        => StartConfirmNpc(npcId, "", "", "", "", "", "", "", "", "", true, "Từ chối nhận kẹo");

    public static void StartConfirmNpc(short npcId,
        string s1, string s2, string s3,
        string s1Sub, string s2Sub, string s3Sub,
        string s1Sub2, string s2Sub2, string s3Sub2)
        => StartConfirmNpc(npcId, s1, s2, s3, s1Sub, s2Sub, s3Sub, s1Sub2, s2Sub2, s3Sub2, true, "Từ chối nhận kẹo");

    public static void StartConfirmNpc(short npcId,
        string s1, string s2, string s3,
        string s1Sub, string s2Sub, string s3Sub,
        string s1Sub2, string s2Sub2, string s3Sub2,
        bool nextInfo, string infoSuKien)
    {
        idNpcService = npcId;
        nameIndex1 = s1; nameIndex2 = s2; nameIndex3 = s3;
        s1SubName = s1Sub; s2SubName = s2Sub; s3SubName = s3Sub;
        s1SubName2 = s1Sub2; s2SubName2 = s2Sub2; s3SubName2 = s3Sub2;
        confirming = true;
        runningOpenNpc = false;
        currentStep = 0;
        _confirmStartTime = Lib.TimeNow();
        _lastStepTime = 0;
        _nextInfoSuKien = nextInfo;
        _infoSuKien = infoSuKien;
    }

    // ═══════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════
    public static void UpdateConfirmNpc()
    {
        if (!confirming) return;

        long now = Lib.TimeNow();

        // Timeout
        if (now - _confirmStartTime > CONFIRM_TIMEOUT)
        {
            Reset();
            GameCanvas.menu.doCloseMenu();
            return;
        }

        // Mở menu nếu chưa mở
        if (!GameCanvas.menu.showMenu && !runningOpenNpc)
        {
            Service.gI().openMenu(idNpcService);
            runningOpenNpc = true;
        }

        if (!GameCanvas.menu.showMenu) return;
        if (_lastStepTime > 0 && now - _lastStepTime < STEP_DELAY) return;

        // Halloween event
        if (_nextInfoSuKien && NextMap.nextSuKienHalloween)
        {
            if (SelectByName(_infoSuKien))
            {
                _infoSuKien = "";
                runningOpenNpc = false;
                _lastStepTime = now;
                return;
            }
            _nextInfoSuKien = false;
        }

        // Step 0 → 1 → 2
        if (currentStep == 0 && !string.IsNullOrEmpty(nameIndex1))
        {
            if (SelectByName(nameIndex1, s1SubName, s1SubName2))
            {
                nameIndex1 = s1SubName = s1SubName2 = "";
                currentStep = 1;
                _lastStepTime = now;
            }
        }
        else if (currentStep == 1 && !string.IsNullOrEmpty(nameIndex2))
        {
            if (SelectByName(nameIndex2, s2SubName, s2SubName2))
            {
                nameIndex2 = s2SubName = s2SubName2 = "";
                currentStep = 2;
                _lastStepTime = now;
            }
        }
        else if (currentStep == 2 && !string.IsNullOrEmpty(nameIndex3))
        {
            if (SelectByName(nameIndex3, s3SubName, s3SubName2))
            {
                nameIndex3 = s3SubName = s3SubName2 = "";
                currentStep = 3;
                _lastStepTime = now;
            }
        }

        // Xong tất cả steps
        if (string.IsNullOrEmpty(nameIndex1) &&
            string.IsNullOrEmpty(nameIndex2) &&
            string.IsNullOrEmpty(nameIndex3))
            confirming = false;
    }

    // ═══════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════
    private static bool SelectByName(string name, string sub1 = "", string sub2 = "")
    {
        if (GameCanvas.menu.menuItems == null || GameCanvas.menu.menuItems.size() == 0)
            return false;

        string norm = Normalize(name);
        string normSub1 = Normalize(sub1);
        string normSub2 = Normalize(sub2);

        for (int i = 0; i < GameCanvas.menu.menuItems.size(); i++)
        {
            try
            {
                var cmd = (Command)GameCanvas.menu.menuItems.elementAt(i);
                if (cmd == null) continue;
                string cap = Normalize(cmd.caption ?? "");

                if (cap == norm ||
                    (!string.IsNullOrEmpty(normSub1) && cap == normSub1) ||
                    (!string.IsNullOrEmpty(normSub2) && cap == normSub2))
                {
                    GameCanvas.menu.menuSelectedItem = i;
                    GameCanvas.menu.performSelect();
                    GameCanvas.menu.doCloseMenu();
                    return true;
                }
            }
            catch { }
        }
        return false;
    }

    // Bỏ khoảng trắng, lowercase, chỉ giữ ký tự in được
    private static string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (char c in input.ToLower())
            if (c >= 32 && c != 127 && c != ' ' && c != '\t' && c != '\n' && c != '\r')
                sb.Append(c);
        return sb.ToString();
    }

    private static void Reset()
    {
        confirming = false;
        runningOpenNpc = false;
        currentStep = 0;
    }

    // ═══════════════════════════════════════════
    // GROUND HELPER
    // ═══════════════════════════════════════════
    public static int GetClosestGroundY(int x, int targetY)
    {
        int best = -1;
        int bestDst = int.MaxValue;

        for (int y = 50; y <= TileMap.pxh; y += 24)
        {
            if (!TileMap.tileTypeAt(x, y, 2)) continue;
            int gy = y - y % 24;
            int dst = Math.Abs(gy - targetY);
            if (dst < bestDst) { bestDst = dst; best = gy; }
        }

        return best != -1 ? best : MainXmap.GetYGround(x);
    }
}