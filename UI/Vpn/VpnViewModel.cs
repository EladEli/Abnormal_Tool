using System.Collections.Generic;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.Vpn
{
    public class VpnViewModel : AttackViewModel
    {
        private int _recordsAmount;
        public int RecordsAmount
        {
            get { return _recordsAmount; }
            set
            {
                _recordsAmount = value;
                OnPropertyChanged();
            }
        }
        public List<string> ExternalIPs { get; set; }
       
        public VpnViewModel() : base()
        {
            ExternalIPs = new List<string>();
        }

        public bool ExecuteVpnActivity()
        {
            if (SelectedUsers.Count*SelectedMachines.Count* ExternalIPs.Count < _recordsAmount)
            {
                return false;
            }
            var ipsIndex = 0;
            var machinesIndex = 0;
            var vpnActivities = new List<BsonDocument>();
            var usersIndex = 0;
            for (var i=0;i<RecordsAmount;i++)
            {
                if (usersIndex >= SelectedUsers.Count) { usersIndex = 0;}
                if (machinesIndex >= SelectedMachines.Count) { machinesIndex = 0;}
                if (ipsIndex >= ExternalIPs.Count) { ipsIndex = 0;}
                vpnActivities.Add(DocumentCreator.VpnEventCreator(SelectedUsers[usersIndex],SelectedMachines[machinesIndex],SelectedDomainControllers.FirstOrDefault(),DomainName,SourceGateway, ExternalIPs[ipsIndex]));
                usersIndex++;
                machinesIndex++;
                ipsIndex++;
            }
            _dbClient.SetCenterProfileForReplay();
            SvcCtrl.StopService("ATACenter");
            _dbClient.InsertBatch(vpnActivities);
            SvcCtrl.StartService("ATACenter");
            return true;
        }
    }
}
