using System;
using System.IO;

public static class Logger
{
    private static readonly string _path = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "app.log");
    private static readonly object _lock = new object();

    public static void Log(string msg)
    {
        lock (_lock)
            File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");
    }
}