namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

public static class IFilterStatementOptionsExtensions
{
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
