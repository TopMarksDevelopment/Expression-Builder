namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterStatementOptionsJsonConverter
    : JsonConverter<IFilterStatementOptions>
{
    public override IFilterStatementOptions? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) =>
        JsonSerializer.Deserialize<FilterStatementOptions>(ref reader, options);

    public override void Write(
        Utf8JsonWriter writer,
        IFilterStatementOptions value,
        JsonSerializerOptions options
    ) => JsonSerializer.Serialize(writer, value as FilterStatementOptions, options);
}
