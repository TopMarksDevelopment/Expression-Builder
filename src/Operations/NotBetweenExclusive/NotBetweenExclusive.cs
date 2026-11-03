namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is not between a minimum and a maximum (the minimum and maximum are matched).</summary>
[GenerateFilterExtensions(FilterExtensionShape.Range)]
public struct NotBetweenExclusive : IOperation
{
    /// <summary>Creates the <see cref="NotBetweenExclusive"/> operation.</summary>
    public NotBetweenExclusive() { }

    /// <inheritdoc />
    public readonly string Name => "NotBetweenExclusive";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.IsNullOr,
        };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    ) =>
        Expression.Not(
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
