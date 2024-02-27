namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using ProtoBuf.Meta;

public interface ISerializer
{
    void PrepForSerialisation(
        MetaType fI,
        ref ICollection<Type> fTypes
        // MetaType mn,
        // ref ICollection<Type> mTypes
    );
}
