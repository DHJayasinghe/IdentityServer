using CommonUtil.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Entities
{
    public partial class AppUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEndDateTimeUtc { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? CreatedDateTimeUtc { get; set; }
        public DateTime? ModifiedDateTimeUtc { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public List<AppUserGroup> UserGroups { get; set; }

        public string Fullname => FirstName + " " + LastName;

        public bool IsAccountLocked() => LockoutEndDateTimeUtc != null && LockoutEndDateTimeUtc > DateTime.UtcNow;

        public bool IsRelatedToUser(int id) => Id == id;

        public string LockoutEndTime()
        {
            if (LockoutEndDateTimeUtc == null) return "0 minutes";

            TimeSpan remainder = ((DateTime)LockoutEndDateTimeUtc).Subtract(DateTime.UtcNow);
            return $"{remainder.Days} days {remainder.Minutes} minutes";
        }

        protected AppUser()
        {
            RefreshTokens = new HashSet<RefreshToken>();
            UserGroups = new List<AppUserGroup>();
        }

        public AppUser(string firstName, string lastName, Email username, Password password)
            : this()
        {
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            PasswordHash = password;
            AccessFailedCount = 0;
            LockoutEnabled = true;
            CreatedDateTimeUtc = DateTime.UtcNow;
        }

        public void UpdateUserGroups(List<AppGroup> groups)
        {
            // mark deleted revoked user groups
            UserGroups.ForEach(usergroup =>
            {
                if (!groups.Any(d => d.Id == usergroup.AppGroupId))
                    usergroup.Delete();
            });


            // add new user group
            groups.ForEach(usegroup =>
            {
                if (!UserGroups.Any(d => d.AppGroupId == usegroup.Id))
                    UserGroups.Add(new AppUserGroup(usegroup));
            });
        }

        public void ChangePassowrd(Password password)
        {
            PasswordHash = password;
            ModifiedDateTimeUtc = DateTime.UtcNow;
        }

        public void RecordFailedLoginAttempt()
        {
            AccessFailedCount += 1;
            if (AccessFailedCount == 3)
                LockoutEndDateTimeUtc = DateTime.UtcNow.AddMinutes(10); // lock account for 10 mins
        }

        public void ResetAccountLockout()
        {
            AccessFailedCount = 0;
            LockoutEndDateTimeUtc = null;
        }

        public void BlockAccount()
        {
            LockoutEndDateTimeUtc = DateTime.UtcNow.AddYears(100);
            ModifiedDateTimeUtc = DateTime.UtcNow;
        }

        public void UnblockAccount()
        {
            AccessFailedCount = 0;
            LockoutEndDateTimeUtc = null;
            ModifiedDateTimeUtc = DateTime.UtcNow;
        }
    }
}
