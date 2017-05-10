using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyCopier.Generators
{
    internal class PropertyAndExpression
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