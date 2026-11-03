namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// A filterable version of an IQueryable
/// </summary>
/// <typeparam name="TClass">The type of the class we're filtering</typeparam>
public interface IQueryFilterable<TClass> : IQueryable<TClass>, IQueryFilterable
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
    IQueryFilterable<TClass> Add<TPropertyType>(
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
    IQueryFilterableConnection<TClass> Add<TPropertyType>(
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
    new IQueryFilterableConnection<TClass> Add<TPropertyType>(
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
    new IQueryFilterable<TClass> SetConnector(Connector connector);

    /// <summary>
    /// Start a group for subsequent statements (like wrapping them in perentheses)
    /// </summary>
    /// <returns>Itself - after opening a new group (for chain calls)</returns>
    new IQueryFilterable<TClass> OpenGroup();

    /// <summary>
    /// Close the group of statements (closing the perentheses)
    /// </summary>
    /// <returns>A connection member so we can chain calls</returns>
    new IQueryFilterableConnection<TClass> CloseGroup();

    /// <summary>
    /// Open a collection for filtering
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="propertyExpression">The enumerable we're going to filter against</param>
    /// <returns>A filter focused on the collection</returns>
    IQueryFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    /// <summary>
    /// Close the collection we've been filtering against
    /// </summary>
    /// <typeparam name="T">The type of the parent member</typeparam>
    /// <returns>A connection member so we can chain calls</returns>
    IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class;

    /// <summary>
    /// Close the collection we've been filtering against
    /// </summary>
    /// <typeparam name="T">The type of the parent member</typeparam>
    /// <param name="expression">An expression to get the parent member (if it exists)</param>
    /// <returns>A connection member so we can chain calls</returns>
    IQueryFilterableConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;
}
