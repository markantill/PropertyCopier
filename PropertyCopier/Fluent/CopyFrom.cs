using System;

namespace PropertyCopier.Fluent
{
    /// <summary>
    /// Helper class for copying properties.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public class CopyFrom<TSource>        
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="From{TSource}"/> class.
        /// </summary>
        /// <param name="source">The input.</param>
        /// <param name="copier">The copier.</param>
        /// <param name="scalarOnly"></param>
        internal CopyFrom(TSource source, Copier copier, bool? scalarOnly = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));            
            _source = source;
            _copier = copier;
            _scalarOnly = scalarOnly;
        }        
        
        private readonly TSource _source;
        private readonly Copier _copier;
        private readonly bool? _scalarOnly;

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        public TTarget To<TTarget>()
            where TTarget : new()
        {
            if (_scalarOnly == true)
            {
                _copier.SetRules<TSource, TTarget>(true);
            }

            var result = _copier.Copy<TSource, TTarget>(_source);
            return result;
        }

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        public TTarget To<TTarget>(Action<TTarget> afterCreationDo)
            where TTarget : class, new()
        {
            var result = To<TTarget>();
            afterCreationDo(result);
            return result;
        }

        /// <summary>
        /// Copy to an existing item.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="target">The target.</param>
        /// <returns>Existing object with properties copied in.</returns>
        public TTarget To<TTarget>(TTarget target)
            where TTarget : new()
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (_scalarOnly == true)
            {
                _copier.SetRules<TSource, TTarget>(true);
            }

            var result = _copier.Copy(_source, target);
            return result;
        }

        #region Obsolete methods        

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        [Obsolete("Use To", false)]
        public TTarget ToNew<TTarget>()
            where TTarget : class, new()
        {         
            return To<TTarget>();
        }

        /// <summary>
        /// Copy to the specified type.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <returns>New instance of target type with property values copied from source.</returns>
        [Obsolete("Use To", false)]
        public TTarget ToNew<TTarget>(Action<TTarget> afterCreationDo)
            where TTarget : class, new()
        {         
            return To(afterCreationDo);
        }

        /// <summary>
        /// To the existing.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="target">The target.</param>
        /// <returns>Existing object with properties copied in.</returns>
        [Obsolete("Use To", false)]
        public TTarget ToExisting<TTarget>(TTarget target)
            where TTarget : class, new()
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            return To(target);
        }

        #endregion        
    }
}