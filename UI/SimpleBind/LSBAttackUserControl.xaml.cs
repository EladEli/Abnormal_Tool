using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Abnormal_UI.Imported;

namespace Abnormal_UI.UI
{
    public partial class LsbAttackUserControl
    {
        private SimpleBindViewModel _model;
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

        //private void RunLSBDistinct()
        //{
        //    UpdateSelected();
        //    var result = _model.LSBDistinct();
        //    if (result)
        //    {
        //        //MessageBox.Show("User activity insertion ended.");
        //        statusTB.Dispatcher.Invoke(new Action(() => statusTB.Text = "User activity insertion ended."),
        //            DispatcherPriority.Normal, null);
        //    }
        //    else
        //    {
        //        //MessageBox.Show("Please select at least 10 users and at least 1 machine!");
        //        statusTB.Dispatcher.Invoke(new Action(() => statusTB.Text = "Please select at least 10 users and at least 1 machine!"),
        //           DispatcherPriority.Normal, null);
        //    }
        //}
        //private void BtnLSBDistinct_OnClick(object sender, RoutedEventArgs e)
        //{
        //    Thread thread = new Thread(RunLSBDistinct);
        //    thread.Start();           
        //}
        private void BtnLSBDistinct_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.LSBDistinct();
            if (result)
            {
                MessageBox.Show("User activity insertion ended.");
                //statusTB.Dispatcher.Invoke(new Action(() => statusTB.Text = "User activity insertion ended."),
                  //  DispatcherPriority.Normal, null);
            }
            else
            {
                MessageBox.Show("Please select at least 10 users and at least 1 machine!");
                //statusTB.Dispatcher.Invoke(new Action(() => statusTB.Text = "Please select at least 10 users and at least 1 machine!"),
                  // DispatcherPriority.Normal, null);
            }
        }
        private void BtnLSBDintense_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.LSBIntense();
            if (result)
            {
                MessageBox.Show("User activity insertion ended.");
            }
            else
            {
                MessageBox.Show("Please select at least 1 user and 1 machine!");
            }
        }
        private void BtnLSBSpecific_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = _model.LSBSingle();
            if (result)
            {
                MessageBox.Show("User activity insertion ended.");
            }
            else
            {
                MessageBox.Show("Please select at least 1 user and 1 machine!");
            }
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
