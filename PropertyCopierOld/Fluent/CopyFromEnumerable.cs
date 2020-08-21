using System;
using System.Collections.Generic;
using System.Linq;

namespace PropertyCopier.Fluent
{
    /// <summary>
    /// Copy generic enumeration contents.
    /// </summary>
    /// <typeparam name="TSource">Generic type of enumeration to copy.</typeparam>
    public class CopyFromEnumerable<TSource>        
    {
        private readonly IEnumerable<TSource> _enumeration;        

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromEnumerable{TSource}" /> class.
        /// </summary>
        /// <param name="enumeration">The enumeration.</param>
        internal CopyFromEnumerable(IEnumerable<TSource> enumeration)
        {
            _enumeration = enumeration;
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="copier">The copier to use.</param>
        /// <returns>Enumeration of copied objects.</returns>
        public IEnumerable<TTarget> To<TTarget>(Copier copier)
            where TTarget : class, new()
        {
            var result = _enumeration.Select(copier.Copy<TSource, TTarget>);
            return result;
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>Enumeration of copied objects.</returns>
        public IEnumerable<TTarget> To<TTarget>()
            where TTarget : class, new()
        {
            return To<TTarget>(new Copier());
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>Enumeration of copied objects.</returns>
        [Obsolete("Use To", false)]
        public IEnumerable<TTarget> ToNew<TTarget>()
            where TTarget : class, new()
        {
            return To<TTarget>();
        }
    }
}