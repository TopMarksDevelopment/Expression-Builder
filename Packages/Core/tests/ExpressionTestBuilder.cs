namespace ExpressionBuilder.Tests;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal record ExpressionTestBuilder<T, T2>(
        string Name,
        string Expected,
        Func<IFilter, IFilterConnection> Factory, 
        Func<IFilterable<T>, IFilterableConnection<T>> Filterable,
        Func<IQueryFilterable<T>, IQueryFilterableConnection<T>> QueryFilterable,
        Func<IFilter<T>, IFilterConnection<T>> Fluent,
        Func<T2, string> ToGet
    ) where T : class, IItemable
    where T2 : IFilterItem
    {
        public override string ToString() => Name;
    }