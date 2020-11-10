using System.Collections.Generic;

namespace Identity.API
{
    public sealed class UserAccountDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public int AccessFailedCount { get; set; }
        public bool AccountLocked { get; set; }
        public bool LockoutEnabled { get; set; }
        public string LockoutEndDateTimeUtc { get; set; }
        public string DateTimeRegistered { get; set; }
        public string DateTimeLastModified { get; set; }
        public string LockoutEndTime { get; set; }
        public IEnumerable<UserGroupDTO> AssignedUserGroups { get; set; }
    }
}
