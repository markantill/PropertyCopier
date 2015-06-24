using System;

namespace PropertyCopier
{
    /// <summary>
    /// Helper class for copying properties.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public class CopyFrom<TSource>
        where TSource : class
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFrom&lt;TSource&gt;"/> class.
        /// </summary>
        /// <param name="source">The input.</param>
        /// <param name="scalarOnly">if set to <c>true</c> copy scalar properties only.</param>
        internal CopyFrom(TSource source, bool scalarOnly)
        {
            _scalarOnly = scalarOnly;
            _source = source;
        }

        #endregion

        #region Fields

        private readonly bool _scalarOnly;
        private readonly TSource _source;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        public TTarget ToNew<TTarget>()
            where TTarget : class, new()
        {
            var result = _scalarOnly
                ? ScalarPropertyCopier<TSource, TTarget>.CopyFrom(_source)
                : PropertyCopier<TSource, TTarget>.CopyFrom(_source);
            return result;
        }

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        public TTarget ToNew<TTarget>(Action<TTarget> afterCreationDo)
            where TTarget : class, new()
        {
            var result = ToNew<TTarget>();
            afterCreationDo(result);
            return result;
        }

        /// <summary>
        /// To the existing.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="target">The target.</param>
        /// <returns>Existing object with properties copied in.</returns>
        public TTarget ToExisting<TTarget>(TTarget target)
            where TTarget : class, new()
        {
            var result = ExistingCopier<TSource, TTarget>.CopyFrom(_source, target);
            return result;
        }

        #endregion
    }
}