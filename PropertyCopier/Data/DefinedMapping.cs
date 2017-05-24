using System;
using System.Linq.Expressions;

namespace PropertyCopier.Data
{
    /// <summary>
    /// A rule for mapping between two types.
    /// </summary>
    internal class DefinedMapping
    {
        public Type SourceType { get; set; }

        public Type TargetType { get; set; }

        public LambdaExpression Mapping { get; set; }
    }
}