namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableEqualExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new Equal(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            options
        );

    public static IFilterableConnection Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            options
        );

    public static IFilterableConnection Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            null
        );

    public static IFilterableConnection Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            [value]
        );

    public static IFilterable Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            null,
            connector
        );

    public static IFilterable Equal<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new Equal(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            options
        );

    public static IFilterableConnection Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            options
        );

    public static IFilterableConnection Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            null
        );

    public static IFilterableConnection Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            null
        );

    public static IFilterable Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            value,
            null,
            connector
        );

    public static IFilterable Equal<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new Equal(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
