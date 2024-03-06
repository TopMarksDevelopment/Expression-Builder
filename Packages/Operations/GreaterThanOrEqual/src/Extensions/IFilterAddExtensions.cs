namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterGreaterThanOrEqualExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilter GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterConnection GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

    public static IFilterConnection GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options
        );

    public static IFilterConnection GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null
        );

    public static IFilterConnection GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null
        );

    public static IFilter GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilter GreaterThanOrEqual<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilter GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilter GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterConnection GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            options
        );

    public static IFilterConnection GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            options
        );

    public static IFilterConnection GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null
        );

    public static IFilterConnection GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null
        );

    public static IFilter GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilter GreaterThanOrEqual<TPropertyType>(
        this IFilter filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new GreaterThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
