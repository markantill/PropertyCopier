using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PropertyCopier
{
    /// <summary>
    /// Helper class for property copier to enable fluent interface.
    /// </summary>
    public static class Copy
    {
        #region Public Methods and Operators

        /// <summary>
        /// Copies from the specified input.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The input.</param>
        /// <returns>The copied object.</returns>
        public static CopyFrom<TSource> PropertiesFrom<TSource>(TSource source)
            where TSource : class
        {
            var result = new CopyFrom<TSource>(source, false);
            return result;
        }

        /// <summary>
        /// Copies the scalar properties from the source
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The copied object.</returns>
        public static CopyFrom<TSource> ScalarPropertiesFrom<TSource>(TSource source)
            where TSource : class
        {
            var result = new CopyFrom<TSource>(source, true);
            return result;
        }

        /// <summary>
        /// Copies scalar properties of each item in the enumeration.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="enumeration">The enumeration.</param>
        /// <returns>Enumeration of copied items.</returns>
        public static CopyFromEnumeration<TSource> EnumerationFrom<TSource>(IEnumerable<TSource> enumeration)
            where TSource : class
        {
            var result = new CopyFromEnumeration<TSource>(enumeration);
            return result;
        }

        /// <summary>
        /// Creates expression to project types.
        /// </summary>
        /// <typeparam name="TSource">The type going in.</typeparam>
        /// <typeparam name="TTarget">The type coming out.</typeparam>
        /// <returns>Expression to transform type.</returns>
        public static Expression<Func<TSource, TTarget>> Expression<TSource, TTarget>() 
            where TTarget : class, new() 
            where TSource : class
        {
            var result = PropertyCopier<TSource, TTarget>.Expression.Value;
            return result;
        }

        #endregion
    }
}