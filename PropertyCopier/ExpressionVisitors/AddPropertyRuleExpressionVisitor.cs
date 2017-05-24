using System;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyCopier.ExpressionVisitors
{
    /// <summary>
    /// Replaces a parameter expression in a rule with the parameter for the source.
    /// e.g. Given the rule x => x.Count * 2 and the parameter "source" will rewrite is to
    /// x => source.Count * 2. The parameters part of the lambda are left unchanged as they
    /// will be discarded.
    /// </summary>
    internal class AddPropertyRuleExpressionVisitor : ExpressionVisitor
    {
        private bool _firstCall = true;
        private readonly Expression _sourceParameter;        
        private ParameterExpression _originalSourceParmeter;        

        public AddPropertyRuleExpressionVisitor(Expression sourceParameter)
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
                var lambdaExpression = node as LambdaExpression;
                _firstCall = false;                
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

                // We don't actually care about the parameters since only the body will be used so
                // just leave them unchanged.
                return Expression.Lambda(base.Visit(lambdaExpression.Body), lambdaExpression.Parameters);
            }

            return base.Visit(node);
        }
    }
}