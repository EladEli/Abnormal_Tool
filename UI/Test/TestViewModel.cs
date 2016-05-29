using System;
using System.Collections.Generic;
using System.Linq;
using Abnormal_UI.Imported;
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
                var entityTypes = new List<UniqueEntityType> {UniqueEntityType.User};
                var entityTypes2 = new List<UniqueEntityType> {UniqueEntityType.Computer};
                var userEntity = _dbClient.GetUniqueEntity(entityTypes);
                var computerEntity = _dbClient.GetUniqueEntity(entityTypes2);
                var suspicousActivitities = new List<BsonDocument>();
                var rnd = new Random();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SAFillerSEAC(userEntity, computerEntity, rnd));
                }
                _dbClient.InsertBatchTest(suspicousActivitities, true);
                return true;
            }
            catch (Exception SeacException)
            {
                Logger.Error(SeacException);
                return false;
            }
        }

        public bool InsertAe()
        {
            try
            {
                var entityTypes = new List<UniqueEntityType> {UniqueEntityType.User};
                var entityTypes2 = new List<UniqueEntityType> {UniqueEntityType.Computer};
                var userEntity = _dbClient.GetUniqueEntity(entityTypes);
                var computerEntity = _dbClient.GetUniqueEntity(entityTypes2);
                var suspicousActivitities = new List<BsonDocument>();
                for (var i = 0; i < _saAmount; i++)
                {
                    suspicousActivitities.Add(DocumentCreator.SAFillerAE(userEntity, computerEntity, SelectedDomainControllers.FirstOrDefault(), DomainName));
                }
                _dbClient.InsertBatchTest(suspicousActivitities, true);
                return true;
            }
            catch (Exception AeException)
            {
                Logger.Error(AeException);
                return false;
            }
        }

        public void AddGateway()
        {
            _dbClient.SetNewGateway(_saAmount);
        }

        public bool StopService(string serviceName)
        {
            SvcCtrl.StopService(serviceName);
            return true;
        }

    }
}
