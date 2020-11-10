using CommonUtil;
using System;
using System.Collections.Generic;

namespace Identity.API.Entities
{
    public partial class AppPermission
    {
        public int Id { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<AppGroupPermission> GroupPermissions { get; protected set; }

        protected AppPermission()
        {
            GroupPermissions = new HashSet<AppGroupPermission>();
        }

        public AppPermission(Maybe<string> name, Maybe<string> description)
            : this()
        {
            if (name.HasNoValue)
                throw new ArgumentException("Permission name is not specified");
            if (description.HasNoValue)
                throw new ArgumentException("Permission description is not specified");

            Name = name.Value;
            Description = description.Value;
        }
    }
}
