namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public interface IFilterable<TClass> : IEnumerable<TClass>, IFilterable
    where TClass : class
{
    new IFilter<TClass> Filter { get; }

    IFilterable<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    );

    IFilterableConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    new IFilterableConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    new IFilterable<TClass> OpenGroup();

    new IFilterableConnection<TClass> CloseGroup();

    IFilterable<TClass> SetConnector(Connector connector);

    IFilterable<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    IFilterableConnection<T> CloseCollection<T>()
        where T : class;

    IFilterableConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;

    Expression<Func<TClass, bool>> ToExpression();
}
