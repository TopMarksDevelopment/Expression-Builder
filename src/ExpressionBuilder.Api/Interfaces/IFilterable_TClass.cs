namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// A filterable version of an IEnumerable
/// </summary>
/// <typeparam name="TClass">The type of the class we're filtering</typeparam>
public interface IFilterable<TClass> : IEnumerable<TClass>, IFilterable
    where TClass : class
{
    /// <summary>
    /// The filter for the class
    /// </summary>
    new IFilter<TClass> Filter { get; }

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="statement">The statement to add</param>
    /// <returns>Itself (for chain calls)</returns>
    IFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    );

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="values">The values we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property we're filtering</typeparam>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <param name="operation">The operation we're applying</param>
    /// <param name="values">The values we're applying with the operation</param>
    /// <param name="options">Any options we're applying to the statement</param>
    /// <returns>A connection member so we can chain calls</returns>
    new IFilterableConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Start a group for subsequent statements (like wrapping them in perentheses)
    /// </summary>
    /// <returns>Itself - after opening a new group (for chain calls)</returns>
    new IFilterable<TClass> OpenGroup();

    /// <summary>
    /// Close the group of statements (closing the perentheses)
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    new IFilterableConnection<TClass> CloseGroup();

    /// <summary>
    /// Set the connector of the last statement we added
    /// </summary>
    /// <param name="connector">The connector we're setting</param>
    /// <returns>Itself (for chain calls)</returns>
    IFilterable<TClass> SetConnector(Connector connector);

    /// <summary>
    /// Open a collection to filter against (will perform an Any check)
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="propertyExpression">The enumerable child property we're going to filtering</param>
    /// <returns>A new filterable focused on the new collection</returns>
    IFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    /// <summary>
    /// Close the collection we've been filtering against
    /// </summary>
    /// <typeparam name="T">The type of the collection we're reverting back to</typeparam>
    /// <returns>A connection member so we can chain calls</returns>
    IFilterableConnection<T> CloseCollection<T>()
        where T : class;

    /// <summary>
    /// Close the collection we've been filtering against
    /// </summary>
    /// <typeparam name="T">The type of the collection we're reverting back to</typeparam>
    /// <param name="expression">An expression pointing to the parent property (if it's available)</param>
    /// <returns></returns>
    IFilterableConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;

    /// <summary>
    /// Convert the filter to an expression
    /// </summary>
    /// <returns>A LINQ expression representing the filter</returns>
    Expression<Func<TClass, bool>> ToExpression();
}
