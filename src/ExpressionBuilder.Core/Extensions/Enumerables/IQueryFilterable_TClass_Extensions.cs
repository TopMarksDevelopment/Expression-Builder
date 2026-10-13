namespace TopMarksDevelopment.ExpressionBuilder;

using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Enumerables;

public static partial class IQueryFilterable_TClass_Extensions
{
    /// <summary>
    /// Allow the IQueryable to have a filter built against it
    /// </summary>
    /// <typeparam name="TClass">The base class of the query</typeparam>
    /// <param name="list">The queryable we're filtering against</param>
    /// <returns>A filterable queryable expression</returns>
    public static IQueryFilterable<TClass> AsFilterable<TClass>(
        this IQueryable<TClass> list
    )
        where TClass : class => new QueryFilterable<TClass>(list);
}
