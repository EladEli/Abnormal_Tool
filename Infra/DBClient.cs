using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Tri.Common.Service;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;

namespace Abnormal_UI.Infra
{
    public class DbClient
    {
        #region Data Members
        
        private static DbClient _dbClient;
        public readonly IMongoDatabase Database;
        public readonly IMongoDatabase TestDatabase;
        public readonly IMongoCollection<BsonDocument> UniqueEntitieCollection;
        public readonly IMongoCollection<BsonDocument> SystemProfileCollection;
        public readonly IMongoCollection<BsonDocument> DataProfileCollection;
        public readonly List<ObjectId> GatewayIdsCollection;
        public List<string> KerberosCollections;
        public List<string> NtlmCollections;
        public List<string> NtlmEventsCollections;
        private readonly Logger _logger;

        #endregion
        
        #region Ctors
        private DbClient()
        {
            const string connectionString = "mongodb://127.0.0.1:27017";
            try
            {
                StaticConfiguration.Initialize();
                _logger = LogManager.GetLogger("TestToolboxLog");
                var client = new MongoClient(connectionString);
                Database = client.GetDatabase("ATA");
                TestDatabase = client.GetDatabase("ATAActivitySimulator");
                DataProfileCollection = Database.GetCollection<BsonDocument>("DataProfile");
                UniqueEntitieCollection = Database.GetCollection<BsonDocument>("UniqueEntity");
                SystemProfileCollection = Database.GetCollection<BsonDocument>("SystemProfile");
                GatewayIdsCollection = new List<ObjectId>(0);
                GatewayIdsCollection = FilterGwIds();
                CreateActivityCollectionsOnTestDb();
            }
            catch (Exception dbException)
            {
                _logger.Error(dbException);
            }
        }
        public static DbClient GetDbClient()
        {
            return _dbClient ?? (_dbClient = new DbClient());
        }

        #endregion

        #region Methods

