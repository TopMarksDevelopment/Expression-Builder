namespace TopMarksDevelopment.ExpressionBuilder.Web;

using Microsoft.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// Reports a filter that could not be built back to the user.
/// </summary>
public static class FilterBuildResultExtensions
{
    /// <summary>
    /// Adds what was wrong with the posted filter to the model state, so the page can show it.
    /// </summary>
    /// <typeparam name="TClass">The class being filtered</typeparam>
    /// <param name="modelState">The model state of the request</param>
    /// <param name="key">The name of the filter on the model, such as <c>Filter</c></param>
    /// <param name="result">The result of <see cref="FilterBuilderModel.Build{TClass}"/></param>
    public static void AddFilterErrors<TClass>(
        this ModelStateDictionary modelState,
        string key,
        FilterBuildResult<TClass> result
    )
        where TClass : class
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(result);

        foreach (var error in result.Errors)
            modelState.AddModelError(key, error.Message);
    }
}
