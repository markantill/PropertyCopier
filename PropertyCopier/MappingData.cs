using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyCopier
{
    internal class MappingData<TSource, TTarget> : MappingData
    {     
        public MappingData()
        {
            InitializerMappingExpression = new Lazy<Expression<Func<TSource, TTarget>>>(
                () => PredefinedExpression ?? ExpressionBuilder.CreateLambdaInitializer(this, new ReadOnlyCollection<MappingData>(Mappings.Values.ToList())));

            InitializerMappingFunction = new Lazy<Func<TSource, TTarget>>(
                () => InitializerMappingExpression.Value.Compile());

            CopyMappingFunction = new Lazy<Func<TSource, TTarget, TTarget>>(
                () => ExpressionBuilder.CreateLambdaPropertyCopier(this).Compile());
        }

        public Dictionary<TypeMapping, MappingData> Mappings { get; set; } = new Dictionary<TypeMapping, MappingData>();

        public Expression<Func<TSource, TTarget>> PredefinedExpression { get; set; }

        public ICollection<Expression<Func<TTarget, object>>> PropertyIgnoreExpressions { get; } = new List<Expression<Func<TTarget, object>>>();

        public ICollection<PropertyRule> PropertyExpressions { get; } = new List<PropertyRule>();

        public ICollection<Action<TSource, TTarget>> AfterMappingActions { get; } = new List<Action<TSource, TTarget>>();

        public Lazy<Expression<Func<TSource, TTarget>>> InitializerMappingExpression { get; set; } 

        public Lazy<Func<TSource, TTarget>> InitializerMappingFunction { get; set; }

        public Lazy<Func<TSource, TTarget, TTarget>> CopyMappingFunction { get; set; }

        public override Type SourceType => typeof(TSource);

        public override Type TargetType => typeof(TTarget);

        public override IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions => PropertyIgnoreExpressions;

        public override IEnumerable<PropertyRule> PropertyLambdaExpressions => PropertyExpressions;

        public override LambdaExpression InitializerExpression => InitializerMappingExpression.Value;
    }

    internal abstract class MappingData
    {
        public bool ScalarOnly { get; set; }

        public abstract Type SourceType { get; }

        public abstract Type TargetType { get; }

        public abstract IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions { get; }

        public abstract IEnumerable<PropertyRule> PropertyLambdaExpressions { get; }

        public abstract LambdaExpression InitializerExpression { get; }
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

        public LambdaExpression Mapping { get; set; }
    }
}