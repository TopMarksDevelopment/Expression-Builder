namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Linq.Expressions;

public static partial class IFilterable_TClass_AddExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            operation,
            [value]
        );

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class {
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

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            operation,
            [value],
            options,
            connector
        );

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            operation,
            value,
            null
        );

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            operation,
            [value]
        );

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            operation,
            value,
            null,
            connector
        );

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            operation,
            [value],
            null,
            connector
        );

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        Connector connector
    ) where TClass : class  => filter.Add<TClass, TPropertyType>(
            propertyExpression,
            operation,
            [],
            null,
            connector
        );

    public static IFilterable<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class  =>
        filter.Add<TClass, TPropertyType>(
            propertyExpression,
            operation,
            [],
            options,
            connector
        );

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation,
        IFilterStatementOptions? options
    ) where TClass : class  =>
        filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            options
        );

    public static IFilterableConnection<TClass> Add<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IOperation operation
    ) where TClass : class  =>
        filter.Add<TPropertyType>(
            propertyExpression,
            operation,
            [],
            null
        );

    #endregion string propertyExpression
}
