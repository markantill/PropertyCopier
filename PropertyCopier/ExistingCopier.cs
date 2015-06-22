using System;

namespace PropertyCopier
{
    public static class ExistingCopier<TSource, TTarget>
        where TSource : class
        where TTarget : class, new()
    {
        // Stores the delegate required to create a new object.
        // As this is compiled it is much faster than reflection.

        #region Static Fields

        private static readonly Lazy<Func<TSource, TTarget, TTarget>> Copier =
            new Lazy<Func<TSource, TTarget, TTarget>>(
                () => ExpressionBuilder.CreateLambdaPropertyCopier<TSource, TTarget>().Compile(),
                true);

        #endregion

        #region Methods

        /// <summary>
        /// Copies from the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        /// Creates a new instance of specified target type, copying property values from source.
        /// </returns>
        internal static TTarget CopyFrom(TSource source, TTarget target)
        {
            var result = Copier.Value(source, target);
            return result;
        }

        #endregion
    }
}