        public List<EntityObject> GetUniqueEntity(UniqueEntityType entityType, bool getDomainController = false)
        {
            return getDomainController
                ? UniqueEntitieCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", entityType.ToString())).
                    ToList().
                    Where(_ => _["IsDomainController"].AsBoolean).
                    Select(_ => new EntityObject(_["Name"].AsString, _["_id"].AsString,null, entityType)).
                    ToList()
                : UniqueEntitieCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", entityType.ToString()))
                    .ToList().Where(_ => _["Name"] != BsonNull.Value)
                    .Select(_ => new EntityObject(_["Name"].AsString, _["_id"].AsString, _.Contains("SamName") ? _["SamName"].AsString:"", entityType))
                    .ToList();
        }
        public List<EntityObject> GetSensitiveGroups()
        {
            return
                UniqueEntitieCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", UniqueEntityType.Group.ToString()))
                    .ToList()
                    .Where(_ => _["IsSensitiveSid"].AsBoolean)
                    .Select(_ => new EntityObject(_["Name"].AsString, _["_id"].AsString, null, UniqueEntityType.Group))
                    .ToList();
        }
        public List<ObjectId> FilterGwIds()
        {
            return
                SystemProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "GatewaySystemProfile"))
                    .ToEnumerable()
                    .Select(gwProfile => gwProfile.GetElement("_id").Value.AsObjectId)
                    .ToList();
        }
        public void SetDetectorProfileForSamr(string retentionPeriodTime)
        {
            var dateTime = DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0));
            var samrDetecotrSystemProfile = DataProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "SamrReconnaissanceDetectorProfile")).ToEnumerable();
            var centerSystemProfile = SystemProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "CenterSystemProfile")).ToEnumerable();
            var newDetector = new ComputerProfile
            {
                DestinationComputerIdToDetectionStartTimeMapping = new Dictionary<string, DateTime>
                {
                    ["4d4193e6-46f6-478f-81ff-50b655279e02"] = dateTime,
                    ["d68772fe-1171-4124-9f73-0f410340bd54"] = dateTime,
                    ["339fdbbf-fdf4-4ec4-b4a3-100e3ca99289"] = dateTime,
                    ["503254de-a822-44cc-9b06-a65cc899d408"] = dateTime,
                    ["76ebfd99-5722-480a-93b6-cbee134c90c1"] = dateTime,
                }
            };
            foreach (var detectorProfile in samrDetecotrSystemProfile)
            {
                detectorProfile["DestinationComputerIdToDetectionStartTimeMapping"] = newDetector.ToBsonDocument()["DestinationComputerIdToDetectionStartTimeMapping"];
                DataProfileCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", detectorProfile["_id"]),
                    detectorProfile);
            }

            
            foreach (var centerProfile in centerSystemProfile)
            {
                var configurationBson = centerProfile["Configuration"];
                configurationBson["SamrReconnaissanceDetectorConfiguration"]["UpsertProfileConfiguration"]["Interval"] ="00:00:30";
                configurationBson["SamrReconnaissanceDetectorConfiguration"]["OperationRetentionPeriod"] = retentionPeriodTime;
                configurationBson["SamrReconnaissanceDetectorConfiguration"]["RemoveOldOperationsConfiguration"]["Interval"] = "00:04:00";
                centerProfile["Configuration"] = configurationBson;
                SystemProfileCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", centerProfile["_id"]),centerProfile);
            }
        }
        public List<ObjectId> GetGwOids()
        {
            return GatewayIdsCollection;
        }
        public void DisposeAbnormalDetectorProfile()
        {
            var mongoQuery = Builders<BsonDocument>.Filter.Eq("_t", "AbnormalBehaviorDetectorProfile");
            DataProfileCollection.DeleteMany(mongoQuery.ToBsonDocument());
        }
        public void ResetUniqueEntityProfile()
        {
            var nullBsonArray = new BsonArray();
            var uniqueEntitiesProfileCollection =
                Database.GetCollection<BsonDocument>("UniqueEntityProfile")
                    .Find(Builders<BsonDocument>.Filter.Eq("_t", "AccountProfile")).ToEnumerable()
                ;
            foreach (var uniqueEntityProfileDocument in uniqueEntitiesProfileCollection)
            {
                uniqueEntityProfileDocument["KerberosTicketHashKeyToFirstKerberosTicketUsageDataMapping"] = nullBsonArray;
                uniqueEntityProfileDocument.Remove("DateToDomainControllerIdToProtocolToSourceComputerIdToBruteForceDataMapping");
                uniqueEntityProfileDocument["DateToActiveHoursMapping"] = nullBsonArray;
                Database.GetCollection<BsonDocument>("UniqueEntityProfile")
                    .ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", uniqueEntityProfileDocument["_id"]),
                        uniqueEntityProfileDocument);
            }
        }
        public void InsertBatch(List<BsonDocument> documents)
        {
            var events = documents.Where(_ => _["_t"].ToString().Contains("EventActivity")).ToList();
            var networkActivities = documents.Where(_ => _["_t"].ToString().Contains("NetworkActivity")).ToList();
            if (events.Any())
            {
                TestDatabase.GetCollection<BsonDocument>("EventActivity").InsertMany(events);
            }
            if (networkActivities.Any())
            {
                TestDatabase.GetCollection<BsonDocument>("NetworkActivity").InsertMany(networkActivities);
            }
        }
        public void InsertSaBatch(List<BsonDocument> documents)
        {
            Database.GetCollection<BsonDocument>("SuspiciousActivity").InsertMany(documents);
        }
        public void SetCenterProfileForReplay()
        {
            var centerSystemProfile =
                SystemProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "CenterSystemProfile")).ToEnumerable();
            foreach (var centerProfile in centerSystemProfile)
            {
                var configurationBson = centerProfile["Configuration"];
                configurationBson["UniqueEntityProfileCacheConfiguration"]["StoreUniqueEntityProfilesConfiguration"]["Interval"] =
                    "00:00:30";
                configurationBson["ActivitySimulatorConfiguration"]["DelayInterval"] = "00:00:05";
                configurationBson["ActivitySimulatorConfiguration"]["SimulationState"] = "Replay";
                configurationBson["AbnormalBehaviorDetectorConfiguration"]["UpsertProfileConfiguration"]["Interval"] =
                    "00:00:05";
                centerProfile["Configuration"] = configurationBson;
                SystemProfileCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", centerProfile["_id"]),
                    centerProfile);
            }
        }
        public void SetCenterProfileForVpn()
        {
            var centerSystemProfile =
                SystemProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "CenterSystemProfile")).ToEnumerable();
            foreach (var centerProfile in centerSystemProfile)
            {
                var commonConfigurationBson = centerProfile["GatewayCommonConfiguration"];
                var radiusSharedSecret = new BsonDocument
                {
                    {"CertificateThumbprint", commonConfigurationBson["HashKeyEncrypted"]["CertificateThumbprint"]},
                    {"EncryptedBytes",commonConfigurationBson["HashKeyEncrypted"]["EncryptedBytes"]}
                };
                commonConfigurationBson["IsRadiusEventListenerEnabled"] = BsonValue.Create(true);
                commonConfigurationBson["RadiusEventListenerSharedSecretEncrypted"] = radiusSharedSecret;
                centerProfile["GatewayCommonConfiguration"] = commonConfigurationBson;
                SystemProfileCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", centerProfile["_id"]),
                    centerProfile);
            }
        }
        public void RenameKerbCollections()
        {
            var monthAgo = DateTime.UtcNow.Subtract(new TimeSpan(27, 0, 0, 0)).ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
            KerberosCollections =
                Database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("Kerberos")).ToList()
                    ;
            try
            {
                if (KerberosCollections.Any())
                {
                    foreach (var collection in KerberosCollections)
                    {
                        if (collection.Contains("KerberosAs"))
                        {
                            Database.RenameCollection(collection,
                                "KerberosAs_" + monthAgo);
                        }
                        else if (collection.Contains("KerberosTgs"))
                        {
                            Database.RenameCollection(collection,
                                "KerberosTgs_" + monthAgo);
                        }
                    }
                    _logger.Debug("Renamed Kerberos Collection");
                }
                else
                {
                    Database.CreateCollection("KerberosAs_" + monthAgo);
                    Database.CreateCollection("KerberosTgs_" + monthAgo);
                    _logger.Debug("Created Kerberos Collection");
                }
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
            NtlmEventsCollections = 
                Database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("NtlmE")).ToList()
                    ;
            try
            {
                if (NtlmEventsCollections.Any())
                {
                    foreach (var collection in NtlmEventsCollections)
                    {
                        Database.RenameCollection(collection, "NtlmEvent_" + monthAgo);
                    }
                    _logger.Debug("Renamed NTLM event collection");
                }
                else
                {
                    Database.CreateCollection("NtlmEvent_" + monthAgo);
                    _logger.Debug("Created NTLM event collection");
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
            NtlmCollections =
                Database.ListCollections()
                    .ToList()
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("Ntlm_")).ToList()
                    ;
            try
            {
                if (NtlmCollections.Any())
                {
                    foreach (var collection in NtlmCollections)
                    {
                        Database.RenameCollection(collection, "Ntlm_" + monthAgo);
                    }
                    _logger.Debug("Renamed NTLM collection");
                }
                else
                {
                    Database.CreateCollection("Ntlm_" + monthAgo);
                    _logger.Debug("Created NTLM collection");
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
                TestDatabase.GetCollection<BsonDocument>("EventActivity");
                TestDatabase.GetCollection<BsonDocument>("NetworkActivity");
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }
        }
        public void ClearTestCollections()
        {
            TestDatabase.DropCollection("NetworkActivity");
            TestDatabase.CreateCollection("NetworkActivity");
            TestDatabase.DropCollection("EventActivity");
            TestDatabase.CreateCollection("EventActivity");
            _logger.Debug("Cleared Test collections");
        }
        public void SetNewGateway(int amount)
        {
            try
            {
                var gatewaySystemProfile = SystemProfileCollection.Find(Builders<BsonDocument>.Filter.Eq("_t", "GatewaySystemProfile")).ToEnumerable().First();
                var gatewayName = gatewaySystemProfile["NetbiosName"].AsString;
                for (var i = 0; i < amount; i++)
                {
                    gatewaySystemProfile["NetbiosName"] = gatewayName + " " + i;
                    gatewaySystemProfile["_id"] = new ObjectId();
                    SystemProfileCollection.InsertOne(gatewaySystemProfile);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            
        }
        public void DisposeDatabae()
        {
            TestDatabase.Client.DropDatabase("ATAActivitySimulator");
        }

        public class ComputerProfile
        {
            public Dictionary<string, DateTime> DestinationComputerIdToDetectionStartTimeMapping { get; set; }
        }
        #endregion
    }
}
