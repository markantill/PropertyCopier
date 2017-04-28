using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier
{
    public class Mapper
    {
        private readonly Dictionary<TypeMapping, MappingData> _mappings =
            new Dictionary<TypeMapping, MappingData>();

        public TTarget Map<TSource, TTarget>(TSource source)            
            where TTarget : new()
        {
            var mappingData = GetMappingData<TSource, TTarget>();
            var target = mappingData.InitializerMappingFunction.Value(source);
            foreach (var mappingDataAfterMappingAction in mappingData.AfterMappingActions)
            {
                mappingDataAfterMappingAction(source, target);
            }

            return target;
        }

        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)            
            where TTarget : new()
        {
            var mappingData = GetMappingData<TSource, TTarget>();
            target = mappingData.CopyMappingFunction.Value(source, target);
            foreach (var mappingDataAfterMappingAction in mappingData.AfterMappingActions)
            {
                mappingDataAfterMappingAction(source, target);
            }

            return target;
        }

        public Expression<Func<TSource, TTarget>> CopyExpression<TSource, TTarget>()            
            where TTarget : new()
        {
            var mappingData = GetMappingData<TSource, TTarget>();
            return mappingData.InitializerMappingExpression.Value;
        }

        public void SetMapping<TSource, TTarget>(bool scalarOnly)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.ScalarOnly = scalarOnly;
        }

        public void IgnoreProperty<TSource, TTarget>(Expression<Func<TTarget, object>> propertyToIgnore)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.PropertyIgnoreExpressions.Add(propertyToIgnore);
        }

        public void AfterCopy<TSource, TTarget>(Action<TSource, TTarget> afterCopyFunction)            
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.AfterMappingActions.Add(afterCopyFunction);
        }

        public void ForProperty<TSource, TTarget, TProperty>(Expression<Func<TTarget, TProperty>> targetProperty,
            Expression<Func<TSource, TProperty>> mappingfunction)             
            where TTarget : new()
        {
            var mappingData = GetOrCreateMappingData<TSource, TTarget>();
            mappingData.PropertyExpressions.Add(
                new PropertyRule { PropertyExpression = targetProperty, MappingRule = mappingfunction });
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
                       InitializerMappingExpression = PropertyCopier<TSource, TTarget>.Expression,
                       InitializerMappingFunction = PropertyCopier<TSource, TTarget>.Copier,
                       CopyMappingFunction = ExistingCopier<TSource, TTarget>.Copier,
                   };
        }

        private MappingData<TSource, TTarget> GetOrCreateMappingData<TSource, TTarget>()            
            where TTarget : new()
        {
            MappingData existingMappingData;
            var typeMapping = new TypeMapping(typeof(TSource), typeof(TTarget));
            if (!_mappings.TryGetValue(new TypeMapping(typeof(TSource), typeof(TTarget)), out existingMappingData))
            {
                existingMappingData = new MappingData<TSource, TTarget>();
                _mappings.Add(typeMapping, existingMappingData);                
                return (MappingData<TSource, TTarget>) existingMappingData;            
            }

            return _mappings[typeMapping] as MappingData<TSource, TTarget>;
        }
    }
}