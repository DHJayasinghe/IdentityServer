using CommonUtil;
using Identity.API.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Data.Repositories
{
    public sealed class AppPermissionRepository
        : Repository<AppPermission>, IAppPermissionRepository
    {
        public AppPermissionRepository(UnitOfWork unitOfWork)
          : base(unitOfWork) { }

        public IReadOnlyCollection<AppPermission> GetList() =>
            _unitOfWork.Query<AppPermission>()
                .ToList()
                .AsReadOnly();

        public bool HasPermission(Maybe<string> nameOrNothing) => nameOrNothing.HasValue
            && _unitOfWork.Query<AppPermission>()
                .Any(e => e.Name.ToUpper() == nameOrNothing.Value.ToUpper());

        public Maybe<AppPermission> FindByName(Maybe<string> nameOrNothing) => nameOrNothing.HasValue
            ? _unitOfWork.Query<AppPermission>()
                .Where(e => e.Name.ToUpper() == nameOrNothing.Value.ToUpper())
                .ToList()
                .AsReadOnly()
                .FirstOrDefault()
            : null;
    }
}
