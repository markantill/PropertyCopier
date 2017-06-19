using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using PropertyCopier.Comparers;
using PropertyCopier.Data;
using PropertyCopier.ExpressionVisitors;
using PropertyCopier.Generators;
using static System.Linq.Expressions.Expression;
using static PropertyCopier.TypeHelper;

namespace PropertyCopier
{    
    /// <summary>
    /// Class for creating expression trees.
    /// </summary>
    internal static class ExpressionBuilder
    {
        /// <summary>
        /// Creates the lambda initializer to create new object and select properties based on properties of source
        /// type where the property names match.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target.</typeparam>
        /// <param name="mappingData">Optional mapping data to be applied.</param>
        /// <returns>Lambda expression to initialise object.</returns>
        internal static Expression<Func<TSource, TTarget>> CreateLambdaInitializer<TSource, TTarget>(            
            MappingData<TSource, TTarget> mappingData)
        {
            return (Expression<Func<TSource, TTarget>>) CreateLambdaInitializer(
                typeof(TSource), 
                typeof(TTarget),
                mappingData);
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
            MappingData mappingData)
        {
            // Were going to build an expression that looks like:
            // source => new Foo { Property1 = bar.Property1, Property2 = bar.Property2 }
            var sourceParameter = Parameter(source, nameof(source));

            var initializer = CreateLambdaInitializerBody(source, target, sourceParameter, mappingData);

            // Create a Lambda expression from the parameter and body we have already created.
            var copyExpression = Lambda(initializer, sourceParameter);


            if (mappingData.AddNullChecking)
            {
                var visitor = new NullSafeVisitor();
                copyExpression = (LambdaExpression)visitor.Visit(copyExpression);
            }

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
            var sourceParameter = Parameter(typeof(TSource), "source");
            var targetParameter = Parameter(typeof(TTarget), "target");            
            var targetProperties = typeof(TTarget).GetProperties();
            var exps = new List<Expression>();
            var comparer = CreateComparer(mappingData);

            var generators = new List<IExpressionGenerator>
            {                
                new IgnoreTargetPropertiesGenerator(),
                new DefinedPropertyRulesGenerator(),
                new DefinedTypeRulesGenerator(),
                new MatchedPropertyNamesGenerator()                              
            };

            AddIfTrue(mappingData.FlattenChildObjects, generators, new FlattenedProperitesGenerator());
            AddIfTrue(mappingData.MapChildObjects, generators, new SingleChildObjectGenerator());
            AddIfTrue(mappingData.MapChildEnumerations, generators, new ChildEnumerationGenerator());
            AddIfTrue(mappingData.MapChildCollections, generators, new ChildCollectionGenerator());

            ICollection<PropertyInfo> availableTargets = targetProperties;

            foreach (IExpressionGenerator expressionGenerator in generators)
            {
                var results = expressionGenerator.GenerateExpressions(sourceParameter, availableTargets, mappingData, comparer);

                foreach (var result in results.Expressions)
                {
                    var targetExp = Property(targetParameter, result.Property);
                    var setExp = Assign(targetExp, result.Expression);
                    exps.Add(setExp);
                }                
            }

            // Finally we want to return the result, there is no Return expression instead
            // just make the last line of the method body what you want to return.
            exps.Add(targetParameter);

            Expression block = Block(exps);
            var exp = Lambda<Func<TSource, TTarget, TTarget>>(
                block,
                sourceParameter,
                targetParameter);
            return exp;
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
                startingExpression = PropertyOrField(startingExpression, nextPropertyName);
                if (split.Length == 1)
                {
                    if (finalType != null)
                    {
                        startingExpression = Convert(startingExpression, finalType);
                        startingExpression = Convert(startingExpression, finalType);
                    }

                    return startingExpression;
                }

                propertyName = string.Join(".", split.Skip(1));
            }
        }

