namespace ExpressionBuilder.Tests;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Enumerables;
using TopMarksDevelopment.ExpressionBuilder.Factories;

public static class TestExtensions
{
    #region Private methods

    public static IFilter Get<TClass>(
        this Func<IFilter, IFilterConnection> filter
    )
        where TClass : class => filter.Invoke(IFilterFor<TClass>()).And();

    public static IFilter<TClass> Get<TClass>(
        this Func<IFilter<TClass>, IFilterConnection<TClass>> filter
    )
        where TClass : class => filter.Invoke(new Filter<TClass>()).And();

    public static IFilterable<TClass> Get<TClass>(
        this Func<
            IFilterable<TClass>,
            IFilterableConnection<TClass>
        > filterable,
        IEnumerable<TClass>? filter
    )
        where TClass : class =>
        filterable
            .Invoke(filter?.ToFilterable() ?? new Filterable<TClass>([]))
            .And();

    public static IQueryFilterable<TClass> Get<TClass>(
        this Func<
            IQueryFilterable<TClass>,
            IQueryFilterableConnection<TClass>
        > queryFilterable,
        IQueryable<TClass>? filter
    )
        where TClass : class =>
        queryFilterable
            .Invoke(
                (filter ?? new HashSet<TClass>().AsQueryable()).AsFilterable()
            )
            .And();

    public static IQueryFilterable<TClass> Get<TClass>(
        this Func<
            IQueryFilterable<TClass>,
            IQueryFilterableConnection<TClass>
        > queryFilterable,
        IEnumerable<TClass>? filter
    )
        where TClass : class =>
        queryFilterable
            .Invoke((filter ?? []).AsQueryable().AsFilterable())
            .And();

    static IFilter IFilterFor<ModelType>() =>
        FilterFactory.Create(typeof(ModelType));

    #endregion Private methods

    #region MatchStrings

    /* Pretty redundant method, however it shows tests are matching the same type/struct */
    public static string ToMatchString<T>(
        this Expression<Func<T, bool>> expression,
        Func<string, string>? applyReplacements = null
    )
        where T : class =>
        (applyReplacements ?? ((s) => s)).Invoke(expression.ToString());

    public static string ToMatchString<T>(this IFilter<T> filter)
        where T : class => filter.ToExpression().ToString();

    public static string ToMatchString<T>(this IFilterable<T> filterable)
        where T : class => filterable.Filter.ToMatchString();

    public static string ToMatchString<TClass>(
        this Func<IFilter, IFilterConnection> filter
    )
        where TClass : class =>
        filter.Get<TClass>().ToExpression<TClass>().ToMatchString();

    public static string ToMatchString<TClass>(
        this Func<IFilter<TClass>, IFilterConnection<TClass>> filter
    )
        where TClass : class => filter.Get().ToMatchString();

    public static string ToMatchString<TClass>(
        this Func<IFilterable<TClass>, IFilterableConnection<TClass>> filterable
    )
        where TClass : class => filterable.Get([]).ToMatchString();

    public static string ToMatchString<TClass>(
        this Func<
            IQueryFilterable<TClass>,
            IQueryFilterableConnection<TClass>
        > queryFilterable
    )
        where TClass : class => queryFilterable.Get([]).Filter.ToMatchString();

    #endregion MatchStrings

    #region Results

    public static IEnumerable<int> ToResult<TClass>(
        this Expression<Func<TClass, bool>> filter,
        IEnumerable<TClass> items
    )
        where TClass : class, IItemable =>
        items.Where(filter.Compile()).Select(x => x.Id).ToArray();

    public static IEnumerable<int> ToResults<TClass>(
        this Func<IFilter, IFilterConnection> filter,
        IEnumerable<TClass> items
    )
        where TClass : class, IItemable =>
        items
            .Where(filter.Get<TClass>().ToExpression<TClass>().Compile())
            .Select(x => x.Id)
            .ToArray();

    public static IEnumerable<int> ToResults<TClass>(
        this Func<IFilter<TClass>, IFilterConnection<TClass>> filter,
        IEnumerable<TClass> items
    )
        where TClass : class, IItemable =>
        items
            .Where(filter.Get().ToExpression<TClass>().Compile())
            .Select(x => x.Id)
            .ToArray();

    public static IEnumerable<int> ToResults<TClass>(
        this Func<IFilterable<TClass>, IFilterableConnection<TClass>> filter,
        IEnumerable<TClass> items
    )
        where TClass : class, IItemable =>
        filter.Get(items).Select(x => x.Id).ToArray();

    public static IEnumerable<int> ToResults<TClass>(
        this Func<
            IQueryFilterable<TClass>,
            IQueryFilterableConnection<TClass>
        > filter,
        IEnumerable<TClass> items
    )
        where TClass : class, IItemable
    {
        var x = filter.Get(items);
        var y = x.Select(x => x.Id);

        return filter.Get(items).Select(x => x.Id).ToArray();
    }

    public static IEnumerable<int> ToResults<TClass>(
        this Func<
            IQueryFilterable<TClass>,
            IQueryFilterableConnection<TClass>
        > filter,
        IQueryable<TClass> items
    )
        where TClass : class, IItemable
    {
        var x = filter.Get(items);
        var y = x.Select(x => x.Id);

        return filter.Get(items).Select(x => x.Id).ToArray();
    }

    #endregion Results
}
