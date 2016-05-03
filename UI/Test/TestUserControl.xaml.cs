using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Abnormal_UI.Imported;

namespace Abnormal_UI.UI.Test
{
    public partial class TestUserControl : UserControl
    {
        private TestViewModel _model;
        private AbnormalViewModel _model2;
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
            List<EntityObject> selectedEntityObjects = new List<EntityObject>();
            foreach (EntityObject selectedItem in BoxUsers.SelectedItems)
            {
                selectedEntityObjects.Add(selectedItem);
            }
            _model.selectedEmpList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            foreach (EntityObject selectedItem in BoxMachines.SelectedItems)
            {
                selectedEntityObjects.Add(selectedItem);
            }
            _model.selectedMachinesList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            foreach (EntityObject selectedItem in BoxDCs.SelectedItems)
            {
                selectedEntityObjects.Add(selectedItem);
            }
            _model.selectedDcsList = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }

        private void Btn1_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            _model.InsertSeac();
        }

        private void Btn2_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSelected();
            _model.InsertAe();
        }

        private void Btn3_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _model2.setCenter();
            //_model.StopService("ATACenter");
        }
    }
}