        /// <summary>
        /// Calls the specified <see cref="Enumerable"/> method that takes a collection and an expression. e.g. Select, Where etc.
        /// </summary>
        /// <param name="collection">The expression representing the collection.</param>
        /// <param name="predicate">The expression that will be run.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="asQueryable">If true the collection will be converted to IQueryable before applying the predicate. This allows it to work
        /// with things like Linq to Entities.</param>
        /// <returns>Expression representing calling the method.</returns>
        internal static Expression CallEnumerableMethod(
            Expression collection,
            LambdaExpression predicate,
            string methodName,
            bool asQueryable = true)
        {
            // Get the collections implementation of IEnumerable<T> so we can figure out what T is for it.
            var collectionType = GetIEnumerableImpl(collection.Type);

            // Cast the collection to the IEnumerable<T> just for safety.            
            collection = Convert(collection, collectionType);

            // Get the type of the element in the collection, T.
            var elemType = collectionType.GetGenericArguments()[0];

            // Get arg1, arg2 from Expression<Func<arg1, arg2>>
            var expTypes = predicate.GetType().GetGenericArguments().First().GetGenericArguments().ToArray();

            // Figure out what the type of the predicate must be, it must be Func<T, bool>
            var predicateType = typeof(Func<,>).MakeGenericType(expTypes);

            if (asQueryable)
            {
                // Generate the Call Expressions
                return GenerateIQueryableCallExpression(
                    collection,
                    predicate,
                    methodName,
                    predicateType,
                    elemType,
                    collectionType,
                    expTypes);
            }
            else
            {
                return GenerateIEnumerableCallExpression(
                    collection,
                    predicate,
                    methodName,
                    predicateType,
                    elemType,
                    collectionType,
                    expTypes);
            }
        }

        /// <summary>
        /// Creates the lambda initializer body.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="sourceParameter">The source parameter.</param>
        /// <param name="mappingData">The mapping data.</param>
        /// <returns>Expression for lambda body.</returns>
        internal static Expression CreateLambdaInitializerBody(
            Type source,
            Type target,
            Expression sourceParameter,
            MappingData mappingData)
        {
            // MemberBindings are going to be values inside the braces of the expression e.g. Property1 = source.Property1
            var bindings = new List<MemberBinding>();
            var targetProperties = target.GetProperties();

            var comparer = CreateComparer(mappingData);

            var generators = new List<IExpressionGenerator>
            {
                new IgnoreTargetPropertiesGenerator(),
                new DefinedPropertyRulesGenerator(),
                new DefinedTypeRulesGenerator(),
                new MatchedPropertyNamesGenerator(),
            };

            AddIfTrue(mappingData.FlattenChildObjects, generators, new FlattenedProperitesGenerator());
            AddIfTrue(mappingData.MapChildObjects, generators, new SingleChildObjectGenerator());
            AddIfTrue(mappingData.MapChildEnumerations, generators, new ChildEnumerationGenerator());
            AddIfTrue(mappingData.MapChildCollections, generators, new ChildCollectionGenerator());

            ICollection<PropertyInfo> availableTargets = targetProperties;

            foreach (IExpressionGenerator expressionGenerator in generators)
            {
                var results =
                    expressionGenerator.GenerateExpressions(sourceParameter, availableTargets, mappingData, comparer);
                var newBindings =
                    results.Expressions.Select(result => Bind(result.Property, result.Expression));
                bindings.AddRange(newBindings);

                availableTargets = results.UnmappedTargetProperties;
            }

            // Create Expression for initialising object with correct values, the new MyClass part of the expression.                        
            var initializer = MemberInit(New(target), bindings);
            return initializer;
        }

        private static void AddIfTrue(
            bool condition,
            ICollection<IExpressionGenerator> expressionGenerators,
            IExpressionGenerator expressionGenerator)
        {
            if (condition)
            {
                expressionGenerators.Add(expressionGenerator);
            }
        }

        private static IEqualityComparer<string> CreateComparer(MappingData mappingData)
        {
            var comparer = new PropertyNameComparer(mappingData.Comparer);
            foreach (var map in mappingData.AssignedMappingsExpressions)
            {
                var targetMemberInfo = GetMemberInfo(map.PropertyExpression);
                var sourceMemberInfo = GetMemberInfo(map.MappingRule);
                comparer.AddMapping(targetMemberInfo.Name, sourceMemberInfo.Name);
            }

            return comparer;
        }

