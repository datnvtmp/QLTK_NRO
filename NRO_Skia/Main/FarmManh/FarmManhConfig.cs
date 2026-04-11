using System.Text.Json.Serialization;

public class FarmManhConfig
{
    public bool IsFarmAoQuan { get; set; }
    public int QuantityAoQuan { get; set; }
    public bool IsFarmGang { get; set; }
    public int QuantityGang { get; set; }
    public bool IsFarmNhan { get; set; }
    public int QuantityNhan { get; set; }
}

[JsonSerializable(typeof(FarmManhConfig))]
internal partial class FarmManhConfigContext : JsonSerializerContext { }
