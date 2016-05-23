using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Windows;


namespace Abnormal_UI.UI.Dsa
{
    public class DirectoryActivitiesViewModel : AttackViewModel
    {
        public Dictionary<string, string> _dsaDictionary { get; set; }
        public List<string> _selectedActivitiesList { get; set; }
        public string _selectedUser { get; set; }
        public string _selectedComputer { get; set; }
        public string _selectedGroup { get; set; }

        public DirectoryActivitiesViewModel() : base()
        {
            PopulateDictionary();
        }

        public void PopulateDictionary()
        {
            _dsaDictionary = new Dictionary<string, string>()
            {
                {"SecurityPrincipalCreated", "command"},
                {"AccountDelegationChanged", "command"},
                {"AccountConstrainedDelegationStateChanged", "death"},
                {"AccountConstrainedDelegationSpnsChanged", "command"},
                {"ComputerOperatingSystemChanged", "command"},
                {"AccountPasswordChanged", "command"},
                {"AccountDisabledChanged", "command"},
                {"AccountExpiryTimeChanged", "command"},
                {"AccountExpired", "command"},
                {"AccountPasswordNeverExpiresChanged", "command"},
                {"AccountPasswordNotRequiredChanged", "command"},
                {"AccountSmartcardRequiredChanged", "command"},
                {"AccountPasswordExpired", "command"},
                {"UserTitleChanged", "command"},
                {"UserPhoneNumberChanged", "command"},
                {"UserMailChanged", "command"},
                {"UserManagerChanged", "command"},
                {"SecurityPrincipalSamNameChanged", "command"},
                {"AccountUpnNameChanged", "command"},
                {"GroupMembershipChanged", "command"},
                {"AccountSupportedEncryptionTypesChanged", "command"},
                {"SecurityPrincipalNameChanged", "command"},
                {"AccountLockedChanged", "command"},
                {"SecurityPrincipalPathChanged", "command"},
                {"SecurityPrincipalDeletedChanged", "command"},
            };

        }

        
        public void ActivateDsa()
        {
            try
            {
                _dbClient.SetGatewayProfileForDsa();
                _dbClient.CleaDsaCollection();
                foreach (var command in _selectedActivitiesList.Select(selectedActivity => _dsaDictionary[selectedActivity]))
                {
                    MessageBox.Show(command);
                    PowershellExec(command);
                }
            }
            catch (Exception dsaEx)
            {
               Logger.Error(dsaEx);
            }
            
        }

        public void AutoDsa()
        {
            try
            {
                _dbClient.SetGatewayProfileForDsa();
                _dbClient.CleaDsaCollection();
                foreach (var dsa in _dsaDictionary)
                {

                    PowershellExec(dsa.Value);
                }
            }
            catch (Exception dsaEx)
            {
                Logger.Error(dsaEx);
            }
        }

        private static void PowershellExec(string command)
        {
            using (var powerShellInstance = PowerShell.Create())
            {
                powerShellInstance.AddScript(command);
                powerShellInstance.Invoke();
            }
        }
    }
}
