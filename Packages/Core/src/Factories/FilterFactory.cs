namespace TopMarksDevelopment.ExpressionBuilder.Factories;

using System;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// Factory methods for creating filters
/// </summary>
public class FilterFactory
{
    /// <summary>
    /// Creates a Filter{TClass} by passing the 'TClass' type.
    /// </summary>
    /// <param name="type">The class type for the filter</param>
    /// <returns>A filter for the specified type</returns>
    /// <exception cref="NullReferenceException">We could not create for that type</exception>
    public static IFilter Create(Type type) =>
        (IFilter?)
            Activator.CreateInstance(typeof(Filter<>).MakeGenericType(type))
        ?? throw new NullReferenceException(nameof(type));
}
