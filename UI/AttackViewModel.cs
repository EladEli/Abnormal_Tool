using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Abnormal_UI.Imported;
using MongoDB.Bson;
using NLog;

namespace Abnormal_UI
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
            Logger = LogManager.GetLogger("DavidTest");
        }

        #endregion

        #region Methods

        public void PopulateModel()
        {
            var entityTypes = new List<UniqueEntityType>();
            entityTypes.Add(UniqueEntityType.User);
            var allUsers = _dbClient.GetUniqueEntity(entityTypes);
            foreach (EntityObject oneEntity in allUsers)
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
            foreach (EntityObject oneEntity in allComputers)
            {
                machinesList.Add(oneEntity);
            }
            machinesList.OrderByDescending(EntityObject => EntityObject.DnsName);

            var domain = _dbClient.GetUniqueEntity(UniqueEntityType.Domain);
            DomainName = domain.FirstOrDefault().name;
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
