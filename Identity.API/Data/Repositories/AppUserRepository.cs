using CommonUtil;
using CommonUtil.Helpers;
using CommonUtil.Specification;
using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Identity.API.Data.Repositories
{
    public sealed class AppUserRepository
        : Repository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(UnitOfWork unitOfWork)
          : base(unitOfWork) { }

        public Maybe<AppUser> GetById(int id) => id > 0
            ? _unitOfWork.Query<AppUser>()
                .Where(e => e.Id == id)
                .Include(e => e.RefreshTokens)
                .Include(e => e.UserGroups)
                .ToList()
                .AsReadOnly()
                .FirstOrDefault()
            : null;

        public Maybe<AppUser> FindByUsername(Maybe<string> usernameOrNothing) => usernameOrNothing.HasValue
            ? _unitOfWork.Query<AppUser>()
                .Where(e => e.Username == usernameOrNothing.Value)
                .Include(e => e.RefreshTokens)
                .Include(e => e.UserGroups)
                .ToList()
                .AsReadOnly()
                .FirstOrDefault()
            : null;

        public bool HasUsername(Maybe<string> usernameOrNothing) =>
            usernameOrNothing.HasValue
            && _unitOfWork.Query<AppUser>()
            .Any(e => e.Username.ToUpper() == usernameOrNothing.Value.ToUpper());

        public new void Update(AppUser entity)
        {
            entity.UserGroups.ToList().ForEach(permission =>
            {
                if (permission.Deleted)
                    _unitOfWork._dbContext.Entry(permission).State = EntityState.Deleted;
                else if (permission.Inserted)
                    _unitOfWork._dbContext.Entry(permission).State = EntityState.Added;
            });
            base.Update(entity);
        }

        public IReadOnlyCollection<AppUser> GetList(
            Specification<AppUser> spec,
            out int filteredResultsCount,
            out int totalResultsCount,
            int take = 10,
            int skip = 0,
            AppUserSortableColumn sortBy = AppUserSortableColumn.Id,
            bool sortDir = true)
        {
            totalResultsCount = _unitOfWork.Query<AppUser>().Count(); // total entries in table
            IQueryable<AppUser> filteredResult = _unitOfWork.Query<AppUser>()
                    .Where(spec.ToExpression())
                    .AsQueryable();

            filteredResultsCount = filteredResult.Count(); // available result count for query

            return filteredResult
                .OrderBy(sortBy.ToString(), sortDir)
                .Skip(skip)
                .Take(take)
                .ToList()
                .AsReadOnly();
        }
    }

    public enum AppUserSortableColumn
    {
        Id,
        FirstName,
        LastName,
        Username
    }
}
