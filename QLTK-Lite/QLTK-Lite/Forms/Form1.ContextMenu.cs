using Newtonsoft.Json;
using QLTK_Lite.Config;
using QLTK_Lite.Models;
using QLTK_Lite.Network;
using QLTK_Lite.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            AddItem(menu, "Mở remote (GUI)", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc)
                        RemoteService.ShowGameWindow(RemoteService.BuildWindowTitle(acc.ID, acc.Username));
            });
            AddItem(menu, "Đóng remote", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc)
                        RemoteService.HideGameWindow(RemoteService.BuildWindowTitle(acc.ID, acc.Username));
            });
            AddItem(menu, "Sắp xếp remote", (s, e) =>
                RemoteService.ArrangeWindows(_botService.GetRunningWindowTitles(accountList)));

            menu.Items.Add(new ToolStripSeparator());
            AddItem(menu, "Hành trang", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc)
                        AppServer.SendToClient(acc.ID, MsgType.CMD_HANHTRANG);
            });
            menu.Items.Add(new ToolStripSeparator());

            AddItem(menu, "Chọn auto", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc) acc.IsSelected = true;
                dgvAccounts.Refresh();
                SaveAccountsToFile(); // ← thêm
            });
            AddItem(menu, "Bỏ chọn auto", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc) acc.IsSelected = false;
                dgvAccounts.Refresh();
                SaveAccountsToFile(); // ← thêm
            });

            menu.Items.Add(new ToolStripSeparator());

            var itemDoiServer = new ToolStripMenuItem("Đổi server");
            foreach (var sv in ServerConfig.LoadServers())
            {
                var s2 = sv;
                itemDoiServer.DropDownItems.Add(sv).Click += (sender, e) =>
                {
                    foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                        if (row.DataBoundItem is AccountModel acc) acc.Server = s2;
                    dgvAccounts.Refresh();
                    SaveAccountsToFile();
                };
            }
            menu.Items.Add(itemDoiServer);
            menu.Items.Add(new ToolStripSeparator());

            AddItem(menu, "Xuất acc đang chọn ra file JSON", (s, e) =>
                ExportToJson(
                    dgvAccounts.SelectedRows.Cast<DataGridViewRow>()
                        .Select(r => r.DataBoundItem as AccountModel)
                        .Where(a => a != null).ToList(),
                    "accounts_export.json"));
            AddItem(menu, "Xuất tất cả ra file JSON", (s, e) =>
                ExportToJson(accountList.ToList(), "accounts_all.json"));
            AddItem(menu, "Nhập tài khoản từ file JSON", (s, e) => ImportFromJson());
            AddItem(menu, "Nhập tài khoản từ file TXT", (s, e) => ImportFromTxt());

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Reset hẹn giờ");
            menu.Items.Add("Sửa data");
            menu.Items.Add(new ToolStripSeparator());

            AddItem(menu, "Đóng process", (s, e) =>
            {
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc)
                        _botService.KillProcess(acc.ID);
            });

            dgvAccounts.ContextMenuStrip = menu;
        }

        private void AddItem(ContextMenuStrip menu, string text, EventHandler onClick)
            => menu.Items.Add(text).Click += onClick;

        private void ExportToJson(List<AccountModel> list, string defaultName)
        {
            if (list.Count == 0) return;
            using (var dlg = new SaveFileDialog { Filter = "JSON file|*.json", FileName = defaultName })
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, JsonConvert.SerializeObject(list, Formatting.Indented));
                    MessageBox.Show($"Đã xuất {list.Count} tài khoản!", "Thành công");
                }
        }

        private void ImportFromJson()
        {
            using (var dlg = new OpenFileDialog { Filter = "JSON file|*.json" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var imported = JsonConvert.DeserializeObject<List<AccountModel>>(File.ReadAllText(dlg.FileName));
                    if (imported == null || imported.Count == 0) return;
                    int added = 0;
                    foreach (var imp in imported)
                    {
                        if (accountList.Any(a => a.Username == imp.Username)) continue;
                        imp.ID = accountList.Count > 0 ? accountList.Max(a => a.ID) + 1 : 1;
                        accountList.Add(imp);
                        added++;
                    }
                    SaveAccountsToFile();
                    MessageBox.Show($"Đã nhập {added} tài khoản!", "Thành công");
                }
                catch { MessageBox.Show("File JSON không hợp lệ!", "Lỗi"); }
            }
        }

        private void ImportFromTxt()
        {
            using (var dlg = new OpenFileDialog { Filter = "Text file|*.txt" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var servers = ServerConfig.LoadServers();
                    int added = 0, skipped = 0;
                    foreach (var line in File.ReadAllLines(dlg.FileName))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var p = line.Split('|');
                        if (p.Length < 3 || p.Length > 4) { skipped++; continue; }
                        var user = p[0].Trim(); var pass = p[1].Trim(); var svRaw = p[2].Trim();
                        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass) || user.Contains(" "))
                        { skipped++; continue; }
                        if (!int.TryParse(svRaw, out int svIdx) || svIdx < 1 || svIdx > servers.Length
                            || accountList.Any(a => a.Username == user))
                        { skipped++; continue; }
                        accountList.Add(new AccountModel
                        {
                            ID = accountList.Count > 0 ? accountList.Max(a => a.ID) + 1 : 1,
                            Username = user,
                            Password = pass,
                            Server = servers[svIdx - 1],
                            Proxy = p.Length > 3 ? p[3].Trim() : "None",
                            Status = "-",
                            CharName = "",
                            DataInGame = ""
                        });
                        added++;
                    }
                    SaveAccountsToFile();
                    MessageBox.Show($"Đã nhập: {added}\nBỏ qua: {skipped} dòng không hợp lệ\n\nĐịnh dạng: user|pass|server|proxy", "Kết quả");
                }
                catch { MessageBox.Show("Đọc file thất bại!", "Lỗi"); }
            }
        }
    }
}