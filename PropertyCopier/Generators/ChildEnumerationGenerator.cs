using System;
using System.Collections;
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
    /// Copy from properties that implement <see cref="IEnumerable{T}"/> to properties that are <see cref="IEnumerable{T}"/>.
    /// Does this by generating Select statements that should work with most Linq providers.
    /// </summary>
    internal class ChildEnumerationGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var expressions = new List<PropertyAndExpression>();

            // Child enumerations e.g. Foo.Children = Bar.Children.Select(barchild => new ChildDto { ID = barchild.ID }
            var enumerations = GetChidEnumerations(mappingData.GetSourceProperties(), targetProperties, memberNameComparer);

            expressions.AddRange(
                from enumeration in enumerations
                let propExpression =
                ExpressionBuilder.CreateNestedPropertyExpression(sourceExpression, enumeration.SourceProperty.Name)
                let enumerableSourceItemType = enumeration.SourceProperty.PropertyType.GetGenericArguments().First()
                let enumerableTargetItemType = enumeration.TargetProperty.PropertyType.GetGenericArguments().First()
                let childMappingData = mappingData.GetMappingFor(enumerableSourceItemType, enumerableTargetItemType)
                let childInitializser =
                ExpressionBuilder.CreateLambdaInitializer(enumerableSourceItemType, enumerableTargetItemType,
                    childMappingData)
                let selectCall =
                ExpressionBuilder.CallEnumerableMethod(propExpression, childInitializser, nameof(Enumerable.Select))
                select new PropertyAndExpression(enumeration.TargetProperty, selectCall));

            var newTargetProperties = targetProperties.Except(expressions.Select(pe => pe.Property)).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                UnmappedTargetProperties = newTargetProperties,
            };
        }

        /// <summary>
        /// Get enumerations we can map where the names match and they are both IEnumerable{T} for 
        /// </summary>
        /// <param name="sourceProperties">The source properties.</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <returns></returns>
        private IEnumerable<PropertyPair> GetChidEnumerations(
            ICollection<PropertyInfo> sourceProperties,
            ICollection<PropertyInfo> targetProperties,
            IEqualityComparer<string> memberNameComparer)
        {
            var matchedNames =
                TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties, memberNameComparer);

            var enumerations =
                from matchedName in matchedNames               
                where matchedName.SourceProperty.PropertyType != typeof(string)
                where matchedName.TargetProperty.PropertyType != typeof(string)
                where !matchedName.SourceProperty.PropertyType.IsValueType
                where !matchedName.TargetProperty.PropertyType.IsValueType
                where typeof(IEnumerable).IsAssignableFrom(matchedName.SourceProperty.PropertyType)
                where matchedName.TargetProperty.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select new PropertyPair { TargetProperty = matchedName.TargetProperty, SourceProperty = matchedName.SourceProperty };
            return enumerations;
        }
    }
}
