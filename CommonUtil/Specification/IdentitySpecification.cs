using System;
using System.Linq.Expressions;

namespace CommonUtil.Specification
{
    internal sealed class IdentitySpecification<T> : Specification<T>
    {
        private readonly bool _all;

        public IdentitySpecification(bool all = true) => _all = all;

        public override Expression<Func<T, bool>> ToExpression() => x => _all;
    }
}
