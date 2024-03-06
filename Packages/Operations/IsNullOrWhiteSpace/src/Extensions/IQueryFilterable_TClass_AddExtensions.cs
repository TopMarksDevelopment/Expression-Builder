namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterable_TClass_IsNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
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

    public static IQueryFilterableConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IQueryFilterableConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null
        );

    public static IQueryFilterable<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
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

    public static IQueryFilterable<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    ) where TClass : class {
        var f = filter.Add<TPropertyType>(
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

    public static IQueryFilterableConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add<TPropertyType>(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IQueryFilterableConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression
    ) where TClass : class  => filter.Add<TPropertyType>(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null
        );

    public static IQueryFilterable<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable<TClass> filter,
        string propertyExpression,
        Connector connector
    ) where TClass : class  => filter.Add<TClass, TPropertyType>(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null,
            connector
        );

    #endregion string propertyExpression
}
