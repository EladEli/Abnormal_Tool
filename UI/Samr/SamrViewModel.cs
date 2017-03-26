using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.Samr
{
    public class SamrViewModel : AttackViewModel
    {

        private List<EntityObject> GroupsList { get; set; }
        public List<EntityObject> SamrUsers { get; set; }
        public List<EntityObject> SamrMachins { get; set; }
        public List<BsonDocument> ActivitiesList { get; set; }

        public SamrViewModel()
        {
            GroupsList = DbClient.GetSensitiveGroups();
            ActivitiesList = new List<BsonDocument>();
        }

        public bool GenerateLearningTime()
        {
            try
            {

                //Get Samr Users and Machines
                SamrUsers = GetSamrUsers();
                SamrMachins = GetSamrMachins();
                var machineCounter = 0;

                //Create Low Rate Machines
                for (var i = 0; i < 4; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[i], SamrMachins[i],
                            DomainControllers.FirstOrDefault(),
                            DomainObject.Name, GroupsList[j], SourceGateway, true,
                            SamrQueryType.QueryGroup,SamrQueryOperation.QueryInformationGroup, DomainObject.Id,2));
                        Logger.Debug("Inserted {3} {2} activity for {1} on {0}",
                            SamrMachins[i].Name,SamrUsers[i].Name, GroupsList[j].Name, SamrQueryType.QueryGroup);
                    }
                    ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[i], SamrMachins[i],
                        DomainControllers.FirstOrDefault(),
                        DomainObject.Name, null, SourceGateway, true,
                        SamrQueryType.QueryUser, SamrQueryOperation.QueryInformationUser, DomainObject.Id, 2));
                    Logger.Debug("Inserted {3} {2} activity for {1} on {0}",
                        SamrMachins[i].Name, SamrUsers[i].Name, "user", SamrQueryType.QueryUser);
                }

                //Create High Rate Machines 
                for (var i = 4; i < 8; i++)
                {
                    for (var j = 0; j < 20; j++)
                    {
                        ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[i], SamrMachins[i],
                            DomainControllers.FirstOrDefault(),
                            DomainObject.Name, GroupsList[j], SourceGateway, true,
                            SamrQueryType.QueryGroup, SamrQueryOperation.QueryInformationGroup,DomainObject.Id,2));
                        Logger.Debug("Inserted {3} {2} activity for {1} on {0}",
                            SamrMachins[i].Name, SamrUsers[i].Name, GroupsList[j].Name, SamrQueryType.QueryGroup);
                    }
                    ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[i], SamrMachins[i],
                        DomainControllers.FirstOrDefault(),
                        DomainObject.Name, null, SourceGateway, true,
                        SamrQueryType.QueryUser, SamrQueryOperation.QueryInformationUser, DomainObject.Id,2));
                    Logger.Trace("Inserted {3} {2} activity for {1} on {0}",
                        SamrMachins[i].Name, SamrUsers[i].Name, "user", SamrQueryType.QueryUser);
                }

                //Create login for the users
                foreach (var samrUser in SamrUsers)
                {
                    ActivitiesList.Add(DocumentCreator.KerberosCreator(samrUser, SamrMachins[machineCounter],
                        DomainControllers.FirstOrDefault(), "domain1.test.local", SourceGateway, null, null, "As", 2));

                    ActivitiesList.Add(DocumentCreator.KerberosCreator(samrUser, SamrMachins[machineCounter],
                        DomainControllers.FirstOrDefault(), "domain1.test.local", SourceGateway,
                        $"{Spn.CIFS}/{SamrMachins[machineCounter].Name}", SamrMachins[machineCounter], "Tgs", 2, 0,
                        ActivitiesList.Last()["_id"].AsObjectId));

                    ActivitiesList.Add(DocumentCreator.KerberosCreator(samrUser, SamrMachins[machineCounter],
                        DomainControllers.FirstOrDefault(), "domain1.test.local", SourceGateway,
                        $"{Spn.CIFS}/{SamrMachins[machineCounter].Name}", DomainControllers.FirstOrDefault(), "Ap", 2, 0,
                        ActivitiesList.Last()["_id"].AsObjectId));

                    Logger.Debug("Inserted AS and TGS activity for {1} on {0}",
                        SamrMachins[machineCounter].Name, samrUser.Name);

                    machineCounter++;
                }

                DbClient.ClearTestCollections();
                SvcCtrl.StopService("ATACenter");
                DbClient.SetCenterProfileForReplay();
                DbClient.SetDetecotorProfileForSamr();
                DbClient.InsertBatch(ActivitiesList);
                SvcCtrl.StartService("ATACenter");
                Logger.Debug("Done inserting SAMR activities");
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }
            
        }

        public bool GenerateSamr()
        {
            try
            {
                ActivitiesList.Clear();
                var sesitiveUser = Users.FirstOrDefault(_ => _.Name == "Administrator");
                var domainId = DbClient.GetUniqueEntity(UniqueEntityType.Domain).First().Id;

                //Create SA for first Low Rate Machine

                ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[0], SamrMachins[0],
                    DomainControllers.FirstOrDefault(),
                    DomainObject.Name, GroupsList[4], SourceGateway, true,
                    SamrQueryType.QueryGroup,SamrQueryOperation.QueryInformationGroup, domainId));

                ActivitiesList.Add(DocumentCreator.SamrCreator(sesitiveUser, SamrMachins[0],
                    DomainControllers.FirstOrDefault(),
                    DomainObject.Name, GroupsList[4], SourceGateway, true,
                    SamrQueryType.QueryUser,SamrQueryOperation.QueryInformationUser, domainId));

                //Create SA for first High Rate Machine
                for (var i = 0; i < 6; i++)
                {
                        ActivitiesList.Add(DocumentCreator.SamrCreator(SamrUsers[4], SamrMachins[4],
                            DomainControllers.FirstOrDefault(),
                            DomainObject.Name, GroupsList[20+i], SourceGateway, true,
                            SamrQueryType.QueryGroup, SamrQueryOperation.QueryInformationGroup, domainId));

                }
                ActivitiesList.Add(DocumentCreator.SamrCreator(sesitiveUser, SamrMachins[4],
                    DomainControllers.FirstOrDefault(),
                    DomainObject.Name, GroupsList[4], SourceGateway, true,
                    SamrQueryType.QueryUser, SamrQueryOperation.QueryInformationUser, domainId));

                DbClient.ClearTestCollections();
                SvcCtrl.StopService("ATACenter");
                DbClient.SetCenterProfileForReplay();
                DbClient.SetDetecotorProfileForSamr();
                DbClient.InsertBatch(ActivitiesList);
                SvcCtrl.StartService("ATACenter");
                Logger.Debug("Done inserting SAMR activities");
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }

        }


        public List<EntityObject> GetSamrUsers()
        {
            try
            {
                var userCount = 0;
                var temp = new List<EntityObject>();
                var i = 0;
                while (userCount < 8)
                {
                    if (Users[i].Name == "Administrator")
                    {
                        i++;
                        continue;
                    }
                    temp.Add(Users[i]);
                    Logger.Trace($"Inserted user {temp[userCount].Name} for for SamR Detection");
                    userCount++;
                    i++;
                }
                return temp;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                return null;
            }

        }

        public List<EntityObject> GetSamrMachins()
        {
            try
            {
                var machineCount = 0;
                var temp = new List<EntityObject>();
                var i = 0;
                while (machineCount < 8)
                {
                    if (Machines[i].Name.Contains("DC") || Machines[i].Name.Contains("CLIENT4"))
                    {
                        i++;
                        continue;
                    }
                    temp.Add(Machines[i]);
                    Logger.Trace($"Inserted machine {temp[machineCount].Name} for for SamR Detection");
                    machineCount++;
                    i++;
                }
                return temp;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                return null;
            }

        }

        public bool Test()
        {
            try
            {
                //Get Samr Users and Machines
                SamrUsers = GetSamrUsers();
                SamrMachins = GetSamrMachins();
                ActivitiesList.Clear();
                MessageBox.Show(SamrUsers.Count.ToString());
                MessageBox.Show(ActivitiesList.Count.ToString());

                var machineCounter = 0;
                MessageBox.Show("1");
                //Create login for the users
                foreach (var samrUser in SamrUsers)
                {
                    ActivitiesList.Add(DocumentCreator.KerberosCreator(samrUser, SamrMachins[machineCounter],
                        DomainControllers.FirstOrDefault(), "domain1.test.local", SourceGateway,
                        $"{Spn.CIFS}/{SamrMachins[machineCounter].Name}", SamrMachins[machineCounter], "Tgs", 2));

                    MessageBox.Show("2");
                    ActivitiesList.Add(DocumentCreator.KerberosCreator(samrUser, SamrMachins[machineCounter],
                        DomainControllers.FirstOrDefault(), "domain1.test.local", SourceGateway,
                        $"{Spn.CIFS}/{DomainControllers.FirstOrDefault()?.Name}", DomainControllers.FirstOrDefault(),
                        "Ap", 2,0, ActivitiesList.Last()["_id"].AsObjectId));
                    machineCounter++;
                }
                MessageBox.Show("3");
                DbClient.ClearTestCollections();
                SvcCtrl.StopService("ATACenter");
                DbClient.SetCenterProfileForReplay();
                DbClient.SetDetecotorProfileForSamr();
                DbClient.InsertBatch(ActivitiesList);
                SvcCtrl.StartService("ATACenter");
                Logger.Debug("Done inserting SAMR activities");
                return true;
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                throw;
            }
        }

        public enum SamrQueryType
        {
            EnumerateUsers,
            QueryGroup,
            QueryDisplayInformation2, //EnumerateGroups
            QueryUser
        }

        public enum SamrQueryOperation
        {
            EnumerateUsersInDomain,
            QueryInformationGroup,
            QueryDisplayInformation2, //EnumerateGroups
            QueryInformationUser
        }


    }
}
