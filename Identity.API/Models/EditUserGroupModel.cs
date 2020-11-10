namespace Identity.API.Models
{
    public sealed class EditUserGroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SelectedPermissions { get; set; }
    }
}
