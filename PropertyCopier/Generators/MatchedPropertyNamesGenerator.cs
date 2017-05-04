using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class MatchedPropertyNamesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression, 
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();

            var matches = GetMatchedProperties(mappingData.GetSourceProperties(), targetProperties);

            foreach (var propertyMatch in matches)
            {
                if (propertyMatch.TargetProperty.PropertyType.IsValueType ||
                    propertyMatch.TargetProperty.PropertyType == typeof(string))
                {
                    var sourceExp = ExpressionBuilder.CreateSourceExpression(
                        propertyMatch.TargetProperty.PropertyType,
                        propertyMatch.SourceProperty.PropertyType,
                        propertyMatch.TargetProperty,
                        propertyMatch.SourceProperty,
                        sourceExpression);
                    expressions.Add(new PropertyAndExpression(propertyMatch.TargetProperty, sourceExp));
                    matched.Add(propertyMatch.TargetProperty);
                }
            }

            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                TargetProperties = newTargetProperties,
                Expressions = expressions,
            };
        }

        internal static IEnumerable<PropertyPair> GetMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties)
        {
            var matches =
                from match in TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties)
                where match.SourceProperty.PropertyType.IsCastableTo(match.TargetProperty.PropertyType)
                select match;

            return matches;
        }
    }
}
