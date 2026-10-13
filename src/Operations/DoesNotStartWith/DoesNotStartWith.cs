namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Api.Helpers;

/// <summary>The text does not start with the value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct DoesNotStartWith : IOperation
{
    /// <summary>Creates the <see cref="DoesNotStartWith"/> operation.</summary>
    public DoesNotStartWith() { }

    /// <inheritdoc />
    public readonly string Name => "DoesNotStartWith";

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
        member
            .ToTrimLowerStringExpression()
            .WorkOnValues(
                values,
                options?.Match ?? Defaults.Match,
                (m, v) =>
                    Expression.Not(
                        Expression.Call(
                            m,
                            HelperMethods.StartsWithMethod,
                            v.ToTrimLowerStringConstant()
                        )
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
