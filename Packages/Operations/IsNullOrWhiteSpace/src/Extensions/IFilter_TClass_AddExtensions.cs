namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public static partial class IFilter_TClass_IsNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
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

    public static IFilterConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class => filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null
        );

    public static IFilter<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
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

    public static IFilter<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
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

    public static IFilterConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) where TClass : class  => filter.Add<TPropertyType>(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterConnection<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression
    ) where TClass : class  => filter.Add<TPropertyType>(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null
        );

    public static IFilter<TClass> IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter<TClass> filter,
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
