using NRO_Skia.Main.AutoTrain;
using NRO_Skia.Main.Core;
using NRO_Skia.Main.CSKB;
using System.Runtime;
using System.Text.Json; // thay Newtonsoft

namespace NRO_Skia;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("App started!");

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

            // ← Thêm vào đây
            GameClient.OnReceived += (type, payload) =>
            {
                if (type == MsgType.CONFIG)
                {
                    var config = JsonSerializer.Deserialize(payload, AccConfigContext.Default.AccConfig);
                    AutoTrain.Config = config; // ← chỉ lưu
                    AutoTrain.ResetApplied(); // ← báo cần apply lại
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
                if(type == MsgType.CSKB_CONFIG)
                {
                    var config = JsonSerializer.Deserialize(payload, CskbConfigContext.Default.CSKBConfig);
                    AutoCSKB.Config = config;
                    AutoCSKB.ResetApplied();
                }
            };
        }

        Win32Window.Run(AppConfig.Width, AppConfig.Height, title, startHidden);
        GameClient.Stop();
    }
}