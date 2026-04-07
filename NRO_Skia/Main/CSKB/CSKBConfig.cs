using System.Text.Json.Serialization;

public class CSKBConfig
{
    public bool IsCSKB { get; set; }
    public int CSKBMap { get; set; }
    public int CSKBZone { get; set; } = -1;
    public int CSKBType { get; set; } // 0: Up CSKB, 1: Chứa CSKB
}
[JsonSerializable(typeof(CSKBConfig))]
internal partial class CskbConfigContext : JsonSerializerContext { }