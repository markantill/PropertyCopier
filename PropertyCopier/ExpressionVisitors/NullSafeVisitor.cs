using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PropertyCopier.ExpressionVisitors
{
    public class NullSafeVisitor : ExpressionVisitor
    {       
        protected override Expression VisitMember(MemberExpression node)
        {
            var returnType = node.Type;

            Expression stack = node;
            Expression safe = node;
            Expression defaultExp = !returnType.CanBeNull()
                ? Expression.Default(returnType)
                : (Expression)Expression.Convert(Expression.Constant(null), returnType);

            while (stack is MemberExpression)
            {
                var next = ((MemberExpression)stack).Expression;
                if (!IsNullSafe(stack))
                {
                    safe =
                        Expression.Condition
                        (
                            Expression.NotEqual(next, Expression.Constant(null)),
                            safe,
                            defaultExp
                        );
                }

                stack = next;
            }

            return safe;
        }

        private bool IsNullSafe(Expression expr)
        {
            var memberExpr = expr as MemberExpression;
            if (memberExpr != null)
            {                
                // Static fields can't have a a null parent;
                switch (memberExpr.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = (PropertyInfo)memberExpr.Member;
                        var getter = property.GetGetMethod();
                        if (getter.IsStatic)
                            return true;
                        break;
                    case MemberTypes.Method:
                        var field = (FieldInfo)memberExpr.Member;
                        if (field.IsStatic)
                            return true;
                        break;
                }

                var obj = memberExpr.Expression;                
                return !obj.Type.CanBeNull();                
            }

            return true;
        }
    }
}