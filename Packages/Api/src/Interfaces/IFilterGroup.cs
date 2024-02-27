namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public interface IFilterGroup : IFilterItem
{
    [JsonIgnore]
    IFilterGroup? LastGroup { get; set; }

    string? ParentPropertyExpression { get; set; }

    ICollection<IFilterItem> Items { get; set; }

    IFilterGroup Close();
}
