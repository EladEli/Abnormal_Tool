using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Abnormal_UI.Imported;
using MongoDB.Bson;
using NLog;

namespace Abnormal_UI.UI
{
    public class AttackViewModel : INotifyPropertyChanged
    {
        #region Data Members
        // baclkup
        public DBClient _dbClient;
        
        public ObjectId SourceGateway;

        public ObservableCollection<EntityObject> Users { get; set; }

        public ObservableCollection<EntityObject> SelectedUsers { get; set; }

        public ObservableCollection<EntityObject> Machines { get; set; }

        public ObservableCollection<EntityObject> SelectedMachines { get; set; }

        public ObservableCollection<EntityObject> DomainControllers { get; set; }

        public ObservableCollection<EntityObject> SelectedDomainControllers { get; set; }

        public Logger Logger;

        public string DomainName { get; set; }
        #endregion

        #region Ctors

        public AttackViewModel()
        {
            _dbClient = DBClient.getDBClient();
            Users = new ObservableCollection<EntityObject>();
            SelectedUsers = new ObservableCollection<EntityObject>();
            Machines = new ObservableCollection<EntityObject>();
            SelectedMachines = new ObservableCollection<EntityObject>();
            DomainControllers = new ObservableCollection<EntityObject>();
            SelectedDomainControllers = new ObservableCollection<EntityObject>();
            DomainName = string.Empty;
            SourceGateway = _dbClient.GetGwOids().FirstOrDefault();
            Logger = LogManager.GetLogger("TestToolboxLog");
        }

        #endregion

        #region Methods

        public void PopulateModel()
        {
            try
            {
                var entityTypes = new List<UniqueEntityType> {UniqueEntityType.User};
                var allUsers = _dbClient.GetUniqueEntity(entityTypes);
                foreach (var oneEntity in allUsers)
                {
                    Users.Add(oneEntity);
                }
                Users.OrderByDescending(entityObject => entityObject.DnsName);

                var domainControllers = _dbClient.GetUniqueEntity(UniqueEntityType.Computer, null, true);
                foreach (var oneDc in domainControllers)
                {
                    DomainControllers.Add(oneDc);
                }

                entityTypes.Clear();
                entityTypes.Add(UniqueEntityType.Computer);
                var allComputers = _dbClient.GetUniqueEntity(entityTypes);
                foreach (var oneEntity in allComputers)
                {
                    Machines.Add(oneEntity);
                }
                Machines.OrderByDescending(entityObject => entityObject.DnsName);

                var domain = _dbClient.GetUniqueEntity(UniqueEntityType.Domain);
                DomainName = domain.FirstOrDefault().name;
            }
            catch (Exception pmException)
            {
                Logger.Error(pmException);
            }
            
        }

        #endregion

        #region INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(
            [CallerMemberName] string caller = "")

        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(caller));
        }

        #endregion
    }

}
