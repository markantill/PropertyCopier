using System.Collections.Generic;
using System.Reflection;

namespace PropertyCopier
{
    /// <summary>
    /// Compare two <see cref="PropertyInfo"/> objects to see if they are the
    /// same property type and name.
    /// </summary>
    internal class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return x.Name == y.Name &&
                   x.PropertyType == y.PropertyType;
        }

        public int GetHashCode(PropertyInfo obj)
        {
            unchecked
            {
                var hashCode = obj?.Name?.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj?.PropertyType.GetHashCode());
                return hashCode ?? 0;
            }
        }
    }
}