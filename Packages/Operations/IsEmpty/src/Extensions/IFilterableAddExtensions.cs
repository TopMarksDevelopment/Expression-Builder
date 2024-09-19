namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Operations;

public static partial class IFilterableIsEmptyExtensions
{
    #region Expression<Func<,>> propertyExpression

    public static IFilterable IsEmpty<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, new IsEmpty(), [], options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    public static IFilterableConnection IsEmpty<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), [], options);

    public static IFilterableConnection IsEmpty<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), []);

    public static IFilterable IsEmpty<TClass, TPropertyType>(
        this IFilterable filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, new IsEmpty(), [], null, connector);

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    public static IFilterable IsEmpty(
        this IFilterable filter,
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

    public static IFilterableConnection IsEmpty(
        this IFilterable filter,
        string propertyExpression,
        IFilterStatementOptions? options
    ) =>
        filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            options
        );

    public static IFilterableConnection IsEmpty(
        this IFilterable filter,
        string propertyExpression
    ) =>
        filter.Add(
            propertyExpression,
            new IsEmpty(),
            Array.Empty<dynamic?>(),
            null
        );

    public static IFilterable IsEmpty(
        this IFilterable filter,
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
