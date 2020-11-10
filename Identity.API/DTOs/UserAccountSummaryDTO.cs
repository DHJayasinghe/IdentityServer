namespace Identity.API
{
    public sealed class UserAccountSummaryDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public string LockoutEndDateTimeUtc { get; set; }
        public string DateTimeRegistered { get; set; }
    }
}
