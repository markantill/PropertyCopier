using System;
using System.Linq.Expressions;
using PropertyCopier.Data;
using PropertyCopier.Extensions;
using static PropertyCopier.ExpressionBuilder;

namespace PropertyCopier.Fluent
{
    public class SetRules<TSource, TTarget>
    {
        internal MappingData<TSource, TTarget> MappingData { get; }

        internal SetRules(MappingData<TSource, TTarget> mappingData)
        {
            MappingData = mappingData;
        }

        /// <summary>
        /// Specify a property on the target not to be mapped to.
        /// </summary>
        /// <param name="propertyToIgnore">Expression identifying the property to be ignored.</param>        
        public SetRules<TSource, TTarget> IgnoreProperty(Expression<Func<TTarget, object>> propertyToIgnore)            
        {
            MappingData.PropertyIgnoreExpressions.Add(propertyToIgnore);
            return this;
        }

        /// <summary>
        /// Actions to be performed after the copy, does not apply when copying with Expressions.
        /// </summary>
        /// <param name="afterCopyFunction">The delegate to run after the copy.</param>
        public SetRules<TSource, TTarget> AfterCopy(Action<TSource, TTarget> afterCopyFunction)            
        {            
            MappingData.AfterMappingActions.Add(afterCopyFunction);
            return this;
        }

        /// <summary>
        /// Set a rule to use for a specific property.
        /// </summary>     
        /// <param name="targetProperty"></param>
        /// <param name="mappingFunction"></param>
        public SetRules<TSource, TTarget> ForProperty(Expression<Func<TTarget, object>> targetProperty,
            Expression<Func<TSource, object>> mappingFunction)            
        {            
            var property = GetMemberInfo(targetProperty);
            var targetType = property.GetReturnType();
            var newMappingFunction = StripUnwantedObjectCast(targetType, mappingFunction);

            MappingData.PropertyExpressions.Add(
                new PropertyRule { PropertyExpression = targetProperty, MappingRule = newMappingFunction });
            return this;
        }

        /// <summary>
        /// Define a specific function to use to map between types.
        /// </summary>
        /// <param name="mappingfunction"></param>
        public SetRules<TSource, TTarget> SetMappingRule(Expression<Func<TSource, TTarget>> mappingfunction)
        {            
            MappingData.PredefinedExpression = mappingfunction;
            return this;
        }

        /// <summary>
        /// Map one property directly to another.
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="targetProperty"></param>
        public SetRules<TSource, TTarget> MapPropertyTo(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty)            
        {            
            var sourceMember = GetMemberInfo(sourceProperty);
            var sourceType = sourceMember.GetReturnType();
            var targetMemeber = GetMemberInfo(targetProperty);
            var targetType = targetMemeber.GetReturnType();

            var newSourceLambda = StripUnwantedObjectCast(sourceType, sourceProperty);
            var newTargetLambda = StripUnwantedObjectCast(targetType, targetProperty);

            MappingData.AssignedMappingsExpressions.Add(
                new PropertyRule { PropertyExpression = newTargetLambda, MappingRule = newSourceLambda });

            return this;
        }

        /// <summary>
        /// Force the creation of the code. Any rules subsequently applied will have not effect.
        /// </summary>
        public void PreCompile()
        {
            if (!MappingData.InitializerMappingFunction.IsValueCreated)
            {
                var func = MappingData.InitializerMappingFunction.Value;
            }

            if (!MappingData.CopyMappingFunction.IsValueCreated)
            {
                var func = MappingData.CopyMappingFunction.IsValueCreated;
            }
        }
    }
}