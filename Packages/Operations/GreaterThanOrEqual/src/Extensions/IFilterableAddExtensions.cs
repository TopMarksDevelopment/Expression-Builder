namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableGreaterThanOrEqualExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterable GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection GreaterThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

    public static IFilterableConnection GreaterThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options
        );

    public static IFilterableConnection GreaterThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new GreaterThanOrEqual(), value, null);

    public static IFilterableConnection GreaterThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new GreaterThanOrEqual(), [value]);

    public static IFilterable GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilterable GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterable GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

    public static IFilterableConnection GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options
        );

    public static IFilterableConnection GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(propertyExpression, new GreaterThanOrEqual(), value, null);

    public static IFilterableConnection GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value
    ) =>
        filter.Add(propertyExpression, new GreaterThanOrEqual(), [value], null);

    public static IFilterable GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilterable GreaterThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
