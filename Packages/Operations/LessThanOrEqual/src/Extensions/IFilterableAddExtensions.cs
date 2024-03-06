namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableLessThanOrEqualExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

    public static IFilterableConnection LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options
        );

    public static IFilterableConnection LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null
        );

    public static IFilterableConnection LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value]
        );

    public static IFilterable LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilterable LessThanOrEqual<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

    public static IFilterableConnection LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options
        );

    public static IFilterableConnection LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null
        );

    public static IFilterableConnection LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            null
        );

    public static IFilterable LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilterable LessThanOrEqual<TPropertyType>(
        this IFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
