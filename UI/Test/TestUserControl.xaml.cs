using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Abnormal_UI.Infra;

namespace Abnormal_UI.UI.Test
{
    public partial class TestUserControl
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
            _model.SelectedUsers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxMachines.SelectedItems.Cast<EntityObject>());
            _model.SelectedMachines = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();

            selectedEntityObjects.AddRange(BoxDCs.SelectedItems.Cast<EntityObject>());
            _model.SelectedDomainControllers = new ObservableCollection<EntityObject>(selectedEntityObjects);
            selectedEntityObjects.Clear();
        }

        private async void Btn1_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            Btn1.IsEnabled = false;
            await Task.Run(() => _model.InsertSeac());
            Btn1.IsEnabled = true;
        }

        private async void Btn2_OnClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            await Task.Run(() => _model.InsertAe());
        }

        private async void Btn4_ClickAsync(object sender, RoutedEventArgs e)
        {
            UpdateSelected();
            var result = false;
            Btn4.IsEnabled = false;
            if (_model.SaAmount != 0)
            {
                await Task.Run(() => _model.AddGateway());
                result = true;
            }
            Btn4.IsEnabled = true;
            MessageBox.Show(result
                ? "Gateway Creation ended."
                : "Please enter Gateways amount");
        }

        private async void GoldenTicketBtn_ClickAsync(object sender, RoutedEventArgs e)
        {
            GoldenTicketBtn.IsEnabled = false;
            await Task.Run(() => _model.GoldenTicketActivity());
            GoldenTicketBtn.IsEnabled = true;
        }
    }
}
