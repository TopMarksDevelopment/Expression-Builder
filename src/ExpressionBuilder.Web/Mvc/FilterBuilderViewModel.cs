namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// What the filter builder's view shows.
/// </summary>
/// <remarks>
/// If you replace <c>Views/Shared/Components/FilterBuilder/Default.cshtml</c> with your own markup,
/// this is its model. Keep the <c>data-eb-*</c> attributes: they are how the script finds its parts.
/// </remarks>
public sealed class FilterBuilderViewModel
{
    /// <summary>
    /// The id of the element that holds the filter builder.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The name of the hidden input that is posted, such as <c>Filter.Json</c>.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The filter to start with, as <see cref="FilterBuilderModel.Json"/>.
    /// </summary>
    public string? Json { get; init; }

    /// <summary>
    /// What can be filtered, in the form the script reads (JSON).
    /// </summary>
    public required string SchemaJson { get; init; }

    /// <summary>
    /// The text of the filter builder.
    /// </summary>
    public required FilterBuilderLabels Labels { get; init; }
}
