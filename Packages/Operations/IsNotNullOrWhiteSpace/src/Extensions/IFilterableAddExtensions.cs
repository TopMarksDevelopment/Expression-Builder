namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public static partial class IFilterableIsNotNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterableConnection IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterableConnection IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            []
        );

    public static IFilterable IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable IsNotNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterableConnection IsNotNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IFilterableConnection IsNotNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression
    ) => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IFilterable IsNotNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null,
            connector
        );

    #endregion string propertyExpression
}
