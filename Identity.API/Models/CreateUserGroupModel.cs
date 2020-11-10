namespace Identity.API.Models
{
    public sealed class CreateUserGroupModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SelectedPermissions { get; set; }
    }
}
