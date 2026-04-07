using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLTK_Lite
{
    internal static class License
    {
        public static bool IsApproved { get; private set; } = false;

        private const string API_URL = "https://script.google.com/macros/s/AKfycbylMnuu3wokBCk1y8BwQZBJ4kLsEXw1ahVjyYBTfcgClazH6nZU8erDLj9PSa6O7sCzjw/exec?hwid=";
        private const string SECRET_SALT = "D@t_B0t_B1m_B1m_2026";

        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        public static async Task<bool> CheckAsync()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Environment.Exit(0);
            }

            // ✅ WMI chạy trên background thread, không chặn UI
            string hwid = await Task.Run(() => GetHWID());
            hwid = hwid.ToUpper().Trim();

            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                string requestUrl = $"{API_URL}{hwid}";
                string jsonResponse = await _httpClient.GetStringAsync(requestUrl);

                var data = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);
                string? status = (string?)data?["status"];
                string? serverToken = (string?)data?["token"];

                if (status == "SUCCESS" && !string.IsNullOrEmpty(serverToken))
                {
                    string expectedToken = GetMD5(hwid + SECRET_SALT);
                    if (serverToken.ToLower() == expectedToken.ToLower())
                    {
                        IsApproved = true;
                        return true;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "Kết nối tới Server API bị timeout (>8s).\nKiểm tra lại mạng hoặc cấu hình Web App.",
                    "Lỗi Timeout",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                ShowPendingForm(hwid);
                return false;
            }
            catch (Exception ex)
            {
                string trueError = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show(
                    $"Lỗi kết nối Server API: {trueError}\n" +
                    "Thường do Link Web App bị cấu hình sai quyền (Chưa chọn 'Anyone') hoặc mất mạng.",
                    "Lỗi API",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ShowPendingForm(hwid);
                return false;
            }

            ShowPendingForm(hwid);
            return false;
        }

        private static string GetMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private static void ShowPendingForm(string hwid)
        {
            var form = new Form
            {
                Text = "Chưa được cấp quyền",
                Width = 420,
                Height = 160,
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            form.Controls.Add(new Label
            {
                Text = "Key của bạn (Liên hệ Zalo 0333347606 AD Đạt bư cụ để dùng mod lỏ cak cak):",
                Left = 12,
                Top = 12,
                Width = 380,
                Height = 20
            });

            var txtHwid = new TextBox
            {
                Text = hwid,
                Left = 12,
                Top = 36,
                Width = 300,
                Height = 24,
                ReadOnly = true
            };

            var btnCopy = new Button
            {
                Text = "Copy",
                Left = 320,
                Top = 34,
                Width = 70,
                Height = 28
            };

            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(hwid);
                MessageBox.Show(
                    "Đã copy key!\nGửi cho admin qua Zalo 0333347606 để được cấp quyền dùng mod lỏ cak cak.",
                    "Thông báo");
            };

            form.Controls.Add(txtHwid);
            form.Controls.Add(btnCopy);
            form.ShowDialog();
        }

        /// <summary>
        /// Lần đầu: WMI chậm ~3-5s, cache lại Registry.
        /// Lần sau: đọc Registry ~0ms, không cần WMI nữa.
        /// </summary>
        private static string GetHWID()
        {
            const string regKey = @"SOFTWARE\QLTK_Lite";
            const string regVal = "hwid_cache";

            // Thử đọc cache từ Registry
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regKey))
                {
                    if (key != null)
                    {
                        string cached = key.GetValue(regVal) as string;
                        if (!string.IsNullOrEmpty(cached))
                            return cached; // ✅ Nhanh, không cần WMI
                    }
                }
            }
            catch { }

            // Lần đầu tiên: lấy từ WMI
            string hwid = GetHWIDFromWMI();

            // Cache vào Registry cho lần sau
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regKey))
                {
                    key?.SetValue(regVal, hwid);
                }
            }
            catch { }

            return hwid;
        }

        private static string GetHWIDFromWMI()
        {
            try
            {
                string cpuID = string.Empty;
                string diskID = string.Empty;

                using (var mc = new System.Management.ManagementClass("win32_processor"))
                using (var moc = mc.GetInstances())
                {
                    foreach (System.Management.ManagementObject mo in moc)
                    {
                        cpuID = mo.Properties["processorID"]?.Value?.ToString();
                        break;
                    }
                }

                using (var mc = new System.Management.ManagementClass("Win32_DiskDrive"))
                using (var moc = mc.GetInstances())
                {
                    foreach (System.Management.ManagementObject mo in moc)
                    {
                        diskID = mo.Properties["SerialNumber"]?.Value?.ToString();
                        break;
                    }
                }

                string rawHwid = $"{cpuID}-{diskID}-{Environment.MachineName}";

                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(rawHwid));
                    return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16);
                }
            }
            catch
            {
                return Environment.MachineName.GetHashCode().ToString("X16");
            }
        }
    }
}