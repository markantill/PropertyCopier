using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PropertyCopier.Data;
using PropertyCopier.ExpressionVisitors;

namespace PropertyCopier.Generators
{
    /// <summary>
    /// If we have any specific rules in the mapping data for specific properties use them.
    /// </summary>
    internal class DefinedPropertyRulesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(
            Expression sourceExpression,
            ICollection<PropertyInfo> targetProperties,
            MappingData mappingData,
            IEqualityComparer<string> memberNameComparer)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();
          
            foreach (var propertyRule in mappingData.PropertyExpressions)
            {
                var predefined = GetPredefinedRules(propertyRule, sourceExpression);                
                matched.Add(predefined.Property);
                expressions.Add(new PropertyAndExpression(predefined.Property, predefined.Expression));
            }

            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                UnmappedTargetProperties = newTargetProperties,
            };
        }

        public IEqualityComparer<string> MemberNameComparer { get; set; }

        private static PropertyAndExpression GetPredefinedRules(PropertyRule propertyLambdaExpression, Expression sourcExpression)
        {
            var targetProperty = (PropertyInfo)ExpressionBuilder.GetMemberInfo(propertyLambdaExpression.PropertyExpression);
            var visitor = new AddPropertyRuleExpressionVisitor(sourcExpression);
            var newExpression = (LambdaExpression)visitor.Visit(propertyLambdaExpression.MappingRule);
            return new PropertyAndExpression(targetProperty, newExpression.Body);
        }
    }
}
