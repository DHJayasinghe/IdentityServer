using CommonUtil;
using Identity.API.Entities;
using System.Collections.Generic;

namespace Identity.API.Data.Repositories
{
    public interface IAppGroupRepository : IRepository<AppGroup>
    {
        IReadOnlyCollection<AppGroup> GetList();

        IReadOnlyCollection<AppGroup> GetList(IEnumerable<int> idArr);

        bool HasGroup(Maybe<string> nameOrNothing);

        Maybe<AppGroup> FindByName(Maybe<string> nameOrNothing);

        Maybe<AppGroup> GetById(int id);

        void Delete(int id);

        new void Update(AppGroup entity);
    }
}
