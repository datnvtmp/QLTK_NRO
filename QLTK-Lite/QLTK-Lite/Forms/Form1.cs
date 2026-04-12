using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using QLTK_Lite.Config;
using QLTK_Lite.Models;
using QLTK_Lite.Network;
using QLTK_Lite.Services;
using QLTK_Lite.Storage;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        internal BindingList<AccountModel> accountList;
        internal TextBox txtTaiKhoan;
        internal TextBox txtPass;
        internal ComboBox cboServer;
        internal TextBox txtProxy;

        internal RichTextBox _logBox;
        private SplitContainer _splitBottom;
        private TabPage _tabAutoTrain;
        private TabPage _tabFarmManh;
        private TabPage _tabCSKB;

        // ── CSKB Queue System ──
        private readonly Queue<int> _cskbUpQueue = new Queue<int>(); // Bot Up IDs chờ giao dịch
        private int _cskbActiveSenderId = -1;   // Bot Up đang giao dịch
        private int _cskbActiveReceiverId = -1;  // Bot Nhận đang giao dịch

        private readonly BotService _botService = new BotService();
        // ← THÊM MỚI
        private readonly System.Windows.Forms.Timer _refreshTimer
            = new System.Windows.Forms.Timer { Interval = 1000 };
        private System.Windows.Forms.Timer _scheduleTimer;
        public Form1()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.app; // Dùng luôn, không cần .ico kèm theo
            if (Updater.HasNewVersion)
                this.Text = $"QLTK {Updater.CURRENT_VERSION} — 🔔 Có phiên bản mới {Updater.LatestVersion}!";
            else
                this.Text = $"QLTK {Updater.CURRENT_VERSION}";
            AppConfig.Load();
            accountList = AccountStorage.LoadAccounts();
            ReindexIds();
            SetupUI();
            EnableDoubleBuffering(dgvAccounts);
            dgvAccounts.CellMouseDown += DgvAccounts_CellMouseDown;
            dgvAccounts.DataSource = accountList;
            FormatColumns();
            CreateLayout();
            CreateContextMenu();
            dgvAccounts.SelectionChanged += DgvAccounts_SelectionChanged;
            _botService.SetAccountSource(() => accountList);
            // ← SỬA: gộp OnStarted/OnStopped kèm timer
            _botService.OnStarted += () => { OnBotStarted(); _refreshTimer.Start(); };
            _botService.OnStopped += () => { OnBotStopped(); _refreshTimer.Stop(); };

            // ← THÊM: timer tick chỉ refresh grid
            _refreshTimer.Tick += (s, e) => dgvAccounts.Refresh();
            // ← Đăng ký thêm events mới
            AppServer.OnDisconnect += OnDisconnectReceived;
            AppServer.OnDataInGame += OnDataInGameReceived; // ← đăng ký
            AppServer.OnCharInfo += OnCharInfoReceived; // giữ event cũ, không break Form1
            AppServer.OnHanhTrang += OnHanhTrangReceived;
            InitCSKBHandlers();

            AppServer.OnInfo += (id, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    var acc = accountList.FirstOrDefault(a => a.ID == id);
                    if (acc != null) acc.LastInfo = payload;

                    var selected = dgvAccounts.SelectedRows
                        .Cast<DataGridViewRow>()
                        .Select(r => r.DataBoundItem as AccountModel)
                        .FirstOrDefault();

                    if (selected?.ID == id)
                        ShowInfo(payload);
                }));
            };



            AppServer.Start(AppConfig.Port);

            // ✅ Thêm vào đây — sau CreateLayout
            this.Load += (s, e) =>
            {
                _splitBottom.SplitterDistance = _splitBottom.Width - RIGHT_PANEL_WIDTH - 4;
            };
            this.Resize += (s, e) =>
            {
                if (_splitBottom != null)
                    _splitBottom.SplitterDistance = _splitBottom.Width - RIGHT_PANEL_WIDTH - 4;
            };

            InitScheduleTimer(); // ← thêm vào đây


            this.FormClosed += (s, e) =>
            {
                _refreshTimer.Stop(); // ← dừng timer khi đóng app
                AppServer.Stop();
                _botService.Dispose();
            };
        }

        
        private void OnDisconnectReceived(int id, string payload)
        {
            this.Invoke((Action)(() =>
            {
                var acc = accountList.FirstOrDefault(a => a.ID == id);
                if (acc == null) return;
                acc.Status = "Offline";
                acc.DataInGame = "";

                // Nếu bot Up disconnect khi đang trong queue → xóa khỏi queue
                if (_cskbUpQueue.Contains(id))
                {
                    var temp = new Queue<int>(_cskbUpQueue.Where(x => x != id));
                    _cskbUpQueue.Clear();
                    foreach (var x in temp) _cskbUpQueue.Enqueue(x);
                    Logger.Log($"[CSKB] Acc Up {acc.Username} disconnect, da xoa khoi queue.");
                }

                // Nếu bot Up disconnect khi đang trade → hủy trade, xử lý queue tiếp
                if (_cskbActiveSenderId == id)
                {
                    Logger.Log($"[CSKB] Acc Up {acc.Username} disconnect khi dang trade. Huy trade.");
                    _cskbActiveSenderId = -1;
                    _cskbActiveReceiverId = -1;
                    ProcessCSKBQueue();
                }
            }));
        }
        private void OnHanhTrangReceived(int id, string payload)
        {
            this.Invoke((Action)(() =>
            {
                var acc = accountList.FirstOrDefault(a => a.ID == id);
                string accName = acc?.Username ?? $"ID {id}";
                new HanhTrangForm(accName, payload).Show();
            }));
        }
        private void OnCharInfoReceived(int id, string payload)
        {
            this.Invoke((Action)(() =>
            {
                var acc = accountList.FirstOrDefault(a => a.ID == id);
                if (acc == null) return;
                acc.CharName = payload;
                acc.Status = "Online";
                acc.DataInGame = "Character loaded";
                AppServer.SendToClient(acc.ID, MsgType.CONFIG, JsonConvert.SerializeObject(acc.Config));
                AppServer.SendToClient(acc.ID, MsgType.BOSS_CONFIG, JsonConvert.SerializeObject(acc.BossConfig));
                AppServer.SendToClient(acc.ID, MsgType.CSKB_CONFIG, JsonConvert.SerializeObject(acc.CSKBConfig));
                AppServer.SendToClient(acc.ID, MsgType.FARM_MANH_CONFIG, JsonConvert.SerializeObject(acc.FarmManhConfig));
            }));
        }

        private void OnDataInGameReceived(int id, string payload)
        {
            this.Invoke((Action)(() =>
            {
                var acc = accountList.FirstOrDefault(a => a.ID == id);
                if (acc == null) return;
                acc.DataInGame = payload;
            }));
        }

        private void InitCSKBHandlers()
        {
            // ── Bot Up báo đã đầy 99 CSKB ──
            AppServer.OnCSKBFull += (senderId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    // Không thêm trùng: đã trong queue hoặc đang trade
                    if (_cskbUpQueue.Contains(senderId) || _cskbActiveSenderId == senderId)
                        return;

                    var sender = accountList.FirstOrDefault(a => a.ID == senderId);
                    _cskbUpQueue.Enqueue(senderId);
                    Logger.Log($"[CSKB] Acc Up {sender?.Username} vao hang doi (queue: {_cskbUpQueue.Count})");
                    ProcessCSKBQueue();
                }));
            };

            // ── Bot Nhận gửi charID về ──
            AppServer.OnCSKBReceiverId += (receiverId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    if (_cskbActiveReceiverId == receiverId && _cskbActiveSenderId != -1)
                    {
                        AppServer.SendToClient(_cskbActiveSenderId, MsgType.CSKB_RECEIVER_ID, payload);
                        Logger.Log($"[CSKB] Da gui ID {payload} tu acc nhan {receiverId} cho acc up {_cskbActiveSenderId}");
                    }
                }));
            };

            // ── Bot Nhận báo rương đầy ──
            AppServer.OnCSKBReceiverFull += (receiverId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    var acc = accountList.FirstOrDefault(a => a.ID == receiverId);
                    if (acc != null)
                    {
                        acc.CSKBConfig.IsFull = true;
                        acc.IsSelected = false;
                        _botService.KillProcess(acc.ID);
                        dgvAccounts.Refresh();
                        SaveAccountsToFile();
                        Logger.Log($"[CSKB] Acc nhan {acc.Username} da bao FULL!");
                    }

                    if (_cskbActiveReceiverId == receiverId)
                    {
                        int failedSenderId = _cskbActiveSenderId;
                        _cskbActiveSenderId = -1;
                        _cskbActiveReceiverId = -1;

                        // Bỏ logic re-queue manual: Giao dịch thực tế đã XONG thì bot nhận mới đi cất và báo FULL.
                        // Nếu acc Up vẫn còn >= 99 đồ, cơ chế heartbeat 10s của nó sẽ tự báo CSKB_FULL để QLTK gọi lại.
                        // Nếu ép re-queue ở đây, acc Up đã hết đồ sẽ đứng im, làm kẹt acc nhận mới.
                        ProcessCSKBQueue();
                    }
                }));
            };

            // ── Bot Nhận cất đồ xong ──
            AppServer.OnCSKBReceiverDone += (receiverId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    var acc = accountList.FirstOrDefault(a => a.ID == receiverId);
                    if (acc != null)
                    {
                        acc.IsSelected = false;
                        _botService.KillProcess(acc.ID);
                        dgvAccounts.Refresh();
                        Logger.Log($"[CSKB] Acc nhan {acc.Username} da cat xong do va TU DONG TAT.");
                    }

                    if (_cskbActiveReceiverId == receiverId)
                    {
                        _cskbActiveSenderId = -1;
                        _cskbActiveReceiverId = -1;
                        ProcessCSKBQueue(); // Xử lý bot Up tiếp theo trong queue
                    }
                }));
            };
        }

        /// <summary>Xử lý queue: ghép bot Up tiếp theo với acc nhận rảnh</summary>
        private void ProcessCSKBQueue()
        {
            // Đang có giao dịch active → chờ
            if (_cskbActiveSenderId != -1) return;
            if (_cskbUpQueue.Count == 0) return;

            // Bot service chưa chạy → chờ user bấm nút Bắt đầu
            if (!_botService.IsRunning)
            {
                Logger.Log($"[CSKB] Bot chua chay! {_cskbUpQueue.Count} acc Up dang doi. Hay bam nut Bat dau.");
                return;
            }

            // Tìm acc nhận rảnh (offline, chưa full)
            var receiver = accountList.FirstOrDefault(a =>
                a.CSKBConfig.IsCSKB &&
                a.CSKBConfig.CSKBType == 1 &&
                !a.CSKBConfig.IsFull &&
                (a.Status == "Offline" || a.Status == "-" || string.IsNullOrEmpty(a.Status)));

            if (receiver == null)
            {
                Logger.Log($"[CSKB] Khong co acc nhan nao! {_cskbUpQueue.Count} acc Up dang doi.");
                return; // Giữ trong queue, sẽ xử lý khi có acc nhận rảnh (DONE)
            }

            int senderId = _cskbUpQueue.Dequeue();
            var sender = accountList.FirstOrDefault(a => a.ID == senderId);

            _cskbActiveSenderId = senderId;
            _cskbActiveReceiverId = receiver.ID;
            receiver.IsSelected = true; // BotService.RunLoop sẽ tự detect và mở acc này
            dgvAccounts.Refresh();
            Logger.Log($"[CSKB] Ghep: Up [{sender?.Username}] <-> Nhan [{receiver.Username}] (con {_cskbUpQueue.Count} trong queue)");
        }


        internal void ReindexIds()
        {
            for (int i = 0; i < accountList.Count; i++)
                accountList[i].ID = i + 1;
            SaveAccountsToFile();
        }

        private void InitScheduleTimer()
        {
            _scheduleTimer = new System.Windows.Forms.Timer { Interval = 15_000 };
            _scheduleTimer.Tick += (s, e) =>
            {
                var now = DateTime.Now;
                foreach (var acc in accountList)
                {
                    if (!acc.CheckSchedule(now, out bool open, out bool kill)) continue;

                    if (open)
                    {
                        acc.IsSelected = true;
                        dgvAccounts.Refresh(); // cập nhật UI
                        Logger.Log($"[Schedule] ACC {acc.CharName ?? acc.Username} TỰ ĐỘNG MỞ");
                    }

                    if (kill)
                    {
                        acc.IsSelected = false;
                        _botService.KillProcess(acc.ID);
                        dgvAccounts.Refresh();
                        Logger.Log($"[Schedule] ACC {acc.CharName ?? acc.Username} TỰ ĐỘNG TẮT");
                    }
                }
            };
            _scheduleTimer.Start();
        }


        internal void SaveAccountsToFile()
        {
            AccountStorage.SaveAccounts(accountList);
        }


    }
}