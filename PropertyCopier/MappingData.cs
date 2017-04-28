using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PropertyCopier
{
    internal class MappingData<TSource, TTarget> : MappingData
    {
        public MappingData()
        {
            InitializerMappingExpression = new Lazy<Expression<Func<TSource, TTarget>>>(
                () => ExpressionBuilder.CreateLambdaInitializer(this));

            InitializerMappingFunction = new Lazy<Func<TSource, TTarget>>(
                () => InitializerMappingExpression.Value.Compile());

            CopyMappingFunction = new Lazy<Func<TSource, TTarget, TTarget>>(
                () => ExpressionBuilder.CreateLambdaPropertyCopier(this).Compile());
        }

        public ICollection<Expression<Func<TTarget, object>>> PropertyIgnoreExpressions { get; } = new List<Expression<Func<TTarget, object>>>();

        public ICollection<PropertyRule> PropertyExpressions { get; } = new List<PropertyRule>();

        public ICollection<Action<TSource, TTarget>> AfterMappingActions { get; } = new List<Action<TSource, TTarget>>();

        public ICollection<DefinedMapping> DefinedMappings { get; set; }

        public Lazy<Expression<Func<TSource, TTarget>>> InitializerMappingExpression { get; set; } 

        public Lazy<Func<TSource, TTarget>> InitializerMappingFunction { get; set; }

        public Lazy<Func<TSource, TTarget, TTarget>> CopyMappingFunction { get; set; }

        public override IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions => PropertyIgnoreExpressions;

        public override IEnumerable<PropertyRule> PropertyLambdaExpressions => PropertyExpressions;

        public override IEnumerable<DefinedMapping> DefinedMappingRules => DefinedMappings;
    }

    internal abstract class MappingData
    {
        public bool ScalarOnly { get; set; }

        public abstract IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions { get; }

        public abstract IEnumerable<PropertyRule> PropertyLambdaExpressions { get; }

        public abstract IEnumerable<DefinedMapping> DefinedMappingRules { get; }
    }

    internal class PropertyRule
    {
        public LambdaExpression PropertyExpression { get; set; }

        public LambdaExpression MappingRule { get; set; }
    }

    internal class DefinedMapping
    {
        public Type SourceType { get; set; }

        public Type TargetType { get; set; }

        public Expression<Func<LambdaExpression>> Mapping { get; set; }
    }
}