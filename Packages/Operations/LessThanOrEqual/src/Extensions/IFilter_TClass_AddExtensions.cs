namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilter_TClass_LessThanOrEqualExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), value, options);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), [value], options);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), value, null);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), [value]);

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            options,
            connector
        );

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), value, options);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), [value], options);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), value, null);

    public static IFilterConnection<TClass> LessThanOrEqual<
        TClass,
        TPropertyType
    >(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new LessThanOrEqual(), [value], null);

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            value,
            null,
            connector
        );

    public static IFilter<TClass> LessThanOrEqual<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new LessThanOrEqual(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
