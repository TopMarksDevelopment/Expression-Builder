namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilter_TClass_NotBetweenExclusiveExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            options
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            null
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            null
        );

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class {
        var f = filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            options,
            connector
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            options
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            options
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            null
        );

    public static IFilterConnection<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            null
        );

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            value,
            null,
            connector
        );

    public static IFilter<TClass> NotBetweenExclusive<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        TPropertyType? value2,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new NotBetweenExclusive(),
            [value, value2],
            null,
            connector
        );

    #endregion string propertyExpression
}
