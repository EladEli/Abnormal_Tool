using System;
using System.Collections.Generic;
using Abnormal_UI.Imported;
using MongoDB.Bson;


namespace Abnormal_UI.Infra
{
    public static class DocumentCreator
    {
        public static BsonDocument KerberosCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainController, string domainName, ObjectId sourceGateway, string targetSpn = null, EntityObject targetMachine = null, string actionType = "As", int daysToSubtruct = 0, int hoursToSubtract = 0, ObjectId parentId = new ObjectId())
        {
            var oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, hoursToSubtract, 0, 0, 0));
            var sourceAccount = new BsonDocument {{"DomainName", domainName}, {"Name", userEntity.name}};

            var resourceIdentifier = new BsonDocument();
            var targetAccount = targetMachine ?? domainController;
            var targetSpnName = $"krbtgt/{domainName}";
            if (targetSpn != null) { targetSpnName = targetSpn; }
            resourceIdentifier.Add("AccountId", targetAccount.id);
            var resourceName = new BsonDocument {{"DomainName", domainName}, {"Name", targetSpnName}};
            resourceIdentifier.Add("ResourceName", resourceName);

            var responseTicket = new BsonDocument
            {
                {"EncryptionType", "Aes256CtsHmacSha196"},
                {"IsReferral", false},
                {"Realm", BsonValue.Create(null)},
                {"ResourceIdentifier", resourceIdentifier},
                {"Size", 1084},
                {"Hash", new byte[16]}
            };

            var networkActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {
                    "_t",
                    new BsonArray(new string[5]
                    {"Entity", "NetworkActivity", "Kerberos", "KerberosKdc", "Kerberos" + actionType})
                },
                {"HorizontalParentId", new ObjectId()},
                {"StartTime", oldTime},
                {"EndTime", oldTime},
                {"SourceIpAddress", "[daf::daf]"},
                {"SourcePort", 51510},
                {"SourceComputerId", computerEntity.id},
                {"SourceComputerSiteId", BsonValue.Create(null)},
                {"SourceComputerCertainty", "High"},
                {"SourceComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"DestinationIpAddress", "[daf::daf]"},
                {"DestinationPort", 88},
                {"DestinationComputerId", domainController.id},
                {"DestinationComputerSiteId", BsonValue.Create(null)},
                {"DestinationComputerCertainty", "High"},
                {"DestinationComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"TransportProtocol", "Tcp"},
                {"SourceAccountName", sourceAccount},
                {"SourceAccountId", userEntity.id},
                {"SourceComputerSupportedEncryptionTypes", new BsonArray(new string[1] {"Rc4Hmac"})},
                {"ResourceIdentifier", resourceIdentifier},
                
                {"Error", "Success"},
                {"NtStatus", BsonValue.Create(null)},
                {"IsSuccess", BsonValue.Create(false)},
                {"Options", new BsonArray(new string[4] {"RenewableOk", "Canonicalize", "Renewable", "Forwardable"})},
                {"RequestedTicketExpiration", DateTime.UtcNow},
                {"SourceGatewaySystemProfileId", sourceGateway},
        };


            if (actionType == "As")
            {
                networkActivityDocument.Add("RequestTicketKerberosId", parentId);
                networkActivityDocument.Add("IsOldPassword", BsonValue.Create(null));
                networkActivityDocument.Add("IsIncludePac", BsonValue.Create(true));
                networkActivityDocument.Add("SourceAccountSupportedEncryptionTypes", new BsonArray(new string[0]));
                networkActivityDocument.Add("SourceAccountBadPasswordTime", BsonValue.Create(null));
                networkActivityDocument.Add("SourceComputerNetbiosName", computerEntity.name);
                networkActivityDocument.Add("EncryptedTimestampEncryptionType", BsonValue.Create(null));
                networkActivityDocument.Add("EncryptedTimestamp", BsonValue.Create(null));
                networkActivityDocument.Add("RequestTicket", BsonValue.Create(null));
                networkActivityDocument.Add("ResponseTicket", responseTicket);


            }
            else
            {
                networkActivityDocument.Add("RequestTicketKerberosId", parentId);
                networkActivityDocument.Add("IsServiceForUserToSelf", BsonValue.Create(false));
                networkActivityDocument.Add("AuthorizationDataSize", BsonValue.Create(null));
                networkActivityDocument.Add("AuthorizationDataEncryptionType", BsonValue.Create(null));
                networkActivityDocument.Add("ParentsOptions", "None");
                networkActivityDocument.Add("AdditionalTickets", new BsonArray(new string[0]));
                networkActivityDocument.Add("RequestTicket", responseTicket);
                networkActivityDocument.Add("ResponseTicket", responseTicket);
            }
            return networkActivityDocument;
        }
        public static BsonDocument SimpleBindCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainControllerName, string domainName, ObjectId sourceGateway, int daysToSubtruct = 0)
        {
            var oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, 0, 0, 0, 0));
            var sourceAccount = new BsonDocument {{"DomainName", domainName}, {"Name", userEntity.name}};

            var resourceIdentifier = new BsonDocument {{"AccountId", domainControllerName.id}};
            var targetSpnName = $"krbtgt/{domainName}";
            var resourceName = new BsonDocument {{"DomainName", domainName}, {"Name", targetSpnName}};
            resourceIdentifier.Add("ResourceName", resourceName);

            var networkActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {"_t", new BsonArray(new string[5] {"Entity", "Activity", "NetworkActivity", "Ldap", "LdapBind"})},
                {"StartTime", oldTime},
                {"EndTime", oldTime},
                {"HorizontalParentId", new ObjectId()},
                {"SourceIpAddress", "[daf::daf]"},
                {"SourcePort", 6666},
                {"SourceComputerId", computerEntity.id},
                {"SourceComputerSiteId", BsonValue.Create(null)},
                {"SourceComputerCertainty", "High"},
                {"SourceComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"DestinationIpAddress", "[daf::daf]"},
                {"DestinationPort", 389},
                {"DestinationComputerId", domainControllerName.id},
                {"DestinationComputerSiteId", BsonValue.Create(null)},
                {"DestinationComputerCertainty", "High"},
                {"DestinationComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"TransportProtocol", "Tcp"},
                {"AuthenticationType", "Simple"},
                {"SourceAccountName", sourceAccount},
                {"SourceAccountId", userEntity.id},
                {
                    "SourceAccountPasswordHash",
                    "9apJEBIqcse0SNKVm4WzIxaHoLkUareMCDlAzhADI72CLmxOMmA8WzicPPI84xxlewEEIqZZOJJ1VqpE5VJT4g=="
                },
                {"ResultCode", "Success"},
                {"IsSuccess", BsonValue.Create(true)},
                {"SourceGatewaySystemProfileId", sourceGateway},
                {"ResourceIdentifier", resourceIdentifier}
            };



            return networkActivityDocument;
        }
        public static BsonDocument SaFillerSeac(List<EntityObject> userEntity, List<EntityObject> computerEntity, Random rnd)
        {
            var detailRecord = new BsonDocument
            {
                {"StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"SourceComputerId", computerEntity[rnd.Next(0, computerEntity.Count)].id},
                {
                    "SourceAccountIds", new BsonArray(new string[20]
                    {
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id
                    })
                },
                {
                    "DestinationComputerIds", new BsonArray(new string[1]
                    {
                        computerEntity[rnd.Next(0, computerEntity.Count)].id
                    })
                }
            };
            var a = userEntity[rnd.Next(0, userEntity.Count)].id;
            var b = userEntity[rnd.Next(0, userEntity.Count)].id;

            var suspicousActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {
                    "_t", new BsonArray(new string[5]
                    {
                        "Entity", "Alert", "SuspiciousActivity", "SuspiciousActivity`1",
                        "LdapSimpleBindCleartextPasswordSuspiciousActivity"
                    })
                },
                {"StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"IsVisible", BsonValue.Create(true)},
                {"Severity", "Low"},
                {"Status", "Open"},
                {"TitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityTitleService"},
                {"TitleDetailKeys", new BsonArray(new string[0] {})},
                {
                    "DescriptionFormatKey",
                    "LdapSimpleBindCleartextPasswordSuspiciousActivityDescriptionServiceSourceAccounts"
                },
                {"DescriptionDetailFormatKeys", new BsonArray(new string[0] {})},
                {"SystemUpdateTime", DateTime.UtcNow},
                {"HasDetails", BsonValue.Create(true)},
                {"HasInput", BsonValue.Create(false)},
                {"InputTitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityInputTitle"},
                {"IsInputProvided", BsonValue.Create(false)},
                {"Note", BsonValue.Create(null)},
                {"RelatedActivityCount", 127},
                {
                    "RelatedUniqueEntityIds", new BsonArray(new string[20]
                    {
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id,
                        userEntity[rnd.Next(0, userEntity.Count)].id, userEntity[rnd.Next(0, userEntity.Count)].id
                    })
                },
                {"DetailsRecords", new BsonArray(new BsonDocument[1] {detailRecord})},
                {"Scope", "Service"}
            };
            return suspicousActivityDocument;
        }
        public static BsonDocument SaFillerAe(List<EntityObject> userEntity, List<EntityObject> computerEntity,EntityObject domainController, string domainName)
        {
            var records = new List<BsonDocument>();
            var detailRecord = new BsonDocument();
            
            for (var i =0; i < 100000; i++)
            {
                detailRecord.Add("DomainName", domainName);
                detailRecord.Add("Name", "ABCDEFGHIJKLMNOP" + i);
                records.Add(detailRecord);
                detailRecord = new BsonDocument();
            }
            var failedSourceAccountNames = new BsonArray(records);

            var suspicousActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {
                    "_t", new BsonArray(new string[4]
                    {
                        "Entity", "Alert", "SuspiciousActivity", "AccountEnumerationSuspiciousActivity"
                    })
                },
                {"StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0))},
                {"IsVisible", BsonValue.Create(true)},
                {"Severity", "Medium"},
                {"Status", "Open"},
                {"TitleKey", "AccountEnumerationSuspiciousActivityTitle"},
                {"TitleDetailKeys", new BsonArray(new string[0] {})},
                {"DescriptionFormatKey", "AccountEnumerationSuspiciousActivityDescription"},
                {"DescriptionDetailFormatKeys", new BsonArray(new string[0] {})},
                {"SystemUpdateTime", DateTime.UtcNow},
                {"HasDetails", BsonValue.Create(false)},
                {"HasInput", BsonValue.Create(false)},
                {"InputTitleKey", "AccountEnumerationSuspiciousActivityInputTitle"},
                {"IsInputProvided", BsonValue.Create(false)},
                {"Note", BsonValue.Create(null)},
                {"RelatedActivityCount", 127},
                {
                    "RelatedUniqueEntityIds", new BsonArray(new string[9]
                    {
                        computerEntity[2].id, domainController.id, userEntity[0].id, userEntity[1].id, userEntity[2].id,
                        userEntity[3].id, userEntity[4].id,
                        userEntity[5].id, userEntity[6].id
                    })
                },
                {"SourceComputerId", computerEntity[2].id},
                {"DestinationComputerIds", new BsonArray(new string[1] {domainController.id})},
                {"FailedSourceAccountNames", failedSourceAccountNames},
                {
                    "SuccessSourceAccountIds", new BsonArray(new string[7]
                    {
                        userEntity[0].id, userEntity[1].id, userEntity[2].id, userEntity[3].id, userEntity[4].id,
                        userEntity[5].id, userEntity[6].id
                    })
                }
            };
            return suspicousActivityDocument;
        }
        public static BsonDocument NotificationCreator(ObjectId alertId)
        {
            var notificationDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {
                    "_t", new BsonArray(new string[4]
                    {
                        "Entity", "Notification", "AlertNotification", "SuspiciousActivityNotification",
                    })
                },
                {"Level", "Notice"},
                {"CreationTime", DateTime.UtcNow},
                {"ExpirationTime", DateTime.UtcNow.AddDays(100)},
                {"CategoryKey", "SuspiciousActivityNotificationCategory"},
                {"AlertId", alertId},
                {"AlertTitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityTitleService"}
            };
            return notificationDocument;
        }
        public static BsonDocument EventCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainControllerName, string domainName,ObjectId sourceGateway, int daysToSubtruct = 0)
        {

            var sourceComputer = new BsonDocument
            {
                {"DomainName", BsonValue.Create(null)},
                {"Name", computerEntity.name}
            };

            var destinationComputer = new BsonDocument
            {
                {"DomainName", BsonValue.Create(null)},
                {"Name", domainControllerName.name}
            };

            var eventActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {
                    "_t",
                    new BsonArray(new string[5] {"Entity", "Activity", "EventActivity", "WindowsEvent", "NtlmEvent"})
                },
                {"SourceGatewaySystemProfileId", sourceGateway},
                {"SourceComputerId", computerEntity.id},
                {"DestinationComputerId", domainControllerName.id},
                {"Time", DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, 4, 0, 0, 0))},
                {"SourceComputerName", sourceComputer},
                {"DestinationComputerName", destinationComputer},
                {"IsTimeMillisecondsAccurate", BsonValue.Create(true)},
                {"CategoryName", "Security"},
                {"ProviderName", "Microsoft-Windows-Security-Auditing"},
                {"SourceAccountName", userEntity.name},
                {"SourceAccountId", userEntity.id},
                {"ErrorCode", "Success"},
                {"IsSuccess", BsonValue.Create(true)}
            };


            return eventActivityDocument;
        }

        public static BsonDocument NtlmCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainController, string domainName, ObjectId sourceGateway, string targetSPN = null, EntityObject targetMachine = null, int daysToSubtruct = 0, int hoursToSubtract = 0)
        {
            var oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, hoursToSubtract, 0, 0, 0));
            var sourceAccount = new BsonDocument {{"DomainName", domainName}, {"Name", userEntity.name}};

            var resourceIdentifier = new BsonDocument();
            var targetAccount = targetMachine ?? domainController;
            var targetSpnName = $"krbtgt/{domainName}";
            if (targetSPN != null) { targetSpnName = targetSPN; }
            resourceIdentifier.Add("AccountId", targetAccount.id);
            var resourceName = new BsonDocument {{"DomainName", domainName}, {"Name", targetSpnName}};
            resourceIdentifier.Add("ResourceName", resourceName);

            var networkActivityDocument = new BsonDocument
            {
                {"_id", new ObjectId()},
                {"_t", new BsonArray(new string[3] {"Entity", "NetworkActivity", "Ntlm"})},
                {"SourceGatewaySystemProfileId", sourceGateway},
                {"SourceComputerId", computerEntity.id},
                {"DestinationComputerId", targetAccount.id},
                {"HorizontalParentId", new ObjectId()},
                {"StartTime", oldTime},
                {"EndTime", oldTime},
                {"SourceIpAddress", "[daf::daf]"},
                {"SourcePort", 51510},
                {"SourceComputerSiteId", BsonValue.Create(null)},
                {"SourceComputerCertainty", "High"},
                {"SourceComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"DestinationIpAddress", "[daf::daf]"},
                {"DestinationPort", 88},
                {"DestinationComputerSiteId", BsonValue.Create(null)},
                {"DestinationComputerCertainty", "High"},
                {"DestinationComputerResolutionMethod", new BsonArray(new string[1] {"RpcNtlm"})},
                {"TransportProtocol", "Tcp"},
                {"Version", 2},
                {"SourceComputerNetbiosName", computerEntity.name},
                {"SourceAccountName", sourceAccount},
                {"SourceAccountId", userEntity.id},
                {"ResourceIdentifier", resourceIdentifier},
                {"DceRpcStatus", BsonValue.Create(null)},
                {"LdapResultCode", BsonValue.Create(null)},
                {"SmbStatus", "Success"},
                {"Smb1Status", BsonValue.Create(null)},
                {"IsSuccess", BsonValue.Create(true)}
            };


            return networkActivityDocument;
        }
    }
}
