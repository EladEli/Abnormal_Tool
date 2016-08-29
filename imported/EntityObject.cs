namespace Abnormal_UI.Imported
{
    public class EntityObject
    {
        public string name { get; set; }
        public UniqueEntityType type { get; set; }
        public string id { get; set; }

        public EntityObject(string entityName, string Id, UniqueEntityType entityType = UniqueEntityType.User)
        {
            name = entityName;
            type = entityType;
            id = Id;
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
