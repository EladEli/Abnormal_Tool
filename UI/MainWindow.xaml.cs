using Abnormal_UI.Infra;
using Abnormal_UI.UI.Abnormal;
using Abnormal_UI.UI.BruteForce;
using Abnormal_UI.UI.Samr;
using Abnormal_UI.UI.SimpleBind;
using Abnormal_UI.UI.Test;
using Abnormal_UI.UI.Vpn;

namespace Abnormal_UI.UI
{
    public partial class MainWindow 
    {
        private readonly AbnormalViewModel abnormalModel;
        public AbnormalAttackUserControl AbnormalAttackWindow { get; set; }
        public LsbAttackUserControl LsbAttackWindow { get; set; }
        public TestUserControl TestWindow { get; set; }
        public VpnUserControl VpnWindow { get; set; }
        public SamrUserControl SamrWindow { get; set; }
        public BruteForceUserControl BruteForceWindow { get; set; }

        public MainWindow()
        {
            InitializeComponent();
           
            abnormalModel = new AbnormalViewModel();
            abnormalModel.PopulateModel();
            AbnormalAttackWindow = new AbnormalAttackUserControl(abnormalModel);

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

            var bruteForceModel = new BruteForceViewModel();
            bruteForceModel.PopulateModel();
            BruteForceWindow = new BruteForceUserControl(bruteForceModel);

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            abnormalModel.DbClient.DisposeDatabae();
        }
    }
}
