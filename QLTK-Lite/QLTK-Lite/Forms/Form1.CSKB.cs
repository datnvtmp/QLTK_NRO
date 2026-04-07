using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using Newtonsoft.Json;
using QLTK_Lite.Models;
using QLTK_Lite.Network;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void InitCSKB(TabPage tab)
        {
            tab.AutoScroll = true;

            int inputX = 110, inputW = 120, gap = 30;
            int y = 10;

            tab.Controls.Add(MakeHeader("CẤU HÌNH CSKB", 10, y));
            y += 30;

            // Bật up CSKB
            tab.Controls.Add(new CheckBox 
            { 
                Name = "chkCSKBEnable", 
                Text = "Bật up CSKB:", 
                Location = new Point(10, y), 
                Width = 100 
            });
            y += gap;

            // Map giao dịch
            tab.Controls.Add(new Label { Text = "Map giao dịch:", Location = new Point(10, y + 3), Width = 100 });
            tab.Controls.Add(new NumericUpDown
            {
                Name = "numCSKBMap",
                Location = new Point(inputX, y),
                Width = inputW,
                Minimum = 0,
                Maximum = 999
            });
            y += gap;

            // Khu giao dịch
            tab.Controls.Add(new Label { Text = "Khu giao dịch:", Location = new Point(10, y + 3), Width = 100 });
            tab.Controls.Add(new NumericUpDown
            {
                Name = "numCSKBZone",
                Location = new Point(inputX, y),
                Width = inputW,
                Minimum = -1,
                Maximum = 99,
                Value = -1
            });
            y += gap;

            // Type
            tab.Controls.Add(new Label { Text = "Loại:", Location = new Point(10, y + 3), Width = 100 });
            var cboType = new ComboBox
            {
                Name = "cboCSKBType",
                Location = new Point(inputX, y),
                Width = inputW,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new object[] { "Up CSKB", "Chứa CSKB" });
            cboType.SelectedIndex = 0;
            tab.Controls.Add(cboType);
            y += gap;

            // Đã đầy (Dành cho acc nhận)
            tab.Controls.Add(new CheckBox
            {
                Name = "chkCSKBIsFull",
                Text = "Đã đầy (Rương):",
                Location = new Point(10, y),
                Width = 120
            });
            y += gap + 10;

            // Nút Lưu
            var btnSave = new Button
            {
                Text = "Lưu cấu hình CSKB",
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(inputX - 30, y)
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

                ReadCSKBConfigFromForm(tab, selectedAccs);
                SaveAccountsToFile();

                foreach (var acc in selectedAccs)
                    AppServer.SendToClient(acc.ID, MsgType.CSKB_CONFIG, JsonConvert.SerializeObject(acc.CSKBConfig));

                Logger.Log($"Đã lưu cấu hình CSKB cho {selectedAccs.Count} tài khoản.");
            };
            tab.Controls.Add(btnSave);
        }

        private void ReadCSKBConfigFromForm(TabPage tab, System.Collections.Generic.List<AccountModel> accounts)
        {
            bool isEnable = ((CheckBox)tab.Controls["chkCSKBEnable"]).Checked;
            int map = (int)((NumericUpDown)tab.Controls["numCSKBMap"]).Value;
            int zone = (int)((NumericUpDown)tab.Controls["numCSKBZone"]).Value;
            int type = ((ComboBox)tab.Controls["cboCSKBType"]).SelectedIndex;
            bool isFull = ((CheckBox)tab.Controls["chkCSKBIsFull"]).Checked;

            foreach (var acc in accounts)
            {
                if (acc.CSKBConfig == null) acc.CSKBConfig = new CSKBConfig();
                acc.CSKBConfig.IsCSKB = isEnable;
                acc.CSKBConfig.CSKBMap = map;
                acc.CSKBConfig.CSKBZone = zone;
                acc.CSKBConfig.CSKBType = type;
                acc.CSKBConfig.IsFull = isFull;
            }
        }

        private void LoadCSKBConfigToForm(TabPage tab, CSKBConfig config)
        {
            if (config == null) return;
            ((CheckBox)tab.Controls["chkCSKBEnable"]).Checked = config.IsCSKB;
            ((NumericUpDown)tab.Controls["numCSKBMap"]).Value = config.CSKBMap;
            ((NumericUpDown)tab.Controls["numCSKBZone"]).Value = config.CSKBZone;
            if (config.CSKBType >= 0 && config.CSKBType < 2)
                ((ComboBox)tab.Controls["cboCSKBType"]).SelectedIndex = config.CSKBType;
            ((CheckBox)tab.Controls["chkCSKBIsFull"]).Checked = config.IsFull;
        }
    }
}