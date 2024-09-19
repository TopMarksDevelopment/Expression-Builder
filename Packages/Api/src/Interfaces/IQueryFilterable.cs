namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// A filterable version of an IQueryable
/// </summary>
public interface IQueryFilterable : IQueryable
{
    /// <summary>
    /// The filter for the class
    /// </summary>
    IFilter Filter { get; }

    /// <summary>
    /// Add a statement to the filter
    /// </summary>
    /// <param name="statement">The statement to add</param>
    /// <returns>Itself (for chain calls)</returns>
    IQueryFilterable Add(IFilterStatement statement);

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
    IQueryFilterableConnection Add<TClass, TPropertyType>(
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
    IQueryFilterableConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    /// <summary>
    /// Set the connector of the last statement we added
    /// </summary>
    /// <param name="connector">The connector to use</param>
    /// <returns>Itself (for chain calls)</returns>
    IQueryFilterable SetConnector(Connector connector);

    /// <summary>
    /// Start a group for subsequent statements (like wrapping them in perentheses)
    /// </summary>
    /// <returns>Itself - after opening a new group (for chain calls)</returns>
    IQueryFilterable OpenGroup();

    /// <summary>
    /// Close the group of statements (closing the perentheses)
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    IQueryFilterableConnection CloseGroup();

    /// <summary>
    /// Open a collection for filtering
    /// </summary>
    /// <param name="propertyExpression">The property we're filtering</param>
    /// <returns>A filter focused on the collection</returns>
    IQueryFilterable OpenCollection(string propertyExpression);

    /// <summary>
    /// Close the collection we've been filtering against
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    IQueryFilterableConnection CloseCollection();
}
