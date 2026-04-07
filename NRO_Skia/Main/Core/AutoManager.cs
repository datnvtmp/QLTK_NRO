using Assets.src.e;
using Mod;
using NRO_Skia.Main.AutoTrain;
using NRO_Skia.Main.Core;
using NRO_Skia.Main.CSKB;
using NRO_Skia.Main.Until;
using System;
using System.Threading.Tasks;

public static class AutoManager
{
    // ═══════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════
    public static int State = 0;
    public static bool IsAutoOffByItem = false;
    public static int IdItem;
    public static int TargetCount;

    private static long _lastSendState = 0;
    private const long SEND_STATE_INTERVAL = 5000;
    public static bool lowGraphic = false;

    // Thêm vào AutoManager — state
    public static bool IsAttackAll = false;
    private static long _lastAttackAllTime = 0;
    // ─── Thêm vào AutoManager — FPS tracking ─────────────────────────────
    private static long _lastFpsTime = 0;
    private static int _fpsCount = 0;
    private static int _fps = 0;

    private static bool _isAutoAttack = false;
    private static bool _isAutoLive = false;

    private static long _lastReceiveTime = 0;
    public static int Ping = -1;


    // ═══════════════════════════════════════════
    // UPDATE — gọi từ GameLoop.RunUpdate()
    // ═══════════════════════════════════════════
    public static void Update()
    {
        MainXmap.Update();
        AutoTrain.Update();
        AutoOffWhenEnough();
        AutoBuyBua.Update();
        AutoCSKB.Update();
        UpdateAttackAll(); // 
        UpdateAutoAttack();
        UpdateAutoLive();
        //TestGD();
        // ITEM
        AutoItemManager.Instance.Tick();
        AutoTrashItem.Tick();
    }

    // ═══════════════════════════════════════════
    // AUTO OFF KHI ĐỦ ITEM
    // ═══════════════════════════════════════════
    private static void AutoOffWhenEnough()
    {
        if (!IsAutoOffByItem) return;
        if (Lib.GetItemQuantity(IdItem) != TargetCount) return;
        GameMidlet.instance.exit();
    }
    private static void UpdateAutoLive()
    {
        if (Char.myCharz().meDead && _isAutoLive) Service.gI().wakeUpFromDead();
    }
    private static void UpdateAutoAttack()
    {
        if (!_isAutoAttack) return;
        long now = Lib.TimeNow();
        if (now - _lastAttackAllTime < 300) return;
        _lastAttackAllTime = now;

        var me = Char.myCharz();
        if (me == null || me.meDead) return;
        if (me.statusMe == 14 || me.statusMe == 5 || me.isWaitMonkey) return;

        // ✅ noReselect đúng chuẩn như AutoTrain
        bool noReselectv = me.myskill != null
            && (me.myskill.template.id == 0 || me.myskill.template.id == 2 ||
                me.myskill.template.id == 3 || me.myskill.template.id == 4 ||
                me.myskill.template.id == 9 || me.myskill.template.id == 17);

        if (!noReselectv)
            return; // nếu không reselect thì thôi, đợi lần sau, tránh trường hợp đang dùng skill khác mà tự nhiên đổi sang skill mới
            
        // Đánh người
        if (me.charFocus != null
            && me.charFocus.statusMe != 14
            && me.charFocus.statusMe != 5)
        {
            me.myskill.lastTimeUseThisSkill = now;
            me.cMP -= me.myskill.manaUse;
            var chars = new MyVector();
            chars.addElement(me.charFocus);
            Service.gI().sendPlayerAttack(new MyVector(), chars, 2);
            return;
        }

        if (me.mobFocus == null) return;

        // Đánh mob
        me.myskill.lastTimeUseThisSkill = now;

        me.cMP -= me.myskill.manaUse;
        var mobs = new MyVector();
        mobs.addElement(me.mobFocus);
        Service.gI().sendPlayerAttack(mobs, new MyVector(), 1);
    }
    // ═══════════════════════════════════════════
    // SEND STATE — throttle 5s
    // ═══════════════════════════════════════════
    public static void SendState(string status)
    {
        long now = Lib.TimeNow();
        if (now - _lastSendState < SEND_STATE_INTERVAL) return;
        _lastSendState = now;

        try
        {
            string msg = "GAME_STATE:" + status;
            GameClient.Send(msg);
            Console.WriteLine(msg);
        }
        catch (Exception ex)
        {
            Logger.Log($"[ERROR - SendState] {ex.Message}\n{ex.StackTrace}");
        }
    }

