using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class ChildEnumerationGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties,
            MappingData mappingData)
        {
            var expressions = new List<PropertyAndExpression>();                        

            // Child enumerations e.g. Foo.Children = Bar.Children.Select(barchild => new ChildDto { ID = barchild.ID }
            var enumerations = GetChidEnumerations(mappingData.GetSourceProperties(), targetProperties);

            expressions.AddRange(
                from enumeration in enumerations
                let propExpression =
                    ExpressionBuilder.CreateNestedPropertyExpression(sourceExpression, enumeration.SourceProperty.Name)
                let enumerableSourceItemType = enumeration.SourceProperty.PropertyType.GetGenericArguments().First()
                let enumerableTargetItemType = enumeration.TargetProperty.PropertyType.GetGenericArguments().First()
                let childMappingData = mappingData.GetMappingFor(enumerableSourceItemType, enumerableTargetItemType)
                let childInitializser =
                    ExpressionBuilder.CreateLambdaInitializer(enumerableSourceItemType, enumerableTargetItemType, childMappingData)
                let selectCall = ExpressionBuilder.CallEnumerableMethod(propExpression, childInitializser, nameof(Enumerable.Select))
                select new PropertyAndExpression(enumeration.TargetProperty, selectCall));

            var newTargetProperties = targetProperties.Except(expressions.Select(pe => pe.Property)).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                TargetProperties = newTargetProperties,
            };
        }

        /// <summary>
        /// Get enumerations we can map where the names match and they are both IEnumerable{T} for 
        /// </summary>
        /// <param name="sourceProperties">The source properties.</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <returns></returns>
        private static IEnumerable<PropertyPair> GetChidEnumerations(ICollection<PropertyInfo> sourceProperties, ICollection<PropertyInfo> targetProperties)
        {
            var enumerations =
                from sProperty in sourceProperties
                join tProperty in targetProperties
                on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where sProperty.PropertyType != typeof(string)
                where tProperty.PropertyType != typeof(string)
                where !sProperty.PropertyType.IsValueType
                where !tProperty.PropertyType.IsValueType
                where sProperty.CanRead
                where tProperty.CanWrite
                where typeof(IEnumerable).IsAssignableFrom(sProperty.PropertyType)
                where tProperty.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select new PropertyPair { TargetProperty = tProperty, SourceProperty = sProperty };
            return enumerations;
        }
    }
}
