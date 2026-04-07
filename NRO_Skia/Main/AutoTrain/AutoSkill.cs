using System;

namespace Mod
{
    public static class AutoSkill
    {
        public static bool IsLoadKeySkill { get; set; } = true;
        public static bool IsAutoSendAttack { get; set; }
        public static long[] LastTimeSendAttack = new long[10];

        // =====================================================================
        // PUBLIC
        // =====================================================================

        public static void AutoSendAttack()
        {
            var me = Char.myCharz();

            if (me.meDead) return;
            if (me.cHP <= 0) return;
            if (me.statusMe == 14) return;
            if (me.statusMe == 5) return;
            if (me.myskill.template.type == 3) return;
            if (me.myskill.template.id is 10 or 11) return;
            if (me.myskill.paintCanNotUseSkill && !GameCanvas.panel.isShow) return;

            int index = GetMySkillIndex();
            if (Lib.TimeNow() - LastTimeSendAttack[index] <= GetCoolDown(me.myskill)) return;
            if (!GameScr.gI().isMeCanAttackMob(me.mobFocus)) return;

            me.myskill.lastTimeUseThisSkill = Lib.TimeNow();
            SendAttackToMobFocus();
            LastTimeSendAttack[index] = Lib.TimeNow();
        }

        public static bool CheckCoolDown(Skill skill)
        {
            return Lib.TimeNow() - LastTimeSendAttack[GetMySkillIndex()] > skill.coolDown + 7000;
        }

        public static void SendAttackToMobFocus()
        {
            try
            {
                var targets = new MyVector();
                targets.addElement(Char.myCharz().mobFocus);
                Service.gI().sendPlayerAttack(targets, new MyVector(), -1);

                var me = Char.myCharz();
                me.cMP -= me.myskill.manaUse;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // =====================================================================
        // PRIVATE
        // =====================================================================

        // Skill có delay thêm 500ms khi dùng
        private static readonly int[] EXTRA_DELAY_SKILLS = { 20, 22, 7, 18, 23 };

        private static long GetCoolDown(Skill skill)
        {
            foreach (int id in EXTRA_DELAY_SKILLS)
                if (skill.template.id == id) return skill.coolDown + 500L;
            return skill.coolDown;
        }

        private static int GetMySkillIndex()
        {
            var mySkill = Char.myCharz().myskill;
            var keys = GameScr.keySkill;
            for (int i = 0; i < keys.Length; i++)
                if (keys[i] == mySkill) return i;
            return 0;
        }
    }
}