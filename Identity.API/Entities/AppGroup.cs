using CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Entities
{
    public partial class AppGroup
    {
        public int Id { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AppGroupPermission> GroupPermissions { get; set; }
        public ICollection<AppUserGroup> UserGroups { get; set; }

        /// <summary>
        /// Determine whether group name is similar to specified name
        /// </summary>
        public bool IsNameSimilarTo(Maybe<string> name) =>
            name.HasValue && name.Value.ToUpper() == Name.ToUpper();

        protected AppGroup()
        {
            GroupPermissions = new List<AppGroupPermission>();
            UserGroups = new HashSet<AppUserGroup>();
        }

        public AppGroup(Maybe<string> name, Maybe<string> description)
            : this()
        {
            if (name.HasNoValue)
                throw new ArgumentException("Group name is not specified");
            if (description.HasNoValue)
                throw new ArgumentException("Group description is not specified");

            Name = name.Value;
            Description = description.Value;
        }

        public void AddPermissions(List<AppPermission> permissions)
        {
            if (permissions == null || !permissions.Any())
                throw new ArgumentException("No permissions are provided");

            permissions.ForEach(permission =>
                GroupPermissions.ToList().Add(new AppGroupPermission(permission)));
        }

        public void Update(Maybe<string> name, Maybe<string> description)
        {
            if (name.HasNoValue)
                throw new ArgumentException("Group name is not specified");
            if (description.HasNoValue)
                throw new ArgumentException("Group description is not specified");

            Name = name.Value;
            Description = description.Value;
        }

        public void UpdatePermissions(List<AppPermission> permissions)
        {
            // mark deleted revoked permissions
            GroupPermissions.ForEach(permission =>
            {
                if (!permissions.Any(d => d.Id == permission.PermissionId))
                    permission.Delete();
            });


            // add new permissions
            permissions.ForEach(permission =>
            {
                if (!GroupPermissions.Any(d => d.PermissionId == permission.Id))
                    GroupPermissions.Add(new AppGroupPermission(permission));
            });
        }
    }
}
