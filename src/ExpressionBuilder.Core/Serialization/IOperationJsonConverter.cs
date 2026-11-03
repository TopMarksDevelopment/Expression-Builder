namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class IOperationJsonConverter : JsonConverter<IOperation>
{
    public override IOperation Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => SerializerBase.FindOperationByName(reader.GetString());

    public override void Write(
        Utf8JsonWriter writer,
        IOperation value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value.Name);
    }
}
