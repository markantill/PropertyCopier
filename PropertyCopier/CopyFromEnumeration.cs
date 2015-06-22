using System.Collections.Generic;
using System.Linq;

namespace PropertyCopier
{
    /// <summary>
    /// Copy generic enumeration contents.
    /// </summary>
    /// <typeparam name="TSource">Generic type of enumeration to copy.</typeparam>
    public class CopyFromEnumeration<TSource>
        where TSource : class
    {
        private readonly IEnumerable<TSource> _enumeration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromEnumeration{TSource}"/> class.
        /// </summary>
        /// <param name="enumeration">The enumeration.</param>
        internal CopyFromEnumeration(IEnumerable<TSource> enumeration)
        {
            _enumeration = enumeration;
        }

        /// <summary>
        /// Create new enumeration copied objects.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>Enumeration of copied objects.</returns>
        public IEnumerable<TTarget> ToNew<TTarget>()
            where TTarget : class, new()
        {
            var result = _enumeration.Select(s => Copy.ScalarPropertiesFrom(s).ToNew<TTarget>());
            return result;
        }
    }
}