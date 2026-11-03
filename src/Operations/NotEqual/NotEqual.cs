namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value does not equal the given value (or, with several values, none of them).</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct NotEqual : IOperation
{
    /// <summary>Creates the <see cref="NotEqual"/> operation.</summary>
    public NotEqual() { }

    /// <inheritdoc />
    public readonly string Name => "NotEqual";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new() { Match = Matches.All, NullHandler = OperationNullHandler.Skip };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        values.Count == 1
            ? Expression.NotEqual(
                member,
                member.ConstantFor(
                    options.ApplyManipulators(values.ElementAt(0))
                )
            )
        : options?.Match == Matches.Any
            ? member.WorkOnValues(
                values,
                Matches.Any,
                (m, v) =>
                    Expression.NotEqual(
                        m,
                        m.ConstantFor(options?.ApplyManipulators(v))
                    )
            )
        : Expression.Not(new In().Build(member, values, options));

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Must have at least one value"
            );
    }
}
