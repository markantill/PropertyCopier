using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyCopier
{    
    /// <summary>
    ///     Class for creating expression trees.
    /// </summary>
    internal static class ExpressionBuilder
    {
        #region Public Methods and Operators

        /// <summary>
        /// Creates the lambda initializer to create new object and select properties based on properties of source
        /// type where the property names match.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="mappingData">Optional mapping data to be applied.</param>
        /// <returns>Lambda expression to initialise object.</returns>
        internal static Expression<Func<TSource, TTarget>> CreateLambdaInitializer<TSource, TTarget>(            
            MappingData<TSource, TTarget> mappingData,
            ICollection<MappingData> mappingDataCollection)
        {
            return (Expression<Func<TSource, TTarget>>) CreateLambdaInitializer(
                typeof(TSource), 
                typeof(TTarget),
                mappingData, mappingDataCollection);
        }

        /// <summary>
        /// Creates the lambda initializer to create new object and select properties based on properties of source
        /// type where the property names match.
        /// </summary>
        /// <param name="source">The type of the source.</param>
        /// <param name="target">The type of the target.</param>
        /// <param name="mappingData">The mapping data.</param>
        /// <returns>Lambda expression to initialise object.</returns>
        internal static LambdaExpression CreateLambdaInitializer(
            Type source,
            Type target,
            MappingData mappingData,
            ICollection<MappingData> mappingDataCollection)
        {
            // Were going to build an expression that looks like:
            // source => new Foo { Property1 = bar.Property1, Property2 = bar.Property2 }
            var sourceParameter = Expression.Parameter(source, nameof(source));

            var initializer = CreateLambdaInitializerBody(source, target, sourceParameter, mappingData, mappingDataCollection);

            // Create a Lambda expression from the parameter and body we have already created.
            var copyExpression = Expression.Lambda(initializer, sourceParameter);
            return copyExpression;
        }

        /// <summary>
        /// Creates the lambda property copier expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>      
        /// <param name="mappingData">The mapping data.</param> 
        /// <returns>Expression to copy properties with same name and type.</returns>
        internal static Expression<Func<TSource, TTarget, TTarget>> CreateLambdaPropertyCopier<TSource, TTarget>(            
            MappingData<TSource, TTarget> mappingData)
        {
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var targetParameter = Expression.Parameter(typeof(TTarget), "target");
            var sourceProperties = GetSourceProperties(typeof(TSource), mappingData.ScalarOnly);
            var targetProperties = typeof(TTarget).GetProperties();

            var alreadyMatched = mappingData.PropertyIgnoreLambdaExpressions == null
                ? new HashSet<PropertyInfo>()
                : new HashSet<PropertyInfo>(mappingData.PropertyIgnoreLambdaExpressions.Select(GetMemberInfo).OfType<PropertyInfo>());

            targetProperties = targetProperties.Except(alreadyMatched, new PropertyInfoComparer()).ToArray();

            // Copying properties is going to require building a statement (multi-line) lambda, 
            // each entry in the list will be one line of "code" in the statement.
            var exps = new List<Expression>();

            foreach (var propertyRule in mappingData.PropertyLambdaExpressions)
            {
                var predefined = GetPredefinedRule(propertyRule, sourceParameter);
                alreadyMatched.Add(predefined.Property);
                exps.Add(predefined.Expression);
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            var matches = GetMatchedProperties(sourceProperties, targetProperties);
            foreach (var match in matches)
            {
                var sourceExp = CreateSourceExpression<TSource, TTarget>(
                    match.TargetProperty,
                    match.SourceProperty,
                    sourceParameter);

                // Expressions will not do boxing or implicit conversions, so make sure the
                // type is explicitly cast to the destination type if not the same.                
                var targetExp = Expression.Property(targetParameter, match.TargetProperty);
                var setExp = Expression.Assign(targetExp, sourceExp);
                exps.Add(setExp);
                alreadyMatched.Add(match.TargetProperty);
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            var joinedNames = GetFlattenedProperties(sourceProperties, targetProperties);

            foreach (var joinedName in joinedNames)
            {
                var sourceExp = CreateNestedPropertyExpression(
                    Expression.Property(sourceParameter, joinedName.SourceProperty),                    
                    joinedName.ChildProperty.Name,
                    joinedName.TargetProperty.PropertyType);

                // Expressions will not do boxing or implicit conversions, so make sure the
                // type is explicitly cast to the destination type if not the same.                
                var targetExp = Expression.Property(targetParameter, joinedName.TargetProperty);
                var setExp = Expression.Assign(targetExp, sourceExp);
                exps.Add(setExp);
                alreadyMatched.Add(joinedName.TargetProperty);
            }


            // Finally we want to return the result, there is no Return expression instead
            // just make the last line of the method body what you want to return.
            exps.Add(targetParameter);

            Expression block = Expression.Block(exps);
            var exp = Expression.Lambda<Func<TSource, TTarget, TTarget>>(
                block,
                sourceParameter,
                targetParameter);
            return exp;
        }

        private static PropertyRuleResult GetPredefinedRule(PropertyRule propertyLambdaExpression, Expression sourcExpression)
        {
            var targetProperty = (PropertyInfo)GetMemberInfo(propertyLambdaExpression.PropertyExpression);
            var visitor = new AddPropertyRuleExpressionVisitor(sourcExpression);
            var newExpression = (LambdaExpression)visitor.Visit(propertyLambdaExpression.MappingRule);
            return new PropertyRuleResult
            {
                Property = targetProperty,
                Expression = newExpression.Body,
            };
        }

        /// <summary>
        /// Creates the expression for nested properties in the string.
        /// </summary>
        /// <param name="startingExpression">The staring expression, for example the inital parameter.</param>
        /// <param name="propertyName">Name of the nested property e.g. "MyObject.MyProperty".</param>
        /// <param name="finalType">The final type to cast to.</param>
        /// <returns>Nested expressions with cast.</returns>
        internal static Expression CreateNestedPropertyExpression(Expression startingExpression, string propertyName, Type finalType = null)
        {
            while (true)
            {
                var split = propertyName.Split('.');
                var nextPropertyName = split.First();
                startingExpression = Expression.PropertyOrField(startingExpression, nextPropertyName);
                if (split.Length == 1)
                {
                    if (finalType != null)
                    {
                        startingExpression = Expression.Convert(startingExpression, finalType);
                    }

                    return startingExpression;
                }

                propertyName = string.Join(".", split.Skip(1));
            }
        }

        /// <summary>
        /// Calls the specified <see cref="Enumerable"/> method that takes a collection and an expression. e.g. Select etc.
        /// </summary>
        /// <param name="collection">The expression representing the collection.</param>
        /// <param name="predicate">The expression that will be run.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Expression representing calling the method.</returns>
        internal static Expression CallEnumerableMethod(
            Expression collection,
            LambdaExpression predicate,
            string methodName)
        {
            // Get the collections implementation of IEnumerable<T> so we can figure out what T is for it.
            var collectionType = TypeHelper.GetIEnumerableImpl(collection.Type);

            // Cast the collection to the IEnumerable<T> just for safety.
            collection = Expression.Convert(collection, collectionType);

            // Get the type of the element in the collection, T.
            var elemType = collectionType.GetGenericArguments()[0];

            // Get arg1, arg2 from Expression<Func<arg1, arg2>>
            var expTypes = predicate.GetType().GetGenericArguments().First().GetGenericArguments().ToArray();

            // Figure out what the type of the predicate must be, it must be Func<T, bool>
            var predicateType = typeof(Func<,>).MakeGenericType(expTypes);

            // Generate the Call Expressions
            return GenerateMethodCallExpression(
                collection,
                predicate,
                methodName,
                predicateType,
                elemType,
                collectionType,
                expTypes);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the lambda initializer body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="sourceParameter">The source parameter.</param>
        /// <param name="mappingData">The mapping data.</param>
        /// <returns>Expression for lambda body.</returns>
        private static Expression CreateLambdaInitializerBody(
            Type source,
            Type target,
            Expression sourceParameter,
            MappingData mappingData,
            ICollection<MappingData> mappingDataCollection)
        {
            // MemberBindings are going to be values inside the braces of the expression e.g. Property1 = source.Property1
            var bindings = new List<MemberBinding>();
            var sourceProperties = GetSourceProperties(source, mappingData.ScalarOnly).ToList();
            var targetProperties = target.GetProperties();
            var alreadyMatched = mappingData.PropertyIgnoreLambdaExpressions == null
                ? new HashSet<PropertyInfo>()
                : new HashSet<PropertyInfo>(mappingData.PropertyIgnoreLambdaExpressions.Select(GetMemberInfo)
                    .OfType<PropertyInfo>());

            targetProperties = targetProperties.Except(alreadyMatched, new PropertyInfoComparer()).ToArray();

            // Apply any specific rules for those properties.

            foreach (var propertyRule in mappingData.PropertyLambdaExpressions)
            {
                var predefined = GetPredefinedRule(propertyRule, sourceParameter);
                alreadyMatched.Add(predefined.Property);
                bindings.Add(Expression.Bind(predefined.Property, predefined.Expression));
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // Any specific rules for the types
            var knownMappings = GetKnownProperties(sourceProperties, targetProperties, mappingDataCollection);
            foreach (var knownMapping in knownMappings)
            {
                var propertyExpression = Expression.Property(sourceParameter, knownMapping.SourceProperty);
                var visitor = new AddPropertyRuleExpressionVisitor(propertyExpression);
                var newMapping = (LambdaExpression)visitor.Visit(knownMapping.DefinedMapping.Mapping);
                alreadyMatched.Add(knownMapping.TargetProperty);
                bindings.Add(Expression.Bind(knownMapping.TargetProperty, newMapping.Body));                
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // normal matches e.g. Foo.ID = Bar.ID
            var matches = GetMatchedProperties(sourceProperties, targetProperties);

            foreach (var propertyMatch in matches)
            {
                if (propertyMatch.TargetProperty.PropertyType.IsValueType ||
                    propertyMatch.TargetProperty.PropertyType == typeof(string))
                {
                    var sourceExp = CreateSourceExpression(
                        source,
                        target,
                        propertyMatch.TargetProperty,
                        propertyMatch.SourceProperty,
                        sourceParameter);
                    bindings.Add(Expression.Bind(propertyMatch.TargetProperty, sourceExp));
                    alreadyMatched.Add(propertyMatch.TargetProperty);
                }
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // nested scalar matches e.g. Foo.OwnerID = Bar.Owner.ID
            var flattenedProperties = GetFlattenedProperties(sourceProperties, targetProperties);

            foreach (var propertyMatch in flattenedProperties)
            {
                var sourceEx = CreateNestedPropertyExpression(
                    Expression.Property(sourceParameter, propertyMatch.SourceProperty),
                    propertyMatch.ChildProperty.Name,
                    propertyMatch.TargetProperty.PropertyType);
                bindings.Add(Expression.Bind(propertyMatch.TargetProperty, sourceEx));
                alreadyMatched.Add(propertyMatch.TargetProperty);
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // Nested Child objects e.g. Foo.Owner = new OwnerDto { ID = bar.Owner.ID, Name = bar.Owner.Name }
            // TODO: see if we already have a mapping for the child and use it if we do.
            var nestedProperties = GetNestedPropertyMatches(sourceProperties, targetProperties);

            foreach (var propertyMatch in nestedProperties)
            {
                var propExpression = CreateNestedPropertyExpression(
                    sourceParameter,
                    propertyMatch.SourceProperty.Name);

                var exp = CreateLambdaInitializerBody(
                    propertyMatch.SourceProperty.PropertyType,
                    propertyMatch.TargetProperty.PropertyType,
                    propExpression,
                    mappingData,
                    mappingDataCollection);

                bindings.Add(Expression.Bind(propertyMatch.TargetProperty, exp));
                alreadyMatched.Add(propertyMatch.TargetProperty);
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // Child enumerations e.g. Foo.Children = Bar.Children.Select(barchild => new ChildDto { ID = barchild.ID }
            var enumerations = GetChidEnumerations(sourceProperties, targetProperties);

            bindings.AddRange(
                from enumeration in enumerations
                let propExpression =
                CreateNestedPropertyExpression(sourceParameter, enumeration.SourceProperty.Name)
                let enumerableSourceItemType = enumeration.SourceProperty.PropertyType.GetGenericArguments().First()
                let enumerableTargetItemType = enumeration.TargetProperty.PropertyType.GetGenericArguments().First()
                let childInitializser =
                CreateLambdaInitializer(enumerableSourceItemType, enumerableTargetItemType, mappingData, mappingDataCollection)
                let selectCall = CallEnumerableMethod(propExpression, childInitializser, nameof(Enumerable.Select))
                select Expression.Bind(enumeration.TargetProperty, selectCall));

            // Create Expression for initialising object with correct values, the new MyClass part of the expression.            
            var initializer = Expression.MemberInit(Expression.New(target), bindings);
            return initializer;
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
                select new PropertyPair {TargetProperty = tProperty, SourceProperty = sProperty};
            return enumerations;
        }

        /// <summary>
        /// Get the properties that match at the child level
        /// e.g. Target.Child.Id = Source.Child.Id
        /// Note this only goes down one level.
        /// </summary>
        /// <param name="sourceProperties">The source properties.</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <returns></returns>
        private static IEnumerable<PropertyPair> GetNestedPropertyMatches(List<PropertyInfo> sourceProperties, PropertyInfo[] targetProperties)
        {
            var joinedObjects =
                from sProperty in sourceProperties
                join tProperty in targetProperties
                on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where sProperty.PropertyType != typeof(string)
                where tProperty.PropertyType != typeof(string)
                where !sProperty.PropertyType.IsValueType
                where !tProperty.PropertyType.IsValueType
                where sProperty.CanRead
                where tProperty.CanWrite
                where tProperty.PropertyType.GetConstructor(Type.EmptyTypes) != null
                select new PropertyPair {TargetProperty = tProperty, SourceProperty = sProperty};
            return joinedObjects;
        }

        /// <summary>
        /// Get the properties of the source we can flatten out in the target
        /// e.g. Target.ChildId = Source.Child.Id
        /// </summary>
        /// <param name="sourceProperties">The source properties</param>
        /// <param name="targetProperties">The target properties.</param>
        /// <returns></returns>
        private static IEnumerable<PropertyPairChild> GetFlattenedProperties(
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
                new PropertyPairChild {TargetProperty = tProperty, ChildProperty = cProperty, SourceProperty = sProperty};

            return joinedNames;
        }

        private static Expression GenerateMethodCallExpression(
            Expression collectionExpression,
            Expression delegateExpression,
            string methodName,
            Type delegateType,
            Type elementType,
            Type collectionType,
            Type[] delegateGenericParamaters)
        {
            // Figure out what the type of the expression representing the predicate must be,
            // Expression<Func<T, bool>>
            var expressionPredicateType = typeof(Expression<>).MakeGenericType(delegateType);

            // Get the Queryable.AsQueryable method for the collection expresions.
            var asQueryableMethod = (MethodInfo)
                TypeHelper.GetGenericMethod(
                    typeof(Queryable),
                    nameof(Queryable.AsQueryable),
                    new[] { elementType },
                    new[] { collectionType },
                    BindingFlags.Static);

            // Apply the AsQueryable method. We need to do this so we have a method that we can
            // pass an expression into that we can build up. If it stays as as IEnumerable we would need to 
            // pass in a delgate not an expression and that wouldn't work if were working with Linq to Entites or similar.
            var collectionAsQueryable = Expression.Call(asQueryableMethod, collectionExpression);

            // Figure out they type now it is an IQueryable<T>.
            var queryableType = typeof(IQueryable<>).MakeGenericType(elementType);

            // Get our actual method to call, signature is Queryable.[methodName]<T>(IQueryable<T>, Expression<Func<T,bool>>)
            var method = (MethodInfo)TypeHelper.GetGenericMethod(
                typeof(Queryable),
                methodName,
                delegateGenericParamaters,
                new[] { queryableType, expressionPredicateType },
                BindingFlags.Static);

            // Actually call the method.
            var call = Expression.Call(method, collectionAsQueryable, Expression.Constant(delegateExpression));
            return call;
        }

        private static void CheckTypesAreCompatable(
            Type source,
            Type target,
            PropertyInfo targetProperty,
            PropertyInfo sourceProperty)
        {
            // Check assignment from one property to another is possible.
            if (!sourceProperty.PropertyType.IsCastableTo(targetProperty.PropertyType))
            {
                throw new ArgumentException(
                    $"Property {source.FullName} {sourceProperty.PropertyType.Name} {sourceProperty.Name} type cannot be mapped to: {target.FullName} {targetProperty.PropertyType.Name} {targetProperty.Name}");
            }
        }

        /// <summary>
        /// Gets the source properties.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="scalarOnly">if set to <c>true</c> [scalar only].</param>
        /// <returns>
        /// The scalar properties.
        /// </returns>
        private static ICollection<PropertyInfo> GetSourceProperties(Type source, bool scalarOnly)
        {
            var sourceProperties = source.GetProperties()
                .Where(p => p != null)
                .Where(p => p.CanRead);
            if (scalarOnly)
            {
                sourceProperties =
                    sourceProperties.Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));
            }

            return sourceProperties.ToList();
        }

        private static Expression CreateSourceExpression<TSource, TTarget>(
            PropertyInfo targetProperty,
            PropertyInfo sourceProperty,
            Expression sourceParameter)
        {
            return CreateSourceExpression(
                typeof(TSource),
                typeof(TTarget),
                targetProperty,
                sourceProperty,
                sourceParameter);
        }

        private static Expression CreateSourceExpression(
            Type source,
            Type target,
            PropertyInfo targetProperty,
            PropertyInfo sourceProperty,
            Expression sourceParameter)
        {
            if (targetProperty.PropertyType == sourceProperty.PropertyType)
            {
                return Expression.Property(sourceParameter, sourceProperty);
            }

            CheckTypesAreCompatable(source, target, targetProperty, sourceProperty);
            Expression sourceExp = 
                Expression.Convert(
                    Expression.Property(sourceParameter, sourceProperty),
                    targetProperty.PropertyType);

            return sourceExp;
        }

        private static IEnumerable<PropertyPair> GetNameMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties)
        {
            var matches =
                from sProperty in sourceProperties
                where sProperty.CanRead
                join tProperty in targetProperties
                on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where tProperty.CanWrite
                select new PropertyPair { TargetProperty = tProperty, SourceProperty = sProperty };

            return matches;
        }

        private static IEnumerable<PropertyPair> GetMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties)
        {
            var matches =
                from match in GetNameMatchedProperties(sourceProperties, targetProperties)
                where match.SourceProperty.PropertyType.IsCastableTo(match.TargetProperty.PropertyType)
                select match;
            
            return matches;
        }

        private static IEnumerable<DefinedMappingPropertyPair> GetKnownProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties,
            ICollection<MappingData> mappingDataCollection)
        {
            var matches = GetNameMatchedProperties(sourceProperties, targetProperties);

            var knownMappings =
                from match in matches
                join mappingData in mappingDataCollection
                on new { Source = match.SourceProperty.PropertyType, Target = match.TargetProperty.PropertyType}
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

        private static MemberInfo GetMemberInfo(LambdaExpression propertyExpression)
        {            
            var body = propertyExpression.Body as MemberExpression;            
            if (body == null)
            {
                var ubody = propertyExpression.Body as UnaryExpression;
                body = ubody?.Operand as MemberExpression;
            }

            if (body == null)
            {
                throw new ArgumentException(
                    $"{nameof(propertyExpression)} must be a member expression. Expression {propertyExpression}",
                    nameof(propertyExpression));
            }

            return body.Member;
        }
        #endregion

        internal class PropertyRuleResult
        {
            public PropertyInfo Property { get; set; }

            public Expression Expression { get; set; }
        }
    }
}