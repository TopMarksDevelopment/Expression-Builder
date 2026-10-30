namespace ExpressionBuilder.Benchmarks;

using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Throw-away versions of the pieces the report builder design talks about (docs/architecture/report-builder.md),
/// so the design can be costed before anything is written: a projection to columns chosen at run time, ordering by a
/// column chosen at run time, and paging. None of this is library code.
/// </summary>
static class ReportSpike
{
    static readonly ModuleBuilder Module = AssemblyBuilder
        .DefineDynamicAssembly(
            new AssemblyName("ReportRows"),
            AssemblyBuilderAccess.RunAndCollect
        )
        .DefineDynamicModule("ReportRows");

    static int _emitted;

    /// <summary>A filter on <c>Placed</c> and <c>Priority</c> whose values depend on <paramref name="i"/>.</summary>
    public static Expression<Func<ReportOrder, bool>> Filter(int i)
    {
        var filter = new Filter<ReportOrder>();

        filter
            .Add(
                "Placed",
                Operation.GreaterThanOrEqual,
                [new DateTime(2024, 1, 1).AddDays(i % 600)]
            )
            .And();
        filter.Add("Priority", Operation.GreaterThanOrEqual, [i % 4]);

        return filter.ToExpression();
    }

    /// <summary>The paths of everything in a class that could be a column: not related objects or collections.</summary>
    public static IReadOnlyList<string> ScalarPaths(Type type)
    {
        var schema = FilterSchema.For(type);

        return
        [
            .. schema
                .Types[schema.Root]
                .Fields.Where(f =>
                    f.Kind is not (FieldKind.Object or FieldKind.Collection)
                )
                .Select(f => f.Path),
        ];
    }

    /// <summary>
    /// <c>x => new object[] { x.Reference, x.Customer.Name }</c>, for columns known only at run time.
    /// </summary>
    /// <param name="guarded">Test each related object for null first, as a query over objects in memory needs</param>
    public static Expression<Func<T, object?[]>> ObjectArray<T>(
        IReadOnlyList<string> paths,
        bool guarded
    )
    {
        var x = Expression.Parameter(typeof(T), "x");

        return Expression.Lambda<Func<T, object?[]>>(
            Expression.NewArrayInit(
                typeof(object),
                paths.Select(p => Value(x, p, guarded, typeof(object)))
            ),
            x
        );
    }

    /// <summary>
    /// <c>x => new Row { C0 = x.Reference, C1 = x.Customer.Name }</c>, where <c>Row</c> is a class made for this set of
    /// columns. Also returns the type and a way to read each column of a row.
    /// </summary>
    public static (
        LambdaExpression Selector,
        Func<object, object?>[] Readers
    ) Emitted<T>(IReadOnlyList<string> paths)
    {
        var x = Expression.Parameter(typeof(T), "x");
        var types = paths
            .Select(p => AllowNull(PropertyType(typeof(T), p)))
            .ToArray();

        TypeBuilder builder;

        lock (Module)
            builder = Module.DefineType(
                $"Row{++_emitted}",
                TypeAttributes.Public | TypeAttributes.Sealed
            );

        builder.DefineDefaultConstructor(MethodAttributes.Public);

        for (var i = 0; i < paths.Count; i++)
            DefineProperty(builder, $"C{i}", types[i]);

        var type = builder.CreateType();
        var selector = Expression.Lambda(
            Expression.MemberInit(
                Expression.New(type),
                paths.Select(
                    (p, i) =>
                        Expression.Bind(
                            type.GetProperty($"C{i}")!,
                            Value(x, p, false, types[i])
                        )
                )
            ),
            x
        );

        var row = Expression.Parameter(typeof(object), "row");
        var readers = paths
            .Select(
                (_, i) =>
                    Expression
                        .Lambda<Func<object, object?>>(
                            Expression.Convert(
                                Expression.Property(
                                    Expression.Convert(row, type),
                                    $"C{i}"
                                ),
                                typeof(object)
                            ),
                            row
                        )
                        .Compile()
            )
            .ToArray();

        return (selector, readers);
    }

    /// <summary>A way to read a column from an object of the class itself.</summary>
    public static Func<T, object?>[] Getters<T>(IReadOnlyList<string> paths)
    {
        var x = Expression.Parameter(typeof(T), "x");

        return
        [
            .. paths.Select(p =>
                Expression
                    .Lambda<Func<T, object?>>(
                        Value(x, p, guarded: true, typeof(object)),
                        x
                    )
                    .Compile()
            ),
        ];
    }

