namespace TopMarksDevelopment.ExpressionBuilder.Enumerables;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class QueryFilterable<TClass> : IQueryFilterable<TClass>
    where TClass : class
{
    IQueryable? _oList;
    IQueryable<TClass> _list;

    public IFilter<TClass> Filter { get; set; }

    QueryFilterable()
    {
        _list = new List<TClass>().AsQueryable();

        Filter = new Filter<TClass>();
    }

    QueryFilterable(IQueryable original, IFilter<TClass> filter)
    {
        _oList = original;
        _list = null!;

        Filter = filter;
    }

    public QueryFilterable(IQueryable<TClass> list)
    {
        _list = list;

        Filter = new Filter<TClass>();
    }

    public Type ElementType => typeof(TClass);

    IQueryable<TClass> FilterList => (IQueryable<TClass>?)_oList ?? _list;

    public Expression Expression =>
        FilterList.Where(Filter.ToExpression()).Expression;

    public IQueryProvider Provider => FilterList.Provider;

    #region Add methods

    public IQueryFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    )
    {
        Filter.Add(statement);

        return this;
    }

    public IQueryFilterableConnection<TClass> Add<TPropertyType>(
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

        return new QueryFilterableConnection<TClass>(this);
    }

    public IQueryFilterableConnection<TClass> Add<TPropertyType>(
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

        return new QueryFilterableConnection<TClass>(this);
    }

    #endregion Add methods

    public IQueryFilterable<TClass> OpenGroup()
    {
        Filter.OpenGroup();

        return this;
    }

    public IQueryFilterableConnection<TClass> CloseGroup()
    {
        Filter.CloseGroup();

        return new QueryFilterableConnection<TClass>(this);
    }

    public IQueryFilterable<TClass> SetConnector(Connector connector)
    {
        Filter.SetLastConnector(connector);

        return this;
    }

    public IQueryFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class =>
        new QueryFilterable<T>(
            _oList ?? _list,
            Filter.OpenCollection<T>(propertyExpression)
        );

    public IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class =>
        new QueryFilterableConnection<T>(
            new QueryFilterable<T>(_oList!, Filter.CloseCollection<T>().And())
        );

    public IQueryFilterableConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    public IEnumerator<TClass> GetEnumerator() =>
        FilterList.Where(Filter.ToExpression()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IFilter IQueryFilterable.Filter => Filter;

    IQueryFilterable IQueryFilterable.Add(IFilterStatement statement)
    {
        Filter.Add(statement);

        return this;
    }

    IQueryFilterableConnection IQueryFilterable.Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options
    )
        where TPropertyType : default =>
        Add(propertyExpression, operation, values, options);

#nullable disable

    // TODO: Adding nullability causes an error, removing
    // `?`'s removes error but causes nullability warning,
    // so added disable statement
    IQueryFilterableConnection IQueryFilterable.Add<TClass1, TPropertyType>(
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

    IQueryFilterable IQueryFilterable.SetConnector(Connector connector) =>
        SetConnector(connector);

    IQueryFilterable IQueryFilterable.OpenGroup() => OpenGroup();

    IQueryFilterableConnection IQueryFilterable.CloseGroup() => CloseGroup();

    public IQueryFilterable OpenCollection(string propertyExpression) =>
        OpenCollection(propertyExpression);

    public IQueryFilterableConnection CloseCollection() => CloseCollection();
}
