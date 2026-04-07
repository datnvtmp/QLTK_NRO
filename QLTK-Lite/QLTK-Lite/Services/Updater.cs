using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QLTK_Lite
{
    internal static class Updater
    {
        // ══════════════════════════════════════════════════════
        // ĐỔI DÒNG NÀY MỖI LẦN RELEASE (khớp với tag trên GitHub)
        // ══════════════════════════════════════════════════════
        public const string CURRENT_VERSION = "v1.2.1";
        // ══════════════════════════════════════════════════════

        private const string BASE_URL = "https://github.com/datnv2k4/Lite/releases/latest/download";
        private const string API_URL = "https://api.github.com/repos/datnv2k4/Lite/releases/latest";
        private const string GAME_EXE_NAME = "NRO_Skia.exe";

        public static bool HasNewVersion { get; private set; } = false;
        public static string LatestVersion { get; private set; } = "";

        private static ProgressForm? _progressForm;

        /// <summary>
        /// Gọi trong Main() trước Application.Run().
        /// Trả về true nếu đang update → Main() nên return luôn.
        /// </summary>
        public static bool CheckAndUpdate()
        {
            try
            {
                using var http = CreateHttpClient();

                // ── 1. Lấy version mới nhất từ GitHub API ────
                // ── 1. Lấy version mới nhất từ GitHub API ────
                try
                {
                    var json = http.GetStringAsync(API_URL).Result;
                    LatestVersion = Regex.Match(json, "\"tag_name\":\"([^\"]+)\"").Groups[1].Value;
                    if (string.IsNullOrEmpty(LatestVersion)) return false;

                    // Chuyển đổi chuỗi "v1.0.x" thành đối tượng Version để so sánh (bỏ chữ 'v')
                    Version vCurrent = new Version(CURRENT_VERSION.TrimStart('v'));
                    Version vLatest = new Version(LatestVersion.TrimStart('v'));

                    // LOGIC ĐÚNG: Chỉ cập nhật nếu bản trên Server LỚN HƠN bản máy
                    if (vLatest <= vCurrent)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

                if (LatestVersion == CURRENT_VERSION) return false;

                // ── 2. Đánh dấu có update ─────────────────────
                HasNewVersion = true;

                // ── 3. Hỏi user ──────────────────────────────
                var answer = MessageBox.Show(
                    $"Có bản cập nhật mới!\n\n" +
                    $"  Phiên bản hiện tại : {CURRENT_VERSION}\n" +
                    $"  Phiên bản mới      : {LatestVersion}\n\n" +
                    $"Cập nhật ngay?",
                    "Cập nhật",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (answer != DialogResult.Yes) return false;

                // ── 4. Chuẩn bị đường dẫn ────────────────────
                var appExe = Application.ExecutablePath;
                var appDir = Path.GetDirectoryName(appExe)!;
                var appNew = appExe + ".new";
                var gameNew = Path.Combine(Path.GetTempPath(), GAME_EXE_NAME + ".new");

                var gameExes = Directory.GetFiles(appDir, GAME_EXE_NAME, SearchOption.AllDirectories);

                // ── 5. Hiện progress form ─────────────────────
                _progressForm = new ProgressForm();
                _progressForm.Show();

                // ── 6. Download QLTK_Lite.exe ─────────────────
                var appFileName = Path.GetFileName(appExe);
                _progressForm.SetStatus($"Đang tải {appFileName}...", 0);
                DownloadFile(http, $"{BASE_URL}/{appFileName}", appNew);

                // ── 7. Download NRO_Skia.exe ──────────────────
                _progressForm.SetStatus($"Đang tải {GAME_EXE_NAME}...", 50);
                DownloadFile(http, $"{BASE_URL}/{GAME_EXE_NAME}", gameNew);

                _progressForm.SetStatus("Chuẩn bị cài đặt...", 90);

                // ── 8. Kiểm tra file hợp lệ ──────────────────
                ValidateFile(appNew, minSizeKb: 50);
                ValidateFile(gameNew, minSizeKb: 100);

                // ── 9. Sinh file .bat ─────────────────────────
                var bat = BuildBat(appExe, appNew, gameExes, gameNew);
                var batPath = Path.Combine(Path.GetTempPath(), "qltk_update.bat");
                File.WriteAllText(batPath, bat, System.Text.Encoding.Default);

                _progressForm.SetStatus("Đang khởi động trình cài đặt...", 100);
                _progressForm.Close();

                // ── 10. Chạy bat và thoát ─────────────────────
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = batPath,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                });

                return true;
            }
            catch (Exception ex)
            {
                _progressForm?.Close();
                MessageBox.Show(
                    $"Cập nhật thất bại:\n\n{ex.Message}\n\nTiếp tục chạy phiên bản hiện tại.",
                    "Lỗi cập nhật",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
        }

        // ══════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Add("User-Agent", "QLTK-Updater");
            return client;
        }

        private static void DownloadFile(HttpClient http, string url, string savePath)
        {
            var bytes = http.GetByteArrayAsync(url).Result;
            if (bytes == null || bytes.Length == 0)
                throw new Exception($"File tải về rỗng: {url}");
            File.WriteAllBytes(savePath, bytes);
        }

        private static void ValidateFile(string path, int minSizeKb)
        {
            if (!File.Exists(path))
                throw new Exception($"Không tìm thấy file: {path}");
            var info = new FileInfo(path);
            if (info.Length < minSizeKb * 1024)
                throw new Exception($"File quá nhỏ, có thể tải lỗi: {path}");
        }

        private static string BuildBat(
            string appExe, string appNew,
            string[] gameExes, string gameNew)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("chcp 65001 >nul");
            sb.AppendLine("echo Dang cap nhat, vui long cho...");
            sb.AppendLine("timeout /t 2 /nobreak >nul");

            sb.AppendLine($"taskkill /f /im {GAME_EXE_NAME} >nul 2>&1");
            sb.AppendLine("timeout /t 1 /nobreak >nul");

            sb.AppendLine($"move /y \"{appNew}\" \"{appExe}\"");
            sb.AppendLine("if errorlevel 1 (");
            sb.AppendLine("    echo Loi khi cap nhat QLTK_Lite.exe!");
            sb.AppendLine("    pause");
            sb.AppendLine("    goto :end");
            sb.AppendLine(")");

            foreach (var gameExe in gameExes)
            {
                sb.AppendLine($"copy /y \"{gameNew}\" \"{gameExe}\"");
                sb.AppendLine("if errorlevel 1 (");
                sb.AppendLine($"    echo Loi khi cap nhat {GAME_EXE_NAME}!");
                sb.AppendLine("    pause");
                sb.AppendLine(")");
            }

            sb.AppendLine($"del \"{gameNew}\" >nul 2>&1");
            sb.AppendLine("echo Cap nhat thanh cong!");
            sb.AppendLine($"start \"\" \"{appExe}\"");
            sb.AppendLine(":end");
            sb.AppendLine("del \"%~f0\"");

            return sb.ToString();
        }
    }

    // ══════════════════════════════════════════════════════════
    // PROGRESS FORM
    // ══════════════════════════════════════════════════════════
    internal class ProgressForm : Form
    {
        private readonly System.Windows.Forms.ProgressBar _bar;
        private readonly Label _label;

        public ProgressForm()
        {
            Text = "Đang cập nhật...";
            Width = 400;
            Height = 120;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ControlBox = false;

            _label = new Label
            {
                Text = "Đang kết nối...",
                Left = 12,
                Top = 12,
                Width = 360,
                Height = 20,
            };

            _bar = new System.Windows.Forms.ProgressBar
            {
                Left = 12,
                Top = 40,
                Width = 360,
                Height = 24,
                Minimum = 0,
                Maximum = 100,
            };

            Controls.Add(_label);
            Controls.Add(_bar);
        }

        public void SetStatus(string msg, int percent)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => SetStatus(msg, percent)));
                return;
            }
            _label.Text = msg;
            _bar.Value = Math.Max(0, Math.Min(100, percent));
            Application.DoEvents();
        }
    }
}