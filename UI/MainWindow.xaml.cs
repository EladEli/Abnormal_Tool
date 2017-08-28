using Abnormal_UI.UI.Abnormal;
using Abnormal_UI.UI.Samr;
using Abnormal_UI.UI.SimpleBind;
using Abnormal_UI.UI.Test;
using Abnormal_UI.UI.Vpn;

namespace Abnormal_UI.UI
{
    public partial class MainWindow 
    {
        private readonly AbnormalViewModel _abnormalModel;
        public AbnormalAttackUserControl AbnormalAttackWindow { get; set; }
        public LsbAttackUserControl LsbAttackWindow { get; set; }
        public TestUserControl TestWindow { get; set; }
        public VpnUserControl VpnWindow { get; set; }
        public SamrUserControl SamrWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            _abnormalModel = new AbnormalViewModel();
            _abnormalModel.PopulateModel();
            AbnormalAttackWindow = new AbnormalAttackUserControl(_abnormalModel);

            var sbModel = new SimpleBindViewModel();
            sbModel.PopulateModel();
            LsbAttackWindow = new LsbAttackUserControl(sbModel);

            var testModel = new TestViewModel();
            testModel.PopulateModel();
            TestWindow = new TestUserControl(testModel);
            
            var vpnModel = new VpnViewModel();
            vpnModel.PopulateModel();
            VpnWindow = new VpnUserControl(vpnModel);

            var samrModel = new SamrViewModel();
            samrModel.PopulateModel();
            SamrWindow = new SamrUserControl(samrModel);

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _abnormalModel.DbClient.DisposeDatabae();
        }
    }
}
