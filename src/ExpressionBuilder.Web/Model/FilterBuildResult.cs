namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Something wrong with a posted filter.
/// </summary>
/// <param name="Path">Where, such as <c>root.items[2].values[0]</c></param>
/// <param name="Message">What is wrong, in words a user can act on</param>
public readonly record struct FilterBuildError(string Path, string Message)
{
    /// <inheritdoc />
    public override string ToString() => $"{Path}: {Message}";
}

/// <summary>
/// The outcome of turning a posted filter into a <see cref="Filter{TClass}"/>.
/// </summary>
/// <typeparam name="TClass">The class being filtered</typeparam>
public sealed class FilterBuildResult<TClass>
    where TClass : class
{
    internal FilterBuildResult(
        Filter<TClass>? filter,
        IReadOnlyList<FilterBuildError> errors
    )
    {
        Filter = filter;
        Errors = errors;
    }

    /// <summary>
    /// The filter, if there were no errors.
    /// </summary>
    public Filter<TClass>? Filter { get; }

    /// <summary>
    /// What is wrong with the posted filter. Empty if it was fine.
    /// </summary>
    public IReadOnlyList<FilterBuildError> Errors { get; }

    /// <summary>
    /// Whether the filter was built. Nothing posted counts as success: a filter with no conditions,
    /// that matches everything.
    /// </summary>
    public bool Succeeded => Filter != null;

    /// <summary>
    /// Gets the filter, or says what was wrong with it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The posted filter was not valid</exception>
    public Filter<TClass> GetFilterOrThrow() =>
        Filter
        ?? throw new InvalidOperationException(
            "The posted filter is not valid: " + string.Join("; ", Errors)
        );
}
