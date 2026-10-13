namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

public class FilterConnection<TClass>(IFilter<TClass> filter, IFilterItem item)
    : IFilterConnection<TClass>
    where TClass : class
{
    readonly IFilter<TClass> _filter = filter;
    readonly IFilterItem _item = item;

    public IFilter<TClass> And()
    {
        _item.Connector = Connector.And;

        return _filter;
    }

    public IFilter<TClass> Or()
    {
        _item.Connector = Connector.Or;

        return _filter;
    }

    public IFilterConnection<TClass> CloseGroup() => _filter.CloseGroup();

    public IFilterConnection<T> CloseCollection<T>()
        where T : class => _filter.CloseCollection<T>();

    IFilter IFilterConnection.And() => And();

    IFilter IFilterConnection.Or() => Or();

    IFilterConnection IFilterConnection.CloseGroup() => CloseGroup();

    public IFilterConnection CloseCollection() => _filter.CloseCollection();

    public IFilterConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class => _filter.CloseCollection(expression);
}
