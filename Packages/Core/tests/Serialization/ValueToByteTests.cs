namespace ExpressionBuilder.Tests;

using TopMarksDevelopment.ExpressionBuilder.Serialization;

[Collection("Serialization checks")]
public class ValueToByteTests
{
    [Theory(DisplayName = "All standard types work")]
    [ClassData(typeof(ValueToByteTestData))]
    public void ValueByteConversionTest(object item)
    {
        byte[]? packedBytes = ValueToByteConverters.Pack(item);

        // Null throws so catch it
        if (item is null)
        {
            Assert.Null(ValueToByteConverters.UnPack<dynamic?>(packedBytes));
            return;
        }

        Assert.Equal(
            item,
            ValueToByteConverters.UnPack(item.GetType(), packedBytes)
        );
    }

    [Fact(DisplayName = "Custom converter works fine")]
    public void CustomConversionTest()
    {
        var item = ValueToByteTestConverter.Test();
        byte[]? packedBytes = ValueToByteConverters.Pack(item);

        Assert.Equal(
            item,
            ValueToByteConverters.UnPack(item.GetType(), packedBytes)
        );
    }
}
