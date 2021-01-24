using CommonUtil.Specification;
using Identity.API.Entities;
using System;
using System.Linq.Expressions;

namespace Identity.API.Logic.Specifications
{
    public sealed class UserAccountWithNameLikeSpec : Specification<AppUser>
    {
        private readonly string _name;

        public UserAccountWithNameLikeSpec(string name) =>
            _name = name.ToLower();

        public override Expression<Func<AppUser, bool>> ToExpression() => account =>
            account.FirstName.ToLower() == _name ||
            account.LastName.ToLower() == _name ||
            account.FirstName.ToLower().Contains(_name) ||
            account.LastName.ToLower().Contains(_name);
    }
}
