using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PropertyCopier.Generators
{
    internal class DefinedPropertyRulesGenerator : IExpressionGenerator
    {
        public ExpressionGeneratorResult GenerateExpressions(Expression sourceExpression, ICollection<PropertyInfo> targetProperties, MappingData mappingData)
        {
            var expressions = new List<PropertyAndExpression>();
            var matched = new List<PropertyInfo>();
          
            foreach (var propertyRule in mappingData.PropertyLambdaExpressions)
            {
                var predefined = GetPredefinedRules(propertyRule, sourceExpression);                
                matched.Add(predefined.Property);
                expressions.Add(new PropertyAndExpression(predefined.Property, predefined.Expression));
            }

            var newTargetProperties = targetProperties.Except(matched).ToArray();
            return new ExpressionGeneratorResult
            {
                Expressions = expressions,
                TargetProperties = newTargetProperties,
            };
        }

        private static PropertyAndExpression GetPredefinedRules(PropertyRule propertyLambdaExpression, Expression sourcExpression)
        {
            var targetProperty = (PropertyInfo)ExpressionBuilder.GetMemberInfo(propertyLambdaExpression.PropertyExpression);
            var visitor = new AddPropertyRuleExpressionVisitor(sourcExpression);
            var newExpression = (LambdaExpression)visitor.Visit(propertyLambdaExpression.MappingRule);
            return new PropertyAndExpression(targetProperty, newExpression.Body);
        }
    }
}