    /// <summary>
    /// Orders by the columns chosen, then by <paramref name="tieBreak"/> so that pages do not overlap, then takes a
    /// page.
    /// </summary>
    public static IQueryable<T> OrderAndPage<T>(
        IQueryable<T> query,
        IReadOnlyList<(string Path, bool Descending)> sort,
        string tieBreak,
        int offset,
        int size,
        bool pageAsParameters
    )
    {
        var first = true;

        foreach (var (path, descending) in sort)
        {
            query = OrderBy(query, path, descending, first);
            first = false;
        }

        query = OrderBy(query, tieBreak, false, first);

        return Take(
            Skip(query, offset, pageAsParameters),
            size,
            pageAsParameters
        );
    }

    public static IQueryable Select<T>(
        IQueryable<T> query,
        LambdaExpression selector
    ) =>
        query.Provider.CreateQuery(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Select),
                [typeof(T), selector.ReturnType],
                query.Expression,
                Expression.Quote(selector)
            )
        );

    public static IQueryable<T> OrderBy<T>(
        IQueryable<T> query,
        string path,
        bool descending,
        bool first
    )
    {
        var x = Expression.Parameter(typeof(T), "x");
        Expression key = x;

        foreach (var name in path.Split('.'))
            key = Expression.Property(key, name);

        var method = (first, descending) switch
        {
            (true, false) => nameof(Queryable.OrderBy),
            (true, true) => nameof(Queryable.OrderByDescending),
            (false, false) => nameof(Queryable.ThenBy),
            (false, true) => nameof(Queryable.ThenByDescending),
        };

        return query.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                method,
                [typeof(T), key.Type],
                query.Expression,
                Expression.Quote(Expression.Lambda(key, x))
            )
        );
    }

    static IQueryable<T> Skip<T>(
        IQueryable<T> query,
        int count,
        bool asParameter
    ) =>
        query.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Skip),
                [typeof(T)],
                query.Expression,
                Count(count, asParameter)
            )
        );

    static IQueryable<T> Take<T>(
        IQueryable<T> query,
        int count,
        bool asParameter
    ) =>
        query.Provider.CreateQuery<T>(
            Expression.Call(
                typeof(Queryable),
                nameof(Queryable.Take),
                [typeof(T)],
                query.Expression,
                Count(count, asParameter)
            )
        );

    /// <summary>
    /// A number as a read of a variable, which a query provider turns into a parameter, or as a constant, which it
    /// writes into the query (the same trick as the filter's own values).
    /// </summary>
    static Expression Count(int count, bool asParameter) =>
        asParameter
            ? Expression.Property(
                Expression.Constant(new Holder(count)),
                nameof(Holder.Value)
            )
            : Expression.Constant(count);

    static Expression Value(
        Expression x,
        string path,
        bool guarded,
        Type result
    )
    {
        var current = x;
        var related = new List<Expression>();
        var names = path.Split('.');

        for (var i = 0; i < names.Length; i++)
        {
            current = Expression.Property(current, names[i]);

            if (guarded && i < names.Length - 1 && !current.Type.IsValueType)
                related.Add(current);
        }

        Expression value = Expression.Convert(current, result);

        // Innermost first, so the outermost test runs first
        for (var i = related.Count - 1; i >= 0; i--)
            value = Expression.Condition(
                Expression.Equal(
                    related[i],
                    Expression.Constant(null, related[i].Type)
                ),
                Expression.Constant(null, result),
                value
            );

        return value;
    }

    static Type PropertyType(Type type, string path)
    {
        foreach (var name in path.Split('.'))
            type = type.GetProperty(name)!.PropertyType;

        return type;
    }

    static Type AllowNull(Type type) =>
        type.IsValueType && System.Nullable.GetUnderlyingType(type) == null
            ? typeof(Nullable<>).MakeGenericType(type)
            : type;

    static void DefineProperty(TypeBuilder builder, string name, Type type)
    {
        const MethodAttributes Accessor =
            MethodAttributes.Public
            | MethodAttributes.SpecialName
            | MethodAttributes.HideBySig;

        var field = builder.DefineField(
            "_" + name,
            type,
            FieldAttributes.Private
        );
        var property = builder.DefineProperty(
            name,
            PropertyAttributes.None,
            type,
            null
        );

        var get = builder.DefineMethod(
            "get_" + name,
            Accessor,
            type,
            Type.EmptyTypes
        );
        var il = get.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ret);

        var set = builder.DefineMethod("set_" + name, Accessor, null, [type]);
        il = set.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, field);
        il.Emit(OpCodes.Ret);

        property.SetGetMethod(get);
        property.SetSetMethod(set);
    }

    internal sealed class Holder(int value)
    {
        public int Value { get; } = value;
    }
}
