namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

/// <summary>The text starts with the value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct StartsWith : IOperation
{
    /// <summary>Creates the <see cref="StartsWith"/> operation.</summary>
    public StartsWith() { }

    /// <inheritdoc />
    public readonly string Name => "StartsWith";

    /// <inheritdoc />
    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.Any,
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
            : member
                .ToTrimLowerStringExpression()
                .WorkOnValues(
                    values,
                    options?.Match ?? Defaults.Match,
                    (m, v) =>
                        Expression.Call(
                            m,
                            H.StartsWithMethod,
                            v.ToTrimLowerStringConstant()
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
