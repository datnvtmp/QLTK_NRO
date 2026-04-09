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

            var chkFarmAoQuan = new CheckBox { Name = "chkFarmAoQuan", Text = "Farm áo/quần", Location = new Point(x, y), AutoSize = true };
            y += gap;
            var chkFarmGang = new CheckBox { Name = "chkFarmGang", Text = "Farm găng", Location = new Point(x, y), AutoSize = true };
            y += gap;
            var chkFarmNhan = new CheckBox { Name = "chkFarmNhan", Text = "Farm nhẫn", Location = new Point(x, y), AutoSize = true };
            y += gap;

            tab.Controls.AddRange(new Control[] { chkFarmAoQuan, chkFarmGang, chkFarmNhan });

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
                IsFarmGang = ((CheckBox)tab.Controls["chkFarmGang"]).Checked,
                IsFarmNhan = ((CheckBox)tab.Controls["chkFarmNhan"]).Checked,
            };
        }

        private void LoadFarmManhConfigToForm(TabPage tab, FarmManhConfig cfg)
        {
            if (cfg == null) cfg = new FarmManhConfig();
            ((CheckBox)tab.Controls["chkFarmAoQuan"]).Checked = cfg.IsFarmAoQuan;
            ((CheckBox)tab.Controls["chkFarmGang"]).Checked = cfg.IsFarmGang;
            ((CheckBox)tab.Controls["chkFarmNhan"]).Checked = cfg.IsFarmNhan;
        }
    }
}
