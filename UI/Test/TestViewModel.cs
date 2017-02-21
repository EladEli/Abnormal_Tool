using System;
using System.Collections.Generic;
using System.Linq;
using Abnormal_UI.Infra;
using MongoDB.Bson;

namespace Abnormal_UI.UI.Test
{
    public class TestViewModel : AttackViewModel
    {

        private int _saAmount;
        public int SaAmount
        {
            get { return _saAmount; }
            set
            {
                _saAmount = value;
                OnPropertyChanged(nameof(SaAmount));
            }
        }
        private readonly Random _random = new Random();
        public TestViewModel()
        {
            _saAmount = 200;
        }
        public bool InsertSeac()
        {
            try
            {
                var userEntity = DbClient.GetUniqueEntity(UniqueEntityType.User);
                var computerEntity = DbClient.GetUniqueEntity(UniqueEntityType.Computer);
                var suspicousActivitities = new List<BsonDocument>();
                var rnd = new Random();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SaFillerSeac(userEntity, computerEntity, rnd));
                }
                DbClient.InsertSaBatch(suspicousActivitities);
                return true;
            }
            catch (Exception seacException)
            {
                Logger.Error(seacException);
                return false;
            }
        }
        public bool InsertAe()
        {
            try
            {
                var userEntity = DbClient.GetUniqueEntity(UniqueEntityType.User);
                var computerEntity = DbClient.GetUniqueEntity(UniqueEntityType.Computer);
                var suspicousActivitities = new List<BsonDocument>();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SaFillerAe(userEntity, computerEntity, SelectedDomainControllers.FirstOrDefault(), DomainName));
                }
                DbClient.InsertSaBatch(suspicousActivitities);
                return true;
            }
            catch (Exception aeException)
            {
                Logger.Error(aeException);
                return false;
            }
        }
        public void AddGateway()
        {
            DbClient.SetNewGateway(_saAmount);
        }
        public bool GoldenTicketActivity()
        {
            try
            {
                var tgsList = new List<BsonDocument>();
                var userEntity = Users.First(_ => _.Name == "user1");
                var machineEntity = Machines.First(_ => _.Name == "CLIENT1");
                for (var loopIndex = 0; loopIndex <= _saAmount; loopIndex++)
                {
                    tgsList.Add(DocumentCreator.KerberosCreator(userEntity, machineEntity,
                        DomainControllers.FirstOrDefault(), DomainName, SourceGateway, $"{(Spn)(_random.Next(0, 5))}/{Machines[loopIndex].Name}", null, "Tgs"));
                }
                DbClient.SetCenterProfileForReplay();
                SvcCtrl.StopService("ATACenter");
                DbClient.InsertBatch(tgsList);
                SvcCtrl.StartService("ATACenter");
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }
    }
}
