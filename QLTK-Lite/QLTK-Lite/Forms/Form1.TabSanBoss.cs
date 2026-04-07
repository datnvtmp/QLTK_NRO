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
        private void InitTabSanBoss(TabPage tab)
        {
            tab.AutoScroll = true;

            int labelX = 6;
            int inputX = 100;
            int inputW = 130;
            int gap = 26;

            tab.Controls.Add(MakeHeader("Săn Boss", labelX, 8));
            int y = 28;

            // [☐] Bật Săn Boss
            tab.Controls.Add(new CheckBox
            {
                Name = "chkSanBoss",
                Text = "Bật Săn Boss",
                Location = new Point(labelX, y),
                AutoSize = true
            });
            y += gap;

            // Map Boss
            tab.Controls.Add(new Label { Text = "Map Boss:", Location = new Point(labelX, y + 3), Width = 90 });
            tab.Controls.Add(new TextBox
            {
                Name = "txtMapBoss",
                Location = new Point(inputX, y),
                Width = inputW,
                BorderStyle = BorderStyle.FixedSingle,
            });
            y += gap;

            // Group
            tab.Controls.Add(new Label { Text = "Group:", Location = new Point(labelX, y + 3), Width = 90 });
            tab.Controls.Add(new NumericUpDown
            {
                Name = "numGroupBoss",
                Location = new Point(inputX, y),
                Width = 60,
                Minimum = 1,
                Maximum = 10,
                Value = 1
            });
            y += gap;

            // Skill săn
            tab.Controls.Add(new Label { Text = "Skill săn:", Location = new Point(labelX, y + 3), Width = 90 });
            tab.Controls.Add(new TextBox
            {
                Name = "txtSkillBoss",
                Location = new Point(inputX, y),
                Width = inputW,
                BorderStyle = BorderStyle.FixedSingle,
            });
            y += gap;

            // Type (ComboBox)
            tab.Controls.Add(new Label { Text = "Type:", Location = new Point(labelX, y + 3), Width = 90 });
            var cboType = new ComboBox
            {
                Name = "cboTypeBoss",
                Location = new Point(inputX, y),
                Width = inputW,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new object[] { "Đấm Boss", "Trói Boss", "Buff / Hồi Sinh" });
            cboType.SelectedIndex = 0;
            tab.Controls.Add(cboType);
            y += gap;

            // Dưới HP
            AddCheckLabelTextRow(tab, "Dưới HP:", "chkDuoiHpBoss", "txtDuoiHpBoss",
                chkX: labelX, txtX: inputX, txtW: inputW, ref y, gap);

            // Delay
            tab.Controls.Add(new Label { Text = "Delay (ms):", Location = new Point(labelX, y + 3), Width = 90 });
            tab.Controls.Add(new NumericUpDown
            {
                Name = "numDelayBoss",
                Location = new Point(inputX, y),
                Width = 70,
                Minimum = 0,
                Maximum = 10000,
                Value = 500,
                Increment = 100
            });
            y += gap;

            // Đợi hồi sinh
            tab.Controls.Add(new CheckBox
            {
                Name = "chkDoiHsBoss",
                Text = "Đợi hồi sinh",
                Location = new Point(labelX, y),
                AutoSize = true
            });
            y += gap;

            // ── Nút Lưu ───────────────────────────────────────────────
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

                var config = ReadBossConfigFromForm(tab);
                foreach (var acc in selectedAccs)
                    acc.BossConfig = config;

                SaveAccountsToFile();

                foreach (var acc in selectedAccs)
                    AppServer.SendToClient(acc.ID, "BOSS_CONFIG",
                        JsonConvert.SerializeObject(acc.BossConfig));

                Logger.Log($"Đã lưu boss config cho {selectedAccs.Count} tài khoản.");
            };

            tab.Controls.Add(btnSave);
            tab.Resize += (s, e) =>
            {
                btnSave.Location = new Point(
                    tab.ClientSize.Width - btnSave.Width - 8,
                    tab.ClientSize.Height - btnSave.Height - 8);
            };
        }

        private BossConfig ReadBossConfigFromForm(TabPage tab)
        {
            return new BossConfig
            {
                IsSanBoss = ((CheckBox)tab.Controls["chkSanBoss"]).Checked,
                MapBoss = tab.Controls["txtMapBoss"].Text.Trim(),
                Group = (int)((NumericUpDown)tab.Controls["numGroupBoss"]).Value,
                SkillBoss = tab.Controls["txtSkillBoss"].Text.Trim(),
                TypeBoss = ((ComboBox)tab.Controls["cboTypeBoss"]).SelectedIndex,
                UseDuoiHp = ((CheckBox)tab.Controls["chkDuoiHpBoss"]).Checked,
                DuoiHp = tab.Controls["txtDuoiHpBoss"].Text.Trim(),
                Delay = (int)((NumericUpDown)tab.Controls["numDelayBoss"]).Value,
                DoiHs = ((CheckBox)tab.Controls["chkDoiHsBoss"]).Checked,
            };
        }

        private void LoadBossConfigToForm(TabPage tab, BossConfig cfg)
        {
            ((CheckBox)tab.Controls["chkSanBoss"]).Checked = cfg.IsSanBoss;
            tab.Controls["txtMapBoss"].Text = cfg.MapBoss;
            ((NumericUpDown)tab.Controls["numGroupBoss"]).Value = cfg.Group;
            tab.Controls["txtSkillBoss"].Text = cfg.SkillBoss;
            ((ComboBox)tab.Controls["cboTypeBoss"]).SelectedIndex = cfg.TypeBoss;
            ((CheckBox)tab.Controls["chkDuoiHpBoss"]).Checked = cfg.UseDuoiHp;
            tab.Controls["txtDuoiHpBoss"].Text = cfg.DuoiHp;
            ((NumericUpDown)tab.Controls["numDelayBoss"]).Value = cfg.Delay;
            ((CheckBox)tab.Controls["chkDoiHsBoss"]).Checked = cfg.DoiHs;
        }
    }
}