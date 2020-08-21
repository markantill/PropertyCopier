using System;
using System.Collections.Generic;
using PropertyCopier.Fluent;

namespace PropertyCopier.Extensions
{
    /// <summary>
    /// Helper methods to copying contents of <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Copy each element in to enumeration.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the enumeration.</typeparam>
        /// <param name="enumerable">The enumerations.</param>
        /// <returns>Copier that can perform the copy.</returns>
        public static CopyFromEnumerable<TSource> Copy<TSource>(this IEnumerable<TSource> enumerable)
        {
            return new CopyFromEnumerable<TSource>(enumerable);
        }
    }
}