namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public interface IFilter
{
    ICollection<IFilterItem> Items { get; }

    ICollection<IFilterItem> Current { get; }

    IFilter Add(IFilterStatement statement);

    IFilterConnection Add<TClass, TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
        where TClass : class;

    IFilterConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    IFilter OpenGroup();

    IFilterConnection CloseGroup();

    IFilter OpenCollection(string propertyExpression);

    IFilterConnection CloseCollection();

    string ToString();

    Expression<Func<T, bool>> ToExpression<T>();
}
