namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Collections;

internal class FilterCollectionJsonConverter<T>
    : JsonConverter<IFilterCollection<T?>>
{
    public override IFilterCollection<T?>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => JsonSerializer.Deserialize<FilterCollection<T>>(ref reader, options);

    public override void Write(
        Utf8JsonWriter writer,
        IFilterCollection<T?> value,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, value.ToList(), options);
}
