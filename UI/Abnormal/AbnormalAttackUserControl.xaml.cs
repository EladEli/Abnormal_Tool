﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Imported;
using MessageBox = System.Windows.MessageBox;

namespace Abnormal_UI.UI.Abnormal
{
    public partial class AbnormalAttackUserControl
    {
        private readonly AbnormalViewModel _model;
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

        private async void BtnActivateUsers_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            _model.IsResultsShown = true;
            BtnActivateUsers.IsEnabled = false;
            var result = await Task.Run(() => _model.ActivateUsers());
            BtnActivateUsers.IsEnabled = true;
            MessageBox.Show(result
                            ? "User activity insertion ended."
                            : "Please make sure that user-computer ratio is at least 1-2");
            _model.IsResultsShown = false;
        }

        private async void BtnAbnormalActivity_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            _model.IsResultsShown = true;
            BtnAbnormalActivity.IsEnabled = false;
            var result = await Task.Run(() => _model.AbnormalActivity());
            BtnAbnormalActivity.IsEnabled = true;
            MessageBox.Show(result 
                            ? "User activity insertion ended." 
                            : "Please choose users");
            _model.IsResultsShown = false;
        }

        private void BtnMakeItRun_OnClick(object sender, RoutedEventArgs e)
        {
            _model.TriggerAbnormalModeling();
        }

        private async void BtnAutoAbnormal_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            BtnAutoAbnormal.IsEnabled = false;
            var result = await Task.Run(() => _model.AutoAbnormal());
            BtnAutoAbnormal.IsEnabled = true;
            MessageBox.Show("User activity insertion ended, you should expect an SA on " + result);
        }
    }
}
