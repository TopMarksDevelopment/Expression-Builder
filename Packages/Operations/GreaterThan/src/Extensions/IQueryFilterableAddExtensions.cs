namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterableGreaterThanExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            options
        );

    public static IQueryFilterableConnection GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            options
        );

    public static IQueryFilterableConnection GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            null
        );

    public static IQueryFilterableConnection GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            null
        );

    public static IQueryFilterable GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            null,
            connector
        );

    public static IQueryFilterable GreaterThan<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            options
        );

    public static IQueryFilterableConnection GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            options
        );

    public static IQueryFilterableConnection GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            null
        );

    public static IQueryFilterableConnection GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            null
        );

    public static IQueryFilterable GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            value,
            null,
            connector
        );

    public static IQueryFilterable GreaterThan<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThan(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
