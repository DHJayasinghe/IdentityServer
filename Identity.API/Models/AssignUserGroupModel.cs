namespace Identity.API.Models
{
    public sealed class AssignUserGroupModel
    {
        public int Id { get; set; }
        public string[] SelectedUserGroups{ get; set; }
    }
}
