namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Extensions;

/// <summary>
/// Extensions for the expression a <see cref="Filter{TClass}"/> makes.
/// </summary>
public static class FilterExpressionExtensions
{
    /// <summary>
    /// Writes the values a filter compares with into its expression as constants.
    /// </summary>
    /// <remarks>
    /// <see cref="Filter{TClass}.ToExpression"/> keeps those values in variables, so that a database provider sends them
    /// as parameters. Constants are easier to read (<c>x.Name == "a"</c> rather than <c>x.Name == value(…).Value</c>) and
    /// to compare, but make a provider such as Entity Framework Core treat every different value as a different query.
    /// </remarks>
    /// <typeparam name="TClass">The class being filtered</typeparam>
    /// <param name="expression">The expression of a filter</param>
    /// <returns>An expression that means the same, with constants</returns>
    public static Expression<Func<TClass, bool>> InlineValues<TClass>(
        this Expression<Func<TClass, bool>> expression
    )
        where TClass : class
    {
        ArgumentNullException.ThrowIfNull(expression);

        return ValueParameters.Inline(expression);
    }
}
