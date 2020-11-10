using CommonUtil;
using System;

namespace Identity.API.Entities
{
    public partial class AppUserGroup
    {
        public Guid Id { get; protected set; }
        public int AppUserId { get; protected set; }
        public int AppGroupId { get; protected set; }
        public AppUser AppUser { get; protected set; }
        public AppGroup AppGroup { get; protected set; }

        public bool Deleted { get; private set; } = false;

        public bool Inserted { get; private set; } = false;

        protected AppUserGroup() { }

        public AppUserGroup(Maybe<AppGroup> usergroup)
        {
            if (usergroup.HasNoValue)
                throw new ArgumentException("User group is not specified");

            AppGroupId = usergroup.Value.Id;
            Inserted = true;
        }

        public AppUserGroup(Maybe<AppUser> appUser, Maybe<AppGroup> appGroup)
            : this(appGroup)
        {
            if (appUser.HasNoValue)
                throw new ArgumentException("User account is not specified");

            AppUserId = appUser.Value.Id;
        }

        public void Delete() => Deleted = true;
    }
}
