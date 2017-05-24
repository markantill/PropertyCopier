using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Holds a property and related expression the represents a rules associated with it.
    /// </summary>
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