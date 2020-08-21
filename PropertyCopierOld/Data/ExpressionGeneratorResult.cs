using System;
using System.Collections.Generic;
using System.Reflection;
using PropertyCopier.Generators;

namespace PropertyCopier.Data
{
    /// <summary>
    /// The output of an <see cref="IExpressionGenerator"/>.
    /// </summary>
    internal class ExpressionGeneratorResult
    {
        /// <summary>
        /// The mapping expressions generated.
        /// </summary>
        public ICollection<PropertyAndExpression> Expressions { get; set; } = new List<PropertyAndExpression>();

        /// <summary>
        /// The target properties that remain unmapped.
        /// </summary>
        public ICollection<PropertyInfo> UnmappedTargetProperties { get; set; } = new List<PropertyInfo>();
    }
}