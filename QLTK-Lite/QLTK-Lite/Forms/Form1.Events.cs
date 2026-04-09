using QLTK_Lite.Models;
using System;
using System.Windows.Forms;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private void DgvAccounts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count != 1) return;
            var row = dgvAccounts.SelectedRows[0];
            if (row.DataBoundItem is AccountModel acc)
            {
                txtTaiKhoan.Text = acc.Username;
                txtPass.Text = acc.Password;
                cboServer.SelectedItem = acc.Server;
                txtProxy.Text = acc.Proxy == "None" ? "" : acc.Proxy;

                if (acc.Config != null)
                {
                    LoadConfigToForm(_tabAutoTrain, acc.Config);
                    LoadCSKBConfigToForm(_tabCSKB, acc.CSKBConfig);
                    LoadFarmManhConfigToForm(_tabFarmManh, acc.FarmManhConfig);
                }
                _logBox.Text = acc.LastInfo;
            }
        }

        private void DgvAccounts_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (!dgvAccounts.Rows[e.RowIndex].Selected)
            {
                dgvAccounts.ClearSelection();
                dgvAccounts.Rows[e.RowIndex].Selected = true;
                dgvAccounts.CurrentCell = dgvAccounts.Rows[e.RowIndex].Cells[e.ColumnIndex];
            }
        }
    }
}