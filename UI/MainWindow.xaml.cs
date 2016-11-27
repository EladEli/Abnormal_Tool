﻿using Abnormal_UI.UI.Abnormal;
using Abnormal_UI.UI.SimpleBind;
using Abnormal_UI.UI.Test;
using Abnormal_UI.UI.Vpn;

namespace Abnormal_UI.UI
{
    public partial class MainWindow 
    {
        private readonly AbnormalViewModel _abnormalModel;
        private readonly SimpleBindViewModel _sbModel;
        private readonly TestViewModel _testModel;
        private readonly VpnViewModel _vpnModel;
        public AbnormalAttackUserControl _abnormalAttackWindow { get; set; }
        public LsbAttackUserControl _lsbAttackWindow { get; set; }
        public TestUserControl _testWindow { get; set; }
        public VpnUserControl _vpnWindow { get; set; }


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
            
            _vpnModel = new VpnViewModel();
            _vpnModel.PopulateModel();
            _vpnWindow = new VpnUserControl(_vpnModel);

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _abnormalModel._dbClient.DisposeDatabae();
        }
    }
}
