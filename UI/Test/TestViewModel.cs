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

            return false;
        }

    }
}
