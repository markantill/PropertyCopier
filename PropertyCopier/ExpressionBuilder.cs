using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Linq.Expressions;

namespace PropertyCopier
{
    /// <summary>
    ///     Class for creating expression trees.
    /// </summary>
    internal static class ExpressionBuilder
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Creates the lambda initializer to create new object and select properties based on properties of source
        ///     type where the property names match.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="scalarOnly">if set to <c>true</c> copy scalar properties only.</param>
        /// <returns>Lambda expression to initialise object.</returns>
        internal static Expression<Func<TSource, TTarget>> CreateLambdaInitializer<TSource, TTarget>(
            bool scalarOnly = false)
        {
            // Were going to build an expression that looks like:
            // source => new Foo { Property1 = bar.Property1, Property2 = bar.Property2 }
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");

            var initializer = CreateLambdaInitializerBody(typeof(TSource), typeof(TTarget), scalarOnly, sourceParameter);

            // Create a Lambda expression from the parameter and body we have already created.
            var copyExpression = Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter);
            return copyExpression;
        }

        internal static LambdaExpression CreateLambdaInitializer(
            Type source,
            Type target,
            bool scalarOnly = false)
        {
            // Were going to build an expression that looks like:
            // source => new Foo { Property1 = bar.Property1, Property2 = bar.Property2 }
            var sourceParameter = Expression.Parameter(source, "source");

            var initializer = CreateLambdaInitializerBody(source, target, scalarOnly, sourceParameter);

            // Create a Lambda expression from the parameter and body we have already created.
            var copyExpression = Expression.Lambda(initializer, sourceParameter);
            return copyExpression;
        }

        /// <summary>
        ///     Creates the lambda property copier.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="scalarOnly">if set to <c>true</c> [scalar only].</param>
        /// <returns>Expression to copy properties with same name and type.</returns>
        internal static Expression<Func<TSource, TTarget, TTarget>> CreateLambdaPropertyCopier<TSource, TTarget>(
            bool scalarOnly = false)
        {
            var sourceParameter = typeof(TSource).Parameter("source");
            var targetParameter = typeof(TTarget).Parameter("target");
            var sourceProperties = GetSourceProperties(typeof(TSource), scalarOnly);

            // Copying properties is going to require building a statement (multi-line) lambda, 
            // each entry in the list will be one line of "code" in the statement.
            var exps = new List<Expression>();

            var matches = GetMatchedProperties(sourceProperties, typeof(TTarget).GetProperties());
            foreach (var match in matches)
            {
                var sourceExp = CreateSourceExpression<TSource, TTarget>(
                    match.TargetProperty,
                    match.SourceProperty,
                    sourceParameter);
                // Expressions will not do boxing or implicit conversions, so make sure the
                // type is explicitly cast to the destination type.
                var targetExp = Expression.Property(targetParameter, match.TargetProperty);
                var setExp = targetExp.Assign(sourceExp);
                exps.Add(setExp);
            }

            // Finally we want to return the result, there is no Return expression instead
            // just make the last line of the method body what you want to return.
            exps.Add(targetParameter);

            Expression block = exps.Block();
            var exp = Expression.Lambda<Func<TSource, TTarget, TTarget>>(
                block,
                sourceParameter,
                targetParameter);
            return exp;
        }

        /// <summary>
        ///     Creates the expression for nested properties in the string.
        /// </summary>
        /// <param name="startingExpression">The staring expression, for example the inital parameter.</param>
        /// <param name="propertyName">Name of the nested property e.g. "MyObject.MyProperty".</param>
        /// <param name="finalType">The final type to cast to.</param>
        /// <returns>Nested expressions with cast.</returns>
        /// <remarks>
        ///     Uses multiple return statements so that the last statement is the recursive call.
        ///     This means the compiler can optimise the recursion to tail recursion.
        /// </remarks>
        internal static Expression CreateNestedPropertyExpression(
            Expression startingExpression,
            string propertyName,
            Type finalType = null)
        {
            var split = propertyName.Split('.');
            var nextPropertyName = split.First();
            startingExpression = Expression.PropertyOrField(startingExpression, nextPropertyName);
            if (split.Length == 1)
            {
                if (finalType != null)
                {
                    startingExpression = startingExpression.Convert(finalType);
                }

                return startingExpression;
            }

            propertyName = string.Join(".", split.Skip(1));
            return CreateNestedPropertyExpression(startingExpression, propertyName, finalType);
        }

        /// <summary>
        ///     Calls the specified enumerable method that takes a collection and an expression. e.g. Select etc.
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
        /// <param name="scalarOnly">if set to <c>true</c> scalar properties only are copied.</param>
        /// <param name="sourceParameter">The source parameter.</param>
        /// <returns>Expression for lambda body.</returns>
        private static Expression CreateLambdaInitializerBody(
            Type source,
            Type target,
            bool scalarOnly,
            Expression sourceParameter)
        {
            // MemberBindings are going to be values inside the braces of the expression e.g. Property1 = source.Property1
            var bindings = new List<MemberBinding>();
            var sourceProperties = GetSourceProperties(source, scalarOnly).ToList();
            var targetProperties = target.GetProperties();
            var alreadyMatched = new HashSet<PropertyInfo>();

            // normal matches e.g. Foo.ID = Bar.ID
            var matches = GetMatchedProperties(sourceProperties, targetProperties);

            foreach (var match in matches)
            {
                if (match.TargetProperty.PropertyType.IsValueType || match.TargetProperty.PropertyType == typeof(string))
                {
                    var sourceExp = CreateSourceExpression(
                        source,
                        target,
                        match.TargetProperty,
                        match.SourceProperty,
                        sourceParameter);
                    bindings.Add(Expression.Bind(match.TargetProperty, sourceExp));
                    alreadyMatched.Add(match.TargetProperty);
                }
            }

            targetProperties = targetProperties.Except(alreadyMatched).ToArray();

            // nested scalar matches e.g. Foo.OwnerID = Bar.Owner.ID
            var joinedNames =
                from sProperty in sourceProperties
                from cProperty in sProperty.PropertyType.GetProperties()
                join tProperty in targetProperties
                    on sProperty.Name.ToUpperInvariant() + cProperty.Name.ToUpperInvariant()
                    equals tProperty.Name.ToUpperInvariant()
                where cProperty.PropertyType.IsCastableTo(tProperty.PropertyType)
                where sProperty.CanRead
                where cProperty.CanWrite
                select new { TargetProperty = tProperty, ChildProperty = cProperty, SourceProperty = sProperty };

            foreach (var joinedName in joinedNames)
            {
                var sourceEx = CreateNestedPropertyExpression(
                    sourceParameter.Property(joinedName.SourceProperty),
                    joinedName.ChildProperty.Name,
                    joinedName.TargetProperty.PropertyType);
                bindings.Add(Expression.Bind(joinedName.TargetProperty, sourceEx));
                alreadyMatched.Add(joinedName.TargetProperty);
            }

            // Nested Child objects e.g. Foo.Owner = new OwnerDto { ID = bar.Owner.ID, Name = bar.Owner.Name }
            var joinedObjects =
                from sProperty in sourceProperties
                join tProperty in target.GetProperties()
                    on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where sProperty.PropertyType != typeof(string)
                where tProperty.PropertyType != typeof(string)
                where !sProperty.PropertyType.IsValueType
                where !tProperty.PropertyType.IsValueType
                where sProperty.CanRead
                where tProperty.CanWrite
                where tProperty.PropertyType.GetConstructor(Type.EmptyTypes) != null
                select new { TargetProperty = tProperty, SourceProperty = sProperty };

            foreach (var joinedObject in joinedObjects)
            {
                var propExpression = CreateNestedPropertyExpression(
                    sourceParameter,
                    joinedObject.SourceProperty.Name);

                var exp = CreateLambdaInitializerBody(
                    joinedObject.SourceProperty.PropertyType,
                    joinedObject.TargetProperty.PropertyType,
                    scalarOnly,
                    propExpression
                    );
                bindings.Add(Expression.Bind(joinedObject.TargetProperty, exp));
                alreadyMatched.Add(joinedObject.TargetProperty);
            }

            // Child enumerations e.g. Foo.Children = Bar.Children.Select(barchild => new ChildDto { ID = barchild.ID }
            var enumerations =
                from sProperty in sourceProperties
                join tProperty in target.GetProperties()
                    on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where sProperty.PropertyType != typeof(string)
                where tProperty.PropertyType != typeof(string)
                where !sProperty.PropertyType.IsValueType
                where !tProperty.PropertyType.IsValueType
                where sProperty.CanRead
                where tProperty.CanWrite
                where typeof(IEnumerable).IsAssignableFrom(sProperty.PropertyType)
                where tProperty.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select new { TargetProperty = tProperty, SourceProperty = sProperty };

            bindings.AddRange(
                (from enumeration in enumerations
                    let propExpression =
                        CreateNestedPropertyExpression(sourceParameter, enumeration.SourceProperty.Name)
                    let enumerableSourceItemType = enumeration.SourceProperty.PropertyType.GetGenericArguments().First()
                    let enumerableTargetItemType = enumeration.TargetProperty.PropertyType.GetGenericArguments().First()
                    let childInitializser =
                        CreateLambdaInitializer(enumerableSourceItemType, enumerableTargetItemType, scalarOnly)
                    let selectCall = CallEnumerableMethod(propExpression, childInitializser, "Select")
                    select Expression.Bind(enumeration.TargetProperty, selectCall)));

            // Create Expression for initialising object with correct values, the new MyClass part of the expression.            
            var initializer = target.New().MemberInit(bindings);
            return initializer;
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
                    "AsQueryable",
                    new[] { elementType },
                    new[] { collectionType },
                    BindingFlags.Static);

            // Apply the AsQueryable method. We need to do this so we have a method that we can
            // pass an expression into that we can build up. If it stays as as IEnumerable we would need to 
            // pass in a delgate not an expression and that wouldn't work if were working with Linq to Entites or similar.
            var collectionAsQueryable = asQueryableMethod.Call(collectionExpression);

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
            return method.Call(collectionAsQueryable, Expression.Constant(delegateExpression));
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
                    string.Format(
                        "Property {0} {1} {2} type cannot be mapped to: {3} {4} {5}",
                        source.FullName,
                        sourceProperty.PropertyType.Name,
                        sourceProperty.Name,
                        target.FullName,
                        targetProperty.PropertyType.Name,
                        targetProperty.Name));
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
        private static IEnumerable<PropertyInfo> GetSourceProperties(Type source, bool scalarOnly)
        {
            var sourceProperties = source.GetProperties()
                .Where(p => p != null)
                .Where(p => p.CanRead);
            if (scalarOnly)
            {
                sourceProperties =
                    sourceProperties.Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string));
            }

            return sourceProperties;
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
            CheckTypesAreCompatable(source, target, targetProperty, sourceProperty);
            Expression sourceExp = Expression.Property(sourceParameter, sourceProperty)
                .Convert(targetProperty.PropertyType);

            return sourceExp;
        }

        private static IEnumerable<TypePair> GetMatchedProperties(
            IEnumerable<PropertyInfo> sourceProperties,
            IEnumerable<PropertyInfo> targetProperties)
        {
            var matches =
                from sProperty in sourceProperties
                where sProperty.CanRead
                join tProperty in targetProperties
                    on sProperty.Name.ToUpperInvariant() equals tProperty.Name.ToUpperInvariant()
                where tProperty.CanWrite
                where sProperty.PropertyType.IsCastableTo(tProperty.PropertyType)
                select new TypePair { TargetProperty = tProperty, SourceProperty = sProperty };

            return matches;
        }

        internal class TypePair
        {
            internal PropertyInfo TargetProperty { get; set; }
            internal PropertyInfo SourceProperty { get; set; }
        }

        #endregion
    }
}