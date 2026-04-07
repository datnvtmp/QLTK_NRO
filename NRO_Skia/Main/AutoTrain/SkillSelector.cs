using System.Collections.Generic;

namespace Mod
{
    public static class SkillSelector
    {
        public static long TimeToBuffHS { get; set; } = 0L;
        public static long LastMonkeyEndTime { get; set; } = 0L;
        public static bool WasMonkey { get; set; } = false;

        // =====================================================================
        // PUBLIC
        // =====================================================================
        public static Skill ChooseSkill()
        {
            bool hasSkill17 = IsSkillEnabled(17);
            Skill best = null;

            foreach (var skill in EnumerateSkills())
            {
                if (!IsSkillReady(skill)) continue;
                if (!CanUseSkill(skill, hasSkill17)) continue;
                if (!HasEnoughMana(skill)) continue;

                if (best == null || skill.coolDown > best.coolDown)
                    best = skill;
            }

            return best;
        }

        // =====================================================================
        // PRIVATE
        // =====================================================================
        private static bool IsSkillReady(Skill skill)
        {
            return Lib.TimeNow() - skill.lastTimeUseThisSkill >= skill.coolDown;
        }

        private static bool CanUseSkill(Skill skill, bool hasSkill17)
        {
            int id = skill.template.id;

            if (hasSkill17 && id == 2) return false; // skill 17 thay thế skill 2
            if (!IsSkillEnabled(id)) return false; // không bật trong danh sách
            if (skill.paintCanNotUseSkill) return false; // đang bị khoá

            //if (id == 25 && !AutoSkill.CheckCoolDown(skill)) return false;
            if (id == 7 && Lib.TimeNow() - TimeToBuffHS < skill.coolDown + 2000) return false;
            if(id == 13 && Char.myCharz().isMonkey == 1) return false;
            return true;
        }

        private static bool HasEnoughMana(Skill skill)
        {
            var me = Char.myCharz();
            long required = skill.template.manaUseType switch
            {
                2 => 1L,
                1 => skill.manaUse * me.cMPFull / 100L,
                _ => skill.manaUse
            };
            return me.cMP >= required;
        }

        private static bool IsSkillEnabled(int skillId)
        {
            var trains = SkillTrain.SkillTrains;
            if (trains == null) return false; // ← guard null
            for (int i = 0; i < trains.Length; i++)
                if (trains[i]?.Id == skillId && trains[i].AutoFlag)
                    return true;
            return false;
        }

        private static IEnumerable<Skill> EnumerateSkills()
        {
            var v = Char.myCharz().vSkill;
            if (v == null) yield break;
            for (int i = 0; i < v.size(); i++)
                yield return (Skill)v.elementAt(i);
        }
    }
}