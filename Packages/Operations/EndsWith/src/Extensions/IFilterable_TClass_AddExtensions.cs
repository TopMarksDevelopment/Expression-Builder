namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterable_TClass_EndsWithExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            options
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            options
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            null
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value]
        );

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            null,
            connector
        );

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class {
        var f = filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            options
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            options
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            null
        );

    public static IFilterableConnection<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            null
        );

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            value,
            null,
            connector
        );

    public static IFilterable<TClass> EndsWith<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) where TClass : class  => filter.Add(
            propertyExpression,
            new EndsWith(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
