using System.Windows.Controls;

namespace Abnormal_UI.UI.Dsa
{
    /// <summary>
    /// Interaction logic for DirectoryActivitiesUserControl.xaml
    /// </summary>
    public partial class DirectoryActivitiesUserControl : UserControl
    {
        private DirectoryActivitiesViewModel _model;
        public DirectoryActivitiesUserControl()
        {
            InitializeComponent();
        }
        public DirectoryActivitiesUserControl(DirectoryActivitiesViewModel model)
        {
            _model = model;
            InitializeComponent();
            DataContext = _model;
        }
    }
}
