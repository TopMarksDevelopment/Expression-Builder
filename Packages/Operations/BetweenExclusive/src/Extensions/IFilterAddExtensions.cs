namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterBetweenExclusiveExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilter BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new BetweenExclusive(), value, options);

    public static IFilterConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new BetweenExclusive(), value, null);

    public static IFilterConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null
        );

    public static IFilter BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilter BetweenExclusive<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilter BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilter BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterConnection BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(propertyExpression, new BetweenExclusive(), value, options);

    public static IFilterConnection BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterConnection BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(propertyExpression, new BetweenExclusive(), value, null);

    public static IFilterConnection BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    ) =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null
        );

    public static IFilter BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilter BetweenExclusive<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion string propertyExpression
}
