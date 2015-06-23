using System;
using System.Linq.Expressions;

namespace PropertyCopier
{
	/// <summary>
	/// Performs the creation of target type and population of its properties based on properties from source object
	/// where source property name is identical to target property name.
	/// </summary>
	/// <typeparam name="TSource">The type of the source.</typeparam>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	internal static class PropertyCopier<TSource, TTarget>
		where TSource : class
		where TTarget : class, new()
	{
		// Stores the delegate required to create a new object.
		// As this is compiled it is much faster than reflection.
		#region Static Fields

	    internal static readonly Lazy<Expression<Func<TSource, TTarget>>> Expression =
            new Lazy<Expression<Func<TSource, TTarget>>>(
	            () => ExpressionBuilder.CreateLambdaInitializer<TSource, TTarget>(),
	            true);

        internal static readonly Lazy<Func<TSource, TTarget>> Copier =
			new Lazy<Func<TSource, TTarget>>(
				() => Expression.Value.Compile(), true);

		#endregion

		#region Methods

		/// <summary>
		/// Copies from the source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>Creates a new instance of specified target type, copying property values from source.</returns>
		internal static TTarget CopyFrom(TSource source)
		{
			var result = Copier.Value(source);
			return result;
		}

		#endregion
	}
}