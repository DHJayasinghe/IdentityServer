using System;

namespace Identity.API
{
    public sealed class GroupPermissionDTO
    {
        public Guid Id { get; set; }
        public int PermissionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
