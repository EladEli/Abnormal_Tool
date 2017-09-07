using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.SimpleBind
{
    public partial class LsbAttackUserControl
    {
        private readonly SimpleBindViewModel _model;
        public LsbAttackUserControl()
        {
            InitializeComponent();
        }
        public LsbAttackUserControl(SimpleBindViewModel model)
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

        private async void BtnLsbDistinct_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLsbDistinct.IsEnabled = false;
            var result = await Task.Run(() => _model.LsbDistinct());
            BtnLsbDistinct.IsEnabled = true;
            MessageBox.Show(result
                ? "User activity insertion ended."
                : "Please select at least 11 users and at least 1 machine!");
        }
        private async void BtnLsbIntense_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLsbIntense.IsEnabled = false;
            var result = await Task.Run(() => _model.LsbIntense());
            BtnLsbIntense.IsEnabled = true;
            MessageBox.Show(result
                ? "User activity insertion ended." 
                : "Please select at least 1 user and 1 machine!");
        }
        private async void BtnLsbSpecific_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLsbSpecific.IsEnabled = false;
            var result = await Task.Run(() => _model.LsbSingle());
            BtnLsbSpecific.IsEnabled = true;
            MessageBox.Show(result ? "User activity insertion ended." : "Please select at least 1 user and 1 machine!");
        }
    }
}
