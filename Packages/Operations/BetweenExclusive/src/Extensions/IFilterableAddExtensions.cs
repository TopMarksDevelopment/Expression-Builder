namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableBetweenExclusiveExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterableConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

    public static IFilterableConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterableConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null
        );

    public static IFilterableConnection BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2]
        );

    public static IFilterable BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilterable BetweenExclusive<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterableConnection BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            options
        );

    public static IFilterableConnection BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterableConnection BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null
        );

    public static IFilterableConnection BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null
        );

    public static IFilterable BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilterable BetweenExclusive<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new BetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion string propertyExpression
}
