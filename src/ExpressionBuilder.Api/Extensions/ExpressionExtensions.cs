namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;
using H = Helpers.HelperMethods;

/// <summary>
/// Extension methods for expressions
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Apply the work on all the values
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property</typeparam>
    /// <param name="member">The member to apply to</param>
    /// <param name="values">The values to apply operation to</param>
    /// <param name="matches">How we match multiple values</param>
    /// <param name="expression">The expression/operation we're applying</param>
    /// <returns>An expression representing the full operation</returns>
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

    /// <summary>
    /// Join the left and right expressions with a connector
    /// </summary>
    /// <param name="left">The left hand side of the join</param>
    /// <param name="right">The right hand side of the join</param>
    /// <param name="connector">The connector we're using to join the two expressions</param>
    /// <returns>A new expression representing the two joined expressions</returns>
    public static Expression JoinExpression(
        this Expression? left,
        Expression right,
        Connector connector
    ) =>
        left == null ? right
        : connector == Connector.And ? Expression.AndAlso(left, right)
        : Expression.OrElse(left, right);

    /// <summary>
    /// Join the left and right expressions based on the Match type
    /// </summary>
    /// <param name="left">The left hand side of the join</param>
    /// <param name="right">The right hand side of the join</param>
    /// <param name="matches">The type of match we're doing to join the expressions</param>
    /// <returns>A new expression representing the two joined expressions</returns>
    public static Expression JoinExpression(
        this Expression? left,
        Expression right,
        Matches matches
    ) =>
        left == null ? right
        : matches == Matches.Any ? Expression.OrElse(left, right)
        : Expression.AndAlso(left, right);

    /// <summary>
    /// Convert the member to a string, trim it, and make it lowercase
    /// </summary>
    /// <param name="member">The member to convert</param>
    /// <returns>An expression representing the full operation</returns>
    public static Expression ToTrimLowerStringExpression(
        this Expression member
    ) =>
        Expression.Call(
            Expression.Call(member.ToStringExpression(), H.TrimMethod),
            H.ToLowerMethod
        );

    /// <summary>
    /// Convert the member to a string, trim it, and make it lowercase
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property</typeparam>
    /// <param name="item">The item to convert</param>
    /// <returns>An expression representing the full operation</returns>
    public static Expression ToTrimLowerStringConstant<TPropertyType>(
        this TPropertyType? item
    ) => Expression.Constant(item?.ToString()?.ToLower().Trim());

    /// <summary>
    /// Convert the member to a string
    /// </summary>
    /// <param name="expression">The expression to convert</param>
    /// <returns>An expression representing the converted expression</returns>
    public static Expression ToStringExpression(this Expression expression) =>
        expression.Type == typeof(string)
            ? expression
            : Expression.Call(
                expression,
                expression.Type.GetMethod("ToString", Type.EmptyTypes)!
            );
}
