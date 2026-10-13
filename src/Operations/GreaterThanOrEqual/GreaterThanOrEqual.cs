namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is greater than, or equal to, the given value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct GreaterThanOrEqual : IOperation
{
    /// <summary>Creates the <see cref="GreaterThanOrEqual"/> operation.</summary>
    public GreaterThanOrEqual() { }

    /// <inheritdoc />
    public readonly string Name => "GreaterThanOrEqual";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.NotNullAnd,
        };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        !values.Any()
            ? throw new ArgumentOutOfRangeException(
                nameof(values),
                "Must have at least one value"
            )
            : member.WorkOnValues(
                values,
                options?.Match ?? Defaults.Match,
                (m, v) =>
                    Expression.GreaterThanOrEqual(
                        m,
                        Expression.Constant(options.ApplyManipulators(v))
                    )
            );

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
