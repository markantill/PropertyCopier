using System;
using System.Collections.Generic;
using System.Reflection;

namespace PropertyCopier.Generators
{
    internal class ExpressionGeneratorResult
    {
        public ICollection<PropertyAndExpression> Expressions { get; set; }

        public ICollection<PropertyInfo> TargetProperties { get; set; }
    }
}