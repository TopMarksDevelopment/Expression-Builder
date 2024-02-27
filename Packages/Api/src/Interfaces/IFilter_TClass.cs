namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public partial interface IFilter<TClass> : IFilter
    where TClass : class
{
    IFilter<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    );

    IFilterConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    new IFilterConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    void SetLastConnector(Connector connector);

    new IFilter<TClass> OpenGroup();

    new IFilterConnection<TClass> CloseGroup();

    IFilter<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class;

    IFilterConnection<T> CloseCollection<T>()
        where T : class;

    IFilterConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;

    Expression<Func<TClass, bool>> ToExpression();
}
