using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Abnormal_UI.Imported;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.Abnormal
{
    public class AbnormalViewModel : AttackViewModel
    {
        private readonly Random _random = new Random();
        public bool includeKerberos { get; set; }
        public bool includeNtlm { get; set; }
        public bool includeEvent { get; set; }
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

        private string[] Spns;
        public AbnormalViewModel() : base()
        {
            minMachines = 1;
            maxMachines = 4;
            includeKerberos = true;
            includeNtlm = false;
            includeEvent = false;
            Spns = new []
            {
                "HOST",
                "HTTP",
                "CIFS",
                "ldap",
                "DNS",
                "RPC"
            };

            
        }
        public bool ActivateUsers()
        {
            try
            {
                if (SelectedMachines.Count / SelectedEmployees.Count < 2)
                {
                    Logger.Debug("Not enough users");
                    return false;
                }
                var choosenTypes = new List<ActivityType>();
                if (includeKerberos)
                {
                    choosenTypes.Add(ActivityType.Kerberos);
                }
                if (includeEvent)
                {
                    choosenTypes.Add(ActivityType.Event);
                }
                if (includeNtlm)
                {
                    choosenTypes.Add(ActivityType.Ntlm);
                }

                CleanDatabase();
                SvcCtrl.StopService("ATACenter");
                Logger.Debug("Center profile set for replay");
                int machinesUsed;
                var activities = GenerateRandomActivities(choosenTypes.ToArray(), out machinesUsed);
                _dbClient.InsertBatch(activities);
                Logger.Debug("Done inserting normal activity");
                Logger.Debug("Used {0} machines", machinesUsed);
                SvcCtrl.StartService("ATACenter");
                Logger.Debug("Gone to sleep for 3 minutes of user profilling");
                Thread.Sleep(180000);
                Logger.Debug("Woke up!");
                return true;
            }
            catch (Exception AcException)
            {
                Logger.Error(AcException);
                return false;
            }
            
        }


        private List<BsonDocument> GenerateRandomActivities(ActivityType[] choosenTypes, out int machinesUsed)
        {
            var networkActivities = new List<BsonDocument>();
            var currentMachinesCounter = 0;
            machinesUsed = 0;
            var computersUsedTodayCounter = 0;
            foreach (var selectedEmployee in SelectedEmployees)
            {
                Logger.Debug("inserting normal activity for {0}", selectedEmployee.name);
                for (var daysToGenerate = 1; daysToGenerate <= 23; daysToGenerate++)
                {
                    computersUsedTodayCounter = _random.Next(minMachines, maxMachines + 1);
                    for (var i = 0; i <= computersUsedTodayCounter - 1; i++)
                    {
                        if (currentMachinesCounter + computersUsedTodayCounter >= SelectedMachines.Count - 1)
                        {
                            currentMachinesCounter = 0;
                            machinesUsed += SelectedMachines.Count;
                        }
                        var currentSelectedMachine = SelectedMachines[currentMachinesCounter + i];
                        var selectedActivityType = choosenTypes[_random.Next(0, choosenTypes.Length)];
                        switch (selectedActivityType)
                        {
                            case ActivityType.Kerberos:
                                networkActivities.AddRange(AddKerberos(currentMachinesCounter, selectedEmployee, currentSelectedMachine, daysToGenerate));
                                break;
                            case ActivityType.Event:
                                networkActivities.Add(
                                    DocumentCreator.EventCreator(selectedEmployee,
                                        currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway,
                                        daysToGenerate));
                                break;
                            case ActivityType.Ntlm:
                                networkActivities.Add(
                                    DocumentCreator.NtlmCreator(selectedEmployee,
                                        currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        Logger.Trace("Inserted {2} activity for {1} on {0}",
                                        DateTime.UtcNow.Subtract(new TimeSpan(daysToGenerate, 0, 0, 0)),
                                        selectedEmployee.name, selectedActivityType);
                    }
                }
                currentMachinesCounter += computersUsedTodayCounter;
            }

            return networkActivities;
        }

        private IReadOnlyCollection<BsonDocument> AddKerberos(int currentMachinesCounter, EntityObject selectedEmployee, EntityObject currentSelectedMachine, int daysToGenerate)
        {
            var output = new List<BsonDocument>();
            var defaultMachine = SelectedMachines[currentMachinesCounter];
            var asActivity = DocumentCreator.KerberosCreator(selectedEmployee, currentSelectedMachine, SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway, null, null, "As", daysToGenerate);
            output.Add(asActivity);
            var tgsActivity = DocumentCreator.KerberosCreator(selectedEmployee, defaultMachine, SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway, $"{Spns[_random.Next(0, 5)]}/{currentSelectedMachine.name}", currentSelectedMachine, "Tgs", daysToGenerate, 0, asActivity["_id"].AsObjectId);
            output.Add(tgsActivity);
            return output;
        }

        private enum ActivityType
        {
            Kerberos,
            Ntlm,
            Event,
        }

        private void CleanDatabase()
        {
            _dbClient.RenameKerbCollections();
            _dbClient.RenameNtlmCollections();
            _dbClient.RenameNtlmEventsCollections();
            _dbClient.ClearTestNaCollection();
            _dbClient.SetCenterProfileForReplay();
        }

        public bool AbnormalActivity(ObservableCollection<EntityObject> specificUser = null)
        {
            try
            {
                var activities = new List<BsonDocument>();
                var choosenTypes = new List<ActivityType>();
                if (includeKerberos)
                {
                    choosenTypes.Add(ActivityType.Kerberos);
                }
                if (includeEvent)
                {
                    choosenTypes.Add(ActivityType.Event);
                }
                if (includeNtlm)
                {
                    choosenTypes.Add(ActivityType.Ntlm);
                }

                var choosenArray = choosenTypes.ToArray();
                _dbClient.ClearTestNaCollection();
                SvcCtrl.RestartService("ATACenter");
                Logger.Debug("Gone to sleep for 2 minutes of tree build");
                Thread.Sleep(120000);
                Logger.Debug("Woke up!");
                SvcCtrl.StopService("ATACenter");
                var hoursCounter = 4;
                if (SelectedEmployees.Count == 0 && specificUser == null)
                {
                    return false;
                }
                else
                {
                    if (specificUser != null)
                    {
                        SelectedEmployees = specificUser;
                    }
                    foreach (var selectedEmployee in SelectedEmployees)
                    {
                        Logger.Debug("inserting abnormal activity for {0}", selectedEmployee.name);
                        foreach (var selectedComputer in SelectedMachines)
                        {
                            hoursCounter++;
                            var currentSelectedMachine = selectedComputer;

                            var selectedActivityType = choosenArray[_random.Next(0, choosenArray.Length)];
                            switch (selectedActivityType)
                            {
                                case ActivityType.Kerberos:
                                    activities.Add(DocumentCreator.KerberosCreator(selectedEmployee, currentSelectedMachine,
                                        SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway, null,
                                        null, "As", 0, hoursCounter));
                                    activities.Add(DocumentCreator.KerberosCreator(selectedEmployee, currentSelectedMachine, SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway,
                                        $"{Spns[_random.Next(0, 5)]}/{currentSelectedMachine.name}", currentSelectedMachine,"Tgs", 0, hoursCounter));
                                    break;
                                case ActivityType.Event:
                                    activities.Add(
                                        DocumentCreator.EventCreator(selectedEmployee,
                                            currentSelectedMachine,
                                            SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway));
                                    break;
                                case ActivityType.Ntlm:
                                    activities.Add(
                                        DocumentCreator.NtlmCreator(selectedEmployee,
                                            currentSelectedMachine,
                                            SelectedDomainControllers.FirstOrDefault(), DomainName, SourceGateway));
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        Logger.Debug("Expect abnormal activity on {0}", selectedEmployee.name);
                    }
                    _dbClient.InsertBatch(activities);
                    Logger.Debug("Done inserting abnormal activity");
                    SvcCtrl.StartService("ATACenter");
                    return true;
                }
            }
            catch (Exception AaException)
            {
                Logger.Error(AaException);
                return false;
            }
        }

        public string AutoAbnormal()
        {
            try
            {
                SelectedEmployees = new ObservableCollection<EntityObject> {};
                SelectedMachines = new ObservableCollection<EntityObject> {};
                SelectedDomainControllers = new ObservableCollection<EntityObject> {};

                for (var i = 0; i < 70; i++)
                {
                    SelectedEmployees.Add(Employees[i]);
                }
                for (var i = 0; i < 250; i++)
                {
                    SelectedMachines.Add(Machines[i]);
                }
                SelectedDomainControllers.Add(DomainControllers.FirstOrDefault());

                ActivateUsers();

                SelectedMachines = new ObservableCollection<EntityObject> {};
                for (var i = 0; i < 10; i++)
                {
                    SelectedMachines.Add(Machines[250 + i]);
                }

                AbnormalActivity(new ObservableCollection<EntityObject> {SelectedEmployees[_random.Next(1, 60)]});

                return SelectedEmployees[0].name;
            }
            catch (Exception AutoException)
            {
                Logger.Error(AutoException);
                return "Not Succeded";
            }
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
