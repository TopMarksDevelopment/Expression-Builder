namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterCollectionJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        var x =  typeToConvert.GetGenericTypeDefinition().IsAssignableTo(typeof(IFilterCollection<>));
        return typeToConvert.GetGenericTypeDefinition().IsAssignableTo(typeof(IFilterCollection<>));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter?)Activator.CreateInstance(
                typeof(FilterCollectionJsonConverter<>).MakeGenericType(
                    [typeToConvert.GetGenericArguments()[0]]
                )
            );
}
