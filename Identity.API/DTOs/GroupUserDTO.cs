using System;

namespace Identity.API
{
    public sealed class GroupUserDTO
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
    }
}
