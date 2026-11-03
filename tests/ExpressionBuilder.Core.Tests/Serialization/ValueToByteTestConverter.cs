using System.Text;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

namespace ExpressionBuilder.Tests;

public struct ValueToByteTestConverter : IProtoConverter
{
    public string StringValue { get; set; }
    public int NumericValue { get; set; }

    public static ValueToByteTestConverter Test() =>
        new() { StringValue = "Test", NumericValue = 123 };

    public readonly byte[] Pack() =>
        [
            .. BitConverter.GetBytes(NumericValue),
            .. Encoding.UTF8.GetBytes(StringValue),
        ];

    public void UnPack(byte[] bytes)
    {
        NumericValue = BitConverter.ToInt32(bytes.AsSpan(0, 4));
        StringValue = Encoding.UTF8.GetString(bytes[4..]);
    }
}
