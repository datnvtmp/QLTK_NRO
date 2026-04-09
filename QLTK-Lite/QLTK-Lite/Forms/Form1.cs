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
        private readonly Dictionary<int, int> _pendingCSKBTrades = new Dictionary<int, int>();

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
            AppServer.OnCSKBFull += (senderId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    var sender = accountList.FirstOrDefault(a => a.ID == senderId);
                    if (sender == null) return;

                    // Tìm acc nhận (Type 1) mà chưa full và không online
                    var receiver = accountList.FirstOrDefault(a => 
                        a.CSKBConfig.IsCSKB && 
                        a.CSKBConfig.CSKBType == 1 && 
                        !a.CSKBConfig.IsFull && 
                        (a.Status == "Offline" || a.Status == "-" || string.IsNullOrEmpty(a.Status)));

                    if (receiver != null)
                    {
                        _pendingCSKBTrades[receiver.ID] = senderId;
                        receiver.IsSelected = true;
                        if (!_botService.IsRunning) _botService.Start();
                        dgvAccounts.Refresh();
                        Logger.Log($"[CSKB] Acc Up {sender.Username} yeu cau, dang mo acc nhan {receiver.Username}");
                    }
                    else
                    {
                        Logger.Log($"[CSKB] Acc Up {sender.Username} yeu cau nhung khong tim thay acc nhan nao trong!");
                    }
                }));
            };

            AppServer.OnCSKBReceiverId += (receiverId, payload) =>
            {
                this.Invoke((Action)(() =>
                {
                    if (_pendingCSKBTrades.TryGetValue(receiverId, out int senderId))
                    {
                        AppServer.SendToClient(senderId, MsgType.CSKB_RECEIVER_ID, payload);
                        Logger.Log($"[CSKB] Da gui ID {payload} tu acc nhan {receiverId} cho acc up {senderId}");
                    }
                }));
            };

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
                }));
            };

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
                }));
            };
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