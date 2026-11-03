namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Text.Json.Serialization;

/// <summary>
/// Core functions of a group collection
/// </summary>
public interface IFilterGroup : IFilterItem
{
    /// <summary>
    /// The group that contains this one
    /// </summary>
    [JsonIgnore]
    IFilterGroup? LastGroup { get; set; }

    /// <summary>
    /// Path to the target collection, if we've targeted one
    /// </summary>
    string? ParentPropertyExpression { get; set; }

    /// <summary>
    /// For a group on a collection: whether <see cref="Matches.Any"/> of the items must satisfy the group
    /// (<c>x.Orders.Any(...)</c>), or <see cref="Matches.All"/> of them (<c>x.Orders.All(...)</c>)
    /// </summary>
    /// <remarks>
    /// <c>null</c> means <see cref="Matches.Any"/>, which is what groups have always done. <c>All</c> is also true
    /// of an empty collection. It does nothing for a group that is not on a collection.
    /// </remarks>
    Matches? Match { get; set; }

    /// <summary>
    /// The items within this group
    /// </summary>
    /// <remarks>Usually <see cref="IFilterStatement"/>'s or other <see cref="IFilterGroup"/>s</remarks>
    ICollection<IFilterItem> Items { get; set; }

    /// <summary>
    /// A method that closes this group
    /// </summary>
    /// <returns>The <see cref="LastGroup"/>, if it exists</returns>
    /// <exception cref="InvalidOperationException">No group to return</exception>
    IFilterGroup Close();
}
