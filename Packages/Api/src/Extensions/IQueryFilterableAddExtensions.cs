namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Linq.Expressions;

public static partial class IQueryFilterableAddExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            operation,
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            value,
            null
        );

    public static IQueryFilterableConnection Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            [value]
        );

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            value,
            null,
            connector
        );

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            [value],
            null,
            connector
        );

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            [],
            null,
            connector
        );

    public static IQueryFilterable Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class =>
        filter.Add(
            propertyExpression,
            operation,
            [],
            options,
            connector
        );

    public static IQueryFilterableConnection Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options
    ) where TClass : class =>
        filter.Add(
            propertyExpression,
            operation,
            [],
            options
        );

    public static IQueryFilterableConnection Add<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation
    ) where TClass : class =>
        filter.Add(
            propertyExpression,
            operation,
            [],
            null
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            operation,
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            operation,
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            operation,
            value,
            null
        );

    public static IQueryFilterableConnection Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            operation,
            [value]
        );

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            operation,
            value,
            null,
            connector
        );

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            operation,
            [value],
            null,
            connector
        );

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        Connector connector
    ) => filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            null,
            connector
        );

    public static IQueryFilterable Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options,
        Connector connector
    ) =>
        filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            options,
            connector
        );

    public static IQueryFilterableConnection Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options
    ) =>
        filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            options
        );

    public static IQueryFilterableConnection Add<TPropertyType>(
        this IQueryFilterable filter,
        string propertyExpression,
        IOperation operation
    ) =>
        filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            null
        );

    #endregion string propertyExpression
}
