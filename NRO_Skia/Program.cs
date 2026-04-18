using NRO_Skia.Main.AutoTrain;
using NRO_Skia.Main.Core;
using NRO_Skia.Main.CSKB;
using NRO_Skia.Main.FarmManh;
using System.Runtime;
using System.Text.Json; // thay Newtonsoft
using SkiaSharp;
using System.IO;

namespace NRO_Skia;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Win32Window.InitializeDpiAwareness();
        // 1. Cài đặt bộ bắt lỗi toàn cục
        AppDomain.CurrentDomain.UnhandledException += (s, e) => LogCrash(e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (s, e) => LogCrash(e.Exception);

        try
        {
            Console.WriteLine("App started!");

            // Kiểm tra Skia sớm để bắt lỗi DLL ngay lập tức
            try { var test = new SKImageInfo(1, 1); Console.WriteLine("SkiaSharp DLL loaded OK."); }
            catch (Exception ex) { LogCrash(new Exception("SkiaSharp native library failed to load!", ex)); }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(2, GCCollectionMode.Forced, true, true);

            Account.Load(args);

            bool startHidden = Array.IndexOf(args, "--hidden") >= 0;
            string title = "NRO";
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "--title") title = args[i + 1];

            AppConfig.Load();

            if (Account.HasAccount)
            {
                GameClient.Start(int.Parse(Account.Id), AppConfig.Port);

                GameClient.OnReceived += (type, payload) =>
                {
                    if (type == MsgType.CONFIG)
                    {
                        var config = JsonSerializer.Deserialize(payload, AccConfigContext.Default.AccConfig);
                        AutoTrain.Config = config;
                        AutoTrain.ResetApplied();
                    }
                    if (type == MsgType.GET_INFO)
                    {
                        string info = AutoSendInFo.SendInfo();
                        GameClient.SendNow(MsgType.INFO, info);
                    }
                    if (type == MsgType.CMD_HANHTRANG)
                    {
                        string json = AutoSendInFo.SendHanhTrang();
                        GameClient.SendNow(MsgType.HANHTRANG, json);
                    }
                    if (type == MsgType.CSKB_CONFIG)
                    {
                        var config = JsonSerializer.Deserialize(payload, CskbConfigContext.Default.CSKBConfig);
                        AutoCSKB.Config = config;
                        AutoCSKB.ResetApplied();
                    }
                    if (type == MsgType.FARM_MANH_CONFIG)
                    {
                        var config = JsonSerializer.Deserialize(payload, FarmManhConfigContext.Default.FarmManhConfig);
                        AutoFarmManh.Config = config;
                        AutoFarmManh.ResetApplied();
                    }
                };
            }

            Win32Window.Run(AppConfig.Width, AppConfig.Height, title, startHidden);
            GameClient.Stop();
        }
        catch (Exception ex)
        {
            LogCrash(ex);
            throw;
        }
    }

    private static long _lastLogTime;

    internal static void LogCrash(object? ex)
    {
        try
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now - _lastLogTime < 1000) return; // Cooldown 1s để tránh treo UI do ghi file quá nhiều
            _lastLogTime = now;

            string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRASH REPORT:\n{ex}\n\n";
            File.AppendAllText("crash.log", msg);
            Console.WriteLine("CRASH! Log updated in crash.log");
        }
        catch { }
    }
}
