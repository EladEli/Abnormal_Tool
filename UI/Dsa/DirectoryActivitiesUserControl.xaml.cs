using System.Collections.Generic;
using System.Linq;
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
            _model._SelectedActivitiesList = selectedActivitiesList;
            selectedActivitiesList.Clear();
        }

        private void ActivateDsaBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            _model.ActivateDsa();
        }

        private void AutoDsaBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            _model.AutoDsa();
        }
    }
}
