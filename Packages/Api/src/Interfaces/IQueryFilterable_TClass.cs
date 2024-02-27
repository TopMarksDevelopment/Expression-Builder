namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public interface IQueryFilterable<TClass> : IQueryable<TClass>, IQueryFilterable
    where TClass : class
{
    new IFilter<TClass> Filter { get; }

    IQueryFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    );

    IQueryFilterableConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    new IQueryFilterableConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    new IQueryFilterable<TClass> SetConnector(Connector connector);

    new IQueryFilterable<TClass> OpenGroup();

    new IQueryFilterableConnection<TClass> CloseGroup();

    IQueryFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class;

    IQueryFilterableConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;
}
