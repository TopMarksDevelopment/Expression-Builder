namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Joins the statement that was just added to the next one (or closes a group)
/// on a filterable queryable.
/// </summary>
public class QueryFilterableConnection<TClass>
    : IQueryFilterableConnection<TClass>
    where TClass : class
{
    IQueryFilterable<TClass> _filterable;

    /// <summary>
    /// Connects the last statement of a filterable
    /// </summary>
    public QueryFilterableConnection(IQueryFilterable<TClass> filterable)
    {
        _filterable = filterable;
    }

    /// <inheritdoc />
    public IQueryFilterable<TClass> And() =>
        _filterable.SetConnector(Connector.And);

    /// <inheritdoc />
    public IQueryFilterable<TClass> Or() =>
        _filterable.SetConnector(Connector.Or);

    /// <inheritdoc />
    public IQueryFilterable<TClass> End() => _filterable;

    /// <inheritdoc />
    public IQueryFilterableConnection<TClass> CloseGroup() =>
        _filterable.CloseGroup();

    /// <inheritdoc />
    public IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class => _filterable.CloseCollection<T>();

    /// <inheritdoc />
    public IQueryFilterableConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    IQueryFilterable IQueryFilterableConnection.And() => And();

    IQueryFilterable IQueryFilterableConnection.Or() => Or();

    IQueryFilterableConnection IQueryFilterableConnection.CloseGroup() =>
        CloseGroup();

    IQueryFilterableConnection IQueryFilterableConnection.CloseCollection() =>
        ((IQueryFilterable)_filterable).CloseCollection();
}
