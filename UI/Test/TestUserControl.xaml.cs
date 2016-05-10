using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Abnormal_UI.Imported;

namespace Abnormal_UI.UI.Test
{
    public partial class TestUserControl : UserControl
    {
        private readonly TestViewModel _model;
        public TestUserControl()
        {
            InitializeComponent();
        }

        public TestUserControl(TestViewModel model)
        {
            _model = model;
            InitializeComponent();
            DataContext = _model;
        }

        private void UpdateSelected()
        {
            var selectedEntityObjects = BoxUsers.SelectedItems.Cast<EntityObject>().ToList();
            _model.selectedEmpList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxMachines.SelectedItems.Cast<EntityObject>());
            _model.selectedMachinesList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxDCs.SelectedItems.Cast<EntityObject>());
            _model.selectedDcsList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }

        private async void Btn1_OnClickAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            Btn1.IsEnabled = false;
            await Task.Run(() => _model.InsertSeac());
            Btn1.IsEnabled = true;
        }

        private async void Btn2_OnClickAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            await Task.Run(() => _model.InsertAe());
        }
    }
}
