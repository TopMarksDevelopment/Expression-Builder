namespace ExpressionBuilder.Tests;

using System.IO;
using System.Text.Json;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using Xunit;

[Collection("Serialization checks")]
public class SerializationTests
{
    [Theory(DisplayName = "Multi Collection Test")]
    [ClassData(typeof(SerializationTestData))]
    public void MultiCollectionTest(Filter<Category> filter)
    {
        var jsonString = JsonSerializer.Serialize(filter);
        var filterFromJson = JsonSerializer.Deserialize<Filter<Category>>(
            jsonString
        );

        Assert.Equal(filter.ToString(), filterFromJson?.ToString());
    }

    [Theory(DisplayName = "Protobuf File Test")]
    [ClassData(typeof(SerializationTestData))]
    public void ProtobufFileTest(Filter<Category> filter)
    {
        using (var file = File.Create("../../Test.dat"))
            filter.SerializeTo(file);

        using var rFile = File.OpenRead("../../Test.dat");

        var actual = Filter<Category>.DeserializeFrom(rFile);

        Assert.Equal(filter.ToString(), actual.ToString());
    }

    [Theory(DisplayName = "Protobuf byte[] Test")]
    [ClassData(typeof(SerializationTestData))]
    public void ProtobufByteArrayTest(Filter<Category> filter)
    {
        filter.SerializeTo(out var bytes);

        var actual = Filter<Category>.DeserializeFrom(bytes);

        Assert.Equal(filter.ToString(), actual.ToString());
    }
}
