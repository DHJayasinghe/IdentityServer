using CommonUtil;
using CommonUtil.Specification;
using Identity.API.Entities;
using System.Collections.Generic;

namespace Identity.API.Data.Repositories
{
    public interface IAppUserRepository : IRepository<AppUser>
    {
        Maybe<AppUser> GetById(int id);

        Maybe<AppUser> FindByUsername(Maybe<string> usernameOrNothing);

        bool HasUsername(Maybe<string> usernameOrNothing);

        new void Update(AppUser entity);

        IReadOnlyCollection<AppUser> GetList(Specification<AppUser> spec,
            out int filteredResultsCount,
            out int totalResultsCount,
            int take = 10,
            int skip = 0,
            AppUserSortableColumn sortBy = AppUserSortableColumn.Id,
            bool sortDir = true);
    }
}
