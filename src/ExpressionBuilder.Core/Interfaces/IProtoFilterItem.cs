using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A filter item that can be saved in, and loaded from, the compact binary (protobuf) form.
/// </summary>
[ProtoBuf.ProtoContract()]
[ProtoBuf.ProtoInclude(45, typeof(ProtoFilterStatement))]
[ProtoBuf.ProtoInclude(46, typeof(FilterGroup))]
public interface IProtoFilterItem
{
    /// <summary>
    /// Gets the item in a form protobuf can save
    /// </summary>
    /// <returns>The item to save</returns>
    IProtoFilterItem Pack();

    /// <summary>
    /// Gets the filter item back from its saved form
    /// </summary>
    /// <returns>The filter item</returns>
    IFilterItem Unpack();
}
