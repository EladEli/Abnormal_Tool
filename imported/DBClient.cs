﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
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
            var entityTypes = new List<UniqueEntityType>();
            entityTypes.Add(entityType);
            return GetUniqueEntity(entityTypes, name, getDomainController);
        }

        public List<EntityObject> GetUniqueEntity(List<UniqueEntityType> entityTypes, string name = null, bool getDomainController = false)
        {
            var allNames = new List<EntityObject>();

            var queryElements = new List<IMongoQuery>();
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

            var result = _uniqueEntitiesCollection.Find(mongoQuery.ToBsonDocument());
            foreach (var oneResult in result.ToEnumerable())
            {
                EntityObject entityObject = null;
                //var resultBson = oneResult.FirstOrDefault().ToBsonDocument();
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
            return _systemProfilesCollection.Find(Query.EQ("_t", "GatewaySystemProfile").ToBsonDocument()).ToEnumerable().Select(GwProfile => GwProfile.GetElement("_id").Value.AsObjectId).ToList();
        }

        public List<ObjectId> GetGwOids()
        {
            return _gatewayIdsCollection;
        }

        public void TriggerAbnormalModeling()
        {
            IMongoQuery mongoQuery = Query.EQ("_t", "AbnormalBehaviorDetectorProfile");
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
                _systemProfilesCollection.ReplaceOne(Builders<BsonDocument>.Filter.Eq("_id", centerProfile["_id"]),
                    centerProfile);

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
                    .Select(_ => _["name"].AsString).Where(_ => _.StartsWith("Ntlm")).ToList()
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
            catch { }
        }
        public void ClearTestNaCollection()
        {
            _testDatabase.DropCollection("NetworkActivity");
            _testDatabase.CreateCollection("NetworkActivity");
            _logger.Debug("Cleared Test NA's collection");
        }

        #endregion
    }
}