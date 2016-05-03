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

        private int saAmount;
        public int SaAmount
        {
            get { return saAmount; }
            set
            {
                saAmount = value;
                OnPropertyChanged();
            }
        }

        public TestViewModel() : base()
        {
            saAmount = 200;
        }

        public bool InsertSeac()
        {
            var entityTypes = new List<UniqueEntityType>();
            entityTypes.Add(UniqueEntityType.User);
            var entityTypes2 = new List<UniqueEntityType>();
            entityTypes2.Add(UniqueEntityType.Computer);
            List<EntityObject> userEntity = _dbClient.GetUniqueEntity(entityTypes);
            List<EntityObject> computerEntity = _dbClient.GetUniqueEntity(entityTypes2);
            //List<BsonDocument> notifications = new List<BsonDocument>();
            List<BsonDocument> suspicousActivitities = new List<BsonDocument>();
            Random rnd = new Random();
            for (int i = 0; i < saAmount; i++)
            {
                suspicousActivitities.Add(DocumentCreator.SAFillerSEAC(userEntity, computerEntity, rnd));
            }
            //_dbClient.InsertBatchSAS(suspicousActivitities);
            _dbClient.InsertBatchTest(suspicousActivitities,true);
            return true;
        }

        public bool InsertAe()
        {
            var entityTypes = new List<UniqueEntityType>();
            entityTypes.Add(UniqueEntityType.User);
            var entityTypes2 = new List<UniqueEntityType>();
            entityTypes2.Add(UniqueEntityType.Computer);
            List<EntityObject> userEntity = _dbClient.GetUniqueEntity(entityTypes);
            List<EntityObject> computerEntity = _dbClient.GetUniqueEntity(entityTypes2);
            //List<BsonDocument> notifications = new List<BsonDocument>();
            List<BsonDocument> suspicousActivitities = new List<BsonDocument>();
            for (int i = 0; i < saAmount; i++)
            {
                suspicousActivitities.Add(DocumentCreator.SAFillerAE(userEntity, computerEntity, selectedDcsList.FirstOrDefault(),DomainName));
            }
            //_dbClient.InsertBatchSAS(suspicousActivitities);
            _dbClient.InsertBatchTest(suspicousActivitities,true);
            return true;
        }

        public bool StopService(string serviceName)
        {
            SvcCtrl.StopService(serviceName);
            return true;
        }

    }
}
