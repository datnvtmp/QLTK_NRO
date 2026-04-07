using System.Drawing;
using System.Windows.Forms;
using QLTK_Lite.Models;

namespace QLTK_Lite
{
    public partial class Form1 : Form
    {
        private Button _btnBat;
        private Button _btnTat;
        private Button _btnUp;
        private Button _btnDown;
        private readonly Color _colorDefault = Color.FromArgb(245, 245, 245);

        private void CreateBotButtons(Panel parent, int x, int y)
        {
            _btnBat = CreateButton(parent, "▶ BẬT", x, y, 75, _colorDefault, Color.Black);
            _btnTat = CreateButton(parent, "⏸ TẮT", x + 80, y, 75, _colorDefault, Color.Black);
            _btnUp = CreateButton(parent, "↑", x + 160, y, 22, _colorDefault, Color.Black);
            _btnDown = CreateButton(parent, "↓", x + 187, y, 22, _colorDefault, Color.Black);

            _btnBat.Click += (s, e) => _botService.Start();
            _btnTat.Click += (s, e) => _botService.Stop();
            _btnUp.Click += (s, e) => MoveAccount(-1);
            _btnDown.Click += (s, e) => MoveAccount(1);
        }

        private void MoveAccount(int direction)
        {
            if (dgvAccounts.SelectedRows.Count != 1) return;

            var selectedRow = dgvAccounts.SelectedRows[0];
            var acc = selectedRow.DataBoundItem as AccountModel;
            if (acc == null) return;

            int oldIndex = accountList.IndexOf(acc);
            int newIndex = oldIndex + direction;

            if (newIndex >= 0 && newIndex < accountList.Count)
            {
                accountList.RemoveAt(oldIndex);
                accountList.Insert(newIndex, acc);
                dgvAccounts.ClearSelection();
                dgvAccounts.Rows[newIndex].Selected = true;
                this.SaveAccountsToFile();
            }
        }

        private void OnBotStarted()
        {
            _btnBat.Enabled = false;
            _btnBat.BackColor = Color.FromArgb(40, 167, 69);
            _btnBat.ForeColor = Color.White;
            _btnTat.Enabled = true;
            _btnTat.BackColor = _colorDefault;
            _btnTat.ForeColor = Color.Black;
        }

        private void OnBotStopped()
        {
            _btnTat.Enabled = false;
            _btnTat.BackColor = Color.FromArgb(220, 53, 69);
            _btnTat.ForeColor = Color.White;
            _btnBat.Enabled = true;
            _btnBat.BackColor = _colorDefault;
            _btnBat.ForeColor = Color.Black;
        }
    }
}