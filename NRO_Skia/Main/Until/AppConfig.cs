using System;
using System.IO;

public static class AppConfig
{
    public static int Port { get; private set; } = 9000;
    public static int Width { get; private set; } = 300;
    public static int Height { get; private set; } = 350;
    public static int FpsVisible { get; private set; } = 30;
    public static int FpsHidden { get; private set; } = 15;

    private static readonly string[] SearchPaths =
    {
        GameConstants.CONFIG_FILE_LOCAL,
        GameConstants.CONFIG_FILE,
    };

    public static void Load()
    {
        foreach (var path in SearchPaths)
        {
            if (!File.Exists(path)) continue;

            foreach (var line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith("#")) continue;
                var parts = line.Split('=');
                if (parts.Length != 2) continue;
                var key = parts[0].Trim().ToLower();
                var val = parts[1].Trim();
                if (key == "port" && int.TryParse(val, out int port)) Port = port;
                if (key == "width" && int.TryParse(val, out int width)) Width = width;
                if (key == "height" && int.TryParse(val, out int height)) Height = height;
                if (key == "fps_visible" && int.TryParse(val, out int fpsV)) FpsVisible = fpsV;
                if (key == "fps_hidden" && int.TryParse(val, out int fpsH)) FpsHidden = fpsH;
            }

            MergeDefault(path); // ← thêm dòng này, bổ sung key còn thiếu
            return;
        }

        CreateDefault();
    }

    private static void MergeDefault(string path)
    {
        try
        {
            var existing = File.ReadAllText(path);
            var append = "";
            if (!existing.Contains("port")) append += "port=9000\n";
            if (!existing.Contains("width")) append += "width=300\nheight=350\n";
            if (!existing.Contains("fps_visible")) append += "\n# FPS\nfps_visible=30\nfps_hidden=20\n";
            if (!string.IsNullOrEmpty(append))
                File.AppendAllText(path, append);
        }
        catch (Exception ex) { Console.WriteLine("[AppConfig] " + ex.Message); }
    }

    private static void CreateDefault()
    {
        var path = SearchPaths[0];
        try
        {
            var existing = File.Exists(path) ? File.ReadAllText(path) : "";
            if (!existing.Contains("fps_visible"))
                File.AppendAllText(path, "\n# FPS\nfps_visible=30\nfps_hidden=20\n");
            if (!existing.Contains("port"))
                File.AppendAllText(path, "port=9000\n");
            if (!existing.Contains("width"))
                File.AppendAllText(path, "width=300\nheight=350\n");
        }
        catch (Exception ex) { Console.WriteLine("[AppConfig] " + ex.Message); }
    }
}