namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections;
using System.Linq.Expressions;

/// <summary>
/// A filterable version of an IEnumerable
/// </summary>
public interface IFilterable : IEnumerable
{
    /// <summary>
    /// The filter we're applying
    /// </summary>
    IFilter Filter { get; }

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <param name="statement">The statement to add</param>
    /// <returns></returns>
    IFilterable Add(IFilterStatement statement);

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="values">The values we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection Add<TClass, TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
        where TClass : class;

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="values">The values we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Open a group for subsequent statements
    /// </summary>
    /// <returns>Itself - after opening a group (for chaining)</returns>
    IFilterable OpenGroup();

    /// <summary>
    /// Close the current group
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection CloseGroup();

    /// <summary>
    /// Open a collection for subsequent statements
    /// </summary>
    /// <param name="propertyExpression">The property we're opening</param>
    /// <returns>A filter focused on the new collection</returns>
    IFilterable OpenCollection(string propertyExpression);

    /// <summary>
    /// Close the current collection
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection CloseCollection();

    /// <summary>
    /// Convert the filter to an expression
    /// </summary>
    /// <typeparam name="TClass">The type of the class we're filtering</typeparam>
    /// <returns>An expression representing the filter</returns>
    Expression<Func<TClass, bool>> ToExpression<TClass>();
}
