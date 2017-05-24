using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Data;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// Match properties based on same name as determined my the memberNameComparer and source type
    /// is castable to target type.
    /// </summary>
    internal class MatchedPropertyNamesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();

            var matches = GetMatchedProperties(mappingData.GetSourceProperties(), targetProperties, memberNameComparer);

            foreach (var propertyMatch in matches)
            {
                if (propertyMatch.TargetProperty.PropertyType.IsValueType ||
                    propertyMatch.TargetProperty.PropertyType == typeof(string))
                {
                    var sourceExp = ExpressionBuilder.CreateSourceExpression(
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
                UnmappedTargetProperties = newTargetProperties,
                Expressions = expressions,
            };
        }

        public IEqualityComparer<string> MemberNameComparer { get; set; }

        internal static IEnumerable<PropertyPair> GetMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            IEqualityComparer<string> memberNameComparer)
        {
            var matches =
                from match in TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties, memberNameComparer)
                where match.SourceProperty.PropertyType.IsCastableTo(match.TargetProperty.PropertyType)
                select match;

            return matches;
        }
    }
}
