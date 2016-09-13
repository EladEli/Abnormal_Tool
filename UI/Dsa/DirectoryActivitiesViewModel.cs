using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;


namespace Abnormal_UI.UI.Dsa
{
    public class DirectoryActivitiesViewModel : AttackViewModel
    {
        public Dictionary<string, string> DsaDictionary { get; set; }
        public List<string> SelectedActivitiesList { get; set; }
        public string SelectedUser { get; set; }
        public string SelectedComputer { get; set; }
        public string SelectedGroup { get; set; }

        public DirectoryActivitiesViewModel() : base()
        {
            PopulateDictionary();
        }

        public void PopulateDictionary()
        {
            DsaDictionary = new Dictionary<string, string>()
            {
                {"SecurityPrincipalCreated", "New-ADComputer -Name $computerName -SAMAccountName $computerName -Path \"CN=Computers,DC=domain1,DC=test,DC=local\""},
                {"AccountDelegationChanged", "command"},
                {"AccountConstrainedDelegationStateChanged", "command"},
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
                _dbClient.ClearDsaCollection();
                foreach (var command in SelectedActivitiesList.Select(selectedActivity => DsaDictionary[selectedActivity]))
                {
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
                _dbClient.ClearDsaCollection();
                foreach (var dsa in DsaDictionary)
                {

                    PowershellExec(dsa.Value);
                }
            }
            catch (Exception dsaEx)
            {
                Logger.Error(dsaEx);
            }
        }

        private void PowershellExec(string command)
        {
            try
            {
                using (var powerShellInstance = PowerShell.Create())
                {
                    powerShellInstance.AddScript(command);
                    powerShellInstance.AddParameter("computerName", SelectedComputer);
                    Logger.Debug($"Before invoke: {command}");
                    powerShellInstance.Invoke();
                    Logger.Debug(powerShellInstance.Streams.Error.ReadAll());
                    Logger.Debug("After invoke");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

        }
    }
}
