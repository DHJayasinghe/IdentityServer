﻿using System;
using System.Linq.Expressions;

namespace CommonUtil.Specification
{
    public abstract class Specification<T>
    {
        public static readonly Specification<T> All = new IdentitySpecification<T>();
        public static readonly Specification<T> None = new IdentitySpecification<T>(false);

        public bool IsSatisfiedBy(T entity)
        {
            Func<T, bool> predicate = ToExpression().Compile();
            return predicate(entity);
        }

        public abstract Expression<Func<T, bool>> ToExpression();

        public Specification<T> And(Specification<T> specification)
        {
            if (this == All)  
                return specification;
            if (specification == All) 
                return this;

            return new AndSpecification<T>(this, specification);
        }


        public Specification<T> Or(Specification<T> specification)
        {
            if (this == All || specification == All)
                return All;

            return new OrSpecification<T>(this, specification);
        }


        public Specification<T> Not() => new NotSpecification<T>(this);
    }
}
