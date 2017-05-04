using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class DefinedTypeRulesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();
            var knownMappings = GetKnownTypeMappings(mappingData.GetSourceProperties(), targetProperties, mappingData.KnownMappings);

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
                TargetProperties = newTargetProperties,
            };
        }

        private static IEnumerable<DefinedMappingPropertyPair> GetKnownTypeMappings(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            IEnumerable<MappingData> mappingDataCollection)
        {
            var matches = TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties);

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
