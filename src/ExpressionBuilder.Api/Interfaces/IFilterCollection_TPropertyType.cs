namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A collection applied on a <see cref="IFilterStatement"/>
/// </summary>
/// <typeparam name="TPropertyType">The type of the values inside the collection</typeparam>
public interface IFilterCollection<TPropertyType>
    : IFilterCollection,
        ICollection<TPropertyType?>
{
    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the collection
    /// </returns>
    new int Count { get; }

    /// <summary>
    /// Adds a set of items to the collection.
    /// </summary>
    /// <param name="items">The item set we're adding</param>
    void AddRange(IEnumerable<TPropertyType?> items);
}
