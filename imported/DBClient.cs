using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        IMongoDatabase _database;
        IMongoDatabase _testDatabase;
        IMongoCollection<BsonDocument> _uniqueEntitiesCollection;
        IMongoCollection<BsonDocument> _systemProfilesCollection;
        List<ObjectId> _gatewayIdsCollection;
        List<string> _kerberosCollections;
        List<string> _ntlmCollections;
        List<string> _ntlmEventsCollections;
        private Logger _logger;

        #endregion
        
        #region Ctors
        private DBClient()
        {
            _logger = LogManager.GetLogger("TestToolboxLog");
            try
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
                _database = _client.GetDatabase("ATA");
                _testDatabase = _client.GetDatabase("ATAActivitySimulator");
                _uniqueEntitiesCollection = _database.GetCollection<BsonDocument>("UniqueEntity");
                _systemProfilesCollection = _database.GetCollection<BsonDocument>("SystemProfile");
                _gatewayIdsCollection = new List<ObjectId>(0);
                _gatewayIdsCollection = FilterGWIds();
                CreateActivityCollectionsOnTestDB();
            }
            catch (Exception dbException)
            {
                _logger.Error(dbException);
            }
            
            
        }
        public static DBClient getDBClient()
        {
            return _dbClient ?? (_dbClient = new DBClient());
        }
        #endregion
        #region Methods

        public List<EntityObject> GetUniqueEntity(UniqueEntityType entityType, string name = null, bool getDomainController = false)
        {
            var entityTypes = new List<UniqueEntityType> {entityType};
            return GetUniqueEntity(entityTypes, name, getDomainController);
        }

        public List<EntityObject> GetUniqueEntity(List<UniqueEntityType> entityTypes, string name = null, bool getDomainController = false)
        {
            var allNames = new List<EntityObject>();

            IMongoQuery mongoQuery;

            var queryElements = entityTypes.Select(oneEntityType => Query.EQ("_t", Enum.GetName(typeof (UniqueEntityType), oneEntityType))).ToList();
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

            var result = _uniqueEntitiesCollection.Find(mongoQuery.ToBsonDocument());
            foreach (var oneResult in result.ToEnumerable())
            {
                if (oneResult.GetValue("Name").GetType() != typeof(BsonNull))
                {
                    var objectType = oneResult.GetValue("_t").AsBsonArray;
                    var currentObjectType = UniqueEntityType.User;
                    if (objectType[objectType.Count - 1] == Enum.GetName(typeof(UniqueEntityType), UniqueEntityType.Computer))
                    {
                        currentObjectType = UniqueEntityType.Computer;
                    }

                    EntityObject entityObject = null;
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
            return _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile").ToBsonDocument()).ToEnumerable().Select(GwProfile => GwProfile.GetElement("_id").Value.AsObjectId).ToList();
        }

        public List<ObjectId> GetGwOids()
        {
            return _gatewayIdsCollection;
        }

        public void TriggerAbnormalModeling()
        {
            var mongoQuery = Query.EQ("_t", "AbnormalBehaviorDetectorProfile");
            _systemProfilesCollection.DeleteMany(mongoQuery.ToBsonDocument());
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
            _testDatabase.GetCollection<BsonDocument>(collectionName).InsertMany(documents);
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
            _database.GetCollection<BsonDocument>(collectionName).InsertMany(documents);
        }
        public void SetCenterProfileForReplay()
        {
            
            var centerSystemProfile = _systemProfilesCollection.Find(Query.EQ("_t", "CenterSystemProfile").ToBsonDocument()).ToEnumerable();
            foreach (var centerProfile in centerSystemProfile)
            {
                var configurationBson = centerProfile["Configuration"];
                configurationBson["UniqueEntityProfileCacheConfiguration"]["StoreUniqueEntityProfilesInterval"] =
                    "00:00:30";
                configurationBson["ActivitySimulatorConfiguration"]["DelayInterval"] = "00:00:05";
                configurationBson["ActivitySimulatorConfiguration"]["SimulationState"] = "Replay";
                centerProfile["Configuration"] = configurationBson;
                _systemProfilesCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", centerProfile["_id"]),
                    centerProfile);

            }
          
        }

        public void SetGatewayProfileForDsa()
        {
            var gatewaySystemProfile =
                _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile").ToBsonDocument()).ToEnumerable();
            foreach (var gatewayProfile in gatewaySystemProfile)
            {
                var configurationBson = gatewayProfile["Configuration"];
                configurationBson["DirectoryServicesResolverConfiguration"]["UpdateDirectoryEntityChangesInterval"] =
                    "00:00:02";
                gatewayProfile["Configuration"] = configurationBson;
                _systemProfilesCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", gatewayProfile["_id"]),
                    gatewayProfile);
            }
        }

        public void RenameKerbCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0)).ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            _kerberosCollections =
                _database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("Kerberos")).ToList()
                    ;
            try
            {
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
            catch (Exception)
            {
                _logger.Debug("Kerberos collection already renamed");
            }
            
        }

        public void RenameNtlmEventsCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0))
                .ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            _ntlmEventsCollections = 
                _database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("NtlmE")).ToList()
                    ;
            try
            {
                foreach (var collection in _ntlmEventsCollections)
                {
                    _database.RenameCollection(collection, "NtlmEvent_" + monthAgo);
                    _logger.Debug("Renamed NTLM event collection");
                }
            }
            catch (Exception)
            {
                _logger.Debug("Ntlm events collection alraedy renamed");
            }
            
        }

        public void RenameNtlmCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0))
                .ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            _ntlmCollections =
                _database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("Ntlm_")).ToList()
                    ;
            try
            {
                foreach (var collection in _ntlmCollections)
                {
                    _database.RenameCollection(collection, "Ntlm_" + monthAgo);
                    _logger.Debug("Renamed NTLM collection");
                }
            }
            catch (Exception)
            {
                _logger.Debug("Ntlm collection alraedy renamed");
            }

        }

        public void CreateActivityCollectionsOnTestDB()
        {
            try
            {
                _testDatabase.CreateCollection("NetworkActivity");
                _testDatabase.CreateCollection("EventActivity");
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }
        public void ClearTestNaCollection()
        {
            _testDatabase.DropCollection("NetworkActivity");
            _testDatabase.CreateCollection("NetworkActivity");
            _logger.Debug("Cleared Test NA's collection");
        }

        public void CleaDsaCollection()
        {
            _database.DropCollection("DirectoryServicesActivity");
            _database.CreateCollection("DirectoryServicesActivity");
            _logger.Debug("Cleared Dsa's collection");
        }

        public bool CheckDatabaseForDsa(string dsaForCheck)
        {
            var dsaCollection = _database.GetCollection<BsonDocument>("DirectoryServicesActivity");
            return dsaCollection.ToBsonDocument().Any(dsa => dsaForCheck == dsa.ToString());
        }

        #endregion
    }
}
