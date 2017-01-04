using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;
using System;

namespace Abnormal_UI.UI.Vpn
{
    public class VpnViewModel : AttackViewModel
    {
        public ObservableCollection<string> ExternalIPs { get; set; }
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
        public VpnViewModel()
        {
            ExternalIPs = new ObservableCollection<string>();
            RecordsAmount = 1;
        }
 
        public string GetRandomIp()
        {
            Random _random = new Random();
            System.Threading.Thread.Sleep(20);
            return string.Format("{0}.{1}.{2}.{3}", _random.Next(0, 223), _random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255));

        }
        public bool ExecuteVpnActivity()
        {
            if (SelectedUsers.Count*SelectedMachines.Count* ExternalIPs.Count < 1)
            {
                return false;
            }
            var ipsIndex = 0;
            var machinesIndex = 0;
            var vpnActivities = new List<BsonDocument>();
            var usersIndex = 0;
            for (var i=0;i<_recordsAmount; i++)
            {
                if (usersIndex >= SelectedUsers.Count) { usersIndex = 0;}
                if (machinesIndex >= SelectedMachines.Count) { machinesIndex = 0;}
                if (ipsIndex >= ExternalIPs.Count) { ipsIndex = 0;}
                vpnActivities.Add(DocumentCreator.VpnEventCreator(SelectedUsers[usersIndex],
                    SelectedMachines[machinesIndex], DomainControllers.FirstOrDefault(), DomainName,
                    SourceGateway, ExternalIPs[ipsIndex]));
                Logger.Debug($"Inserted Vpn activity for {SelectedUsers[usersIndex]} on IP: {ExternalIPs[ipsIndex]}");
                usersIndex++;
                machinesIndex++;
                ipsIndex++;
            }
            _dbClient.SetCenterProfileForReplay();
            //_dbClient.SetCenterProfileForVpn();
            SvcCtrl.StopService("ATACenter");
            _dbClient.InsertBatch(vpnActivities);
            SvcCtrl.StartService("ATACenter");
            Logger.Debug("Done inserting vpn activities");
            return true;
        }

        public bool ExecuteAutoVpnActivity()
        {
            if (SelectedUsers.Count * SelectedMachines.Count < 1)
            {
                return false;
            }
            var ipsIndex = 0;
            var machinesIndex = 0;
            var vpnActivities = new List<BsonDocument>();
            var usersIndex = 0;
            var ip_address = "";
            for (var i = 0; i < _recordsAmount; i++)
            {
                if (usersIndex >= SelectedUsers.Count) { usersIndex = 0; }
                if (machinesIndex >= SelectedMachines.Count) { machinesIndex = 0; }
                ip_address = GetRandomIp();
                System.Threading.Thread.Sleep(50);
                vpnActivities.Add(DocumentCreator.VpnEventCreator(SelectedUsers[usersIndex],
                    SelectedMachines[machinesIndex], DomainControllers.FirstOrDefault(), DomainName,
                    SourceGateway, ip_address));
                Logger.Debug($"Inserted Vpn activity for {SelectedUsers[usersIndex]} on IP: {ip_address}");
                usersIndex++;
                machinesIndex++;
                ipsIndex++;
            }
            _dbClient.SetCenterProfileForReplay();
            //_dbClient.SetCenterProfileForVpn();
            SvcCtrl.StopService("ATACenter");
            _dbClient.InsertBatch(vpnActivities);
            SvcCtrl.StartService("ATACenter");
            Logger.Debug("Done inserting vpn activities");
            return true;
        }
    }
}
