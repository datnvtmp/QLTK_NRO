public class BossConfig
{
    public bool IsSanBoss { get; set; }
    public string MapBoss { get; set; } = "";
    public int Group { get; set; } = 1;
    public string SkillBoss { get; set; } = "";
    public int TypeBoss { get; set; } = 0;  // 0=Đấm 1=Trói 2=Buff
    public bool UseDuoiHp { get; set; }
    public string DuoiHp { get; set; } = "";
    public int Delay { get; set; } = 500;
    public bool DoiHs { get; set; }
}