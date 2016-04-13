using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI
{
    public class AbnormalViewModel : AttackViewModel
    {
        public Boolean includeAs { get; set; }
        public Boolean includeTgs { get; set; }
        public Boolean includeEvent { get; set; }
        private int minMachines;
        public int MinMachines
        {
            get { return minMachines; }
            set
            {
                minMachines = value;
                OnPropertyChanged();
            }
        }
        private int maxMachines;
        public int MaxMachines
        {
            get { return maxMachines; }
            set
            {
                maxMachines = value;
                OnPropertyChanged();
            }
        }
        public AbnormalViewModel() : base()
        {
            minMachines = 1;
            maxMachines = 4;
            includeAs = true;
            includeTgs = false;
            includeEvent = false;
        }
        public bool ActivateUsers()
        {
            _dbClient.RenameKerbCollections();
            _dbClient.RenameNtlmEventsCollections();
            _dbClient.ClearTestNaCollection();
            SvcCtrl.StopService("ATACenter");
            _dbClient.SetCenterProfileForReplay();
            Logger.Debug("Center profile set for replay");
            List<BsonDocument> kerberosAss = new List<BsonDocument>();
            List<BsonDocument> kerberosTgss = new List<BsonDocument>();
            List<BsonDocument> NtlmEvents = new List<BsonDocument>();
            if (selectedMachinesList.Count / selectedEmpList.Count < 2)
            {
                return false;
            }

            int userIterationCounter = 0;
            int currentMachineCounter = 0;
            Random rnd = new Random();
            var machinesUsed = 0;
            var maxUsedMachines = 0;

            foreach (var selectedUser in selectedEmpList)
            {
                Logger.Debug("inserting normal activity for {0}",selectedUser.name);
                for (int daysToGenerate = 1; daysToGenerate <= 23; daysToGenerate++)
                {
                    var pcsUsedTodayCounter = rnd.Next(minMachines, maxMachines + 1);
                    var rcsUsedTodayCounter = rnd.Next(minMachines, maxMachines + 1);
                    maxUsedMachines = Math.Max(pcsUsedTodayCounter, rcsUsedTodayCounter);
                    if (includeAs)
                    {
                        for (int i = 0; i <= (pcsUsedTodayCounter - 1); i++)
                        {
                            if ((currentMachineCounter + pcsUsedTodayCounter) >= selectedMachinesList.Count - 1)
                            {
                                currentMachineCounter = 0;
                                    machinesUsed = machinesUsed + selectedMachinesList.Count;

                            }
                            EntityObject currentSelectedMachine =
                                selectedMachinesList[currentMachineCounter + i];
                            var activity = DocumentCreator.KerberosCreator(selectedUser, currentSelectedMachine, selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, null, null, "As", daysToGenerate);
                            kerberosAss.Add(activity);
                        }
                        _dbClient.InsertBatch(kerberosAss);
                        
                        Logger.Trace("Inserted {2} Kerberos AS activities for {0} on {1}", selectedUser.name, DateTime.UtcNow.Subtract(new TimeSpan(daysToGenerate, 0, 0, 0)), kerberosAss.Count);

                        kerberosAss = new List<BsonDocument>();
                    }

                    if (includeTgs)
                    {
                        for (int i = 0; i <= (rcsUsedTodayCounter - 1); i++)
                        {
                            if ((currentMachineCounter + rcsUsedTodayCounter) >= selectedMachinesList.Count - 1) { currentMachineCounter = 0; }
                            EntityObject currentSelectedMachine = selectedMachinesList[currentMachineCounter + i];
                            EntityObject defaultMachine = selectedMachinesList[currentMachineCounter];
                            var activity = DocumentCreator.KerberosCreator(selectedUser, defaultMachine, selectedDcsList.FirstOrDefault(), DomainName,sourceGateway, string.Format("CIFS/{0}", currentSelectedMachine), currentSelectedMachine, "Tgs", daysToGenerate);
                            kerberosTgss.Add(activity);
                        }
                        _dbClient.InsertBatch(kerberosTgss);
                        Logger.Trace("Inserted {2} Kerberos Tgs activities for {0} on {1}", selectedUser.name, DateTime.UtcNow.Subtract(new TimeSpan(daysToGenerate, 0, 0, 0)), kerberosTgss.Count);

                        kerberosTgss = new List<BsonDocument>();
                    }
                    if (includeEvent)
                    {
                        for (int i = 0; i <= (pcsUsedTodayCounter - 1); i++)
                        {
                            if ((currentMachineCounter + pcsUsedTodayCounter) >= selectedMachinesList.Count - 1)
                            {
                                currentMachineCounter = 0;
                                machinesUsed = machinesUsed + selectedMachinesList.Count;
                            }
                            EntityObject currentSelectedMachine = selectedMachinesList[currentMachineCounter + i];
                            var activity = DocumentCreator.EventCreator(selectedUser, currentSelectedMachine,
                                selectedDcsList.FirstOrDefault(), DomainName, sourceGateway,daysToGenerate);
                            NtlmEvents.Add(activity);
                        }
                        _dbClient.InsertBatch(NtlmEvents,false,false,true);
                        Logger.Trace("Inserted {2} Kerberos AS activities for {0} on {1}", selectedUser.name, DateTime.UtcNow.Subtract(new TimeSpan(daysToGenerate, 0, 0, 0)), kerberosAss.Count);
                        NtlmEvents = new List<BsonDocument>();
                    }
                }
                userIterationCounter++;
                currentMachineCounter = currentMachineCounter + maxUsedMachines;
                
            }
            machinesUsed = machinesUsed + currentMachineCounter;
            Logger.Debug("Done inserting normal activity");
            Logger.Debug("Used {0} machines",machinesUsed);
            SvcCtrl.StartService("ATACenter");
            Logger.Debug("Gone to sleep for 1 minute of user profilling");
            Thread.Sleep(60000);
            Logger.Debug("Woke up!");

            return true;
        }
        public bool AbnormalActivity(ObservableCollection<EntityObject> specificUser = null)
        {
            _dbClient.ClearTestNaCollection();
            SvcCtrl.RestartService("ATACenter");
            Logger.Debug("Gone to sleep for 2 minutes of tree build");
            Thread.Sleep(120000);
            Logger.Debug("Woke up!");
            SvcCtrl.StopService("ATACenter");

            List<BsonDocument> networkActivitities = new List<BsonDocument>();
            List<BsonDocument> ntlmEventActivitities = new List<BsonDocument>();
            int hoursCounter = 4;
            if (selectedEmpList.Count == 0 && specificUser == null)
            {
                return false;
            }
            else
            {
                if (specificUser != null)
                {
                    selectedEmpList = specificUser;
                }
                foreach (var selectedUser in selectedEmpList)
                {
                    Logger.Debug("inserting abnormal activity for {0}", selectedUser.name);
                    foreach (var selectedComputer in selectedMachinesList)
                    {
                        hoursCounter++;
                        var currentSelectedMachine = selectedComputer;
                        if (includeAs)
                        {
                            var Activity = DocumentCreator.KerberosCreator(selectedUser, currentSelectedMachine, selectedDcsList.FirstOrDefault(), DomainName,sourceGateway, null, null, "As", 0, hoursCounter);
                            networkActivitities.Add(Activity);
                            
                        }
                        Logger.Trace("Inserted {2} Kerberos As abnormal activities for {0} on {1}", selectedUser.name, DateTime.UtcNow, selectedMachinesList.Count);

                        if (includeTgs)
                        {
                            var Activity = DocumentCreator.KerberosCreator(selectedUser, currentSelectedMachine, selectedDcsList.FirstOrDefault(), DomainName,sourceGateway, string.Format("CIFS/{0}", currentSelectedMachine), currentSelectedMachine, "Tgs", 0, hoursCounter);
                            networkActivitities.Add(Activity);
                        }
                        if (includeEvent)
                        {
                            var Activity = DocumentCreator.EventCreator(selectedUser, currentSelectedMachine,
                                selectedDcsList.FirstOrDefault(), DomainName, sourceGateway);
                            ntlmEventActivitities.Add(Activity);

                        }
                    }
                    Logger.Debug("Expect abnormal activity on {0}",selectedUser.name);
                }
                _dbClient.InsertBatch(networkActivitities);
                _dbClient.InsertBatch(ntlmEventActivitities,false,false,true);
                Logger.Debug("Done inserting abnormal activity");
                SvcCtrl.StartService("ATACenter");
                return true;
            }
        }
        public string AutoAbnormal()
        {
            selectedEmpList = new ObservableCollection<EntityObject> { };
            selectedMachinesList = new ObservableCollection<EntityObject> { };
            selectedDcsList = new ObservableCollection<EntityObject> { };

            for (int i = 0; i < 70; i++) { selectedEmpList.Add(empList[i]); }
            for (int i = 0; i < 250; i++) { selectedMachinesList.Add(machinesList[i]); }
            selectedDcsList.Add(dcsList.FirstOrDefault());

            ActivateUsers();

            Random rnd2 = new Random();
            selectedMachinesList = new ObservableCollection<EntityObject> { };
            for (int i = 0; i < 10; i++) { selectedMachinesList.Add(machinesList[250 + i]); }

            AbnormalActivity(new ObservableCollection<EntityObject> { selectedEmpList[rnd2.Next(1, 60)] });

            //TriggerAbnormalModeling();
            return selectedEmpList[0].name;
        }
        public void TriggerAbnormalModeling()
        {
            _dbClient.TriggerAbnormalModeling();
        }

        public void setCenter()
        {
            _dbClient.SetCenterProfileForReplay();
        }
    }
}
