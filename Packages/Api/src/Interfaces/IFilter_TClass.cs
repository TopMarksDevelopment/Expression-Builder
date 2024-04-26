namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// The absolute core items required for a filter
/// </summary>
/// <typeparam name="TClass"></typeparam>
public partial interface IFilter<TClass> : IFilter
    where TClass : class
{
    /// <summary>
    /// Add a pre-built statement directly to <see cref="Current"/>
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the values in the statement</typeparam>
    /// <param name="statement">The pre-built statement</param>
    /// <returns>The same filter for fluent calls</returns>
    IFilter<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    );

    /// <summary>
    /// Creates a <see cref="IFilterStatement"/> which is added to <see cref="Current"/>
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the values</typeparam>
    /// <param name="propertyExpression">The lambda expression to attain the property name</param>
    /// <param name="operation">The operation we're performing</param>
    /// <param name="values">The values we're applying to this statement</param>
    /// <param name="options">Any options applied to this statement</param>
    /// <returns>A connection for fluent calls</returns>
    IFilterConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Creates a <see cref="IFilterStatement"/> which is added to <see cref="Current"/>
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the values</typeparam>
    /// <param name="propertyExpression">The string represetation of the property</param>
    /// <param name="operation">The operation we're performing</param>
    /// <param name="values">The values we're applying to this statement</param>
    /// <param name="options">Any options applied to this statement</param>
    /// <returns>A connection for fluent calls</returns>
    new IFilterConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Set the connector of the last item in the <see cref="Current"/> list
    /// </summary>
    /// <param name="connector">The connector to apply (And/Or)</param>
    void SetLastConnector(Connector connector);

    /// <summary>
    /// Open a group, wrapping everything inside is parentheses <c>( )</c>
    /// </summary>
    /// <remarks>This allows for more complex expressions</remarks>
    /// <returns>The same filter for fluent calls</returns>
    new IFilter<TClass> OpenGroup();

    /// <summary>
    /// Closes the group of statements
    /// </summary>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    new IFilterConnection<TClass> CloseGroup();

    /// <summary>
    /// Access a subcollection and continue to query against that
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyExpression"></param>
    /// <returns></returns>
    IFilter<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// (where the type is specified)
    /// </summary>
    /// <typeparam name="T">The type of the parent collection</typeparam>
    /// <remarks>This must be specified if you plan to do fluent calls</remarks>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>
    IFilterConnection<T> CloseCollection<T>()
        where T : class;

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// (where the type comes from a navigation property)
    /// </summary>
    /// <typeparam name="T">The type of the parent collection</typeparam>
    /// <param name="expression">The expression which points to the parent property (if navigation exists)</param>
    /// <remarks>This must be specified if you plan to do fluent calls</remarks>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>
    IFilterConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;

    /// <summary>
    /// Conver the filter to an expression which is used in `Where()` calls
    /// </summary>
    /// <returns>The filter in LINQ's expression form</returns>
    Expression<Func<TClass, bool>> ToExpression();
}
