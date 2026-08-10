using Application.Services.CommonSrv.SearchSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Common.Helpers
{
    public static class SearchQueryHelper
    {
        private static readonly MethodInfo ContainsMethod = typeof(string)
            .GetMethod(nameof(string.Contains), [typeof(string)])!;

        public static Expression<Func<TEntity, bool>> ContainsAny<TEntity>(
            IEnumerable<string> terms,
            params Expression<Func<TEntity, string>>[] selectors)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "item");
            Expression body = Expression.Constant(false);

            foreach (var term in terms.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().Take(20))
            {
                foreach (var selector in selectors)
                {
                    var value = new ReplaceParameterVisitor(selector.Parameters[0], parameter).Visit(selector.Body)!;
                    var notNull = Expression.NotEqual(value, Expression.Constant(null, typeof(string)));
                    var contains = Expression.Call(value, ContainsMethod, Expression.Constant(term));
                    body = Expression.OrElse(body, Expression.AndAlso(notNull, contains));
                }
            }

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        public static int CandidateCount(int requestedCount) =>
            Math.Min(Math.Max(requestedCount * 4, requestedCount), SearchRequestDto.MaxPerTypeCount * 3);

        public static Expression<Func<TEntity, bool>> Or<TEntity>(
            Expression<Func<TEntity, bool>> left,
            Expression<Func<TEntity, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "item");
            var leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body)!;
            var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;
            return Expression.Lambda<Func<TEntity, bool>>(Expression.OrElse(leftBody, rightBody), parameter);
        }

        private sealed class ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
        }
    }
}
