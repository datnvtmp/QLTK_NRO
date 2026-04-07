using System.Text.Json.Serialization;

public class AccConfig
{
    public bool IsTrain { get; set; }
    public int MapIndex { get; set; }
    public bool IsKhu { get; set; }
    public int Khu { get; set; }
    public int LoaiQuai { get; set; }
    public string IdQuai { get; set; } = "";
    public string ListSkill { get; set; } = "";

    // ── Trên / Dưới HP ──
    public bool UseTrenHp { get; set; }
    public string TrenHp { get; set; } = "";

    public bool UseDuoiHp { get; set; }
    public string DuoiHp { get; set; } = "";
    public string IDMobLoaiTru { get; set; } = "";
    public string IdSl { get; set; } = "";

    // ── Item ──
    public bool UseIdDung { get; set; }
    public string IdDung { get; set; } = "";

    public bool UseIdVut { get; set; }
    public string IdVut { get; set; } = "";

    // ── Né Char ──
    public bool UseNeChar { get; set; }
    public string NeChar { get; set; } = "";

    // ── Linh Tính ──
    public bool BongTai { get; set; }
    public bool CoDen { get; set; }
    public bool DoiHs { get; set; }
    public bool MuaTdlt { get; set; }
    public bool PemMatMau { get; set; }
    public bool PemFullMau { get; set; }
    public bool UpSkh { get; set; }
    public bool DungIm { get; set; }
    public bool UpGiap { get; set; }
    public bool IsAntiKS { get; set; }

    // ── Lịch ──
    public bool UseAutoOn { get; set; }
    public int AutoOnH { get; set; }
    public int AutoOnM { get; set; }

    public bool UseAutoOff { get; set; }
    public int AutoOffH { get; set; }
    public int AutoOffM { get; set; }

    public bool UseOffSau { get; set; }
    public int AutoOffSauH { get; set; }
    public int AutoOffSauM { get; set; }

}

[JsonSerializable(typeof(AccConfig))]
internal partial class AccConfigContext : JsonSerializerContext { }