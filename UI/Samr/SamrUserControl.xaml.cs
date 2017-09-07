using System;
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
        private async void LearningButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
            LearningButton.IsEnabled = false;
            var result = await Task.Run(() => _model.ExecuteLearningTime());
            MessageBox.Show(result ? "Learning time inserted succesfully" : "Learning time failed");
            LearningButton.IsEnabled = true;
        }

        private async void SaButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
            SaButton.IsEnabled = false;
            var result = await Task.Run(() => _model.ExecuteSamrDetection());
            MessageBox.Show(result ? "SAMR Activities time inserted succesfully" : "SA failed");
            SaButton.IsEnabled = true;
        }

        private string GetRating()
        {
            return LowRateButton.IsChecked != null && (bool) LowRateButton.IsChecked ? "Low" : "High";
        }

        private void RatingButton_OnClick(object sender, RoutedEventArgs e) => _model.SamrCouples.Add(
            new SamrViewModel.CoupledSamr((EntityObject) BoxUsers.SelectedItem, (EntityObject) BoxMachines.SelectedItem,
                GetRating()));
    }
}
