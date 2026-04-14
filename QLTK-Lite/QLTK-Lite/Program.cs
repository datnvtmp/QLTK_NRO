using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLTK_Lite
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            // Ép buộc dùng TLS 1.2 để Update hoạt động trên Windows 8
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!await License.CheckAsync()) return;
            if (Updater.CheckAndUpdate()) return;
            var mainForm = new Form1();
            Updater.StartPeriodicUpdateCheck(mainForm);
            Application.Run(mainForm);
        }
    }
}
