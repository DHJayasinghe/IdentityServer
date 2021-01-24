using CommonUtil.Specification;
using Identity.API.Entities;
using System;
using System.Linq.Expressions;

namespace Identity.API.Logic.Specifications
{
    public sealed class UserAccountWithUsernameSpec : Specification<AppUser>
    {
        private readonly string _email;

        public UserAccountWithUsernameSpec(string email) =>
            _email = email.ToLower();

        public override Expression<Func<AppUser, bool>> ToExpression() =>
            account => account.Username == _email;
    }
}
