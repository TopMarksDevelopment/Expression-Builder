namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value equals the given value (or, with several values, any of them).</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public partial struct Equal : IOperation
{
    /// <summary>Creates the <see cref="Equal"/> operation.</summary>
    public Equal() { }

    /// <inheritdoc />
    public readonly string Name => "Equal";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.Any, NullHandler = OperationNullHandler.Skip };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    )
    {
        if (values.Count > 1)
            return new In().Build(member, values, options);

        Expression value = Expression.Constant(values.First());

        // The member's own type is the one to compare as: a nullable member (`int?`) can't be compared to
        // a plain value (`int`), and `TPropertyType?` is just `int` when the type isn't known to be nullable
        if (
            member.Type != value.Type
            && Nullable.GetUnderlyingType(value.Type) == null
        )
            value = Expression.Convert(value, member.Type);

        return Expression.Equal(member, value);
    }

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );

        if (
            statement.Options?.Match == Matches.All
            && statement.Values.Count > 1
        )
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "No more than one value when matching `All`"
            );
    }
}
