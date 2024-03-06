namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterableStartsWithExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            options
        );

    public static IQueryFilterableConnection StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            options
        );

    public static IQueryFilterableConnection StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            null
        );

    public static IQueryFilterableConnection StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            null
        );

    public static IQueryFilterable StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            null,
            connector
        );

    public static IQueryFilterable StartsWith<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            options
        );

    public static IQueryFilterableConnection StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            options
        );

    public static IQueryFilterableConnection StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            null
        );

    public static IQueryFilterableConnection StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            null
        );

    public static IQueryFilterable StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            value,
            null,
            connector
        );

    public static IQueryFilterable StartsWith<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new StartsWith(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
