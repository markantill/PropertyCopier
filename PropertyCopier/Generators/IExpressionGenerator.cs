using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal interface IExpressionGenerator
    {
        ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData);
    }

    internal class ExpressionGeneratorResult
    {
        public ICollection<PropertyAndExpression> Expressions { get; set; }

        public ICollection<PropertyInfo> TargetProperties { get; set; }
    }

    public class PropertyAndExpression
    {
        public PropertyAndExpression(PropertyInfo property, Expression expression)
        {
            Property = property;
            Expression = expression;
        }

        public Expression Expression { get; set; }

        public PropertyInfo Property { get; set; }
    }
}
