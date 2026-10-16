namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Text.Json;

/// <summary>
/// What the filter builder posts. Put one on your page model or view model, post the form, then turn
/// it into a <see cref="Filter{TClass}"/> with <see cref="Build{TClass}"/>.
/// </summary>
/// <example>
/// <code>
/// public class ProductSearch
/// {
///     public FilterBuilderModel Filter { get; set; } = new();
/// }
///
/// // In the handler
/// var result = search.Filter.Build&lt;Product&gt;();
///
/// if (!result.Succeeded)
///     return BadRequest(result.Errors);
///
/// var products = db.Products.Where(result.Filter!).ToList();
/// </code>
/// </example>
public class FilterBuilderModel
{
    /// <summary>
    /// The filter, as the script writes it. Leave it alone: it is what is posted.
    /// </summary>
    /// <remarks>
    /// It is a small JSON document of plain text - properties, operations and values - that is checked
    /// against the class being filtered. See <see cref="FilterDocument"/> for its form.
    /// </remarks>
    public string? Json { get; set; }

    /// <summary>
    /// Whether nothing has been posted yet, or the filter has no conditions.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Json))
                return true;

            try
            {
                return JsonSerializer
                    .Deserialize<FilterDocument>(
                        Json,
                        FilterDocument.JsonOptions
                    )
                    ?.Root?.Items
                    is not { Count: > 0 };
            }
            catch (JsonException)
            {
                // Something was posted, even if it is not usable: Build reports what is wrong
                return false;
            }
        }
    }

    /// <summary>
    /// Checks the posted filter and turns it into a filter that can be applied to a list or a query.
    /// </summary>
    /// <typeparam name="TClass">The class being filtered</typeparam>
    /// <param name="options">The settings; the same ones the page was built with</param>
    /// <returns>The filter, or what was wrong with what was posted</returns>
    public FilterBuildResult<TClass> Build<TClass>(
        FilterBuilderOptions? options = null
    )
        where TClass : class
    {
        options ??= new();

        var schema = FilterSchema.For<TClass>(options);
        var (items, errors) = new FilterCompiler(schema, options).Compile(Json);

        return new FilterBuildResult<TClass>(
            items == null ? null : new Filter<TClass>() { Items = items },
            errors
        );
    }

    /// <summary>
    /// Shows an existing filter in the filter builder, such as one that was saved earlier.
    /// </summary>
    /// <typeparam name="TClass">The class being filtered</typeparam>
    /// <param name="filter">The filter</param>
    /// <param name="options">The settings; the same ones the page is built with</param>
    /// <returns>A model to put on the page</returns>
    /// <exception cref="NotSupportedException">
    /// The filter uses something the filter builder does not offer, such as a property that is
    /// ignored
    /// </exception>
    public static FilterBuilderModel FromFilter<TClass>(
        Filter<TClass> filter,
        FilterBuilderOptions? options = null
    )
        where TClass : class
    {
        ArgumentNullException.ThrowIfNull(filter);

        var schema = FilterSchema.For<TClass>(options ?? new());

        return new FilterBuilderModel
        {
            Json = new FilterWriter(schema).Write(filter.Items).ToJson(),
        };
    }
}
