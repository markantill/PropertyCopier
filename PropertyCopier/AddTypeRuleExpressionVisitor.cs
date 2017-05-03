using System;
using System.Linq;
using System.Linq.Expressions;

namespace PropertyCopier
{
    public class AddTypeRuleExpressionVisitor : ExpressionVisitor
    {
        private bool _firstCall = true;
        private readonly Expression _sourceProperty;        
        private ParameterExpression _originalSourceParmeter;        

        public AddTypeRuleExpressionVisitor(Expression sourceProperty)
        {
            _sourceProperty = sourceProperty;            
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
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