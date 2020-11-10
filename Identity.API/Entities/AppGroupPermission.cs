using CommonUtil;
using System;

namespace Identity.API.Entities
{
    public partial class AppGroupPermission
    {
        public Guid Id { get; protected set; }
        public int PermissionId { get; protected set; }
        public int AppGroupId { get; protected set; }
        public AppPermission Permission { get; protected set; }
        public AppGroup AppGroup { get; protected set; }

        public bool Deleted { get; private set; } = false;

        public bool Inserted { get; private set; } = false;

        protected AppGroupPermission()
        {
        }

        public AppGroupPermission(Maybe<AppPermission> permission)
        {
            if (permission.HasNoValue)
                throw new ArgumentException("Permission is not specified");

            PermissionId = permission.Value.Id;
            Inserted = true;
        }

        public AppGroupPermission(Maybe<AppPermission> permission, Maybe<AppGroup> appGroup)
            : this(permission)
        {
            if (appGroup.HasNoValue)
                throw new ArgumentException("Group is not specified");

            AppGroupId = appGroup.Value.Id;
        }

        public void Delete() => Deleted = true;
    }
}
