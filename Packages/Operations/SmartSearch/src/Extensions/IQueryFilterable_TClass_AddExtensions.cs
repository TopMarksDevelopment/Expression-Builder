namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterable_TClass_SmartSearchExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null
        );

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null,
            connector
        );

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class {
        var f = filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options,
            connector
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null
        );

    public static IQueryFilterableConnection<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null
        );

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null,
            connector
        );

    public static IQueryFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
