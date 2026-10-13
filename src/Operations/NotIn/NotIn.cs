namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The value is not one of a list of values.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct NotIn : IOperation
{
    /// <summary>Creates the <see cref="NotIn"/> operation.</summary>
    public NotIn() { }

    /// <inheritdoc />
    public readonly string Name => "NotIn";

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
    ) => Expression.Not(new In().Build(member, values, options));

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement)
    {
        if (statement.Options?.Match == Matches.Any)
            throw new ArgumentException(
                nameof(statement.Options.Match),
                "This method can only match `All`"
            );

        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be values to match"
            );
    }
}
