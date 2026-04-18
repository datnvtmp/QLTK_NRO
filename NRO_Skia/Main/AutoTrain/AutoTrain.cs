using Mod;
using NRO_Skia.Main.Core;
using NRO_Skia.Main.CSKB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRO_Skia.Main.AutoTrain
{
    public static class AutoTrain
    {
        public static AccConfig Config { get; set; } = new AccConfig();

        private static bool _configApplied = false;
        public static void ResetApplied() => _configApplied = false;

        public static void ApplyConfig(AccConfig config)
        {
            try
            {
                IsRunning = config.IsTrain;        // ← sửa IsTrain → Train
                TargetMapId = config.MapIndex;
                TargetZoneId = config.IsKhu ? config.Khu : -1;
                MobType = config.LoaiQuai;

                // ID quái
                if (!string.IsNullOrEmpty(config.IdQuai))
                    ListMobIds = config.IdQuai.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int v) ? v : -1)
                        .Where(v => v >= 0).ToArray();
                else
                    ListMobIds = null;

                // List skill
                if (!string.IsNullOrEmpty(config.ListSkill))
                {
                    var enabledIds = config.ListSkill.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int v) ? v : -1)
                        .Where(v => v >= 0).ToHashSet();

                    if (SkillTrain.SkillTrains != null)
                        foreach (var st in SkillTrain.SkillTrains)
                        {
                            if (st == null) continue;
                            st.AutoFlag = enabledIds.Contains(st.Id);
                        }
                }

                // ← check Use flag trước khi apply
                TrenHp = (config.UseTrenHp && long.TryParse(config.TrenHp, out long maxHp)) ? maxHp : 0L;
                DuoiHp = (config.UseDuoiHp && long.TryParse(config.DuoiHp, out long minHp)) ? minHp : long.MaxValue;

                // ← check UseNeChar
                if (config.UseNeChar && !string.IsNullOrEmpty(config.NeChar))
                {
                    NeCharNames = config.NeChar.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0).ToArray();
                    AutoNeChar = true;
                }
                else
                {
                    NeCharNames = null;
                    AutoNeChar = false;
                }

                MobLoaiTru = int.TryParse(config.IDMobLoaiTru, out int mobLoaiTru) ? mobLoaiTru : -1;

                // Linh tinh
                IsAutoUsePorata = config.BongTai;
                IsAutoFlag = config.CoDen;
                IsWaitHS = config.DoiHs;
                IsAutoBuyTDLT = config.MuaTdlt;
                IsAttackFull = config.PemMatMau;   // ← thêm dòng này, bị thiếu
                UPSKH = config.UpSkh;
                UpGiap = config.UpGiap;
                IsDontMove = config.DungIm;

                IsAntiKS = config.IsAntiKS;

                if (config.UseIdDung && !string.IsNullOrEmpty(config.IdDung))
                    AutoItemManager.Instance.ApplyFromConfig(config.IdDung);
                else
                    AutoItemManager.Instance.StopAll();
                if (config.UseIdVut && !string.IsNullOrEmpty(config.IdVut))
                {
                    AutoTrashItem.SetIds(config.IdVut);
                    AutoTrashItem.Start();
                }
                else
                {
                    AutoTrashItem.Stop();
                    AutoTrashItem.TrashIds.Clear();
                }

                GameScr.info1?.addInfo("Đã áp dụng config từ QLTK!");
            }
            catch (Exception ex)
            {
                GameScr.info1?.addInfo("Lỗi khi áp dụng config: " + ex.Message);
                Logger.Log($"[ERROR - APPLY_CONFIG]: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ── Config ────────────────────────────────────────────────────────────
        public static int TargetMapId { get; set; } = -1;
        public static int TargetZoneId { get; set; } = -1;
        public static int MobType { get; set; } = 0;
        public static int MobLoaiTru { get; set; } = -1;
        public static long DuoiHp { get; set; } = long.MaxValue;
        public static long TrenHp { get; set; } = 0L;
        public static bool UpGiap { get; set; }
        public static int[] ListMobIds { get; set; } = null;
        public static string[] NeCharNames { get; set; } = null;

        // ── State flags ───────────────────────────────────────────────────────
        public static bool IsRunning { get; private set; }
        public static bool IsGoingBack { get; private set; }
        public static bool IsBuyingTDLT { get; private set; }
        public static bool IsDontMove { get; set; } = true;
        public static bool IsAutoFlag { get; set; }
        public static bool IsAttackFull { get; set; }
        public static bool IsWaitHS { get; set; }
        public static bool IsUpMapPrivate { get; set; }
        public static bool IsAutoBuyTDLT { get; set; }
        public static bool IsAutoUsePorata { get; set; }
        public static bool AutoNeChar { get; set; }
        public static bool UPSKH { get; set; }
        // ── Anti-KS ───────────────────────────────────────────────────────────
        public static bool IsAntiKS { get; set; }
        private static int _lastAttackedMobId = -1; // ID mob mình vừa đánh


        // ── Timing ────────────────────────────────────────────────────────────
        private static long _lastUpdateTime;
        private static long _lastZoneChangeTime;
        private static long _lastBuyTime;
        public static long TimeUseBuffHS;
        private static long _lastAttackTime; // ← thêm vào khu vực Timing

        private static int _lastMobIndex = 0;

        // ── Buy steps ─────────────────────────────────────────────────────────
        private static int _buyStep;
        private static int _shopMapId;
        private static int _shopNpcId;

        // ── Skill 7 state ─────────────────────────────────────────────────────
        private static long _timeSelectSkill7;
        private static bool _isWaitingSkill7;

        private const int STEP_GO_TO_MAP = 0;
        private const int STEP_BUY_ITEM = 1;
        private const int STEP_USE_ITEM = 2;
        private const int STEP_RETURN = 3;

        private static int COUNT_USE_TDLT = 3;

        // ── MyVector helper ───────────────────────────────────────────────────
        private static IEnumerable<T> Enumerate<T>(MyVector v)
        {
            if (v == null) yield break;
            for (int i = 0; i < v.size(); i++)
                yield return (T)v.elementAt(i);
        }

        public static void Update()
        {
            // Apply config lazy — bất kể đang chạy hay không
            // Thay vì check != null, check có skill thực sự
            if (!_configApplied && Config != null && SkillTrain.SkillTrains != null)
            {
                ApplyConfig(Config);
                _configApplied = true;
            }

            if (!IsRunning) return;
            if (MainXmap.isXmaping) return;
            if (AutoBossNapa.IsRunning) return;
            if (AutoBuyBua.IsRunning) return;
            if (AutoCSKB.IsRunning) return;


            var me = Char.myCharz();

            if (me.meDead) { HandleDead(); return; }
            if (IsGoingBack) { HandleGoingBack(); return; }
            if (IsBuyingTDLT) { HandleBuyingTDLT(); return; }
            if (IsAutoBuyTDLT && ShouldBuyTDLT()) { StartBuyTDLT(); return; }

            if (Char.myCharz().cStamina < GameConstants.MIN_STAMINA){ UseStaminaItem(); return; }

            if (TargetMapId != TileMap.mapID && !IsUpMapPrivate)
            {
                MainXmap.StartGoToMap(TargetMapId);
                return;
            }

            if (me.cHP <= me.cHPFull * 20L / 100L || me.cMP <= me.cMPFull * 20L / 100L) Lib.UseMagicTree();

            if (IsAutoFlag) HandleAutoFlag();
            else if (GameCanvas.gameTick % 180 == 0 && me.cFlag != 0) Service.gI().getFlag(1, 0);
            if (IsUpMapPrivate && GameScr.vCharInMap?.size() != 1 && GameCanvas.gameTick % 100 == 0)
                Lib.UseItem(GameConstants.ID_ITEM_TICKET_PRIVATE);

            if (UpGiap && GameCanvas.gameTick % 200 == 0)
            {
                AutoGLT.UseGLT();
                for (int i = 0; i < Char.vItemTime.size(); i++)
                {
                    var it = Char.vItemTime.elementAt(i) as ItemTime;

                    if (it.idIcon == GameConstants.ID_ITEM_TDLT_ICON)
                    {
                        Lib.UseItem(GameConstants.ID_ITEM_TDLT);
                        break;
                    }
                }
            }
            DoAutoTrain();
        }

        // =====================================================================
        // TRAIN CORE
        // =====================================================================

        private static void DoAutoTrain()
        {

            if (AutoSendInFo.GetGoldBase() == -1L)
                AutoSendInFo.StartAuto();

            UseFusionItem();
            ChangeZoneIfNeeded();

            long now = Lib.TimeNow();
            if (!ShouldUpdate(now)) return;

            _lastUpdateTime = now;
            AttackMobs();
        }

        private static void AttackMobs()
        {

            var me = Char.myCharz();
            if (me.statusMe == 14 || me.statusMe == 5 || me.isWaitMonkey) return;

            if (SkillSelector.LastMonkeyEndTime > 0 && Lib.TimeNow() - SkillSelector.LastMonkeyEndTime < 2000) return;

            var current = me.mobFocus;
            if (IsValidMob(current) && IsMatchMobType(current))
            {
                AttackMob(current);
                return;
            }
            me.mobFocus = null; // ← clear nếu không hợp lệ
            var next = FindValidMob();
            if (next == null) return;

            me.mobFocus = next;
            AttackMob(next);
            MoveToMob(next);
        }

        // Tách riêng hàm check type để dùng lại
        private static bool IsMatchMobType(Mob mob)
        {
            if (MobType == 0) return true;
            var tpl = mob.getTemplate();
            if (tpl == null) return false;
            return MobType switch
            {
                1 => tpl.type == Mob.TYPE_DI,
                2 => tpl.type == Mob.TYPE_BAY,
                3 => ListMobIds != null && Array.IndexOf(ListMobIds, mob.mobId) >= 0,
                _ => false
            };
        }


        private static Mob FindValidMob()
        {
            if (GameScr.vMob == null) return null;
            int count = GameScr.vMob.size();
            if (count == 0) return null;

            // Kiểm tra sớm để tránh lặp vô ích
            if (MobType == 3 && ListMobIds == null)
            {
                GameScr.info1.addInfo("Danh sách ID quái trống!");
                return null;
            }

            for (int i = 0; i < count; i++)
            {
                int idx = (_lastMobIndex + i) % count;
                var mob = GameScr.vMob.elementAt(idx) as Mob;
                if (!IsValidMob(mob)) continue;

                var tpl = mob.getTemplate();
                if (tpl == null) continue; // ← fix lỗi 1

                bool matched = IsMatchMobType(mob);
                if (!matched) continue;

                _lastMobIndex = (idx + 1) % count;
                return mob;
            }
            return null;
        }

        private static bool IsValidMob(Mob mob)
        {
            if (mob == null) return false;
            if (mob.hp <= 0)
            {
                if (mob.mobId == _lastAttackedMobId)
                    _lastAttackedMobId = -1; // Chết rồi, reset
                return false;
            }
            if (mob.status == 0 || mob.status == 1) return false;
            if (mob.isMobMe) return false;
            // AntiKS: mob này mình đang đánh dở → đánh chết luôn dù HP không full
            // AntiKS: chỉ đánh mob full HP hoặc mob mình đang đánh dở
            if (IsAntiKS && mob.mobId != _lastAttackedMobId)
            {
                float hpPercent = (float)mob.hp / mob.maxHp;
                if (hpPercent < 0.6f) return false; // Dưới 60% → người khác đang đánh, bỏ qua
            }
            if (IsAttackFull && mob.hp != mob.maxHp) return false;
            if (MobLoaiTru == mob.mobId) return true;  // loại trừ bỏ qua filter HP

            return mob.hp >= TrenHp && mob.hp <= DuoiHp;
        }

        private static void MoveToMob(Mob mob)
        {
            if (IsDontMove) return;
            var me = Char.myCharz();
            me.cx = mob.xFirst;
            me.cy = mob.yFirst;
            Service.gI().charMove();
        }

        private static void AttackMob(Mob mob)
        {
            var tpl = mob.getTemplate();
            GameClient.Send(MsgType.DATA_INGAME, $"[ Train ] {tpl.name} => [ {AutoSendInFo.FormatPower(mob.hp)} ]");

            var skill = SkillSelector.ChooseSkill();
            if (skill == null) return;
            _lastAttackTime = Lib.TimeNow(); // ← thêm dòng này

            if (IsAntiKS) _lastAttackedMobId = mob.mobId;

            UseSkillOnMob(skill);
        }

        // ── Skill logic ───────────────────────────────────────────────────────
        // ── Skill 13 state ────────────────────────────────────────────────────────
        private static long _timeSelectSkill13;
        private static bool _isWaitingSkill13;
        private static void UseSkillOnMob(Skill skill)
        {
            var me = Char.myCharz();
            if (me.isCharge) return;

            // Skill 7 — cần delay 1.5s sau khi chọn
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
                targets.addElement(Char.myCharz());
                Service.gI().sendPlayerAttack(new MyVector(), targets, -1);

                // Cập nhật mốc thời gian để UI chạy bóng mờ hồi chiêu
                skill.lastTimeUseThisSkill = Lib.TimeNow();

                SkillSelector.TimeToBuffHS = Lib.TimeNow();
                TimeUseBuffHS = Lib.TimeNow();
                _isWaitingSkill7 = false;
                return;
            }

            _isWaitingSkill7 = false;


            // Skill 13 — đợi 2s trước khi biến khỉ
            if (skill.template.id == 13)
            {
                if (!_isWaitingSkill13)
                {
                    _timeSelectSkill13 = Lib.TimeNow();
                    _isWaitingSkill13 = true;
                    GameScr.gI().doSelectSkill(skill, true);
                    GameScr.info1.addInfo("Đợi 2s để biến khỉ...");
                    return;
                }

                if (Lib.TimeNow() - _timeSelectSkill13 < 2500) return; // chưa đủ 2s
                GameScr.gI().doSelectSkill(skill, true);
                _isWaitingSkill13 = false;
                return;
            }

            _isWaitingSkill13 = false; // reset nếu skill khác được chọn

            bool noReselect = skill == me.myskill
                && (skill.template.id == 0 || skill.template.id == 2 ||
                    skill.template.id == 3 || skill.template.id == 4 ||
                    skill.template.id == 23 || skill.template.id == 18 ||
                    skill.template.id == 9 || skill.template.id == 17);

            if (!noReselect)
                GameScr.gI().doSelectSkill(skill, true);
            else
                AutoSkill.AutoSendAttack();
        }

        // =====================================================================
        // DEAD / REVIVE
        // =====================================================================

        private static void HandleDead()
        {
            if (!IsGoingBack)
            {
                IsGoingBack = true;
                Char.myCharz().mobFocus = null;
                GameScr.info1.addInfo("Bạn chết rồi, đang hồi sinh...");
                GameClient.Send(MsgType.DATA_INGAME, "[ Train ] Đã chết, đang hồi sinh...");
            }

            if (IsWaitHS) return;

            if (GameCanvas.gameTick % GameConstants.TICK_REVIVE == 0)
                Service.gI().returnTownFromDead();
        }

        private static void HandleGoingBack()
        {
            MainXmap.StartGoToMap(TargetMapId);

            if (TargetMapId == TileMap.mapID && GameCanvas.gameTick % GameConstants.TICK_REVIVE == 0)
            {
                IsGoingBack = false;
                GameScr.info1.addInfo("Đã hồi sinh, tiếp tục auto!");
                GameClient.Send(MsgType.DATA_INGAME, "[ Train ] Đã hồi sinh, tiếp tục auto!");
            }
        }

        // =====================================================================
        // FLAG
        // =====================================================================

        private static void HandleAutoFlag()
        {
            if (Char.myCharz().cFlag != 0) return;
            if (GameCanvas.gameTick % 180 == 0)
                Service.gI().getFlag(1, 8);
        }

        // =====================================================================
        // ITEM — Stamina / Fusion
        // =====================================================================

        private static void UseStaminaItem()
        {
            if (Lib.UseStaminaItem())
            {
                GameScr.info1.addInfo("Ăn nho rồi :v");
                return;
            }

            int.TryParse(Char.myCharz().luongKhoaStr, out int ngocK);
            int.TryParse(Char.myCharz().luongStr, out int ngoc);
            
            // Ưu tiên dùng biến int nếu parse chuỗi thất bại
            if (ngocK == 0) ngocK = Char.myCharz().luongKhoa;
            if (ngoc == 0) ngoc = Char.myCharz().luong;

            if (ngocK < 4 && ngoc < 4)
            {
                return;
            }

            AutoBuyBua.Start();
        }

        private static void UseFusionItem()
        {
            if (!IsAutoUsePorata) return;
            if (Char.myCharz().isNhapThe) return;
            if (GameCanvas.gameTick % GameConstants.TICK_USE_ITEM != 0) return;

            if (!Lib.UsePorata())
                Char.myCharz().isNhapThe = true;
        }

        // =====================================================================
        // ZONE
        // =====================================================================

        private static void ChangeZoneIfNeeded()
        {
            if (IsUpMapPrivate) return;

            if (AutoNeChar) { TryEscapeChar(); return; }

            if (TargetZoneId == -1 || TargetZoneId == TileMap.zoneID) return;

            long now = Lib.TimeNow();
            if (now - _lastZoneChangeTime < GameConstants.TIME_CHANGE_ZONE) return;
            if (now - MainXmap.LAST_TIME_FINISH_XMAP < 2000) return;

            Service.gI().requestChangeZone(TargetZoneId, -1);
            _lastZoneChangeTime = now;
        }

        private static void TryEscapeChar()
        {
            if (NeCharNames == null || GameScr.vCharInMap == null) return;

            long now = Lib.TimeNow();
            if (now - _lastZoneChangeTime < GameConstants.TIME_CHANGE_ZONE) return;

            foreach (var c in Enumerate<Char>(GameScr.vCharInMap))
            {
                if (c == Char.myCharz() || c.cName == null) continue;

                bool found = false;
                foreach (var name in NeCharNames)
                    if (c.cName.IndexOf(name) != -1) { found = true; break; }

                if (!found) continue;

                Service.gI().requestChangeZone(-1, -1);
                _lastZoneChangeTime = now;
                GameScr.info1.addInfo($"Phát hiện {c.cName}, đổi khu!");
                GameClient.Send(MsgType.DATA_INGAME, $"[ Train ] Phát hiện {c.cName}, đổi khu!");
                return;
            }
        }

        // =====================================================================
        // MUA TDLT
        // =====================================================================

        private static bool ShouldBuyTDLT()
        {
            foreach (var it in Enumerate<ItemTime>(Char.vItemTime))
            {
                if (it.idIcon == GameConstants.ID_ITEM_TDLT_ICON)
                {
                    if (it.minute >= GameConstants.MIN_TDLT_TIME)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        private static void StartBuyTDLT()
        {
            if (IsBuyingTDLT) return;

            switch (Char.myCharz().cgender)
            {
                case 0: _shopMapId = GameConstants.ID_MAP_AURU; _shopNpcId = GameConstants.ID_NPC_BUMA; break;
                case 1: _shopMapId = GameConstants.ID_MAP_NAMEK; _shopNpcId = GameConstants.ID_NPC_DENDE; break;
                default: _shopMapId = GameConstants.ID_MAP_KAKAROT; _shopNpcId = GameConstants.ID_NPC_APPULE; break;
            }

            IsBuyingTDLT = true;
            _buyStep = STEP_GO_TO_MAP;

            GameScr.info1.addInfo("TDLT sắp hết, đang đi mua...");
            GameClient.Send(MsgType.DATA_INGAME, "[ Train ] TDLT sắp hết, đang đi mua...");
        }

        private static void HandleBuyingTDLT()
        {
            long now = Lib.TimeNow();
            if (now - _lastBuyTime < GameConstants.TIME_BUY_DELAY) return;
            _lastBuyTime = now;

            switch (_buyStep)
            {
                case STEP_GO_TO_MAP: BuyStep_GoToMap(); break;
                case STEP_BUY_ITEM: BuyStep_BuyItem(); break;
                case STEP_USE_ITEM: BuyStep_UseItem(); break;
                case STEP_RETURN: BuyStep_Return(); break;
            }
        }

        private static void BuyStep_GoToMap()
        {
            if (TileMap.mapID != _shopMapId)
            {
                if (!MainXmap.isXmaping) MainXmap.StartGoToMap(_shopMapId);
            }
            else _buyStep = STEP_BUY_ITEM;
        }

        private static void BuyStep_BuyItem()
        {
            if (TileMap.mapID != _shopMapId) { _buyStep = STEP_GO_TO_MAP; return; }

            Service.gI().openMenu(_shopNpcId);
            Service.gI().confirmMenu((short)_shopNpcId, 0);

            if (Lib.ExistItemBag(GameConstants.ID_ITEM_TDLT))
            {
                Service.gI().buyItem(1, GameConstants.ID_ITEM_TDLT3, 0);
                _buyStep = STEP_USE_ITEM;
            }
            else
            {
                Service.gI().buyItem(1, GameConstants.ID_ITEM_TDLT, 0);
                GameScr.info1.addInfo("Đang mua TDLT...");
                GameClient.Send(MsgType.DATA_INGAME, "[ Train ] Đang mua TDLT...");
            }
        }

        private static void BuyStep_UseItem()
        {
            if (Lib.ExistItemBag(GameConstants.ID_ITEM_TDLT))
            {
                Lib.UseItem(GameConstants.ID_ITEM_TDLT);
                GameScr.info1.addInfo("Đang cắn TDLT...");
                GameClient.Send(MsgType.DATA_INGAME, "[ Train ] Đang cắn TDLT...");
                _buyStep = STEP_RETURN;
            }
            else _buyStep = STEP_BUY_ITEM;
        }

        private static void BuyStep_Return()
        {
            if (!ShouldBuyTDLT())
            {
                if (TileMap.mapID != TargetMapId)
                    MainXmap.StartGoToMap(TargetMapId);
                else
                {
                    IsBuyingTDLT = false;
                    _buyStep = 0;
                    GameScr.info1.addInfo("Tiếp tục treo!");
                }
            }
            else _buyStep = STEP_USE_ITEM;
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private static bool ShouldUpdate(long now)
        {
            bool correctZone = TargetZoneId == TileMap.zoneID
                            || TargetZoneId == -1
                            || IsUpMapPrivate
                            || AutoNeChar;

            return correctZone && now - _lastUpdateTime >= GameConstants.TIME_UPDATE_TICK;
        }
    }
}