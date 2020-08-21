using System;
using PropertyCopier.Data;

namespace PropertyCopier.StaticCaches
{
    /// <summary>
    /// For copying into an existing object.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    internal static class ExistingCopier<TSource, TTarget>        
        where TTarget : new()
    {
        // Stores the delegate required to create a new object.
        // As this is compiled it is much faster than reflection.        
        internal static readonly Lazy<Func<TSource, TTarget, TTarget>> Copier =
            new Lazy<Func<TSource, TTarget, TTarget>>(
                () => ExpressionBuilder.CreateLambdaPropertyCopier<TSource, TTarget>(new MappingData<TSource, TTarget>()).Compile());        

        /// <summary>
        /// Copies from the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Creates a new instance of specified target type, copying property values from source.
        /// </returns>
        internal static TTarget From(TSource source, TTarget target)
        {
            var result = Copier.Value(source, target);
            return result;
        }

        /// <summary>
        /// Copies from the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Creates a new instance of specified target type, copying property values from source.
        /// </returns>
        [Obsolete("Use From instead")]
        internal static TTarget CopyFrom(TSource source, TTarget target)
        {            
            return From(source, target);
        }
    }
}