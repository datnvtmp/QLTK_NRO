using System;
using System.Linq;
using System.Windows.Forms;
using QLTK_Lite.Config;
using QLTK_Lite.Models;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void BtnThem_Click(object sender, EventArgs e)
        {
            string username = txtTaiKhoan.Text.Trim();
            string password = txtPass.Text;
            string server = cboServer.SelectedItem?.ToString() ?? ServerConfig.LoadServers()[0];
            string proxy = txtProxy.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập Tài khoản!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus(); return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập Mật khẩu!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPass.Focus(); return;
            }

            accountList.Add(new AccountModel
            {
                IsSelected = false,
                ID = accountList.Count > 0 ? accountList.Max(a => a.ID) + 1 : 1,
                CharName = "",
                DataInGame = "",
                Username = username,
                Password = password,
                Server = server,  // ✅ lưu tên string, không phải index
                Status = "-",
                Proxy = string.IsNullOrEmpty(proxy) ? "None" : proxy
            });
            SaveAccountsToFile();
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0) return;

            if (dgvAccounts.SelectedRows.Count == 1)
            {
                var row = dgvAccounts.SelectedRows[0];
                if (row.DataBoundItem is AccountModel acc)
                {
                    acc.Username = txtTaiKhoan.Text.Trim();
                    acc.Password = txtPass.Text;
                    acc.Server = cboServer.SelectedItem?.ToString() ?? "Super 1";
                    acc.Proxy = string.IsNullOrEmpty(txtProxy.Text.Trim()) ? "None" : txtProxy.Text.Trim();
                }
            }
            else
            {
                // Chọn nhiều dòng: chỉ cập nhật Proxy
                foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                    if (row.DataBoundItem is AccountModel acc)
                        acc.Proxy = string.IsNullOrEmpty(txtProxy.Text.Trim()) ? "None" : txtProxy.Text.Trim();
            }
            dgvAccounts.Refresh();
            SaveAccountsToFile();
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0) return;
            foreach (DataGridViewRow row in dgvAccounts.SelectedRows)
                if (row.DataBoundItem is AccountModel acc)
                    accountList.Remove(acc);
            SaveAccountsToFile();
        }
    }
}