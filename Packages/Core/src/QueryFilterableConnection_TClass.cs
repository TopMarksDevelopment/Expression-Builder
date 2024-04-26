namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

public class QueryFilterableConnection<TClass>
    : IQueryFilterableConnection<TClass>
    where TClass : class
{
    IQueryFilterable<TClass> _filterable;

    public QueryFilterableConnection(IQueryFilterable<TClass> filterable)
    {
        _filterable = filterable;
    }

    public IQueryFilterable<TClass> And() =>
        _filterable.SetConnector(Connector.And);

    public IQueryFilterable<TClass> Or() =>
        _filterable.SetConnector(Connector.Or);

    public IQueryFilterable<TClass> End() => _filterable;

    public IQueryFilterableConnection<TClass> CloseGroup() =>
        _filterable.CloseGroup();

    public IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class => _filterable.CloseCollection<T>();

    public IQueryFilterableConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    IQueryFilterable IQueryFilterableConnection.And() => And();

    IQueryFilterable IQueryFilterableConnection.Or() => Or();

    IQueryFilterableConnection IQueryFilterableConnection.CloseGroup() => CloseGroup();

    IQueryFilterableConnection IQueryFilterableConnection.CloseCollection() => CloseCollection<object>();
/*
    Expression<Func<TClass1, bool>> IQueryFilterableConnection.ToExpression<TClass1>()
    {
        throw new NotImplementedException();
    }*/
}
