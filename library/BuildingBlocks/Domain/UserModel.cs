using System.Text.Json.Serialization;

namespace BookInventory.BuildingBlocks.Domain
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class UserActionModel
    {
        public string CreatedByName { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid CreatedById { get; set; }
    }
}
