using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class IgnoreTargetPropertiesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData)
        {
            var alreadyMatched = mappingData.PropertyIgnoreLambdaExpressions == null
                ? new HashSet<PropertyInfo>()
                : new HashSet<PropertyInfo>(mappingData.PropertyIgnoreLambdaExpressions.Select(ExpressionBuilder.GetMemberInfo)
                    .OfType<PropertyInfo>());

            var newTargetProperties = targetProperties.Except(alreadyMatched, new PropertyInfoComparer()).ToArray();

            return new ExpressionGeneratorResult
            {
                TargetProperties = newTargetProperties,
                Expressions = new List<PropertyAndExpression>(),
            };
        }
    }
}
