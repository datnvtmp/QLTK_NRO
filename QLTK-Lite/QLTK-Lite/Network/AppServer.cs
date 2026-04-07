using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace QLTK_Lite.Network
{
    public static class AppServer
    {
        private static TcpListener _listener;
        private static bool _running;
        public static Action<GameInfo> OnReceived;
        
        public static event Action<int, string> OnCharInfo;
        public static event Action<int, string> OnDisconnect;
        public static event Action<int, string> OnDataInGame;
        public static event Action<int, string> OnInfo;
        public static event Action<int, string> OnHanhTrang;

        // ── CSKB EVENTS ──────────────────────────────────────────────────
        public static event Action<int, string> OnCSKBFull;
        public static event Action<int, string> OnCSKBReceiverId;
        public static event Action<int, string> OnCSKBReceiverFull;
        public static event Action<int, string> OnCSKBReceiverDone;

        private static readonly Dictionary<int, NetworkStream> _clients
            = new Dictionary<int, NetworkStream>();
        private static readonly object _clientLock = new object();

        public static void SendToClient(int id, string type, string payload = "")
        {
            lock (_clientLock)
            {
                if (!_clients.TryGetValue(id, out var stream)) return;
                try { SocketHelper.Send(stream, $"{type}|{payload}"); }
                catch { _clients.Remove(id); }
            }
        }

        private static readonly Dictionary<string, Action<int, string>> _handlers =
         new Dictionary<string, Action<int, string>>
         {
             [MsgType.CHAR_INFO] = (id, payload) => OnCharInfo?.Invoke(id, payload),
             [MsgType.DISCONNECT] = (id, payload) => OnDisconnect?.Invoke(id, payload),
             [MsgType.DATA_INGAME] = (id, payload) => OnDataInGame?.Invoke(id, payload),
             [MsgType.INFO] = (id, payload) => OnInfo?.Invoke(id, payload),
             [MsgType.HANHTRANG] = (id, payload) => OnHanhTrang?.Invoke(id, payload),

             // CSKB Handlers
             [MsgType.CSKB_FULL] = (id, payload) => OnCSKBFull?.Invoke(id, payload),
             [MsgType.CSKB_RECEIVER_ID] = (id, payload) => OnCSKBReceiverId?.Invoke(id, payload),
             [MsgType.CSKB_RECEIVER_FULL] = (id, payload) => OnCSKBReceiverFull?.Invoke(id, payload),
             [MsgType.CSKB_RECEIVER_DONE] = (id, payload) => OnCSKBReceiverDone?.Invoke(id, payload),
         };


        public static void Start(int port)
        {
            if (IsPortInUse(port))
            { MessageBox.Show("Cong " + port + " dang bi chiem!", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try { _listener = new TcpListener(IPAddress.Loopback, port); _listener.Start(); }
            catch (Exception ex)
            { MessageBox.Show("Khong the mo cong " + port + "!" + ex.Message, "Loi AppServer"); return; }
            _running = true;
            new Thread(AcceptLoop) { IsBackground = true, Name = "AppServer" }.Start();
            Logger.Log($"[AppServer] Da khoi dong tren cong {port}");
        }

        public static void Stop() { _running = false; _listener?.Stop(); }

        private static bool IsPortInUse(int port) =>
            IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(l => l.Port == port);

        private static void AcceptLoop()
        {
            while (_running)
                try
                {
                    var c = _listener.AcceptTcpClient();
                    new Thread(() => HandleClient(c))
                    {
                        IsBackground = true
                    }.Start();
                }
                catch { }
        }

        private static void HandleClient(TcpClient client)
        {
            int clientId = -1;
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream(); 
                while (_running)
                {
                    var msg = SocketHelper.Receive(stream);
                    if (msg == null) break;

                    var p = msg.Split('|');
                    if (p.Length < 2 || !int.TryParse(p[0], out int id)) continue;

                    if (clientId == -1)
                    {
                        clientId = id;
                        lock (_clientLock) _clients[id] = stream;
                    }

                    string type = p[1];
                    string payload = p.Length > 2 ? string.Join("|", p.Skip(2)) : "";

                    if (_handlers.TryGetValue(type, out var handler))
                        handler(id, payload);
                }
            }
            catch (Exception ex) { Logger.Log($"[HandleClient] lỗi: {ex.Message}"); }
            finally
            {
                stream?.Close();
                client?.Close();
                if (clientId != -1)
                {
                    lock (_clientLock) _clients.Remove(clientId);
                    OnDisconnect?.Invoke(clientId, "");
                }
            }
        }
    }
}
