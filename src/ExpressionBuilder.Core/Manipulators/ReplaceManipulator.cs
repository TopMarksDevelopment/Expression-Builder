using System.Linq.Expressions;
using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// A manipulator that replaces text, as <c>string.Replace</c> does, in the property and values.
/// </summary>
public struct ReplaceManipulator : IEntityManipulator
{
    static readonly MethodInfo _method = typeof(string).GetMethod(
        "Replace",
        [typeof(string), typeof(string)]
    )!;

    /// <inheritdoc />
    public readonly string Name => "ReplaceManipulator";

    /// <inheritdoc />
    public readonly Type[] ExpectedTypes => [typeof(string), typeof(string)];

    /// <inheritdoc />
    public object?[] Arguments { get; set; }

    /// <summary>
    /// Creates a manipulator whose arguments are set later (for serialisers)
    /// </summary>
    public ReplaceManipulator() => Arguments = [];

    /// <summary>
    /// Creates a manipulator
    /// </summary>
    /// <param name="searchterm">The text to find</param>
    /// <param name="replacementTerm">The text to replace it with</param>
    public ReplaceManipulator(string searchterm, string replacementTerm) =>
        Arguments = [searchterm, replacementTerm];

    /// <inheritdoc />
    public readonly Expression ManipulateMember(Expression member) =>
        Expression.Call(
            member.ToStringExpression(),
            _method,
            Arguments.Select(Expression.Constant)
        );

    /// <inheritdoc />
    public readonly object? ManipulateValue<TPropertyType>(
        TPropertyType? value
    ) =>
        _method.Invoke(
            typeof(TPropertyType) == typeof(string) ? value : value?.ToString(),
            Arguments
        );

    /// <inheritdoc />
    public readonly void Validate()
    {
        if (Arguments.Length != 2)
            throw new InvalidDataException("Only 2 values are permitted");
    }
}
