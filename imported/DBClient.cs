using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NLog;

namespace Abnormal_UI.Imported
{
    public class DBClient
    {
        #region Data Members

        private static DBClient _dbClient;
        MongoClient _client;
        MongoServer _server;
        MongoDatabase _database;
        MongoDatabase _testDatabase;
        MongoCollection<BsonDocument> _uniqueEntitiesCollection;
        MongoCollection<BsonDocument> _systemProfilesCollection;
        List<ObjectId> _gatewayIdsCollection;
        List<string> _kerberosCollections;
        List<string> _ntlmEventsCollections;
        private Logger _logger;

        #endregion
        
        #region Ctors
        private DBClient()
        {
          
            string connectionString;

            if ("mongodb://:27017" ==
                (connectionString =
                    string.Format("mongodb://{0}:27017",
                        Interaction.InputBox("Please enter the server Address", "Target Server", "127.0.0.1", -1, -1))))
            {
                connectionString = "mongodb://127.0.0.1:27017";
            }

            _client = new MongoClient(connectionString);
            _server = _client.GetServer();
            _database = _server.GetDatabase("ATA");
            _testDatabase = _server.GetDatabase("ATAActivitySimulator");
            _uniqueEntitiesCollection = _database.GetCollection("UniqueEntity");
            _systemProfilesCollection = _database.GetCollection("SystemProfile");
            _gatewayIdsCollection = new List<ObjectId>(0);
            _logger = LogManager.GetLogger("DavidTest");
            _gatewayIdsCollection = FilterGWIds();
            CreateActivityCollectionsOnTestDB();
            
        }
        public static DBClient getDBClient()
        {
            return _dbClient ?? (_dbClient = new DBClient());
        }
        #endregion
        #region Methods

        public List<EntityObject> GetUniqueEntity(UniqueEntityType entityType, string name = null, bool getDomainController = false)
        {
            List<UniqueEntityType> entityTypes = new List<UniqueEntityType>();
            entityTypes.Add(entityType);
            return GetUniqueEntity(entityTypes, name, getDomainController);
        }

        public List<EntityObject> GetUniqueEntity(List<UniqueEntityType> entityTypes, string name = null, bool getDomainController = false)
        {
            List<EntityObject> allNames = new List<EntityObject>();

            List<IMongoQuery> queryElements = new List<IMongoQuery>();
            IMongoQuery mongoQuery;

            foreach (var oneEntityType in entityTypes)
            {
                queryElements.Add(Query.EQ("_t", Enum.GetName(typeof(UniqueEntityType), oneEntityType)));
            }
            if (queryElements.Count > 0)
            {
                mongoQuery = Query.Or(queryElements);
            }
            else
            {
                return allNames;
            }

            if (!string.IsNullOrEmpty(name))
            {
                mongoQuery = Query.And(mongoQuery, Query.EQ("Name", name));
            }

            if (getDomainController)
            {
                mongoQuery = Query.And(mongoQuery, Query.EQ("IsDomainController", true));
            }

            var result = _uniqueEntitiesCollection.Find(mongoQuery);
            foreach (var oneResult in result)
            {
                EntityObject entityObject = null;
                var resultBson = oneResult.FirstOrDefault().ToBsonDocument();
                if (oneResult.GetValue("Name").GetType() != typeof(BsonNull))
                {
                    BsonArray objectType = oneResult.GetValue("_t").AsBsonArray;
                    UniqueEntityType currentObjectType = UniqueEntityType.User;
                    if (objectType[objectType.Count - 1] == Enum.GetName(typeof(UniqueEntityType), UniqueEntityType.Computer))
                    {
                        currentObjectType = UniqueEntityType.Computer;
                    }

                    if (objectType[objectType.Count - 1] == Enum.GetName(typeof(UniqueEntityType), UniqueEntityType.Domain))
                    {
                        entityObject = new EntityObject(oneResult.GetValue("Name").AsString, oneResult.GetValue("_id").AsString, oneResult.GetValue("DnsName").AsString, currentObjectType);
                    }
                    else
                    {
                        entityObject = new EntityObject(oneResult.GetValue("Name").AsString, oneResult.GetValue("_id").AsString, currentObjectType);
                    }
                    allNames.Add(entityObject);
                }
            }
            return allNames;
        }

