using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PropertyCopier.Data;
using PropertyCopier.Fluent;

namespace PropertyCopier
{    
    /// <summary>
    /// Copies property values from one object to another.
    /// </summary>
    public class Copier
    {
        private readonly Dictionary<TypeMapping, MappingData> _mappings =
            new Dictionary<TypeMapping, MappingData>();      

        /// <summary>
        /// Copy from an object of a specific type.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public CopyFrom<TSource> From<TSource>(TSource source)
            where TSource : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var result = new CopyFrom<TSource>(source, this);
            return result;
        }

        /// <summary>
        /// The generated <see cref="Expression"/> that will create a new instance
        /// of TTarget from TSource.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <returns>The <see cref="CopyExpression{TSource,TTarget}"/></returns>
        public Expression<Func<TSource, TTarget>> CopyExpression<TSource, TTarget>()            
            where TTarget : new()
        {
            var mappingData = GetMappingData<TSource, TTarget>();
            return mappingData.InitializerMappingExpression.Value;
        }

        /// <summary>
        /// Set the rules for the copier to use.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="scalarOnly">If true only copy scalar properties, values types and strings. Default value is false.</param>
        /// <param name="comparer">The rules to use to compare names, the default is InvariantCultureIgnoreCase.</param>
        public SetRules<TSource, TTarget> SetRules<TSource, TTarget>(
            bool scalarOnly = false,            
            bool flattenChildObjects = true,
            bool copyChildObjects = true,
            bool copyChildEnumerations = true,
            bool copyChildCollections = true,
            bool addNullChecking = false,
            StringComparer comparer = null)
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.ScalarOnly = scalarOnly;
            mappingData.FlattenChildObjects = flattenChildObjects;
            mappingData.MapChildObjects = copyChildObjects;
            mappingData.MapChildEnumerations = copyChildEnumerations;
            mappingData.MapChildCollections = copyChildCollections;
            mappingData.AddNullChecking = addNullChecking;
            mappingData.Comparer = comparer ?? StringComparer.InvariantCultureIgnoreCase;

            var result = new SetRules<TSource, TTarget>(mappingData);
            return result;
        }


        internal TTarget Copy<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            var target = mappingData.InitializerMappingFunction.Value(source);
            foreach (var mappingDataAfterMappingAction in mappingData.AfterMappingActions)
            {
                mappingDataAfterMappingAction(source, target);
            }

            return target;
        }

        internal TTarget Copy<TSource, TTarget>(TSource source, TTarget target)
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            target = mappingData.CopyMappingFunction.Value(source, target);
            foreach (var mappingDataAfterMappingAction in mappingData.AfterMappingActions)
            {
                mappingDataAfterMappingAction(source, target);
            }

            return target;
        }

        private MappingData<TSource, TTarget> GetMappingData<TSource, TTarget>()            
            where TTarget : new()
        {
            MappingData existingMappingData;
            _mappings.TryGetValue(new TypeMapping(typeof(TSource), typeof(TTarget)), out existingMappingData);
            return existingMappingData as MappingData<TSource, TTarget> ??
                   new MappingData<TSource, TTarget>
                   {
                       ScalarOnly = false,
                       Mappings = _mappings,
                   };
        }

        private MappingData<TSource, TTarget> GetOrCreateMappingData<TSource, TTarget>()            
            where TTarget : new()
        {
            MappingData existingMappingData;
            var typeMapping = new TypeMapping(typeof(TSource), typeof(TTarget));
            if (!_mappings.TryGetValue(new TypeMapping(typeof(TSource), typeof(TTarget)), out existingMappingData))
            {
                existingMappingData = new MappingData<TSource, TTarget> { Mappings = _mappings };
                _mappings.Add(typeMapping, existingMappingData);                
                return (MappingData<TSource, TTarget>) existingMappingData;            
            }

            return _mappings[typeMapping] as MappingData<TSource, TTarget>;
        }
    }
}