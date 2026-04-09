using QLTK_Lite.Config;
using QLTK_Lite.Models;
using QLTK_Lite.Network;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private const int RIGHT_PANEL_WIDTH = 220;

        private void SetupUI()
        {
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;

            dgvAccounts.BackgroundColor = Color.White;
            dgvAccounts.BorderStyle = BorderStyle.FixedSingle;
            dgvAccounts.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgvAccounts.GridColor = Color.FromArgb(180, 180, 180);

            dgvAccounts.DefaultCellStyle.BackColor = Color.White;
            dgvAccounts.DefaultCellStyle.ForeColor = Color.Black;
            dgvAccounts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvAccounts.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvAccounts.EnableHeadersVisualStyles = false;
            dgvAccounts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvAccounts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvAccounts.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvAccounts.ColumnHeadersHeight = 35;
            dgvAccounts.RowHeadersVisible = false;
            dgvAccounts.AllowUserToAddRows = false;
            dgvAccounts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAccounts.AllowUserToResizeRows = false;
            dgvAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        private void FormatColumns()
        {
            foreach (DataGridViewColumn col in dgvAccounts.Columns)
                col.ReadOnly = true;

            // ✅ Đặt thứ tự đúng như code gốc
            dgvAccounts.Columns["IsSelected"].DisplayIndex = 0;
            dgvAccounts.Columns["ID"].DisplayIndex = 1;
            dgvAccounts.Columns["CharName"].DisplayIndex = 2;
            dgvAccounts.Columns["DataInGame"].DisplayIndex = 3;
            dgvAccounts.Columns["Username"].DisplayIndex = 4;
            dgvAccounts.Columns["Password"].DisplayIndex = 5;
            dgvAccounts.Columns["Server"].DisplayIndex = 6;
            dgvAccounts.Columns["Status"].DisplayIndex = 7;
            dgvAccounts.Columns["Proxy"].DisplayIndex = 8;

            dgvAccounts.Columns["IsSelected"].ReadOnly = false;
            dgvAccounts.Columns["IsSelected"].HeaderText = "✓";
            dgvAccounts.Columns["IsSelected"].Width = 30;
            dgvAccounts.Columns["ID"].HeaderText = "ID";
            dgvAccounts.Columns["ID"].Width = 40;
            dgvAccounts.Columns["CharName"].HeaderText = "CharName";
            dgvAccounts.Columns["CharName"].Width = 90;
            dgvAccounts.Columns["DataInGame"].HeaderText = "DataInGame";
            dgvAccounts.Columns["DataInGame"].Width = 160;
            dgvAccounts.Columns["Username"].HeaderText = "Tài khoản";
            dgvAccounts.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvAccounts.Columns["Username"].Width = 140;
            dgvAccounts.Columns["Password"].Visible = false;
            dgvAccounts.Columns["Config"].Visible = false;
            dgvAccounts.Columns["LastInfo"].Visible = false;
            dgvAccounts.Columns["BossConfig"].Visible = false;
            dgvAccounts.Columns["CSKBConfig"].Visible = false;
            dgvAccounts.Columns["TrainStartTime"].Visible = false;

            dgvAccounts.Columns["Server"].HeaderText = "Server";
            dgvAccounts.Columns["Server"].Width = 60;
            dgvAccounts.Columns["Status"].HeaderText = "Status";
            dgvAccounts.Columns["Status"].Width = 60;
            dgvAccounts.Columns["Proxy"].HeaderText = "Proxy";
            dgvAccounts.Columns["Proxy"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void CreateLayout()
        {
            var split = new SplitContainer
            {
                Orientation = Orientation.Horizontal,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                IsSplitterFixed = true
            };
            this.Controls.Add(split);
            split.SplitterDistance = 220;

            // ── Panel phải trên (nhập TK/MK/nút) ────────────────────────────
            var pnlRight = new Panel { Width = RIGHT_PANEL_WIDTH, Dock = DockStyle.Right, BackColor = Color.White };
            split.Panel1.Controls.Add(pnlRight);

            dgvAccounts.Parent = split.Panel1;
            dgvAccounts.Dock = DockStyle.Fill;
            dgvAccounts.BringToFront();

            int startY = 15, spacing = 28;

            txtTaiKhoan = CreateInputBox(pnlRight, "Tài khoản:", startY);
            txtPass = CreateInputBox(pnlRight, "Mật khẩu:", startY + spacing);
            txtPass.PasswordChar = '•';

            pnlRight.Controls.Add(new Label
            {
                Text = "Máy chủ:",
                AutoSize = true,
                Location = new Point(5, startY + spacing * 2 + 4)
            });
            cboServer = new ComboBox
            {
                Location = new Point(70, startY + spacing * 2),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboServer.Items.AddRange(ServerConfig.LoadServers());
            cboServer.SelectedIndex = 0;
            pnlRight.Controls.Add(cboServer);

            txtProxy = CreateInputBox(pnlRight, "Proxy:", startY + spacing * 3);

            int btnY = startY + spacing * 4 + 10;
            var btnThem = CreateButton(pnlRight, "Thêm", 10, btnY, 63, Color.FromArgb(245, 245, 245), Color.Black);
            var btnSua = CreateButton(pnlRight, "Sửa", 78, btnY, 63, Color.FromArgb(245, 245, 245), Color.Black);
            var btnXoa = CreateButton(pnlRight, "Xóa", 146, btnY, 63, Color.FromArgb(245, 245, 245), Color.Black);
            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;

            int btnY2 = btnY + 36;
            CreateBotButtons(pnlRight, 10, btnY2);

            // ── Panel dưới: Tab trái + Log phải ──────────────────────────────
            split.Panel2.Padding = new Padding(4);

            _splitBottom = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                IsSplitterFixed = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            split.Panel2.Controls.Add(_splitBottom);
            // SplitterDistance set trong Form1.cs → this.Load để có width thật

            var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(15, 6) };
            _tabAutoTrain = new TabPage("Auto Train");
            _tabFarmManh = new TabPage("Farm Mảnh");
            _tabCSKB = new TabPage("CSKB");
            var tabBoss = new TabPage("Săn Boss");
            var tabConfig = new TabPage("Cấu hình chung");
            tabs.TabPages.AddRange(new[] { _tabAutoTrain, _tabFarmManh, _tabCSKB, tabBoss, tabConfig });
            _splitBottom.Panel1.Controls.Add(tabs);

            // ── MỚI: Panel bọc ngoài để chứa cả RichTextBox + Button ──
            var pnlLog = new Panel { Dock = DockStyle.Fill };
            _splitBottom.Panel2.Controls.Add(pnlLog);

            var btnUpdate = new Button
            {
                Text = "🔄 Cập nhật",
                Dock = DockStyle.Bottom,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += BtnCapNhat_Click;

            var txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            // Thứ tự Add quan trọng: Button trước, RichTextBox sau
            pnlLog.Controls.Add(txtLog);
            pnlLog.Controls.Add(btnUpdate);
            _logBox = txtLog;

            InitTabAutoTrain(_tabAutoTrain);
            InitTabFarmManh(_tabFarmManh);
            InitTabSanBoss(tabBoss);
            InitCSKB(_tabCSKB); // ← thêm dòng này
        }
        private void BtnCapNhat_Click(object sender, EventArgs e)
        {
            var acc = dgvAccounts.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as AccountModel)
                .FirstOrDefault(a => a != null);

            if (acc == null)
            {
                MessageBox.Show("Chọn 1 tài khoản!", "Thông báo");
                return;
            }

            AppServer.SendToClient(acc.ID, MsgType.GET_INFO, "");
            Logger.Log($"Đã gửi yêu cầu cập nhật info cho ID {acc.ID}");
        }

        public void ShowInfo(string info)
        {
            if (_logBox.InvokeRequired)
                _logBox.Invoke((Action)(() => ShowInfo(info)));
            else
                _logBox.Text = info;
        }
        internal TextBox CreateInputBox(Panel parent, string label, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                ForeColor = Color.Black,
                Location = new Point(5, y + 4)
            });
            var txt = new TextBox
            {
                Location = new Point(70, y),
                Width = 140,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        internal Button CreateButton(Panel parent, string text, int x, int y, int width, Color bgColor, Color fgColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = bgColor,
                ForeColor = fgColor,
                Cursor = Cursors.Hand,
                Padding = new Padding(0)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            parent.Controls.Add(btn);
            return btn;
        }

        private void EnableDoubleBuffering(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }
    }
}