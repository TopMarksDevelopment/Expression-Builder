namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterable_TClass_IsNotEmptyExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable<TClass> IsNotEmpty<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, new IsNotEmpty(), [], options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterableConnection<TClass> IsNotEmpty<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNotEmpty(), [], options);

    public static IFilterableConnection<TClass> IsNotEmpty<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNotEmpty(), [], null);

    public static IFilterable<TClass> IsNotEmpty<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNotEmpty(), [], null, connector);

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable<TClass> IsNotEmpty<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add<TPropertyType>(
            propertyExpression,
            new IsNotEmpty(),
            [],
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterableConnection<TClass> IsNotEmpty<
        TClass,
        TPropertyType
    >(
        this IFilterable<TClass> filter,
        string propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add<TPropertyType>(
            propertyExpression,
            new IsNotEmpty(),
            [],
            options
        );

    public static IFilterableConnection<TClass> IsNotEmpty<
        TClass,
        TPropertyType
    >(this IFilterable<TClass> filter, string propertyExpression)
        where TClass : class =>
        filter.Add<TPropertyType>(
            propertyExpression,
            new IsNotEmpty(),
            [],
            null
        );

    public static IFilterable<TClass> IsNotEmpty<TClass, TPropertyType>(
        this IFilterable<TClass> filter,
        string propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add<TClass, TPropertyType>(
            propertyExpression,
            new IsNotEmpty(),
            [],
            null,
            connector
        );

    #endregion string propertyExpression
}
