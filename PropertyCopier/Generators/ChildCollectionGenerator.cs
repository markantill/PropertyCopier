using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Data;
using static PropertyCopier.TypeHelper;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// Copies from properties that implement <see cref="IEnumerable{T}"/> to various collections.
    /// </summary>
    internal class ChildCollectionGenerator : IExpressionGenerator
    {
#if NET45
        private readonly Type[] _listTypes = { typeof(List<>), typeof(IList<>), typeof(IReadOnlyList<>), typeof(ICollection<>), typeof(IReadOnlyCollection<>) };
        private readonly Type[] _hashSetTypes = { typeof(HashSet<>), typeof(ISet<>) };
#elif NET40
        Type[] _listTypes = { typeof(List<>), typeof(IList<>), typeof(ICollection<>) };
        Type[] _hashSetTypes = { typeof(HashSet<>) };
#endif
        private readonly Type[] _linkedListTypes = { typeof(LinkedList<>) };
        private readonly Type[] _readOnlyCollectionTypes = { typeof(ReadOnlyCollection<>) };

        public ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData, IEqualityComparer<string> memberNameComparer)
        {
            var collectionTypes = 
                _listTypes
                .Union(_hashSetTypes)
                .Union(_linkedListTypes)
                .Union(_readOnlyCollectionTypes);

            var matchedProperties = 
                GetNameMatchedProperties(mappingData.GetSourceProperties(),
                targetProperties, memberNameComparer);

            var matches =
                from matchedProperty in matchedProperties
                where matchedProperty.SourceProperty.PropertyType.IsGenericType
                where GetIEnumerableImpl(matchedProperty.SourceProperty.PropertyType) != null
                where matchedProperty.TargetProperty.PropertyType.IsGenericType
                where collectionTypes.Contains(matchedProperty.TargetProperty.PropertyType.GetGenericTypeDefinition())
                select matchedProperty;

            var result = new ExpressionGeneratorResult();            
            var newTargetProperties = targetProperties.ToList();

            foreach (var match in matches)
            {
                var propExpression =
                    ExpressionBuilder.CreateNestedPropertyExpression(sourceExpression, match.SourceProperty.Name);
                var enumerableSourceItemType = match.SourceProperty.PropertyType.GetGenericArguments().First();
                var enumerabvarargetItemType = match.TargetProperty.PropertyType.GetGenericArguments().First();
                var childMappingData = mappingData.GetMappingFor(enumerableSourceItemType, enumerabvarargetItemType);
                var childInitializser =
                    ExpressionBuilder.CreateLambdaInitializer(enumerableSourceItemType, enumerabvarargetItemType,
                        childMappingData);
                var selectCall =
                    ExpressionBuilder.CallEnumerableMethod(propExpression, childInitializser,
                        nameof(Enumerable.Select), asQueryable: false);
                var collection = CreationRule(match.TargetProperty.PropertyType.GetGenericTypeDefinition(),
                    enumerabvarargetItemType, selectCall);
                result.Expressions.Add(new PropertyAndExpression(match.TargetProperty, collection));

                newTargetProperties.Remove(match.TargetProperty);
            }

            result.UnmappedTargetProperties = newTargetProperties;
            return result;
        }        

        private Expression CreationRule(Type targetType, Type itemType, Expression sourceExpression)
        {
            Expression result = null;


            if (_listTypes.Contains(targetType))
            {
                return CreateConstructorTakesIEnumerable(typeof(List<>), itemType, sourceExpression);
            }

            if (_hashSetTypes.Contains(targetType))
            {
                return CreateConstructorTakesIEnumerable(typeof(HashSet<>), itemType, sourceExpression);
            }

            if (_linkedListTypes.Contains(targetType))
            {
                return CreateConstructorTakesIEnumerable(typeof(LinkedList<>), itemType, sourceExpression);
            }

            if (_readOnlyCollectionTypes.Contains(targetType))
            {
                var listConstructor = CreateConstructorTakesIEnumerable(typeof(List<>), itemType, sourceExpression);
                var constructor = typeof(ReadOnlyCollection<>).MakeGenericType(itemType).GetConstructor(new [] { typeof(List<>).MakeGenericType(itemType) });                   
                result = Expression.New(constructor, listConstructor);                
            }

            return result;
        }

        private Expression CreateConstructorTakesIEnumerable(Type targetType, Type itemType, Expression sourceExpression)
        {
            var inputType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var collectionType = targetType.MakeGenericType(itemType);
            var constructor = collectionType.GetConstructors().Single(c => c.GetParameters().Length == 1 && c.GetParameters().Single().ParameterType == inputType);
            var result = Expression.New(constructor, sourceExpression);
            return result;
        }
    }
}
