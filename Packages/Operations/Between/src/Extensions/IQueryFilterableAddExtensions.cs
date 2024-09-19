namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterableBetweenExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, new Between(), value, options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterable Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new Between(),
            [value, value2],
            options,
            connector
        );

    public static IQueryFilterableConnection Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new Between(), value, options);

    public static IQueryFilterableConnection Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new Between(), [value, value2], options);

    public static IQueryFilterableConnection Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new Between(), value, null);

    public static IQueryFilterableConnection Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    )
        where TClass : class =>
        filter.Add(propertyExpression, new Between(), [value, value2], null);

    public static IQueryFilterable Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, new Between(), value, null, connector);

    public static IQueryFilterable Between<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new Between(),
            [value, value2],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(propertyExpression, new Between(), value, options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterable Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new Between(),
            [value, value2],
            options,
            connector
        );

    public static IQueryFilterableConnection Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(propertyExpression, new Between(), value, options);

    public static IQueryFilterableConnection Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    ) =>
        filter.Add(propertyExpression, new Between(), [value, value2], options);

    public static IQueryFilterableConnection Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(propertyExpression, new Between(), value, null);

    public static IQueryFilterableConnection Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    ) => filter.Add(propertyExpression, new Between(), [value, value2], null);

    public static IQueryFilterable Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(propertyExpression, new Between(), value, null, connector);

    public static IQueryFilterable Between<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new Between(),
            [value, value2],
            null,
            connector
        );

    #endregion string propertyExpression
}
