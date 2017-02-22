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
                OnPropertyChanged(nameof(RecordsAmount));
            }
        }
        public VpnViewModel()
        {
            ExternalIPs = new ObservableCollection<string>();
            RecordsAmount = 1;
        }
 
        public string GetRandomIp()
        {
            var random = new Random();
            System.Threading.Thread.Sleep(20);
            return $"{random.Next(0, 223)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}";

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
                    SelectedMachines[machinesIndex], DomainControllers.FirstOrDefault(), DomainObject.Name,
                    SourceGateway, ExternalIPs[ipsIndex]));
                Logger.Debug($"Inserted Vpn activity for {SelectedUsers[usersIndex]} on IP: {ExternalIPs[ipsIndex]}");
                usersIndex++;
                machinesIndex++;
                ipsIndex++;
            }
            DbClient.SetCenterProfileForReplay();
            DbClient.SetCenterProfileForVpn();
            SvcCtrl.StopService("ATACenter");
            DbClient.InsertBatch(vpnActivities);
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
            var machinesIndex = 0;
            var vpnActivities = new List<BsonDocument>();
            var usersIndex = 0;
            for (var i = 0; i < _recordsAmount; i++)
            {
                if (usersIndex >= SelectedUsers.Count) { usersIndex = 0; }
                if (machinesIndex >= SelectedMachines.Count) { machinesIndex = 0; }
                var ipAddress = GetRandomIp();
                System.Threading.Thread.Sleep(50);
                vpnActivities.Add(DocumentCreator.VpnEventCreator(SelectedUsers[usersIndex],
                    SelectedMachines[machinesIndex], DomainControllers.FirstOrDefault(), DomainObject.Name,
                    SourceGateway, ipAddress));
                Logger.Debug($"Inserted Vpn activity for {SelectedUsers[usersIndex]} on IP: {ipAddress}");
                usersIndex++;
                machinesIndex++;
            }
            DbClient.SetCenterProfileForReplay();
            DbClient.SetCenterProfileForVpn();
            SvcCtrl.StopService("ATACenter");
            DbClient.InsertBatch(vpnActivities);
            SvcCtrl.StartService("ATACenter");
            Logger.Debug("Done inserting vpn activities");
            return true;
        }
    }
}
