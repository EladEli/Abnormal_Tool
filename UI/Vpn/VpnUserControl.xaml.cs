using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.Vpn
{
    public partial class VpnUserControl : UserControl
    {
        private readonly VpnViewModel _model;
        public VpnUserControl()
        {
            InitializeComponent();
        }
        public VpnUserControl(VpnViewModel model)
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

            selectedEntityObjects.AddRange(BoxMachines.SelectedItems.Cast<EntityObject>());
            _model.SelectedMachines = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxDCs.SelectedItems.Cast<EntityObject>());
            _model.SelectedDomainControllers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }

        private async void ExecuteVpnBtn_OnClickAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            ExecuteVpnBtn.IsEnabled = false;
            UpdateSelected();
            var result = await Task.Run(() => _model.ExecuteVpnActivity());
            ExecuteVpnBtn.IsEnabled = true;
            MessageBox.Show(result
                ? "Vpn activity insertion ended."
                : "Please select at least 1 user,machine,ip!");
        }

        private void AddIpBtn_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _model.ExternalIPs.Add(IpBox.Text);
            IpBox.Text = "0.0.0.0";
        }
    }
}
