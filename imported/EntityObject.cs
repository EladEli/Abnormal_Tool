namespace Abnormal_UI
{
    public class EntityObject
    {
        public string name { get; set; }
        public UniqueEntityType type { get; set; }

        public string id { get; set; }

        public string DnsName { get; set; }

        public EntityObject(string entityName, string Id, UniqueEntityType entityType = UniqueEntityType.User)
        {
            name = entityName;
            type = entityType;
            id = Id;
        }

        public EntityObject(string entityName, string Id, string dnsName, UniqueEntityType entityType = UniqueEntityType.User)
        {
            name = entityName;
            type = entityType;
            id = Id;
            DnsName = dnsName;
        }
    }

    public enum UniqueEntityType
    {
        Group,
        User,
        Computer,
        Domain,
    }
}
