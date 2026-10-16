namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Api.Helpers;

/// <summary>The text does not end with the value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct DoesNotEndWith : IOperation
{
    /// <summary>Creates the <see cref="DoesNotEndWith"/> operation.</summary>
    public DoesNotEndWith() { }

    /// <inheritdoc />
    public readonly string Name => "DoesNotEndWith";

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
                            HelperMethods.EndsWithMethod,
                            v.ToTrimLowerStringConstant(options)
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
