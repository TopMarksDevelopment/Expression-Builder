namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Joins the statement that was just added to the next one (or closes a group)
/// on a <see cref="Filter{TClass}"/>.
/// </summary>
/// <param name="filter">The filter being built</param>
/// <param name="item">The statement or group that was just added</param>
public class FilterConnection<TClass>(IFilter<TClass> filter, IFilterItem item)
    : IFilterConnection<TClass>
    where TClass : class
{
    readonly IFilter<TClass> _filter = filter;
    readonly IFilterItem _item = item;

    /// <inheritdoc />
    public IFilter<TClass> And()
    {
        _item.Connector = Connector.And;

        return _filter;
    }

    /// <inheritdoc />
    public IFilter<TClass> Or()
    {
        _item.Connector = Connector.Or;

        return _filter;
    }

    /// <inheritdoc />
    public IFilterConnection<TClass> CloseGroup() => _filter.CloseGroup();

    /// <inheritdoc />
    public IFilterConnection<T> CloseCollection<T>()
        where T : class => _filter.CloseCollection<T>();

    IFilter IFilterConnection.And() => And();

    IFilter IFilterConnection.Or() => Or();

    IFilterConnection IFilterConnection.CloseGroup() => CloseGroup();

    /// <inheritdoc />
    public IFilterConnection CloseCollection() => _filter.CloseCollection();

    /// <inheritdoc />
    public IFilterConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class => _filter.CloseCollection(expression);
}
