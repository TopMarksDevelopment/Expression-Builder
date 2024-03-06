namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableIsNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class{
        var f = filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterableConnection IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterableConnection IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            []
        );

    public static IFilterable IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable IsNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    ){
        var f = filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

        return 
            connector == Connector.Or
                ? f.Or()
                : f.And();
    }

    public static IFilterableConnection IsNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IFilterableConnection IsNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression
    ) => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IFilterable IsNullOrWhiteSpace(
        this IFilterable filter,
        string propertyExpression,
        Connector connector
    ) => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null,
            connector
        );

    #endregion string propertyExpression
}
