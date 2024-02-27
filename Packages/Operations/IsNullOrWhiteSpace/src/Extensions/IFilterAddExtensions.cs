namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public static partial class IFilterIsNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
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

    public static IFilterConnection IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterConnection IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null
        );

    public static IFilter IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
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

    public static IFilter IsNullOrWhiteSpace(
        this IFilter filter,
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

    public static IFilterConnection IsNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IFilterConnection IsNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression
    ) => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IFilter IsNullOrWhiteSpace(
        this IFilter filter,
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
