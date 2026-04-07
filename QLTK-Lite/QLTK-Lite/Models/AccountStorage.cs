using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using QLTK_Lite.Models;

namespace QLTK_Lite.Storage
{
    public static class AccountStorage
    {
        public static string DataFolder  => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib");
        public static string AccountFile => Path.Combine(DataFolder, "account");

        public static void EnsureFiles()
        {
            if (!Directory.Exists(DataFolder))  Directory.CreateDirectory(DataFolder);
            if (!File.Exists(AccountFile))       File.WriteAllText(AccountFile, "[]");
        }

        public static BindingList<AccountModel> LoadAccounts()
        {
            EnsureFiles();
            var list = JsonConvert.DeserializeObject<List<AccountModel>>(
                           File.ReadAllText(AccountFile)) ?? new List<AccountModel>();
            return new BindingList<AccountModel>(list);
        }

        public static void SaveAccounts(BindingList<AccountModel> accounts)
        {
            EnsureFiles();
            File.WriteAllText(AccountFile,
                JsonConvert.SerializeObject(accounts, Formatting.Indented));
        }
    }
}
