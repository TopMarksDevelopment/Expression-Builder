namespace ExpressionBuilder.Tests.Serialization;

using System.IO;
using System.Text.Json;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using Xunit;

[Collection("Serialization checks")]
public class SerializationTests
{
    public static IEnumerable<object[]> All() =>
        SerializationTestData.GetAll();
    
    [Theory(DisplayName = "Multi Collection Test")]
    [MemberData(nameof(All))]
    public void MultiCollectionTest(Filter<Category> filter)
    {
        var jsonString = JsonSerializer.Serialize(filter);
        var filterFromJson = JsonSerializer.Deserialize<Filter<Category>>(
            jsonString
        );

        Assert.Equal(filter.ToString(), filterFromJson?.ToString());
    }

    [Theory(DisplayName = "Protobuf File Test")]
    [MemberData(nameof(All))]
    public void ProtobufFileTest(Filter<Category> filter)
    {
        using (var file = File.Create("../../Test.dat"))
            filter.SerializeTo(file);

        using var rFile = File.OpenRead("../../Test.dat");

        var actual = Filter<Category>.DeserializeFrom(rFile);

        Assert.Equal(filter.ToString(), actual.ToString());
    }

    [Theory(DisplayName = "Protobuf byte[] Test")]
    [MemberData(nameof(All))]
    public void ProtobufByteArrayTest(Filter<Category> filter)
    {
        filter.SerializeTo(out var bytes);

        var actual = Filter<Category>.DeserializeFrom(bytes);

        Assert.Equal(filter.ToString(), actual.ToString());
    }
}
