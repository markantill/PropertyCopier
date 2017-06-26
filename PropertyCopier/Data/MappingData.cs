using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Base class for holding all the rules for mapping between one type an another.
    /// Has no generics for easier consumption.
    /// </summary>
    internal abstract class MappingData
    {
        public bool IgnoreComplexObjects { get; set; }

        public StringComparer Comparer { get; set; } = StringComparer.InvariantCultureIgnoreCase;

        public abstract Type SourceType { get; }

        public abstract Type TargetType { get; }

        public ICollection<PropertyRule> AssignedMappingsExpressions { get; protected set; }

        public abstract IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions { get; }

        public ICollection<PropertyRule> PropertyExpressions { get; protected set; }

        public abstract LambdaExpression InitializerExpression { get; }

        public ICollection<PropertyInfo> GetSourceProperties()
        {
            var sourceProperties = SourceType.GetProperties()
                .Where(p => p != null)
                .Where(p => p.CanRead);
            if (IgnoreComplexObjects)
            {
                sourceProperties =
                    sourceProperties.Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));
            }

            return sourceProperties.ToList();
        }

        public MappingData GetMappingFor(Type sourceType, Type targetType)
        {
            var mappingData = KnownMappings.FirstOrDefault(
                                  m => m.SourceType == sourceType && m.TargetType == targetType) ??
                              new DefaultMappingData(sourceType, targetType, KnownMappings);

            return mappingData;
        }

        public abstract ICollection<MappingData> KnownMappings { get; }

        public bool FlattenChildObjects { get; set; } = true;

        public bool MapChildObjects { get; set; } = true;

        public bool MapChildEnumerations { get; set; } = true;

        public bool MapChildCollections { get; set; } = true;

        public bool AddNullChecking { get; set; } = false;
    }

    /// <summary>
    /// Holds all the rules for mapping between one type an another.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    internal class MappingData<TSource, TTarget> : MappingData
    {     
        public MappingData()
        {
            PropertyExpressions = new List<PropertyRule>();
            AssignedMappingsExpressions = new List<PropertyRule>(); 

            InitializerMappingExpression = new Lazy<Expression<Func<TSource, TTarget>>>(
                () => PredefinedExpression ?? ExpressionBuilder.CreateLambdaInitializer(this));

            InitializerMappingFunction = new Lazy<Func<TSource, TTarget>>(
                () => InitializerMappingExpression.Value.Compile());

            CopyMappingFunction = new Lazy<Func<TSource, TTarget, TTarget>>(
                () => ExpressionBuilder.CreateLambdaPropertyCopier(this).Compile());
        }

        public Dictionary<TypeMapping, MappingData> Mappings { get; set; } = new Dictionary<TypeMapping, MappingData>();

        public Expression<Func<TSource, TTarget>> PredefinedExpression { get; set; }       

        public ICollection<Expression<Func<TTarget, object>>> PropertyIgnoreExpressions { get; } = new List<Expression<Func<TTarget, object>>>();        

        public ICollection<Action<TSource, TTarget>> AfterMappingActions { get; } = new List<Action<TSource, TTarget>>();

        public Lazy<Expression<Func<TSource, TTarget>>> InitializerMappingExpression { get; set; } 

        public Lazy<Func<TSource, TTarget>> InitializerMappingFunction { get; set; }

        public Lazy<Func<TSource, TTarget, TTarget>> CopyMappingFunction { get; set; }

        public override Type SourceType => typeof(TSource);

        public override Type TargetType => typeof(TTarget);

        public override IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions => PropertyIgnoreExpressions;

        public override LambdaExpression InitializerExpression => InitializerMappingExpression.Value;

        public override ICollection<MappingData> KnownMappings => Mappings.Values.ToList();        
    }  
}