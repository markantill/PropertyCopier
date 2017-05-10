using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// Produces a set of expressions for target properties based on a rule. Also returns
    /// remaining target properties than the rule does not apply to.
    /// </summary>
    internal interface IExpressionGenerator
    {
        ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData);
    }
}
