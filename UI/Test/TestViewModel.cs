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
                OnPropertyChanged();
            }
        }

        public TestViewModel() : base()
        {
            _saAmount = 200;
        }

        public bool InsertSeac()
        {
            try
            {
                var userEntity = _dbClient.GetUniqueEntity(UniqueEntityType.User);
                var computerEntity = _dbClient.GetUniqueEntity(UniqueEntityType.Computer);
                var suspicousActivitities = new List<BsonDocument>();
                var rnd = new Random();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SaFillerSeac(userEntity, computerEntity, rnd));
                }
                _dbClient.InsertSaBatch(suspicousActivitities);
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
                var userEntity = _dbClient.GetUniqueEntity(UniqueEntityType.User);
                var computerEntity = _dbClient.GetUniqueEntity(UniqueEntityType.Computer);
                var suspicousActivitities = new List<BsonDocument>();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SaFillerAe(userEntity, computerEntity, SelectedDomainControllers.FirstOrDefault(), DomainName));
                }
                _dbClient.InsertSaBatch(suspicousActivitities);
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
            _dbClient.SetNewGateway(_saAmount);
        }

        public bool GoldenTicketActivity()
        {

            return false;
        }

    }
}
