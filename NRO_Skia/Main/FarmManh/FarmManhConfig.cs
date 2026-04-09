using System.Text.Json.Serialization;

public class FarmManhConfig
{
    public bool IsFarmAoQuan { get; set; }
    public bool IsFarmGang { get; set; }
    public bool IsFarmNhan { get; set; }
}

[JsonSerializable(typeof(FarmManhConfig))]
internal partial class FarmManhConfigContext : JsonSerializerContext { }
