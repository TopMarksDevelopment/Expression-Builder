namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterIsNotNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilter IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterConnection IsNotNullOrWhiteSpace<
        TClass,
        TPropertyType
    >(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            options
        );

    public static IFilterConnection IsNotNullOrWhiteSpace<
        TClass,
        TPropertyType
    >(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNotNullOrWhiteSpace(), [], null);

    public static IFilter IsNotNullOrWhiteSpace<TClass, TPropertyType>(
        this IFilter filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            [],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilter IsNotNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterConnection IsNotNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IFilterConnection IsNotNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression
    ) =>
        filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IFilter IsNotNullOrWhiteSpace(
        this IFilter filter,
        string propertyExpression,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new IsNotNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null,
            connector
        );

    #endregion string propertyExpression
}
