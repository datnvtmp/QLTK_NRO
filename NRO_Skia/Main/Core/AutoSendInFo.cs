using System;
using System.Text;

namespace NRO_Skia.Main.Core
{
    public class AutoSendInFo
    {
        private static AutoSendInFo _instance;
        private static long timeToSendInfo = 0L;
        private static long goldBase = -1L;
        private static long autoStartTime = 0L;

        // Lấy thời gian hiện tại dưới dạng Milliseconds (tương đương System.currentTimeMillis() trong Java)
        private static long CurrentTimeMillis => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static AutoSendInFo GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AutoSendInFo();
            }
            return _instance;
        }

        public static string SendInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("┌─────────────────────┐");
            sb.AppendLine($"│ Cập nhật lúc: {DateTime.Now:HH:mm:ss}");
            sb.AppendLine("├─── NHÂN VẬT ────────┤");
            sb.AppendLine($"│ Tên  : {Char.myCharz().cName}");
            sb.AppendLine($"│ Map  : {TileMap.mapID} - Khu: {TileMap.zoneID}");
            sb.AppendLine($"│ Vàng : {FormatPower(Char.myCharz().xu)}");
            sb.AppendLine($"│ Ngọc : {FormatPower(Char.myCharz().luong)}");
            sb.AppendLine($"│ Khóa : {FormatPower(Char.myCharz().luongKhoa)}");
            sb.AppendLine($"│ SM   : {FormatPower(Char.myCharz().cPower)}");
            if (goldBase != -1L)
            {
                long goldDiff = Char.myCharz().xu - goldBase;
                long elapsed = CurrentTimeMillis - autoStartTime;
                double hours = elapsed / 3600000.0;
                long goldPerHour = hours > 0 ? (long)(goldDiff / hours) : 0L;

                sb.AppendLine("├─── AUTO TRAIN ──────┤");
                sb.AppendLine($"│ Thời gian : {FormatTime(elapsed)}");
                sb.AppendLine($"│ Vàng lên  : +{FormatPower(goldDiff)}");
                sb.AppendLine($"│ Vàng/1h   : {FormatPower(goldPerHour)}");
            }

            sb.Append("└─────────────────────┘");
            return sb.ToString();
        }

        public static void Update()
        {
            if (Char.myCharz().cName != null)
            {
                string info = "[ THÔNG TIN ]\n - Tên Nhân Vật: "
                            + Char.myCharz().cName
                            + "\n - Map: "
                            + TileMap.mapID
                            + " - Khu: "
                            + TileMap.zoneID
                            + "\n - Vàng: "
                            + FormatPower(Char.myCharz().xu)
                            + "\n - Ngọc - Khóa: "
                            + Char.myCharz().luongStr
                            + " - "
                            + Char.myCharz().luongKhoaStr
                            + "\n - Sức mạnh: "
                            + FormatPower(Char.myCharz().cPower);

                if (goldBase != -1L)
                {
                    long goldNow = Char.myCharz().xu;
                    long goldDiff = goldNow - goldBase;
                    long elapsed = CurrentTimeMillis - autoStartTime;
                    double hours = elapsed / 3600000.0;
                    long goldPerHour = hours > 0.0 ? (long)(goldDiff / hours) : 0L;

                    info += "\n\n[ AUTO INFO ]\n - Thời gian train: "
                            + FormatTime(elapsed)
                            + "\n - Vàng up được: "
                            + FormatPower(goldDiff)
                            + "\n - Vàng 1h: "
                            + FormatPower(goldPerHour);
                }

                if (CurrentTimeMillis - timeToSendInfo > 5000L)
                {
                    timeToSendInfo = CurrentTimeMillis;
                    // Giả định Socket_Client đã được chuyển đổi sang C#
                    GameClient.Send("INFO:" + info);
                }
            }
        }

        /// <summary>
        /// Định dạng số (Ví dụ: 1000000 -> 1.000.000)
        /// </summary>
        public static string FormatPower(long n)
        {
            // Trong C#, cách nhanh nhất để định dạng dấu chấm phân cách là dùng ToString
            // Thay thế dấu phẩy mặc định bằng dấu chấm để giống logic Java của bạn
            return n.ToString("#,##0").Replace(',', '.');
        }

        public static void StartAuto()
        {
            goldBase = Char.myCharz().xu;
            autoStartTime = CurrentTimeMillis;
        }
        public static string SendHanhTrang()
        {
            var bag = Char.myCharz().arrItemBag;
            if (bag == null) return "[]";
            var sb = new System.Text.StringBuilder("[");
            bool first = true;
            for (int i = 0; i < bag.Length; i++)
            {
                var item = bag[i];
                if (item == null) continue;
                if (!first) sb.Append(",");
                first = false;
                string name = (item.template?.name ?? "").Replace("\"", "'");
                sb.Append("{")
                  .Append($"\"id\":{item.template?.id ?? -1},")
                  .Append($"\"iconID\":{item.template?.iconID ?? -1},")
                  .Append($"\"name\":\"{name}\",")
                  .Append($"\"quantity\":{item.quantity}")
                  .Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }
        public static long GetGoldBase()
        {
            return goldBase;
        }

        public static string FormatTime(long ms)
        {
            long sec = ms / 1000L;
            long h = sec / 3600L;
            long m = (sec % 3600L) / 60L;
            long s = sec % 60L;
            return $"{h}h {m}m {s}s";
        }
    }
}