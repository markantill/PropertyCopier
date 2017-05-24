using System;
using System.Linq.Expressions;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Represents a property as an expression with a corresponding mapping rule.
    /// </summary>
    internal class PropertyRule
    {
        public LambdaExpression PropertyExpression { get; set; }

        public LambdaExpression MappingRule { get; set; }
    }
}