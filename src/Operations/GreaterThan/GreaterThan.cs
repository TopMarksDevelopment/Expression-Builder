namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is greater than the given value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct GreaterThan : IOperation
{
    /// <summary>Creates the <see cref="GreaterThan"/> operation.</summary>
    public GreaterThan() { }

    /// <inheritdoc />
    public readonly string Name => "GreaterThan";

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
                    Expression.GreaterThan(
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
