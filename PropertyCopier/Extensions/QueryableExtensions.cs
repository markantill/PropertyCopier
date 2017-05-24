using System;
using System.Linq;
using System.Linq.Expressions;
using PropertyCopier.Fluent;

namespace PropertyCopier.Extensions
{
    /// <summary>
    /// Helper methods to copying contents of <see cref="IQueryable{T}"/>
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Copy each element in the queryable. Generates an <see cref="Expression"/> so the result
        /// is also an <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the queryable.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <returns>Copier that can perform the copy.</returns>
        public static CopyFromQueryable<TSource> Copy<TSource>(this IQueryable<TSource> queryable)
        {
            return new CopyFromQueryable<TSource>(queryable);
        }
    }
}