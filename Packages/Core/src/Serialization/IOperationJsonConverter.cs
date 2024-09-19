namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class IOperationJsonConverter : JsonConverter<IOperation>
{
    ICollection<Type> _foundTypes = [];

    public override IOperation Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        SerializerBase.FindOperationByName(reader.GetString(), ref _foundTypes);

    public override void Write(
        Utf8JsonWriter writer,
        IOperation value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value.Name);
    }
}
