using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.BruteForce
{
    public partial class BruteForceUserControl
    {
        private readonly BruteForceViewModel _model;
        public BruteForceUserControl(BruteForceViewModel model)
        {
            _model = model;
            InitializeComponent();
            DataContext = _model;
        }
        private void UpdateSelected()
        {
            var selectedEntityObjects = BoxUsers.SelectedItems.Cast<EntityObject>().ToList();
            _model.SelectedUsers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
            
            selectedEntityObjects.AddRange(BoxDCs.SelectedItems.Cast<EntityObject>());
            _model.SelectedDomainControllers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }
        private async void KerberonBtn_OnClickAsync(object sender, RoutedEventArgs e)
        {
            KerberosBtn.IsEnabled = false;
            UpdateSelected();
            var result = await Task.Run(() => _model.BruteForce(AuthType.Kerberos));
            MessageBox.Show(result 
                ? "BruteForce using Kerberos succedded" 
                : "Not enough users/password");
            KerberosBtn.IsEnabled = true;
        }
        private async void NtlmBtn_OnClickAsync(object sender, RoutedEventArgs e)
        {
            NtlmBtn.IsEnabled = false;
            UpdateSelected();
            var result = await Task.Run(() => _model.BruteForce(AuthType.Ntlm));
            MessageBox.Show(result
                ? "Brute force using Ntlm succedded"
                : "Not enough users/password");
            NtlmBtn.IsEnabled = true;
        }
        private async void LdapBtn_OnClickAsync(object sender, RoutedEventArgs e)
        {
            LdapBtn.IsEnabled = false;
            UpdateSelected();
            var result = await Task.Run(() => _model.BruteForce(AuthType.Basic));
            MessageBox.Show(result
                ? "Brute force using Ldap succedded"
                : "Not enough users/password");
            LdapBtn.IsEnabled = true;
        }
        private void LoadBtn_OnClickAsync(object sender, RoutedEventArgs e)
        {
            LoadBtn.IsEnabled = false;
            var result = _model.LoadDictionary();
            MessageBox.Show(result
                ? "Dictionary loaded."
                : "Invalid dictionary");
            LoadBtn.IsEnabled = true;
        }
    }
}
