using System;
using System.Collections.Generic;
using Abnormal_UI.Imported;
using MongoDB.Bson;


namespace Abnormal_UI.Infra
{
    public static class DocumentCreator
    {
        public static BsonDocument KerberosCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainController, string domainName, ObjectId sourceGateway, string targetSPN = null, EntityObject targetMachine = null, string actionType = "As", int daysToSubtruct = 0, int hoursToSubtract = 0)
        {
            var oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, hoursToSubtract, 0, 0, 0));
            var sourceAccount = new BsonDocument();
            sourceAccount.Add("DomainName", domainName);
            sourceAccount.Add("Name", userEntity.name);

            BsonDocument resourceIdentifier = new BsonDocument();
            EntityObject targetAccount = null;
            if (targetMachine != null) { targetAccount = targetMachine; }
            else { targetAccount = domainController; }

            string targetSPNName = string.Format("krbtgt/{0}", domainName);
            if (targetSPN != null) { targetSPNName = targetSPN; }
            resourceIdentifier.Add("AccountId", targetAccount.id);
            BsonDocument resourceName = new BsonDocument();
            resourceName.Add("DomainName", domainName);
            resourceName.Add("Name", targetSPNName);
            resourceIdentifier.Add("ResourceName", resourceName);

            BsonDocument networkActivityDocument = new BsonDocument();
            networkActivityDocument.Add("_id", new ObjectId());
            networkActivityDocument.Add("_t", new BsonArray(new string[5] { "Entity", "NetworkActivity", "Kerberos", "KerberosKdc", "Kerberos" + actionType }));
            networkActivityDocument.Add("HorizontalParentId", new ObjectId());
            networkActivityDocument.Add("StartTime", oldTime);
            networkActivityDocument.Add("EndTime", oldTime);
            networkActivityDocument.Add("ProcessingTime", oldTime);
            networkActivityDocument.Add("SourceIpAddress", "[daf::daf]");
            networkActivityDocument.Add("SourcePort", 51510);
            networkActivityDocument.Add("SourceComputerId", computerEntity.id);
            networkActivityDocument.Add("SourceComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("SourceComputerCertainty", "High");
            networkActivityDocument.Add("SourceComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("DestinationIpAddress", "[daf::daf]");
            networkActivityDocument.Add("DestinationPort", 88);
            networkActivityDocument.Add("DestinationComputerId", targetAccount.id);
            networkActivityDocument.Add("DestinationComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("DestinationComputerCertainty", "High");
            networkActivityDocument.Add("DestinationComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("TransportProtocol", "Tcp");
            networkActivityDocument.Add("SourceAccountName", sourceAccount);
            networkActivityDocument.Add("SourceAccountId", userEntity.id);
            networkActivityDocument.Add("SourceComputerSupportedEncryptionTypes", new BsonArray(new string[1] { "Rc4Hmac" }));
            networkActivityDocument.Add("ResourceIdentifier", resourceIdentifier);
            networkActivityDocument.Add("RequestTicketHash", new byte[16]);
            networkActivityDocument.Add("RequestTicketKerberosId", new ObjectId());
            networkActivityDocument.Add("Error", "Success");
            networkActivityDocument.Add("NtStatus", BsonValue.Create(null));
            networkActivityDocument.Add("ResponseTicketEncryptionType", "Aes256CtsHmacSha196");
            networkActivityDocument.Add("ResponseTicketHash", new byte[16]);
            networkActivityDocument.Add("IsSuccess", BsonValue.Create(false));
            networkActivityDocument.Add("Options", new BsonArray(new string[4] { "RenewableOk", "Canonicalize", "Renewable", "Forwardable" }));
            networkActivityDocument.Add("RequestedTicketExpiration", DateTime.UtcNow);
            networkActivityDocument.Add("SourceGatewaySystemProfileId", sourceGateway);
            

            if (actionType == "As")
            {
                networkActivityDocument.Add("IsOldPassword", BsonValue.Create(null));
                networkActivityDocument.Add("IsIncludePac", BsonValue.Create(true));
                networkActivityDocument.Add("SourceAccountSupportedEncryptionTypes", new BsonArray(new string[0]));
                networkActivityDocument.Add("SourceAccountBadPasswordTime", BsonValue.Create(null));
                networkActivityDocument.Add("SourceComputerNetbiosName", computerEntity.name);
                networkActivityDocument.Add("EncryptedTimestampEncryptionType", BsonValue.Create(null));
                networkActivityDocument.Add("EncryptedTimestamp", BsonValue.Create(null));
                networkActivityDocument.Add("ResponseTicketSize", 0);
            }
            else
            {
                networkActivityDocument.Add("IsReferralRequestTicket", BsonValue.Create(false));
                networkActivityDocument.Add("IsReferralResponseTicket", BsonValue.Create(false));
                networkActivityDocument.Add("IsServiceForUserToSelf", BsonValue.Create(false));
                networkActivityDocument.Add("RequestTicketEncryptionType", "Aes256CtsHmacSha196");
                networkActivityDocument.Add("AuthorizationDataSize", BsonValue.Create(null));
                networkActivityDocument.Add("AuthorizationDataEncryptionType", BsonValue.Create(null));
                networkActivityDocument.Add("RequestTicketSize", 0);
                networkActivityDocument.Add("ParentsOptions", "None");
            }
            return networkActivityDocument;
        }
        public static BsonDocument SimpleBindCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainControllerName, string domainName, ObjectId sourceGateway, int daysToSubtruct = 0)
        {
            DateTime oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, 0, 0, 0, 0));
            BsonDocument sourceAccount = new BsonDocument();
            sourceAccount.Add("DomainName", domainName);
            sourceAccount.Add("Name", userEntity.name);

            BsonDocument resourceIdentifier = new BsonDocument();
            resourceIdentifier.Add("AccountId", domainControllerName.id);
            string targetSPNName = string.Format("krbtgt/{0}", domainName);

            BsonDocument resourceName = new BsonDocument();
            resourceName.Add("DomainName", domainName);
            resourceName.Add("Name", targetSPNName);
            resourceIdentifier.Add("ResourceName", resourceName);

            BsonDocument networkActivityDocument = new BsonDocument();

            networkActivityDocument.Add("_id", new ObjectId());
            networkActivityDocument.Add("_t", new BsonArray(new string[5] { "Entity", "Activity", "NetworkActivity", "Ldap", "LdapBind" }));
            networkActivityDocument.Add("StartTime", oldTime);
            networkActivityDocument.Add("EndTime", oldTime);
            networkActivityDocument.Add("ProcessingTime", oldTime);
            networkActivityDocument.Add("HorizontalParentId", new ObjectId());
            networkActivityDocument.Add("SourceIpAddress", "[daf::daf]");
            networkActivityDocument.Add("SourcePort", 6666);
            networkActivityDocument.Add("SourceComputerId", computerEntity.id);
            networkActivityDocument.Add("SourceComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("SourceComputerCertainty", "High");
            networkActivityDocument.Add("SourceComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("DestinationIpAddress", "[daf::daf]");
            networkActivityDocument.Add("DestinationPort", 389);
            networkActivityDocument.Add("DestinationComputerId", domainControllerName.id);
            networkActivityDocument.Add("DestinationComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("DestinationComputerCertainty", "High");
            networkActivityDocument.Add("DestinationComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("TransportProtocol", "Tcp");
            networkActivityDocument.Add("AuthenticationType", "Simple");
            networkActivityDocument.Add("SourceAccountName", sourceAccount);
            networkActivityDocument.Add("SourceAccountId", userEntity.id);
            networkActivityDocument.Add("SourceAccountPasswordHash", "9apJEBIqcse0SNKVm4WzIxaHoLkUareMCDlAzhADI72CLmxOMmA8WzicPPI84xxlewEEIqZZOJJ1VqpE5VJT4g==");
            networkActivityDocument.Add("ResultCode", "Success");
            networkActivityDocument.Add("IsSuccess", BsonValue.Create(true));
            networkActivityDocument.Add("SourceGatewaySystemProfileId", sourceGateway);
            networkActivityDocument.Add("ResourceIdentifier", resourceIdentifier);


            return networkActivityDocument;
        }
        public static BsonDocument SAFillerSEAC(List<EntityObject> userEntity, List<EntityObject> computerEntity, Random rnd)
        {
           

            //Random rnd = new Random();
            BsonDocument detailRecord = new BsonDocument();
            detailRecord.Add("StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            detailRecord.Add("EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            detailRecord.Add("SourceComputerId", computerEntity[rnd.Next(0, computerEntity.Count)].id);
            detailRecord.Add("SourceAccountIds", new BsonArray(new string[20]
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
            }));
            detailRecord.Add("DestinationComputerIds", new BsonArray(new string[1]
                {
                    computerEntity[rnd.Next(0, computerEntity.Count)].id
                }));
            var a = userEntity[rnd.Next(0, userEntity.Count)].id;
            var b = userEntity[rnd.Next(0, userEntity.Count)].id;

            BsonDocument suspicousActivityDocument = new BsonDocument();
            suspicousActivityDocument.Add("_id", new ObjectId());
            suspicousActivityDocument.Add("_t",
                new BsonArray(new string[5]
                {
                    "Entity", "Alert", "SuspiciousActivity", "SuspiciousActivity`1",
                    "LdapSimpleBindCleartextPasswordSuspiciousActivity"
                }));
            suspicousActivityDocument.Add("StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            suspicousActivityDocument.Add("EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            suspicousActivityDocument.Add("IsVisible", BsonValue.Create(true));
            suspicousActivityDocument.Add("Severity", "Low");
            suspicousActivityDocument.Add("Status", "Open");
            suspicousActivityDocument.Add("TitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityTitleService");
            suspicousActivityDocument.Add("TitleDetailKeys", new BsonArray(new string[0] {}));
            suspicousActivityDocument.Add("DescriptionFormatKey",
                "LdapSimpleBindCleartextPasswordSuspiciousActivityDescriptionServiceSourceAccounts");
            suspicousActivityDocument.Add("DescriptionDetailFormatKeys", new BsonArray(new string[0] {}));
            suspicousActivityDocument.Add("SystemUpdateTime", DateTime.UtcNow);
            suspicousActivityDocument.Add("HasDetails", BsonValue.Create(true));
            suspicousActivityDocument.Add("HasInput", BsonValue.Create(false));
            suspicousActivityDocument.Add("InputTitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityInputTitle");
            suspicousActivityDocument.Add("IsInputProvided", BsonValue.Create(false));
            suspicousActivityDocument.Add("Note", BsonValue.Create(null));
            suspicousActivityDocument.Add("RelatedActivityCount", 127);
            suspicousActivityDocument.Add("RelatedUniqueEntityIds",
                new BsonArray(new string[20]
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
                }));
            suspicousActivityDocument.Add("DetailsRecords",
                new BsonArray(new BsonDocument[1] {detailRecord}));
            suspicousActivityDocument.Add("Scope", "Service");
            return suspicousActivityDocument;
        }
        public static BsonDocument SAFillerAE(List<EntityObject> userEntity, List<EntityObject> computerEntity,EntityObject domainController, string domainName)
        {
            var records = new List<BsonDocument>();
            BsonDocument detailRecord = new BsonDocument();
            
            for (int i =0; i < 100000; i++)
            {
                detailRecord.Add("DomainName", domainName);
                detailRecord.Add("Name", "ABCDEFGHIJKLMNOP" + i);
                records.Add(detailRecord);
                detailRecord = new BsonDocument();
            }
            var failedSourceAccountNames = new BsonArray(records);

            BsonDocument suspicousActivityDocument = new BsonDocument();
            suspicousActivityDocument.Add("_id", new ObjectId());
            suspicousActivityDocument.Add("_t",
                new BsonArray(new string[4]
                {
                    "Entity", "Alert", "SuspiciousActivity", "AccountEnumerationSuspiciousActivity"
                }));
            suspicousActivityDocument.Add("StartTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            suspicousActivityDocument.Add("EndTime", DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0, 0, 0)));
            suspicousActivityDocument.Add("IsVisible", BsonValue.Create(true));
            suspicousActivityDocument.Add("Severity", "Medium");
            suspicousActivityDocument.Add("Status", "Open");
            suspicousActivityDocument.Add("TitleKey", "AccountEnumerationSuspiciousActivityTitle");
            suspicousActivityDocument.Add("TitleDetailKeys", new BsonArray(new string[0] { }));
            suspicousActivityDocument.Add("DescriptionFormatKey",
                "AccountEnumerationSuspiciousActivityDescription");
            suspicousActivityDocument.Add("DescriptionDetailFormatKeys", new BsonArray(new string[0] { }));
            suspicousActivityDocument.Add("SystemUpdateTime", DateTime.UtcNow);
            suspicousActivityDocument.Add("HasDetails", BsonValue.Create(false));
            suspicousActivityDocument.Add("HasInput", BsonValue.Create(false));
            suspicousActivityDocument.Add("InputTitleKey", "AccountEnumerationSuspiciousActivityInputTitle");
            suspicousActivityDocument.Add("IsInputProvided", BsonValue.Create(false));
            suspicousActivityDocument.Add("Note", BsonValue.Create(null));
            suspicousActivityDocument.Add("RelatedActivityCount", 127);
            suspicousActivityDocument.Add("RelatedUniqueEntityIds", new BsonArray(new string[9]
            {
                computerEntity[2].id, domainController.id, userEntity[0].id, userEntity[1].id, userEntity[2].id,
                userEntity[3].id, userEntity[4].id,
                userEntity[5].id, userEntity[6].id
            }));
            suspicousActivityDocument.Add("SourceComputerId", computerEntity[2].id);
            suspicousActivityDocument.Add("DestinationComputerIds", new BsonArray(new string[1] { domainController.id }));
            suspicousActivityDocument.Add("FailedSourceAccountNames", failedSourceAccountNames);
            suspicousActivityDocument.Add("SuccessSourceAccountIds",
                new BsonArray(new string[7]
                {
                    userEntity[0].id, userEntity[1].id, userEntity[2].id, userEntity[3].id, userEntity[4].id,
                    userEntity[5].id, userEntity[6].id
                }));
            return suspicousActivityDocument;
        }
        public static BsonDocument NotificationCreator(ObjectId alertId)
        {
            BsonDocument notificationDocument = new BsonDocument();
            notificationDocument.Add("_id", new ObjectId());
            notificationDocument.Add("_t",
                new BsonArray(new string[4]
                {
                    "Entity", "Notification", "AlertNotification", "SuspiciousActivityNotification",
                }));
            notificationDocument.Add("Level", "Notice");
            notificationDocument.Add("CreationTime", DateTime.UtcNow);
            notificationDocument.Add("ExpirationTime", DateTime.UtcNow.AddDays(100));
            notificationDocument.Add("CategoryKey", "SuspiciousActivityNotificationCategory");
            notificationDocument.Add("AlertId", alertId);
            notificationDocument.Add("AlertTitleKey", "LdapSimpleBindCleartextPasswordSuspiciousActivityTitleService");
            return notificationDocument;
        }
        public static BsonDocument EventCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainControllerName, string domainName,ObjectId sourceGateway, int daysToSubtruct = 0)
        {

            BsonDocument sourceComputer = new BsonDocument();
            sourceComputer.Add("DomainName", BsonValue.Create(null));
            sourceComputer.Add("Name", computerEntity.name);

            BsonDocument destinationComputer = new BsonDocument();
            destinationComputer.Add("DomainName", BsonValue.Create(null));
            destinationComputer.Add("Name", domainControllerName.name);

            BsonDocument eventActivityDocument = new BsonDocument();

            eventActivityDocument.Add("_id", new ObjectId());
            eventActivityDocument.Add("_t", new BsonArray(new string[5] { "Entity", "Activity", "EventActivity", "WindowsEvent", "NtlmEvent" }));
            eventActivityDocument.Add("SourceGatewaySystemProfileId", sourceGateway);
            eventActivityDocument.Add("SourceComputerId", computerEntity.id);
            eventActivityDocument.Add("DestinationComputerId", domainControllerName.id);
            eventActivityDocument.Add("Time", DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, 4, 0, 0, 0)));
            eventActivityDocument.Add("SourceComputerName", sourceComputer);
            eventActivityDocument.Add("DestinationComputerName", destinationComputer);
            eventActivityDocument.Add("IsTimeMillisecondsAccurate", BsonValue.Create(true));
            eventActivityDocument.Add("CategoryName", "Security");
            eventActivityDocument.Add("ProviderName", "Microsoft-Windows-Security-Auditing");
            eventActivityDocument.Add("SourceAccountName", userEntity.name);
            eventActivityDocument.Add("SourceAccountId", userEntity.id);
            eventActivityDocument.Add("ErrorCode", "Success");
            eventActivityDocument.Add("IsSuccess", BsonValue.Create(true));

            return eventActivityDocument;
        }

        public static BsonDocument NtlmCreator(EntityObject userEntity, EntityObject computerEntity, EntityObject domainController, string domainName, ObjectId sourceGateway, string targetSPN = null, EntityObject targetMachine = null, string actionType = "As", int daysToSubtruct = 0, int hoursToSubtract = 0)
        {
            DateTime oldTime = DateTime.UtcNow.Subtract(new TimeSpan(daysToSubtruct, hoursToSubtract, 0, 0, 0));
            BsonDocument sourceAccount = new BsonDocument();
            sourceAccount.Add("DomainName", domainName);
            sourceAccount.Add("Name", userEntity.name);

            BsonDocument resourceIdentifier = new BsonDocument();
            EntityObject targetAccount = null;
            if (targetMachine != null) { targetAccount = targetMachine; }
            else { targetAccount = domainController; }
            string targetSPNName = string.Format("krbtgt/{0}", domainName);
            if (targetSPN != null) { targetSPNName = targetSPN; }
            resourceIdentifier.Add("AccountId", targetAccount.id);

            BsonDocument resourceName = new BsonDocument();
            resourceName.Add("DomainName", domainName);
            resourceName.Add("Name", targetSPNName);
            resourceIdentifier.Add("ResourceName", resourceName);

            BsonDocument networkActivityDocument = new BsonDocument();

            networkActivityDocument.Add("_id", new ObjectId());
            networkActivityDocument.Add("_t", new BsonArray(new string[4] { "Entity", "Activity", "NetworkActivity", "Ntlm" }));
            networkActivityDocument.Add("SourceGatewaySystemProfileId", sourceGateway);
            networkActivityDocument.Add("SourceComputerId", computerEntity.id);
            networkActivityDocument.Add("DestinationComputerId", targetAccount.id);
            networkActivityDocument.Add("HorizontalParentId", new ObjectId());
            networkActivityDocument.Add("StartTime", oldTime);
            networkActivityDocument.Add("EndTime", oldTime);
            networkActivityDocument.Add("ProcessingTime", oldTime);
            networkActivityDocument.Add("SourceIpAddress", "[daf::daf]");
            networkActivityDocument.Add("SourcePort", 51510);
            networkActivityDocument.Add("SourceComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("SourceComputerCertainty", "High");
            networkActivityDocument.Add("SourceComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("DestinationIpAddress", "[daf::daf]");
            networkActivityDocument.Add("DestinationPort", 88);
            networkActivityDocument.Add("DestinationComputerSiteId", BsonValue.Create(null));
            networkActivityDocument.Add("DestinationComputerCertainty", "High");
            networkActivityDocument.Add("DestinationComputerResolutionMethod", new BsonArray(new string[1] { "RpcNtlm" }));
            networkActivityDocument.Add("TransportProtocol", "Tcp");
            networkActivityDocument.Add("Version", 2);
            networkActivityDocument.Add("SourceComputerNetbiosName", computerEntity.name);
            networkActivityDocument.Add("SourceAccountName", sourceAccount);
            networkActivityDocument.Add("SourceAccountId", userEntity.id);
            networkActivityDocument.Add("ResourceIdentifier", resourceIdentifier);
            networkActivityDocument.Add("DceRpcStatus", BsonValue.Create(null));
            networkActivityDocument.Add("LdapResultCode", BsonValue.Create(null));
            networkActivityDocument.Add("SmbStatus", "Success");
            networkActivityDocument.Add("Smb1Status", BsonValue.Create(null));
            networkActivityDocument.Add("IsSuccess", BsonValue.Create(true));
            
            return networkActivityDocument;
        }
    }
}
