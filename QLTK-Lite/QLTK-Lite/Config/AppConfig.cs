using System;
using System.IO;

namespace QLTK_Lite.Config
{
    public static class AppConfig
    {
        public static int Port { get; private set; } = 9000;

        private static readonly string[] SearchPaths =
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "config.txt"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt"),
        };

        public static void Load()
        {
            foreach (var path in SearchPaths)
            {
                if (!File.Exists(path)) continue;
                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;
                    var p = line.Split('=');
                    if (p.Length == 2 && p[0].Trim().ToLower() == "port" && int.TryParse(p[1].Trim(), out int port))
                        Port = port;
                }
                return;
            }
            CreateDefault();
        }

        private static void CreateDefault()
        {
            var path = SearchPaths[0];
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, new[] { "# Cong ket noi", "port=9000" });
            }
            catch (Exception ex) { Console.WriteLine("[AppConfig] " + ex.Message); }
        }
    }
}
