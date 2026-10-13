namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterable_TClass_SmartSearchExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), value, options);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), [value], options);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), value, null);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), [value]);

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null,
            connector
        );

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            options,
            connector
        );

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), value, options);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), [value], options);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), value, null);

    public static IFilterableConnection<TClass> SmartSearch<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, new SmartSearch(), [value], null);

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            value,
            null,
            connector
        );

    public static IFilterable<TClass> SmartSearch<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new SmartSearch(),
            [value],
            null,
            connector
        );

    #endregion string propertyExpression
}