    // ═══════════════════════════════════════════
    // CHAT LỆNH — nhận lệnh từ chat game
    // ═══════════════════════════════════════════

    // ═══════════════════════════════════════════
    // CHAR LOGIN SUCCESS
    // ═══════════════════════════════════════════
    public static void CharNameLoginSuccess()
    {
        SkillTrain.Init(); // ← skill đã có đủ lúc này
        GameClient.Send(MsgType.CHAR_INFO, Char.myCharz().cName);
        Char.myCharz().cspeed = 8;
    }

    // ═══════════════════════════════════════════
    // PAINT HUD
    // ═══════════════════════════════════════════
    public static void PaintGD(mGraphics g)
    {

        // ── Map info ─────────────────────────────────────────────────
        string map = $"[{TileMap.mapID}] {TileMap.mapName}-[K{TileMap.zoneID}]";
        mFont.tahoma_7b_white.drawString(g, map, 10, 88, 0);

        // ── Coordinates ──────────────────────────────────────────────
        var me = Char.myCharz();
        if (me == null) return;

        string coord = $"X: {me.cx} - Y: {me.cy}";
        mFont.tahoma_7b_white.drawString(g, coord, 10, 103, 0);
    }
    public static void PaintScr(mGraphics g)
    {
        _fpsCount++;
        long now = Lib.TimeNow();
        if (now - _lastFpsTime >= 1000)
        {
            _fps = _fpsCount;
            _fpsCount = 0;
            _lastFpsTime = now;
        }
        if (GameCanvas.panel != null && GameCanvas.panel.isShow) return;

        string screenName = GameCanvas.currentScreen?.GetType().Name ?? "null";
        string pingStr = Ping < 0 ? "---" : $"{Ping}ms";
        string info = $"FPS:{_fps} | {screenName}";  // ← thêm pingStr

        int textW = mFont.tahoma_7_white.getWidth(info) + 6;
        int fpsX = (GameCanvas.w - textW) / 2;
        int fpsY = 0;

        g.setColor(0x003366);
        g.fillRect(fpsX, fpsY + 25, textW, 12);
        mFont.tahoma_7b_white.drawString(g, info, fpsX + 3, fpsY + 25, 0);
    }


    // Trong AutoManager.cs — khi muốn clear
    public static void SetLowGraphic(bool value)
    {
        AutoManager.lowGraphic = value;
        GameCanvas.clearImageCache(); // gọi sang GameCanvas
    }
    private static void UpdateAttackAll()
    {
        if (!IsAttackAll) return;
        if (MainXmap.isXmaping) return;

        long now = Lib.TimeNow();
        if (now - _lastAttackAllTime < 100) return;
        _lastAttackAllTime = now;

        var me = Char.myCharz();
        if (me == null || me.meDead) return;

        // ✅ Thêm isWaitMonkey
        if (me.statusMe == 14 || me.statusMe == 5 || me.isWaitMonkey) return;

        // ✅ Thêm check sau khi hết khỉ
        if (SkillSelector.LastMonkeyEndTime > 0
            && now - SkillSelector.LastMonkeyEndTime < 2000) return;

        if (GameScr.vMob == null) return;

        var current = me.mobFocus;
        bool currentValid = current != null
            && current.hp > 0
            && current.status != 0
            && current.status != 1
            && !current.isMobMe;

        if (!currentValid)
        {
            me.mobFocus = null;
            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                var mob = (Mob)GameScr.vMob.elementAt(i);
                if (mob == null || mob.hp <= 0) continue;
                if (mob.status == 0 || mob.status == 1) continue;
                if (mob.isMobMe) continue;
                me.mobFocus = mob;
                break;
            }
        }

        if (me.mobFocus == null) return;

        var skill = SkillSelector.ChooseSkill();
        if (skill == null) return;

