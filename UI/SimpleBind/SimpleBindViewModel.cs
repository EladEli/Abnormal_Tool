using System;
using System.Collections.Generic;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.SimpleBind
{
    public class SimpleBindViewModel : AttackViewModel
    {
        public bool LsbIntense()
        {
            try
            {
                DbClient.ClearTestCollections();
                SvcCtrl.StopService("ATACenter");
                DbClient.SetCenterProfileForReplay();
                Logger.Debug("Center profile set for replay");
                var networkActivitities = new List<BsonDocument>();
                if (SelectedUsers.Count < 1 || SelectedMachines.Count < 1)
                {
                    return false;
                }
                for (var i = 0; i < 110; i++)
                {
                    networkActivitities.Add(DocumentCreator.SimpleBindCreator(SelectedUsers.FirstOrDefault(),
                        SelectedMachines[0], SelectedDomainControllers.FirstOrDefault(), DomainObject.Name, SourceGateway));
                }
                DbClient.InsertBatch(networkActivitities);
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
        public bool LsbDistinct()
        {
            try
            {
                var networkActivitities = new List<BsonDocument>();
                if (SelectedUsers.Count < 11 || SelectedMachines.Count < 1)
                {
                    return false;
                }
                DbClient.ClearTestCollections();
                SvcCtrl.StopService("ATACenter");
                DbClient.SetCenterProfileForReplay();
                Logger.Debug("Center profile set for replay");
                networkActivitities.AddRange(
                    SelectedUsers.Select(
                        user =>
                            DocumentCreator.SimpleBindCreator(user, SelectedMachines[0],
                                SelectedDomainControllers.FirstOrDefault(), DomainObject.Name, SourceGateway)));
                DbClient.InsertBatch(networkActivitities);
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
        public bool LsbSingle()
        {
            try
            {
                DbClient.ClearTestCollections();
                var networkActivitities = new List<BsonDocument>();
                if (SelectedUsers.Count < 1 || SelectedMachines[0].Name == null)
                {
                    return false;
                }
                networkActivitities.Add(DocumentCreator.SimpleBindCreator(SelectedUsers.FirstOrDefault(),
                    SelectedMachines[0], SelectedDomainControllers.FirstOrDefault(), DomainObject.Name, SourceGateway));
                DbClient.InsertBatch(networkActivitities);
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
