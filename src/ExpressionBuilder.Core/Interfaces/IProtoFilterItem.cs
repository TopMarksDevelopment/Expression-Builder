using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

[ProtoBuf.ProtoContract()]
[ProtoBuf.ProtoInclude(45, typeof(ProtoFilterStatement))]
[ProtoBuf.ProtoInclude(46, typeof(FilterGroup))]
public interface IProtoFilterItem
{
    IProtoFilterItem Pack();
    IFilterItem Unpack();
}
