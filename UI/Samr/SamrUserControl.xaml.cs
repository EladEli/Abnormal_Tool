using System.Threading.Tasks;
using System.Windows;

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
            var result = await Task.Run(() => _model.GenerateLearningTime());
            MessageBox.Show(result ? "Learning time inserted succesfully" : "Learning time failed");
            LearningButton.IsEnabled = true;
        }

        private async void SaButton_OnClickAsync(object sender, RoutedEventArgs e)
        {
            SaButton.IsEnabled = false;
            var result = await Task.Run(() => _model.GenerateSamr());
            MessageBox.Show(result ? "SAMR Activities time inserted succesfully" : "SA failed");
            SaButton.IsEnabled = true;
        }

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            Test.IsEnabled = false;
            var result = await Task.Run(() => _model.Test());
            MessageBox.Show(result ? "DB Change Succeeded" : "Big Bdad Bom");
            Test.IsEnabled = true;
        }
    }
}
