using Newtonsoft.Json;
using QLTK_Lite.Models;
using QLTK_Lite.Network;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void InitTabFarmManh(TabPage tab)
        {
            tab.AutoScroll = true;
            int x = 10;
            int y = 10;
            int gap = 30;

            tab.Controls.Add(MakeHeader("Farming", x, y));
            y += 25;

            AddFarmManhRow(tab, "Farm áo/quần", "chkFarmAoQuan", "cboQuantityAoQuan", ref y, x, gap);
            AddFarmManhRow(tab, "Farm găng", "chkFarmGang", "cboQuantityGang", ref y, x, gap);
            AddFarmManhRow(tab, "Farm nhẫn", "chkFarmNhan", "cboQuantityNhan", ref y, x, gap);

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

                var config = ReadFarmManhConfigFromForm(tab);
                foreach (var acc in selectedAccs)
                {
                    acc.FarmManhConfig = config;
                }

                SaveAccountsToFile();

                foreach (var acc in selectedAccs)
                    AppServer.SendToClient(acc.ID, MsgType.FARM_MANH_CONFIG, JsonConvert.SerializeObject(acc.FarmManhConfig));

                Logger.Log($"Đã lưu config Farm Mảnh cho {selectedAccs.Count} tài khoản.");
            };

            tab.Controls.Add(btnSave);
            tab.Resize += (s, e) =>
            {
                btnSave.Location = new Point(
                    tab.ClientSize.Width - btnSave.Width - 8,
                    tab.ClientSize.Height - btnSave.Height - 8);
            };
        }

        private FarmManhConfig ReadFarmManhConfigFromForm(TabPage tab)
        {
            return new FarmManhConfig
            {
                IsFarmAoQuan = ((CheckBox)tab.Controls["chkFarmAoQuan"]).Checked,
                QuantityAoQuan = int.Parse(((ComboBox)tab.Controls["cboQuantityAoQuan"]).Text),
                IsFarmGang = ((CheckBox)tab.Controls["chkFarmGang"]).Checked,
                QuantityGang = int.Parse(((ComboBox)tab.Controls["cboQuantityGang"]).Text),
                IsFarmNhan = ((CheckBox)tab.Controls["chkFarmNhan"]).Checked,
                QuantityNhan = int.Parse(((ComboBox)tab.Controls["cboQuantityNhan"]).Text),
            };
        }

        private void LoadFarmManhConfigToForm(TabPage tab, FarmManhConfig cfg)
        {
            if (cfg == null) cfg = new FarmManhConfig();
            ((CheckBox)tab.Controls["chkFarmAoQuan"]).Checked = cfg.IsFarmAoQuan;
            ((ComboBox)tab.Controls["cboQuantityAoQuan"]).Text = cfg.QuantityAoQuan.ToString();
            ((CheckBox)tab.Controls["chkFarmGang"]).Checked = cfg.IsFarmGang;
            ((ComboBox)tab.Controls["cboQuantityGang"]).Text = cfg.QuantityGang.ToString();
            ((CheckBox)tab.Controls["chkFarmNhan"]).Checked = cfg.IsFarmNhan;
            ((ComboBox)tab.Controls["cboQuantityNhan"]).Text = cfg.QuantityNhan.ToString();
        }

        private void AddFarmManhRow(TabPage tab, string text, string chkName, string cboName, ref int y, int x, int gap)
        {
            var chk = new CheckBox { Name = chkName, Text = text, Location = new Point(x, y), AutoSize = true };
            var cbo = new ComboBox { Name = cboName, Location = new Point(x + 120, y - 2), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };
            cbo.Items.AddRange(new object[] { "40", "80" });
            cbo.Text = "40";
            tab.Controls.AddRange(new Control[] { chk, cbo });
            y += gap;
        }
    }
}
