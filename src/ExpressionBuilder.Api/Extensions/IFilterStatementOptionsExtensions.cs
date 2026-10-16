namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// Extensions for applying manipulators to a filter statement
/// </summary>
public static class IFilterStatementOptionsExtensions
{
    /// <summary>
    /// Apply manipulators to a value
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="options">The options to apply</param>
    /// <param name="value">The value to apply the manipulators to</param>
    /// <returns>The value with the manipulators applied</returns>
    public static object? ApplyManipulators<TPropertyType>(
        this IFilterStatementOptions? options,
        TPropertyType? value
    )
    {
        if (
            options == null
            || options.Manipulators == null
            || !options.Manipulators.Any()
        )
            return value;

        object? val = value;

        foreach (var manip in options.Manipulators)
            val = manip.ManipulateValue(val);

        return val;
    }

    /// <summary>
    /// Apply manipulators to a member
    /// </summary>
    /// <param name="options">The options to apply</param>
    /// <param name="value">The member to apply the manipulators to</param>
    /// <returns>The member with the manipulators applied</returns>
    public static Expression ApplyManipulators(
        this IFilterStatementOptions? options,
        Expression value
    )
    {
        var expr = value;

        if (
            options == null
            || options.Manipulators == null
            || !options.Manipulators.Any()
        )
            return expr;

        foreach (var manip in options.Manipulators)
            expr = manip.ManipulateMember(expr);

        return expr;
    }
}
