using System;
using System.Linq.Expressions;
using PropertyCopier.Fluent;

namespace PropertyCopier
{
    public interface ICopier
    {
        /// <summary>
        /// Copy from an object of a specific type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        CopyFrom<TSource> From<TSource>(TSource source)
            where TSource : class;

        /// <summary>
        /// The generated <see cref="Expression"/> that will create a new instance
        /// of TTarget from TSource.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <returns>The <see cref="Copier.CopyExpression{TSource,TTarget}"/></returns>
        Expression<Func<TSource, TTarget>> CopyExpression<TSource, TTarget>()            
            where TTarget : new();

        /// <summary>
        /// Set the rules for the copier to use.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>        
        /// <param name="comparer">The rules to use to compare names, the default is InvariantCultureIgnoreCase.</param>
        SetRules<TSource, TTarget> SetRules<TSource, TTarget>(            
            bool flattenChildObjects = true,
            bool copyChildObjects = true,
            bool copyChildEnumerations = true,
            bool copyChildCollections = true,
            bool addNullChecking = false,
            StringComparer comparer = null)
            where TTarget : new();
    }
}