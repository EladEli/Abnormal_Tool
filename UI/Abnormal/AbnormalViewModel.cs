using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Abnormal_UI.Infra;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Abnormal_UI.UI.Abnormal
{
    [SuppressMessage("ReSharper", "TooWideLocalVariableScope")]
    public class AbnormalViewModel : AttackViewModel
    {
        #region Data Members

        private readonly Random _random = new Random();
        private readonly string[] _spns;
        private bool _isResultsShown;
        private string _logString;
        public bool IncludeKerberos { get; set; }
        public bool IncludeNtlm { get; set; }
        public bool IncludeEvent { get; set; }
        public string LogString
        {
            get { return _logString; }
            set
            {
                _logString = value;
                OnPropertyChanged();
            }
        }
        public bool IsResultsShown
        {
            get { return _isResultsShown; }
            set
            {
                _isResultsShown = value;
                OnPropertyChanged();
            }
        }
        public int MinMachines { get; set; }
        public int MaxMachines { get; set; }

        #endregion

        #region Ctors
        public AbnormalViewModel()
        {
            IncludeKerberos = true;
            IncludeNtlm = false;
            IncludeEvent = false;
            _isResultsShown = false;
            LogString = "";
            MinMachines = 1;
            MaxMachines = 3;
            _spns = new[]
            {
                "HOST",
                "HTTP",
                "CIFS",
                "ldap",
                "DNS",
                "RPC"
            };
        }

        #endregion

        #region Methods

        public bool ActivateUsers()
        {
            try
            {
                if (SelectedMachines.Count / SelectedUsers.Count < MaxMachines)
                {
                    LogString = Helper.Log("Not enough users",LogString);
                    return false;
                }
                var choosenTypes = ChooseActivtyType();
                PrepareDatabaseForInsertion();
                SvcCtrl.StopService("ATACenter");
                LogString = Helper.Log("Center profile set for insertion", LogString);
                var activities = GenerateRandomActivities(choosenTypes);
                _dbClient.InsertBatch(activities);
                LogString = Helper.Log("Done inserting normal activity", LogString);
                SvcCtrl.StartService("ATACenter");
                LogString = Helper.Log("Gone to sleep for 3 minutes of user profilling", LogString);
                Thread.Sleep(270000);
                LogString = Helper.Log("Woke up!", LogString);
                _dbClient.ClearTestCollections();
                SvcCtrl.RestartService("ATACenter");
                var abnormalDetectorProfile =
                    _dbClient.SystemProfilesCollection.Find(
                        Query.EQ("_t", "AbnormalBehaviorDetectorProfile").ToBsonDocument()).ToEnumerable().First();
                LogString = Helper.Log("Gone to sleep for tree build", LogString);
                while (!abnormalDetectorProfile["AccountTypeToModelMapping"].AsBsonArray.Any())
                {
                    Thread.Sleep(5000);
                    abnormalDetectorProfile =
                    _dbClient.SystemProfilesCollection.Find(
                        Query.EQ("_t", "AbnormalBehaviorDetectorProfile").ToBsonDocument()).ToEnumerable().First();
                }
                LogString = Helper.Log("Woke up!", LogString);
                return true;
            }
            catch (Exception acException)
            {
                LogString = Helper.Log(acException, LogString);
                return false;
            }
        }
        public bool AbnormalActivity(ObservableCollection<EntityObject> specificUser = null)
        {
            try
            {
                if (SelectedUsers.Count == 0 && specificUser == null)
                {
                    return false;
                }
                if (specificUser != null)
                {
                    SelectedUsers = specificUser;
                }
                _dbClient.ClearTestCollections();
                var choosenArray = ChooseActivtyType();
                SvcCtrl.StopService("ATACenter");
                var activities = GenerateRandomActivities(choosenArray, true);
                _dbClient.InsertBatch(activities);
                LogString = Helper.Log("Done inserting Abnormal activities", LogString);
                SvcCtrl.StartService("ATACenter");
                return true;
            }
            catch (Exception aaException)
            {
                LogString = Helper.Log(aaException, LogString);
                return false;
            }
        }

        public string AutoAbnormal()
        {
            try
            {
                SelectedUsers = new ObservableCollection<EntityObject>();
                SelectedMachines = new ObservableCollection<EntityObject>();
                SelectedDomainControllers = new ObservableCollection<EntityObject>();
                for (var i = 0; i < 70; i++)
                {
                    SelectedUsers.Add(Users[i]);
                }
                for (var i = 0; i < 280; i++)
                {
                    SelectedMachines.Add(Machines[i]);
                }
                SelectedDomainControllers.Add(DomainControllers.FirstOrDefault());

                ActivateUsers();

                SelectedMachines = new ObservableCollection<EntityObject>();
                for (var i = 0; i < 16; i++)
                {
                    SelectedMachines.Add(Machines[400 + i]);
                }
                AbnormalActivity(new ObservableCollection<EntityObject> {SelectedUsers[_random.Next(1, 60)]});
                return SelectedUsers[0].Name;
            }
            catch (Exception autoException)
            {
                LogString = Helper.Log(autoException, LogString);
                return "Not Succeded";
            }
        }

        private List<BsonDocument> GenerateRandomActivities(ActivityType[] choosenTypes, bool isAbnormal=false)
        {
            var currentMachinesCounter = 0;
            var computersUsedTodayCounter = 0;
            var networkActivities = new List<BsonDocument>();
            var daysToRun = 23;
            var abnormalString = "normal";
            var limit = 1;
            if (isAbnormal)
            {
                daysToRun = 0;
                abnormalString = "abnormal";
                limit = 0;
            }
            EntityObject currentSelectedMachine;
            ActivityType selectedActivityType;
            foreach (var selectedUser in SelectedUsers)
            {
                LogString = Helper.Log($"inserting {abnormalString} activity for {selectedUser.Name}", LogString);
                for (var daysToGenerate = limit; daysToGenerate <= daysToRun; daysToGenerate++)
                {
                    computersUsedTodayCounter = isAbnormal ? SelectedMachines.Count : _random.Next(MinMachines, MaxMachines + 1);
                    for (var i = 0; i < computersUsedTodayCounter; i++)
                    {
                        if (currentMachinesCounter + computersUsedTodayCounter >= SelectedMachines.Count - 1)
                        {
                            currentMachinesCounter = 0;
                        }
                        currentSelectedMachine = !isAbnormal ? SelectedMachines[currentMachinesCounter + i] : SelectedMachines[i];
                        selectedActivityType = choosenTypes[_random.Next(0, choosenTypes.Length)];
                        switch (selectedActivityType)
                        {
                            case ActivityType.Kerberos:
                                networkActivities.Add(
                                    DocumentCreator.KerberosCreator(selectedUser,
                                        currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(),
                                        DomainName, SourceGateway, null, null, "As", daysToGenerate));
                                networkActivities.Add(
                                    DocumentCreator.KerberosCreator(selectedUser,
                                        currentSelectedMachine, SelectedDomainControllers.FirstOrDefault(),
                                        DomainName, SourceGateway,
                                        $"{_spns[_random.Next(0, 5)]}/{SelectedMachines[currentMachinesCounter].Name}",
                                        currentSelectedMachine, "Tgs",
                                        daysToGenerate, 0, networkActivities.Last()["_id"].AsObjectId));
                                break;
                            case ActivityType.Event:
                                networkActivities.Add(
                                    DocumentCreator.EventCreator(selectedUser,
                                        currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway,
                                        daysToGenerate));
                                break;
                            case ActivityType.Ntlm:
                                networkActivities.Add(
                                    DocumentCreator.NtlmCreator(selectedUser,
                                        currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        Logger.Trace("Inserted {3} {2} activity for {1} on {0}",
                            DateTime.UtcNow.Subtract(new TimeSpan(daysToGenerate, 0, 0, 0)),
                            selectedUser.Name,abnormalString, selectedActivityType);
                    }
                }
                currentMachinesCounter += computersUsedTodayCounter;
            }
            return networkActivities;
        }

        private ActivityType[] ChooseActivtyType()
        {
            var choosenTypes = new List<ActivityType>();
            if (IncludeKerberos)
            {
                choosenTypes.Add(ActivityType.Kerberos);
            }
            if (IncludeEvent)
            {
                choosenTypes.Add(ActivityType.Event);
            }
            if (IncludeNtlm)
            {
                choosenTypes.Add(ActivityType.Ntlm);
            }
            return choosenTypes.ToArray();
        }

        private void PrepareDatabaseForInsertion()
        {
            _dbClient.RenameKerbCollections();
            _dbClient.RenameNtlmCollections();
            _dbClient.RenameNtlmEventsCollections();
            _dbClient.ClearTestCollections();
            _dbClient.SetCenterProfileForReplay();
        }

        public void ResetAbnormalProfile()
        {
            SvcCtrl.StopService("ATACenter");
            _dbClient.ResetUniqueEntityProfile();
            _dbClient.DisposeAbnormalDetectorProfile();
            SvcCtrl.StartService("ATACenter");
        }

        private enum ActivityType
        {
            Kerberos,
            Ntlm,
            Event
        }

        #endregion
    }
}