using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyCopier.Fluent
{
    /// <summary>
    /// Copy generic enumeration contents.
    /// </summary>
    /// <typeparam name="TSource">Generic type of enumeration to copy.</typeparam>
    public class CopyFromQueryable<TSource>        
    {
        private readonly IQueryable<TSource> _queryable;        

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromEnumerable{TSource}" /> class.
        /// </summary>
        /// <param name="queryable">The enumeration.</param>
        internal CopyFromQueryable(IQueryable<TSource> queryable)
        {
            _queryable = queryable;
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="copier">The copier to use.</param>
        /// <returns>Enumeration of copied objects.</returns>
        public IQueryable<TTarget> To<TTarget>(Copier copier)
            where TTarget : class, new()
        {
            var result = _queryable.Select(copier.CopyExpression<TSource, TTarget>());
            return result;
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>Enumeration of copied objects.</returns>
        public IQueryable<TTarget> To<TTarget>()
            where TTarget : class, new()
        {
            return To<TTarget>(new Copier());
        }
    }
}