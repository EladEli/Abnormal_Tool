using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.Samr
{
    public partial class SamrUserControl
    {
        private readonly SamrViewModel _model;
        public SamrUserControl()
        {
            InitializeComponent();
        }
        public SamrUserControl(SamrViewModel model)
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

        private async void LearningButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
            LearningButton.IsEnabled = false;
            UpdateSelected();
            var result = await Task.Run(() => _model.GenerateLearningTime());
            MessageBox.Show(result ? "Learning time inserted succesfully" : "Learning time failed");
            LearningButton.IsEnabled = true;
        }

        private void SaButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
        }
    }
}
