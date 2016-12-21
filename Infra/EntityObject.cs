namespace Abnormal_UI.Infra
{
    public class EntityObject
    {
        public string Name { get; set; }
        public UniqueEntityType Type { get; set; }
        public string Id { get; set; }
        public string SamName { get; set; }

        public EntityObject(string entityName, string id, string samName=null, UniqueEntityType entityType = UniqueEntityType.User)
        {
            Name = entityName;
            Type = entityType;
            Id = id;
            SamName = samName;
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
