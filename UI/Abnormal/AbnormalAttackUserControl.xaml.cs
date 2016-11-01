using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Infra;

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

        private async void ActivateUsers_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            _model.IsResultsShown = true;
            BtnActivateUsers.IsEnabled = false;
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
            var result = await Task.Run(() => _model.ActivateUsers());
            BtnActivateUsers.IsEnabled = true;
            MessageBox.Show(result
                            ? "User activity insertion ended."
                            : $"Please make sure that user-computer ratio is at least 1-{_model.MaxMachines}");
            _model.IsResultsShown = false;
        }

        private async void AbnormalActivity_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            _model.IsResultsShown = true;
            BtnAbnormalActivity.IsEnabled = false;
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
            var result = await Task.Run(() => _model.AbnormalActivity());
            BtnAbnormalActivity.IsEnabled = true;
            MessageBox.Show(result 
                            ? "User activity insertion ended." 
                            : "Please choose users");
            _model.IsResultsShown = false;
        }

        private async void ResetAbnormalProfile_OnClickAsync(object sender, RoutedEventArgs e)
        {
            ResetAbnormalProfile.IsEnabled = false;
            await Task.Run(() => _model.ResetAbnormalProfile());
            MessageBox.Show("Abnormal profile restarted");
            ResetAbnormalProfile.IsEnabled = true;
        }

        private async void AutoAbnormal_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            _model.IsResultsShown = true;
            BtnAutoAbnormal.IsEnabled = false;
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
            var result = await Task.Run(() => _model.AutoAbnormal());
            BtnAutoAbnormal.IsEnabled = true;
            _model.IsResultsShown = false;
            MessageBox.Show("User activity insertion ended, you should expect an SA on " + result);
            BtnAbnormalActivity.IsEnabled = true;
        }
    }
}
