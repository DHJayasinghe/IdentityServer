namespace Identity.API
{
    public sealed class PermissionDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public bool Active { get; set; }
    }
}
