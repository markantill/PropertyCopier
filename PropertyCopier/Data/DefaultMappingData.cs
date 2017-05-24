using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PropertyCopier.Data
{
    /// <summary>
    /// Default values for mapping data with no special rules applied.
    /// </summary>
    internal class DefaultMappingData : MappingData
    {
        public DefaultMappingData(Type sourceType, Type targetType, ICollection<MappingData> knownMappings)
        {
            SourceType = sourceType;
            TargetType = targetType;
            KnownMappings = knownMappings;
            AssignedMappingsExpressions  = new List<PropertyRule>();
            PropertyExpressions = new List<PropertyRule>();
        }

        public override Type SourceType { get; }

        public override Type TargetType { get; }        

        public override IEnumerable<LambdaExpression> PropertyIgnoreLambdaExpressions { get; } = new List<LambdaExpression>();        

        public override LambdaExpression InitializerExpression { get; } 

        public override ICollection<MappingData> KnownMappings { get; }
    }
}