using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace QLTK_Lite
{
    // Tạo một class nhỏ để tự động hứng dữ liệu từ JSON (gọn và an toàn hơn)
    public class ItemData { public int id; public int iconID; public string name; public int quantity; }

    public class HanhTrangForm : Form
    {
        public HanhTrangForm(string accName, string json)
        {
            Text = $"Hành trang — {accName}";
            Size = new Size(580, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 46);
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };

            try
            {
                // Dùng Newtonsoft.Json để parse, tự động không phân biệt hoa/thường
                var items = JsonConvert.DeserializeObject<List<ItemData>>(json);

                if (items != null)
                {
                    foreach (var it in items)
                        panel.Controls.Add(MakeCard(it));
                }
            }
            catch (Exception ex)
            {
                // Ghi log ex.Message ra nếu cần debug xem chuỗi JSON bị lỗi form chỗ nào
            }

            Controls.Add(panel);
        }

        private Panel MakeCard(ItemData it)
        {
            var card = new Panel { Width = 235, Height = 74, Margin = new Padding(6), BackColor = Color.FromArgb(49, 50, 68), Cursor = Cursors.Hand };

            // Gom gọn việc tạo các Label/PictureBox vào một cụm AddRange
            card.Controls.AddRange(new Control[]
            {
                new PictureBox { Size = new Size(52, 52), Location = new Point(10, 11), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(24, 24, 37), Image = LoadIcon(it.iconID) },
                new Label { Text = it.name ?? "Không rõ", ForeColor = Color.FromArgb(205, 214, 244), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(70, 10), Size = new Size(155, 18), AutoEllipsis = true },
                new Label { Text = $"SL: x{it.quantity:N0}", ForeColor = Color.FromArgb(166, 227, 161), Font = new Font("Segoe UI Semibold", 9f), Location = new Point(70, 32), AutoSize = true },
                new Label { Text = $"ID: #{it.id}", ForeColor = Color.FromArgb(166, 173, 200), Font = new Font("Consolas", 8.5f), Location = new Point(70, 52), AutoSize = true }
            });

            // Gộp hàm xử lý hiệu ứng Hover cực ngắn
            Action<bool> setHover = hover => {
                if (hover || !card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)))
                    card.BackColor = hover ? Color.FromArgb(69, 71, 90) : Color.FromArgb(49, 50, 68);
            };

            card.MouseEnter += delegate { setHover(true); };
            card.MouseLeave += delegate { setHover(false); };
            foreach (Control c in card.Controls)
            {
                c.MouseEnter += delegate { setHover(true); };
                c.MouseLeave += delegate { setHover(false); };
            }

            return card;
        }

        private static Image LoadIcon(int iconID)
        {
            if (iconID < 0) return null;
            for (int z = 1; z <= 4; z++)
            {
                string p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "rms", $"{z}Small{iconID}");
                if (File.Exists(p)) return Image.FromStream(new MemoryStream(File.ReadAllBytes(p)));
            }
            return null;
        }
    }
}