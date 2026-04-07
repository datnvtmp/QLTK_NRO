using System;
using System.IO;

public static class Logger
{
    private static readonly string _path;
    private static readonly object _lock = new object();

    static Logger()
    {
        var dir = Path.GetDirectoryName(Environment.ProcessPath)
                  ?? Directory.GetCurrentDirectory();
        _path = Path.Combine(dir, "game_debug.log");
    }

    public static void Log(string msg)
    {
        // In ra console trước để chắc chắn dữ liệu không bị mất nếu ghi file lỗi
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {msg}");

        lock (_lock)
        {
            try
            {
                File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");
            }
            catch (IOException)
            {
                // File đang bị process khác chiếm, bỏ qua lần ghi này để không crash
                // Bạn có thể chọn ghi tạm ra một biến string chờ lần sau ghi bù
            }
            catch (Exception)
            {
                // Các lỗi bảo mật hoặc hệ thống khác
            }
        }
    }
}