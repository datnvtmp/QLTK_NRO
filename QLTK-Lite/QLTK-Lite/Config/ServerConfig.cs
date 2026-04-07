using System;
using System.IO;
using System.Linq;

namespace QLTK_Lite.Config
{
    public static class ServerConfig
    {
        private static readonly string[] Defaults = { "Super 1", "Super 2", "Super 3" };

        private static string FilePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "lib", "servers.txt");

        public static string[] LoadServers()
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllLines(FilePath, Defaults);
                return Defaults;
            }
            var lines = File.ReadAllLines(FilePath)
                .Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
            if (lines.Length == 0) { File.WriteAllLines(FilePath, Defaults); return Defaults; }
            return lines;
        }
    }
}
