using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonUtil.ValueTypes
{
    public abstract class ValueObject
    {
        // provide the list of components that comprise the class
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            var valueObject = (ValueObject)obj;

            // comparison is done by using SequenceEqual() on the two sets of components
            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        // takes each component and uses it to build up the resulting hash code
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        public static bool operator ==(ValueObject a, ValueObject b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject a, ValueObject b)
        {
            return !(a == b);
        }
    }
}
