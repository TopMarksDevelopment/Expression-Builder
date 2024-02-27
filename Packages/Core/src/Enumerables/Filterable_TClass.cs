namespace TopMarksDevelopment.ExpressionBuilder.Enumerables;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using TopMarksDevelopment.ExpressionBuilder.Api;

public class Filterable<TClass> : IFilterable<TClass>
    where TClass : class
{
    readonly IEnumerable? _oList;
    readonly IEnumerable<TClass> _list;

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

    public IFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    )
    {
        Filter.Add(statement);

        return this;
    }

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

    public IFilterable<TClass> OpenGroup()
    {
        Filter.OpenGroup();

        return this;
    }

    public IFilterableConnection<TClass> CloseGroup()
    {
        Filter.CloseGroup();

        return new FilterableConnection<TClass>(this);
    }

    public IFilterable<TClass> SetConnector(Connector connector)
    {
        Filter.SetLastConnector(connector);

        return this;
    }

    public IFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class =>
        new Filterable<T>(
            _oList ?? _list,
            Filter.OpenCollection(propertyExpression)
        );

    public IFilterableConnection<T> CloseCollection<T>()
        where T : class =>
        new FilterableConnection<T>(
            new Filterable<T>(_oList!, Filter.CloseCollection<T>().And())
        );

    public IFilterableConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    public IEnumerator<TClass> GetEnumerator() =>
        (_oList as IEnumerable<TClass> ?? _list)
            .Where(ToExpression().Compile())
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

    IFilterableConnection IFilterable.CloseGroup() => CloseCollection();

    public IFilterable OpenCollection(string propertyExpression) =>
        OpenCollection(propertyExpression);

    public IFilterableConnection CloseCollection() => CloseCollection();

    public Expression<Func<TClass1, bool>> ToExpression<TClass1>() =>
        (Filter.ToExpression() as Expression<Func<TClass1, bool>>)!;
}
