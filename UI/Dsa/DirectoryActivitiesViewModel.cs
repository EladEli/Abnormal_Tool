using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.Dsa
{
    public class DirectoryActivitiesViewModel : AttackViewModel
    {
        public Dictionary<string, string> _DsaDictionary { get; set; }
        public List<string> _SelectedActivitiesList { get; set; }
        public string _selectedUser { get; set; }
        public string _selectedComputer { get; set; }
        public string _selectedGroup { get; set; }

        public DirectoryActivitiesViewModel() : base()
        {
            PopulateDictionary();
        }

        public void PopulateDictionary()
        {
            _DsaDictionary = new Dictionary<string, string>()
            {
                {"SecurityPrincipalCreated", "command"},
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

        
        public async void ActivateDsa()
        {
            try
            {
                _dbClient.SetGatewayProfileForDsa();
                foreach (var command in _SelectedActivitiesList.Select(dsa => _DsaDictionary[dsa]))
                {
                    await Task.Run(() => PowershellExec(command));
                }
            }
            catch (Exception dsaEx)
            {
               Logger.Error(dsaEx);
            }
            
        }

        public async void AutoDsa()
        {
            try
            {
                _dbClient.SetGatewayProfileForDsa();
                foreach (var dsa in _DsaDictionary)
                {
                    await Task.Run(() => PowershellExec(dsa.Value));
                }
            }
            catch (Exception dsaEx)
            {
                Logger.Error(dsaEx);
            }
        }

        public void PowershellExec(string command)
        {
            
        }
    }
}
