namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Enumerables;

public static class IFilterableExtensions
{
    /// <summary>
    /// Convert an IEnumerable to a filterable list
    /// </summary>
    /// <typeparam name="T">The type of the enumerable</typeparam>
    /// <param name="list">The list we're going to filter against</param>
    /// <returns>A filterable enumerable</returns>
    public static IFilterable<T> ToFilterable<T>(this IEnumerable<T> list)
        where T : class => new Filterable<T>(list);

    /// <summary>
    /// Apply a filter to the class, using a func/expression
    /// </summary>
    /// <typeparam name="TClass">The class' type</typeparam>
    /// <param name="list">The list we're going to build the filter against</param>
    /// <param name="func">The function to apply against the list</param>
    /// <returns></returns>
    public static IEnumerable<TClass> Filter<TClass>(
        this IEnumerable<TClass> list,
        Expression<
            Func<IFilterable<TClass>, IFilterableConnection<TClass>>
        > func
    )
        where TClass : class =>
        func.Compile().Invoke(new Filterable<TClass>(list)).And();

    /// <summary>
    /// Apply a filter to a child of the class, using a func/expression
    /// </summary>
    /// <typeparam name="TClass">The list class' type</typeparam>
    /// <typeparam name="TSubClass">The type of the child</typeparam>
    /// <param name="list">The list we're going to filter against</param>
    /// <param name="child">The function that points to the enumerable child</param>
    /// <param name="func">The function we're going to apply to child enumerable</param>
    /// <returns></returns>
    public static IFilterableConnection<TClass> Filter<TClass, TSubClass>(
        this IFilterable<TClass> list,
        Expression<Func<TClass, IEnumerable<TSubClass>>> child,
        Expression<
            Func<IFilterable<TSubClass>, IFilterableConnection<TSubClass>>
        > func
    )
        where TClass : class
        where TSubClass : class =>
        func.Compile()
            .Invoke(list.OpenCollection(child))
            .CloseCollection<TClass>();
}
