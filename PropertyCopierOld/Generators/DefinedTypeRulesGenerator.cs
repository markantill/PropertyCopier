using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Data;
using PropertyCopier.ExpressionVisitors;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// If we have any rules for mapping between types in the mapping data apply them to properties with the same name
    /// and those respective types.
    /// </summary>
    internal class DefinedTypeRulesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();
            var knownMappings = GetKnownTypeMappings(
                mappingData.GetSourceProperties(),
                targetProperties,
                mappingData.KnownMappings,
                memberNameComparer);

            foreach (var knownMapping in knownMappings)
            {
                var propertyExpression = Expression.Property(sourceExpression, knownMapping.SourceProperty);
                var visitor = new AddPropertyRuleExpressionVisitor(propertyExpression);
                var newMapping = (LambdaExpression)visitor.Visit(knownMapping.DefinedMapping.Mapping);
                matched.Add(knownMapping.TargetProperty);
                expressions.Add(new PropertyAndExpression(knownMapping.TargetProperty, newMapping.Body));
            }

            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                UnmappedTargetProperties = newTargetProperties,
            };
        }

        public IEqualityComparer<string> MemberNameComparer { get; set; }

        private static IEnumerable<DefinedMappingPropertyPair> GetKnownTypeMappings(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            IEnumerable<MappingData> mappingDataCollection,
            IEqualityComparer<string> memberNameComparer)
        {
            var matches = TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties, memberNameComparer);

            var knownMappings =
                from match in matches
                join mappingData in mappingDataCollection
                on new { Source = match.SourceProperty.PropertyType, Target = match.TargetProperty.PropertyType }
                equals new { Source = mappingData.SourceType, Target = mappingData.TargetType }
                select new DefinedMappingPropertyPair
                {
                    SourceProperty = match.SourceProperty,
                    TargetProperty = match.TargetProperty,
                    DefinedMapping = new DefinedMapping
                    {
                        SourceType = mappingData.SourceType,
                        TargetType = mappingData.TargetType,
                        Mapping = mappingData.InitializerExpression
                    },
                };

            return knownMappings;
        }
    }
}
