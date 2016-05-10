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

        public DBClient _dbClient;
        private string domainName { get; set; }

        public string DomainName
        {
            get { return domainName; }
            set
            {
                domainName = value;
                OnPropertyChanged();
            }
        }

        public ObjectId sourceGateway;

        public ObservableCollection<EntityObject> empList { get; set; }

        public ObservableCollection<EntityObject> selectedEmpList { get; set; }

        public ObservableCollection<EntityObject> machinesList { get; set; }

        public ObservableCollection<EntityObject> selectedMachinesList { get; set; }

        public ObservableCollection<EntityObject> dcsList { get; set; }

        public ObservableCollection<EntityObject> selectedDcsList { get; set; }

        public Logger Logger;

        #endregion

        #region Ctors

        public AttackViewModel()
        {
            _dbClient = DBClient.getDBClient();
            empList = new ObservableCollection<EntityObject>();
            selectedEmpList = new ObservableCollection<EntityObject>();
            machinesList = new ObservableCollection<EntityObject>();
            selectedMachinesList = new ObservableCollection<EntityObject>();
            dcsList = new ObservableCollection<EntityObject>();
            selectedDcsList = new ObservableCollection<EntityObject>();
            DomainName = string.Empty;
            sourceGateway = _dbClient.GetGwOids().FirstOrDefault();
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
                    empList.Add(oneEntity);
                }
                empList.OrderByDescending(EntityObject => EntityObject.DnsName);

                var domainControllers = _dbClient.GetUniqueEntity(UniqueEntityType.Computer, null, true);
                foreach (var oneDC in domainControllers)
                {
                    dcsList.Add(oneDC);
                }

                entityTypes.Clear();
                entityTypes.Add(UniqueEntityType.Computer);
                var allComputers = _dbClient.GetUniqueEntity(entityTypes);
                foreach (var oneEntity in allComputers)
                {
                    machinesList.Add(oneEntity);
                }
                machinesList.OrderByDescending(EntityObject => EntityObject.DnsName);

                var domain = _dbClient.GetUniqueEntity(UniqueEntityType.Domain);
                DomainName = domain.FirstOrDefault().name;
            }
            catch (Exception PmException)
            {
                Logger.Error(PmException);
            }
            
        }

        #endregion

        #region INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(
            [CallerMemberName] string caller = "")

        {
            if (PropertyChanged != null)
            {
                PropertyChanged( this, new PropertyChangedEventArgs(caller));
            }
        }
        #endregion
    }

}
