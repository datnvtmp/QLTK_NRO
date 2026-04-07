using Newtonsoft.Json;
using QLTK_Lite.Models;
using QLTK_Lite.Network;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void InitTabAutoTrain(TabPage tab)
        {
            tab.AutoScroll = true;

            // ══════════════════════════════════════════════════════════════
            // CỘT 1  –  TRAIN  (x = 6)
            // ══════════════════════════════════════════════════════════════
            int inputX = 90;
            int inputW = 115;
            int gap = 25;

            tab.Controls.Add(MakeHeader("Train", 6, 8));
            int y = 28;

            var mapsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "maps.txt");
            if (!File.Exists(mapsPath))
            {
                File.WriteAllText(mapsPath, "0. Làng Aru\n1. Rừng Xanh\n2. Hang Tối");
                MessageBox.Show($"Chưa có file maps.txt, đã tạo mặc định tại:\n{mapsPath}\nVui lòng chỉnh sửa rồi khởi động lại!", "Thông báo");
            }

            var chkTrain = new CheckBox { Name = "chkTrain", Text = "Train:", Location = new Point(6, y), Width = 70 };
            var cboMap = new ComboBox { Name = "cboMap", Location = new Point(inputX, y), Width = inputW, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMap.Items.AddRange(File.ReadAllLines(mapsPath));
            if (cboMap.Items.Count > 0) cboMap.SelectedIndex = 0;
            tab.Controls.AddRange(new Control[] { chkTrain, cboMap });
            y += gap;

            var chkKhu = new CheckBox { Name = "chkKhu", Text = "Khu:", Location = new Point(6, y), Width = 70 };
            var numKhu = new NumericUpDown { Name = "numKhu", Location = new Point(inputX, y), Width = 60, Minimum = 0, Maximum = 99 };
            tab.Controls.AddRange(new Control[] { chkKhu, numKhu });
            y += gap;

            tab.Controls.Add(new Label { Text = "Loại quái:", Location = new Point(6, y + 3), Width = 80 });
            var cboLoai = new ComboBox { Name = "cboLoai", Location = new Point(inputX, y), Width = inputW, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoai.Items.AddRange(new object[] { "Tất cả", "Quái đất", "Quái bay", "Theo ID" });
            cboLoai.SelectedIndex = 0;
            tab.Controls.Add(cboLoai);
            y += gap;

            tab.Controls.Add(new Label { Text = "ID quái:", Location = new Point(6, y + 3), Width = 80 });
            tab.Controls.Add(new TextBox { Name = "txtIdQuai", Location = new Point(inputX, y), Width = inputW, BorderStyle = BorderStyle.FixedSingle });
            y += gap;

            tab.Controls.Add(new Label { Text = "List skill:", Location = new Point(6, y + 3), Width = 80 });
            tab.Controls.Add(new TextBox { Name = "txtListSkill", Location = new Point(inputX, y), Width = inputW, BorderStyle = BorderStyle.FixedSingle });
            y += gap;

            AddCheckLabelTextRow(tab, "Trên hp:", "chkUseTrenHp", "txtTrenHp",
                chkX: 6, txtX: inputX, txtW: inputW, ref y, gap);
            AddCheckLabelTextRow(tab, "Dưới hp:", "chkUseDuoiHp", "txtDuoiHp",
                chkX: 6, txtX: inputX, txtW: inputW, ref y, gap);

            tab.Controls.Add(new Label { Text = "ID loại trừ:", Location = new Point(6, y + 3), Width = 80 });
            tab.Controls.Add(new TextBox { Name = "txtIdLoaiTru", Location = new Point(inputX, y), Width = inputW, BorderStyle = BorderStyle.FixedSingle });
            y += gap;

            tab.Controls.Add(new Label { Text = "ID-SL:", Location = new Point(6, y + 3), Width = 80 });
            tab.Controls.Add(new TextBox { Name = "txtIdSl", Location = new Point(inputX, y), Width = inputW, BorderStyle = BorderStyle.FixedSingle });
            y += gap;

            // ══════════════════════════════════════════════════════════════
            // CỘT 2  –  ITEM + LINH TÍNH  (x = 240)
            // ══════════════════════════════════════════════════════════════
            int rx = 240;
            int itemTxtX = rx + 80;  // TextBox bắt đầu tại đây
            int itemTxtW = 130;
            int ry = 8;

            tab.Controls.Add(MakeHeader("Item", rx, ry));
            ry += 20;

            AddCheckLabelTextRow(tab, "ID Dùng:", "chkUseIdDung", "txtIdDung",
                chkX: rx, txtX: itemTxtX, txtW: itemTxtW, ref ry, gap);
            AddCheckLabelTextRow(tab, "ID Vứt:", "chkUseIdVut", "txtIdVut",
                chkX: rx, txtX: itemTxtX, txtW: itemTxtW, ref ry, gap);
            AddCheckLabelTextRow(tab, "Né char:", "chkUseNeChar", "txtNeChar",
                chkX: rx, txtX: itemTxtX, txtW: itemTxtW, ref ry, gap);

            tab.Controls.Add(MakeHeader("Linh Tính", rx, ry));
            ry += 20;

            int checkGap = 22;
            AddCheckRow(tab, rx, ry, "Bông tai", "chkBongTai", "Pem khi mất máu", "chkPemMatMau"); ry += checkGap;
            AddCheckRow(tab, rx, ry, "Cờ đen", "chkCoDen", "Pem khi full máu", "chkPemFullMau"); ry += checkGap;
            AddCheckRow(tab, rx, ry, "Đợi HS", "chkDoiHs", "Up SKH", "chkUpSkh"); ry += checkGap;
            AddCheckRow(tab, rx, ry, "Mua TDLT", "chkMuaTdlt", "Đứng im", "chkDungIm"); ry += checkGap;
            AddCheckRow(tab, rx, ry, "Up giáp", "chkUpGiap", "Chống ks", "chkChongKs"); ry += checkGap;

            // ══════════════════════════════════════════════════════════════
            // CỘT 3  –  LỊCH  (x = 490)
            // ══════════════════════════════════════════════════════════════
            int lx = 490;
            int lNumX = lx + 80;   // NumericUpDown giờ bắt đầu tại đây
            int ly = 8;

            tab.Controls.Add(MakeHeader("Lịch", lx, ly));
            ly += 20;

            // [☐] Tự ON lúc:   [numH] giờ [numM] phút
            AddCheckTimeRow(tab, "Tự ON:", "chkUseAutoOn", "numOnH", "numOnM",
                labelX: lx, numStartX: lNumX, ref ly, gap);

            // [☐] Tự OFF lúc:  [numH] giờ [numM] phút
            AddCheckTimeRow(tab, "Tự OFF:", "chkUseAutoOff", "numOffH", "numOffM",
                labelX: lx, numStartX: lNumX, ref ly, gap);

            // [☐] OFF sau:     [numH] giờ [numM] phút
            AddCheckTimeRow(tab, "OFF sau:", "chkUseOffSau", "numOffSauH", "numOffSauM",
                labelX: lx, numStartX: lNumX, ref ly, gap);

            // ── Nút Lưu cấu hình ─────────────────────────────────────────
            var btnSave = new Button
            {
                Text = "Lưu cấu hình",
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;

            btnSave.Click += (s, e) =>
            {
                var selectedAccs = dgvAccounts.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Select(r => r.DataBoundItem as AccountModel)
                    .Where(a => a != null)
                    .ToList();

                if (selectedAccs.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất 1 tài khoản!", "Thông báo");
                    return;
                }

                var config = ReadConfigFromForm(tab);
                foreach (var acc in selectedAccs)
                {
                    acc.Config = config;
                    // Reset TrainStartTime để "OFF sau" đếm lại từ lúc lưu
                    if (config.UseOffSau)
                        acc.TrainStartTime = DateTime.Now;
                }

                SaveAccountsToFile();

                foreach (var acc in selectedAccs)
                    AppServer.SendToClient(acc.ID, "CONFIG", JsonConvert.SerializeObject(acc.Config));

                Logger.Log($"Đã lưu config cho {selectedAccs.Count} tài khoản.");
                MessageBox.Show($"Đã lưu config cho {selectedAccs.Count} tài khoản!", "OK");
            };

            tab.Controls.Add(btnSave);
            tab.Resize += (s, e) =>
            {
                btnSave.Location = new Point(
                    tab.ClientSize.Width - btnSave.Width - 8,
                    tab.ClientSize.Height - btnSave.Height - 8);
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// [☐] [Label] [TextBox]
        /// </summary>
        private void AddCheckLabelTextRow(
            TabPage tab,
            string labelText, string chkName, string txtName,
            int chkX, int txtX, int txtW,
            ref int y, int gap)
        {
            tab.Controls.Add(new CheckBox
            {
                Name = chkName,
                Location = new Point(chkX, y + 2),
                Width = 16,
                Height = 16
            });
            tab.Controls.Add(new Label
            {
                Text = labelText,
                Location = new Point(chkX + 18, y + 3),
                Width = txtX - (chkX + 18) - 2
            });
            tab.Controls.Add(new TextBox
            {
                Name = txtName,
                Location = new Point(txtX, y),
                Width = txtW,
                BorderStyle = BorderStyle.FixedSingle
            });
            y += gap;
        }

        /// <summary>
        /// [☐] [Label]  [NumH] giờ  [NumM] phút
        /// Checkbox để bật/tắt tính năng, NumericUpDown giờ:phút thẳng hàng.
        /// </summary>
        private void AddCheckTimeRow(
            TabPage tab,
            string labelText, string chkName, string nameH, string nameM,
            int labelX, int numStartX,
            ref int y, int gap,
            int maxH = 23, int maxM = 59)
        {
            int numW = 45;
            int sepW = 26;

            // Checkbox bật/tắt
            tab.Controls.Add(new CheckBox
            {
                Name = chkName,
                Location = new Point(labelX, y + 2),
                Width = 16,
                Height = 16
            });

            // Label tên (sau checkbox)
            tab.Controls.Add(new Label
            {
                Text = labelText,
                Location = new Point(labelX + 18, y + 3),
                Width = numStartX - (labelX + 18) - 2
            });

            // NumericUpDown giờ
            tab.Controls.Add(new NumericUpDown
            {
                Name = nameH,
                Location = new Point(numStartX, y),
                Width = numW,
                Minimum = 0,
                Maximum = maxH
            });

            // "giờ"
            tab.Controls.Add(new Label
            {
                Text = "giờ",
                Location = new Point(numStartX + numW + 2, y + 3),
                Width = sepW,
                AutoSize = false
            });

            // NumericUpDown phút
            tab.Controls.Add(new NumericUpDown
            {
                Name = nameM,
                Location = new Point(numStartX + numW + sepW + 4, y),
                Width = numW,
                Minimum = 0,
                Maximum = maxM
            });

            // "phút"
            tab.Controls.Add(new Label
            {
                Text = "phút",
                Location = new Point(numStartX + numW + sepW + 4 + numW + 2, y + 3),
                Width = 30,
                AutoSize = false
            });

            y += gap;
        }

        private AccConfig ReadConfigFromForm(TabPage tab)
        {
            return new AccConfig
            {
                IsTrain = ((CheckBox)tab.Controls["chkTrain"]).Checked,
                MapIndex = ((ComboBox)tab.Controls["cboMap"]).SelectedIndex,
                IsKhu = ((CheckBox)tab.Controls["chkKhu"]).Checked,
                Khu = (int)((NumericUpDown)tab.Controls["numKhu"]).Value,
                LoaiQuai = ((ComboBox)tab.Controls["cboLoai"]).SelectedIndex,
                IdQuai = tab.Controls["txtIdQuai"].Text.Trim(),
                ListSkill = tab.Controls["txtListSkill"].Text.Trim(),

                UseTrenHp = ((CheckBox)tab.Controls["chkUseTrenHp"]).Checked,
                TrenHp = tab.Controls["txtTrenHp"].Text.Trim(),
                UseDuoiHp = ((CheckBox)tab.Controls["chkUseDuoiHp"]).Checked,
                DuoiHp = tab.Controls["txtDuoiHp"].Text.Trim(),

                IDMobLoaiTru = tab.Controls["txtIdLoaiTru"].Text.Trim(),
                IdSl = tab.Controls["txtIdSl"].Text.Trim(),

                UseIdDung = ((CheckBox)tab.Controls["chkUseIdDung"]).Checked,
                IdDung = tab.Controls["txtIdDung"].Text.Trim(),
                UseIdVut = ((CheckBox)tab.Controls["chkUseIdVut"]).Checked,
                IdVut = tab.Controls["txtIdVut"].Text.Trim(),
                UseNeChar = ((CheckBox)tab.Controls["chkUseNeChar"]).Checked,
                NeChar = tab.Controls["txtNeChar"].Text.Trim(),

                BongTai = ((CheckBox)tab.Controls["chkBongTai"]).Checked,
                CoDen = ((CheckBox)tab.Controls["chkCoDen"]).Checked,
                DoiHs = ((CheckBox)tab.Controls["chkDoiHs"]).Checked,
                MuaTdlt = ((CheckBox)tab.Controls["chkMuaTdlt"]).Checked,
                PemMatMau = ((CheckBox)tab.Controls["chkPemMatMau"]).Checked,
                PemFullMau = ((CheckBox)tab.Controls["chkPemFullMau"]).Checked,
                UpSkh = ((CheckBox)tab.Controls["chkUpSkh"]).Checked,
                DungIm = ((CheckBox)tab.Controls["chkDungIm"]).Checked,
                UpGiap = ((CheckBox)tab.Controls["chkUpGiap"]).Checked,
                IsAntiKS = ((CheckBox)tab.Controls["chkChongKs"]).Checked,
                // Lịch
                UseAutoOn = ((CheckBox)tab.Controls["chkUseAutoOn"]).Checked,
                AutoOnH = (int)((NumericUpDown)tab.Controls["numOnH"]).Value,
                AutoOnM = (int)((NumericUpDown)tab.Controls["numOnM"]).Value,

                UseAutoOff = ((CheckBox)tab.Controls["chkUseAutoOff"]).Checked,
                AutoOffH = (int)((NumericUpDown)tab.Controls["numOffH"]).Value,
                AutoOffM = (int)((NumericUpDown)tab.Controls["numOffM"]).Value,

                UseOffSau = ((CheckBox)tab.Controls["chkUseOffSau"]).Checked,
                AutoOffSauH = (int)((NumericUpDown)tab.Controls["numOffSauH"]).Value,
                AutoOffSauM = (int)((NumericUpDown)tab.Controls["numOffSauM"]).Value,
            };
        }

        private void LoadConfigToForm(TabPage tab, AccConfig cfg)
        {
            ((CheckBox)tab.Controls["chkTrain"]).Checked = cfg.IsTrain;

            var cboMap = (ComboBox)tab.Controls["cboMap"];
            foreach (var item in cboMap.Items)
                if (item.ToString().StartsWith(cfg.MapIndex + "."))
                { cboMap.SelectedItem = item; break; }

            ((CheckBox)tab.Controls["chkKhu"]).Checked = cfg.IsKhu;
            ((NumericUpDown)tab.Controls["numKhu"]).Value = cfg.Khu;
            ((ComboBox)tab.Controls["cboLoai"]).SelectedIndex = cfg.LoaiQuai;
            tab.Controls["txtIdQuai"].Text = cfg.IdQuai;
            tab.Controls["txtListSkill"].Text = cfg.ListSkill;

            ((CheckBox)tab.Controls["chkUseTrenHp"]).Checked = cfg.UseTrenHp;
            tab.Controls["txtTrenHp"].Text = cfg.TrenHp;
            ((CheckBox)tab.Controls["chkUseDuoiHp"]).Checked = cfg.UseDuoiHp;
            tab.Controls["txtDuoiHp"].Text = cfg.DuoiHp;

            tab.Controls["txtIdLoaiTru"].Text = cfg.IDMobLoaiTru;
            tab.Controls["txtIdSl"].Text = cfg.IdSl;

            ((CheckBox)tab.Controls["chkUseIdDung"]).Checked = cfg.UseIdDung;
            tab.Controls["txtIdDung"].Text = cfg.IdDung;
            ((CheckBox)tab.Controls["chkUseIdVut"]).Checked = cfg.UseIdVut;
            tab.Controls["txtIdVut"].Text = cfg.IdVut;
            ((CheckBox)tab.Controls["chkUseNeChar"]).Checked = cfg.UseNeChar;
            tab.Controls["txtNeChar"].Text = cfg.NeChar;

            ((CheckBox)tab.Controls["chkBongTai"]).Checked = cfg.BongTai;
            ((CheckBox)tab.Controls["chkCoDen"]).Checked = cfg.CoDen;
            ((CheckBox)tab.Controls["chkDoiHs"]).Checked = cfg.DoiHs;
            ((CheckBox)tab.Controls["chkMuaTdlt"]).Checked = cfg.MuaTdlt;
            ((CheckBox)tab.Controls["chkPemMatMau"]).Checked = cfg.PemMatMau;
            ((CheckBox)tab.Controls["chkPemFullMau"]).Checked = cfg.PemFullMau;
            ((CheckBox)tab.Controls["chkUpSkh"]).Checked = cfg.UpSkh;
            ((CheckBox)tab.Controls["chkDungIm"]).Checked = cfg.DungIm;
            ((CheckBox)tab.Controls["chkUpGiap"]).Checked = cfg.UpGiap;
            ((CheckBox)tab.Controls["chkChongKs"]).Checked = cfg.IsAntiKS;

            // Lịch
            ((CheckBox)tab.Controls["chkUseAutoOn"]).Checked = cfg.UseAutoOn;
            ((NumericUpDown)tab.Controls["numOnH"]).Value = cfg.AutoOnH;
            ((NumericUpDown)tab.Controls["numOnM"]).Value = cfg.AutoOnM;

            ((CheckBox)tab.Controls["chkUseAutoOff"]).Checked = cfg.UseAutoOff;
            ((NumericUpDown)tab.Controls["numOffH"]).Value = cfg.AutoOffH;
            ((NumericUpDown)tab.Controls["numOffM"]).Value = cfg.AutoOffM;

            ((CheckBox)tab.Controls["chkUseOffSau"]).Checked = cfg.UseOffSau;
            ((NumericUpDown)tab.Controls["numOffSauH"]).Value = cfg.AutoOffSauH;
            ((NumericUpDown)tab.Controls["numOffSauM"]).Value = cfg.AutoOffSauM;
        }

        private Label MakeHeader(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
        }

        private void AddCheckRow(TabPage tab, int startX, int y,
            string text1, string name1,
            string text2 = "", string name2 = "")
        {
            tab.Controls.Add(new CheckBox { Name = name1, Text = text1, Location = new Point(startX, y), AutoSize = true });
            if (!string.IsNullOrEmpty(text2))
                tab.Controls.Add(new CheckBox { Name = name2, Text = text2, Location = new Point(startX + 100, y), AutoSize = true });
        }
    }
}