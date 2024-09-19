namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IQueryFilterableIsEmptyExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IQueryFilterable IsEmpty<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, new IsEmpty(), [], options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterableConnection IsEmpty<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), [], options);

    public static IQueryFilterableConnection IsEmpty<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), [], null);

    public static IQueryFilterable IsEmpty<TClass, TPropertyType>(
        this IQueryFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), [], null, connector);

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IQueryFilterable IsEmpty(
        this IQueryFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
    {
        var f = filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            options
        );

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IQueryFilterableConnection IsEmpty(
        this IQueryFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IQueryFilterableConnection IsEmpty(
        this IQueryFilterable filter,
        string propertyExpression
    ) =>
        filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IQueryFilterable IsEmpty(
        this IQueryFilterable filter,
        string propertyExpression,
        Connector connector
    ) =>
        filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            null,
            connector
        );

    #endregion string propertyExpression
}
