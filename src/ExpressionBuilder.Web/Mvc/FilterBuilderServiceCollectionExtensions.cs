namespace Microsoft.Extensions.DependencyInjection;

using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// Registers the filter builder.
/// </summary>
public static class FilterBuilderServiceCollectionExtensions
{
    /// <summary>
    /// Registers the settings of the filter builder. Optional: without it, the defaults are used.
    /// </summary>
    /// <remarks>
    /// Inject <c>IOptions&lt;FilterBuilderOptions&gt;</c> where you call
    /// <see cref="FilterBuilderModel.Build{TClass}"/>, so that the page and the check of what it
    /// posts use the same settings.
    /// </remarks>
    /// <param name="services">The services</param>
    /// <param name="configure">Changes the settings</param>
    /// <returns>The services, so calls can be chained</returns>
    public static IServiceCollection AddFilterBuilder(
        this IServiceCollection services,
        Action<FilterBuilderOptions>? configure = null
    )
    {
        var builder = services.AddOptions<FilterBuilderOptions>();

        if (configure != null)
            builder.Configure(configure);

        return services;
    }
}
