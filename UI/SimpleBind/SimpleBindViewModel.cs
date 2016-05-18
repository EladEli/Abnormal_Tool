using System;
using System.Collections.Generic;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.SimpleBind
{
    public class SimpleBindViewModel : AttackViewModel
    {
        public SimpleBindViewModel() : base()
        {
        }

        public bool LSBIntense()
        {
            try
            {
                _dbClient.ClearTestNaCollection();
                SvcCtrl.StopService("ATACenter");
                _dbClient.SetCenterProfileForReplay();
                Logger.Debug("Center profile set for replay");
                var networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 1 || selectedMachinesList.Count < 1)
                {
                    return false;
                }
                for (var i = 0; i < 110; i++)
                {
                    networkActivitities.Add(DocumentCreator.SimpleBindCreator(selectedEmpList.FirstOrDefault(),
                        selectedMachinesList[0], selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0));
                }
                _dbClient.InsertBatch(networkActivitities);
                Logger.Debug("Done inserting Ldap activities");
                SvcCtrl.StartService("ATACenter");
                return true;
            }
            catch (Exception intenseExpcetion)
            {
                Logger.Error(intenseExpcetion);
                return false;
            }
            
        }
        public bool LSBDistinct()
        {
            try
            {
                var networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 11 || selectedMachinesList.Count < 1)
                {
                    return false;
                }
                _dbClient.ClearTestNaCollection();
                SvcCtrl.StopService("ATACenter");
                _dbClient.SetCenterProfileForReplay();
                Logger.Debug("Center profile set for replay");
                networkActivitities.AddRange(selectedEmpList.Select(user => DocumentCreator.SimpleBindCreator(user, selectedMachinesList[0], selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0)));
                _dbClient.InsertBatch(networkActivitities);
                Logger.Debug("Done inserting Ldap activities");
                SvcCtrl.StartService("ATACenter");
                return true;
            }
            catch (Exception distinctException)
            {
                Logger.Error(distinctException);
                return false;
            }
        }
        public bool LSBSingle()
        {
            try
            {
                _dbClient.ClearTestNaCollection();
                var networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 1 || selectedMachinesList[0].name == null)
                {
                    return false;
                }
                networkActivitities.Add(DocumentCreator.SimpleBindCreator(selectedEmpList.FirstOrDefault(),
                    selectedMachinesList[0], selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0));
                _dbClient.InsertBatch(networkActivitities);
                return true;
            }
            catch (Exception singleException)
            {
                Logger.Error(singleException);
                return false;
            }
        }
    }
}
