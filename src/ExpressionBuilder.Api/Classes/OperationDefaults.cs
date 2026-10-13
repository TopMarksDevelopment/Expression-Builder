namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// The default values for operations
/// </summary>
public class OperationDefaults
{
    /// <summary>
    /// If there are multiple values, are we matching
    /// <see cref="Matches.Any"/> or <see cref="Matches.All"/> of them
    /// </summary>
    public Matches Match { get; init; }

    /// <summary>
    /// If we're using a chain of properties, like <c>Birth.Country</c>,
    /// state if we should skip null checks
    /// </summary>
    public OperationNullHandler NullHandler { get; init; }
}