        public List<ObjectId> FilterGWIds()
        {
            List<ObjectId> _gatewayIds = new List<ObjectId>();
            foreach (var GwProfile in _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile")))
            {
                _gatewayIds.Add(GwProfile.GetElement("_id").Value.AsObjectId);
            }
            return _gatewayIds;
        }

        public List<ObjectId> GetGwOids()
        {
            return _gatewayIdsCollection;
        }

        public void TriggerAbnormalModeling()
        {
            IMongoQuery mongoQuery = Query.EQ("_t", "AbnormalBehaviorDetectorProfile");
            _systemProfilesCollection.Remove(mongoQuery);
        }

        public void InsertBatch(List<BsonDocument> documents, bool isSa = false, bool isPhoto = false, bool isEvent = false)
        {
            string collectionName;
            if (isSa)
                collectionName = "SuspiciousActivity";
            else if (isPhoto)
                collectionName = "UserPhoto";
            else if (isEvent)
                collectionName = "EventActivity";
            else
                collectionName = "NetworkActivity";
            _testDatabase.GetCollection(collectionName).InsertBatch(documents);
        }

        public void InsertBatchTest(List<BsonDocument> documents, bool isSa = false, bool isPhoto = false, bool isEvent = false)
        {
            string collectionName;
            if (isSa)
                collectionName = "SuspiciousActivity";
            else if (isPhoto)
                collectionName = "UserPhoto";
            else if (isEvent)
                collectionName = "EventActivity";
            else
                collectionName = "NetworkActivity";
            _database.GetCollection(collectionName).InsertBatch(documents);
        }
        public void SetCenterProfileForReplay()
        {
            
            var centerSystemProfile = _systemProfilesCollection.Find(Query.EQ("_t", "CenterSystemProfile"));
            foreach (var centerProfile in centerSystemProfile)
            {
                var configurationBson = centerProfile.GetElement("Configuration").Value.AsBsonDocument;
                var uniqueEntityProfileCacheConfigurationBson =
                    configurationBson.GetElement("UniqueEntityProfileCacheConfiguration").Value.AsBsonDocument;
                uniqueEntityProfileCacheConfigurationBson.Remove("StoreUniqueEntityProfilesInterval");
                uniqueEntityProfileCacheConfigurationBson.Add("StoreUniqueEntityProfilesInterval", "00:00:30");
                configurationBson.Remove("UniqueEntityProfileCacheConfiguration");
                configurationBson.Add("UniqueEntityProfileCacheConfiguration", uniqueEntityProfileCacheConfigurationBson);

                var activitySimulatorConfigurationBson =
                    configurationBson.GetElement("ActivitySimulatorConfiguration").Value.AsBsonDocument;

                activitySimulatorConfigurationBson.Remove("DelayInterval");
                activitySimulatorConfigurationBson.Remove("SimulationState");
                activitySimulatorConfigurationBson.Add("DelayInterval","00:00:05");
                activitySimulatorConfigurationBson.Add("SimulationState","Replay");
                    
                configurationBson.Remove("ActivitySimulatorConfiguration");
                configurationBson.Add("ActivitySimulatorConfiguration", activitySimulatorConfigurationBson);

                centerProfile.Remove("Configuration");
                centerProfile.Add("Configuration", configurationBson);
                _systemProfilesCollection.Save(centerProfile);

            }
          
        }

        public void RenameKerbCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0)).ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            _kerberosCollections = _database.GetCollectionNames().Where(collectionName => collectionName.Contains("Kerberos")).ToList();
            foreach (var collection in _kerberosCollections)
            {
                if (collection.Contains("KerberosAs"))
                {
                    _database.RenameCollection(collection,
                   "KerberosAs_" + monthAgo);
                }

                else if (collection.Contains("KerberosTgs"))
                {
                    _database.RenameCollection(collection,
                    "KerberosTgs_" + monthAgo);
                }
            }
            _logger.Debug("Renamed Kerberos Collection");
        }

        public void RenameNtlmEventsCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0))
                .ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            _ntlmEventsCollections =
                _database.GetCollectionNames().Where(collectionName => collectionName.Contains("NtlmEvent")).ToList();
            foreach (var collection in _ntlmEventsCollections)
            {
                _database.RenameCollection(collection, "NtlmEvent_" + monthAgo);
                _logger.Debug("Renamed NTLM event collection");
            }
        }

        public void CreateActivityCollectionsOnTestDB()
        {
            try
            {
                _testDatabase.CreateCollection("NetworkActivity");
                _testDatabase.CreateCollection("EventActivity");
            }
            catch { }
        }
        public void ClearTestNaCollection()
        {
            _testDatabase.DropCollection("NetworkActivity");
            _testDatabase.CreateCollection("NetworkActivity");
            _logger.Debug("Cleared Test NA's collection");
        }

        //public void InsertBatchPhotos(List<BsonDocument> photos)
        //{
        //    _database.GetCollection("UserPhoto").InsertBatch(photos);
        //}

        #endregion
    }
}
