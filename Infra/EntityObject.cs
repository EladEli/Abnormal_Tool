namespace Abnormal_UI.Infra
{
    public class EntityObject
    {
        public string Name { get; set; }
        public UniqueEntityType Type { get; set; }
        public string Id { get; set; }
        public string Domain { get; set; }

        public EntityObject(string entityName, string id,string domain,  UniqueEntityType entityType = UniqueEntityType.User)
        {
            Name = entityName;
            Type = entityType;
            Id = id;
            Domain = domain;
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
