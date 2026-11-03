namespace TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// How null handling is done when applying the operation
/// </summary>
public enum OperationNullHandler
{
    /// <summary>
    /// Skip the null checks completely
    /// </summary>
    Skip,

    /// <summary>
    /// The property must not be null and the operation applies
    /// </summary>
    NotNullAnd,

    /// <summary>
    /// The property is null or matches the operation
    /// </summary>
    IsNullOr,
}
