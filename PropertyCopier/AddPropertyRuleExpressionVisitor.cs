using System;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyCopier
{
    public class AddPropertyRuleExpressionVisitor : ExpressionVisitor
    {
        private bool _firstCall = true;
        private readonly ParameterExpression _sourceParameter;        
        private ParameterExpression _originalSourceParmeter;        

        public AddPropertyRuleExpressionVisitor(ParameterExpression sourceParameter)
        {
            _sourceParameter = sourceParameter;            
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {

            if (node == _originalSourceParmeter)
            {
                return _sourceParameter;
            }

            return node;
        }

        public override Expression Visit(Expression node)
        {
            if (_firstCall)
            {
                _firstCall = false;
                var lambdaExpression = node as LambdaExpression;
                if (lambdaExpression == null)
                {
                    throw new ArgumentException($"{nameof(node)} must be of type {nameof(LambdaExpression)}",
                        nameof(node));
                }

                var parameters = lambdaExpression.Parameters;
                if (parameters.Count != 1)
                {
                    throw new ArgumentException($"{nameof(node)} must have exactly one parameters");
                }

                _originalSourceParmeter = parameters.Single();

            }

            return base.Visit(node);
        }
    }
}