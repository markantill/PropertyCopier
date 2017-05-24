using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Data;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// Produces a set of expressions for target properties based on a rule. Also returns
    /// remaining target properties than the rule does not apply to.
    /// </summary>
    internal interface IExpressionGenerator
    {
        /// <summary>
        /// Generate appropriate expressions with the rules provided.
        /// </summary>
        /// <param name="sourceExpression">The source expression.</param>
        /// <param name="targetProperties">The target properties to inspect.</param>
        /// <param name="mappingData">The mapping data rules/</param>
        /// <param name="memberNameComparer">The comparer to use for matching names.</param>
        /// <returns>Any expressions generated and any target properties still un-mapped.</returns>
        ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer);        
    }
}
