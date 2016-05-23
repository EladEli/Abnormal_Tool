using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Abnormal_UI.UI.Dsa
{
    public partial class DirectoryActivitiesUserControl : UserControl
    {
        private readonly DirectoryActivitiesViewModel _model;
        public DirectoryActivitiesUserControl()
        {
            InitializeComponent();
        }
        public DirectoryActivitiesUserControl(DirectoryActivitiesViewModel model)
        {
            _model = model;
            InitializeComponent();
            DataContext = _model;
        }

        private void UpdateSelected()
        {
            var selectedActivitiesList = DsaListBox.SelectedItems.Cast<string>().ToList();
            _model._selectedActivitiesList = new List<string>(selectedActivitiesList);
            selectedActivitiesList.Clear();

            _model._selectedUser = UserTextBox.Text;
            _model._selectedComputer = ComputerTextBox.Text;
            _model._selectedGroup = GroupTextBox.Text;
        }

        private async void ActivateDsaBtn_ClickAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            ActivateDsaBtn.IsEnabled = false;
            UpdateSelected();
            await Task.Run(() => _model.ActivateDsa());
            ActivateDsaBtn.IsEnabled = true;
        }

        private async void AutoDsaBtn_ClickAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            AutoDsaBtn.IsEnabled = false;
            UpdateSelected();
            await Task.Run(() => _model.AutoDsa());
            AutoDsaBtn.IsEnabled = true;
        }
    }
}
