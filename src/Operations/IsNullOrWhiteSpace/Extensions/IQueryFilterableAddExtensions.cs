namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterableIsNullOrWhiteSpaceExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterableConnection IsNullOrWhiteSpace<
        TClass,
        TPropertyType
    >(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNullOrWhiteSpace(), [], options);

    public static IQueryFilterableConnection IsNullOrWhiteSpace<
        TClass,
        TPropertyType
    >(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsNullOrWhiteSpace(), [], null);

    public static IQueryFilterable IsNullOrWhiteSpace<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            [],
            null,
            connector
        );

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable IsNullOrWhiteSpace(
        this IQueryFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterableConnection IsNullOrWhiteSpace(
        this IQueryFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IQueryFilterableConnection IsNullOrWhiteSpace(
        this IQueryFilterable filter,
        string propertyExpression
    ) =>
        filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IQueryFilterable IsNullOrWhiteSpace(
        this IQueryFilterable filter,
        string propertyExpression,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new IsNullOrWhiteSpace(),
            Array.Empty<dynamic?>(),
            null,
            connector
        );

    #endregion string propertyExpression
}
