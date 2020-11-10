using CommonUtil;
using Identity.API.Entities;
using System.Collections.Generic;

namespace Identity.API.Data.Repositories
{
    public interface IAppPermissionRepository : IRepository<AppPermission>
    {
        IReadOnlyCollection<AppPermission> GetList();

        bool HasPermission(Maybe<string> permissionOrNothing);

        Maybe<AppPermission> FindByName(Maybe<string> nameOrNothing);
    }
}
