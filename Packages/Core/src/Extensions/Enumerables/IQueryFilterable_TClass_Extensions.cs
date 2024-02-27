namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Enumerables;

public static partial class IQueryFilterable_TClass_Extensions
{
    public static IQueryFilterable<TClass> AsFilterable<TClass>(
        this IQueryable<TClass> list
    ) where TClass : class
        => new QueryFilterable<TClass>(list);
}
