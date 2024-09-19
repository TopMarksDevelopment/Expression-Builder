namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Extensions for adding statements to a filter
/// </summary>
public static partial class IFilter_TClass_AddExtensions
{
    #region Expression<Func<,>> propertyExpression

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, operation, value, options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], options, connector);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, options);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], options);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, null);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value]);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, null, connector);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], null, connector);

    #endregion Expression<Func<,>> propertyExpression

    #region string propertyExpression

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class
    {
        var f = filter.Add(propertyExpression, operation, value, options);

        return connector == Connector.Or ? f.Or() : f.And();
    }

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], options, connector);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, options);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        IFilterStatementOptions? options
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], options);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, null);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <returns>A connector (so calls can be chained)</returns>
    public static IFilterConnection<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], null);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, value, null, connector);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="filter">The filter to add the statement to</param>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="value">The value we're applying with the operation</param>
    /// <param name="connector">The connector to use</param>
    /// <returns>The filter with the statement added</returns>
    public static IFilter<TClass> Add<TClass, TPropertyType>(
        this IFilter<TClass> filter,
        string propertyExpression,
        IOperation operation,
        TPropertyType? value,
        Connector connector
    )
        where TClass : class =>
        filter.Add(propertyExpression, operation, [value], null, connector);

    #endregion string propertyExpression
}
