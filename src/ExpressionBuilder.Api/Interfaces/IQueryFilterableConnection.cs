namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A connection for fluent calls on <see cref="IQueryFilterable"/>s'
/// </summary>
public interface IQueryFilterableConnection
{
    /// <summary>
    /// Apply <see cref="Connector.And"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    IQueryFilterable And();

    /// <summary>
    /// Apply <see cref="Connector.Or"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    IQueryFilterable Or();

    /// <summary>
    /// Close the open group
    /// </summary>
    /// <returns>The expression outside the group</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    IQueryFilterableConnection CloseGroup();

    /// <summary>
    /// Close the open collection
    /// </summary>
    /// <returns>The expression outside the collection</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    IQueryFilterableConnection CloseCollection();
}
