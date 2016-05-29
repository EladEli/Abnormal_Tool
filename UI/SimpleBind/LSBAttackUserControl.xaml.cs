using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Abnormal_UI.Imported;

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
            _model.SelectedEmployees = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxMachines.SelectedItems.Cast<EntityObject>());
            _model.SelectedMachines = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxDCs.SelectedItems.Cast<EntityObject>());
            _model.SelectedDomainControllers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }

        private async void BtnLSBDistinct_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLSBDistinct.IsEnabled = false;
            var result = await Task.Run(() => _model.LSBDistinct());
            BtnLSBDistinct.IsEnabled = true;
            MessageBox.Show(result
                ? "User activity insertion ended."
                : "Please select at least 11 users and at least 1 machine!");
        }
        private async void BtnLSBDintense_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLSBIntense.IsEnabled = false;
            var result = await Task.Run(() => _model.LSBIntense());
            BtnLSBIntense.IsEnabled = true;
            MessageBox.Show(result
                ? "User activity insertion ended." 
                : "Please select at least 1 user and 1 machine!");
        }
        private async void BtnLSBSpecific_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnLSBSpecific.IsEnabled = false;
            var result = await Task.Run(() => _model.LSBSingle());
            BtnLSBSpecific.IsEnabled = true;
            MessageBox.Show(result ? "User activity insertion ended." : "Please select at least 1 user and 1 machine!");
        }
    }

    public static class ControlExtension
    {
        public static void UpdateControlSafe(this Control control, Action code)
        {
            if (!control.Dispatcher.CheckAccess())
                control.Dispatcher.BeginInvoke(code);
            else
                code.Invoke();
        }
    }
}
