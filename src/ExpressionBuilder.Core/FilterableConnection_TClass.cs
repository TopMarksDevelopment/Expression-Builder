namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

public class FilterableConnection<TClass> : IFilterableConnection<TClass>
    where TClass : class
{
    IFilterable<TClass> _filterable;

    public FilterableConnection(IFilterable<TClass> filterable)
    {
        _filterable = filterable;
    }

    public IFilterable<TClass> And() => _filterable.SetConnector(Connector.And);

    public IFilterable<TClass> Or() => _filterable.SetConnector(Connector.Or);

    public IFilterableConnection<TClass> CloseGroup() =>
        _filterable.CloseGroup();

    public IFilterableConnection<TParentClass> CloseCollection<TParentClass>()
        where TParentClass : class =>
        _filterable.CloseCollection<TParentClass>();

    public IFilterableConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class => CloseCollection<TParentClass>();

    IFilterable IFilterableConnection.And() => And();

    IFilterable IFilterableConnection.Or() => Or();

    IFilterableConnection IFilterableConnection.CloseGroup() => CloseGroup();

    IFilterableConnection IFilterableConnection.CloseCollection() =>
        CloseCollection<object>();
}
