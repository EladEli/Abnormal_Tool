using System.Windows;
using System.Windows.Input;
using Abnormal_UI.UI.Abnormal;
using Abnormal_UI.UI.Dsa;
using Abnormal_UI.UI.Test;
using AbnormalAttackUserControl = Abnormal_UI.UI.Abnormal.AbnormalAttackUserControl;
using LsbAttackUserControl = Abnormal_UI.UI.SimpleBind.LsbAttackUserControl;

namespace Abnormal_UI.UI
{
   
    public partial class MainWindow 
    {
        private AbnormalViewModel _abnormalModel;
        private SimpleBindViewModel _sbModel;
        private TestViewModel _testModel;
        private DirectoryActivitiesViewModel _dsaModel;
        public AbnormalAttackUserControl _abnormalAttackWindow { get; set; }
        public LsbAttackUserControl _lsbAttackWindow { get; set; }
        public TestUserControl _testWindow { get; set; }
        public DirectoryActivitiesUserControl _dsaWindow { get; set; }


        public MainWindow()
        {
            InitializeComponent();
           
            _abnormalModel = new AbnormalViewModel();
            _abnormalModel.PopulateModel();
            _abnormalAttackWindow = new AbnormalAttackUserControl(_abnormalModel);

            _sbModel = new SimpleBindViewModel();
            _sbModel.PopulateModel();
            _lsbAttackWindow = new LsbAttackUserControl(_sbModel);

            _testModel = new TestViewModel();
            _testModel.PopulateModel();
            _testWindow = new TestUserControl(_testModel);

            _dsaModel = new DirectoryActivitiesViewModel();
            _dsaModel.PopulateModel();
            _dsaWindow = new DirectoryActivitiesUserControl(_dsaModel);

            DataContext = this;
        }

        private void Root_MouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            Close();
        }

    }
}
