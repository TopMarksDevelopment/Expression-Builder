namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Joins the statement that was just added to the next one (or closes a group)
/// on a filterable enumerable.
/// </summary>
public class FilterableConnection<TClass> : IFilterableConnection<TClass>
    where TClass : class
{
    IFilterable<TClass> _filterable;

    /// <summary>
    /// Connects the last statement of a filterable
    /// </summary>
    public FilterableConnection(IFilterable<TClass> filterable)
    {
        _filterable = filterable;
    }

    /// <inheritdoc />
    public IFilterable<TClass> And() => _filterable.SetConnector(Connector.And);

    /// <inheritdoc />
    public IFilterable<TClass> Or() => _filterable.SetConnector(Connector.Or);

    /// <inheritdoc />
    public IFilterableConnection<TClass> CloseGroup() =>
        _filterable.CloseGroup();

    /// <inheritdoc />
    public IFilterableConnection<TParentClass> CloseCollection<TParentClass>()
        where TParentClass : class =>
        _filterable.CloseCollection<TParentClass>();

    /// <inheritdoc />
    public IFilterableConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class => CloseCollection<TParentClass>();

    IFilterable IFilterableConnection.And() => And();

    IFilterable IFilterableConnection.Or() => Or();

    IFilterableConnection IFilterableConnection.CloseGroup() => CloseGroup();

    IFilterableConnection IFilterableConnection.CloseCollection() =>
        ((IFilterable)_filterable).CloseCollection();
}
