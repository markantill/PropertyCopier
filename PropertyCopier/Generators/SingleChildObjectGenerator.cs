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
    /// Generate expressions for properties of a child object (not enumeration) that
    /// correspond with a source target object.
    /// e.g. Target.Child = new Child { Id = Source.Child.Id }
    /// </summary>
    internal class SingleChildObjectGenerator : IExpressionGenerator
    {
        private readonly NullSafeVisitor _nullSafeVisitor = new NullSafeVisitor();

        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();            

            // Nested Child objects e.g. Foo.Owner = new OwnerDto { ID = bar.Owner.ID, Name = bar.Owner.Name }            
            var nestedProperties = GetNestedPropertyMatches(mappingData.GetSourceProperties(), targetProperties, memberNameComparer);

            foreach (var propertyMatch in nestedProperties)
            {
                var sourcePropertyType = propertyMatch.SourceProperty.PropertyType;
                var targetPropertyType = propertyMatch.TargetProperty.PropertyType;

                var propExpression = ExpressionBuilder.CreateNestedPropertyExpression(
                    sourceExpression,
                    propertyMatch.SourceProperty.Name);

                var newMappingData = mappingData.GetMappingFor(sourcePropertyType, targetPropertyType);

                var exp = ExpressionBuilder.CreateLambdaInitializerBody(
                    sourcePropertyType,
                    targetPropertyType,
                    propExpression,
                    newMappingData);

                expressions.Add(new PropertyAndExpression(propertyMatch.TargetProperty, exp));
                matched.Add(propertyMatch.TargetProperty);
            }

            targetProperties = targetProperties.Except(matched).ToArray();
            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                UnmappedTargetProperties = newTargetProperties,
            };
        }

        public IEqualityComparer<string> MemberNameComparer { get; set; }


        /// <summary>
        /// Get the properties that match at the child level
        /// e.g. Target.Child.Id = Source.Child.Id
        /// Note this only goes down one level.
        /// </summary>
        /// <param name="sourceProperties">The source properties.</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <param name="memberNameComparer"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyPair> GetNestedPropertyMatches(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            IEqualityComparer<string> memberNameComparer)
        {
            var matchedProperties =
                TypeHelper.GetNameMatchedProperties(sourceProperties, targetProperties, memberNameComparer);

            var joinedObjects =
                from matchedProperty in matchedProperties                
                where matchedProperty.SourceProperty.PropertyType != typeof(string)
                where matchedProperty.TargetProperty.PropertyType != typeof(string)
                where !matchedProperty.SourceProperty.PropertyType.IsValueType
                where !matchedProperty.TargetProperty.PropertyType.IsValueType
                where TypeHelper.GetIEnumerableImpl(matchedProperty.TargetProperty.PropertyType) == null
                where matchedProperty.TargetProperty.PropertyType.GetConstructor(Type.EmptyTypes) != null
                select matchedProperty;

            return joinedObjects;
        }
    }
}
