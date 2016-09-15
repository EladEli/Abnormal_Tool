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
        public readonly IMongoDatabase _database;
        public readonly IMongoDatabase _testDatabase;
        private readonly IMongoCollection<BsonDocument> _uniqueEntitiesCollection;
        public readonly IMongoCollection<BsonDocument> _systemProfilesCollection;
        readonly List<ObjectId> _gatewayIdsCollection;
        public List<string> _kerberosCollections;
        public List<string> _ntlmCollections;
        public List<string> _ntlmEventsCollections;
        private readonly Logger _logger;

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
                        $"mongodb://{Interaction.InputBox("Please enter the server Address", "Target Server", "127.0.0.1", -1, -1)}:27017"))
                {
                    connectionString = "mongodb://127.0.0.1:27017";
                }
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase("ATA");
                _testDatabase = client.GetDatabase("ATAActivitySimulator");
                _uniqueEntitiesCollection = _database.GetCollection<BsonDocument>("UniqueEntity");
                _systemProfilesCollection = _database.GetCollection<BsonDocument>("SystemProfile");
                _gatewayIdsCollection = new List<ObjectId>(0);
                _gatewayIdsCollection = FilterGwIds();
                CreateActivityCollectionsOnTestDb();
            }
            catch (Exception dbException)
            {
                _logger.Error(dbException);
            }
            
            
        }
        public static DBClient GetDbClient()
        {
            return _dbClient ?? (_dbClient = new DBClient());
        }
        #endregion
        #region Methods
        
        public List<EntityObject> GetUniqueEntity(UniqueEntityType entityType, bool getDomainController = false)
        {
            return getDomainController
                ? _uniqueEntitiesCollection.Find(Query.EQ("_t", entityType.ToString()).ToBsonDocument()).
                    ToList().
                    Where(_ => _["IsDomainController"].AsBoolean).
                    Select(_ => new EntityObject(_["Name"].AsString, _["_id"].AsString, entityType)).
                    ToList()
                : _uniqueEntitiesCollection.Find(Query.EQ("_t", entityType.ToString()).ToBsonDocument())
                    .ToList().Where(_ => _["Name"] != BsonNull.Value)
                    .Select(_ => new EntityObject(_["Name"].AsString, _["_id"].AsString, entityType))
                    .ToList();
        }

        public List<ObjectId> FilterGwIds()
        {
            return
                _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile").ToBsonDocument())
                    .ToEnumerable()
                    .Select(gwProfile => gwProfile.GetElement("_id").Value.AsObjectId)
                    .ToList();
        }

        public List<ObjectId> GetGwOids()
        {
            return _gatewayIdsCollection;
        }

        public void DisposeAbnormalDetectorProfile()
        {
            var mongoQuery = Query.EQ("_t", "AbnormalBehaviorDetectorProfile");
            _systemProfilesCollection.DeleteMany(mongoQuery.ToBsonDocument());
        }

        public void InsertBatch(List<BsonDocument> documents)
        {
            var events = documents.Where(_ => _["_t"].ToString().Contains("NtlmEvent")).ToList();
            var networkActivities = documents.Where(_ => _["_t"].ToString().Contains("NetworkActivity")).ToList();
            if (events.Any())
            {
                _testDatabase.GetCollection<BsonDocument>("EventActivity").InsertMany(events);
            }
            if (networkActivities.Any())
            {
                _testDatabase.GetCollection<BsonDocument>("NetworkActivity").InsertMany(networkActivities);
            }
        }

        public void InsertSaBatch(List<BsonDocument> documents)
        {
            _database.GetCollection<BsonDocument>("SuspiciousActivity").InsertMany(documents);
        }
        public void SetCenterProfileForReplay()
        {

            var centerSystemProfile =
                _systemProfilesCollection.Find(Query.EQ("_t", "CenterSystemProfile").ToBsonDocument()).ToEnumerable();
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
                    "00:00:01";
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

        public void CreateActivityCollectionsOnTestDb()
        {
            try
            {
                _testDatabase.GetCollection<BsonDocument>("EventActivity");
                _testDatabase.GetCollection<BsonDocument>("NetworkActivity");
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
            _testDatabase.DropCollection("EventActivity");
            _testDatabase.CreateCollection("EventActivity");
            _logger.Debug("Cleared Test collections");
        }

        public void ClearDsaCollection()
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

        public void SetNewGateway(int amount)
        {
            try
            {
                var gatewaySystemProfile = _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile").ToBsonDocument()).ToEnumerable().First();
                var gatewayName = gatewaySystemProfile["NetbiosName"].AsString;
                for (var i = 0; i < amount; i++)
                {
                    gatewaySystemProfile["NetbiosName"] = gatewayName + " " + i;
                    gatewaySystemProfile["_id"] = new ObjectId();
                    _systemProfilesCollection.InsertOne(gatewaySystemProfile);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            
        }

        public void DisposeDatabae()
        {
            _testDatabase.Client.DropDatabase("ATAActivitySimulator");

        }

        #endregion
    }
}
