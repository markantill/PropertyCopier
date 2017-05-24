using System;
using System.Linq.Expressions;
using PropertyCopier.Data;

namespace PropertyCopier.StaticCaches
{
	/// <summary>
	/// Performs the creation of target type and population of its properties based on properties from source object
	/// where source property name matched target property name.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	internal static class PropertyCopier<TSource, TTarget>		
		where TTarget : new()
	{
		// Stores the delegate required to create a new object.
		// As this is compiled it is much faster than reflection.
	    internal static readonly Lazy<Expression<Func<TSource, TTarget>>> Expression =
            new Lazy<Expression<Func<TSource, TTarget>>>(
	            () => ExpressionBuilder.CreateLambdaInitializer(new MappingData<TSource,TTarget>()));

	    internal static readonly Lazy<Func<TSource, TTarget>> Copier =
	        new Lazy<Func<TSource, TTarget>>(
	            () => Expression.Value.Compile(), true);

		/// <summary>
		/// Copies from the source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>Creates a new instance of specified target type, copying property values from source.</returns>
		internal static TTarget From(TSource source)
		{
			var result = Copier.Value(source);
			return result;
		}

	    /// <summary>
	    /// Copies from the source.
	    /// </summary>
	    /// <param name="source">The source.</param>
	    /// <returns>Creates a new instance of specified target type, copying property values from source.</returns>
        [Obsolete("Use From instead")]
	    internal static TTarget CopyFrom(TSource source)
	    {
	        return From(source);
	    }
    }
}