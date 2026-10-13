namespace TopMarksDevelopment.ExpressionBuilder.Enumerables;

using System.Collections;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// An <see cref="IEnumerable{T}"/> that is filtered by the statements added to it.
/// Enumerating it applies the filter to the underlying list.
/// </summary>
public class Filterable<TClass> : IFilterable<TClass>
    where TClass : class
{
    readonly IEnumerable? _oList;
    readonly IEnumerable<TClass> _list;

    /// <inheritdoc />
    public IFilter<TClass> Filter { get; set; }

    Filterable()
    {
        _list = [];

        Filter = new Filter<TClass>();
    }

    Filterable(IEnumerable list, IFilter<TClass> filter)
    {
        _oList = list;
        _list = [];

        Filter = filter;
    }

    /// <summary>
    /// Filters a list
    /// </summary>
    public Filterable(IEnumerable<TClass> list)
    {
        _list = list;

        Filter = new Filter<TClass>();
    }

    internal Filterable(IEnumerable<TClass> list, Filter<TClass> filter)
    {
        _list = list;

        Filter = filter;
    }

    #region Add methods

    /// <inheritdoc />
    public IFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    )
    {
        Filter.Add(statement);

        return this;
    }

    /// <inheritdoc />
    public IFilterableConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
    {
        Add(
            FilterStatement<TPropertyType>.Create(
                propertyExpression,
                operation,
                values,
                options
            )
        );

        return new FilterableConnection<TClass>(this);
    }

    /// <inheritdoc />
    public IFilterableConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
    {
        Add(
            new FilterStatement<TPropertyType>(
                propertyExpression,
                operation,
                values,
                options
            )
        );

        return new FilterableConnection<TClass>(this);
    }

    #endregion Add methods

    /// <inheritdoc />
    public IFilterable<TClass> OpenGroup()
    {
        Filter.OpenGroup();

        return this;
    }

    /// <inheritdoc />
    public IFilterableConnection<TClass> CloseGroup()
    {
        Filter.CloseGroup();

        return new FilterableConnection<TClass>(this);
    }

    /// <inheritdoc />
    public IFilterable<TClass> SetConnector(Connector connector)
    {
        Filter.SetLastConnector(connector);

        return this;
    }

    /// <inheritdoc />
    public IFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class =>
        new Filterable<T>(
            _oList ?? _list,
            Filter.OpenCollection(propertyExpression)
        );

    /// <inheritdoc />
    public IFilterableConnection<T> CloseCollection<T>()
        where T : class =>
        new FilterableConnection<T>(
            new Filterable<T>(_oList!, Filter.CloseCollection<T>().And())
        );

    /// <inheritdoc />
    public IFilterableConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    /// <inheritdoc />
    public IEnumerator<TClass> GetEnumerator() =>
        (_oList as IEnumerable<TClass> ?? _list)
            .Where(ToExpression().Compile())
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public Expression<Func<TClass, bool>> ToExpression() =>
        Filter.ToExpression();

    IFilter IFilterable.Filter => Filter;

#nullable disable

    // TODO: Adding nullability causes an error, removing
    // `?`'s removes error but causes nullability warning,
    // so added disable statement
    IFilterableConnection IFilterable.Add<TClass1, TPropertyType>(
        Expression<Func<TClass1, TPropertyType>> propertyExpression,
        IOperation operation,
        TPropertyType[] values,
        IFilterStatementOptions options
    )
        where TClass1 : class =>
        Add(
            (propertyExpression as Expression<Func<TClass, TPropertyType>>)!,
            operation,
            values,
            options
        );

#nullable restore

    IFilterable IFilterable.Add(IFilterStatement statement)
    {
        Filter.Add(statement);

        return this;
    }

    IFilterableConnection IFilterable.Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options
    )
        where TPropertyType : default =>
        Add(propertyExpression, operation, values, options);

    IFilterable IFilterable.OpenGroup() => OpenGroup();

    IFilterableConnection IFilterable.CloseGroup() => CloseGroup();

    /// <inheritdoc />
    public IFilterable OpenCollection(string propertyExpression)
    {
        Filter.OpenCollection(propertyExpression);

        return this;
    }

    /// <inheritdoc />
    public IFilterableConnection CloseCollection()
    {
        Filter.CloseCollection();

        return new FilterableConnection<TClass>(this);
    }

    /// <inheritdoc />
    public Expression<Func<TClass1, bool>> ToExpression<TClass1>() =>
        (Filter.ToExpression() as Expression<Func<TClass1, bool>>)!;
}
