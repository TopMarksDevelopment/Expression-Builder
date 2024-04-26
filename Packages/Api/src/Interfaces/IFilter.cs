namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

/// <summary>
/// The absolute core items required for a filter without a dedicated type
/// </summary>
public interface IFilter
{
    /// <summary>
    /// The items that make apply in this filter
    /// </summary>
    ICollection<IFilterItem> Items { get; }

    /// <summary>
    /// The items the Filter is currently applying changes to
    /// </summary>
    ICollection<IFilterItem> Current { get; }

    /// <summary>
    /// Add a pre-built statement directly to <see cref="Current"/>
    /// </summary>
    /// <param name="statement">The pre-built statement</param>
    /// <returns>The same filter for fluent calls</returns>
    IFilter Add(IFilterStatement statement);

    /// <summary>
    /// Creates a <see cref="IFilterStatement"/> which is added to <see cref="Current"/>
    /// </summary>
    /// <typeparam name="TClass">The base class that we're filtering against</typeparam>
    /// <typeparam name="TPropertyType">The type of the values</typeparam>
    /// <param name="propertyExpression">The lambda expression to attain the property name</param>
    /// <param name="operation">The operation we're performing</param>
    /// <param name="values">The values we're applying to this statement</param>
    /// <param name="options">Any options applied to this statement</param>
    /// <returns>A connection for fluent calls</returns>
    IFilterConnection Add<TClass, TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
        where TClass : class;

    /// <summary>
    /// Creates a <see cref="IFilterStatement"/> which is added to <see cref="Current"/>
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the values</typeparam>
    /// <param name="propertyExpression">The string represetation of the property</param>
    /// <param name="operation">The operation we're performing</param>
    /// <param name="values">The values we're applying to this statement</param>
    /// <param name="options">Any options applied to this statement</param>
    /// <returns>A connection for fluent calls</returns>
    IFilterConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Open a group, wrapping everything inside is parentheses <c>( )</c>
    /// </summary>
    /// <remarks>This allows for more complex expressions</remarks>
    /// <returns>The same filter for fluent calls</returns>
    IFilter OpenGroup();

    /// <summary>
    /// Closes the group of statements
    /// </summary>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    IFilterConnection CloseGroup();

    /// <summary>
    /// Access a subcollection and continue to query against that
    /// </summary>
    /// <param name="propertyExpression"></param>
    /// <returns>The same filter for fluent calls</returns>
    IFilter OpenCollection(string propertyExpression);

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// </summary>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>
    IFilterConnection CloseCollection();

    /// <summary>
    /// See a string representation of the full filter
    /// </summary>
    /// <returns>The filter in string form</returns>
    string ToString();

    /// <summary>
    /// Conver the filter to an expression which is used in `Where()` calls
    /// </summary>
    /// <typeparam name="T">The class type of which we're filtering against</typeparam>
    /// <returns>The filter in LINQ's expression form</returns>
    Expression<Func<T, bool>> ToExpression<T>();
}
