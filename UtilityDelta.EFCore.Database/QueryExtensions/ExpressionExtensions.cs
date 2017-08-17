using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UtilityDelta.EFCore.Database.QueryExtensions
{
    public static class ExpressionExtensions
    {
        private const string ParameterX = "x";

        public static Expression<Func<T, bool>> Combine<T, TNav>(this Expression<Func<T, TNav>> parent, Expression<Func<TNav, bool>> nav)
        {
            var param = Expression.Parameter(typeof(T), ParameterX);
            var visitor = new ReplacementVisitor(parent.Parameters[0], param);
            var newParentBody = visitor.Visit(parent.Body);
            visitor = new ReplacementVisitor(nav.Parameters[0], newParentBody);
            var body = visitor.Visit(nav.Body);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        private class ReplacementVisitor : ExpressionVisitor
        {
            private readonly Expression m_oldExpr;
            private readonly Expression m_newExpr;

            public ReplacementVisitor(Expression oldExpr, Expression newExpr)
            {
                m_oldExpr = oldExpr;
                m_newExpr = newExpr;
            }

            public override Expression Visit(Expression node) => 
                node == m_oldExpr ? m_newExpr : base.Visit(node);
        }
    }
}
