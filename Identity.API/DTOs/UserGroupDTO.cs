using System.Collections.Generic;

namespace Identity.API
{
    public sealed class UserGroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UsersCount { get; set; }
        public int PermissionsCount { get; set; }
        public IEnumerable<GroupPermissionDTO> Permissions { get; set; }
        public IEnumerable<GroupUserDTO> AppUsers { get; set; }
    }
}