        internal static LambdaExpression StripUnwantedObjectCast(Type desiredReturnType, LambdaExpression lambdaExpression)
        {            
            LambdaExpression result = lambdaExpression;

            // Check if we have an unwanted cast to object put in by the compiler
            // if so make a new expression that strips it out            
            if (desiredReturnType != typeof(object))
            {
                var body = lambdaExpression.Body;
                var unary = body as UnaryExpression;
                if (unary != null && unary.NodeType == ExpressionType.Convert && unary.Type == typeof(object))
                {
                    var newBody = unary.Operand;
                    result = Lambda(
                        newBody,
                        lambdaExpression.Parameters);                    
                }
            }

            return result;            
        }

        private static Expression GenerateIQueryableCallExpression(
            Expression ienumerableExpression,
            LambdaExpression delegateExpression,
            string methodName,
            Type delegateType,
            Type elementType,
            Type collectionType,
            Type[] delegateGenericParamaters)
        {
            // Figure out what the type of the expression representing the predicate must be,
            // Expression<Func<T, bool>>
            var expressionPredicateType = typeof(Expression<>).MakeGenericType(delegateType);

            // Get the Queryable.AsQueryable method for the collection expressions.
            var asQueryableMethod = (MethodInfo)
                GetGenericMethod(
                    typeof(Queryable),
                    nameof(Queryable.AsQueryable),
                    new[] { elementType },
                    new[] { collectionType },
                    BindingFlags.Static);

            // Apply the AsQueryable method. We need to do this so we have a method that we can
            // pass an expression into that we can build up. If it stays as IEnumerable we would need to 
            // pass in a delegate not an expression and that wouldn't work if were working with Linq to Entities or similar.
            var collectionAsQueryable = Call(asQueryableMethod, ienumerableExpression);

            // Figure out they type now it is an IQueryable<T>.
            var queryableType = typeof(IQueryable<>).MakeGenericType(elementType);

            // Get our actual method to call, signature is Queryable.[methodName]<T>(IQueryable<T>, Expression<Func<T,bool>>)
            var method = (MethodInfo)GetGenericMethod(
                typeof(Queryable),
                methodName,
                delegateGenericParamaters,
                new[] { queryableType, expressionPredicateType },
                BindingFlags.Static);

            // Actually call the method.
            var call = Call(method, collectionAsQueryable, Constant(delegateExpression));
            return call;
        }

        private static Expression GenerateIEnumerableCallExpression(
            Expression ienumerableExpression,
            LambdaExpression delegateExpression,
            string methodName,
            Type delegateType,
            Type elementType,
            Type collectionType,
            Type[] delegateGenericParamaters)
        {
            var enumerableType = GetIEnumerableImpl(collectionType);

            // Get our actual method to call, signature is Queryable.[methodName]<T>(IQueryable<T>, Expression<Func<T,bool>>)
            var method = (MethodInfo)GetGenericMethod(
                typeof(Enumerable),
                methodName,
                delegateGenericParamaters,
                new[] { enumerableType, delegateType },
                BindingFlags.Static);

            var func = delegateExpression.Compile();

            // Actually call the method.
            var call = Call(method, ienumerableExpression, Constant(func));
            return call;
        }

        /// <summary>
        /// Create an expression for the property on the source object.
        /// Add in a cast if required.
        /// </summary>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <param name="sourceExpression">The source expression.</param>
        /// <returns></returns>
        internal static Expression CreateSourceExpression(            
            PropertyInfo targetProperty,
            PropertyInfo sourceProperty,
            Expression sourceExpression)
        {
            if (targetProperty.PropertyType == sourceProperty.PropertyType)
            {
                return Property(sourceExpression, sourceProperty);
            }

            CheckTypesAreCompatable(targetProperty, sourceProperty);
            Expression sourceExp = 
                Convert(
                    Property(sourceExpression, sourceProperty),
                    targetProperty.PropertyType);

            return sourceExp;
        }

        /// <summary>
        /// Get the information on the member represented by property expression.
        /// </summary>
        /// <param name="propertyExpression">The expression representing the property.</param>
        /// <returns>The <see cref="MemberInfo"/> of the property.</returns>
        internal static MemberInfo GetMemberInfo(LambdaExpression propertyExpression)
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
    }
}