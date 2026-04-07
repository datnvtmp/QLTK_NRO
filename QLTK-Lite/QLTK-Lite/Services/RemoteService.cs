using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QLTK_Lite.Services
{
    public static class RemoteService
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr h, int n);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr h, IntPtr a, int x, int y, int cx, int cy, uint f);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr h, out RECT r);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr h);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int left, top, right, bottom; }

        public static string BuildWindowTitle(int id, string username)
            => $"NRO_ID{id}_{username}";

        public static void ShowGameWindow(string title)
        {
            var h = FindWindow(null, title);
            if (h == IntPtr.Zero) return;
            ShowWindow(h, 9); // SW_RESTORE
            ShowWindow(h, 5); // SW_SHOW
            SetForegroundWindow(h);
        }

        public static void HideGameWindow(string title)
        {
            var h = FindWindow(null, title);
            if (h != IntPtr.Zero) ShowWindow(h, 0); // SW_HIDE
        }

        public static void ArrangeWindows(IEnumerable<string> titles)
        {
            var hwnds = new List<IntPtr>();
            foreach (var t in titles)
            {
                var h = FindWindow(null, t);
                if (h != IntPtr.Zero && IsWindowVisible(h)) hwnds.Add(h);
            }
            if (hwnds.Count == 0)
            {
                MessageBox.Show("Không có cửa sổ nào đang mở!", "Thông báo");
                return;
            }
            var screen = Screen.PrimaryScreen.WorkingArea;
            int curX = screen.Left, curY = screen.Top, rowH = 0;
            for (int i = 0; i < hwnds.Count; i++)
            {
                GetWindowRect(hwnds[i], out RECT r);
                int winW = r.right - r.left, winH = r.bottom - r.top;
                if (i > 0 && curX + winW > screen.Right) { curX = screen.Left; curY += rowH; rowH = 0; }
                SetWindowPos(hwnds[i], IntPtr.Zero, curX, curY, 0, 0, 0x0001);
                curX += winW;
                if (winH > rowH) rowH = winH;
            }
        }
    }
}