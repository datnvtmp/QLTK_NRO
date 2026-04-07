namespace QLTK_Lite.Models
{
    public class CSKBConfig
    {
        public bool IsCSKB { get; set; }
        public int CSKBMap { get; set; }
        public int CSKBZone { get; set; } = 0;
        public int CSKBType { get; set; } // 0: Up CSKB, 1: Chứa CSKB
        public bool IsFull { get; set; }
    }
}
