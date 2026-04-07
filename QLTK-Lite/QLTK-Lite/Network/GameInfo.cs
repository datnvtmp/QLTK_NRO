using System;

namespace QLTK_Lite.Network
{
    public class GameInfo
    {
        public int ID { get; set; }
        public string Type { get; set; } // ← thêm
        public string Payload { get; set; } // ← thêm

        // Giữ lại để không break code cũ
        public string CharName { get; set; }
        public string DataInGame { get; set; }
        public string Status { get; set; }

        // Helper tách payload
        public string[] Parts =>
            Payload?.Split('|') ?? Array.Empty<string>();
    }
}