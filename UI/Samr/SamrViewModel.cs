using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Abnormal_UI.UI.Samr
{
    public class SamrViewModel : AttackViewModel
    {
        #region Data Members
        public List<BsonDocument> ActivitiesList { get; set; }
        public ObservableCollection<CoupledSamr> SamrCouples { get; set; }
        private BsonDocument SamrReconnaissanceDetectorProfile { get; set; }
        private readonly Random _random = new Random();
        #endregion

        #region Ctors
        public SamrViewModel()
        {
            ActivitiesList = new List<BsonDocument>();
            SamrCouples = new ObservableCollection<CoupledSamr>();
        }

        #endregion

        #region Methods

        public bool ExecuteLearningTime()
        {
            try
            {
                var sourceMachine = Machines.Single(_ => _.Name == "APP1");
                var sourceUser = Users.Single(_ => _.Name == "triservice");
                // Generate Samr for domainController learning time
                foreach (var domainController in DomainControllers)
                {
                    ActivitiesList.Add(DocumentCreator.SamrCreator(sourceUser, sourceMachine,
                        domainController,
                        DomainList.Single(_ => _.Id == sourceUser.Domain).Name
                        , DomainList.Single(_ => _.Id == sourceMachine.Domain).Name, SourceGateway, true,
                        SamrQueryType.EnumerateUsers, SamrQueryOperation.EnumerateUsersInDomain,
                        DomainList.Single(_ => _.Id == sourceMachine.Domain).Id, 35));
                }

                InsertActivities(true);

                do
                {
                    SamrReconnaissanceDetectorProfile = GetSamrDetectorProfile();
                } while (SamrReconnaissanceDetectorProfile["DestinationComputerIdToDetectionStartTimeMapping"]
                             .AsBsonArray.Count != DomainControllers.Count);

                foreach (var coupledSamr in SamrCouples)
                {
                    var samrAmount = coupledSamr.RatingType == "Low" ? 10 : 21;
                    for (var samrIndex = 0; samrIndex < samrAmount; samrIndex++)
                    {
                        var queriedObject = Users[_random.Next(Users.Count)];
                        ActivitiesList.Add(DocumentCreator.SamrCreator(coupledSamr.User, coupledSamr.Machine,
                            DomainControllers.First(_=> _.Domain == DomainList.Single(__ => __.Id == coupledSamr.Machine.Domain).Id),
                            DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway, true,
                            SamrQueryType.QueryUser, SamrQueryOperation.QueryInformationUser,
                            DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Id, 10, queriedObject));
                    }
                }

                InsertActivities();

                do
                {
                    SamrReconnaissanceDetectorProfile = GetSamrDetectorProfile();
                } while (SamrReconnaissanceDetectorProfile["DateToQueryToSamrQueryDataMapping"]
                             .AsBsonArray.Count == 0);

                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                return false;
            }
        }
        public bool ExecuteSamrDetection()
        {
            try
            {
                var sensitiveGroupList = DbClient.GetSensitiveGroups();

                foreach (var coupledSamr in SamrCouples)
                {
                    var domainController = DomainControllers.First(_ =>
                        _.Domain == DomainList.Single(__ => __.Id == coupledSamr.Machine.Domain).Id);

                    if (coupledSamr.RatingType.ToLower() == "low")
                    {
                        var administratorObject = Users.First(_=>_.Name == "Administrator");
                        
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway));
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway,
                            $"{(Spn) _random.Next(0, 5)}/{DomainControllers.FirstOrDefault()?.Name}", null, "Tgs", 0,
                            0, ActivitiesList.Last()["_id"].AsObjectId));
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway,
                            $"{(Spn) _random.Next(0, 5)}/{DomainControllers.FirstOrDefault()?.Name}", null, "Ap", 0,
                            0, ActivitiesList.Last()["_id"].AsObjectId));
                        ActivitiesList.Add(DocumentCreator.SamrCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController,
                            DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway, true,
                            SamrQueryType.QueryUser, SamrQueryOperation.QueryInformationUser,
                            DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Id, 0,
                            administratorObject));
                    }
                    else
                    {
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway));
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway,
                            $"{(Spn)_random.Next(0, 5)}/{DomainControllers.FirstOrDefault()?.Name}", null, "Tgs", 0,
                            0, ActivitiesList.Last()["_id"].AsObjectId));
                        ActivitiesList.Add(DocumentCreator.KerberosCreator(coupledSamr.User, coupledSamr.Machine,
                            domainController, DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                            , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway,
                            $"{(Spn)_random.Next(0, 5)}/{DomainControllers.FirstOrDefault()?.Name}", null, "Ap", 0,
                            0, ActivitiesList.Last()["_id"].AsObjectId));

                        foreach (var group in sensitiveGroupList)
                        {
                            ActivitiesList.Add(DocumentCreator.SamrCreator(coupledSamr.User, coupledSamr.Machine,
                                domainController,
                                DomainList.Single(_ => _.Id == coupledSamr.User.Domain).Name
                                , DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Name, SourceGateway, true,
                                SamrQueryType.QueryGroup, SamrQueryOperation.QueryInformationGroup,
                                DomainList.Single(_ => _.Id == coupledSamr.Machine.Domain).Id, 0,
                                group));
                        }
                    }
                }
                InsertActivities();
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                return false;
            }
        }
        private BsonDocument GetSamrDetectorProfile() => DbClient.DataProfileCollection.Find(
            Builders<BsonDocument>.Filter.Eq("_t", "SamrReconnaissanceDetectorProfile")).ToList().First();
        public enum SamrQueryType
        {
            QueryUser,
            QueryGroup,
            EnumerateUsers,
            EnumerateGroups
        }
        public enum SamrQueryOperation
        {
            EnumerateUsersInDomain,
            EnumerateGroupsInDomain,
            QueryInformationGroup,
            QueryDisplayInformation2, //EnumerateGroups
            QueryInformationUser
        }

        private void InsertActivities(bool isLearning=false)
        {
            DbClient.ClearTestCollections();
            SvcCtrl.StopService("ATACenter");
            if (isLearning)
            {
                DbClient.SetCenterProfileForReplay();
                DbClient.SetDetectorProfileForSamr();
            }
            DbClient.InsertBatch(ActivitiesList);
            ActivitiesList.Clear();
            SvcCtrl.StartService("ATACenter");
        }

        #endregion

        public class CoupledSamr
        {
            public EntityObject User { get; }
            public EntityObject Machine { get; }
            public string RatingType { get; }

            public CoupledSamr(EntityObject user,EntityObject machine, string type)
            {
                User = user;
                Machine = machine;
                RatingType = type;
            }

            public override string ToString() => $"{User.Name} {Machine.Name} {RatingType}";
        }
    }
}
