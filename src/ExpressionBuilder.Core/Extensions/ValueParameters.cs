namespace TopMarksDevelopment.ExpressionBuilder.Extensions;

using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// A value held where a query provider can see it is a value, not part of the query's shape.
/// </summary>
/// <remarks>
/// Entity Framework Core turns a captured variable (<c>holder.Value</c>) into a parameter of the SQL, but a constant
/// into text in the SQL. Parameters let the database and EF reuse one plan and one cache entry for every value, where
/// constants make each different value its own query.
/// </remarks>
internal interface IValueHolder
{
    object? Value { get; }
}

/// <inheritdoc cref="IValueHolder" />
internal sealed class ValueHolder<T>(T value) : IValueHolder
{
    /// <summary>The value. A property, not a field, so it reads like one of the filter's own members.</summary>
    public T Value { get; } = value;

    object? IValueHolder.Value => Value;
}

/// <summary>
/// Moves the values of a filter out of its expression, so a database sees them as parameters.
/// </summary>
internal static class ValueParameters
{
    static readonly ConcurrentDictionary<Type, ConstructorInfo> Holders = new();

    /// <summary>
    /// Replaces each constant that is a value to compare with (text, numbers, dates, ids, enums, and lists of
    /// those) with a read of a variable that holds it.
    /// </summary>
    /// <remarks>
    /// Left as constants: <c>null</c>, <c>true</c>/<c>false</c> and the empty text, which are part of the shape of a
    /// query (<c>IS NULL</c>, a condition that is always true, an empty-text test) and which EF simplifies when it
    /// can see them; and anything that is not a plain value.
    /// </remarks>
    internal static Expression<Func<T, bool>> Parameterise<T>(
        Expression<Func<T, bool>> expression
    ) => (Expression<Func<T, bool>>)new Parameteriser().Visit(expression);

    /// <summary>
    /// The opposite: puts the values back into the expression as constants.
    /// </summary>
    internal static Expression<Func<T, bool>> Inline<T>(
        Expression<Func<T, bool>> expression
    ) => (Expression<Func<T, bool>>)new Inliner().Visit(expression);

    static bool IsPlainValue(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive && type != typeof(bool)
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }

    static bool ShouldParameterise(ConstantExpression constant)
    {
        if (constant.Value is null)
            return false;

        var type = constant.Type;

        if (constant.Value is string text)
            return text.Length > 0 && type == typeof(string);

        if (IsPlainValue(type))
            return true;

        // A list of plain values, such as the choices of "is one of"
        var item = type.IsArray ? type.GetElementType() : ItemOf(type);

        return item != null && item != typeof(bool) && IsPlainValue(item);
    }

    /// <summary>The type of the items, if this is a list of one kind of thing (and not a dictionary).</summary>
    static Type? ItemOf(Type type)
    {
        var items = type.GetInterfaces()
            .Append(type)
            .Where(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            )
            .Select(i => i.GetGenericArguments()[0])
            .Distinct()
            .ToList();

        return items.Count == 1 && !typeof(IDictionary).IsAssignableFrom(type)
            ? items[0]
            : null;
    }

    sealed class Parameteriser : ExpressionVisitor
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (!ShouldParameterise(node))
                return node;

            var holder = Holders
                .GetOrAdd(
                    node.Type,
                    t =>
                        typeof(ValueHolder<>)
                            .MakeGenericType(t)
                            .GetConstructor([t])!
                )
                .Invoke([node.Value]);

            return Expression.Property(Expression.Constant(holder), "Value");
        }
    }

    sealed class Inliner : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node) =>
            node
                is {
                    Expression: ConstantExpression
                    {
                        Value: IValueHolder holder
                    },
                }
                ? Expression.Constant(holder.Value, node.Type)
                : base.VisitMember(node);
    }
}
