using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PropertyCopier.Data;
using PropertyCopier.Extensions;
using PropertyCopier.Fluent;
using static PropertyCopier.ExpressionBuilder;

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
        /// Set the rules for the mapping to use.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="scalarOnly">If true only copy scalar properties, values types and strings. Default value is false.</param>
        /// <param name="comparer">The rules to use to compare names, the default is InvariantCultureIgnoreCase.</param>
        public void SetMapping<TSource, TTarget>(bool scalarOnly = false, StringComparer comparer = null)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.ScalarOnly = scalarOnly;
            mappingData.Comparer = comparer ?? StringComparer.InvariantCultureIgnoreCase;
        }

        /// <summary>
        /// Specify a property on the target not to be mapped to.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="propertyToIgnore">Expression identifying the property to be ignored.</param>        
        public void IgnoreProperty<TSource, TTarget>(Expression<Func<TTarget, object>> propertyToIgnore)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.PropertyIgnoreExpressions.Add(propertyToIgnore);
        }

        /// <summary>
        /// Actions to be performed after the copy, does not apply when copying with Expressions.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="afterCopyFunction">The delegate to run after the copy.</param>
        public void AfterCopy<TSource, TTarget>(Action<TSource, TTarget> afterCopyFunction)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.AfterMappingActions.Add(afterCopyFunction);
        }

        /// <summary>
        /// Set a rule to use for a specific property.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>        
        /// <param name="targetProperty"></param>
        /// <param name="mappingFunction"></param>
        public void ForProperty<TSource, TTarget>(Expression<Func<TTarget, object>> targetProperty,
            Expression<Func<TSource, object>> mappingFunction)             
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            var property = GetMemberInfo(targetProperty);
            var targetType = property.GetReturnType();
            var newMappingFunction = StripUnwantedObjectCast(targetType, mappingFunction);

            mappingData.PropertyExpressions.Add(
                new PropertyRule { PropertyExpression = targetProperty, MappingRule = newMappingFunction });
        }

        /// <summary>
        /// Define a specific function to use to map between types.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="mappingfunction"></param>
        public void SetMappingRule<TSource, TTarget>(Expression<Func<TSource, TTarget>> mappingfunction)
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.PredefinedExpression = mappingfunction;
        }

        /// <summary>
        /// Map one property directly to another.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>        
        /// <typeparam name="TTarget">The target type/</typeparam>
        /// <param name="sourceProperty"></param>
        /// <param name="targetProperty"></param>
        public void MapPropertyTo<TSource, TTarget>(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty) 
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            var sourceMember = GetMemberInfo(sourceProperty);
            var sourceType = sourceMember.GetReturnType();
            var targetMemeber = GetMemberInfo(targetProperty);
            var targetType = targetMemeber.GetReturnType();

            var newSourceLambda = StripUnwantedObjectCast(sourceType, sourceProperty);
            var newTargetLambda = StripUnwantedObjectCast(targetType, targetProperty);

            mappingData.AssignedMappingsExpressions.Add(
                new PropertyRule {PropertyExpression = newTargetLambda, MappingRule = newSourceLambda});            
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