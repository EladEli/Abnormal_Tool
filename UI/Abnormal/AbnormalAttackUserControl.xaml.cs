using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Abnormal_UI.Imported;

namespace Abnormal_UI.UI
{
    public partial class AbnormalAttackUserControl
    {
        private AbnormalViewModel _model;
        public AbnormalAttackUserControl()
        {
            InitializeComponent();
        }
        public AbnormalAttackUserControl(AbnormalViewModel model)
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

        private void BtnActivateUsers_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.ActivateUsers();
            if (result)
            {
                MessageBox.Show("User activity insertion ended.");
            }
            else
            {
                MessageBox.Show("Please make sure that user-computer ratio is at least 1-2");
            }
        }

        private void BtnAbnormalActivity_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.AbnormalActivity();
            if (result)
            {
                MessageBox.Show("User activity insertion ended.");
            }
            else
            {
                MessageBox.Show("Please choose users");
            }
        }

        private void BtnMakeItRun_OnClick(object sender, RoutedEventArgs e)
        {
            _model.TriggerAbnormalModeling();
        }

        private void BtnAutoAbnormal_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.AutoAbnormal();
            MessageBox.Show("User activity insertion ended, you should expect an SA on " + result);
        }

        private void b_Click(object sender, RoutedEventArgs e)
        {
           // UpdateSelected();
           // var result = _model.InsertSEAC();
           // MessageBox.Show("Ended inserting SA's");

           _model.setCenter();
           MessageBox.Show("SetCenter");
            

           // MessageBox.Show(_model.GetGWIDS().ToString());
           
        }
    }
}
