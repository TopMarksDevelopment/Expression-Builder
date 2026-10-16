namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is between a minimum and a maximum (neither included).</summary>
[GenerateFilterExtensions(FilterExtensionShape.Range)]
public struct BetweenExclusive : IOperation
{
    /// <summary>Creates the <see cref="BetweenExclusive"/> operation.</summary>
    public BetweenExclusive() { }

    /// <inheritdoc />
    public readonly string Name => "BetweenExclusive";

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
        Expression.AndAlso(
            Expression.GreaterThan(
                member,
                Expression.Constant(
                    options.ApplyManipulators(values.ElementAt(0))
                )
            ),
            Expression.LessThan(
                member,
                Expression.Constant(
                    options.ApplyManipulators(values.ElementAt(1))
                )
            )
        );

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Values.Count != 2)
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "Can only have 2 values"
            );
    }
}
