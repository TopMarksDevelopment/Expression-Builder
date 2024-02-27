namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Collections;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterCollectionJsonConverter<T> : JsonConverter<IFilterCollection<T?>>
{
    public override IFilterCollection<T?>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<FilterCollection<T>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IFilterCollection<T?> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}
