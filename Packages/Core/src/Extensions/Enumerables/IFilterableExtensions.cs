namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Enumerables;

public static class IFilterableExtensions
{
    public static IFilterable<T> ToFilterable<T>(
        this IEnumerable<T> list
    ) where T : class
        => new Filterable<T>(list);

    public static IEnumerable<TClass> Filter<TClass>(
        this IEnumerable<TClass> list,
        Expression<Func<IFilterable<TClass>, IFilterableConnection<TClass>>> func
    ) where TClass : class
        => func.Compile().Invoke(new Filterable<TClass>(list)).And();

    public static IFilterableConnection<TClass> Filter<TClass, TSubClass>(
        this IFilterable<TClass> list,
        Expression<Func<TClass, IEnumerable<TSubClass>>> child,
        Expression<Func<IFilterable<TSubClass>, IFilterableConnection<TSubClass>>> func
    ) where TClass : class where TSubClass : class
        => func.Compile().Invoke(list.OpenCollection(child)).CloseCollection<TClass>();
}