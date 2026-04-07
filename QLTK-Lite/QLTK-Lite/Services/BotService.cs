using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLTK_Lite.Models;

namespace QLTK_Lite.Services
{
    public class BotService : IDisposable
    {
        private readonly Dictionary<int, Process> _processes = new Dictionary<int, Process>();
        private bool _isRunning;
        private System.Threading.CancellationTokenSource _cts;

        public bool IsRunning => _isRunning;
        public event Action OnStarted;
        public event Action OnStopped;

        private static string ExePath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "lib", "NRO_Skia.exe");

        private Func<IEnumerable<AccountModel>> _getAccounts;
        public void SetAccountSource(Func<IEnumerable<AccountModel>> fn) => _getAccounts = fn;

        public bool Start()
        {
            if (_isRunning) return false;
            if (!File.Exists(ExePath))
            {
                MessageBox.Show("Không tìm thấy NRO_Skia.exe!", "Lỗi");
                return false;
            }
            _isRunning = true;
            _cts = new System.Threading.CancellationTokenSource();
            _ = RunLoop(_cts.Token); // bắt đầu vòng lặp, không block UI
            OnStarted?.Invoke();
            return true;
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _cts?.Cancel();
            KillAll();
            OnStopped?.Invoke();
        }

        public void KillProcess(int id)
        {
            if (!_processes.TryGetValue(id, out var p)) return;
            try { if (!p.HasExited) p.Kill(); } catch { }
            _processes.Remove(id);
        }

        public IEnumerable<string> GetRunningWindowTitles(IEnumerable<AccountModel> accounts)
        {
            foreach (var kv in _processes)
            {
                if (kv.Value.HasExited) continue;
                var acc = accounts.FirstOrDefault(a => a.ID == kv.Key);
                if (acc != null)
                    yield return RemoteService.BuildWindowTitle(acc.ID, acc.Username);
            }
        }

        private async Task RunLoop(System.Threading.CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var accounts = _getAccounts?.Invoke() ?? Enumerable.Empty<AccountModel>();
                var selectedIds = new HashSet<int>(accounts.Where(a => a.IsSelected).Select(a => a.ID));

                // kill những acc bị bỏ chọn
                foreach (var id in _processes.Keys.Where(id => !selectedIds.Contains(id)).ToList())
                    KillProcess(id);

                // mở những acc được chọn mà chưa chạy
                foreach (var acc in accounts.Where(a => a.IsSelected).ToList())
                {
                    if (token.IsCancellationRequested) break;

                    if (_processes.TryGetValue(acc.ID, out var ex))
                    {
                        if (!ex.HasExited) continue; // đang chạy → bỏ qua
                        _processes.Remove(acc.ID);   // chết rồi → xóa để mở lại
                    }

                    LaunchProcess(acc);
                    try { await Task.Delay(750, token); } catch (TaskCanceledException) { break; } // delay giữa mỗi nick
                }

                if (token.IsCancellationRequested) break;
                try { await Task.Delay(2000, token); } catch (TaskCanceledException) { break; } // nghỉ 2s sau khi xử lý xong, rồi mới check lại
            }
        }

        private void LaunchProcess(AccountModel acc)
        {
            if (!License.IsApproved)
            {
                Environment.Exit(0);
                return;
            }

            try
            {
                // Set TrainStartTime nếu chưa có (bật thủ công không qua Tự ON)
                if (!acc.TrainStartTime.HasValue)
                    acc.TrainStartTime = DateTime.Now;
                var title = RemoteService.BuildWindowTitle(acc.ID, acc.Username);
                var psi = new ProcessStartInfo(ExePath)
                {
                    WorkingDirectory = Path.GetDirectoryName(ExePath),
                    Arguments = $"--hidden --title \"{title}\" " +
                                $"--id {acc.ID} " +
                                $"--user \"{acc.Username}\" " +
                                $"--pass \"{acc.Password}\" " +
                                $"--server \"{acc.Server}\" " +
                                $"--proxy \"{acc.Proxy}\""
                };
                var proc = Process.Start(psi);
                if (proc != null) _processes[acc.ID] = proc;
            }
            catch { }
        }

        private void KillAll()
        {
            foreach (var p in _processes.Values)
                try { if (!p.HasExited) p.Kill(); } catch { }
            _processes.Clear();
        }

        public void Dispose() { Stop(); }
    }
}