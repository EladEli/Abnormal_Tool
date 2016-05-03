using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI
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
                List<BsonDocument> networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 1 || selectedMachinesList.Count < 1)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < 110; i++)
                    {
                        networkActivitities.Add(DocumentCreator.SimpleBindCreator(selectedEmpList.FirstOrDefault(),
                            selectedMachinesList[0], selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0));
                    }
                    _dbClient.InsertBatch(networkActivitities);
                    return true;
                }
            }
            catch (Exception IntenseExpcetion)
            {
                Logger.Error(IntenseExpcetion);
                return false;
            }
            
        }
        public bool LSBDistinct()
        {
            try
            {
                List<BsonDocument> networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 10 || selectedMachinesList.Count < 1)
                {
                    return false;
                }
                else
                {
                    foreach (var user in selectedEmpList)
                    {
                        networkActivitities.Add(DocumentCreator.SimpleBindCreator(user, selectedMachinesList[0],
                            selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0));
                    }
                    _dbClient.InsertBatch(networkActivitities);
                    return true;
                }
            }
            catch (Exception DistinctException)
            {
                Logger.Error(DistinctException);
                return false;
            }
        }
        public bool LSBSingle()
        {
            try
            {
                List<BsonDocument> networkActivitities = new List<BsonDocument>();
                if (selectedEmpList.Count < 1 || selectedMachinesList[0].name == null)
                {
                    return false;
                }
                else
                {
                    networkActivitities.Add(DocumentCreator.SimpleBindCreator(selectedEmpList.FirstOrDefault(),
                        selectedMachinesList[0], selectedDcsList.FirstOrDefault(), DomainName, sourceGateway, 0));
                    _dbClient.InsertBatch(networkActivitities);
                    return true;
                }
            }
            catch (Exception SingleException)
            {
                Logger.Error(SingleException);
                return false;
            }
        }
    }
}
