using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class FlattenedProperitesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();
                        
            var flattenedProperties = GetFlattenedProperties(mappingData.GetSourceProperties(), targetProperties);

            foreach (var propertyMatch in flattenedProperties)
            {
                var sourceEx = ExpressionBuilder.CreateNestedPropertyExpression(
                    Expression.Property(sourceExpression, propertyMatch.SourceProperty),
                    propertyMatch.ChildProperty.Name,
                    propertyMatch.TargetProperty.PropertyType);
                expressions.Add(new PropertyAndExpression(propertyMatch.TargetProperty, sourceEx));
                matched.Add(propertyMatch.TargetProperty);
            }


            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                TargetProperties = newTargetProperties,
                Expressions = expressions,
            };
        }

        /// <summary>
        /// Get the properties of the source we can flatten out in the target
        /// e.g. Target.ChildId = Source.Child.Id
        /// </summary>
        /// <param name="sourceProperties">The source properties</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <returns></returns>
        internal static IEnumerable<PropertyPairChild> GetFlattenedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties)
        {
            var joinedNames =
                from sProperty in sourceProperties
                from cProperty in sProperty.PropertyType.GetProperties()
                join tProperty in targetProperties
                on sProperty.Name.ToUpperInvariant() + cProperty.Name.ToUpperInvariant()
                equals tProperty.Name.ToUpperInvariant()
                where cProperty.PropertyType.IsCastableTo(tProperty.PropertyType)
                where sProperty.CanRead
                where cProperty.CanWrite
                select
                new PropertyPairChild { TargetProperty = tProperty, ChildProperty = cProperty, SourceProperty = sProperty };

            return joinedNames;
        }
    }
}
