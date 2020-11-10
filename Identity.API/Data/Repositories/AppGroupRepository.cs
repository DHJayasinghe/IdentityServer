using CommonUtil;
using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Data.Repositories
{
    public sealed class AppGroupRepository
        : Repository<AppGroup>, IAppGroupRepository
    {
        public AppGroupRepository(UnitOfWork unitOfWork)
         : base(unitOfWork) { }

        public IReadOnlyCollection<AppGroup> GetList() =>
            _unitOfWork.Query<AppGroup>()
                .Include(e => e.GroupPermissions)
                .Include(e => e.UserGroups)
                .ToList().AsReadOnly();

        public bool HasGroup(Maybe<string> nameOrNothing) => nameOrNothing.HasValue
            && _unitOfWork.Query<AppGroup>().Any(e => e.Name.ToUpper() == nameOrNothing.Value.ToUpper());

        public Maybe<AppGroup> FindByName(Maybe<string> nameOrNothing) => nameOrNothing.HasValue
            ? _unitOfWork.Query<AppGroup>()
                .Where(e => e.Name.ToUpper() == nameOrNothing.Value.ToUpper())
                .Include(e => e.GroupPermissions)
                .ToList()
                .AsReadOnly()
                .FirstOrDefault()
            : null;

        public Maybe<AppGroup> GetById(int id) => id > 0
            ? _unitOfWork.Query<AppGroup>()
                .Where(e => e.Id == id)
                .Include(e => e.GroupPermissions)
                    .ThenInclude(e => e.Permission)
                .Include(e => e.UserGroups)
                    .ThenInclude(e => e.AppUser)
                .ToList()
                .AsReadOnly()
                .FirstOrDefault()
            : null;

        public IReadOnlyCollection<AppGroup> GetList(IEnumerable<int> idArr) =>
            _unitOfWork.Query<AppGroup>()
                .Where(e => idArr.Contains(e.Id))
                .Include(e => e.GroupPermissions)
                    .ThenInclude(e => e.Permission)
            .ToList()
            .AsReadOnly();

        public void Delete(int id)
        {
            Maybe<AppGroup> group = GetById(id);
            // remove aggregates
            if (group.Value.GroupPermissions != null && group.Value.GroupPermissions.Any())
                _unitOfWork.Delete(group.Value.GroupPermissions);
            if (group.Value.UserGroups != null && group.Value.UserGroups.Any())
                _unitOfWork.Delete(group.Value.UserGroups);
            base.Delete(id);
        }

        public new void Update(AppGroup entity)
        {
            entity.GroupPermissions.ToList().ForEach(permission =>
            {
                if (permission.Deleted)
                    _unitOfWork._dbContext.Entry(permission).State = EntityState.Deleted;
                else if (permission.Inserted)
                    _unitOfWork._dbContext.Entry(permission).State = EntityState.Added;
            });
            base.Update(entity);
        }
    }
}
