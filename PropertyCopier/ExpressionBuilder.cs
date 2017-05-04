using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PropertyCopier.Generators;
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
            var sourceParameter = Expression.Parameter(source, nameof(source));

            var initializer = CreateLambdaInitializerBody(source, target, sourceParameter, mappingData);

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
            var targetProperties = typeof(TTarget).GetProperties();
            var exps = new List<Expression>();

            var generators = new IExpressionGenerator[]
            {
                new IgnoreTargetPropertiesGenerator(),
                new DefinedPropertyRulesGenerator(),
                new DefinedTypeRulesGenerator(),
                new MatchedPropertyNamesGenerator(),
                new FlattenedProperitesGenerator()                
            };

            ICollection<PropertyInfo> availableTargets = targetProperties;

            foreach (IExpressionGenerator expressionGenerator in generators)
            {
                var results = expressionGenerator.GenerateExpressions(sourceParameter, availableTargets, mappingData);

                foreach (var result in results.Expressions)
                {
                    var targetExp = Expression.Property(targetParameter, result.Property);
                    var setExp = Expression.Assign(targetExp, result.Expression);
                    exps.Add(setExp);
                }                
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
        /// Calls the specified <see cref="Enumerable"/> method that takes a collection and an expression. e.g. Select, Where etc.
        /// </summary>
        /// <param name="collection">The expression representing the collection.</param>
        /// <param name="predicate">The expression that will be run.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Expression representing calling the method.</returns>
        internal static Expression CallEnumerableMethod(
            Expression collection,
            Expression predicate,
            string methodName)
        {
            // Get the collections implementation of IEnumerable<T> so we can figure out what T is for it.
            var collectionType = GetIEnumerableImpl(collection.Type);

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
            
            var generators = new IExpressionGenerator[]
            {
                new IgnoreTargetPropertiesGenerator(),
                new DefinedPropertyRulesGenerator(), 
                new DefinedTypeRulesGenerator(), 
                new MatchedPropertyNamesGenerator(),
                new FlattenedProperitesGenerator(),
                new SingleChildObjectGenerator(),
                new ChildEnumerationGenerator(),
            };

            ICollection<PropertyInfo> availableTargets = targetProperties;

            foreach (IExpressionGenerator expressionGenerator in generators)
            {                
                var results = expressionGenerator.GenerateExpressions(sourceParameter, availableTargets, mappingData);
                var newBindings =
                    results.Expressions.Select(result => Expression.Bind(result.Property, result.Expression));
                bindings.AddRange(newBindings);

                availableTargets = results.TargetProperties;
            }        
                    
            // Create Expression for initialising object with correct values, the new MyClass part of the expression.            
            var initializer = Expression.MemberInit(Expression.New(target), bindings);
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
                GetGenericMethod(
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
            var method = (MethodInfo)GetGenericMethod(
                typeof(Queryable),
                methodName,
                delegateGenericParamaters,
                new[] { queryableType, expressionPredicateType },
                BindingFlags.Static);

            // Actually call the method.
            var call = Expression.Call(method, collectionAsQueryable, Expression.Constant(delegateExpression));
            return call;
        }

        internal static Expression CreateSourceExpression(
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