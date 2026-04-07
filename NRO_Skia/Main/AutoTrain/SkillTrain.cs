namespace Mod
{
    public class SkillTrain
    {
        // ── Skill mặc định bật auto ───────────────────────────────────────────
        private static readonly int[] DEFAULT_AUTO_IDS = { 0, 4, 13, 17 };
        public string Name { get; }
        public int Id { get; }
        public bool AutoFlag { get; set; }

        public static SkillTrain[] SkillTrains { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        public SkillTrain(string name, int id, bool autoFlag)
        {
            Name = name;
            Id = id;
            AutoFlag = autoFlag;
        }

        // ── Khởi tạo danh sách từ skill của nhân vật ─────────────────────────
        public static void Init()
        {
            var vSkill = Char.myCharz().vSkill;
            int count = vSkill.size();

            SkillTrains = new SkillTrain[count];

            for (int i = 0; i < count; i++)
            {
                var skill = (Skill)vSkill.elementAt(i);
                bool autoOn = Array.IndexOf(DEFAULT_AUTO_IDS, skill.template.id) >= 0;
                SkillTrains[i] = new SkillTrain(skill.template.name, skill.template.id, autoOn);
            }
        }
    }
}