        // ✅ Thêm check buff HS
        if (now - AutoTrain.TimeUseBuffHS < 1000) return;

        UseSkillAttackAll(skill);
    }

    private static void UseSkillAttackAll(Skill skill)
    {
        var me = Char.myCharz();
        if (me.isCharge) return; // ✅ check isCharge

        // ✅ Tracking monkey
        if (me.isMonkey == 1)
            SkillSelector.WasMonkey = true;
        else if (SkillSelector.WasMonkey)
        {
            SkillSelector.LastMonkeyEndTime = Lib.TimeNow();
            SkillSelector.WasMonkey = false;
            GameScr.info1?.addInfo("HẾT KHỈ, ĐỢI 2s...");
            return;
        }

        // ✅ Skill 7 delay 1.5s
        if (skill.template.id == 7)
        {
            if (!_isWaitingSkill7)
            {
                GameScr.gI().doSelectSkill(skill, true);
                _timeSelectSkill7 = Lib.TimeNow();
                _isWaitingSkill7 = true;
                return;
            }
            if (Lib.TimeNow() - _timeSelectSkill7 < 1500) return;

            var targets = new MyVector();
            targets.addElement(me);
            Service.gI().sendPlayerAttack(new MyVector(), targets, -1);
            SkillSelector.TimeToBuffHS = Lib.TimeNow();
            AutoTrain.TimeUseBuffHS = Lib.TimeNow();
            _isWaitingSkill7 = false;
            return;
        }

        _isWaitingSkill7 = false;

        // ✅ noReselect đúng chuẩn như AutoTrain
        bool noReselect = skill == me.myskill
            && (skill.template.id == 0 || skill.template.id == 2 ||
                skill.template.id == 3 || skill.template.id == 4 ||
                skill.template.id == 9 || skill.template.id == 17);

        if (!noReselect)
            GameScr.gI().doSelectSkill(skill, true);
        else
            AutoSkill.AutoSendAttack();
    }
    private static bool _isWaitingSkill7;
    private static long _timeSelectSkill7;

    public static bool PhimTat()
    {
        switch (GameCanvas.keyAsciiPress)
        {
            case 'a':
                if (IsAttackAll)
                {
                    IsAttackAll = false;
                }
                _isAutoAttack = !_isAutoAttack;
                GameScr.info1?.addInfo(_isAutoAttack ? "Bật Auto Attack!" : "Tắt Auto Attack!");

                break;
            case 't':
                if (_isAutoAttack) _isAutoAttack = false;
                IsAttackAll = !IsAttackAll;
                GameScr.info1?.addInfo(IsAttackAll ? "Bật Attack All!" : "Tắt Attack All!");
                break;
            case 'c':
                if (Lib.UseItemBoolean(193)) return true;
                else if (Lib.UseItemBoolean(194)) return true;
                return false;
                break;
            case 'f':
                Lib.UsePorata();
                Service.gI().petStatus((sbyte)3); // gửi lên server

                break;
            case 's':
                break;
            case 'm':
                Service.gI().openUIZone();
                break;
            case 'j':
                MainXmap.LoadMapLeft();
                break;
            case 'k':
                MainXmap.LoadMapCenter();
                break;
            case 'l':
                MainXmap.LoadMapRight();
                break;
            case 'x':
                break;
            case 'g':
                if (Char.myCharz().charFocus != null)
                {
                    Service.gI().giaodich(0, Char.myCharz().charFocus.charID, -1, -1);
                }
                else GameScr.info1?.addInfo("Chưa chọn mục tiêu!");
                break;
            case 'd':
                break;
            case 'e':
                _isAutoLive = !_isAutoLive;
                GameScr.info1?.addInfo(_isAutoLive ? "Bật Auto hồi sinh!" : "Tắt Auto hồi sinh!");
                break;
            case 'v':
                if (Char.myCharz().mobFocus != null)
                {
                    var me = Char.myCharz();
                    me.cx = me.mobFocus.xFirst;
                    me.cy = me.mobFocus.yFirst;
                    Service.gI().charMove();
                }
                break;
            default:
                return false;
        }
        return true;
    }
}