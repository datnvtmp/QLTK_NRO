using Newtonsoft.Json;
using System;

namespace QLTK_Lite.Models
{
    public class AccountModel
    {
        public bool IsSelected { get; set; }
        public int ID { get; set; }
        public string CharName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string Proxy { get; set; }
        public AccConfig Config { get; set; } = new AccConfig();
        public BossConfig BossConfig { get; set; } = new BossConfig();
        public CSKBConfig CSKBConfig { get; set; } = new CSKBConfig();
        public FarmManhConfig FarmManhConfig { get; set; } = new FarmManhConfig();

        [JsonIgnore] public string DataInGame { get; set; } = "-";
        [JsonIgnore] public string Status { get; set; } = "-";
        [JsonIgnore] public string LastInfo { get; set; } = "";
        [JsonIgnore] public DateTime? TrainStartTime { get; set; }

        public bool CheckSchedule(DateTime now, out bool shouldOpen, out bool shouldKill)
        {
            shouldOpen = false;
            shouldKill = false;

            if (Config == null) return false;

            // Tự ON
            if (Config.UseAutoOn && now.Hour == Config.AutoOnH && now.Minute == Config.AutoOnM && !IsSelected)
            {
                shouldOpen = true;
                TrainStartTime = now;
            }

            // Tự OFF theo giờ cố định
            if (Config.UseAutoOff && now.Hour == Config.AutoOffH && now.Minute == Config.AutoOffM && IsSelected)
                shouldKill = true;

            // OFF sau X giờ Y phút
            if (Config.UseOffSau && IsSelected && TrainStartTime.HasValue)
            {
                var limit = TimeSpan.FromHours(Config.AutoOffSauH) + TimeSpan.FromMinutes(Config.AutoOffSauM);
                if (now - TrainStartTime.Value >= limit)
                {
                    shouldKill = true;
                    TrainStartTime = null;
                }
            }

            return shouldOpen || shouldKill;
        }
    }
}