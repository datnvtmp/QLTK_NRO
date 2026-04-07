using System;
using System.Net.Sockets;
using System.Threading;

public static class GameClient
{
    private static int _gameId;
    private static int _port;
    private static TcpClient _client;
    private static NetworkStream _stream;
    private static bool _running;

    public static event Action<string, string> OnReceived; // type, payload

    // Cache để chỉ gửi khi data thay đổi
    private static string _lastCharName = null;
    private static string _lastDataInGame = null;
    private static string _lastStatus = null;
    private static string _lastMsg = null;

    // Biến lưu thời gian gửi lần cuối (tính bằng mili-giây)
    private static long _lastSendTime = 0;

    private static readonly object _sendLock = new object();
    private static System.Threading.Timer _heartbeatTimer;

    // ── Start ─────────────────────────────────────────────────────────────────
    public static void Start(int id, int port)
    {
        _gameId = id;
        _port = port;
        _running = true;

        var t = new Thread(ConnectLoop)
        {
            IsBackground = true,
            Name = "GameClient"
        };
        t.Start();

        _heartbeatTimer = new System.Threading.Timer(
            _ => Heartbeat(), null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30));
    }

    public static void Stop()
    {
        _running = false;
        _heartbeatTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _heartbeatTimer?.Dispose();
        _client?.Close();
    }
    public static void Send(string msg)
    {
        long now = Environment.TickCount64;
        if (msg == _lastMsg || now - _lastSendTime < 1000) return;

        _lastSendTime = now;
        _lastMsg = msg;
        TrySend($"{_gameId}|{msg}");
    }

    public static void Send(string type, string payload = "")
    {
        long now = Environment.TickCount64;
        if (now - _lastSendTime < 1000) return;

        _lastSendTime = now;
        TrySend($"{_gameId}|{type}|{payload}");
    }

    public static void SendNow(string type, string payload = "")
    {
        TrySend($"{_gameId}|{type}|{payload}");
    }
    // ── Heartbeat — gửi lại data hiện tại dù không đổi ───────────────────────
    private static void Heartbeat()
    {
        if (_lastCharName == null) return;

        // Không áp dụng rate-limit 1s cho heartbeat vì nó chạy 30s/lần
        TrySend($"{_gameId}|{_lastCharName}|{_lastDataInGame}|{_lastStatus}");
    }

    // ── Gửi thật sự ──────────────────────────────────────────────────────────
    private static void TrySend(string msg)
    {
        lock (_sendLock)
        {
            try
            {
                if (_stream == null) return;
                SocketHelper.Send(_stream, msg);
            }
            catch
            {
                _stream = null;
                _client?.Close();
            }
        }
    }

    // ── Reconnect loop với backoff tăng dần ──────────────────────────────────
    private static void ConnectLoop()
    {
        int delay = 1000;

        while (_running)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect("127.0.0.1", _port);
                lock (_sendLock) _stream = _client.GetStream();

                Logger.Log($"[GameClient] Connected id={_gameId} port={_port}");
                new Thread(ReceiveLoop) { IsBackground = true }.Start();

                delay = 1000;
                Console.WriteLine($"[GameClient] Connected to app (ID={_gameId})");

                while (_running && _client.Connected)
                    Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                lock (_sendLock) _stream = null;
                _client?.Close();
                Logger.Log($"[GameClient] Retry in {delay / 1000}s lỗi: {ex.Message}");
                Thread.Sleep(delay);
                delay = Math.Min(delay * 2, 30000);
            }
        }
    }

    private static void ReceiveLoop()
    {
        Logger.Log("[ReceiveLoop] bắt đầu");

        while (_running && _client != null && _client.Connected)
        {
            try
            {
                var msg = SocketHelper.Receive(_stream);
                if (msg == null) break;
                var idx = msg.IndexOf('|');
                var type = idx >= 0 ? msg.Substring(0, idx) : msg;
                var payload = idx >= 0 ? msg.Substring(idx + 1) : "";

                OnReceived?.Invoke(type, payload);
            }
            catch (Exception ex)
            {
                Logger.Log($"[ReceiveLoop] exception: {ex.Message}");
                break;
            }
        }
        Logger.Log("[ReceiveLoop] kết thúc");
    }
}