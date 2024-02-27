namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Linq.Expressions;
using H = Helpers.HelperMethods;

public static class ExpressionExtensions
{
    public static Expression WorkOnValues<TPropertyType>(
        this Expression member,
        IFilterCollection<TPropertyType?> values,
        Matches matches,
        Func<Expression, TPropertyType?, Expression> expression
    )
    {
        Expression? expr = null;

        foreach (var value in values)
            expr = expr.JoinExpression(
                expression.Invoke(member, value),
                matches
            );

        return expr ?? Expression.Constant(true);
    }

    public static Expression JoinExpression(
        this Expression? left,
        Expression right,
        Connector connector
    ) => left == null 
        ? right 
        : connector == Connector.And
            ? Expression.AndAlso(left, right)
            : Expression.OrElse(left, right);

    public static Expression JoinExpression(
        this Expression? left,
        Expression right,
        Matches matches
    ) =>
        left == null
            ? right
            : matches == Matches.Any
                ? Expression.OrElse(left, right)
                : Expression.AndAlso(left, right);

    public static Expression ToTrimLowerStringExpression(this Expression member)
        => Expression.Call(
            Expression.Call(
                member.ToStringExpression(), 
                H.TrimMethod
            ),
            H.ToLowerMethod
        );

    public static Expression ToTrimLowerStringConstant<TPropertyType>(this TPropertyType? item)
        => Expression.Constant(
            item?.ToString()?.ToLower().Trim()
        );

    public static Expression ToStringExpression(this Expression expression)
        => expression.Type == typeof(string)
            ? expression
            : Expression.Call(
                expression,
                expression.Type.GetMethod("ToString", Type.EmptyTypes)!
            );
}
