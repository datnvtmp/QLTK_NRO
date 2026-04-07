using System.Collections.Generic;

namespace NRO_Skia.Main.AutoTrain
{
    /// <summary>
    /// Theo dõi HP từng con quái theo từng tick.
    /// Mục đích: phát hiện HP giảm do NGƯỜI KHÁC đánh (không phải mình).
    /// </summary>
    public static class MobHpTracker
    {
        // HP snapshot tick trước — key = mobId
        private static readonly Dictionary<int, long> _lastHp = new Dictionary<int, long>();

        // Thời điểm gần nhất HP con quái đó bị giảm — key = mobId
        private static readonly Dictionary<int, long> _lastDecreaseTime = new Dictionary<int, long>();

        // Cửa sổ thời gian (ms) để coi "HP vừa giảm" còn hiệu lực
        private const long HP_CHANGE_WINDOW_MS = 2500;

        // Cửa sổ thời gian (ms): nếu mình attack trong khoảng này thì coi là MÌNH gây ra HP giảm
        private const long MY_ATTACK_WINDOW_MS = 2000;

        /// <summary>
        /// Gọi mỗi tick trong AutoTrain.Update() để cập nhật snapshot HP.
        /// </summary>
        public static void Tick()
        {
            if (GameScr.vMob == null) return;

            long now = Lib.TimeNow();

            for (int i = 0; i < GameScr.vMob.size(); i++)
            {
                var mob = GameScr.vMob.elementAt(i) as Mob;
                if (mob == null || mob.hp <= 0) continue;

                long prevHp;
                bool known = _lastHp.TryGetValue(mob.mobId, out prevHp);

                if (known && mob.hp < prevHp)
                {
                    // HP vừa giảm → ghi nhận thời điểm
                    _lastDecreaseTime[mob.mobId] = now;
                }

                _lastHp[mob.mobId] = mob.hp;
            }
        }

        /// <summary>
        /// Kiểm tra xem con quái này có đang bị NGƯỜI KHÁC đánh không.
        /// Logic: HP vừa giảm gần đây VÀ mình không attack nó trong cùng khoảng thời gian đó.
        /// </summary>
        /// <param name="mob">Con quái cần kiểm tra</param>
        /// <param name="myLastAttackTime">Thời điểm mình vừa attack (từ _lastAttackTime)</param>
        public static bool IsAttackedByOther(Mob mob, long myLastAttackTime)
        {
            if (mob == null) return false;

            long now = Lib.TimeNow();

            long decTime;
            if (!_lastDecreaseTime.TryGetValue(mob.mobId, out decTime))
                return false; // HP chưa từng giảm → chưa ai đánh

            // HP giảm quá lâu rồi → không còn tính
            if (now - decTime > HP_CHANGE_WINDOW_MS)
                return false;

            // Mình vừa attack gần đây → coi như MÌNH gây ra HP giảm
            if (now - myLastAttackTime < MY_ATTACK_WINDOW_MS)
                return false;

            // HP giảm gần đây + mình không attack → người khác đang đánh
            return true;
        }

        /// <summary>
        /// Xóa dữ liệu của một con quái (khi nó chết hoặc đổi khu).
        /// </summary>
        public static void Remove(int mobId)
        {
            _lastHp.Remove(mobId);
            _lastDecreaseTime.Remove(mobId);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu (khi đổi map).
        /// </summary>
        public static void Clear()
        {
            _lastHp.Clear();
            _lastDecreaseTime.Clear();
        }
    }
}