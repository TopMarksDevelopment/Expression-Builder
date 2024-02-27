namespace ExpressionBuilder.Tests;

using System;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

public record TestBuilder<T>(
    string Name, 
    Expression<Func<T, bool>> Expected, 
    Func<IFilter, IFilterConnection> Factory, 
    Func<IFilterable<T>, IFilterableConnection<T>> Filterable,
    Func<IQueryFilterable<T>, IQueryFilterableConnection<T>> QueryFilterable,
    Func<IFilter<T>, IFilterConnection<T>> Fluent
) where T : class, IItemable
{
    public override string ToString() => Name;
}