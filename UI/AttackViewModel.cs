using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;
using NLog;

namespace Abnormal_UI.UI
{
    public class AttackViewModel : INotifyPropertyChanged
    {
        #region Data Members
        public DbClient DbClient;
        
        public ObjectId SourceGateway;

        public ObservableCollection<EntityObject> Users { get; set; }

        public ObservableCollection<EntityObject> SelectedUsers { get; set; }

        public ObservableCollection<EntityObject> Machines { get; set; }

        public ObservableCollection<EntityObject> SelectedMachines { get; set; }

        public ObservableCollection<EntityObject> DomainControllers { get; set; }

        public ObservableCollection<EntityObject> SelectedDomainControllers { get; set; }

        public Logger Logger;
        public enum Spn
        {
            HOST,
            HTTP,
            CIFS,
            LDAP,
            DNS,
            RPC
        }
        public EntityObject DomainObject { get; set; }
        #endregion

        #region Ctors

        public AttackViewModel()
        {
            DbClient = DbClient.GetDbClient();
            Users = new ObservableCollection<EntityObject>();
            SelectedUsers = new ObservableCollection<EntityObject>();
            Machines = new ObservableCollection<EntityObject>();
            SelectedMachines = new ObservableCollection<EntityObject>();
            DomainControllers = new ObservableCollection<EntityObject>();
            SelectedDomainControllers = new ObservableCollection<EntityObject>();
            SourceGateway = DbClient.GetGwOids().FirstOrDefault();
            DomainObject = DbClient.GetUniqueEntity(UniqueEntityType.Domain).First(_=>_.Name == "domain1");
            Logger = LogManager.GetLogger("TestToolboxLog");
        }

        #endregion

        #region Methods

        public void PopulateModel(UniqueEntityType uniqueType = UniqueEntityType.Computer)
        {
            try
            {
                var allUsers = DbClient.GetUniqueEntity(UniqueEntityType.User);
                Users = new ObservableCollection<EntityObject>(allUsers.OrderBy(entityObject => entityObject.Name).AsEnumerable());

                var domainControllers = DbClient.GetUniqueEntity(UniqueEntityType.Computer,true);
                DomainControllers = new ObservableCollection<EntityObject>(domainControllers.OrderBy(entityObject => entityObject.Name).AsEnumerable());

                var allComputers = DbClient.GetUniqueEntity(uniqueType);
                Machines = new ObservableCollection<EntityObject>(allComputers.OrderBy(entityObject => entityObject.Name).AsEnumerable());
            }
            catch (Exception pmException)
            {
                Logger.Error(pmException);
            }
        }

        #endregion

        #region INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
