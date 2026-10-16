namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;

internal class FilterItemCollectionJsonConverter
    : JsonConverter<ICollection<IFilterItem>>
{
    /// <summary>
    /// The filter item types, by their <see cref="IFilterItem.typeRef"/>. Built once, as it
    /// only depends on this assembly.
    /// </summary>
    static readonly Lazy<Dictionary<string, Type>> ItemTypes = new(() =>
    {
        var typeDictionary = new Dictionary<string, Type>();

        var types = typeof(FilterItemCollectionJsonConverter)
            .Assembly.GetTypes()
            .Where(typeof(IFilterItem).IsAssignableFrom)
            .Where(p => !p.IsInterface && !p.IsAbstract);

        foreach (var type in types)
        {
            // Generic items (statements) are closed with any type, just to read their typeRef
            var typeToUse = IsAssignableToGenericType(
                type,
                typeof(IFilterStatement<>)
            )
                ? type.MakeGenericType([typeof(string)])
                : type;

            typeDictionary.Add(
                ((IFilterItem)Activator.CreateInstance(typeToUse)!).typeRef,
                type
            );
        }

        return typeDictionary;
    });

    public override ICollection<IFilterItem> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var filterCollection = new List<IFilterItem>();

        var items =
            JsonSerializer.Deserialize<List<JsonDocument>>(ref reader, options)
            ?? throw new JsonException("Failed to parse array");

        foreach (var item in items)
        {
            var itemJson =
                item.RootElement.ToString()
                ?? throw new JsonException("Could not parse JSON");
            var typeString =
                item.RootElement.GetProperty("typeRef").GetString()
                ?? throw new JsonException("A typeRef was not found");

            if (!ItemTypes.Value.TryGetValue(typeString, out Type? value))
                throw new JsonException(
                    $"Could not find the item type for the \"typeRef\" value \"{typeString}\""
                );

            var typeToUse = value;

            if (
                IsAssignableToGenericType(typeToUse, typeof(IFilterStatement<>))
            )
            {
                var dynamicParse = JsonSerializer.Deserialize<PropertyType>(
                    itemJson,
                    options
                )!;

                typeToUse = typeToUse.MakeGenericType([
                    Type.GetType(dynamicParse.propertyType)
                        ?? throw new JsonException(
                            $"Could not find the type \"{dynamicParse.propertyType}\""
                        ),
                ]);
            }

            if (
                JsonSerializer.Deserialize(itemJson, typeToUse, options)
                is not IFilterItem filterItem
            )
                throw new JsonException("Could not parse item");

            filterCollection.Add(filterItem);
        }

        return filterCollection;
    }

    public override void Write(
        Utf8JsonWriter writer,
        ICollection<IFilterItem> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            var itemType = item.GetType();

            if (
                item is IFilterGroup
                || item is IFilterStatement
                || (
                    itemType.IsGenericType
                    && itemType.IsAssignableTo(
                        typeof(IFilterStatement<>).MakeGenericType(
                            itemType.GenericTypeArguments[0]
                        )
                    )
                )
            )
                JsonSerializer.Serialize(writer, item, item.GetType(), options);
            else
                throw new JsonException(
                    "UNKNOWN JSON: item type couldn't be serialized"
                );
        }

        writer.WriteEndArray();
    }

    public static bool IsAssignableToGenericType(
        Type givenType,
        Type genericType
    )
    {
        foreach (var it in givenType.GetInterfaces())
            if (
                it.IsGenericType
                && it.GetGenericTypeDefinition() == genericType
            )
                return true;

        if (
            givenType.IsGenericType
            && givenType.GetGenericTypeDefinition() == genericType
        )
            return true;

        var baseType = givenType.BaseType;

        return baseType == null
            ? false
            : IsAssignableToGenericType(baseType, genericType);
    }

    internal class PropertyType
    {
        public string propertyType { get; set; } = null!;
    }
}
